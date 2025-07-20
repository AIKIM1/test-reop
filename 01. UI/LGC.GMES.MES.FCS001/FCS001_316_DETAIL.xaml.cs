/*************************************************************************************
 Created Date : 2023.01.29
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 고온챔버 현황 세부 현황
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.29  DEVELOPER : Initial Created.
**************************************************************************************/

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
    public partial class FCS001_316_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqptID;
        private string _sLaneID;
        private string _sHeader = ObjectDic.Instance.GetObjectName("고온챔버 세부 현황");
        private bool bAuthority = false; // 권한 유무
        private string _sBtnResume = string.Empty; //연속시작
        private bool bFuntionLimit = false; //버튼별 제한 사용 여부
        Util _Util = new Util();


        public FCS001_316_DETAIL()
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

            Header += _sHeader + " : " + _sCol + "-" + _sStg;

            //Combo Setting
            //InitCombo();


            btnInit.Click += btn_Click;
            btnResume.Click += btn_Click;
            btnPause.Click += btn_Click;

            //bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_001_TRAY_UPDATE_STAT"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            //if (bUseFlag)
            //{
            //    btnUpdateStat.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    btnUpdateStat.Visibility = Visibility.Collapsed;
            //}

            //ROW1.Height = new GridLength(0);
            //ROW3.Height = new GridLength(0);

            //SetFormVisiableMaint(false);
            //SetFormVisiableRepair(false);

            //FrameCol.Width = new GridLength(0);

            //230817 버튼별 권한 관리 로직 추가 START
            // 1. 법인/동 적용 여부 체크
            // 2. USER 권한 체크
            if (IsAreaCommoncodeAttrUse())
            {
                CheckFormAuthority(LoginInfo.USERID);
            }
            else
            {
                btnResume.IsEnabled = true; //연속시작
            }
            //230817 버튼별 권한 관리 로직 추가 END

            GetLoadEqpInfo();   // 설비정보 로딩
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

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
                    case "btnInit":
                        sMsg = "FM_ME_0273";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. 설비상태 초기화 하시겠습니까?
                        sCtrStatus = "I";
                        break;

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

        #endregion

        #region Mehod
        
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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGHCHAMBER_LOAD_EQPT_HVF", "RQSTDT", "RSLTDT", dtRqst);

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
                    //txtOpStatus.Text = drItem["OPSTATUS_NAME"].ToString();

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

                    if (bFuntionLimit == true)
                    {
                        if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
                        {
                            btnPause.IsEnabled = false;       //일시정지 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                //if (_sBtnItin == "Y" && bAuthority == false)
                                if (bAuthority == false)
                                {
                                    btnInit.IsEnabled = true;         //설비상태초기화 버튼
                                }
                                else
                                {
                                    btnInit.IsEnabled = true;         //설비상태초기화 버튼
                                }
                            }

                            //Tray Run 상태일경우 연속시작버튼 Enable true
                            if (drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                            {
                                if ((_sBtnResume == "Y" && bAuthority == true) || _sBtnResume == "N")
                                {
                                    btnResume.IsEnabled = true;       //연속시작 버튼
                                }
                            }
                            else
                            {
                                btnResume.IsEnabled = false;      //연속시작 버튼
                            }
                        }
                        else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
                        {
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            if (FrameOperation.AUTHORITY.Equals("W"))
                            {
                                //if (_sBtnItin == "Y" && bAuthority == false)
                                if (bAuthority == false)
                                {
                                    btnPause.IsEnabled = true;         //일시정지 버튼 
                                }
                            }
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        }
                        else
                        {
                            btnPause.IsEnabled = false;        //일시정지 버튼    
                            btnResume.IsEnabled = false;       //연속시작 버튼
                            btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        }
                    }

                    else
                    {
                        //if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
                        //{
                        //    btnPause.IsEnabled = false;       //일시정지 버튼
                        //    if (FrameOperation.AUTHORITY.Equals("W"))
                        //    {
                        //        btnInit.IsEnabled = true;         //설비상태초기화 버튼
                        //    }

                        //    //Tray Run 상태일경우 연속시작버튼 Enable true
                        //    if (drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                        //    {
                        //        btnResume.IsEnabled = true;       //연속시작 버튼
                        //    }
                        //    else
                        //    {
                        //        btnResume.IsEnabled = false;      //연속시작 버튼
                        //    }
                        //}
                        //else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
                        //{
                        //    btnResume.IsEnabled = false;       //연속시작 버튼
                        //    if (FrameOperation.AUTHORITY.Equals("W") && drItem["ISS_RSV_FLAG"].ToString().Equals("S"))
                        //    {
                        //        btnPause.IsEnabled = true;         //일시정지 버튼
                        //    }
                        //    btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        //}
                        //else
                        //{
                        //    btnPause.IsEnabled = false;        //일시정지 버튼
                        //    btnResume.IsEnabled = false;       //연속시작 버튼
                        //    btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        //}
                        btnPause.IsEnabled = true;        //일시정지 버튼
                        btnResume.IsEnabled = true;       //연속시작 버튼
                        btnInit.IsEnabled = true;         //설비상태초기화 버튼
                    }

                    #endregion

                    switch (drItem["EQPT_CTRL_STAT_CODE"].ToString())
                    {
                        case "P":
                            btnPause.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "S":
                            btnResume.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "I":
                            btnInit.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
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
                    btnResume.IsEnabled = true;
                    bAuthority = true;
                }
                else
                {
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


                    if (Util.NVC(dtResult.Rows[0]["ATTR2"].ToString()) == "Y")
                    {
                        _sBtnResume = "Y";
                    }
                    else
                    {
                        _sBtnResume = "N";
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

        #endregion
    }
}
