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
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_APPR_ASSIGN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string areaId = string.Empty;
        string eqptId = string.Empty;
        string wrkDate = string.Empty;
        string strtDttm = string.Empty;
        string endDttm = string.Empty;
        string lossCode = string.Empty;
        string lossDetlCode = string.Empty;
        string lossNote = string.Empty;

        public COM001_014_APPR_ASSIGN()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Data
        private void SetApprUserCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_PRD_SEL_EQPT_LOSS_CHG_APPR_USER";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("SYSID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = areaId;
            dr["SYSID"] = LoginInfo.SYSID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.DisplayMemberPath = "USERNAME";
            cbo.SelectedValuePath = "USERID";
            cbo.SelectedIndex = 0;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                areaId = Util.NVC(tmps[0]);
                eqptId = Util.NVC(tmps[1]);
                wrkDate = Util.NVC(tmps[2]);
                strtDttm = Util.NVC(tmps[3]);
                endDttm = Util.NVC(tmps[4]);
                lossCode = Util.NVC(tmps[5]);
                lossDetlCode = Util.NVC(tmps[6]);
                lossNote = Util.NVC(tmps[7]);
            }

            SetApprUserCombo(cboApproval);
        }
   
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //설비 Loss 수정 요청을 하시겠습니까? 
                Util.MessageConfirm("SFU5178", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });

            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Mehod
        private void Save()
        {
            DataSet ds = new DataSet();
            DataTable RQSTDT = ds.Tables.Add("INDATA");
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("APPR_STAT", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CNTT", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("APPR_USERID", typeof(string));
            RQSTDT.Columns.Add("SPLT_MRG_FLAG", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQPTID"] = eqptId;
            dr["STRT_DTTM"] = strtDttm;
            dr["END_DTTM"] = endDttm;
            dr["WRK_DATE"] = wrkDate;
            dr["APPR_STAT"] = "W";
            dr["APPR_REQ_LOSS_CODE"] = lossCode;
            dr["APPR_REQ_LOSS_DETL_CODE"] = lossDetlCode;
            dr["APPR_REQ_LOSS_CNTT"] = lossNote;
            dr["USERID"] = LoginInfo.USERID;
            dr["APPR_USERID"] = Util.GetCondition(cboApproval);
            dr["SPLT_MRG_FLAG"] = "M";
            RQSTDT.Rows.Add(dr);
            
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_INS_REQ_MERGE", "INDATA", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (loadingIndicator != null)
                        loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, ds);
        }

        private bool Validation()
        {
            if (string.IsNullOrEmpty(cboApproval.Text))
            {
                Util.MessageInfo("SFU1692"); // 승인자가 필요합니다.
                return false;
            }
            return true;
        }
        #endregion
    }
}
