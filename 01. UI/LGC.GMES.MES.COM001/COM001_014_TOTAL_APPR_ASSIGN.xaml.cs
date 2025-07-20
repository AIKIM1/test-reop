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
    public partial class COM001_014_TOTAL_APPR_ASSIGN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string areaId = string.Empty;
        DataTable dtLoss = new DataTable();

        public COM001_014_TOTAL_APPR_ASSIGN()
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
                dtLoss = (DataTable)tmps[1];
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
            RQSTDT.Columns.Add("LOSS_SEQNO", typeof(string));
            RQSTDT.Columns.Add("APPR_STAT", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CNTT", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("APPR_USERID", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("TRBL_CODE", typeof(string));
            RQSTDT.Columns.Add("EIOSTAT", typeof(string));

            if (dtLoss != null && dtLoss.Rows.Count > 0)
            {
                for(int i = 0; i < dtLoss.Rows.Count; i++)
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["EQPTID"] = Util.NVC(dtLoss.Rows[i]["EQPTID"]);
                    dr["STRT_DTTM"] = Util.NVC(dtLoss.Rows[i]["STRT_DTTM"]);
                    dr["END_DTTM"] = Util.NVC(dtLoss.Rows[i]["END_DTTM"]);
                    dr["WRK_DATE"] = Util.NVC(dtLoss.Rows[i]["WRK_DATE"]);
                    dr["LOSS_SEQNO"] = Util.NVC(dtLoss.Rows[i]["LOSS_SEQNO"]);
                    dr["APPR_STAT"] = "W";
                    dr["APPR_REQ_LOSS_CODE"] = Util.NVC(dtLoss.Rows[i]["APPR_REQ_LOSS_CODE"]);
                    dr["APPR_REQ_LOSS_DETL_CODE"] = Util.NVC(dtLoss.Rows[i]["APPR_REQ_LOSS_DETL_CODE"]);
                    dr["APPR_REQ_LOSS_CNTT"] = Util.NVC(dtLoss.Rows[i]["APPR_REQ_LOSS_CNTT"]);
                    dr["USERID"] = Util.NVC(dtLoss.Rows[i]["USERID"]);
                    dr["APPR_USERID"] = Util.GetCondition(cboApproval);
                    dr["LOSS_CODE"] = Util.NVC(dtLoss.Rows[i]["LOSS_CODE"]);
                    dr["LOSS_DETL_CODE"] = Util.NVC(dtLoss.Rows[i]["LOSS_DETL_CODE"]);
                    dr["LOTID"] = Util.NVC(dtLoss.Rows[i]["LOTID"]);
                    dr["TRBL_CODE"] = Util.NVC(dtLoss.Rows[i]["TRBL_CODE"]);
                    dr["EIOSTAT"] = Util.NVC(dtLoss.Rows[i]["EIOSTAT"]);
                    RQSTDT.Rows.Add(dr);
                }
            }

            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_INS_REQ", "INDATA", null, (bizResult, bizException) =>
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
