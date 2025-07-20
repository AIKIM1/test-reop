/*************************************************************************************
 Created Date : 2020.10.06
      Creator : 신광희 차장
   Decription : 작업시작 팝업 (ASSY0004.ASSY004_001_RUNSTART 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.06  신광희 : Initial Created.
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_008_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_008_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _productCode = string.Empty;
        private string _lotId = string.Empty;
        private string _carrierId = string.Empty;
        private string _equipmentMountPositionId = string.Empty;
        private string _workType = String.Empty;
        public string _newProductLot = string.Empty;

        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        private BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private bool _isSaved = false;

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

        public ASSY004_008_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //String[] sFilter = { "PROD_LOT_OPER_MODE" };
            //_combo.SetCombo(cboLotMode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            //if (cboLotMode.Items.Count > 0)
            //    cboLotMode.SelectedValue = "L"; // UI 는 비정규 모드.

            //String[] sFilter2 = { "IRREGL_PROD_LOT_TYPE_CODE" };
            //_combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            _equipmentSegmentCode = Util.NVC(parameters[0]);
            _equipmentCode = Util.NVC(parameters[1]);
            _productCode = Util.NVC(parameters[2]);
            _lotId = Util.NVC(parameters[3]);
            _carrierId = Util.NVC(parameters[4]);
            _equipmentMountPositionId = Util.NVC(parameters[5]);
            _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(parameters[6]);

            grdMsg.Visibility = Visibility.Collapsed;
            txtInputLot.Text = _lotId;
            txtInputCstID.Text = _carrierId;
            txtInProdID.Text = Util.NVC(parameters[7]);
            txtInWipQty.Text = Util.NVC(parameters[8]);
            _workType = Util.NVC(parameters[9]);

            ApplyPermissions();

            GetEqptInfo();
            
            if (!_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                tbOutCst.Visibility = Visibility.Collapsed;
                txtOutCstID.Visibility = Visibility.Collapsed;
            }

            if (!_productCode.Equals(Process.NOTCHING))
                tbQty.Text = ObjectDic.Instance.GetObjectName("재공수량");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
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
            if (_isSaved)
                DialogResult = MessageBoxResult.OK;
            else
                DialogResult = MessageBoxResult.Cancel;
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

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

                if(CommonVerify.HasTableRow(dtRslt))
                {
                    //txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
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

                const string bizRuleName = "BR_PRD_REG_START_LOT_VD_R2R_L";
                // 착공 처리..
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOT_MODE", typeof(string));
                inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WORK_TYPE", typeof(string));

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
                newRow["EQPTID"] = _equipmentCode;
                //newRow["WORK_TYPE"] = "P"; // P : 정상 , V : V/D재작업 , W : 재와인딩
                newRow["LOT_MODE"] = "L"; //Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = "N"; //Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WORK_TYPE"] = _workType;
                inDataTable.Rows.Add(newRow);

                newRow = inMtrl.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId; //_equipmentMountPositionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] ="A";
                newRow["INPUT_LOTID"] = _lotId;
                newRow["CSTID"] = _carrierId;
                inMtrl.Rows.Add(newRow);

                newRow = inOutLot.NewRow();
                newRow["OUT_CSTID"] = txtOutCstID.Text;
                newRow["OUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;
                newRow["EQPT_MOUNT_PSTN_STATE"] = null;
                inOutLot.Rows.Add(newRow);


                string xml = indataSet.GetXml();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_INPUT,IN_OUTLOT", "OUT_LOT", indataSet);

                btnOK.IsEnabled = false;
                _isSaved = true;

                if ((bool)dsRslt?.Tables?.Contains("OUT_LOT") && dsRslt?.Tables["OUT_LOT"]?.Rows?.Count > 0)

                    _newProductLot = Util.NVC(dsRslt?.Tables["OUT_LOT"]?.Rows[0]["OUT_LOTID"]);

                if(string.IsNullOrEmpty(_newProductLot))
                    _newProductLot = _lotId;

                HiddenLoadingIndicator();

                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", _newProductLot); // [%1] LOT이 생성 되었습니다.

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
        private bool ValidationRunStart()
        {
            if(string.IsNullOrEmpty(txtOutCstID.Text.Trim()))
            {
                Util.MessageValidation("SFU6051", (action) =>
                {
                    txtOutCstID.Focus();                    
                });
                return false;
            }

            /*
            if (txtOutCstID.Text.Equals(txtInputCstID.Text))
            {
                // 입력오류 : 투입과 동일한 Carrier ID는 사용할 수 없습니다.
                Util.MessageValidation("SFU6053", (action) =>
                {
                    txtOutCstID.Text = string.Empty;
                    txtOutCstID.Focus();
                });
                return false;
            }
            */

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
            DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #endregion

        
    }
}
