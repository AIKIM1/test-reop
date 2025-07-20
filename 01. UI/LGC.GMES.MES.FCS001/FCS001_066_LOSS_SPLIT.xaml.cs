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
using System.Windows.Input;
using System.Windows.Controls;
using System.Globalization;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_066_LOSS_SPLIT : C1Window, IWorkArea
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

        string initLoss;
        string initLossName;
        string initLossDetl;
        string initLossDetlName;
        string initEioStat;
        Int32 _LOSS_SEQ;
        Int32 _MAX_SEQNO;
        Int64 _PRE_LOSS_SEQNO;

        string strAreaType;


        DateTime InitStartTime;
        DateTime InitEndTime;

        bool commttiedFlag = false;

        string _WORK_DATE = "";


        DataTable _dtBeforeSet = new DataTable();
        public FCS001_066_LOSS_SPLIT()
        {
            InitializeComponent();            
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


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
                _EQSGID = Util.NVC(tmps[7]);
                _PROCID = Util.NVC(tmps[8]);

            }

            setInit();
            initGrid();

            GetMaxSeqno();

            if (Convert.ToInt32(_LOSS_SEQ) + 90 < _MAX_SEQNO)
            {
                Util.MessageValidation("SFU1574");  //분할 할 수 있는 횟수를 초과하였습니다.
                btnSave.Visibility = Visibility.Collapsed;
                this.DialogResult = MessageBoxResult.OK;

            }

            if ((Convert.ToInt32(_LOSS_SEQ) + 90 > _MAX_SEQNO) && (_LOSS_SEQ % 100 != 0))
            {
                Util.MessageValidation("SFU3495"); //분할된 데이터를 또 분할 할 수 없습니다.
                this.DialogResult = MessageBoxResult.OK;
            }

            if (GetMergeLossCnt() > 1)
            {
                Util.MessageValidation("SFU3500");//다른 LOSS와 merge한 데이터 이므로 분할 할 수 없습니다.
                this.DialogResult = MessageBoxResult.OK;
            }

            GetAreaType();

            if (!string.Equals(strAreaType, "P"))  //PACK 부서가 아닐 경우  작업자 기능 비활성화.
            {
                InitUser();
            }

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
                    Util.MessageValidation("SFU3231"); //종료시간이 시작시간보다 이전입니다.
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                    return;
                }

                if (end_dttm > DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                {
                    Util.Alert("SFU3232"); //종료시간이 기존 종료시간 보다 이후입니다.
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                    DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "MINUTE", (int)(DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Subtract(start_dttm).TotalMinutes));
                    return;
                }
                if (((int)end_dttm.Subtract(start_dttm).TotalMinutes) > int.Parse(txtTime.Text))
                {
                    Util.Alert("SFU3232"); //종료시간이 기존 종료시간 보다 이후입니다.
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

                }
                else if (rdoEnd.IsChecked == true) //종료시간에서 빼기
                {
                    if (end_dttm.AddMinutes((-1) * minute) < DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                    {
                        Util.MessageValidation("SFU3233"); //시작시간이 기존 시작시간 보다 이전입니다.
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
            try
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

                dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("START_DTTM_HIDDEN", typeof(string));

                row = dt.NewRow();
                row["EQPTID"] = _EQPTID;
                row["START_DTTM_HIDDEN"] = _START_DTTM;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSS_HIST_BY_DATE", "RQUST", "RSLT", dt);
                if (result.Rows.Count != 0)
                {
                    initLoss = Convert.ToString(result.Rows[0]["LOSS_CODE"]);
                    initLossName = Convert.ToString(result.Rows[0]["LOSS_NAME"]);
                    initLossDetl = Convert.ToString(result.Rows[0]["LOSS_DETL_CODE"]);
                    initLossDetlName = Convert.ToString(result.Rows[0]["LOSS_DETL_NAME"]);
                    initEioStat = Convert.ToString(result.Rows[0]["EIOSTAT"]);
                    _LOSS_SEQ = int.Parse(Convert.ToString(result.Rows[0]["PRE_LOSS_SEQNO"]));
                    //  _PRE_LOSS_SEQNO = int.Parse(Convert.ToString(result.Rows[0]["LOSS_SEQNO"]));
                }
            }
            catch (Exception ex)
            {

            }


        }

        private void initCombo()
        {
            try
            {   //원인설비
                string[] sFilter = { _EQPTID };
                if (string.Equals(strAreaType, "P"))   //C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용) 김준겸 A
                {
                    combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                }
                else
                {
                    combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
                }

                //Loss분류
                C1ComboBox[] cboLossChild = { cboLossDetl };
                combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild);


                //부동내용
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboOccurEqpt };
                string[] sFilter2 = { _AREA, _PROCID };
                combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter2);

                if (!initLoss.Equals(""))
                {
                    cboLoss.SelectedValue = initLoss;
                }

                if (!initLossDetl.Equals(""))
                {
                    cboLossDetl.SelectedValue = initLossDetl;
                }

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

            DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "START_DTTM", DateTime.ParseExact(_START_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            DataTableConverter.SetValue(dgSplit.Rows[0].DataItem, "END_DTTM", DateTime.ParseExact(_END_DTTM, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (!AddRowValidation())
            {
                return;
            }
            //commttiedFlag = false;
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
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSS", cboLoss.SelectedValue.ToString().Equals("SELECT") ? initLoss : cboLoss.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSS_NAME", cboLoss.SelectedValue.ToString().Equals("SELECT") ? initLossName : cboLoss.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSSDETAIL", cboLossDetl.SelectedValue.ToString().Equals("SELECT") ? initLossDetl : cboLossDetl.SelectedValue.ToString());
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "LOSSDETAIL_NAME", cboLossDetl.SelectedValue.ToString().Equals("SELECT") ? initLossDetlName : cboLossDetl.Text);
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "REMARK", txtRemark.Text);
            if (string.Equals(strAreaType, "P"))
            {
                DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "EQPTNAME", cboOccurEqpt.SelectedValue.ToString());
                DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "WORKUSER", txtPerson.Text);
            }

            dgSplitInfo.IsReadOnly = true;
            txtPerson.IsReadOnly = true;
            resetSplitTime(starttime, endtime);
            commttiedFlag = false;
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
            //작업 담당자 필수 : 
            if (string.Equals(strAreaType, "P"))
            {
                if (cboOccurEqpt.Text.ToString().Equals("-SELECT-")) // C20200728 - 000321 원인설비 - SELECT - 초기화 설정(PACK만 적용)
                {
                    Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                    return false;
                }
                if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU1842"); //작업자를 선택 하세요.
                    return false;
                }
            }
            else
            {
                if (!cboLoss.SelectedValue.Equals("RUN"))
                {
                    if (cboOccurEqpt.Text.Equals("-SELECT-"))
                    {
                        return false;
                    }

                }
            }
            return true;
        }
        private void resetSplitTime(string starttime, string endtime)
        {
            if (rdoStart.IsChecked == true)
            {
                DateTime dt = DateTime.Parse(endtime);
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
                Util.MessageValidation("SFU3496"); //추가된 상태가 없습니다.
                return;
            }


            int total = 0;
            for (int i = 0; i < dgSplitInfo.GetRowCount(); i++)
            {
                total += Util.NVC_Int(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "MINUTE"));
            }
            if (total != int.Parse(txtTime.Text.ToString()))
            {
                Util.MessageValidation("SFU3497"); //총 시간을 다 입력해주세요
                return;
            }


            DataSet ds = new DataSet();

            DataTable inData = ds.Tables.Add("INDATA");
            inData.Columns.Add("EQPTID", typeof(string));
            inData.Columns.Add("FROM_SEQNO", typeof(Int32));
            inData.Columns.Add("TO_SEQNO", typeof(Int32));
            inData.Columns.Add("USE_FLAG", typeof(string));
            inData.Columns.Add("USERID", typeof(string));

            DataRow row = inData.NewRow();
            row["EQPTID"] = _EQPTID;
            row["FROM_SEQNO"] = _LOSS_SEQ;
            row["TO_SEQNO"] = _LOSS_SEQ;
            row["USE_FLAG"] = "N";
            row["USERID"] = LoginInfo.USERID;

            inData.Rows.Add(row);


            DataTable inLoss = ds.Tables.Add("INLOSS");
            inLoss.Columns.Add("EQPTID", typeof(string));
            inLoss.Columns.Add("STRT_DTTM", typeof(DateTime));
            inLoss.Columns.Add("END_DTTM", typeof(DateTime));
            inLoss.Columns.Add("LOSS_SEQNO", typeof(Int32));
            inLoss.Columns.Add("TO_LOSS_SEQNO", typeof(Int32));
            inLoss.Columns.Add("LOSS_CODE", typeof(string));
            inLoss.Columns.Add("LOSS_DETL_CODE", typeof(string));
            inLoss.Columns.Add("NOTE", typeof(string));
            inLoss.Columns.Add("USERID", typeof(string));
            if (string.Equals(strAreaType, "P"))
            {
                inLoss.Columns.Add("EQPTNAME", typeof(string));
                inLoss.Columns.Add("WORKUSER", typeof(string));
            }

            for (int i = 0; i < dgSplitInfo.Rows.Count; i++)
            {
                row = inLoss.NewRow();                
                row["STRT_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "START_DTTM_HIDDEN")));
                row["END_DTTM"] = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "END_DTTM_HIDDEN")));
                row["LOSS_SEQNO"] = _LOSS_SEQ;
                row["TO_LOSS_SEQNO"] = _MAX_SEQNO + i + 1;
                row["LOSS_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS"))).Equals("") ? initLoss : Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS")));
                row["LOSS_DETL_CODE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSSDETAIL"))).Equals("") ? initLossDetl : Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSSDETAIL")));
                row["NOTE"] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "REMARK"))).Equals("") ? null : Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "REMARK")));                
                row["EQPTID"] = _EQPTID;
                row["USERID"] = LoginInfo.USERID;
                if (string.Equals(strAreaType, "P"))
                {
                    row["EQPTNAME"] = Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "EQPTNAME"));
                    row["WORKUSER"] = txtPerson.Tag; //Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "WORKUSER"));
                }                
                inLoss.Rows.Add(row);
            }

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (string.Equals(strAreaType, "P"))
                {
                    new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_SPLIT_POPUP_PACK", "INDATA, INLOSS", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1270"); //저장되었습니다.
                                                         
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
                else
                {
                    new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_SPLIT_POPUP", "INDATA, INLOSS", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1270"); //저장되었습니다.
                                                         //win.btnSearch_Click(null, null);
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
            //작업자 초기화.
            txtPerson.Text = "";
            txtPerson.Tag = null;
            txtPerson.IsReadOnly = false;
            //비고 초기화
            if (string.Equals(strAreaType, "P"))
            {
                txtRemark.Text = string.Empty;
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
                Util.MessageValidation("SFU3234"); //더이상 추가할 수 없습니다.
                return false;
            }


            if ((sumMinute + int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")))) != initMiunte)
            {
                if (commttiedFlag == false)
                {
                    Util.MessageValidation("SFU3235"); //경과시간을 설정해주세요
                    return false;
                }
            }

            if (dgSplitInfo.GetRowCount() == 0)
            {
                if (commttiedFlag == false)
                {
                    Util.Alert("SFU3235"); //경과시간을 설정해주세요
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
                Util.MessageException(ex);
            }
        }

        private int GetMergeLossCnt()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("WRK_DATE", typeof(string));
                dt.Columns.Add("PRE_LOSS_SEQNO", typeof(Int64));

                DataRow row = dt.NewRow();
                row["EQPTID"] = _EQPTID;
                row["WRK_DATE"] = _WORK_DATE;
                row["PRE_LOSS_SEQNO"] = _LOSS_SEQ;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_PRE_LOSS_CNT", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count != 0)
                {
                    return int.Parse(Convert.ToString(result.Rows[0]["MERGE_CNT"]));
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private void GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    strAreaType = Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtPerson_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                txtPerson.Tag = null;
            }
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void InitUser()
        {
            bg_txtRemark.SetValue(Grid.ColumnSpanProperty, 6);

            // 뒷배경 비활성화
            bg_txtUser.Visibility = Visibility.Collapsed;
            bg_txtPerson.Visibility = Visibility.Collapsed;
            bg_btnPerson.Visibility = Visibility.Collapsed;

            // 텍스트 박스,버튼 비활성화
            txtUser.Visibility = Visibility.Collapsed;
            txtPerson.Visibility = Visibility.Collapsed;
            btnPerson.Visibility = Visibility.Collapsed;

            //하단 Grid 작업자 표시 비활성화
            WORKUSER.Visibility = Visibility.Collapsed;
            EQPTNAME.Visibility = Visibility.Collapsed;
        }

        #region < 해체 담당자 찾기 >

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtPerson.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            //grdMain.Children.Add(wndPerson); _grid     
            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPerson.Text = wndPerson.USERNAME;
                txtPerson.Tag = wndPerson.USERID;
            }
        }


        #endregion

        #endregion

    }
}
