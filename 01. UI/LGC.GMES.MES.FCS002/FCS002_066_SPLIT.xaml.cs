/*************************************************************************************
 Created Date : 2021.01.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_066_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        string _EQPTID = "";
        string _LOSS_SEQ = "";
        string _START_DTTM = "";
        string _END_DTTM = "";
        string _AREA_ID = "";

        int _MAX_SEQNO = 0;

        bool commit_flag = false;

        DataTable _dtBeforeSet = new DataTable();
        public FCS002_066_SPLIT()
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _EQPTID = Util.NVC(tmps[0]);
                _LOSS_SEQ = Util.NVC(tmps[1]);
                _START_DTTM = Util.NVC(tmps[2]);
                _END_DTTM = Util.NVC(tmps[3]);
                _AREA_ID = Util.NVC(tmps[4]);

            }
            SetDefault();

            GetMaxSeqno();

            if (Convert.ToInt32(_LOSS_SEQ) + 90 < _MAX_SEQNO) {
                Util.AlertInfo("SFU1574");  //분할 할 수 있는 횟수를 초과하였습니다.
                btnSave.Visibility = Visibility.Collapsed;
            }
        }
        
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SetDefault();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (commit_flag == false)
                {
                    Util.MessageValidation("SFU3436"); // 값 수정 후 Enter키를 눌러주세요.
                    return;
                }
                DataSet dsData = new DataSet();

                DataTable dtFrom = dsData.Tables.Add("FROMDT");
                dtFrom.Columns.Add("EQPTID", typeof(string));
                dtFrom.Columns.Add("FROM_SEQNO", typeof(Int32));
                dtFrom.Columns.Add("TO_SEQNO", typeof(Int32));
                dtFrom.Columns.Add("USE_FLAG", typeof(string));
                dtFrom.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = dtFrom.NewRow();
                row["EQPTID"] = _EQPTID;
                row["FROM_SEQNO"] = _LOSS_SEQ;
                row["TO_SEQNO"] = _LOSS_SEQ;
                row["USE_FLAG"] = 'N';
                row["USERID"] = LoginInfo.USERID;

                dtFrom.Rows.Add(row);

            
                DataTable dtTo = dsData.Tables.Add("TODT");
                dtTo.Columns.Add("EQPTID", typeof(string));
                dtTo.Columns.Add("LOSS_SEQNO", typeof(Int32));
                dtTo.Columns.Add("TO_LOSS_SEQNO", typeof(Int32));
                dtTo.Columns.Add("STRT_DTTM", typeof(DateTime));
                dtTo.Columns.Add("END_DTTM", typeof(DateTime));
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("AREAID", typeof(string));

                for (int i = 0; i < dgSplit.Rows.Count; i++)
                {
                    row = dtTo.NewRow();
                    row["EQPTID"] = _EQPTID;
                    row["LOSS_SEQNO"] = _LOSS_SEQ;
                    row["TO_LOSS_SEQNO"] = _MAX_SEQNO + i + 1;
                    row["STRT_DTTM"] = Convert.ToDateTime(DataTableConverter.GetValue(dgSplit.Rows[i].DataItem, "START_DTTM"));
                    row["END_DTTM"] = Convert.ToDateTime(DataTableConverter.GetValue(dgSplit.Rows[i].DataItem, "END_DTTM"));
                    row["USERID"] = LoginInfo.USERID;
                    row["AREAID"] = _AREA_ID;
                    dtTo.Rows.Add(row);
                }
            
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_SPLIT", "FROMDT,TODT", null, dsData);

                Util.AlertInfo("SFU1573");  //분할 되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSplit_CommittedRowEdit(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            commit_flag = true;

            if (dgSplit.CurrentRow.Index < dgSplit.Rows.Count - 1)
            {
                Util.AlertInfo("SFU1527");  //마지막 시간만 수정 가능합니다.
                dgSplit.ItemsSource = DataTableConverter.Convert(_dtBeforeSet); //수정이전상태로 돌리기
                return;
            }

            DataTable dtDefault = DataTableConverter.Convert(dgSplit.ItemsSource);

            DataTable dtSplit = new DataTable();

            dtSplit.Columns.Add("START_DTTM", typeof(DateTime));
            dtSplit.Columns.Add("END_DTTM", typeof(DateTime));
            dtSplit.Columns.Add("TERM", typeof(decimal));

            DateTime dtLastStart = new DateTime();

            dtLastStart = Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss");

            for (int i = 0; i < dtDefault.Rows.Count; i++)
            {
                DataRow dr = dtSplit.NewRow();
                dr["START_DTTM"] = dtLastStart;
                dr["END_DTTM"] = dtLastStart.AddMinutes(Convert.ToInt16(dtDefault.Rows[i]["TERM"]));
                dr["TERM"] = Convert.ToInt16(dtDefault.Rows[i]["TERM"]);

                dtSplit.Rows.Add(dr);

                dtLastStart = dtLastStart.AddMinutes(Convert.ToInt16(dtDefault.Rows[i]["TERM"]));
            }

            DataRow drLast = dtSplit.NewRow();
            drLast["START_DTTM"] = dtLastStart;
            drLast["END_DTTM"] = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss");

            TimeSpan ts = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss") - dtLastStart;

            if (Convert.ToInt16(ts.TotalMinutes) <= 0)
            {
                Util.AlertInfo("SFU1694");  //시간을 잘못 조정 하셨습니다.
                dgSplit.ItemsSource = DataTableConverter.Convert(_dtBeforeSet); //수정이전상태로 돌리기
            }
            else
            {
                drLast["TERM"] = Convert.ToInt16(ts.TotalMinutes);

                dtSplit.Rows.Add(drLast);

                Util.gridClear(dgSplit);

                dgSplit.ItemsSource = DataTableConverter.Convert(dtSplit);
            }
        }
        #endregion

        #region Mehod
        #region [조회]
        public void SetDefault()
        {
            try
            {
                Util.gridClear(dgSplit);

                DataTable dtDefault = new DataTable();

                dtDefault.Columns.Add("START_DTTM", typeof(DateTime));
                dtDefault.Columns.Add("END_DTTM", typeof(DateTime));
                dtDefault.Columns.Add("TERM", typeof(decimal));

                DataRow dr = dtDefault.NewRow();
                dr["START_DTTM"] = Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss");
                dr["END_DTTM"] = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss");

                TimeSpan ts = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss") - Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss");

                dr["TERM"] = Convert.ToInt16(ts.TotalMinutes);


                dtDefault.Rows.Add(dr);

                dgSplit.ItemsSource = DataTableConverter.Convert(dtDefault);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void GetMaxSeqno()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOSS_SEQNO", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _EQPTID;
                dr["LOSS_SEQNO"] = _LOSS_SEQ;
                RQSTDT.Rows.Add(dr);

                DataTable dtMax = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_MAX_SEQNO", "RQSTDT", "RSLTDT", RQSTDT);

                _MAX_SEQNO = Convert.ToInt32(dtMax.Rows[0]["MAX_SEQNO"]);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #endregion

        private void dgSplit_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            _dtBeforeSet = DataTableConverter.Convert(dgSplit.ItemsSource);
        }
    }
}
