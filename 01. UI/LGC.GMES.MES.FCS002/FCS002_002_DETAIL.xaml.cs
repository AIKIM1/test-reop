/*************************************************************************************
 Created Date : 2020.12.07
      Creator : Dooly
   Decription : JIG 충방전기 현황 세부 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.07  DEVELOPER : Initial Created.
  2021.08.24  KDH       : 강제출고요청 기능 추가
  2022.05.06  KDH       : 설비제어버튼 활성화 설정 로직 변경
  2022.07.25  KSH       : 설비제어버튼 활성화 설정 로직 변경(J/F OCV의 경우 연속시작이 아닌 재시작 활성화)
  2022.07.27  KSH       : 수동 예약 기능추가(Box 수리도 미리 사전구현)
  2022.08.05  LJM       : 수동 예약 기능 오류 수정 (충방전기 현황 세부 현황 수정하면서 같이 수정)
  2022.08.08  LHD       : 사유코드 콤보박스의 SelectedValue 및 파라미터 사용부분 수정
  2022.09.29  LJM       : 수동 예약 기능 숨김, 수동 기능 추가 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_002_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqptID;
        private string _sLaneID;
        private string _ColHeader;
        private string _RowHeader;
        // 2022.07.27  KSH       : 수동 예약 기능추가
        private string sMaintStartTime;
        private Boolean EqpMode = false;
        private string _sHeader = ObjectDic.Instance.GetObjectName("JIG 세부 현황");

        Util _Util = new Util(); // 2021.08.24 강제출고요청 기능 추가

        public FCS002_002_DETAIL()
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
            _ColHeader = tmps[5] as string;
            _RowHeader = tmps[6] as string;

            Header += _sHeader + " : " + _ColHeader + "-" + _RowHeader;

            // 2022.07.27  KSH       : 수동 예약 기능추가
            //Combo Setting
            InitCombo();

            //Event Add
            btnRestart.Click += btn_Click;
            btnPause.Click += btn_Click;
            btnResume.Click += btn_Click;
            btnStop.Click += btn_Click;
            btnInit.Click += btn_Click;
            btnItin.Click += btn_Click;
            btnUnloadReq.Click += btn_Click; // 2021.08.24 강제출고요청 기능 추가

            // 2021.08.24 강제출고요청 기능 추가 START
            if (_Util.IsAreaCommonCodeUse("JF_FORCE_OUT", "FORCE_OUT")) //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            {
                btnUnloadReq.Visibility = Visibility.Visible;
            }
            else
            {
                btnUnloadReq.Visibility = Visibility.Collapsed;
            }
            // 2021.08.24 강제출고요청 기능 추가 END
            // 2022.07.27  KSH       : 수동 예약 기능추가
            FrameCol.Width = new GridLength(0);

            GetLoadEqpInfo();           // 설비정보 로딩
            GetBoxReserve();            // 수동모드 예약&취소 판단
        }

        #endregion

        #region Initialize
        // 2022.07.27  KSH       : 수동 예약 기능추가
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

        // 2022.07.27  KSH       : 수동 예약 기능추가
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

        // 2022.09.20  LJM       : 수동 기능 추가  
        private void chkMan_Checked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(true);
            GetLoadBoxMaint();
        }

        // 2022.09.20  LJM       : 수동 기능 추가 
        private void chkMan_Unchecked(object sender, RoutedEventArgs e)
        {
            SetFormVisiableMaint(false);
        }

        // 2022.07.27  KSH       : 수동 예약 기능추가
        // 2022.09.20  LJM       : 수동 예약 기능 숨김 
        //private void chkReserve_Checked(object sender, RoutedEventArgs e)
        //{
        //    SetFormVisiableMaint(true);
        //    SetFormVisiableRepair(true);
        //    GetLoadBoxMaint();
        //}

        // 2022.07.27  KSH       : 수동 예약 기능추가
        // 2022.09.20  LJM       : 수동 예약 기능 숨김
        //private void chkReserve_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    SetFormVisiableMaint(false);
        //    SetFormVisiableRepair(false);
        //}

        // 2022.07.27  KSH       : 수동 예약 기능추가
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
                        if (chkMan.IsChecked == false )
                        {
                            //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
                            Util.MessageInfo("FM_ME_0157"); 
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

        // 2022.07.27  KSH       : 수동 예약 기능추가
        // 
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
                    if (chkMan.IsChecked == false)
                    {
                        //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
                        Util.MessageInfo("FM_ME_0157");  
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
                    dr["BOX_MAINT_MODE"] = chkMan.IsChecked == true ? "Y" : "N";//"N";
                    //dr["BOX_MAINT_RES"] = chkReserve.IsChecked == true ? "Y" : "N";
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

        private void btn_Click(object sender, RoutedEventArgs e)
        {

            Button bSend = sender as Button;

            string sMsg = string.Empty;
            string sCtrStatus = string.Empty;

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
                        break;
                    // 2021.08.24 강제출고요청 기능 추가 START
                    case "btnUnloadReq":
                        if (txtTrayID.Text.ToString() == string.Empty)
                        {
                            Util.MessageInfo("FM_ME_0170"); //설비내에 Tray가 존재하지 않습니다.
                            return;
                        }
                        sMsg = "FM_ME_0094";  //강제출고요청을 하시겠습니까?
                        sCtrStatus = "F";
                        break;
                    // 2021.08.24 강제출고요청 기능 추가 END
                }

                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow dr = inData.NewRow();
                        dr["EQPTID"] = _sEqptID;
                        dr["EQPT_CTRL_STAT_CODE"] = sCtrStatus;
                        dr["IFMODE"] = "OFF";
                        dr["USERID"] = LoginInfo.USERID;
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
                                    Util.MessageInfo("FM_ME_0186"); //요청 완료하였습니다.
                                    C1Window_Loaded(null, null);
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0185"); //요청 실패하였습니다.
                                }

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



        #endregion

        #region Mehod

        // 2022.07.27  KSH       : 수동 예약 기능추가
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

        // 2022.07.27  KSH       : 수동 예약 기능추가
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
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _sEqptID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_JIG_FORMATION_LOAD_EQPT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow drItem = dtRslt.Rows[0];
                    txtPos.Text = _ColHeader + ":" + _RowHeader;

                    if (!string.IsNullOrEmpty(drItem["CSTID"].ToString()))
                        txtTrayID.Text = drItem["CSTID"].ToString();

                    if (!string.IsNullOrEmpty(drItem["TIME"].ToString()))
                        txtTime.Text = drItem["TIME"].ToString();

                    if (!string.IsNullOrEmpty(drItem["TROUBLE"].ToString()))
                        txtTrouble.Text = drItem["TROUBLE"].ToString();

                    txtStatus.Text = drItem["STATUS_NAME"].ToString();
                    txtOpStatus.Text = drItem["OPSTATUS_NAME"].ToString();

                    //2022.09.27  LJM 수동모드 추가 
                    if (drItem["EIOSTAT"].ToString().Equals("U"))
                        chkMan.IsChecked = true;

                    //Header += " : " + _sCol + "-" + _sStg;

                    #region [버튼 Enable 변경 -2016 04 08 jeong hyeon sik]
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
                    if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7")) //4(T):Trouble, 7(S):일시정지
                    {
                        btnPause.IsEnabled = false;       //일시정지 버튼                    
                        btnStop.IsEnabled = false;        //현재작업종료 버튼
                        if (FrameOperation.AUTHORITY.Equals("W"))
                        {
                            btnInit.IsEnabled = true;         //설비상태초기화 버튼
                            btnItin.IsEnabled = true;         //IT정보초기화 버튼
                            btnRestart.IsEnabled = true;
                        }

                        //Tray 종료상태일경우 재시작버튼 Enable true
                        if (drItem["ISS_RSV_FLAG"].ToString().Equals("E"))
                        {
                            if (FrameOperation.AUTHORITY.Equals("W")) btnRestart.IsEnabled = true;      //재시작 버튼
                        }
                        else
                            btnRestart.IsEnabled = false;      //재시작 버튼

                        //20220506_설비제어버튼 활성화 설정 로직 변경 START
                        ////Tray Run 상태일경우 연속시작버튼 Enable true
                        //if (drItem["OPSTATUS"].ToString().Equals("S"))
                        //{
                        //    if (FrameOperation.AUTHORITY.Equals("W")) btnResume.IsEnabled = true;       //연속시작 버튼
                        //}
                        //else
                        //    btnResume.IsEnabled = false;      //연속시작 버튼

                        //Tray Run 상태일경우 연속시작버튼 Enable true
                        if (drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                        {
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                //2022.07.25 J/F OCV 의 경우 연속시작이 아닌 재시작 버튼 Enable true
                                if (drItem["PROCID"].ToString().Substring(0, 4).Equals("FFJ3"))
                                {
                                    btnRestart.IsEnabled = true;      //재시작 버튼
                                }
                                else
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
                        //20220506_설비제어버튼 활성화 설정 로직 변경 END
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
                    }
                    else
                    {
                        btnRestart.IsEnabled = false;      //재시작 버튼
                        btnPause.IsEnabled = false;        //일시정지 버튼    
                        btnResume.IsEnabled = false;       //연속시작 버튼
                        btnStop.IsEnabled = false;         //현재작업종료 버튼
                        btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        if (FrameOperation.AUTHORITY.Equals("W")) btnItin.IsEnabled = true;          //IT정보초기화 버튼
                    }

                    //샘플 생산 때문에 임시로 막음 (170809)
                    //if (FrameOperation.AUTHORITY.Equals("W")) btnRestart.IsEnabled = true;
                    #endregion

                    switch (drItem["EQPT_CTRL_STAT_CODE"].ToString())
                    {
                        case "Q":
                            btnRestart.IsEnabled = false;
                            Header += _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "P":
                            btnPause.IsEnabled = false;
                            Header += _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "S":
                            btnResume.IsEnabled = false;
                            Header += _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "E":
                            btnStop.IsEnabled = false;
                            Header += _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "I":
                            btnInit.IsEnabled = false;
                            Header += _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
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

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 2022.07.27  LJM       : 수동 기능 추가
        private void SetFormEnableChkMode()
        {
            try
            {
                if (string.IsNullOrEmpty(txtTrayID.Text))
                {
                    chkMan.IsChecked = true;
                    //chkReserve.IsChecked = false;
                }
                else
                {
                    chkMan.IsChecked = false;
                    //chkReserve.IsChecked = true;
                }

                //현재 상태가 수동이면
                if (chkMan.IsChecked == true)
                {
                    chkMan.IsChecked = false;
                    SetFormVisiableMaint(true);
                    SetFormVisiableRepair(false);
                }

                //if (chkReserve.IsChecked == true)
                //{
                //    chkReserve.IsChecked = false;
                //    SetFormVisiableMaint(true);
                //    SetFormVisiableRepair(false);
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        // 2022.07.27  KSH       : 수동 예약 기능추가
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
                        // sMaintStartTime = dtRslt.Rows[iRow]["MAINT_STRT_DTTM"].ToString();
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
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 2022.07.27  KSH       : 수동 예약 기능추가
        // 2022.09.20  LJM       : 수동 예약 기능 숨김, 수동 기능 추가 
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

                ////박스가 수동예약 모드면
                //if (chkReserve.IsChecked == true)
                //{
                //
                //    DataSet ds = new DataSet();
                //    DataTable dt = ds.Tables.Add("INDATA");
                //    dt.Columns.Add("EQPTID", typeof(string));
                //    dt.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                //    dt.Columns.Add("IFMODE", typeof(string));
                //    dt.Columns.Add("USERID", typeof(string));
                //
                //    DataRow dr = dt.NewRow();
                //    dr["EQPTID"] = _sEqptID;
                //    dr["EQPT_CTRL_STAT_CODE"] = parmMode.Equals("Y") ? "M" : string.Empty;
                //    dr["IFMODE"] = "OFF";
                //    dr["USERID"] = LoginInfo.USERID;
                //    dt.Rows.Add(dr);
                //
                //    new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", (bizResult, bizException) =>
                //    {
                //        try
                //        {
                //            if (bizException != null)
                //            {
                //                Util.MessageException(bizException);
                //                return;
                //            }
                //
                //            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                //                EqpMode = true;
                //            else
                //                EqpMode = false;
                //
                //            GetBoxReserve();
                //
                //        //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
                //        if (EqpMode)
                //            {
                //                ShowLoadingIndicator();
                //                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_FORMEQPT_MAINT_HIST", "RQSTDT", "RSLTDT", sDt);
                //                Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
                //            Close();
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Util.MessageException(ex);
                //        }
                //        finally
                //        {
                //            HiddenLoadingIndicator();
                //        }
                //    }, ds);
                //}
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



        #endregion

        // 2022.07.27  KSH       : 수동 예약 기능추가
        private void btnMaintRepair_Click(object sender, RoutedEventArgs e)
        {
            if (ROW3.Height == new GridLength(34))
                SetFormVisiableRepair(false);
            else
                SetFormVisiableRepair(true);
        }
    }
}
