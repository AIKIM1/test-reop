/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.10.20  복현수      MES 리빌딩 PJT (TRANSACTION_SERIAL_NO 추가, 시간 타입 변경)





**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        string _EQPTID = "";
        string _LOSS_SEQ = "";
        string _START_DTTM = "";
        string _END_DTTM = "";
        string _AREA_ID = "";
        string _TRAN_SEQ = ""; //2024.10.20 MES 리빌딩 PJT

        int _MAX_SEQNO = 0;

        bool commit_flag = false;

        DataTable _dtBeforeSet = new DataTable();
        public COM001_014_SPLIT()
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
                _TRAN_SEQ = Util.NVC(tmps[5]); //2024.10.20 MES 리빌딩 PJT

            }
            SetDefault();

            GetMaxSeqno();

            if (Convert.ToInt32(_LOSS_SEQ) + 90 < _MAX_SEQNO) {
                Util.AlertInfo("SFU1574");  //분할 할 수 있는 횟수를 초과하였습니다.
                btnSave.Visibility = Visibility.Collapsed;
            }
        }


        //#region [제거 처리]
        //private void delete_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Button bt = sender as Button;

        //    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

        //    if (((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index == 0) {
        //        Util.AlertInfo("시작은제거할수없습니다.");
        //        return;
        //    }

        //    try
        //    {

        //        //dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

        //        DataTable dtDefault = DataTableConverter.Convert(dgSplit.ItemsSource);

        //        dtDefault.Rows.RemoveAt(((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index);

        //        for (int i = 0; i < dtDefault.Rows.Count; i++)
        //        {
        //            DataRow dr = dtSplit.NewRow();
        //            dr["START_DTTM"] = (DateTime)dtDefault.Rows[i]["START_DTTM"];
        //            dr["END_DTTM"] = ((DateTime)dtDefault.Rows[i]["START_DTTM"]).AddMinutes(Convert.ToInt16(dtDefault.Rows[i]["TERM"]));
        //            dr["TERM"] = Convert.ToInt16(dtDefault.Rows[i]["TERM"]);

        //            dtSplit.Rows.Add(dr);

        //            dtLastStart = ((DateTime)dtDefault.Rows[i]["START_DTTM"]).AddMinutes(Convert.ToInt16(dtDefault.Rows[i]["TERM"]));
        //        }

        //        dgSplit.ItemsSource = DataTableConverter.Convert(dtDefault);

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}
        //#endregion


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
                dtFrom.Columns.Add("TRANSACTION_SERIAL_NO", typeof(string));

                DataRow row = null;
                row = dtFrom.NewRow();
                row["EQPTID"] = _EQPTID;
                row["FROM_SEQNO"] = _LOSS_SEQ;
                row["TO_SEQNO"] = _LOSS_SEQ;
                row["USE_FLAG"] = 'N';
                row["USERID"] = LoginInfo.USERID;
                row["TRANSACTION_SERIAL_NO"] = _TRAN_SEQ;

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

            dtLastStart = Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss"); //2024.10.20 MES 리빌딩 PJT

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
            drLast["END_DTTM"] = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss"); //2024.10.20 MES 리빌딩 PJT

            TimeSpan ts = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss") - dtLastStart; //2024.10.20 MES 리빌딩 PJT

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
                dr["START_DTTM"] = Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss"); //2024.10.20 MES 리빌딩 PJT
                dr["END_DTTM"] = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss"); //2024.10.20 MES 리빌딩 PJT

                TimeSpan ts = Util.StringToDateTime(_END_DTTM, "yyyyMMddHHmmss") - Util.StringToDateTime(_START_DTTM, "yyyyMMddHHmmss"); //2024.10.20 MES 리빌딩 PJT

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
                RQSTDT.Columns.Add("TRANSACTION_SERIAL_NO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _EQPTID;
                dr["LOSS_SEQNO"] = _LOSS_SEQ;
                dr["TRANSACTION_SERIAL_NO"] = _TRAN_SEQ;

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
