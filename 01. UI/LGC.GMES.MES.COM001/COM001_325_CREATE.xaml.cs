/*************************************************************************************
 Created Date : 2020.04.01
      Creator : 
   Decription : 공정 품질승인 등록 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.01  : Initial Created. 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_325_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sAREAID = string.Empty;
        private string sEQSGID = string.Empty;
        private string sEQSGNAME = string.Empty;
        private string sEQPTID = string.Empty;
        private string sEQPTNAME = string.Empty;
        private string sCALDATE = string.Empty;
        private string sPLANDATE = string.Empty;
        private string sPROCID = string.Empty;

        private Util util = new Util();

        public COM001_325_CREATE()
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
        private void Initialize()
        {
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "RESOURCE_QUALITY_PROCESS_STEP" };
            _combo.SetCombo(cboApprStep, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { "RESOURCE_QUALITY_CONFIRM" };
            _combo.SetCombo(cboAccept, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE");
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sAREAID = Util.NVC(tmps[0]);
                sEQSGID = Util.NVC(tmps[1]);
                sEQSGNAME = Util.NVC(tmps[2]);
                sEQPTID = Util.NVC(tmps[3]);
                sEQPTNAME = Util.NVC(tmps[4]);
                sCALDATE = Util.NVC(tmps[5]);
                sPLANDATE = Util.NVC(tmps[6]);
                sPROCID = Util.NVC(tmps[7]);

                txtEqsg.Text = sEQSGNAME;
                txtEquipment.Text = sEQPTNAME;
                txtAcceptDate.Text = sCALDATE + " " + DateTime.Now.ToString("HH:mm");
                txtRUSER.Text = LoginInfo.USERNAME;

                Initialize();
                getPlan(); 
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void btnProd_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                //COM001_325_CREATE_PROD wndPopup = new COM001_325_CREATE_PROD();
                COM001_325_CREATE_FIND_PROD wndPopup = new COM001_325_CREATE_FIND_PROD();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    wndPopup.Header = ObjectDic.Instance.GetObjectName("제품등록");
                    object[] Parameters = new object[6];
                    Parameters[0] = sPLANDATE;
                    Parameters[1] = LoginInfo.CFG_SHOP_ID;
                    Parameters[2] = sAREAID;
                    Parameters[3] = sEQSGID;
                    Parameters[4] = sEQPTID;
                    Parameters[5] = sPROCID;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseProd);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!validationSave())
                return;

            // 추가하시겠습니까?
            Util.MessageConfirm("SFU2965", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Mehod
        private void Save()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CALDATE", typeof(DateTime));
                inDataTable.Columns.Add("EQPT_QLTY_APPR_STEP", typeof(string));
                inDataTable.Columns.Add("EQPT_QLTY_APPR_RSLT_CODE", typeof(string));
                inDataTable.Columns.Add("APPR_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("APPR_USERID", typeof(string));
                inDataTable.Columns.Add("APPR_NOTE", typeof(string));
                inDataTable.Columns.Add("APPR_PRODID", typeof(string));
                inDataTable.Columns.Add("INSUSER", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                newRow["EQSGID"] = sEQSGID;
                newRow["EQPTID"] = sEQPTID;
                newRow["CALDATE"] = sCALDATE;
                newRow["EQPT_QLTY_APPR_STEP"] = Util.GetCondition(cboApprStep);
                newRow["EQPT_QLTY_APPR_RSLT_CODE"] = Util.GetCondition(cboAccept);
                newRow["APPR_DTTM"] = Util.NVC(txtAcceptDate.Text);
                newRow["APPR_USERID"] = LoginInfo.USERID;
                newRow["APPR_NOTE"] = Util.NVC(txtRemark.Text);
                newRow["APPR_PRODID"] = Util.NVC(txtPRODID.Text);
                newRow["INSUSER"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_TB_SFC_PROD_EQPT_QLTY_APPR", "RQSTDT", null, inDataTable);

                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                this.Dispatcher.BeginInvoke(new Action(() => btnClose_Click(null, null)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void getPlan()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PLAN_DATE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = sPROCID;
                dr["PLAN_DATE"] = sPLANDATE;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; ;
                dr["AREAID"] = sAREAID; ;
                dr["EQSGID"] = sEQSGID;
                dr["EQPTID"] = sEQPTID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_QLTY_PROD_PLAN", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        txtPRODID.Text = Util.NVC(row["PRODID"]);
                        txtPJT.Text = Util.NVC(row["PRJT_NAME"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void OnCloseProd(object sender, EventArgs e)
        {
            //COM001_325_CREATE_PROD window = sender as COM001_325_CREATE_PROD;
            COM001_325_CREATE_FIND_PROD window = sender as COM001_325_CREATE_FIND_PROD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtPRODID.Text = window._getProductID;
                txtPJT.Text = window._getPrjt;
            }
        }

        #region Validation
        private bool validationSave()
        {
            try
            {
                bool bRet = false;

                if (cboApprStep.SelectedValue.ToString().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU8174");  //품질승인 단계를 선택 하십시오.
                    return bRet;
                }

                if (cboAccept.SelectedValue.ToString().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU8175");  //승인여부를 선택 하십시오.
                    return bRet;
                }

                if (string.IsNullOrEmpty(txtPRODID.Text))
                {
                    Util.MessageValidation("SFU7008");  //제품코드가 입력되지 않았습니다.
                    return bRet;
                }
                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #endregion
    }
}
