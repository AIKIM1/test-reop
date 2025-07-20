/*************************************************************************************
 Created Date :
      Creator : 
   Decription : Tray 정보조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29 DEV    Initial Coding
  2021.05.13 PSM    Cell Data Grid 범위설정 및 Cell ID 검색 하여 배경색 표시하는 로직 변경
                    설정 버튼 클릭하여 배경색이 바뀐 경우 새로운 Tray를 조회하거나, 설정버튼을 다시 누를 때 까지 바뀌지 않아야 함.   
  2021.09.30 PSM    rdoCurrA 추가 (powergrading, powercharging 선택 시 전류 단위는 A)
  2021.10.12 KDH    Auto Calibration Lot 표시 
  2022.01.28 KDH    O등급 표시 : O(공정검사의뢰) , O(PQC검사 의뢰)
  2022.03.03 PSM    특별등록 시 적재된 트레이 모두 특별 등록
  2022.05.26 JYD    Cell ID 입력후 엔터검색시 우측 Cell ID 검색안되는 현상 수정.목록 갯수에 따른 열 조정.
  2022.07.04 JYD    LOTID, CSTID 널체크에 "" 체크 추가. Cell 정보 기본 설정 추가
  2022.07.11 JYD    대기상태일때 Sample 출하 버튼 무조건 비활성화(김룡근)
  2022.08.19 LJM    변경이력 버튼 추가 
  2022.08.25 최도훈 Aging 입고 버튼에 Null 예외처리 추가
  2022.09.01 최도훈 과불량 변경 버튼 추가. 동작시 불량한계초과여부 변경(WIPATTR 내 DFCT_LIMIT_OVER_FLAG)
  2022.09.21 최도훈 BR_GET_LOAD_TRAY_INFO 호출시 파라미터 AREAID 추가
  2022.09.27 최도훈 Carrier의 비정상코드(ABNORM_TRF_RSN_CODE) 표시, 비정상 상태 해제 버튼 추가 (ESWA3 RTD팀 요청사항)
  2022.09.28 LJM    이력의 마지막 공정은 종료상태이지만 TRAY 공정상태는 작업중일 때 진행중인 공정 종료 확인 팝업 활성화되도록 수정 
  2022.10.07 최도훈 Crack 해제 기능 추가 (WipAttr 내 SCRP_TRAY_FLAG)
  2022.11.11 이정미 Tray 예약, Truoble 상태 미표기 오류 수정
  2023.01.05 형준우 대기상태에서 공정종료 시 에러나는 메세지 출력 수정
  2023.01.12 이정미 Cell 목록 중 취출된 Cell 구분하기 위해 음영 추가
  2023.01.18 이해령 강제출고버튼이벤트 BIZ 에러 처리위해 Tray Catch 추가
  2023.01.23 최완영 Sample 출고시  TrayId(LOTID)가 없을경우  Error 처리  추가.
  2023.01.25 이정미 Dummy Tray Maker 추가
  2023.01.26 이해령 LOTID 없을 경우에는 버튼 비활성화(처음 LOAD시 비활성화, Clear 부분에서 비활성화, GetTrayInfo에서 Tray 조회시 버튼 활성화) 
  2023.02.13 주훈종 [E20230213-000118] 특별관리등록 기존 200개 제한에서 1000개 처리로 변경, 200개씩 나눠서 비즈룰 호출하는 방식으로 변경
  2023.04.04 김수용 [E20230331-001977] 보정dOCV 값 조회 추가
  2023.04.24 박승렬 [E20230417-000026] LOT HOLD 사유, LOT_TYPE 추가
  2023.04.25 하유승 Tary 정보조회의 컬렴명 변경, 수동포트 반송해제 버튼 이벤트 함수 수정
  2023.04.26 하유승 Tray 정보조회의 수동포트 반송해제 버튼 이벤트 메시지 Type 추가
  2023.05.09 하유승 Tray 정보조회 수동포트 반송해제 관련 작업 롤백(배포 연기) & 단적재불일치 메시지 코드 내용 변환 추가 
  2023.05.18 김호선 CT 검사기 정보 Grid 추가, CT 검사기 샘플출고 버튼 추가
  2023.05.19 하유승 Tary 정보조회의 컬렴명 변경, 수동포트 반송해제 버튼 이벤트 함수 수정 - 수동포트 반송해제 클릭 시 초기화 팝업 출력
  2023.05.31 박승렬 수동포트 반송 해제 버튼 클릭 시 호출하는 BIZ INPUT PARAMETER 오타 수정 
  2023.06.07 이정미 Degas 입고 예상 시간 추가
  2023.06.08 하유승 수동포트 반송 해제 사유와 단적재불일치 메시지 코드 내용을 동시 표기되도록 수정
  2023.07.03 최도훈 Aging 입고 버튼 index 에러 수정
  2023.07.31 최도훈 J/F RestHeating 공정 선택시 JIG 압력, J/F 온도 값 조회되도록 수정
  2023.08.03 권순범 수동포트 반송해제/비정상 반송해제 버튼 활성화/비활성화 기능 추가
  2023.08.08 이지은 샘플출고 Biz Exception 발생시 오류메세지 정상적으로 발생하도록 Try Catch 추가
  2023.08.10 이해령 Crack 변경 BR로 변경하여 SCRP_TYPE_CODE 관리 , 0821 메시지 추가 		
  2023.08.25 이해령 Crack 변경 수동포트가 아닌 대상공정설비 위치 확인 후 상태변경 (대상설비에서 Crack 알람 후 Tray 위치정보 입력하게 함)	
  2023.08.10 이해령 Crack 변경 BR로 변경하여 SCRP_TYPE_CODE 관리 , 0821 메시지 추가 				
  2023.08.15 손동혁 NA1동 요청 (Wetting Grid , Hpcd 전류 ) 사용 NA1동만 보이게 추가                   		
  2023.09.25 김용식 NJ2동 요청 Hpcd 초기전압, hpcd 종료전압 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_110.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_021_NJ : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        // ---------------------------------------------
        //          Line ID 상수
        // ---------------------------------------------
        public const string FORMATION_EQPTYPE = "1";
        public const string LOWAGING_EQPTYPE = "3";
        public const string HIGHAGING_EQPTYPE = "4";
        public const string OUTAGING_EQPTYPE = "7";
        public const string PREAGING_EQPTYPE = "9";
        public const string JUDGE_EQPTYPE = "B";
        public const string SELECTOR_EQPTYPE = "6";
        public const string GRADER_EQPTYPE = "5";
        public const string REPRINT_EQPTYPE = "K";
        public const string AGING_LINE_1 = "001";
        public const string AGING_LINE_2 = "002";
        public const string CHARGE = "11";
        public const string DISCHARGE = "12";

        public const string OCV_DATATYPE = "O";
        public const string FITTED_OCV_DATATYPE = "DOCV2";
        public const string IMP_DATATYPE = "R";
        public const string FITTEDIMP_DATATYPE = "D";
        public const string FITTEDCAPACITY_DATATYPE = "F";
        public const string CAPA_DATATYPE = "C";
        public const string CURR_DATATYPE = "I";
        public const string VOLT_DATATYPE = "V";
        public const string CURRMU_DATATYPE = "U";
        public const string DELTA_TEMP = "DT";
        public const string HPCDPRESS_DATATYPE = "HP";
        public const string FIRST_VOLT_DATATYPE = "FV";
        //20230925 김용식 HPCD 초기전압, 종료전압 추가
        public const string INIT_VOLT_DATATYPE = "IV";
        public const string END_VOLT_DATATYPE = "EV";
        public const string TEMP_SUB_DATATYPE = "SV";
        public const string POWERGRADE_DATATYPE = "P";
        public const string OCV_SAMPLE_DATATYPE = "S";
        public const string IMP_SAMPLE_DATATYPE = "T";
        public const string CCVAL_DATATYPE = "CCV";
        public const string CCTIME_DATATYPE = "CCT";
        public const string CVVAL_DATATYPE = "CVV";
        public const string CVTIME_DATATYPE = "CVT";
        public const string HPCDCurr_DATATYPE = "HPC";
        public const string PJCODE = "B";
        /* 2014.1.17 정종덕D // [CSR ID:2444504] 자동차 셀 전 모델 및 호기 W-code 및 비선형 mV/day 기준 정보 등록 
         * 저전압 데이터 타입추가(트레이 조회화면에서 사용)
         * */
        public const string LOWVOLTAGE_OCVJUDG_OCV_DATATYPE = "LOW_R";
        public const string LOWVOLTAGE_OCVJUDG_DOCV_DATATYPE = "LOW_E";

        //2014.02.26 강진구D | 2491135   | 대용량 방전기 전압값 표시 件 / 방전 전 전압, 방전 후 전압 상수 추가 POWERGRADING_STRAT_VOLT_DATATYPE, POWERGRADING_END_VOLT_DATATYPE
        public const string POWERGRADING_STRAT_VOLT_DATATYPE = "PWDS_V";
        public const string POWERGRADING_END_VOLT_DATATYPE = "PWDE_V";
        //[180807 이수진C] JIG CNB 관련 추가
        public const string JIG_CNB_PRESS_DATATYPE = "JIG_CNB_P";
        public const string JIG_CNB_TEMP_DATATYPE = "JIG_CNB_T";
        //200408 KJE : 용량 선별화 판정 추가
        public const string FITTEDCAPACITY_SAS_DATATYPE = "ML_F";
        //2022-07-04 : 셀 기본정보 추가
        public const string DEFAULT_VEW_CELL = "DEFAULT_VEW_CELL";

        public bool _FinCheck = false;  // 충방전기 자동 Pin체크에서 넘어온 경우 체크
                                        // Tray 선택 화면에서 넘어 올 경우
        public bool _oldTray = false; //
        public bool _bTrayInfo = false;

        Util _Util = new Util();         //2021.10.12 Auto Calibration Lot 표시
        public bool bAtCalibUse = false; //2021.10.12 Auto Calibration Lot 표시

        public bool bCTSampleUse = false; //2023.04.27  CT 검사기 공정 Sample 출고 가능 여부

        private string _sTrayID; //TRAY_ID
        private string _sTrayNo; //TRAY_NO
        private string _sLotID; //LOTID
        private string _sTrayLine;
        private string _sTrayLineName;
        private string _sFinCD = "PROC";   //C:Default , P:FIN Check용
        private string _sModelID;
        private string _sCurrOper;
        private string _sCurrYN;
        private string _sOPStartTime;
        private string _sEqpID;
        private string _sSpecial;
        private string _sShipmentYN;
        private string _sActYN = "N";   //다른 창에서 넘어오는지 체크 해서 Active Event 제어
        private int _AgingOutPriority = 0;
        private string _CtType= string.Empty; //CT 검사 등록 확인용
        private int iLastRow;  //공정이력 마지막줄 확인을 위함 //200420 KJE

        public const string OCVDATATYPE = "O";
        public const string IMPDATATYPE = "R";
        public const string CAPADATATYPE = "C";
        public const string CURRDATATYPE = "I";
        public const string VOLTDATATYPE = "V";

        private bool bsetRange = false; //범위설정
        private bool bsetCellRange = false; //Cell ID 설정

        private DataSet dsRslt = new DataSet();

        bool bUseFlag = false; //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 검증 후 삭제

        // 2021-05-13
        // Cell data grid에서 범위설정, Cell ID 설정 시 해당 Cell ID 담기 위한 용도
        // Cell 범위 설정되어 Background Color가 바뀐 경우, 다른 공정의 측정값이 조회(좌측 공정 진행 grid 선택시 변경) 되더라도 유지되어야 함. (위치 정보 필요)
        private DataTable dtValueSublot = new DataTable();
        private DataTable dtSublotList = new DataTable();
        private DataTable dtWipCell = new DataTable();

        public string TrayID
        {
            set { this._sTrayID = value; }
            get { return this._sTrayID; }
        }

        public string TrayNO
        {
            set { this._sTrayNo = value; }
            get { return this._sTrayNo; }
        }

        public string FinCD
        {
            set { this._sFinCD = value; }
            get { return this._sFinCD; }
        }

        public bool FinCheck
        {
            set { this._FinCheck = value; }
            get { return this._FinCheck; }
        }

        public string EQPID
        {
            set { this._sEqpID = value; }
            get { return this._sEqpID; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }
        public FCS001_021_NJ()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTrayID.Text) || !string.IsNullOrEmpty(txtTrayNo.Text))
            {
                ClearControlValue();
            }
            try
            {
                bAtCalibUse = _Util.IsAreaCommonCodeUse("ATCALIB_TYPE_CODE", "ATCALIB_TYPE_CODE");  //2021.10.12 Auto Calibration Lot 표시
                                                                                                    //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if (!_Util.IsAreaCommonCodeUse("FORM_CT_SMPL_YN", "USE_YN")) // 2023.04.27 CT 검사기 공정 사용 유무
                {
                    btnCTSampleOut.Visibility = Visibility.Hidden;
                    CtGrd.Height = new GridLength(0);
                }

                ///2023.08.15
                ///2023.08.15 NA 1동 요청 (Wetting Grid , Hpcd 전류 사용 NA 1동만 보이게 추가 검증 후 삭제
                bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_001"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if (bUseFlag)
                {
                    dgWetting.Visibility = System.Windows.Visibility.Visible;
                    dgWetting.UpdateLayout();
                    rdoHPCDCurr.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    dgWetting.Visibility = System.Windows.Visibility.Collapsed;
                    dgWetting.UpdateLayout();
                    rdoHPCDCurr.Visibility = System.Windows.Visibility.Collapsed;
                }



                dtValueSublot.Columns.Add("SUBLOTID", typeof(string));
                dtSublotList.Columns.Add("SUBLOTID", typeof(string));
                //다른 화면에서 넘어온 경우
                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    ClearControlValue();
                    TrayID = Util.NVC(parameters[0]);
                    TrayNO = Util.NVC(parameters[1]);
                    FinCD = Util.NVC(parameters[2]);
                    FinCheck = Util.NVC(parameters[3]).Equals("true") ? true : false;
                    EQPID = Util.NVC(parameters[4]);
                    if (Util.NVC(parameters[5]).Equals("Y")) { chkHist.IsChecked = true; }
                    else { chkHist.IsChecked = false; } //PROCESS_HIST_YN
                    if (!string.IsNullOrEmpty(_sTrayNo)) { txtTrayNo.Text = _sTrayNo; }
                    if (!string.IsNullOrEmpty(_sTrayID)) { txtTrayID.Text = _sTrayID; }
                    GetTrayInfo();
                }
                //조회된 Tray가 없으면 모든 버튼 DISABLE 시킴
                if(string.IsNullOrEmpty(txtTrayNo.Text) || string.IsNullOrEmpty(txtTrayID.Text))
                {
                    SetButtonEnable(false);
                }

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //  InitCombo();
            //  InitControl();
            //  SetEvent();
        }

        #endregion

        #region [Method]
        private void GetTrayInfo()
        {
            try
            {
                //CELL Data Grid 범위설정, Cell ID 설정 부분은 새로운 Tray 조회 될 때만 Clear 되어야 함.
                txtRange1.Text = string.Empty;
                txtRange2.Text = string.Empty;
                txtCellList.Text = string.Empty;
                dtValueSublot.Rows.Clear();
                dtSublotList.Rows.Clear();

                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSTAT", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("PROCESS_HIST_YN", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(txtTrayID.Text)) dr["CSTID"] = Util.NVC(txtTrayID.Text);
                else dr["CSTID"] = null;
                if (!string.IsNullOrEmpty(txtTrayNo.Text)) dr["LOTID"] = Util.NVC(txtTrayNo.Text);
                else dr["LOTID"] = null;
                //if (!string.IsNullOrEmpty(_sFinCD)) dr["WIPSTAT"] = _sFinCD;
                if (!string.IsNullOrEmpty(_sEqpID)) dr["EQPTID"] = _sEqpID;
                else dr["EQPTID"] = null;

                if ((bool)chkHist.IsChecked) dr["PROCESS_HIST_YN"] = "Y";
                else dr["PROCESS_HIST_YN"] = "N";

                if ((dr["CSTID"] == null || dr["CSTID"].ToString() == "") &&
                    (dr["LOTID"] == null || dr["LOTID"].ToString() == ""))
                {
                    Util.Alert("FM_ME_0069"); //Tray ID 또는 Tray No를 입력해주세요.
                    return;
                }

                if (!string.IsNullOrEmpty(dr["LOTID"].ToString())) dr["CSTID"] = null; //tray no 가 있을경우 tray_id 안던지기
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inDataTable.Rows.Add(dr);
                
                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inDataTable);

                if(bUseFlag) //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 검증 후 삭제
                {
                    dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_LOAD_TRAY_INFO", "INDATA", "RET_TRAY_PROCESS,RET_DELTA_OCV,RET_TRAY_INFO,RET_TRAY_OP_STATUS,RET_OUT_OCV,RET_DEGAS_ESTIMATED,RET_W_LOW_VOLT,RET_LAST_OP,RET_CT_INFO,RET_WETTING_INFO,OUTDATA", inDataSet);
                }
                else
                {
                    dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_LOAD_TRAY_INFO", "INDATA", "RET_TRAY_PROCESS,RET_DELTA_OCV,RET_TRAY_INFO,RET_TRAY_OP_STATUS,RET_OUT_OCV,RET_DEGAS_ESTIMATED,RET_W_LOW_VOLT,RET_LAST_OP,RET_CT_INFO,OUTDATA", inDataSet);
                }
                

                var _mhsMsgCodeTable = (DataTable)setTableData("CSTID,LANGID", "", new DataTable());
                var _mhsDr = (DataRow)setTableData("CSTID,LANGID", $"{dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString()},{LoginInfo.LANGID}", null, _mhsMsgCodeTable.NewRow());

                _mhsMsgCodeTable.Rows.Add(_mhsDr);

                var _mhsDsRslt = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_MHS_MSG_NAME_UI_BY_CSTID", "RQSTDT", "RSLTDT", _mhsMsgCodeTable);
                var gFMPC = getForcManlPortCode();

                //비정상 반송 코드 조회
                var _TrayAbnormalCodeTable = (DataTable)setTableData("CSTID", "", new DataTable());
                var _TrayAbnormal = (DataRow)setTableData("CSTID", $"{dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString()}", null, _TrayAbnormalCodeTable.NewRow());
                _TrayAbnormalCodeTable.Rows.Add(_TrayAbnormal);
                var dsTrayAbnormalCodeRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_ABNORM_TRF_RSN_CODE", "RQSTDT", "RSLTDT", _TrayAbnormalCodeTable);

                if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("1"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("EMPTY_TRAY"); // 공 Tray
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("2"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("INFO_DEL") + " " + ObjectDic.Instance.GetObjectName("TRAY"); // 정보삭제 TRAY
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("3"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("HISTORY") + " " + ObjectDic.Instance.GetObjectName("TRAY"); // 이력 TRAY
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["RET_TRAY_INFO"].Rows.Count == 0)
                {
                    Util.Alert("FM_ME_0078");  //Tray 정보가 존재하지 않습니다.
                    return;
                }
                _sTrayLine = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQSGID"].ToString();
                _sTrayLineName = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQSGNAME"].ToString();
                _sCurrOper = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].ToString();
                txtTrayID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString();
                //txtTrayID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                if (string.IsNullOrEmpty(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString()))
                    txtTrayID.Text = TrayID;
                txtLotID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROD_LOTID"].ToString();
                txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                _sLotID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROD_LOTID"].ToString();
                txtPRODID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PRODID"].ToString();
                txtPRODID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtDfctLmtOverFlag.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["DFCT_LIMIT_OVER_FLAG"].ToString();
                txtDfctLmtOverFlag.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtAbnormTrfRsnCode.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ABNORM_TRF_RSN_CODE"].ToString();
                txtAbnormTrfRsnCode.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtCrackState.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SCRP_TRAY_FLAG"].ToString();
                txtCrackState.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtROUTID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ROUTID"].ToString();
                txtROUTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCNAME"].ToString();
                txtOper.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Tag = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].ToString();
                // 4월 24일 추가
                txtLotHold.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["HOLD_RSN_CNTT"].ToString();
                txtLotHold.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtLotTYPE.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOT_TYPE"].ToString();
                txtLotTYPE.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;


                _sTrayNo = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString();
                _sLotID = string.Empty;
                _sTrayID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString();

                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString() == string.Empty)
                {
                    txtOpPlanTime.Text = string.Empty;
                    txtOpPlanTime.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                }
                else
                {
                    txtOpPlanTime.Text = DateTime.Parse(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString()).ToString();
                    txtOpPlanTime.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                }

                if (!gFMPC.Rows[0]["FORM_FORC_MANL_PORT"].ToString().Equals("N"))
                {
                    if (_mhsDsRslt.Rows.Count > 0)
                    {
                        txtTrayID.Foreground = new SolidColorBrush(Colors.Blue);
                        txtDummyMaker.Text = gFMPC.Rows[0]["FORM_FORC_MANL_PORT_NAME"].ToString() + "," + _mhsDsRslt.Rows[0]["MSGNAME"].ToString().Trim();
                    }
                    else
                    {
                        txtTrayID.Foreground = new SolidColorBrush(Colors.Blue);
                        txtDummyMaker.Text = gFMPC.Rows[0]["FORM_FORC_MANL_PORT_NAME"].ToString().Trim();
                    }
                }else if (_mhsDsRslt.Rows.Count > 0)
                {
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Blue);
                    txtDummyMaker.Text = _mhsDsRslt.Rows[0]["MSGNAME"].ToString().Trim();
                }
                else if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["DUMMY_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Blue);
                    txtDummyMaker.Text = " " + MessageDic.Instance.GetMessage("SUF9013") + " : " + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["INSUSER"].ToString().Trim();
                }
                else
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Black);

                //특별관리
                _sSpecial = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_FLAG"].ToString();
                _sShipmentYN = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ISS_RSV_FLAG"].ToString();
                if (!_sSpecial.Equals("N"))
                {
                    if (string.IsNullOrEmpty(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_GR_ID"].ToString().Trim()))
                    {
                        txtSpc.Text = ObjectDic.Instance.GetObjectName("SPECIAL");
                        txtSpc.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    }
                    else
                    {
                        txtSpc.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_GR_ID"].ToString().Trim();
                        txtSpc.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    }
                    if (!string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_REL_SCHD_DTTM"])))
                    {
                        txtSpclRelDate.Text = Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_REL_SCHD_DTTM"]);
                    } // PSM : 2021-07-01 특별관리 해제일시 추가

                    txtSpc.Foreground = new SolidColorBrush(Colors.Red);
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Red);
                    lblSpecial.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpSelect.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpDes.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpclRelDate.Foreground = new SolidColorBrush(Colors.Red);

                    txtSpSelect.Text = Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NAME"]);
                    txtSpSelect.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                    if (_sSpecial.Equals("I"))
                    {
                        txtSpc.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtTrayID.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        lblSpecial.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpSelect.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpDes.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpclRelDate.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    }
                    txtSpDes.Text = " " + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NOTE"].ToString().Trim();
                }
                else
                {
                    txtSpc.Foreground = new SolidColorBrush(Colors.Black);
                    lblSpecial.Foreground = new SolidColorBrush(Colors.Black);

                    //특별관리 아닐때도 더미로 생성시 생성내역 표시
                    txtSpDes.Foreground = new SolidColorBrush(Colors.Black);
                    txtSpDes.Text = " " + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NOTE"].ToString().Trim();
                    txtSpDes.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                }
                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString().Equals("7"))
                {
                    string sName = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CT_TYPE_CODE"].ToString().Equals("Y") ? "CT_SAMPLE_SHIPPING" : "SAMPLE_SHIPPING";
                    txtSample.Text = ObjectDic.Instance.GetObjectName(sName) + "("
                        + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["UPDUSER"].ToString() + ")";
                    //sample 출고해제 버튼 
                    btnSampleOut.Content = ObjectDic.Instance.GetObjectName("SAMPLE_REL");  //Sample 출고해제
                }
                else if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString().Equals("9"))
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("FORCE_SHIPPING") + "("   //강제출고중
                       + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["UPDUSER"].ToString() + ")";
                }
                else
                {
                    txtSample.Text = string.Empty;
                    txtSample.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    //sample 출고해제 버튼 
                    btnSampleOut.Content = ObjectDic.Instance.GetObjectName("SAMPLE_SHIP");  //Sample 출고
                }
                txtTrayOpStatus.Foreground = new SolidColorBrush(Colors.Black);


                switch (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString())
                {
                    case "PROC":
                        txtTrayOpStatus.Tag = "S";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("WORKING");  //작업중
                        break;
                    case "END":
                        txtTrayOpStatus.Tag = "END";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("완공");  //완공
                        break;
                    case "WAIT":
                        txtTrayOpStatus.Tag = "S";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("대기");  //대기
                        break;
                    case "TERM":
                        txtTrayOpStatus.Tag = "TERM";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("재공종료"); //재공종료
                        break;
                    default:
                        txtTrayOpStatus.Tag = string.Empty;
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("INFO_ERR");  //정보이상
                        break;
                }

                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ISS_RSV_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayOpStatus.Tag = "P";
                    txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("RESV");  //예약
                }

                //추가요청
                if (dsRslt.Tables["RET_TRAY_OP_STATUS"].Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayOpStatus.Tag = "T";
                    txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("TROUBLE");  //Trouble
                }

                txtNextOp.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["NEXT_OP_NAME"].ToString();
                txtNextOp.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNextOp.Tag = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["NEXT_OP_ID"].ToString();
                txtTrayNo.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString();
                // txtTrayNo.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtInCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["INPUT_SUBLOT_QTY"].ToString();
                txtInCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNowCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIP_QTY"].ToString();
                txtNowCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtGoodCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["GOOD_SUBLOT_QTY"].ToString();
                _sFinCD = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTSTAT"].ToString();
                _sModelID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["MDLLOT_ID"].ToString();
                _AgingOutPriority = int.Parse(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString());
                _CtType = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CT_TYPE_CODE"].ToString();
                _FinCheck = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ATCALIB_TYPE_CODE"].ToString().Equals("P") ? true : false; //2021.10.12 Auto Calibration Lot 표시

                if(_AgingOutPriority == 7 && _CtType == "Y")
                {
                    btnCTSampleOut.Content = ObjectDic.Instance.GetObjectName("CT_SAMPLE_REL");  //CT Sample 출고해제
                }
                else
                {
                    btnCTSampleOut.Content = ObjectDic.Instance.GetObjectName("CT_SAMPLE_SHIP");  //CT Sample 출고
                }

                //조회된 Tray가 있으면 모든버튼 Enable 시킴
                if (!string.IsNullOrEmpty(txtTrayNo.Text) && !string.IsNullOrEmpty(txtTrayID.Text))
                {
                    SetButtonEnable(true);
                }

                if (_FinCheck || _sFinCD.Equals("P")) //충방전기 자동 Pin체크에서 넘어온경우 
                {
                    btnManual.IsEnabled = false;
                    btnCell.IsEnabled = false;
                    btnDOCV.IsEnabled = false;
                }
                else
                {
                    //MENUAUTH   
                    /*    if (MENUAUTH.Equals("W"))
                        {
                            btnManual.IsEnabled = true;
                            btnCell.IsEnabled = true;
                            btnDOCV.IsEnabled = true;
                        }*/
                }

                //공정종료 상태이고 차기공정이 Aging인 Tray만 Aging 입고처리 버튼 활성화 조건에서 MENUAUTH 제외
                string sPGC = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].ToString().Substring(2, 1);
                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString().Equals("WAIT") &&
                    (sPGC.Equals("3") || sPGC.Equals("4") || sPGC.Equals("7") || sPGC.Equals("9")))
                {
                    btnAging.IsEnabled = true;

                }
                else
                {
                    btnAging.IsEnabled = false;
                }
                txtTrayOpStatus.Foreground = new SolidColorBrush(Colors.Black);

                Util.GridSetData(dgDOCV, dsRslt.Tables["RET_DELTA_OCV"], this.FrameOperation);
                Util.GridSetData(dgWLowVolt, dsRslt.Tables["RET_W_LOW_VOLT"], this.FrameOperation);

                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("TIME_OVER_YN");
                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("JUDG_OP_YN");
                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("PROFILE_YN");

                Util.GridSetData(dgProcess, dsRslt.Tables["RET_TRAY_PROCESS"], this.FrameOperation);
                iLastRow = -1;

                //마지막 공정 Row 저장(CURR_YN = 'Y'일 때만)

                if (dgProcess.Rows.Count > 0 && Util.NVC(DataTableConverter.GetValue(dgProcess.Rows.Count - 1, "CURR_YN")).Equals("Y"))
                {
                    iLastRow = dgProcess.GetRowCount() - 1;
                }

                if (!_FinCheck) //"P"
                {

                    if (dsRslt.Tables["RET_DEGAS_ESTIMATED"] != null && dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows.Count > 0
                        && !string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["STARTTIME"]))
                        && !string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["ENDTIME"])))
                    {
                        DataGridRowAdd(dgProcess, 1);
                        int liRow = dgProcess.Rows.Count - 1;
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROCNAME", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["OP_NAME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WIPDTTM_ST", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["STARTTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WIPDTTM_ED", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["ENDTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WORKTIME", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["WORKTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "EQP_ID", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["EQP_ID"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROCID", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["OP_ID"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "JUDG_OP_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["JUDG_OP_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROFILE_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["PROFILE_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "IRR_HIST_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["IRR_HIST_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "CURR_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["CURR_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "TIME_OVER_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["TIME_OVER_YN"].ToString());
                        //   RowHeader

                        /*dgProcess.Rows[liRow].HeaderPresenter.text
                           fpsProcess.ActiveSheet.RowHeader.Rows[liRow].Label = ObjectDic.GetObjectName("FORECAST");  //예상

                        if (dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["TIME_OVER_YN"].ToString().Equals("Y"))
                            dgProcess.Rows[liRow].Presenter.Background = new SolidColorBrush(Colors.Red);
                        else
                            dgProcess.Rows[liRow].Presenter.Background = new SolidColorBrush(Colors.Yellow);*/        
                    }
                }

                //viewpoint
                //진행공정목록 마지막 Row로 Scroll View Point 설정
                //fpsProcess.SetViewportTopRow(0, fpsProcess.ActiveSheet.Rows.Count);

                if (_sFinCD.Equals("D") || _sFinCD.Equals("H"))
                {
                    // btnDelete.Content = ObjectDic.Instance.GetObjectName("INFO_RESTORE");  //정보복원
                    btnHistory.IsEnabled = false;
                }
                else
                {
                    // btnDelete.Content = ObjectDic.Instance.GetObjectName("INFO_DEL");  //정보삭제

                    //마지막 공정일 경우만 btnHistory 활성화 시킴
                    btnHistory.IsEnabled = false;
                    if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].Equals(dsRslt.Tables["RET_LAST_OP"].Rows[0]["PROCID"])) btnHistory.IsEnabled = true;
                }

                //int RowCount = (Util.NVC_Int(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["INPUT_SUBLOT_QTY"].ToString()) + 1) / 2;
                int RowCount = 18;
                if (RowCount > 0)
                {

                    DataGridRowAdd(dgCell, RowCount);
                    for (int i = 0; i < RowCount; i++)
                    {
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CELL_NO", i + 1);
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CELL_NO1", RowCount + i + 1);
                    }
                    SetRadioButton(_sCurrOper, 0);
                }
                if (_sCurrOper.Substring(2, 1).Equals(LOWAGING_EQPTYPE) || _sCurrOper.Substring(2, 1).Equals(OUTAGING_EQPTYPE) || _sCurrOper.Substring(2, 1).Equals(PREAGING_EQPTYPE))
                {
                    if (_AgingOutPriority == 9)
                    {
                        btnForceOut.IsEnabled = false;
                        btnSampleOut.IsEnabled = false;
                        btnCTSampleOut.IsEnabled = false;
                    }
                    else
                    {
                        btnForceOut.IsEnabled = true;
                        btnSampleOut.IsEnabled = true;
                        btnCTSampleOut.IsEnabled = true;
                        //MENUAUTH
                        /*if (MENUAUTH.Equals("W"))
                        {
                            btnForceOut.Enabled = true;
                            btnSampleOut.Enabled = true;
                        }*/
                    }
                }
                else
                {
                    btnForceOut.IsEnabled = false;
                    btnSampleOut.IsEnabled = false;
                }

                //Sample 출고해제 추가
                if (_AgingOutPriority == 7)
                {
                    //MANUAUTH
                    //if (MENUAUTH.Equals("W"))
                    btnSampleOut.IsEnabled = true;
                }

                // 대기일때 무조건 비활성화(김룡근)
                if (_AgingOutPriority != 7 && dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                {
                    btnSampleOut.IsEnabled = false;
                }

                //샘풀출고중인 경우 btnCTSampleOut 버튼 비활성화
                if (_AgingOutPriority == 7 && _CtType != "Y")
                {
                    btnCTSampleOut.IsEnabled = false;
                }
                if(_AgingOutPriority == 7 && _CtType == "Y")
                {
                    btnSampleOut.IsEnabled = false;
                }

               //CT 검사 정보 출력
               Util.GridSetData(dgCT, dsRslt.Tables["RET_CT_INFO"], this.FrameOperation);

                //  2023.08.15 NA1동 요청 (Wetting Grid , Hpcd 전류 ) 사용 NA 1동만 보이게 추가          
                if(bUseFlag)         		
                Util.GridSetData(dgWetting, dsRslt.Tables["RET_WETTING_INFO"], this.FrameOperation);

                //수동포트 반송 해제 버튼 활성화/비활성화
                if (gFMPC.Rows[0]["FORM_FORC_MANL_PORT"].ToString().Equals("N") || string.IsNullOrEmpty(gFMPC.Rows[0]["FORM_FORC_MANL_PORT"].ToString()))
                {
                    btnChgDfctLmtOverFlag.IsEnabled = false;
                }
                else
                {
                    btnChgDfctLmtOverFlag.IsEnabled = true;
                }

                //비정상 반송해제 활성화/비활성화
                if(dsTrayAbnormalCodeRslt.Rows.Count > 0)
                {
                    if(dsTrayAbnormalCodeRslt.Rows[0]["ABNORM_TRF_RSN_CODE"].ToString().Equals("N") || string.IsNullOrEmpty(dsTrayAbnormalCodeRslt.Rows[0]["ABNORM_TRF_RSN_CODE"].ToString()))
                    {
                        btnRlsTrfRsnCode.IsEnabled = false;
                    }
                    else
                    {
                        btnRlsTrfRsnCode.IsEnabled = true;
                    }
                }

                //Crack 버튼 활성화/비활성화
                if (string.IsNullOrEmpty(txtCrackState.Text))
                {
                    btnCrackChg.IsEnabled = false;
                }
                else
                {
                    btnCrackChg.IsEnabled = true;
                }

                // 상대판정정보 : 데이터가 있을때만 버튼 활성화
                GetRelativeRJudgeSpec(_sTrayNo, null, (dtRJudg, exRJudg) =>
                {
                    if (exRJudg != null)
                    {
                        btnRelativeData.IsEnabled = false;
                        btnRelativeData.Tag = null;

                        Util.MessageException(exRJudg);
                        return;
                    }

                    if (dtRJudg != null && dtRJudg.Rows.Count > 0)
                    {
                        btnRelativeData.IsEnabled = true;
                        btnRelativeData.Tag = dtRJudg;
                    }
                    else
                    {
                        btnRelativeData.IsEnabled = false;
                        btnRelativeData.Tag = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ClearControlValue()
        {
            try
            {
                _sTrayLine = string.Empty;
                txtLotID.Text = string.Empty;
                txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                _sLotID = string.Empty;
                // txtCALot.Text = string.Empty;
                // txtCALot.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                // txtANLot.Text = string.Empty;
                // txtANLot.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtROUTID.Text = string.Empty;
                txtROUTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Text = string.Empty;
                txtOper.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Tag = string.Empty;
                txtInCellCnt.Text = string.Empty;
                txtInCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNowCellCnt.Text = string.Empty;
                txtNowCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                _sFinCD = string.Empty;
                _sEqpID = string.Empty;
                _sModelID = string.Empty;
                txtTrayOpStatus.Text = string.Empty;
                txtTrayOpStatus.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayOpStatus.Tag = string.Empty;
                txtNextOp.Text = string.Empty;
                txtNextOp.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtPRODID.Text = string.Empty;
                txtPRODID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtDfctLmtOverFlag.Text = string.Empty;
                txtDfctLmtOverFlag.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtAbnormTrfRsnCode.Text = string.Empty;
                txtAbnormTrfRsnCode.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtCrackState.Text = string.Empty;
                txtCrackState.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtSpc.Text = string.Empty;
                txtSpSelect.Text = string.Empty;
                txtSpDes.Text = string.Empty;
                txtTrayID.Foreground = new SolidColorBrush(Colors.Black);
                lblSpecial.Foreground = new SolidColorBrush(Colors.Black);
                txtSpclRelDate.Text = string.Empty;
                //txtRange1.Text = string.Empty;
                //txtRange2.Text = string.Empty;
                //txtCellList.Text = string.Empty;
                txtOpPlanTime.Text = string.Empty;
                txtOpPlanTime.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtLotHold.Text = string.Empty;
                txtLotHold.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtLotTYPE.Text = string.Empty;
                txtLotTYPE.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtDummyMaker.Text = string.Empty;
                txtDummyMaker.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                //모든 라디오버튼 비활성화
                foreach (Control c in rdoGroup.Children)
                {
                    if (c.GetType().Equals(typeof(RadioButton)))
                    {
                        (c as RadioButton).Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
                        (c as RadioButton).IsEnabled = false;
                    }
                }

                //모든 버튼 비활성화
                SetButtonEnable(false);
                
                Util.gridClear(dgProcess);
                Util.gridClear(dgCell);
                Util.gridClear(dgDOCV);
                Util.gridClear(dgTemp);
                Util.gridClear(dgWLowVolt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetRadioButton(string pCurrOper, int pRow)
        {
            rdoCapa.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoCurr.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoCurr.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoOCV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoImp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoFittedCapa.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            //200408 KJE : 용량 선별화 판정 추가
            rdoFittedCapaSAS.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoPreOCV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoPredOCV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoFittedImp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdonCCVal.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdonCVVal.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdonCCTime.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdonCVTime.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoGrade.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            // 2014.02.26 강진구D // 2491135   // 대용량 방전기 전압값 표시 件 / ( 대용량 방전 전, 후 전압 조회 기능 추가)
            rdoPGSVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style; //Power Grading Start
            rdoPGEVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style; //Power Grading End
            /*[CSR ID:2407996] Ftting 용량 UI 적용의 건
             * 수정내용 : fitted capacity 조회시
             * 수정자 : 정종덕D
             */
            //[180807 이수진C] JIG CNB 관련 추가
            rdoJigCnbPress.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoJigCnbTemp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            //191128 scpark 가압단락검사 관련 추가
            rdoDeltaTemp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            //200120 scpark 가압단락검사 압력 추가
            rdoHPCDPress.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            //20230316 suyong.kim 보정dOCV 추가
            rdoFittedOCV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;

            //20230815 손동혁 HPCD전류 추가
            rdoHPCDCurr.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;

            //20230925 김용식 HPCD 초기전압, 종료전압 추가
            rdoIniVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
            rdoEndVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;

            foreach (Control c in rdoGroup.Children)
            {
                if (c.GetType().Equals(typeof(RadioButton)))
                {
                    (c as RadioButton).IsChecked = false;
                    (c as RadioButton).IsEnabled = false;
                }

            }

            switch (pCurrOper.Substring(2, 2))
            {
                case "13":

                case "81":
                case "A1": //13: OCV, 81: 전용OCV, A1:DELTA OCV , J3: JIG OCV
                case "S3":
                    rdoHPCDPress.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoHPCDPress.IsEnabled = true;
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdoPredOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPredOCV.IsEnabled = true;
                    rdoPreOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPreOCV.IsEnabled = true;
                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;
                case "J0": //JIG 대기
                    GetDataQuery(DEFAULT_VEW_CELL);
                    break;
                case "J1": //J1: JIG 충전
                case "J2": //J2: JIG 방전
                    rdoJigCnbPress.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbPress.IsEnabled = true;
                    rdoJigCnbTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbTemp.IsEnabled = true;
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;
                case "J3":
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoJigCnbTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbTemp.IsEnabled = true;
                    rdoJigCnbPress.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbPress.IsEnabled = true;
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;
                case "11": // 
                case "12": //11: 충전, 12: 방전
                case "S1":
                case "S2":
                    /* [CSR ID:2619904] Fitting용량 산출을 위한 기준 조건 추가 요청 건
                     * 수정내용 : fitted capacity 조회시(충전공정시 추가)
                     * 수정자 : 윤홍기D
                     */
                    if (pCurrOper.Substring(2, 2).Equals("11"))
                    {
                        rdonCCVal.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdonCCVal.IsEnabled = true;
                        rdonCVVal.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdonCVVal.IsEnabled = true;
                        rdonCCTime.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdonCCTime.IsEnabled = true;
                        rdonCVTime.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdonCVTime.IsEnabled = true;
                        rdoPreOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoPreOCV.IsEnabled = true;
                    }
                    /*[CSR ID:2407996] Ftting 용량 UI 적용의 건
                     * 수정내용 : fitted capacity 조회시(방전공정시만)
                     * 수정자 : 정종덕D
                     */
                    if (pCurrOper.Substring(2, 2).Equals("12"))
                    {
                        rdoPreOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoPreOCV.IsEnabled = true;
                        rdoFittedCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoFittedCapa.IsEnabled = true;
                    }
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;
                case "41":
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoGrade.IsChecked = true;
                    rdo_Click(rdoGrade, new RoutedEventArgs());
                    break;
                case "14":
                case "15":
                case "16":  //41: AC-IMP, 14:DC-IMP, 15: 충전DC-IMP, 16: 방전DC-IMP ,
                    rdoImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoImp.IsEnabled = true;
                    rdoImp.IsChecked = true;
                    rdoFittedImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoFittedImp.IsEnabled = true;
                    rdo_Click(rdoImp, new RoutedEventArgs());
                    break;
                case "17": //17 : POWER GRADE
                    rdoImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoImp.IsEnabled = true;
                    rdoImp.IsChecked = true;
                    rdoFittedImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoFittedImp.IsEnabled = true;
                    rdoCurrA.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurrA.IsEnabled = true;
                    // 2014.02.26 강진구D // 2491135   // 대용량 방전기 전압값 표시 件 / ( 대용량 방전 전, 후 전압 조회 기능 추가)
                    rdonCCVal.IsEnabled = false;
                    rdonCCTime.IsEnabled = false;
                    rdonCVVal.IsEnabled = false;
                    rdonCVTime.IsEnabled = false;

                    /* 2015-04-02   정종덕   [CSR ID:2741066] FCS,FSA 내 고율 충전 공정 추가 요청의 件
                     * 변경 내용 :  rdoPGSVolt, rdoPGEVolt  명칭변경
                     * */
                    rdoPGSVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPGSVolt.IsEnabled = true;
                    rdoPGEVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPGEVolt.IsEnabled = true;

                    rdo_Click(rdoImp, new RoutedEventArgs());
                    break;
                /* 2015-04-02   정종덕   [CSR ID:2741066] FCS,FSA 내 고율 충전 공정 추가 요청의 件
                 * 변경 내용 :  Power Charge 공정
                 */
                case "19": //19 : POWER Charge
                    rdoImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoImp.IsEnabled = true;
                    rdoImp.IsChecked = true;
                    /* 2015-04-28   정종덕   [CSR ID:2721809] FCS,FSA 내 고율 충전 공정 추가 요청의 件
                     * 변경 내용 : fitting DCIR 충전 false -> true
                     */
                    rdoFittedImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoFittedImp.IsEnabled = true;  //fitted 삭제 개발후 오픈 해야함
                    rdoCurrA.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurrA.IsEnabled = true;

                    // 2014.02.26 강진구D // 2491135   // 대용량 방전기 전압값 표시 件 / ( 대용량 방전 전, 후 전압 조회 기능 추가)
                    rdonCCVal.IsEnabled = false;
                    rdonCCTime.IsEnabled = false;
                    rdonCVVal.IsEnabled = false;
                    rdonCVTime.IsEnabled = false;
                    rdoPGSVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPGSVolt.IsEnabled = true;
                    rdoPGEVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPGEVolt.IsEnabled = true;

                    rdo_Click(rdoImp, new RoutedEventArgs());
                    break;
                case "B1":
                    if (pCurrOper.Equals("FFB104"))
                    {
                        rdoPreOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoPreOCV.IsEnabled = true;
                        rdoPreOCV.IsChecked = true;
                        rdoPredOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoPredOCV.IsEnabled = true;
                        rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoGrade.IsEnabled = true;
                        rdo_Click(rdoPreOCV, new RoutedEventArgs());
                    }
                    else
                    {
                        rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoGrade.IsEnabled = true;
                        rdoGrade.IsChecked = true;
                        rdoFittedCapaSAS.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoFittedCapaSAS.IsEnabled = true;  //200408 KJE : 용량 선별화 판정 추가
                        rdoPredOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoPredOCV.IsEnabled = true;
                        rdo_Click(rdoGrade, new RoutedEventArgs());
                    }
                    break;
                //[180807 이수진C] JIG CNB 관련 추가
                case "J5":
                case "J7":
                    rdoJigCnbPress.IsChecked = true;
                    rdoJigCnbPress.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbPress.IsEnabled = true;
                    rdoJigCnbTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoJigCnbTemp.IsEnabled = true;
                    rdo_Click(rdoJigCnbPress, new RoutedEventArgs());
                    break;
                //[190807 김지은] 가압단락검사 관련 추가
                case "U1":
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoCurr.IsChecked = true;
                    
                    //20230925 김용식 HPCD 초기전압, 종료전압 추가
                    // ESNJ 
                    rdoIniVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIniVolt.IsEnabled = true;
                    rdoEndVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoEndVolt.IsEnabled = true;

                    rdoDeltaTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoDeltaTemp.IsEnabled = true;
                    rdoHPCDPress.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoHPCDPress.IsEnabled = true;
                    rdoHPCDCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoHPCDCurr.IsEnabled = true;
                    rdo_Click(rdoCurr, new RoutedEventArgs());
                    break;
                default:
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoGrade.IsChecked = true;
                    rdo_Click(rdoGrade, new RoutedEventArgs());
                    break;
            }
            if (pCurrOper.Substring(2, 2).Equals("17")|| pCurrOper.Substring(2,2).Equals("19"))
            {
                rdoCurr.Visibility = Visibility.Collapsed;
                rdoCurrA.Visibility = Visibility.Visible;
            }
            else
            {
                rdoCurrA.Visibility = Visibility.Collapsed;
                rdoCurr.Visibility = Visibility.Visible;
            }

            //2023.03.16 suyong.kim DOCV2 추가
            if (pCurrOper.Substring(2, 4) == "A101")
            {
                rdoFittedOCV.IsEnabled = true;
                //rdoFittedOCV.IsChecked = true;
                rdoFittedOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                //rdo_Click(rdoFittedOCV, new RoutedEventArgs());
            }
        }
        private void SetRange()
        {
            dtValueSublot.Rows.Clear();
            if (string.IsNullOrEmpty(txtRange1.Text) || string.IsNullOrEmpty(txtRange2.Text))
                return;
            bsetRange = true;

            string range1 = txtRange1.Text.ToUpper();
            string range2 = txtRange2.Text.ToUpper();

            int col1 = dgCell.Columns["VALUE"].Index;
            int col2 = dgCell.Columns["VALUE1"].Index;

            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                for (int j = col1; j <= col2; j = j + 3) //측정값 컬럼
                {
                    string value = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).ToUpper();

                    if (!IsNumeric(value)) //범위가 등급일 경우
                    {
                        if (value.Equals(range1) || value.Equals(range2))
                        {
                            DataRow dr = dtValueSublot.NewRow();
                            dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j - 1].Name));
                            dtValueSublot.Rows.Add(dr);
                        }
                    }

                    else  //범위가 측정값일 경우
                    {
                        if (!IsNumeric(range1) || !IsNumeric(range2)) return;
                        if (double.Parse(value) >= double.Parse(range1) && double.Parse(value) <= double.Parse(range2))
                        {
                            DataRow dr = dtValueSublot.NewRow();
                            dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j - 1].Name));
                            dtValueSublot.Rows.Add(dr);
                        }
                    }
                }
            }

            //LoadedCellPresenter에서 처리
            DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);
            Util.GridSetData(dgCell, dtTemp, this.FrameOperation);
        }

        private void SetCellRange()
        {
            dtSublotList.Rows.Clear();
            if (string.IsNullOrEmpty(txtCellList.Text)) return;
            bsetCellRange = true;

            string[] arrCell = txtCellList.Text.Split(',');
            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                int col1 = dgCell.Columns["SUBLOTID"].Index;  //CELL ID 1 컬럼
                int col2 = dgCell.Columns["SUBLOTID1"].Index; //CELL ID 2 컬럼
                for (int j = col1; j <= col2; j += 3)
                {
                    string curCell = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name));
                    for (int k = 0; k < arrCell.Length; k++)
                    {
                        if (curCell.Equals(arrCell[k]))
                        {
                            DataRow dr = dtSublotList.NewRow();
                            dr["SUBLOTID"] = curCell;
                            dtSublotList.Rows.Add(dr);
                        }
                    }
                }
            }

            //LoadedCellPresenter에서 처리
            DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);
            Util.GridSetData(dgCell, dtTemp, this.FrameOperation);
        }
        private void GetTemp()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _sTrayNo;
                dr["PROCID"] = _sCurrOper;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAYINFO_BY_TRAYNO_TEMP", "INDATA", "OUTDATA", inDataTable);
                Util.gridClear(dgTemp);

                if (dtRslt.Rows.Count > 0)
                {
                    DataGridRowAdd(dgTemp, 1);
                }

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    DataTableConverter.SetValue(dgTemp.Rows[0].DataItem, "TEMP" + drRslt[0].ToString(), drRslt[1].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetDataQuery(string pDataType)
        {
            DataSet InDataSet = new DataSet();
            DataSet OutDataSet = new DataSet();

            string sDataType = string.Empty;
            string sBizID = string.Empty;
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("OP_START_TIME", typeof(string));
                inDataTable.Columns.Add("MEAS_TYPE_CD", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _sTrayNo;
                dr["EQPTID"] = _sEqpID;
                dr["PROCID"] = _sCurrOper;
                dr["OP_START_TIME"] = _sOPStartTime;
                dr["MEAS_TYPE_CD"] = pDataType;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID ;

                inDataTable.Rows.Add(dr);
                if (_sCurrYN != "N")
                {
                    switch (pDataType)
                    {
                        // 기본정보 Cell 보기
                        case DEFAULT_VEW_CELL:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_DEFAULT_VIEW";
                            break;
                        // 2011.09.19 AUTO CALIBRATION 기능추가로 인해 수정.
                        case CAPA_DATATYPE:
                            if (_FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CAPA_AUTO_CALI";
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CAPA";
                            }
                            break;
                        /*[CSR ID:2407996] Ftting 용량 UI 적용의 건
                         * 수정내용 : fitted capacity 조회시
                         * 수정자 : 정종덕D
                         */
                        case FITTEDCAPACITY_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_FITCAPA";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FITCAPA_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FITCAPA";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        //200408 KJE : 용량 선별화 판정 추가
                        case FITTEDCAPACITY_SAS_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_SAS";
                            break;

                        case CURR_DATATYPE:
                            if (_sCurrOper.Substring(2, 2).Equals("U1"))
                            {
                                //2021.10.12 Auto Calibration Lot 표시 START
                                ////200120 KJE : 가압단락검사기 측정전류
                                //sBizID = "DA_SEL_LOAD_CELL_DATA_CURRMU";
                                if (bAtCalibUse && _FinCheck)
                                {
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_CURRMU_AUTO_CALI"; // 개발완료
                                }
                                else
                                {
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_CURRMU";
                                }
                                //2021.10.12 Auto Calibration Lot 표시 END
                            }
                            else
                            {
                                if (_FinCheck)
                                {
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_CURR_AUTO_CALI";
                                }
                                else
                                {
                                    /* 2015-04-02   정종덕   [CSR ID:2741066] FCS,FSA 내 고율 충전 공정 추가 요청의 件
                                     * 변경 내용 :  Power Charge 공정 조건 추가 "19"
                                     * */
                                    if (_sCurrOper.ToString().Substring(2, 2).Equals("17") || _sCurrOper.ToString().Substring(2, 2).Equals("19"))
                                        sBizID = "DA_SEL_LOAD_CELL_DATA_PG_CURR";
                                    else
                                        sBizID = "DA_SEL_LOAD_CELL_DATA_CURR";
                                }
                            }
                            break;

                        case VOLT_DATATYPE:
                            if (_sCurrOper.Substring(2, 2).Equals("U1"))
                            {
                                //2021.10.12 Auto Calibration Lot 표시 START
                                //sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_VOLT";
                                if (bAtCalibUse && _FinCheck)
                                {
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_VOLT_AUTO_CALI"; // 개발완료
                                }
                                else
                                {
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_VOLT";
                                }
                                //2021.10.12 Auto Calibration Lot 표시 END
                            }
                            else
                            {
                                if (_FinCheck)
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_VOLT_AUTO_CALI";
                                else
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_VOLT";
                            }
                            break;

                        case OCV_DATATYPE:
                            if (_FinCheck)
                                sBizID = "DA_SEL_LOAD_CELL_DATA_OCV_AUTO_CALI";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_OCV";
                            break;

                        case IMP_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_PW";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PW_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PW";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        case FITTEDIMP_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_FITPW";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FITPW_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FITPW";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        case CCVAL_DATATYPE:
                            if (_FinCheck)
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CCVAL_AUTO_CALI";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CCVAL";
                            break;

                        case CVVAL_DATATYPE:
                            if (_FinCheck)
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CVVAL_AUTO_CALI";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CVVAL";
                            break;

                        case CCTIME_DATATYPE:
                            if (_FinCheck)
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CCTIME_AUTO_CALI";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CCTIME";
                            break;

                        case CVTIME_DATATYPE:
                            if (_FinCheck)
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CVTIME_AUTO_CALI";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CVTIME";
                            break;

                        /* 2014.1.17 정종덕D // [CSR ID:2444504] 자동차 셀 전 모델 및 호기 W-code 및 비선형 mV/day 기준 정보 등록 
                        * 저전압 OCV판정 데이터 조회 추가
                        * */
                        case LOWVOLTAGE_OCVJUDG_OCV_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_LOWVOLAGE_OCV";
                            break;

                        case LOWVOLTAGE_OCVJUDG_DOCV_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_LOWVOLAGE_DOCV";
                            break;

                        //2014.02.26 강진구D // 2491135   // 대용량 방전기 전압값 표시 件 / ( 대용량 방전 전, 후 전압 조회 기능 추가)
                        case POWERGRADING_STRAT_VOLT_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_PGS_VOLT";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PGS_VOLT_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PGS_VOLT";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        case POWERGRADING_END_VOLT_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_PGE_VOLT";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PGE_VOLT_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PGE_VOLT";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        //[180807 이수진C] JIG CNB 관련 추가
                        case JIG_CNB_PRESS_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_PRESS";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_PRESS_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_PRESS";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;
                        case JIG_CNB_TEMP_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_TEMP";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_TEMP_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_TEMP";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;
                        case CURRMU_DATATYPE:
                            sBizID = "";
                            break;
                        case DELTA_TEMP:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_DELTA_TEMP";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_DELTA_TEMP_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_DELTA_TEMP";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;
                        case HPCDPRESS_DATATYPE:
                            //2021.10.12 Auto Calibration Lot 표시 START
                            //sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_PRESS";
                            if (bAtCalibUse && _FinCheck)
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_PRESS_AUTO_CALI"; // 개발완료
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_PRESS";
                            }
                            //2021.10.12 Auto Calibration Lot 표시 END
                            break;

                        ////2023.03.16 suyong.kim DOCV2 추가
                        case FITTED_OCV_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_FITTED_DOCV";
                            break;
                        ///2023.08.15 dhson95 가압단락 최소전류,최대전류 추가
                        case HPCDCurr_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_CURR";
                            break;
                        case INIT_VOLT_DATATYPE:
                            //20230925 김용식 HPCD 초기전압, 종료전압 추가
                            sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_INIT_VOLT";
                            break;
                        case END_VOLT_DATATYPE:
                            //20230925 김용식 HPCD 초기전압, 종료전압 추가
                            sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_END_VOLT";
                            break;
                        default:
                            if (_sFinCD.Equals("P"))
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_P";
                            else
                            {
                                if (_sCurrOper.Substring(2, 2).Equals("61"))
                                    sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_C_SELECT";
                                else
                                {
                                    if (_sFinCD == "C")
                                        sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_C";
                                    else
                                        sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD";
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (pDataType)
                    {
                        // 기본정보 Cell 보기
                        case DEFAULT_VEW_CELL:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_DEFAULT_VIEW";
                            break;
                        case CURR_DATATYPE:
                            if (_sCurrOper.Substring(2, 2).Equals("U1"))
                            {
                                //200120 KJE : 가압단락검사기 측정전류
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CURRMU_HIST";
                            }
                            else if (_sCurrOper == "171")
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PG_CURR_HIST";
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CHG_HIST";
                            }
                            break;
                        case CAPA_DATATYPE:
                        case VOLT_DATATYPE:
                        /*[CSR ID:2407996] Ftting 용량 UI 적용의 건
                         * 수정내용 : fitted capacity 조회시
                         * 수정자 : 정종덕D
                         */
                        case FITTEDCAPACITY_DATATYPE:
                            if (_sCurrOper != "171")
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CHG_HIST";
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_PG_CURR_HIST";
                            }
                            break;
                        //200408 KJE : 용량 선별화 판정 추가
                        case FITTEDCAPACITY_SAS_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_SAS";
                            break;
                        case OCV_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_OCV_HIST";
                            break;
                        case IMP_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_PW_HIST";
                            break;
                        case FITTEDIMP_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_FITPW_HIST";
                            break;
                        case CCVAL_DATATYPE:
                        case CVVAL_DATATYPE:
                        case CCTIME_DATATYPE:
                        case CVTIME_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_CCV_HIST";
                            break;
                        case CURRMU_DATATYPE:
                            sBizID = "";
                            break;

                        case DELTA_TEMP:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_DELTA_TEMP_HIST";
                            break;

                        case HPCDPRESS_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_PRESS";
                            break;
                        case HPCDCurr_DATATYPE:
                            sBizID = "DA_SEL_LOAD_CELL_DATA_HPCD_CURR";
                            break;
                            
                        default:
                            if (_sFinCD.Equals("C"))
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_C";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD";
                            break;
                    }
                }


                DataTable dtRslt = new ClientProxy() .ExecuteServiceSync(sBizID, "INDATA", "OUTDATA", inDataTable);
                dtWipCell = dtRslt.Copy();

                if (dtRslt.Rows.Count > 0)
                {
                    int rowIdx = (dtRslt.Rows.Count + 1) / 2;

                    // 목록 갯수에 따른 열 조정
                    dgCell.ClearRows();
                    DataGridRowAdd(dgCell, rowIdx);
                    for (int i = 0; i < rowIdx; i++)
                    {
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CELL_NO", i + 1);
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CELL_NO1", rowIdx + i + 1);
                    }

                    //cell 목록 2열로 표현하기 위해 for 사용
                    for (int iRow = 0; iRow < rowIdx; iRow++)
                    {
                        DataRow[] drCell = dtRslt.Select(" CSTSLOT = " + Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "CELL_NO")));
                        if (drCell.Length > 0)
                        {
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID", drCell[0]["SUBLOTID"]);
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE", drCell[0]["VALUE"]);
                            //dgCell.Rows[iRow].Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID", dtRslt.Rows[iRow]["SUBLOTID"]);
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE", dtRslt.Rows[iRow]["VALUE"]);
                        }
                        //cell 개수가 홀수개일 때,
                        if (dtRslt.Rows.Count <= iRow + rowIdx) { continue; }
                        DataRow[] drCell1 = dtRslt.Select(" CSTSLOT = " + Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "CELL_NO1")));
                        if (drCell1.Length > 0)
                        {
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID1", drCell1[0]["SUBLOTID"]);
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE1", drCell1[0]["VALUE"]);
                            //dgCell.Rows[iRow].Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID1", dtRslt.Rows[iRow + rowIdx]["SUBLOTID"]);
                            DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE1", dtRslt.Rows[iRow + rowIdx]["VALUE"]);
                        }
                    }
                    DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);
                    Util.gridClear(dgCell);
                    Util.GridSetData(dgCell, dtTemp, this.FrameOperation);     
                    // SetRange();
                    // SetCellRange();

                    #region [제품검사여부 확인 표시 등급+(제품검사)]
                    if ((bool)rdoGrade.IsChecked)
                    {
                        DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_CELL_DATA_FINCD_PQC", "INDATA", "OUTDATA", dtRslt);

                        //20220128_O등급 표시 : O(공정검사의뢰) , O(PQC검사 의뢰) START
                        //for (int iRow = 0; iRow < dgCell.Rows.Count(); iRow++)
                        //{
                        //    if (dtRslt1.Select(" SUBLOTID = '" + Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID")) + "'").Length > 0)
                        //    {
                        //        DataTableConverter.SetValue(dgCell.Rows[iRow], "VALUE", Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow], "VALUE")) + "(" + ObjectDic.Instance.GetObjectName("PROD_INSP") + ")");
                        //    }
                        //    if (dtRslt1.Select(" SUBLOTID = '" + Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID1")) + "'").Length > 0)
                        //    {
                        //        DataTableConverter.SetValue(dgCell.Rows[iRow], "VALUE1", Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow], "VALUE1")) + "(" + ObjectDic.Instance.GetObjectName("PROD_INSP") + ")");
                        //    }
                        //}

                        if (dtRslt1.Rows.Count > 0)
                        {
                            for (int iRow = 0; iRow < dgCell.Rows.Count(); iRow++)
                            {
                                string sSubLotID = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID"));
                                string sSubLotID1 = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "SUBLOTID1"));

                                string sValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "VALUE"));
                                string sValue1 = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, "VALUE1"));

                                string sType = string.Empty;
                                string sType1 = string.Empty;


                                if (dtRslt1.Select(" SUBLOTID = '" + sSubLotID + "'").Length > 0)
                                {
                                    DataRow[] drInfo = dtRslt1.Select(" SUBLOTID = '" + sSubLotID + "'");
                                    if (drInfo.Length > 0)
                                    {
                                        sType = Util.NVC(drInfo[0]["TYPE"]);
                                    }
                                    DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE", sValue + "(" + ObjectDic.Instance.GetObjectName(sType) + ")");
                                }
                                if (dtRslt1.Select(" SUBLOTID = '" + sSubLotID1 + "'").Length > 0)
                                {
                                    DataRow[] drInfo = dtRslt1.Select(" SUBLOTID = '" + sSubLotID1 + "'");
                                    if (drInfo.Length > 0)
                                    {
                                        sType1 = Util.NVC(drInfo[0]["TYPE"]);
                                    }
                                    DataTableConverter.SetValue(dgCell.Rows[iRow].DataItem, "VALUE1", sValue1 + "(" + ObjectDic.Instance.GetObjectName(sType1) + ")");
                                }
                            }
                        }
                        //20220128_O등급 표시 : O(공정검사의뢰) , O(PQC검사 의뢰) END
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        public static Boolean IsNumeric(string pTarget)
        {
            double dNullable;
            return double.TryParse(pTarget, System.Globalization.NumberStyles.Any, null, out dNullable);
        }
        public static Boolean IsNumeric(object oTagraet)
        {
            return IsNumeric(oTagraet.ToString());
        }
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int rowCount)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();

                int addRows = 0;
                if (Math.Abs(rowCount) > 0)
                {
                    if (rowCount + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else
                    {
                        addRows = rowCount;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetButtonEnable(bool enable)
        {
            btnManual.IsEnabled = enable;
            btnCell.IsEnabled = enable;
            btnDOCV.IsEnabled = enable;
            btnAging.IsEnabled = enable;
            btnHistory.IsEnabled = enable;
            btnForceOut.IsEnabled = enable;
            btnSampleOut.IsEnabled = enable;
            btnCTSampleOut.IsEnabled = enable;
            btnSpecial.IsEnabled = enable;
            btnGrade.IsEnabled = enable;
            btnChgHist.IsEnabled = enable;
            btnChgDfctLmtOverFlag.IsEnabled = enable;
            btnRlsTrfRsnCode.IsEnabled = enable;
            btnCrackChg.IsEnabled = enable;
            btnWGradeJudge.IsEnabled = enable;
        }

        private object setTableData(string dataFormat, string setData = "", DataTable dt = null, DataRow dr = null)
        {
            var _dataFormat = dataFormat.Split(',');

            if (dt != null)
            {
                getResultData(dt, _dataFormat);

                return dt;
            }

            if (dr != null)
            {
                var _setData = setData.Split(',');
                getResultData(dr, _dataFormat, _setData);

                return dr;
            }

            return null;
        }

        private DataTable getResultData(DataTable dt, string[] str)
        {
            str.ToList().ForEach((retStr) => dt.Columns.Add(retStr, typeof(string)));

            return dt;
        }

        private DataRow getResultData(DataRow dr, string[] str, string[] data)
        {
            foreach (var tmp in str.Select((val, idx) => new { val, idx }))
            {
                dr[tmp.val] = data[tmp.idx];
            }

            return dr;
        }

        private DataTable getForcManlPortCode()
        {
            var _addColumnData = "LOTID,LANGID";

            DataTable _FstDataTable = (DataTable)setTableData(_addColumnData, "", new DataTable());

            _FstDataTable.Rows.Add((DataRow)setTableData(_addColumnData, $"{dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString()},{LoginInfo.LANGID}", null, _FstDataTable.NewRow()));

           return new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_FORC_MANL_PORT_CODE", "RQSTDT", "RSLTDT", _FstDataTable);
        }

        public void GetRelativeRJudgeSpec(string sLOTID, string sPROCID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["LOTID"] = sLOTID;
            Indata["PROCID"] = sPROCID;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_SEL_RJUDG_SPEC_LIST", "RQSTDT", "RSLTDT", IndataTable, (Result, Exception) =>
            {
                try
                {
                    ACTION_COMPLETED?.Invoke(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            });
        }
        #endregion


        #region [Event]
        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && txtTrayID.Text.Length == 10)
                {
                    //StartLoader();
                    _sLotID = null;
                    _sTrayNo = null;
                    _sTrayID = txtTrayID.Text;
                    txtTrayNo.Text = null;
                    // txtTrayNo.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    ClearControlValue();
                    if ((bool)chkHist.IsChecked)
                    {
                        Util.gridClear(dgCell);

                        FCS001_021_SEL_TRAY sel_tray = new FCS001_021_SEL_TRAY();
                        sel_tray.FrameOperation = FrameOperation;

                        object[] parameters = new object[1];
                        parameters[0] = Util.NVC(txtTrayID.Text);

                        C1WindowExtension.SetParameters(sel_tray, parameters);
                        sel_tray.Closed += new EventHandler(sel_tray_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => sel_tray.ShowModal()));
                        return;
                    }
                    ClearControlValue();
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void sel_tray_Closed(object sender, EventArgs e)
        {
            FCS001_021_SEL_TRAY window = sender as FCS001_021_SEL_TRAY;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                txtTrayNo.Text = _sTrayNo;
                txtTrayNo.Focus();
            }
            this.grdMain.Children.Remove(window);
        }

        private void txtTrayNo_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    _sTrayNo = txtTrayNo.Text;
                    _sLotID = string.Empty;
                    txtTrayID.Text = string.Empty;
                    //txtTrayID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    ClearControlValue();
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
            //확인 메세지 팝업

            Util.MessageConfirm("FM_ME_0435", (result) =>  //동일한 Rack에 들어있는 모든 Tray에 대하여 특별등록 진행됩니다.
            {
                if (result == MessageBoxResult.OK)
                {
                    FCS001_021_SPECIAL_MANAGEMENT specialManagement = new FCS001_021_SPECIAL_MANAGEMENT();
                    specialManagement.FrameOperation = FrameOperation;

                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(txtTrayID.Text.Trim());
                    parameters[1] = "Y"; //적재된 Tray 특별관리 진행
                    C1WindowExtension.SetParameters(specialManagement, parameters);
                    specialManagement.Closed += new EventHandler(specialManagement_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => specialManagement.ShowModal()));
                    specialManagement.BringToFront();
                }
                else
                {
                    return;
                }
            });
        }

        private void specialManagement_Closed(object sender, EventArgs e)
        {
            FCS001_021_SPECIAL_MANAGEMENT window = sender as FCS001_021_SPECIAL_MANAGEMENT;

            //if (window.DialogResult == MessageBoxResult.Yes)
            if (window.sResultReturn == "Y") //E20230213-000118
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSampleOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["LOTID"] = _sTrayNo;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                //2023.01.23  TrayID  Null 일경우  Error
                if(string.IsNullOrEmpty(_sTrayNo))
                {
                    Util.Alert("FM_ME_0069");//TrayId값이 NULL 입니다.
                    return;
                }
                
                if(_AgingOutPriority == 7 && _CtType == "Y")
                {
                    Util.Alert("SFU9016");//CT 샘플출고 해제 버튼으로 해제하십시오.
                    return;
                }

                if (_AgingOutPriority == 7)
                {
                    Util.MessageConfirm("FM_ME_0009", (result) =>  //[Tray ID : {0}]를 Sample 해제하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_SAMPLE_IN", "INDATA", "OUTDATA", inDataTable);

                                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0067");  //Sample 해제를 완료하였습니다.
                                }
                                else
                                {
                                    Util.Alert("ME_0068");  //Sample 해제에 실패하였습니다.
                                }
                                ClearControlValue();
                                GetTrayInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                    }, new string[] { _sTrayID });
                }
                else
                {
                    Util.MessageConfirm("FM_ME_0008", (result) =>  //[Tray ID : {0}]를 Sample 출고 하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {

                          try
                          {
                            inDataSet.Tables.Add(inDataTable);
                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_SAMPLE_OUT", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", inDataSet);
                            if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.MessageInfo("FM_ME_0065");  //Sample 출고 지시를 완료하였습니다.
                                ClearControlValue();
                                GetTrayInfo();
                                if (dsRslt.Tables["OUT_SAMPLE_PORT"].Rows.Count > 0)
                                {
                                    //TSK_116 tsk116 = new TSK_116();
                                    //tsk116.OUTPORT = dsRslt.Tables["OUT_SAMPLE_PORT"];

                                    //if (tsk116.ShowDialog(this) == DialogResult.Yes)
                                    //{
                                    //}
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0066");
                                }
                              }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                    }, new string[] { _sTrayID });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //정보삭제 - 미사용
        //private void btnDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        DataTable inDataTable = new DataTable();
        //        inDataTable.Columns.Add("CSTID", typeof(string));
        //        inDataTable.Columns.Add("LOTID", typeof(string));
        //        inDataTable.Columns.Add("WIPSTAT", typeof(string));
        //        inDataTable.Columns.Add("USERID", typeof(string));

        //        if (_sFinCD.Equals("C") || _sFinCD.Equals("E") || _sFinCD.Equals("P"))
        //        {
        //            Util.MessageConfirm("FM_ME_0155", (result) => //삭제하시겠습니까?
        //            {
        //                if (result == MessageBoxResult.Cancel)
        //                {
        //                    return;
        //                }

        //                DataRow dr = inDataTable.NewRow();
        //                dr["TRAY_ID"] = _sTrayID;
        //                dr["TRAY_NO"] = _sTrayNo;
        //                dr[""] = _sFinCD;
        //                dr["USER_ID"] = LoginInfo.USERID;
        //                inDataTable.Rows.Add(dr);
        //                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_DEL_RECOVER", "INDATA", "OUTDATA", inDataTable);

        //                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
        //                {
        //                    Util.MessageInfo("FM_ME_0154"); //삭제완료하였습니다.
        //                    ClearControlValue();
        //                    _sFinCD = "D";
        //                    GetTrayInfo();
        //                }
        //                else if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("2"))
        //                {
        //                    Util.Alert("FM_ME_0110"); //공정이 종료되어야 삭제가능합니다.\r\n종료 후 삭제하세요.
        //                    return;
        //                }
        //                else
        //                {
        //                    Util.Alert("FM_ME_0153"); //삭제실패하였습니다.
        //                }

        //            });
        //        }
        //        else if (_sFinCD.Equals("D") || _sFinCD.Equals("H"))
        //        {
        //            Util.MessageConfirm("FM_ME_0141", (result) =>
        //            {
        //                if (result == MessageBoxResult.No) { return; }

        //                DataRow dr = inDataTable.NewRow();
        //                dr["TRAY_ID"] = _sTrayID;
        //                dr["TRAY_NO"] = _sTrayNo;
        //                dr["WIPSTAT"] = _sFinCD;
        //                dr["USER_ID"] = LoginInfo.USERID;
        //                inDataTable.Rows.Add(dr);
        //                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_DEL_RECOVER", "INDATA", "OUTDATA", inDataTable);
        //                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
        //                {
        //                    Util.MessageInfo("FM_ME_0140");  //복구 완료하였습니다.
        //                    ClearControlValue();

        //                    _sFinCD = "C";
        //                    GetTrayInfo();
        //                }
        //                else
        //                    Util.Alert("FM_ME_0121");  //동일한 Tray ID를 가진 Tray가 존재합니다. 복구할 수 없습니다.
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        private void btnGrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_021_GRADE_DISTRIBUTION grade_distribution = new FCS001_021_GRADE_DISTRIBUTION();
                grade_distribution.FrameOperation = FrameOperation;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(txtTrayID.Text.Trim());
                parameters[1] = Util.NVC(txtTrayNo.Text.Trim());

                C1WindowExtension.SetParameters(grade_distribution, parameters);
                grade_distribution.Closed += new EventHandler(grade_distribution_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => grade_distribution.ShowModal()));
                grade_distribution.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void grade_distribution_Closed(object sender, EventArgs e)
        {
            FCS001_021_GRADE_DISTRIBUTION window = sender as FCS001_021_GRADE_DISTRIBUTION;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            FCS001_024 fcs001_024 = new FCS001_024();
            fcs001_024.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = txtTrayID.Text;
            parameters[1] = txtTrayNo.Text;
            parameters[2] = null; // FINCD
            parameters[3] = "Y"; // ACT_YN
            this.FrameOperation.OpenMenuFORM("SFU010710040", "FCS001_024", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Tray별 Cell Data"), true, parameters);
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtROUTID.Text))
            {
                Util.Alert("FM_ME_0100"); //공정경로 정보가 존재하지않습니다.
                return;
            }
            FCS001_021_SEL_JUDG_OP wndPopup = new FCS001_021_SEL_JUDG_OP();
            wndPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = txtTrayNo.Text;
            parameters[1] = txtROUTID.Text;

            C1WindowExtension.SetParameters(wndPopup, parameters);
            wndPopup.Closed += new EventHandler(wndPopup_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            wndPopup.BringToFront();
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_021_SEL_JUDG_OP window = sender as FCS001_021_SEL_JUDG_OP;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnAging_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTrayNo.Text)) return;

            string endTime = string.Empty;
            for (int i = dgProcess.GetRowCount() - 1; i >= 0; i--)
            {
                if (DataTableConverter.GetValue(dgProcess.Rows[i].DataItem, "CURR_YN").ToString().Equals("Y")
                    && DataTableConverter.GetValue(dgProcess.Rows[i].DataItem, "PROCID").ToString().Equals(txtOper.Tag.ToString()))
                {
                    //시작 공정일 때
                    if (dgProcess.GetRowCount() == 1 || i < 1) endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    else endTime = Util.NVC(DataTableConverter.GetValue(dgProcess.Rows[i - 1].DataItem, "WIPDTTM_ED"));
                }
            }

            FCS001_021_AGING_CARRY_IN AgingPopup = new FCS001_021_AGING_CARRY_IN();
            AgingPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = txtTrayNo.Text;
            parameters[1] = txtTrayID.Text;
            parameters[2] = endTime;

            C1WindowExtension.SetParameters(AgingPopup, parameters);
            AgingPopup.Closed += new EventHandler(AgingPopup_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => AgingPopup.ShowModal()));
            AgingPopup.BringToFront();
        }

        private void AgingPopup_Closed(object sender, EventArgs e)
        {
            FCS001_021_AGING_CARRY_IN window = sender as FCS001_021_AGING_CARRY_IN;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnForceOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0094", result => //강제출고요청을 하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel) return;

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LOTID"] = _sTrayNo;
                    dr["PROCID"] = _sCurrOper;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INSIDE_EQP", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows.Count == 0) { Util.Alert("FM_ME_0090"); } //강제출고 가능한 Tray가 존재하지 않습니다.
                    else
                    {
                        string sTray = string.Empty;
                        foreach (DataRow drTray in dtRslt.Rows)
                        {
                            sTray += drTray["CSTID"].ToString() + " ";
                        }

                        Util.MessageConfirm("FM_ME_0010", result2 => //[Tray ID : {0}]를 강제출고하시겠습니까?
                        {
                            try
                            {
                                if (result2 != MessageBoxResult.OK) return;

                                DataSet inDataSet = new DataSet();
                                DataTable inData = new DataTable();
                                inData.TableName = "INDATA";
                                inData.Columns.Add("SRCTYPE", typeof(string));
                                inData.Columns.Add("IFMODE", typeof(string));
                                inData.Columns.Add("USERID", typeof(string));
                                inData.Columns.Add("LANGID", typeof(string));
                                inData.Columns.Add("AREAID", typeof(string));
                                DataRow dr2 = inData.NewRow();
                                dr2["SRCTYPE"] = "UI";
                                dr2["IFMODE"] = "OFF";
                                dr2["USERID"] = LoginInfo.USERID;
                                dr2["LANGID"] = LoginInfo.LANGID;
                                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                                inData.Rows.Add(dr2);

                                DataTable inLot = new DataTable();
                                inLot.TableName = "INLOT";
                                inLot.Columns.Add("LOTID", typeof(string));
                                foreach (DataRow drTray in dtRslt.Rows)
                                {
                                    DataRow lotRow = inLot.NewRow();
                                    lotRow["LOTID"] = drTray["LOTID"];
                                    inLot.Rows.Add(lotRow);
                                }
                                inDataSet.Tables.Add(inData);
                                inDataSet.Tables.Add(inLot);

                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_FORCE_OUT_MULTI", "INDATA,INLOT", "OUTDATA", inDataSet);

                                if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0092"); //강제출고 요청을 완료하였습니다.
                                }
                                else
                                {
                                    Util.Alert("FM_ME_0091"); //강제출고 요청에 실패하였습니다.
                                }
                                GetTrayInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, new string[] { sTray });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID) || string.IsNullOrEmpty(_sTrayNo)) return;

            Util.MessageConfirm("FM_ME_0234", result => //종료하시겠습니까?
            {
                if (result == MessageBoxResult.No) return;
                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("TRAY_ID", typeof(string));
                    inDataTable.Columns.Add("TRAY_NO", typeof(string));
                    inDataTable.Columns.Add("WIPSTAT", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["TRAY_ID"] = _sTrayID;
                    dr["TRAY_NO"] = _sTrayNo;
                    dr["WIPSTAT"] = _sFinCD;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_MANUAL_HISTORY", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        Util.Alert("ME_0221");  //정상종료하였습니다.
                        ClearControlValue();

                        _sFinCD = "H";
                        GetTrayInfo();
                    }
                    else
                        Util.Alert("ME_0220");  //정상종료 요청에 실패하였습니다.
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnDOCV_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID)) return;

            Util.MessageConfirm("FM_ME_0030", result => //Delta OCV를 계산하시겠습니까?
            {
                if (result != MessageBoxResult.OK) return;
                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("CSTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["CSTID"] = _sTrayID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_DOCV_CALC", "INDATA", "OUTDATA", inDataTable);
                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        Util.MessageInfo("FM_ME_0219");  //정상적으로 계산하였습니다.
                    else
                        Util.Alert("FM_ME_0096");  //계산중 오류가 발생하였습니다.
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void btnWGradeJudge_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0087", result =>
            {
                if (result == MessageBoxResult.Cancel) return;

                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    inDataTable.Columns.Add("MANUAL_YN", typeof(string));
                    // inDataTable.Columns.Add("LOT_NO", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LOTID"] = _sTrayNo;
                    dr["MANUAL_YN"] = (bool)rdoManual.IsChecked ? "Y" : (bool)rdoAuto.IsChecked ? "N" : null;
                    //  dr["LOT_NO"] = _sLotID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_W_MANUAL_LOT", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
                    {
                        Util.MessageInfo("FM_ME_0086"); //W등급 재판정을 완료하였습니다.
                        ClearControlValue();
                        GetTrayInfo();
                    }
                    else
                    {
                        Util.Alert("FM_ME_0085");  //W등급 재판정에 실패하였습니다.
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnSetRange_Click(object sender, RoutedEventArgs e)
        {
            SetRange();
        }

        private void btnSetCell_Click(object sender, RoutedEventArgs e)
        {
            SetCellRange();
        }

        private void btnChgHist_Click(object sender, RoutedEventArgs e)
        {

            FCS001_021_CHANGE_HIST wndRunStart = new FCS001_021_CHANGE_HIST();
            wndRunStart.FrameOperation = FrameOperation;

            if(wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _sTrayNo;

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
            }

        }

        private void btnChgDfctLmtOverFlag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_sTrayNo)) return;

                var dsFMP = getForcManlPortCode();

                var _FORM_FORC_MANL_PORT = from dtCnt in Enumerable.Range(0, dsFMP.Rows.Count)
                                           select new
                                           {
                                               _portFlag = dsFMP.Rows[dtCnt]["FORM_FORC_MANL_PORT"].ToString(),
                                               _name = dsFMP.Rows[dtCnt]["FORM_FORC_MANL_PORT_NAME"].ToString(),
                                               _initFlag = dsFMP.Rows[dtCnt]["INIT_FLAG"].ToString()
                                           };

                _FORM_FORC_MANL_PORT.ToList().ForEach((retData) =>
                {
                    if (retData._portFlag.Equals("N"))
                    {
                        Util.Alert("FM_ME_0486"); //수동포트 반송 중이 아닙니다.
                    }
                    else
                    {
                        try
                        {
                            if (retData._initFlag.Equals("N"))
                            {
                                Util.Alert("FM_ME_0483", retData._name); //반송 초기화 대상이 아닙니다.
                            }
                            else
                            {
                                Util.MessageConfirm("FM_ME_0484", (result) =>  //[1%]반송 초기화 대상 입니다. 초기화 하시겠습니까?
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        var _addColumnData = "SRCTYPE,LOTID,FORM_FORC_MANL_PORT_CODE,UPDUSER"; // E20230531-000728  오타 수정

                                        DataTable _SecDataTable = (DataTable)setTableData(_addColumnData, "", new DataTable());

                                        _SecDataTable.Rows.Add((DataRow)setTableData(_addColumnData, $"UI,{dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString()},INIT,{LoginInfo.USERID}", null, _SecDataTable.NewRow()));

                                        new ClientProxy().ExecuteService("BR_FORM_SET_FORM_FORC_MANL_PORT_CODE", "INDATA", null, _SecDataTable, (bizResult, bizException) =>
                                        {
                                            try
                                            {
                                                if (bizException != null)
                                                {
                                                    Util.MessageException(bizException);
                                                    return;
                                                }
                                            }
                                            catch (Exception ex) { Util.MessageException(ex); }
                                        });

                                        ClearControlValue();
                                        GetTrayInfo();
                                    }
                                    else { return; }
                                }, new string[] { retData._name });
                            }
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnRlsTrfRsnCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_sTrayID)) return;

                Util.MessageConfirm("FM_ME_0079", (result) =>  //Tray 정보를 변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("TRF_RSN_CODE", typeof(string));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));
                        inDataTable.Columns.Add("UPDDTTM", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["TRF_RSN_CODE"] = "N";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        dr["CSTID"] = _sTrayID;
                        inDataTable.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TRAY_ABNORM_TRF_RSN_CODE", "RQSTDT", "RSLTDT", inDataTable);

                        Util.MessageInfo("FM_ME_0074"); //Tray 정보 변경을 완료하였습니다.
                        ClearControlValue();
                        GetTrayInfo();
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                Util.MessageException(ex);
            }
        }

        private void btnCrackChg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sCrackState = txtCrackState.Text;
                string Message = "";


                if (string.IsNullOrWhiteSpace(_sTrayNo)) return;

                DataTable inDataTableDA = new DataTable();
                inDataTableDA.Columns.Add("LOTID", typeof(string));

                DataRow drDA = inDataTableDA.NewRow();
                drDA["LOTID"] = _sTrayNo;
                inDataTableDA.Rows.Add(drDA);

                DataTable dtRsltDA = new ClientProxy().ExecuteServiceSync("DA_SEL_CRACK_TRAY_OPER_EQPT_CHK", "RQSTDT", "RSLTDT", inDataTableDA);  // Tray의 위치정보가 공정설비인지 확인

                if (sCrackState.Equals("Y") && dtRsltDA.Rows.Count == 0)
                {
                    Message = "FM_ME_0512";   // Crack Tray 상태를 C으로 변경하겠습니까?
                }
                else if (sCrackState.Equals("Y") && dtRsltDA.Rows.Count > 0)
                {
                    Message = "FM_ME_0513";   // Crack Tray 상태를 N으로 변경하겠습니까?  Tray의 위치정보가 공정설비이면 Y -> N
                }
                else if ((sCrackState.Equals("C")) && dtRsltDA.Rows.Count > 0)
                {
                    Message = "FM_ME_0513";   // Crack Tray 상태를 N으로 변경하겠습니까?
                }
                else if ((sCrackState.Equals("C")) && dtRsltDA.Rows.Count == 0)
                {
                    Util.MessageInfo("FM_ME_0514");     // 이미 Crack Tray 상태를 C으로 변경하였습니다.
                    return;
                }
                else
                {
                    Util.MessageInfo("FM_ME_0515");     // 이미 Crack Tray 상태를 N으로 변경하였습니다.
                    return;
                }

                if (sCrackState.Equals("N") || string.IsNullOrWhiteSpace(sCrackState))
                {
                    Util.MessageInfo("FM_ME_0217");     // 정보 변경 가능한 Tray가 아닙니다.
                    return;
                }


                Util.MessageConfirm(Message, (result) =>  // Crack Tray 상태를 C/N으로 변경하겠습니까?
                {

                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        //inDataTable.Columns.Add("FLAG", typeof(string));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));
                        inDataTable.Columns.Add("UPDDTTM", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = _sTrayNo;
                        dr["SRCTYPE"] = "UI";
                        //dr["FLAG"] = (sCrackState.Equals("Y")) ? "C" : "N";   // 'Y'인 경우 현재 위치에서 가까운 수동포트 이동, 'C'인 경우 차기 공정 진행 (Flag : Y > C > N)
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        inDataTable.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_WIPATTR_SCRP_TRAY_FLAG", "INDATA", "OUTDATA", inDataTable);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
                        {
                            Util.MessageInfo("FM_ME_0074"); //Tray 정보 변경을 완료하였습니다.
                            ClearControlValue();
                            GetTrayInfo();
                        }
                        else
                        {
                            Util.MessageInfo("FM_ME_0514");     // 이미 Crack Tray 상태를 C로 변경하였습니다.
                        }
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                Util.MessageException(ex);
            }
        }

        private void txtROUTID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //OpenRouteForm(_sTrayLine, _sModelID, txtRouteID.Text);
        }

        private void txtRange_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
                SetCellRange();
            }
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_sTrayNo))
            {
                RadioButton rbClick = (RadioButton)sender;
                if (rbClick.Name.ToString().Equals("rdoCapa") && (rbClick.IsEnabled == true)) GetDataQuery(CAPA_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoCurr") && (rbClick.IsEnabled == true)) GetDataQuery(CURR_DATATYPE);
                //2021-09-30 전류단위 A 일 때 값 조회되도록 수정
                if (rbClick.Name.ToString().Equals("rdoCurrA") && (rbClick.IsEnabled == true)) GetDataQuery(CURR_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoGrade") && (rbClick.IsEnabled == true)) GetDataQuery("");
                if (rbClick.Name.ToString().Equals("rdoImp") && (rbClick.IsEnabled == true)) GetDataQuery(IMP_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoFittedImp") && (rbClick.IsEnabled == true)) GetDataQuery(FITTEDIMP_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoOCV") && (rbClick.IsEnabled == true)) GetDataQuery(OCV_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoVolt") && (rbClick.IsEnabled == true)) GetDataQuery(VOLT_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdonCCVal") && (rbClick.IsEnabled == true)) GetDataQuery(CCVAL_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdonCVVal") && (rbClick.IsEnabled == true)) GetDataQuery(CVVAL_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdonCCTime") && (rbClick.IsEnabled == true)) GetDataQuery(CCTIME_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdonCVTime") && (rbClick.IsEnabled == true)) GetDataQuery(CVTIME_DATATYPE);
                /*[CSR ID:2407996] Ftting 용량 UI 적용의 건
               * 수정내용 : fitted capacity 조회시
               * 수정자 : 정종덕D
               */
                if (rbClick.Name.ToString().Equals("rdoFittedCapa") && (rbClick.IsChecked == true)) GetDataQuery(FITTEDCAPACITY_DATATYPE);
                /* 2014.1.17 정종덕D // [CSR ID:2444504] 자동차 셀 전 모델 및 호기 W-code 및 비선형 mV/day 기준 정보 등록 
                * 저전압 OCV판정 
                * */
                if (rbClick.Name.ToString().Equals("rdoPreOCV") && (rbClick.IsChecked == true)) GetDataQuery(LOWVOLTAGE_OCVJUDG_OCV_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoPredOCV") && (rbClick.IsChecked == true)) GetDataQuery(LOWVOLTAGE_OCVJUDG_DOCV_DATATYPE);
                // 2014.02.26 강진구D // 2491135   // 대용량 방전기 전압값 표시 件 / ( 대용량 방전 전, 후 전압 조회 기능 추가)
                if (rbClick.Name.ToString().Equals("rdoPGSVolt") && (rbClick.IsChecked == true)) GetDataQuery(POWERGRADING_STRAT_VOLT_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoPGEVolt") && (rbClick.IsChecked == true)) GetDataQuery(POWERGRADING_END_VOLT_DATATYPE);
                //[180807 이수진C] JIG CNB 관련 추가
                if (rbClick.Name.ToString().Equals("rdoJigCnbPress") && (rbClick.IsChecked == true)) GetDataQuery(JIG_CNB_PRESS_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoJigCnbTemp") && (rbClick.IsChecked == true)) GetDataQuery(JIG_CNB_TEMP_DATATYPE);
                //20191128가압단락검사기추가
                if (rbClick.Name.ToString().Equals("rdoDeltaTemp") && (rbClick.IsChecked == true)) GetDataQuery(DELTA_TEMP);
                //20200120가압단락검사기 압력추가
                if (rbClick.Name.ToString().Equals("rdoHPCDPress") && (rbClick.IsChecked == true)) GetDataQuery(HPCDPRESS_DATATYPE);
                //200408 KJE : 용량 선별화 판정 추가
                if (rbClick.Name.ToString().Equals("rdoFittedCapaSAS") && (rbClick.IsChecked == true)) GetDataQuery(FITTEDCAPACITY_SAS_DATATYPE);
                ////2023.03.16 suyong.kim DOCV2 추가
                if (rbClick.Name.ToString().Equals("rdoFittedOCV") && (rbClick.IsChecked == true)) GetDataQuery(FITTED_OCV_DATATYPE);
                ////2023.03.16 dhson95 가압단락 최소전류,최대전류 추가
                if (rbClick.Name.ToString().Equals("rdoHPCDCurr") && (rbClick.IsChecked == true)) GetDataQuery(HPCDCurr_DATATYPE);
                //20230925 김용식 HPCD 초기전압, 종료전압 추가
                if (rbClick.Name.ToString().Equals("rdoIniVolt") && (rbClick.IsChecked == true)) GetDataQuery(INIT_VOLT_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoEndVolt") && (rbClick.IsChecked == true)) GetDataQuery(END_VOLT_DATATYPE);
            }
            else
            {
                // Util.Alert("FM_ME_0071"); //Tray ID를 정확히 입력해주세요.
            }
        }

        private void dgProcess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    int row = cell.Row.Index;
                    // txtRange1.Text = string.Empty;
                    // txtRange1.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style; // 공정선택시 범위 설정 초기화
                    // txtRange2.Text = string.Empty;
                    // txtRange2.Style = Application.Current.Resources["Content_Inputform_ReadOnlyTextBoxStyle"] as Style;
                    _sCurrOper = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID"));
                    _sCurrYN = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "CURR_YN"));

                    if (_sCurrOper.Equals("000") || string.IsNullOrEmpty(_sCurrOper)) return;
                    //2019-09-09 scpark 시작시간이 없을겨우 아무것도 처리안함 DateTime.Parse 에러 발생함
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ST"))))
                    {
                        _sOPStartTime = "";
                    }
                    else
                    {
                        _sOPStartTime = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ST").ToString());
                        DateTime dateOP = DateTime.Parse(_sOPStartTime);
                        _sOPStartTime = dateOP.ToString("yyyyMMddHHmmss");
                    }
                    SetRadioButton(_sCurrOper, row);
                    GetTemp();

                    GetRelativeRJudgeSpec(_sTrayNo, _sCurrOper, (dtRJudg, exRJudg) =>
                    {
                        if (exRJudg != null)
                        {
                            btnRelativeData.IsEnabled = false;
                            btnRelativeData.Tag = null;

                            Util.MessageException(exRJudg);
                            return;
                        }

                        if (dtRJudg != null && dtRJudg.Rows.Count > 0)
                        {
                            btnRelativeData.IsEnabled = true;
                            btnRelativeData.Tag = dtRJudg;
                        }
                        else
                        {
                            btnRelativeData.IsEnabled = false;
                            btnRelativeData.Tag = null;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProcess_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                if (pnt == null) return;

                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);
                if (cell == null) return;
                C1.WPF.DataGrid.DataGridRow gridRow = cell.Row;

                if (gridRow != null)
                {
                    int row = cell.Row.Index;

                    // if(MENUAUTH.EQUALS("W") 
                    bool bPossEnd = false;

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED"))))
                        bPossEnd = true;
                    else
                    {
                        //200420 KJE
                        //이력의 마지막 공정이 종료상태인데 TRAY 공정상태는 작업중일 때 공정 종료 가능하도록 함
                        if (iLastRow != -1 && row == iLastRow
                            && !string.IsNullOrEmpty(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED").ToString())
                            && txtTrayOpStatus.Tag.ToString().Equals("S"))
                            bPossEnd = true;
                    }

                    if (bPossEnd.Equals(false))
                    {
                        return;
                    }

                    if (bPossEnd.Equals(true))
                    {
                        //진행중인 공정을 종료하시겠습니까?
                        Util.MessageConfirm("FM_ME_0236", result =>
                        {
                            if (bPossEnd && result == MessageBoxResult.OK)
                            {
                                try //20230103 추가
                                {
                                    string CurrOP = string.Empty;
                                    CurrOP = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID").ToString();

                                    DataTable inDataTable = new DataTable();
                                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                                    inDataTable.Columns.Add("IFMODE", typeof(string));
                                    inDataTable.Columns.Add("AREAID", typeof(string)); //2021-05-10 AREAID 추가
                                    inDataTable.Columns.Add("LOTID", typeof(string));
                                    inDataTable.Columns.Add("PROCID", typeof(string));
                                    inDataTable.Columns.Add("CURROP", typeof(string));
                                    inDataTable.Columns.Add("ACTUSER", typeof(string));

                                    DataRow dr = inDataTable.NewRow();
                                    dr["SRCTYPE"] = "UI";
                                    dr["IFMODE"] = "OFF";
                                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                    dr["LOTID"] = _sTrayNo;
                                    dr["PROCID"] = _sCurrOper;
                                    dr["CURROP"] = _sCurrOper.Substring(2, 1);
                                    dr["ACTUSER"] = LoginInfo.USERID;
                                    inDataTable.Rows.Add(dr);

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_INFO_OP_END", "INDATA", "OUTDATA", inDataTable);
                                    if (dtRslt.Rows.Count == 0)
                                    {
                                        Util.Alert("FM_ME_0112");  //공정종료에 실패하였습니다.
                                        return;
                                    }
                                    Util.Alert("FM_ME_0111");  //공정종료를 완료하였습니다.
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                GetTrayInfo();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDOCV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int row = cell.Row.Index;
                // txtRange1.Text = string.Empty;
                // txtRange1.Style = Application.Current.Resources["Content_Inputform_ReadOnlyTextBoxStyle"] as Style;
                // txtRange2.Text = string.Empty;
                // txtRange2.Style = Application.Current.Resources["Content_Inputform_ReadOnlyTextBoxStyle"] as Style;
                _sCurrOper = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID").ToString();
                SetRadioButton(_sCurrOper, dgDOCV.CurrentRow.Index);
                GetTemp();
            }
        }

        private void dgCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Column.Name.Equals("SUBLOTID") || cell.Column.Name.Equals("SUBLOTID1"))
                {
                    //Open Cell Form
                    string sCellId = cell.Text;
                    FCS001_022 fcs022 = new FCS001_022();
                    fcs022.FrameOperation = FrameOperation;

                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(sCellId);
                    parameters[1] = "Y"; //_sActYN

                    //this.FrameOperation.OpenMenu("SFU010710020", true, parameters);

                    this.FrameOperation.OpenMenuFORM("SFU010710020", "FCS001_022", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);
                }
            }
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name).Equals("SUBLOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (Util.NVC(e.Cell.Column.Name).Equals("SUBLOTID1"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
           
                if (dtWipCell.Rows.Count > 0)
                {
                    for (int i = 0; i < dtWipCell.Rows.Count; i++)
                    {
                        if (e.Cell.Text.Equals(Util.NVC(dtWipCell.Rows[i]["SUBLOTID"])) && Util.NVC(dtWipCell.Rows[i]["SPLT_FLAG"]).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }

                if (e.Cell.Column.Name.Equals("VALUE") && e.Cell.Column.Name.Equals("VALUE1")) e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                if (dtSublotList.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSublotList.Rows.Count; i++)
                    {
                        if (e.Cell.Text.Equals(Util.NVC(dtSublotList.Rows[i]["SUBLOTID"])))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Tomato);
                        } 
                    }
                }

                if (dtValueSublot.Rows.Count > 0)
                {
                    for (int i = 0; i < dtValueSublot.Rows.Count; i++)
                    {
                        if (e.Cell.Text.Equals(Util.NVC(dtValueSublot.Rows[i]["SUBLOTID"])))
                        {
                            dataGrid[e.Cell.Row.Index, e.Cell.Column.Index + 1].Presenter.Background = new SolidColorBrush(Colors.Tomato);
                        }
                    }
                }              
            }));

        }

        private void dgProcess_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!FinCheck)
                {
                    //TH_TRAY_PROCESS_IRR_HIST (=>WipHistory) 데이터 일 경우
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CURR_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }

                    //비정상진행공정일 경우(수동 공정변경 등)
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "IRR_HIST_YN")).Equals("Y"))
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);

                    //특성공정작업 예상시간
                    if (e.Cell.Row.Index == dataGrid.Rows.Count - 1
                    && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PROCID")).Equals("000"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TIME_OVER_YN")).Equals("Y"))
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        else
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }

                    //Degas 입고 예상시간  
                   if (e.Cell.Row.Index == dataGrid.Rows.Count - 1
                   && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PROCID")).Equals("DGS_INTO_FORECAST"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TIME_OVER_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                }
                else
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
            }));
        }

        private void dgProcess_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgProcess.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;

            if (e.Row.Index == dataGrid.Rows.Count - 1 &&
                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[dataGrid.Rows.Count - 1].DataItem, "PROCID")).Equals("000"))
            {
                tb.Text = ObjectDic.Instance.GetObjectName("FORECAST");
                dataGrid.Rows[e.Row.Index].HeaderPresenter.Content = tb;
            }
        }

        private void dgCT_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int row = cell.Row.Index;
                _sCurrOper = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQSG_NAME").ToString();
                SetRadioButton(_sCurrOper, dgCT.CurrentRow.Index);
            }
        }

        private void btnCTSampleOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["LOTID"] = _sTrayNo;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                //2023.01.23  TrayID  Null 일경우  Error
                if (string.IsNullOrEmpty(_sTrayNo))
                {
                    Util.Alert("FM_ME_0069");//TrayId값이 NULL 입니다.
                    return;
                }

                if (_AgingOutPriority == 7 && _CtType == "Y")
                {
                    Util.MessageConfirm("FM_ME_0488", (result) =>  //[Tray ID : {0}]를 CTC Sample 해제하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_CTC_SAMPLE_IN", "INDATA", "OUTDATA", inDataTable);

                                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0067");  //Sample 해제를 완료하였습니다.
                                }
                                else
                                {
                                    Util.Alert("ME_0068");  //Sample 해제에 실패하였습니다.
                                }
                                ClearControlValue();
                                GetTrayInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                    }, new string[] { _sTrayID });
                }
                else
                {
                    Util.MessageConfirm("FM_ME_0487", (result) =>  //[Tray ID : {0}]를 CTC Sample 출고 하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_CTC_SAMPLE_OUT", "INDATA", "OUTDATA", inDataTable);
                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.MessageInfo("FM_ME_0065");  //Sample 출고 지시를 완료하였습니다.

                            }
                            else
                            {
                                Util.MessageInfo("FM_ME_0066");
                            }
                            ClearControlValue();
                            GetTrayInfo();
                        }
                    }, new string[] { _sTrayID });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRoutInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtROUTID.Text)) return;

            object[] parameters = new object[3];
            parameters[0] = Util.NVC(_sTrayLine);
            parameters[1] = Util.NVC(_sModelID);
            parameters[2] = Util.NVC(txtROUTID.Text);

            this.FrameOperation.OpenMenu("SFU10745100", true, parameters); //Route 정보 조회
        }

        /// <summary>
        /// 상대판정정보 : 데이터가 있을때만 버튼 활성화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRelativeData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnRelativeData.Tag == null || !(btnRelativeData.Tag is DataTable)) return;

                FCS001_021_RELATIVE_RJUDGE popRelativeRJudge = new FCS001_021_RELATIVE_RJUDGE();
                popRelativeRJudge.FrameOperation = FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = btnRelativeData.Tag;

                C1WindowExtension.SetParameters(popRelativeRJudge, parameters);
                this.Dispatcher.BeginInvoke(new Action(() => popRelativeRJudge.ShowModal()));
                popRelativeRJudge.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
    #endregion

 
}
