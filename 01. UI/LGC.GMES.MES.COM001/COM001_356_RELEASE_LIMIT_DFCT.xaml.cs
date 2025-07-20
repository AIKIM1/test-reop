/*************************************************************************************
 Created Date : 2019.05.29
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.29  INS 김동일K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.ComponentModel;
using System.Threading;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// ASSY004_COM_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_356_RELEASE_LIMIT_DFCT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Eqptid       = string.Empty;
        private string _EqptName     = string.Empty;
        private string _Actdttm      = string.Empty;
        private string _AlarmCode    = string.Empty;
        private string _Alarmname    = string.Empty;
        private string _Seqno        = string.Empty;
        
        
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private bool bRls = false;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_356_RELEASE_LIMIT_DFCT()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            //CommonCombo _combo = new CommonCombo();

            //String[] sFilter = { "PROD_LOT_OPER_MODE" };
            //_combo.SetCombo(cboLotMode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            //if (cboLotMode.Items.Count > 0)
            //    cboLotMode.SelectedValue = "L"; // UI 는 비정규 모드.

            //String[] sFilter2 = { "IRREGL_PROD_LOT_TYPE_CODE" };
            //_combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");

            //// 생산 일자 Combo
            //String[] sFilter3 = { "DATE_TYPE" };
            //_combo.SetCombo(cboDay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            //if (cboDay != null && cboDay.Items != null && cboDay.Items.Count > 0)
            //    cboDay.SelectedIndex = 0;
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _Seqno          = Util.NVC(tmps[0]);
            _Eqptid         = Util.NVC(tmps[1]);
            _EqptName       = Util.NVC(tmps[2]);
            _Actdttm        = Util.NVC(tmps[3]);
            _AlarmCode      = Util.NVC(tmps[4]);
            _Alarmname      = Util.NVC(tmps[5]);                      

            txtActdttm.Text   = _Actdttm;
            txtAlarmcode.Text = _AlarmCode;
            txtAlarmName.Text = _Alarmname;
            txtEqptName.Text  = _EqptName;

        }

        private void btnRelease(object sender, RoutedEventArgs e)
        {

            if (!CanRelease())
                return;

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_EQPT_ALARM_LIMIT_RELEASE";

                DataTable InTable = new DataTable("INDATA");
                InTable.Columns.Add("SRCTYPE", typeof(string));
                InTable.Columns.Add("IFMODE", typeof(string));
                InTable.Columns.Add("EQPTID", typeof(string));
                InTable.Columns.Add("USERID", typeof(string));
                InTable.Columns.Add("EIOSTAT", typeof(string));
                InTable.Columns.Add("EQPT_ALARM_CODE", typeof(string));
                InTable.Columns.Add("LOTID", typeof(string));
                InTable.Columns.Add("EQPT_LOT_PROG_MODE", typeof(string));
                InTable.Columns.Add("EQPT_ALARM_SEQNO", typeof(string));
                InTable.Columns.Add("EQPT_ALARM_REL_USERID", typeof(string));
                InTable.Columns.Add("EQPT_ALARM_REL_NOTE", typeof(string));

                DataRow dr = InTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["EQPTID"] = _Eqptid;
                dr["USERID"] = LoginInfo.USERID;
                dr["EIOSTAT"] = "";
                dr["EQPT_ALARM_CODE"] = _AlarmCode;
                dr["LOTID"] = "";
                dr["EQPT_LOT_PROG_MODE"] = "";
                dr["EQPT_ALARM_SEQNO"] = _Seqno;
                dr["EQPT_ALARM_REL_USERID"] = txtUserName.Tag;
                dr["EQPT_ALARM_REL_NOTE"] = txtReleaseNote.Text;


                InTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, InTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    bRls = true;
                    this.DialogResult = MessageBoxResult.OK;
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bRls)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }

        private void txtUserName_KeyDown(object  sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                popupUser();
            }
        }

        #endregion

        #region Mehod

        #region [BizRule]
        private void GetEqptInfo()
        {
            //try
            //{
            //    ShowLoadingIndicator();

            //    DataTable inTable = _Biz.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

            //    DataRow newRow = inTable.NewRow();
            //    newRow["EQPTID"] = _EqptID;

            //    inTable.Rows.Add(newRow);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

            //    if (dtRslt != null && dtRslt.Rows.Count > 0)
            //    {
            //        txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
            //        txtWODetail.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);                    
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }
                
        private void RunStart(string sNewLot)
        {
            //try
            //{
            //    string sBizName = string.Empty;
            //    string sRetDataSet = string.Empty;

            //    if (string.Equals(_procid, Process.LAMINATION))
            //    {
            //        sBizName = "BR_PRD_REG_START_PROD_LOT_LM_L";
            //        sRetDataSet = "OUT_LOT";
            //    }
            //    else if (string.Equals(_procid, Process.STACKING_FOLDING))
            //    {
            //        sBizName = "BR_PRD_REG_START_PROD_LOT_FD_L";
            //        sRetDataSet = "OUT_LOT";
            //    }
            //    else if (string.Equals(_procid, Process.PACKAGING))
            //    {
            //        sBizName = "BR_PRD_REG_START_PROD_LOT_CL_L";
            //        sRetDataSet = "";
            //    }
                
            //    ShowLoadingIndicator();
                
            //    // 착공 처리..
            //    DataSet indataSet = new DataSet();
               
            //    DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            //    inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //    inDataTable.Columns.Add("IFMODE", typeof(string));
            //    inDataTable.Columns.Add("EQPTID", typeof(string));
            //    inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            //    inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
            //    inDataTable.Columns.Add("LOT_MODE", typeof(string));
            //    inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
            //    inDataTable.Columns.Add("USERID", typeof(string));

            //    DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");                
            //    inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            //    inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            //    inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
            //    inMtrl.Columns.Add("CSTID", typeof(string));

            //    DataTable inTable = indataSet.Tables["IN_EQP"];

            //    DataRow newRow = inTable.NewRow();
            //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //    newRow["EQPTID"] = _EqptID;
            //    newRow["PROD_LOTID"] = sNewLot;
            //    //newRow["WO_DETL_ID"] = "";
            //    newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
            //    newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
            //    newRow["USERID"] = LoginInfo.USERID;

            //    inTable.Rows.Add(newRow);
                
            //    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "IN_EQP,IN_INPUT", sRetDataSet.Equals("") ? null : sRetDataSet, indataSet);

            //    btnOK.IsEnabled = false;

            //    bSave = true;

            //    NEW_PROD_LOT = sNewLot;

            //    //this.DialogResult = MessageBoxResult.OK;

            //    HiddenLoadingIndicator();

            //    tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", sNewLot); // [%1] LOT이 생성 되었습니다.

            //    grdMsg.Visibility = Visibility.Visible;

            //    AsynchronousClose();
            //}
            //catch (Exception ex)
            //{
            //    HiddenLoadingIndicator();
            //    Util.MessageException(ex);
            //}
        }

        private void popupUser()
        {
            CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];
            Parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;

                txtReleaseNote.Focus();
            }
        }
        private bool CanRelease()
        {
            bRls = false;
            if (txtUserName.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1843"); // 입력오류 : 작업자를 입력하세요
                return bRls;
            }
            if (txtReleaseNote.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU8204"); // 입력오류 : 조치내역을 입력하세요
                return bRls;
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOK);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #endregion
        
    }
}
