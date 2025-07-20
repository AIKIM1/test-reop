/*************************************************************************************
 Created Date : 2020.10.22
      Creator : 신광희
   Decription : CNB2동 증설 - 노칭 공정진척 - 작업시작(ASSY004_001_RUNSTART Copy 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.22  신광희 : Initial Created.
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
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_001_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_001_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _procid = string.Empty;
        private string _Lotid = string.Empty;
        private string _cstid = string.Empty;
        private string _InputPstnID = string.Empty;

        public string NEW_PROD_LOT = string.Empty;

        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private bool bSave = false;
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

        public ASSY005_001_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "PROD_LOT_OPER_MODE" };
            _combo.SetCombo(cboLotMode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            if (cboLotMode.Items.Count > 0)
                cboLotMode.SelectedValue = "L"; // UI 는 비정규 모드.

            String[] sFilter2 = { "IRREGL_PROD_LOT_TYPE_CODE" };
            _combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");
            
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

            _LineID = Util.NVC(tmps[0]);
            _EqptID = Util.NVC(tmps[1]);
            _procid = Util.NVC(tmps[2]);
            _Lotid = Util.NVC(tmps[3]);
            _cstid = Util.NVC(tmps[4]);
            _InputPstnID = Util.NVC(tmps[5]);
            _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[6]);

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            txtInputLot.Text = _Lotid;
            txtInputCstID.Text = _cstid;
            txtInProdID.Text = Util.NVC(tmps[7]);
            txtInWipQty.Text = Util.NVC(tmps[8]);

            txtEquipment.Text = "[" + _EqptID + "] " + Util.NVC(tmps[9]);

            GetEqptInfo();
            
            if (!_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                tbOutCst.Visibility = Visibility.Collapsed;
                txtOutCstID.Visibility = Visibility.Collapsed;
            }

            if (!_procid.Equals(Process.NOTCHING))
                tbQty.Text = ObjectDic.Instance.GetObjectName("재공수량");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtOutCstID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutCstID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutCstID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizRule]
        private void GetEqptInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    txtWODetail.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
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
        
        private void RunStart()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = string.Empty;

                if (_procid.Equals(Process.VD_LMN))
                    sBizName = "BR_PRD_REG_START_LOT_VD_R2R_L";
                else if (_procid.Equals(Process.NOTCHING))
                    sBizName = "BR_PRD_REG_START_LOT_NT_L";

                // 착공 처리..
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WORK_TYPE", typeof(string));
                inDataTable.Columns.Add("LOT_MODE", typeof(string));
                inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
                inMtrl.Columns.Add("CSTID", typeof(string));

                DataTable inOutLot = indataSet.Tables.Add("IN_OUTLOT");
                inOutLot.Columns.Add("OUT_CSTID", typeof(string));
                inOutLot.Columns.Add("OUT_LOTID", typeof(string));
                inOutLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inOutLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["WORK_TYPE"] = "P"; // P : 정상 , V : V/D재작업 , W : 재와인딩
                newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                newRow = inMtrl.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _InputPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = _Lotid;
                newRow["CSTID"] = _cstid;

                inMtrl.Rows.Add(newRow);

                newRow = inOutLot.NewRow();
                newRow["OUT_CSTID"] = txtOutCstID.Text;
                newRow["OUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;
                newRow["EQPT_MOUNT_PSTN_STATE"] = null;

                inOutLot.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "IN_EQP,IN_INPUT,IN_OUTLOT", "OUT_LOT", indataSet);

                btnOK.IsEnabled = false;

                bSave = true;

                if ((bool)dsRslt?.Tables?.Contains("OUT_LOT") && dsRslt?.Tables["OUT_LOT"]?.Rows?.Count > 0)
                    NEW_PROD_LOT = Util.NVC(dsRslt?.Tables["OUT_LOT"]?.Rows[0]["OUT_LOTID"]);

                if (NEW_PROD_LOT.Equals(""))
                    NEW_PROD_LOT = _Lotid;

                HiddenLoadingIndicator();

                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", NEW_PROD_LOT); // [%1] LOT이 생성 되었습니다.

                grdMsg.Visibility = Visibility.Visible;

                AsynchronousClose();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanRunStart()
        {
            bool bRet = false;
            // UnLoader 식별기준코드 (RF_ID, CST_ID) 체크
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
            {
                if (txtOutCstID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU6051", (action) =>
                    {
                        txtOutCstID.Focus();
                    });
                    return bRet;
                }

                if (txtOutCstID.Text.Equals(txtInputCstID.Text))
                {
                    // 입력오류 : 투입과 동일한 Carrier ID는 사용할 수 없습니다.
                    Util.MessageValidation("SFU6053", (action) =>
                    {
                        txtOutCstID.Text = "";
                        txtOutCstID.Focus();
                    });
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
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
