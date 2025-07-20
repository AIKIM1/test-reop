/*************************************************************************************
 Created Date : 2016.07.25
      Creator : scpark
   Description : LG 화학 UI 에서 사용할 Combo 용 클래스
--------------------------------------------------------------------------------------
 [Change History]
  2016.07.25 / scpark : Initial Created.
  2018.12.17 손우석 CSR ID 3858186 [G.MES] Audi BEV C-sample G.MES 출하 구성 결합이력 데이터 출력 기능 추가 요청의 건 [요청번호]C20181130_58186
  2019.05.15 염규범 C20190423_79897 설비 LOSSS 처리 화면 - 추가요청건) 김관영 책임 요청의 건
  2019.07.07 염규범 이정진 책임 요청의 건 ( 선처리 요청 - 김정균 책임님 ) 
  2020.04.03 김민석 [CSR ID:C20200406-000405] Master Lot 화면 수정으로 combo case에 cboRoutGroup, cboProductRout 추가
  2020.05.07 김동일 C20200406-000377 관련 콤보 조회 추가
  2020.07.22 김동일 C20200603-000041 OFF LINE 사용 저장위치 콤보 조회 추가
  2020.09.22 고재영 C20200922-000291 공정 Interlock PJT 소형 조립 Dry Room 입출고 기능 개발 건 - 자재군 cboMTGRID 추가
**************************************************************************************/
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
    public class CommonCombo_Back
    {
        public CommonCombo_Back() { }

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
            NONE
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
                        if (cbParentArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                        }
                        else
                        {
                            sFilter[i] = "";
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1) {
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
                    case "cboShop":
                    case "SHOP":
                        SetShop(cb, cs, sFilter);
                        break;
                    case "cboShopByAreaType":
                    case "SHOP_AREATYPE":
                        SetShopByAreaType(cb, cs, sFilter);
                        break;
                    case "SHOP_AUTH":
                        SetShopAuth(cb, cs, sFilter);
                        break;
                    case "cboArea":
                    case "AREA":
                        SetArea(cb, cs, sFilter);
                        break;
                    case "AREA_CP":
                        SetArea_CP(cb, cs, sFilter);
                        break;
                    //2018.12.08
                    case "cboArea_Areatype":
                    case "cboAreaByAreaType":
                    case "AREA_AREATYPE":
                        SetAreaByAreaType(cb, cs, sFilter);
                        break;
                    case "AREA_NO_AUTH":
                        SetAreaNoAuth(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegment":
                    case "cboLine":
                    case "EQUIPMENTSEGMENT":
                        SetLine(cb, cs, sFilter);
                        break;
                    case "PROCESSEQUIPMENTSEGMENT":
                        SetProcessLine(cb, cs, sFilter);
                        break;
                    case "LINE_CP":
                        SetLine_CP(cb, cs, sFilter);
                        break;
                    case "LINE_FCS":
                        SetLine_FCS(cb, cs, sFilter);
                        break;
                    case "EQSGID_PACK":
                        SetLineForPack(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_AREATYPE":
                        SetLineByAreaType(cb, cs, sFilter);
                        break;
                    case "cboProcess":
                    case "PROCESS":
                    case "cboProcessSRS":
                        SetProcess(cb, cs, sFilter);
                        break;
                    case "ProcessCWA":
                        SetProcessCWA(cb, cs, sFilter);
                        break;
                    // 2017.04.13 Start ==
                    case "cboProcessERP":
                        SetProcessERP(cb, cs, sFilter);
                        break;
                    // 2017.04.13 End   ==
                    case "BOX_PROCESS":
                        SetProcessBox(cb, cs, sFilter);
                        break;
                    case "PROCESSWITHAREA":
                        SetProcessWithArea(cb, cs, sFilter);
                        break;
                    case "PROCESSWITHAREANOLOGININFO":
                        SetProcessWithAreaNoLoginInfo(cb, cs, sFilter);
                        break;
                    case "UNIT_BY_EQPTID":
                    case "cboUnit":
                        SetUnit(cb, cs, sFilter);
                        break;
                    case "cboEquipment":
                    case "EQUIPMENT":
                        SetEquipment(cb, cs, sFilter);
                        break;
                    case "cboEquipmentEqptLoss":
                        SetEquipmentEqptLoss(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_EQSGID_PROCID":
                        SetEquipment_EQSG_PROC(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_LINEGRUP_PROCID":
                        SetEquipment_LinGroup_PROC(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_NT":
                        SetEquipment_NT(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_EQPTID":
                        SetEquipmentWithEqptID(cb, cs, sFilter);
                        break;
                    case "cboShift":
                    case "SHIFT":
                        SetShift(cb, cs, sFilter);
                        break;
                    case "cboProcSel":
                        SetProcSel(cb, cs, sFilter);
                        break;
                    case "SHIFT_AREA":
                        SetShiftByArea(cb, cs, sFilter);
                        break;
                    case "cboLoss":
                    case "LOSSCODE":
                        SetEqptLoss(cb, cs, sFilter);
                        break;
                    case "cboLossDetl":
                    case "LOSSDETL":
                        SetEqptLossDetl(cb, cs, sFilter);
                        break;
                    case "COMMCODES":
                        SetCommonCodeS(cb, cs, sFilter);
                        break;
                    case "FCRCODE":
                        SetFCRCode(cb, cs, sFilter);
                        break;
                    case "COMMCODE":
                        SetCommonCode(cb, cs, sFilter);
                        break;
                    case "COMMCODEATTR":
                        SetCommonCodeAttr(cb, cs, sFilter);
                        break;
                    case "COMMCODEATTR_MCS":
                        SetCommonCodeAttr_MCS(cb, cs, sFilter);
                        break;
                    case "COMMCODEATTRS":
                        SetCommonCodeAttrs(cb, cs, sFilter);
                        break;
                    case "COMMCODEATTR2":
                        SetCommonCodeAttr2(cb, cs, sFilter);
                        break;
                    case "cboOccurEqpt":
                        //Pack 오창 원인설비 MAIN 설비랑 Unit 설비만 보여주기는내용으로 변경
                        // 2019-05-13 염규범 S
                        if (LoginInfo.CFG_AREA_ID.Equals("P1") || LoginInfo.CFG_AREA_ID.Equals("P2") || LoginInfo.CFG_AREA_ID.Equals("P6"))
                        {
                            SetOccurPackEqpt(cb, cs, sFilter);
                        }
                        else
                        {
                            SetOccurEqpt(cb, cs, sFilter);
                        }
                        break;
                    case "STOCKSEQ":
                        SetStockSeq(cb, cs, sFilter);
                        break;
                    case "cboMaterialCode":
                        SetMaterialCode(cb, cs, sFilter);
                        break;
                    case "PRODBYBICELL":
                        SetProdCodeByBiCellType(cb, cs, sFilter);
                        break;
                    case "cboProcessSegmentByEqsgid":
                        SetProcessSegmentByEqsgid(cb, cs, sFilter);
                        break;
                    case "ProcessSegmentByEqsgid_M_P":
                        SetProcessSegmentByEqsgid_M_P(cb, cs, sFilter);
                        break;
                    case "cboRouteByPcsgid":
                        SetRouteByPcsgid(cb, cs, sFilter);
                        break;
                    case "ROUTEBYPCSGMODL":
                        SetRouteByModlPcsg(cb, cs, sFilter);
                        break;
                    case "ROUTEBYPCSGMODLPRODID":
                        SetRouteByModlPcsgProdId(cb, cs, sFilter);
                        break;
                    case "cboMaterialiD":
                        SetMaterialId(cb, cs, sFilter);
                        break;
                    case "cboRouteByModlid":
                    case "ROUTEBYMODLID":
                        SetRouteByModlid(cb, cs, sFilter);
                        break;
                    case "cboModel":
                    case "MODEL":
                        SetModel(cb, cs, sFilter);
                        break;
                    case "cboModelLot": // 모델콤보 - cell포장 전용
                        SetModelLot(cb, cs, sFilter);
                        break;
                    case "PRJ_MODEL": // 모델콤보 - 프로젝트모델조회
                        SetProject_Model(cb, cs, sFilter);
                        break;
                    case "PRJ_MODEL_AUTH":
                        SetProject_Model_Auth(cb, cs, sFilter);
                        break;
                       
                    case "PRJ_PRODUCT":
                        SetProject_Product(cb, cs, sFilter);
                         break;

                    case "PRJ_PRODUCT_PACK":
                        SetProject_Product_Pack(cb, cs, sFilter);
                        break;

                    case "PRJ_PRODUCT_PILOT":
                        SetProject_Product_PILOT(cb, cs, sFilter);
                        break;
                    case "cboRout":
                        SetRout(cb, cs, sFilter);
                        break;
                    case "cboRoutGroup":// 라인에 따른 Rout 중복 제거 조회
                        SetRoutGroup(cb, cs, sFilter);
                        break;
                    case "cboProduct":
                    case "PRODUCT":
                        SetProduct(cb, cs, sFilter);
                        break;
                    case "PRODUCTMULTI":
                        SetProductMulti(cb, cs, sFilter);
                        break;
                    case "cboProductRout":// Rout에 따른 제품 조회
                        SetProductRout(cb, cs, sFilter);
                        break;
                    case "cboPrdtClass":
                    case "PRDTCLASS":
                        SetProductClass(cb, cs, sFilter);
                        break;
                    case "cboPrdtClassByProcId":
                        setProductClassByProcId(cb, cs, sFilter);
                        break;
                    case "cboPRJModelPack":
                        setPRJModelPack(cb, cs, sFilter);
                        break;
                    case "cboPilotProc":
                    case "PILOTPROC":
                        SetPilotProc(cb, cs, sFilter);
                        break;
                    case "PILOTLINE":
                        SetPilotLine(cb, cs, sFilter);
                        break;
                    case "cboProducts":
                        SetProducts(cb, cs, sFilter);
                        break;
                    case "cboProductPilot":
                        SetPilotProduct(cb, cs, sFilter);
                        break;
                    case "cboProcessPack":
                        SetProcessPack(cb, cs, sFilter);
                        break;
                    case "PROCBYPCSGID":
                        SetByPcsgid(cb, cs, sFilter);
                        break;                        
                    case "cboProcessRout":
                    case "PROCESSROUT":
                        SetProcessRout(cb, cs, sFilter);
                        break;
                    case "cboProductModel":
                    case "PRODUCTMODEL":
                        SetProductModel(cb, cs, sFilter);
                        break;                        
                    case "cboWork":
                    case "WORK":
                        SetWork(cb, cs, sFilter);
                        break;
                    case "cboReason":
                    case "cboHoldReason":
                    case "cboUnHoldReason":
                    case "REASON":
                        SetReason(cb, cs, sFilter);
                        break;
                    case "HOLD_REASON":
                        SetHoldReason(cb, cs, sFilter);
                        break;
                    case "cboWordCenter":
                    case "WORKCENTER":
                        SetWorkCenter(cb, cs, sFilter);
                        break;
                    case "cboWorkOrder":
                    case "WORKORDER":
                        SetWorkOrder(cb, cs, sFilter);
                        break;                   
                    case "SRSTANK":
                        SetSRSTank(cb, cs, sFilter);
                        break;
                    case "cboDepartment":
                    case "DEPARTMENT":
                        SetDepartment(cb, cs, sFilter);
                        break;
                    case "cboMoveToArea":
                    case "MOVETOAREA":
                        SetMoveToShop(cb, cs, sFilter);
                        break;
                    case "cboFromArea":
                    case "FROMAREA":
                        SetMoveToShop(cb, cs, sFilter);
                        break;
                    case "cboLotType":
                    case "LOTTYPE":
                        SetLotType(cb, cs, sFilter);
                        break;
                    case "cboWoType":
                    case "WOTYPE":
                        SetWoType(cb, cs, sFilter);
                        break;
                    case "cboExceptArea":
                        SetExpectArea(cb, cs, sFilter);
                        break;
                    case "cboLabelName":                    
                        SetLabelName(cb, cs, sFilter);
                        break;
                    case "cboLabelVersion":
                        SetLabelVersion(cb, cs, sFilter);
                        break;
                    case "LABELCODE_BY_PROD":
                        SetLabelCodeByProd(cb, cs, sFilter);
                        break;
                    case "LABELCODE_BY_PROD_MULTI":
                        SetLabelCodeByProdMulti(cb, cs, sFilter);
                        break;
                    case "cboProcessSeq":
                        SetProcessSeq(cb, cs, sFilter);
                        break;
                    case "PRODUCT_BY_BOM":
                        SetProductByBom(cb, cs, sFilter);
                        break;
                    case "cboVDEquipmentSegment":
                        SetVDEquipmentSegment(cb, cs, sFilter);
                        break;
                    case "CboProdCbo":
                        SetProdCbo(cb, cs, sFilter);
                        break;
                    case "CboErpStaus":
                        SetErpSatus(cb, cs, sFilter);
                        break;
                    case "cboVDProcess":
                        SetVDProcess(cb, cs, sFilter);
                        break;
                    case "cboVDEquipment":
                        SetVDEquipment(cb, cs, sFilter);
                        break;
                    case "cboVDEquipmentElec":
                        SetVDEquipmentElec(cb, cs, sFilter);
                        break;
                    case "LABELCODE_BY_SHIPTO_PRODID":
                        SetLabelCode_Shipto_prodid(cb, cs, sFilter);
                        break;
                    case "PROC_USER":
                        SetProcUser(cb, cs, sFilter);
                        break;
                    case "SHOPRELEATION":
                        SetShopRelation(cb, cs, sFilter);
                        break;
                    case "EQPT_CURR_MOUNT_MTRL_CBO":
                        SetEqptMountPsts(cb, cs, sFilter);
                        break;
                    case "EQPT_CURR_MOUNT_MTRL_CBO_SRC":
                        SetEqptMountPstsSRC(cb, cs, sFilter);
                        break;
                    case "EQPT_CURR_MOUNT_MTRL_CBO_STP":
                        SetEqptMountPstsSTP(cb, cs, sFilter);
                        break;
                    case "BOM_MTRL":
                        SetBOMMaterial(cb, cs, sFilter);
                        break;
                    case "SLOC":
                        SetcboLoc(cb, cs, sFilter);
                        break;
                    case "cboLocFrom":
                    case "FROM_SLOC":                
                        SetcboLocFrom(cb, cs, sFilter);
                        break;
                    case "cboLocFromPack":
                    case "FROM_SLOC_PACK":
                        SetcboLocFromPack(cb, cs, sFilter);
                        break;
                    case "CboSloc":
                        SetSloc(cb, cs, sFilter);
                        break;
                    case "cboLocTo":
                        SetcboLocTo(cb, cs, sFilter);
                        break;
                    case "cboLocToPack":
                        SetcboLocToPack(cb, cs, sFilter);
                        break;
                    case "FROMSLOC_BY_AREA":
                        SetcboLocFrom_ByArea(cb, cs, sFilter);
                        break;
                    case "TOSLOC_BY_AREA":
                        SetcboLocTo_ByArea(cb, cs, sFilter);                        
                        break;
                    case "SLOC_BY_TOSLOC":
                        SetcboLoc_by_toSloc(cb, cs, sFilter);
                        break;
                    case "SLOC_BY_TOSLOC_PROC":
                        SetcboLoc_by_toSlocProc(cb, cs, sFilter);
                        break;
                    case "SLOC_BY_COST":
                        SetcboCostLoc(cb, cs, sFilter);
                        break;
                    case "cboComp":
                        SetcboComp(cb, cs, sFilter);
                        break;
                    case "cboCompPack":
                        SetcboCompPack(cb, cs, sFilter);
                        break;
                    case "SHIPTO_CP":  //출하처 콤보 - CELL포장 전용
                        SetcboSHIPTO(cb, cs, sFilter);
                        break;
                    case "PROD_MDL":  //제품 콤보 - CELL포장 전용
                        SetcboPROD_MDL(cb, cs, sFilter);
                        break;
                    case "PROJECT_CP":  //프로젝트 - CELL포장 전용
                        SetcboPRJT_CP(cb, cs, sFilter);
                        break;                        
                    case "EQPT_CURR_MOUNT_MTRL_ECLD_CBO":
                        SetEqptMountMtrlPsts(cb, cs, sFilter);
                        break;
                    case "TRANSLOC":
                    case "cboTransLoc":
                        SetShipToInfo(cb, cs, sFilter);
                        break;
                    case "SHIPTO_BY_FROMAREAID":
                        SetShipToInfo_BY_FROMAREAID(cb, cs, sFilter);
                        break;
                    case "cboStatus":
                        SetStatus(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_BY_EQGRID":  // 설비 그룹으로 라인 조회 (STACKING & FOLDING)
                        SetEquipmentSegmentByEQSGID(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_EQSGID":
                        SetEquipmentByEQSGID(cb, cs, sFilter); // 설비 그룹, 라인.. 등으로 설비 조회 (STACKING & FOLDING)
                        break;
                    case "cboAreaSRS":
                        SetAreaSRS(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegmentSRS":
                        SetEquipmentSegmentSRS(cb, cs, sFilter);
                        break;
                    case "cboModelSRS":
                        SetModelSRS(cb, cs, sFilter);
                        break;
                    case "cboOutModelSRS":
                        SetOutModelSRS(cb, cs, sFilter);
                        break;
                    case "cboPrdMixProdid":
                        SetOutProdMixProdid(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSlitter":
                        SetEquipmentSlitter(cb, cs, sFilter);
                        break;
                    case "cboEqptModel":
                        SetEqptModel(cb, cs, sFilter);
                        break;
                    case "ALLAREA":
                    case "cboAreaAll":
                        SetAllArea(cb, cs, sFilter);
                        break;
                    case "ACTIVITIREASON":
                    case "cboActivitiReason":
                        SetActivitiReason(cb, cs, sFilter);
                        break;
                    case "CBO_AREA_ACTIVITIREASON": // 동별 HOLD코드
                        SetAreaActivitiReason(cb, cs, sFilter);
                        break;
                    case "cboActivitiReasonMTRL": // 자재 활동ID
                        SetActivitiReason_MTRL(cb, cs, sFilter);
                        break;
                    case "YIELDREASON": // 수율반영폐기에서 사유
                        SetActivitiReason_YIELD(cb, cs, sFilter);
                        break;
                    case "YIELDCAUSEPROC": // 수율반영폐기에서 원인공정
                        SetActivitiCauseProc(cb, cs, sFilter);
                        break;
                    case "CboSearchModel":
                        SetSearchModel(cb, cs, sFilter);
                        break;
                    case "cboOutLine": // SRS포장출고, 선분산포장출고
                        SetOutLine(cb, cs, sFilter);
                        break;
                    case "cboLotStatus": // SRS포장출고
                        SetLotStatus(cb, cs, sFilter);
                        break;
                    case "cboElecWareHouse": // 전극창고
                        SetElecWareHouse(cb, cs, sFilter);
                        break;
                    case "cboElecRack": // 전극창고 Rack ID
                        SetElecRackID(cb, cs, sFilter);
                        break;
                    case "WOINPUTPRODUCT":  // 조립 WO반제품코드
                        SetWorkOrderInputProduct(cb, cs, sFilter);
                        break;
                    case "COST_CENTER":  // 코스트센터
                        SetCostCenter(cb, cs, sFilter);
                        break;
                    case "MOVERECEIVE":  // 인수이력 조회시
                        SetMoveReceive(cb, cs, sFilter);
                        break;
                    case "cboWipStat":  // wip 상태
                        SetWipStat(cb, cs, sFilter);
                        break;
                    case "cboProcessByAreaid":  // wip 상태
                        SetProcessByAreaid(cb, cs, sFilter);
                        break;
                    case "cboMixProcess":  // 믹서 공정
                        SetMixProcess(cb, cs, sFilter);
                        break;
                    case "cboInspection": //VD QA 대상 LOT조회
                        SetInspection(cb, cs, sFilter);
                        break;
                    case "cboInputArea": // 외주 전극 입고 동
                        SetInputArea(cb, cs, sFilter);
                        break;
                    case "cboProcid": // 외주 전극 입고 동
                        SetInputProcess(cb, cs, sFilter);
                        break;
                    case "PRJT_NAME": // 프로젝트명
                        SetPrjtName(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_PLANT":
                        SetLine_Plant(cb, cs, sFilter);
                        break;
                    case "cboModelMerge":
                        SetModelMerge(cb, cs, sFilter);
                        break;
                    case "FROMSHOP":
                    case "cboShopFrom":
                        SetShopFrom(cb, cs, sFilter);
                        break;
                    case "cboLastLoss":
                        SetLastLoss(cb, cs, sFilter);
                        break;
                    case "cboLossEqsgProc":
                        SetLossEqsgProc(cb, cs, sFilter);
                        break;
                    case "cboVDFloor":
                        SetVDFloor(cb, cs, sFilter);
                        break;
                    case "cboMixProd":
                        SetMixProd(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_EXCEPT":
                        SetEquipt_Except(cb, cs, sFilter);
                        break;
                    case "PLANT_AREA":
                        SetPlant_Area(cb, cs, sFilter);
                        break;
                    case "CORE_RADIUS": // 코어 반지름
                        SetCoreRadius(cb, cs, sFilter);
                        break;
                    case "COMMCODE_WITHOUT_CODE":
                        SetCommonCodeWithoutCode(cb, cs, sFilter);
                        break;
                    case "WORKTYPE_COMMCODE_CBO":
                        SetCommonCodeWorktypeCode(cb, cs, sFilter);
                        break;
                    case "SpecialResonCodebyAreaCode":
                        SetSpecialResonCodebyAreaCode(cb, cs, sFilter);
                        break;

                    case "cboLocation":
                    case "LOCATION":
                        SetAreaLocationCode(cb, cs, sFilter);
                        break;
                    case "cboVDArea":
                        SetVdArea(cb, cs, sFilter);
                        break;
                    case "cboMixerEquipment":
                        setMixerEquipment(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_EXCLUDE_LINE":
                        SetLineExcludeLine(cb, cs, sFilter);
                        break;
                    case "LINEBYSHOP":
                        SetLineByShop(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegmentAssy":
                        SetLineAssy(cb, cs, sFilter);
                        break;                    
                    case "cboEquipmentSegmentForm":
                        SetLineForm(cb, cs, sFilter);
                        break;
                    case "cboPrjtName":
                        SetPrjtName(cb, cs);
                        break;
                    case "cboLossCodeProc":
                        SetLossCodeProc(cb, cs, sFilter);
                        break;
                    case "cboLossCodeProcPack":
                        SetLossCodeProcPack(cb, cs, sFilter);
                        break;
                    case "cboWorkSupplier":
                        SetWorkSupplier(cb, cs, sFilter);
                        break;
                    case "cboInboxType":
                        SetInboxType(cb, cs, sFilter);
                        break;
                    case "cboDsfWarehouse": // DSF 대기창고 관리  - 창고 Combo
                    case "DSF_WAREHOUSE":
                        SetDsfWarehouse(cb, cs, sFilter);
                        break;
                    case "cboDsfWarehouseRack": // DSF 대기창고 관리  - 위치(Rack) Combo
                    case "DSF_WAREHOUSE_RACK":
                        SetDsfWarehouseRack(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_NT_NEW":
                        SetEquipment_NT_NEW(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_BY_EQSGID_NEW":
                        SetEquipmentByEQSGID_NEW(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_NEW":
                        SetEquipment_NEW(cb, cs, sFilter);
                        break;
                    case "cboEquipmentAssy":
                        SetEquipmentAssy(cb, cs, sFilter);
                        break;
                    
                    case "REQ_CODE": // 활성화 후공정 물품청구코드
                        SetReqCode(cb, cs, sFilter);
                        break;
                    case "FORM_MOVE_COMMCODES": // 활성화 후공정 초소형 이동코드
                        SetFormMoveCodeCR(cb, cs, sFilter);
                        break;
                    case "PROCESS_SORT": // 활성화 후공정 공정 정렬
                        SetProcess_Sort(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_FORM": // 활성화 후공정 공정 정렬
                        SetLine_Form(cb, cs, sFilter);
                        break;
                    case "C_PRODUCT_TRANSFER":  // c 생산 인계작업장
                        SetCProductTransfer(cb, cs, sFilter);
                        break;
                    case "PROCESS_MOVE": // 활성화 후공정 공정 정렬
                        SetProcess_Move(cb, cs, sFilter);
                        break;
                    case "TARGET_PROCESS": // 활성화 후공정 재공이동 대상공정
                        SetTaget_Procss(cb, cs, sFilter);
                        break;
                    case "EQPT_CURR_MOUNT_MTRL_CBO_ALL":    // 투입위치 콤보 전체 조회
                        SetAllEqptMountPsts(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_WITHOUT_SEL_EQSGID":
                        SetEquipmentSegmentWithoutSelEQSGID(cb, cs, sFilter);
                        break;
                    case "COST_SLOC":   //비용저장위치
                        SetCostSloc(cb, cs, sFilter);
                        break;
                    case "PILOT_COST_SLOC":   // 시생산 비용저장위치
                        SetPilotCostSloc(cb, cs, sFilter);
                        break;
                    case "SLOC_BY_GMES_USE":
                        SetSlocByGmesUse(cb, cs, sFilter);
                        break;
                    case "CPROD_STATUS": //C생산 관리 상태
                        SetCProdStatus(cb, cs, sFilter);
                        break;
                    case "CPROD_MAGAZINE_LINE": //C생산 매거진 출고 라인
                        SetCProdMagLine(cb, cs, sFilter);
                        break;
                    case "CPROD_LOCATION": //C생산 작업장 상태
                        SetCProdLocation(cb, cs, sFilter);
                        break;
                    case "FORM_WRK_TYPE_CODE": //활성화 후공정 작업구분
                        SetFormWrkTypeCode(cb, cs, sFilter);
                        break;
                    case "FORM_WRK_TYPE_CODE_LINE": //활성화 후공정 작업구분
                        SetFormWrkTypeLineCode(cb, cs, sFilter);
                        break;
                    case "FORM_GRADE_TYPE_CODE": //활성화 후공정 작업구분
                        SetFormGradeTypeCode(cb, cs, sFilter);
                        break;
                    case "FORM_GRADE_TYPE_CODE_LINE": //활성화 후공정 작업구분
                        SetFormGradeTypeLineCode(cb, cs, sFilter);
                        break;

                    case "cboScrapAct": //활성화 후공정 폐기이력구분(폐기/폐기취소
                        SetScrapActivity(cb, cs, sFilter);
                        break;
                    case "AREA_COMMON_CODE":    // 동별 공통코드
                        SetAreaCommonCodeByTypeCode(cb, cs, sFilter);
                        break;
                    case "cboDefectType":    // 팩_폐기공정
                        SetDefectType(cb, cs, sFilter);
                        break;
                    case "cboTransTabCellType":    // C생산 이력조회 CELL TYPE 콤보
                        SetTransTabCellType(cb, cs, sFilter);
                        break;
                    case "EQUIPMENT_MAIN_LEVEL":
                        SetEquipmentMainLevel(cb, cs, sFilter);
                        break;
                    case "AREA_COM_CODE_WORKTYPE_CBO":    // C생산 이력조회 CELL TYPE 콤보
                        SetAreaComCodeWorkType(cb, cs, sFilter);
                        break;
                    case "cboFloor":    // 동별 창고 층 정보
                        SetFloor(cb, cs, sFilter);
                        break;
                    case "QMS_INSPEC_COMMCODES":    // QMS 의뢰구분 콤보
                        SetQmsInspCommonCodeS(cb, cs, sFilter);
                        break;
                    case "cboProcessSegment":
                        SetProcessSegment(cb, cs, sFilter);
                        break;
                    case "PROCESSSEGMENTLINE":
                        SetProcessSegmentLine(cb, cs, sFilter);
                        break;
                    case "PRJT_NAME_ELEC": // 극성별 프로젝트명
                        SetPrjtNameElec(cb, cs, sFilter);
                        break;
                    case "PRJT_NAME_BY_PRODCODE": // 프로젝트명별 제품코드 
                        SetPrjtNameByProductCode(cb, cs, sFilter);
                        break;
                    case "PRDT_LANE": // 제품별 LANE수
                        SetProductLane(cb, cs, sFilter);
                        break;
                    case "AREA_PROCESS_SORT": // 활성화 후공정 동별 공정
                        SetAREA_Process_Sort(cb, cs, sFilter);
                        break;
                    case "PROCESS_EQUIPMENT": // 활성화 후공정 공정별 LINE
                        SetPROCESS_EQUIPMENTSEGMENT(cb, cs, sFilter);
                        break;
                    case "POLYMER_PROCESS": // 활성화 후공정 폴리머 공정 전체
                        SetPOLYMER_PROCESS(cb, cs, sFilter);
                        break;
                    case "POLYMER_PROCESS_NO_SELECT": // 활성화 후공정 폴리머 공정 전체(자동 조회없음) 
                        SetPOLYMER_PROCESS_NO_SELECT(cb, cs, sFilter);
                        break;
                    case "POLYMER_PROCESS_AREA": // 활성화 후공정 폴리머 공정에 대한 동
                        SetPOLYMER_PROCESS_AREA(cb, cs, sFilter);
                        break;
                    case "POLYMER_PROCESS_AREA_EQSG": //활성화 후공정 폴리머 공정, 동에 대한 라인
                        SetPOLYMER_PROCESS_AREA_EQSG(cb, cs, sFilter);
                        break;
                    case "FROM_EQUIPMENTSEGMENT_CPROD": // C 생산 입고 인계라인 콤보
                        SetCprodFromEqsgID(cb, cs, sFilter);
                        break;
                    case "FROM_EQUIPMENT_CPROD": // C 생산 입고 인계설비 콤보
                        SetCprodFromEqptID(cb, cs, sFilter);
                        break;
                    case "POLYMER_EQUIPMENT":
                        SetEquipment_PROCESS(cb, cs, sFilter);
                        break;
                    case "INBOX_STAT":
                        INBOXStat(cb, cs, sFilter);
                        break;
                    case "PROCESS_MOBILE_BOX": // 소형 포장 공정
                        SetMobileBoxProcess(cb, cs, sFilter);
                        break;
                    case "TO_PROCID_CPROD": // C 생산 대상 공정 콤보
                        SetCprodToProcID(cb, cs, sFilter);
                        break;
                    case "THICK_PROCESS": // C 생산 대상 공정 콤보
                        SetCommonCode(cb, cs, sFilter);
                        break;
                    case "POLYMER_PROCESS_ROUTE": // 활성화 후공정 폴리머 ROUT에 대한 공정 전체
                        SetPOLYMER_PROCESS_ROUTE(cb, cs, sFilter);
                        break;

                    case "MOVE_AREA": // 인수, 인계 공정 전체
                        SetPOLYMER_MOVE_AREA(cb, cs, sFilter);
                        break;
                    case "PROD_LOT_CTNR": //활성화 후공정 생산LOT에 대한 대차ID 정보 조회
                        SetProdLot_Ctnr(cb, cs, sFilter);
                        break;
                    case "cboPrjtNameMtrlRecieve": //입고가능자재LOT 탭 프로젝트 명 콤보박스
                        SetPrjtNameMtrlInput(cb, cs, sFilter);
                        break;
                    case "POLYMER_DEFECT_GROUP":  //활성화 후 공정 불량그룹 
                        SetDefectGroup(cb, cs, sFilter);
                        break;
                    case "DEFECT_WH_ID": //활성화 후공정 불량창고
                        SetDefectWh_Id(cb, cs, sFilter);
                        break;
                    case "COMMCODES_PRDT_REQ_PRCS":
                        SetCommonCode_PRDT_REQ_PRCS(cb, cs, sFilter);
                        break;
                    case "PKG_LINE": // 활성화 후공정 폴리머 PKG LINE 정보 조회
                        SetPOLYMER_PKGLINE(cb, cs, sFilter);
                        break;
                    case "PROCESS_POUCH":    // 조립 파우치 공정
                        SetProcess_Pouch(cb, cs, sFilter);
                        break;
                    case "PACKWAY":
                        SetPackWay(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_AUTO": // 활성화 자동차 후공정 라인
                        SetEQUIPMENTSEGMENT_AUTO(cb, cs, sFilter);
                        break;
                    case "WH_QA":    //폴란드 증설 : 라미 모니터링 창고재고정보 콤보박스  
                        SetWhQA(cb, cs);
                        break;
                    case "MEB_WH_QA":    //폴란드 증설 : MEB 라미, 노칭 창고재고정보 콤보박스  
                        SetMEBWhQA(cb, cs, sFilter);
                        break;

                    case "ELTR_AREA_FOR_ASSY":  // 전극 동정보 (조립에서 사용)
                        SetEltrAreaForAssy(cb, cs, sFilter);
                        break;
                    case "CWALAMIPORT":    //폴란드 증설 : 라미 포트ID
                        SetLamiPort(cb, cs, sFilter);
                        break;
                    case "CWALAMISTOCKER":    //폴란드 증설 : 라미 스토커
                        SetLamiStocker(cb, cs, sFilter);
                        break;
                    case "CWAMCSEQUIPMENT":    //폴란드 증설 : 설비정보
                        SetMCSEquipment(cb, cs, sFilter);
                        break;
                    case "CWAMCSEQUIPMENT_MEB":    //폴란드 증설 : MEB 노칭 수동출고 설비정보
                        SetMCSEquipment_MEB(cb, cs, sFilter);
                        break;
                    case "CWAMCSWORKORDER_MEB":    //폴란드 증설 : MEB 노칭 수동출고 WO 정보조회
                        SetMCSWorkOrder_MEB(cb, cs, sFilter);
                        break;
                    case "CWAMCSMTRL_MEB":    //폴란드 증설 : MEB 노칭 수동출고 자재 정보조회
                        SetMCSMtrl_MEB(cb, cs, sFilter);
                        break;
                    case "CWAMCSPORT_MEB":    //폴란드 증설 : MEB 노칭 수동출고 목적지포트 정보조회
                        SetMCSPort_MEB(cb, cs, sFilter);
                        break;

                    case "ERLY_DETT_ITEM":  //선감지 항목 ㅈ회
                        SetErly_Dett_Item(cb, cs, sFilter);
                        break;
                    case "EQUIPMENTSEGMENT_PROC":  //공정으로 라인 조회
                        SetEquipmentSegmentProc(cb, cs, sFilter);
                        break;
                    case "PROCESS_PCSGID":
                        SetProcessPCSGID(cb, cs, sFilter);
                        break;
                    case "PROCESS_PCSGID_V":
                        SetProcessPCSGID_V(cb, cs, sFilter);
                        break;
                    case "EQPT_CURR_MOUNT_MTRL_CBO_L":
                        SetEqptMountPstsExcludeScreenDisp(cb, cs, sFilter);
                        break;
                    case "SHIFTGRCODE":
                        SetShiftGrCode(cb, cs, sFilter);
                        break;
                    case "RWK_PRJT_CBO_L":
                        SetRwkProjectByEquipmentSegment(cb, cs, sFilter);
                        break;
                    case "HOLD_CODE_LVL1":
                        SetHoldCode_Lvl1(cb, cs, sFilter);
                        break;
                    case "HOLD_CODE_LVL2":
                        SetHoldCode_Lvl2(cb, cs, sFilter);
                        break;
                    case "PROCESS_BY_AREAID_PCSG":
                        SetProcessByAreaPcsg(cb, cs, sFilter);
                        break;
                    case "TOOL_TYPE_CODE":
                        SetToolTypeCode(cb, cs, sFilter);
                        break;
                    case "PRODUCTION_PLAN_CATEGORY":
                        SetProductionPlanCategory(cb, cs, sFilter);
                        break;
                    case "PROCESS_TOOL":
                        SetProcessTool(cb, cs, sFilter);
                        break;
                    case "PROCESSWH":
                        SetProcessWH(cb, cs, sFilter);
                        break;
                    case "PROCESS_BY_SBL_ABNORM_BAS":
                        SetProcessBySBL_ABNORM_BAS(cb, cs, sFilter);
                        break;
                    // 2020-06-12
                    // 염규범S
                    // PACK 라벨 프린트 관리를 통한 콤버 생성
                    case "LABELCODE_BY_EQPTID":
                        SetLabelEqptCbo(cb, cs, sFilter);
                        break;
                    //2020-07-02
                    //김준겸A
                    // [CWA PI]  개별 셀 (WIP) Lot 을 Box or Pallet 포장가능 하도록 기능 구현 CSR ID : C20200602-000008 요청자 : 오경석 책임
                    case "cboWHID":
                        SetPackWHID(cb, cs, sFilter);
                        break;
                    case "cboRackId":
                        SetPackRackID(cb, cs, sFilter);
                        break;
                    case "OFF_LINE_USE_SLOC": // OFF LINE 사용 저장위치
                        SetOffLineSLocCbo(cb, cs, sFilter);
                        break;
                    case "PROCESS_BY_AREAID_PCSG_ETC":
                        SetProcessByAreaEtc(cb, cs, sFilter);
                        break;
                    case "AREA_PACK":
                        SetAreaPack(cb, cs, sFilter);
                        break;
                    case "cboMTGRID":
                        SetMaterialGroupId(cb, cs, sFilter);
                        break;
                    case "CUSTID":
                        SetcboCustID(cb, cs, sFilter);
                        break;
                    case "LOGIS_PROD":
                        setComboLogisProdIdPack(cb, cs, sFilter);
                        break;
                    case "LOGIS_EQSG_FOR_MEB":
                        setComboLogisEqsgIdPack(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetMaterialGroupId(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            cbo.Text = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MATERIAL_GROUP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "MTGRNAME";
            cbo.SelectedValuePath = "MTGRID";

            cbo.ItemsSource = AddStatus(dtResult, cs, "MTGRID", "MTGRNAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetPackRackID(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";                
                RQSTDT.Columns.Add("LANGID", typeof(string));                
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();                
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WHID"] = sFilter[0] == "" ? null : sFilter[0]; 
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACK_RACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPackWHID(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sFilter[0] == "" ? LoginInfo.CFG_AREA_ID : sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO_PACK", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetHoldCode_Lvl1(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_DFCT_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetHoldCode_Lvl2(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["DFCT_CODE"] = sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_HOLD_DFCT_CODE_LVL2_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetTransTabCellType(C1ComboBox cb, ComboStatus cs, string[] sFilter)
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BC_FC_CELL_TYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cb.DisplayMemberPath = "CBO_NAME";
            cb.SelectedValuePath = "CBO_CODE";
            cb.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cb.SelectedIndex = 0;
        }

        private void SetCProdMagLine(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("FLOOR", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0];
            dr["FLOOR"] = sFilter[1];
            dr["PROCID"] = sFilter[2];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQSG_BY_AREAID_FLOOR_PROCID", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count < 1)
            {
                RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQSG_BY_AREAID_FLOOR_PROCID", "RQSTDT", "RSLTDT", RQSTDT);
            }

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 1;
        }

        private void SetCProdLocation(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter[0];
            dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_CPROD", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetCProdStatus(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCDSEQ", typeof(decimal));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter[0];
            dr["CMCDSEQ"] = Util.NVC_Decimal(sFilter[1]);
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_STAT_CPROD", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                CommonCombo _combo = new CMM001.Class.CommonCombo();

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
                CommonCombo _combo = new CMM001.Class.CommonCombo();

                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    _combo.SetCombo(cbChild);
                }
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
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;

                //if (!sFilter[0].Equals(""))
                //{
                //    dr["SHOPID"] = sFilter[0];
                //}
                //else
                //{
                //    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //}
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        /// <summary>
        /// CELL 포장에서 AREAID 선택시에 AREAID^SHOPID 를 리턴함.
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
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

        /// <summary>
        /// CELL 포장에서 AREAID 선택시에 AREAID^SHOPID 를 리턴함.
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetLine_CP(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["EXCEPT_GROUP"] = "VD";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetLine_FCS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PCSGID"] = "F";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "EQSGNAME";
                cbo.SelectedValuePath = "EQSGID";
                cbo.ItemsSource = AddStatus(dtResult, cs, "EQSGID", "EQSGNAME").Copy().AsDataView();

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
                Util.MessageException(ex);
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
        private void SetAreaPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dtDr = dt.NewRow();
                    dtDr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr);

                    cbo.ItemsSource = DataTableConverter.Convert(dt);
                    cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.ItemsSource = DataTableConverter.Convert(dtResult);
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

        private void SetLineForPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                cbo.SelectedIndex = 0;

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
                dr["SHOPID"] = string.IsNullOrWhiteSpace(sFilter[0])? null: sFilter[0];               
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

        private void SetLineAssy(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {

            // CNJ 원형9,10호기 와인더(in-Line)화면 전용 4개 input  2017.03.05 강민준
            // String[] sFilter = { LoginInfo.CFG_AREA_ID , gubun, _processCode, "CST_ID"};
            int iCnt = sFilter.Length;

            if (iCnt == 4)
            {
                try
                {
                    DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("PROD_GROUP", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));
                    inDataTable.Columns.Add("OUT_LOT_TYPE", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = sFilter[0];
                    dr["PROD_GROUP"] = sFilter[1];
                    dr["PROCID"] = sFilter[2];
                    dr["OUT_LOT_TYPE"] = sFilter[3];

                    inDataTable.Rows.Add(dr);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(inDataTable);
                    string xml = ds.GetXml();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR_WND", "RQSTDT", "RSLTDT", inDataTable);

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
            else
            {
                try
                {
                    DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("PROD_GROUP", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = sFilter[0];
                    dr["PROD_GROUP"] = sFilter[1];
                    dr["PROCID"] = sFilter[2];


                    inDataTable.Rows.Add(dr);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(inDataTable);
                    string xml = ds.GetXml();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", inDataTable);

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
        }
               

        private void SetLineForm(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                //inDataTable.Columns.Add("PROD_GROUP", typeof(string));
                //inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                //dr["PROD_GROUP"] = sFilter[1];
                //dr["PROCID"] = sFilter[2];
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

        private void SetPrjtName(C1ComboBox cbo, ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STOCK_PRJTNAME", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcessPCSGID_V(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_PCSGID_VERIFI", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcessERP(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ERPRPTIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["ERPRPTIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSERP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcessBox(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BOX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        if (dtResult.Rows[0]["CBO_CODE"].ToString() == LoginInfo.CFG_PROC_ID)
                        {
                            cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                        }
                        else
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }else
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
        
        private void SetProcessPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 1;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessRout(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["PRODID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["ROUTID"] = sFilter[3] == "" ? null : sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_ROUT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 1;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 1;
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

        private void SetProcessWithAreaNoLoginInfo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                
                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessWH(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_WITH_AREA_CWA", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    if (cbo.SelectedValue == null)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                        if (cbo.SelectedIndex < 0)
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

        private void SetProcessCWA(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_CWA", "RQSTDT", "RSLTDT", RQSTDT);

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
                Util.MessageException(ex);                
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

        /// <summary>
        /// 폴란드 증설 설비정보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMCSEquipment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTTYPE"] = sFilter[0];
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EQPTID", "RQSTDT", "RSLTDT", RQSTDT);

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


       
        /// <summary>
        /// 폴란드 증설  MEB 노칭 수동출고 설비정보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMCSEquipment_MEB(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
           

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
         
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_EQPTID", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 폴란드 증설  MEB 노칭 수동출고 WO정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMCSWorkOrder_MEB(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_WORKORDER", "RQSTDT", "RSLTDT", RQSTDT);

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


        /// <summary>
        /// 폴란드 증설  MEB 노칭 수동출고 자재정보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMCSMtrl_MEB(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_MTRL", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 폴란드 증설  MEB 노칭 수동출고 목적지 포트 정보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMCSPort_MEB(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_PORT", "RQSTDT", "RSLTDT", RQSTDT);

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
        

        private void SetEquipmentEqptLoss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["COATER_EQPT_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEquipment_PROCESS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = sFilter[0];
                 RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PROCESS_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetEquipment_EQSG_PROC(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["PROCID"] = sFilter[1] == "" ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void SetEquipment_LinGroup_PROC(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("LINE_GRUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["LINE_GRUP"] = sFilter[1] == "" ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_LINEGRUP_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEquipment_NT(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[2];
                dr["ELTR_TYPE_CODE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetOccurEqpt(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_OCCUREQP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetOccurPackEqpt(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_OCCUREQP_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEqptLoss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTLOSSCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (sFilter[0].Equals("popup"))
                {
                    DataRow row = dtResult.NewRow();
                    row["CBO_NAME"] = "RUN";
                    row["CBO_CODE"] = "RUN";
                    dtResult.Rows.InsertAt(row,0);

                    
                         
                }
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = sFilter[0].Equals("popup") ? 1 : 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetEqptLossDetl(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOSS_CODE"] = sFilter[0];
                dr["EQPTID"] = sFilter[1];
                dr["AREAID"] = sFilter[2];
                dr["PROCID"] = sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTLOSSDETLCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        //private void SetEqptAction(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        //{
        //    try
        //    {

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
        //        RQSTDT.Columns.Add("EQPTID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["LOSS_CODE"] = sFilter[0];
        //        dr["EQPTID"] = sFilter[1];
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTLOSSNOWORKCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cbo.DisplayMemberPath = "CBO_NAME";
        //        cbo.SelectedValuePath = "CBO_CODE";
        //        cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

        //        cbo.SelectedIndex = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void SetFCRCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FCRCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetStockSeq(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.ConvertEmptyToNull(sFilter[0]);
                dr["STCK_CNT_YM"] = Util.ConvertEmptyToNull(sFilter[1]);
                dr["STCK_CNT_CMPL_FLAG"] = Util.ConvertEmptyToNull(sFilter[2]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void SetCommonCodeAttr(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["CMCDTYPE"] = sFilter[0];
                dr["ATTRIBUTE1"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetCommonCodeAttr_MCS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["CMCDTYPE"] = sFilter[0];
                dr["ATTRIBUTE1"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1_MCS", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCommonCodeAttrs(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                string[] columnArr = { "CMCDTYPE", "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                for (int i = 0; i < columnArr.Length; i++)
                    RQSTDT.Columns.Add(columnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                for (int i = 0; i < sFilter.Length; i++)
                    dr[columnArr[i]] = string.IsNullOrEmpty(sFilter[i]) ? (object)DBNull.Value : sFilter[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetMaterialCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("PROD_LEVEL", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROD_LEVEL"] = sFilter[0];
            dr["EQSGID"] = sFilter[1];

            RQSTDT.Rows.Add(dr);


            //if (sFilter[0].Equals("IN"))
            //{
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("IN", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["IN"] = 1;
            //    RQSTDT.Rows.Add(dr);

            //}
            //else if(sFilter[0].Equals("OUT"))
            //{
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("OUT", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["OUT"] = 1;
            //    RQSTDT.Rows.Add(dr);
            //}



            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PACKED_SRS", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProdCodeByBiCellType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("BICELLTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["BICELLTYPE"] = sFilter[0].Equals("") ? null : sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_BICELLTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProcessSegmentByEqsgid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENT_BY_EQSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProcessSegmentByEqsgid_M_P(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PCSG_BY_EQSGID_M_P_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetByPcsgid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("PCSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PCSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetRouteByPcsgid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PCSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            dr["PCSGID"] = sFilter[1].Equals("") ? null : sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUTE_BY_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetRouteByModlPcsg(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PCSGID", typeof(string));
            RQSTDT.Columns.Add("PRJ_NAME", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            dr["PCSGID"] = sFilter[1].Equals("") ? null : sFilter[1];
            dr["PRJ_NAME"] = sFilter[2].Equals("") ? null : sFilter[2];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUTE_BY_PCSG_MODL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetRouteByModlPcsgProdId(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PCSGID", typeof(string));
            RQSTDT.Columns.Add("PRJ_NAME", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0];
            dr["PCSGID"] = sFilter[1].Equals("") ? null : sFilter[1];
            dr["PRJ_NAME"] = sFilter[2].Equals("") ? null : sFilter[2];
            dr["PRODID"] = sFilter[2].Equals("") ? null : sFilter[3];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUTE_BY_PCSG_MODL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }


        private void SetRouteByModlid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("MODLID", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["MODLID"] = sFilter[0].Equals("") ? null : sFilter[0];
            dr["PRODID"] = sFilter[1].Equals("") ? null : sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUTE_BY_MODLID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("ELEC", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ELEC"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }


        private void SetMaterialId(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
            dr["PROCID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["EQPTID"] = sFilter[2] == "" ? null : sFilter[2];
            RQSTDT.Rows.Add(dr);


            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProject_Model(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0] =="" ? null : sFilter[0];
            dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProject_Model_Auth(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";            
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            DataRow dr = RQSTDT.NewRow();            
            dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
            dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProject_Product(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROJECT_MODEL", typeof(string));
            RQSTDT.Columns.Add("PRDCLASS", typeof(string));


            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = null; // sFilter[0];
            dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
            dr["PROJECT_MODEL"] = sFilter[3] == ""? null : sFilter[3];
            dr["PRDCLASS"] = sFilter[4] == ""? null : sFilter[4];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJPRODUCT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.ItemsSource = null;

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProject_Product_Pack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROJECT_MODEL", typeof(string));
            RQSTDT.Columns.Add("PRDCLASS", typeof(string));
            RQSTDT.Columns.Add("PRODTYPE", typeof(string));


            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = null; // sFilter[0];
            dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
            dr["PROJECT_MODEL"] = sFilter[3] == "" ? null : sFilter[3];
            dr["PRDCLASS"] = sFilter[4] == "" ? null : sFilter[4];
            dr["PRODTYPE"] = "PROD";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJPRODUCT_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.ItemsSource = null;

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProject_Product_PILOT(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROJECT_MODEL", typeof(string));
            RQSTDT.Columns.Add("PRDCLASS", typeof(string));
            RQSTDT.Columns.Add("PILOT_GUBUN", typeof(string));

            //var obj = cbo.SelectedItem;
            //System.Data.DataRowView drv = obj as System.Data.DataRowView;
            //string value = drv[1].ToString();


            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = null; // sFilter[0];
            dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
            dr["PROJECT_MODEL"] = sFilter[3] == "" ? null : sFilter[3];
            dr["PRDCLASS"] = sFilter[4] == "" ? null : sFilter[4];
            dr["PILOT_GUBUN"] = sFilter[5] == "" ? "" : sFilter[5];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJPRODUCT_PILOT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.ItemsSource = null;

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetRout(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
            dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
            dr["PRODID"] = sFilter[2] == "" ? null : sFilter[2];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 1;
        }

        private void SetRoutGroup(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
            dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ROUT_GROUP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 1;
        }

        private void SetProduct(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[2] == ""? null : sFilter[2];
                dr["PROCID"] = sFilter[3] == "" ? null : sFilter[3];
                dr["MODLID"] =  sFilter[4] == "" ? null : sFilter[4];
                dr["AREA_TYPE_CODE"] = sFilter[5];
                dr["PRDT_CLSS_CODE"] = sFilter[6] == "" ? null : sFilter[6];
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetProductMulti(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["PROCID"] = sFilter[3] == "" ? null : sFilter[3];
                dr["MODLID"] = sFilter[4] == "" ? null : sFilter[4];
                dr["PRDT_CLSS_CODE"] = sFilter[5] == "" ? null : sFilter[5];
                dr["AREA_TYPE_CODE"] = sFilter[6];
                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //bool chk = dtResult.Rows.Count == 0 ? false : true;

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

        

        /// <summary>
        /// Rout에 따른 제품 조회
        /// DisplayMemberPath와 ValuePath 모두 CODE로 되어있음
        /// </summary>
        private void SetProductRout(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProductClass(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));                
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[2];
                dr["AREA_TYPE_CODE"] = sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void setProductClassByProcId(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["PROCID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["AREA_TYPE_CODE"] = sFilter[3];
                dr["SHOPID"] = sFilter[4] == "" ? null : sFilter[4];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void setPRJModelPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPilotProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));              
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PILOT_GUBUN", typeof(string));
                DataRow dr = RQSTDT.NewRow();               

                dr["LANGID"] = LoginInfo.LANGID;               
                dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["PILOT_GUBUN"] = sFilter[2] == "" ? "" : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PILOTPROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPilotLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PILOT_GUBUN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PILOT_GUBUN"] = sFilter[1] == "" ? "" : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_PILOT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                if(dtResult.Rows.Count == 0)
                {
                    cs = ComboStatus.NA;
                }

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
       
        private void SetPilotProduct(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["MODLID"] = sFilter[3] == "" ? null : sFilter[3];
                dr["AREA_TYPE_CODE"] = sFilter[4];
                dr["PRDT_CLSS_CODE"] = sFilter[5] == "" ? null : sFilter[5];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProductModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];                              
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTMODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        //DA_BAS_SEL_PROD_CBO
        private void SetProducts(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["EQPTID"] = sFilter[2] == "" ? null : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetProcSel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProdCbo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTMODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEqptModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
             //   RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["EQSGID"] = sFilter[0] == "" ? null : sFilter[0];
           //     dr["EQPTID"] = sFilter[2] == "" ? null : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTMODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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




        private void SetWork(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter[0]; //REWORK_JUDGE,SCRAP_JUDGE
            RQSTDT.Rows.Add(dr);


            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetReason(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));                
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["ACTID"] = sFilter[2];                
                dr["DFCT_TYPE_CODE"] = sFilter[3];

                //dr["ACTID"] = sFilter.Length == 1 ? "DEFECT_LOT" : sFilter[1];  //sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_REASON_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        
        private void SetWorkCenter(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));               

                DataRow dr = RQSTDT.NewRow();

                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];

                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;                
                               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKCENTER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetWorkOrder(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("SHOPID", typeof(string)); 
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                //PACK DATA가 없어서 테스트를 위해 임시로 로직에서 값을 던져줌...
                //정상적으로 전환시 아래 주석 사용.
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["EQSGID"] = sFilter[0];

                //dr["SHOPID"] = sFilter[0];
                //dr["AREAID"] = sFilter[1];
                dr["EQSGID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKORDER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                if (dtResult.Rows.Count == 0)
                {
                    DataRow dr1 = dtResult.NewRow();
                    dr1["CBO_NAME"] = " - " + ObjectDic.Instance.GetObjectName("없음") + " -";
                    dr1["CBO_CODE"] = "";
                    dtResult.Rows.InsertAt(dr1, 0);

                    cbo.ItemsSource = dtResult.Copy().AsDataView();
                }
                else
                {
                    cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                }

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSRSTank(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SRSTANK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKCENTER_CBO", "RQSTDT", "RSLTDT", null);
        }

        private void SetDepartment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DEPARTMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKCENTER_CBO", "RQSTDT", "RSLTDT", null);
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

        private void SetMoveToShop(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetExpectArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOGIN_AREA", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOGIN_AREA"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EXCEPT_LOGINAREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetLabelName(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABELMASTER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 1; 
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void SetLabelVersion(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                dr["LABEL_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABELVERSION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetLabelCodeByProd(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = (sFilter[0] == "") || (sFilter[0] == "null") ? null : sFilter[0];
                dr["SHIPTO_ID"] = (sFilter[1] == "") || (sFilter[1] == "null") ? null : sFilter[1];
                dr["PROCID"] = (sFilter[2] == "") || (sFilter[2] == "null") ? null : sFilter[2];
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_TYPE_CODE"] = (sFilter[3] == "") || (sFilter[3] == "null") ? null : sFilter[3];
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLabelCodeByProdMulti(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));



                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = (sFilter[0] == "") || (sFilter[0] == "null") ? null : sFilter[0];
                dr["SHIPTO_ID"] = (sFilter[1] == "") || (sFilter[1] == "null") ? null : sFilter[1];
                dr["PROCID"] = (sFilter[2] == "") || (sFilter[2] == "null") ? null : sFilter[2];
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_TYPE_CODE"] = (sFilter[3] == "") || (sFilter[3] == "null") ? null : sFilter[3];
                dr["LABEL_CODE"] = (sFilter[4] == "") || (sFilter[4] == "null") ? null : sFilter[4];


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetProcessSeq(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_SEQ_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProductByBom(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WOID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_BOM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void SetVDEquipmentSegment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FOR_VD", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    if (dtResult.Select("CBO_CODE = '" + LoginInfo.CFG_EQSG_ID + "'").Length != 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                        if (cbo.SelectedIndex < 0)
                        {
                            cbo.SelectedIndex = 0;
                        }
                        return;
                    }

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
        //SetVDProcess
        private void SetVDProcess(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_FOR_VD", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;


                //if (!LoginInfo.CFG_PROC_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                //    if (cbo.SelectedIndex < 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                //    cbo.SelectedIndex = 0;
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetVDEquipment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                dr["EQSGID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_FOR_VD", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    if (dtResult.Select("CBO_CODE = '" + LoginInfo.CFG_EQPT_ID + "'").Length != 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                        if (cbo.SelectedIndex < 0)
                            cbo.SelectedIndex = 0;

                        return;
                    }
                    cbo.SelectedIndex = 0;

                }
                else
                {
                    cbo.SelectedIndex = 0;
                }

             //   cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboLocFrom(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];//Process.CELL_BOXING;                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FROMSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLocFromPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                //dr["PROCID"] = sFilter[1];//Process.CELL_BOXING;                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FROMSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLocTo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["SHOPID"] = sFilter[0];
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLocToPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["SHIP_TYPE_CODE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLoc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SLOC_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];                
                dr["AREAID"] = sFilter[1];
                dr["SLOC_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboCostLoc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SLOC_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = sFilter[0];
                dr["SLOC_ID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SLOC", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "SLOC_NAME";
                cbo.SelectedValuePath = "SLOC_ID";
                cbo.ItemsSource = AddStatus(dtResult, cs, "SLOC_ID", "SLOC_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboLocFrom_ByArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SLOC_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["SLOC_TYPE_CODE"] = sFilter[1] =="" ? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FROMSLOC_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLocTo_ByArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHIP_TYPE_CODE"] = sFilter[0];
                dr["FROM_AREAID"] = sFilter[1];
                dr["SHOPID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLoc_by_toSloc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("SLOC_TYPE_CODE", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_SLOC_ID"] = sFilter[0];
                dr["SLOC_TYPE_CODE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_TOSLOC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboLoc_by_toSlocProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("SLOC_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["TO_SLOC_ID"] = sFilter[1];
                dr["SLOC_TYPE_CODE"] = sFilter[2];
                dr["SHIP_TYPE_CODE"] = sFilter[3];
                dr["PROCID"] = sFilter[4];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_TOSLOC_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboCompPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_SLOC_ID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["SHIP_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPSLOC_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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
        /// <summary>
        /// 출하처 콤보 - CELL포장 전용
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetcboSHIPTO(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["EQSGID"] = sFilter[1];
                dr["AREAID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIPTO_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

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
        
        /// <summary>
        /// 모델에 해당하는 제품 콤보 - CELL포장 전용
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetcboPRJT_CP(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sFilter[1])? null : sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRJTNAME_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 모델에 해당하는 제품 콤보 - CELL포장 전용
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetcboPROD_MDL(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["MDLLOT_ID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODID_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetVDEquipmentElec(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_ELEC", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetSearchModel(C1ComboBox cbo, ComboStatus cs, String[] sFilter )
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[2];
                dr["PRDT_CLSS_CODE"] = sFilter[0].ToString().Equals("") ? null : sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResults = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_SH", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResults, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cbo.SelectedIndex = 0;

                //if (sFilter[0].ToString().Equals(""))
                //{
                    
                //}
                //else
                //{
                //    dr["PRDT_CLSS_CODE"] = sFilter[0];
                //    dr["EQSGID"] = sFilter[2];
                //    RQSTDT.Rows.Add(dr);

                //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_SH", "RQSTDT", "RSLTDT", RQSTDT);

                //    cbo.DisplayMemberPath = "CBO_NAME";
                //    cbo.SelectedValuePath = "CBO_CODE";
                //    cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                //    cbo.SelectedIndex = 0;
                //}
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLabelCode_Shipto_prodid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHIPTO_ID"] = sFilter[0];
                dr["PRODID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABELCODE_BY_SHIPTO_PRODID", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 공정별 사용자
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetProcUser(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                dr["PROCID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_USER_BYPROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// Shop 간 이동
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetShopRelation(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_RELATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 설비 별 투입 위치 코드 조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEqptMountPsts(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                dr["MOUNT_MTRL_TYPE_CODE"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 설비 별 투입 위치 코드 조회 SRC
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEqptMountPstsSRC(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                dr["MOUNT_MTRL_TYPE_CODE"] = sFilter[1];
                dr["MOUNT_PSTN_GR_CODE"] = sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_BY_GROUP", "RQSTDT", "RSLTDT", RQSTDT);

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
        /// <summary>
        /// 설비 별 투입 위치 코드 조회 STP
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEqptMountPstsSTP(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("MOUNT_PSTN_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                dr["MOUNT_MTRL_TYPE_CODE"] = sFilter[1];
                dr["MOUNT_PSTN_GR_CODE"] = sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_STP", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void SetEqptMountMtrlPsts(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                dr["MOUNT_MTRL_TYPE_CODE"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_ECLD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetShipToInfo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPTO_INFO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetShipToInfo_BY_FROMAREAID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHIP_TYPE_CODE"] = sFilter[0];
                dr["FROM_AREAID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        
        private void SetStatus(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_REPAIRE_STATUS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 작지 BOM 투입 자재 코드
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetBOMMaterial(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WOID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WO_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 라인 조회 시 설비 그룹에 존재하는 설비의 라인정보 조회 (STACKING & FOLDING 사용 : 동일 PROCID 코드이므로 구분 필요.)
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipmentSegmentByEQSGID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["EQGRID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_EQGRID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 설비그룹에 존재하는 설비정보 조회 (STACKING & FOLDING 사용 : 동일 PROCID 코드이므로 구분 필요.)
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipmentByEQSGID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["EQGRID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetHoldReason(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSACTIVITYREASON", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// SRS포장출고화면 -  SRS slitter Area
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetAreaSRS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_SRS", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// SRS포장출고화면 -  SRS slitter Line
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipmentSegmentSRS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_SRS", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// SRS포장출고화면 - SRS 전극창고 대기 LOT중 포장이 안된 모델
        /// 
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetModelSRS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("PRODLEVEL", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = "S9000";
                dr["WIPSTAT"] = Wip_State.WAIT;
                dr["PRODLEVEL"] = sFilter[0];
                dr["MODLID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MODEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        /// <summary>
        /// SRS포장출고화면 - 포장출고  Pallet조회
        /// 
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetOutModelSRS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODUCT_LEVEL", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODUCT_LEVEL"] = sFilter[0];
                dr["MODLID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MODEL_SRS_OUT", "RQSTDT", "RSLTDT", RQSTDT);

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
        /// <summary>
        /// 선분산출고 - 포장출고 이력조회 제품코드 콤보박스
        /// 
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetOutProdMixProdid(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("PRODUCT_LEVEL", typeof(string));
                //RQSTDT.Columns.Add("MODLID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["PRODUCT_LEVEL"] = sFilter[0];
                //dr["MODLID"] = sFilter[1];
                //RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WIP_MOVE_ORD_RPODID", "RQSTDT", "RSLTDT", null);

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


        private void SetEquipmentSlitter(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTIUSE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.SRS_SLITTING;
                dr["EQPTIUSE"] = "Y";
                dr["EQSGID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_SLITTER", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetActivitiReason_MTRL(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITIREASON_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetActivitiReason_YIELD(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITIREASON_YIELD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        
        private void SetActivitiCauseProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_CODE", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_CODE"] = sFilter[0];
                dr["ACTID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITICAUSEPROC_YIELD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void SetCommonCodeS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetCommonCode_PRDT_REQ_PRCS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_REQ_PRCS", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetQmsInspCommonCodeS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_QMS_INSP_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetOutLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SHIP_TYPE_CODE"] = sFilter[0];
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIPTO_FOR_SRS", "RQSTDT", "RSLTDT", RQSTDT);

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

        //private void SetOutMix(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("SHOPID", typeof(string));
        //        RQSTDT.Columns.Add("SHIP_TYPE_CODE", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["SHIP_TYPE_CODE"] = sFilter[0];
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIPTO_FOR_SRS", "RQSTDT", "RSLTDT", RQSTDT);

        //        cbo.DisplayMemberPath = "CBO_NAME";
        //        cbo.SelectedValuePath = "CBO_CODE";
        //        cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

        //        cbo.SelectedIndex = 0;


        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void SetElecWareHouse(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ELEC_PRDT_WH_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetElecRackID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["WH_ID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ELEC_RACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetWorkOrderInputProduct(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("ELECTRODETYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["WOID"] = sFilter[1];
                dr["ELECTRODETYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WO_PRODUCT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCostCenter(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COST_CENTER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetMoveReceive(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO_MOVEREC", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetWipStat(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_STAT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void INBOXStat(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_INBOX_STAT", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetMixProcess(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_FOR_RMTRL", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
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
        private void SetInspection(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_QMS", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }
        private void SetInputArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_FOR_INPUT_MTRL", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;

        }

        private void SetErpSatus(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ERP_STATUS", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }
        
        private void SetInputProcess(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FOR_INPUT_MTRL", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetSloc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = sFilter[0];

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_SHOP", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetPrjtName(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PRJT_NAME_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetPrjtNameElec(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("ELEC_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ELEC_TYPE_CODE"] = sFilter[0];

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJT_ELEC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetPrjtNameByProductCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
            RQSTDT.Columns.Add("ELEC_TYPE", typeof(string));
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PRJT_NAME"] = sFilter[0];
            dr["ELEC_TYPE"] = sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_PJT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            if (cbo.Items.Count == 2)
                cbo.SelectedIndex = 1;
            else
                cbo.SelectedIndex = 0;
        }

        private void SetProductLane(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PRODID"] = sFilter[0];

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_LANE_QTY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetLine_Plant(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                cbo.SelectedIndex = 0;

                //if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                //    if (cbo.SelectedIndex < 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                //    cbo.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetModelMerge(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("MODLID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQSGID"] = sFilter[0];
            dr["PROCID"] = sFilter[1];
            dr["MODLID"] = sFilter[2];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL_CBO_MERGE", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetShopFrom(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("SYSID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USERID"] = LoginInfo.USERID;
            dr["SYSID"] = LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetLastLoss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = sFilter[0];
            dr["USERID"] = LoginInfo.USERID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LAST_LOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetLossEqsgProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sFilter[0];
            dr["PROCID"] = sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOSS_DETL_AREA_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetVDFloor(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = sFilter[0];
            dr["EQSGID"] = sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_VD_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetMixProd(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("WIPSTAT", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["PROCID"] = sFilter[0];
            dr["WIPSTAT"] = sFilter[1];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MIX_PROD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetEquipt_Except(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                //EQSGID

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQSG_EXCEPT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPlant_Area(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID_FROM", typeof(string));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));
                RQSTDT.Columns.Add("SHOPID_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID_FROM"] = sFilter[1];
                dr["FROM_AREAID"] = sFilter[2];
                dr["SHOPID_TO"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PLANT_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

                //if (sFilter[1].Equals("ALL"))
                //{
                //    if (!LoginInfo.CFG_AREA_ID.Equals(""))
                //    {
                //        cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                //        if (cbo.SelectedIndex < 0)
                //        {
                //            cbo.SelectedIndex = 0;
                //        }
                //    }
                //    else
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                //    cbo.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCoreRadius(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CORE_RADIUS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCommonCodeWithoutCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCommonCodeWorktypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["CMCDTYPE"] = sFilter[0];
                dr["ATTRIBUTE1"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKTYPE_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        public void SetSpecialResonCodebyAreaCode(C1ComboBox cbo, ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SPCL_RSNCODE_BY_AREAID", "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAreaLocationCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetVdArea(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_VD", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void setMixerEquipment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_MIXER_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetLineExcludeLine(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                string sSelEqsgID = string.Empty;

                if (sFilter.Length > 1)
                    sSelEqsgID = Util.NVC(sFilter[1]);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = dtResult.Select("CBO_CODE <> '" + sSelEqsgID + "'").CopyToDataTable();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtTemp, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

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

        /// <summary>
        /// 공정별 Loss 코드 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetLossCodeProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["EQPTID"] = sFilter[2];
                dr["USERID"] = LoginInfo.USERID;
                //dr["EQSGID"] = sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 공정별 Loss 코드 콤보 Pack
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetLossCodeProcPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[1];
                dr["EQPTID"] = sFilter[2];
                dr["EQSGID"] = sFilter[3];
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC_PACK", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 활성화 후공정 작업업체 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetWorkSupplier(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["AREAID"] = sFilter[1];
                dr["PROCID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_WRK_SUPPLIER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 활성화 후공정 InBox Type 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetInboxType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_PROC_INBOX_TYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// DSF 대기창고 - 창고 Combo
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetDsfWarehouse(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DSF_WH_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// DSF 대기창고 - 위치(Rack) Combo
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetDsfWarehouseRack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                cbo.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["WH_ID"] = sFilter[0];                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DSF_WH_RACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipment_NT_NEW(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[2];

                // 2017.11.06 정규환. 극성이 ALL로 입력된 경우 전체 보여주기 위해 수정
                if (!string.IsNullOrEmpty(sFilter[1]))
                    dr["ELTR_TYPE_CODE"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (dtResult.Rows.Count == 2)
                {
                    cbo.SelectedIndex = 1;
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

        /// <summary>
        /// 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipmentByEQSGID_NEW(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["EQGRID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    if (dtResult?.Select("CBO_CODE = '" + LoginInfo.CFG_EQPT_ID + "'")?.Length > 0)
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbo.SelectedIndex < 0)
                    {
                        if (dtResult.Rows.Count == 2)
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

        /// <summary>
        /// 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEquipment_NEW(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["COATER_EQPT_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbo.SelectedIndex < 0)
                    {
                        if (dtResult.Rows.Count == 2)
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

        private void SetEquipmentAssy(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["COATER_EQPT_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if(!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

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
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }




        private void SetReqCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1];
              
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_REQ_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

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
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void SetFormMoveCodeCR(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
              

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[1];
                if(sFilter[2] != string.Empty)
                {
                    dr["EQSGID"] = sFilter[2];
                }
               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FORM_MOVE_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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



        private void SetProcess_Sort(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ASSYPROCID", typeof(string));
                RQSTDT.Columns.Add("LISTCHECK", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                //조립투입일 경우 A4000 와싱공정으로 이동
                if (sFilter[3].ToString() != "")
                {
                    dr["ASSYPROCID"] = sFilter[3];
                }
                if (sFilter[2].ToString() != "")
                {
                    dr["LISTCHECK"] = sFilter[2];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_SORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (sFilter[1].ToString() == "SEARCH")
                {
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


        private void SetTaget_Procss(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("FLOWID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["ROUTID"] = sFilter[1];
                dr["FLOWID"] = sFilter[2];
                dr["PROCID"] = sFilter[3];
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TARGET_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetProcess_Move(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                //조립투입일 경우 A4000 와싱공정으로 이동
        
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //if (sFilter[1].ToString() == "SEARCH")
                //{
                //    if (!LoginInfo.CFG_PROC_ID.Equals(""))
                //    {
                //        cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                //        if (cbo.SelectedIndex < 0)
                //        {
                //            cbo.SelectedIndex = 0;
                //        }

                //        if (cbo.SelectedIndex < 0)
                //        {
                //            cbo.SelectedIndex = 0;
                //        }
                //    }
                //    else
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                    cbo.SelectedIndex = 0;
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        
        private void SetLine_Form(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCProductTransfer(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable floorResult = new ClientProxy().ExecuteServiceSync("CUS_SEL_EQUIPMENTATTR_TBL", "RQSTDT", "RSLTDT", RQSTDT);
                string floor = string.IsNullOrEmpty(floorResult.Rows[0]["S06"].ToString()) ? null : floorResult.Rows[0]["S06"].ToString();

                RQSTDT = new DataTable("RQSTDT");
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                dr["EQPTID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable resultTable = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_CPROD_TRANS", "RQSTDT", "RSLTDT", RQSTDT);

                //층정보 없으면 Default
                int index = 0;

                if (!string.IsNullOrEmpty(floor))
                {
                    for (int i = 0; i < resultTable.Rows.Count; i++)
                    {
                        if (resultTable.Rows[i]["FLOOR"].Equals(floor))
                        {
                            index = i+1;
                            break;
                        }
                    }
                }

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(resultTable, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCProductMagazineTransfer(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = sFilter[0] ;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_CPROD_TRANS", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count < 1)
                {
                    RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));

                    dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = sFilter[0];
                    RQSTDT.Rows.Add(dr);

                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_CPROD_TRANS", "RQSTDT", "RSLTDT", RQSTDT);
                }

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAllEqptMountPsts(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_ALL", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEquipmentSegmentWithoutSelEQSGID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                if (sFilter.Length > 2)
                    dr["EXCEPT_GROUP"] = sFilter[2];
                if (sFilter.Length > 3)
                    dr["EXCEPT_EQSGID"] = sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_EXCEPTGRP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataTable dtTemp = null;
                
                if (dtResult.Select("CBO_CODE <> '" + sFilter[1] + "'").Length > 0)
                {
                    dtTemp = dtResult.Select("CBO_CODE <> '" + sFilter[1] + "'").CopyToDataTable();
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                cbo.ItemsSource = AddStatus(dtTemp, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

                //if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                //    if (cbo.SelectedIndex < 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                //    cbo.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCostSloc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COST_SLOC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "SLOC_NAME";
                cbo.SelectedValuePath = "SLOC_ID";
                cbo.ItemsSource = AddStatus(dtResult, cs, "SLOC_ID", "SLOC_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetPilotCostSloc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PILOT_SLOC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "SLOC_NAME";
                cbo.SelectedValuePath = "SLOC_ID";
                cbo.ItemsSource = AddStatus(dtResult, cs, "SLOC_ID", "SLOC_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSlocByGmesUse(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_GMES_USE", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 활성화 후공정 작업구분 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetFormWrkTypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WRK_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        /// <summary>
        /// 활성화 후공정 불량창고 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetDefectWh_Id(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_DEFECT_WH_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        /// <summary>
        /// 활성화 후공정 작업구분 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetFormWrkTypeLineCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("INPUT_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["EQSGID"] = sFilter[1];
                //dr["INPUT_TYPE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WRK_TYPE_CODE_LINE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 활성화 후공정 등급 콤보
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetFormGradeTypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTR1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["COM_TYPE_CODE"] = sFilter[1];
                dr["ATTR1"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetFormGradeTypeLineCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["EQSGID"] = sFilter[1];
                dr["QLTY_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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




        private void SetProdLot_Ctnr(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
               
                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_LOT_CTNR", "RQSTDT", "RSLTDT", RQSTDT);

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


        /// <summary>
        /// 활성화 후공정 폐기이력 구분(폐기/폐기 취소)
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetScrapActivity(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
              

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
              
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SCRAP_ACTIVITY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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
                dr["AREAID"] = sFilter[0];
                dr["COM_TYPE_CODE"] = sFilter[1];
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

        private void SetFloor(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FLOOR_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        //SetDefectType
        private void SetDefectType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITY_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEquipmentMainLevel(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                bool bAutoSelect = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                dr["COATER_EQPT_TYPE_CODE"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 1)
                    bAutoSelect = true;

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    if (dtResult?.Select("CBO_CODE = '" + LoginInfo.CFG_EQPT_ID + "'")?.Length > 0)
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbo.SelectedIndex < 0)
                    {
                        if (bAutoSelect)
                        {
                            cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                        }
                        else
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    if (bAutoSelect)
                    {
                        cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        //동별 공정
        private void SetAREA_Process_Sort(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_PROCESS_SORT", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPROCESS_EQUIPMENTSEGMENT(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetPOLYMER_PROCESS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if(sFilter[0] != string.Empty)
                {
                    dr["AREAID"] = sFilter[0];
                }
               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPOLYMER_PROCESS_NO_SELECT(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void SetPOLYMER_PKGLINE(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                if (sFilter[0] != string.Empty)
                {
                    dr["AREAID"] = sFilter[0];
                }
                if (sFilter[1] != string.Empty)
                {
                    dr["PROCID"] = sFilter[1];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC", "RQSTDT", "RSLTDT", RQSTDT);

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



        private void SetPOLYMER_PROCESS_ROUTE(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ROUTID",  typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (sFilter[0] != string.Empty)
                {
                    dr["AREAID"] = sFilter[0];
                }
                if (sFilter[1] != string.Empty)
                {
                    dr["ROUTID"] = sFilter[1];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();


                //if (!LoginInfo.CFG_PROC_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                //    if (cbo.SelectedIndex < 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
                //else
                //{
                    cbo.SelectedIndex = 0;
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetPOLYMER_PROCESS_AREA(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetDefectGroup(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = sFilter[0];
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DEFECT_GROUP", "RQSTDT", "RSLTDT", RQSTDT);

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




        private void SetPOLYMER_MOVE_AREA(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_MOVE_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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



        private void SetPOLYMER_PROCESS_AREA_EQSG(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS_AREA_EQSG", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCprodFromEqsgID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                bool bAutoSelect = false;

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("TO_EQSGID", typeof(string));
                //RQSTDT.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["TO_EQSGID"] = sFilter[0];
                //dr["CPROD_WRK_TYPE_CODE"] = sFilter[1].Equals("") ? null : sFilter[1];

                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CPROD_FROM_EQSG_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CPROD_WRK_TYPE_CODE"] = sFilter[0];
                dr["AREAID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CPROD_FROM_EQSG_CBO", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult.Rows.Count == 1)
                    bAutoSelect = true;

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

                //if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                //    if (cbo.SelectedIndex < 0)
                //    {
                //        if (bAutoSelect)
                //        {
                //            cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                //        }
                //        else
                //        {
                //            cbo.SelectedIndex = 0;
                //        }
                //    }
                //}
                //else
                //{
                //    if (bAutoSelect)
                //    {
                //        cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                //    }
                //    else
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCprodFromEqptID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                bool bAutoSelect = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[1];
                dr["CPROD_WRK_TYPE_CODE"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CPROD_FROM_EQPT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 1)
                    bAutoSelect = true;

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

                //if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                //{
                //    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                //    if (cbo.SelectedIndex < 0)
                //    {
                //        if (bAutoSelect)
                //        {
                //            cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                //        }
                //        else
                //        {
                //            cbo.SelectedIndex = 0;
                //        }
                //    }
                //}
                //else
                //{
                //    if (bAutoSelect)
                //    {
                //        cbo.SelectedIndex = cbo.Items.Count > 1 ? cbo.Items.Count - 1 : 0;
                //    }
                //    else
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetCprodToProcID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CPROD_TO_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetMobileBoxProcess(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_MOBILE_BOX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        if (dtResult.Rows[0]["CBO_CODE"].ToString() == LoginInfo.CFG_PROC_ID)
                        {
                            cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                        }
                        else
                        {
                            cbo.SelectedIndex = 0;
                        }
                    }
                    else
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

        private void SetEQUIPMENTSEGMENT_AUTO(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENTSEGMENT_AUTO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
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

        private void SetShiftGrCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                RQSTDT.Columns.Add("SHFT_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQSGID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["PROCID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["SHFT_GR_CODE"] = sFilter[3] == "" ? null : sFilter[3];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_CBO_L", "RQSTDT", "RSLTDT", RQSTDT);

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
            }

            return dt;
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

        private void SetAreaComCodeWorkType(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTR1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                dr["ATTR1"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_WORKTYPE", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPrjtNameMtrlInput(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_IWMS_PANCAKE_INFO_HIST_CBO", "RQSTDT", "RSLTDT", null);

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

        private void SetUnit(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sFilter[0];
                dr["EQPTID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_UNIT_BY_EQPTID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcess_Pouch(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_POUCH", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetPackWay(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACKWAY", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 폴란드 증설 : 라미 모니터링 (창고재고정보, QA검사 콤보박스) 
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        private void SetWhQA(C1ComboBox cbo, ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_QA_CBO", null, "RSLTDT", null);

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


        /// <summary>
        /// 폴란드 증설 : MEB 노칭,라미 창고재고정보 콤보박스
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetMEBWhQA(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGRID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MEB_WH_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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





        /// <summary>
        /// 폴란드 증설 : 라미포트 ID
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        private void SetLamiPort(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (sFilter[0] != string.Empty)
                {
                    dr["EQPTID"] = sFilter[0];
                }
                if (sFilter[1] != string.Empty)
                {
                    dr["EQGRID"] = sFilter[1];
                }
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LAMI_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 폴란드 증설 : 라미Stocker
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        private void SetLamiStocker(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CHECK_MEB", typeof(string));
                RQSTDT.Columns.Add("CHECK_LAMI", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if(sFilter[0] != string.Empty)
                {
                    dr["CHECK_MEB"] = sFilter[0];
                }
                if (sFilter[1] != string.Empty)
                {
                    dr["CHECK_LAMI"] = sFilter[1];
                }
                if (sFilter[2] != string.Empty)
                {
                    dr["EQGRID"] = sFilter[2];
                }
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LAMI_STOCKER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// <summary>
        /// 선감지 항목 조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>

        private void SetErly_Dett_Item(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ERLY_DETT_GR1_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ERLY_DETT_GR1_CODE"] = sFilter[0];
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_ERLY_DETT_ITEM", "RQSTDT", "RSLTDT", RQSTDT);

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



        private void SetEltrAreaForAssy(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INF_ELTR_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        /// <summary>
        /// 공정 문자열로 라인정보 조회
        /// </summary>
        private void SetEquipmentSegmentProc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PROD_GROUP", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO", "RQSTDT", "RSLTDT", inDataTable);

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

        /// <summary>
        /// 설비 별 투입 위치 코드 조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="sFilter"></param>
        private void SetEqptMountPstsExcludeScreenDisp(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sFilter[0];
                dr["MOUNT_MTRL_TYPE_CODE"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_L", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetRwkProjectByEquipmentSegment(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQSGID"] = sFilter[0].Equals("") ? null : sFilter[0].Equals("SELECT") ? null : sFilter[0];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_RWK_PRJT_BY_EQSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetProcessByAreaPcsg(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_PCSG_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        //2019.09.25 김대근 금형조회 콤보박스 추가
        private void SetToolTypeCode(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TOOL_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProductionPlanCategory(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTION_PLAN_CATEGORY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        //2019.12.06 김대근 금형이 등록된 공정 조회
        private void SetProcessTool(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_TOOL_BY_AREAID", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcessBySBL_ABNORM_BAS(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_SBL_ABNORM_BAS", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetLabelEqptCbo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["PROCID"] = sFilter[1];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTID_BY_EQSG_LABEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetOffLineSLocCbo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["AREAID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_OFF_LINE_USE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetProcessByAreaEtc(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCTYPE", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCTYPE"] = sFilter[1];
                dr["PCSGID"] = sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_ETC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetcboCustID(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0];
                dr["PRODID"] = sFilter[1];
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUSTID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetCommonCodeAttr2(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
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
                dr["CMCDTYPE"] = sFilter[1];
                dr["ATTRIBUTE1"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_FROM_ATTRIBUTE2", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "ATTRIBUTE2", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setComboLogisProdIdPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                RQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = string.IsNullOrEmpty(Util.NVC(sFilter[0])) ? null : sFilter[0];
                dr["PACK_CELL_AUTO_LOGIS_FLAG"] = string.IsNullOrEmpty(Util.NVC(sFilter[1])) ? "Y" : sFilter[1];
                dr["PACK_MEB_LINE_FLAG"] = string.IsNullOrEmpty(Util.NVC(sFilter[2])) ? null : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOGIS_CELL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setComboLogisEqsgIdPack(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                RQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.IsNullOrEmpty(Util.NVC(sFilter[0])) ? LoginInfo.CFG_AREA_ID : sFilter[0];
                dr["PACK_CELL_AUTO_LOGIS_FLAG"] = string.IsNullOrEmpty(Util.NVC(sFilter[1])) ? "Y" : sFilter[1];
                dr["PACK_MEB_LINE_FLAG"] = string.IsNullOrEmpty(Util.NVC(sFilter[2])) ? null : sFilter[2];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOGIS_LINE_CBO_MEB", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}