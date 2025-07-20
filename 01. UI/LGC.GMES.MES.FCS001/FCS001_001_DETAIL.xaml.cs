/***************************************************************************************************************
 Created Date : 2020.11.30
      Creator : Dooly
   Decription : 충방전기 현황 세부 현황
----------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.11.19  DEVELOPER : Initial Created.
  2021.10.21  KDH       : Box To Box 요청 기능 추가 (상태값 : 'K')
  2022.05.23  이정미    : MAINT_CODE 저장 데이터 변경
  2022.07.25  KSH       : 설비제어버튼 활성화 설정 로직 변경(OCV의 경우 연속시작이 아닌 재시작 활성화)
  20222.08.05 이정미    : 수동 예약 기능 INDATA 컬럼 변경 및 조건 변경
  2022.08.08  LHD       : 사유코드 콤보박스의 SelectedValue 및 파라미터 사용부분 수정
  2022.09.26  최도훈    : Header 중복 표기 수정
  2022.12.14  조영대    : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.17  이해령    : 수동 모드 - Tray 존재시 체크 불가능 / 
  2023.07.06  권순범    : Box To Box, 강제출고요청 이력 저장 추가
  2023.07.27  이정미    : IT 정보 초기화, Box To Box 요청 버튼 제한 (FORM_FORMATION_MANA 권한 필요)
  2023.08.10  이정미    : 버튼 제한 오류 수정
  2023.08.17  이정미    : 충방전기 설비 작업 제어 버튼별 권한 관리 (FORM_FORMATION_MANA 권한 필요)
  2023.08.22  이정미    : 일시정지 상태일 때 Tray 유무 고려하지 않고 수동모드 선택되는 문제 수정, 
                          수동/수동예약 체크박스 선택 불가시 체크박스 회색 처리
  2023.10.13  이정미    : IT 정보 초기화 Validation 추가 - Box 내 Tray 존재시 IT정보초기화 사용 불가 (ESGM1 요청건) => 임시로 원복
  2023.10.16  조영대    : 작업가능 모델, 라우트, 차수 조회
  2023.11.22  조영대    : IT 정보 초기화 Validation 추가 - 예약 Tray 의 경우 메세지 Validation
  2023.12.13  손동혁    : 실제 설비가 박스에 도착을해도 신호를 받지 못하여서 UI를 통해 설비가 도착했다고 DB에서 업데이트 하여 재시작을 할 수 있도록 트레이 도착 보고 버튼 추가 (NA 특화)
  2024.03.28  복현수    : 검사공정 신규화면 추가에 따라 화면 연결 버튼 및 파라미터 추가
****************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_001_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqptID;
        private string _sLaneID;
        private string _sCstID;
        private string sMaintStartTime;
        private Boolean EqpMode = false;
        private string _sHeader = ObjectDic.Instance.GetObjectName("충방전기 세부 현황");
        private bool bAuthority = false; // 권한 유무
        private string _sBtnRestart = string.Empty; //재시작
        private string _sBtnResume = string.Empty; //연속시작
        private string _sBtnItin = string.Empty; //IT 정보초기화
        private string _sBtnBoxToBoxReq = string.Empty; //Box To Box 요청 
        private bool bFuntionLimit = false; //버튼별 제한 사용 여부
        bool bUseFlag = false; //
        Util _Util = new Util();


        public FCS001_001_DETAIL()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sRow = tmps[0] as string;
            _sCol = tmps[1] as string;
            _sStg = tmps[2] as string;
            _sEqptID = tmps[3] as string;
            _sLaneID = tmps[4] as string;
            _sCstID = tmps[5] as string;

            Header += _sHeader + " : " + _sCol + "-" + _sStg;

            //Combo Setting
            InitCombo();

            dtpRepairDate.SelectedDateTime = DateTime.Today;

            //Event Add
            btnOutStatus.Click += btn_Click;
            btnUnloadReq.Click += btn_Click;
            btnItin.Click += btn_Click;
            btnInit.Click += btn_Click;
            btnStop.Click += btn_Click;
            btnResume.Click += btn_Click;
            btnPause.Click += btn_Click;
            btnRestart.Click += btn_Click;
            btnBoxToBoxReq.Click += btn_Click; //20211021 Box To Box 요청 기능 추가 (상태값:'L')
            btnInitFire.Click += btn_Click; //2023.12.17 화재 초기화 추가

            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_001_TRAY_UPDATE_STAT"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                btnUpdateStat.Visibility = Visibility.Visible;
            }else
            {
                btnUpdateStat.Visibility = Visibility.Collapsed;
            }
           
            //ROW1.Height = new GridLength(0);
            //ROW3.Height = new GridLength(0);

            //SetFormVisiableMaint(false);
            //SetFormVisiableRepair(false);

            FrameCol.Width = new GridLength(0);

            //230817 버튼별 권한 관리 로직 추가 START
            // 1. 법인/동 적용 여부 체크
            // 2. USER 권한 체크
            if (IsAreaCommoncodeAttrUse())
            {
                CheckFormAuthority(LoginInfo.USERID);
            }
            else
            {
                btnRestart.IsEnabled = true; //재시작
                btnResume.IsEnabled = true; //연속시작
                btnItin.IsEnabled = true; //IT 정보초기화
                btnBoxToBoxReq.IsEnabled = true; //Box To Box 요청 
            }
            //230817 버튼별 권한 관리 로직 추가 END

            GetLoadEqpInfo();   // 설비정보 로딩
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "FORMEQPT_MAINT_TYPE_CODE" };
            _combo.SetCombo(cboMaintCd, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");

            //cboMaintCd.SelectedIndexChanged += CboMaintCd_SelectedIndexChanged;
            cboMaintCd.SelectedValueChanged += CboMaindCd_SelectedValueChanged;
        }
        #endregion

        #region Event

        private void CboMaindCd_SelectedValueChanged(object sender, EventArgs e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORMEQPT_MAINT_CODE";
            dr["ATTRIBUTE1"] = cboMaintCd.SelectedValue.ToString();

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

            if (dtResult.Rows.Count > 0)
            {
                cboMaintType.DisplayMemberPath = "CBO_NAME";
                cboMaintType.SelectedValuePath = "CBO_CODE";

                DataRow d = dtResult.NewRow();
                d["CBO_NAME"] = "-SELECT-";
                d["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(d, 0);

                cboMaintType.ItemsSource = dtResult.Copy().AsDataView();
                cboMaintType.SelectedIndex = 0;
            }
            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow d = dt.NewRow();
                d["CBO_NAME"] = "-SELECT-";
                d["CBO_CODE"] = "SELECT";
                dt.Rows.InsertAt(d, 0);
                cboMaintType.ItemsSource = dt.Copy().AsDataView();
                cboMaintType.SelectedIndex = 0;

                cboMaintType.IsEnabled = false;
            }

        }

        //private void CboMaintCd_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    #region MyRegion
        //    //CommonCombo_Form _combo = new CommonCombo_Form();

        //    //string[] sFilter = { "ET" + cboMaintCd.SelectedValue.ToString() };
        //    //_combo.SetCombo(cboMaintType, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");

        //    //if (cboMaintType.Items.Count <= 1)
        //    //    cboMaintType.IsEnabled = false;
        //    //else
        //    //    cboMaintType.IsEnabled = true; 
        //    #endregion

        //    DataTable RQSTDT = new DataTable();
        //    RQSTDT.TableName = "RQSTDT";
        //    RQSTDT.Columns.Add("LANGID", typeof(string));
        //    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
        //    RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

        //    DataRow dr = RQSTDT.NewRow();
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dr["CMCDTYPE"] = "FORMEQPT_MAINT_CODE";
        //    dr["ATTRIBUTE1"] = cboMaintCd.SelectedValue.ToString();

        //    RQSTDT.Rows.Add(dr);

        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

        //    cboMaintType.DisplayMemberPath = "CBO_NAME";
        //    cboMaintType.SelectedValuePath = "CBO_CODE";

        //    DataRow d = dtResult.NewRow();
        //    dr["CBO_NAME"] = "-SELECT-";
        //    dr["CBO_CODE"] = "SELECT";
        //    dtResult.Rows.InsertAt(d, 0);

        //    cboMaintType.SelectedIndex = 0;
        //}

        private void chkMan_Checked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(true);
            SetFormVisiableRepair(false);
            GetLoadBoxMaint();
        }

        private void chkMan_Unchecked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(false);
            SetFormVisiableRepair(false);
        }

        private void chkReserve_Checked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(true);
            SetFormVisiableRepair(true);
            GetLoadBoxMaint();
        }

        private void chkReserve_Unchecked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(false);
            SetFormVisiableRepair(false);
        }

        private void btnMaintSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        //수동모드 || 수동모드 예약 체크여부
                        if (chkMan.IsChecked == false && chkReserve.IsChecked == false)
                        {
                            Util.MessageInfo("FM_ME_0157"); //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
                            return;
                        }

                        //수동모드 && 수동모드 예약 체크여부
                        if (chkMan.IsChecked == true && chkReserve.IsChecked == true)
                        {
                            Util.MessageInfo("FM_ME_0173");  //수동모드와 수동모드 예약을 동시에 적용할 수 없습니다.
                            return;
                        }

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("ACTDTTM", typeof(string));
                        dtRqst.Columns.Add("SHIFT_USER_NAME", typeof(string));
                        dtRqst.Columns.Add("MAINT_CODE", typeof(string));
                        dtRqst.Columns.Add("MAINT_CNTT", typeof(string));
                        dtRqst.Columns.Add("MAINT_STAT_CODE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["EQPTID"] = _sEqptID;
                        dr["ACTDTTM"] = "99990101000000";
                        dr["SHIFT_USER_NAME"] = Util.GetCondition(txtMaintUser, sMsg: "FM_ME_0123");  //등록자를 입력하세요.
                        if (string.IsNullOrEmpty(dr["SHIFT_USER_NAME"].ToString())) return;
                        dr["MAINT_CODE"] = Util.GetCondition(cboMaintType, sMsg: "FM_ME_0144");  //부동유형을 선택하세요.//cboMaintCd
                        if (string.IsNullOrEmpty(dr["MAINT_CODE"].ToString())) return;

                        if (cboMaintType.IsEnabled)
                        {
                            if (string.IsNullOrEmpty(Util.GetCondition(cboMaintType, sMsg: "FM_ME_0143")))  //부동사유를 선택하세요.
                                return;
                            else
                                dr["MAINT_CODE"] = dr["MAINT_CODE"].ToString();   // + Util.GetCondition(cboMaintType);
                        }

                        dr["MAINT_CNTT"] = Util.GetCondition(txtMaintDesc, sMsg: "FM_ME_0142");  //부동내용을 입력하세요.
                        if (string.IsNullOrEmpty(dr["MAINT_CNTT"].ToString())) return;

                        dr["USERID"] = LoginInfo.USERID;
                        dr["MAINT_STAT_CODE"] = "C";
                        dtRqst.Rows.Add(dr);
                        SetEqpModeEnable("Y", dtRqst);
                    }
                });


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnRepairSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", (re) =>
                {
                    if (re != MessageBoxResult.OK)
                    {
                        return;
                    }

                    //수동모드 || 수동모드 예약 체크여부
                    if (chkMan.IsChecked == false && chkReserve.IsChecked == false)
                    {
                        Util.MessageInfo("FM_ME_0157");  //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
                        return;
                    }

                    //수동모드 && 수동모드 예약 체크여부
                    if (chkMan.IsChecked == true && chkReserve.IsChecked == true)
                    {
                        Util.MessageInfo("FM_ME_0173");  //수동모드와 수동모드 예약을 동시에 적용할 수 없습니다.
                        return;
                    }


                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("EQP_ID", typeof(string));
                    dtRqst.Columns.Add("MNT_TIME", typeof(string));
                    dtRqst.Columns.Add("EQP_STATUS_CD", typeof(string));
                    dtRqst.Columns.Add("SHIFT_USER_NAME", typeof(string));
                    dtRqst.Columns.Add("EQP_MNT_CD", typeof(string));
                    dtRqst.Columns.Add("MNT_PART", typeof(string));
                    dtRqst.Columns.Add("MNT_CONTENTS", typeof(string));
                    dtRqst.Columns.Add("MDF_START_TIME", typeof(string));
                    dtRqst.Columns.Add("MDF_ID", typeof(string));
                    dtRqst.Columns.Add("MNT_FLAG", typeof(string));
                    dtRqst.Columns.Add("CHECK", typeof(string));
                    dtRqst.Columns.Add("BOX_MAINT_MODE", typeof(string));
                    dtRqst.Columns.Add("BOX_MAINT_RES", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["EQP_ID"] = _sEqptID;
                    dr["MNT_TIME"] = Util.GetCondition(dtpRepairDate.SelectedDateTime.ToString("yyyyMMdd"));
                    //dtpRepairDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["EQP_STATUS_CD"] = "T";
                    dr["SHIFT_USER_NAME"] = Util.GetCondition(txtRepairUser, sMsg: "FM_ME_0180");  //수리자를 입력해주세요.
                    if (string.IsNullOrEmpty(dr["SHIFT_USER_NAME"].ToString())) return;

                    //20220808 사유코드 (M3)로 만 관리하도록 수정
                    //dr["EQP_MNT_CD"] = Util.GetCondition(cboMaintCd) + Util.GetCondition(cboMaintType);
                    dr["EQP_MNT_CD"] = Util.GetCondition(cboMaintType);

                    dr["MNT_PART"] = Util.GetCondition(txtRepairPart, sMsg: "FM_ME_0179");  //수리부품을 입력해주세요.
                    if (string.IsNullOrEmpty(dr["MNT_PART"].ToString())) return;

                    dr["MNT_CONTENTS"] = Util.GetCondition(txtRepairDesc, sMsg: "FM_ME_0178");  //수리내용을 입력해주세요.
                    if (string.IsNullOrEmpty(dr["MNT_CONTENTS"].ToString())) return;

                    dr["MDF_START_TIME"] = sMaintStartTime;
                    dr["MDF_ID"] = LoginInfo.USERID;
                    dr["MNT_FLAG"] = "R";
                    dr["CHECK"] = "N";
                    dr["BOX_MAINT_MODE"] = chkMan.IsChecked == true ? "Y" : "N";
                    dr["BOX_MAINT_RES"] = chkReserve.IsChecked == true ? "Y" : "N";
                    dtRqst.Rows.Add(dr);

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService("BR_SET_EQP_STATUS_FROM_MAINT", "INDATA", null, dtRqst, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //저장하였습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0215"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Close();
                                    }
                                });
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    });
                });
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnVerificationSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_001_DETAIL_VERIFICATION FCS001_001_DETAIL_VERIFICATION = new FCS001_001_DETAIL_VERIFICATION();
                FCS001_001_DETAIL_VERIFICATION.FrameOperation = FrameOperation;

                object[] Parameters = new object[6];
                Parameters[0] = _sEqptID;
                Parameters[1] = _sLaneID;
                Parameters[2] = _sRow;
                Parameters[3] = _sCol;
                Parameters[4] = _sStg;
                Parameters[5] = _sCstID;

                this.FrameOperation.OpenMenuFORM("SFU010712315", "FCS001_001_DETAIL_VERIFICATION", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("VERIFICATION KIT"), true, Parameters);

                Close(); //FCS001_001_DETAIL 팝업 close
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button bSend = sender as Button;

            string sMsg = string.Empty;
            string sCtrStatus = string.Empty;
            string sInitFire = string.Empty;

            try
            {
                switch (bSend.Name)
                {
                    case "btnRestart":
                        //샘플 생산 때문에 임시로 막음 (170809)
                        //if (txtTrayID.Text.ToString() == string.Empty)
                        //{
                        //    Util.AlertMsg("ME_0170");  //설비내에 Tray가 존재하지 않습니다.
                        //    return;
                        //}
                        sMsg = "FM_ME_0269";  //재시작 하시겠습니까?
                        sCtrStatus = "Q";
                        break;
                    case "btnPause":
                        if (txtTrayID.Text.ToString() == string.Empty)
                        {
                            Util.MessageInfo("FM_ME_0170"); //설비내에 Tray가 존재하지 않습니다.
                            return;
                        }
                        sMsg = "FM_ME_0270";  //일시정지 하시겠습니까?
                        sCtrStatus = "P";
                        break;
                    case "btnResume":
                        if (txtTrayID.Text.ToString() == string.Empty)
                        {
                            sMsg = "FM_ME_0170";  //설비내에 Tray가 존재하지 않습니다.
                            return;
                        }
                        sMsg = "FM_ME_0271";  //연속시작 하시겠습니까?
                        sCtrStatus = "S";
                        break;
                    case "btnStop":
                        sMsg = "FM_ME_0272";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. 현재작업 종료 하시겠습니까?
                        sCtrStatus = "E";
                        break;
                    case "btnInit":
                        sMsg = "FM_ME_0273";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. 설비상태 초기화 하시겠습니까?
                        sCtrStatus = "I";
                        break;
                    case "btnItin":
                        sMsg = "FM_ME_0274";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. IT 정보 초기화 하시겠습니까?
                        sCtrStatus = null;

                        #region IT 정보 초기화 Validation
                        DataTable inData = new DataTable("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("MENU_ID", typeof(string));
                        inData.Columns.Add("USER_IP", typeof(string));
                        inData.Columns.Add("PC_NAME", typeof(string));

                        DataRow dr = inData.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["EQPTID"] = _sEqptID;
                        dr["EQPT_CTRL_STAT_CODE"] = sCtrStatus;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["MENU_ID"] = LoginInfo.CFG_MENUID;
                        dr["USER_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        inData.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EQP_CTR_STATUS_VALIDATION", "INDATA", "OUTDATA", inData);
                        if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("RETVAL"))
                        {
                            if (Util.NVC(dtRslt.Rows[0]["RETVAL"]).Equals("2"))
                            {
                                sMsg = "FM_ME_0602";
                            }
                        }
                        #endregion

                        break;
                    case "btnUnloadReq":
                        if (txtTrayID.Text.ToString() == string.Empty)
                        {
                            Util.MessageInfo("FM_ME_0170"); //설비내에 Tray가 존재하지 않습니다.
                            return;
                        }
                        sMsg = "FM_ME_0094";  //강제출고요청을 하시겠습니까?
                        sCtrStatus = "F";
                        break;
                    case "btnOutStatus":
                        sMsg = "FM_ME_0276";  //출고 상태로 변경 하시겠습니까?
                        sCtrStatus = "L";
                        break;
                    //20211021 Box To Box 요청 기능 추가 (상태값:'L') START
                    case "btnBoxToBoxReq":
                        sMsg = "FM_ME_0431";  //Box To Box 요청 하시겠습니까?
                        sCtrStatus = "L";
                        break;
                    //20211021 Box To Box 요청 기능 추가 (상태값:'L') END
                    case "btnInitFire":
                        sMsg = "FM_ME_0623";  //화재 초기화 하시겠습니까?
                        sInitFire = "Y"; // 2023.12.17 화재 초기화 추가
                        break;
                }

                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {

                        //231013 IT 정보 초기화 Validation 추가 START
                        if (string.IsNullOrEmpty(sCtrStatus) && LoginInfo.CFG_AREA_ID == "AB")
                        {
                            if (CheckTrayExist(_sEqptID) == true)
                            {
                                Util.MessageInfo("FM_ME_0185"); //요청 실패하였습니다.
                                return;
                            }
                        }
                        //231013 IT 정보 초기화 Validation 추가 END


                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                        inData.Columns.Add("INIT_FIRE_FLAG", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("MENU_ID", typeof(string));
                        inData.Columns.Add("USER_IP", typeof(string));
                        inData.Columns.Add("PC_NAME", typeof(string));

                        DataRow dr = inData.NewRow();
                        dr["EQPTID"] = _sEqptID;
                        dr["EQPT_CTRL_STAT_CODE"] = sCtrStatus;
                        dr["INIT_FIRE_FLAG"] = sInitFire; // 2023.12.17 화재 초기화 추가
                        dr["IFMODE"] = "OFF";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["MENU_ID"] = LoginInfo.CFG_MENUID;
                        dr["USER_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        inData.Rows.Add(dr);

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    if (bSend.Name == "btnBoxToBoxReq" || bSend.Name == "btnUnloadReq")  //BOX to BOX 요청 또는 강제출고요청 시 이력 추가
                                    {
                                        string sACTID = "";

                                        if (bSend.Name == "btnBoxToBoxReq")
                                            sACTID = "UI_FORMEQP_MANL_BOX_TO_BOX";
                                        else if (bSend.Name == "btnUnloadReq")
                                            sACTID = "UI_REQ_FORMEQP_FORC_ISS";

                                        EQPT_CONTROL_HIST(sACTID);
                                    }

                                    Util.MessageInfo("FM_ME_0186"); //요청 완료하였습니다.
                                    C1Window_Loaded(null, null);
                                }
                                else
                                    Util.MessageInfo("FM_ME_0185"); //요청 실패하였습니다.

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                                HiddenLoadingIndicator();
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnMaintRepair_Click(object sender, RoutedEventArgs e)
        {
            if (ROW3.Height == new GridLength(34))
                SetFormVisiableRepair(false);
            else
                SetFormVisiableRepair(true);
        }

        #endregion

        #region Mehod

        private void SetFormVisiableMaint(Boolean parmValue)
        {
            try
            {
                if (parmValue == true)
                {
                    FrameCol.Width = new GridLength(2, GridUnitType.Star);
                }
                else
                {
                    if (ROW3.Height == new GridLength(0))
                        FrameCol.Width = new GridLength(0);
                    else
                        FrameCol.Width = new GridLength(2, GridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormVisiableRepair(Boolean parmValue)
        {
            try
            {
                if (parmValue == true)
                {
                    ROW3.Height = new GridLength(34);
                    ROW4.Height = new GridLength(1, GridUnitType.Star);

                    txtRepairUser.Text = LoginInfo.USERNAME;
                }
                else
                {
                    if (FrameCol.Width == new GridLength(2, GridUnitType.Star))
                    {
                        ROW3.Height = new GridLength(0);
                        ROW4.Height = new GridLength(0);
                    }

                    //if (ROW1.Height == new GridLength(0))
                    //    FrameCol.Width = new GridLength(0);
                    //else
                    //    FrameCol.Width = new GridLength(2, GridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetLoadEqpInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = _sLaneID;
                dr["EQPTID"] = _sEqptID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_LOAD_EQPT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow drItem = dtRslt.Rows[0];

                    txtPos.Text = drItem["LANE_ID"].ToString() + "-" + drItem["EQP_COL_LOC"].ToString() + "-" + drItem["EQP_STG_LOC"].ToString();

                    if (!string.IsNullOrEmpty(drItem["CSTID"].ToString()))
                        txtTrayID.Text = drItem["CSTID"].ToString();

                    if (dtRslt.Rows.Count == 2)
                        txtTrayID2.Text = dtRslt.Rows[1]["CSTID"].ToString();

                    if (!string.IsNullOrEmpty(drItem["TIME"].ToString()))
                        txtTime.Text = drItem["TIME"].ToString();

                    if (!string.IsNullOrEmpty(drItem["TROUBLE"].ToString()))
                        txtTrouble.Text = drItem["TROUBLE"].ToString();

                    txtStatus.Text = drItem["STATUS_NAME"].ToString();
                    txtOpStatus.Text = drItem["OPSTATUS_NAME"].ToString();

                    if (drItem["UPPER_RUN_MODE_CD"].ToString().Equals("UI") || drItem["STATUS"].ToString().Equals("2"))
                        chkMan.IsChecked = true;

                    if (drItem["EIOSTAT"].ToString().Equals("U") && string.IsNullOrEmpty(txtTrayID.Text.ToString()) && string.IsNullOrEmpty(txtTrayID2.Text.ToString()))
                        chkMan.IsChecked = true;

                    #region 버튼 Enable 변경 -2016 04 08 jeong hyeon sik
                    /*
                    STATUS
                        1       COMM_STATUS_NOR_YN = 'N'    - 통신이상
                        2       RUN_MODE_CD = 'M'           - Maintenance
                        3       UPPER_RUN_MODE_CD = 'M'     - Manual
                        4       EQP_STATUS_CD = 'T'         - Trouble
                        5       EQP_STATUS_CD = 'R'         - 작업중
                        6       EQP_STATUS_CD = 'I'         - 대기중
                        7       EQP_STATUS_CD = 'S'         - 일시정지
                        8       EQP_STATUS_CD = 'P'         - POWER ON
                        9       EQP_STATUS_CD = 'O'         - POWER OFF

                    */
                    
                    // 2023.12.17 화재초기화 추가
                    if ("F".Equals(drItem["FIRE_OCCR_FLAG"]) || "O".Equals(drItem["FIRE_OCCR_FLAG"])) //화재 초기화 버튼 활성화 조건
                    {
                        btnInitFire.IsEnabled = true;
                    }
                    else
                    {
                        btnInitFire.IsEnabled = false;
                    }

                    //230817 버튼별 권한 관리 로직 추가 START
                    if (bFuntionLimit == true)
                    {
                        if (bUseFlag)
                        {
                            if (drItem["STATUS"].ToString().Equals("7") && drItem["MANUAL_LOAD_END"].ToString().Equals("Y"))
                            {
                                btnUpdateStat.IsEnabled = true;       //설비상태 도착보고 버튼   
                            }
                            else
                            {
                                btnUpdateStat.IsEnabled = false;       //설비상태 도착보고 버튼   
                            }
                        }

                        if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
                        {
                            btnPause.IsEnabled = false;       //일시정지 버튼                    
                            btnStop.IsEnabled = false;        //현재작업종료 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                if (_sBtnItin == "Y" && bAuthority == false)
                                {
                                    btnInit.IsEnabled = true;         //설비상태초기화 버튼
                                    btnUnloadReq.IsEnabled = true;    //강제출고요청 버튼
                                }
                                else
                                {
                                    btnInit.IsEnabled = true;         //설비상태초기화 버튼
                                    btnItin.IsEnabled = true;         //IT 정보초기화 버튼
                                    btnUnloadReq.IsEnabled = true;    //강제출고요청 버튼
                                }
                            }

                            //tray 종료상태일경우 재시작버튼 Enable true
                            if (drItem["ISS_RSV_FLAG"].ToString().Equals("E"))
                            {
                                if (FrameOperation.AUTHORITY.Equals("W"))
                                {
                                    if (_sBtnRestart == "Y" && bAuthority == false)
                                    {
                                        btnOutStatus.IsEnabled = true;    //출고상태변경 버튼
                                    }
                                    else
                                    {
                                        btnRestart.IsEnabled = true;      //재시작 버튼
                                        btnOutStatus.IsEnabled = true;    //출고상태변경 버튼
                                    }
                                }
                            }
                            else
                            {
                                btnRestart.IsEnabled = false;      //재시작 버튼
                                btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                            }

                            //Tray Run 상태일경우 연속시작버튼 Enable true
                            if (drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                            {
                                //2022.07.25 OCV 의 경우 연속시작이 아닌 재시작 버튼 Enable true
                                if (drItem["PROCID"].ToString().Substring(0, 4).Equals("FF13"))
                                {
                                    if ((_sBtnRestart == "Y" && bAuthority == true) || _sBtnRestart == "N")
                                    {
                                        btnRestart.IsEnabled = true;      //재시작 버튼
                                    }
                                }
                                else
                                {
                                    if ((_sBtnResume == "Y" && bAuthority == true) || _sBtnResume == "N")
                                    {
                                        btnResume.IsEnabled = true;       //연속시작 버튼
                                    }
                                }
                            }
                            else
                            {
                                //btnRestart.IsEnabled = false;     //재시작 버튼
                                btnResume.IsEnabled = false;      //연속시작 버튼
                            }
                        }
                        else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
                        {
                            btnRestart.IsEnabled = false;      //재시작 버튼   
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                if (_sBtnItin == "Y" && bAuthority == false)
                                {
                                    btnPause.IsEnabled = true;         //일시정지 버튼 
                                    btnStop.IsEnabled = true;          //현재작업종료 버튼  
                                }
                                else
                                {
                                    btnPause.IsEnabled = true;         //일시정지 버튼 
                                    btnStop.IsEnabled = true;          //현재작업종료 버튼
                                    btnItin.IsEnabled = true;          //IT정보초기화 버튼
                                }
                            }
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                            btnUnloadReq.IsEnabled = false;    //강제출고요청 버튼
                            btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                        }
                        else
                        {
                            btnRestart.IsEnabled = false;      //재시작 버튼
                            btnPause.IsEnabled = false;        //일시정지 버튼    
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            btnStop.IsEnabled = false;         //현재작업종료 버튼
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                if ((_sBtnItin == "Y" && bAuthority == true) || _sBtnItin == "N")
                                {
                                    btnItin.IsEnabled = true;          //IT정보초기화 버튼
                                }
                            }

                            btnUnloadReq.IsEnabled = false;    //강제출고요청 버튼
                            btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                        }
                    }

                    else
                    {
                        if (bUseFlag)
                        {
                            if (drItem["STATUS"].ToString().Equals("7") && drItem["MANUAL_LOAD_END"].ToString().Equals("Y"))
                            {
                                btnUpdateStat.IsEnabled = true;       //설비상태 도착보고 버튼   
                            }
                            else
                            {
                                btnUpdateStat.IsEnabled = false;       //설비상태 도착보고 버튼   
                            }
                        }

                        if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
                        {
                            btnPause.IsEnabled = false;       //일시정지 버튼                    
                            btnStop.IsEnabled = false;        //현재작업종료 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                btnInit.IsEnabled = true;         //설비상태초기화 버튼
                                btnItin.IsEnabled = true;         //IT 정보초기화 버튼
                                btnUnloadReq.IsEnabled = true;    //강제출고요청 버튼
                            }

                            //tray 종료상태일경우 재시작버튼 Enable true
                            if (drItem["ISS_RSV_FLAG"].ToString().Equals("E"))
                            {
                                if (FrameOperation.AUTHORITY.Equals("W"))
                                {
                                    btnRestart.IsEnabled = true;      //재시작 버튼
                                    btnOutStatus.IsEnabled = true;    //출고상태변경 버튼
                                }
                            }
                            else
                            {
                                btnRestart.IsEnabled = false;      //재시작 버튼
                                btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                            }

                            //Tray Run 상태일경우 연속시작버튼 Enable true
                            if (drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                            {
                                //2022.07.25 OCV 의 경우 연속시작이 아닌 재시작 버튼 Enable true
                                if (drItem["PROCID"].ToString().Substring(0, 4).Equals("FF13"))
                                {
                                    btnRestart.IsEnabled = true;      //재시작 버튼
                                }
                                else
                                {
                                    btnResume.IsEnabled = true;       //연속시작 버튼
                                }
                            }
                            else
                            {
                                //btnRestart.IsEnabled = false;     //재시작 버튼
                                btnResume.IsEnabled = false;      //연속시작 버튼
                            }
                        }
                        else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
                        {
                            btnRestart.IsEnabled = false;      //재시작 버튼   
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                btnPause.IsEnabled = true;         //일시정지 버튼 
                                btnStop.IsEnabled = true;          //현재작업종료 버튼
                                btnItin.IsEnabled = true;          //IT정보초기화 버튼
                            }
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                            btnUnloadReq.IsEnabled = false;    //강제출고요청 버튼
                            btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                        }
                        else
                        {
                            btnRestart.IsEnabled = false;      //재시작 버튼
                            btnPause.IsEnabled = false;        //일시정지 버튼    
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            btnStop.IsEnabled = false;         //현재작업종료 버튼
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                                btnItin.IsEnabled = true;          //IT정보초기화 버튼
                            btnUnloadReq.IsEnabled = false;    //강제출고요청 버튼
                            btnOutStatus.IsEnabled = false;    //출고상태변경 버튼
                        }
                    }
                    //230817 버튼별 권한 관리 로직 추가 END

                    //샘플 생산 때문에 임시로 막음 (170809)
                    //btnRestart.Enabled = true;
                    #endregion

                    #region 체크박스 Enable 변경 -2023 01 17
                    if ((txtTrayID.Text.ToString() == string.Empty) && (txtTrayID2.Text.ToString() == string.Empty))
                    {
                        chkReserve.IsEnabled = false;
                        chkReserve.Opacity = 0.5;
                    }
                    if ((txtTrayID.Text.ToString() != string.Empty) || (txtTrayID2.Text.ToString() != string.Empty))
                    {
                        chkMan.IsEnabled = false;
                        chkMan.Opacity = 0.5;
                    }
                    #endregion

                    switch (drItem["EQPT_CTRL_STAT_CODE"].ToString())
                    {
                        case "Q":
                            btnRestart.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "P":
                            btnPause.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "S":
                            btnResume.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "E":
                            btnStop.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "I":
                            btnInit.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "F":
                            btnUnloadReq.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "L":
                            btnOutStatus.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        default:
                            break;
                    }

                    // 작업가능 모델, 라우트, 차수
                    GetWorkAvailble();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetWorkAvailble()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQPTID"] = _sEqptID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_WORK_AVAIABLE_INFO", "RQSTDT", "RSLTDT", dtRqst);
            dgWorkAvaiableModel.SetItemsSource(dtRslt, FrameOperation, false, "WORK_TYPE = 'MODEL'");
            dgWorkAvaiableRout.SetItemsSource(dtRslt, FrameOperation, false, "WORK_TYPE = 'ROUT'");
            dgWorkAvaiableSeqs.SetItemsSource(dtRslt, FrameOperation, false, "WORK_TYPE = 'SEQS'");

        }

        private void GetBoxReserve()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = _sEqptID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CTR_STATUS", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["EQPT_CTRL_STAT_CODE"].ToString().Equals("M"))
                        chkReserve.IsChecked = true;
                    else
                        chkReserve.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormEnableChkMode()
        {
            try
            {
                if (string.IsNullOrEmpty(txtTrayID.Text))
                {
                    chkMan.IsChecked = true;
                    chkReserve.IsChecked = false;
                }
                else
                {
                    chkMan.IsChecked = false;
                    chkReserve.IsChecked = true;
                }

                //현재 상태가 수동이면
                if (chkMan.IsChecked == true)
                {
                    chkMan.IsChecked = false;
                    SetFormVisiableMaint(true);
                    SetFormVisiableRepair(false);
                }

                if (chkReserve.IsChecked == true)
                {
                    chkReserve.IsChecked = false;
                    SetFormVisiableMaint(true);
                    SetFormVisiableRepair(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetLoadBoxMaint()
        {
            try
            {
                // 초기화 먼저 진행
                txtMaintUser.Text = LoginInfo.USERNAME;
                txtMaintDesc.IsReadOnly = false;
                txtMaintDesc.Text = string.Empty;
                btnMaintSave.Visibility = Visibility.Visible;
                btnMaintSave.IsEnabled = false;
                btnMaintRepair.Visibility = Visibility.Visible;
                btnMaintRepair.IsEnabled = false;
                btnVerificationSearch.Visibility = Visibility.Visible;
                btnVerificationSearch.IsEnabled = false; 
                cboMaintCd.SelectedIndex = 0;
                cboMaintCd.IsEnabled = true;

                //데이터 SELECT
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MNT_FLAG", typeof(string));
                dtRqst.Columns.Add("MDF_END_TIME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = _sEqptID;
                dr["MNT_FLAG"] = "C";
                dr["MDF_END_TIME"] = "99990101000000";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TH_EQP_MAINT", "RQSTDT", "RSLTDT", dtRqst);

                //데이터 존재시
                if (dtRslt.Rows.Count > 0)
                {
                    btnMaintSave.IsEnabled = false;
                    if (FrameOperation.AUTHORITY.Equals("W")) btnMaintRepair.IsEnabled = true;
                    for (int iRow = 0; iRow < dtRslt.Rows.Count; iRow++)
                    {
                        txtMaintUser.Text = dtRslt.Rows[iRow]["SHFT_USER_NAME"].ToString();
                        txtMaintDesc.Text = dtRslt.Rows[iRow]["MAINT_CNTT"].ToString();

                        //sMaintStartTime = dtRslt.Rows[iRow]["MAINT_STRT_DTTM"].ToString();
                        // ACTDTTM으로 변경
                        sMaintStartTime = dtRslt.Rows[iRow]["ACTDTTM"].ToString();

                        /* 2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가 
                            * 부동유형 추가에 따른 변경, cmbMaintType추가
                            * */
                        //iComboIndex = Convert.ToInt32(dsOutData.Tables["OUT_DATA"].Rows[iRow]["EQP_MNT_CD"]);
                        //cmbMaintCD.SelectedIndex = iComboIndex;
                        if (dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Length > 1)
                        {
                            cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 1);
                            cboMaintType.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 2);
                            // 20220808 - dtRslt에 cboMaintType의 데이터가 M0, M1 임으로, (0,2)로 변경하여 콤보박스 채움
                            //cboMaintType.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(1, 1);

                        }
                        else
                        {
                            cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 1);
                        }

                        txtMaintUser.IsReadOnly = true;
                        cboMaintCd.IsEnabled = false;
                        cboMaintType.IsEnabled = false;
                        txtMaintDesc.IsReadOnly = true;
                    }

                    //박스 부동사유 정보가 있으면 수리이력을 입력하도록 한다.
                    SetFormVisiableRepair(true);
                }
                else
                {
                    //부동사유 데이터가 없으면 수리입력 불가능 하도록 변경.
                    if (FrameOperation.AUTHORITY.Equals("W")) btnMaintSave.IsEnabled = true;
                    btnMaintRepair.IsEnabled = false;
                    SetFormVisiableRepair(false);
                }

                bool bVerifyUseFlag = _Util.IsAreaCommonCodeUse("FORM_VERIFICATION_USE", "USE_FLAG"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if(bVerifyUseFlag)
                {
                    btnVerificationSearch.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEqpModeEnable(string parmMode, DataTable sDt)
        {
            try
            {
                EqpMode = false;

                //박스가 수동모드면
                if (chkMan.IsChecked == true)
                {
                    DataSet ds = new DataSet();
                    DataTable dt = ds.Tables.Add("INDATA");
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("CHECK", typeof(string));
                    dt.Columns.Add("IFMODE", typeof(string));
                    dt.Columns.Add("USERID", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["EQPTID"] = _sEqptID;
                    dr["CHECK"] = parmMode;
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dt.Rows.Add(dr);

                    new ClientProxy().ExecuteService_Multi("BR_SET_EQP_STATUS_UPPER_RUN_MODE", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                EqpMode = false;
                            else
                                EqpMode = true;

                            //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
                            if (EqpMode)
                            {
                                ShowLoadingIndicator();
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_FORMEQPT_MAINT_HIST", "RQSTDT", "RSLTDT", sDt);
                                Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                                Close();
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
                    }, ds);
                }

                //박스가 수동예약 모드면
                if (chkReserve.IsChecked == true)
                {
                    DataSet ds = new DataSet();
                    DataTable dt = ds.Tables.Add("INDATA");
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                    dt.Columns.Add("IFMODE", typeof(string));
                    dt.Columns.Add("USERID", typeof(string));
                    dt.Columns.Add("MENU_ID", typeof(string));
                    dt.Columns.Add("USER_IP", typeof(string));
                    dt.Columns.Add("PC_NAME", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["EQPTID"] = _sEqptID;
                    dr["EQPT_CTRL_STAT_CODE"] = parmMode.Equals("Y") ? "M" : string.Empty;
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["MENU_ID"] = LoginInfo.CFG_MENUID;
                    dr["USER_IP"] = LoginInfo.USER_IP;
                    dr["PC_NAME"] = LoginInfo.PC_NAME;
                    dt.Rows.Add(dr);

                    new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                EqpMode = true;
                            else
                                EqpMode = false;
                            GetBoxReserve();

                            //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
                            if (EqpMode)
                            {
                                ShowLoadingIndicator();
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_FORMEQPT_MAINT_HIST", "RQSTDT", "RSLTDT", sDt);
                                Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                                Close();
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
                    }, ds);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void EQPT_CONTROL_HIST(string sACTID)
        {
            try
            {
                DataSet indataSet2 = new DataSet();
                DataTable inData2 = indataSet2.Tables.Add("INDATA");
                inData2.Columns.Add("SRCTYPE", typeof(string));
                inData2.Columns.Add("IFMODE", typeof(string));
                inData2.Columns.Add("EQPTID", typeof(string));
                inData2.Columns.Add("USERID", typeof(string));
                inData2.Columns.Add("ACTID", typeof(string));

                DataRow dr2 = inData2.NewRow();
                dr2["SRCTYPE"] = "UI";
                dr2["EQPTID"] = _sEqptID;
                dr2["IFMODE"] = "OFF";
                dr2["USERID"] = LoginInfo.USERID;
                dr2["ACTID"] = sACTID;
                inData2.Rows.Add(dr2);

                DataTable inCSTID = indataSet2.Tables.Add("INCSTID");
                inCSTID.Columns.Add("CSTID", typeof(string));

                if (txtTrayID.Text != "")
                {
                    DataRow dr3 = inCSTID.NewRow();
                    dr3["CSTID"] = txtTrayID.Text;
                    inCSTID.Rows.Add(dr3);
                }

                if (txtTrayID2.Text != "")
                {
                    DataRow dr4 = inCSTID.NewRow();
                    dr4["CSTID"] = txtTrayID2.Text;
                    inCSTID.Rows.Add(dr4);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_EQPT_CONTROL_HIST", "INDATA,INCSTID", "OUTDATA", indataSet2);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

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

        //230817 버튼별 권한 관리 로직 추가 START
        private void CheckFormAuthority(string sUserID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = sUserID;
                newRow["AUTHID"] = "FORM_FORMATION_MANA";
                newRow["USE_FLAG"] = 'Y';
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    btnRestart.IsEnabled = true;
                    btnResume.IsEnabled = true;
                    btnItin.IsEnabled = true;
                    btnBoxToBoxReq.IsEnabled = true;
                    bAuthority = true;
                }
                else
                {
                    if (_sBtnBoxToBoxReq == "N")
                    {
                        btnBoxToBoxReq.IsEnabled = true;
                    }
                    bAuthority = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bAuthority = false;
            }
        }

        private bool IsAreaCommoncodeAttrUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_FOMT_UI_CTRL";
                dr["COM_CODE"] = "FORM_FOMT_UI_CTRL";
                dr["USE_FLAG"] = 'Y';
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["ATTR1"].ToString()) == "Y")
                    {
                        _sBtnRestart = "Y";
                    }
                    else
                    {
                        _sBtnRestart = "N";
                    }

                    if (Util.NVC(dtResult.Rows[0]["ATTR2"].ToString()) == "Y")
                    {
                        _sBtnResume = "Y";
                    }
                    else
                    {
                        _sBtnResume = "N";
                    }

                    if (Util.NVC(dtResult.Rows[0]["ATTR3"].ToString()) == "Y")
                    {
                        _sBtnItin = "Y";
                    }
                    else
                    {
                        _sBtnItin = "N";
                    }

                    if (Util.NVC(dtResult.Rows[0]["ATTR4"].ToString()) == "Y")
                    {
                        _sBtnBoxToBoxReq = "Y";
                    }
                    else
                    {
                        _sBtnBoxToBoxReq = "N";
                    }
                    bFuntionLimit = true;
                    return true;
                }
                else
                {
                    bFuntionLimit = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bFuntionLimit = false;
                return false;
            }
        }
        //230817 버튼별 권한 관리 로직 추가 END


        //231013 IT 정보 초기화 Validation 추가 START
        private bool CheckTrayExist(string _sEqptID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _sEqptID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_IS_EXIST_EQP_TEMP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["TRAY_EXST_FLAG"].ToString()) == "1")
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
                Util.MessageException(ex);
                return false;
            }
        }
        //231013 IT 정보 초기화 Validation 추가 END

        #endregion

        private void btnUpdateStat_Click(object sender, RoutedEventArgs e)
        {
                //Tray 도착 상태 보고를 하시겠습니까?
                Util.MessageConfirm("FM_ME_0603", (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        try
                        {


                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                            RQSTDT.Columns.Add("AREAID", typeof(string));
                            RQSTDT.Columns.Add("IFMODE", typeof(string));
                            RQSTDT.Columns.Add("USERID", typeof(string));
                            RQSTDT.Columns.Add("EQPTID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["IFMODE"] = "ON";
                            dr["USERID"] = LoginInfo.USERID;
                            dr["EQPTID"] = _sEqptID;
                           
                            RQSTDT.Rows.Add(dr);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_FORM_LOAD_END_FL", "RQSTDT", "RSLTDT", RQSTDT);


                           
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            C1Window_Loaded(null, null);
                        }
                    }
                });       
        }
    }
}
