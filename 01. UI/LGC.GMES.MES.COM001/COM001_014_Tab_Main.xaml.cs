/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 관리
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.12.24 손우석 CSR ID 3852388 GMES UI Eqpt.loss 조회 오류 수정 요청의 건 [요청번호]C20181123_52388
  2019.04.29 염규범 CSR ID C20190423_79897 오창 자동차1,2 동 소형 1동 CWA 2동 포함 처리
  2019.07.19 김도형 CSR ID C20190717_43980 CMI 포함 처리
  2020.05.21 김준겸 CSR ID C20200507-000004 작업자 칼럼 추가 요청 건
  2020.05.21 김준겸 CSR ID C20200427-000204 '비고'사이즈 조정시 다음줄로 줄바꿈 처리.
  2020.06.30 김준겸 CSR ID C20200427-000204 '원인,해결조치' 하단 Grid 출력안되는 원인 해결.
  2020.09.02 김준겸 CSR ID C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용)
  2022.01.13 안유수 CSR ID C20220113-000221 설비 Loss 등록 화면에서 조회 조건 중 작업 조 선택이 안되는 현상 수정
  2022.08.23 윤지해 CSR ID C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
  2022.08.23 정용석 CSR ID C20220512-000432 [PEE Technology Team] GMES ESWA PACK - Additional pop-up and query to register EQPT Loss details
--------------------------------------------------------------------------------------
 Created Date : 2022.12.16
      Creator : 김린겸
   Decription : (Tab 0) 설비 LOSS 관리
