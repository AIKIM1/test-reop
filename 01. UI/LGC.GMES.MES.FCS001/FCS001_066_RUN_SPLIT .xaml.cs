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
using System.Globalization;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_066_RUN_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();

        string _AREA = "";
        string _EQSGID = "";
        string _EQPTID = "";
        string _PROCID = "";
        string _TIME = "";
        string _START_DTTM = "";
        string startTime = "";
        string endTime = "";
        string _END_DTTM = "";

        DateTime InitStartTime;
        DateTime InitEndTime;

        bool commttiedFlag = false;

        string _WORK_DATE = "";
        FCS001_066 win = null;


        DataTable _dtBeforeSet = new DataTable();
        public FCS001_066_RUN_SPLIT()
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

                _EQPTID = Util.NVC(tmps[0]).Substring(1, Util.NVC(tmps[0]).Length - 1);
                _TIME = Util.NVC(tmps[1]);
                _START_DTTM = Util.NVC(tmps[2]);
                _END_DTTM = Util.NVC(tmps[3]);
                _AREA = Util.NVC(tmps[4]);
                _WORK_DATE = Util.NVC(tmps[5]);
                win = tmps[6] as FCS001_066;
                _EQSGID = Util.NVC(tmps[7]);
                _PROCID = Util.NVC(tmps[8]);

            }

            setInit();
            initGrid();
        }
        private void dgSplit_committed(object sender, DataGridCellEventArgs e)
        {
            commttiedFlag = true;
            DateTime start_dttm = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM")));
            DateTime end_dttm = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM")));
            double minute = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")));


            if (e.Cell.Column.Index == 1)
            {
                if (end_dttm < start_dttm)
                {
                    Util.MessageValidation("종료시간이 시작시간보다 이전입니다.");
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                    return;
                }

                if (end_dttm > DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                {
                    Util.Alert("종료시간이 기존 종료시간 보다 이후입니다.");
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                    return;
                }
                if (((int)end_dttm.Subtract(start_dttm).TotalMinutes) > int.Parse(txtTime.Text))
                {
                    Util.Alert("종료시간이 기존 종료시간 보다 이후입니다.");
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                    return;
                }
                
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)end_dttm.Subtract(start_dttm).TotalMinutes);
            }
            if (e.Cell.Column.Index == 2)
            {

                if (rdoStart.IsChecked == true) //시작시간에서 추가
                {
                    if (start_dttm.AddMinutes(minute) > DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                    {
                        Util.MessageValidation("SFU3215"); //종료시간이 기존 종료시간 보다 큽니다.
                        DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                        DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                        return;
                    }
                    if (minute == 0)
                    {
                        Util.MessageValidation("SFU3216"); //경과시간 0은 입력할 수 없습니다.
                        DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                        DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                        return;
                    }


                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", start_dttm.AddMinutes(minute));

                }else if(rdoEnd.IsChecked == true) //종료시간에서 빼기
                {
                    if (end_dttm.AddMinutes((-1)* minute) < DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                    {
                        Util.MessageValidation("시작시간이 기존 시작시간 보다 이전입니다.");
                        //DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                        //DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture)).Minutes);
                        return;
                    }

                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", end_dttm.AddMinutes(((-1) * minute)));
                }

         
            }

            initCombo();

        }

        private void setInit()
        {
            startTime = DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("HH:mm:ss");
            endTime = DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("HH:mm:ss");

            InitStartTime = DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            InitEndTime = DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            txtRunStartTime.Text = InitStartTime.ToString("HH:mm:ss");
            txtRunEndTime.Text = InitEndTime.ToString("HH:mm:ss");

            txtTime.Text = ((int)(InitEndTime.Subtract(InitStartTime).TotalMinutes)).ToString();


            DataTable dt = new DataTable();
            dt.Columns.Add("START_DTTM", typeof(string));
            dt.Columns.Add("START_DTTM_HIDDEN", typeof(string));
            dt.Columns.Add("END_DTTM", typeof(string));
            dt.Columns.Add("END_DTTM_HIDDEN", typeof(string));
            dt.Columns.Add("MINUTE", typeof(int));

            DataRow row = dt.NewRow();
            row["START_DTTM"] = InitStartTime;
            row["START_DTTM_HIDDEN"] = _START_DTTM;
            row["END_DTTM"] = InitEndTime;
            row["END_DTTM_HIDDEN"] = _END_DTTM;
            row["MINUTE"] = (int)(InitEndTime.Subtract(InitStartTime).TotalMinutes);
            dt.Rows.Add(row);

            dgSplit.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void initCombo()
        {
            try
            {             //원인설비
                string[] sFilter = { _EQPTID };
                combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

                //Loss분류
                C1ComboBox[] cboLossChild = { cboLossDetl };
                string[] sFilter3 = { "popup" };
                combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter:sFilter3);
                //cboLoss.SelectedIndex = "RUN";

                //DataTable dt = new DataTable();
                //dt = DataTableConverter.Convert(cboLoss.ItemsSource);

                //DataRow row = dt.NewRow();
                //row["CBO_CODE"] = "RUN";
                //row["CBO_NAME"] = "RUN";
                //dt.Rows.Add(row);

                //cboLoss.ItemsSource = DataTableConverter.Convert(dt);
                //cboLoss.SelectedValue = "RUN";


                //부동내용
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboOccurEqpt };
                string[] sFilter2 = { _AREA, _PROCID };
                combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter2);

                //현상코드
                String[] sFilterFailure = { "F" };
                combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

                //원인코드
                String[] sFilterCause = { "C" };
                combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

                //조치코드
                String[] sFilterResolution = { "R" };
                combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");
            }
            catch (Exception ex) { }
        }

        private void rdoButton_Click(object sender, RoutedEventArgs e)
        {

            if (rdoStart.IsChecked == true)
            {
                starttime.IsReadOnly = true;
                endtime.IsReadOnly = false;
                minute.IsReadOnly = false;



            }
            else if (rdoEnd.IsChecked == true)
            {
                starttime.IsReadOnly = true;
                endtime.IsReadOnly = false;
                minute.IsReadOnly = false;
            }
            //else if (rdoSelect.IsChecked == true)
            //{
            //    starttime.IsReadOnly = false;
            //    endtime.IsReadOnly = true;
            //    minute.IsReadOnly = false;
            //}

            DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (!AddRowValidation())
            {
                return;
            }
            commttiedFlag = false;
            string starttime = Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM"));
            string endtime = Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM"));

            if (!validationAddRow(starttime, endtime))
            {
                return;
            }

            dgSplitInfo.IsReadOnly = false;
            dgSplitInfo.BeginNewRow();
            dgSplitInfo.EndNewRow(true);

            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "EQPTID", _EQPTID);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "START_DTTM", DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM").ToString()).ToString("HH:mm:ss")); //hh:mm:ss형식
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "START_DTTM_HIDDEN", DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM"));
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "END_DTTM", DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM").ToString()).ToString("HH:mm:ss"));
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "END_DTTM_HIDDEN", Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM")));
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "MINUTE", Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")));
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "EQPTNAME", cboOccurEqpt.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSS", cboLoss.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSS_NAME", cboLoss.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSSDETAIL", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboLossDetl.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSSDETAIL_NAME", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboLossDetl.Text);
            //DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "FAILURE", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboFailure.SelectedValue.ToString());
            //DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "FAILURE_NAME", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboFailure.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "CAUSE", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboCause.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "CAUSE_NAME", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboCause.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "RESOLUTION", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboResolution.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "RESOLUTION_NAME", cboLoss.SelectedValue.ToString().Equals("RUN") ? null : cboResolution.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "REMARK", txtRemark.Text);

            dgSplitInfo.IsReadOnly = true;

            resetSplitTime(starttime,endtime);
        }
        private void initGrid()
        {
            DataTable dt = new DataTable();

            foreach (C1.WPF.DataGrid.DataGridColumn col in dgSplitInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));

            }

            dgSplitInfo.BeginEdit();
            dgSplitInfo.ItemsSource = DataTableConverter.Convert(dt);
            dgSplitInfo.EndEdit();

        }
        private bool validationAddRow(string starttime, string endtime)
        {
            if (!cboLoss.SelectedValue.Equals("RUN"))
            {
                if (cboOccurEqpt.Text.Equals("-SELECT-"))
                {
                    return false;
                }

                if (cboLoss.Text.Equals("-SELECT-"))
                {
                    return false;
                }
                //if (cboLossDetl.Text.Equals("-SELECT-"))
                //{
                //    return false;
                //}
                if (cboCause.Text.Equals("-SELECT-"))
                {
                    return false;
                }
                if (cboFailure.Text.Equals("-SELECT-"))
                {
                    return false;
                }
                if (cboResolution.Text.Equals("-SELECT-"))
                {
                    return false;
                }
            }

            return true;
        }
        private void resetSplitTime(string starttime, string endtime)
        {
            if (rdoStart.IsChecked == true)
            {
                DateTime dt = DateTime.Parse(endtime).AddSeconds(1);
                if (dt > DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                {
                    dgSplit.IsEnabled = false;
                    return;
                }

                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", dt);
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", InitEndTime);
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM"))).Subtract(DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM")))).TotalMinutes));
                
            }
            else if (rdoEnd.IsChecked == true)
            {
                DateTime dt = DateTime.Parse(starttime).AddSeconds(-1);
                if (dt < DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                {
                    dgSplit.IsEnabled = false;
                    return;
                }
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", dt);
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", InitStartTime);
                DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM"))).Subtract(DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM")))).TotalMinutes));
            }

            initCombo();
            txtRemark.Text = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (dgSplitInfo.GetRowCount() == 0)
            {
                Util.MessageValidation("추가된 상태가 없습니다.");
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgSplitInfo.ItemsSource);
            if (dt.Select("LOSS_NAME <> 'RUN'").Length == 0)
            {
                Util.MessageValidation("Loss를 하나라도 추가해야 합니다.");
                return;
            }

            DataSet ds = new DataSet();

            DataTable inData = ds.Tables.Add("INDATA");
            inData.Columns.Add("AREAID", typeof(string));
            inData.Columns.Add("EQPTID", typeof(string));
            inData.Columns.Add("WRK_DATE", typeof(string));
            inData.Columns.Add("START_DTTM", typeof(DateTime));
            inData.Columns.Add("END_DTTM", typeof(DateTime));
            inData.Columns.Add("USERID", typeof(string));
            inData.Columns.Add("START", typeof(string));
            inData.Columns.Add("START_DTTM_YMDHMS", typeof(string));

            DataRow row = inData.NewRow();
            row["AREAID"] = _AREA;
            row["EQPTID"] = _EQPTID;
            row["WRK_DATE"] = _WORK_DATE.Replace("-", "");
            row["START_DTTM"] = DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            row["END_DTTM"] = DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            row["USERID"] = LoginInfo.USERID;
            row["START"] = rdoStart.IsChecked == true ? "S" : "E" ;
            row["START_DTTM_YMDHMS"] = _START_DTTM;
            inData.Rows.Add(row);

            DataTable inLoss = ds.Tables.Add("INLOSS");
            inLoss.Columns.Add("START_DTTM", typeof(DateTime));
            inLoss.Columns.Add("END_DTTM", typeof(DateTime));
            inLoss.Columns.Add("OCCR_EQPTID", typeof(string));
            inLoss.Columns.Add("LOSS_CODE", typeof(string));
            inLoss.Columns.Add("LOSS_DETL_CODE", typeof(string));
            inLoss.Columns.Add("SYMP_CODE", typeof(string));
            inLoss.Columns.Add("CAUSE_CODE", typeof(string));
            inLoss.Columns.Add("REPAIR_CODE", typeof(string));
            inLoss.Columns.Add("NOTE", typeof(string));

            row = null;

            if (rdoStart.IsChecked == true)
            {
                for (int i = 0; i < dgSplitInfo.GetRowCount(); i++)
                {
                    row = inLoss.NewRow();
                    row["START_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "START_DTTM_HIDDEN")));
                    row["END_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "END_DTTM_HIDDEN")));
                    row["OCCR_EQPTID"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "EQPTID")));
                    row["LOSS_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS")));
                    row["LOSS_DETL_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS"))).Equals("RUN") ? null : Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSSDETAIL")));
                    row["SYMP_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "FAILURE")));
                    row["CAUSE_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "CAUSE")));
                    row["REPAIR_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "RESOLUTION")));
                    row["NOTE"] = txtRemark.Text;
                    inLoss.Rows.Add(row);
                }
            }
            else if (rdoEnd.IsChecked == true)
            {
                for (int i = dgSplitInfo.GetRowCount() - 1; i >= 0; i--)
                {
                    row = inLoss.NewRow();
                    row["START_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "START_DTTM_HIDDEN")));
                    row["END_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "END_DTTM_HIDDEN")));
                    row["OCCR_EQPTID"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "EQPTID")));
                    row["LOSS_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS")));
                    row["LOSS_DETL_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS"))).Equals("RUN") ? null : Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSSDETAIL")));
                    row["SYMP_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "FAILURE")));
                    row["CAUSE_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "CAUSE")));
                    row["REPAIR_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "RESOLUTION")));
                    row["NOTE"] = txtRemark.Text;
                    inLoss.Rows.Add(row);
                }
            }

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_RUN_SPLT", "INDATA, INLOSS", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("BR_EQPT_EQPTLOSS_RUN_SPLT", bizException.Message, bizException.ToString());
                            return;
                        }

                        //Util.AlertConfirm("완료");
                        win.btnSearch_Click(null, null);
                        this.DialogResult = MessageBoxResult.OK;



                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2807"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning); //조회 오류
            }


        }
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {

            setInit();
            initCombo();
            dgSplitInfo.ItemsSource = null;
            initGrid();
            dgSplit.IsEnabled = true;
            commttiedFlag = false;

            
        }
        private void cboLossDetl_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if (cboLoss.Text.Equals("-SELECT-"))
            //{
            //    Util.Alert("LOSS를 선택해주세요");
            //    return;
            //}
            //if ((!cboLoss.Text.Equals("RUN")) )
            //{
            //    Util.Alert("상태가 RUN입니다.");
            //    return;
            //}
        }
        private void btnSearchLossCode_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidateFCR())
            //{
            //    return;
            //}

            FCS001_066_FCR_LIST wndFCRList = new FCS001_066_FCR_LIST();
            wndFCRList.FrameOperation = FrameOperation;

            if (wndFCRList != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = _AREA;//Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = _PROCID;//Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = _EQSGID;//Convert.ToString(cboEquipmentSegment.SelectedValue);

                C1WindowExtension.SetParameters(wndFCRList, Parameters);

                wndFCRList.Closed += new EventHandler(wndFCRList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCRList.ShowModal()));
                wndFCRList.BringToFront();

            }

        }
        private void wndFCRList_Closed(object sender, EventArgs e)
        {
            FCS001_066_FCR_LIST window = sender as FCS001_066_FCR_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboFailure.SelectedValue = window.F_CODE;
                cboCause.SelectedValue = window.C_CODE;
                cboResolution.SelectedValue = window.R_CODE;
            }
        }

        private bool AddRowValidation()
        {
            int initMiunte = int.Parse(txtTime.Text);
            int sumMinute = 0;
            for (int i = 0; i < dgSplitInfo.GetRowCount(); i++)
            {
                sumMinute += int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "MINUTE")));
            }

            if (initMiunte == sumMinute)
            {
                Util.MessageValidation("더이상 추가할 수 없습니다.");
                return false;
            }

       
            if ((sumMinute + int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")))) != initMiunte)
            {
                if (commttiedFlag == false)
                {
                    Util.MessageValidation("경과시간을 설정해주세요");
                    return false;
                }
            }

            if (dgSplitInfo.GetRowCount() == 0)
            {
                if (commttiedFlag == false)
                {
                    Util.Alert("경과시간을 설정해주세요");
                    return false;
                }
            }
            

            return true;
        }
        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            FCS001_066_LOSS_DETL wndLossDetl = new FCS001_066_LOSS_DETL();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = _AREA;//Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = _PROCID;//Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = _EQPTID;//Convert.ToString(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }
        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            FCS001_066_LOSS_DETL window = sender as FCS001_066_LOSS_DETL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE; //window.??
                cboLossDetl.SelectedValue = window._LOSS_DETL_CODE;

            }
        }

        #endregion

        #region Mehod
        #region [조회]







        #endregion

        #endregion


    }
}
