/*************************************************************************************
 Created Date : 2020.10.07
      Creator : 김태균
   Description : LG 화학 UI 에서 사용할 Combo 용 클래스 - 활성화용 분리
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07 김태균 : Initial Created.
  2021.05.18 조영대 : 공Tray 상세현황 건물콤보박스에 AREAID 정보 연결
  2022.03.21 이정미 : C/V 명령 조건 정보 관리, Aging 출고 Station 공정 관리 콤보박스 함수 추가
  2022.05.19 이정미 : FCS-FP 계획연동 Report LINE_SHOPID 콤보박스 함수 추가
  2022.05.27 이정미 : 콤보박스 함수 추가 - ALLAREA
  2022.06.16 이정미 : 특성기 불량 셀 등록 콤보박스 오류 수정
  2022.06.28 이정미 : 콤보박스 함수 추가 - SYSTEM 동별 공통코드 추가
  2022.06.27 강동희 : Ultium Cell 기준으로(MAIN & MACHIN) 수정
  2022.07.07 이정미 : 2022.06.28 기준으로 RollBack
  2022.07.07 강동희 : DB Null값 처리 되도록 수정
  2022.09.22 강동희 : 설비 LIST 조회 추가
  2022.11.14 최완영 : ConmboStatus 추가 SELECT_ALL (필수조건이나 전체조건 가능)
  2022.11.18 최완영 : SELECT_ALL  관련 AddStatus() 로직 수정
  2022.11.22 이정미 : SYSTEM_AREA_COMMON_CODE INDATA 추가
  2023.01.01 형준우 : SetEqpId 함수 INDATA 추가
  2023.01.06 최완영 : 조회조건  LOTTYPE  Default 값 양산 설정 
  2023.02.28 이제섭 : CELL 포장 > 포장 출고 화면 입고창고 콤보박스 추가
  2023.04.24 권순범 : DEGAS_SUBEQP INDATA 추가 
  2023.05.16 이지은 : EMS 설비 관리 체계 변경으로 컨베이어 설비 판넬 코드 변경
  2023.05.26 임근영 : Trouble 분석 화면 설비 알람 레벨 콤보박스 추가 
  2023.06.02 이지은 : 목적지(CNVL_LOCATION_ID) 조회 쿼리 추가
  2023.07.20 최경아 : 포장완료 Pallet Cell 선별요청 콤보박스 추가 
  2023.08.24 손동혁 : LANEMODEL LANEMODELROUTE 콤보박스 추가 
  2023.08.24 이의철 : 선택된 라우터만 조회 쿼리 추가
  2023.11.10 김태오 : cboGroup 조회 쿼리 추가.
  2023.11.12 조영대 : SetBcrLoc 메소드 AREAID 인수 추가
  2023.11.14 이지은 : QA관리 > PQC검사의뢰 라인 Mapping 정보 추가
  2023.11.17 박나연 : ROUT ROUT_GR_CODE로 조회 추가, Route op ROUT_GR_CODE로 조회 추가
  2023.11.14 이지은 : QA관리 > PQC검사의뢰 조립라인 모델 Mapping 정보 추가
  2023.12.27 형준우 : SetCombo 함수에서 parent_cbo가 null 인 경우 에러 나오지 않도록 예외처리
  2024.02.21 권순범 : LINE_SHOPID_BLDG_CODE 추가
  2024.03.14 이현승 : 상온/출하 Aging 예약 화면 PKG Lot ID 조회시 Lot 타입 반영
  2024.07.08 김용준 : 상온/출하 Aging 예약 화면 NextOP ComboBox 조회 추가
*************************************************************************************************/
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Class
{
    public class CommonCombo_Form
    {
        public CommonCombo_Form() { }

        /// <summary>
        /// 콤보 바인딩 후 -ALL-, -Select-  추가 enum
        /// </summary>
        public enum ComboStatus
        {
            /// <summary>
            /// 콤보 바인딩 후 ALL 을 최상단에 표시
            /// </summary>
            ALL,

            /// <summary>
            /// 콤보 바인딩 후 Select 을 최상단에 표시 (필수선택 항목에 사용)
            /// </summary>
            SELECT,

            /// <summary>
            /// 바인딩 후 선택 안해도 될경우(선택 안해도 되는 콤보일때 사용)
            /// </summary>
            NA,

            /// <summary>
            /// 바인딩만 하고 끝 (바인딩후 제일 1번째 항목을 표시) 
            /// </summary>
            NONE,
            /// <summary>
            /// 콤보 바인딩 후 Select 을 최상단에 표시 ALL을  최하단 표시 (필수선택 항목에 사용)
            /// </summary>
            SELECT_ALL,
        }

        public void SetComboObjParent(C1ComboBox cb, ComboStatus cs, C1ComboBox[] cbChild = null, object[] objParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_obj", objParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {

                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
                //cb.SelectionChanged -= Cb_SelectionChanged;
                //cb.SelectionChanged += Cb_SelectionChanged;
            }
        }

        public void SetCombo(C1ComboBox cb, ComboStatus cs, C1ComboBox[] cbChild = null, C1ComboBox[] cbParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_cbo", cbParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {

                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
                //cb.SelectionChanged -= Cb_SelectionChanged;
                //cb.SelectionChanged += Cb_SelectionChanged;
            }
        }

        public void SetCombo(C1ComboBox cb)
        {
            try
            {
                Hashtable hashTag = cb.Tag as Hashtable;
                ComboStatus cs = (ComboStatus)Enum.Parse(typeof(ComboStatus), hashTag["all_status"].ToString());
                String[] sFilter = new String[10];
                

                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    C1ComboBox[] cbParentArray = hashTag["parent_cbo"] as C1ComboBox[];
                    int i = 0;
                    for (i = 0; i < cbParentArray.Length; i++)
                    {
                        if (cbParentArray[i] == null)
                        {
                            sFilter[i] = "";
                        }
                        else
                        {
                            if (cbParentArray[i].SelectedValue != null)
                            {
                                sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                            }
                            else
                            {
                                sFilter[i] = "";
                            }
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1)
                        {
                            sFilter[i] = s;
                            i++;
                        }

                    }
                }
                else if (hashTag.Contains("parent_obj") && hashTag["parent_obj"] != null)
                {
                    object[] objParentArray = hashTag["parent_obj"] as object[];
                    int i = 0;
                    for (i = 0; i < objParentArray.Length; i++)
                    {
                        switch (objParentArray[i].GetType().Name)
                        {
                            case "LGCDatePicker":
                                LGCDatePicker lgcDp = objParentArray[i] as LGCDatePicker;
                                if (lgcDp.DatepickerType.ToString().Equals("Month"))
                                {
                                    sFilter[i] = lgcDp.SelectedDateTime.ToString("yyyyMM");
                                }
                                else
                                {
                                    sFilter[i] = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                                }

                                break;
                            case "C1ComboBox":
                                C1ComboBox cbObj = objParentArray[i] as C1ComboBox;

                                if (cbObj.SelectedValue != null)
                                {
                                    sFilter[i] = cbObj.SelectedValue.ToString();
                                }
                                else
                                {
                                    sFilter[i] = "";
                                }

                                break;
                            case "TextBox":
                                TextBox tb = objParentArray[i] as TextBox;
                                sFilter[i] = tb.Text;
                                break;

                            case "String":
                                string st = objParentArray[i] as string;
                                sFilter[i] = st;
                                break;
                        }

                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1)
                        {
                            sFilter[i] = s;
                            i++;
                        }

                    }
                }
                else if (hashTag.Contains("filter") && hashTag["filter"] != null)
                {
                    sFilter = hashTag["filter"] as String[];
                }
                else
                {
                    sFilter[0] = "";
                }

                switch (hashTag["combo_case"].ToString())
                {
                    case "SHOP":
                        SetShop(cb, cs, sFilter);
                        break;
                    case "SHOP_AREATYPE":
                        SetShopByAreaType(cb, cs, sFilter);
                        break;
                    case "SHOP_AUTH":
                        SetShopAuth(cb, cs, sFilter);
                        break;
                    case "AREA":
                        SetArea(cb, cs, sFilter);
                        break;
                    case "AREA_CP":
                        SetArea_CP(cb, cs, sFilter);
                        break;
                    case "AREA_AREATYPE":
                        SetAreaByAreaType(cb, cs, sFilter);
                        break;
                    case "AREA_NO_AUTH":
                        SetAreaNoAuth(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT":
                        SetLine(cb, cs, sFilter);
                        break;
                    case "PROCESSEQUIPMENTSEGMENT":
                        SetProcessLine(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_AREATYPE":
                        SetLineByAreaType(cb, cs, sFilter);
                        break;
                    case "PROCESS":
                        SetProcess(cb, cs, sFilter);
                        break;
                    case "PROCESSWITHAREA":
                        SetProcessWithArea(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT":
                        SetEquipment(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_EQPTID":
                        SetEquipmentWithEqptID(cb, cs, sFilter);
                        break;
                    case "SHIFT":
                        SetShift(cb, cs, sFilter);
                        break;
                    case "SHIFT_AREA":
                        SetShiftByArea(cb, cs, sFilter);
                        break;
                    case "COMMCODE":
                        SetCommonCode(cb, cs, sFilter);
                        break;
                    case "LOTTYPE":
                        SetLotType(cb, cs, sFilter);
                        break;
                    case "WOTYPE":
                        SetWoType(cb, cs, sFilter);
                        break;
                    case "ACTIVITIREASON":
                        SetActivitiReason(cb, cs, sFilter);
                        break;
                    case "CBO_AREA_ACTIVITIREASON": // 동별 HOLD코드
                        SetAreaActivitiReason(cb, cs, sFilter);
                        break;
                    case "cboLotStatus": // SRS포장출고
                        SetLotStatus(cb, cs, sFilter);
                        break;
                    case "cboProcessByAreaid":  // wip 상태
                        SetProcessByAreaid(cb, cs, sFilter);
                        break;
                    case "LINEBYSHOP":
                        SetLineByShop(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegmentForm":
                    case "cboDummyLineID":
                        SetLineForm(cb, cs, sFilter);
                        break;
                    case "cboProcessSegment":
                        SetProcessSegment(cb, cs, sFilter);
                        break;
                    case "PROCESSSEGMENTLINE":
                        SetProcessSegmentLine(cb, cs, sFilter);
                        break;
                    case "PROCESS_PCSGID":
                        SetProcessPCSGID(cb, cs, sFilter);
                        break;
                    //여기서부터 활성화 추가
                    case "AREA_COMMON_CODE":    // 동별 공통코드
                        SetAreaCommonCodeByTypeCode(cb, cs, sFilter);
                        break;
                    case "LINEMODEL":
                        SetLineModel(cb, cs, sFilter);
                        break;
                    case "LANE":
                        SetLane(cb, cs, sFilter);
                        break;
                    case "ROUTE_OP":
                        SetRouteOp(cb, cs, sFilter);
                        break;
                    case "AGINGKIND":
                        SetAgingKind(cb, cs, sFilter);
                        break;
                    case "MODEL":
                        SetModel(cb, cs, sFilter);
                        break;
                    case "TRAYTYPE":
                        SetTrayType(cb, cs, sFilter);
                        break;
                    case "BLDG": //물리적 건물 번호
                        SetBldgCd(cb, cs, sFilter);
                        break;
                    case "COL":
                        SetCol(cb, cs, sFilter);
                        break;
                    case "STG":
                        SetStg(cb, cs, sFilter);
                        break;
                    case "ROW":
                        SetRow(cb, cs, sFilter);
                        break;
                    case "AGING_ROW":
                        SetAgingRow(cb, cs, sFilter);
                        break;
                    case "AGING_COL":
                        SetAgingCol(cb, cs, sFilter);
                        break;
                    case "AGING_STG":
                        SetAgingStg(cb, cs, sFilter);
                        break;
                    case "SCLINE":
                        SetSCLine(cb, cs, sFilter);
                        break;
                    case "EMPTY_OUT_LOC":
                        SetEmptyOutLoc(cb, cs, sFilter);
                        break;
                    case "LINE":
                        SetFormLine(cb, cs, sFilter);
                        break;
                    case "ROUTE":
                        SetFormRoute(cb, cs, sFilter);
                        break;
                    case "ROUTE_EX":
                        SetFormRoute_Ex(cb, cs, sFilter);
                        break;
                    case "ROUTE_RESV": //선택된 라우터만 조회 쿼리 추가
                        SetFormRoute_Resv(cb, cs, sFilter);
                        break;
                    case "ROUTE_NEXTOP":
                        SetFormRouteNextOp(cb, cs, sFilter);
                        break;
                    case "CMN":
                        SetCommonCode(cb, cs, sFilter);
                        break;
                    case "CMN_WITH_OPTION":
                        SetCommonCodeWithOption(cb, cs, sFilter);
                        break;
                    case "FORM_CMN":
                        SetCommonCode_FORM(cb, cs, sFilter);
                        break;
                    case "EQPT_GR_TYPE_CODE":
                        SetEqptGrTypeCode(cb, cs, sFilter);
                        break;
                    case "JUDGE_OP":
                        SetJudgeOp(cb, cs, sFilter);
                        break;
                    case "ROUTE_OP_MAX_END_TIME":
                        SetRouteOpMaxEndTime(cb, cs, sFilter);
                        break;
                    case "SCEQPID":
                        SetScEqpId(cb, cs, sFilter);
                        break;
                    case "EQPID":
                        SetEqpId(cb, cs, sFilter);
                        break;
                    case "EQPSUBSTRING":
                        SetEqpIdSubString(cb, cs, sFilter);
                        break;
                    case "cboModelLot": // Cell 포장 전용 모델 Lot 조회 
                        SetModelLot(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTGROUP":
                        SetEquipmentGroup(cb, cs, sFilter);
                        break;
                    case "LOSSEQP":
                        SetLossEqp(cb, cs, sFilter);
                        break;
                    case "EQPLOSS":
                        SetEqpLoss(cb, cs, sFilter);
                        break;
                    case "EQPLOSSDETAIL":
                        SetEqpLossDetail(cb, cs, sFilter);
                        break;
                    case "LASTLOSSCODE":
                        SetEqpLastLoss(cb, cs, sFilter);
                        break;
                    case "FCR":
                        SetFcr(cb, cs, sFilter);
                        break;
                    case "OCCUREQP":
                        SetOccurEqp(cb, cs, sFilter);
                        break;
                    case "FCRGROUP":
                        SetFcrGroup(cb, cs, sFilter);
                        break;
                    case "PLANT":
                        SetPlant(cb, cs, sFilter);
                        break;
                    case "EQPDEGASEOL":
                        SetEqpIdDegasEol(cb, cs, sFilter);
                        break;
                    case "TROUBLE":
                        SetTrouble(cb, cs, sFilter);
                        break;
                    case "EQPIDBYLANE":
                        SetEqpIdByLane(cb, cs, sFilter);
                        break;
                    case "EQPIDBYLANEJIG":
                        SetEqpIdByLaneJIG(cb, cs, sFilter);
                        break;
                    case "LANEFORM":
                        SetLaneForm(cb, cs, sFilter);
                        break;
                    case "LINELANE":
                        SetFormLineLane(cb, cs, sFilter);
                        break;

                    case "PQCDEPT":
                        SetPqcDept(cb, cs, sFilter);
                        break;
                    case "PQCUSER":
                        SetPqcUser(cb, cs, sFilter);
                        break;
                    case "QAREQSTEP":
                        SetQaReqStep(cb, cs, sFilter);
                        break;

                    case "CELL_DEFECT":
                        SetCellDefect(cb, cs, sFilter);
                        break;
                    case "DEFECT_KIND":
                        SetDefectKind(cb, cs, sFilter);
                        break;
                    case "ETRAY_LOC":
                        SetEmptyTrayLoc(cb, cs, sFilter);
                        break;
                    case "PORT_MAIN":
                        SetPortMain(cb, cs, sFilter);
                        break;
                    case "FORMEQGR":
                        SetFormEqgr(cb, cs, sFilter);
                        break;
                    case "EQPFLOOR":
                        SetFormFloor(cb, cs, sFilter);
                        break;
                    case "FORMMAINEQP":
                        SetFormEqpMain(cb, cs, sFilter);
                        break;
                    case "DEGASCHM":
                        SetDegasChm(cb, cs, sFilter);
                        break;
                    case "FORMTOOUTLOC":
                        SetFormToOutLoc(cb, cs, sFilter);
                        break;
                    case "FORM_SHIFT":
                        SetFormShift(cb, cs, sFilter);
                        break;
                    case "EQPDEGASEOL_WC":
                        SetEqpDegasEolWc(cb, cs, sFilter);
                        break;
                    case "FORM_WH":
                        GetWHInfo(cb, cs, sFilter);
                        break;
                    case "PROC_BY_GR":
                        SetOpByProcGr(cb, cs, sFilter);
                        break;
                    case "LOT":
                        SetLotId(cb, cs, sFilter); //20210404 상온/출하 Aging 예약 화면에 사용되는 Lot List 조회 쿼리 추가
                        break;
                    case "PROCGRP_BY_LINE":
                        SetProcGrpByLine(cb, cs, sFilter); //2021.04.09 Line별 공정 그룹 설정 추가 START
                        break;
                    case "DEGAS_SUBEQP":
                        SetDegasSubEqp(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                    //컨베이어 명령정보 Start
                    case "CNVR_GRP":
                        SetCnvrGrp(cb, cs, sFilter);
                        break;
                    case "BCR_LOC":
                        SetBcrLoc(cb, cs, sFilter);
                        break;
                    //Aging 출고 Station 공정 관리 Start
                    case "CNV_OUTPUT":
                        SetCNVOutPut(cb, cs, sFilter);
                        break;
                    //JIG Unload Dummy Tray Start
                    case "JIG_EQPT":
                        SetJigEqpt(cb, cs, sFilter);
                        break;
                    //활성화 Lot 관리 Start
                    case "FORM_LINE":
                        SetEquipmentSegmentCombo(cb, cs, sFilter);
                        break;
                    //FCS-FP 계획연동 Report Start
                    case "LINE_SHOPID":
                        SetFormLine_ShopID(cb, cs, sFilter);
                        break;
                    //활성화 동코드 조건으로 라인 조회
                    case "LINE_SHOPID_BLDG_CODE":
                        SetFormLine_ShopID_BLDG_CODE(cb, cs, sFilter);
                        break;
                    //AREA Start
                    case "ALLAREA":
                        SetAllArea(cb, cs, sFilter);
                        break;
                    // SYSTEM 동별 공통코드 
                    case "SYSTEM_AREA_COMMON_CODE":    // 동별 공통코드
                        SetSystemAreaCommonCodeByTypeCode(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTFORM":
                        SetEquipmentForm(cb, cs, sFilter);
                        break;
                    // CELL 포장 > 포장 출고 입고창고 콤보박스
                    case "cboLocTo":
                        SetcboLocTo(cb, cs, sFilter);
                        break;
                    // CELL 포장 > 포장 출고 출하처 콤보박스
                    case "cboComp":
                        SetcboComp(cb, cs, sFilter);
                        break;
                    // 작업관리 > Trouble 분석 설비알람레벨 콤보박스
                    case "EQPT_ALARM_LEVEL_CODE":
                        SetEqptAlarmLevel(cb, cs, sFilter);
                        break;
                    //2023.06.02 CnvrLocation
                    case "CNVR_LOCATION":
                        SetcboCVLocation(cb, cs, sFilter);
                        break;
                    //CELL 포장 > 포장완료 Pallet Cell 선별요청 
                    case "SORTING":
                        SetSorting(cb, cs, sFilter);
                        break;

                    /// 충방전기 Gripper LANE > MODEL 콤보박스
                    case "LANEMODEL":
                        SetLaneModel(cb, cs, sFilter);
                        break;
                    /// 충방전기 Gripper LANEMODEL > ROUTE 콤보박스
                    case "LANEMODELROUTE":
                        SetFormlLaneModelRoute(cb, cs, sFilter);
                        break;
                    case "GROUP":
                        SetGroup(cb, cs, sFilter);
                        break;
                    case "PQCASSYLINE":
                        SetPQCAssyLine(cb, cs, sFilter);
                        break;
                    case "ROUTE_TYPE":
                        SetFormRouteType(cb, cs, sFilter);
                        break;
                    case "ROUTE_OP_GR":
                        SetRouteOpGr(cb, cs, sFilter);
                        break;
                    case "PQCLINEMODEL":
                        SetPqcLineModel(cb, cs, sFilter);
                        break;
                    //// 상온/출하 Aging 예약 - Aging Type 선택에 따라 NEXTOP 순서 변경
                    case "AGINGTYPE_NEXTOP":
                        SetCommonCode_AgingType_NextOP(cb, cs, sFilter);
                        break;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
                case ComboStatus.SELECT_ALL:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);

                    DataRow dr1 = dt.NewRow();
                    dr1[sDisplay] = "-ALL-";
                    dr1[sValue] = "";
                    dt.Rows.InsertAt(dr1, dt.Rows.Count);
                    break;
            }

            return dt;
        }

        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                CommonCombo_Form _combo = new CMM001.Class.CommonCombo_Form();

                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    _combo.SetCombo(cbChild);
                }
            }
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                CommonCombo_Form _combo = new CMM001.Class.CommonCombo_Form();

                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    _combo.SetCombo(cbChild);
                }
            }
        }

        private void SetDefaultCbo(C1ComboBox cbo, ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new DataTable();

                dtResult.Columns.Add("CBO_CODE", typeof(string));
                dtResult.Columns.Add("CBO_NAME", typeof(string));

                DataRow newRow = dtResult.NewRow();

                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "NA", "구현안됨" };
                dtResult.Rows.Add(newRow);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFcr(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FCR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FCR_TYPE_CODE"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_MMD_EQPT_FCR_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFcrGroup(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (!sFilter[0].Equals(""))
                {
                    dr["PROCID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FCR_GROUP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetOccurEqp(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["EQPTID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_OCCUR_EQPT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqpLastLoss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["EQPTID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LAST_LOSS_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqpLossDetail(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["LOSS_CODE"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSS_DETL_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqpLoss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLossEqp(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["LANE_ID"] = sFilter[0];
                }

                if (!sFilter[1].Equals(""))
                {
                    dr["EQPTTYPE"] = sFilter[1];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_IN_EQPTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEquipmentGroup(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["EQGRID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTGROUP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetShop(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["SHOPID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_SHOP_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetShopByAreaType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_SHOP_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetShopAuth(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_SHOP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_SHOP_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!sFilter[0].Equals(""))
                {
                    dr["SHOPID"] = sFilter[0];
                }
                else
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                }
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAreaNoAuth(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!sFilter[0].Equals(""))
                {
                    dr["SHOPID"] = sFilter[0];
                }
                else
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (sFilter[1].Equals("ALL"))
                {
                    if (!LoginInfo.CFG_AREA_ID.Equals(""))
                    {
                        cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                        if (cbo.SelectedIndex < 0)
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetArea_CP(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_SYSTEM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        if (((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString() == LoginInfo.CFG_AREA_ID)
                        {
                            cbo.SelectedIndex = i;
                            return;
                        }
                    }
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAreaByAreaType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!sFilter[0].Equals(""))
                {
                    dr["SHOPID"] = sFilter[0];
                }
                else
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                }
                dr["AREA_TYPE_CODE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];

                if (!string.IsNullOrEmpty(sFilter[1]))
                    dr["PROCID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLineByAreaType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["AREA_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLineByShop(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROD_GROUP", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = string.IsNullOrWhiteSpace(sFilter[0]) ? null : sFilter[0];
                dr["PROD_GROUP"] = string.IsNullOrWhiteSpace(sFilter[1]) ? null : sFilter[1];
                dr["PROCID"] = string.IsNullOrWhiteSpace(sFilter[2]) ? null : sFilter[2];
                if (sFilter.Length > 3) dr["AREAID"] = string.IsNullOrWhiteSpace(sFilter[3]) ? null : sFilter[3];
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQSG_BY_SHOP_CBO", "RQSTDT", "RSLTDT", inDataTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        if (CommonVerify.HasTableRow(dtResult) && dtResult.Rows.Count == 2)
                        {
                            cbo.SelectedIndex = 1;
                        }
                        else
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLineForm(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                inDataTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_PKG_CBO", "RQSTDT", "RSLTDT", inDataTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        if (CommonVerify.HasTableRow(dtResult) && dtResult.Rows.Count == 2)
                        {
                            cbo.SelectedIndex = 1;
                        }
                        else
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessPCSGID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_PCSGID", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcess(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessWithArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["EQSGID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_WITH_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEquipment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["COATER_EQPT_TYPE_CODE"] = sFilter[2];

                if (sFilter.Length > 3)
                    dr["EQPT_RSLT_CODE"] = sFilter[3];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbo.SelectedIndex < 0)
                        cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEquipmentWithEqptID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["EQPTID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQPTID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetShift(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["PROCID"] = sFilter[2] == "" ? null : sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetShiftByArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCommonCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                if (sFilter.Length > 1)
                    dr["CMCODE"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLotType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_SHOP_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_SHOP_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;

                }

                //2023.01.06  값 선택할수 있도록 추가
                if (sFilter.Length > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        if (((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString() == sFilter[0])
                        {
                            cbo.SelectedIndex = i;
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void SetWoType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WOTYPE_COMBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLotStatus(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_LOT_STATUS", "RQSTDT", "RSLTDT", RQSTDT);


                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetActivitiReason(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITIREASON_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAreaActivitiReason(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["ACTID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_ACTIVITIREASON_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessByAreaid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        #region MultiSelectionBox 구현부분
        //메소드 직접 콜해서 사용
        public void SetMultiBoxMaterialCode(MultiSelectionBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MATERIAL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
        }
        #endregion

        #region [기본 콤보 박스 및 데이터 그리드 콤보 박스 바인딩 메소드 2017.01.13 신광희]
        public static void CommonBaseCombo(string bizRuleName, C1ComboBox cbo, string[] arrColumn, string[] arrCondition, ComboStatus status, string selectedValueText, string displayMemberText, string selectedValue = null)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                        inDataTable.Columns.Add(col, typeof(string));

                    DataRow dr = inDataTable.NewRow();

                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });

                cbo.DisplayMemberPath = displayMemberText;
                cbo.SelectedValuePath = selectedValueText;
                cbo.ItemsSource = StatusAdd(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
                cbo.SelectedIndex = 0;

                if (!string.IsNullOrEmpty(selectedValue))
                {
                    cbo.SelectedValue = selectedValue;

                    if (cbo.SelectedIndex < 0)
                        cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static void SetDataGridComboItem(string bizRuleName, string[] arrColumn, string[] arrCondition, ComboStatus status, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                        inDataTable.Columns.Add(col, typeof(string));

                    DataRow dr = inDataTable.NewRow();

                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = StatusAdd(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static void SetFindPopupCombo(string bizRuleName, PopupFindControl pop, string[] arrColumn, string[] arrCondition, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                pop.ItemsSource = null;

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                    {
                        inDataTable.Columns.Add(col, typeof(string));
                    }

                    DataRow dr = inDataTable.NewRow();
                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                    {
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                    }
                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                pop.ItemsSource = DataTableConverter.Convert(dtBinding);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private static DataTable StatusAdd(DataTable dt, ComboStatus cs, string selectedValueText, string displayMemberText)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[selectedValueText] = null;
                    dr[displayMemberText] = "-ALL-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[selectedValueText] = "SELECT";
                    dr[displayMemberText] = "-SELECT-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[selectedValueText] = string.Empty;
                    dr[displayMemberText] = "-N/A-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NONE:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(cs), cs, null);
            }

            return dt;
        }
        #endregion

        private void SetProcessSegment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessSegmentLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PCSGID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTLINE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAreaCommonCodeByTypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //여기서부터 활성화 추가
        private void SetLineModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MODEL", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLane(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("LANE_LAST_CHAR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[0])) ? "Y" : null;
                //if (sFilter.Length > 1)
                //    dr["LANE_LAST_CHAR"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //DataTable dtMerge;
                //if (bAddOriData)
                //{
                //    DataTable dtOrigin = ((DataView)cbo.ItemsSource).ToTable();
                //    dtMerge = dtOrigin.Copy();
                //    dtMerge.Merge(dtResult);
                //    dtResult = dtMerge.Copy();
                //}
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetRouteOp(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                cbo.Clear();
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("BYPASS_OP_YN", typeof(string));
                RQSTDT.Columns.Add("OP_FILTER", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = string.IsNullOrEmpty(sFilter[0]) ? null : sFilter[0];
                dr["PROC_GR_CODE"] = string.IsNullOrEmpty(sFilter[1]) ? null : sFilter[1];
                dr["BYPASS_OP_YN"] = string.IsNullOrEmpty(sFilter[2]) ? null : sFilter[2];
                dr["OP_FILTER"] = string.IsNullOrEmpty(sFilter[3]) ? null : sFilter[3];
                RQSTDT.Rows.Add(dr);

                string sBiz = "DA_BAS_SEL_ROUTE_OP_CBO";
                if (string.IsNullOrEmpty(sFilter[0])) sBiz = "DA_BAS_SEL_ALL_OP_CBO";
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAgingKind(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                if (sFilter.Length > 1)
                    dr["CMCODE_LIST"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetModel(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FORM_MODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetTrayType(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USE_FLAG"] = (sFilter[0].Equals("ALL")) ? null : (string.IsNullOrEmpty(sFilter[0])) ? "Y" : sFilter[0];
                if (!string.IsNullOrEmpty(sFilter[1]))
                {
                    dr["AREAID"] = sFilter[1];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_TRAY_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetBldgCd(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "BLDG_CODE";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_BLDG", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetCol(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("AGING_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = string.IsNullOrEmpty(sFilter[0]) ? null : sFilter[0];
                dr["S71"] = string.IsNullOrEmpty(sFilter[1]) ? null : sFilter[1];
                dr["AGING_YN"] = (sFilter.Length > 2) ? sFilter[2] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_COL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetStg(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("AGING_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = string.IsNullOrEmpty(sFilter[0]) ? null : sFilter[0];
                dr["S71"] = string.IsNullOrEmpty(sFilter[1]) ? null : sFilter[1];
                dr["AGING_YN"] = (sFilter.Length > 2) ? sFilter[2] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_STG_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetRow(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("S01", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["S71"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["S01"] = (sFilter.Length > 2) ? sFilter[2] : null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_ROW_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetAgingRow(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["S71"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["EQPTID"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_AGING_ROW_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetAgingCol(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("X_PSTN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["S71"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["EQPTID"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["X_PSTN"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_AGING_COL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetAgingStg(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("X_PSTN", typeof(string));
                RQSTDT.Columns.Add("Y_PSTN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["S71"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["EQPTID"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["X_PSTN"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                dr["Y_PSTN"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_AGING_STG_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetSCLine(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("S71", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = sFilter[0];
                dr["S71"] = sFilter[1];
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_UNIT_SCLINE_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEmptyOutLoc(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_OUT_LOC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormLine(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                //RQSTDT.Columns.Add("BLDG_CD", typeof(string)); // 사용여부 확인 후 반영

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                Hashtable hashTag = cbo.Tag as Hashtable;
                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    if (sFilter[0] != null)
                    {
                        //if (sFilter[0].Length > 1)
                        //{
                        //    dr["AREAID"] = sFilter[0];
                        //}
                        //else // 사용여부 확인 후 반영
                        //{
                        //    dr["BLDG_CD"] = sFilter[0];
                        //}
                    }
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[1])) ? "Y" : null;
                }
                else
                {
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[0])) ? "Y" : null;
                }
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormRoute(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["ROUT_RSLT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["MODEL_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                dr["ROUTE_TYPE_DG"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                if (sFilter.Length > 5) dr["ROUT_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[5])) ? sFilter[5] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormRoute_Ex(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["ROUT_RSLT_GR_CODE"] = null;
                dr["MODEL_TYPE_CODE"] = null;
                dr["ROUTE_TYPE_DG"] = null;
                if (sFilter.Length > 5) dr["ROUT_TYPE_CODE"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormRouteNextOp(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["PROC_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["ROUT_RSLT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                dr["MODEL_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                dr["ROUTE_TYPE_DG"] = (!string.IsNullOrEmpty(sFilter[5])) ? sFilter[5] : null;
                if (sFilter.Length > 5) dr["ROUT_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[6])) ? sFilter[6] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //선택된 라우터만 조회 쿼리 추가
        private void SetFormRoute_Resv(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["PROC_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["ROUT_RSLT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                dr["MODEL_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                dr["ROUTE_TYPE_DG"] = (!string.IsNullOrEmpty(sFilter[5])) ? sFilter[5] : null;
                if (sFilter.Length > 6) dr["ROUT_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[6])) ? sFilter[6] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE_RESV", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCommonCode_FORM(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];

                if (sFilter.Length > 1)
                    dr["CMCODE_LIST"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 상온/출하 Aging 예약 - Aging Type 선택에 따라 NEXTOP 순서 변경
        private void SetCommonCode_AgingType_NextOP(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));
                RQSTDT.Columns.Add("AGING_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (sFilter.Length > 1)
                    dr["CMCDTYPE"] = sFilter[1];

                if (sFilter.Length > 2)
                    dr["CMCODE_LIST"] = sFilter[2];       
                         
                dr["AGING_TYPE"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AGINGTYPE_NEXTOP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqptGrTypeCode(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (string.IsNullOrEmpty(sFilter[0]))
                {
                    dr["EQSGID"] = null;
                }
                else
                {
                    dr["EQSGID"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_KIND_CD", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetJudgeOp(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(sFilter[0])) dr["ROUTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_ROUTE_JUDGE_OP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqpId(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTLEVEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (string.IsNullOrEmpty(sFilter[0])) ? null : sFilter[0];
                dr["EQSGID"] = (string.IsNullOrEmpty(sFilter[1])) ? null : sFilter[1];
                dr["EQPTLEVEL"] = (string.IsNullOrEmpty(sFilter[2])) ? null : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCommonCodeWithOption(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTR1", typeof(string));
                RQSTDT.Columns.Add("ATTR2", typeof(string));
                RQSTDT.Columns.Add("ATTR3", typeof(string));
                RQSTDT.Columns.Add("ATTR4", typeof(string));
                RQSTDT.Columns.Add("ATTR5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                if (sFilter.Length > 1)
                {
                    dr["ATTR1"] = (string.IsNullOrEmpty(sFilter[1])) ? null : sFilter[1];
                    dr["ATTR2"] = (string.IsNullOrEmpty(sFilter[2])) ? null : sFilter[2];
                    dr["ATTR3"] = (string.IsNullOrEmpty(sFilter[3])) ? null : sFilter[3];
                    dr["ATTR4"] = (string.IsNullOrEmpty(sFilter[4])) ? null : sFilter[4];
                    dr["ATTR5"] = (string.IsNullOrEmpty(sFilter[5])) ? null : sFilter[5];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_WITH_OPTION", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetRouteOpMaxEndTime(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("ROUTE_ID", typeof(string));
                RQSTDT.Columns.Add("EQP_KIND_CD", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["ROUTE_ID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null; //20220707_DB NULL값 처리
                dr["EQP_KIND_CD"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null; //20220707_DB NULL값 처리
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_ROUTE_OP_MAX_END_TIME", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetScEqpId(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(sFilter[0])) dr["COM_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_SC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqpIdSubString(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string)); //EQP_KIND_CD
                //RQSTDT.Columns.Add("START", typeof(Int16));
                //RQSTDT.Columns.Add("LENGTH", typeof(Int16));
                //RQSTDT.Columns.Add("TARGETSTRING", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = sFilter[0];
                //if(sFilter.Length > 1)
                //{
                //    dr["START"] = Convert.ToInt16(sFilter[1]);
                //    dr["LENGTH"] = Convert.ToInt16(sFilter[2]);
                //    dr["TARGETSTRING"] = sFilter[3];
                //}

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_EQPID_SUBSTRING", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetModelLot(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0];
            dr["EQPTID"] = sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetPlant(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_PLANT", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqpIdByLane(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("EQP_KIND_CD", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(Util.NVC(sFilter[0])))
                {
                    dr["LANE_ID"] = Util.NVC(sFilter[0]);
                }

                dr["EQP_KIND_CD"] = Util.NVC(sFilter[1]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQP_BY_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqpIdByLaneJIG(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("EQP_KIND_CD", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(Util.NVC(sFilter[0])))
                {
                    dr["LANE_ID"] = Util.NVC(sFilter[0]);
                }
                dr["EQP_KIND_CD"] = Util.NVC(sFilter[1]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQP_BY_LANE_JIG_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqpIdDegasEol(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string)); //EQPT_GR_TYPE_CODE
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = (Util.NVC(sFilter[0]).Equals("Q")) ? "5" : sFilter[0];
                dr["EQSGID"] = (string.IsNullOrEmpty(sFilter[1])) ? null : sFilter[1]; //Util.NVC(sFilter[1]);
                dr["LANE_ID"] = (string.IsNullOrEmpty(sFilter[2])) ? null : sFilter[2]; //Util.NVC(sFilter[2]);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_DEGAS_EOL", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetTrouble(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string)); //EQPT_GR_TYPE_CODE

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = Util.NVC(sFilter[0]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_TROUBLE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetLaneForm(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("LANE_LAST_CHAR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[0])) ? "Y" : null;
                if (sFilter.Length > 1)
                    dr["LANE_LAST_CHAR"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFormLineLane(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_LANE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //PQC 검사
        private void SetPqcUser(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USER_DEPT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USER_DEPT"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_PQC_USER", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        ///2023.08.24 충방전기 Gripper LANE > MODEL 
        private void SetLaneModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANE_ID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LANE_MODEL", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        ///2023.08.24 충방전기 Gripper LANEMODEL > ROUTE
        private void SetFormlLaneModelRoute(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LANE_MODEL_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetPqcDept(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["COM_TYPE_CODE"] = "COMBO_PQC_USER";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_PQC_DEPT", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetQaReqStep(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_QA_REQSTEP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetCellDefect(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = "Y";
                dr["DFCT_TYPE_CODE"] = sFilter[0];
                dr["DFCT_GR_TYPE_CODE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_TM_CELL_DEFECT", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "DFCT_NAME";
                cbo.SelectedValuePath = "DFCT_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "DFCT_CODE", "DFCT_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetDefectKind(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = "Y";
                dr["DFCT_GR_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEFECT_KIND", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetEmptyTrayLoc(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CFG_SYSTEM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CFG_SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EMPTY_TRAY_LOC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetPortMain(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_PORT_MAIN", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetFormEqgr(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_EQGR", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetFormFloor(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_EQPT_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFormEqpMain(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = sFilter[0] != null ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_MAIN_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFormToOutLoc(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = sFilter[0] != null ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_TO_OUT_LOC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDegasChm(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_DEGAS_CHAMBER", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetFormShift(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetEqpDegasEolWc(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("S70", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = sFilter[0];
                if (sFilter.Length > 1) dr["EQSGID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_DEGAS_EOL_WC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWHInfo(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FORM_WHID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetOpByProcGr(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("BYPASS_OP_YN", typeof(string));
                RQSTDT.Columns.Add("OP_FILTER", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (!string.IsNullOrEmpty(sFilter[0])) dr["PROC_GR_CODE"] = sFilter[0];
                dr["OP_FILTER"] = sFilter[3];
                RQSTDT.Rows.Add(dr);

                string sBiz = "DA_BAS_SEL_ALL_OP_CBO";
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //20210404 상온/출하 Aging 예약 화면에 사용되는 Lot List 조회 쿼리 추가
        public void SetLotId(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("SPCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("AGING_TYPE", typeof(string));
                RQSTDT.Columns.Add("TO_PROC_FIX", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));  //2024.03.14  LH& : PKG Lot ID 조회시 Lot 타입 반영

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(sFilter[0])) dr["EQSGID"] = sFilter[0];
                if (!string.IsNullOrEmpty(sFilter[1])) dr["MDLLOT_ID"] = sFilter[1];
                if (!string.IsNullOrEmpty(sFilter[2])) dr["ROUTID"] = sFilter[2];
                if (!string.IsNullOrEmpty(sFilter[3])) dr["SPCL_FLAG"] = sFilter[3];
                if (!string.IsNullOrEmpty(sFilter[4])) dr["AGING_TYPE"] = sFilter[4];
                if (!string.IsNullOrEmpty(sFilter[4])) dr["TO_PROC_FIX"] = sFilter[5];
                if (!string.IsNullOrEmpty(sFilter[6])) dr["LOTTYPE"] = sFilter[6];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_LOT_LARGE_WIP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //2021.04.09 Line별 공정 그룹 설정 추가 START
        public void SetProcGrpByLine(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = (string.IsNullOrEmpty(sFilter[0])) ? null : sFilter[0];
                dr["CMCDTYPE"] = (string.IsNullOrEmpty(sFilter[1])) ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2021.04.09 Line별 공정 그룹 설정 추가 END


        // 2021-04-09 Degas SubEqp
        public void SetDegasSubEqp(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEGAS_SUBEQP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //컨베이어 명령 정보 관리 Start
        private void SetCnvrGrp(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTLEVEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //EMS 설비 관리체계 변경으로 동별로 코드 상이, 후에 기준정보 도입하여 변경 필요
                if (!string.IsNullOrEmpty(sFilter[0]))
                {
                    dr["EQPTLEVEL"] = sFilter[0];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CNVR_GRP", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBcrLoc(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQP_GRP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQP_GRP"] = sFilter[0] != null ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_BCR_LOC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //Aging 출고 Station 공정 관리 Start
        private void SetCNVOutPut(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0] != null ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_PORT_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //JIG Unload Dummy Tray Start
        private void SetJigEqpt(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_JIG_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //활성화 Lot 관리 Start
        private void SetEquipmentSegmentCombo(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0] != null ? sFilter[0] : null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //cbo.Width = DropDownWidth(cbo);
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFormLine_ShopID(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

  
                dr["SHOPID"] = sFilter[0] != null ? sFilter[0] : null;

                Hashtable hashTag = cbo.Tag as Hashtable;
                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    if (sFilter[0] != null)
                    {
                        if (sFilter[0].Length > 1)
                        {
                            dr["SHOPID"] = sFilter[0];
                        }
                    }
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[1])) ? "Y" : null;
                }
                else
                {
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[0])) ? "Y" : null;
                }
         
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormLine_ShopID_BLDG_CODE(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("S40", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["S40"]    = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAllArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_ALL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSystemAreaCommonCodeByTypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTR3", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                dr["ATTR3"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEquipmentForm(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("EQPTLEVEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["EQSGID"] = sFilter[0] != null ? sFilter[0] : null;
                dr["EQPTTYPE"] = sFilter[1] != null ? sFilter[1] : null;
                dr["EQPTLEVEL"] = sFilter[2] != null ? sFilter[2] : null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.Text = string.Empty;
                    cbo.SelectedIndex = -1;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboLocTo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["SHIP_TYPE_CODE"] = "CELL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboComp(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_SLOC_ID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqptAlarmLevel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["CMCDTYPE"] = "EQPT_ALARM_LEVEL_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_ALARM_LEVEL_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboCVLocation(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GR_TYPE_CODE"] = sFilter[0];
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CNVR_LOCATION", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSorting(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = string.IsNullOrEmpty(sFilter[0]) ? null : sFilter[0];
                if (sFilter.Length > 1) dr["ATTRIBUTE1"] = string.IsNullOrEmpty(sFilter[1]) ? null : sFilter[1];


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetGroup(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
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
                dr["COM_TYPE_CODE"] = "FORM_ZONE_CODE";
                dr["COM_CODE"] = null ;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_SORT", "RQSTDT", "RSLTDT", RQSTDT);                                                                          

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetPQCAssyLine(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                Hashtable hashTag = cbo.Tag as Hashtable;
                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    if (sFilter[0] != null)
                    {
                        //if (sFilter[0].Length > 1)
                        //{
                        //    dr["AREAID"] = sFilter[0];
                        //}
                        //else // 사용여부 확인 후 반영
                        //{
                        //    dr["BLDG_CD"] = sFilter[0];
                        //}
                    }
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[1])) ? "Y" : null;
                }
                else
                {
                    dr["ONLY_X"] = (string.IsNullOrEmpty(sFilter[0])) ? "Y" : null;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE_MAPPING", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormRouteType(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(sFilter[1])) ? sFilter[1] : null;
                dr["ROUT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[2])) ? sFilter[2] : null;
                dr["MODEL_TYPE_CODE"] = (!string.IsNullOrEmpty(sFilter[3])) ? sFilter[3] : null;
                dr["ROUTE_TYPE_DG"] = (!string.IsNullOrEmpty(sFilter[4])) ? sFilter[4] : null;
                if (sFilter.Length > 5) dr["ROUT_RSLT_GR_CODE"] = (!string.IsNullOrEmpty(sFilter[5])) ? sFilter[5] : null;
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetRouteOpGr(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                cbo.Clear();
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("PROC_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("ROUT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("BYPASS_OP_YN", typeof(string));
                RQSTDT.Columns.Add("OP_FILTER", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = string.IsNullOrEmpty(sFilter[0]) ? null : sFilter[0];
                dr["PROC_GR_CODE"] = string.IsNullOrEmpty(sFilter[1]) ? null : sFilter[1];
                dr["ROUT_GR_CODE"] = string.IsNullOrEmpty(sFilter[2]) ? null : sFilter[2];
                dr["BYPASS_OP_YN"] = string.IsNullOrEmpty(sFilter[3]) ? null : sFilter[3];
                dr["OP_FILTER"] = string.IsNullOrEmpty(sFilter[4]) ? null : sFilter[4];
                RQSTDT.Rows.Add(dr);

                string sBiz = "DA_BAS_SEL_ROUTE_OP_CBO";
                if (string.IsNullOrEmpty(sFilter[0])) sBiz = "DA_BAS_SEL_ALL_OP_CBO";
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetPqcLineModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = (!string.IsNullOrEmpty(sFilter[0])) ? sFilter[0] : null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MODEL_ASSY", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}