--------------------------------------------------------------------------------------
  //COM001_014 : TabControl 용으로 사용, Tab_Main & Tab_Unit 파일 새로 추가후 기존 코드 복사함
  2022.12.16 김린겸 CSR ID C20220929-000399 원통형 후공정 설비 loss 기능 개선 요청
  2022.12.20 이재호 CSR ID C20221216-000583 설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회
  2023.01.16 이재호 CSR ID C20221216-000583 설비 LOSS 등록 부동내역 C1ComboBox -> Popcontrol 변경
  2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
  2023.03.07 윤지해 CSR ID E20230220-000068	FCR 초기화, Loss코드, 부동내용 필수 등록으로 validation 수정
  2023.03.10 김린겸 조회 버튼시 Loading... 추가
  2023.03.16 오화백 CSR ID C20230103-000963  [전극/조립MES팀]ESHM GMES 시스템 구축 (AZS Stacking 일 경우 Machine 설비 LOSS  수정가능하도록 처리)
  2023.03.23 안유수 SM     설비 LOSS 일괄 저장 기능 ERP 마감 VALIDATION 추가
  2023.03.23 윤지해 CSR ID E20230321-001518	GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
  2023.03.24 이윤중 CSR ID E20230322-001797  [활성화] 설비 Loss 등록 화면 소스 일원화 작업 / ESNB ESS LINE 로직 추가(공통코드 활용) - SetEquipmentSegmentCombo 호출 bizrule(BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO) 변경
  2023.04.03 김린겸 setMapColor(), setMapColor_Machine() row값 Get, if (row != null) 안으로 이동
  2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
  2023.05.22 이다혜 CSR ID E20230420-001240 설비 LOSS 수정 화면 추가에 따른 Validation 추가
  2023.05.23 윤지해 CSR ID E20230330-001442 FCR 그룹 등록내용 선택 시 FCR 콤보박스에 없을 경우 자동입력되지 않도록 수정
  2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS LV2, LV3, FCR 등록 로직 추가(PACK, 소형 제외)
  2023.06.08 김도형 CSR ID [E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
  2023.06.21 안유수 CSR ID E20230608-000919 TROUBLE_CAUSE_EQPT_STAT 컬럼 추가
  2023.07.03 이윤중 CSR ID E20230627-000461 2023-07-01 Loss 체계 개선 이전 날짜 선택 불가 로직 추가 (임시)
  2023.07.06 이윤중 CSR ID E20230627-000461 설비 Loss 변경 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
  2023.07.07 김도형 CSR ID [E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
  2023.07.18 윤지해 CSR ID E20230703-000158 COM001_014_LOSS_DETL_FCR 화면으로 부동내용 팝업 변경
  2023.07.20 김도형 CSR ID [E20230707-001616] 원인 설비별 LOSS 관리 <> 'Y' 일 경우 원인설비공정-> 메인설비공정으로 변경
  2023.07.17 김대현 CSR ID E20230711-000645  MES 시스템의 설비 Loss 수정 승인 요청 기능을 위한 신규 개발/기능 변경
  2023.08.09 김대현 CSR ID E20230711-000645  MES 시스템의 설비 Loss 수정 승인 요청 기능을 위한 신규 개발/기능 변경(Pack 로직 적용)
  2023.08.24 김대현 CSR ID E20230819-001260  설비 Loss 등록/수정 시 중복 Loss 제거
  2023.09.22 안유수 CSR ID E20230913-000991  설비 Loss 저장 시 중복 데이터Validation 추가
  2023.11.07 김대현 ValidateEqptLossAppr() 파라미터에서 STRT_DTTM, END_DTTM 제거
  2023.12.19 김대현 E20231208-001776 설비 Loss 등록, 수정 화면 통합
  2024.01.03 이병윤 CSR ID E20231212-000731 bMobile여부 삭제 소형조립 원인설비 선택, Loss분류 선택, 부동내역 정상적으로 선택 수정
  2024.05.02 지광현 CSR ID E20240502-000937 활성화 공정 콤보박스에서 선택할 설비가 존재하는 공정만 조회되도록 수정
  2024.05.31 안유수 CSR ID E20240527-000288 설비 LOSS 데이터 저장 조건 변경 -> 기존 STRT_DTTM_YMDHMS 조건으로 업데이트 처리하는 조건에서,
                           PRE_LOSS_SEQNO 값 기준으로 처리 되도록 수정, 화면을 REFLASH 처리하지 않은 상태에서 수정된 LOSS 이력에 대한 VALIDATION 추가
  2024.07.16 안유수 CSR ID E20240704-001632 일괄저장 기능 사용 시 INDATA 한건만 들어오도록 수정
  2024.07.16 김대현 CSR ID E20240626-000975 TEST/개발 W/O 자동실적 적용 전후 데이터 조회 기능 추가
  2024.10.19 복현수 MES 리빌딩 PJT : 화면 아래쪽 상세 Loss에 TRANSACTION_SERIAL_NO 숨김칼럼으로 조회되도록 추가, Loss 초기화시 추가된 칼럼 값 사용하도록 추가
  2024.10.24 이지은 MES 리빌딩 PJT : RUN_SPLIT 화면 팝업시 TRANSACTION_SERIAL_NO 파라미터 추가
  2024.10.25 복현수 MES 리빌딩 PJT : LOSS_SPLIT 화면 팝업시 TRANSACTION_SERIAL_NO 파라미터 추가
  2025-02-24 이민형 MES 리빌딩 PJT : Test/개발 Check 버튼 주석 처리
  2025.03.22 오화백 HD 증설      : 공용PC에서도 일괄저장 사용하도록 처리
  2025-05-16 천진수 MI2_OSS_0253 설비LOSS RUN구간 시간분할취소(DELETE)후 GRID조회후 ROW선택제외(삭제된ROW)
  2025.07.10 오화백 ESHG 증설      :  설비 Loss Machine Multi 기능 통합
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_Tab_Main : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        Hashtable hash_loss_color = new Hashtable();

        DataTable dtMainList = new DataTable();

        DataTable dtBeforeList = new DataTable();

        DataTable AreaTime;

        DataTable dtShift;

        DataTable dtRemarkMandatory;

        DataTable dtQA;                 // CSR : C20220512-000432
        bool isOnlyRemarkSave = false;  // CSR : C20220512-000432
        bool isTotalSave = false;       // CSR : C20220512-000432

        String sMainEqptID;
        DataSet dsEqptTimeList = null;
        Util _Util = new Util();
        Hashtable org_set;

        bool bPack;
        bool bMPPD = false; // Modifiable person on the previous day : Pack 전용 전일 수정 가능자 (신규 권한)
        bool bUseEqptLossAppr = false; // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가
        string sSearchDay = "";

        string strAttr1 = string.Empty;
        string strAttr2 = string.Empty;
        string sNowDay = string.Empty;

        string _procid = string.Empty;
        string _wrk_date = string.Empty;
        string _eqptid = string.Empty;
        string _areaid = string.Empty;

        bool bForm;
        CommonCombo combo = new CommonCombo();

        List<string> liProcId;

        int iEqptCnt;

        string RunSplit; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분
        string _grid_eqpt = string.Empty;
        string _grid_area = string.Empty;
        string _grid_proc = string.Empty;
        string _grid_eqsg = string.Empty;
        string _grid_shit = string.Empty;

        // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        string _fTypeCode = "F";
        string _cTypeCode = "C";
        string _rTypeCode = "R";

        // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS 등록 여부 확인
        bool isCauseEquipment = false;
        string occurEqptFlag = string.Empty;
        bool bMobile;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        //Machine 설비 Loss 수정 가능여부 Flag :    2023.03.16 오화백
        string MachineEqptChk = string.Empty;

        //Machine Multi 2025 07 10 오화백
        bool MachineMultiChk = false;
        char[] delimiters = { ',', ' ' }; // Multi 설비 리스트 Split 용 구분자.
        bool checkBox_Change_Def = true;
        bool checkBoxChangeError_Def = true;
        string splitTime_Start = "";
        string splitTime_End = "";

        string FCode = string.Empty;
        string CCode = string.Empty;
        string RCode = string.Empty;

        string FName = string.Empty;
        string CName = string.Empty;
        string RName = string.Empty;
       
        //Machine Multi 2025 07 10 오화백

        DateTime dBaseDate = new DateTime();
        C1.WPF.DataGrid.DataGridRow drCurrDetail = new C1.WPF.DataGrid.DataGridRow();

        public COM001_014_Tab_Main()
        {
            InitializeComponent();

            InitCombo();

            InitGrid();

            GetLossColor();

            if (string.Equals(GetAreaType(), "E"))
            {
                lotTname.Visibility = Visibility.Visible;
            }
            if (!string.Equals(GetAreaType(), "P"))  //PACK 부서를 제외한 작업자 버튼 및 TEXTBOX 비활성화.
            {
                InitUser();
            }
            if (bPack)
            {
                GetPackAuth();
            }

            GetEqptLossApprAuth();  // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가
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
            // 2023.05.28 윤지해 CSR ID E20230330-001442 소형 조립 확인 추가
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("M"))
                bMobile = true;
            else
                bMobile = false;

            CommonCombo _combo = new CommonCombo();

            if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {
                bPack = false;
                chkMain.IsEnabled = true;

                //2023.03.07 활성화 구분 추가
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                    bForm = true;
                else
                    bForm = false;
                /*
                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboShift };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcess, cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cboProcessChild = { cboShift, cboEquipment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);

                //작업조
                C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent);
                */

                SetAreaCombo(cboArea);
                SetEquipmentSegmentCombo(cboEquipmentSegment);
                SetProcessCombo(cboProcess);
                SetEquipmentCombo(cboEquipment);
                SetShiftCombo(cboShift);

                //Machine 설비 콤보 조회 - by 오화백 2023-03-16
                SetMachineEqptCombo(cboEquipment_Machine);
                //Machine Multi 2025 07 10 오화백
                SetMachineMultiEqptCombo();

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
                cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
                SetTroubleUnitColumnDisplay();

                //2023-07-03 - 설비 Loss 체계 개편 관련 제한 로직 추가 - yjlee
                ldpDatePicker.SelectedDataTimeChanged += ldpDatePicker_SelectedDataTimeChanged;
            }
            else
            {
                // Pack인 경우
                bPack = true;
                bForm = false;
                // 2021.11.12 김건식 - PACK 전체사용으로 IF분기 제거
                chkMain.IsEnabled = true;

                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);

                SetEquipment();
                //SetShift();

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
                ldpDatePicker.SelectedDataTimeChanged += ldpDatePicker_SelectedDataTimeChanged;
                //ldpDatePicker.DataContextChanged += ldpDatePicker_DataContextChanged;

                //설비 변경 시 공정도 같이 변경 Event
                cboEquipment.SelectedItemChanged += CboEquipment_SelectedItemChanged;

                //공정은 개인이 선택 불가
                cboProcess.IsEnabled = false;
                //처음은 ALL선택
                cboProcess.SelectedIndex = 0;
            }

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboColor.Items.Add(cbItemTitle);

            C1ComboBoxItem cbItemRun = new C1ComboBoxItem();
            cbItemRun.Content = "Run";
            cbItemRun.Background = ColorToBrush(GridBackColor.R);
            cboColor.Items.Add(cbItemRun);

            C1ComboBoxItem cbItemWait = new C1ComboBoxItem();
            cbItemWait.Content = "Wait";
            cbItemWait.Background = ColorToBrush(GridBackColor.W);
            cboColor.Items.Add(cbItemWait);

            C1ComboBoxItem cbItemTrouble = new C1ComboBoxItem();
            cbItemTrouble.Content = "Trouble";
            cbItemTrouble.Background = ColorToBrush(GridBackColor.T);
            cboColor.Items.Add(cbItemTrouble);

            C1ComboBoxItem cbItemOff = new C1ComboBoxItem();
            cbItemOff.Content = "OFF";
            cbItemOff.Background = ColorToBrush(GridBackColor.F);
            cboColor.Items.Add(cbItemOff);

            C1ComboBoxItem cbItemUserStop = new C1ComboBoxItem();
            cbItemUserStop.Content = "UserStop";
            cbItemUserStop.Background = ColorToBrush(GridBackColor.U);
            cboColor.Items.Add(cbItemUserStop);

            cboColor.SelectedIndex = 0;

            //작업조 Default 셋팅

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));


                DataRow row = dt.NewRow();

                // Machine 설비수정 가능할 경우 - by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                    }
                    else
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment_Machine.SelectedValue);
                    }
                }
                else
                {
                    row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                }

                dt.Rows.Add(row);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SET_SHIFT", "RQSTDT", "RSLTDT", dt);

                if (dtRslt.Rows.Count != 0)
                {
                    cboShift.SelectedValue = Convert.ToString(dtRslt.Rows[0]["SHFT_ID"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitInsertCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //loss 코드
            ////C1ComboBox[] cboLossChild = { cboLossDetl };
            ////_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild);
            InitLossCombo();

            //부동코드

            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경_주석처리
            ////현상코드
            //String[] sFilterFailure = { "F" };
            //_combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

            ////원인코드
            //String[] sFilterCause = { "C" };
            //_combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

            ////조치코드
            //String[] sFilterResolution = { "R" };
            //_combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");
            #endregion

            //원인설비
            InitOccurEqptCombo();

            //최근
            if (bUseEqptLossAppr)
            {
                string[] sFilterLoss = { Convert.ToString(cboEquipment.SelectedValue) };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "LAST_LOSS_ALL");
            }
            else
            {
                C1ComboBox[] cboLastLossParent = { cboEquipment };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);
            }

            //동-라인-공정별 로스 맵핑
            string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
            _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
        }

        private void InitLossCombo()
        {
            CommonCombo _combo = new CommonCombo();

            if (LoginInfo.CFG_AREA_ID == "P1" || LoginInfo.CFG_AREA_ID == "P2" || LoginInfo.CFG_AREA_ID == "P6")
            {
                if (bUseEqptLossAppr && !string.IsNullOrEmpty(strAttr1) && !string.IsNullOrEmpty(strAttr2))
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboEquipmentSegment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPackAll");
                }
                else
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboEquipmentSegment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPack");
                }
            }
            // 2023.05.28 윤지해 CSR ID E20230330-001442 추가
            else if (occurEqptFlag.Equals("Y"))
            {
                string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), occurEqptFlag, string.Empty };
                //_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
                _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeAll_OccurEqpt");  // 2024.03.19 김태오S  공정별 설비 Loss 분류 제한 내용 건, 호출 비즈 변경.
            }
            else
            {
                if (bUseEqptLossAppr && !string.IsNullOrEmpty(strAttr1) && !string.IsNullOrEmpty(strAttr2))
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeAll");
                }
                else
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
                }
            }
        }

        private void InitOccurEqptCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboOccurEqptParent = { cboEquipment };
            String[] sFilterEquipment = { /*cboEquipment.GetStringValue("MAIN_EQPTID")*/ Util.GetCondition(cboEquipment) };
            if (string.Equals(GetAreaType(), "P"))   //C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용) 김준겸 A
            {
                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cboOccurEqptParent);
                cboOccurEqpt.SelectedValueChanged -= cboOccurEqpt_SelectedValueChanged;
            }
            //else if (!bMobile && isCauseEquipment)  // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK, 소형이 아니고 원인설비별 LOSS 등록을 하는 곳일 경우 등록된 원인설비로 LIST-UP && 이벤트 추가
            else if (isCauseEquipment) // 2024.01.03 이병윤 CSR ID E20231212-000731 bMobile여부 삭제
            {
                combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, sFilter: sFilterEquipment, sCase: "cboOccurFcrEqpt");
                cboOccurEqpt.SelectedValueChanged += cboOccurEqpt_SelectedValueChanged;
            }
            else
            {
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilterEquipment);
                }
                else
                {
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                }
                cboOccurEqpt.SelectedValueChanged -= cboOccurEqpt_SelectedValueChanged;
            }
        }

        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {
                //2018.12.24
                //int gridRowCount = 220;
                //2019.01.07
                //int gridRowCount = 400;
                //2020.05.20 김준겸 S 설비 LOSS 화면의 설비가 많을 경우 겹치는 현상으로 인한 RowCount조정.
                //int gridRowCount = 500;
                //_grid.Height = gridRowCount * 15;

                _grid.Width = 3000;

                for (int i = 0; i < 361; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = GridLength.Auto;
                    }
                    else { gridCol1.Width = new GridLength(3); }

                    _grid.ColumnDefinitions.Add(gridCol1);

                }

                //for (int i = 0; i < gridRowCount; i++)
                //{
                //    RowDefinition gridRow1 = new RowDefinition();
                //    gridRow1.Height = new GridLength(15);
                //    _grid.RowDefinitions.Add(gridRow1);
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2020.05.14 김준겸 A 설비 Loss 작업자 등록 (PACK 부서만 활성화)
        /// </summary>
        private void InitUser()
        {
            // 뒷배경 비활성화
            bg_txtUser.Visibility = Visibility.Hidden;
            bg_txtPerson.Visibility = Visibility.Hidden;
            bg_btnPerson.Visibility = Visibility.Hidden;

            // 텍스트 박스,버튼 비활성화
            txtUser.Visibility = Visibility.Hidden;
            txtPerson.Visibility = Visibility.Hidden;
            btnPerson.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GetLossColor()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtRslt);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["LOSS_NAME"];
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["DISPCOLOR"].ToString()));
                    cboColor.Items.Add(cbItem);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetPackAuth()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("AUTHID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "PACK_LOSS_ENGR_CWA";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 0)
                {
                    bMPPD = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetEqptLossApprAuth()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = Util.NVC(cboArea.SelectedValue);
                drRQSTDT["COM_TYPE_CODE"] = "EQPT_LOSS_CHG_APPR_USE_SYSTEM";    // 해당 동, 시스템의 '설비 Loss 수정' 화면 사용 여부 확인
                drRQSTDT["COM_CODE"] = LoginInfo.SYSID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (dtRSLTDT != null && dtRSLTDT.Rows.Count > 0)
                {
                    bUseEqptLossAppr = true;

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR1"])))
                    {
                        strAttr1 = Util.NVC(dtRSLTDT.Rows[0]["ATTR1"]);
                    }
                    else
                    {
                        strAttr1 = "1";
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR2"])))
                    {
                        strAttr2 = Util.NVC(dtRSLTDT.Rows[0]["ATTR2"]);
                    }
                    else
                    {
                        strAttr2 = "7";
                    }
                }
                else
                {
                    bUseEqptLossAppr = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetNowDate()
        {
            string nowDate = string.Empty;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

            nowDate = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

            return nowDate;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기



            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();

            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            //row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToShortDateString();
            row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd");
            dt.Rows.Add(row);

            AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (AreaTime.Rows.Count == 0) { }
            if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return;
            }
            TimeSpan tmp = DateTime.Parse(System.DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;//DateTime.Parse("06:59:59").TimeOfDay;//

            if (tmp < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
            {
                ldpDatePicker.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            }
        }

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                //cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                SetEquipment();
                SetShift();
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
            }
        }
        #endregion

        #region [설비] - 조회 조건
        private void CboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }
            else
            {
                if (cboEquipment.SelectedIndex == 0)
                    cboProcess.SelectedIndex = 0;
                else
                    cboProcess.SelectedValue = liProcId[cboEquipment.SelectedIndex - 1].ToString();
                SetTroubleUnitColumnDisplay();
            }
        }
        #endregion

        #region [작업일] - 조회 조건
        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //2023.07.06 - 설비 Loss 등록 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);

            if (dBaseDate != null
                && ldpDatePicker.SelectedDateTime < dBaseDate)
            {
                Util.MessageValidation("SFU9040", dBaseDate.ToString("yyyy-MM-dd")); // 설비Loss 체계 개편에 따라, 7월 이전 설비Loss 등록이 불가합니다. 
                ldpDatePicker.SelectedDateTime = dBaseDate;
                return;
            }

            if (!bPack)
            {
                return;
            }

            if (ldpDatePicker.SelectedDateTime.Year > 1 && ldpDatePicker.SelectedDateTime.Year > 1)
            {
                SetShift();
            }
        }


        #endregion

        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //C20210723 - 000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment) || string.IsNullOrEmpty(sEquipmentSegment))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sProcess = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProcess) || string.IsNullOrEmpty(sProcess))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                string sEqpt = Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요
                if (sEqpt.Equals("")) return;

                //if (chkTestDev.IsChecked == true)
                //{
                //    if (GetLossTestDev() == false)
                //    {
                //        //Test/개발 설비Loss 이력이 없습니다.
                //        Util.MessageInfo("SFU8580");
                //        return;
                //    }
                //}
                
                //Machine Multi 2025 07 10 오화백
                if (chkMachMulti.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(cboEquipment_Machine_Multi.SelectedItemsToString))
                    {
                        Util.MessageValidation("SFU10016"); //머신설비를 선택하세요
                        return;
                    }
                }

                // 2023.05.28 윤지해 CSR ID E20230330-001442 추가
                //occurEqptFlag = !bPack && !bMobile && isCauseEquipment ? "Y" : "N";
                setOccurEqptFlag();
                popLossDetl.ItemsSource = null; // 2023.05.28 윤지해 CSR ID E20230330-001442 부동내용 기존 조회된 내역 초기화 추가

                //초기화
                InitInsertCombo();

                ClearGrid();
                drCurrDetail = null;

                txtEqptName.Text = "";
                txtStart.Text = "";
                txtEnd.Text = "";
                txtStartHidn.Text = "";
                txtEndHidn.Text = "";
                rtbLossNote.Document.Blocks.Clear();
                //txtLossNote.Text = "";
                txtFCRCode.Text = "";
                txtPerson.Text = "";
                txtPerson.Tag = null;
                _grid_area = Util.GetCondition(cboArea);
                _grid_eqsg = Util.GetCondition(cboEquipmentSegment);
                _grid_eqpt = Util.GetCondition(cboEquipment);
                _grid_proc = Util.GetCondition(cboProcess);
                _grid_shit = Util.GetCondition(cboShift);

                cboLoss.SelectedIndex = 0;
                cboOccurEqpt.SelectedIndex = 0;
                popLossDetl.SelectedValue = string.Empty;
                popLossDetl.SelectedText = string.Empty;
                cboFailure.SelectedIndex = 0;
                cboCause.SelectedIndex = 0;
                cboResolution.SelectedIndex = 0;
                cboLastLoss.SelectedIndex = 0;

                if (this.dtQA != null) this.dtQA.Clear();       // CSR : C20220512-000432
                this.isOnlyRemarkSave = false;                  // CSR : C20220512-000432
                this.isTotalSave = false;                       // CSR : C20220512-000432

                SelectRemarkMandatory();

                SelectLossRunArea();

                indataset();
                // Machine 설비수정 가능할 경우  Main 설비, Machine 설비 전체 조회가 되도록 - by 오화백 2023 03 16
                if ((MachineEqptChk == "Y" || MachineMultiChk == true) && chkMain.IsChecked == false)
                {
                    //Machine Multi 2025 07 10 오화백
                    if (MachineMultiChk == true && chkMachMulti.IsChecked == true)
                    {
                        GetEqptLossRawList_MachineMulti();
                    }
                    else
                    {
                        GetEqptLossRawList_Machine();
                        SelectProcess_Machine();
                    }
                }
                else
                {
                    GetEqptLossRawList();
                    SelectProcess();
                }

                //Machine Multi 2025 07 10 오화백
                if (MachineMultiChk == true && chkMachMulti.IsChecked == true)
                {
                    checkBoxChangeError_Def = false;
                    GetEqptLossDetailList_MachineMulti();
                    checkBoxChangeError_Def = true;
                    ShowLossColorMap();
                }
                else
                {
                    GetEqptLossDetailList();
                }
                sMainEqptID = "A" + Util.GetCondition(cboEquipment);

                sSearchDay = ldpDatePicker.SelectedDateTime.ToString("yyyyMMdd");
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

        /// <summary>
        /// 색지도 내에서 클릭시 발생
        /// </summary>
        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {

            //Machine Multi 2025 07 10 오화백
            if (MachineMultiChk == true && chkMachMulti.IsChecked == true)
            {

            }
            else
            {

                //org_set.Add("COL", nCol);
                //org_set.Add("ROW", nRow);
                //org_set.Add("COLOR", _border.Background);
                //org_set.Add("TIME", sEqptTimeList);
                //org_set.Add("STATUS", sStatus);
                //org_set.Add("EQPTID", sTitle);
                Border aa = sender as Border;

                org_set = aa.Tag as Hashtable;

                //if (chkTestDev.IsChecked == true)
                //{
                //    return;
                //}

                if (e.ChangedButton == MouseButton.Right)
                {
                    if (!org_set["STATUS"].ToString().Equals("R"))
                    {
                        ContextMenu menu = this.FindResource("_gridMenu") as ContextMenu;
                        menu.PlacementTarget = sender as Border;
                        menu.IsOpen = true;

                        for (int i = 0; i < menu.Items.Count; i++)
                        {
                            MenuItem item = menu.Items[i] as MenuItem;

                            switch (item.Name.ToString())
                            {
                                case "LossDetail":
                                    item.Header = ObjectDic.Instance.GetObjectName("Loss내역보기");
                                    item.Click -= lossDetail_Click;
                                    item.Click += lossDetail_Click;
                                    break;

                                case "LossSplit":
                                    item.Header = ObjectDic.Instance.GetObjectName("Loss분할");
                                    item.Click -= lossSplit_Click;
                                    item.Click += lossSplit_Click;
                                    break;
                            }
                        }

                    }
                    if (RunSplit.Equals("Y"))
                    {
                        if (org_set["STATUS"].ToString().Equals("R")) //추가
                        {
                            string startTime = txtStartHidn.Text;
                            string endTime = txtEndHidn.Text;
                            if (startTime.Equals("") || endTime.Equals(""))
                            {
                                return;
                            }


                            // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
                            string sSelectFilter = "";
                            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                            {
                                sSelectFilter += " AND EQPTID = '" + org_set["EQPTID"].ToString().Substring(1) + "'";
                            }

                            DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime + sSelectFilter).CopyToDataTable();
                            if (dt.Select("EIOSTAT <> 'R'").Count() > 0)
                            {
                                Util.MessageValidation("SFU3204"); //운영설비 사이에 Loss가 존재합니다.
                                btnReset_Click(null, null);
                                return;
                            }

                            COM001_014_RUN_SPLIT wndPopup = new COM001_014_RUN_SPLIT();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[12];
                                Parameters[0] = org_set["EQPTID"].ToString();
                                Parameters[1] = org_set["TIME"].ToString();
                                Parameters[2] = startTime;
                                Parameters[3] = endTime;
                                Parameters[4] = cboArea.SelectedValue.ToString();
                                Parameters[5] = Util.GetCondition(ldpDatePicker); //ldpDatePicker.SelectedDateTime.ToShortDateString();
                                Parameters[6] = this;
                                Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                                Parameters[8] = cboProcess.SelectedValue.ToString();
                                Parameters[9] = "Main"; //TabControl, 0:Main, 1:Unit
                                                        //Parameters[10]= !bPack && !bMobile && isCauseEquipment ? "Y" : "N";  // 2023.05.28 윤지해 CSR ID E20230330-001442 추가
                                Parameters[10] = !bPack && isCauseEquipment ? "Y" : "N";  // 2024.01.03 이병윤 CSR ID E20231212-000731 bMobile여부 삭제
                                Parameters[11] = dt.Rows[0]["TRANSACTION_SERIAL_NO"].ToString();

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                    }

                }
                else
                {
                    // Machine 설비수정 가능할 경우  Main 설비외 전체 선택이 가능하도록 - by 오화백 2023 03 16
                    if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                    {
                        if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
                        {
                            btnReset_Click(null, null);
                        }
                        else
                        {
                            // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
                            if (!org_set["EQPTID"].ToString().Equals(sMainEqptID)
                                && (cboEquipment_Machine.SelectedValue != null && cboEquipment_Machine.SelectedValue.GetString() == string.Empty))//메인설비 아닌경우 선택안되도록
                            {
                                Util.AlertInfo("SFU2863");
                                return;
                            }

                            if (!(dtMainList.Select("CHK = 1").Count() == 0))
                            {
                                if (!(dtMainList.Select("CHK = 1")[0]["EQPTID"]).ToString().Equals(org_set["EQPTID"].ToString().Substring(1)))
                                {
                                    btnReset_Click(null, null);
                                }
                            }

                            setMapColor_Machine(org_set["EQPTID"].ToString(), org_set["TIME"].ToString(), "MAP");
                        }
                    }
                    else
                    {
                        if (!org_set["EQPTID"].ToString().Equals(sMainEqptID))//메인설비 아닌경우 선택안되도록
                        {
                            Util.AlertInfo("SFU2863");
                        }

                        if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
                        {
                            btnReset_Click(null, null);
                        }
                        else
                        {
                            setMapColor(org_set["TIME"].ToString(), "MAP");
                        }
                    }

                }
            }
        }

        private void lossDetail_Click(object sender, RoutedEventArgs e)
        {
            COM001_014_TROUBLE wndPopup = new COM001_014_TROUBLE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = org_set["EQPTID"].ToString();
                Parameters[1] = org_set["TIME"].ToString();
                Parameters[2] = cboShift.SelectedValue.ToString().Equals("") ? 20 : 10;
                Parameters[3] = Util.GetCondition(ldpDatePicker);//ldpDatePicker.SelectedDateTime.ToShortDateString();


                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.Show()));

            }
        }
        private void lossSplit_Click(object sender, RoutedEventArgs e)
        {
            if (!org_set["STATUS"].ToString().Equals("R")) //추가
            {
                string startTime = txtStartHidn.Text;
                string endTime = txtEndHidn.Text;
                if (startTime.Equals("") || endTime.Equals(""))
                {
                    return;
                }

                DataTable tmp = new DataTable();
                tmp.Columns.Add("EQPTID", typeof(string));
                tmp.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));

                DataRow dr = tmp.NewRow();
                dr["EQPTID"] = org_set["EQPTID"].ToString().Substring(1);
                dr["STRT_DTTM_YMDHMS"] = startTime;
                tmp.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_END_DTTM", "RQSTDT", "RSLTDT", tmp);
                if (result.Rows.Count != 0)
                {
                    if (Convert.ToString(result.Rows[0]["END_DTTM"]).Equals(""))
                    {
                        Util.MessageValidation("SFU4244"); // 부동이 끝나지 않아 데이터를 분할 할 수 없습니다.
                        return;
                    }
                }

                // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
                string sSelectFilter = "";
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    sSelectFilter += " AND EQPTID = '" + org_set["EQPTID"].ToString().Substring(1) + "'";
                }

                DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime + sSelectFilter).CopyToDataTable();
                if (dt.Select("EIOSTAT = 'R'").Count() > 0)
                {
                    Util.MessageValidation("SFU3511"); //Run상태가 존재합니다
                    btnReset_Click(null, null);
                    return;
                }

                if (dt.Select().Count() > 1)
                {
                    Util.MessageValidation("SFU3512"); //하나의 부동상태만 선택해주세요
                    btnReset_Click(null, null);
                    return;
                }

                //if (!ValidateEqptLossAppr("SPLIT"))
                //{
                //    return;
                //}

                COM001_014_LOSS_SPLIT wndPopup = new COM001_014_LOSS_SPLIT();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[11];
                    Parameters[0] = org_set["EQPTID"].ToString();
                    Parameters[1] = org_set["TIME"].ToString();
                    Parameters[2] = startTime;
                    Parameters[3] = endTime;
                    Parameters[4] = cboArea.SelectedValue.ToString();
                    Parameters[5] = Util.GetCondition(ldpDatePicker);
                    Parameters[6] = this;
                    Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[8] = cboProcess.SelectedValue.ToString();
                    //Parameters[9] = !bPack && !bMobile && isCauseEquipment ? "Y" : "N";  // 2023.05.28 윤지해 CSR ID E20230330-001442 추가
                    Parameters[9] = !bPack && isCauseEquipment ? "Y" : "N";  // 2024.01.03 이병윤 CSR ID E20231212-000731 bMobile여부 삭제
                    Parameters[10] = dt.Rows[0]["TRANSACTION_SERIAL_NO"].ToString();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopup_Closed2);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }


        /// <summary>
        /// 초기화 버튼 클릭시
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            drCurrDetail = null;
            resetMapColor();
            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            //txtLossNote.Text = "";
            txtFCRCode.Text = "";
            txtPerson.Text = "";
            txtPerson.Tag = null;

            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            popLossDetl.SelectedValue = string.Empty;
            popLossDetl.SelectedText = string.Empty;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

            if (this.dtQA != null) this.dtQA.Clear();       // CSR : C20220512-000432
            this.isOnlyRemarkSave = false;                  // CSR : C20220512-000432
            this.isTotalSave = false;                       // CSR : C20220512-000432
        }

        private void orginalCbo()
        {
            cboArea.SelectedValue = _grid_area;
            cboEquipment.SelectedValue = _grid_eqpt;
            cboEquipmentSegment.SelectedValue = _grid_eqsg;
            cboProcess.SelectedValue = _grid_proc;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // CSR : C20220512-000432 Popup 띄우는 조건
            // 저장 체크중간에 Popup을 띄워야 하는 조건 때문에 기존에 Save 함수를 Validation 부분과 Transaction 부분으로 분리
            if (!this.SaveValidation())
            {
                return;
            }

            // Remark만 저장하는 경우는 Popup 안띄우고 저장후 종료.
            if (this.isOnlyRemarkSave)
            {
                this.SaveProcessOnlyRemark();
                return;
            }

            if (this.IsOpenQAPopup())
            {
                // Open Popup
                COM001_014_QA wndPopupQA = new COM001_014_QA();
                wndPopupQA.FrameOperation = FrameOperation;

                if (wndPopupQA != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = false;      // 그냥 저장
                    C1WindowExtension.SetParameters(wndPopupQA, Parameters);
                    wndPopupQA.Closed -= new EventHandler(this.wndPopupQA_Closed);
                    wndPopupQA.Closed += new EventHandler(this.wndPopupQA_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopupQA.ShowModal()));
                }
            }
            else
            {
                // 당일 수정건이 아닌 경우

                if (bUseEqptLossAppr)
                {
                    if (ValidationChkDDay().Equals("AUTH_ONLY"))
                    {
                        string lossCode = Util.GetCondition(cboLoss);
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LOSS_CODE"] = lossCode;
                        RQSTDT.Rows.Add(dr);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                        if (result.Rows.Count != 0)
                        {
                            string upprLossCode = Convert.ToString(result.Rows[0]["UPPR_LOSS_CODE"]);
                            // 수정할 Loss가 비조업인 경우
                            if (upprLossCode.Equals("10000"))
                            {
                                ApprovalProcess();
                            }
                            else
                            {
                                this.SaveProcess();
                            }
                        }
                    }
                    else
                    {
                        this.SaveProcess();
                    }
                }
                else
                {
                    this.SaveProcess();
                }
            }
        }

        private void btnTotalSave_Click(object sender, RoutedEventArgs e)
        {
            // CSR : C20220512-000432 Popup 띄우는 조건
            // 저장 체크중간에 Popup을 띄워야 하는 조건 때문에 기존에 Save 함수를 Validation 부분과 Transaction 부분으로 분리
            if (chkMachMulti.IsChecked == false)
            {
                if (!this.TotalSaveValidation())
                {
                    return;
                }
            }


            // Remark만 저장하는 경우는 Popup 안띄우고 저장후 종료.
            if (this.isOnlyRemarkSave)
            {
                this.TotalSaveProcessOnlyRemark();
                return;
            }

            //해당Loss를 일괄로 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    this.isTotalSave = true;
                    if (this.IsOpenQAPopup())
                    {
                        // Open Popup
                        COM001_014_QA wndPopupQA = new COM001_014_QA();
                        wndPopupQA.FrameOperation = FrameOperation;

                        if (wndPopupQA != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = true;      // 여러개 저장
                            C1WindowExtension.SetParameters(wndPopupQA, Parameters);
                            wndPopupQA.Closed -= new EventHandler(this.wndPopupQA_Closed);
                            wndPopupQA.Closed += new EventHandler(this.wndPopupQA_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopupQA.ShowModal()));
                        }
                    }
                    else
                    {
                        if (bUseEqptLossAppr)
                        {
                            // 당일 수정건이 아닌 경우
                            if (ValidationChkDDay().Equals("AUTH_ONLY"))
                            {
                                string lossCode = Util.GetCondition(cboLoss);
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                                DataRow dr = RQSTDT.NewRow();
                                dr["LOSS_CODE"] = lossCode;
                                RQSTDT.Rows.Add(dr);

                                DataTable dtUpprLoss = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                                if (dtUpprLoss.Rows.Count != 0)
                                {
                                    string upprLossCode = Convert.ToString(dtUpprLoss.Rows[0]["UPPR_LOSS_CODE"]);
                                    // 수정할 Loss가 비조업인 경우
                                    if (upprLossCode.Equals("10000"))
                                    {
                                        TotalApprovalProcess();
                                    }
                                    else
                                    {    //Machine Multi 2025 07 10 오화백
                                        if (chkMachMulti.IsChecked == true)
                                        {
                                            TotalMultiSaveProcess();
                                        }
                                        else
                                        {
                                            this.TotalSaveProcess();
                                        }
                                    }
                                }
                            }
                            else
                            {     //Machine Multi 2025 07 10 오화백
                                if (chkMachMulti.IsChecked == true)
                                {
                                    TotalMultiSaveProcess();
                                }
                                else
                                {
                                    this.TotalSaveProcess();
                                }
                            }
                        }
                        else
                        {      //Machine Multi 2025 07 10 오화백
                            if (chkMachMulti.IsChecked == true)
                            {
                                TotalMultiSaveProcess();
                            }
                            else
                            {
                                this.TotalSaveProcess();
                            }
                        }
                    }
                }
            });
        }

        // CSR : C20220512-000432 질문 Popup 띄우기 여부 확인.
        private bool IsOpenQAPopup()
        {
            bool returnValue = false;

            if (this.GetPackApplyLIneByUI(_grid_eqsg))
            {
                // 질문답변 다 입력후에 다시 저장버튼 누른 경우, 질문지 popup 안띄우고 그대로 저장.
                if (CommonVerify.HasTableRow(this.dtQA))
                {
                    returnValue = false;
                }
                else
                {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        private string ValidationChkDDay()
        {
            string bDDay = string.Empty;

            DateTime dtNowDay = DateTime.ParseExact(GetNowDate(), "yyyyMMdd", null);
            DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);

            if ((dtNowDay - dtSearchDay).TotalDays >= 0 && (dtNowDay - dtSearchDay).TotalDays < Convert.ToDouble(strAttr1))
            {
                bDDay = "ALL";
            }
            else if ((dtNowDay - dtSearchDay).TotalDays >= Convert.ToDouble(strAttr1) && (dtNowDay - dtSearchDay).TotalDays <= Convert.ToDouble(strAttr2))
            {
                bDDay = "AUTH_ONLY";
            }
            else
            {
                bDDay = "NO_REG";
            }

            return bDDay;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Validation 부분)
        private bool SaveValidation()
        {
            orginalCbo();

            this.isOnlyRemarkSave = false;
            #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석처리
            //TextRange textRange = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd);
            // Remark만 단순히 저장하는 경우에는 질문지 저장 안함.
            //if (!string.IsNullOrEmpty(textRange.Text) && !textRange.Text.Equals("\r\n") && cboLoss.Text.Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty())
            //{
            //    this.isOnlyRemarkSave = true;
            //}

            #endregion


            if (!event_valridtion())
            {
                return false;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") > 0)
            {
                Util.MessageValidation("SFU3490"); //하나의 부동내역을 저장 할 경우 check box선택을 모두 해제 후 \r\n 한개의 행만 더블클릭  해주세요
                return false;
            }

            //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
            if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
            {
                string sLoss = Util.GetCondition(cboLoss);
                string sLossDetl = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

                DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLoss + "' AND ATTRIBUTE4 = '" + sLossDetl + "'");

                if (rows.Length > 0)
                {
                    int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
                    string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

                    //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
                    if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
                    {
                        Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
                        rtbLossNote.Focus();
                        return false;
                    }
                }
            }

            if (new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim().Length > 1000)
            {
                Util.MessageValidation("SFU5182");  //비고는 최대 1000자 까지 가능합니다.
                rtbLossNote.Focus();
                return false;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return false;
            }

            // C20210213-000002 : 폴란드 PACK에 대하여 신규 권한 추가 | 김건식
            if (bPack && LoginInfo.CFG_SHOP_ID.Equals("G481"))
            {
                if (!CeckPackProcessing())
                {
                    Util.MessageValidation("SFU8333"); // 전일에 대하여 저장기능을 사용할 수 없는 사용자입니다. \n권한이 있는 사용자에게 문의 하십시오.
                    return false;
                }

            }

            // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK만 VALIDATION 추가
            if (bPack && (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT"))) // C20200728 - 000321 원인설비 - SELECT - 초기화 설정(PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return false;
            }

            //작업 담당자 필수 :
            if (string.Equals(GetAreaType(), "P"))
            {
                if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU1842"); //작업자를 선택 하세요.
                    return false;
                }
            }

            ValidateNonRegisterLoss("ONE");

            // 리마크만 저장할경우 Loss 및 Loss Detail Validation 체크 안함.
            if (!isOnlyRemarkSave)
            {
                // 설비 Check
                if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) || cboEquipment.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU1153"); // 설비를 선택하세요
                    return false;
                }

                // Loss Check
                if (cboLoss.SelectedIndex < 0 || string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()) || cboLoss.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                    return false;
                }

                // Loss Detail Check
                if (popLossDetl.SelectedValue.IsNullOrEmpty())
                {
                    Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                    return false;
                }
            }

            if (!this.isOnlyRemarkSave)  // Remark만 저장하는 경우는 FCR 그룹 필수 체크 안함
            {
                //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
                //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
                string sEqptLossFcrCheck = GetprodEqptLossFcrGroupChkResult(Util.GetCondition(cboEquipment),   //메인설비
                                                                            Util.GetCondition(cboOccurEqpt),   //원인설비
                                                                            popLossDetl.SelectedValue.ToString(),
                                                                            Util.GetCondition(cboLoss),
                                                                            Util.GetCondition(cboFailure),
                                                                            Util.GetCondition(cboCause),
                                                                            Util.GetCondition(cboResolution)
                                                                           );
                if (!(string.Equals(sEqptLossFcrCheck, "Y")))
                {
                    switch (sEqptLossFcrCheck)
                    {
                        case "N1":
                            Util.MessageInfo("SFU3212"); //  현상을 선택해주세요
                            break;
                        case "N2":
                            Util.MessageInfo("SFU3213"); // 원인을 선택해주세요
                            break;
                        case "N3":
                            Util.MessageInfo("SFU3214"); // 조치를 선택해주세요
                            break;
                        default:
                            Util.MessageInfo("SFU9202"); // FCR 그룹이 등록되지 않은 내역입니다.(현상/원인/조치)
                            break;

                    }
                    return false;
                }
            }

            if (!SaveDupDataChk())
            {
                Util.MessageInfo("SUF9018");
                return false;
            }

            if (!ValidateEqptLossAppr("SAVE"))
            {
                return false;
            }
            //Machine Multi 2025 07 10 오화백
            if (chkMachMulti.IsChecked == false)
            {
                if (!SaveDataChk(GetPreLossSeqnoForSaveALL()))
                {
                    Util.MessageInfo("SUF9018"); // 업데이트된 LOSS DATA가 존재합니다.  화면을 다시 조회해주세요.
                    return false;
                }
            }

            return true;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Transaction 부분 Only Remark)
        private void SaveProcessOnlyRemark()
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_END <= '" + txtEndHidn.Text + "'", "");

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            DataRow dr = RQSTDT.NewRow();

            string msg = "";

            msg = "SFU3441";//"해당 Loss의 비고 항목만 저장하시겠습니까?";

            //Machine 설비 사용 체크 by 오화백 2023 03.16
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);

                }
            }
            else
            {
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
            }
            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
            dr["LOSS_CODE"] = null;
            dr["LOSS_DETL_CODE"] = null;
            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
            dr["SYMP_CODE"] = null;
            dr["CAUSE_CODE"] = null;
            dr["REPAIR_CODE"] = null;
            dr["OCCR_EQPTID"] = null;
            dr["USERID"] = LoginInfo.USERID;

            if (string.Equals(GetAreaType(), "P"))
            {
                dr["WRK_USERNAME"] = txtPerson.Tag;
            }

            RQSTDT.Rows.Add(dr);

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    else
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK", "RQSTDT", "RSLTDT", RQSTDT);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    btnSearch_Click(null, null);
                    chkT.IsChecked = false;
                    chkW.IsChecked = false;
                    chkU.IsChecked = false;

                    Util.AlertInfo("SFU1270");  //저장되었습니다.
                }
            });
        }
        private DataTable GetLossSaveDataSet()
        {
            DataTable indata = new DataTable(); //indataSet.Tables["INDATA"];
            indata.TableName = "INDATA";
            indata.Columns.Add("AREAID", typeof(string));
            indata.Columns.Add("EQPTID", typeof(string));
            indata.Columns.Add("WRK_DATE", typeof(string));
            indata.Columns.Add("STRT_DTTM", typeof(string));
            indata.Columns.Add("END_DTTM", typeof(string));
            indata.Columns.Add("LOSS_CODE", typeof(string));
            indata.Columns.Add("LOSS_DETL_CODE", typeof(string));
            indata.Columns.Add("LOSS_NOTE", typeof(string));
            indata.Columns.Add("SYMP_CODE", typeof(string));
            indata.Columns.Add("CAUSE_CODE", typeof(string));
            indata.Columns.Add("REPAIR_CODE", typeof(string));
            indata.Columns.Add("OCCR_EQPTID", typeof(string));
            indata.Columns.Add("SYMP_CNTT", typeof(string));
            indata.Columns.Add("CAUSE_CNTT", typeof(string));
            indata.Columns.Add("REPAIR_CNTT", typeof(string));
            indata.Columns.Add("USERID", typeof(string));
            indata.Columns.Add("PRE_LOSS_SEQNO", typeof(string));
            indata.Columns.Add("CHKW", typeof(string));
            indata.Columns.Add("CHKT", typeof(string));
            indata.Columns.Add("CHKU", typeof(string));
            indata.Columns.Add("SAVETYPE", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                indata.Columns.Add("WRK_USERNAME", typeof(string));
            }

            DataRow newRow = indata.NewRow();

            newRow["AREAID"] = _areaid;
            newRow["EQPTID"] = _eqptid;
            newRow["WRK_DATE"] = _wrk_date;
            newRow["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            newRow["END_DTTM"] = Util.GetCondition(txtEndHidn);
            newRow["LOSS_CODE"] = Util.GetCondition(cboLoss);
            newRow["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
            newRow["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
            newRow["SYMP_CODE"] = Util.GetCondition(cboFailure);
            newRow["CAUSE_CODE"] = Util.GetCondition(cboCause);
            newRow["REPAIR_CODE"] = Util.GetCondition(cboResolution);
            newRow["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
            newRow["USERID"] = LoginInfo.USERID;
            newRow["PRE_LOSS_SEQNO"] = GetPreLossSeqnoForSave();
            newRow["SAVETYPE"] = "SAVE";
            if (string.Equals(GetAreaType(), "P"))
            {
                newRow["WRK_USERNAME"] = txtPerson.Tag;
            }

            if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true) //일괄등록이 하나라도 체크되어 있으면 Run 은 살린 상태로 개별 저장
            {
                if (chkT.IsChecked == true)
                {
                    newRow["CHKT"] = "T";
                }
                else
                {
                    newRow["CHKT"] = "";
                }

                if (chkW.IsChecked == true)
                {
                    newRow["CHKW"] = "W";
                }
                else
                {
                    newRow["CHKW"] = "";
                }

                if (chkU.IsChecked == true)
                {
                    newRow["CHKU"] = "U";
                }
                else
                {
                    newRow["CHKU"] = "";
                }
            }

            indata.Rows.Add(newRow);

            return indata;
        }


        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Transaction 부분)
        private void SaveProcess()
        {
            DataTable INDATA = GetLossSaveDataSet();

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            try
            {

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true) //일괄등록이 하나라도 체크되어 있으면 Run 은 살린 상태로 개별 저장
                {
                    // UPD 조건 다름..
                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST_V02", "RQSTDT", "RSLTDT", INDATA);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_EACH_V02", "RQSTDT", "RSLTDT", INDATA);
                    this.SaveQA(INDATA);
                }
                else
                {
                    new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_V02", "RQSTDT", "RSLTDT", INDATA);

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST_V02", "RQSTDT", "RSLTDT", INDATA);

                        new ClientProxy().ExecuteServiceSync("BR_EQPT_EQPTLOSS_UPD_ALARM_NON_TRGT_V02", "INDATA", null, INDATA);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    this.SaveQA(INDATA);
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                if (dgDetail.Rows.Count != 0)
                {
                    dgDetail.ScrollIntoView(idx, 0);
                }

                Util.AlertInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApprovalProcess()
        {
            string eqptId = string.Empty;
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    eqptId = Util.GetCondition(cboEquipment);
                }
                else
                {
                    eqptId = Util.GetCondition(cboEquipment_Machine);
                }
            }
            else
            {
                eqptId = Util.GetCondition(cboEquipment);
            }
            string wrkDate = Util.GetCondition(ldpDatePicker);
            string strtDttm = Util.GetCondition(txtStartHidn);
            string endDttm = Util.GetCondition(txtEndHidn);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
            string lossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;

            //결재 요청 Popup 창 Open
            COM001_014_APPR_ASSIGN popup = new COM001_014_APPR_ASSIGN();
            popup.FrameOperation = FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.GetString();
                Parameters[1] = eqptId;
                Parameters[2] = wrkDate;
                Parameters[3] = strtDttm;
                Parameters[4] = endDttm;
                Parameters[5] = lossCode;
                Parameters[6] = lossDetlCode;
                Parameters[7] = lossNote;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(Popup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void TotalApprovalProcess()
        {
            string wrkDate = Util.GetCondition(ldpDatePicker);
            string strtDttm = Util.GetCondition(txtStartHidn);
            string endDttm = Util.GetCondition(txtEndHidn);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
            string lossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;

            DataTable dtLossData = new DataTable();
            dtLossData.Columns.Add("EQPTID", typeof(string));
            dtLossData.Columns.Add("STRT_DTTM", typeof(string));
            dtLossData.Columns.Add("END_DTTM", typeof(string));
            dtLossData.Columns.Add("WRK_DATE", typeof(string));
            dtLossData.Columns.Add("LOSS_SEQNO", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_CODE", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_DETL_CODE", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_CNTT", typeof(string));
            dtLossData.Columns.Add("USERID", typeof(string));
            dtLossData.Columns.Add("APPR_USERID", typeof(string));
            dtLossData.Columns.Add("LOSS_CODE", typeof(string));
            dtLossData.Columns.Add("LOSS_DETL_CODE", typeof(string));
            dtLossData.Columns.Add("LOTID", typeof(string));
            dtLossData.Columns.Add("TRBL_CODE", typeof(string));
            dtLossData.Columns.Add("EIOSTAT", typeof(string));

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = dtLossData.NewRow();
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EQPTID"));
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    dr["WRK_DATE"] = wrkDate;
                    dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    dr["APPR_REQ_LOSS_CODE"] = lossCode;
                    dr["APPR_REQ_LOSS_DETL_CODE"] = lossDetlCode;
                    dr["APPR_REQ_LOSS_CNTT"] = lossNote;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["APPR_USERID"] = string.Empty;
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_CODE"));
                    dr["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_DETL_CODE"));
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    dr["TRBL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "TRBL_CODE"));
                    dr["EIOSTAT"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EIOSTAT"));
                    dtLossData.Rows.Add(dr);
                }
            }

            //결재 요청 Popup 창 Open
            COM001_014_TOTAL_APPR_ASSIGN popup = new COM001_014_TOTAL_APPR_ASSIGN();
            popup.FrameOperation = FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.GetString();
                Parameters[1] = dtLossData;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(Popup_Closed2);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }
        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Validation 부분)
        private bool TotalSaveValidation()
        {
            orginalCbo();

            this.isOnlyRemarkSave = false;
            TextRange textRange = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd);

            #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석
            // Remark만 단순히 저장하는 경우에는 질문지 저장 안함.
            //if (!string.IsNullOrEmpty(textRange.Text) && !textRange.Text.Equals("\r\n") && cboLoss.Text.Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty())
            //{
            //    this.isOnlyRemarkSave = true;
            //}
            #endregion

            if (!event_valridtion())
            {
                return false;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return false;
            }

            //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
            if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
            {
                string sLoss = Util.GetCondition(cboLoss);
                string sLossDetl = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

                DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLoss + "' AND ATTRIBUTE4 = '" + sLossDetl + "'");

                if (rows.Length > 0)
                {
                    int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
                    string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

                    //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
                    if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
                    {
                        Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
                        rtbLossNote.Focus();
                        return false;
                    }
                }
            }

            if (new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim().Length > 1000)
            {
                Util.MessageValidation("SFU5182");  //비고는 최대 1000자 까지 가능합니다.
                rtbLossNote.Focus();
                return false;
            }

            // C20210213-000002 : 폴란드 PACK에 대하여 신규 권한 추가 | 김건식
            if (bPack && LoginInfo.CFG_SHOP_ID.Equals("G481"))
            {
                if (!CeckPackProcessing())
                {
                    Util.MessageValidation("SFU8333"); // 전일에 대하여 저장기능을 사용할 수 없는 사용자입니다. \n권한이 있는 사용자에게 문의 하십시오.
                    return false;
                }
            }

            // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK만 체크하도록 수정
            if (bPack && (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT"))) // C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return false;
            }

            if (cboLoss.Text.ToString().Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty() && (string.IsNullOrEmpty(textRange.Text) || !textRange.Text.Equals("\r\n")))
            {
                Util.MessageValidation("SFU3485"); //저장내역을 입력해주세요
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return false;
            }
            //Machine Multi 2025 07 10 오화백
            if (chkMachMulti.IsChecked == false && _Util.GetDataGridCheckCnt(dgDetail, "CHK") == 1)
            {
                Util.MessageValidation("SFU3487"); //일괄등록의 경우 한개 이상의 부동내역을 선택해주세요
                return false;
            }

            if (string.Equals(GetAreaType(), "P"))
            {
                if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU1842"); //작업자를 선택 하세요.
                    return false;
                }
            }

            // 설비 Check
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) || cboEquipment.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageInfo("SFU3514"); // 설비를 선택하세요
                return false;
            }

            ValidateNonRegisterLoss("TOTAL");

            // 리마크만 저장할경우 Loss 및 Loss Detail Validation 체크 안함.
            if (!isOnlyRemarkSave)
            {
                // Loss Check
                if (cboLoss.SelectedIndex < 0 || string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()) || cboLoss.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                    return false;
                }

                // Loss Detail Check
                if (popLossDetl.SelectedValue.IsNullOrEmpty())
                {
                    Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                    return false;
                }
            }

            if (!this.isOnlyRemarkSave)  // Remark만 저장하는 경우는 FCR 그룹 필수 체크 안함
            {
                //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
                //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
                string sEqptLossFcrCheck = GetprodEqptLossFcrGroupChkResult(Util.GetCondition(cboEquipment),  //메인설비
                                                                            Util.GetCondition(cboOccurEqpt),  //원인설비
                                                                            popLossDetl.SelectedValue.ToString(),
                                                                            Util.GetCondition(cboLoss),
                                                                            Util.GetCondition(cboFailure),
                                                                            Util.GetCondition(cboCause),
                                                                            Util.GetCondition(cboResolution)
                                                                           );
                if (!(string.Equals(sEqptLossFcrCheck, "Y")))
                {
                    switch (sEqptLossFcrCheck)
                    {
                        case "N1":
                            Util.MessageInfo("SFU3212"); //  현상을 선택해주세요
                            break;
                        case "N2":
                            Util.MessageInfo("SFU3213"); // 원인을 선택해주세요
                            break;
                        case "N3":
                            Util.MessageInfo("SFU3214"); // 조치를 선택해주세요
                            break;
                        default:
                            Util.MessageInfo("SFU9202"); // FCR 그룹이 등록되지 않은 내역입니다.(현상/원인/조치)
                            break;

                    }
                    return false;
                }
            }

            if (!ValidateEqptLossAppr("TOTAL"))
            {
                return false;
            }
            //Machine Multi 2025 07 10 오화백
            if (chkMachMulti.IsChecked == false)
            {
                if (!SaveDataChk(GetPreLossSeqnoForSaveALL()))
                {
                    Util.MessageInfo("SUF9018"); // 업데이트된 LOSS DATA가 존재합니다.  화면을 다시 조회해주세요.
                    return false;
                }
            }

            return true;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Transaction 부분 Only Remark)
        private void TotalSaveProcessOnlyRemark()
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

                    DataSet ds = new DataSet();
                    DataTable RQSTDT = ds.Tables.Add("INDATA");
                    //RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                    RQSTDT.Columns.Add("END_DTTM", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
                    RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CHKW", typeof(string));
                    RQSTDT.Columns.Add("CHKT", typeof(string));
                    RQSTDT.Columns.Add("CHKU", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
                    }

                    for (int i = 0; i < dgDetail.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            DataRow dr = RQSTDT.NewRow();

                            //Machine 설비 사용 체크 by 오화백 2023.03.16
                            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                            {
                                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                                {
                                    dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                }
                                else
                                {
                                    dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine, "SFU3514"); //설비는필수입니다.
                                }
                            }
                            else
                            {
                                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                            }

                            if (dr["EQPTID"].Equals("")) return;
                            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                            dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                            dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                            dr["LOSS_CODE"] = null;
                            dr["LOSS_DETL_CODE"] = null;
                            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
                            dr["SYMP_CODE"] = null;
                            dr["CAUSE_CODE"] = null;
                            dr["REPAIR_CODE"] = null;
                            dr["OCCR_EQPTID"] = null;
                            dr["USERID"] = LoginInfo.USERID;
                            if (string.Equals(GetAreaType(), "P"))
                            {
                                dr["WRK_USERNAME"] = txtPerson.Tag;
                            }

                            RQSTDT.Rows.Add(dr);
                        }
                    }

                    if (string.Equals(GetAreaType(), "P"))
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL_PACK", "INDATA", null, ds);
                    }
                    else
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL", "INDATA", null, ds);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    btnSearch_Click(null, null);
                    chkT.IsChecked = false;
                    chkW.IsChecked = false;
                    chkU.IsChecked = false;

                    dgDetail.ScrollIntoView(idx, 0);

                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                }
            });
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Transaction 부분)
        private void TotalSaveProcess()
        {
            if (!this.isTotalSave)
            {
                return;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

            //DataSet ds = new DataSet();
            //DataTable RQSTDT = ds.Tables.Add("INDATA");
            //RQSTDT.TableName = "RQSTDT";
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("PRE_LOSS_SEQNO", typeof(string));
            RQSTDT.Columns.Add("SAVETYPE", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            // (int i = 0; i < dgDetail.GetRowCount(); i++)
            //{
            //if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
            //{
            DataRow dr = RQSTDT.NewRow();

            dr["AREAID"] = _areaid;
            dr["EQPTID"] = _eqptid;
            dr["WRK_DATE"] = _wrk_date;
            //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
            //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
            dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
            if (dr["LOSS_CODE"].Equals("")) return;

            dr["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
            if (dr["LOSS_DETL_CODE"].Equals(""))
            {
                if (popLossDetl.SelectedValue.IsNullOrEmpty())
                {
                    // 부동내용을 입력하세요.
                    Util.MessageValidation("SFU3631");
                    return;
                }
            }

            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
            dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
            dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
            dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
            dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
            dr["USERID"] = LoginInfo.USERID;
            if (string.Equals(GetAreaType(), "P"))
            {
                dr["WRK_USERNAME"] = txtPerson.Tag;
            }
            dr["PRE_LOSS_SEQNO"] = GetPreLossSeqnoForSaveALL();
            dr["SAVETYPE"] = "ALL";

            RQSTDT.Rows.Add(dr);
            // }
            //}

            try
            {

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                {
                    Util.MessageValidation("SFU3489");//개별등록일 경우 일괄저장 기능 사용 불가
                    return;
                }
                else
                {
                    new ClientProxy().ExecuteServiceSync("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL_V02", "INDATA", null, RQSTDT);

                    this.SaveQA(RQSTDT);        // 질문지 저장.
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                if (dgDetail.GetRowCount() != 0)
                {
                    dgDetail.ScrollIntoView(idx, 0);
                }

                Util.MessageInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
        //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
        private string GetprodEqptLossFcrGroupChkResult(string sMainEqptID, string sEqptID, string sLossDetlCode, string sLossCode, string sSympCode, string sCauseCode, string sRepairCode)
        {
            string sChkResult = "N";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MAIN_EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MAIN_EQPTID"] = sMainEqptID;  // 메인설비
                dr["EQPTID"] = sEqptID;      // 원인설비
                dr["LOSS_DETL_CODE"] = sLossDetlCode;
                dr["LOSS_CODE"] = sLossCode;
                dr["SYMP_CODE"] = sSympCode;
                dr["CAUSE_CODE"] = sCauseCode;
                dr["REPAIR_CODE"] = sRepairCode;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_FCR_MAPP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sChkResult = Util.NVC(dtResult.Rows[0][0].ToString());
                }
                else
                {
                    sChkResult = "";
                }

                return sChkResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return "";
            }
        }

        /// <summary>
        /// 삭제 더블클릭시
        /// 데이터 원복
        /// </summary>
        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            occurEqptFlag = "N";
            svMap.ScrollToVerticalOffset(10);
            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            //if (chkTestDev.IsChecked == true)
            //{
            //    return;
            //}

            if (dgDetail.CurrentRow != null)
            {
                if (RunSplit.Equals("Y"))
                {
                    if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("Y"))
                    {
                        if (!ValidateEqptLossAppr("CLICK"))
                        {
                            return;
                        }
                        //가동 데이터를 분할 한 데이터이므로 추가된 Loss가 초기화 됩니다. 그래도 삭제하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3205"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {

                            if (result.ToString().Equals("OK"))
                            {
                                try
                                {
                                    DataSet ds = new DataSet();
                                    DataTable dt = ds.Tables.Add("INDATA");

                                    dt.Columns.Add("STRT_DTTM", typeof(DateTime));
                                    dt.Columns.Add("END_DTTM", typeof(DateTime));
                                    dt.Columns.Add("EQPTID", typeof(string));
                                    dt.Columns.Add("WRK_DATE", typeof(string));
                                    dt.Columns.Add("AREAID", typeof(string));
                                    dt.Columns.Add("USERID", typeof(string));
                                    dt.Columns.Add("START_DTTM_YMDHMS", typeof(string));

                                    DataRow row = dt.NewRow();
                                    row["STRT_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["END_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                    row["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "WRK_DATE"));
                                    row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                                    row["USERID"] = LoginInfo.USERID;
                                    row["START_DTTM_YMDHMS"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                    dt.Rows.Add(row);

                                    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RUN_SPLT_RESET", "INDATA", null, ds);

                                    btnSearch_Click(null, null);

                                    /*  MI2_OSS_0253 설비LOSS RUN구간 시간분할취소(DELETE)후 GRID조회후 ROW선택제외(삭제된ROW) 250516 천진수
                                    if (dgDetail.GetRowCount() != 0)
                                    {
                                        dgDetail.ScrollIntoView(idx, 0);
                                    }
                                    */


                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }
                        }
                        );
                    }


                }

                if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "CHECK_DELETE")).Equals("DELETE") && dgDetail.CurrentColumn != null && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("N")) //삭제 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }

                    //삭제하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet ds = new DataSet();
                                DataTable dt = ds.Tables.Add("IN_RESET");

                                dt.Columns.Add("EQPTID", typeof(string));
                                dt.Columns.Add("WRK_DATE", typeof(string));
                                dt.Columns.Add("STRT_DTTM", typeof(string));
                                dt.Columns.Add("END_DTTM", typeof(string));
                                dt.Columns.Add("USERID", typeof(string));
                                dt.Columns.Add("TRANSACTION_SERIAL_NO", typeof(string)); //2024.10.19 MES 리빌딩 PJT

                                DataRow dr = dt.NewRow();
                                //Machine Multi 2025 07 10 오화백
                                if (chkMachMulti.IsChecked == true)
                                {
                                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                }
                                else
                                {
                                    //Machine 설비 사용 체크 by 오화백 2023.03.16
                                    if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                                    {
                                        if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                                        {
                                            dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                        }
                                        else
                                        {
                                            dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine, "SFU3514"); //설비는필수입니다.
                                        }
                                    }
                                    else
                                    {
                                        dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                                    }
                                }
                               
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;
                                dr["TRANSACTION_SERIAL_NO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "TRANSACTION_SERIAL_NO")); //2024.10.19 MES 리빌딩 PJT

                                dt.Rows.Add(dr);

                                DataTable inLoss = ds.Tables.Add("IN_CHG_HIST");
                                inLoss.Columns.Add("EQPTID", typeof(string));
                                inLoss.Columns.Add("WRK_DATE", typeof(string));
                                inLoss.Columns.Add("STRT_DTTM", typeof(string));
                                inLoss.Columns.Add("END_DTTM", typeof(string));
                                inLoss.Columns.Add("USERID", typeof(string));

                                dr = inLoss.NewRow();
                                //Machine 설비 사용 체크 by 오화백 2023.03.16
                                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                                {
                                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                                    {
                                        dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                    }
                                    else
                                    {
                                        dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine, "SFU3514"); //설비는필수입니다.
                                    }
                                }
                                else
                                {
                                    dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                                }
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;
                                inLoss.Rows.Add(dr);

                                new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RESET", "IN_RESET,IN_CHG_HIST", null, ds);

                                //UPDATE 처리후 재조회
                                btnSearch_Click(null, null);
                                if (dgDetail.GetRowCount() != 0)
                                {
                                    dgDetail.ScrollIntoView(idx, 0);
                                }


                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("SPLIT") && dgDetail.CurrentColumn != null) //분할 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }

                    //분할하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3120"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            COM001_014_SPLIT wndPopup = new COM001_014_SPLIT();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[6];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO"));
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                Parameters[4] = Convert.ToString(cboArea.SelectedValue);
                                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "TRANSACTION_SERIAL_NO"));

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("MERGE") && dgDetail.CurrentColumn != null) //병합 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }

                    //병합하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2876"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet dsData = new DataSet();

                                DataTable dtIn = dsData.Tables.Add("INDATA");
                                dtIn.Columns.Add("EQPTID", typeof(string));
                                dtIn.Columns.Add("FROM_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("TO_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("USERID", typeof(string));

                                DataRow row = null;
                                row = dtIn.NewRow();
                                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                row["FROM_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100;
                                row["TO_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100 + 99;
                                row["USERID"] = LoginInfo.USERID;

                                dtIn.Rows.Add(row);



                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_SPLIT_RESET", "INDATA", "OUTDATA", dsData);


                                if (Convert.ToInt16(dsRslt.Tables["OUTDATA"].Rows[0]["CNT"]) > 0)
                                {
                                    Util.AlertInfo("SFU1516");  //등록된 데이터를 지우고 병합해주세요.
                                    return;
                                }
                                else
                                {
                                    //UPDATE 처리후 재조회
                                    btnSearch_Click(null, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else //선택처리
                {
                    // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
                    if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                    {
                        btnReset_Click(null, null);

                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"))) == false)
                        {
                            setMapColor_Machine("A" + Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);
                        }
                    }
                    else
                    {
                        btnReset_Click(null, null);
                        setMapColor(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);
                    }
                }
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_014_SPLIT window = sender as COM001_014_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }
        private void wndPopup_Closed2(object sender, EventArgs e)
        {
            COM001_014_LOSS_SPLIT window = sender as COM001_014_LOSS_SPLIT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        // CSR : C20220512-000432
        private void wndPopupQA_Closed(object sender, EventArgs e)
        {
            COM001_014_QA window = sender as COM001_014_QA;

            bool? isTotalSave = window.TOTAL_SAVE;
            switch (window.DialogResult)
            {
                case MessageBoxResult.OK:           // 질문답변 Data가 있다면 질문답변 Data 저장하기
                    this.dtQA = window.STANDARD_PROCESS_QUESTION_TABLE;
                    break;
                case MessageBoxResult.None:         // 질문답변 Data가 없으면 그냥 저장.
                    break;
                default:
                    break;
            }

            // Loss Save Transaction...
            if (isTotalSave == null)
            {
                return;
            }
            else if (isTotalSave == true)
            {
                this.TotalSaveProcess();
            }
            else
            {
                this.SaveProcess();
            }
        }

        /// <summary>
        /// 상세 데이터 색변환하기
        /// </summary>
        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
                    string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
                    string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));

                    if (sCheck.Equals("DELETE"))
                    {
                        System.Drawing.Color color = GridBackColor.Color4;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                    else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
                    }
                    else
                    {
                        //Machine Multi 2025 07 10 오화백
                        if (chkMachMulti.IsChecked == false)
                        {
                            int originSeconds = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ORIGIN_SECONDS"));
                            int eqptLossMandRegBasOverTime = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_LOSS_MAND_REG_BAS_OVER_TIME"));

                            if (eqptLossMandRegBasOverTime > 0 && eqptLossMandRegBasOverTime != 180 && originSeconds >= eqptLossMandRegBasOverTime)
                            {
                                //C20210126-000047 경과시간(초) 가 필수입력 기준시간(초) 보다 크면 색 지정. 180(초)는 전사 기본값이기 때문에 색 변경하지 않음.
                                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 102, 0, 51));
                            }
                            else
                            {
                                System.Drawing.Color color = GridBackColor.Color6;
                                e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            }
                        }
                        else
                        {
                            System.Drawing.Color color = GridBackColor.Color6;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        }
                    }

                }

                //link 색변경
                if (e.Cell.Column.Name != null)
                {
                    if (e.Cell.Column.Name.Equals("CHECK_DELETE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name.Equals("SPLIT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }

                // 비고 칼럼 사이즈
                if (e.Cell.Column.Name.Equals("txtNote"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                }

            }));

        }

        private void cboLossEqsgProc_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLossEqsgProc.SelectedValue.Equals("SELECT"))
            {
                string[] loss = cboLossEqsgProc.SelectedValue.ToString().Split('-');
                string[] lossText = cboLossEqsgProc.Text.ToString().Split('-');

                cboLoss.SelectedValue = loss[0];

                // 2023.05.28 윤지해 CSR ID E20230330-001442  LOSS LV3 리스트에 없을 경우 빈칸으로 입력
                if (!loss[1].Equals(""))
                {
                    DataTable dt = DataTableConverter.Convert(popLossDetl.ItemsSource);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int inx = 0; inx < dt.Rows.Count; inx++)
                        {
                            if (dt.Rows[inx]["CBO_CODE"].ToString() == loss[1])
                            {
                                popLossDetl.SelectedValue = loss[1];
                                popLossDetl.SelectedText = lossText[0];
                            }
                        }
                    }
                    //popLossDetl.SelectedValue = loss[1];
                    //popLossDetl.SelectedText = lossText[0];
                }
                else
                {
                    popLossDetl.SelectedValue = null;
                    popLossDetl.SelectedText = null;
                }

            }
        }

        #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS, FCR 매핑 추가
        private void popLossDetl_ValueChanged(object sender, EventArgs e)
        {
            // 부동내용이 바뀌면 현상, 원인, 조치 전체 초기화
            CommonCombo _combo = new CommonCombo();

            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

            string failCode = string.Empty;
            string causeCode = string.Empty;
            string resolCode = string.Empty;
            string selectedText = string.Empty;
            string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

            cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
            String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
            cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

            #region 2023.03.28 윤지해 CSR ID E20230321-001518  GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
            // 현상이 1:1 매칭일 경우 자동으로 INDEX 세팅
            // 2023.05.28 기존 ESGM만 적용했던 건 전체 적용으로 변경
            if (/*LoginInfo.CFG_SHOP_ID.Equals("G671") && */cboFailure.Items.Count == 2)
            {
                SetCboFailure_Index();
            }
            else
            {
                cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
            }
            #endregion
        }

        private void cboFailure_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            /*
             * <현상 변경>
             * 1. N/A 선택될 경우              : 현상 콤보박스 초기화 O
             *                                   원인, 조치 콤보박스 초기화 X
             * 2. N/A 가 아닌 값이 선택될 경우 : 현상 콤보박스 초기화 X
             *                                   원인 - 선택된 값이 없을 경우 현상, 조치 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             *                                   조치 - 선택된 값이 없을 경우 현상, 원인 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             */
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {
                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboFailure.SelectedValue.IsNullOrEmpty())
                {
                    cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                    String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                    cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                }
                else
                {
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboCause_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {

                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboCause.SelectedValue.IsNullOrEmpty())
                {
                    cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                    String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                    cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                }
                else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboResolution_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {

                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboResolution.SelectedValue.IsNullOrEmpty())
                {
                    cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                    String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                    cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                }
                else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Main 체크박스 클릭시 - Machine 설비Loss 여부 체크 후  Machine 설비 콤보박스 Visible 여부  오화백 2023-03.16 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMain_Click(object sender, RoutedEventArgs e)
        {
            if (chkMain.IsChecked == true)
            {
                Machine.Visibility = Visibility.Collapsed;
                
                //Machine Multi 2025 07 10 오화백 
                MachineMulti.Visibility = Visibility.Collapsed;
                grMulti.Visibility = Visibility.Collapsed;
                if (chkMachMulti.IsChecked == true)
                {
                    chkMachMulti.IsChecked = false;
                    initControls();
                }
            }
            else
            {
                if (MachineEqptChk == "Y")
                {
                    Machine.Visibility = Visibility.Visible;
                }
                else

                {
                    Machine.Visibility = Visibility.Collapsed;
                }
                //Machine Multi 2025 07 10 오화백 
                SetMachineMulti(cboProcess.SelectedValue.ToString());
            }
        }

        private void chkTestDev_Click(object sender, RoutedEventArgs e)
        {
            //if (chkTestDev.IsChecked == true)
            //{
            //    btnSave.IsEnabled = false;
            //    btnTotalSave.IsEnabled = false;
            //    btnEMSWOReq.IsEnabled = false;
            //    btnEqpRemark.IsEnabled = false;
            //    btnReset.IsEnabled = false;
            //}
            //else
            //{
                btnSave.IsEnabled = true;
                btnTotalSave.IsEnabled = true;
                btnEMSWOReq.IsEnabled = true;
                btnEqpRemark.IsEnabled = true;
                btnReset.IsEnabled = true;
            //}
        }

        // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비 변경 시 이벤트
        private void cboOccurEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // LOSS LV2 변경
            CommonCombo _combo = new CommonCombo();
            // PACK, 소형이 아니고 원인설비별 LOSS 등록 여부 체크
            if (occurEqptFlag.Equals("Y"))
            {
                if (!(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))   // 원인설비 선택했을 경우 : BM/PD만 LIST-UP
                {
                    string[] sFilterLoss = { Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboOccurEqpt.SelectedValue), string.Empty };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeOccurEqpt");
                    setOccurEqptLossCombo();
                }
                else // BM/PD 제외 LIST-UP
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), occurEqptFlag, string.Empty };
                    //_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeAll_OccurEqpt");   // 2024.03.19 김태오S  공정별 설비 Loss 분류 제한 내용 건, 호출 비즈 변경.
                }
            }

            // 최근등록 콤보박스 변경
            C1ComboBox[] cboLastLossParent = { cboEquipment };
            if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
            {
                string[] sFilterOccurEqpt = { occurEqptFlag };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent, sFilter: sFilterOccurEqpt, sCase: "cboLastLossPart");
            }
            else
            {
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);
            }

            // 동-라인-공정별 로스 매핑 콤보박스 변경
            if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
            {
                string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue), occurEqptFlag };
                _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "cboLossEqsgProcPart");
            }
            else
            {
                string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
                _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
            }
        }
        #endregion

        #region Mehod

        /// <summary>
        /// 색지도초기화
        /// </summary>
        private void ClearGrid()
        {
            try
            {
                foreach (Border _border in _grid.Children.OfType<Border>())
                {
                    _grid.UnregisterName(_border.Name);
                }

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                //if (chkTestDev.IsChecked == true)
                //{
                //    dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게
                //    dtBeforeList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);//저장 버튼 클릭 시점에서 업데이트된 LOSS DATA와 비교할 DataTable
                //}
                //else
                //{
                    dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게
                    dtBeforeList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN", "RQSTDT", "RSLTDT", RQSTDT);//저장 버튼 클릭 시점에서 업데이트된 LOSS DATA와 비교할 DataTable
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// Machine 설비도 같이 조회 되도록 DA가 변경   - by 오화백 2023.01.20
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList_Machine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    dr["EQPTID"] = cboEquipment.SelectedValue.GetString();
                }
                else
                {
                    dr["EQPTID"] = cboEquipment_Machine.SelectedValue.GetString();
                }
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                //if (chkTestDev.IsChecked == true)
                //{
                //    dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_UNIT_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_UNIT_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게 
                //    dtBeforeList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);//저장 버튼 클릭 시점에서 업데이트된 LOSS DATA와 비교할 DataTable
                //}
                //else
                //{
                dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_UNIT", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_UNIT", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게 
                dtBeforeList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN", "RQSTDT", "RSLTDT", RQSTDT);//저장 버튼 클릭 시점에서 업데이트된 LOSS DATA와 비교할 DataTable
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 색지도 처리
        /// </summary>
        private void SelectProcess()
        {
            try
            {
                _grid.RowDefinitions.Clear();
                _grid.Children.Clear();

                string sEqptID = Util.GetCondition(cboEquipment);
                string sEqptType = (chkMain.IsChecked.Equals(true)) ? "M" : "A";
                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);

                //if (cboProcess.SelectedValue.ToString() == PROC.PACKAGING || cboProcess.SelectedValue.ToString() == PROC.DEGAS ||
                //    cboProcess.SelectedValue.ToString() == PROC.ASSY || cboProcess.SelectedValue.ToString() == PROC.WASHING)
                //{
                //    if ((bool)chkSubEqpt.Checked)
                //    {
                //        sEqptType = "%";
                //    }
                //}

                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();

                //txtStart.Text = "";
                //txtEnd.Text = "";
                //txtTroubleName.Text = "";
                //txtStartHidn.Text = "";
                //txtEndHidn.Text = "";
                //txtEqptName.Text = "";
                //txtMdesc.Text = "";

                //spdMList.ActiveSheet.RowCount = 0;

                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                //spdMList.ActiveSheet.RowCount = (hash_title.Count) + 1;

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정
                    hash_color.Add(sTitle, sID);
                }

                //첫줄 생성처리
                //for (int k = 0; k < hash_title.Count; k++)
                //{
                //    RowDefinition gridRow = new RowDefinition();
                //    gridRow.Height = new GridLength(15);
                //    _grid.RowDefinitions.Add(gridRow);
                //}

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                //for (int i = 0; i < 1000; i++)
                {

                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);

                        _grid.Children.Add(_lable);

                    }

                    //spdMList.ActiveSheet.Cells[nRow, nCol].HorizontalAlignment = CellHorizontalAlignment.Left;

                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);


                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {


                        hash_rs = (Hashtable)hash_list[sEqptTimeList];

                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정


                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);
                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        //if(i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count)
                        //{
                        //    for (int k = 0; k < hash_title.Count; k++)
                        //    {
                        //        RowDefinition gridRow = new RowDefinition();
                        //        gridRow.Height = new GridLength(15);
                        //        _grid.RowDefinitions.Add(gridRow);
                        //    }

                        //}

                        cnt = 0;
                        inc++;
                        //if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        //{
                        //spdMList.ActiveSheet.RowCount = spdMList.ActiveSheet.RowCount + (hash_title.Count) + 1;
                        //}
                    }

                }

                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }

                ////--- 위에서 시간 중복 Hastable 처리
                //foreach (DictionaryEntry de in hash_Merge)
                //{
                //    int nRow1 = int.Parse(de.Key.ToString());
                //    spdMList.ActiveSheet.SetRowMerge(nRow1, FarPoint.Win.Spread.Model.MergePolicy.Always);
                //    FarPoint.Win.LineBorder bevelbrdr = new FarPoint.Win.LineBorder(GridBackColor.Color3, 1);
                //    spdMList.ActiveSheet.Rows[nRow1].Border = bevelbrdr;
                //}

                //spdMList.ActiveSheet.Protect = true;
                //spdMList.ResumeLayout();

                //svMap.ScrollToHorizontalOffset =

                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 색지도 처리 
        /// Machine 설비수정 가능할 경우 SelectProcess복사해서 사용  - by 오화백 2023-03-16
        /// </summary>
        private void SelectProcess_Machine()
        {
            try
            {
                string sEqptID = string.Empty;
                string sEqptType = string.Empty;
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    sEqptID = Util.GetCondition(cboEquipment);
                    sEqptType = "A";
                }
                else
                {
                    sEqptID = Util.GetCondition(cboEquipment_Machine);
                    sEqptType = "M";
                }

                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);



                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();


                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName_Machine(sEqptID);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정                    
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);

                        _grid.Children.Add(_lable);
                    }
                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];
                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정
                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;


                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);

                        }
                    }

                    cnt++;
                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                    }

                }
                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 부동 내역 조회
        /// - 시간 차이가 180초 이상 인 경우
        /// - OP Key-In 인 경우
        /// - LOSS CODE ( 38000)  자재교체인 경우
        /// 색깔 구분
        /// - 분홍색 (2)
        ///   : 180초 이상
        ///   : 기준정보 기준시간 초과인 경우 ( 시작시간이 0 인  기준정보 )
        /// - 회색 (1)
        ///   : OP Key-In
        ///   : 기준정보 기준시간 이내인 경우 ( 시작시간이 0 인  기준정보 )
        /// </summary>
        private void GetEqptLossDetailList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                /// Machine 설비수정 가능할 경우 - by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment);
                    }
                    else
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                    }
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["ASC"] = (bool)chkLossSort.IsChecked ? null : "Y";
                dr["REVERSE_CHECK"] = (bool)chkLossSort.IsChecked ? "Y" : null;
                dr["MIN_SECONDS"] = (bool)chkSearchAll.IsChecked ? 0 : 180;

                RQSTDT.Rows.Add(dr);

                //if (chkTestDev.IsChecked == true)
                //{
                //    if (string.Equals(GetAreaType(), "P"))
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_PACK_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //    else
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_TRBL_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //}
                //else
                //{
                if (string.Equals(GetAreaType(), "P"))
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_TRBL", "RQSTDT", "RSLTDT", RQSTDT);
                }
                //}




                if (cboShift.SelectedValue != null && !string.IsNullOrEmpty(cboShift.SelectedValue.GetString()))
                //if (!cboShift.SelectedValue.ToString().Equals(""))
                {
                    DateTime dJobDate_st = new DateTime();
                    DateTime dJobDate_ed = new DateTime();

                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift_st = drShift[0]["SHFT_STRT_HMS"].ToString();
                        String sShift_ed = drShift[0]["SHFT_END_HMS"].ToString();

                        dJobDate_st = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_st.Substring(0, 2) + ":" + sShift_st.Substring(2, 2) + ":" + sShift_st.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        dJobDate_ed = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                        //작업조의 end시간이 기준시간 보다 작을때
                        if (TimeSpan.Parse(sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2)) < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
                        {
                            dJobDate_ed = DateTime.ParseExact(ldpDatePicker.SelectedDateTime.AddDays(1).ToString("yyyyMMdd") + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        }
                    }

                    try
                    {
                        RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").CopyToDataTable();
                        if (chkLossSort.IsChecked == true)
                        {
                            RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").Reverse().CopyToDataTable();
                        }

                    }
                    catch (Exception ex)
                    {
                        DataTable dt = new DataTable();
                        foreach (DataColumn col in RSLTDT.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.ColumnName));
                        }

                        RSLTDT = dt;
                    }

                }



                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);


                txtRequire.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtWriteEnd.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();


                // 2019-09-30 황기근 사원 수정
                //restrictSave();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 시간 목록
        /// </summary>
        private DataSet GetEqptTimeList(string sJobDate, string sShiftCode)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = sJobDate;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0) { }
            if (Convert.ToString(result.Rows[0]["JOBDATE_YYYYMMDD"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return null;
            }




            DataSet ds = new DataSet();

            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                if (!bPack)
                {
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                }
                else
                {
                    // Pack
                    RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                    RQSTDT.Columns.Add("FROMDATE", typeof(string));
                    RQSTDT.Columns.Add("TODATE", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);

                if (!bPack)
                {
                    dr["PROCID"] = Util.GetCondition(cboProcess);
                    RQSTDT.Rows.Add(dr);

                    dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dr["FROMDATE"] = sJobDate;
                    dr["TODATE"] = sJobDate;
                    RQSTDT.Rows.Add(dr);

                    dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);
                }

                DataTable RSLTDT = new DataTable("RSLTDT");

                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();

                if (sShiftCode.Equals(""))
                {
                    // String sShift = dtShift.Compute("MIN(SHFT_STRT_HMS)", "").ToString();
                    String sShift = dtShift.Rows[0]["SHFT_STRT_HMS"].ToString();
                    if (sShift.Length > 0)
                    {
                        // dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {


                        dJobDate = DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }
                    iTerm = 20;
                    iIncrease = 20;
                }
                else
                {
                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift = drShift[0]["SHFT_STRT_HMS"].ToString();
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {
                        dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];//DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }

                    if (drShift[0]["SHFT_GR_CODE"].ToString().Equals(""))
                    {
                        Util.MessageValidation("SFU3442"); //  "해당 조의 교대 수가 없습니다.
                                                           // ds.Tables. // = null;

                        return ds;
                    }

                    iTerm = int.Parse(drShift[0]["SHFT_GR_CODE"].ToString()) * 10;
                    iIncrease = 10;
                }



                DataTable dtGetDate = new ClientProxy().ExecuteServiceSync("COR_SEL_GETDATE", null, "RSLTDT", null);

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));

                }//

                DataTable RSLTDT1 = RSLTDT.Select("STARTTIME <=" + Convert.ToDateTime(dtGetDate.Rows[0]["SYSTIME"]).ToString("yyyyMMddHHmmss"), "").CopyToDataTable();


                ds.Tables.Add(RSLTDT1);

                ds.Tables[0].TableName = "RSLTDT";
                return ds;
            }
            catch (Exception ex)
            {
                //---commMessage.Show(ex.Message);
                return ds;
            }
        }

        /// <summary>
        /// 설비명 가져오기
        /// </summary>
        private DataTable GetEqptName(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _grid_eqpt;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private bool GetLossTestDev()
        {
            bool chk = true;
            DataTable RSLTDT = new DataTable();
            DataTable RQSTDT = new DataTable();

            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                }
            }
            else
            {
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
            }
            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            RQSTDT.Rows.Add(dr);

            RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSS_HIST_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);

            if (RSLTDT == null || RSLTDT.Rows.Count == 0)
            {
                chk = false;
            }

            return chk;
        }


        /// <summary>
        ///  Machine 설비명 가져오기  - by 오화백 2023 03.16
        /// </summary>
        private DataTable GetEqptName_Machine(string sEqptID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE_UNIT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        /// <summary>
        /// 시간별 상태 목록 가져오기
        /// </summary>
        private DataTable GetEqptLossList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                if (bPack)
                {
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;

                if (bPack)
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = Util.GetCondition(cboArea);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                }
                RQSTDT.Rows.Add(dr);

                //if (chkTestDev.IsChecked == true)
                //{
                //    if (bPack)
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_PACK_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //    else
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //}
                //else
                //{
                if (bPack)
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP", "RQSTDT", "RSLTDT", RQSTDT);
                }
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;

        }

        /// <summary>
        /// 최초 상태 가져오기
        /// </summary>
        private DataTable GetEqptLossFirstList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                if (bPack)
                {
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;

                if (bPack)
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = Util.GetCondition(cboArea);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                }
                RQSTDT.Rows.Add(dr);

                //if (chkTestDev.IsChecked == true)
                //{
                //    if (bPack)
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST_PACK_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //    else
                //    {
                //        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST_TEST_DEV", "RQSTDT", "RSLTDT", RQSTDT);
                //    }
                //}
                //else
                //{
                if (bPack)
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST", "RQSTDT", "RSLTDT", RQSTDT);
                }
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Hashtable hash_rs = new Hashtable();
                    for (int j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        hash_rs.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                    }
                    hash_return.Add(dt.Rows[i]["STARTTIME"].ToString(), hash_rs);
                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                hash_return = null;
            }
            return hash_return;
        }

        /// <summary>
        /// 색지도 클릭시 색상 초기화
        /// </summary>
        private void resetMapColor()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            DataRow[] dtRow = dtMainList.Select("CHK = '1'", "");

            foreach (DataRow dr in dtRow)
            {
                dr["CHK"] = "0";
            }

        }

        /// <summary>
        /// 색지도 클릭시 색깔 칠하기
        /// </summary>
        private void setMapColor(String sTime, String sType, C1.WPF.DataGrid.DataGridRow row = null)
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END > '" + sTime + "'", "");
            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");

            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            try
            {
                if (dtRow.Length > 0)
                {
                    dtRow[0]["CHK"] = "1";

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                    {
                        if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                        }
                        else
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRow[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                        }
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

                        if (row != null)
                        {
                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            drCurrDetail = row;
                            CommonCombo _combo = new CommonCombo();

                            // 항상 존재하는 값
                            string lossCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            string lossDetlCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));

                            // Optional 값
                            string failCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            string causeCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            string resolCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            #endregion

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME")).Equals(""))
                            {
                                txtEqptName.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME"));
                            }

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                            {
                                C1ComboBox[] cboOccurEqptParent = { cboEquipment };
                                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                                cboOccurEqpt.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                            }

                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            //{
                            //    cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            //{
                            //    popLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            //    popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                            //{
                            //    cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                            //{
                            //    cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                            //{
                            //    cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            //}

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            {
                                InitLossCombo();
                                cboLoss.SelectedValue = lossCode;
                            }

                            // 2023.05.28 윤지해 CSR ID E20230330-001442  LOSS LV3 리스트에 없을 경우 빈칸으로 입력
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            {
                                if (!string.IsNullOrEmpty(lossDetlCode))
                                {
                                    DataTable dt = DataTableConverter.Convert(popLossDetl.ItemsSource);
                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        for (int inx = 0; inx < dt.Rows.Count; inx++)
                                        {
                                            if (dt.Rows[inx]["CBO_CODE"].ToString() == Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")))
                                            {
                                                popLossDetl.SelectedValue = lossDetlCode;
                                                popLossDetl.SelectedText = dt.Rows[inx]["CBO_NAME"].ToString();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    popLossDetl.SelectedValue = null;
                                    popLossDetl.SelectedText = null;
                                }
                                //popLossDetl.SelectedValue = lossDetlCode;
                                //popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            }

                            // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS, FCR 매핑
                            string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                            // 현상
                            String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");

                            // 원인
                            String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");

                            // 조치
                            String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                            #endregion

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                            {
                                new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                                //txtLossNote.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                            }
                        }

                    }

                    Border borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        //  dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;  (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString();
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    Hashtable hashStart = borderS.Tag as Hashtable;
                    Hashtable hashEnd = borderE.Tag as Hashtable;

                    DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                    //색칠해야할 셀갯수 =  row 차이 / (설비갯수 + 시간디스플레이) * 컬럼수 + 종료컬럼 - 시작컬럼
                    int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));


                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = new SolidColorBrush(Colors.Blue);
                    }

                    //마지막 칸 정리
                    Border borderEndMinusOne = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                    Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                    if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                    {
                        borderE.Background = new SolidColorBrush(Colors.Blue);
                    }

                    int iRow = Grid.GetRow(borderS);



                    txtEqptName.Text = GetEqptName(hashStart["EQPTID"].ToString(), "M").Rows[0][1].ToString();

                    if (sType.Equals("LIST"))
                    {
                        svMap.ScrollToVerticalOffset((15 * iRow - 8));
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        /// 색지도 클릭시 색깔 칠하기
        /// Machine 설비수정 가능할 경우  Main 설비외 전체 선택이 가능하도록 by오화백  2023-01-20
        /// </summary>
        private void setMapColor_Machine(String sEqptid, String sTime, String sType, C1.WPF.DataGrid.DataGridRow row = null)
        {

            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END > '" + sTime + "' and EQPTID = '" + sEqptid.Substring(1) + "'");
            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");



            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            try
            {
                if (dtRow.Length > 0)
                {
                    dtRow[0]["CHK"] = "1";

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                    {
                        if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                        }
                        else
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRow[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                        }
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

                        if (row != null)
                        {
                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            drCurrDetail = row;
                            CommonCombo _combo = new CommonCombo();

                            // 기존 데이터 불러올 때 FCR 설정(Machine)
                            // 항상 존재하는 값
                            string lossCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            string lossDetlCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));

                            // Optional 값
                            string failCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            string causeCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            string resolCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            #endregion

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME")).Equals(""))
                            {
                                txtEqptName.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                            {
                                C1ComboBox[] cboOccurEqptParent = { cboEquipment };
                                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                                cboOccurEqpt.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                            }

                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            //{
                            //    cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            //{
                            //    popLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            //    popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                            //{
                            //    cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                            //{
                            //    cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                            //{
                            //    cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            //}

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            {
                                InitLossCombo();
                                cboLoss.SelectedValue = lossCode;
                            }

                            // 2023.05.28 윤지해 CSR ID E20230330-001442  LOSS LV3 리스트에 없을 경우 빈칸으로 입력
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            {
                                if (!string.IsNullOrEmpty(lossDetlCode))
                                {
                                    DataTable dt = DataTableConverter.Convert(popLossDetl.ItemsSource);
                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        for (int inx = 0; inx < dt.Rows.Count; inx++)
                                        {
                                            if (dt.Rows[inx]["CBO_CODE"].ToString() == Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")))
                                            {
                                                popLossDetl.SelectedValue = lossDetlCode;
                                                popLossDetl.SelectedText = dt.Rows[inx]["CBO_NAME"].ToString();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    popLossDetl.SelectedValue = null;
                                    popLossDetl.SelectedText = null;
                                }
                            }
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            //{
                            //    popLossDetl.SelectedValue = lossDetlCode;
                            //    popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            //}

                            // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS, FCR 매핑
                            string causeEqptid = cboOccurEqpt.SelectedValue.ToString().Equals("SELECT") || cboOccurEqpt.SelectedValue.IsNullOrEmpty() ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                            // 현상
                            String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");

                            // 원인
                            String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");

                            // 조치
                            String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                            #endregion

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                            {
                                new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                                //txtLossNote.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                            }
                        }

                    }

                    Border borderS = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("S" + sEqptid.Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("S" + sEqptid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        //  dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;  (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString();
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("S" + sEqptid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    Hashtable hashStart = borderS.Tag as Hashtable;
                    Hashtable hashEnd = borderE.Tag as Hashtable;

                    DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                    //색칠해야할 셀갯수 =  row 차이 / (설비갯수 + 시간디스플레이) * 컬럼수 + 종료컬럼 - 시작컬럼
                    int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));


                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = new SolidColorBrush(Colors.Blue);
                    }

                    //마지막 칸 정리
                    Border borderEndMinusOne = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                    Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                    if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                    {
                        borderE.Background = new SolidColorBrush(Colors.Blue);
                    }

                    int iRow = Grid.GetRow(borderS);



                    txtEqptName.Text = (GetEqptName_Machine(sEqptid.Substring(1)).Rows[0]["EQPTNAME"]).ToString();
                    if (row == null)
                    {
                        cboOccurEqpt.SelectedValue = sEqptid.Substring(1);
                    }

                    if (sType.Equals("LIST"))
                    {
                        svMap.ScrollToVerticalOffset((15 * iRow - 8));
                    }
                }
            }
            catch (Exception e)
            {

            }
        }


        /// <summary>
        /// return true : 정상작업, false : 작업 불가능
        /// " * 폴란드 PACK에 대해서만" 전일의 설비 LOSS 저장할수 있는 신규 권한을 추가하여 관리함.
        /// 신규 쓰기 권한을 가지고 있는 인원은 전일 / 당일 모두 저장기능 사용 가능
        /// 기존 쓰기 권한을 가지고 있는 인원은 당일의 데이터만 저장기능 가능
        /// </summary>
        private Boolean CeckPackProcessing()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

                string sNowDay = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

                // 작업자가 당일로 조회 날짜를 변경하고 저장할수 있으므로 조회당시(Search 버튼 이벤트)의 날짜를 가져와 비교함.
                /*
                if(sNowDay.Equals(sSearchDay) || String.IsNullOrEmpty(sSearchDay))
                {
                    return true;
                }
                */

                // 전기일 -2 일 까지 수정 가능 | 홍기룡
                DateTime dtNowDay = DateTime.ParseExact(sNowDay, "yyyyMMdd", null);
                DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);

                if (((dtNowDay - dtSearchDay).TotalDays < 3) || String.IsNullOrEmpty(sSearchDay))
                {
                    return true;
                }

                if (!bMPPD)
                {
                    COM001_014_AUTH_PERSON wndPerson = new COM001_014_AUTH_PERSON();
                    wndPerson.FrameOperation = this.FrameOperation;

                    if (wndPerson != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = "PACK_LOSS_ENGR_CWA";
                        C1WindowExtension.SetParameters(wndPerson, Parameters);

                        wndPerson.Closed += new EventHandler(wndUser_Closed);
                        wndPerson.ShowModal();
                        wndPerson.CenterOnScreen();
                        wndPerson.BringToFront();
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #region 2023.03.28 윤지해 CSR ID E20230321-001518  GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
        private void SetCboFailure_Index()
        {
            cboFailure.SelectedIndex = 1;
            cboFailure_SelectedItemChanged(null, null);
        }
        #endregion

       
        #endregion

        #region [Biz]

        #region [### 설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                //Pack일 경우 PROCID Parameter 제외
                if (!bPack)
                    RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegment;

                if (!bPack)
                    dr["PROCID"] = Util.GetCondition(cboProcess);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                if (bPack)
                {
                    liProcId = new List<string>();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        liProcId.Add(dtResult.Rows[i]["PROCID"].ToString());
                    }

                    DataRow drIns = dtResult.NewRow();
                    drIns["CBO_NAME"] = "-SELECT-";
                    drIns["CBO_CODE"] = "";
                    dtResult.Rows.InsertAt(drIns, 0);
                }

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                //Pack일 경우 설비 초기 세팅 -SELECT-, 설비 변경 시 공정 변경 이벤트 활용을 위해서
                if (!LoginInfo.CFG_EQPT_ID.Equals("") && !bPack)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Shift 정보 가져오기]
        private void SetShift()
        {
            try
            {
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = sEquipmentSegment;
                dr["FROMDATE"] = Util.GetCondition(ldpDatePicker);
                dr["TODATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cboShift.DisplayMemberPath = "SHFT_NAME";
                cboShift.SelectedValuePath = "SHFT_ID";

                DataRow drIns = dtResult.NewRow();
                drIns["SHFT_NAME"] = "-ALL-";
                drIns["SHFT_ID"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboShift.ItemsSource = dtResult.Copy().AsDataView();

                if (cboShift.SelectedIndex < 0)
                    cboShift.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region  Machine 설비 LOSS 가능 공정 확인 :  MachineEqpt_Loss_Modify_Chk()  by 오화백 2023.03.16
        /// <summary>
        /// Machine 설비 LOSS 가능 공정 확인
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        public void MachineEqpt_Loss_Modify_Chk(string Procid)
        {

            if (string.IsNullOrEmpty(Procid)) return;

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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = Procid;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {

                    MachineEqptChk = "Y";
                }
                else
                {
                    MachineEqptChk = string.Empty;
                }
                // Main 체크 해제 및 Machine Loss 수정 가능여부 확인 
                if (chkMain.IsChecked == false && MachineEqptChk == "Y")
                {
                    Machine.Visibility = Visibility.Visible;
                }
                else
                {
                    Machine.Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion



        #endregion


        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);

            //public static readonly System.Drawing.Color L11000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L12000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L13000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L14000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L15000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L16000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L21000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L22000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L23000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L31000 = System.Drawing.Color.FromArgb(0, 176, 240);
            //public static readonly System.Drawing.Color L32000 = System.Drawing.Color.FromArgb(255, 0, 0);
            //public static readonly System.Drawing.Color L33000 = System.Drawing.Color.FromArgb(255, 0, 0);
            //public static readonly System.Drawing.Color L34000 = System.Drawing.Color.FromArgb(228, 109, 10);
            //public static readonly System.Drawing.Color L35000 = System.Drawing.Color.FromArgb(0, 112, 192);
            //public static readonly System.Drawing.Color L36000 = System.Drawing.Color.FromArgb(83, 142, 213);
            //public static readonly System.Drawing.Color L37000 = System.Drawing.Color.FromArgb(112, 48, 160);
            //public static readonly System.Drawing.Color L38000 = System.Drawing.Color.FromArgb(112, 48, 160);
            //public static readonly System.Drawing.Color L39000 = System.Drawing.Color.FromArgb(148, 39, 84);
            //public static readonly System.Drawing.Color L3A000 = System.Drawing.Color.FromArgb(165, 165, 165);
            //public static readonly System.Drawing.Color L3B000 = System.Drawing.Color.FromArgb(255, 255, 0);
            //public static readonly System.Drawing.Color L41000 = System.Drawing.Color.FromArgb(0, 0, 255);
        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType == null || sType.Equals(""))
                        {
                            color = System.Drawing.Color.White;
                        }
                        else
                        {
                            //color = System.Drawing.Color.White;
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                color = System.Drawing.Color.White;
            }
            return color;
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        private void cboLastLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLastLoss.SelectedValue.Equals("SELECT"))
            {
                string[] sLastLoss = cboLastLoss.SelectedValue.ToString().Split('-');
                string[] sLastText = cboLastLoss.Text.ToString().Split('-');

                cboLoss.SelectedValue = sLastLoss[0];

                // 2023.05.28 윤지해 CSR ID E20230330-001442  LOSS LV3 리스트에 없을 경우 빈칸으로 입력
                if (!sLastLoss[1].Equals(""))
                {
                    DataTable dt = DataTableConverter.Convert(popLossDetl.ItemsSource);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int inx = 0; inx < dt.Rows.Count; inx++)
                        {
                            if (dt.Rows[inx]["CBO_CODE"].ToString() == sLastLoss[1])
                            {
                                popLossDetl.SelectedValue = sLastLoss[1];
                                popLossDetl.SelectedText = string.IsNullOrEmpty(sLastText[1]) ? sLastText[0] : sLastText[1];
                            }
                        }

                        popLossDetl_ValueChanged(null, null);
                    }
                    //popLossDetl.SelectedValue = sLastLoss[1];
                    //popLossDetl.SelectedText = sLastText[0];
                }
                else
                {
                    popLossDetl.SelectedValue = null;
                    popLossDetl.SelectedText = null;
                }
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            //Machine 설비 사용 체크 by 오화백 2023 03 16 
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    Indata["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                else
                {
                    Indata["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                }
            }
            else
            {
                Indata["EQPTID"] = Util.GetCondition(cboEquipment);
            }
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
            Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);

            IndataTable.Rows.Add(Indata);

            string sShiftID = string.Empty;
            string sShiftNM = string.Empty;
            string sWorkerID = string.Empty;
            string sWorkerNM = string.Empty;

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt.Rows.Count > 0)
                {
                    sShiftID = Util.NVC(dtRslt.Rows[0]["SHFT_ID"]);
                    sShiftNM = Util.NVC(dtRslt.Rows[0]["SHFT_NAME"]);
                    sWorkerID = Util.NVC(dtRslt.Rows[0]["WRK_USERID"]);
                    sWorkerNM = Util.NVC(dtRslt.Rows[0]["WRK_USERNAME"]);
                }
            }
            catch
            {
            }

            if (string.Equals(GetAreaType(), "E"))
            {
                COM001_014_EQPCOMMNET wndEqpComment = new COM001.COM001_014_EQPCOMMNET();
                wndEqpComment.FrameOperation = FrameOperation;

                //if (wndEqpComment != null)
                //{
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                //Machine 설비 사용 체크 by 오화백 2023 03 16 
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        Parameters[1] = cboEquipment.SelectedValue.ToString();
                        Parameters[5] = cboEquipment.Text;
                    }
                    else
                    {
                        Parameters[1] = cboEquipment_Machine.SelectedValue.ToString();
                        Parameters[5] = cboEquipment_Machine.Text;
                    }
                }
                else
                {
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[5] = cboEquipment.Text;
                }
                Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                Parameters[3] = "";
                Parameters[4] = "";


                //Parameters[5] = cboEquipment.Text;
                Parameters[6] = sShiftNM; // 작업조명
                Parameters[7] = sShiftID; // 작업조코드
                Parameters[8] = sWorkerNM; // 작업자명
                Parameters[9] = sWorkerID; // 작업자 ID
                                           //C1WindowExtension.SetParameters(wndEqpComment, Parameters);
                this.FrameOperation.OpenMenu("SFU010130090", true, Parameters);

                //wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                //_grid.Children.Add(wndEqpComment);
                //wndEqpComment.BringToFront();
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT();
                wndEqpComment.FrameOperation = FrameOperation;

                if (wndEqpComment != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                    Parameters[3] = "";
                    Parameters[4] = "";
                    Parameters[5] = cboEquipment.Text;
                    Parameters[6] = sShiftNM; // 작업조명
                    Parameters[7] = sShiftID; // 작업조코드
                    Parameters[8] = sWorkerNM; // 작업자명
                    Parameters[9] = sWorkerID; // 작업자 ID
                    C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                    wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                    //_grid.Children.Add(wndEqpComment);
                    wndEqpComment.BringToFront();
                }
            }

        }
        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            if (string.Equals(GetAreaType(), "E"))
            {
                //COM001_014_EQPCOMMNET window = sender as COM001_014_EQPCOMMNET;
                //if (window.DialogResult == MessageBoxResult.OK)
                //{
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                }
            }

        }


        private void btnRegiFcr_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }
            COM001_014_FCR wndFCR = new COM001_014_FCR();
            wndFCR.FrameOperation = FrameOperation;

            if (wndFCR != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqsg);

                C1WindowExtension.SetParameters(wndFCR, Parameters);

                wndFCR.Closed += new EventHandler(wndFCR_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCR.ShowModal()));
                wndFCR.BringToFront();

            }

        }
        private void wndFCR_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR window = sender as COM001_014_FCR;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnSearchLossCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }

            COM001_014_FCR_LIST wndFCRList = new COM001_014_FCR_LIST();
            wndFCRList.FrameOperation = FrameOperation;

            if (wndFCRList != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqsg);

                C1WindowExtension.SetParameters(wndFCRList, Parameters);

                wndFCRList.Closed += new EventHandler(wndFCRList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCRList.ShowModal()));
                wndFCRList.BringToFront();

            }

        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            COM001_014_APPR_ASSIGN window = sender as COM001_014_APPR_ASSIGN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void Popup_Closed2(object sender, EventArgs e)
        {
            COM001_014_TOTAL_APPR_ASSIGN window = sender as COM001_014_TOTAL_APPR_ASSIGN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void wndFCRList_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR_LIST window = sender as COM001_014_FCR_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                #region 2023.05.23 윤지해 CSR ID E20230330-001442 FCR 그룹 선택 시 FCR 콤보박스 리스트 변경
                CommonCombo _combo = new CommonCombo();

                // 항상 존재하는 값
                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

                // Optional 값
                string failCode = window.F_CODE;
                string causeCode = window.C_CODE;
                string resolCode = window.R_CODE;

                // 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS, FCR 매핑
                string causeEqptId = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                // 현상
                cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptId };
                _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

                // 원인
                cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptId };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                // 조치
                cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptId };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                #endregion

                txtFCRCode.Text = window.FCR_GR_CODE;
            }
        }
        private bool ValidateFCR()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.Equals("") || cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.Equals("") || cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3207"); //공정을 선택해주세요
                return false;
            }
            return true;
        }

        private void txtLossCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchFCRCode();
            }
        }

        private void SearchFCRCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FCR_GR_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["FCR_GR_CODE"] = txtFCRCode.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_FCR_GR", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3208"); //해당 FCR그룹코드가 없습니다. 등록해주세요

                    txtFCRCode.Text = string.Empty;
                    cboFailure.SelectedIndex = 0;
                    cboCause.SelectedIndex = 0;
                    cboResolution.SelectedIndex = 0;

                    return;
                }

                cboFailure.SelectedValue = result.Rows[0]["F_CODE"];
                cboCause.SelectedValue = result.Rows[0]["C_CODE"];
                cboResolution.SelectedValue = result.Rows[0]["R_CODE"];
            }
            catch (Exception ex)
            {

            }
        }
        private void indataset()
        {
            _procid = Util.GetCondition(cboProcess);
            _wrk_date = Util.GetCondition(ldpDatePicker);
            _areaid = Util.GetCondition(cboArea);
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    _eqptid = Util.GetCondition(cboEquipment);
                }
                else
                {
                    _eqptid = Util.GetCondition(cboEquipment_Machine);
                }
            }
            else
            {
                _eqptid = Util.GetCondition(cboEquipment);
            }
        }

        //운영설비도 Split되는지 확인
        private void SelectLossRunArea()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "LOSS_RUN_AREA";
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_LOSSRUNAREA", "INDATA", "RSLT", dt);

                RunSplit = result.Rows.Count == 0 ? "N" : "Y";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SelectRemarkMandatory()
        {
            try
            {

                dtRemarkMandatory = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "EQPT_LOSS_REMARK_MANDATORY";
                row["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                row["PROCID"] = Util.GetCondition(cboProcess);

                dt.Rows.Add(row);

                dtRemarkMandatory = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_REMARK_MANDATORY", "INDATA", "RSLT", dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK/소형 제외 원인설비별 LOSS 등록 여부에 따라 BM/PD 조회내역에서 제외 또는 포햠하도록 수정
        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }
            // 2023.07.18 윤지해 CSR ID E20230703-000158 - COM001_014_LOSS_DETL_FCR로 변경
            //COM001_014_LOSS_DETL wndLossDetl = new COM001_014_LOSS_DETL();
            COM001_014_LOSS_DETL_FCR wndLossDetl = new COM001_014_LOSS_DETL_FCR();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                #region 
                /* 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                 * 2022.12.20 C20221216-000583 설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회
                 */
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                //Parameters[2] = Convert.ToString(_grid_eqpt);
                //Machine 설비 사용 체크 by 오화백 2023.03.16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        Parameters[2] = Convert.ToString(_grid_eqpt);
                    }
                    else
                    {
                        Parameters[2] = Util.GetCondition(cboEquipment_Machine);
                    }
                }
                else
                {
                    Parameters[2] = Convert.ToString(_grid_eqpt);
                };
                Parameters[3] = (cboLoss.SelectedValue.IsNullOrEmpty() || cboLoss.SelectedValue.ToString().Equals("SELECT")) ? "" : cboLoss.SelectedValue.ToString();
                Parameters[4] = occurEqptFlag;
                Parameters[5] = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                #endregion
                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            // 2023.07.18 윤지해 CSR ID E20230703-000158 - COM001_014_LOSS_DETL_FCR로 변경
            //COM001_014_LOSS_DETL window = sender as COM001_014_LOSS_DETL;
            COM001_014_LOSS_DETL_FCR window = sender as COM001_014_LOSS_DETL_FCR;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                popLossDetl.SelectedValue = window._LOSS_DETL_CODE;
                popLossDetl.SelectedText = window._LOSS_DETL_NAME;

                // 2023.03.07 윤지해 CSR ID E20230220-000068	FCR 초기화
                popLossDetl_ValueChanged(null, null);

                cboFailure.SelectedValue = window._FAIL_CODE;
                cboCause.SelectedValue = window._CAUSE_CODE;
                cboResolution.SelectedValue = window._RESOL_CODE;
            }
        }

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(0);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);

            grUp.RowDefinitions[1].Height = new GridLength(0);


        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(300);
            grUp.RowDefinitions[1].Height = new GridLength(8);
            grUp.RowDefinitions[2].Height = GridLength.Auto;
            grUp.RowDefinitions[3].Height = new GridLength(8);
            grUp.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        }

        private void ValidateNonRegisterLoss(string saveType)
        {
            try
            {
                int idx = -1;
                if (saveType.Equals("TOTAL"))
                {
                    //제일 마지막에 클릭한 index찾기
                    for (int i = dgDetail.GetRowCount() - 1; i > 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            idx = i;
                            break;
                        }
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));

                DataRow row = dt.NewRow();
                //Machine 설비 사용 체크 by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                    }
                    else
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment_Machine.SelectedValue);
                    }
                }
                else
                {
                    row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                }
                //row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) :  ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + txtStart.Text;
                //우크라이나어 세팅시 날짜 포맷형식 에러로 인한 수정 2019.07.19.
                row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) : ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + txtStart.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSETAIL_VALID", "RQSTDT", "RSLT", dt);

                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU3515", Convert.ToString(result.Rows[0]["STRT_DTTM"]));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private string VadliationERPEnd()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("WRKDATE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboArea.SelectedValue.ToString();
            dr["WRKDATE"] = Util.GetCondition(ldpDatePicker);
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_LOSS_CLOSE", "RQSTDT", "RSLT", dt);

            if (result.Rows.Count != 0)
            {
                return Convert.ToString(result.Rows[0]["ERP_CLOSING_FLAG"]);
            }

            return "OPEN";
        }


        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }

        private void chkLossSort_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }

        }

        private void chkLossSort_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }
        }

        private void btnChgHistory_Click(object sender, RoutedEventArgs e)
        {
            //if (chkTestDev.IsChecked == true)
            //{
            //    return;
            //}

            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                COM001_014_CHG_HIST wndHist = new COM001_014_CHG_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "EQPTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "PRE_LOSS_SEQNO"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);
                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            COM001_014_CHG_HIST window = sender as COM001_014_CHG_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        /// <summary>
        /// 2019.09.30 황기근 사원 수정
        /// CSR ID: C20190910_82826
        /// PLANT 별로 LOSS 수정 제약을 줌.
        /// 이전 날짜 조회 시 하루 변경 가능 시간 기준 정보 조회 후 가능 시간이 아니면 저장 및 일괄저장 버튼 비활성화.
        /// 단, 설비LOSS 관리 권한 보유자는 수정 가능
        /// </summary>
        private void restrictSave()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "LOSS_MODIFY_RESTRICT_SHOP";
            RQSTDT1.Rows.Add(row);

            DataTable shopList = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT1);
            DataRow[] shop = shopList.Select("CBO_Code = '" + LoginInfo.CFG_SHOP_ID + "'");

            if (shop.Count() > 0)   // LOSS 수정 제한하는 PLANT일 때
            {
                DateTime pickedDate = ldpDatePicker.SelectedDateTime;
                if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.ToString("yyyy-MM-dd")))
                    return;

                DateTime dtCaldate = Convert.ToDateTime(AreaTime.Rows[0]["JOBDATE_YYYYMMDD"]);
                string sCaldate = dtCaldate.ToString("yyyy-MM-dd");
                btnSave.IsEnabled = true;
                btnTotalSave.IsEnabled = true;

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.Columns.Add("USERID", typeof(string));
                RQSTDT2.Columns.Add("AUTHID", typeof(string));

                DataRow row2 = RQSTDT2.NewRow();
                row2["USERID"] = LoginInfo.USERID;
                row2["AUTHID"] = "EQPTLOSS_MGMT";
                RQSTDT2.Rows.Add(row2);

                DataTable auth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT2);

                if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")))
                {
                    TimeSpan due = DateTime.Parse(Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(0, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(2, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(4, 2)).TimeOfDay;
                    TimeSpan searchTime = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;
                    if (searchTime >= due && auth.Rows.Count <= 0)
                    {
                        btnSave.IsEnabled = false;
                        btnTotalSave.IsEnabled = false;
                    }
                }
                else
                {
                    if (auth.Rows.Count <= 0)
                    {
                        btnSave.IsEnabled = false;
                        btnTotalSave.IsEnabled = false;
                    }
                }
            }


        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void txtPerson_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                txtPerson.Tag = null;
            }
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);
            if (dBaseDate != null
                && ldpDatePicker.SelectedDateTime < dBaseDate)
            {
                ldpDatePicker.SelectedDateTime = dBaseDate;
            }

            GetEqptLossApprAuth();

            if (bPack) return;
            //cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;
            //cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;

            SetEquipmentSegmentCombo(cboEquipmentSegment);

            //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            //cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            //cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            //cboShift.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            SetProcessCombo(cboProcess);
            SetTroubleUnitColumnDisplay();
            //cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            //cboShift.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            SetShiftCombo(cboShift);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            //cboShift.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            SetEquipmentCombo(cboEquipment);
            SetTroubleUnitColumnDisplay();
            //Machine 설비 Loss 수정 가능 여부 by 오화백  2023 03 16
            MachineEqpt_Loss_Modify_Chk(cboProcess.SelectedValue.GetString());
            //cboShift.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            //Machine Multi 2025 07 10 오화백
            SetMachineMulti(cboProcess.SelectedValue.GetString());
        }
        //Machine Multi 2025 07 10 오화백
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            SetShiftCombo(cboShift);
            SetTroubleUnitColumnDisplay();
            //SetShiftCombo(cboShift);    // 2023.05.28 중복 - 주석처리?
            //Machine 설비 콤보 조회  by 오화백 2023 03 16
            SetMachineEqptCombo(cboEquipment_Machine);
            //-Machine Multi 2025 07 10 오화백
            SetMachineMultiEqptCombo();
        }


        #region [ PopUp Event ]

        #region < 해체 담당자 찾기 >

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtPerson.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            //grdMain.Children.Add(wndPerson); _grid
            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPerson.Text = wndPerson.USERNAME;
                txtPerson.Tag = wndPerson.USERID;
            }
        }


        #endregion

        #endregion

        private void SetTroubleUnitColumnDisplay()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["EQPT_LOSS_UNIT_ALARM_DISP_FLAG"].GetString() == "Y")
                    {
                        //2022.11.23
                        //dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        //dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                    }

                    if (ChkLnsLami())
                    {
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;
                }

                #region 2023.05.28 윤지해 CSR ID E20230330-001442 원인설비별 LOSS 등록 여부 확인
                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("CAUSE_EQPT_LOSS_MNGT_FLAG"))
                {
                    if (Util.NVC(dtResult.Rows[0]["CAUSE_EQPT_LOSS_MNGT_FLAG"]).Equals("Y"))
                    {
                        isCauseEquipment = true;
                    }
                    else
                    {
                        isCauseEquipment = false;
                    }
                }
                else
                {
                    isCauseEquipment = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private bool ChkLnsLami()
        {
            const string bizRuleName = "DA_EQP_SEL_LNS_IN_LAMI";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            { return true; }
            else { return false; }

        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                //bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_LOSS_CBO";
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
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

        private void SetProcessCombo(C1ComboBox cbo)
        {
            //string equipmentSegmentCode = cboEquipmentSegment.SelectedValue.GetString();

            //const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";

            //string[] arrColumn = { "LANGID", "EQSGID" };
            //string[] arrCondition = { LoginInfo.LANGID, equipmentSegmentCode };
            //string selectedValueText = "CBO_CODE";
            //string displayMemberText = "CBO_NAME";

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F") // 활성화이면
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                //string bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
                //2024.05.02 지광현 선택할 설비가 존재하는 공정만 조회되도록 수정
                string bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM_EXIST_EQPT";
                //C1ComboBox[] cbProcessParent = { cboEquipmentSegment, cboArea };
                //combo.SetCombo(cbo, CommonCombo.ComboStatus.NONE/*, cbChild: cbProcessChild*/, cbParent: cbProcessParent, sCase: sCase2);

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
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
            else
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
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
            //Machine 설비 Loss 수정 가능 여부  by 오화백 2023 03.16
            MachineEqpt_Loss_Modify_Chk(cboProcess.SelectedValue.GetString());
            //Machine Multi by 2025 07 10 오화백
            SetMachineMulti(cboProcess.SelectedValue.GetString());
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
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

        private void SetShiftCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;

        }

        /// <summary>
        /// Machine 설비 조회  - by 오화백 2023.02.20
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMachineEqptCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
            dr["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) ? null : cboEquipment.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;

        }

        private bool event_valridtion()
        {
            //Machine 설비 사용 체크 by 오화백 2023 02 20
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
                    {
                        // 질문1 조회된 데이터가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Util.GetCondition(cboEquipment_Machine)) || Util.GetCondition(cboEquipment_Machine).Equals(""))
                    {
                        // 질문1 조회된 데이터가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return false;
                    }

                }
            }
            else
            {
                if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
                {
                    // 질문1 조회된 데이터가 없습니다.
                    Util.MessageValidation("SFU1905");
                    return false;
                }
            }

            return true;
        }

        private void btnEMSWOReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!event_valridtion())
                {
                    return;
                }

                // 전송 하시겠습니까?
                Util.MessageConfirm("SFU3609", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inData = new DataTable("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        //row["EQPTID"] = _grid_eqpt;

                        //Machine 설비 사용 체크 by 오화백 2023 03 16
                        if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                        {
                            if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                            {
                                row["EQPTID"] = _grid_eqpt;
                            }
                            else
                            {
                                row["EQPTID"] = Util.GetCondition(cboEquipment_Machine);

                            }
                        }
                        else
                        {
                            row["EQPTID"] = _grid_eqpt;
                        }
                        row["USERID"] = LoginInfo.USERID;

                        inData.Rows.Add(row);

                        new ClientProxy().ExecuteService("BR_EQPT_EQPTLOSS_BM_WO_TO_EMS", "INDATA", null, inData, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1880"); //전송 완료 되었습니다.
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll1_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void SaveQA(DataSet ds)
        {
            // Line Check
            if (!this.GetPackApplyLIneByUI(_grid_eqsg))
            {
                return;
            }

            foreach (DataTable dt in ds.Tables)
            {
                DataTable dtCopy = dt.Copy();
                this.SaveQA(dtCopy, true);
            }
        }

        private void SaveQA(DataTable dtINDATA, bool isLineCheck = false)
        {
            // Valiation Check...
            // 일괄저장시에는 Line Check를 이미 한 상태이므로 그냥 진행.
            // 단일저장시에는 Line Check를 한 상태가 아니므로 Check후 진행
            if (!isLineCheck)
            {
                if (!this.GetPackApplyLIneByUI(_grid_eqsg))
                {
                    return;
                }
            }

            if (!CommonVerify.HasTableRow(dtINDATA))
            {
                return;
            }

            if (!CommonVerify.HasTableRow(this.dtQA))
            {
                return;
            }

            try
            {
                DataSet dsINDATA = new DataSet();
                string bizRuleName = "BR_EQPT_EQPTLOSS_REG_QN_ANS";
                // DTINDATA
                dtINDATA.TableName = "INDATA";
                //dtINDATA.Columns.Add("AREAID");

                //foreach (DataRow drINDATA in dtINDATA.Rows)
                //{
                //    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                //}
                dtINDATA.AcceptChanges();

                // DTINQA
                DataTable dtINQA = new DataTable("INQA");
                dtINQA.Columns.Add("QUESTION", typeof(string));
                dtINQA.Columns.Add("ANSWER", typeof(string));

                foreach (DataRow drQA in this.dtQA.Select())
                {
                    DataRow drINQA = dtINQA.NewRow();
                    drINQA["QUESTION"] = drQA["CBO_CODE"];
                    drINQA["ANSWER"] = drQA["ANSWER"];
                    dtINQA.Rows.Add(drINQA);
                }

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINQA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetPackApplyLIneByUI(string equipmentSegmentID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
                drRQSTDT["CBO_CODE"] = "COM001_014_Tab_Main";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        if (drRSLTDT["ATTRIBUTE1"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE2"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE3"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE4"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE5"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }

        private void cboLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLoss.Text.Equals("-SELECT-"))
            {
                // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK/소형 아니고 원인설비별 LOSS 등록 여부 체크되어 있고, 원인설비를 선택했을 경우 3레벨에 MMD에 등록된 내역으로 LIST-UP
                // 기타의 경우 기존과 동일하게 LIST-UP
                string bizRuleName = string.Empty;
                string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQPTID", "LOSS_CODE", "CAUSE_EQPTID" };
                string IN_AREA = _grid_area.IsNullOrEmpty() ? cboArea.SelectedValue.ToString() : _grid_area;
                string IN_PROC = _grid_proc.IsNullOrEmpty() ? cboProcess.SelectedValue.ToString() : _grid_proc;
                string IN_EQPT = _grid_eqpt.IsNullOrEmpty() ? cboEquipment.SelectedValue.ToString() : _grid_eqpt;
                string IN_LOSSCODE = cboLoss.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLoss.SelectedValue.ToString();
                string IN_CAUSEEQPT = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                string[] arrCondition = { LoginInfo.LANGID, IN_AREA, IN_PROC, IN_EQPT, IN_LOSSCODE, IN_CAUSEEQPT };

                if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.ToString().Equals("SELECT") || cboOccurEqpt.SelectedValue.ToString().Equals("")))
                {
                    bizRuleName = "DA_BAS_SEL_EQPTLOSSDETLCODE_OCCUREQP_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQPTLOSSDETLCODE_CBO";
                }
                CommonCombo.SetFindPopupCombo(bizRuleName, popLossDetl, arrColumn, arrCondition, (string)popLossDetl.SelectedValuePath, (string)popLossDetl.DisplayMemberPath);
                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
            }
            else if (cboLoss.Text.Equals("-SELECT-"))
            {
                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
                popLossDetl.ItemsSource = null; // 2023.05.28 윤지해 CSR ID E20230330-001442 초기화 추가
            }
            // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
            popLossDetl_ValueChanged(null, null);
        }

        private bool ValidateEqptLossAppr(string validateType)
        {
            // 설비 LOSS 수정 화면 추가에 따른 Validation 추가, 설비 Loss 수정 화면을 사용할 경우 확인
            if (bUseEqptLossAppr)
            {
                DataTable RQSTDT;
                DataRow dr;
                DataTable result;

                //sSearchDay와 현재시간이 D+1 일 경우 Validation(비조업) 2023.07.19 김대현
                if (ValidationChkDDay().Equals("NO_REG"))
                {
                    string strParam = (Convert.ToDouble(strAttr2) + 1).ToString();
                    Util.MessageValidation("SFU5180", ObjectDic.Instance.GetObjectName(strParam));  // strAttr2 + 일이전 설비 Loss는 등록 불가합니다.
                    return false;
                }

                // 2025-03-21  BY 오화백  공용PC에서도 일괄저장 기능사용가능하도록 수정
                //if (ValidationChkDDay().Equals("AUTH_ONLY") && LoginInfo.USERTYPE.Equals("P"))
                //{
                //    Util.MessageValidation("SFU5179"); // 공용 PC 사용자권한으로는 Loss 등록이 불가합니다. \n개인권한 사용자로 로그인하여 등록해주시기 바랍니다. 
                //    return false;
                //}

                // 선택한 Loss 건 중 승인 대기중인 건이 있으면 return
                // 저장(SAVE), 일괄저장(TOTAL), RowMouseDoubleClick(CLICK), Split(SPLIT)
                dr = null;
                result = null;

                RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOSS_SEQNO", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("END_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("APPR_STAT", typeof(string));

                switch (validateType)
                {
                    case "SAVE":
                        dr = RQSTDT.NewRow();
                        if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                        {
                            if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                            {
                                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                            }
                            else
                            {
                                dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                            }
                        }
                        else
                        {
                            dr["EQPTID"] = Util.GetCondition(cboEquipment);
                        }

                        if (string.IsNullOrEmpty(txtStartHidn.Text) && string.IsNullOrEmpty(txtEndHidn.Text))
                        {
                            Util.MessageValidation("SFU3538");  //선택된 데이터가 없습니다
                            return false;
                        }
                        dr["WRK_DATE"] = sSearchDay;
                        dr["STRT_DTTM"] = DateTime.ParseExact(txtStartHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["END_DTTM"] = DateTime.ParseExact(txtEndHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                    case "TOTAL":
                        for (int i = 0; i < dgDetail.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dr = RQSTDT.NewRow();
                                dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EQPTID"));
                                dr["WRK_DATE"] = sSearchDay;
                                //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "STRT_DTTM"));
                                //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "END_DTTM"));
                                dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                                dr["APPR_STAT"] = "W";
                                RQSTDT.Rows.Add(dr);
                            }
                        }
                        break;
                    case "SPLIT":
                        dr = RQSTDT.NewRow();
                        if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                        {
                            if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                            {
                                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                            }
                            else
                            {
                                dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                            }
                        }
                        else
                        {
                            dr["EQPTID"] = Util.GetCondition(cboEquipment);
                        }
                        dr["WRK_DATE"] = sSearchDay;
                        dr["STRT_DTTM"] = DateTime.ParseExact(txtStartHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["END_DTTM"] = DateTime.ParseExact(txtEndHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                    case "CLICK":
                        dr = RQSTDT.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "EQPTID"));
                        dr["WRK_DATE"] = sSearchDay;
                        //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "STRT_DTTM"));
                        //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "END_DTTM"));
                        dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "PRE_LOSS_SEQNO"));
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                }

                result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_CHK_APPR", "RQSTDT", "RSLT", RQSTDT);
                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU5176"); // 설비 LOSS 수정을 통한 승인 대기 중인 설비 LOSS 건이 있습니다.
                    return false;
                }
            }
            return true;
        }
        private string GetPreLossSeqnoForSave()
        {
            string pre_loss_seqno = string.Empty;
            string eiostat = string.Empty;

            if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
            {
                if (chkT.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'T'";
                    }
                    else
                    {
                        eiostat += ",'T'";
                    }
                }

                if (chkW.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'W'";
                    }
                    else
                    {
                        eiostat += ",'W'";
                    }
                }

                if (chkU.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'U'";
                    }
                    else
                    {
                        eiostat += ",'U'";
                    }
                }
            }

            object[] prelossseqno = string.IsNullOrEmpty(eiostat) ? dtBeforeList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_START < '" + txtEndHidn.Text + "'", "").Select(x => x["PRE_LOSS_SEQNO"]).ToArray() :
                dtBeforeList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_START < '" + txtEndHidn.Text + "'" + "AND EIOSTAT IN (" + eiostat + ")", "").Select(x => x["PRE_LOSS_SEQNO"]).ToArray();

            for (int i = 0; i < prelossseqno.Count(); i++)
            {
                if (string.IsNullOrEmpty(pre_loss_seqno))
                {
                    pre_loss_seqno += prelossseqno[i];
                }
                else
                {
                    pre_loss_seqno += "," + prelossseqno[i];
                }
            }

            return pre_loss_seqno;
        }

        //private string GetLossSeqnoForSave()
        //{
        //    string loss_seqno = string.Empty;

        //    object[] prelossseqno = dtBeforeList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_START < '" + txtEndHidn.Text + "'", "").Select(x => x["LOSS_SEQNO"]).ToArray();

        //    for (int i = 0; i < prelossseqno.Count(); i++)
        //    {
        //        if (i != prelossseqno.Count() - 1)
        //        {
        //            loss_seqno += prelossseqno[i] + ",";
        //        }
        //        else
        //        {
        //            loss_seqno += prelossseqno[i];
        //        }
        //    }

        //    return loss_seqno;
        //}

        private string GetPreLossSeqnoForSaveALL()
        {
            string pre_loss_seqno = string.Empty;

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (string.IsNullOrEmpty(pre_loss_seqno))
                    {
                        pre_loss_seqno += Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    }
                    else
                    {
                        pre_loss_seqno += "," + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    }
                }
            }

            return pre_loss_seqno;
        }

        //private string GetLossSeqnoForSaveALL()
        //{
        //    string loss_seqno = string.Empty;

        //    for (int i = 0; i < dgDetail.GetRowCount(); i++)
        //    {
        //        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
        //        {
        //            if (i != dgDetail.GetRowCount() - 1)
        //            {
        //                loss_seqno += Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_SEQNO")) + ",";
        //            }
        //            else
        //            {
        //                loss_seqno += Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_SEQNO"));
        //            }
        //        }
        //    }

        //    return loss_seqno;
        //}

        private bool SaveDataChk(string prelossseqno)
        {
            //UPDATE하려는 LOSS DATA에 변경된 이력이 있는지 체크
            string _endnullseqno = string.Empty;
            string _prelossseqno = string.Empty;

            DataTable dtAfterList = new DataTable();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            dr["EQPTID"] = _eqptid;
            dr["WRK_DATE"] = _wrk_date;

            RQSTDT.Rows.Add(dr);

            _prelossseqno = prelossseqno;

            if (string.IsNullOrEmpty(_prelossseqno))
                return true;

            dtAfterList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN", "RQSTDT", "RSLTDT", RQSTDT);

            DataRow[] a = dtBeforeList.Select("PRE_LOSS_SEQNO IN (" + prelossseqno + ")" + "AND END_TIME IS NULL");
            if (a.Count() != 0)
            {
                _endnullseqno = a[0]["PRE_LOSS_SEQNO"].ToString();
            }

            object[] oldloss = !string.IsNullOrEmpty(_endnullseqno) ? dtBeforeList.Select("PRE_LOSS_SEQNO IN (" + prelossseqno + ")" + "AND PRE_LOSS_SEQNO <>" + _endnullseqno).Select(x => x["UPDDTTM"]).ToArray() :
                               dtBeforeList.Select("PRE_LOSS_SEQNO IN (" + prelossseqno + ")").Select(x => x["UPDDTTM"]).ToArray();
            object[] newloss = !string.IsNullOrEmpty(_endnullseqno) ? dtAfterList.Select("PRE_LOSS_SEQNO IN (" + prelossseqno + ")" + "AND PRE_LOSS_SEQNO <>" + _endnullseqno).Select(x => x["UPDDTTM"]).ToArray() :
                               dtAfterList.Select("PRE_LOSS_SEQNO IN (" + prelossseqno + ")").Select(x => x["UPDDTTM"]).ToArray();
            object[] oldendnullloss = !string.IsNullOrEmpty(_endnullseqno) ? dtBeforeList.Select("PRE_LOSS_SEQNO =" + _endnullseqno).Select(x => x["PRE_LOSS_SEQNO"]).ToArray() : null;
            object[] newendnullloss = !string.IsNullOrEmpty(_endnullseqno) ? dtAfterList.Select("PRE_LOSS_SEQNO =" + _endnullseqno).Select(x => x["PRE_LOSS_SEQNO"]).ToArray() : null;
            if (newloss.Except(oldloss).Count() > 0 || oldloss.Count() != newloss.Count())
            {
                return false;
            }
            else if (oldendnullloss != null)
            {
                if (oldendnullloss.Count() != newendnullloss.Count() || !string.IsNullOrEmpty(_endnullseqno) && newendnullloss.Length == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool SaveDupDataChk()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            //Machine 설비 사용 체크 by 오화백 2023 02 20
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                }
            }
            else
            {
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
            }
            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            dr["END_DTTM"] = Util.GetCondition(txtEndHidn);



            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSS_DUP_DATA_CHK", "RQSTDT", "RSLTDT", RQSTDT);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return false;
            }

            return true;
        }

        private void cboOccurEqpt_dropDownChanged(object sender, EventArgs e)
        {
            if (cboOccurEqpt.IsDropDownOpen)
            {
                setOccurEqptFlag();
                InitOccurEqptCombo();
            }
            else
                return;
        }

        private void setOccurEqptFlag()
        {
            occurEqptFlag = !bPack && isCauseEquipment ? "Y" : "N"; // 2024.01.03 이병윤 CSR ID E20231212-000731 bMobile여부 삭제
            return;
        }

        private void setOccurEqptLossCombo()
        {
            if (drCurrDetail == null)
                return;

            //Detail 더블클릭 이후 원인설비 선택 변경 시 Detail 정보의 lossCode,lossDetlCode, failCode 원인설비별 Loss Lv.3 FCR 매핑 기준정보 등록 확인하여 있으면 콤보박스 세팅
            CommonCombo _combo = new CommonCombo();

            string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
            string lossCode = Util.NVC(DataTableConverter.GetValue(drCurrDetail.DataItem, "LOSS_CODE"));
            string lossDetlCode = Util.NVC(DataTableConverter.GetValue(drCurrDetail.DataItem, "LOSS_DETL_CODE"));
            string failCode = Util.NVC(DataTableConverter.GetValue(drCurrDetail.DataItem, "SYMP_CODE"));
            string causeCode = Util.NVC(DataTableConverter.GetValue(drCurrDetail.DataItem, "CAUSE_CODE"));
            string resolCode = Util.NVC(DataTableConverter.GetValue(drCurrDetail.DataItem, "REPAIR_CODE"));

            if (!String.IsNullOrEmpty(lossCode))
            {
                try
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    RQSTDT.Columns.Add("FCR_TYPE_CODE", typeof(string));
                    RQSTDT.Columns.Add("FAIL_CODE", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_EQPTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROCID"] = _grid_proc;
                    dr["LOSS_CODE"] = lossCode;
                    dr["LOSS_DETL_CODE"] = lossDetlCode;
                    dr["FCR_TYPE_CODE"] = _fTypeCode;
                    dr["FAIL_CODE"] = failCode;
                    dr["EQPTID"] = _grid_eqpt;
                    dr["CAUSE_EQPTID"] = causeEqptid;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_FCRCODE_EQPT_SPCL_LOSS_LV3_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count != 0)
                    {
                        cboLoss.SelectedValue = lossCode;

                        if (!string.IsNullOrEmpty(lossDetlCode))
                        {
                            DataTable dt = DataTableConverter.Convert(popLossDetl.ItemsSource);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                for (int inx = 0; inx < dt.Rows.Count; inx++)
                                {
                                    if (dt.Rows[inx]["CBO_CODE"].ToString() == lossDetlCode)
                                    {
                                        popLossDetl.SelectedValue = lossDetlCode;
                                        popLossDetl.SelectedText = dt.Rows[inx]["CBO_NAME"].ToString();
                                    }
                                }
                            }
                        }

                        // 현상
                        String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");

                        // 원인
                        String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");

                        // 조치
                        String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            else
                return;
        }


        #region  Machine Multi 관련   : 2025 07 10 오화백

        /// <summary>
        ///  컨트롤 초기화 : Machine Multi 2025 07 10 오화백
        /// </summary>
        private void initControls()
        {
            //화면 초기화
            InitInsertCombo();

            ClearGrid();
            drCurrDetail = null;

            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            txtFCRCode.Text = "";
            txtPerson.Text = "";
            txtPerson.Tag = null;




            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            popLossDetl.SelectedValue = string.Empty;
            popLossDetl.SelectedText = string.Empty;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

            if (this.dtQA != null) this.dtQA.Clear();
            this.isOnlyRemarkSave = false;
            this.isTotalSave = false;

            Util.gridClear(dgDetail);

            txtRequire.Text = "";
            txtWriteEnd.Text = "";

            chkW.IsChecked = false;
            chkT.IsChecked = false;
            chkU.IsChecked = false;
        }
        
        private void SetMachineMulti(string procId)
        {
            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));
            inTable.Columns.Add("USE_FLAG", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_MULTI_PROCESS";
            dr["COM_CODE"] = procId;
            dr["USE_FLAG"] = "Y";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (chkMain.IsChecked == false)
                {
                    grMulti.Visibility = Visibility.Visible;
                    MachineMultiChk = true;
                }
                else
                {
                    chkMachMulti.IsChecked = false;
                    grMulti.Visibility = Visibility.Collapsed;
                    MachineMultiChk = false;
                }
            }
            else
            {
                chkMachMulti.IsChecked = false;
                grMulti.Visibility = Visibility.Collapsed;
                MachineMultiChk = false;
            }
        }

        private void chkMachMulti_Checked(object sender, RoutedEventArgs e)
        {
            Machine.Visibility = Visibility.Collapsed;
            MachineMulti.Visibility = Visibility.Visible;
            btnSave.IsEnabled = false;
            tbEqptName.Visibility = Visibility.Collapsed;
            tbOccrEqpt.Visibility = Visibility.Collapsed;
            txtEqptName.Visibility = Visibility.Collapsed;
            cboOccurEqpt.Visibility = Visibility.Collapsed;
        }

        private void chkMachMulti_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkMain.IsChecked == false && MachineEqptChk.Equals("Y"))
            {
                Machine.Visibility = Visibility.Visible;
            }
            else
            {
                Machine.Visibility = Visibility.Collapsed;
            }
            MachineMulti.Visibility = Visibility.Collapsed;
            btnSave.IsEnabled = true;
            tbEqptName.Visibility = Visibility.Visible;
            tbOccrEqpt.Visibility = Visibility.Visible;
            txtEqptName.Visibility = Visibility.Visible;
            cboOccurEqpt.Visibility = Visibility.Visible;
            initControls();
        }

       
        private void GetEqptLossDetailList_MachineMulti()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = SelectEquipment();
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["ASC"] = (bool)chkLossSort.IsChecked ? null : "Y";
                dr["REVERSE_CHECK"] = (bool)chkLossSort.IsChecked ? "Y" : null;
                dr["MIN_SECONDS"] = (bool)chkSearchAll.IsChecked ? 0 : 180;
                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_MNT_ALL", "RQSTDT", "RSLTDT", RQSTDT);

                if (cboShift.SelectedValue != null && !string.IsNullOrEmpty(cboShift.SelectedValue.GetString()))
                {
                    DateTime dJobDate_st = new DateTime();
                    DateTime dJobDate_ed = new DateTime();

                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift_st = drShift[0]["SHFT_STRT_HMS"].ToString();
                        String sShift_ed = drShift[0]["SHFT_END_HMS"].ToString();

                        dJobDate_st = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_st.Substring(0, 2) + ":" + sShift_st.Substring(2, 2) + ":" + sShift_st.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        dJobDate_ed = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                        //작업조의 end시간이 기준시간 보다 작을때
                        if (TimeSpan.Parse(sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2)) < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
                        {
                            dJobDate_ed = DateTime.ParseExact(ldpDatePicker.SelectedDateTime.AddDays(1).ToString("yyyyMMdd") + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        }
                    }

                    try
                    {
                        RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").CopyToDataTable();
                        if (chkLossSort.IsChecked == true)
                        {
                            RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").Reverse().CopyToDataTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        DataTable dt = new DataTable();
                        foreach (DataColumn col in RSLTDT.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.ColumnName));
                        }

                        RSLTDT = dt;
                    }
                }

                if (RSLTDT.Rows.Count == 0)
                {
                    Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);
                }
                else
                {
                    string asc = (bool)chkLossSort.IsChecked ? "desc" : "asc";
                    DataTable RSLTDT1 = RSLTDT.Select("", "EQPTID ASC, STRT_DTTM " + asc).CopyToDataTable();
                    Util.GridSetData(dgDetail, RSLTDT1, FrameOperation, true);
                }

                txtRequire.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtWriteEnd.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEqptLossListMulti(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["WRK_DATE"] = sJobDate;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSS_MAP_MNT_ALL", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return RSLTDT;
        }

        private void GetEqptLossRawList_MachineMulti()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();

                string[] eqptMachine = { };
                eqptMachine = cboEquipment_Machine_Multi.SelectedItemsToString.Split(delimiters);

                foreach (string eqpt in eqptMachine)
                {
                    dr = RQSTDT.NewRow();
                    dr["EQPTID"] = eqpt;
                    dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    dr["MIN_SECONDS"] = (bool)chkSearchAll.IsChecked ? 0 : 180;
                    RQSTDT.Rows.Add(dr);
                }

                dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_MULTI", "RQSTDT", "RSLTDT", RQSTDT)
                    : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_MULTI", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게

                // dtMainList null 제외.
                if (dtMainList.Rows.Count > 0)
                {
                    if (RunSplit.Equals("Y")) dtMainList = dtMainList.Select("", "EQPTID, START_TIME ASC").CopyToDataTable<DataRow>();
                    else dtMainList = dtMainList.Select("", "EQPTID, START_TIME ASC").CopyToDataTable<DataRow>();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEqptNameMulti(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE_MNT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private string SelectEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboEquipment_Machine_Multi.SelectedItems.Count; i++)
            {
                if (i < cboEquipment_Machine_Multi.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboEquipment_Machine_Multi.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboEquipment_Machine_Multi.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        private void ShowLossColorMap()
        {
            try
            {
                string sEqptID = SelectEquipment();
                string sEqptType = "M";

                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);

                Hashtable hash_color = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();

                #region ...[HashTable 초기화]
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();

                #endregion

                #region 일자 별, 설비 타이틀, 설비 Loss 내역 조회

                // 일자 + 작업 시작 기준 정보 조회 yyyy-mm-dd HH:mm:ss.000                
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptNameMulti(sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossListMulti(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHashMulti(dtEqptLossList);

                #endregion

                Hashtable hash_rs = new Hashtable();

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * dtEqptName.Rows.Count + inc;

                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();
                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (30) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);

                        _grid.Children.Add(_lable);
                    }

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];

                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }

                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = "A" + dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = string.Empty;

                        if (hash_list[sTitle] != null)
                        {
                            if (((DataTable)(hash_list[sTitle])).Rows.Count != 0)
                            {
                                DataRow[] dr = ((DataTable)hash_list[sTitle]).Select("EQPTID = '" + sTitle.Right(sTitle.Length - 1) + "' and STRT_DTTM_YMDHMS < '" + sEqptTimeList + "'");
                                sStatus = dr.Length != 0 ? dr[dr.Length - 1]["LOSS_CODE"].ToString() : dtEqptLossList.Select("EQPTID = '" + sTitle.Right(sTitle.Length - 1) + "'")[0]["LOSS_CODE"].ToString();
                            }
                        }

                        System.Drawing.Color color = GetMultiColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);
                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_Multi_MouseDown;

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);
                        }
                    }
                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 540)
                    {
                        cnt = 0;
                        inc++;
                    }
                }
                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _border_Multi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            #region Ctrl or Shift 미 입력 시, 색지도 및 체크박스 초기화.
            if (e.ChangedButton == MouseButton.Left && !Keyboard.Modifiers.GetString().Equals("Control") && !Keyboard.Modifiers.GetString().Equals("Shift"))
            {
                resetMapColorMulti();
            }
            #endregion

            #region 색지도 좌클릭 Event 하단 Grid Check 적용 및 색깔 적용.
            if (e.ChangedButton == MouseButton.Left)
            {
                Border aa = sender as Border;
                org_set = aa.Tag as Hashtable;

                string sEqptID = org_set["EQPTID"].ToString().Substring(1);
                string sEqptID_Pre = "";

                sEqptID_Pre = cboEquipment_Machine_Multi.SelectedItemsToString;

                if (!sEqptID_Pre.Contains(sEqptID))
                {
                    _grid_eqpt = sEqptID_Pre;
                    GetEqptLossRawList_MachineMulti();
                }

                string sTime = org_set["TIME"].ToString();

                #region 색지도 Click Loss 하단 리스트에서 Row 순번 탐색.
                DataRow drRow = null;
                bool bFindRow = false;
                int nFindRowIdx = -1;
                for (int r = 0; r < dgDetail.Rows.Count; r++)
                {
                    drRow = (dgDetail.Rows[r].DataItem as DataRowView).Row;
                    Int64 nTime = 0;
                    Int64 nTime_Start = 0;
                    Int64 nTime_End = 0;
                    Int64.TryParse(sTime, out nTime);
                    Int64.TryParse(drRow["HIDDEN_START"].ToString(), out nTime_Start);
                    Int64.TryParse(drRow["HIDDEN_END"].ToString(), out nTime_End);

                    if (org_set["EQPTID"].ToString().Substring(1).Equals(drRow["EQPTID"].ToString()) && (nTime_Start <= nTime && nTime_End > nTime))
                    {
                        bFindRow = true;
                        nFindRowIdx = r;
                        break;
                    }
                }

                if (nFindRowIdx == -1 && !org_set["STATUS"].ToString().Equals("R"))
                {
                    Util.MessageValidation("SFU10018"); // 선택 영역엔 Loss가 존재하지 않습니다.
                    return;
                }
                #endregion

                if (bFindRow)
                {
                    dgDetail.ScrollIntoView(nFindRowIdx, 0);
                    dgDetail.SelectedIndex = nFindRowIdx;

                    #region 기 Check 되어있을 경우, 해제 및 콤보박스 초기화
                    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[nFindRowIdx].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[nFindRowIdx].DataItem, "CHK")).Equals("True"))
                    {
                        checkBox_Change_Def = false;
                        DataTableConverter.SetValue(dgDetail.Rows[nFindRowIdx].DataItem, "CHK", false);
                        checkBox_Change_Def = true;
                        bool chk = true;
                        setMapColorMulti(org_set["TIME"].ToString(), "MAP", nFindRowIdx, chk);
                    }
                    #endregion
                    else
                    {
                        checkBox_Change_Def = false;
                        DataTableConverter.SetValue(dgDetail.Rows[nFindRowIdx].DataItem, "CHK", true);
                        checkBox_Change_Def = true;
                        bool chk = false;
                        setMapColorMulti(org_set["TIME"].ToString(), "MAP", nFindRowIdx, chk);
                    }
                }
                else if (org_set["STATUS"].ToString().Equals("R"))
                {
                    string eqpt = org_set["EQPTID"].ToString().Substring(1);


                    DataRow[] dtRowTemp = dtMainList.Select("HIDDEN_START <= '" + sTime +
                                                        "' AND HIDDEN_END > '" + sTime +
                                                        "' AND EQPTID = '" + eqpt + "'");

                    if (dtRowTemp.Count() > 0)
                    {
                        if (dtRowTemp[0]["CHK"].ToString() == "1")
                        {
                            bool chk = true;
                            setMapColorMulti(org_set["TIME"].ToString(), "MAP", nFindRowIdx, chk);
                        }
                        else
                        {
                            bool chk = false;
                            setMapColorMulti(org_set["TIME"].ToString(), "MAP", nFindRowIdx, chk);
                        }
                    }
                }
            }
            #endregion 

            #region 색지도 우클릭 Event 하단 Grid Check 적용 및 색깔 적용 (1개 항목만), Popup 호출.
            else if (e.ChangedButton == MouseButton.Right)
            {
                resetMapColorMulti();

                Border aa = sender as Border;

                org_set = aa.Tag as Hashtable;

                string sTime = org_set["TIME"].ToString();

                #region 색지도 Click Loss 하단 리스트에서 Row 순번 탐색.
                DataRow drRow = null;
                int nFindRowIdx = -1;
                for (int r = 0; r < dgDetail.Rows.Count; r++)
                {
                    drRow = (dgDetail.Rows[r].DataItem as DataRowView).Row;
                    Int64 nTime = 0;
                    Int64 nTime_Start = 0;
                    Int64 nTime_End = 0;
                    Int64.TryParse(sTime, out nTime);
                    Int64.TryParse(drRow["HIDDEN_START"].ToString(), out nTime_Start);
                    Int64.TryParse(drRow["HIDDEN_END"].ToString(), out nTime_End);

                    if (org_set["EQPTID"].ToString().Substring(1).Equals(drRow["EQPTID"].ToString()) && (nTime_Start <= nTime && nTime_End > nTime))
                    {
                        nFindRowIdx = r;
                        break;
                    }
                }

                if (nFindRowIdx == -1 && !org_set["STATUS"].ToString().Equals("R"))
                {
                    Util.MessageValidation("SFU10018"); // 선택 영역엔 Loss가 존재하지 않습니다.
                    return;
                }
                #endregion

                if (!org_set["STATUS"].ToString().Equals("R"))
                {
                    ContextMenu menu = this.FindResource("_gridMenu") as ContextMenu;
                    menu.PlacementTarget = sender as Border;
                    menu.IsOpen = true;

                    for (int i = 0; i < menu.Items.Count; i++)
                    {
                        MenuItem item = menu.Items[i] as MenuItem;

                        switch (item.Name.ToString())
                        {
                            case "LossDetail":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss내역보기");
                                item.Click -= lossDetail_Click;
                                item.Click += lossDetail_Click;
                                break;

                            case "LossSplit":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss분할");
                                item.Click -= lossSplit_Click;
                                item.Click += lossSplit_Click;
                                break;
                        }
                    }
                }
                if (RunSplit.Equals("Y"))
                {
                    if (org_set["STATUS"].ToString().Equals("R")) //추가
                    {
                        for (int r = 0; r < dtMainList.Rows.Count; r++)
                        {

                            drRow = dtMainList.Rows[r];
                            Int64 nTime_Start = 0;
                            Int64 nTime_End = 0;
                            Int64 nTime = 0;
                            Int64.TryParse(org_set["TIME"].ToString(), out nTime);
                            Int64.TryParse(drRow["HIDDEN_START"].ToString(), out nTime_Start);
                            Int64.TryParse(drRow["HIDDEN_END"].ToString(), out nTime_End);

                            if (org_set["EQPTID"].ToString().Substring(1).Equals(drRow["EQPTID"].ToString()) && (nTime_Start <= nTime && nTime_End > nTime))
                            {
                                nFindRowIdx = r;
                                splitTime_Start = nTime_Start.ToString();
                                splitTime_End = nTime_End.ToString();
                                break;
                            }
                        }

                        DataTable dt = dtMainList.Select("HIDDEN_START >= " + drRow["HIDDEN_START"].ToString() + " and HIDDEN_END <= " + drRow["HIDDEN_END"].ToString() + " and EQPTID = '" +
                            org_set["EQPTID"].ToString().Substring(1) + "' ").CopyToDataTable();
                        if (dt.Select("EIOSTAT <> 'R'").Count() > 1)
                        {
                            Util.MessageValidation("SFU3204"); //운영설비 사이에 Loss가 존재합니다.
                            btnReset_Click(null, null);
                            return;
                        }

                        COM001_014_RUN_SPLIT wndPopup = new COM001_014_RUN_SPLIT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[11];
                            Parameters[0] = org_set["EQPTID"].ToString();
                            Parameters[1] = org_set["TIME"].ToString();
                            Parameters[2] = splitTime_Start;
                            Parameters[3] = splitTime_End;
                            Parameters[4] = cboArea.SelectedValue.ToString();
                            Parameters[5] = Util.GetCondition(ldpDatePicker);
                            Parameters[6] = this;
                            Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[8] = cboProcess.SelectedValue.ToString();
                            Parameters[9] = "Main";
                            Parameters[10] = !bPack && isCauseEquipment ? "Y" : "N";

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }
                #endregion
            }
        }

        #region 하단 Grid (Loss List) 체크박스 변경 Event
        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (MachineMultiChk == false || chkMachMulti.IsChecked == false)
            {
                return;
            }
            if (checkBox_Change_Def && checkBoxChangeError_Def) // 2번 수행 방어 로직 (상단 색지도 클릭 시 Check box 활성화)
            {
                int idx = -1;
                DataRow[] tempRow = dtMainList.Select();


                for (int i = 0, j = 0; j < dgDetail.Rows.Count && i < tempRow.Count(); i++, j++)
                {
                    if (String.IsNullOrEmpty(tempRow[i]["LOSS_CODE"].GetString()) && tempRow[i]["EIOSTAT"].GetString() == "R")
                    {
                        j = j - 1;
                        continue;
                    }
                    for (int m = 0, n = j; n < dgDetail.Rows.Count && m < tempRow.Count(); m++)
                    {
                        if (DataTableConverter.GetValue(dgDetail.Rows[n].DataItem, "HIDDEN_START").GetString().Equals(tempRow[m]["HIDDEN_START"].GetString())
                            && DataTableConverter.GetValue(dgDetail.Rows[n].DataItem, "EQPTID").GetString().Equals(tempRow[m]["EQPTID"].GetString()) &&
                             ((DataTableConverter.GetValue(dgDetail.Rows[n].DataItem, "CHK").GetString().Equals("1") && tempRow[m]["CHK"].GetString().Equals("0"))
                            || (DataTableConverter.GetValue(dgDetail.Rows[n].DataItem, "CHK").GetString().Equals("0") && tempRow[m]["CHK"].GetString().Equals("1"))
                            ))
                        {
                            idx = m;
                            break;
                        }
                    }
                    if (idx != -1) break;
                }

                if (idx != -1)
                {
                    if (tempRow[idx]["CHK"].GetString().Equals("1")) tempRow[idx]["CHK"] = "0";
                    else tempRow[idx]["CHK"] = "1";
                    // inc : 색지도 -> 24시간 기준, 20초 단위로 설정. 주 / 야로 나눌 시 24시간 기준, 10초 단위로 설정. -> 색지도 선택 부분 영향
                    int inc = 20;

                    if (Util.GetCondition(cboShift).Equals(""))
                    {
                        inc = 20;
                    }
                    else
                    {
                        inc = 10;
                    }

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    dStartTime = Math.Truncate(Convert.ToDouble(tempRow[idx]["HIDDEN_START"]) / inc) * inc;
                    dEndTime = Math.Truncate(Convert.ToDouble(tempRow[idx]["HIDDEN_END"]) / inc) * inc;


                    #region 색지도 선택 부분 색 표시
                    Border borderS = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    Hashtable hashStart = borderS.Tag as Hashtable;
                    Hashtable hashEnd = borderE.Tag as Hashtable;

                    DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));
                    int col = 0;
                    if (Convert.ToInt16(hashEnd["COL"]) > Convert.ToInt16(hashStart["COL"])) col = Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]);
                    else col = (Convert.ToInt16(hashStart["COL"]) - Convert.ToInt16(hashEnd["COL"])) * (-1);
                    int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 540 + col;

                    if (tempRow[idx]["CHK"].ToString().Equals("1"))
                    {
                        for (int j = 0; j < cellCnt; j++)
                        {
                            Border _border = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                            _border.Background = new SolidColorBrush(Colors.Blue);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < cellCnt; j++)
                        {
                            Border _border = _grid.FindName("SA" + tempRow[idx]["EQPTID"].ToString().Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;
                            Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                            _border.Background = org_set["COLOR"] as SolidColorBrush;
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion

        private void resetMapColorMulti()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            foreach (DataRow dr in dtMainList.Select())
            {
                dr["CHK"] = "0";
            }

            for (int r = 0; r < dgDetail.Rows.Count; r++)
            {
                dgDetail.Rows[r].DataItem.SetValue("CHK", false);

            }
        }

        private void setMapColorMulti(String sTime, String sType, int nFindRowIdx, bool chk, C1.WPF.DataGrid.DataGridRow row = null)
        {
            string Eqptid_In_Grid = "";
            if (nFindRowIdx != -1)
            {
                Eqptid_In_Grid = dgDetail.Rows[nFindRowIdx].DataItem.GetValue("EQPTID").ToString();
            }
            else
            {
                Eqptid_In_Grid = org_set["EQPTID"].ToString().Substring(1);
            }

            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime +
                                                "' AND HIDDEN_END > '" + sTime +
                                                "' AND EQPTID = '" + Eqptid_In_Grid + "'");

            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1' AND EQPTID = '" + Eqptid_In_Grid + "'", "HIDDEN_START ASC");

            if (!chk) dtRow[0]["CHK"] = "1";
            else dtRow[0]["CHK"] = "0";

            #region 색지도 색칠 부분 Keyboard Shift 선택 일 경우, 체크박스 또한 다중 Check 및 색지도 색칠 부분 영역 설정.

            // inc : 색지도 -> 24시간 기준, 20초 단위로 설정. 주 / 야로 나눌 시 24시간 기준, 10초 단위로 설정. 
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            double dStartTime = new Double();
            Double dEndTime = new Double();

            txtStart.Text = dtRow[0]["START_TIME"].ToString();
            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

            if (Keyboard.Modifiers.GetString().Equals("Shift"))
            {
                DataRow[] Eqpt_Distinct = dtMainList.Select("CHK = 1 AND EQPTID <> '" + Eqptid_In_Grid + "'");
                if (Eqpt_Distinct.Count() > 0)
                {
                    Util.MessageValidation("SFU10017"); // 색지도 내 연속 선택은 같은 설비 내에서 가능합니다.
                    if (!chk) dtRow[0]["CHK"] = "0";
                    else dtRow[0]["CHK"] = "1";

                    checkBox_Change_Def = false;
                    if (nFindRowIdx > -1)
                    {
                        DataTableConverter.SetValue(dgDetail.Rows[nFindRowIdx].DataItem, "CHK", false);
                    }
                    checkBox_Change_Def = true;

                    return;
                }

                if (dtRowBefore.Length > 0)
                {
                    DataRow drRow = null;
                    for (int r = 0; r < dgDetail.Rows.Count; r++)
                    {

                        drRow = (dgDetail.Rows[r].DataItem as DataRowView).Row;
                        Int64 nTime = 0;
                        Int64 nTime_Start = 0;
                        Int64 nTime_End = 0;
                        Int64.TryParse(sTime, out nTime);
                        Int64.TryParse(drRow["HIDDEN_START"].ToString(), out nTime_Start);
                        Int64.TryParse(drRow["HIDDEN_END"].ToString(), out nTime_End);

                        if (((Convert.ToInt64(dtRowBefore[0]["HIDDEN_END"]) <= nTime_Start && nTime_End < nTime)
                            || (nTime <= nTime_Start && nTime_End <= Convert.ToInt64(dtRowBefore[0]["HIDDEN_START"])))
                            && drRow["EQPTID"].ToString().Equals(Eqptid_In_Grid))
                        {
                            DataTableConverter.SetValue(dgDetail.Rows[r].DataItem, "CHK", true);
                        }
                    }

                    // 기존 선택 항목과 현재 선택항목 사이 색 적용.
                    if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;
                    }
                }
                // 현재 영역만 색 적용. (처음 Shift 누를 시)
                else
                {
                    dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                    dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;
                }
            }
            // 현재 영역만 색 적용. (Shift x 일 시)
            else
            {
                dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;
            }
            #endregion

            #region 색지도 선택 부분 색 표시
            Border borderS = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + dStartTime.ToString()) as Border;
            Border borderE = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + dEndTime.ToString()) as Border;

            if (borderS == null)
            {
                borderS = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
            }
            if (borderE == null)
            {
                borderE = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
            }

            Hashtable hashStart = borderS.Tag as Hashtable;
            Hashtable hashEnd = borderE.Tag as Hashtable;

            DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                            Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                            Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                            Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                            Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                            Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

            int col = 0;
            if (Convert.ToInt16(hashEnd["COL"]) > Convert.ToInt16(hashStart["COL"])) col = Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]);
            else col = (Convert.ToInt16(hashStart["COL"]) - Convert.ToInt16(hashEnd["COL"])) * (-1);
            int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 540 + col;

            if (Keyboard.Modifiers.GetString().Equals("Shift"))
            {
                for (int j = 0; j < cellCnt; j++)
                {
                    Border _border = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                    _border.Background = new SolidColorBrush(Colors.Blue);
                }
            }
            else
            {
                if (!chk) // Check 되어있지 않았을 때, Blue 적용.
                {
                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = new SolidColorBrush(Colors.Blue);
                    }
                }
                else // 기 Check 시, 기존 색 적용.
                {
                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("SA" + Eqptid_In_Grid.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = org_set["COLOR"] as SolidColorBrush;
                    }
                }
            }
            #endregion
        }

   

        private void SetMachineMultiEqptCombo()
        {
            try
            {
                if (string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString())) return;
                if (string.IsNullOrEmpty(cboProcess.SelectedValue.ToString())) return;

                DataTable inTable1 = new DataTable("RQSTDT");
                inTable1.Columns.Add("LANGID", typeof(string));
                inTable1.Columns.Add("AREAID", typeof(string));
                inTable1.Columns.Add("EQSGID", typeof(string));
                inTable1.Columns.Add("PROCID", typeof(string));
                inTable1.Columns.Add("EQPTID", typeof(string));

                DataRow dr1 = inTable1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
                dr1["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
                dr1["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
                dr1["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) ? null : cboEquipment.SelectedValue.GetString();
                inTable1.Rows.Add(dr1);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MACHINE_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", inTable1);

                cboEquipment_Machine_Multi.DisplayMemberPath = "CBO_NAME";
                cboEquipment_Machine_Multi.SelectedValuePath = "CBO_CODE";

                this.cboEquipment_Machine_Multi.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {

            }
        }

        private Hashtable rsToHashMulti(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                int cnt = dt.Select().ToList().GroupBy(row => row["EQPTID"]).Count();
                List<System.Data.DataRow> list = dt.Select().ToList().GroupBy(row => row["EQPTID"]).Select(group => group.First()).ToList();
                for (int i = 0; i < list.Count(); i++)
                {
                    hash_return.Add("A" + list[i]["EQPTID"], dt.Select("EQPTID = '" + list[i]["EQPTID"] + "'").CopyToDataTable());
                }

            }
            catch (Exception ex)
            {
                hash_return = null;
            }
            return hash_return;
        }

        private System.Drawing.Color GetMultiColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType.Equals(""))
                        {
                            color = System.Drawing.Color.White;
                        }
                        else
                        {
                            color = System.Drawing.Color.FromName(hash_loss_color[sType].ToString());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // 예외 발생 시, White 설정.
                color = System.Drawing.Color.White;
            }
            return color;
        }

        private void TotalMultiSaveProcess()
        {
            if (!this.isTotalSave)
            {
                return;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

            DataSet ds = new DataSet();
            DataTable RQSTDT = ds.Tables.Add("INDATA");
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            DataRow[] dtRow = dtMainList.Select();

            foreach (DataRow dr in dtRow)
            {
                if (dr["CHK"].ToString() == "1" && dr["EIOSTAT"].ToString() == "R" && String.IsNullOrEmpty(dr["LOSS_CODE"].ToString()))
                {
                    DataRow dr2 = RQSTDT.NewRow();

                    dr2["EQPTID"] = dr["EQPTID"].ToString();

                    if (dr["EQPTID"].Equals(""))
                    {
                        Util.MessageValidation("SFU3514");  //설비는필수입니다.
                        return;
                    }
                    dr2["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    dr2["STRT_DTTM"] = dr["HIDDEN_START"].ToString();
                    dr2["END_DTTM"] = dr["HIDDEN_END"].ToString();
                    dr2["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
                    if (dr2["LOSS_CODE"].Equals("")) return;

                    dr2["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                    if (dr2["LOSS_DETL_CODE"].Equals(""))
                    {
                        if (popLossDetl.SelectedValue.IsNullOrEmpty())
                        {
                            // 부동내용을 입력하세요.
                            Util.MessageValidation("SFU3631");
                            return;
                        }
                    }

                    dr2["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                    dr2["SYMP_CODE"] = Util.GetCondition(cboFailure);
                    dr2["CAUSE_CODE"] = Util.GetCondition(cboCause);
                    dr2["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                    dr2["OCCR_EQPTID"] = "";
                    dr2["USERID"] = LoginInfo.USERID;
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        dr2["WRK_USERNAME"] = txtPerson.Tag;
                    }
                    RQSTDT.Rows.Add(dr2);
                }
            }


            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = RQSTDT.NewRow();

                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EQPTID"));

                    if (dr["EQPTID"].Equals(""))
                    {
                        Util.MessageValidation("SFU3514");  //설비는필수입니다.
                        return;
                    }
                    dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
                    if (dr["LOSS_CODE"].Equals("")) return;

                    dr["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                    if (dr["LOSS_DETL_CODE"].Equals(""))
                    {
                        if (popLossDetl.SelectedValue.IsNullOrEmpty())
                        {
                            // 부동내용을 입력하세요.
                            Util.MessageValidation("SFU3631");
                            return;
                        }
                    }

                    dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                    dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                    dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                    dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                    dr["OCCR_EQPTID"] = "";
                    dr["USERID"] = LoginInfo.USERID;
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        dr["WRK_USERNAME"] = txtPerson.Tag;
                    }
                    RQSTDT.Rows.Add(dr);
                }
            }

            try
            {

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                {
                    Util.MessageValidation("SFU3489");//개별등록일 경우 일괄저장 기능 사용 불가
                    return;

                }
                else
                {
                    ShowLoadingIndicator();
                    DoEvents();

                    if (string.Equals(GetAreaType(), "P"))
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL_PACK", "INDATA", null, ds);
                    }
                    else
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL", "RQSTDT", null, ds);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                dgDetail.ScrollIntoView(idx, 0);

                Util.MessageInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


    }
}