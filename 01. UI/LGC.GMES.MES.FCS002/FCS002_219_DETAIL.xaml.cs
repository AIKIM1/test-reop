/*************************************************************************************
 Created Date : 2023.01.16
      Creator : Dooly
   Decription : CPF 충방전기 세부 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.16  DEVELOPER : Initial Created.
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
    public partial class FCS002_219_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqptID;
        private string _sEqptNAME;
        private string _sLaneID;
        private string sMaintStartTime;
        private Boolean EqpMode = false;
        private string _sHeader = ObjectDic.Instance.GetObjectName("전용 OCV 세부 현황");

        public FCS002_219_DETAIL()
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
            _sEqptNAME = tmps[5] as string;

            Header = _sHeader + " : "+ ObjectDic.Instance.GetObjectName("[*]"+ _sEqptNAME);

            //Combo Setting
            InitCombo();
            
            if (sender != null)
            {
                //Event Add
                btnInit.Click += btn_Click;
                btnUnloadReq.Click += btn_Click;
                btnRestart.Click += btn_Click;
                btnBCRRead.Click += btn_Click;
                btnItin.Click += btn_Click;
            }
            FrameCol.Width = new GridLength(0);

            





            GetLoadEqpInfo();       // 설비정보 로딩
            //GetBoxReserve();        // 수동모드 예약&취소 판단
            //SetFormEnableChkMode(); // TRAY_ID 여부에 따라 수동모드 && 수동모드 예약 판단








        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //string[] sFilter = { "FORMEQPT_MAINT_TYPE_CODE" };
            //_combo.SetCombo(cboMaintCd, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");

            //cboMaintCd.SelectedValueChanged += CboMaindCd_SelectedValueChanged;
        }
        #endregion

        #region Event

        private void CboMaindCd_SelectedValueChanged(object sender, EventArgs e)
        {
            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("LANGID", typeof(string));
            //RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            //RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["LANGID"] = LoginInfo.LANGID;
            //dr["CMCDTYPE"] = "FORMEQPT_MAINT_CODE";
            //dr["ATTRIBUTE1"] = cboMaintCd.SelectedValue.ToString();

            //RQSTDT.Rows.Add(dr);

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

            //if (dtResult.Rows.Count > 0)
            //{
            //    cboMaintType.DisplayMemberPath = "CBO_NAME";
            //    cboMaintType.SelectedValuePath = "CBO_CODE";

            //    DataRow d = dtResult.NewRow();
            //    d["CBO_NAME"] = "-SELECT-";
            //    d["CBO_CODE"] = "SELECT";
            //    dtResult.Rows.InsertAt(d, 0);

            //    cboMaintType.ItemsSource = dtResult.Copy().AsDataView();
            //    cboMaintType.SelectedIndex = 0;
            //    cboMaintType.IsEnabled = true;
            //}
            //else
            //{
            //    DataTable dt = new DataTable();
            //    dt.Columns.Add("CBO_NAME", typeof(string));
            //    dt.Columns.Add("CBO_CODE", typeof(string));

            //    DataRow d = dt.NewRow();
            //    d["CBO_NAME"] = "-SELECT-";
            //    d["CBO_CODE"] = "SELECT";
            //    dt.Rows.InsertAt(d, 0);
            //    cboMaintType.ItemsSource = dt.Copy().AsDataView();
            //    cboMaintType.SelectedIndex = 0;

            //    cboMaintType.IsEnabled = false;
            //}

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
                    case "btnBCRRead":
                        sMsg = "FM_ME_0521";  //BCR재읽기 요청 하시겠습니까?
                        sCtrStatus = "B";
                        break;
                    case "btnItin":
                        sMsg = "FM_ME_0274";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. IT 정보 초기화 하시겠습니까?
                        sCtrStatus = null;
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
                        //case "btnOutStatus":
                        //    sMsg = "FM_ME_0276";  //출고 상태로 변경 하시겠습니까?
                        //    sCtrStatus = "L";
                        //    break;
                        //case "btnBoxToBoxReq":
                        //    sMsg = "FM_ME_0431";  //Box To Box 요청 하시겠습니까?
                        //    sCtrStatus = "L";
                        //    break;
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
                        new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
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

        private void btnMaintRepair_Click(object sender, RoutedEventArgs e)
        {
            //if (ROW3.Height == new GridLength(34))
            //    SetFormVisiableRepair(false);
            //else
            //    SetFormVisiableRepair(true);
        }

        #endregion

        #region Mehod

        private void SetFormVisiableMaint(Boolean parmValue)
        {
            //try
            //{
            //    if (parmValue == true)
            //    {
            //        FrameCol.Width = new GridLength(2, GridUnitType.Star);
            //    }
            //    else
            //    {
            //        if (ROW3.Height == new GridLength(0))
            //            FrameCol.Width = new GridLength(0);
            //        else
            //            FrameCol.Width = new GridLength(2, GridUnitType.Star);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void SetFormVisiableRepair(Boolean parmValue)
        {
            //try
            //{
            //    if (parmValue == true)
            //    {
            //        ROW3.Height = new GridLength(34);
            //        ROW4.Height = new GridLength(1, GridUnitType.Star);

            //        txtRepairUser.Text = LoginInfo.USERNAME;
            //    }
            //    else
            //    {
            //        if (FrameCol.Width == new GridLength(2, GridUnitType.Star))
            //        {
            //            ROW3.Height = new GridLength(0);
            //            ROW4.Height = new GridLength(0);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
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
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = _sLaneID;
                dr["EQPTID"] = _sEqptID;
                dr["EQPT_GR_TYPE_CODE"] = "8";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_LOAD_EQPT_MB", "RQSTDT", "RSLTDT", dtRqst);

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


                    if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
                    {
                        

                        if (drItem["STATUS"].ToString().Equals("4"))   //4(T):Trouble, 7(S):일시정지
                        {
                            btnBCRRead.IsEnabled = true;       //일시정지 버튼                    

                        }
                        if (FrameOperation.AUTHORITY.Equals("W"))
                        {
                            btnInit.IsEnabled = true;         //설비상태초기화 버튼
                            btnUnloadReq.IsEnabled = true;    //강제출고요청 버튼
                        }

                   
                    }
                    else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
                    {
                        btnRestart.IsEnabled = false;      //재시작 버튼   
                        btnBCRRead.IsEnabled = false;
                        btnInit.IsEnabled = false;         //설비상태초기화 버튼
                        btnUnloadReq.IsEnabled = false;
                    }
                    else
                    {
                        btnRestart.IsEnabled = false;      //재시작 버튼

                        btnBCRRead.IsEnabled = false;
                        btnInit.IsEnabled = true;         //설비상태초기화 버튼
                        btnUnloadReq.IsEnabled = false;

                    }
                    #endregion

                    switch (drItem["EQPT_CTRL_STAT_CODE"].ToString())
                    {
                        case "Q":
                            btnRestart.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        //case "P":
                        //    btnPause.IsEnabled = false;
                        //    Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                        //    break;
                        //case "S":
                        //    btnResume.IsEnabled = false;
                        //    Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                        //    break;
                        //case "E":
                        //    btnStop.IsEnabled = false;
                        //    Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                        //    break;
                        case "I":
                            btnInit.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "F":
                            btnUnloadReq.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "L":
                            //btnOutStatus.IsEnabled = false;
                            Header = _sHeader + "(" + drItem["EQPT_CTRL_STAT_NAME"].ToString() + ")";
                            break;
                        case "B":
                            btnBCRRead.IsEnabled = false;
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

        private void GetBoxReserve()
        {
            //try
            //{
            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "RQSTDT";
            //    dtRqst.Columns.Add("EQPTID", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["EQPTID"] = _sEqptID;
            //    dtRqst.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CTR_STATUS_MB", "RQSTDT", "RSLTDT", dtRqst);

            //    if (dtRslt.Rows.Count > 0)
            //    {
            //        if (dtRslt.Rows[0]["EQPT_CTRL_STAT_CODE"].ToString().Equals("M"))
            //        {
            //            chkReserve.IsChecked = true;
            //        }
            //        else
            //        {
            //            chkReserve.IsChecked = false;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void SetFormEnableChkMode()
        {
            //try
            //{
            //    if (string.IsNullOrEmpty(txtTrayID.Text))
            //    {
            //        chkMan.IsEnabled = true;
            //        chkReserve.IsEnabled = false;
            //    }
            //    else
            //    {
            //        chkMan.IsEnabled = false;
            //        chkReserve.IsEnabled = true;
            //    }

            //    //현재 상태가 수동이면
            //    if (chkMan.IsChecked == true)
            //    {
            //        chkMan.IsEnabled = false;
            //        SetFormVisiableMaint(true);
            //        SetFormVisiableRepair(false);
            //    }

            //    if (chkReserve.IsChecked == true)
            //    {
            //        chkReserve.IsEnabled = false;
            //        SetFormVisiableMaint(true);
            //        SetFormVisiableRepair(false);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void GetLoadBoxMaint()
        {
            //try
            //{
            //    // 초기화 먼저 진행
            //    txtMaintUser.Text = LoginInfo.USERNAME;
            //    txtMaintDesc.IsReadOnly = false;
            //    txtMaintDesc.Text = string.Empty;
            //    btnMaintSave.Visibility = Visibility.Visible;
            //    btnMaintSave.IsEnabled = false;
            //    btnMaintRepair.Visibility = Visibility.Visible;
            //    btnMaintRepair.IsEnabled = false;
            //    cboMaintCd.SelectedIndex = 0;
            //    cboMaintCd.IsEnabled = true;

            //    //데이터 SELECT
            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "RQSTDT";
            //    dtRqst.Columns.Add("EQPTID", typeof(string));
            //    dtRqst.Columns.Add("MNT_FLAG", typeof(string));
            //    dtRqst.Columns.Add("MDF_END_TIME", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["EQPTID"] = _sEqptID;
            //    dr["MNT_FLAG"] = "C";
            //    dr["MDF_END_TIME"] = "99990101000000";
            //    dtRqst.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TH_EQP_MAINT_MB", "RQSTDT", "RSLTDT", dtRqst);

            //    //데이터 존재시
            //    if (dtRslt.Rows.Count > 0)
            //    {
            //        btnMaintSave.IsEnabled = false;
            //        if (FrameOperation.AUTHORITY.Equals("W")) btnMaintRepair.IsEnabled = true;
            //        for (int iRow = 0; iRow < dtRslt.Rows.Count; iRow++)
            //        {
            //            txtMaintUser.Text = dtRslt.Rows[iRow]["SHFT_USER_NAME"].ToString();
            //            txtMaintDesc.Text = dtRslt.Rows[iRow]["MAINT_CNTT"].ToString();

            //            sMaintStartTime = dtRslt.Rows[iRow]["ACTDTTM"].ToString();

            //            if (dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Length > 1)
            //            {
            //                cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 1);
            //                cboMaintType.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 2);
            //            }
            //            else
            //            {
            //                cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["MAINT_CODE"].ToString().Substring(0, 1);
            //            }

            //            txtMaintUser.IsReadOnly = true;
            //            cboMaintCd.IsEnabled = false;
            //            cboMaintType.IsEnabled = false;
            //            txtMaintDesc.IsReadOnly = true;
            //        }

            //        //박스 부동사유 정보가 있으면 수리이력을 입력하도록 한다.
            //        SetFormVisiableRepair(true);
            //    }
            //    else
            //    {
            //        //부동사유 데이터가 없으면 수리입력 불가능 하도록 변경.
            //        if (FrameOperation.AUTHORITY.Equals("W")) btnMaintSave.IsEnabled = true;
            //        btnMaintRepair.IsEnabled = false;
            //        SetFormVisiableRepair(false);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void SetEqpModeEnable(string parmMode, DataTable sDt)
        {
            //try
            //{
            //    EqpMode = false;

            //    //박스가 수동모드면
            //    if (chkMan.IsChecked == true)
            //    {
            //        DataSet ds = new DataSet();
            //        DataTable dt = ds.Tables.Add("INDATA");
            //        dt.Columns.Add("EQPTID", typeof(string));
            //        dt.Columns.Add("CHECK", typeof(string));
            //        dt.Columns.Add("IFMODE", typeof(string));
            //        dt.Columns.Add("USERID", typeof(string));

            //        DataRow dr = dt.NewRow();
            //        dr["EQPTID"] = _sEqptID;
            //        dr["CHECK"] = parmMode;
            //        dr["IFMODE"] = "OFF";
            //        dr["USERID"] = LoginInfo.USERID;
            //        dt.Rows.Add(dr);

            //        new ClientProxy().ExecuteService_Multi("BR_SET_EQP_STATUS_UPPER_RUN_MODE_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
            //        {
            //            try
            //            {
            //                if (bizException != null)
            //                {
            //                    Util.MessageException(bizException);
            //                    return;
            //                }

            //                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
            //                    EqpMode = false;
            //                else
            //                    EqpMode = true;

            //                //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
            //                if (EqpMode)
            //                {
            //                    ShowLoadingIndicator();
            //                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_FORMEQPT_MAINT_HIST_MB", "RQSTDT", "RSLTDT", sDt);
            //                    Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
            //                    Close();
            //                }

            //            }
            //            catch (Exception ex)
            //            {
            //                Util.MessageException(ex);
            //            }
            //            finally
            //            {
            //                HiddenLoadingIndicator();
            //            }
            //        }, ds);
            //    }

            //    //박스가 수동예약 모드면
            //    if (chkReserve.IsChecked == true)
            //    {
            //        DataSet ds = new DataSet();
            //        DataTable dt = ds.Tables.Add("INDATA");
            //        dt.Columns.Add("EQPTID", typeof(string));
            //        dt.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
            //        dt.Columns.Add("IFMODE", typeof(string));
            //        dt.Columns.Add("USERID", typeof(string));

            //        DataRow dr = dt.NewRow();
            //        dr["EQPTID"] = _sEqptID;
            //        dr["EQPT_CTRL_STAT_CODE"] = parmMode.Equals("Y") ? "M" : string.Empty;
            //        dr["IFMODE"] = "OFF";
            //        dr["USERID"] = LoginInfo.USERID;
            //        dt.Rows.Add(dr);

            //        new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
            //        {
            //            try
            //            {
            //                if (bizException != null)
            //                {
            //                    Util.MessageException(bizException);
            //                    return;
            //                }

            //                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
            //                    EqpMode = true;
            //                else  
            //                    EqpMode = false;
            //                GetBoxReserve();

            //                //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
            //                if (EqpMode)
            //                {
            //                    ShowLoadingIndicator();
            //                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_FORMEQPT_MAINT_HIST_MB", "RQSTDT", "RSLTDT", sDt);
            //                    Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
            //                    Close();
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.MessageException(ex);
            //            }
            //            finally
            //            {
            //                HiddenLoadingIndicator();
            //            }
            //        }, ds);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
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

    }
}
