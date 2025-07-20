/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_001_01 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        public FCS001_001_01()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tmps = C1WindowExtension.GetParameters(this);

            //if (tmps != null && tmps.Length >= 1)
            //{
            //    _reqNo = Util.NVC(tmps[0]);
            //    _reqRslt = Util.NVC(tmps[1]);
            //}
            //SetRead();
        }

        #endregion

        #region Mehod
        #region [조회]
        //public void SetRead()
        //{
        //    try
        //    {
        //        DataSet inData = new DataSet();
        //        DataTable dtRqst = inData.Tables.Add("INDATA");

        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("REQ_NO", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["REQ_NO"] = _reqNo;

        //        dtRqst.Rows.Add(dr);

        //        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST_BOX", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

        //        Util.gridClear(dgRequest);
        //        dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

        //        Util.gridClear(dgGrator);
        //        dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

        //        txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();


        //        if (_reqRslt.Equals("DEL")) {
        //            grApp.Visibility = Visibility.Collapsed;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}
        #endregion
        #endregion

        #region STM_111
        //#region [Declaration & Constructor]
        //private string _sRow;
        //private string _sCol;
        //private string _sStg;
        //private string _sEqpID;
        //private string _sLaneID;
        //private string sMaintStartTime;
        //private Boolean EqpMode = false;

        //public string ROW
        //{
        //    set { this._sRow = value; }
        //}

        //public string COL
        //{
        //    set { this._sCol = value; }
        //}

        //public string STG
        //{
        //    set { this._sStg = value; }
        //}

        //public string EQP
        //{
        //    set { this._sEqpID = value; }
        //}

        //public string LANEID
        //{
        //    set { this._sLaneID = value; }
        //}
        //#endregion

        //#region [Initialize]
        //public STM_111()
        //{
        //    InitializeComponent();
        //}

        //private void STM_111_Load(object sender, EventArgs e)
        //{
        //    //Combo Setting
        //    InitCombo();

        //    SetFormVisiableMaint(false);
        //    SetFormVisiableRepair(false);

        //    GetLoadEqpInfo();           // 설비정보 로딩
        //    GetBoxReserve();            // 수동모드 예약&취소 판단
        //    SetFormEnableChkMode();     // TRAY_ID 여부에 따라 수동모드 && 수동모드 예약 판단

        //    //Control Setting
        //    InitControl();
        //}

        ///// <summary>
        ///// Setting Combo Items
        ///// </summary>
        //private void InitCombo()
        //{
        //    string[] sFilter = { "EMC" };
        //    ComboBox[] cboMaintCdChild = { cboMaintType };
        //    ComCombo.SetCombo(cboMaintCd, ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");
        //    cboMaintCd.SelectedIndexChanged += CboMaintCd_SelectedIndexChanged;
        //}

        ///// <summary>
        ///// Control Setting
        ///// </summary>
        //private void InitControl()
        //{
        //    dtpRepairDate.Value = DateTime.Now;
        //    dtpRepairDate.MinDate = DateTime.Now;
        //}
        //#endregion

        //#region [Method]
        ///// <summary>
        ///// 설비정보 로딩
        ///// </summary>
        //public void GetLoadEqpInfo()
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("LANE_ID", typeof(string));
        //        dtRqst.Columns.Add("EQP_ID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANE_ID"] = _sLaneID;
        //        dr["EQP_ID"] = _sEqpID;
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new BizUtil().ExecuteServiceSync("SEL_FORMATION_LOAD_EQPT", "RQSTDT", "RSLTDT", dtRqst);

        //        if (dtRslt.Rows.Count > 0)
        //        {
        //            DataRow drItem = dtRslt.Rows[0];

        //            Util.SetTextBoxReadOnly(txtPos, drItem["LANE_ID"].ToString() + "-" + drItem["EQP_COL_LOC"].ToString() + "-" + drItem["EQP_STG_LOC"].ToString());

        //            if (!string.IsNullOrEmpty(drItem["TRAY_ID"].ToString()))
        //                Util.SetTextBoxReadOnly(txtTrayID, drItem["TRAY_ID"].ToString());

        //            if (dtRslt.Rows.Count == 2)
        //                Util.SetTextBoxReadOnly(txtTrayID2, dtRslt.Rows[1]["TRAY_ID"].ToString());

        //            if (!string.IsNullOrEmpty(drItem["TIME"].ToString()))
        //                txtTime.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss}", (DateTime)drItem["TIME"]);

        //            if (!string.IsNullOrEmpty(drItem["TROUBLE"].ToString()))
        //                Util.SetTextBoxReadOnly(txtTrouble, drItem["TROUBLE"].ToString());

        //            Util.SetTextBoxReadOnly(txtStatus, drItem["STATUS_NAME"].ToString());
        //            Util.SetTextBoxReadOnly(txtOpStatus, drItem["OPSTATUS_NAME"].ToString());

        //            if (drItem["UPPER_RUN_MODE_CD"].ToString().Equals("M") || drItem["STATUS"].ToString().Equals("2"))
        //                chkMan.Checked = true;

        //            if (drItem["RUN_MODE_CD"].ToString().Equals("M"))
        //                chkMan.Checked = true;

        //            Text += " : " + drItem["EQP_COL_LOC"].ToString() + "-" + drItem["EQP_STG_LOC"].ToString();

        //            #region 버튼 Enable 변경 -2016 04 08 jeong hyeon sik
        //            /*
        //            STATUS
        //                1       COMM_STATUS_NOR_YN = 'N'    - 통신이상
        //                2       RUN_MODE_CD = 'M'           - Maintenance
        //                3       UPPER_RUN_MODE_CD = 'M'     - Manual
        //                4       EQP_STATUS_CD = 'T'         - Trouble
        //                5       EQP_STATUS_CD = 'R'         - 작업중
        //                6       EQP_STATUS_CD = 'I'         - 대기중
        //                7       EQP_STATUS_CD = 'S'         - 일시정지
        //                8       EQP_STATUS_CD = 'P'         - POWER ON
        //                9       EQP_STATUS_CD = 'O'         - POWER OFF

        //            */
        //            if (drItem["STATUS"].ToString().Equals("4") || drItem["STATUS"].ToString().Equals("7"))   //4(T):Trouble, 7(S):일시정지
        //            {
        //                btnPause.Enabled = false;       //일시정지 버튼                    
        //                btnStop.Enabled = false;        //현재작업종료 버튼
        //                if (MENUAUTH.Equals("W"))
        //                {
        //                    btnInit.Enabled = true;         //설비상태초기화 버튼
        //                    btnItin.Enabled = true;         //IT 정보초기화 버튼
        //                    btnUnloadReq.Enabled = true;    //강제출고요청 버튼
        //                }

        //                //tray 종료상태일경우 재시작버튼 Enable true
        //                if (drItem["TRAY_OP_STATUS_CD"].ToString().Equals("E"))
        //                {
        //                    if (MENUAUTH.Equals("W"))
        //                    {
        //                        btnRestart.Enabled = true;      //재시작 버튼
        //                        btnOutStatus.Enabled = true;    //출고상태변경 버튼
        //                    }
        //                }
        //                else
        //                {
        //                    btnRestart.Enabled = false;      //재시작 버튼
        //                    btnOutStatus.Enabled = false;    //출고상태변경 버튼
        //                }

        //                //Tray Run 상태일경우 연속시작버튼 Enable true
        //                if (drItem["TRAY_OP_STATUS_CD"].ToString().Equals("S"))
        //                {
        //                    if (MENUAUTH.Equals("W"))
        //                    {
        //                        btnResume.Enabled = true;       //연속시작 버튼
        //                    }
        //                }
        //                else
        //                {
        //                    btnResume.Enabled = false;      //연속시작 버튼
        //                }
        //            }
        //            else if (drItem["STATUS"].ToString().Equals("5"))    //5(R):Run
        //            {
        //                btnRestart.Enabled = false;      //재시작 버튼   
        //                btnResume.Enabled = false;       //연속시작 버튼
        //                if (MENUAUTH.Equals("W"))
        //                {
        //                    btnPause.Enabled = true;         //일시정지 버튼 
        //                    btnStop.Enabled = true;          //현재작업종료 버튼
        //                    btnItin.Enabled = true;          //IT정보초기화 버튼
        //                }
        //                btnInit.Enabled = false;         //설비상태초기화 버튼
        //                btnUnloadReq.Enabled = false;    //강제출고요청 버튼
        //                btnOutStatus.Enabled = false;    //출고상태변경 버튼
        //            }
        //            else
        //            {
        //                btnRestart.Enabled = false;      //재시작 버튼
        //                btnPause.Enabled = false;        //일시정지 버튼    
        //                btnResume.Enabled = false;       //연속시작 버튼
        //                btnStop.Enabled = false;         //현재작업종료 버튼
        //                btnInit.Enabled = false;         //설비상태초기화 버튼
        //                if (MENUAUTH.Equals("W"))
        //                    btnItin.Enabled = true;          //IT정보초기화 버튼
        //                btnUnloadReq.Enabled = false;    //강제출고요청 버튼
        //                btnOutStatus.Enabled = false;    //출고상태변경 버튼
        //            }

        //            //샘플 생산 때문에 임시로 막음 (170809)
        //            //btnRestart.Enabled = true;
        //            #endregion

        //            switch (drItem["EQP_CTR_STATUS_CD"].ToString())
        //            {
        //                case "Q":
        //                    btnRestart.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "P":
        //                    btnPause.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "S":
        //                    btnResume.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "E":
        //                    btnStop.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "I":
        //                    btnInit.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "F":
        //                    btnUnloadReq.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                case "L":
        //                    btnOutStatus.Enabled = false;
        //                    Text += "(" + drItem["EQP_CTR_STATUS_CD_NAME"].ToString() + ")";
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        ///// <summary>
        ///// Box 수동 예약/취소에 대한 기능
        ///// </summary>
        //private void GetBoxReserve()
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("EQP_ID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["EQP_ID"] = _sEqpID;
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new BizUtil().ExecuteServiceSync("SEL_CTR_STATUS", "RQSTDT", "RSLTDT", dtRqst);

        //        if (dtRslt.Rows.Count > 0)
        //        {
        //            if (dtRslt.Rows[0]["EQP_CTR_STATUS_CD"].ToString().Equals("M"))
        //                chkReserve.Checked = true;
        //            else
        //                chkReserve.Checked = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void SetFormEnableChkMode()
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(txtTrayID.Text))
        //        {
        //            chkMan.Enabled = true;
        //            chkReserve.Enabled = false;
        //        }
        //        else
        //        {
        //            chkMan.Enabled = false;
        //            chkReserve.Enabled = true;
        //        }

        //        //현재 상태가 수동이면
        //        if (chkMan.Checked)
        //        {
        //            chkMan.Enabled = false;
        //            SetFormVisiableMaint(true);
        //            SetFormVisiableRepair(false);
        //        }

        //        if (chkReserve.Checked)
        //        {
        //            chkReserve.Enabled = false;
        //            SetFormVisiableMaint(true);
        //            SetFormVisiableRepair(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void GetLoadBoxMaint()
        //{
        //    try
        //    {
        //        // 초기화 먼저 진행
        //        Util.SetTextBoxReadOnly(txtMaintUser, LoginInfo.USERNAME);
        //        txtMaintDesc.ReadOnly = false;
        //        Util.SetTextBoxReadOnly(txtMaintDesc, string.Empty);
        //        btnMaintSave.Visible = true;
        //        btnMaintSave.Enabled = false;
        //        btnMaintRepair.Visible = true;
        //        btnMaintRepair.Enabled = false;
        //        cboMaintCd.SelectedIndex = 0;
        //        cboMaintCd.Enabled = true;

        //        //데이터 SELECT
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("EQP_ID", typeof(string));
        //        dtRqst.Columns.Add("MNT_FLAG", typeof(string));
        //        dtRqst.Columns.Add("MDF_END_TIME", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["EQP_ID"] = _sEqpID;
        //        dr["MNT_FLAG"] = "C";
        //        dr["MDF_END_TIME"] = "99990101000000";
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new BizUtil().ExecuteServiceSync("SEL_TH_EQP_MAINT", "RQSTDT", "RSLTDT", dtRqst);

        //        //데이터 존재시
        //        if (dtRslt.Rows.Count > 0)
        //        {
        //            btnMaintSave.Enabled = false;
        //            if (MENUAUTH.Equals("W")) btnMaintRepair.Enabled = true;
        //            for (int iRow = 0; iRow < dtRslt.Rows.Count; iRow++)
        //            {
        //                Util.SetTextBoxReadOnly(txtMaintUser, dtRslt.Rows[iRow]["SHIFT_USER_NAME"].ToString());
        //                Util.SetTextBoxReadOnly(txtMaintDesc, dtRslt.Rows[iRow]["MNT_CONTENTS"].ToString());
        //                sMaintStartTime = dtRslt.Rows[iRow]["MDF_START_TIME"].ToString();

        //                /* 2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가 
        //                    * 부동유형 추가에 따른 변경, cmbMaintType추가
        //                    * */
        //                //iComboIndex = Convert.ToInt32(dsOutData.Tables["OUT_DATA"].Rows[iRow]["EQP_MNT_CD"]);
        //                //cmbMaintCD.SelectedIndex = iComboIndex;
        //                if (dtRslt.Rows[iRow]["EQP_MNT_CD"].ToString().Length > 1)
        //                {
        //                    cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["EQP_MNT_CD"].ToString().Substring(0, 1);
        //                    cboMaintType.SelectedValue = dtRslt.Rows[iRow]["EQP_MNT_CD"].ToString().Substring(1, 1);

        //                }
        //                else
        //                {
        //                    cboMaintCd.SelectedValue = dtRslt.Rows[iRow]["EQP_MNT_CD"].ToString().Substring(0, 1);
        //                }

        //                txtMaintUser.ReadOnly = true;
        //                cboMaintCd.Enabled = false;
        //                cboMaintType.Enabled = false;
        //                txtMaintDesc.ReadOnly = true;
        //            }

        //            //박스 부동사유 정보가 있으면 수리이력을 입력하도록 한다.
        //            SetFormVisiableRepair(true);
        //        }
        //        else
        //        {
        //            //부동사유 데이터가 없으면 수리입력 불가능 하도록 변경.
        //            if (MENUAUTH.Equals("W")) btnMaintSave.Enabled = true;
        //            btnMaintRepair.Enabled = false;
        //            SetFormVisiableRepair(false);
        //        }
        //    }
        //    catch (Exception exSys)
        //    {
        //        Util.ExceptionMsg(exSys);
        //    }
        //}

        //private void SetFormVisiableMaint(Boolean parmValue)
        //{
        //    try
        //    {
        //        if (parmValue == true)
        //        {
        //            tlpMaint.Visible = true;
        //            if (!tlpRepair.Visible)
        //                Size = new Size(975, 447);
        //            else
        //                Size = new Size(975, 598);
        //        }
        //        else
        //        {
        //            tlpMaint.Visible = false;
        //            if (!tlpRepair.Visible)
        //                Size = new Size(617, 447);
        //            else
        //                Size = new Size(975, 598);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void SetFormVisiableRepair(Boolean parmValue)
        //{
        //    try
        //    {
        //        if (parmValue == true)
        //        {
        //            tlpRepair.Visible = true;
        //            Util.SetTextBoxReadOnly(txtRepairUser, LoginInfo.USERNAME);
        //            Size = new Size(975, 598);
        //        }
        //        else
        //        {
        //            tlpRepair.Visible = false;
        //            if (!tlpMaint.Visible)
        //                Size = new Size(617, 447);
        //            else
        //                Size = new Size(975, 447);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void SetEqpModeEnable(string parmMode)
        //{
        //    try
        //    {
        //        EqpMode = false;

        //        //박스가 수동모드면
        //        if (chkMan.Checked == true)
        //        {
        //            DataTable dtRqst = new DataTable();
        //            dtRqst.TableName = "RQSTDT";
        //            dtRqst.Columns.Add("EQP_ID", typeof(string));
        //            dtRqst.Columns.Add("CHECK", typeof(string));

        //            DataRow dr = dtRqst.NewRow();
        //            dr["EQP_ID"] = _sEqpID;
        //            dr["CHECK"] = parmMode;
        //            dtRqst.Rows.Add(dr);

        //            DataTable dtRslt = new BizUtil().ExecuteServiceSync("SET_EQP_STATUS_UPPER_RUN_MODE", "INDATA", "OUTDATA", dtRqst, bRegBizLog: true);

        //            if (!dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
        //                EqpMode = true;
        //            else
        //                EqpMode = false;
        //        }

        //        //박스가 수동예약 모드면
        //        if (chkReserve.Checked == true)
        //        {
        //            DataTable dtRqst = new DataTable();
        //            dtRqst.TableName = "RQSTDT";
        //            dtRqst.Columns.Add("EQP_ID", typeof(string));
        //            dtRqst.Columns.Add("EQP_CTR_STATUS_CD", typeof(string));

        //            DataRow dr = dtRqst.NewRow();
        //            dr["EQP_ID"] = _sEqpID;
        //            dr["EQP_CTR_STATUS_CD"] = parmMode.Equals("Y") ? "M" : string.Empty;
        //            dtRqst.Rows.Add(dr);

        //            DataTable dtRslt = new BizUtil().ExecuteServiceSync("SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", dtRqst, bRegUILog: true, bRegBizLog: true);

        //            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("1"))
        //                EqpMode = true;
        //            else
        //                EqpMode = false;

        //            GetBoxReserve();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}
        //#endregion

        //#region [Event]
        //private void CboMaintCd_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    string[] sFilter = { "ET" + cboMaintCd.SelectedValue.ToString() };
        //    ComCombo.SetCombo(cboMaintType, ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");
        //    if (cboMaintType.Items.Count <= 1)
        //        cboMaintType.Enabled = false;
        //    else
        //        cboMaintType.Enabled = true;
        //}

        //private void chkMan_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chkMan.Checked)
        //    {
        //        SetFormVisiableMaint(true);
        //        SetFormVisiableRepair(false);
        //        GetLoadBoxMaint();
        //    }
        //    else
        //    {
        //        SetFormVisiableMaint(false);
        //        SetFormVisiableRepair(false);
        //    }
        //}

        //private void chkReserve_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chkReserve.Checked)
        //    {
        //        SetFormVisiableMaint(true);
        //        SetFormVisiableRepair(false);
        //        GetLoadBoxMaint();
        //    }
        //    else
        //    {
        //        SetFormVisiableMaint(false);
        //        SetFormVisiableRepair(false);
        //    }
        //}

        //private void btnMaintSave_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //저장하시겠습니까?
        //        if (Util.QuestionMsg("ME_0214") != DialogResult.Yes) return;

        //        //수동모드 || 수동모드 예약 체크여부
        //        if (chkMan.Checked == false && chkReserve.Checked == false)
        //        {
        //            Util.AlertMsg("ME_0157");  //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
        //            return;
        //        }

        //        //수동모드 && 수동모드 예약 체크여부
        //        if (chkMan.Checked == true && chkReserve.Checked == true)
        //        {
        //            Util.AlertMsg("ME_0173");  //수동모드와 수동모드 예약을 동시에 적용할 수 없습니다.
        //            return;
        //        }

        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("EQP_ID", typeof(string));
        //        dtRqst.Columns.Add("MNT_TIME", typeof(string));
        //        dtRqst.Columns.Add("EQP_STATUS_CD", typeof(string));
        //        dtRqst.Columns.Add("SHIFT_USER_NAME", typeof(string));
        //        dtRqst.Columns.Add("EQP_MNT_CD", typeof(string));
        //        dtRqst.Columns.Add("MNT_CONTENTS", typeof(string));
        //        dtRqst.Columns.Add("MDF_ID", typeof(string));
        //        dtRqst.Columns.Add("MNT_FLAG", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["EQP_ID"] = _sEqpID;
        //        dr["MNT_TIME"] = "99990101000000";
        //        dr["EQP_STATUS_CD"] = "T";
        //        dr["SHIFT_USER_NAME"] = Util.GetCondition(txtMaintUser, sMsg: "ME_0123");  //등록자를 입력하세요.
        //        if (string.IsNullOrEmpty(dr["SHIFT_USER_NAME"].ToString())) return;
        //        dr["EQP_MNT_CD"] = Util.GetCondition(cboMaintCd, sMsg: "ME_0144");  //부동유형을 선택하세요.
        //        if (string.IsNullOrEmpty(dr["EQP_MNT_CD"].ToString())) return;

        //        if (cboMaintType.Enabled)
        //        {
        //            if (string.IsNullOrEmpty(Util.GetCondition(cboMaintType, sMsg: "ME_0143")))  //부동사유를 선택하세요.
        //                return;
        //            else
        //                dr["EQP_MNT_CD"] = dr["EQP_MNT_CD"].ToString() + Util.GetCondition(cboMaintType);
        //        }

        //        dr["MNT_CONTENTS"] = Util.GetCondition(txtMaintDesc, sMsg: "ME_0142");  //부동내용을 입력하세요.
        //        if (string.IsNullOrEmpty(dr["MNT_CONTENTS"].ToString())) return;

        //        dr["MDF_ID"] = LoginInfo.USERID;
        //        dr["MNT_FLAG"] = "C";
        //        dtRqst.Rows.Add(dr);
        //        SetEqpModeEnable("Y");

        //        //BOX에 대한 수동처리가 됐을경우에만 부동사용 저장 - 2014.12.08 정영진D
        //        if (EqpMode)
        //        {
        //            DataTable dtRslt = new BizUtil().ExecuteServiceSync("INS_TH_EQP_MAINT_SM", "RQSTDT", null, dtRqst, bRegBizLog: true);
        //            Util.ConfirmMsg("ME_0215");  //저장하였습니다.
        //            Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void btnRepairSave_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //저장하시겠습니까?
        //        if (Util.QuestionMsg("ME_0214") != DialogResult.Yes) return;


        //        //수동모드 || 수동모드 예약 체크여부
        //        if (chkMan.Checked == false && chkReserve.Checked == false)
        //        {
        //            Util.AlertMsg("ME_0157");  //상태가 수동모드 또는 수동모드 예약인 경우만 가능합니다.
        //            return;
        //        }

        //        //수동모드 && 수동모드 예약 체크여부
        //        if (chkMan.Checked == true && chkReserve.Checked == true)
        //        {
        //            Util.AlertMsg("ME_0173");  //수동모드와 수동모드 예약을 동시에 적용할 수 없습니다.
        //            return;
        //        }

        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("EQP_ID", typeof(string));
        //        dtRqst.Columns.Add("MNT_TIME", typeof(string));
        //        dtRqst.Columns.Add("EQP_STATUS_CD", typeof(string));
        //        dtRqst.Columns.Add("SHIFT_USER_NAME", typeof(string));
        //        dtRqst.Columns.Add("EQP_MNT_CD", typeof(string));
        //        dtRqst.Columns.Add("MNT_PART", typeof(string));
        //        dtRqst.Columns.Add("MNT_CONTENTS", typeof(string));
        //        dtRqst.Columns.Add("MDF_START_TIME", typeof(string));
        //        dtRqst.Columns.Add("MDF_ID", typeof(string));
        //        dtRqst.Columns.Add("MNT_FLAG", typeof(string));
        //        dtRqst.Columns.Add("CHECK", typeof(string));
        //        dtRqst.Columns.Add("BOX_MAINT_MODE", typeof(string));
        //        dtRqst.Columns.Add("BOX_MAINT_RES", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["EQP_ID"] = _sEqpID;
        //        dr["MNT_TIME"] = Util.GetCondition(dtpRepairDate);
        //        dr["EQP_STATUS_CD"] = "T";
        //        dr["SHIFT_USER_NAME"] = Util.GetCondition(txtRepairUser, sMsg: "ME_0180");  //수리자를 입력해주세요.
        //        if (string.IsNullOrEmpty(dr["SHIFT_USER_NAME"].ToString())) return;

        //        dr["EQP_MNT_CD"] = Util.GetCondition(cboMaintCd) + Util.GetCondition(cboMaintType);
        //        dr["MNT_PART"] = Util.GetCondition(txtRepairPart, sMsg: "ME_0179");  //수리부품을 입력해주세요.
        //        if (string.IsNullOrEmpty(dr["MNT_PART"].ToString())) return;

        //        dr["MNT_CONTENTS"] = Util.GetCondition(txtRepairDesc, sMsg: "ME_0178");  //수리내용을 입력해주세요.
        //        if (string.IsNullOrEmpty(dr["MNT_CONTENTS"].ToString())) return;

        //        dr["MDF_START_TIME"] = sMaintStartTime;
        //        dr["MDF_ID"] = LoginInfo.USERID;
        //        dr["MNT_FLAG"] = "R";
        //        dr["CHECK"] = "N";
        //        dr["BOX_MAINT_MODE"] = chkMan.Checked ? "Y" : "N";
        //        dr["BOX_MAINT_RES"] = chkReserve.Checked ? "Y" : "N";
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new BizUtil().ExecuteServiceSync("SET_EQP_STATUS_FROM_MAINT", "INDATA", null, dtRqst, bRegBizLog: true);
        //        Util.ConfirmMsg("ME_0215");  //저장하였습니다.
        //        Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.ExceptionMsg(ex);
        //    }
        //}

        //private void btn_Click(object sender, EventArgs e)
        //{
        //    Button bSend = sender as Button;

        //    string sMsg = string.Empty;
        //    string sCtrStatus = string.Empty;

        //    switch (bSend.Name)
        //    {
        //        case "btnRestart":
        //            //샘플 생산 때문에 임시로 막음 (170809)
        //            //if (txtTrayID.Text.ToString() == string.Empty)
        //            //{
        //            //    Util.AlertMsg("ME_0170");  //설비내에 Tray가 존재하지 않습니다.
        //            //    return;
        //            //}
        //            sMsg = "ME_0269";  //재시작 하시겠습니까?
        //            sCtrStatus = "Q";
        //            break;
        //        case "btnPause":
        //            if (txtTrayID.Text.ToString() == string.Empty)
        //            {
        //                Util.AlertMsg("ME_0170");  //설비내에 Tray가 존재하지 않습니다.
        //                return;
        //            }
        //            sMsg = "ME_0270";  //일시정지 하시겠습니까?
        //            sCtrStatus = "P";
        //            break;
        //        case "btnResume":
        //            if (txtTrayID.Text.ToString() == string.Empty)
        //            {
        //                Util.AlertMsg("ME_0170");  //설비내에 Tray가 존재하지 않습니다.
        //                return;
        //            }
        //            sMsg = "ME_0271";  //연속시작 하시겠습니까?
        //            sCtrStatus = "S";
        //            break;
        //        case "btnStop":
        //            sMsg = "ME_0272";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. 현재작업 종료 하시겠습니까?
        //            sCtrStatus = "E";
        //            break;
        //        case "btnInit":
        //            sMsg = "ME_0273";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. 설비상태 초기화 하시겠습니까?
        //            sCtrStatus = "I";
        //            break;
        //        case "btnItin":
        //            sMsg = "ME_0274";  //설비내에 트레이가 존재 하는지 확인하신 후 작업하십시요. IT 정보 초기화 하시겠습니까?
        //            sCtrStatus = null;
        //            break;
        //        case "btnUnloadReq":
        //            if (txtTrayID.Text.ToString() == string.Empty)
        //            {
        //                Util.AlertMsg("ME_0170");  //설비내에 Tray가 존재하지 않습니다.
        //                return;
        //            }
        //            sMsg = "ME_0094";  //강제출고요청을 하시겠습니까?
        //            sCtrStatus = "F";
        //            break;
        //        case "btnOutStatus":
        //            sMsg = "ME_0276";  //출고 상태로 변경 하시겠습니까?
        //            sCtrStatus = "L";
        //            break;

        //    }

        //    if (Util.QuestionMsg(sMsg) != DialogResult.Yes) return;

        //    DataTable dtRqst = new DataTable();
        //    dtRqst.TableName = "INDATA";
        //    dtRqst.Columns.Add("EQP_ID", typeof(string));
        //    dtRqst.Columns.Add("EQP_CTR_STATUS_CD", typeof(string));

        //    DataRow dr = dtRqst.NewRow();
        //    dr["EQP_ID"] = _sEqpID;
        //    dr["EQP_CTR_STATUS_CD"] = sCtrStatus;
        //    dtRqst.Rows.Add(dr);

        //    DataSet dsRqst = new DataSet();
        //    dsRqst.Tables.Add(dtRqst);

        //    DataSet dsRslt = new BizUtil().ExecuteServiceSync_Multi("SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", dsRqst, bRegUILog: true, bRegBizLog: true);

        //    if (dsRslt.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("1"))
        //    {
        //        Util.ConfirmMsg("ME_0186");  //요청 완료하였습니다.
        //        STM_111_Load(null, null);
        //    }
        //    else
        //        Util.AlertMsg("ME_0185");  //요청 실패하였습니다.
        //}

        //private void btnMaintRepair_Click(object sender, EventArgs e)
        //{
        //    if (tlpRepair.Visible)
        //        SetFormVisiableRepair(false);
        //    else
        //        SetFormVisiableRepair(true);
        //}
        //#endregion
        #endregion

    }
}
