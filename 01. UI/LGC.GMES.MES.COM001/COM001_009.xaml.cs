/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.03.13  정문교C   : 대상 LOT 조회 수정
  2017.11.21  INS염규범   GEMS FOLDING 실적 마감 단어 수정 요청 CSR20171011_02178  
  2019.02.08  고현영 : 슬리팅 TAG수량 변경하도록 수정
  2019.07.16  김동일      폴란드 1,2 동 라미 공정진척 3LOSS 적용에 따른 수정
  2020.06.26  김동일      C20200625-000271 기준정보를 통한 투입관련 Tab의 투입 및 투입취소버튼 제어기능 추가
  2021.06.23  이상훈      C20210623-000317 공정별 생산실적 수정(조립) 화면 조회시 다국어 처리 
  2021.07.13  김지은      [GM JV Proj.]조회조건-생산구분 추가
  2022.01.05  정재홍      [C20210810-000354] 코터,롤프레스 완공 상태에서 실적수정 불가하도록 기준정보 추가
  2022.03.04  윤세진      [원통형 9, 10호] Assy 공정 투입 취소 시 투입량 Para 추가
  2023.01.17  윤기업      롤맵 수불 적용
  2023.01.19  정재홍    : CSR : C20221212-000194 - NG TAG Abnormal 
  2023.01.25  윤기업      롤맵 수불인 경우 양품량 수정 방지 - 02-26 양품량 수정 가능하게 변경.. 
  2023.02.21  윤기업      롤맵 수불 조회모드 추가에 따른 PARA 추가
  2023.02.27  윤기업    : 롤맵 실적 - 양품량 변경 및 생산량 고정, 양품수량 직접수정시 생산량 수정반영
  2023.07.26  이호섭    : E20230726-000790 LANE_PTN_QTY 수량 반영 부분 전극 제외 처리
  2023.09.06  주동석   : 모든 경우에 다음공정 투입시 수정 못하도록 Blocking 하도록 수정
  2023.12.07  김태우      전극 판정등급 수정 기능 추가.
  2024.04.08  김용군      E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
  2024.08.27  조성근    : [E20240827-000372] - 롤프레스 롤맵 실적 수정팝업 추가
  2025.02.03  백상우    : [MES2.0] ROLL PRESS공정 ROLLMAP 도입으로 색지정보 탭 사용안함, 탭 숨김처리
  2025.03.24  이민형    : [HD_OSS_0134]체크 없이 데이터 수정 시 에러 발생, 에러 발생 메시지도 보완.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_009 : UserControl, IWorkArea
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

        // Folding & Stacking 구분자
        string _EQGRID = string.Empty;

        double _dInQty = 0;
        double _dOutQty = 0;
        double _dprOutQty = 0;
        double _dEqptQty = 0;
        double _dEqptOrgQty = 0;
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

        // Roll Map
        private bool _isOriginRollMapEquipment = false;
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        string _EQPTNAME = string.Empty;
        private bool _isRollMapLot;
        private string sBeforeValue = string.Empty;

        //투입위치
        private string _sBeforeMOUNT_PSTN_NAME = string.Empty;

        public COM001_009()
        {
            InitializeComponent();

            InitCombo();

            GetAreaType(cboProcess.SelectedValue.ToString());
            AreaCheck(cboProcess.SelectedValue.ToString());
            SetProcessNumFormat(cboProcess.SelectedValue.ToString());

            IsElectrodeGradeInfo();
        }

        public C1DataGrid LOTINFO_GRID
        {
            get { return dgLotInfo; }
            set { dgLotInfo = value; }
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
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "ESMI_A4_EXCEPT_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged; //공정변할때 lane_qty, coating ver 보여주기 관리
            ////CboProcess_SelectedItemChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            // 2021.07.13 : 생산구분 추가
            // 생산구분
            string[] sFilterProdDiv = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterProdDiv);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

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

            dtpCaldate.SelectedDataTimeChanged += dtpCaldate_SelectedDataTimeChanged;

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
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
                Parameters[10] = "COM001_009"; //호출화면

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

            object[] parameters = new object[10];
            parameters[0] = _PROCID;
            parameters[1] = _EQSGID;
            parameters[2] = _EQPTID;
            parameters[3] = _LOTID;
            parameters[4] = _WIPSEQ;
            parameters[5] = _LANE_QTY;
            parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
            parameters[7] = txtProdVerCode.Text;

            C1Window popup = obj as C1Window;
            C1WindowExtension.SetParameters(popup, parameters);
            if (popup != null)
            {
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void PopupRollMapUpdate_Closed(object sender, EventArgs e)
        {

            CMM_RM_CT_RESULT popup = sender as CMM_RM_CT_RESULT;

            if (popup.IsUpdated) PopupRollMapUpdated();
        }

        private void PopupRollMapUpdate_RP_Closed(object sender, EventArgs e)
        {

            CMM_RM_RP_RESULT popup = sender as CMM_RM_RP_RESULT;

            if (popup.IsUpdated) PopupRollMapUpdated();
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

            // 2, 양품량 재계산 후 저장
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

                // 실적 저장
                //Save(true);
                SaveRollMapResult(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
            }


            // 3. 실적 저장 후 Refresh Data 조회
            int idx = _util.GetDataGridRowIndex(dgLotList, "LOTID", sLotid);
            if (idx >= 0)
            {
                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);

                //row 색 바꾸기
                dgLotList.SelectedIndex = idx;
                dgLotList.CurrentCell = dgLotList.GetCell(idx, dgLotList.Columns.Count - 1);

                SetValue(dgLotList.Rows[idx].DataItem);
                SetProcessNumFormat(_PROCID);

                if (_PROCID.Equals(Process.COATING) || _PROCID.Equals(Process.SRS_COATING))
                {
                    GetMaterial();
                }

                ProcCheck(_PROCID, _EQPTID);
                GetDefectInfo();
                GetInputHistory();

                GetEqpFaultyData();
                GetQuality();
                GetColor();

                // CSR : C20221212-000194 - NG TAG Abnormal
                //dgcTagQty.Visibility = Visibility.Collapsed;

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(_PROCID);

                // ROLLMAP 실적수정 버튼 사용
                SetRollMapEquipment(true);
            }
        }

        private void SetRollMapEquipment(bool isChecked = false)
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(_EQPTID);
            _isOriginRollMapEquipment = _isRollMapEquipment;

            // ROLLMAP LOT CHECK
            _isRollMapLot = SelectRollMapLot();

            SetRollMapLotAttribute(_LOTID);

            // NEXT 공정 투입 CHECK 
            if (_isRollMapEquipment && isChecked == false)
            {
                SetRollMapSBL(_LOTID, _WIPSEQ);
            }

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
            catch (Exception ex) { }

            return false;
        }

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
            catch (Exception ex) { }

            return false;
        }

        private void SetRollMapLotAttribute(string sLotID)
        {
            try
            {
                // 롤프레스 공정은 롤맵2.0 실적 적용 대상이 현재 아님(2024.11.04) 코터 공정에 한하여 롤맵수정 버튼 속성 지정 함.
                if (_isOriginRollMapEquipment == true && (string.Equals(_PROCID, Process.COATING) || string.Equals(_PROCID, Process.ROLL_PRESSING)))
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
            catch (Exception ex) { }
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

        private void SetWipReasonCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    // 전체 체크가 없어서 주석 처리
                    /* for (int i = 0; i < dataGrid.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                            if (e.Cell.Row.Index != i)
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    } */

                    // 재 조건 조정 배분 로직 추가 [2021-07-27]
                    DataRow[] row = DataTableConverter.Convert(dataGrid.ItemsSource).Select("RESNGRID='" +
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNGRID")) + "' AND PRCS_ITEM_CODE='GRP_QTY_DIST'");

                    if (row.Length > 0)
                    {
                        decimal iCurrQty = 0;
                        decimal iResQty = 0;
                        decimal iInitQty = row.Sum(g => g.GetValue("FRST_AUTO_RSLT_RESNQTY").GetDecimal());

                        decimal iDistQty = DataTableConverter.Convert(dataGrid.ItemsSource).AsEnumerable()
                                                .Where(g => g.Field<string>("RESNGRID") == Util.NVC(row[0]["RESNGRID"]) &&
                                                                   g.Field<string>("PRCS_ITEM_CODE") != "GRP_QTY_DIST" &&
                                                                   g.Field<string>("RESNCODE") != Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE")))
                                                .Sum(g => Util.NVC_Decimal(g.Field<string>("RESNQTY")));

                        if (iInitQty < (iDistQty + Util.NVC_Decimal(e.Cell.Value)))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", iInitQty - iDistQty);

                        for (int i = 0; i < dataGrid.Rows.Count; i++)
                        {
                            iCurrQty = 0;
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNGRID"), row[0]["RESNGRID"]) &&
                                string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST"))
                            {
                                iCurrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                if (iCurrQty <= (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty))
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                    iResQty += iCurrQty;
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", iCurrQty - (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty));
                                    iResQty = iDistQty + Util.NVC_Decimal(e.Cell.Value);
                                }
                            }
                        }
                    }

                }

                dataGrid.EndEdit();
            }
        }

        public void SaveDefectForRollMap(bool bAllSave = false)
        {
            try
            {
                if (dgDefect.GetRowCount() <= 0) return;

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;
                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataTable IndataTable = inDataSet.Tables.Add("IN_LOT");
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(Int32));

                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = Util.NVC(_LOTID);
                inDataRow["WIPSEQ"] = Util.NVC(_WIPSEQ);
                IndataTable.Rows.Add(inDataRow);

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DATACOLLECT_DEFECT_CT", "IN_EQP,IN_LOT", null, inDataSet);
                }
                catch (Exception ex) { Util.MessageException(ex); }

                //if (!bAllSave)
                //    Util.MessageInfo("SFU1270");     // 저장 되었습니다

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "7");

                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                    return;
                }

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
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0)
                || DataTableConverter.GetValue(rb.DataContext, "CHK").GetInt().Equals(0) // 2024.10.25. 김영국 - DB Type 변경에 따른 로직 대응 처리.
                )
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));
                string sEqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();
                string sWoId = DataTableConverter.GetValue(rb.DataContext, "WOID").ToString();

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

                //PREMIXING
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
                    GetInBox(sLotId, iWipSeq);
                }

                ProcCheck(_PROCID, sEqptID);

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    GetEqpt_Dfct_Apply_Flag();

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                        GetDefectInfo_NJ();
                    else
                        GetDefectInfo_FOL();
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

                if (IsEquipmentAttr(sEqptID))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed;

                // ROLLMAP 실적수정 버튼 사용
                SetRollMapEquipment(true);

                GetEqpFaultyData();
                GetQuality();
                GetColor();
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    GetQualityCount();
                    if (TabReInput.Visibility == Visibility.Visible)
                    {
                        GetDefectReInputList();
                    }
                }



                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    String[] sFilter1 = { _EQPTID, "PROD" };
                    CommonCombo _combo = new CommonCombo();
                    _combo.SetCombo(cboInHalfMountPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                    GetHalfProductList();
                }

                //if (_PROCID.Equals(Process.SLITTING))
                //{
                //    dgcTagQty.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    dgcTagQty.Visibility = Visibility.Collapsed;
                //}

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(_PROCID);



                // 전극 등급 정보 NFF만 해당하여 리빌딩MES 에서는 제외 함.
                //if (tiElectrodeGradeInfo.Visibility.Equals(Visibility.Visible)) SelectElectrodeGradeInfo(sLotId, iWipSeq.GetString());

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

        #region 양품수량 변경
        private void txtOutQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.COATING))
            {
                isChangeDefect = true;

                // 양품수 변경시 투입시 산출
                double dIn = Convert.ToDouble(txtInputQty.Value);
                double dOut = Convert.ToDouble(txtOutQty.Value);
                double dDefect = Convert.ToDouble(txtDefectQty.Value);
                double dLoss = Convert.ToDouble(txtLossQty.Value);
                double dReq = Convert.ToDouble(txtPrdtReqQty.Value);
                double dLen = Convert.ToDouble(txtLengthExceedQty.Value);

                dIn = dOut + dDefect + dLoss + dReq - dLen;

                txtInputQty.Value = dIn;

                _dInQty = dIn;

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
                txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

                isChangeDefect = true;


                if (_isRollMapEquipment && _isRollMapLot && _PROCID.Equals(Process.COATING))
                {
                    SetWipReasonCommittedEdit(sender, e);
                }

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
                    //// 길이 초과가 있는경우 양품수에 합산 불량에서는 제외
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
                    //    dIn = dOut + dTotalDefect;
                    //}
                    if (_isRollMapEquipment && _isRollMapLot)
                    {
                        dOut = dIn - dTotalDefect;
                    }
                    else
                    {
                        dIn = dOut + dTotalDefect;
                    }

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

                if (dOut < 0)
                {
                    //Util.AlertInfo("SFU1884");      //전체 불량 수량이 양품 수량보다 클 수 없습니다.
                    Util.MessageValidation("SFU1884");
                    return;
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

                txtInputQty.Value = dIn;
                // RollMap 실적 생산량 변경 없음
                if (_isRollMapEquipment && _isRollMapLot)
                {
                    txtOutQty.ValueChanged -= txtOutQty_ValueChanged;
                }

                txtOutQty.Value = dOut;
                if (_isRollMapEquipment && _isRollMapLot)
                {
                    txtOutQty.ValueChanged += txtOutQty_ValueChanged;
                }

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

                //if(string.Equals(_PROCID,Process.ASSEMBLY))
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

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(e.Column.Name).Equals("RESNQTY") ||
                    Util.NVC(e.Column.Name).Equals("A_TYPE_DFCT_QTY") ||
                    Util.NVC(e.Column.Name).Equals("C_TYPE_DFCT_QTY") ||
                    Util.NVC(e.Column.Name).Equals("TAG_QTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }

                    // RollMap용 수량 변경 금지 처리
                    if (_isRollMapEquipment && _isRollMapLot &&
                        (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST") ||
                         string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y")))
                        e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // RollMap용 수량 변경 금지 처리
                        if (_isRollMapEquipment && _isRollMapLot &&
                            (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST") ||
                             string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y")))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
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
            }
            catch (Exception ex) { Util.MessageException(ex); }
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
            }

        }

        private void dgSubLot_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name.Equals("WIPQTY"))
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
            if (_ERP_CLOSE.Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return;
            }

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

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(_PROCID, Process.ASSEMBLY))
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
                dgDefect.EndEditRow(true);

                if (_LOTID.Equals(""))
                {
                    //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                    Util.MessageValidation("SFU1381");
                    return;

                }

                if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                {
                    // 변경사유는 필수 입력항목입니다.
                    Util.MessageValidation("SFU2076");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                {
                    // 수정자를 입력 하세요.
                    Util.MessageValidation("SFU3491");
                    return;
                }

                if (_ERP_CLOSE.Equals("CLOSE"))
                {
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return;
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
                        return;
                    }
                    #endregion
                }

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
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.13 : 생산구분 추가

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (Util.GetCondition(txtLotId).Equals("")) //lot id 가 없는 경우
                {
                    //dr["AREAID"] = Util.GetCondition(cboArea, "동을선택하세요.");
                    dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                    if (dr["AREAID"].Equals("")) return;

                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                    if (dr["EQSGID"].Equals("")) return;

                    //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                    if (dr["PROCID"].Equals("")) return;

                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);

                    dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);    // 2021.07.13 : 생산구분 추가
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                    if (dr["PROCID"].Equals("")) return;

                    dr["LOTID"] = Util.GetCondition(txtLotId);
                    bLot = true;
                }

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;

                if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                    sBizName = "DA_PRD_SEL_EDIT_LOT_LIST_MOBILE";
                else
                    sBizName = "DA_PRD_SEL_EDIT_LOT_LIST";

                //DataSet ds = new DataSet();
                //ds.Tables.Add(dtRqst);
                //string xml = ds.GetXml();

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
                            DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);

                            //row 색 바꾸기
                            dgLotList.SelectedIndex = idx;
                            dgLotList.CurrentCell = dgLotList.GetCell(idx, dgLotList.Columns.Count - 1);

                            SetValue(dgLotList.Rows[idx].DataItem);
                            //GetHalfProductList();
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

                //DataTableConverter.GetValue(dgProductLotChoice_Checked.SelectedItem, "EQGRID");

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

                dtRqst.Rows.Add(dr);

                string BizName = string.Empty;
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    //BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
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


        #region [### 선분산투입자재 탭 조회 ###]
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
                sBizName = "DA_PRD_SEL_EDIT_SUBLOT_LIST";

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
                    newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
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

                    newRow["A_TYPE_DFCT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    newRow["C_TYPE_DFCT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

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
                    dr["CLCTVAL01"] = row["CLCTVAL01"].ToString().Trim();
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
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(string)); // MES 2.0 Int32 => string 으로 변경 (유재홍 선임 요청)

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
                        ////inDataTable.Rows[0]["CALDATE"] = dr["CALDATE"];

                        if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.WASHING))
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
                        drIn["FORCE_FLAG"] = "N";               // Y이면 다음 공정 투입 여부 체크 안함
                        drIn["REQ_NOTE"] = txtReqNote.Text;

                        if (_PROCID.Equals(Process.WINDING))
                            drIn["CHANGE_WIPQTY_FLAG"] = "Y";

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

        #region [### 저장 ###]
        private void Save(bool isRollMapSave = false)
        {
            try
            {

                if (_PROCID.Equals(Process.WINDING))
                {
                    if (Math.Abs(txtAssyResultQty.Value) > 0)
                    {
                        // 추가불량 오류 : 추가불량 수량을 재 확인하신 후 반영(저장)해 주세요.
                        Util.MessageValidation("SFU3665");
                        //return;
                    }
                }

                //if (_PROCID.Equals(Process.WASHING) == false)   //와싱이 아닐 경우만
                //{
                //    if (!SaveSubLot())
                //        return;
                //}

                if (!SaveSubLot())
                    return;

                ShowLoadingIndicator();
                DoEvents();

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
                            drIn["LOSS_QTY"] = Convert.ToDecimal(txtLossQty.Value);
                            drIn["LOSS_QTY2"] = Convert.ToDecimal(txtLossQty.Value) * dLaneQty * dLanePtnQty;
                        }
                        if (txtDefectQty.Value != 0)
                        {
                            drIn["DFCT_QTY"] = Convert.ToDecimal(txtDefectQty.Value);
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
                        drIn["FORCE_FLAG"] = "N";               // Y이면 다음 공정 투입 여부 체크 안함
                                                                //drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
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

                                    //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    //{
                                    //    if (double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_A"))) == 0 &&
                                    //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_C"))) == 0 &&
                                    //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "REG_F"))) == 0)
                                    //        //dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"))) + double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                    //        dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                    //    else
                                    //        dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "CALC_F"));
                                    //}
                                    //else
                                    //{
                                    //    dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_FOL.Rows[icnt].DataItem, "CALC_F"));
                                    //}

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
                else if (string.Equals(_PROCID, Process.COATING) && _isRollMapEquipment && _isRollMapLot && !isRollMapSave)
                {
                    SaveRollMapResult(false);
                    HiddenLoadingIndicator();
                    return;
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
                    //Decimal dLanePtnQty = string.IsNullOrWhiteSpace(_LANEPTNQTY) ? 1 : Convert.ToDecimal(_LANEPTNQTY);
                    Decimal dLanePtnQty;

                    if (string.IsNullOrWhiteSpace(_LANEPTNQTY) || _AREATYPE.Equals("E"))
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

                    drIn["REQ_USERID"] = Util.NVC(txtUserName.Tag);
                    drIn["USERID"] = LoginInfo.USERID;
                    drIn["WRK_USERID"] = txtWorker.Tag;
                    drIn["WRK_USER_NAME"] = txtWorker.Text;
                    //drIn["NOTE"] = txtNote.Text;
                    drIn["NOTE"] = SetWipNote();
                    drIn["FORCE_FLAG"] = "N";               // Y이면 다음 공정 투입 여부 체크 안함
                                                            //drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
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

        private void SaveRollMapResult(bool isRollMapSave = false)
        {
            if (isRollMapSave)
            {
                if (!SaveSubLot()) return;
            }

            try
            {
                // 롤맵 수정 Windows에서 저장 내역이 있는 경우 - isRollMapSave true 인 경우
                // 4. BR_ACT_REG_MODIFY_LOT 호출
                if (isRollMapSave)
                {
                    SaveModifyLot();
                }
                else
                {
                    // 저장 버튼 클릭 시 - isRollMapSave false 인 경우(if(string.Equals(_PROCID, Process.COATING) && _isRollMapEquipment && _isRollMapLot && !isRollMapSave))
                    // 1.불량정보 저장, 2. BR_PRD_REG_DATACOLLECT_DEFECT_CT 호출, 3. 불량/LOSS/물청 재조회 4. BR_ACT_REG_MODIFY_LOT 호출
                    if (CommonVerify.HasDataGridRow(dgDefect) && _LOTID.Trim().Length > 0)
                    {
                        // 저장버튼 클릭 시 롤맵 불량, LOSS, 물청 UPDATE 가능 여부 체크 함.
                        //if (!IsRollMapSBL(_LOTID, _WIPSEQ)) return;

                        SaveDefect((dataSet, ex) =>
                        {
                            if (ex == null) SaveModifyLot();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveModifyLot()
        {
            const string bizRuleName = "BR_ACT_REG_MODIFY_LOT";
            const string inputSets = "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ";

            DataSet inDataSet = GetDataSetModifyLot();

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inputSets, "OUT_WIPHISTORY", inDataSet);

                GetLotList();
                ClearValue();

                Util.MessageInfo("SFU1270");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataSet GetDataSetModifyLot()
        {
            DataSet ReturnDataSet = GetSaveDataSet();

            #region [INDATA]

            DataTable inDataTable = ReturnDataSet.Tables["INDATA"];

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
            //Decimal dLanePtnQty = string.IsNullOrWhiteSpace(_LANEPTNQTY) ? 1 : Convert.ToDecimal(_LANEPTNQTY);
            decimal dLanePtnQty;

            if (string.IsNullOrWhiteSpace(_LANEPTNQTY) || _AREATYPE.Equals("E"))
                dLanePtnQty = 1;
            else dLanePtnQty = Convert.ToDecimal(_LANEPTNQTY);

            decimal dWipqty_ed = Convert.ToDecimal(txtOutQty.Value);

            drIn["LANE_QTY"] = dLaneQty; // Convert.ToInt16(txtLaneQty.Text);
            drIn["PROD_VER_CODE"] = Util.GetCondition(txtProdVerCode);


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

            drIn["REQ_USERID"] = Util.NVC(txtUserName.Tag);
            drIn["USERID"] = LoginInfo.USERID;
            drIn["WRK_USERID"] = txtWorker.Tag;
            drIn["WRK_USER_NAME"] = txtWorker.Text;
            drIn["NOTE"] = SetWipNote();
            drIn["FORCE_FLAG"] = "N";               // Y이면 다음 공정 투입 여부 체크 안함
                                                    //drIn["CHANGE_WIPQTY_FLAG"] = isChangeDefect ? "Y" : "N";
                                                    // [2017-09-01 : 소형 관련 수정] 
            drIn["CHANGE_WIPQTY_FLAG"] = "Y";       // 소형 공정 N
            drIn["REQ_NOTE"] = txtReqNote.Text;
            inDataTable.Rows.Add(drIn);

            #endregion

            #region [IN_LOSS]
            DataTable inDataLoss = ReturnDataSet.Tables["IN_LOSS"];

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

            DataTable inDataDfct = ReturnDataSet.Tables["IN_DFCT"];

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

            DataTable inDataPrdtReq = ReturnDataSet.Tables["IN_PRDT_REQ"];

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

            return ReturnDataSet;
        }

        private void SaveDefect(Action<DataSet, Exception> actionCompleted = null)
        {

            const string bizRuleName = "BR_PRD_REG_WIPREASONCOLLECT_RM";
            const string inputSets = "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ"; ;

            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
            inDataTable.Columns.Add("CHECK_YN", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            // IN_DFCT
            DataTable inDataDfct = indataSet.Tables.Add("IN_DFCT");
            inDataDfct.Columns.Add("RESNCODE", typeof(string));
            inDataDfct.Columns.Add("RESNQTY", typeof(decimal));
            inDataDfct.Columns.Add("RESNQTY2", typeof(decimal));
            inDataDfct.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataDfct.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataDfct.Columns.Add("RESNNOTE", typeof(string));
            inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(int));
            inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(decimal));
            inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(decimal));

            // IN_LOSS
            DataTable inDataLoss = indataSet.Tables.Add("IN_LOSS");
            inDataLoss.Columns.Add("RESNCODE", typeof(string));
            inDataLoss.Columns.Add("RESNQTY", typeof(Decimal));
            inDataLoss.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataLoss.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataLoss.Columns.Add("RESNNOTE", typeof(string));
            inDataLoss.Columns.Add("DFCT_TAG_QTY", typeof(int));

            // IN_PRDT_REQ
            DataTable inDataPrdtReq = indataSet.Tables.Add("IN_PRDT_REQ");
            inDataPrdtReq.Columns.Add("RESNCODE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNNOTE", typeof(string));
            inDataPrdtReq.Columns.Add("COST_CNTR_ID", typeof(string));

            DataRow drIn = inDataTable.NewRow();
            drIn["LOTID"] = _LOTID;
            drIn["WIPSEQ"] = _WIPSEQ;
            drIn["CHECK_YN"] = "Y";
            drIn["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(drIn);

            DataTable dtdefect = DataTableConverter.Convert(dgDefect.ItemsSource);
            decimal dLaneQty = string.IsNullOrWhiteSpace(txtLaneQty.Text) ? 1 : Convert.ToDecimal(txtLaneQty.Text);
            decimal dLanePtnQty;

            if (string.IsNullOrWhiteSpace(_LANEPTNQTY) || _AREATYPE.Equals("E"))
                dLanePtnQty = 1;
            else dLanePtnQty = Convert.ToDecimal(_LANEPTNQTY);

            #region [IN_DFCT]
            DataRow[] drDefect = dtdefect.Select("ACTID ='DEFECT_LOT'");
            foreach (DataRow dr in drDefect)
            {
                DataRow drInDfct = inDataDfct.NewRow();
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
                drInDfct["RESNCODE_CAUSE"] = string.Empty;
                drInDfct["PROCID_CAUSE"] = string.Empty;


                inDataDfct.Rows.Add(drInDfct);
            }

            #endregion

            #region [IN_LOSS]
            DataRow[] drLoss = dtdefect.Select("ACTID ='LOSS_LOT'");

            foreach (DataRow dr in drLoss)
            {
                DataRow drInLoss = inDataLoss.NewRow();
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
                drInLoss["RESNCODE_CAUSE"] = string.Empty;
                drInLoss["PROCID_CAUSE"] = string.Empty;

                inDataLoss.Rows.Add(drInLoss);
            }

            #endregion

            #region [IN_PRDT_REQ]

            DataRow[] drCharge = dtdefect.Select("ACTID ='CHARGE_PROD_LOT'");
            foreach (DataRow dr in drCharge)
            {
                DataRow drInPrdtReq = inDataPrdtReq.NewRow();

                drInPrdtReq["RESNCODE"] = dr["RESNCODE"];
                if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                {
                    drInPrdtReq["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                    drInPrdtReq["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]) * dLaneQty * dLanePtnQty;
                }
                else
                {
                    drInPrdtReq["RESNQTY"] = 0;
                    drInPrdtReq["RESNQTY2"] = 0;
                }
                drInPrdtReq["RESNCODE_CAUSE"] = string.Empty;
                drInPrdtReq["PROCID_CAUSE"] = string.Empty;
                drInPrdtReq["RESNNOTE"] = dr["RESNNOTE"];
                drInPrdtReq["COST_CNTR_ID"] = dr["COST_CNTR_ID"];
                inDataPrdtReq.Rows.Add(drInPrdtReq);
            }
            #endregion

            try
            {
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, inputSets, null, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    SaveDefectForRollMap();
                    GetDefectInfo();

                    actionCompleted?.Invoke(bizResult, bizException);
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
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputHalfProductCancelAssembly()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                DataSet inDataSet = _Biz.GetBR_PRD_DEL_INPUT_LOT_AS();

                DataTable inDataTable = inDataSet.Tables["INDATA"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = _LOTID;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["INLOT"];
                for (int i = 0; i < dgInHalfProduct.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")));
                        inInputTable.Rows.Add(row);
                    }
                }
                string xmlText = inDataSet.GetXml();
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //GetHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

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
                string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetHalfProductList();

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

        #region[##투입바구니 조회##]

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

        #region [Clear]
        private void ClearValue()
        {
            txtOutQty.ValueChanged -= txtOutQty_ValueChanged;
            txtInputDiffQty.ValueChanged -= txtInputDiffQty_ValueChanged;

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
            ////txtReqNote.Text = "";
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

            _dtLengthExceeed.Clear();

            Util.gridClear(dgDefect);
            Util.gridClear(dgQualityInfo);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgColor);
            Util.gridClear(dgSubLot);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgDefect_FOL);
            Util.gridClear(dgInHalfProduct);
            Util.gridClear(dgDefect_NJ);

            isChangeDefect = false;

            txtOutQty.ValueChanged += txtOutQty_ValueChanged;

            ////txtUserName.Text = "";
            ////txtUserName.Tag = "";

            btnSave.IsEnabled = true;
            btnRollMapUpdate.IsEnabled = true;

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

                txtOutQty.IsEnabled = false;

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

                // 원형, 초소형 공정
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

                dgQualityInfo.Columns["CLCT_ITVL"].Visibility = Visibility.Collapsed;

                grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                cTabHalf.Visibility = Visibility.Collapsed;
                cTabInputMaterial.Visibility = Visibility.Collapsed;
                cTabColor.Visibility = Visibility.Collapsed;

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

                //btnSubLotSave.Visibility = Visibility.Visible;

                if (sProcID.Equals(Process.PACKAGING))
                {
                    dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                    tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");

                    cTabInBox.Visibility = Visibility.Visible;
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
                }

                tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.PRE_MIXING)){
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

                if (sProcID.Equals(Process.MIXING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;
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

                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Tray ID");
                }
                else
                {
                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Visible;
                }

                // Mixing, Coating 양품수량 변경
                if (sProcID.Equals(Process.MIXING) || sProcID.Equals(Process.COATING))
                    txtOutQty.IsEnabled = true;



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

                        dgLotList.Columns["WINDING_RUNCARD_ID"].Visibility = Visibility.Visible;
                        dgLotList.Columns["LOTID_AS"].Visibility = Visibility.Visible;
                        dgDefect.Columns["INPUT_QTY_APPLY_TYPE_CODE_NAME"].Visibility = Visibility.Visible;

                        cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtInputQty.IsEnabled = false;
                        txtOutQty.IsEnabled = false;

                        cTabInputHalfProduct.Visibility = Visibility.Visible;
                        btnInHalfProductInPutCancel.Visibility = Visibility.Collapsed;
                        dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;

                        if (sProcID.Equals(Process.ASSEMBLY))
                        {
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
                        }
                        else
                        {   //WASHING

                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");

                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;

                            //btnSubLotSave.Visibility = Visibility.Collapsed;
                        }

                    }
                }
                else
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;
                    cTabInputHalfProduct.Visibility = Visibility.Collapsed;
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
            txtOutQty.ValueChanged -= txtOutQty_ValueChanged;

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

            if (Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.ASSEMBLY) || Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.WASHING))
            {
                txtLengthExceedQty.Value = 0;
                txtLaneQty.Text = string.Empty;
                txtDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DIFF_QTY")));
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

            txtProdVerCode.Text = Util.NVC(DataTableConverter.GetValue(oContext, "PROD_VER_CODE"));
            //txtNote.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));            

            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _EQPTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTNAME"));
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

            if (_PROCID.Equals(Process.STACKING_FOLDING))
            {
                cboChange.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_WRK_TYPE_CODE"));
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

            }

            txtOutQty.ValueChanged += txtOutQty_ValueChanged;
            txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
        }
        #endregion

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
            }

            // IN_PRDT_REQ
            DataTable inDataPrdtReq = indataSet.Tables.Add("IN_PRDT_REQ");
            inDataPrdtReq.Columns.Add("LOTID", typeof(string));
            inDataPrdtReq.Columns.Add("WIPSEQ", typeof(string));
            inDataPrdtReq.Columns.Add("RESNCODE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNNOTE", typeof(string));
            inDataPrdtReq.Columns.Add("COST_CNTR_ID", typeof(string));

            return indataSet;
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
                    //// 길이 초과가 있는경우 양품수에 합산 불량에서는 제외
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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            if (Util.NVC(e.Cell.Column.Name) == "REG_A" || Util.NVC(e.Cell.Column.Name) == "REG_C")
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
                            else if (Util.NVC(e.Cell.Column.Name) == "REG_F")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            }
                        }
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

                if (Util.NVC(e.Column.Name).Equals("REG_A") ||
                    Util.NVC(e.Column.Name).Equals("REG_C") ||
                    Util.NVC(e.Column.Name).Equals("REG_F"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }

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
                chkFoldingStacking();

                // 2017.11.21  INS염규범 GEMS FOLDING 실적 마감 단어 수정 요청 CSR20171011_02178
                if (_EQGRID == "STK")
                {
                    dgDefect_FOL.Columns["REG_F"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("STACKING CELL") };
                    dgDefect_FOL.Columns["CALC_F"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("STACKING CELL") };
                }
                else
                {

                    dgDefect_FOL.Columns["REG_F"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("입력"), ObjectDic.Instance.GetObjectName("FOLDED CELL") };
                    dgDefect_FOL.Columns["CALC_F"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("계산"), ObjectDic.Instance.GetObjectName("FOLDED CELL") };
                };

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
                    // ERP 생산실적이 마감 되었습니다.
                    Util.MessageValidation("SFU3494");
                    return;
                }

                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {

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

                            Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

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
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void chkFoldingStacking()
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LOTID"] = _LOTID;

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQGRID_BYLOTID", "INDATA", "OUTDATA", inDataTable, (dtResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    _EQGRID = dtResult.Rows[0]["EQGRID"].ToString();

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
                string xml = indataSet.GetXml();

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
                double dAlphaQty = 0;
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
                txtInputDiffQty.ValueChanged += txtInputDiffQty_ValueChanged;
            }
        }

        private void OnClickInputMaterial(object sender, RoutedEventArgs e)
        {

            if (Util.GetCondition(cboEquipment).ToString().Equals(""))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.

                return;
            }
            //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);


            CMM001.Popup.CMM_INPUT_MATERIAL wndPopup = new CMM001.Popup.CMM_INPUT_MATERIAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = _LOTID;
                Parameters[1] = _WORKORDER;
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
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
            GetMaterialSummary(_LOTID, _WORKORDER);
        }
        // KEY DOWN


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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            //if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C" || Convert.ToString(e.Cell.Column.Name) == "REG_L" 
                            //|| Convert.ToString(e.Cell.Column.Name) == "REG_R" || Convert.ToString(e.Cell.Column.Name) == "REG_ML" || Convert.ToString(e.Cell.Column.Name) == "REG_MR")
                            //{
                            if (Util.NVC(e.Cell.Column.Name).StartsWith("REG"))
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
                            else if (Util.NVC(e.Cell.Column.Name) == "INPUT_FC")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            }
                        }
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

                if (Util.NVC(e.Column.Name).Equals("REG_A") ||
                    Util.NVC(e.Column.Name).Equals("REG_C") ||
                    Util.NVC(e.Column.Name).Equals("REG_F") ||
                    Util.NVC(e.Column.Name).Equals("INPUT_FC"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }

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

        #region [##PKG 바구니 실적 수정 POPUP##]
        private void btnInputBox_Click(object sender, RoutedEventArgs e)
        {
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
        #endregion


        private void SetInputHistButtonControls(string sProcID)
        {
            try
            {
                bool bRet = false;
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

                    btnInHalfProductInPutQty.Visibility = Visibility.Visible;       // 투입반제품이력 Tab : 투입량수정                    
                    btnInHalfProductInPutCancel.Visibility = Visibility.Visible;    // 투입반제품이력 Tab : 투입취소
                }
                else
                {
                    btnInputBox.Visibility = Visibility.Collapsed;                  // 투입바구니 Tab : 바구니투입

                    btnInHalfProductInPutQty.Visibility = Visibility.Collapsed;     // 투입반제품이력 Tab : 투입량수정                    
                    btnInHalfProductInPutCancel.Visibility = Visibility.Collapsed;  // 투입반제품이력 Tab : 투입취소
                }
            }
            catch (Exception ex)
            {
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
            catch (Exception ex) { }

            return false;
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
                        btnSaveElectrodeGradeInfo.Visibility = Visibility.Collapsed;

                        Util.GridSetData(dgElectrodeGradeInfo, bizResult, null, true);

                        for (int index = 0; index < bizResult.Rows.Count; index++)
                        {
                            C1.WPF.DataGrid.DataGridColumn JudgColumn = dgElectrodeGradeInfo.Rows[index].DataGrid.Columns["GRD_JUDG_CODE"] as C1.WPF.DataGrid.DataGridColumn;
                            string JudgCode = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "GRD_JUDG_CODE"));
                            string UserId = Util.NVC(DataTableConverter.GetValue(dgElectrodeGradeInfo.Rows[index].DataItem, "USERID"));

                            if (!JudgCode.Equals("") && UserId.Equals(""))
                            {
                                JudgColumn.IsReadOnly = true;
                                btnSaveElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                JudgColumn.IsReadOnly = false;
                                IsElectrodeGradeInfo();
                                //btnSaveElectrodeGradeInfo.Visibility = Visibility.Visible;
                            }

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

                                if (!JudgCode.Equals("") && UserId.Equals(""))
                                {
                                    JudgColumn.IsReadOnly = true;
                                    btnSaveElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    IsElectrodeGradeInfo();
                                    //JudgColumn.IsReadOnly = false;
                                    //btnSaveElectrodeGradeInfo.Visibility = Visibility.Visible;
                                }
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
    }
}


