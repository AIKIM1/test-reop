/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.31  조영대 : Validation 추가
  2023.04.04  김수용 : 보정dOCV 항목 재전송 로직 추가
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_021_SEL_JUDG_OP : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sTrayNo = string.Empty;
        private string sRouteId = string.Empty;
        private int iTry;  //200408 KJE : 용량 선별화 판정 추가
        DispatcherTimer timer = new DispatcherTimer();    //객체생성

        public string TrayNo
        {
            set { this.sTrayNo = value; }
        }

        public string RouteId
        {
            set { this.sRouteId = value; }
        }
        public FCS001_021_SEL_JUDG_OP()
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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //다른 화면에서 넘어온 경우
                object[] parameters = C1WindowExtension.GetParameters(this);
                if (parameters != null && parameters.Length >= 1)
                {
                    sTrayNo = Util.NVC(parameters[0]);
                    sRouteId = Util.NVC(parameters[1]);
                }
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter = { sRouteId };
            _combo.SetCombo(cboJudge, CommonCombo_Form.ComboStatus.SELECT, sCase: "JUDGE_OP", sFilter: sFilter);
        }

        #endregion

        #region [Method]
        #endregion

        #region [Event]

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            cboJudge.ClearValidation();
            if (cboJudge.GetBindValue() == null)
            {
                cboJudge.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0248"));
                return;
            }

            Util.MessageConfirm("FM_ME_0176", (result) => //수동판정을 등록하시겠습니까?
            {
                if (result == MessageBoxResult.Cancel) return;
                else
                {
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("LOTID", typeof(string));          //TRAY_NO
                        dtRqst.Columns.Add("JUDG_PROCID", typeof(string));     //JUDG_OP_ID
                        dtRqst.Columns.Add("USERID", typeof(string));         //MDF_ID
                        dtRqst.Columns.Add("FITTED_MODE", typeof(string));    //FITTED_MODE
                        dtRqst.Columns.Add("NOT_INIT_GRADE_FLAG", typeof(string)); //NOT_INIT_GRADE
                        dtRqst.Columns.Add("MENUID", typeof(string));
                        dtRqst.Columns.Add("USER_PC_IP", typeof(string));
                        dtRqst.Columns.Add("PC_NAME", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = sTrayNo;
                        dr["JUDG_PROCID"] = Util.GetCondition(cboJudge, sMsg: "FM_ME_0248");  //판정공정을 선택해주세요.
                        if (string.IsNullOrEmpty(dr["JUDG_PROCID"].ToString())) return;
                        dr["USERID"] = LoginInfo.USERID;

                        int FittedMode = 0;
                        if ((bool)chkFitted.IsChecked) FittedMode = FittedMode + 1;
                        if ((bool)chkFittedCapa.IsChecked) FittedMode = FittedMode + 2;
                        dr["FITTED_MODE"] = FittedMode;
                        dr["NOT_INIT_GRADE_FLAG"] = chkNotJudgCell.IsChecked == true ? "Y" : "N";
                        dr["MENUID"] = LoginInfo.CFG_MENUID;
                        dr["USER_PC_IP"] = LoginInfo.USER_IP;
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CELL_JUDGMENT_MANUAL", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            DialogResult = MessageBoxResult.Yes;
                            Util.MessageInfo("FM_ME_0175");  //수동판정 등록을 완료하였습니다.
                        }
                        else
                        {
                            DialogResult = MessageBoxResult.No;
                            Util.MessageInfo("FM_ME_0174");  //수동판정 등록에 실패하였습니다.
                        }

                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            });

        }
        /// <summary>
        /// 200408 KJE : 용량 선별화 판정 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private void btnSASReSend_Click(object sender, RoutedEventArgs e)
        {
            cboJudge.ClearValidation();
            if (cboJudge.GetBindValue() == null)
            {
                cboJudge.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0248"));
                return;
            }

            Util.MessageConfirm("FM_ME_0362", (result) =>  //ML 재판정을 위하여 Tray 정보를 재전송 하시겠습니까? 
            {
                if (result == MessageBoxResult.Cancel)
                    return;
                else
                {
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("LOTID", typeof(string)); //TRAY_NO
                        dtRqst.Columns.Add("PROCID", typeof(string)); //OP_ID
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = sTrayNo; //TRAY_NO
                        dr["PROCID"] = Util.GetCondition(cboJudge, sMsg: "FM_ME_0248");  //판정공정을 선택해주세요.
                        if (string.IsNullOrEmpty(dr["PROCID"].ToString())) return;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_FITTED_SAS_TRNF_MANUAL", "INDATA", "OUTDATA", dtRqst);


                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            timer.Interval = TimeSpan.FromMilliseconds(1000);    //시간간격 설정
                            timer.Tick += new EventHandler(timer_Tick);          //이벤트 추가
                            timer.Start();                                       //타이머 시작. 종료는 timer.Stop(); 으로 한다
                            btnSASReSend.IsEnabled = false;
                            iTry = 1;
                            lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0038") + " ("
                            + ObjectDic.Instance.GetObjectName("시도횟수") + " : " + iTry.ToString() + ")";  //ML 재전송 요청완료, 응답을 기다리는 중 (시도횟수 : #)                     
                        }

                        //test
                        //DataTable dtRslt = new DataTable();

                        //timer.Interval = TimeSpan.FromMilliseconds(1000);    //시간간격 설정
                        //timer.Tick += new EventHandler(timer_Tick);          //이벤트 추가
                        //timer.Start();                                       //타이머 시작. 종료는 timer.Stop(); 으로 한다
                        //btnSASReSend.IsEnabled = false;
                        //iTry = 1;
                        //lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0038") + " ("
                        //+ ObjectDic.Instance.GetObjectName("시도횟수") + " : " + iTry.ToString() + ")";  //ML 재전송 요청완료, 응답을 기다리는 중 (시도횟수 : #)                     
                    }
                    catch (Exception ex)
                    { Util.MessageException(ex); }
                }
            }
            );
        }

        /// <summary>
        /// 200408 KJE : 용량 선별화 판정 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Stop();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string)); //TRAY_NO

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sTrayNo;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_FITTED_JUDG_RESULT_U", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    btnSASReSend.IsEnabled = true;
                    lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0039");  //ML 데이터 송신 받았습니다. 판정이 완료되었습니다.
                }
                else
                {
                    if (iTry < 20)
                    {
                        iTry++;
                        lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0038") + " ("
                        + ObjectDic.Instance.GetObjectName("시도횟수") + " : " + iTry.ToString() + ")";   //ML 재전송 요청완료, 응답을 기다리는 중 (시도횟수 : #)
                        timer.Start();
                    }
                    else
                    {
                        timer.Tick -= new EventHandler(timer_Tick);
                        btnSASReSend.IsEnabled = true;
                        lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0040");  //ML 응답없음
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        /// <summary>
        /// 20230315 suyong.kim dOCV2 항목 재전송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void btnDOCVReSend_Click(object sender, RoutedEventArgs e)
        {
            cboJudge.ClearValidation();

            Util.MessageConfirm("FM_ME_0473", (result) =>  //보정 DOCV 재판정을 위하여 Tray 정보를 재전송 하시겠습니까? 
            {
                if (result == MessageBoxResult.Cancel)
                    return;
                else
                {
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("LOTID", typeof(string)); //TRAY_NO
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = sTrayNo; //TRAY_NO
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_DOCV_TRNF_INFO_MANUAL", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                           lblReturnSAS.Text = ObjectDic.Instance.GetObjectName("UC_0043");  //DOCV 항목 재전송 요청완료.
                        }                  
                    }
                    catch (Exception ex)
                    { Util.MessageException(ex); }
                }
            }
            );
        }
    }
}
