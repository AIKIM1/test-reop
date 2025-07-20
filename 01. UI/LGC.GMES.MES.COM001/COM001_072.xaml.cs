/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.03.13  정문교C   : 대상 LOT 조회 수정
  2018.02.20  이상훈    : [C20180209_07002] 시스템관리 공정별 생상실적 수정 화면에 와인더 공정 전극 투입_투입취소 기능 추가 요청
  2018.08.01  이상훈    : [C20180718_42535] 와싱 Tray 실적 수정시 FCS로 data 전송 기능 추가
  2019.01.02  고현영    : TAG수 수정 가능 요청사항 반영
  2019.03.27  강호운    : Package 공정 바구니 투입취소 기능 추가 [CSR ID:3946580] Pilot 3호 PKG 실적수정 요청 건 | [요청번호]C20190312_46580 | [서비스번호]3946580 
  2019.07.16  김동일    : 폴란드 1,2 동 라미 공정진척 3LOSS 적용에 따른 수정
  2020.06.26  김동일    : C20200625-000271 기준정보를 통한 투입관련 Tab의 투입 및 투입취소버튼 제어기능 추가
  2021.06.23  이상훈    : C20210623-000317 공정별 생산실적 수정(조립) 화면 조회시 다국어 처리 
  2021.09.09  신광희    : 롤맵 팝업 차트 버전 파라메터 추가
  2021.10.07  오화백    : 버튼셀 관련 Cell관리, Tray 확정처리 추가
  2021.11.02  신광희    : 롤맵 팝업 호출 시 EquipmentName -> EquipmentName + [EquipmentId] 로 표기되도록 수정
  2022.01.05  정재홍    : [C20210810-000354] - 코터,롤프레스 완공 상태에서 실적수정 불가하도록 기준정보 추가
  2022.08.03  정재홍    : [C20220720-000474] - GMES 생산실적 롤프레스, 슬리터 TAG 컬럼 추가 및 데이터 수정 요청
  2023.01.12  신광희    : 2차 슬리터 롤맵 팝업 호출 추가
  2023.02.27  윤기업    : 롤맵 실적수정 팝업 추가
  2023.03.09  성민식    : [C20220921-000333] - 오창 1공장 소형 조립 WINDING 공정 완성 LOT TRAY 생성, 삭제, 확정, 확정취소, 배출, 수량저장 버튼 추가
  2023.07.12  성민식    : [E20230420-001142] - 오창 2공장 소형 조립 WINDING 공정 완성 LOT TRAY 생성, 삭제, 확정, 확정취소, 배출, 수량저장 버튼 추가
  2023.07.17  성민식    : [E20230424-000089] - ESNJ ZZS 공정 WIP 수량 수정 가능하도록 변경
  2023.07.26  이호섭    : E20230726-000790 LANE_PTN_QTY 수량 반영 부분 전극 제외 처리
  2023.09.06  주동석 : 모든 경우에 다음공정 투입시 수정 못하도록 Blocking 하도록 수정
  2023.09.13  정재홍    : [E20230508-000004] - [ESGM] 전극,조립간 인수인계 완료된 롤정보 실적 수정 및 홀드 불가
  2023.11.08  정재홍    : [E20231031-000379] - 조립 인계된 팬케잌 LOT 실적 수정 인터락 (재공종료시)
  2023.12.07  김태우      전극 판정등급 수정 기능 추가.
  2024.01.11  오수현    : [E20231215-001801] - 완성 LOT TRAY 관리 대상 여부를 COMMONCODE로 관리 (ShopID:A010,F030,G182)
  2024.01.23  성민식    : [E20240118-001257] - ESNJ 소형 파우치 WIPQTY2_ED 수량 LANE_PTN_QTY 영향 안 받도록 수정
  2024.01.26  오수현    : [E20231215-001801] - 기능 롤백. 완성 LOT TRAY 관리 대상 여부(ShopID:A010,F030,G182)를 공통코드로 관리하던 기능 삭제. EQSGID와 CSTID의 설비세그멘터가 동일한지 여부로만 관리하도록 함
  2024.02.15  양영재    : [E20231221-001759] - GM1 특정 스토커 적재된 HOLD LOT 실적 저장 허용
  2024.02.20  오수현    : [E20230901-001504] - 대차ID 항목 표시
  2024.03.26  김용군    : [E20240221-000898] - ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
  2024.05.25  배현우    : [E20240524-001573] - 전극 등급 정보 수정 가능 하도록 오류 수정
  2024.06.18  성민식    : [E20240605-001056] - Ass'y 공정 투입반제품 투입취소 시 '투입재공생성'이 아닌 '투입종료취소' 로 변경
  2024.08.27  조성근    : [E20240827-000372] - 롤프레스 롤맵 실적 수정팝업 추가
  2024.09.13  김지호    : [E20240911-000950] - RollMAp 실적수정 팝업 창 닫을 때 메시지박스 추가
  2024.09.23  김지호    : [E20240911-000950] - ERP 마감 전에 RollMAp 실적수정을 하지 않으면 메시지박스 출력되지 않았지만, 메시지박스 출력 되도록 수정
  2025.02.03  백상우    : [MES2.0] ROLL PRESS공정 ROLLMAP 도입으로 색지정보 탭 사용안함, 탭 숨김처리
  2025.03.24  이민형    : [HD_OSS_0134]체크 없이 데이터 수정 시 에러 발생, 에러 발생 메시지도 보완.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ASSY001;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_072 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREATYPE = "";
        private string _StackingYN = string.Empty;
        string _AREAID = "";
        string _PROCID = "";
        string _PRODID = "";
        string _EQSGID = "";
        string _EQPTID = "";
        string _LOTID = "";
        string _WIPSEQ = "";
        string _VERCODE = "";
        string _LANEPTNQTY = "";
        string _WIPSTAT = "";
        string _WIP_TYPE_CODE = "";
        string _UNITCODE = "";
        string _COATING_GUBUN = "";
        string _COATING_SIDE_TYPE_CODE = "";
        string _LANE_QTY = "";
        string _WORKORDER = "";
        string _ERP_CLOSE = "";
        string _WIP_NOTE = "";
        string _bool = "true";
        string _IS_SMALL_TYPE = "";
        string _CELL_MNGT_TYPE_CODE = "";
        string _EQPTNAME = "";
        string _WIP_WRK_TYPE_CODE = "";
        double _dInQty = 0;
        double _dOutQty = 0;
        double _dprOutQty = 0;
        double _dEqptQty = 0;
        double _dEqptOrgQty = 0;
        string _LANE = "";
        string _CSTID = "";

        //C20210323-000097
        double _OUT_QTY = 0; //양품량 최초값 저장용. 신규값 할당하지 말것.
        double _DEFECT_QTY = 0;  //불량량 최초값 저장용. 신규값 할당하지 말것.
        double _LOSS_QTY = 0;    //LOSS량 최초값 저장용. 신규값 할당하지 말것
        double _PRDT_REQ_QTY = 0;    //물품청구 최초값 저장용. 신규값 할당하지 말것
        string _LOTID_AS = "";

        // Roll Map
        private bool _isOriginRollMapEquipment = false;
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private bool _isRollMapLot;

        //투입자재 추가
        C1DataGrid dgLotInfo;

        DataTable _dtLengthExceeed = new DataTable();

        private double gDfctAQty = 0;
        private double gDfctCQty = 0;

        bool isChangeDefect = false;

        private System.DateTime dtCaldate;

        private readonly Util _util = new Util();
        private string _EQPT_DFCT_APPLY_FLAG = string.Empty;

        private int iCnt = 0;
        private DataTable dtBOM = new DataTable();
        private DataTable dtBOM_CHK = new DataTable();
        private int iBomcnt = 0;

        private BizDataSet _Biz = new BizDataSet();

        //C20180209_07002 추가
        private DateTime _dtMinValid;
        private string _maxPeviewProcessEndDay = string.Empty;

        private DataTable _dtCellManagement;
        private string _cellManageGroup = string.Empty;

        //2021-10-07 오화백 : 버튼셀
        private string _Product_Lev = string.Empty;

        //C20220921-000333
        private bool _EQSGID_YN = false;

        //[E20231215-001801] (COMMONCODE) 완성 LOT TRAY 관리 대상 여부
        private bool _isOutputLotTrayMgmt = false;

        //[E20231221-001759] HOLD LOT 실적 저장 허용 스토커 RACKID 확인
        private string _Rack_ID = "";
        private string _WIPHOLD = "N";

        private string sBeforeValue = string.Empty;

        public COM001_072()
        {
            InitializeComponent();

            InitCombo();

            GetAreaType(cboProcess.SelectedValue.ToString());
            AreaCheck(cboProcess.SelectedValue.ToString());
            SetProcessNumFormat(cboProcess.SelectedValue.ToString());
            IsElectrodeGradeInfo();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //ESMI-A4동 6Line 제외처리
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "ESMI_A4_EXCEPT_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }

            //공정
            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged; //공정변할때 lane_qty, coating ver 보여주기 관리
            ////CboProcess_SelectedItemChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            // 완성LOT탭 비활성
            cTabHalf.Visibility = Visibility.Collapsed;

            string[] sFilter = { "WORKING_TYPE", Process.STACKING_FOLDING };
            _combo.SetCombo(cboChange, CommonCombo.ComboStatus.NONE, sCase: "WORKTYPE_COMMCODE_CBO", sFilter: sFilter);
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnQualitySave);
            listAuth.Add(btnSubLotSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            GetCellManagementInfo();
            dtpCaldate.SelectedDataTimeChanged += dtpCaldate_SelectedDataTimeChanged;

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();

            //GetOutputLotTrayMgmt();  //[E20231215-001801] 2025.01.25 기능 롤백
        }
        #endregion


        #region [ROLLMAP]
        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // RollMap 실적 수정 Popup Call              

                object[] Parameters = new object[11];
                Parameters[0] = _PROCID;
                Parameters[1] = _EQSGID;
                Parameters[2] = _EQPTID;
                Parameters[3] = _LOTID;
                Parameters[4] = _WIPSEQ;
                Parameters[5] = _LANE_QTY;
                Parameters[6] = _EQPTNAME;
                Parameters[7] = _VERCODE;
                Parameters[8] = "Y"; //Test Cut Visible True
                Parameters[9] = "N"; //Search Mode False
                Parameters[10] = "COM001_072"; //호출화면

                // RollMap 실적 수정 Popup Call
                if (_PROCID == Process.COATING)
                {
                    CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                    if (popupRollMapUpdate != null)
                    {


                        C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                        if (popupRollMapUpdate != null)
                        {
                            popupRollMapUpdate.ShowModal();
                            popupRollMapUpdate.CenterOnScreen();
                            popupRollMapUpdate.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                        }
                    }
                }
                else if (_PROCID == Process.ROLL_PRESSING)
                {
                    CMM_RM_RP_RESULT popupRollMapUpdate = new CMM_RM_RP_RESULT { FrameOperation = FrameOperation };

                    if (popupRollMapUpdate != null)
                    {
                        C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                        if (popupRollMapUpdate != null)
                        {
                            popupRollMapUpdate.ShowModal();
                            popupRollMapUpdate.CenterOnScreen();
                            popupRollMapUpdate.Closed += new EventHandler(PopupRollMapUpdate_RP_Closed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void PopupRollMapUpdate_Closed(object sender, EventArgs e)
        {

            CMM_RM_CT_RESULT popup = sender as CMM_RM_CT_RESULT;
            // ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회

            if (popup.IsUpdated) PopupRollMapUpdated();

            if (!_ERP_CLOSE.Equals("CLOSE"))
            {
                // [E20240911-000950] - RollMAp 실적수정 팝업 창 닫을 때 메시지박스 추가 (ERP 마감 전까지만 메시지박스 출력, ERP 마감 후에는 메시지 박스 출력 되지 않음)
                Util.MessageValidation("SFU9025");
            }
        }

        private void PopupRollMapUpdate_RP_Closed(object sender, EventArgs e)
        {

            CMM_RM_RP_RESULT popup = sender as CMM_RM_RP_RESULT;
            // ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회

            if (popup.IsUpdated) PopupRollMapUpdated();

            if (!_ERP_CLOSE.Equals("CLOSE"))
            {
                // [E20240911-000950] - RollMAp 실적수정 팝업 창 닫을 때 메시지박스 추가 (ERP 마감 전까지만 메시지박스 출력, ERP 마감 후에는 메시지 박스 출력 되지 않음)
                Util.MessageValidation("SFU9025");
            }
        }

        private void PopupRollMapUpdated()
        {
            // ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회
            string sLotid = _LOTID;

            //투입수량 변경으로 인하여 재 조회 처리 및 선택된 LOT 체크
            GetLotList(sLotid);

            // 1. RollMap 변경된 불량 재조회
            // 만약 수정하고 있던 불량/LOSS/물품청구 데이터가 있다면 반영 안됨
            GetDefectInfo();

            // 2, 양품량 재계산
            try
            {
                txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

                isChangeDefect = true;

                DataTable dtInfo = DataTableConverter.Convert(dgDefect.ItemsSource);

                double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID <> 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));
                double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID = 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));

                double dLoss = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND RESNGRID <> 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND RESNGRID = 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_EXCEED = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND PRCS_ITEM_CODE = 'LENGTH_EXCEED'")));
                double dPrdtReq = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                double dPrdtReq_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID = 'DEFECT_TOP'")));


                double dIn = _dInQty;
                double dOut = _dOutQty;
                double dTotalDefect = 0;
                double dDiff = 0;

                //dOut = dIn - dTotalDefect;

                // 불량 합산 
                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                    // 코팅이고 단면이 아닌경우
                    dTotalDefect = dDefect + dLoss + dPrdtReq + (dDefect_top * 0.5) + (dLoss_top * 0.5) + (dPrdtReq_top * 0.5);
                else
                    dTotalDefect = dDefect + dLoss + dPrdtReq + dDefect_top + dLoss_top + dPrdtReq_top;


                // 생산량 산출
                if (_PROCID.Equals(Process.COATING))
                {
                    //ROLLMAP 양품량 산출
                    dOut = dIn - dTotalDefect;
                }
                else
                {
                    dOut = Convert.ToDouble(Convert.ToDecimal(dIn) + Convert.ToDecimal(dLoss_EXCEED) - Convert.ToDecimal(dTotalDefect));    //E20241101-001339 소수점 계산 오류로 CONVERT 추가
                }

                if (dOut < 0)
                {
                    //Util.MessageValidation("SFU1884");
                    //return;
                    dOut = 0;
                }

                txtInputQty.Value = dIn;
                txtOutQty.Value = dOut;
                txtLengthExceedQty.Value = dLoss_EXCEED;
                txtDiffQty.Value = dDiff;

                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                {
                    txtDefectQty.Value = dDefect + (dDefect_top * 0.5);
                    txtLossQty.Value = dLoss + (dLoss_top * 0.5);
                    txtPrdtReqQty.Value = dPrdtReq + (dPrdtReq_top * 0.5);
                }
                else
                {
                    txtDefectQty.Value = dDefect + dDefect_top;
                    txtLossQty.Value = dLoss + dLoss_top;
                    txtPrdtReqQty.Value = dPrdtReq + dPrdtReq_top;
                }

                if (_ERP_CLOSE.Equals("CLOSE") == false)
                {
                    Util.MessageValidation("SFU9025");
                }

                // 실적 저장
                //Save(true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
            }
        }

        private void SetRollMapEquipment()
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(_EQPTID);
            _isOriginRollMapEquipment = _isRollMapEquipment;

            // ROLLMAP LOT CHECK
            _isRollMapLot = SelectRollMapLot();

            SetRollMapLotAttribute(_LOTID);

            // NEXT 공정 투입 CHECK 
            //if (_isRollMapEquipment)
            //{
            //    SetRollMapSBL(_LOTID, _WIPSEQ);
            //}

            // ROLLMAP 수불일 경우 양품량 변경 방지
            // 2023-02-26 변경 기존과 동일하게 양품량 수정 가능하도록 
            //if (_isRollMapEquipment)
            //    txtOutQty.IsEnabled = false;
        }

        /// <summary>
        /// Lane Qty
        /// </summary>

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _PROCID;
                dr["EQSGID"] = _EQSGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null & dtResult.Rows.Count > 0)
                    if (string.Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }

        private void SetRollMapLotAttribute(string sLotID)
        {
            try
            {
                if (_isOriginRollMapEquipment == true)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    DataRow row = dt.NewRow();
                    row["LOTID"] = sLotID;
                    dt.Rows.Add(row);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                    if (result != null && result.Rows.Count > 0)
                    {
                        // ROLLMAP 실적 + ROLLMAP LOT ==> ROLLMAP 실적 수정 가능 
                        if (_isRollMapResultLink == true)
                        {
                            if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y") && _isRollMapLot)
                            {
                                _isRollMapEquipment = true;
                                btnRollMapUpdate.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                _isRollMapEquipment = false;
                                btnRollMapUpdate.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            _isRollMapEquipment = false;
                            btnRollMapUpdate.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    _isRollMapEquipment = false;
                    btnRollMapUpdate.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRollMapSBL(string sLotID, string sWipSeq)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WIPSEQ", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = sLotID;
                row["WIPSEQ"] = sWipSeq;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RM_LOT_SBL_UPDATE", "INDATA", null, dt);

            }
            catch (Exception ex)
            {
                btnRollMapUpdate.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);


            }
        }

        private bool SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.gridClear(dgLotList);
                ClearValue();

                GetAreaType(cboProcess.SelectedValue.ToString());
                AreaCheck(cboProcess.SelectedValue.ToString());
                SetProcessNumFormat(cboProcess.SelectedValue.ToString());
                IsElectrodeGradeInfo();
            }
        }
        #endregion

        #region [작업일] - 조회조건 ####### Visibility="Collapsed" #######
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region [LOT] - 조회 조건
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [작업대상 목록 에서 선택]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(false) ||
                DataTableConverter.GetValue(rb.DataContext, "CHK").Nvc().Equals("0") ||
                DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));
                string sEqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();
                string sWoId = DataTableConverter.GetValue(rb.DataContext, "WOID").ToString();
                string sWIPTypeCode = DataTableConverter.GetValue(rb.DataContext, "WIP_WRK_TYPE_CODE").ToString();

                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                SetProcessNumFormat(_PROCID);

                //설비작업조건 유닛설비명 안보이게. PACKAGING 일경우만 보이게함
                if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                {
                    dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Collapsed;
                }

                //PREMIXING [2018.04.24 MIXING 적용]
                if (_PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.MIXING))
                {
                    GetMaterialSummary(sLotId, sWoId);
                }

                if (_PROCID.Equals(Process.COATING) || _PROCID.Equals(Process.SRS_COATING))
                {
                    GetMaterial();
                }

                if (_AREATYPE.Equals("A") && !_PROCID.Equals(Process.NOTCHING))
                {
                    GetSubLot();
                }

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                    {
                        dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Visible;
                    }

                    GetInBox(sLotId, iWipSeq);
                }

                ProcCheck(_PROCID, sEqptID);

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    GetEqpt_Dfct_Apply_Flag();

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                        GetDefectInfo_NJ();
                    else
                    {
                        GetDefectInfo_FOL();

                        if (sWIPTypeCode.Equals("SCRAP"))
                            dgDefect_FOL.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;
                        else
                            dgDefect_FOL.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Collapsed;
                    }

                }
                else if (_PROCID.Equals(Process.STP))
                {
                    GetDefectInfo_NJ();
                }
                else
                {
                    GetDefectInfo();
                }

                GetInputHistory();

                GetEqpFaultyData();
                GetQuality();
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    GetQualityCount();
                    if (TabReInput.Visibility == Visibility.Visible)
                    {
                        GetDefectReInputList();
                    }
                }

                if (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING) || _PROCID.Equals(Process.PACKAGING))
                {
                    GetQualityCount();
                }

                GetColor();

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    String[] sFilter1 = { _EQPTID, "PROD" };
                    CommonCombo _combo = new CommonCombo();
                    _combo.SetCombo(cboInHalfMountPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                    GetHalfProductList();
                }

                //대기 반제품 투입위치 콤보
                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    // 자재 투입위치 코드
                    String[] sFilter1 = { _EQPTID, "PROD" };
                    CommonCombo combo = new CommonCombo();
                    combo.SetCombo(cboWaitHalfProduct, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                }

                ///[C20180209_07002] 추가
                if (_PROCID.Equals(Process.WINDING))
                {
                    // 자재 투입위치 코드
                    String[] sFilter1 = { _EQPTID, null };
                    String[] sFilter2 = { _EQPTID, "PROD" };
                    CommonCombo combo = new CommonCombo();
                    combo.SetCombo(cboInputHalfWindingMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                    combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                    GetInputHalfWinding();
                }

                //if (_PROCID.Equals(Process.SLITTING))
                //{
                //    dgcTagQty.Visibility = Visibility.Visible;
                //}

                GetEqptCond();

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(_PROCID);

                #region # RollMap 설비 
                if (IsEquipmentAttr(sEqptID))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed;

                // ROLLMAP 실적수정 버튼 사용
                SetRollMapEquipment();

                // 전극 등급 정보 
                // MES 2.0 유재홍 선임 요청으로 수정.
                if (tiElectrodeGradeInfo.Visibility.Equals(Visibility.Visible) && IsElectrodeGradeInfo()) SelectElectrodeGradeInfo(sLotId, iWipSeq.GetString());
                
                #endregion
            }
        }

        private void GetEqpt_Dfct_Apply_Flag()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _PROCID;
                dr["EQSGID"] = _EQSGID;

                RQSTDT.Rows.Add(dr);

                DataTable SResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_APPLY_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

                _EQPT_DFCT_APPLY_FLAG = SResult.Rows[0]["EQPT_DFCT_APPLY_FLAG"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [작지선택] ####### Visibility="Collapsed" #######
        private void btnWorkorder_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            CMM_WORKORDER wndPopup = new CMM_WORKORDER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = _AREAID;
                Parameters[2] = _EQSGID;
                Parameters[3] = _PROCID;
                Parameters[4] = _EQPTID;
                Parameters[5] = txtWorkorderDetail.Text;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndWorkOrder_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndWorkOrder_Closed(object sender, EventArgs e)
        {
            CMM_WORKORDER window = sender as CMM_WORKORDER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //if (!txtWorkorder.Text.Equals(window.WOID))
                //{
                //    SetChgFont(txtWorkorder);
                //}

                txtWorkorder.Text = window.WOID;
                txtWorkorderDetail.Text = window.WOIDDETAIL;

            }
        }
        #endregion

        #region [작업일자]
        private void dtpCaldate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                else
                    dtPik.Focus();
            }));
        }
        #endregion

        #region [작업조선택]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            CMM_SHIFT wndPopup = new CMM_SHIFT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = _AREAID;
                Parameters[2] = _EQSGID;
                Parameters[3] = _PROCID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!txtShift.Tag.Equals(window.SHIFTCODE))
                {
                    //SetChgFont(txtShift);
                }
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }
        #endregion

        #region [작업자]
        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.AlertInfo("SFU1646");  //선택된 작업조가 없습니다.
                Util.MessageValidation("SFU1646");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_EQSGID);
                Parameters[3] = Util.NVC(_PROCID);
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EQPTID);  //EQPTID 추가 
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();

            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
        }
        #endregion

        #region [요청자]
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        #endregion

        #region 투입수, 양품수량 변경
        private void txtInputQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            isChangeDefect = true;
            //SetLengthExceeedClear();
        }

        private void txtOutQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            isChangeDefect = true;
            //SetLengthExceeedClear();
        }

        private void txtInputQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter) && isChangeDefect)
                SetChangeQty(true);
        }
        private void txtInputQty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isChangeDefect)
                SetChangeQty(true);
        }

        private void txtOutQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter) && isChangeDefect)
                SetChangeQty(false);
        }

        private void txtOutQty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isChangeDefect)
                SetChangeQty(false);
        }

        #endregion

        #region 재투입 변경
        private void txtInputDiffQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            double dDiff = 0;
            double dDefect = 0;

            dDefect = txtDefectQty.Value + txtLossQty.Value + txtPrdtReqQty.Value;

            // 차이수량 산출  (투입량 + 재투입) - (양품량+불량량+Loss량+물품청구)
            if (txtInputDiffQty.Value.ToString().Equals("NaN") || txtInputDiffQty.Value == 0)
                dDiff = _dInQty - (_dOutQty + dDefect);
            else
                dDiff = (_dInQty + txtInputDiffQty.Value) - (_dOutQty + dDefect);

            txtDiffQty.Value = dDiff;

        }
        #endregion

        #region COATING버전 팝업]
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_ELECRECIPE wndPopup = new CMM001.Popup.CMM_ELECRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _PRODID;
                Parameters[1] = _PROCID;
                Parameters[2] = _AREAID;
                Parameters[3] = _EQPTID;
                Parameters[4] = _LOTID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndRecipe_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndRecipe_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_ELECRECIPE window = sender as CMM001.Popup.CMM_ELECRECIPE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtProdVerCode.Text = window._ReturnRecipeNo;
                txtLaneQty.Text = window._ReturnLaneQty;
                //txtPtnQty.Text = window._ReturnPtnQty;
            }
        }
        #endregion

        #region [불량,로스,물청 수량변경]
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                txtInputQty.ValueChanged -= txtInputQty_ValueChanged;
                txtOutQty.ValueChanged -= txtOutQty_ValueChanged;
                txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

                isChangeDefect = true;

                DataTable dtInfo = DataTableConverter.Convert(dgDefect.ItemsSource);

                //double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                //double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID = 'DEFECT_TOP'")));
                double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND ISNULL(RESNGRID,'') <> 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));
                double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND ISNULL(RESNGRID,'') = 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));

                double dLoss = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND ISNULL(RESNGRID,'') <> 'DEFECT_TOP' AND ISNULL(PRCS_ITEM_CODE,'') <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND ISNULL(RESNGRID,'') = 'DEFECT_TOP' AND ISNULL(PRCS_ITEM_CODE,'') <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_EXCEED = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND ISNULL(PRCS_ITEM_CODE,'') = 'LENGTH_EXCEED'")));
                double dPrdtReq = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND ISNULL(RESNGRID,'') <> 'DEFECT_TOP'")));
                double dPrdtReq_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND ISNULL(RESNGRID,'') = 'DEFECT_TOP'")));

                double dLoss_BeforeEXCEED = Convert.ToDouble(Util.NVC_Decimal(_dtLengthExceeed.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND ISNULL(PRCS_ITEM_CODE,'') = 'LENGTH_EXCEED'")));

                //dLoss_EXCEED = dLoss_EXCEED - dLoss_BeforeEXCEED;

                //double dIn = Convert.ToDouble(txtInputQty.Value);
                //double dOut = Convert.ToDouble(txtOutQty.Value);
                //double dTotalDefect = dDefect + dLoss + dPrdtReq + (dDefect_top * 0.5) + (dLoss_top * 0.5) + (dPrdtReq_top * 0.5);
                double dIn = _dInQty;
                double dOut = _dOutQty;
                double dTotalDefect = 0;
                double dAlphaQty = 0;
                double dDiff = 0;

                //dOut = dIn - dTotalDefect;

                // 불량 합산
                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                    // 코팅이고 단면이 아닌경우
                    dTotalDefect = dDefect + dLoss + dPrdtReq + (dDefect_top * 0.5) + (dLoss_top * 0.5) + (dPrdtReq_top * 0.5);
                else
                    dTotalDefect = dDefect + dLoss + dPrdtReq + dDefect_top + dLoss_top + dPrdtReq_top;

                // 생산량 산출
                if (_PROCID.Equals(Process.COATING) ||
                    _PROCID.Equals(Process.LAMINATION) ||
                    _PROCID.Equals(Process.STACKING_FOLDING) ||
                    _PROCID.Equals(Process.WINDING) ||
                    _PROCID.Equals(Process.WASHING)
                    )
                {
                    // 길이 초과가 있는경우 양품수에 합산 불량에서는 제외
                    //if (_PROCID.Equals(Process.COATING))
                    //    dOut += dLoss_EXCEED;
                    //dIn = dOut + dTotalDefect;

                    //if (_PROCID.Equals(Process.COATING))
                    //{
                    //    //dIn = dOut + dTotalDefect - dLoss_EXCEED;
                    //    dOut = dIn + dLoss_EXCEED - dTotalDefect;
                    //}
                    //else
                    //{
                    dIn = dOut + dTotalDefect;
                    //}
                }
                else if (_PROCID.Equals(Process.ASSEMBLY))
                {
                    // 차이수량 산출  (투입량 + 재투입) - (양품량+불량량+Loss량+물품청구)
                    if (txtInputDiffQty.Value.ToString().Equals("NaN") || txtInputDiffQty.Value == 0)
                        dDiff = dIn - (dOut + dTotalDefect);
                    else
                        dDiff = (dIn + txtInputDiffQty.Value) - (dOut + dTotalDefect);
                }
                else if (!_PROCID.Equals(Process.PACKAGING))
                {
                    // 전극
                    //dIn += dLoss_EXCEED;
                    //dOut = dIn - dTotalDefect;

                    //dIn = dOut + dTotalDefect - dLoss_EXCEED;
                    dOut = dIn + dLoss_EXCEED - dTotalDefect;
                }

                if (_PROCID.Equals(Process.WINDING))
                {
                    double inputQtyByType = GetInputQtyByApplyTypeCode().GetDouble();
                    double adddefectqty = (_dEqptOrgQty + inputQtyByType) - (dOut + dTotalDefect);

                    if (Math.Abs(adddefectqty) > 0)
                    {
                        txtAssyResultQty.Value = adddefectqty;
                        txtAssyResultQty.FontWeight = FontWeights.Bold;
                        txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        txtAssyResultQty.Value = 0;
                        txtAssyResultQty.FontWeight = FontWeights.Normal;
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                        if (convertFromString != null)
                            txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                }

                // ======================================= 저장전 체크 필요
                if (dOut < 0)
                {
                    //Util.AlertInfo("SFU1884");      //전체 불량 수량이 양품 수량보다 클 수 없습니다.
                    Util.MessageValidation("SFU1884");
                    return;
                }

                txtInputQty.Value = dIn;
                txtOutQty.Value = dOut;
                txtLengthExceedQty.Value = dLoss_EXCEED;
                txtDiffQty.Value = dDiff;

                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                {
                    txtDefectQty.Value = dDefect + (dDefect_top * 0.5);
                    txtLossQty.Value = dLoss + (dLoss_top * 0.5);
                    txtPrdtReqQty.Value = dPrdtReq + (dPrdtReq_top * 0.5);
                }
                else
                {
                    txtDefectQty.Value = dDefect + dDefect_top;
                    txtLossQty.Value = dLoss + dLoss_top;
                    txtPrdtReqQty.Value = dPrdtReq + dPrdtReq_top;
                }

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    // 차이수량 산출
                    dAlphaQty = (dOut + dTotalDefect) - dIn;
                    txtInputDiffQty.Value = dAlphaQty;
                }

                //if (!_PROCID.Equals(Process.STACKING_FOLDING))
                //    return;

                //double atypeqty;
                //double ctypeqty;
                //double resnqty;

                //if (e.Cell.Column.Name.Equals("A_TYPE_DFCT_QTY") || e.Cell.Column.Name.Equals("C_TYPE_DFCT_QTY"))
                //{
                //    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY"));
                //    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY"));
                //    atypeqty = double.Parse(sAtype);
                //    ctypeqty = double.Parse(sCtype);

                //    if (gDfctAQty > 0 && gDfctCQty > 0)
                //    {
                //        resnqty = Math.Round((atypeqty / gDfctAQty + ctypeqty / gDfctCQty) / 2, 0);

                //        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", resnqty);
                //    }
                //}
                //else if (e.Cell.Column.Name.Equals("RESNQTY"))
                //{
                //    string sResnQty = e.Cell.Text;
                //    resnqty = double.Parse(sResnQty);

                //    atypeqty = (resnqty * 2) * gDfctAQty;
                //    ctypeqty = (resnqty * 2) * gDfctCQty;

                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY", atypeqty / 2);
                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY", ctypeqty / 2);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInputQty.ValueChanged += txtInputQty_ValueChanged;
                txtOutQty.ValueChanged += txtOutQty_ValueChanged;
                txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
            }
        }

        #endregion

        #region [품질정보] - 측정값
        private void txtVal_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            //DataTableConverter.GetValue(tb.DataContext, "CHK").Equals(0))
            if (tb.Visibility.Equals(Visibility.Visible))
            {
                DataTableConverter.SetValue(tb.DataContext, "CLCTVAL01", tb.Text);
            }

        }
        #endregion

        #region [품질정보] - rdoLot_Checked, Time 
        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
                return;

            GetQuality();
        }
        private void rdoTime_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
                return;

            GetQuality();
        }
        #endregion
       
        private void dgMaterial_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                sBeforeValue = string.Empty;
                if (!dgMaterial.CurrentCell.Column.Name.Equals("CHK"))
                {
                    if (dgMaterial.SelectedIndex > -1)
                    {
                        sBeforeValue = dgMaterial.CurrentCell.Value.ToString();
                        DataTableConverter.SetValue(dgMaterial.Rows[dgMaterial.SelectedIndex].DataItem, "CHK", 1);
                    }
                }
            }
            catch { }            
        }

        #region [자재투입 dgMaterial_CommittedEdit]
        private void dgMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!dgMaterial.CurrentCell.IsEditing)
                {
                    if (dgMaterial.CurrentCell.Column.Name.Equals("MTRLID"))
                    {
                        string sMTRLNAME;
                        string vMTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.CurrentRow.DataItem, "MTRLID"));

                        if (vMTRLID.Equals("") || dgMaterial.SelectedIndex < 0)
                        {
                            return;
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("MTRLID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["MTRLID"] = vMTRLID;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_MTRLDESC", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count > 0)
                            {                               
                                sMTRLNAME = result.Rows[0]["MTRLDESC"].ToString();
                                DataTableConverter.SetValue(dgMaterial.Rows[dgMaterial.SelectedIndex].DataItem, "MTRLDESC", sMTRLNAME);

                                DataTable dt2 = (dgMaterial.ItemsSource as DataView).Table;
                                Util.GridSetData(dgMaterial, dt2, FrameOperation, true);                                
                            }
                        }
                    }

                    if (!dgMaterial.CurrentCell.Column.Name.Equals("CHK")
                        && string.Equals(sBeforeValue.Trim(), dgMaterial.CurrentCell.Value.ToString().Trim()))  // 투입자재 그리드에 변경점이 없으면 체크 해제처리 한다.
                    {
                        if (dgMaterial.SelectedIndex > -1)
                        {
                            DataTableConverter.SetValue(dgMaterial.Rows[dgMaterial.SelectedIndex].DataItem, "CHK", 0);
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); return; }
        }
        #endregion

        #region [sublot 수량 변경]
        private void dgSubLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // 수정 가능여부에 따른 CHK 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODIFY_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                }
            }));

        }

        private void dgSubLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("CHK") || e.Column.Name.Equals("WIPQTY"))
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "MODIFY_YN").Equals("N"))
                {
                    e.Cancel = true;
                }

                //if (_PROCID.Equals(Process.WASHING) && !"N".Equals(_CELL_MNGT_TYPE_CODE)) //초소형, 셀관리 일경우 이벤트 취소
                //{
                //    e.Cancel = true;
                //}
            }

        }

        private void dgSubLot_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                /* 2019-05-07 최상민                 
                 * */
                if (e.Cell.Column.Name.Equals("WIPQTY") || e.Cell.Column.Name.Equals("CHK"))
                //if (e.Cell.Column.Name.Equals("WIPQTY"))
                //if (e.Cell.Column.Name.Equals("WIPQTY") || (_PROCID.Equals(Process.PACKAGING) && e.Cell.Column.Name.Equals("CHK")))
                {
                    ////C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                    ////if (dg.GetRowCount() == 0) return;

                    ////DataTable dtTmp = DataTableConverter.Convert(dg.ItemsSource);


                    ////Decimal dDefect = 0;
                    ////dDefect = Convert.ToDecimal(txtDefectQty.Value);
                    ////Decimal dLoss = 0;
                    ////dLoss = Convert.ToDecimal(txtLossQty.Value);
                    ////Decimal dPrdtReqQty = 0;
                    ////dPrdtReqQty = Convert.ToDecimal(txtPrdtReqQty.Value);

                    ////if (Convert.ToDecimal(Util.NVC_NUMBER(dtTmp.Compute("SUM(WIPQTY)", ""))) - dDefect - dLoss - dPrdtReqQty < 0)
                    ////{
                    ////    //Util.Alert("SFU1721");  //양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                    ////    Util.MessageValidation("SFU1721");
                    ////    return;
                    ////}

                    ////txtOutQty.Value =Convert.ToDouble(dtTmp.Compute("SUM(WIPQTY)", ""));

                    (dgSubLot.GetCell(e.Cell.Row.Index, dgSubLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [불량/Loss/물품청구 탭저장] ####### Visibility="Collapsed" #######
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1587"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private bool ValidationSaveDefectReInput()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectReInput))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (_LOTID.Length < 1)
            {
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }
        #endregion

        #region [품질정보 추가] ####### Visibility="Collapsed" #######
        private void btnQualityAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfo != null && dgQualityInfo.Rows.Count > 0)
                AddQuality();
        }
        #endregion

        #region [품질정보 탭조회]
        private void btnQualitySelect_Click(object sender, RoutedEventArgs e)
        {
            GetQuality();
        }
        #endregion

        #region [품질정보 탭저장]
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
            //"SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality();
                }
            });
        }
        #endregion

        #region [투입자재 탭 행추가]
        private void btnAddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
                return;

            if (string.IsNullOrEmpty(_LOTID))
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["CHK"] = true;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);

        }
        #endregion

        #region [투입자재 탭 행삭제]
        private void btnRemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
                return;

            DataTable dt = (dgMaterial.ItemsSource as DataView).Table;

            dt.Rows[dgMaterial.CurrentRow.Index].Delete();
            dt.AcceptChanges();

            Util.GridSetData(dgMaterial, dt, FrameOperation, true);

        }
        #endregion

        #region [투입자재 탭 삭제]
        private void btnDeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs != null)
            {
                //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                Util.MessageConfirm("SFU1815", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                        SetMaterial(_LOTID, "D");
                });
            }

        }
        #endregion

        #region [투입자재 탭 저장]
        private void btnSaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgMaterial;
            DataRow[] drs = Util.gridGetChecked(ref dg, "CHK");

            if (drs == null || drs.Length < 1)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }

            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU1984");  //투입자재 LOT ID를 입력하세요.
                    return;
                }

                if (string.IsNullOrEmpty(dr["INPUT_QTY"].ToString()) || dr["INPUT_QTY"].ToString().Equals("0.00000"))
                {
                    Util.MessageValidation("SFU1953");  //투입 수량을 입력하세요.
                    return;
                }
            }
            SetMaterial(_LOTID, "A");
        }
        #endregion

        #region [색지정보 탭저장]
        private void btnSaveColor_Click(object sender, RoutedEventArgs e)
        {
            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
            //"SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveColor();
                }
            });
        }
        #endregion

        #region [완성Lot 탭저장] ####### Visibility="Collapsed" #######
        private void btnSubLotSave_Click(object sender, RoutedEventArgs e)
        {
            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
            //        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSubLot();
                }
            });
        }
        #endregion

        #region [투입 반제품 탭]
        private void btnInHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHalfProductList();
        }

        private void dgInHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            if (dg != null)
                            {
                                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                         checkBox.IsChecked.HasValue &&
                                                         !(bool)checkBox.IsChecked))
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                if (chk != null) chk.IsChecked = false;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                        box.IsChecked.HasValue &&
                                                        (bool)box.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                }
                            }
                            break;
                    }
                }
            }));
        }

        private void cboInHalfMountPstnID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            GetHalfProductList();
        }

        private void btnInHalfProductInPutQty_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            COM001_009_MODIFY_INPUT_LOT_QTY popModifyQty = new COM001_009_MODIFY_INPUT_LOT_QTY { FrameOperation = FrameOperation };
            object[] parameters = new object[6];
            parameters[0] = _EQPTID;
            parameters[1] = _LOTID;
            parameters[2] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_LOTID").GetString();
            parameters[3] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_SEQNO").GetString();
            parameters[4] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_QTY").GetString();
            parameters[5] = _PROCID;
            C1WindowExtension.SetParameters(popModifyQty, parameters);

            popModifyQty.Closed += new EventHandler(popModifyQty_Closed);
            grdMain.Children.Add(popModifyQty);
            popModifyQty.BringToFront();
        }

        private void popModifyQty_Closed(object sender, EventArgs e)
        {
            COM001_009_MODIFY_INPUT_LOT_QTY pop = sender as COM001_009_MODIFY_INPUT_LOT_QTY;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetLotList(_LOTID);
            }

            grdMain.Children.Remove(pop);
        }

        private void btnInHalfProductInPutCancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return;
            }

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(_PROCID, Process.ASSEMBLY) || string.Equals(_PROCID, Process.WASHING))
                    {
                        InputHalfProductCancelAssembly();
                    }
                    else
                    {
                        InputHalfProductCancel();
                    }

                    GetLotList(_LOTID);
                }
            });
        }

        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                //저장하시겠습니까?
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (_WIPSTAT.Equals("EQPT_END"))
                        {
                            SaveWO();
                        }
                        else
                        {
                            Save();
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool CanSave()
        {
            bool bRet = false;
            try
            {
                dgDefect.EndEditRow(true);

                if (_LOTID.Equals(""))
                {
                    //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                    Util.MessageValidation("SFU1381");
                    return bRet;
                }

                if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return bRet;
                }

                if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return bRet;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return bRet;
                }

                if (_AREATYPE.Equals("E"))
                {
                    #region C20210810-000354 
                    DataTable dtInfo = new DataTable();
                    dtInfo.Columns.Add("AREAID", typeof(String));
                    dtInfo.Columns.Add("PROCID", typeof(String));
                    dtInfo.Columns.Add("LOTID", typeof(String));

                    DataRow drinfo = dtInfo.NewRow();
                    drinfo["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drinfo["PROCID"] = _PROCID;
                    drinfo["LOTID"] = _LOTID;
                    dtInfo.Rows.Add(drinfo);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_AREA_RSLT_MOD_IMPO", "INDATA", "OUTDATA", dtInfo);

                    if (dtRslt.Rows[0]["RESULT_VALUE"].ToString() == "NG")
                    {
                        Util.MessageValidation("SFU8454"); // 공정이동 후 수정하시기 바랍니다.
                        return bRet;
                    }
                    #endregion

                    #region E20230508-000004 
                    DataTable dtMove = new DataTable();
                    dtMove.Columns.Add("LOTID", typeof(String));

                    DataRow drMove = dtMove.NewRow();
                    drMove["LOTID"] = _LOTID;
                    dtMove.Rows.Add(drMove);

                    DataTable dtDalv = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_SHOP_MOVE_LOT_VALD", "RQSTDT", "RSLTDT", dtMove);

                    if (dtDalv.Rows[0]["RSLT_MSG"].ToString() == "NG")
                    {
                        Util.MessageValidation("SFU8913", Util.NVC(dtDalv.Rows[0]["LOTID"].ToString())); // 이동중인 Lot [%1]입니다. 확인 후 진행하세요
                        return bRet;
                    }
                    #endregion

                    #region E20231031-000379
                    if (_util.IsCommonCodeUse("PROD_REVISE_MOVE_SHOP_CHK", "EDIT_ALL"))
                    {
                        DataTable dtPlant = new DataTable();
                        dtPlant.Columns.Add("LOTID", typeof(String));

                        DataRow drPlant = dtPlant.NewRow();
                        drPlant["LOTID"] = _LOTID;
                        dtPlant.Rows.Add(drPlant);

                        DataTable dtRsltVald = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PLANT_MOVE_VALD", "INDATA", "OUTDATA", dtPlant);

                        if (dtRsltVald.Rows.Count > 0)
                        {
                            if (dtRsltVald.Rows[0]["EDIT_YN"].ToString() == "EDIT_N")
                            {
                                Util.MessageValidation("SFU8922", Util.NVC(dtRsltVald.Rows[0]["LOTID"].ToString())); // 해당 LOT [ %1 ]은 PLANT 이동 이력이 있습니다. 실적 수정이 불가합니다.
                                return bRet;
                            }
                        }
                    }
                    #endregion

                    #region E20231221-001759 - GM1 특정 스토커 적재된 HOLD LOT 실적 저장 허용
                    // [E20231221-001759] 홀드 실적 수정 저장 전에 입력한 WIPQTY(양품량)이 0이나 음수인 경우에는 저장이 안되도록 설정

                    if (IsAreaCommonCodeUse("ELEC_HOLD_PROD_RESULT_MODIFY_YN", _Rack_ID))
                    {
                        if (txtOutQty.Value <= 0)
                        {
                            Util.MessageValidation("SFU9911"); // 양품량은 0이나 음수로 변경될 수 없습니다.
                            return bRet;
                        }
                    }
                    #endregion

                    bRet = true;
                    return bRet;
                }

                #region 폴딩불량비율 [2018-04-03]
                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    if (_WIP_WRK_TYPE_CODE.Equals("SCRAP"))
                    {
                        DataTable dt = ((DataView)dgDefect_FOL.ItemsSource).Table.Select().CopyToDataTable();
                        int sum = (int)dt.AsEnumerable().Sum(r => r.GetValue("DFCT_QTY_DDT_RATE").GetDecimal());
                        if ((sum != 100))
                        {
                            Util.MessageValidation("SFU4518");      //불량 재생비율의 합은 100% 입니다.
                            return bRet;
                        }
                    }
                }
                #endregion

                #region WO공정이면 실적확정 수량 체크 (투입 수량과 실적확정 수량 일치 여부 확인) [2020-02-24]
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _PROCID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VWPROCESS_PROCID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1456"); // 공정 정보가 없습니다.
                    return bRet;
                }

                // WO 사용하는 공정이 아니면 실적확정 수량 체크 안 함.
                if (dtResult.Rows[0]["PLAN_MNGT_TYPE_CODE"].ToString() != "WO")
                {
                    bRet = true;
                }
                else
                {
                    DataTable SearchResult = Util.Get_ResultQty_Chk(_LOTID, _PROCID, _EQPTID, _EQSGID, _WIPSEQ);

                    if (SearchResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3530"); //작업실적이 없습니다.
                        return bRet;
                    }

                    // 체크 여부 
                    string sChkFlag = SearchResult.Rows[0]["RSLT_CNFM_QTY_CHK_TYPE"].ToString();  // N : 체크안함 , Y : 수량일치, Z : 차이 수량 허용            

                    // 투입/완성 수량 일치
                    if (sChkFlag == "Y")
                    {
                        //double dGapQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GAP_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GAP_QTY")));
                        double dGapQty = Util.NVC(txtInputDiffQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtInputDiffQty.Value));

                        if (dGapQty != 0)
                        {
                            //차이수량이 존재하여 실적 확정이 불가 합니다.\r\n생산실적을 재 확인 해주세요.
                            Util.MessageValidation("SFU3701");
                            return bRet;
                        }
                    }
                    // 투입/완성 수량 허용범위 체크
                    else if (sChkFlag == "Z")
                    {
                        // 허용수량
                        double dDIFF_PERMIT = Convert.ToDouble(SearchResult.Rows[0]["IN_OUT_DIFF_PERMIT_QTY"].ToString());

                        // 투입량
                        double dInputQty = Util.NVC(txtInputQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtInputQty.Value));

                        // 양품량
                        double dOutQty = Util.NVC(txtOutQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtOutQty.Value));

                        // 불량량
                        double dDefectQty = Util.NVC(txtDefectQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtDefectQty.Value));

                        // LOSS량
                        double dLossQty = Util.NVC(txtLossQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtLossQty.Value));

                        // 물품청구
                        double dPrdtReqQty = Util.NVC(txtPrdtReqQty.Value).Equals("") ? 0 : double.Parse(Util.NVC(txtPrdtReqQty.Value));

                        double dProdQty = dOutQty + dDefectQty + dLossQty + dPrdtReqQty;

                        if ((dInputQty + dDIFF_PERMIT) < dProdQty)
                        {
                            // 허용범위를 초과하였습니다.
                            Util.MessageValidation("SFU4261");
                            return bRet;
                        }
                    }

                    bRet = true;
                }
                #endregion

                //C20210323-000097
                if (!string.IsNullOrEmpty(_LOTID_AS) && _PROCID.Equals(Process.WINDING))
                {
                    double changedOutQty = txtOutQty.Value;
                    double changedDefectQty = txtDefectQty.Value;
                    double changedLossQty = txtLossQty.Value;
                    double changedPrdtReqQty = txtPrdtReqQty.Value;

                    if (_OUT_QTY != changedOutQty)
                    {
                        Util.MessageValidation("SFU3787");  //다음 공정에 투입된 LOT 의 양품량은 변경될 수 없습니다.
                        bRet = false;
                        return bRet;
                    }

                    if (_DEFECT_QTY + _LOSS_QTY + _PRDT_REQ_QTY != changedDefectQty + changedLossQty + changedPrdtReqQty)
                    {
                        Util.MessageValidation("SFU3788");  //다음 공정에 투입된 LOT 의 불량량은 변경될 수 없습니다.
                        bRet = false;
                        return bRet;
                    }

                    /*
                    if (_LOSS_QTY != changedLossQty)
                    {
                        Util.MessageValidation("SFU3789");  //다음 공정에 투입된 LOT 의 LOSS량은 변경될 수 없습니다.
                        bRet = false;
                        return bRet;
                    }
                    */
                }

                return bRet;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [Winding 투입 반제품 탭] [C20180209_07002]
        private void cboInputHalfWindingMountPstsID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHalfWinding();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInputHalfWindingLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInputHalfWinding();
            }
        }

        private void btnInputHalfWindingSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHalfWinding();
        }

        private void btnInputHalfWindingCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationInputHalfWindingCancel(dgInputHalfWinding))
                    return;

                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InputHalfWindingCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 대기Pancake Event  [C20180209_07002]
        private void cboPancakeMountPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitPancake();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationWaitPanCakeInput())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InputWaitPancake();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            //DataGridCurrentCellChanged(sender, e);
            try
            {
                // 대기 1개만 선택.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null && dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null && checkBox.IsChecked.HasValue && !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_maxPeviewProcessEndDay).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay;
                        int.TryParse(_maxPeviewProcessEndDay, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitPancakeLot.Text.Trim().Length > 0)
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_dtMinValid.AddDays(iDay) >= dtValid)
                                {
                                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        #endregion

        #region [RollMap]
        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {

            string lotUnitCode = string.Empty;
            // LOT 단위 ID -> EA : 원통형 롤맵 UI 팝업 호출 , M 기존 롤맵 UI 팝업 호출
            lotUnitCode = GetLotUnitCode();

            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName = string.Empty;

            if (string.Equals(_PROCID, Process.COATING))
            {
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_COATER" : "COM001_RM_CHART_CT";
            }
            else if (string.Equals(_PROCID, Process.ROLL_PRESSING))
            {
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_ROLLPRESS" : "COM001_RM_CHART_RP";
            }
            else if (string.Equals(_PROCID, Process.SLITTING))
            {
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_SLITTING" : "COM001_RM_CHART_SL";
            }
            else if (string.Equals(_PROCID, Process.TWO_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_TWOSLITTING";
            }
            else if (string.Equals(_PROCID, Process.HALF_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_MOBILE_HALFSLITTING";
            }
            else if (string.Equals(_PROCID, Process.SLIT_REWINDING) || string.Equals(_PROCID, Process.REWINDING))
            {
                mainFormName = "COM001_RM_CHART_RW";
            }
            else
            {
                return;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] Parameters = new object[10];
            Parameters[0] = _PROCID;
            Parameters[1] = _EQSGID;
            Parameters[2] = _EQPTID;
            Parameters[3] = _LOTID;
            Parameters[4] = _WIPSEQ;
            Parameters[5] = _LANE;
            Parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
            Parameters[7] = txtProdVerCode.Text;

            C1Window popup = obj as C1Window;
            C1WindowExtension.SetParameters(popup, Parameters);
            if (popup != null)
            {
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }
        #endregion
        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 공정의 AreaType ###]
        public void GetAreaType(string sProcID)
        {
            try
            {
                ShowLoadingIndicator();

                _AREATYPE = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = sProcID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    _AREATYPE = dtRslt.Rows[0]["PCSGID"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 작업대상 가져오기 ###]
        public void GetLotList(string ProdLotID = null)
        {
            try
            {
                ShowLoadingIndicator();

                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (Util.GetCondition(txtLotId).Equals("")) //lot id 가 없는 경우
                {
                    //dr["AREAID"] = Util.GetCondition(cboArea, "동을선택하세요.");
                    dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                    if (dr["AREAID"].Equals("")) return;

                    // 시스템 관리 >> 공정별 생산 실적 수정 화면에서 라인 검색 조건에 ALL 추가 요청. 요청자 by 정영호C
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                    //if (dr["EQSGID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);

                    //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                    if (dr["PROCID"].Equals("")) return;

                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                    if (dr["PROCID"].Equals("")) return;

                    dr["LOTID"] = Util.GetCondition(txtLotId);
                    bLot = true;
                }

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;

                if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                    sBizName = "DA_PRD_SEL_EDIT_LOT_LIST_MOBILE_SM";
                else
                    sBizName = "DA_PRD_SEL_EDIT_LOT_LIST_SM";


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && bLot == true)
                {
                    _AREATYPE = dtRslt.Rows[0]["AREATYPE"].ToString();
                    AreaCheck(dtRslt.Rows[0]["PROCID"].ToString());
                }

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // [2017-09-01 : 소형 관련 수정]
                    if (!string.IsNullOrWhiteSpace(ProdLotID))
                    {
                        int idx = _util.GetDataGridRowIndex(dgLotList, "LOTID", ProdLotID);
                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", 1);

                            //row 색 바꾸기
                            dgLotList.SelectedIndex = idx;
                            dgLotList.CurrentCell = dgLotList.GetCell(idx, dgLotList.Columns.Count - 1);

                            SetValue(dgLotList.Rows[idx].DataItem);
                       
                           // GetHalfProductList();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 불량/Loss/물품청구 - ATYPE, CTYPE 가져오기 ###]
        private void GetMBOMInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = txtWorkorderDetail.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }
                        else
                        {
                            if (_StackingYN.Equals("Y"))
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }


                            if (txtInAType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1337");      //HALF TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1306");        //ATYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            if (txtInCType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1401");     //MONO TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1326");        //CTYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            gDfctAQty = double.Parse(txtInAType.Text);
                            gDfctCQty = double.Parse(txtInCType.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetMBOMInfo_NJ()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = txtWorkorder.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                    newRow["CMCDTYPE"] = "NJ_FOLDING_TYPE";
                else
                    newRow["CMCDTYPE"] = "NJ_STP_TYPE";

                inTable.Rows.Add(newRow);

                dtBOM_CHK = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MBOM_INFO_STP", "RQSTDT", "RSLTDT", inTable);

                iBomcnt = dtBOM_CHK.Rows.Count;

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }

                        //dtBOM = searchResult.Copy();

                        //if (iBomcnt == 1)
                        if (_PROCID.Equals(Process.STP))
                        {
                            InitDefectDataGrid();

                            dtBOM = dtBOM_CHK.Copy();

                            for (int i = 0; i < dtBOM_CHK.Rows.Count; i++)
                            {
                                string sColName = string.Empty;
                                string sColName2 = string.Empty;
                                string sHeader = string.Empty;

                                List<string> dIndex = new List<string>();
                                List<string> dIndex2 = new List<string>();

                                int ichk = 0;

                                ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                sColName = "REG" + (ichk).ToString();
                                sHeader = Util.NVC(dtBOM_CHK.Rows[i]["ATTRIBUTE3"]).ToString();

                                // 불량 수량 컬럼 위치 변경.
                                int iColIdx = 0;

                                dIndex.Add(ObjectDic.Instance.GetObjectName("입력"));
                                dIndex.Add(sHeader);

                                iColIdx = dgDefect_NJ.Columns["INPUT_FC"].Index;

                                Util.SetGridColumnNumeric(dgDefect_NJ, sColName, dIndex, sHeader, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // [입력, FOLDED CELL]     

                                if (dgDefect_NJ.Columns.Contains(sColName))
                                {
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                                }

                                if (dgDefect_NJ.Rows.Count == 0) continue;

                                SetBOMCnt(i, Util.NVC(dtBOM_CHK.Rows[i]["PRODUCT_LEVEL3_CODE"]), Util.NVC(dtBOM_CHK.Rows[i]["PROC_INPUT_CNT"]));

                                iCnt = i;

                                C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgDefect_NJ.Columns[sColName]
                                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

                            }
                        }
                        else
                        {
                            InitDefectDataGrid();

                            dtBOM = searchResult.Copy();

                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                string sColName = string.Empty;
                                string sColName2 = string.Empty;
                                string sHeader = string.Empty;

                                List<string> dIndex = new List<string>();
                                List<string> dIndex2 = new List<string>();

                                int ichk = 0;

                                ichk = Get_Type_Chk(Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                sColName = "REG" + (ichk).ToString();
                                sHeader = Util.NVC(searchResult.Rows[i]["ATTRIBUTE3"]).ToString();

                                // 불량 수량 컬럼 위치 변경.
                                int iColIdx = 0;

                                dIndex.Add(ObjectDic.Instance.GetObjectName("입력"));
                                dIndex.Add(sHeader);

                                iColIdx = dgDefect_NJ.Columns["INPUT_FC"].Index;

                                Util.SetGridColumnNumeric(dgDefect_NJ, sColName, dIndex, sHeader, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // [입력, FOLDED CELL]     

                                if (dgDefect_NJ.Columns.Contains(sColName))
                                {
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                                }

                                if (dgDefect_NJ.Rows.Count == 0) continue;

                                SetBOMCnt(i, Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]), Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"]));

                                iCnt = i;

                                C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgDefect_NJ.Columns[sColName]
                                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

                            }
                        }
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void InitDefectDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgDefect_NJ);

                for (int i = dgDefect_NJ.Columns.Count; i-- > 0;)
                {
                    if (dgDefect_NJ.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        dgDefect_NJ.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgDefect_NJ.Columns.Count; i-- > 0;)
                {
                    if (dgDefect_NJ.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgDefect_NJ.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgDefect_NJ.Columns[i].Name))
                                dt.Columns.RemoveAt(i);

                        dgDefect_NJ.Columns.RemoveAt(i);
                    }
                }
            }
        }
        #endregion

        #region [### 불량/Loss/물품청구 탭 조회 ###]
        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string BizNAme = string.Empty;
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                    BizNAme = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";
                else
                    //C20210222-000365 불량/Loss항목 표준화 적용 DA_QCA_SEL_WIPRESONCOLLECT_INFO -> BR_PRD_SEL_WIPRESONCOLLECT_INFO 변경
                    BizNAme = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    if (dgDefect.Columns.Contains("RESNNAME"))
                        dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                    if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                        dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                    if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                        dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;

                    DataSet ds = new DataSet();
                    ds.Tables.Add(inTable);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizNAme, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                    if (CommonVerify.HasTableInDataSet(dsResult))
                    {
                        //'AP' 동별 / 공정별
                        //'LP' 라인 / 공정별
                        dgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                        dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                    }

                    _dtLengthExceeed = dsResult.Tables["OUTDATA"].Copy();
                }
                else
                {
                    dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizNAme, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgDefect, dtResult, null);

                    _dtLengthExceeed = dtResult.Copy();

                    if (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("DFCT_SYS_TYPE") && Util.NVC(dtResult.Rows[0]["DFCT_SYS_TYPE"]).Equals("Q"))
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 설비품질정보 탭 조회 ###]
        private void GetEqpFaultyData() //string sLot, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgEqpFaulty.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 품질정보 탭 조회 ###]
        private void GetQuality()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));

                // [2017-09-01 : 소형 관련 수정]
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    //dtRqst.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CLCT_BAS_CODE", typeof(string));
                    dtRqst.Columns.Add("CLCTSEQ", typeof(string));
                }
                else if (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING) || _PROCID.Equals(Process.PACKAGING))
                {
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CLCTSEQ", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _PROCID;
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;

                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    dr["EQPTID"] = _EQPTID;
                    dr["CLCT_BAS_CODE"] = (bool)rdoLot.IsChecked ? "Lot" : "Time";
                    dr["CLCTSEQ"] = cboCLCTSEQ.SelectedValue == null ? "1" : cboCLCTSEQ.SelectedValue.ToString();
                }
                else if (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING) || _PROCID.Equals(Process.PACKAGING))
                {
                    dr["EQPTID"] = _EQPTID;
                    dr["CLCTSEQ"] = cboCLCTSEQ.SelectedValue == null ? "1" : cboCLCTSEQ.SelectedValue.ToString();
                }

                dtRqst.Rows.Add(dr);

                string BizName = string.Empty;

                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    //BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
                    BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT_MODIFY";

                    if ((bool)rdoLot.IsChecked)
                    {
                        cboCLCTSEQ.IsEnabled = false;
                        dgQualityInfo.Columns["CLCT_ITVL"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        cboCLCTSEQ.IsEnabled = true;
                        dgQualityInfo.Columns["CLCT_ITVL"].Visibility = Visibility.Visible;
                    }
                }
                else if (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING) || _PROCID.Equals(Process.PACKAGING))
                {
                    //입력하지 않은 품질지표도 보이도록.
                    //전극과 조립 공정의 품질정보 가지고 오는 기준이 달라서 DA_QCA_SEL_WIPDATACOLLECT_LOT_PROC_END 활용 못함
                    BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT_MODIFY";
                }
                else
                {
                    BizName = "DA_QCA_SEL_WIPDATACOLLECT_LOT_PROC_END";
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgQualityInfo);
                //dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dtRqst.Columns.Remove("WIPSEQ");
                    dtRqst.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    dtRqst.Columns.Add("VER_CODE", typeof(string));
                    dtRqst.Columns.Add("LANEQTY", typeof(Int16));

                    dtRqst.Rows[0]["CLCT_PONT_CODE"] = string.IsNullOrWhiteSpace(_COATING_SIDE_TYPE_CODE) ? null : _COATING_SIDE_TYPE_CODE;
                    dtRqst.Rows[0]["VER_CODE"] = string.IsNullOrWhiteSpace(_VERCODE) ? null : _VERCODE;
                    dtRqst.Rows[0]["LANEQTY"] = string.IsNullOrWhiteSpace(_LANE_QTY) ? null : _LANE_QTY;

                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM_END", "INDATA", "RSLTDT", dtRqst);
                }

                Util.GridSetData(dgQualityInfo, dtRslt, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetQualityCount()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT_MODIFY_COUNT", "INDATA", "OUTDATA", dtRqst);

                cboCLCTSEQ.ItemsSource = null;
                cboCLCTSEQ.ItemsSource = dtRslt.Copy().AsDataView();

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    tbCLCTSEQ.Visibility = Visibility.Collapsed;
                    cboCLCTSEQ.Visibility = Visibility.Collapsed;
                }
                else if (dtRslt.Rows.Count == 1)
                {
                    tbCLCTSEQ.Visibility = Visibility.Collapsed;
                    cboCLCTSEQ.Visibility = Visibility.Collapsed;
                    cboCLCTSEQ.SelectedIndex = 0;
                }
                else
                {
                    tbCLCTSEQ.Visibility = Visibility.Visible;
                    cboCLCTSEQ.Visibility = Visibility.Visible;
                    cboCLCTSEQ.SelectedIndex = 0;
                }

                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [### 선분산 믹서 투입자재 탭 조회 ###]
        private void GetMaterialSummary(string sLotID, string sWOID)
        {
            try
            {
                Util.gridClear(dgMaterialList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                Indata["WOID"] = sWOID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgMaterialList, dtResult, FrameOperation, true);

                // 믹서공정은 투입자재 총사용량 = 생산량 [2017-03-02]
                double inputMtrlSumQty = 0;
                foreach (DataRow row in dtResult.Rows)
                {
                    inputMtrlSumQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));
                }

                if (inputMtrlSumQty > 0)
                {
                    txtInputQty.Value = inputMtrlSumQty;
                    //SetInputQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion

        #region [### 투입자재 탭 조회 ###]
        private void GetMaterial()
        {
            try
            {
                Util.gridClear(dgMaterial);

                SetGridComboItem(dgMaterial.Columns["MTRLID"], _WORKORDER);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL2", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgMaterial, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion

        #region [### 투입자재 탭 Wo의 자재 콤보 셋팅 ###]
        void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL2", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }
        #endregion

        #region [### 색지정보 조회 ###]
        private void GetColor()
        {
            try
            {
                if (_PROCID != Process.ROLL_PRESSING)
                    return;

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LOTID"] = _LOTID;
                dr["PROCID"] = _PROCID;
                dr["WIPSEQ"] = _WIPSEQ;


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "RSLTDT", dtRqst);

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Rows[0]["EQPTID"] = _EQPTID;

                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG", "INDATA", "RSLTDT", dtRqst);

                }

                Util.GridSetData(dgColor, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 완성 LOT 탭 조회 ###]
        private void GetSubLot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string sBizName = string.Empty;
                sBizName = "DA_PRD_SEL_EDIT_SUBLOT_LIST_SM";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _LOTID;
                //dr["PR_LOTID"] = "BAPA701R32";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSubLot, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 불량/Loss/물품청구 탭 저장 ###]
        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                int iSeq = 0;

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);


                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = _LOTID.Trim();
                    newRow["WIPSEQ"] = int.TryParse(_WIPSEQ, out iSeq) ? iSeq : 1;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bShowMsg)
                        {
                            //Util.AlertInfo("SFU1275");      //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                            // ClearValue();
                            GetLotList();
                        }

                        GetDefectInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 품질정보 탭 DATA 추가 ###] ####### Visibility="Collapsed" #######
        private void AddQuality()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _PROCID;
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", dtRqst);

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _PROCID;
                drAdd["LOTID"] = _LOTID;

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM", "INDATA", "OUTDATA", dtRqstAdd);

                //CLCTSEQ TYPE 문제로 MERGE 사용 못했슴
                //dtRsltAdd.Columns["CLCTSEQ"].DataType = typeof(Int16);
                //dtRslt.Columns["CLCTSEQ"].DataType = typeof(Int16);

                //dtRslt.Merge(dtRsltAdd);

                object oMax = dtRslt.Compute("MAX(CLCTSEQ)", String.Empty);

                int iMax = 1;
                if (!oMax.Equals(DBNull.Value))
                {
                    iMax = Convert.ToInt16(oMax) + 1;
                }

                foreach (DataRow dr1 in dtRsltAdd.Rows)
                {
                    DataRow drNew = dtRslt.NewRow();
                    drNew["CLCTITEM"] = dr1["CLCTITEM"];
                    drNew["CLCTNAME"] = dr1["CLCTNAME"];
                    drNew["CLCTUNIT"] = dr1["CLCTUNIT"];
                    drNew["USL"] = dr1["USL"];
                    drNew["LSL"] = dr1["LSL"];
                    drNew["CLCTSEQ"] = iMax;
                    drNew["INSP_VALUE_TYPE_CODE"] = dr1["INSP_VALUE_TYPE_CODE"];
                    drNew["TEXTVISIBLE"] = dr1["TEXTVISIBLE"];
                    drNew["COMBOVISIBLE"] = dr1["COMBOVISIBLE"];

                    dtRslt.Rows.Add(drNew);
                }

                //Util.gridClear(dgQualityInfo);

                //dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgQualityInfo, dtRslt, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 품질정보 탭 저장 ###] ####### Visibility="Collapsed" #######
        private void SaveQuality()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("CLCTMAX", typeof(string));
                dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                //foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgQualityInfo))
                //{
                //    if (!row["CLCTVAL01"].Equals(DBNull.Value))
                //    {
                //        DataRow dr = dtRqst.NewRow();
                //        dr["SRCTYPE"] = "UI";
                //        dr["LOTID"] = _LOTID;
                //        dr["WIPSEQ"] = _WIPSEQ;
                //        dr["CLCTSEQ"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                //        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                //        dr["CLCTVAL01"] = row["CLCTVAL01"];// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                //        dr["CLCTMAX"] = row["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                //        dr["CLCTMIN"] = row["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                //        dr["EQPTID"] = _EQPTID;
                //        dr["USERID"] = LoginInfo.USERID;

                //        dtRqst.Rows.Add(dr);
                //    }
                //}

                //foreach (DataRowView row in DataGridHandler.GetAddedItems(dgQualityInfo))
                //{

                //    if (!row["CLCTVAL01"].Equals(DBNull.Value))
                //    {
                //        DataRow dr = dtRqst.NewRow();
                //        dr["SRCTYPE"] = "UI";
                //        dr["LOTID"] = _LOTID;
                //        dr["WIPSEQ"] = _WIPSEQ;
                //        dr["CLCTSEQ"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                //        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                //        dr["CLCTVAL01"] = row["CLCTVAL01"];// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                //        dr["CLCTMAX"] = row["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                //        dr["CLCTMIN"] = row["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                //        dr["EQPTID"] = _EQPTID;
                //        dr["USERID"] = LoginInfo.USERID;

                //        dtRqst.Rows.Add(dr);
                //    }
                //}

                foreach (DataRowView row in dgQualityInfo.ItemsSource)
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["LOTID"] = _LOTID;
                    dr["WIPSEQ"] = _WIPSEQ;
                    dr["CLCTSEQ"] = string.IsNullOrWhiteSpace(row["CLCTSEQ"].ToString()) || row["CLCTSEQ"].ToString().Equals("0") ? 1 : row["CLCTSEQ"];
                    dr["CLCTITEM"] = row["CLCTITEM"];
                    if ("NaN".Equals(row["CLCTVAL01"].ToString().Trim()))
                    {
                        dr["CLCTVAL01"] = "";
                    }
                    else
                    {
                        dr["CLCTVAL01"] = row["CLCTVAL01"].ToString().Trim();
                    }
                    dr["CLCTMAX"] = row["USL"];
                    dr["CLCTMIN"] = row["LSL"];
                    dr["EQPTID"] = _EQPTID;
                    dr["USERID"] = LoginInfo.USERID;

                    dtRqst.Rows.Add(dr);
                }

                if (dtRqst.Rows.Count > 0)
                {
                    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIPDATACOLLECT", "INDATA", null, dtRqst);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT_PROC_END", "INDATA", null, dtRqst);

                    //Util.AlertInfo("SFU1270");  //저장되었습니다.
                    Util.MessageInfo("SFU1270");
                    GetQuality();
                }
                else
                {
                    //Util.Alert("SFU1566");  //변경된 데이터가 없습니다.
                    Util.MessageValidation("SFU1566");
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 투입자재 탭 저장 ###]
        private void SetMaterial(string LotID, string PROC_TYPE)
        {
            if (dgMaterial.Rows.Count < 1)
                return;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["LOTID"] = _LOTID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("IN_INPUT");
                InLotdataTable.Columns.Add("INPUT_LOTID", typeof(string));
                InLotdataTable.Columns.Add("MTRLID", typeof(string));
                InLotdataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                InLotdataTable.Columns.Add("PROC_TYPE", typeof(string));
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(string));

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals(""))
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                            inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = Util.NVC(row["INPUT_SEQNO"]);
                            InLotdataTable.Rows.Add(inLotDataRow);
                        }
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                #region [Merge LotTrace (2018.04.24)]
                string sProcID = Util.NVC(cboProcess.SelectedValue);
                if (sProcID.Equals(Process.COATING) || sProcID.Equals(Process.TOP_COATING) || sProcID.Equals(Process.BACK_COATING))
                {
                    MergeLotTrace(_LOTID);
                }
                #endregion

                Thread.Sleep(500);

                GetMaterial();
                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion

        #region [### 색지정보 탭 저장 ###]
        private void SaveColor()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));
                dtRqst.Columns.Add("CLCTSEQ", typeof(string));

                DataTable dt = (dgColor.ItemsSource as DataView).Table;
                DataRow dr = null;

                foreach (DataRow _iRow in dt.Rows)
                {
                    dr = dtRqst.NewRow();

                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = _LOTID;
                    dr["EQPTID"] = _EQPTID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CLCTITEM"] = _iRow["CLCTITEM"];
                    dr["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]);
                    dr["WIPSEQ"] = _WIPSEQ;
                    dr["CLCTSEQ"] = 1;
                    dtRqst.Rows.Add(dr);
                }

                if (dtRqst.Rows.Count > 0)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                    //Util.AlertInfo("SFU1270");  //저장되었습니다.
                    Util.MessageInfo("SFU1270");
                    GetColor();
                }
                else
                {
                    //Util.Alert("SFU1566");  //변경된 데이터가 없습니다.
                    Util.MessageValidation("SFU1566");
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 완성LOT 탭 저장 ###]
        private bool SaveSubLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = GetSaveDataSet();
                DataTable inDataTable = inData.Tables["INDATA"];

                DataTable dtSubLot = DataTableConverter.Convert(dgSubLot.ItemsSource);

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                foreach (DataRow dr in dtSubLot.Rows)
                {
                    if (dr["CHK"].Equals(1))
                    {
                        inDataTable.Clear();
                        DataRow drIn = inDataTable.NewRow();

                        drIn["LOTID"] = dr["LOTID"];
                        drIn["WIPSEQ"] = dr["WIPSEQ"];

                        // if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.WASHING))
                        drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");

                        drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["WOID"] = dr["WOID"];
                        drIn["WIPQTY_ED"] = dr["WIPQTY"];
                        drIn["WIPQTY2_ED"] = dr["WIPQTY"];
                        drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["SHIFT"] = txtShift.Tag;
                        drIn["WRK_USERID"] = txtWorker.Tag;
                        drIn["WRK_USER_NAME"] = txtWorker.Text;
                        //drIn["NOTE"] = txtNote.Text;
                        drIn["NOTE"] = SetWipNote();

                        /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            //drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함


                            2023.09.20 고해선 책임 요청으로 원복
                         */
                        drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                        drIn["REQ_NOTE"] = txtReqNote.Text;

                        if (_PROCID.Equals(Process.WINDING))
                        {
                            drIn["CHANGE_WIPQTY_FLAG"] = "Y";
                            drIn["MODL_CHG_FRST_PROD_LOT_FLAG"] = (bool)chkModelChangeFirstProductLotFlag.IsChecked ? "Y" : null;
                        }

                        //E20230424-000089 : ESNJ ZZS 공정 WIP 수량 수정 가능하도록 변경
                        if (LoginInfo.CFG_SHOP_ID == "G182")
                        {
                            if (_PROCID.Equals(Process.ZZS))
                            {
                                drIn["CHANGE_WIPQTY_FLAG"] = "Y";
                            }
                        }

                        inDataTable.Rows.Add(drIn);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", "OUT_WIPHISTORY", inData);
                    }
                }

                //DataSet dsRslt = new DataSet();
                //if (inDataTable != null && inDataTable.Rows.Count > 0)
                //    dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", "OUT_WIPHISTORY", inData);

                return true;

                //GetSubLot();
                ////Util.AlertInfo("SFU1270");  //저장되었습니다.
                //Util.MessageInfo("SFU1270");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### WO 저장 ###] ####### Visibility="Collapsed" ####### 
        private void SaveWO()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow drIn = inDataTable.NewRow();
                drIn["LOTID"] = _LOTID;
                drIn["WOID"] = Util.GetCondition(txtWorkorder);
                drIn["NOTE"] = Util.GetCondition(txtNote);
                drIn["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(drIn);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT_WOID", "INDATA", null, inData);

                GetLotList();
                ClearValue();

                //Util.AlertInfo("SFU1270");  //저장되었습니다.
                Util.MessageInfo("SFU1270");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Merge LotTrace (2018.04.24)]
        private void MergeLotTrace(string sLotID)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                InLotdataTable.Columns.Add("LOTID", typeof(string));

                inLotDataRow = InLotdataTable.NewRow();
                inLotDataRow["LOTID"] = sLotID;
                InLotdataTable.Rows.Add(inLotDataRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOTTRACE_FOR_LOT", "INDATA,INLOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region [### 저장 ###]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                DoEvents();

                if (_PROCID.Equals(Process.WINDING))
                {
                    if (Math.Abs(txtAssyResultQty.Value) > 0)
                    {
                        // 추가불량 오류 : 추가불량 수량을 재 확인하신 후 반영(저장)해 주세요.
                        Util.MessageValidation("SFU3665");
                        //return;
                    }
                }

                // 완성 LOT 저장
                //if (_PROCID.Equals(Process.WASHING) == false)   //와싱이 아닐 경우만
                //{
                //    if (!SaveSubLot())
                //        return;
                //}

                if (!SaveSubLot())
                    return;

                if (_PROCID.Equals(Process.STACKING_FOLDING) || _PROCID.Equals(Process.STP))
                {
                    #region ### STACKING_FOLDING ###
                    DataSet inData = GetSaveDataSet();

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    {
                        #region 남경

                        #region [INDATA]

                        DataTable inDataTable = inData.Tables["INDATA"];

                        DataRow drIn = inDataTable.NewRow();
                        drIn["LOTID"] = _LOTID;
                        drIn["WIPSEQ"] = _WIPSEQ;

                        DateTime dtTime;
                        dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                        drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["SHIFT"] = txtShift.Tag;
                        drIn["WOID"] = txtWorkorder.Text;

                        //2017-02-04 QTY2 계산시 기준정보 미입력시 QTY와 값이 동일하도록 1로 셋팅.
                        Decimal dLaneQty = string.IsNullOrWhiteSpace(txtLaneQty.Text) ? 1 : Convert.ToDecimal(txtLaneQty.Text);
                        Decimal dLanePtnQty = string.IsNullOrWhiteSpace(_LANEPTNQTY) ? 1 : Convert.ToDecimal(_LANEPTNQTY);

                        Decimal dWipqty_ed = Convert.ToDecimal(txtOutQty.Value);

                        drIn["LANE_QTY"] = dLaneQty;
                        drIn["PROD_VER_CODE"] = Util.GetCondition(txtProdVerCode);

                        //if (!_AREATYPE.Equals("A")) //조립이 아닐경우에만 셋팅

                        if (!_WIP_TYPE_CODE.Equals("PROD")) //조립이 아닐경우에만 셋팅
                        {
                            drIn["WIPQTY_ED"] = dWipqty_ed;
                            drIn["WIPQTY2_ED"] = dWipqty_ed * dLaneQty * dLanePtnQty;
                        }

                        if (txtLossQty.Value != 0)
                        {
                            drIn["LOSS_QTY"] = txtLossQty.Value;
                            drIn["LOSS_QTY2"] = Convert.ToDecimal(txtLossQty.Value) * dLaneQty * dLanePtnQty;
                        }
                        if (txtDefectQty.Value != 0)
                        {
                            drIn["DFCT_QTY"] = txtDefectQty.Value;
                            drIn["DFCT_QTY2"] = Convert.ToDecimal(txtDefectQty.Value) * dLaneQty * dLanePtnQty;
                        }
                        if (txtPrdtReqQty.Value != 0)
                        {
                            drIn["PRDT_REQ_QTY"] = txtPrdtReqQty.Value;
                            drIn["PRDT_REQ_QTY2"] = Convert.ToDecimal(txtPrdtReqQty.Value) * dLaneQty * dLanePtnQty;
                        }

                        if (_PROCID.Equals(Process.PACKAGING) || _PROCID.Equals(Process.ASSEMBLY))
                        {
                            drIn["INPUT_DIFF_QTY"] = txtInputDiffQty.Value;
                        }

                        drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["WRK_USERID"] = txtWorker.Tag;
                        drIn["WRK_USER_NAME"] = txtWorker.Text;
                        //drIn["NOTE"] = txtNote.Text;
                        drIn["NOTE"] = SetWipNote();

                        /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            //drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                            2023.09.20 고해선 책임 요청으로 원복
                         */
                        drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                        // drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
                        // [2017-09-01 : 소형 관련 수정] 
                        if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                            drIn["CHANGE_WIPQTY_FLAG"] = "N";       // 소형 공정 N
                        else
                            drIn["CHANGE_WIPQTY_FLAG"] = "Y";       // 소형 공정 N

                        drIn["REQ_NOTE"] = txtReqNote.Text;
                        inDataTable.Rows.Add(drIn);

                        #endregion

                        #region [IN_LOSS]
                        DataTable inDataLoss = inData.Tables["IN_LOSS"];

                        DataTable dtdefect = DataTableConverter.Convert(dgDefect_NJ.ItemsSource);
                        DataRow[] drLoss = dtdefect.Select("ACTID ='LOSS_LOT'");

                        foreach (DataRow dr in drLoss)
                        {
                            DataRow drInLoss = inDataLoss.NewRow();

                            drInLoss["LOTID"] = _LOTID;
                            drInLoss["WIPSEQ"] = _WIPSEQ;
                            drInLoss["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_FC"]).Equals(""))
                            {
                                drInLoss["RESNQTY"] = Convert.ToDecimal(dr["CALC_FC"]);
                                drInLoss["RESNQTY2"] = Convert.ToDecimal(dr["CALC_FC"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInLoss["RESNQTY"] = 0;
                                drInLoss["RESNQTY2"] = 0;
                            }
                            drInLoss["RESNCODE_CAUSE"] = "";
                            drInLoss["PROCID_CAUSE"] = "";

                            inDataLoss.Rows.Add(drInLoss);
                        }

                        #endregion

                        #region [IN_DFCT]

                        DataTable inDataDfct = inData.Tables["IN_DFCT"];

                        DataRow[] drDefect = dtdefect.Select("ACTID ='DEFECT_LOT'");
                        foreach (DataRow dr in drDefect)
                        {

                            DataRow drInDfct = inDataDfct.NewRow();

                            drInDfct["LOTID"] = _LOTID;
                            drInDfct["WIPSEQ"] = _WIPSEQ;
                            drInDfct["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_FC"]).Equals(""))
                            {
                                drInDfct["RESNQTY"] = Convert.ToDecimal(dr["CALC_FC"]);
                                drInDfct["RESNQTY2"] = Convert.ToDecimal(dr["CALC_FC"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInDfct["RESNQTY"] = 0;
                                drInDfct["RESNQTY2"] = 0;
                            }
                            drInDfct["RESNCODE_CAUSE"] = "";
                            drInDfct["PROCID_CAUSE"] = "";

                            inDataDfct.Rows.Add(drInDfct);
                        }

                        #endregion

                        #region [IN_PRDT_REQ]

                        DataTable inDataPrdtReq = inData.Tables["IN_PRDT_REQ"];

                        DataRow[] drCharge = dtdefect.Select("ACTID ='CHARGE_PROD_LOT'");
                        foreach (DataRow dr in drCharge)
                        {
                            DataRow drInPrdtReq = inDataPrdtReq.NewRow();

                            drInPrdtReq["LOTID"] = _LOTID;
                            drInPrdtReq["WIPSEQ"] = _WIPSEQ;
                            drInPrdtReq["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_FC"]).Equals(""))
                            {
                                drInPrdtReq["RESNQTY"] = Convert.ToDecimal(dr["CALC_FC"]);
                                drInPrdtReq["RESNQTY2"] = Convert.ToDecimal(dr["CALC_FC"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInPrdtReq["RESNQTY"] = 0;
                                drInPrdtReq["RESNQTY2"] = 0;
                            }
                            drInPrdtReq["RESNCODE_CAUSE"] = "";
                            drInPrdtReq["PROCID_CAUSE"] = "";
                            drInPrdtReq["RESNNOTE"] = dr["RESNNOTE"];
                            drInPrdtReq["COST_CNTR_ID"] = dr["COST_CNTR_ID"];

                            inDataPrdtReq.Rows.Add(drInPrdtReq);
                        }
                        #endregion

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", "OUT_WIPHISTORY", inData);

                        #region Cell_Type 저장

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        RQSTDT1.Columns.Add("SHOPID", typeof(String));
                        RQSTDT1.Columns.Add("AREAID", typeof(String));
                        RQSTDT1.Columns.Add("PROCID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = _LOTID;
                        dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        dr1["PROCID"] = _PROCID;
                        RQSTDT1.Rows.Add(dr1);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LOTID", typeof(String));
                        RQSTDT.Columns.Add("WIPSEQ", typeof(String));
                        RQSTDT.Columns.Add("PRODID", typeof(String));
                        RQSTDT.Columns.Add("ACTID", typeof(String));
                        RQSTDT.Columns.Add("RESNCODE", typeof(String));
                        RQSTDT.Columns.Add("REG_QTY", typeof(Decimal));
                        RQSTDT.Columns.Add("CALC_QTY", typeof(Decimal));
                        RQSTDT.Columns.Add("USERID", typeof(String));

                        for (int icnt = 2; icnt < dgDefect_NJ.GetRowCount() + 2; icnt++)
                        {
                            for (int jcnt = 0; jcnt < SearchResult.Rows.Count; jcnt++)
                            {
                                string sCode = Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]);

                                if (sCode.Equals("FC") || sCode.Equals("SC"))
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LOTID"] = _LOTID;
                                    dr["WIPSEQ"] = _WIPSEQ;
                                    dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                                    dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "ACTID"));
                                    dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "RESNCODE"));
                                    dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "INPUT_FC"));
                                    dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "CALC_FC"));
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                }
                                else
                                {
                                    int ichk = Get_Type_Chk(sCode);

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LOTID"] = _LOTID;
                                    dr["WIPSEQ"] = _WIPSEQ;
                                    dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                                    dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "ACTID"));
                                    dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "RESNCODE"));
                                    dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "REG" + ichk.ToString()));
                                    //dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "CALC_FC"));
                                    dr["CALC_QTY"] = 0;
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                }
                            }
                        }

                        new ClientProxy().ExecuteService("BR_PRD_REG_CELL_TYPE_DFCT_HIST", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        });
                        #endregion

                        #endregion
                    }
                    else
                    {
                        #region 오창
                        #region [INDATA]

                        DataTable inDataTable = inData.Tables["INDATA"];

                        DataRow drIn = inDataTable.NewRow();
                        drIn["LOTID"] = _LOTID;
                        drIn["WIPSEQ"] = _WIPSEQ;

                        DateTime dtTime;
                        dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                        drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["SHIFT"] = txtShift.Tag;
                        drIn["WOID"] = txtWorkorder.Text;

                        //2017-02-04 QTY2 계산시 기준정보 미입력시 QTY와 값이 동일하도록 1로 셋팅.
                        Decimal dLaneQty = string.IsNullOrWhiteSpace(txtLaneQty.Text) ? 1 : Convert.ToDecimal(txtLaneQty.Text);
                        Decimal dLanePtnQty = string.IsNullOrWhiteSpace(_LANEPTNQTY) ? 1 : Convert.ToDecimal(_LANEPTNQTY);

                        Decimal dWipqty_ed = Convert.ToDecimal(txtOutQty.Value);

                        //if (txtLossQty.Value != 0)
                        //{
                        drIn["LANE_QTY"] = dLaneQty; // Convert.ToInt16(txtLaneQty.Text);
                                                     //}
                        drIn["PROD_VER_CODE"] = Util.GetCondition(txtProdVerCode);

                        //if (!_AREATYPE.Equals("A")) //조립이 아닐경우에만 셋팅

                        if (!_WIP_TYPE_CODE.Equals("PROD")) //조립이 아닐경우에만 셋팅
                        {
                            //drIn["WIPQTY_ED"] = txtOutQty.Value;
                            //drIn["WIPQTY2_ED"] = Convert.ToDecimal(txtOutQty.Value) * dLaneQty * dLanePtnQty;
                            drIn["WIPQTY_ED"] = dWipqty_ed;
                            drIn["WIPQTY2_ED"] = dWipqty_ed * dLaneQty * dLanePtnQty;

                            // Biz에서 처리
                            ////drIn["WIPQTY_ED_DIFF"] = dWipqty_ed - Convert.ToDecimal(_dprOutQty);
                            ////drIn["WIPQTY2_ED_DIFF"] = (dWipqty_ed - Convert.ToDecimal(_dprOutQty)) * dLaneQty * dLanePtnQty;

                        }

                        if (txtLossQty.Value != 0)
                        {
                            drIn["LOSS_QTY"] = txtLossQty.Value;
                            drIn["LOSS_QTY2"] = Convert.ToDecimal(txtLossQty.Value) * dLaneQty * dLanePtnQty;
                        }
                        if (txtDefectQty.Value != 0)
                        {
                            drIn["DFCT_QTY"] = txtDefectQty.Value;
                            drIn["DFCT_QTY2"] = Convert.ToDecimal(txtDefectQty.Value) * dLaneQty * dLanePtnQty;
                        }
                        if (txtPrdtReqQty.Value != 0)
                        {
                            drIn["PRDT_REQ_QTY"] = txtPrdtReqQty.Value;
                            drIn["PRDT_REQ_QTY2"] = Convert.ToDecimal(txtPrdtReqQty.Value) * dLaneQty * dLanePtnQty;
                        }

                        //if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && txtInputDiffQty.Value != 0) //차이수량 보여주기
                        //{
                        //drIn["INPUT_DIFF_QTY"] = txtInputDiffQty.Value;
                        //}

                        if (_PROCID.Equals(Process.PACKAGING) || _PROCID.Equals(Process.ASSEMBLY))
                        {
                            drIn["INPUT_DIFF_QTY"] = txtInputDiffQty.Value;
                        }

                        drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["WRK_USERID"] = txtWorker.Tag;
                        drIn["WRK_USER_NAME"] = txtWorker.Text;
                        //drIn["NOTE"] = txtNote.Text;
                        drIn["NOTE"] = SetWipNote();

                        /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            //drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                            2023.09.20 고해선 책임 요청으로 원복
                         */
                        drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                        // drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
                        // [2017-09-01 : 소형 관련 수정] 
                        if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                            drIn["CHANGE_WIPQTY_FLAG"] = "N";       // 소형 공정 N
                        else
                            drIn["CHANGE_WIPQTY_FLAG"] = "Y";       // 소형 공정 N

                        drIn["REQ_NOTE"] = txtReqNote.Text;
                        inDataTable.Rows.Add(drIn);

                        #endregion

                        #region [IN_LOSS]
                        DataTable inDataLoss = inData.Tables["IN_LOSS"];

                        DataTable dtdefect = DataTableConverter.Convert(dgDefect_FOL.ItemsSource);
                        DataRow[] drLoss = dtdefect.Select("ACTID ='LOSS_LOT'");

                        foreach (DataRow dr in drLoss)
                        {
                            DataRow drInLoss = inDataLoss.NewRow();

                            drInLoss["LOTID"] = _LOTID;
                            drInLoss["WIPSEQ"] = _WIPSEQ;
                            drInLoss["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_F"]).Equals(""))
                            {
                                drInLoss["RESNQTY"] = Convert.ToDecimal(dr["CALC_F"]);
                                drInLoss["RESNQTY2"] = Convert.ToDecimal(dr["CALC_F"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInLoss["RESNQTY"] = 0;
                                drInLoss["RESNQTY2"] = 0;
                            }
                            drInLoss["RESNCODE_CAUSE"] = "";
                            drInLoss["PROCID_CAUSE"] = "";
                            // 폴딩 불량비율 [2018-04-03]
                            drInLoss["DFCT_QTY_DDT_RATE"] = dr["DFCT_QTY_DDT_RATE"];
                            inDataLoss.Rows.Add(drInLoss);
                        }

                        #endregion

                        #region [IN_DFCT]

                        DataTable inDataDfct = inData.Tables["IN_DFCT"];

                        DataRow[] drDefect = dtdefect.Select("ACTID ='DEFECT_LOT'");
                        foreach (DataRow dr in drDefect)
                        {

                            DataRow drInDfct = inDataDfct.NewRow();

                            drInDfct["LOTID"] = _LOTID;
                            drInDfct["WIPSEQ"] = _WIPSEQ;
                            drInDfct["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_F"]).Equals(""))
                            {
                                drInDfct["RESNQTY"] = Convert.ToDecimal(dr["CALC_F"]);
                                drInDfct["RESNQTY2"] = Convert.ToDecimal(dr["CALC_F"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInDfct["RESNQTY"] = 0;
                                drInDfct["RESNQTY2"] = 0;
                            }
                            drInDfct["RESNCODE_CAUSE"] = "";
                            drInDfct["PROCID_CAUSE"] = "";
                            // 폴딩 불량비율 [2018-04-03]
                            drInDfct["DFCT_QTY_DDT_RATE"] = dr["DFCT_QTY_DDT_RATE"];

                            inDataDfct.Rows.Add(drInDfct);
                        }

                        #endregion

                        #region [IN_PRDT_REQ]

                        DataTable inDataPrdtReq = inData.Tables["IN_PRDT_REQ"];

                        DataRow[] drCharge = dtdefect.Select("ACTID ='CHARGE_PROD_LOT'");
                        foreach (DataRow dr in drCharge)
                        {
                            DataRow drInPrdtReq = inDataPrdtReq.NewRow();

                            drInPrdtReq["LOTID"] = _LOTID;
                            drInPrdtReq["WIPSEQ"] = _WIPSEQ;
                            drInPrdtReq["RESNCODE"] = dr["RESNCODE"];
                            if (!Util.NVC(dr["CALC_F"]).Equals(""))
                            {
                                drInPrdtReq["RESNQTY"] = Convert.ToDecimal(dr["CALC_F"]);
                                drInPrdtReq["RESNQTY2"] = Convert.ToDecimal(dr["CALC_F"]) * dLaneQty * dLanePtnQty;
                            }
                            else
                            {
                                drInPrdtReq["RESNQTY"] = 0;
                                drInPrdtReq["RESNQTY2"] = 0;
                            }
                            drInPrdtReq["RESNCODE_CAUSE"] = "";
                            drInPrdtReq["PROCID_CAUSE"] = "";
                            drInPrdtReq["RESNNOTE"] = dr["RESNNOTE"];
                            drInPrdtReq["COST_CNTR_ID"] = dr["COST_CNTR_ID"];
                            // 폴딩 불량비율 [2018-04-03]
                            drInPrdtReq["DFCT_QTY_DDT_RATE"] = dr["DFCT_QTY_DDT_RATE"];

                            inDataPrdtReq.Rows.Add(drInPrdtReq);
                        }
                        #endregion

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", "OUT_WIPHISTORY", inData);

                        #region Cell_Type 저장
                        string sInAType = string.Empty;
                        string sInCType = string.Empty;
                        string sOut = string.Empty;
                        string sAtype = string.Empty;
                        string sCtype = string.Empty;
                        string sFtype = string.Empty;

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        RQSTDT1.Columns.Add("SHOPID", typeof(String));
                        RQSTDT1.Columns.Add("AREAID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = _LOTID;
                        dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        RQSTDT1.Rows.Add(dr1);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE", "RQSTDT", "RSLTDT", RQSTDT1);

                        if (_StackingYN.Equals("Y"))
                        {
                            for (int i = 0; i < SearchResult.Rows.Count; i++)
                            {
                                if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                {
                                    sInAType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                                else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                {
                                    sInCType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                                else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("SC"))
                                {
                                    sOut = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < SearchResult.Rows.Count; i++)
                            {
                                if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                {
                                    sInAType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                                else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                {
                                    sInCType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                                else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("FC"))
                                {
                                    sOut = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                                }
                            }
                        }

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LOTID", typeof(String));
                        RQSTDT.Columns.Add("WIPSEQ", typeof(String));
                        RQSTDT.Columns.Add("PRODID", typeof(String));
                        RQSTDT.Columns.Add("ACTID", typeof(String));
                        RQSTDT.Columns.Add("RESNCODE", typeof(String));
                        RQSTDT.Columns.Add("REG_QTY", typeof(Decimal));
                        RQSTDT.Columns.Add("CALC_QTY", typeof(Decimal));
                        RQSTDT.Columns.Add("USERID", typeof(String));

                        for (int icnt = 2; icnt < dgDefect_FOL.GetRowCount() + 2; icnt++)
                        {
                            for (int jcnt = 0; jcnt < 3; jcnt++)
                            {
                                if (jcnt == 0)
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LOTID"] = _LOTID;
                                    dr["WIPSEQ"] = _WIPSEQ;
                                    dr["PRODID"] = sInAType;
                                    dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "ACTID"));
                                    dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "RESNCODE"));
                                    dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_A"));
                                    dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "CALC_A"));
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                }
                                else if (jcnt == 1)
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LOTID"] = _LOTID;
                                    dr["WIPSEQ"] = _WIPSEQ;
                                    dr["PRODID"] = sInCType;
                                    dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "ACTID"));
                                    dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "RESNCODE"));
                                    dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_C"));
                                    dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "CALC_C"));
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                }
                                else if (jcnt == 2)
                                {
                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LOTID"] = _LOTID;
                                    dr["WIPSEQ"] = _WIPSEQ;
                                    dr["PRODID"] = sOut;
                                    dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "ACTID"));
                                    dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "RESNCODE"));
                                    dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_F"));
                                    dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "CALC_F"));
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                }
                            }
                        }

                        new ClientProxy().ExecuteService("BR_PRD_REG_CELL_TYPE_DFCT_HIST", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        });
                        #endregion

                        #endregion
                    }
                    #endregion
                }
                else
                {
                    //string BizName = _PROCID.Equals(Process.ASSEMBLY) ? "BR_PRD_REG_MODIFY_LOT_AS" : "BR_ACT_REG_MODIFY_LOT";
                    string bizRuleName, inputSets;
                    if (string.Equals(_PROCID, Process.ASSEMBLY))
                    {
                        bizRuleName = "BR_PRD_REG_MODIFY_LOT_REINPUT_AS";
                        inputSets = "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ,IN_REINPUT";
                    }
                    // [E20231221-001759] - 홀드 상태 LOT && 홀드 수정 가능 STK의 RACK ID에 재공 존재 시 HOLD LOT 실적 수정 : 실적 수정 후 WIPHOLDHISTORY도 수정
                    else if (_WIPHOLD.Equals("Y") && IsAreaCommonCodeUse("ELEC_HOLD_PROD_RESULT_MODIFY_YN", _Rack_ID))
                    {
                        bizRuleName = "BR_PRD_REG_MODIFY_HOLD_LOT";
                        inputSets = "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ";
                    }
                    else
                    {
                        bizRuleName = "BR_ACT_REG_MODIFY_LOT";
                        inputSets = "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ";
                    }

                    DataSet inData = GetSaveDataSet();

                    #region [INDATA]

                    DataTable inDataTable = inData.Tables["INDATA"];

                    DataRow drIn = inDataTable.NewRow();
                    drIn["LOTID"] = _LOTID;
                    drIn["WIPSEQ"] = _WIPSEQ;

                    DateTime dtTime;
                    dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                    drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");
                    drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                    drIn["SHIFT"] = txtShift.Tag;
                    drIn["WOID"] = txtWorkorder.Text;

                    //2017-02-04 QTY2 계산시 기준정보 미입력시 QTY와 값이 동일하도록 1로 셋팅.
                    Decimal dLaneQty = string.IsNullOrWhiteSpace(txtLaneQty.Text) ? 1 : Convert.ToDecimal(txtLaneQty.Text);
                    //2023-07-27 전극 QTY2 계산시 LANE_PTN_QTY 제외처리
                    //2024.01.23 E20240118-001257 - ESNJ 소형 파우치 WIPQTY2_ED 수량 LANE_PTN_QTY 영향 안 받도록 수정
                    //Decimal dLanePtnQty = string.IsNullOrWhiteSpace(_LANEPTNQTY) ? 1 : Convert.ToDecimal(_LANEPTNQTY);
                    Decimal dLanePtnQty;

                    if (string.IsNullOrWhiteSpace(_LANEPTNQTY) || _AREATYPE.Equals("E") || LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_SHOP_ID == "G184")
                        dLanePtnQty = 1;
                    else dLanePtnQty = Convert.ToDecimal(_LANEPTNQTY);

                    Decimal dWipqty_ed = Convert.ToDecimal(txtOutQty.Value);

                    //if (txtLossQty.Value != 0)
                    //{
                    drIn["LANE_QTY"] = dLaneQty; // Convert.ToInt16(txtLaneQty.Text);
                                                 //}
                    drIn["PROD_VER_CODE"] = Util.GetCondition(txtProdVerCode);

                    //if (!_AREATYPE.Equals("A")) //조립이 아닐경우에만 셋팅

                    if (!_WIP_TYPE_CODE.Equals("PROD")) //조립이 아닐경우에만 셋팅
                    {
                        //drIn["WIPQTY_ED"] = txtOutQty.Value;
                        //drIn["WIPQTY2_ED"] = Convert.ToDecimal(txtOutQty.Value) * dLaneQty * dLanePtnQty;
                        drIn["WIPQTY_ED"] = dWipqty_ed;
                        drIn["WIPQTY2_ED"] = dWipqty_ed * dLaneQty * dLanePtnQty;

                        // Biz에서 처리
                        ////drIn["WIPQTY_ED_DIFF"] = dWipqty_ed - Convert.ToDecimal(_dprOutQty);
                        ////drIn["WIPQTY2_ED_DIFF"] = (dWipqty_ed - Convert.ToDecimal(_dprOutQty)) * dLaneQty * dLanePtnQty;

                    }

                    if (txtLossQty.Value != 0)
                    {
                        drIn["LOSS_QTY"] = txtLossQty.Value;
                        drIn["LOSS_QTY2"] = Convert.ToDecimal(txtLossQty.Value) * dLaneQty * dLanePtnQty;
                    }
                    if (txtDefectQty.Value != 0)
                    {
                        drIn["DFCT_QTY"] = txtDefectQty.Value;
                        drIn["DFCT_QTY2"] = Convert.ToDecimal(txtDefectQty.Value) * dLaneQty * dLanePtnQty;
                    }
                    if (txtPrdtReqQty.Value != 0)
                    {
                        drIn["PRDT_REQ_QTY"] = txtPrdtReqQty.Value;
                        drIn["PRDT_REQ_QTY2"] = Convert.ToDecimal(txtPrdtReqQty.Value) * dLaneQty * dLanePtnQty;
                    }

                    //if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && txtInputDiffQty.Value != 0) //차이수량 보여주기
                    //{
                    //drIn["INPUT_DIFF_QTY"] = txtInputDiffQty.Value;
                    //}

                    if (_PROCID.Equals(Process.PACKAGING) || _PROCID.Equals(Process.ASSEMBLY))
                    {
                        drIn["INPUT_DIFF_QTY"] = txtInputDiffQty.Value;
                    }

                    drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                    drIn["USERID"] = LoginInfo.USERID;
                    drIn["WRK_USERID"] = txtWorker.Tag;
                    drIn["WRK_USER_NAME"] = txtWorker.Text;
                    //drIn["NOTE"] = txtNote.Text;
                    drIn["NOTE"] = SetWipNote();

                    /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            //drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                            2023.09.20 고해선 책임 요청으로 원복
                         */
                    drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                    //drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
                    // [2017-09-01 : 소형 관련 수정] 
                    if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                        drIn["CHANGE_WIPQTY_FLAG"] = "N";       // 소형 공정 N
                    else
                        drIn["CHANGE_WIPQTY_FLAG"] = "Y";       // 소형 공정 N

                    drIn["REQ_NOTE"] = txtReqNote.Text;

                    if (string.Equals(_PROCID, Process.WINDING))
                    {
                        drIn["MODL_CHG_FRST_PROD_LOT_FLAG"] = (bool)chkModelChangeFirstProductLotFlag.IsChecked ? "Y" : "";
                    }

                    inDataTable.Rows.Add(drIn);

                    #endregion

                    #region [IN_LOSS]
                    DataTable inDataLoss = inData.Tables["IN_LOSS"];

                    DataTable dtdefect = DataTableConverter.Convert(dgDefect.ItemsSource);
                    DataRow[] drLoss = dtdefect.Select("ACTID ='LOSS_LOT'");

                    foreach (DataRow dr in drLoss)
                    {
                        DataRow drInLoss = inDataLoss.NewRow();

                        drInLoss["LOTID"] = _LOTID;
                        drInLoss["WIPSEQ"] = _WIPSEQ;
                        drInLoss["RESNCODE"] = dr["RESNCODE"];
                        if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                        {
                            drInLoss["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                            drInLoss["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]) * dLaneQty * dLanePtnQty;
                            drInLoss["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                        }
                        else
                        {
                            drInLoss["RESNQTY"] = 0;
                            drInLoss["RESNQTY2"] = 0;
                            drInLoss["DFCT_TAG_QTY"] = 0;
                        }
                        drInLoss["RESNCODE_CAUSE"] = "";
                        drInLoss["PROCID_CAUSE"] = "";

                        inDataLoss.Rows.Add(drInLoss);
                    }

                    #endregion

                    #region [IN_DFCT]

                    DataTable inDataDfct = inData.Tables["IN_DFCT"];

                    DataRow[] drDefect = dtdefect.Select("ACTID ='DEFECT_LOT'");
                    foreach (DataRow dr in drDefect)
                    {

                        DataRow drInDfct = inDataDfct.NewRow();

                        drInDfct["LOTID"] = _LOTID;
                        drInDfct["WIPSEQ"] = _WIPSEQ;
                        drInDfct["RESNCODE"] = dr["RESNCODE"];
                        if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                        {
                            drInDfct["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                            drInDfct["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]) * dLaneQty * dLanePtnQty;
                            drInDfct["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                        }
                        else
                        {
                            drInDfct["RESNQTY"] = 0;
                            drInDfct["RESNQTY2"] = 0;
                            drInDfct["DFCT_TAG_QTY"] = 0;
                        }
                        drInDfct["RESNCODE_CAUSE"] = "";
                        drInDfct["PROCID_CAUSE"] = "";

                        inDataDfct.Rows.Add(drInDfct);
                    }

                    #endregion

                    #region [IN_PRDT_REQ]

                    DataTable inDataPrdtReq = inData.Tables["IN_PRDT_REQ"];

                    DataRow[] drCharge = dtdefect.Select("ACTID ='CHARGE_PROD_LOT'");
                    foreach (DataRow dr in drCharge)
                    {
                        DataRow drInPrdtReq = inDataPrdtReq.NewRow();

                        drInPrdtReq["LOTID"] = _LOTID;
                        drInPrdtReq["WIPSEQ"] = _WIPSEQ;
                        drInPrdtReq["RESNCODE"] = dr["RESNCODE"];
                        if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                        {
                            drInPrdtReq["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                            drInPrdtReq["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]) * dLaneQty * dLanePtnQty;
                            drInPrdtReq["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                        }
                        else
                        {
                            drInPrdtReq["RESNQTY"] = 0;
                            drInPrdtReq["RESNQTY2"] = 0;
                            drInPrdtReq["DFCT_TAG_QTY"] = 0;
                        }
                        drInPrdtReq["RESNCODE_CAUSE"] = "";
                        drInPrdtReq["PROCID_CAUSE"] = "";
                        drInPrdtReq["RESNNOTE"] = dr["RESNNOTE"];
                        drInPrdtReq["COST_CNTR_ID"] = dr["COST_CNTR_ID"];

                        inDataPrdtReq.Rows.Add(drInPrdtReq);
                    }
                    #endregion

                    if (string.Equals(_PROCID, Process.ASSEMBLY))
                    {
                        DataTable inReasonTable = inData.Tables.Add("IN_REINPUT");
                        inReasonTable.Columns.Add("LOTID", typeof(string));
                        inReasonTable.Columns.Add("WIPSEQ", typeof(string));
                        inReasonTable.Columns.Add("ACTID", typeof(string));
                        inReasonTable.Columns.Add("RESNCODE", typeof(string));
                        inReasonTable.Columns.Add("RESNQTY", typeof(double));
                        inReasonTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                        inReasonTable.Columns.Add("PROCID_CAUSE", typeof(string));
                        inReasonTable.Columns.Add("RESNNOTE", typeof(string));
                        inReasonTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                        inReasonTable.Columns.Add("LANE_QTY", typeof(int));
                        inReasonTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                        inReasonTable.Columns.Add("COST_CNTR_ID", typeof(string));
                        inReasonTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                        inReasonTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                        for (int i = 0; i < dgDefectReInput.Rows.Count - dgDefectReInput.BottomRows.Count; i++)
                        {
                            DataRow newRow = inReasonTable.NewRow();
                            newRow["LOTID"] = _LOTID;
                            newRow["WIPSEQ"] = _WIPSEQ;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNCODE"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")));
                            newRow["RESNCODE_CAUSE"] = string.Empty;
                            newRow["PROCID_CAUSE"] = string.Empty;
                            newRow["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNNOTE"));
                            newRow["DFCT_TAG_QTY"] = 0;
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                            {
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "COST_CNTR_ID"));
                            }
                            else
                            {
                                newRow["COST_CNTR_ID"] = string.Empty;
                            }

                            newRow["A_TYPE_DFCT_QTY"] = 0;
                            newRow["C_TYPE_DFCT_QTY"] = 0;
                            inReasonTable.Rows.Add(newRow);
                        }
                    }

                    //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_MODIFY_LOT", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", "OUT_WIPHISTORY", inData);
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inputSets, "OUT_WIPHISTORY", inData);

                }
                GetLotList();
                ClearValue();

                //Util.AlertInfo("SFU1270");  //저장되었습니다.
                Util.MessageInfo("SFU1270");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [투입 반제품 탭]
        private void GetHalfProductList()
        {
            try
            {
                Util.gridClear(dgInHalfProduct);

                string bizRuleName = string.Equals(_PROCID, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
                const string materialType = "PROD";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = string.IsNullOrWhiteSpace(txtInHalfProductLot.Text.Trim()) ? null : txtInHalfProductLot.Text.Trim();
                dr["MTRLTYPE"] = materialType;
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInHalfMountPstnID.SelectedValue.GetString()) ? null : cboInHalfMountPstnID.SelectedValue.GetString();
                dr["EQPTID"] = _EQPTID;
                dr["PROD_LOTID"] = _LOTID;

                indataTable.Rows.Add(dr);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 투입 LOT 종료 취소
        private void InputHalfProductCancelAssembly()
        {
            try
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgInHalfProduct, "CHK");

                ShowLoadingIndicator();

                #region 투입취소 - 투입 LOT 종료 취소
                string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT_AS"; //투입 LOT 종료 취소 - WASHING
                //if (_isOutputLotTrayMgmt && string.Equals(_PROCID, Process.ASSEMBLY)) //[E20231215-001801] _isOutputLotTrayMgmt 기능 삭제
                DataSet inDataSet = new DataSet();

                if (string.Equals(_PROCID, Process.ASSEMBLY))
                {
                    // [E20240605-001056] - Ass'y 공정 투입반제품 투입취소 시 '투입재공생성'이 아닌 '투입종료취소' 로 변경
                    bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT"; // 투입 LOT 종료 취소 - 오창 소형 Assy용

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataTable inLotTable = inDataSet.Tables.Add("INLOT");
                    inLotTable.Columns.Add("LOTID", typeof(string));
                    inLotTable.Columns.Add("WIPQTY", typeof(int));
                    inLotTable.Columns.Add("WIPQTY2", typeof(int));
                    inLotTable.Columns.Add("INPUT_SEQNO", typeof(string));


                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);

                    newRow = inLotTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")));
                    newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")));
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_SEQNO")));

                    inLotTable.Rows.Add(newRow);
                }
                else
                {
                    //WASHING 공정 투입취소 로직 분리

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataTable inLotTable = inDataSet.Tables.Add("INLOT");
                    inLotTable.Columns.Add("LOTID", typeof(string));
                    inLotTable.Columns.Add("LOTSTAT", typeof(string));
                    inLotTable.Columns.Add("WIPQTY", typeof(int));
                    inLotTable.Columns.Add("WIPQTY2", typeof(int));
                    inLotTable.Columns.Add("INPUT_SEQNO", typeof(string));


                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);

                    newRow = inLotTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_LOTID"));
                    newRow["LOTSTAT"] = "RELEASED";
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")));
                    newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_QTY")));
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[rowIndex].DataItem, "INPUT_SEQNO"));

                    inLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);
                #endregion 투입취소 - 투입 LOT 종료 취소


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion 투입 LOT 종료 취소

        #region [E20231215-001801] (COMMONCODE) 완성 LOT TRAY 관리 
        private void GetOutputLotTrayMgmt()
        {
            try
            {
                ShowLoadingIndicator();

                _isOutputLotTrayMgmt = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "OUTPUT_LOT_TRAY_MGMT"; // 양품 완성 LOT TRAY 관리
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID; //TRAY를 수정할 SHOP ID

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                string sProcID = Util.NVC(cboProcess.SelectedValue);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (Util.NVC(dtResult.Rows[i]["ATTRIBUTE1"]).Equals(sProcID))
                        {
                            _isOutputLotTrayMgmt = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                _isOutputLotTrayMgmt = false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion [E20231215-001801] (COMMONCODE) 완성 LOT TRAY 관리

        private void InputHalfProductCancel()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInHalfProduct.Rows.Count - dgInHalfProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetHalfProductList();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        #endregion

        #region[##PKG 투입 BOX이력조회##]
        private void GetInBox(string lotid, int wipseq)
        {
            try
            {

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_BOX_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = lotid;
                newRow["WIPSEQ"] = wipseq.Equals("") ? 1 : Convert.ToDecimal(wipseq);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputBox, searchResult, FrameOperation);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Winding 전극 투입이력 조회###] [C20180209_07002]
        private void GetInputHalfWinding()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST";

                //ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = string.IsNullOrEmpty(_WIPSEQ) ? 1 : Convert.ToDecimal(_WIPSEQ);
                newRow["INPUT_LOTID"] = string.IsNullOrEmpty(txtInputHalfWindingLotID.Text) ? null : txtInputHalfWindingLotID.Text.Trim();
                newRow["EQPT_MOUNT_PSTN_ID"] = cboInputHalfWindingMountPstsID.SelectedValue.ToString().Equals("") ? null : cboInputHalfWindingMountPstsID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputHist.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputHalfWinding, searchResult, FrameOperation);

                        if (dgInputHalfWinding.CurrentCell != null)
                            dgInputHalfWinding.CurrentCell = dgInputHalfWinding.GetCell(dgInputHalfWinding.CurrentCell.Row.Index, dgInputHalfWinding.Columns.Count - 1);
                        else if (dgInputHalfWinding.Rows.Count > 0 && dgInputHalfWinding.GetCell(dgInputHalfWinding.Rows.Count, dgInputHalfWinding.Columns.Count - 1) != null)
                            dgInputHalfWinding.CurrentCell = dgInputHalfWinding.GetCell(dgInputHalfWinding.Rows.Count, dgInputHalfWinding.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        // HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                // HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Winding 전극 Pancake 대상 조회###] [C20180209_07002]
        public void GetWaitPancake()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_WN_BY_LV3_CODE";
                string sInMtrlClssCode = GetInputMtrlClssCode();

                DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _EQSGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["PROCID"] = _PROCID;
                newRow["WOID"] = _WORKORDER;
                newRow["IN_LOTID"] = txtWaitPancakeLot.Text;
                newRow["INPUT_MTRL_CLSS_CODE"] = sInMtrlClssCode;
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {


                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (CommonVerify.HasTableRow(searchResult) && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _dtMinValid);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }
                );
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Winding 투입 취소 처리 ###] [C20180209_07002]
        private void InputHalfWindingCancel()
        {

            try
            {
                //const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                //string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputHalfWinding.Rows.Count - dgInputHalfWinding.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputHalfWinding, "CHK", i)) continue;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHalfWinding.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHalfWinding.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHalfWinding.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHalfWinding.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHalfWinding.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetInputHalfWinding();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [### Winding 전극 Class Code 조회 ###] [C20180209_07002]
        private string GetInputMtrlClssCode()
        {
            try
            {
                if (cboPancakeMountPstnID?.SelectedValue == null)
                {
                    return string.Empty;
                }

                const string bizRuleName = "DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID";
                string sInputMtrlClssCode = string.Empty;

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _EQPTID;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                    txtWaitPancakeInputClssCode.Text = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }
        #endregion

        #region [### Winding 전극 입력값 확인 ###] [C20180209_07002]
        private bool ValidationWaitPanCakeInput()
        {

            if (_LOTID == null)
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return false;
            }

            if (cboPancakeMountPstnID.SelectedValue == null || cboPancakeMountPstnID.SelectedValue.Equals("") || cboPancakeMountPstnID.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitPancake, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // [20180323] 실적마감 체크 기능 추가
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            /*
            int rowIndex = _util.GetDataGridRowIndex(dgMaterialInput, "EQPT_MOUNT_PSTN_ID", cboPancakeMountPstnID.SelectedValue.ToString());
            if (rowIndex >= 0)
            {
                string classCode = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "PRDT_CLSS_CODE"));

                if (classCode != "C")
                {
                    //string sInPancake = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "INPUT_LOTID"));
                    string sInState = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "WIPSTAT"));
                    string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                    if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
                    {
                        //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        return false;
                    }
                }
            }
            */
            return true;
        }
        #endregion

        #region [### Winding 전극 입력값 확인 ###] [C20180209_07002]
        private bool ValidationInputHalfWindingCancel(C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // [20180323] 실적마감 체크 기능 추가
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }
        #endregion

        #region [### Winding 전극 입력 처리  ###] [C20180209_07002]
        private void InputWaitPancake()
        {
            try
            {
                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                const string bizRuleName = "BR_PRD_REG_START_END_INPUT_IN_LOT_WN_SM";

                DataSet indataSet = BR_PRD_REG_START_END_INPUT_IN_LOT_WN_SM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = LoginInfo.USERID;

                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = _WIPSEQ;
                newRow["PROD_CALDATE"] = dtpCaldate.SelectedDateTime; //  dtTime.ToString("yyyy-MM-dd"); ;


                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));
                    newRow["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "WIPQTY")));

                    inInputTable.Rows.Add(newRow);
                    break;
                }

                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetProductLot();
                        GetWaitPancake();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataSet BR_PRD_REG_START_END_INPUT_IN_LOT_WN_SM()
        {
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_WIPSEQ", typeof(Decimal));
            inDataTable.Columns.Add("PROD_CALDATE", typeof(DateTime));

            //inDataTable.Columns.Add("LANGID", typeof(string));

            DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
            inInputTable.Columns.Add("ACTQTY", typeof(Decimal));

            return indataSet;
        }

        #endregion

        #region [### 투입이력 조회 ###]
        private void GetInputHistory()
        {
            try
            {
                if (!cTabInputHalf.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                // 폴란드 1,2 동 라미공정 투입 3LOSS 적용에 따른 수정
                string sBizName = string.Empty;

                if (LoginInfo.CFG_SHOP_ID.Equals("G481") && _PROCID.Equals(Process.LAMINATION))
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END_L";
                else
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = _WIPSEQ;
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region # Roll Map
        private bool IsEquipmentAttr(string sEqptID)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(sEqptID).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }
        #endregion
        #endregion

        #region [Validation]
        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }
            if (_LOTID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
                return false;
            }

            return true;

        }

        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #region [요청자]
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;

                txtReqNote.Focus();
            }
        }
        #endregion

        #region 투입수, 양품수 변경시 투입, 양품수 계산
        private void SetChangeQty(bool bin)
        {
            double dIn = Convert.ToDouble(txtInputQty.Value);
            double dOut = Convert.ToDouble(txtOutQty.Value);
            double dDefect = Convert.ToDouble(txtDefectQty.Value);
            double dLoss = Convert.ToDouble(txtLossQty.Value);
            double dReq = Convert.ToDouble(txtPrdtReqQty.Value);
            double dLen = Convert.ToDouble(txtLengthExceedQty.Value);

            // 생산량 산출
            if (bin)
                dOut = dIn + dLen - (dDefect + dLoss + dReq);
            else
                dIn = dOut + dDefect + dLoss + dReq - dLen;

            txtInputQty.Value = dIn;
            txtOutQty.Value = dOut;

            _dInQty = dIn;
            _dOutQty = dOut;

            // Winding 설비 완성 차이수량
            if (_PROCID.Equals(Process.WINDING))
            {
                double inputQtyByType = GetInputQtyByApplyTypeCode().GetDouble();
                double adddefectqty = (_dEqptOrgQty + inputQtyByType) - (dOut + dDefect + dLoss + dReq);

                if (Math.Abs(adddefectqty) > 0)
                {
                    txtAssyResultQty.Value = adddefectqty;
                    txtAssyResultQty.FontWeight = FontWeights.Bold;
                    txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    txtAssyResultQty.Value = 0;
                    txtAssyResultQty.FontWeight = FontWeights.Normal;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                    if (convertFromString != null)
                        txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                }

            }

        }
        #endregion

        #region [Clear]
        private void ClearValue()
        {
            txtInputQty.ValueChanged -= txtInputQty_ValueChanged;
            txtOutQty.ValueChanged -= txtOutQty_ValueChanged;

            txtSelectLot.Text = "";
            txtWorkorder.Text = "";
            txtWorkorderDetail.Text = "";
            txtLotStatus.Text = "";
            txtShift.Text = "";
            txtStartTime.Text = "";
            txtWorker.Text = "";
            txtEndTime.Text = "";
            txtInputQty.Value = 0;
            txtOutQty.Value = 0;
            txtDefectQty.Value = 0;
            txtLossQty.Value = 0;
            txtPrdtReqQty.Value = 0;
            txtInputDiffQty.Value = 0;
            txtProdVerCode.Text = "";
            txtLaneQty.Text = "";
            txtNote.Text = "";

            tbCellManagement.Text = string.Empty;

            ////txtReqNote.Text = "";
            _dtLengthExceeed.Clear();
            txtDiffQty.Value = 0;
            txtAssyResultQty.Value = 0;

            _AREAID = "";
            _PROCID = "";
            _PRODID = "";
            _EQSGID = "";
            _EQPTID = "";
            _LOTID = "";
            _WIPSEQ = "";
            _VERCODE = "";
            _LANEPTNQTY = "";
            _WIPSTAT = "";
            _WIP_TYPE_CODE = "";
            _UNITCODE = "";
            _COATING_GUBUN = "";
            _COATING_SIDE_TYPE_CODE = "";
            _LANE_QTY = "";
            _dInQty = 0;
            _dOutQty = 0;
            _dprOutQty = 0;
            _dEqptQty = 0;
            _dEqptOrgQty = 0;
            _WORKORDER = "";
            _ERP_CLOSE = "";
            _WIP_NOTE = "";
            _StackingYN = "";
            _IS_SMALL_TYPE = "";
            _CELL_MNGT_TYPE_CODE = "";
            _EQPTNAME = "";
            _LANE = "";
            _CSTID = "";

            //C20210323-000097
            _OUT_QTY = 0;
            _DEFECT_QTY = 0;
            _LOSS_QTY = 0;
            _PRDT_REQ_QTY = 0;

            Util.gridClear(dgDefect);
            Util.gridClear(dgDefect_FOL);
            Util.gridClear(dgQualityInfo);
            Util.gridClear(dgColor);
            Util.gridClear(dgSubLot);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgInHalfProduct);
            Util.gridClear(dgDefect_NJ);
            Util.gridClear(dgWaitHalfProduct);

            Util.gridClear(dgInputHalfWinding); //[C20180209_07002] 추가
            Util.gridClear(dgWaitPancake); //[C20180209_07002] 추가
            Util.gridClear(dgInputBox); ///2019.04.10 강호운 수정
            Util.gridClear(dgEqptCond);

            isChangeDefect = false;

            txtInputQty.ValueChanged += txtInputQty_ValueChanged;
            txtOutQty.ValueChanged += txtOutQty_ValueChanged;

            ////txtUserName.Text = "";
            ////txtUserName.Tag = "";

            btnSave.IsEnabled = true;

            rdoLot.IsChecked = true;
            cboCLCTSEQ.ItemsSource = null;
            cboCLCTSEQ.Text = null;

            tbType0.Visibility = Visibility.Collapsed;
            txtType0.Visibility = Visibility.Collapsed;
            tbType0.Text = "";
            txtType0.Text = "";
            tbType1.Visibility = Visibility.Collapsed;
            txtType1.Visibility = Visibility.Collapsed;
            tbType1.Text = "";
            txtType1.Text = "";
            tbType2.Visibility = Visibility.Collapsed;
            txtType2.Visibility = Visibility.Collapsed;
            tbType2.Text = "";
            txtType2.Text = "";
            tbType3.Visibility = Visibility.Collapsed;
            txtType3.Visibility = Visibility.Collapsed;
            tbType3.Text = "";
            txtType3.Text = "";
            tbType4.Visibility = Visibility.Collapsed;
            txtType4.Visibility = Visibility.Collapsed;
            tbType4.Text = "";
            txtType4.Text = "";
            tbType5.Visibility = Visibility.Collapsed;
            txtType5.Visibility = Visibility.Collapsed;
            tbType5.Text = "";
            txtType5.Text = "";
            tbType6.Visibility = Visibility.Collapsed;
            txtType6.Visibility = Visibility.Collapsed;
            tbType6.Text = "";
            txtType6.Text = "";
            tbType7.Visibility = Visibility.Collapsed;
            txtType7.Visibility = Visibility.Collapsed;
            tbType7.Text = "";
            txtType7.Text = "";
            tbType8.Visibility = Visibility.Collapsed;
            txtType8.Visibility = Visibility.Collapsed;
            tbType8.Text = "";
            txtType8.Text = "";
            chkModelChangeFirstProductLotFlag.IsChecked = false;
            btnRollMap.Visibility = Visibility.Collapsed;
            btnRollMapUpdate.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 탭, Grid Column Visibility
        private void AreaCheck(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                TabReInput.Visibility = Visibility.Collapsed;
                tbInputDiffQty.Visibility = Visibility.Collapsed;
                txtInputDiffQty.Visibility = Visibility.Collapsed;
                txtInputDiffQty.IsEnabled = false;
                txtLengthExceedQty.Visibility = Visibility.Collapsed;

                // [2017-09-01 : 소형 관련 수정]
                tbDiffQty.Visibility = Visibility.Collapsed;
                txtDiffQty.Visibility = Visibility.Collapsed;

                tbLaneQty.Visibility = Visibility.Collapsed;
                tbProdVerCode.Visibility = Visibility.Collapsed;
                txtLaneQty.Visibility = Visibility.Collapsed;
                txtProdVerCode.Visibility = Visibility.Collapsed;
                btnVersion.Visibility = Visibility.Collapsed;
                txtAssyResultQty.Visibility = Visibility.Collapsed;
                tbAssyResultQty.Visibility = Visibility.Collapsed;
                cTabInputHalfWinding.Visibility = Visibility.Collapsed; // [C20180209_07002] 
                cTabPancake.Visibility = Visibility.Collapsed; // [C20180209_07002] 

                btnLotDeleted.Visibility = Visibility.Collapsed; // [C20180209_07002] 

                tbModelChangeFirstProductLotFlag.Visibility = Visibility.Collapsed;
                chkModelChangeFirstProductLotFlag.Visibility = Visibility.Collapsed;

                dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Collapsed;

                dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;

                // E20230901-001504 대차ID 적용
                if (LoginInfo.CFG_SHOP_ID.Equals("A010") && sProcID.Equals(Process.WINDING) && !"Y".Equals(_IS_SMALL_TYPE) && SetAssyCSTIDView())
                {
                    btnCSTIDSave.Visibility = Visibility.Visible;
                    tbCSTID.Visibility = Visibility.Visible;
                    txtCSTID.Visibility = Visibility.Visible;
                }
                else
                {
                    btnCSTIDSave.Visibility = Visibility.Collapsed;
                    tbCSTID.Visibility = Visibility.Collapsed;
                    txtCSTID.Visibility = Visibility.Collapsed;
                }

                // [2023-02-21 : 소형 관련 수정] C20220921-000333
                btnCellWindingConfirmCancel.Visibility = Visibility.Collapsed;
                btnDischarge.Visibility = Visibility.Collapsed;
                btnQtySave.Visibility = Visibility.Collapsed;


                // 활성화 후공정
                dgLotList.Columns["DIFF_QTY"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["REQ_QTY_WS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["BOXQTY_IN"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["BOXQTY"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["WINDING_RUNCARD_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LOTID_AS"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["MODL_CHG_FRST_PROD_LOT_FLAG"].Visibility = Visibility.Collapsed;

                dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["INPUT_QTY_APPLY_TYPE_CODE_NAME"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;

                dgSubLot.Columns["WIPQTY"].IsReadOnly = false;

                //tbAType.Visibility = Visibility.Collapsed;
                //tbCType.Visibility = Visibility.Collapsed;

                dgSubLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["WIPSTAT_NAME"].Visibility = Visibility.Collapsed;

                dgQualityInfo.Columns["CLCT_ITVL"].Visibility = Visibility.Collapsed;

                grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                cTabHalf.Visibility = Visibility.Collapsed;
                cTabInputMaterial.Visibility = Visibility.Collapsed;
                cTabColor.Visibility = Visibility.Collapsed;
                cTabWaitHalfProduct.Visibility = Visibility.Collapsed;

                rdoLot.Visibility = Visibility.Collapsed;
                rdoTime.Visibility = Visibility.Collapsed;
                tbCLCTSEQ.Visibility = Visibility.Collapsed;
                cboCLCTSEQ.Visibility = Visibility.Collapsed;
                btnQualitySelect.Visibility = Visibility.Collapsed;

                cTabInBox.Visibility = Visibility.Collapsed;

                cTabInputHalf.Visibility = Visibility.Collapsed;

                //if (sProcID.Equals(Process.PACKAGING))
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");
                //else
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");

                dgLotList.Columns["EQPT_END_QTY"].Header = ObjectDic.Instance.GetObjectName("설비양품량");
                dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");

                //완성 LOT WASHING 용
                StackTrayCreate.Visibility = Visibility.Collapsed;
                //Tray 생성 PKG 용
                btnOutCreate.Visibility = Visibility.Collapsed;

                btnOutDel.Visibility = Visibility.Collapsed;
                btnOutConfirm.Visibility = Visibility.Collapsed;
                btnOutCell.Visibility = Visibility.Collapsed;
                btnOutSave.Visibility = Visibility.Collapsed;
                //btnOutConfirmCancel.Visibility = Visibility.Collapsed;

                btnTrayToFCS.Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.PACKAGING))
                {
                    dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                    tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");

                    cTabInBox.Visibility = Visibility.Visible;
                    /*2019-05-07 최상민                     
                     * [CSR ID:3984206] [업무요청] GEMS 7호라인 PKG 확정수량 수정요청의 건 | [요청번호]C20190429_84206 | [서비스번호]3984206 
                     *  -> 패키징 공정 tray 삭제 처리 기능 추가 & 차이수량 관리 로직 추가
                    */
                    btnOutDel.Visibility = Visibility.Visible; //tray 삭제
                    btnOutCreate.Visibility = Visibility.Visible; //tray 추가
                    btnOutCell.Visibility = Visibility.Visible; // cell 관리
                    btnOutConfirm.Visibility = Visibility.Visible; //Tray 확정
                }
                else
                {
                    if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                         ||
                        (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                    {
                        dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량(투입)");
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량(투입)");
                    }
                    else
                    {
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");
                    }
                }

                if (!_AREATYPE.Equals("A") || sProcID.Equals(Process.WINDING))
                {
                    dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Visible;
                }

                if (_AREATYPE.Equals("A") && !sProcID.Equals(Process.NOTCHING)) //반제품 탭 보여주기
                {
                    cTabHalf.Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구");

                    txtInputQty.IsEnabled = false;
                    txtOutQty.IsEnabled = false;
                }
                else
                {
                    dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");
                    dgLotList.Columns["WIPQTY2_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Lane)");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Header = ObjectDic.Instance.GetObjectName("불량량(Lane)");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Lane)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Header = ObjectDic.Instance.GetObjectName("물품청구(Lane)");

                    txtInputQty.IsEnabled = true;
                    txtOutQty.IsEnabled = true;
                }

                tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;

                //[Merge LotTrace (2018.04.24)
                if (sProcID.Equals(Process.PRE_MIXING) || sProcID.Equals(Process.MIXING))
                {
                    cTabInputMaterial.Visibility = Visibility;
                    btnAddMaterial.Visibility = Visibility.Collapsed;
                    btnRemoveMaterial.Visibility = Visibility.Collapsed;
                    btnEqptMaterial.Visibility = Visibility.Collapsed;
                    btnSaveMaterial.Visibility = Visibility.Collapsed;
                    btnDeleteMaterial.Visibility = Visibility.Collapsed;
                    dgMaterial.Visibility = Visibility.Collapsed;
                    btnInputMaterial.Visibility = Visibility.Visible;
                    dgMaterialList.Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.COATING)) // E2000
                {
                    tbLaneQty.Visibility = Visibility.Visible;
                    tbProdVerCode.Visibility = Visibility.Visible;
                    txtLaneQty.Visibility = Visibility.Visible;
                    txtProdVerCode.Visibility = Visibility.Visible;
                    btnVersion.Visibility = Visibility.Visible;
                    btnInputMaterial.Visibility = Visibility.Collapsed;
                    btnEqptMaterial.Visibility = Visibility.Collapsed;
                    dgMaterialList.Visibility = Visibility.Collapsed;
                    btnDeleteMaterial.Visibility = Visibility.Visible;
                    btnSaveMaterial.Visibility = Visibility.Visible;
                    btnRemoveMaterial.Visibility = Visibility.Visible;
                    btnAddMaterial.Visibility = Visibility.Visible;
                    dgMaterial.Visibility = Visibility.Visible;
                }

                // [C20180209_07002] 
                if (sProcID.Equals(Process.WINDING))
                {
                    cTabInputHalfWinding.Visibility = Visibility.Visible;
                    cTabPancake.Visibility = Visibility.Visible;
                    btnLotDeleted.Visibility = Visibility.Visible; // [C20180209_07002] 
                }

                if (sProcID.Equals(Process.ROLL_PRESSING) || sProcID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible;
                }
                if (sProcID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;

                    if (IsElectrodeGradeInfo())
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Visible;
                        SetGridGrdCombo(dgElectrodeGradeInfo.Columns["GRD_JUDG_CODE"]);
                    }
                    else
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                    }
                }
                if (sProcID.Equals(Process.ROLL_PRESSING))
                {
                    //cTabColor.Visibility = Visibility.Visible;
                    cTabColor.Visibility = Visibility.Collapsed;

                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                }
                if (sProcID.Equals(Process.COATING) || sProcID.Equals(Process.SRS_COATING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;

                    // SRS코터 투입자재란에 실제수량이 아닌 1BATCH수량이라 입력값 보여주지 않음 [2017-05-23]
                    if (string.Equals(sProcID, Process.SRS_COATING))
                        dgMaterial.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                    else
                        dgMaterial.Columns["INPUT_QTY"].Visibility = Visibility.Visible;

                }

                if (sProcID.Equals(Process.NOTCHING))
                {
                    dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Visible;
                }
                if (sProcID.Equals(Process.LAMINATION))
                {
                    #region
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("카세트ID");
                    #endregion
                }
                if (sProcID.Equals(Process.PACKAGING)) //차이수량 보여주기
                {
                    dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Visible;

                    tbInputDiffQty.Text = ObjectDic.Instance.GetObjectName("차이수량");
                    tbInputDiffQty.Visibility = Visibility.Visible;
                    txtInputDiffQty.Visibility = Visibility.Visible;

                    txtLengthExceedQty.Visibility = Visibility.Collapsed;

                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Visible;

                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Collapsed;

                    dgSubLot.Columns["WIPQTY"].IsReadOnly = true;
                    #region
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Tray ID");
                    #endregion
                }
                else
                {
                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Visible;
                }

                // 길이초과 칼럼은 전극에서 MIXING제외 조립에선 NOTCHING만
                if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                     ||
                    (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                {
                    tbInputDiffQty.Text = ObjectDic.Instance.GetObjectName("길이초과");
                    tbInputDiffQty.Visibility = Visibility.Visible;
                    txtLengthExceedQty.Visibility = Visibility.Visible;
                    txtInputDiffQty.Visibility = Visibility.Collapsed;

                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.STACKING_FOLDING))
                {
                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    {
                        cTabDefect_NJ.Visibility = Visibility.Visible;
                        cTabDefect_FOL.Visibility = Visibility.Collapsed;
                        cTabDefect.Visibility = Visibility.Collapsed;
                        cTabDefect_NJ.IsSelected = true;
                        cboChange.Visibility = Visibility.Visible;
                        tbWorktype.Visibility = Visibility.Visible;
                        btnChange.Visibility = Visibility.Visible;
                        dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        cTabDefect_NJ.Visibility = Visibility.Collapsed;
                        cTabDefect_FOL.Visibility = Visibility.Visible;
                        cTabDefect.Visibility = Visibility.Collapsed;
                        cTabDefect_FOL.IsSelected = true;
                        cboChange.Visibility = Visibility.Visible;
                        tbWorktype.Visibility = Visibility.Visible;
                        btnChange.Visibility = Visibility.Visible;
                        dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;
                        #region
                        dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                        dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("카세트ID");
                        #endregion
                    }
                }
                else if (sProcID.Equals(Process.STP))
                {
                    cTabDefect_NJ.Visibility = Visibility.Visible;
                    cTabDefect_FOL.Visibility = Visibility.Collapsed;
                    cTabDefect.Visibility = Visibility.Collapsed;
                    cTabDefect_NJ.IsSelected = true;
                    cboChange.Visibility = Visibility.Visible;
                    tbWorktype.Visibility = Visibility.Visible;
                    btnChange.Visibility = Visibility.Visible;
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;
                }
                else
                {
                    cTabDefect_NJ.Visibility = Visibility.Collapsed;
                    cTabDefect.Visibility = Visibility.Visible;
                    cTabDefect_FOL.Visibility = Visibility.Collapsed;
                    cTabDefect.IsSelected = true;
                    cboChange.Visibility = Visibility.Collapsed;
                    tbWorktype.Visibility = Visibility.Collapsed;
                    btnChange.Visibility = Visibility.Collapsed;
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Collapsed;
                }

                // [2017-09-01 : 소형 관련 수정] 원형, 초소형 투입 반제품 탭 
                if (sProcID.Equals(Process.WINDING) || sProcID.Equals(Process.ASSEMBLY) || sProcID.Equals(Process.WASHING))
                {
                    dgLotList.Columns["EQPT_END_QTY"].Header = ObjectDic.Instance.GetObjectName("설비투입수량");

                    cTabEqptDefect.Visibility = Visibility.Collapsed;

                    rdoLot.Visibility = Visibility.Visible;
                    rdoTime.Visibility = Visibility.Visible;
                    btnQualitySelect.Visibility = Visibility.Visible;

                    if (sProcID.Equals(Process.WINDING))
                    {
                        txtAssyResultQty.Visibility = Visibility.Visible;
                        tbAssyResultQty.Visibility = Visibility.Visible;
                        tbModelChangeFirstProductLotFlag.Visibility = Visibility.Visible;
                        chkModelChangeFirstProductLotFlag.Visibility = Visibility.Visible;

                        dgLotList.Columns["WINDING_RUNCARD_ID"].Visibility = Visibility.Visible;
                        dgLotList.Columns["LOTID_AS"].Visibility = Visibility.Visible;
                        dgLotList.Columns["MODL_CHG_FRST_PROD_LOT_FLAG"].Visibility = Visibility.Visible;

                        dgDefect.Columns["INPUT_QTY_APPLY_TYPE_CODE_NAME"].Visibility = Visibility.Visible;

                        cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtInputQty.IsEnabled = false;
                        txtOutQty.IsEnabled = false;

                        cTabInputHalfProduct.Visibility = Visibility.Visible;
                        btnInHalfProductInPutCancel.Visibility = Visibility.Collapsed; ;
                        dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;

                        if (sProcID.Equals(Process.ASSEMBLY))
                        {
                            cTabWaitHalfProduct.Visibility = Visibility.Visible;

                            cTabHalf.Visibility = Visibility.Collapsed;
                            dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Visible;

                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");

                            tbInputDiffQty.Text = ObjectDic.Instance.GetObjectName("재투입");
                            tbInputDiffQty.Visibility = Visibility.Visible;
                            txtInputDiffQty.Visibility = Visibility.Visible;

                            tbDiffQty.Visibility = Visibility.Visible;
                            txtDiffQty.Visibility = Visibility.Visible;

                            txtInputDiffQty.IsEnabled = true;

                            dgLotList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;

                            btnInHalfProductInPutCancel.Visibility = Visibility.Visible; ;

                            //C20181022_22742 반제품 대기 추가
                            dgWaitHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                            dgWaitHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Visible;
                            dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Visible;

                        }
                        else
                        {   //WASHING
                            cTabWaitHalfProduct.Visibility = Visibility.Visible;

                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");

                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;

                            StackTrayCreate.Visibility = Visibility.Visible;
                            btnOutDel.Visibility = Visibility.Visible;
                            btnOutConfirm.Visibility = Visibility.Visible;
                            btnOutCell.Visibility = Visibility.Visible;
                            btnOutSave.Visibility = Visibility.Visible;
                            //btnOutConfirmCancel.Visibility = Visibility.Visible;
                            dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Visible;
                            btnTrayToFCS.Visibility = Visibility.Visible;
                            btnInHalfProductInPutCancel.Visibility = Visibility.Visible; ;

                            //C20181022_22742 반제품 대기 추가
                            dgWaitHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                            dgWaitHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;
                            dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;

                        }

                    }
                }
                else
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;
                    cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.NOTCHING) || sProcID.Equals(Process.LAMINATION) || sProcID.Equals(Process.STACKING_FOLDING) || sProcID.Equals(Process.PACKAGING))
                {
                    btnQualitySelect.Visibility = Visibility.Visible;
                }


                #region 폴란드 1,2 동 라미공정 투입 3LOSS 적용에 따른 추가
                if (LoginInfo.CFG_SHOP_ID.Equals("G481") && sProcID.Equals(Process.LAMINATION))
                {
                    cTabInputHalf.Visibility = Visibility.Visible;

                    //if (dgInputHist.Visibility == Visibility.Visible)
                    //{
                    //    if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                    //        dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                    //    if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                    //        dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                    //    if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                    //        dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                    //    if (dgInputHist.Columns.Contains("RMN_QTY"))
                    //        dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                    //}
                }
                #endregion

                #region 전극 재와인딩 공정 Tag컬럼 Visibility
                if (sProcID.Equals(Process.SLIT_REWINDING) && IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", Process.SLIT_REWINDING))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                }
                #endregion
                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(sProcID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ProcCheck(string sProcID, string sEqptID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                if (sProcID.Equals(Process.STACKING_FOLDING))
                {
                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    {
                        GetMBOMInfo_NJ();
                    }
                    else
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("EQPTID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["EQPTID"] = sEqptID;
                        RQSTDT.Rows.Add(dr);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQGRID", "RQSTDT", "RSLTDT", RQSTDT);

                        if (SearchResult.Rows[0]["EQGRID"].ToString().Equals("FOL"))
                            _StackingYN = "N";
                        else
                            _StackingYN = "Y";

                        grdMBomTypeCnt.Visibility = Visibility.Visible;

                        if (_StackingYN.Equals("Y"))
                        {
                            tbAType.Text = "HALFTYPE";
                            tbCType.Text = "MONOTYPE";

                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("REG_A"))
                                dgDefect_FOL.Columns["REG_A"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("HALFTYPE") }; //C20210623-000317 다국어 처리
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("REG_C"))
                                dgDefect_FOL.Columns["REG_C"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("MONOTYPE") };
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("CALC_A"))
                                dgDefect_FOL.Columns["CALC_A"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("HALFTYPE") };
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("CALC_C"))
                                dgDefect_FOL.Columns["CALC_C"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("MONOTYPE") };
                        }
                        else
                        {
                            tbAType.Text = "ATYPE";
                            tbCType.Text = "CTYPE";

                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("REG_A"))
                                dgDefect_FOL.Columns["REG_A"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("ATYPE") };
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("REG_C"))
                                dgDefect_FOL.Columns["REG_C"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("CTYPE") };
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("CALC_A"))
                                dgDefect_FOL.Columns["CALC_A"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("ATYPE") };
                            if (dgDefect_FOL != null && dgDefect_FOL.Columns.Contains("CALC_C"))
                                dgDefect_FOL.Columns["CALC_C"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("CTYPE") };
                        }

                        GetMBOMInfo();
                    }
                }
                else if (sProcID.Equals(Process.STP))
                {
                    GetMBOMInfo_NJ();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공정별 숫자 Format
        private void SetProcessNumFormat(string sProcid)
        {
            // 숫자값 Format 
            string sFormat = string.Empty;

            if (sProcid.Equals(Process.MIXING))
            {
                // MIXING
                sFormat = "###,##0.###";
            }
            else if (_AREATYPE.Equals("A"))
            {
                // 조립
                sFormat = "###,##0";
            }
            else
            {
                // 전극
                if (_UNITCODE.Equals("EA"))
                    sFormat = "###,##0.#";
                else
                    sFormat = "###,##0.##";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["A_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["C_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["RESNQTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgMaterial.Columns["INPUT_QTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgEqpFaulty.Columns["DFCT_QTY"])).Format = sFormat;

            txtInputQty.Format = sFormat;
            txtOutQty.Format = sFormat;
            txtDefectQty.Format = sFormat;
            txtLossQty.Format = sFormat;
            txtPrdtReqQty.Format = sFormat;
            txtInputDiffQty.Format = sFormat;
            txtLengthExceedQty.Format = sFormat;

        }
        #endregion

        #region [조회 데이터 바인딩]
        private void SetValue(object oContext)
        {
            txtInputQty.ValueChanged -= txtInputQty_ValueChanged;
            txtOutQty.ValueChanged -= txtOutQty_ValueChanged;
            txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

            txtSelectLot.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID")); ;
            txtWorkorder.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            txtWorkorderDetail.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WO_DETL_ID"));
            txtLotStatus.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSNAME"));

            dtpCaldate.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE")));
            dtCaldate = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE")));

            txtShift.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            txtShift.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            txtStartTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "STARTDTTM"));
            txtWorker.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            txtWorker.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USERID"));
            txtEndTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "ENDDTTM"));
            txtInputQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY").ToString()));
            txtOutQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED").ToString()));
            txtDefectQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_DFCT_QTY").ToString()));
            txtLossQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_LOSS_QTY").ToString()));
            txtPrdtReqQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_PRDT_REQ_QTY").ToString()));
            txtInputDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_DIFF_QTY").ToString()));

            _EQSGID_YN = false;

            if (Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.ASSEMBLY) || Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.WASHING))
            {
                txtLengthExceedQty.Value = 0;
                txtLaneQty.Text = string.Empty;
                txtDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DIFF_QTY").GetString()));
                _COATING_GUBUN = string.Empty;
                _COATING_SIDE_TYPE_CODE = string.Empty;

                if (string.Equals(DataTableConverter.GetValue(oContext, "PROCID").GetString(), Process.ASSEMBLY))
                {
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(oContext, "PROCID").GetString()))
                    {
                        if (GetReInputReasonApplyFlag(DataTableConverter.GetValue(oContext, "EQPTID").GetString()) == "Y")
                        {
                            TabReInput.Visibility = Visibility.Visible;
                            txtInputDiffQty.IsEnabled = false;
                        }
                        else
                        {
                            txtInputDiffQty.IsEnabled = true;
                            TabReInput.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        txtInputDiffQty.IsEnabled = true;
                        TabReInput.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                txtLengthExceedQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "LENGTH_EXCEED")));
                txtLaneQty.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_QTY"));
                txtDiffQty.Value = 0;
                _COATING_GUBUN = Util.NVC(DataTableConverter.GetValue(oContext, "COATING_GUBUN"));
                _COATING_SIDE_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "COATING_SIDE_TYPE_CODE"));
            }

            // E20230901-001504 대차ID 적용
            txtCSTID.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WINDING_RUNCARD_ID"));

            txtProdVerCode.Text = Util.NVC(DataTableConverter.GetValue(oContext, "PROD_VER_CODE"));
            //txtNote.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));            

            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _PRODID = Util.NVC(DataTableConverter.GetValue(oContext, "PRODID"));
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _VERCODE = Util.NVC(DataTableConverter.GetValue(oContext, "PROD_VER_CODE"));
            _LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
            _WIPSTAT = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSTAT"));
            _WIP_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_TYPE_CODE"));
            _UNITCODE = Util.NVC(DataTableConverter.GetValue(oContext, "UNIT_CODE"));
            ////_COATING_GUBUN = Util.NVC(DataTableConverter.GetValue(oContext, "COATING_GUBUN"));
            ////_COATING_SIDE_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "COATING_SIDE_TYPE_CODE"));
            _LANE_QTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            _WORKORDER = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            _ERP_CLOSE = Util.NVC(DataTableConverter.GetValue(oContext, "ERP_CLOSE"));
            _WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));
            _LANE = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            _Rack_ID = GetRackID(txtSelectLot.Text);
            _WIPHOLD = Util.NVC(DataTableConverter.GetValue(oContext, "WIPHOLD"));

            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(oContext, "IS_SMALL_TYPE").GetString()))
            {
                _IS_SMALL_TYPE = Util.NVC(DataTableConverter.GetValue(oContext, "IS_SMALL_TYPE"));
            }
            else
            {
                _IS_SMALL_TYPE = "";
            }
            _CELL_MNGT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "CELL_MNGT_TYPE_CODE"));

            DisplayCellManagementType(dgLotList);

            _EQPTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTNAME"));

            txtNote.Text = GetWipNote();

            _dInQty = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY").ToString()));
            _dOutQty = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED").ToString()));
            _dprOutQty = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED").ToString()));

            if (_PROCID.Equals(Process.WINDING))
            {
                if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(oContext, "EQPT_END_QTY")), out _dEqptQty))
                {
                    _dEqptQty = 0;
                }
                if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(oContext, "EQPT_END_QTY_ORG")), out _dEqptOrgQty))
                {
                    _dEqptOrgQty = 0;
                }

                if (DataTableConverter.GetValue(oContext, "MODL_CHG_FRST_PROD_LOT_FLAG").GetString() == "Y")
                {
                    chkModelChangeFirstProductLotFlag.IsChecked = true;
                }
                else
                {
                    chkModelChangeFirstProductLotFlag.IsChecked = false;
                }
            }

            if (_WIPSTAT.Equals("EQPT_END"))
            {
                btnWorkorder.Visibility = Visibility.Visible;
            }
            else
            {
                btnWorkorder.Visibility = Visibility.Collapsed;
            }

            if (Util.NVC(DataTableConverter.GetValue(oContext, "WIPHOLD")).Equals("Y") || Util.NVC(DataTableConverter.GetValue(oContext, "ABNORM_FLAG")).Equals("Y"))
            {
                btnSave.IsEnabled = false;
                btnRollMapUpdate.IsEnabled = false;
            }
            else
            {
                btnSave.IsEnabled = true;
                btnRollMapUpdate.IsEnabled = true;
            }

            // [E20231221-001759] - GM1 특정 스토커 적재된 HOLD LOT 실적 저장 허용
            if (IsAreaCommonCodeUse("ELEC_HOLD_PROD_RESULT_MODIFY_YN", _Rack_ID))
            {
                if (String.Equals(_WIPSTAT, "WAIT")) // 재고상태 WAIT인 경우일 때만 수정 가능
                {
                    btnSave.IsEnabled = true;
                }
            }

            if (_PROCID.Equals(Process.STACKING_FOLDING))
            {
                cboChange.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_WRK_TYPE_CODE"));
                //폴딩 불량비율 [2018-04-03]
                _WIP_WRK_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_WRK_TYPE_CODE"));
            }

            if (_PROCID.Equals(Process.WINDING))
            {
                double adddefectqty = _dEqptQty - (txtOutQty.Value + txtDefectQty.Value + txtLossQty.Value + txtPrdtReqQty.Value);

                if (Math.Abs(adddefectqty) > 0)
                {
                    txtAssyResultQty.Value = adddefectqty;
                    txtAssyResultQty.FontWeight = FontWeights.Bold;
                    txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    txtAssyResultQty.Value = 0;
                    txtAssyResultQty.FontWeight = FontWeights.Normal;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                    if (convertFromString != null)
                        txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                }

                if ("Y".Equals(_IS_SMALL_TYPE))
                {
                    StackTrayCreate.Visibility = Visibility.Visible;
                    btnOutCell.Visibility = Visibility.Visible;
                    btnOutConfirm.Visibility = Visibility.Visible;
                    btnOutDel.Visibility = Visibility.Visible;
                    dgSubLot.Columns["WIPSTAT_NAME"].Visibility = Visibility.Visible;

                }

                //if (LoginInfo.CFG_SHOP_ID.Equals("A010") || LoginInfo.CFG_SHOP_ID.Equals("F030") || LoginInfo.CFG_SHOP_ID.Equals("G182"))
                //f (_isOutputLotTrayMgmt) // E20231215-001801 SHOP_ID COMMONCODE 관리로 변경
                //{
                DataTable dtEqsgid = new DataTable();
                dtEqsgid = GetEqsgidDataTable();

                if (dtEqsgid != null && dtEqsgid.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEqsgid.Rows.Count; i++)
                    {
                        if (Util.NVC(dtEqsgid.Rows[i]["CBO_CODE"]).Equals(_EQSGID))
                        {
                            StackTrayCreate.Visibility = Visibility.Visible;
                            btnOutConfirm.Visibility = Visibility.Visible;
                            btnOutDel.Visibility = Visibility.Visible;
                            btnCellWindingConfirmCancel.Visibility = Visibility.Visible;
                            btnDischarge.Visibility = Visibility.Visible;
                            btnQtySave.Visibility = Visibility.Visible;
                            _EQSGID_YN = true;// EQSGID와 CSTID의 설비세그먼트가 동일하면 true
                        }
                    }
                }
                //}

            }

            //C20210323-000097
            _OUT_QTY = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED").ToString()));
            _DEFECT_QTY = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_DFCT_QTY").ToString()));
            _LOSS_QTY = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_LOSS_QTY").ToString()));
            _LOTID_AS = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID_AS"));
            _PRDT_REQ_QTY = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_PRDT_REQ_QTY").ToString()));

            txtInputQty.ValueChanged += txtInputQty_ValueChanged;
            txtOutQty.ValueChanged += txtOutQty_ValueChanged;
            txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
        }
        #endregion

        private DataTable GetEqsgidDataTable()
        {
            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROD_GROUP", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("OUT_LOT_TYPE", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = _AREAID;
            dr["PROD_GROUP"] = "CR";
            dr["PROCID"] = _PROCID;
            dr["OUT_LOT_TYPE"] = "CST_ID";

            inDataTable.Rows.Add(dr);
            DataSet ds = new DataSet();
            ds.Tables.Add(inDataTable);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR_WND", "RQSTDT", "RSLTDT", inDataTable);
            return dtResult;
        }


        #region [저장 Biz DataSet 생성]
        private DataSet GetSaveDataSet()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataTable.Columns.Add("CALDATE", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WIPQTY_ED", typeof(Decimal));
            inDataTable.Columns.Add("WIPQTY2_ED", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(Int16));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("REQ_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CHANGE_WIPQTY_FLAG", typeof(string));
            inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(Decimal));
            inDataTable.Columns.Add("FORCE_FLAG", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("WIPQTY_ED_DIFF", typeof(Decimal));
            inDataTable.Columns.Add("WIPQTY2_ED_DIFF", typeof(Decimal));
            inDataTable.Columns.Add("MODL_CHG_FRST_PROD_LOT_FLAG", typeof(string));

            // IN_LOSS 
            DataTable inDataLoss = indataSet.Tables.Add("IN_LOSS");
            inDataLoss.Columns.Add("LOTID", typeof(string));
            inDataLoss.Columns.Add("DFCT_TAG_QTY", typeof(string));
            inDataLoss.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE", typeof(string));
            inDataLoss.Columns.Add("RESNQTY", typeof(Decimal));
            inDataLoss.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataLoss.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataLoss.Columns.Add("RESNNOTE", typeof(string));
            // 폴딩 불량비율 [2018-04-03]
            inDataLoss.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));

            // IN_DFCT
            DataTable inDataDfct = indataSet.Tables.Add("IN_DFCT");
            inDataDfct.Columns.Add("LOTID", typeof(string));
            inDataDfct.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataDfct.Columns.Add("RESNCODE", typeof(string));
            inDataDfct.Columns.Add("RESNQTY", typeof(Decimal));
            inDataDfct.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataDfct.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataDfct.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataDfct.Columns.Add("RESNNOTE", typeof(string));
            //폴란드 불량 수량 때문에 
            //inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(string));
            //inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(string));
            //inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(string));

            if (LoginInfo.LANGID.Equals("pl-PL"))
            {
                inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(Decimal));
                inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(Decimal));
                inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(Decimal));
            }
            else
            {
                inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(string));
                inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(string));
                inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(string));
                // 폴딩 불량비율 [2018-04-03]
                inDataDfct.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));
            }

            // IN_PRDT_REQ
            DataTable inDataPrdtReq = indataSet.Tables.Add("IN_PRDT_REQ");
            inDataPrdtReq.Columns.Add("LOTID", typeof(string));
            inDataPrdtReq.Columns.Add("DFCT_TAG_QTY", typeof(string));
            inDataPrdtReq.Columns.Add("WIPSEQ", typeof(string));
            inDataPrdtReq.Columns.Add("RESNCODE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY2", typeof(string));
            inDataPrdtReq.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNNOTE", typeof(string));
            inDataPrdtReq.Columns.Add("COST_CNTR_ID", typeof(string));
            // 폴딩 불량비율 [2018-04-03]
            inDataPrdtReq.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));
            return indataSet;
        }




        #endregion

        #region 생산수, 양품수 변경시 길이초과 불량 그리드에서 Clear
        private void SetLengthExceeedClear()
        {
            DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);

            // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
            dt.Select("PRCS_ITEM_CODE = 'LENGTH_EXCEED'").ToList<DataRow>().ForEach(r => r["RESNQTY"] = 0);
            dt.AcceptChanges();

            _dtLengthExceeed.Clear();
            _dtLengthExceeed = dt.Copy();

            dgDefect.ItemsSource = DataTableConverter.Convert(dt);

            txtLengthExceedQty.Value = 0;

        }
        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string GetWipNote()
        {
            string sReturn;
            string[] sWipNote = _WIP_NOTE.Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = _WIP_NOTE;
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }

        private string SetWipNote()
        {
            string sReturn;
            string[] sWipNote = _WIP_NOTE.Split('|');

            sReturn = txtNote.Text + "|";

            for (int nlen = 1; nlen < sWipNote.Length; nlen++)
            {
                sReturn += sWipNote[nlen] + "|";
            }

            return sReturn.Substring(0, sReturn.Length - 1);
        }

        #endregion

        #endregion

        private void dgDefect_FOL_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!_PROCID.Equals(Process.STACKING_FOLDING))
                    return;

                double Reg_Atype;
                double Reg_Ctype;
                double Reg_Resnqty;
                double Cal_Resnqty;

                double Division_A;
                double Division_C;

                string sEQP_DFCT_QTY = string.Empty;
                double dEQP_DFCT_QTY;

                sEQP_DFCT_QTY = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "EQP_DFCT_QTY"));
                dEQP_DFCT_QTY = double.Parse(sEQP_DFCT_QTY);

                if (e.Cell.Column.Name.Equals("REG_A"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sFtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sATemp = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_A"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Resnqty = double.Parse(sFtype);
                    double Temp_A = double.Parse(sATemp);
                    Cal_Resnqty = double.Parse(sCalc_FC);

                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);

                    if (gDfctAQty > 0 && gDfctCQty > 0)
                    {
                        if (Reg_Ctype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype);
                        }
                        else
                        {
                            if (Division_A != Division_C)
                            {
                                if (Division_A > Division_C)
                                {
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_C * gDfctCQty));

                                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty + dEQP_DFCT_QTY);
                                    //}
                                    //else
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty);
                                    //}

                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    else
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_A * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + dEQP_DFCT_QTY);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                                    //}
                                    //else
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                    //}

                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    else
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                }
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                //{
                                //    if (Reg_Resnqty == 0)
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                //    else
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                //}
                                //else
                                //{
                                //    if (Reg_Resnqty == 0)
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                //    else
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                //}

                                if (Reg_Resnqty == 0)
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                else
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                            }
                        }

                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_A", Reg_Atype);
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_C"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sFtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sCTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_C"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Resnqty = double.Parse(sFtype);
                    double Temp_C = double.Parse(sCTemp);
                    Cal_Resnqty = double.Parse(sCalc_FC);

                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);

                    if (gDfctAQty > 0 && gDfctCQty > 0)
                    {
                        if (Reg_Atype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype);
                        }
                        else
                        {
                            if (Division_A != Division_C)
                            {
                                if (Division_A > Division_C)
                                {
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_C * gDfctCQty));

                                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                    //}
                                    //else
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                    //} 

                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    else
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_A * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + dEQP_DFCT_QTY);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                                    //}
                                    //else
                                    //{
                                    //    if (Reg_Resnqty == 0)
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    //    else
                                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                    //}

                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    else
                                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                }
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                //{
                                //    if (Reg_Resnqty == 0)
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                //    else
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                //}
                                //else
                                //{
                                //    if (Reg_Resnqty == 0)
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                //    else
                                //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                //}

                                if (Reg_Resnqty == 0)
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                else
                                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                            }
                        }

                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_C", Reg_Ctype);
                    }

                }
                else if (e.Cell.Column.Name.Equals("REG_F"))
                {

                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));
                    string sTemp_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_F"));

                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "REG_C"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    double dTemp = double.Parse(sTemp_FC);

                    Reg_Resnqty = double.Parse(sReg_FC);
                    Cal_Resnqty = double.Parse(sCalc_FC);


                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);

                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                    //{
                    //    if (Reg_Atype == 0 && Reg_Ctype == 0)
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + dEQP_DFCT_QTY);
                    //    }

                    //    if (Division_A > Division_C)
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                    //    }
                    //    else
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                    //    }
                    //}
                    //else
                    //{
                    //    if (Reg_Atype == 0 && Reg_Ctype == 0)
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                    //    }

                    //    if (Division_A > Division_C)
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                    //    }
                    //    else
                    //    {
                    //        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                    //    }
                    //}

                    if (Reg_Atype == 0 && Reg_Ctype == 0)
                    {
                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                    }

                    if (Division_A > Division_C)
                    {
                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                    }

                    DataTableConverter.SetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "TEMP_F", sReg_FC);
                }

                txtInputQty.ValueChanged -= txtInputQty_ValueChanged;
                txtOutQty.ValueChanged -= txtOutQty_ValueChanged;

                isChangeDefect = true;

                DataTable dtInfo = DataTableConverter.Convert(dgDefect_FOL.ItemsSource);

                double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'DEFECT_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'DEFECT_LOT' AND RESNGRID = 'DEFECT_TOP'")));
                double dLoss = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'LOSS_LOT' AND RESNGRID <> 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED'")));
                double dLoss_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'LOSS_LOT' AND RESNGRID = 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED'")));
                double dLoss_EXCEED = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'LOSS_LOT' AND PRCS_ITEM_CODE = 'LENGTH_EXCEED'")));
                double dPrdtReq = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                double dPrdtReq_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(CALC_F)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID = 'DEFECT_TOP'")));

                double dLoss_BeforeEXCEED = Convert.ToDouble(Util.NVC_Decimal(_dtLengthExceeed.Compute("sum(CALC_F)", "ACTID = 'LOSS_LOT' AND PRCS_ITEM_CODE = 'LENGTH_EXCEED'")));

                //dLoss_EXCEED = dLoss_EXCEED - dLoss_BeforeEXCEED;

                //double dIn = Convert.ToDouble(txtInputQty.Value);
                //double dOut = Convert.ToDouble(txtOutQty.Value);
                //double dTotalDefect = dDefect + dLoss + dPrdtReq + (dDefect_top * 0.5) + (dLoss_top * 0.5) + (dPrdtReq_top * 0.5);
                double dIn = _dInQty;
                double dOut = _dOutQty;
                double dTotalDefect = 0;
                double dAlphaQty = 0;

                //dOut = dIn - dTotalDefect;

                // 불량 합산
                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                    // 코팅이고 단면이 아닌경우
                    dTotalDefect = dDefect + dLoss + dPrdtReq + (dDefect_top * 0.5) + (dLoss_top * 0.5) + (dPrdtReq_top * 0.5);
                else
                    dTotalDefect = dDefect + dLoss + dPrdtReq + dDefect_top + dLoss_top + dPrdtReq_top;

                // 생산량 산출
                if (_PROCID.Equals(Process.COATING) ||
                    _PROCID.Equals(Process.LAMINATION) ||
                    _PROCID.Equals(Process.STACKING_FOLDING) ||
                    _PROCID.Equals(Process.WINDING) ||
                    _PROCID.Equals(Process.WASHING)
                    )
                {
                    // 길이 초과가 있는경우 양품수에 합산 불량에서는 제외
                    //if (_PROCID.Equals(Process.COATING))
                    //    dOut += dLoss_EXCEED;
                    //dIn = dOut + dTotalDefect;

                    //if (_PROCID.Equals(Process.COATING))
                    //{
                    //    //dIn = dOut + dTotalDefect - dLoss_EXCEED;
                    //    dOut = dIn + dLoss_EXCEED - dTotalDefect;
                    //}
                    //else
                    //{
                    dIn = dOut + dTotalDefect;
                    //}
                }
                else if (!_PROCID.Equals(Process.PACKAGING))
                {
                    // 전극
                    //dIn += dLoss_EXCEED;
                    //dOut = dIn - dTotalDefect;

                    //dIn = dOut + dTotalDefect - dLoss_EXCEED;
                    dOut = dIn + dLoss_EXCEED - dTotalDefect;
                }

                if (dOut < 0)
                {
                    //Util.AlertInfo("SFU1884");      //전체 불량 수량이 양품 수량보다 클 수 없습니다.
                    Util.MessageValidation("SFU1884");
                    return;
                }

                txtInputQty.Value = dIn;
                txtOutQty.Value = dOut;
                txtLengthExceedQty.Value = dLoss_EXCEED;

                if (_PROCID.Equals(Process.COATING) && _COATING_GUBUN != "CS")
                {
                    txtDefectQty.Value = dDefect + (dDefect_top * 0.5);
                    txtLossQty.Value = dLoss + (dLoss_top * 0.5);
                    txtPrdtReqQty.Value = dPrdtReq + (dPrdtReq_top * 0.5);
                }
                else
                {
                    txtDefectQty.Value = dDefect + dDefect_top;
                    txtLossQty.Value = dLoss + dLoss_top;
                    txtPrdtReqQty.Value = dPrdtReq + dPrdtReq_top;
                }

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    // 차이수량 산출
                    dAlphaQty = (dOut + dTotalDefect) - dIn;
                    txtInputDiffQty.Value = dAlphaQty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInputQty.ValueChanged += txtInputQty_ValueChanged;
                txtOutQty.ValueChanged += txtOutQty_ValueChanged;
            }
        }

        private void dgDefect_FOL_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C")
                    {
                        string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
                        if (sActid == "DEFECT_LOT")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            //e.Cell.Column.IsReadOnly = false;
                            //e.Cell.Column.EditOnSelection = true;
                        }
                        else
                        {
                            //e.Cell.Column.EditOnSelection = false;
                            //e.Cell.Column.IsReadOnly = true;
                            //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "REG_F" || Convert.ToString(e.Cell.Column.Name) == "DFCT_QTY_DDT_RATE")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }

                }
            }));
        }

        private void dgDefect_FOL_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C"))
                {
                    string sActid = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ACTID"));
                    if (sActid != "DEFECT_LOT")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectInfo_FOL()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                inDataTable.Columns.Add("TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["TYPE"] = _StackingYN;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_QCA_SEL_CELL_TYPE_DFCT_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgDefect_FOL, searchResult, FrameOperation, false);

                        _dtLengthExceeed = searchResult.Copy();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //private void Worktype_Change()
        //{
        //    try
        //    {
        //        if (_PROCID.Equals(Process.STACKING_FOLDING))
        //        {

        //            DataTable inTable = new DataTable();
        //            inTable.TableName = "RQSTDT";
        //            inTable.Columns.Add("SRCTYPE", typeof(String));
        //            inTable.Columns.Add("EQPTID", typeof(String));
        //            inTable.Columns.Add("PROD_LOTID", typeof(String));
        //            inTable.Columns.Add("WIP_WRK_TYPE_CODE", typeof(String));

        //            DataRow newRow = inTable.NewRow();
        //            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        //            newRow["EQPTID"] = _EQPTID;
        //            newRow["PROD_LOTID"] = _LOTID;
        //            newRow["WIP_WRK_TYPE_CODE"] = cboChange.SelectedValue;

        //            inTable.Rows.Add(newRow);

        //            new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOTATTR_WIP_WRK_TYPE_CODE_PRODLOT", "INDATA", "OUTDATA", inTable);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_LOTID.Equals(""))
                {
                    //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                    Util.MessageValidation("SFU1381");
                    return;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    //ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return;
                }

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    #region 폴딩불량비율 [2018-09-11]
                    if (cboChange.SelectedValue.Equals("SCRAP"))
                    {
                        if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                        {
                            // 사유를 입력하세요.
                            Util.MessageValidation("SFU1594");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                        {
                            // 요청자를 입력 하세요.
                            Util.MessageValidation("SFU3451");
                            return;
                        }

                        DataTable dt = ((DataView)dgDefect_FOL.ItemsSource).Table.Select().CopyToDataTable();
                        int sum = (int)dt.AsEnumerable().Sum(r => r.GetValue("DFCT_QTY_DDT_RATE").GetDecimal());
                        if ((sum != 100))
                        {
                            Util.MessageValidation("SFU4518");      //불량 재생비율의 합은 100% 입니다.
                            return;
                        }
                    }
                    #endregion

                    DataTable inTable = new DataTable();
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("SRCTYPE", typeof(String));
                    inTable.Columns.Add("EQPTID", typeof(String));
                    inTable.Columns.Add("PROD_LOTID", typeof(String));
                    inTable.Columns.Add("WIP_WRK_TYPE_CODE", typeof(String));
                    inTable.Columns.Add("USERID", typeof(String));

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = _EQPTID;
                    newRow["PROD_LOTID"] = _LOTID;
                    newRow["WIP_WRK_TYPE_CODE"] = cboChange.SelectedValue;
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("BR_PRD_REG_LOTATTR_WIP_WRK_TYPE_CODE_PRODLOT", "INDATA", null, inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            // 폴딩 불량재생비율 저장 
                            if (cboChange.SelectedValue.Equals("SCRAP"))
                            {
                                btnSave_Click(null, null);
                            }
                            else
                            {
                                Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                                btnSearch_Click(null, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboChange_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboChange.SelectedValue.Equals("SCRAP"))
                dgDefect_FOL.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;
            else
                dgDefect_FOL.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Winding  추가 불량 산출
        /// </summary>
        /// <returns></returns>
        private decimal GetInputQtyByApplyTypeCode()
        {
            decimal qty = 0;

            if (CommonVerify.HasDataGridRow(dgDefect))
            {
                DataTable dt = ((DataView)dgDefect.ItemsSource).Table;

                decimal qtyPlus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "PLUS").Sum(s => s.GetValue("RESNQTY").GetDecimal());
                decimal qtyMinus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "MINUS").Sum(s => s.GetValue("RESNQTY").GetDecimal());
                return qtyPlus - qtyMinus;
            }
            return qty;
        }

        private void btnDefectReInputSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefectReInput()) return;
            SaveDefectReInput();
        }

        private string GetReInputReasonApplyFlag(string equipmentCode)
        {
            const string bizRuleName = "DA_PRD_SEL_REINPUT_RSN_APPLY_FLAG";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            return CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0][0].GetString() : string.Empty;
        }

        private void GetDefectReInputList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _EQPTID;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefectReInput.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefectReInput.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectReInput()
        {
            try
            {
                dgDefectReInput.EndEdit();

                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_FOR_REINPUT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefectReInput.Rows.Count - dgDefectReInput.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = _LOTID;
                    newRow["WIPSEQ"] = _WIPSEQ;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNNOTE"));
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        GetLotList(_LOTID);
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetDefectReInputList();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefectReInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                txtInputQty.ValueChanged -= txtInputQty_ValueChanged;
                txtOutQty.ValueChanged -= txtOutQty_ValueChanged;
                txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

                isChangeDefect = true;

                DataTable dtInfo = DataTableConverter.Convert(dgDefect.ItemsSource);

                //double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                //double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID = 'DEFECT_TOP'")));
                double dDefect = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID <> 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));
                double dDefect_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RESNGRID = 'DEFECT_TOP' AND RSLT_EXCL_FLAG = 'N'")));

                double dLoss = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND RESNGRID <> 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND RESNGRID = 'DEFECT_TOP' AND PRCS_ITEM_CODE <> 'LENGTH_EXCEED' AND RSLT_EXCL_FLAG = 'N'")));
                double dLoss_EXCEED = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND PRCS_ITEM_CODE = 'LENGTH_EXCEED'")));
                double dPrdtReq = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID <> 'DEFECT_TOP'")));
                double dPrdtReq_top = Convert.ToDouble(Util.NVC_Decimal(dtInfo.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RESNGRID = 'DEFECT_TOP'")));


                double dIn = _dInQty;
                double dOut = _dOutQty;
                double dTotalDefect = 0;
                //double dAlphaQty = 0;
                double dDiff = 0;

                DataTable dtReInput = DataTableConverter.Convert(dgDefectReInput.ItemsSource);
                double dDefectReInput = Convert.ToDouble(Util.NVC_Decimal(dtReInput.Compute("sum(RESNQTY)", "RSLT_EXCL_FLAG = 'N'")));

                //dOut = dIn - dTotalDefect;

                dTotalDefect = dDefect + dLoss + dPrdtReq + dDefect_top + dLoss_top + dPrdtReq_top;

                if (_PROCID.Equals(Process.ASSEMBLY))
                {
                    txtInputDiffQty.Value = dDefectReInput;

                    // 차이수량 산출  (투입량 + 재투입) - (양품량+불량량+Loss량+물품청구)
                    if (txtInputDiffQty.Value.ToString(CultureInfo.InvariantCulture).Equals("NaN") || txtInputDiffQty.Value.Equals(0))
                        dDiff = dIn - (dOut + dTotalDefect);
                    else
                        dDiff = (dIn + txtInputDiffQty.Value) - (dOut + dTotalDefect);
                }

                // ======================================= 저장전 체크 필요
                if (dOut < 0)
                {
                    //Util.AlertInfo("SFU1884");      //전체 불량 수량이 양품 수량보다 클 수 없습니다.
                    Util.MessageValidation("SFU1884");
                    return;
                }

                txtInputQty.Value = dIn;
                txtOutQty.Value = dOut;
                txtLengthExceedQty.Value = dLoss_EXCEED;
                txtDiffQty.Value = dDiff;

                txtDefectQty.Value = dDefect + dDefect_top;
                txtLossQty.Value = dLoss + dLoss_top;
                txtPrdtReqQty.Value = dPrdtReq + dPrdtReq_top;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInputQty.ValueChanged += txtInputQty_ValueChanged;
                txtOutQty.ValueChanged += txtOutQty_ValueChanged;
                txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
            }
        }

        private void cboWaitHalfProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnWaitHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void btnWaitHalfProductInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationWaitHalfProductInput()) return;

                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        WaitHalfProductInput();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void OnClickInputMaterial(object sender, RoutedEventArgs e)
        {

            #region [Merge LotTrace (2018.04.24) 주석처리]
            //if (Util.GetCondition(cboEquipment).ToString().Equals(""))
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.

            //    return;
            //}
            #endregion
            //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);


            CMM001.Popup.CMM_INPUT_MATERIAL wndPopup = new CMM001.Popup.CMM_INPUT_MATERIAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = _LOTID;
                Parameters[1] = _WORKORDER;
                Parameters[2] = _EQPTID;  // Util.NVC(cboEquipment.SelectedValue); // 2018.04.24 주석처리
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = _PROCID;
                Parameters[5] = _PRODID;
                Parameters[6] = Util.NVC(_VERCODE);
                Parameters[7] = _dInQty;
                Parameters[8] = _bool;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(Material_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void Material_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_INPUT_MATERIAL window = sender as CMM001.Popup.CMM_INPUT_MATERIAL;
            #region [Merge LotTrace (2018.04.24)]
            MergeLotTrace(_LOTID);
            #endregion
            GetMaterialSummary(_LOTID, _WORKORDER);
        }

        private void LoadedQualityInfoCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            //else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01") || string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));

                                if (panel != null)
                                {

                                    C1NumericBox numeric = panel.Children[0] as C1NumericBox;



                                    if (numeric != null && !string.IsNullOrEmpty(Util.NVC(numeric.Value)) && numeric.Value != 0 && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                    {
                                        // 프레임버그로 값 재 설정 [2017-12-06]
                                        if (!string.IsNullOrEmpty(sValue) && !string.Equals(sValue, "NaN"))
                                            numeric.Value = Convert.ToDouble(sValue);

                                        if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        }

                                        else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        }
                                        else
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                        }
                                    }

                                    numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                    numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;

                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                }));
            }
        }
        protected virtual void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line);

                            iRowIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_NJ_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                int iChk = 1;
                int iTot = 0;
                double Min = 0;
                double Max = 0;
                List<double> dIndex = new List<double>();

                string sColname = string.Empty;
                string sCol = string.Empty;
                string sNo = string.Empty;

                sColname = e.Cell.Column.Name;

                sCol = sColname.Substring(0, 3);
                sNo = sColname.Substring(3, 1);

                int iColIdx = 0;
                int iRowIdx = 0;

                iColIdx = dgDefect_NJ.Columns[sColname].Index;
                iRowIdx = e.Cell.Row.Index;

                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                        int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                        string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                        double dTemp = double.Parse(sTemp);

                        if (dTemp == 0)
                        {
                            iChk = 0;
                        }

                        double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                        double Division = Math.Floor(dTemp / dInputCnt);
                        dIndex.Add(Division);

                        iTot = iTot + 1;
                        //iFcChk = 0;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                            //iFcChk = 0;
                        }
                    }
                }
                else
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                        int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                        string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                        double dTemp = double.Parse(sTemp);

                        if (dTemp == 0)
                        {
                            iChk = 0;
                        }

                        double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                        double Division = Math.Floor(dTemp / dInputCnt);
                        dIndex.Add(Division);

                        iTot = iTot + 1;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                        }
                    }
                }

                Max = dIndex[0];
                Min = dIndex[0];

                for (int icnt = 0; icnt < iTot; icnt++)
                {
                    if (Max < dIndex[icnt])
                    {
                        Max = dIndex[icnt];
                    }

                    if (Min > dIndex[icnt])
                    {
                        Min = dIndex[icnt];
                    }
                }


                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                    double Reg_Resnqty = double.Parse(sReg_FC);

                    //if (iChk == 0 && iFcChk == 0)
                    if (iChk == 0)
                    {
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    }
                }
                else
                {
                    if (iChk != 0)
                    {
                        if (iBomcnt == 1)
                        {
                            //for (int i = 0; i < iCnt + 1; i++)
                            //{
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = String.Empty;

                            sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;

                            if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                            //else
                            //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            //}
                        }
                        else
                        {
                            for (int i = 0; i < iCnt + 1; i++)
                            {
                                int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                string sTemp = String.Empty;

                                sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                                double dTemp = double.Parse(sTemp);

                                if (dTemp == 0)
                                {
                                    iChk = 0;
                                }

                                double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                                double Division = Math.Floor(dTemp / dInputCnt);
                                dIndex.Add(Division);

                                iTot = iTot + 1;

                                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                                //else
                                //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            }
                        }
                    }
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                    double Reg_Resnqty = double.Parse(sReg_FC);
                    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_NJ_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C" || Convert.ToString(e.Cell.Column.Name) == "REG_L" 
                    //|| Convert.ToString(e.Cell.Column.Name) == "REG_R" || Convert.ToString(e.Cell.Column.Name) == "REG_ML" || Convert.ToString(e.Cell.Column.Name) == "REG_MR")
                    //{
                    if (e.Cell.Column.Name.ToString().StartsWith("REG"))
                    {
                        string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
                        if (sActid == "DEFECT_LOT")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        }
                        else
                        {
                            //e.Cell.Column.EditOnSelection = false;
                            //e.Cell.Column.IsReadOnly = true;
                            //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "INPUT_FC")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                }
            }));
        }

        private void dgDefect_NJ_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                //if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C") || e.Column.Name.Equals("REG_L") || e.Column.Name.Equals("REG_R")
                //    || e.Column.Name.Equals("REG_ML") || e.Column.Name.Equals("REG_MR"))
                if (e.Column.Name.ToString().StartsWith("REG"))
                {
                    string sActid = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ACTID"));
                    if (sActid != "DEFECT_LOT")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private int Get_Type_Chk(string sType)
        {
            int iCode = 0;

            if (_PROCID.Equals(Process.STACKING_FOLDING))
            {
                if (sType == "AM")
                {
                    iCode = 0;
                }
                else if (sType == "AT")
                {
                    iCode = 1;
                }
                else if (sType == "CM")
                {
                    iCode = 2;
                }
                else if (sType == "CT")
                {
                    iCode = 3;
                }
                else if (sType == "LM")
                {
                    iCode = 4;
                }
                else if (sType == "LT")
                {
                    iCode = 5;
                }
                else if (sType == "MA")
                {
                    iCode = 6;
                }
                else if (sType == "MB")
                {
                    iCode = 7;
                }
                else if (sType == "ML")
                {
                    iCode = 8;
                }
                else if (sType == "MM")
                {
                    iCode = 9;
                }
                else if (sType == "MR")
                {
                    iCode = 10;
                }
                else if (sType == "MT")
                {
                    iCode = 11;
                }
                else if (sType == "RM")
                {
                    iCode = 12;
                }
                else if (sType == "RT")
                {
                    iCode = 13;
                }
            }
            else if (_PROCID.Equals(Process.STP))
            {
                if (sType == "SB") //SRC Mono-Type Cell
                {
                    iCode = 0;
                }
                else if (sType == "SH") //SRC HALF-Type Cell
                {
                    iCode = 1;
                }
                else if (sType == "SM") //SRC Mono-Middle-Type Cell
                {
                    iCode = 2;
                }
                else if (sType == "ST") //SRC Mono-Top-Type Cell
                {
                    iCode = 3;
                }
            }
            return iCode;
        }

        private void SetBOMCnt(int i, string sCode, string sCnt)
        {
            if (i == 0)
            {
                tbType0.Visibility = Visibility.Visible;
                txtType0.Visibility = Visibility.Visible;

                tbType0.Text = sCode;
                txtType0.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 1)
            {
                tbType1.Visibility = Visibility.Visible;
                txtType1.Visibility = Visibility.Visible;

                tbType1.Text = sCode;
                txtType1.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 2)
            {
                tbType2.Visibility = Visibility.Visible;
                txtType2.Visibility = Visibility.Visible;

                tbType2.Text = sCode;
                txtType2.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 3)
            {
                tbType3.Visibility = Visibility.Visible;
                txtType3.Visibility = Visibility.Visible;

                tbType3.Text = sCode;
                txtType3.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 4)
            {
                tbType4.Visibility = Visibility.Visible;
                txtType4.Visibility = Visibility.Visible;

                tbType4.Text = sCode;
                txtType4.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 5)
            {
                tbType5.Visibility = Visibility.Visible;
                txtType5.Visibility = Visibility.Visible;

                tbType5.Text = sCode;
                txtType5.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 6)
            {
                tbType6.Visibility = Visibility.Visible;
                txtType6.Visibility = Visibility.Visible;

                tbType6.Text = sCode;
                txtType6.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 7)
            {
                tbType7.Visibility = Visibility.Visible;
                txtType7.Visibility = Visibility.Visible;

                tbType7.Text = sCode;
                txtType7.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 8)
            {
                tbType8.Visibility = Visibility.Visible;
                txtType8.Visibility = Visibility.Visible;

                tbType8.Text = sCode;
                txtType8.Text = Convert.ToDouble(sCnt).ToString();
            }
        }

        private void GetDefectInfo_NJ()
        {
            try
            {
                //ShowLoadingIndicator();

                string bizRuleName = string.Empty;

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                    bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST_NJ";
                else if (_PROCID.Equals(Process.STP))
                    bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST_STP";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                //inDataTable.Columns.Add("TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                //newRow["TYPE"] = _StackingYN;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Defect_Sum();

                        Util.GridSetData(dgDefect_NJ, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputBox_Click(object sender, RoutedEventArgs e)
        {
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK");

            if (idx == -1)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return;
            }

            COM001_009_MODIFY_PKG_BOX wndBox = new COM001_009_MODIFY_PKG_BOX();
            wndBox.FrameOperation = FrameOperation;


            if (wndBox != null)
            {

                object[] Parameters = new object[7];

                Parameters[0] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "EQPTID"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "WO_DETL_ID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "PROCID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "SHIFT"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "CALDATE"));
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID"));

                C1WindowExtension.SetParameters(wndBox, Parameters);

                wndBox.Closed += new EventHandler(wndBox_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndBox.ShowModal()));
            }
        }
        private void wndBox_Closed(object sender, EventArgs e)
        {
            COM001_009_MODIFY_PKG_BOX window = sender as COM001_009_MODIFY_PKG_BOX;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                string lotid = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                int wipseq = int.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "WIPSEQ")));

                GetLotList();

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")).Equals(lotid))
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "CHK", true);
                        break;
                    }
                }

                GetInBox(lotid, wipseq);

            }
        }
        private bool CanCreateTray()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void btnOutCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateTray())
                return;

            // 특별 Tray 정보 Check.
            string sSamePkgLot = "Y";
            string messageCode = string.Empty;

            /* PKG 특별 Tray 처리 주석 처리( 추후 생산실적 처리 화면에서 기능 추가 요청시 진행 예정)
             * 2019-06-17 최상민 
                        DataTable dtRslt = GetSpecialTrayInfo();
                        if (dtRslt != null && dtRslt.Rows.Count > 0)
                        {
                            string sSpclProdLot = Util.NVC(dtRslt.Rows[0]["SPCL_PROD_LOTID"]);
                            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK");

                            if (!sSpclProdLot.Equals("") && iRow >= 0)
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[iRow].DataItem, "LOTID")).Equals(sSpclProdLot))
                                {
                                    sSamePkgLot = "N";

                                    //sMsg = "선택한 조립 LOT과 특별 TRAY로 설정된 조립 LOT이 다릅니다.";
                                    messageCode = "SFU1665";
                                }
                            }
                        }
            */
            if (string.IsNullOrEmpty(messageCode))
            {
                ASSY001_007_TRAY_CREATE wndTrayCreate = new ASSY001_007_TRAY_CREATE();
                wndTrayCreate.FrameOperation = FrameOperation;

                if (wndTrayCreate != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "WIPSEQ"));
                    //Parameters[4] = (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                    Parameters[4] = "Y";
                    Parameters[5] = "";//cboTrayType.SelectedValue.ToString();
                    Parameters[6] = sSamePkgLot;

                    C1WindowExtension.SetParameters(wndTrayCreate, Parameters);

                    wndTrayCreate.Closed += new EventHandler(wndTrayCreate_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                    grdMain.Children.Add(wndTrayCreate);
                    wndTrayCreate.BringToFront();
                }
            }
            else
            {
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY001_007_TRAY_CREATE wndTrayCreate = new ASSY001_007_TRAY_CREATE();
                        wndTrayCreate.FrameOperation = FrameOperation;

                        if (wndTrayCreate != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "WIPSEQ"));
                            //Parameters[4] = (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                            Parameters[4] = "Y";
                            Parameters[5] = "";// cboTrayType.SelectedValue.ToString();
                            Parameters[6] = sSamePkgLot;

                            C1WindowExtension.SetParameters(wndTrayCreate, Parameters);

                            wndTrayCreate.Closed += new EventHandler(wndTrayCreate_Closed);

                            // 팝업 화면 숨겨지는 문제 수정.
                            //this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                            grdMain.Children.Add(wndTrayCreate);
                            wndTrayCreate.BringToFront();
                        }
                    }
                });
            }
        }

        private void wndTrayCreate_Closed(object sender, EventArgs e)
        {
            ASSY001_007_TRAY_CREATE window = sender as ASSY001_007_TRAY_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                // tray 생성 후 trace 모드인 경우는 cell 팝업 호출.
                /*
                    if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                    {
                        ASSY001_007_CELL_LIST wndCellList = new ASSY001_007_CELL_LIST();
                        wndCellList.FrameOperation = FrameOperation;

                        if (wndCellList != null)
                        {
                            object[] Parameters = new object[8];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[4] = Util.NVC(window.CREATE_TRAYID);
                            Parameters[5] = Util.NVC(window.CREATE_TRAY_QTY);
                            Parameters[6] = Util.NVC(window.CREATE_OUT_LOT);
                            Parameters[7] = false;  // View Mode. (Read Only)

                            C1WindowExtension.SetParameters(wndCellList, Parameters);

                            wndCellList.Closed += new EventHandler(wndCellList_Closed);

                            // 팝업 화면 숨겨지는 문제 수정.
                            this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                            //grdMain.Children.Add(wndCellList);
                            //wndCellList.BringToFront();
                        }
                    }
                */
                GetLotList(_LOTID);
                GetSubLot();
                //GetOutTray();
            }

            this.grdMain.Children.Remove(window);
        }

        private void WaitHalfProductInput()
        {
            try
            {
                if (string.IsNullOrEmpty(_LOTID) == true)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                    return;
                }

                ShowLoadingIndicator();

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgWaitHalfProduct, "CHK");

                DataSet inDataSet = new DataSet();
                string bizRuleName = string.Empty;
                string bizInDataName = "INDATA,INLOT";

                if (string.Equals(_PROCID, Process.ASSEMBLY))
                {
                    bizRuleName = "BR_PRD_REG_INPUT_LOT_COMPLETE_UI";
                    bizInDataName = "INDATA,INLOT";

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("CALDATE", typeof(string));
                    inDataTable.Columns.Add("SHIFT", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));
                    inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["CALDATE"] = dtpCaldate.SelectedDateTime;
                    dr["SHIFT"] = txtShift.Tag;
                    dr["EQPTID"] = _EQPTID;
                    dr["PROCID"] = _PROCID;
                    dr["PROD_LOTID"] = _LOTID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                    inDataTable.Rows.Add(dr);

                    DataTable ininput = inDataSet.Tables.Add("INLOT");
                    ininput.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = ininput.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                    ininput.Rows.Add(newRow);



                }
                else if (string.Equals(_PROCID, Process.WASHING)) //C20181022_22742 추가
                {
                    bizRuleName = "BR_PRD_REG_INPUT_LOT_WS_UI";
                    bizInDataName = "IN_EQP,IN_INPUT";

                    DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                    inDataTable.Columns.Add("PROD_WIPSEQ", typeof(Decimal));
                    inDataTable.Columns.Add("PROD_CALDATE", typeof(DateTime));

                    DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    ininput.Columns.Add("INPUT_LOTID", typeof(string));
                    ininput.Columns.Add("INPUT_QTY", typeof(Decimal));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EQPTID;
                    newRow["USERID"] = LoginInfo.USERID;

                    newRow["PROD_LOTID"] = _LOTID;
                    newRow["PROD_WIPSEQ"] = _WIPSEQ;
                    newRow["PROD_CALDATE"] = dtpCaldate.SelectedDateTime; //  dtTime.ToString("yyyy-MM-dd");
                    inDataTable.Rows.Add(newRow);

                    for (int i = 0; i < dgWaitHalfProduct.Rows.Count - dgWaitHalfProduct.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgWaitHalfProduct, "CHK", i)) continue;

                        newRow = ininput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue.ToString();
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[i].DataItem, "LOTID"));
                        newRow["INPUT_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[i].DataItem, "WIPQTY2")));

                        ininput.Rows.Add(newRow);
                        break;
                    }
                }


                new ClientProxy().ExecuteService_Multi(bizRuleName, bizInDataName, null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //GetProductLot();
                    GetWaitHalfProductList();
                    Util.MessageInfo("SFU1275");

                    GetLotList(_LOTID);

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationWaitHalfProductInput()
        {
            if (cboWaitHalfProduct.SelectedValue == null || cboWaitHalfProduct.SelectedValue.GetString() == "SELECT" || string.IsNullOrEmpty(cboWaitHalfProduct.SelectedValue.GetString()))
            {
                //투입 위치를 선택하세요.
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitHalfProduct, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void dgWaitHalfProduct_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgWaitHalfProduct.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgWaitHalfProduct.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            var chkSelected = cell.Presenter.Content as CheckBox;

            if (chkSelected != null && cell.Presenter != null && cell.Presenter.Content != null
                && chkSelected.IsChecked.HasValue && !(bool)chkSelected.IsChecked)
            {
                DataTableConverter.SetValue(dgWaitHalfProduct.Rows[cell.Row.Index].DataItem, "CHK", true);
                chkSelected.IsChecked = true;

                for (int inx = 0; inx < dgWaitHalfProduct.Rows.Count; inx++)
                {
                    if (inx != cell.Row.Index)
                    {
                        DataTableConverter.SetValue(dgWaitHalfProduct.Rows[inx].DataItem, "CHK", false);

                        if (dgWaitHalfProduct.GetCell(inx, cell.Column.Index).Presenter != null)
                        {
                            var chk = dgWaitHalfProduct.GetCell(inx, cell.Column.Index).Presenter.Content as CheckBox;
                            if (chk != null) chk.IsChecked = false;
                        }
                    }
                }

            }
            else
            {
                if (chkSelected != null && cell.Presenter != null && cell.Presenter.Content != null
                    && chkSelected.IsChecked.HasValue && (bool)chkSelected.IsChecked)
                {
                    DataTableConverter.SetValue(dgWaitHalfProduct.Rows[cell.Row.Index].DataItem, "CHK", false);
                    chkSelected.IsChecked = false;
                }
            }
        }


        //private void dgWaitHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        //{
        //    Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        C1DataGrid dg = sender as C1DataGrid;

        //        CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

        //        if (chk != null)
        //        {
        //            switch (Convert.ToString(e.Cell.Column.Name))
        //            {
        //                case "CHK":
        //                    if (dg != null)
        //                    {
        //                        var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
        //                        if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
        //                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
        //                                                 checkBox.IsChecked.HasValue &&
        //                                                 !(bool)checkBox.IsChecked))
        //                        {
        //                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
        //                            chk.IsChecked = true;

        //                            for (int i = 0; i < dg.Rows.Count; i++)
        //                            {
        //                                if (i != e.Cell.Row.Index)
        //                                {
        //                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

        //                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
        //                                    {
        //                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
        //                                        if (chk != null) chk.IsChecked = false;
        //                                    }
        //                                }
        //                            }

        //                        }
        //                        else
        //                        {
        //                            var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
        //                            if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
        //                                                dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
        //                                                box.IsChecked.HasValue &&
        //                                                (bool)box.IsChecked))
        //                            {
        //                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
        //                                chk.IsChecked = false;
        //                            }
        //                        }
        //                    }
        //                    break;
        //            }
        //            if (dg?.CurrentCell != null)
        //                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
        //            else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
        //                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
        //        }
        //    }));
        //}

        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void GetWaitHalfProductList()
        {
            try
            {
                Util.gridClear(dgWaitHalfProduct);
                string bizRuleName;
                if (string.Equals(_PROCID, Process.ASSEMBLY))
                {
                    if ("Y".Equals(_IS_SMALL_TYPE) == true)
                    {
                        bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_ASS";
                    }
                    else
                    {
                        bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_AS";
                    }
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_WS";
                }

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));
                indataTable.Columns.Add("WOID", typeof(string));

                if (!string.Equals(_PROCID, Process.ASSEMBLY))
                {
                    indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                }


                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _EQSGID;
                dr["PROCID"] = _PROCID;
                dr["INPUT_LOTID"] = string.IsNullOrEmpty(txtTrayId.Text.Trim()) ? null : txtTrayId.Text.Trim();
                dr["WOID"] = _WORKORDER;
                if (!string.Equals(_PROCID, Process.ASSEMBLY))
                {
                    dr["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                }

                indataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgWaitHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCompleteTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (_EQSGID_YN)
                {
                    if (e.Key == Key.Enter)
                    {
                        if (!ValidationCreateTray()) return;

                        Util.MessageConfirm("SFU1241", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                CreateTray_WNS();

                            }
                        });
                    }

                }
                else
                {

                    if (e.Key == Key.Enter)
                    {
                        if (!ValidationCreateTray()) return;

                        if (_PROCID.Equals(Process.WINDING) && "Y".Equals(_IS_SMALL_TYPE))
                        {
                            GetTrayFormLoad("I");
                        }
                        else
                        {
                            CreateTray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationCreateTray()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(txtCompleteTrayID.Text.Trim()) || txtCompleteTrayID.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675");
                txtCompleteTrayID.SelectAll();
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtCompleteTrayID.Text.ToUpper(), @"^[a-zA-Z0-9]+$");
            if (!chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtCompleteTrayID.Text.ToUpper());
                txtCompleteTrayID.SelectAll();
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        private void CreateTray()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "";

                if ("Y".Equals(_IS_SMALL_TYPE) == true)
                {
                    bizRuleName = "BR_PRD_REG_START_OUT_LOT_WSS_UI";
                }
                else
                {
                    bizRuleName = "BR_PRD_REG_START_OUT_LOT_WS_UI";
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("CALDATE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = _LOTID;
                dr["EQPT_LOTID"] = string.Empty;
                dr["CSTID"] = txtCompleteTrayID.Text.Trim();
                dr["OUTPUT_QTY"] = 0;
                dr["CALDATE"] = dtpCaldate.SelectedDateTime;

                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", "OUT_LOT", (bizResult, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    txtCompleteTrayID.Text = string.Empty;
                    txtCompleteTrayID.Focus();

                    GetLotList(_LOTID);
                    GetSubLot();

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CreateTray_WNS()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string sBizNAme = string.Empty;

                        sBizNAme = "BR_PRD_REG_EQPT_END_OUT_LOT_WN_CST";

                        DataSet inDataSet = new DataSet();
                        DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                        DataRow drow = inEQP.NewRow();

                        inEQP.Columns.Add("SRCTYPE", typeof(string));
                        inEQP.Columns.Add("IFMODE", typeof(string));
                        inEQP.Columns.Add("EQPTID", typeof(string));
                        inEQP.Columns.Add("USERID", typeof(string));
                        inEQP.Columns.Add("PROD_LOTID", typeof(string));
                        inEQP.Columns.Add("EQPT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUTPUT_QTY", typeof(string));
                        inEQP.Columns.Add("CSTID", typeof(string));


                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = _EQPTID;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = _LOTID;
                        drow["EQPT_LOTID"] = _LOTID;
                        drow["OUT_LOTID"] = "";
                        drow["OUTPUT_QTY"] = Convert.ToDecimal(324);
                        drow["CSTID"] = txtCompleteTrayID.Text;

                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            GetSubLot();

                        }, inDataSet);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }


        private void GetTrayFormLoad(string sType)
        {
            try
            {
                CMM_TRAY_CELL_INFO popupTrayCellInfo = new CMM_TRAY_CELL_INFO
                {
                    FrameOperation = FrameOperation
                };

                // SET PARAMETER
                object[] parameters = new object[10];
                parameters[0] = _PROCID;    //procID
                parameters[1] = _EQSGID;    //lineID
                parameters[2] = _EQPTID;    //eqptID
                parameters[3] = _EQPTNAME;  //eqptName
                parameters[4] = _LOTID;     //prod lotID
                parameters[7] = sType;      //trayTag
                parameters[8] = _WORKORDER; //woDetlId
                if ("I".Equals(sType))
                {
                    parameters[5] = string.Empty;   //out lotID
                    parameters[6] = txtCompleteTrayID.Text; //trayID
                    parameters[9] = string.Empty;   //wipqty
                }
                else //U
                {
                    int rowidx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                    parameters[5] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[rowidx].DataItem, "LOTID"));     //out lotID
                    parameters[6] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[rowidx].DataItem, "CSTID"));     //trayID
                    parameters[9] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[rowidx].DataItem, "WIPQTY"));    //wipqty
                }

                C1WindowExtension.SetParameters(popupTrayCellInfo, parameters);

                popupTrayCellInfo.Closed += new EventHandler(TrayCellInfo_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupTrayCellInfo.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TrayCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_TRAY_CELL_INFO popup = sender as CMM_TRAY_CELL_INFO;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtCompleteTrayID.Text = string.Empty;
                txtCompleteTrayID.Focus();

                GetLotList(_LOTID);
                GetSubLot();
            }
            grdMain.Children.Remove(popup);
        }

        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            string messageCode = "SFU1230";

            if (!ValidationTrayDelete()) return;

            try
            {
                if (_EQSGID_YN)
                {
                    if (!ValidationTrayDelete_WNS(dgSubLot))
                        return;


                    if (!string.IsNullOrEmpty(ValidationTrayCellQtyCode()))
                        messageCode = ValidationTrayCellQtyCode();

                    Util.MessageConfirm(messageCode, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteTray_WNS(dgSubLot);
                        }
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(ValidationTrayCellQtyCode()))
                        messageCode = ValidationTrayCellQtyCode();

                    Util.MessageConfirm(messageCode, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (_PROCID.Equals(Process.WINDING) && "Y".Equals(_IS_SMALL_TYPE))
                            {
                                DeleteTrayWNS();
                            }
                            else
                            {
                                DeleteTray();
                            }
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void DeleteTrayWNS()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WNS";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("OUT_LOTID", typeof(string));
                inInput.Columns.Add("TRAYID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["PROD_LOTID"] = _LOTID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                for (int i = 0; i < dgSubLot.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["OUT_LOTID"] = DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "LOTID").GetString();
                        row["TRAYID"] = DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CSTID").GetString();
                        inInput.Rows.Add(row);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetLotList(_LOTID);
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteTray()
        {
            try
            {
                ShowLoadingIndicator();


                const string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS_UI";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("TRAYID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);

                int rowidx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                        {
                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["IFMODE"] = IFMODE.IFMODE_OFF;
                            dr["EQPTID"] = _EQPTID;
                            dr["PROD_LOTID"] = _LOTID;
                            dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                            dr["TRAYID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            dr["WO_DETL_ID"] = txtWorkorder.Text;
                            dr["USERID"] = LoginInfo.USERID;
                            inDataTable.Rows.Add(dr);

                            new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP", null, ds);
                            inDataTable.Rows.Remove(dr);
                        }
                    }
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                txtCompleteTrayID.Text = string.Empty;
                txtCompleteTrayID.Focus();

                GetLotList(_LOTID);
                GetSubLot();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void DeleteTray_WNS(C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WNS";
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("OUT_LOTID", typeof(string));
                inInput.Columns.Add("TRAYID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["PROD_LOTID"] = _LOTID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);


                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["OUT_LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID").GetString();
                        row["TRAYID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "CSTID").GetString();
                        inInput.Rows.Add(row);
                    }
                }
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private bool ValidationTrayDelete_WNS(C1DataGrid dg)
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 삭제할 수 없습니다.
                                Util.MessageValidation("SFU3619");
                                return false;
                            }

                            //// 확정 여부 확인
                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 건은 삭제하실 수 없습니다.
                                Util.MessageValidation("SFU3621");
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private string ValidationTrayCellQtyCode()
        {
            double dCellQty = 0;
            string returnmessageCode = string.Empty;
            foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                {
                    string cellQty = DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetString();
                    if (!string.IsNullOrEmpty(cellQty))
                        double.TryParse(cellQty, out dCellQty);

                    if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                    {
                        return "SFU1320";
                    }
                }
            }

            return returnmessageCode;
        }

        private bool ValidationTrayDelete()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }



        private void btnOutConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_EQSGID_YN)
                {
                    if (!ValidationTrayConfirm_WNS(dgSubLot))
                        return;

                    Util.MessageConfirm("SFU2044", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ConfirmTray_WNS(dgSubLot);
                        }
                    });
                }
                else
                {
                    if (!ValidationTrayConfirm()) return;

                    Util.MessageConfirm("SFU2044", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (_PROCID.Equals(Process.WINDING) && "Y".Equals(_IS_SMALL_TYPE))
                            {
                                ConfirmTrayWNS();
                            }
                            else if (_PROCID.Equals(Process.PACKAGING)) // 자동차 조립 PKG
                            {
                                if (!CanTrayConfirm())
                                    return;

                                TrayConfirmProcess();
                            }
                            else
                            {
                                //2021-10-07 오화백  버튼셀 추가
                                if (GetProduct_Level() == "B")
                                {
                                    ConfirmTray_ButtonCell();
                                }
                                else
                                {
                                    ConfirmTray();
                                }

                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanTrayConfirm()
        {
            bool bRet = false;

            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return bRet;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "WIPQTY")), out dTmp))
            {
                if (dTmp == 0)
                {
                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                    Util.MessageValidation("SFU1685");
                    return bRet;
                }
            }
            else
            {
                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                Util.MessageValidation("SFU1687");
                return bRet;
            }

            string sRet = string.Empty;
            string sMsg = string.Empty;
            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfo(out sRet, out sMsg);
            if (sRet.Equals("NG"))
            {
                Util.MessageValidation(sMsg);
                return bRet;
            }
            else if (sRet.Equals("EXCEPTION"))
                return bRet;

            bRet = true;
            return bRet;
        }

        private void TrayConfirmProcess()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

            if (idx < 0)
                return;

            TrayConfirm();

            GetLotList(_LOTID);
            GetSubLot();
        }

        private void TrayConfirm()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_TRAY_CONFIRM_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "WIPQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] = "";

                inCst.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL_TERM", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetLotList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();

                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        private void GetTrayInfo(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK")].DataItem, "CSTID"));

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        sMsg = "SFU3045";   // TRAY가 미확정 상태가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
        }

        private void ConfirmTrayWNS()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_WNS_UI";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("INLOT");
                inInput.Columns.Add("SEQ", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("PROD_LOTID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                int seq = 1;
                for (int i = 0; i < dgSubLot.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["SEQ"] = seq.GetString();
                        row["CSTID"] = DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CSTID").GetString();
                        row["LOTID"] = DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "LOTID").GetString();
                        row["PROD_LOTID"] = _LOTID;
                        inInput.Rows.Add(row);
                        seq = seq + 1;
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,INLOT", "OUT_EQP", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        //GetLotList(_LOTID);
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationTrayConfirm()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgSubLot, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        private bool ValidationTrayConfirm_WNS(C1DataGrid dg)
        {
            try
            {
                if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgSubLot, "CHK");
                if (rowIndex < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                Util.MessageValidation("SFU3616");
                                return false;
                            }

                            // 확정 여부 확인
                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                Util.MessageValidation("SFU1235");
                                return false;
                            }

                            if (item["LOCATION_NG"].GetString() == "NG")
                            {
                                Util.MessageValidation("SFU3638");
                                return false;
                            }

                            double dTmp;
                            if (double.TryParse(Util.NVC(item["WIPQTY"]), out dTmp))
                            {
                                if (dTmp.Equals(0))
                                {
                                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                                    Util.MessageValidation("SFU1685");
                                    return false;
                                }
                            }
                            else
                            {
                                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                                Util.MessageValidation("SFU1687");
                                return false;
                            }

                            string returnMessage;
                            string messageCode;

                            // Tray 현재 작업중인지 여부 확인.
                            GetTrayInfo(dg, out returnMessage, out messageCode);

                            if (returnMessage.Equals("NG"))
                            {
                                Util.MessageValidation(messageCode);
                                return false;
                            }
                            else if (returnMessage.Equals("EXCEPTION"))
                                return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetTrayInfo(C1DataGrid dg, out string returnMessage, out string messageCode)
        {
            try
            {
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                //const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WNS";
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WN_CST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PR_LOTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("TRAYID", typeof(string));


                DataRow indata = inDataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = _LOTID;
                indata["PROCID"] = _PROCID;
                indata["EQSGID"] = _EQSGID;
                indata["EQPTID"] = _EQPTID;
                indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CSTID"));
                inDataTable.Rows.Add(indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    //if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    if (dtResult.Rows[0]["PROC_TRAY_CNFM_FLAG"].GetString() != "Y")
                    {
                        returnMessage = "OK";
                        messageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        messageCode = "SFU1431";
                    }
                }
                else
                {
                    returnMessage = "NG";
                    //sMsg = "존재하지 않습니다.";
                    messageCode = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnMessage = "EXCEPTION";
                messageCode = ex.Message;
            }
        }

        private void ConfirmTray_WNS(C1DataGrid dg)
        {
            try
            {

                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_WNS";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }


                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmTray()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "";

                if ("Y".Equals(_IS_SMALL_TYPE) == true)
                {
                    bizRuleName = "BR_PRD_REG_END_OUT_LOT_WSS_UI";

                }
                else
                {
                    bizRuleName = "BR_PRD_REG_END_OUT_LOT_WS_UI";
                }

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                inDataTable.Columns.Add("CALDATE", typeof(string));

                DataTable inInputLot = indataSet.Tables.Add("IN_CST");
                inInputLot.Columns.Add("OUT_LOTID", typeof(string));
                inInputLot.Columns.Add("CSTID", typeof(string));
                inInputLot.Columns.Add("OUTPUT_QTY", typeof(int));

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataTable inCstTable = indataSet.Tables["IN_CST"];

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;

                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = _EQPTID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = _LOTID;
                    dr["EQPT_LOTID"] = string.Empty;
                    dr["CALDATE"] = dtpCaldate.SelectedDateTime;
                    indataTable.Rows.Add(dr);

                    DataRow newRow = inCstTable.NewRow();
                    newRow["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                    newRow["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetString()) ? 0 : DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetDecimal();
                    inCstTable.Rows.Add(newRow);

                    //string xmlText = indataSet.GetXml();
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_CST", null, indataSet);
                    indataTable.Rows.Remove(dr);
                    inCstTable.Rows.Remove(newRow);
                }

                HiddenLoadingIndicator();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                //GetLotList(_LOTID);
                GetSubLot();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnOutCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellChange()) return;

            int rowidx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

            if (_PROCID.Equals(Process.WINDING) && "Y".Equals(_IS_SMALL_TYPE))
            {
                string dispatchYn = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[rowidx].DataItem, "DISPATCH_YN"));
                if ("Y".Equals(dispatchYn))
                {
                    GetTrayFormLoad("Y");   //확정후 조회
                }
                else
                {
                    GetTrayFormLoad("U");   //업데이트
                }
            }
            else if (string.Equals(_CELL_MNGT_TYPE_CODE, "C")) // 초소형
            {
                // 마감이후 2018.10월 초 이후 반영 예정
                //CellByPositionC();

                if (_IS_SMALL_TYPE == "Y")
                {
                    CellByPositionC();
                }
                else
                {
                    WashingCellManagement();
                }
            }
            else if (string.Equals(_CELL_MNGT_TYPE_CODE, "P")) // 위치관리
            {
                // 위치관리
                CellByPositionP();
            }
            else if (_PROCID.Equals(Process.PACKAGING)) // 자동차 조립 PKG
            {
                ChangeCellInfo();
            }
        }

        private void ChangeCellInfo()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

            if (idx < 0)
                return;

            string sTrayQty = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CST_CELL_QTY"));//cboTrayType.SelectedValue == null ? "25" : cboTrayType.SelectedValue.ToString();            
            string trayID = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID")).Replace("\0", "");
            string outLOTID = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));

            ASSY001_007_CELL_LIST wndCellList = new ASSY001_007_CELL_LIST();
            wndCellList.FrameOperation = FrameOperation;

            if (wndCellList != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "WIPSEQ"));
                Parameters[4] = trayID;
                Parameters[5] = sTrayQty;
                Parameters[6] = outLOTID;
                Parameters[7] = false;  // View Mode. (Read Only)
                Parameters[8] = "TERM_CELL";

                C1WindowExtension.SetParameters(wndCellList, Parameters);

                wndCellList.Closed += new EventHandler(wndCellList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                //grdMain.Children.Add(wndCellList);
                //wndCellList.BringToFront();
            }
        }

        private void wndCellList_Closed(object sender, EventArgs e)
        {
            ASSY001_007_CELL_LIST window = sender as ASSY001_007_CELL_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetLotList(_LOTID);
            GetSubLot();

            this.grdMain.Children.Remove(window);
        }

        private void CellByPositionC()
        {
            CMM_WASHING_CSH_CELL_INFO popCellInfo = new CMM_WASHING_CSH_CELL_INFO { FrameOperation = FrameOperation };
            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

            object[] parameters = new object[7];
            parameters[0] = _LOTID;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));
            parameters[3] = _EQPTID;

            if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) != "WAIT")
            {
                parameters[4] = "R";
            }
            else
            {
                parameters[4] = "W";
            }
            parameters[5] = "Y";
            //2021-10-07 오화백  버튼셀 여부 추가
            parameters[6] = GetProduct_Level();


            C1WindowExtension.SetParameters(popCellInfo, parameters);
            popCellInfo.Closed += new EventHandler(popCellInfo_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popCellInfo.ShowModal()));
        }

        private void popCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_WASHING_CSH_CELL_INFO pop = sender as CMM_WASHING_CSH_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetLotList(_LOTID);
                GetSubLot();
            }

            grdMain.Children.Remove(pop);
        }

        private void WashingCellManagement()
        {
            if (!ValidationCellChange()) return;

            if(_AREAID == "MC") //ESOC2 9동 NFF 소형 조립 Washing Cell 관리 팝업
            {
                COM001_072_WASHING_CELL_MANAGEMENT popCellManagement = new COM001_072_WASHING_CELL_MANAGEMENT { FrameOperation = FrameOperation };
                //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                object[] parameters = new object[9];
                parameters[0] = _LOTID;
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID"));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));
                parameters[3] = _EQPTID;
                //상태가 미확정일 경우에만 저장/삭제가 가능하다 - 그 외 나머지 상태는 조회만 가능
                if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) != "WAIT")
                {
                    //Read 모드
                    parameters[4] = "R";
                }
                else
                {   //Write 모드
                    parameters[4] = "W";
                }
                parameters[5] = "N";    //completeProd 여부
                parameters[6] = LoginInfo.USERID; //UcAssyShift.TextWorker.Tag;     // 작업자 ID
                parameters[7] = idx > 0 ? Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx - 1].DataItem, "CSTID")) : "";
                parameters[8] = idx < dgSubLot.GetRowCount() - 1 ? Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx + 1].DataItem, "CSTID")) : "";

                C1WindowExtension.SetParameters(popCellManagement, parameters);
                popCellManagement.Closed += popCellManagement_Closed;

                Dispatcher.BeginInvoke(new Action(() => popCellManagement.ShowModal()));
            }
            else
            {
                CMM_WASHING_CELL_MANAGEMENT popCellManagement = new CMM_WASHING_CELL_MANAGEMENT { FrameOperation = FrameOperation };
                int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                object[] parameters = new object[7];
                parameters[0] = _LOTID;
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID"));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));
                parameters[3] = _EQPTID;
                //상태가 미확정일 경우에만 저장/삭제가 가능하다 - 그 외 나머지 상태는 조회만 가능
                if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) != "WAIT")
                {
                    //Read 모드
                    parameters[4] = "R";
                }
                else
                {   //Write 모드
                    parameters[4] = "W";
                }
                parameters[5] = "Y"; //completeProd 여부
                parameters[6] = LoginInfo.USERID;     // 작업자 ID

                C1WindowExtension.SetParameters(popCellManagement, parameters);
                popCellManagement.Closed += popCellManagement_Closed;

                //Dispatcher.BeginInvoke(new Action(() => popCellPosition.ShowModal()));
                grdMain.Children.Add(popCellManagement);
                popCellManagement.BringToFront();
            }
 
        }

        private void popCellManagement_Closed(object sender, EventArgs e)
        {
            CMM_WASHING_CELL_MANAGEMENT pop = sender as CMM_WASHING_CELL_MANAGEMENT;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetLotList(_LOTID);
                GetSubLot();
            }
            grdMain.Children.Remove(pop);
        }

        private void CellByPositionP()
        {
            CMM_WASHING_WG_CELL_INFO popCellPosition = new CMM_WASHING_WG_CELL_INFO { FrameOperation = FrameOperation };
            int idx = _util.GetDataGridFirstRowIndexByCheck(dgSubLot, "CHK");

            object[] parameters = new object[10];
            parameters[0] = _PROCID;
            parameters[1] = _EQSGID;
            parameters[2] = _EQPTID;
            parameters[3] = _EQPTNAME;
            parameters[4] = _LOTID;
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "LOTID"));
            parameters[6] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CSTID"));

            if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) == "WAIT")
            {
                parameters[7] = "U";
            }
            else
            {
                parameters[7] = "R";
            }

            parameters[8] = txtWorkorder.Text;
            parameters[9] = "Y"; //생산LOT 확정 후 수정 여부

            C1WindowExtension.SetParameters(popCellPosition, parameters);
            popCellPosition.Closed += popCellPosition_Closed;

            grdMain.Children.Add(popCellPosition);
            popCellPosition.BringToFront();
        }

        private void popCellPosition_Closed(object sender, EventArgs e)
        {
            CMM_WASHING_WG_CELL_INFO pop = sender as CMM_WASHING_WG_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetLotList(_LOTID);
                GetSubLot();
            }
        }

        private bool ValidationCellChange()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            if (_util.GetDataGridCheckCnt(dgSubLot, "CHK") > 1)
            {
                Util.MessageValidation("SFU3719", ObjectDic.Instance.GetObjectName("CELL관리"));
                return false;
            }
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_EQSGID_YN)
                {
                    if (!ValidationConfirmCancel_WNS(dgSubLot))
                        return;


                    Util.MessageConfirm("SFU1243", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ConfirmTrayCancel_WNS(dgSubLot);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private bool ValidationConfirmCancel_WNS(C1DataGrid dg)
        {
            try
            {

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return false;
                }


                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 확정취소 할 수 없습니다.
                                Util.MessageValidation("SFU3617");
                                return false;
                            }

                            // 확정 여부 확인
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정 상태만 확정 취소할 수 있습니다.
                                Util.MessageValidation("SFU3618");
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private void ConfirmTrayCancel_WNS(C1DataGrid dg)
        {
            try
            {
                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_CANCEL_WNS";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_EQSGID_YN)
                {
                    if (!ValidationDischarge_WNS(dgSubLot))
                        return;
                    //배출 하시겠습니까?
                    Util.MessageConfirm("SFU3613", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DischargeTray_WNS(dgSubLot);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationDischarge_WNS(C1DataGrid dg)
        {
            try
            {
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 Tray만 배출가능 합니다.
                                Util.MessageValidation("SFU3614");
                                return false;
                            }

                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건이 존재합니다.
                                Util.MessageValidation("SFU3620");
                                return false;
                            }

                            if (!string.Equals(Util.NVC(item["LOCATION_NG"]).GetString(), "OK"))
                            {
                                //투입위치 정보를 확인 하세요.
                                Util.MessageValidation("SFU1980");
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void DischargeTray_WNS(C1DataGrid dg)
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_CONFIRM_TRAY_WN_CST";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("INLOT");
                inInput.Columns.Add("SEQ", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                inInput.Columns.Add("PROD_LOTID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                int seq = 1;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["SEQ"] = seq.GetString();
                        row["CSTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "CSTID").GetString();
                        row["PROD_LOTID"] = _LOTID;
                        inInput.Rows.Add(row);
                        seq = seq + 1;
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,INLOT", "OUT_EQP", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnQtySave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_EQSGID_YN)
                {
                    if (!ValidationQtySave_WNS())
                        return;

                    //저장하시겠습니까?
                    Util.MessageConfirm("SFU1241", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveTray_Qty();
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveTray_Qty()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WN_CST";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                for (int i = 0; i < dgSubLot.Rows.Count - dgSubLot.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgSubLot, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EQPTID;
                    newRow["USERID"] = LoginInfo.USERID;

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CSTID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "WIPQTY")));

                    inDataTable.Rows.Add(newRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_DATA", null, indataSet);
                    inDataTable.Rows.Remove(newRow);

                }
                //Util.Alert("정상처리 되었습니다.");
                Util.MessageInfo("SFU1275");

                GetSubLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool ValidationQtySave_WNS()
        {

            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }



        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTraySave()) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTray();
                }
            });
        }

        private bool ValidationTraySave()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        private void SaveTray()
        {
            try
            {
                ShowLoadingIndicator();

                dgSubLot.EndEdit();

                const string bizRuleName = "BR_PRD_REG_UPD_OUT_LOT_WS_UI";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("OUT_LOTID", typeof(string));
                inLot.Columns.Add("CSTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(int));

                DataTable inSpcl = indataSet.Tables.Add("IN_SPCL");
                inSpcl.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                inSpcl.Columns.Add("SPCL_CST_NOTE", typeof(string));
                inSpcl.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                for (int i = 0; i < dgSubLot.Rows.Count - dgSubLot.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgSubLot, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EQPTID;
                    newRow["PROD_LOTID"] = _LOTID;
                    newRow["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(newRow);

                    // Tray 정보 DataTable             
                    DataRow dr = inLot.NewRow();
                    dr["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "LOTID"));
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "CSTID"));
                    dr["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[i].DataItem, "WIPQTY")));
                    inLot.Rows.Add(dr);

                    DataRow dataRow = inSpcl.NewRow();
                    dataRow["SPCL_CST_GNRT_FLAG"] = null;
                    dataRow["SPCL_CST_NOTE"] = null;
                    dataRow["SPCL_CST_RSNCODE"] = null;
                    inSpcl.Rows.Add(dataRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT,IN_SPCL", null, indataSet);
                    inDataTable.Rows.Remove(newRow);
                    inLot.Rows.Remove(dr);
                }

                HiddenLoadingIndicator();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                GetLotList(_LOTID);
                GetSubLot();


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (!"N".Equals(_CELL_MNGT_TYPE_CODE))
                {
                    btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = false;
                }
                else
                {
                    btnOutCell.IsEnabled = false;
                    btnOutSave.IsEnabled = true;
                }

                if (dgRow != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutDel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutCell.IsEnabled = true;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutSave.IsEnabled = false;
                        //btnOutConfirmCancel.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutDel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                        //btnOutConfirmCancel.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN")) // 활성화입고
                    {
                        btnOutDel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                        //btnOutConfirmCancel.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "WIPSTAT_CODE")).Equals("EQPT_END"))
                    {
                        btnQtySave.IsEnabled = true;
                    }
                    else
                    {
                        btnQtySave.IsEnabled = false;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutCell.IsEnabled = true;
                        if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutSave.IsEnabled = false;
                        //btnOutConfirmCancel.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutDel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutCell.IsEnabled = true;
                    if (!"N".Equals(_CELL_MNGT_TYPE_CODE)) btnOutSave.IsEnabled = false;
                    //btnOutConfirmCancel.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSubLot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgSubLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int idx = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CHK").GetString();

                for (int i = 0; i < dgSubLot.GetRowCount(); i++)
                {
                    if (checkFlag == "0")
                    {
                        DataTableConverter.SetValue(dgSubLot.Rows[i].DataItem, "CHK", idx == i);

                    }
                    else
                    {
                        DataTableConverter.SetValue(dgSubLot.Rows[i].DataItem, "CHK", false);
                    }
                }
                SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
            }

        }

        private void btnOutConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirmCancel()) return;

            try
            {
                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmCancelTray();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmCancelTray()
        {
            try
            {
                ShowLoadingIndicator();

                // 원각/ 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS_UI";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable InCSTTable = indataSet.Tables.Add("IN_CST");
                InCSTTable.Columns.Add("OUT_LOTID", typeof(string));
                InCSTTable.Columns.Add("CSTID", typeof(string));

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["USERID"] = LoginInfo.USERID;
                indataTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                DataTable inCstTable = indataSet.Tables["IN_CST"];
                newRow = inCstTable.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iRow].DataItem, "LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iRow].DataItem, "CSTID"));
                inCstTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null, (searchResult, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetLotList(_LOTID);
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationTrayConfirmCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        /// <summary>
        /// FCS 전송 버튼 이벤트 처리
        /// [C20180718_42535] 와싱 Tray 실적 수정시 FCS로 data 전송 기능 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrayToFCS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgSubLot, "CHK");
                if (rowIndex < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_END_OUT_LOT_TO_FCS_WS";


                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));

                DataTable indataTable = indataSet.Tables["INDATA"];

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;
                    if (DataTableConverter.GetValue(row.DataItem, "FORM_MOVE_STAT_CODE").GetString().Equals("WAIT"))
                    {
                        Util.MessageInfo("SFU4995");
                        HiddenLoadingIndicator();
                        return;
                    }
                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["EQPTID"] = _EQPTID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = _LOTID;
                    dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();

                    indataTable.Rows.Add(dr);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                    indataTable.Rows.Remove(dr);
                }

                HiddenLoadingIndicator();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                //GetLotList(_LOTID);
                GetSubLot();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// C20180730_53252 LOT 삭제 버튼 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLotDeleted_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgLotList, "CHK");

                if (rowIndex < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                string sLotID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "LOTID"));
                string sOutLotID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "WINDING_RUNCARD_ID"));
                string sErpClose = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "ERP_CLOSE"));
                string sHold = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "WIPHOLD"));
                double sCnfmPrdtReqQty = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CNFM_PRDT_REQ_QTY")).Equals("") ? 0 : Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CNFM_PRDT_REQ_QTY")).Replace(",", ""));

                if (sErpClose.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return;
                }

                if (sHold.Equals("Y"))
                {
                    // HOLD 된 LOT ID 입니다.
                    Util.MessageValidation("SFU1340");
                    return;
                }

                if (sCnfmPrdtReqQty != 0)
                {
                    // 물품청구가 있는 LOT 은 삭제할 수 없습니다.
                    Util.MessageValidation("SFU5014");
                    return;
                }

                //삭제 하시겠습니까?
                Util.MessageConfirm("SFU1230", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DoLotDeleted();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DoLotDeleted()
        {
            try
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgLotList, "CHK");

                string sLotID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "LOTID"));
                string sOutLotID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "WINDING_RUNCARD_ID"));
                string sErpClose = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "ERP_CLOSE"));
                string sHold = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "WIPHOLD"));
                double sCnfmPrdtReqQty = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CNFM_PRDT_REQ_QTY")).Equals("") ? 0 : Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[rowIndex].DataItem, "CNFM_PRDT_REQ_QTY")).Replace(",", ""));

                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_DELETE_PROD_OUT_LOT_WN";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");

                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                //inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                //inDataTable.Columns.Add("NOTE", typeof(string));

                DataTable indataTable = indataSet.Tables["INDATA"];

                DataRow dr = indataTable.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                //dr["NOTE"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = sLotID;
                //dr["OUT_LOTID"] = sOutLotID;

                indataTable.Rows.Add(dr);

                //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        btnSearch_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetCellManagementInfo()
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_MNGT_TYPE_CODE";
            inTable.Rows.Add(dr);
            _dtCellManagement = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", inTable);

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            DataRow dataRow = inDataTable.NewRow();
            dataRow["LANGID"] = LoginInfo.LANGID;
            dataRow["CMCDTYPE"] = "CMCDTYPE";
            dataRow["CMCODE"] = "CELL_MNGT_TYPE_CODE";
            inDataTable.Rows.Add(dataRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
                _cellManageGroup = dtResult.Rows[0]["CMCDNAME"].GetString();
        }

        private void DisplayCellManagementType(C1DataGrid dg)
        {

            if (!CommonVerify.HasDataGridRow(dg) || !CommonVerify.HasTableRow(_dtCellManagement))
            {
                tbCellManagement.Text = string.Empty;
            }
            else
            {
                string cellType = string.Empty;

                var query = (from t in _dtCellManagement.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == _CELL_MNGT_TYPE_CODE
                             select new { cellType = t.Field<string>("CBO_NAME") }).FirstOrDefault();

                if (query != null)
                    cellType = query.cellType;

                tbCellManagement.Text = "[" + _cellManageGroup + "  : " + cellType + "]";
            }
        }

        private void btnEqptCondSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEqptCondSave())
                return;

            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetEqptCond();
                }
            });
        }

        private void SetEqptCond()
        {
            try
            {
                ShowLoadingIndicator();

                dgEqptCond.EndEdit();

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = indataSet.Tables["IN_EQP"];

                    DataRow newRow = null;
                    DataTable in_Data = indataSet.Tables["IN_DATA"];

                    // Biz Core Multi 처리 없으므로 임시로 Unit 단위로 비즈 호출 처리 함.
                    // 추후 Multi Biz 생성 시 처리 방법 변경 필요.
                    string sUnitID = "";
                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        string sTmp = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "UNIT_EQPTID"));

                        if (i == 0)
                        {
                            sUnitID = sTmp;

                            newRow = null;

                            newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            newRow["EQPTID"] = _EQPTID;
                            newRow["UNIT_EQPTID"] = sUnitID;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["LOTID"] = _LOTID;
                            newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                            inTable.Rows.Add(newRow);

                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);
                        }
                        else
                        {
                            if (sUnitID.Equals(sTmp))
                            {
                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                            else
                            {
                                // data 존재 시 biz call
                                if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                                {
                                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                                    inTable.Rows.Clear();
                                    in_Data.Rows.Clear();
                                }

                                sUnitID = sTmp;

                                newRow = null;

                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = _EQPTID;
                                newRow["UNIT_EQPTID"] = sUnitID;
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["LOTID"] = _LOTID;
                                newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                                inTable.Rows.Add(newRow);

                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                        }
                    }

                    // 마지막 Unit 처리.
                    if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                    }
                }
                else
                {
                    DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = indataSet.Tables["IN_EQP"];

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EQPTID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = _LOTID;
                    newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                    inTable.Rows.Add(newRow);

                    DataTable in_Data = indataSet.Tables["IN_DATA"];

                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        newRow = null;

                        newRow = in_Data.NewRow();
                        newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                        newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                        in_Data.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                    inTable.Rows.Clear();
                    in_Data.Rows.Clear();

                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                }

                GetEqptCond();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetEqptCond()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("BEFORE_LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["EQSGID"] = _EQSGID;
                newRow["BEFORE_LOTID"] = "TEMP_LOT"; // NULL 또는 공백으로 BIZ 호출 시 TIMEOUT 으로 인해 임의 값 설정.

                inDataTable.Rows.Add(newRow);

                string BizName = "DA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgEqptCond, dtRslt, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CanEqptCondSave()
        {
            bool bRet = false;

            if (dgEqptCond.ItemsSource == null || dgEqptCond.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1651");      //선택된 항목이 없습니다.
                return bRet;
            }

            if (_LOTID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        //2019.03.27  강호운 Package 공정 바구니 투입취소 기능 추가[CSR ID:3946580] Pilot 3호 PKG 실적수정 요청 건 | [요청번호] C20190312_46580 | [서비스번호]3946580 
        private void btnInputLotCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return;
            }

            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputBox, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return;
            }

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(_PROCID, Process.ASSEMBLY) || string.Equals(_PROCID, Process.WASHING))
                    {
                        InputHalfProductCancelAssembly();
                    }
                    else
                    {
                        InputHalfProductCancel(dgInputBox);
                    }
                }
                GetLotList(_LOTID);
                GetInBox(_LOTID, Convert.ToInt16(_WIPSEQ));
            });
        }
        //2019.03.27  강호운 Package 공정 바구니 투입취소 기능 추가 [CSR ID:3946580] Pilot 3호 PKG 실적수정 요청 건 | [요청번호]C20190312_46580 | [서비스번호]3946580 
        private void InputHalfProductCancel(C1DataGrid grid)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT_CL";

                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET2_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROCID"] = _PROCID;

                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < grid.Rows.Count - grid.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(grid, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_QTY")));
                    // 최상민 2019-05-14
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "CSTID"));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetHalfProductList();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void SetInputHistButtonControls(string sProcID)
        {
            try
            {
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("ATTRIBUTE1", typeof(string));
                dt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_LOT_CANCEL_TERM_USE";
                dr["ATTRIBUTE1"] = Util.NVC(cboArea.SelectedValue);
                dr["ATTRIBUTE2"] = sProcID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", dt);
                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE4"]).Trim().Equals("Y"))
                {
                    btnInputBox.Visibility = Visibility.Visible;                    // 투입바구니 Tab : 바구니투입
                    btnInputLotCancel.Visibility = Visibility.Visible;              // 투입바구니 Tab : 투입취소

                    btnInHalfProductInPutQty.Visibility = Visibility.Visible;       // 투입반제품이력 Tab : 투입량수정                    
                    btnInHalfProductInPutCancel.Visibility = Visibility.Visible;    // 투입반제품이력 Tab : 투입취소

                    btnInputHalfWindingCancel.Visibility = Visibility.Visible;      // 전극/자재투입이력 Tab : 투입취소

                    btnWaitHalfProductInput.Visibility = Visibility.Visible;        // 대기반제품 Tab : 투입

                    btnWaitPancakeInPut.Visibility = Visibility.Visible;            // 대기Pancake Tab : 투입
                }
                else
                {
                    btnInputBox.Visibility = Visibility.Collapsed;                    // 투입바구니 Tab : 바구니투입
                    btnInputLotCancel.Visibility = Visibility.Collapsed;              // 투입바구니 Tab : 투입취소

                    btnInHalfProductInPutQty.Visibility = Visibility.Collapsed;       // 투입반제품이력 Tab : 투입량수정                    
                    btnInHalfProductInPutCancel.Visibility = Visibility.Collapsed;    // 투입반제품이력 Tab : 투입취소

                    btnInputHalfWindingCancel.Visibility = Visibility.Collapsed;      // 전극/자재투입이력 Tab : 투입취소

                    btnWaitHalfProductInput.Visibility = Visibility.Collapsed;        // 대기반제품 Tab : 투입

                    btnWaitPancakeInPut.Visibility = Visibility.Collapsed;            // 대기Pancake Tab : 투입
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //2021-10-07 오화백 버튼셀 여부 체크 
        private string GetProduct_Level()
        {
            string confirmDate;

            const string bizRuleName = "DA_BAS_SEL_VW_PRODUCT_MODEL_INFO";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PRODID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["PRODID"] = _PRODID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (searchResult.Rows.Count > 0)
            {
                confirmDate = searchResult.Rows[0]["PRODUCT_LEVEL3_CODE"].ToString();
            }
            else
            {
                confirmDate = string.Empty;
            }

            return confirmDate;
        }
        //2021-10-07 오화백 버튼셀 여부 체크 
        private void ConfirmTray_ButtonCell()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_EQPT_END_OUT_LOT_WSB";

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(int));

                DataTable indataTable = indataSet.Tables["IN_EQP"];

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;

                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = _EQPTID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = _LOTID;
                    dr["EQPT_LOTID"] = string.Empty;
                    dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                    dr["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetString()) ? 0 : DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetDecimal();
                    indataTable.Rows.Add(dr);
                    //string xmlText = indataSet.GetXml();
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP", null, indataSet);
                    indataTable.Rows.Remove(dr);
                }

                HiddenLoadingIndicator();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                //GetLotList(_LOTID);
                GetSubLot();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }

        private string GetLotUnitCode()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_VW_LOT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _LOTID;
                inTable.Rows.Add(dr);
                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    return result.Rows[0]["LOTUID"].GetString();
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }
        private void btnSaveElectrodeGradeInfo_Click(object sender, RoutedEventArgs e)
        {
            SaveElectrodeGradeInfo(dgElectrodeGradeInfo);
        }
        private void SaveElectrodeGradeInfo(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                dg.EndEdit(true);
                ShowLoadingIndicator();

                DataSet inCollectLot = dtDataCollect_Grd_Judg(dg);

                if (inCollectLot.Tables["RQSTDT"].Rows.Count == 0)
                {
                    Util.MessageInfo("SFU9333");//전극 등급 판정값이 없습니다.
                    HiddenLoadingIndicator();
                    return;
                }

                foreach (DataRow dr in inCollectLot.Tables["RQSTDT"].Rows)
                {
                    if (string.IsNullOrEmpty(dr["GRD_JUDG_CODE"].ToString()))
                    {
                        Util.MessageInfo("SFU9333");//전극 등급 판정값이 없습니다.
                        HiddenLoadingIndicator();
                        return;
                    }
                }

                new ClientProxy().ExecuteService_Multi("DA_PRD_REG_GRD_JUDG_RM_NFF_MANUAL", "RQSTDT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (!bAllSave)
                        Util.MessageInfo("SFU9334");     // 품질 정보가 저장되었습니다.

                }, inCollectLot);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        private DataSet dtDataCollect_Grd_Judg(C1DataGrid dg)
        {
            DataSet inDataSet = new DataSet();

            DataTable IndataTable = inDataSet.Tables.Add("RQSTDT");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(decimal));
            IndataTable.Columns.Add("GRD_JUDG_CLSS_CODE", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            //IndataTable.Columns.Add("GRD_AVG_VALUE", typeof(decimal));
            IndataTable.Columns.Add("GRD_JUDG_CODE", typeof(string));
            IndataTable.Columns.Add("GRD_BAS_TYPE", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;

            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("GRD_JUDG_CLSS_CODE") != null
                         select new { GRD_JUDG_CLSS_CODE = t.Field<string>("GRD_JUDG_CLSS_CODE"), GRD_JUDG_CODE = t.Field<string>("GRD_JUDG_CODE") }).Distinct();

            foreach (var a in query.Reverse())
            {
                DataRow inDataRow = null;
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = _LOTID;
                inDataRow["WIPSEQ"] = _WIPSEQ;
                //inDataRow["GRD_AVG_VALUE"] = a.GRD_AVG_VALUE;
                inDataRow["GRD_JUDG_CODE"] = a.GRD_JUDG_CODE;
                inDataRow["GRD_JUDG_CLSS_CODE"] = a.GRD_JUDG_CLSS_CODE;
                inDataRow["PROCID"] = _PROCID;
                inDataRow["GRD_BAS_TYPE"] = "PLM";
                inDataRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(inDataRow);
            }

            dg.UpdateLayout();
            return inDataSet;
        }
        private void dgElectrodeGradeInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (dataGrid.GetRowCount() > 0)
                    {
                        foreach (C1.WPF.DataGrid.DataGridRow r in dataGrid.Rows)
                        {
                            string JudgColumn = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[r.Index].DataItem, "GRD_JUDG_CODE"));

                            if (JudgColumn.Equals(""))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(243, 165, 165));
                            }
                        }
                    }


                }
            }));
        }
        private bool IsElectrodeGradeInfo() //슬리팅에서만 사용하기 위해 동별공통코드로 변경
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "GRD_JUDG_DISP_AREA";
                dr["COM_CODE"] = cboProcess.SelectedValue.ToString();   //E4000
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    btnSaveElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                    tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                    dgElectrodeGradeInfo.IsReadOnly = true;
                    return false;
                }

                foreach (DataRow drow in dtResult.Rows)
                {
                    if (drow["ATTR1"].Equals("SAVE_CHECK"))
                    {
                        btnSaveElectrodeGradeInfo.Visibility = Visibility.Visible;
                        dgElectrodeGradeInfo.IsReadOnly = false;
                    }
                    else
                    {
                        btnSaveElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                        dgElectrodeGradeInfo.IsReadOnly = true;
                    }
                }

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }
        private void dgElectrodeGradeInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        gradejudgmentCode = x.Field<string>("GRD_JUDG_CLSS_CODE")
                    }).Select(g => new
                    {
                        GradejudgmentCode = g.Key.gradejudgmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewGradejudgmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {

                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString() == item.GradejudgmentCode && previewGradejudgmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString())
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["GRD_AVG_VALUE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["GRD_AVG_VALUE"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["GRD_JUDG_CODE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["GRD_JUDG_CODE"].Index)));
                            }
                        }

                        previewGradejudgmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectElectrodeGradeInfo(string lotId, string wipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["WIPSEQ"] = wipseq; 
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_GRD_JUDG_LOT", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {

                        Util.GridSetData(dgElectrodeGradeInfo, bizResult, null, true);

                        for (int index = 0; index < bizResult.Rows.Count; index++)
                        {
                            C1.WPF.DataGrid.DataGridColumn JudgColumn = dgElectrodeGradeInfo.Rows[index].DataGrid.Columns["GRD_JUDG_CODE"] as C1.WPF.DataGrid.DataGridColumn;
                            string JudgCode = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "GRD_JUDG_CODE"));
                            string UserId = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "USERID"));


                            JudgColumn.IsReadOnly = false;
                            IsElectrodeGradeInfo();

                        }
                    }
                    else
                    {
                        new ClientProxy().ExecuteService("DA_PRD_SEL_GRD_JUDG_LOT_LANE_RESULT_OUT", "RQSTDT", "RSLTDT", inTable, (bizResult2, bizException2) =>
                        {
                            if (bizException2 != null)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(bizException2);
                                return;
                            }
                            Util.GridSetData(dgElectrodeGradeInfo, bizResult2, null, true);

                            for (int index = 0; index < bizResult2.Rows.Count; index++)
                            {
                                C1.WPF.DataGrid.DataGridComboBoxColumn JudgColumn = dgElectrodeGradeInfo.Rows[index].DataGrid.Columns["GRD_JUDG_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                                string JudgCode = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "GRD_JUDG_CODE"));
                                string UserId = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "USERID"));

                               
                                IsElectrodeGradeInfo();
                                  
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        private void SetGridGrdCombo(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MATC_WINDING";
            dr["ATTRIBUTE1"] = "MATC_GRD";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);


            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

        }

        // [E20231221-001759] - GM1 특정 스토커 적재된 HOLD LOT 실적 저장 허용
        private string GetRackID(string lotid)
        {
            try
            {
                string rackid = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = lotid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_RACK_ID_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    rackid = Util.NVC((dtResult.Rows[0]["WH_ID"]));
                }
                return rackid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        // E20230901-001504
        #region WINDING 공정의 대차ID 항목 표기여부
        private bool SetAssyCSTIDView()
        {
            if (String.IsNullOrEmpty(Convert.ToString(cboEquipmentSegment.SelectedValue)))
            {
                return false;
            }

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "ASSY_CSTID_VIEW";
            dr["CBO_CODE"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }
        #endregion

        private void btnCSTIDSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationSTIDSave())
                SaveCSTID();
            else
            {
                Util.MessageValidation("SFU3552");  //저장 할 DATA가 없습니다.
                return;
            }
        }

        private bool ValidationSTIDSave()
        {
            if (txtCSTID.IsNullOrEmpty())
                return false;

            return true;
        }

        //ESMI-A4동 6 Line 제외처리
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_EXCEPT_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

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

        #region 대차ID 매핑
        private void SaveCSTID()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("SRCTYPE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = Util.NVC(txtSelectLot.Text);
            newRow["CSTID"] = Util.NVC(txtCSTID.Text);
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

            inTable.Rows.Add(newRow);


            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                        }
                    });
                }
            });
        }
        #endregion       
    }
}