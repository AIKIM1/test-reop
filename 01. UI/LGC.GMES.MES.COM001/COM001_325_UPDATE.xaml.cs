/*************************************************************************************
 Created Date : 2020.04.01
      Creator : 
   Decription : 생산설비 품질승인 수정 Popup
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
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_325_UPDATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sAREAID = string.Empty;
        private string sEQSGID = string.Empty;
        private string sEQSGNAME = string.Empty;
        private string sEQPTID = string.Empty;
        private string sEQPTNAME = string.Empty;
        private string sCALDATE = string.Empty;
        private string sMONTH = string.Empty;
        private string sQUALITYSTEP = string.Empty;
        private string sPRODID = string.Empty;
        private string sPJT = string.Empty;

        private Util util = new Util();
 
        public COM001_325_UPDATE()
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

            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter2 = { "RESOURCE_QUALITY_CONFIRM " };
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
                sQUALITYSTEP = Util.NVC(tmps[6]);
                sPRODID = Util.NVC(tmps[7]);
                sPJT = Util.NVC(tmps[8]);

                txtEqsg.Text = sEQSGNAME;
                txtEquipment.Text = sEQPTNAME;
                txtQualityStep.Text = sQUALITYSTEP;
                txtAcceptDate.Text = sCALDATE;
                txtPRODID.Text = sPRODID;
                txtPJT.Text = sPJT;

                txtUPDUser.Text = LoginInfo.USERNAME;
                
                Initialize();
                Search();
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void btnUpd_Click(object sender, RoutedEventArgs e)
        {
            if (!validationSave())
                return;

            // 해당 품질정보를 수정 하시겠습니까
            Util.MessageConfirm("SFU8176", (result) =>
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

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (!validationSave())
                return;

            // 해당 품질정보를 삭제 하시겠습니까
            Util.MessageConfirm("SFU8177", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save("Y");
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

        private void Search()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CALDATE", typeof(DateTime));
                inDataTable.Columns.Add("EQPT_QLTY_APPR_STEP", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                newRow["EQSGID"] = sEQSGID;
                newRow["EQPTID"] = sEQPTID;
                newRow["CALDATE"] = sCALDATE + " 00:00:00";
                newRow["EQPT_QLTY_APPR_STEP"] = sQUALITYSTEP;
                newRow["DEL_FLAG"] = "N";

                inDataTable.Rows.Add(newRow);

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_EQPT_QLTY_ARRP_CUS", "RQSTDT", "RSLTDT", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    cboAccept.SelectedValue = Util.NVC(dtRslt.Rows[0]["EQPT_QLTY_APPR_RSLT_CODE"]);
                    txtRemark.Text = Util.NVC(dtRslt.Rows[0]["APPR_NOTE"]);
                    txtRUSER.Text = Util.NVC(dtRslt.Rows[0]["APPR_USERNAME"]);
                    if (Util.NVC(cboAccept.SelectedValue).Equals(string.Empty))
                        cboAccept.SelectedIndex = 0;
                }
            }
            catch (Exception ex) {  }
        }
        private void Save(string DEL_FLAG = "N")
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
                inDataTable.Columns.Add("APPR_NOTE", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("INSUSER", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                newRow["EQSGID"] = sEQSGID;
                newRow["EQPTID"] = sEQPTID;
                newRow["CALDATE"] = sCALDATE;
                newRow["EQPT_QLTY_APPR_STEP"] = Util.NVC(txtQualityStep.Text);
                newRow["EQPT_QLTY_APPR_RSLT_CODE"] = Util.GetCondition(cboAccept);
                newRow["APPR_DTTM"] = Util.NVC(txtAcceptDate.Text) + " " + teTimeEditor.ToString();
                newRow["APPR_NOTE"] = Util.NVC(txtRemark.Text);
                newRow["DEL_FLAG"] = DEL_FLAG;
                newRow["INSUSER"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_TB_SFC_PROD_EQPT_QLTY_APPR", "RQSTDT", null, inDataTable);

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

        #region Validation
        private bool validationSave()
        {
            try
            {
                bool bRet = false;

                if (cboAccept.SelectedValue.ToString().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU8175");  //승인여부를 선택 하십시오.
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
