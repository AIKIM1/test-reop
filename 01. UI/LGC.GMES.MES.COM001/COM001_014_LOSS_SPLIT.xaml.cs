/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.09.14  김준겸 A :  CSR ID : C20200728-000321 PACK 부서 해당 (작업자 추가, 원인설비 초기화 -SELECT-설정)
  2022.08.23 윤지해 CSR ID C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건.
  2023.06.05 윤지해 CSR ID E20230330-001442 원인설비별 LOSS LV2, LV3 등록 로직 추가(PACK, 소형 제외)
  2023.06.25 조영대 : 설비 Loss Level 2 Code 사용 체크 및 변환
  2023.07.13 서인석 긴급배포(장현석 팀장)   입력 종료시간이 시작시간보다 작을 경우 validation 로직 추가(엔터키를 치지 않았을 경우 버그) 
  2023.07.26 김대현 CSR ID E20230711-000645 공용 PC 사용자(P) 권한 체크로직 추가
  2024.03.19 김대현 공용 PC 사용자로 비조업 Split 등록시 Validation
  2024.10.25 복현수 MES 리빌딩 PJT : TRANSACTION_SERIAL_NO 파라미터 받아오는 부분 추가, DA_EQP_SEL_EQPTLOSS_MAX_SEQNO 인데이터로 추가
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
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_LOSS_SPLIT : C1Window, IWorkArea
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
        string _TRAN_SEQ = ""; //2024.10.20 MES 리빌딩 PJT

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

        string strAttr1 = "";
        string strAttr2 = "";
        bool bUseEqptLossAppr = false;
        // 2023.06.05 윤지해 CSR ID E20230330-001442 
        string occurEqptFlag = string.Empty;

        DataTable _dtBeforeSet = new DataTable();
        public COM001_014_LOSS_SPLIT()
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
                occurEqptFlag = Util.NVC(tmps[9], "N"); // 2023.06.05 윤지해 CSR ID E20230330-001442 추가
                _TRAN_SEQ = Util.NVC(tmps[10]);
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

            GetEqptLossApprAuth();
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
            {
                //원인설비
                // 2023.06.05 윤지해 CSR ID E20230330-001442 원인설비 콤보박스 세팅 로직 변경
                string[] sFilter = { _EQPTID };
                if (string.Equals(strAreaType, "P"))   //C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용) 김준겸 A
                {
                    combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                    cboOccurEqpt.SelectedValueChanged -= cboOccurEqpt_SelectedValueChanged;
                }
                else if (occurEqptFlag.Equals("Y"))
                {
                    combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "cboOccurFcrEqpt");
                    cboOccurEqpt.SelectedValueChanged += cboOccurEqpt_SelectedValueChanged;
                }
                else
                {
                    combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
                    cboOccurEqpt.SelectedValueChanged -= cboOccurEqpt_SelectedValueChanged;
                }

                //Loss분류
                // 2023.06.05 윤지해 CSR ID E20230330-001442 비조업, 비부하 관련 로직, 원인설비별 LOSS 분류 가져오기 추가
                C1ComboBox[] cboLossChild = { cboLossDetl };
                string[] sFilterLoss = { _AREA, _PROCID, _EQPTID, occurEqptFlag, string.Empty};

                if (occurEqptFlag.Equals("Y"))
                {
                    combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
                }
                else
                {
                    if (bUseEqptLossAppr)
                    {
                        combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeAll");
                    }
                    else
                    {
                        combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
                    }
                }
                //combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild);

                //부동내용
                // 2023.06.05 윤지해 CSR ID E20230330-001442 부동내용 콤보박스 세팅 로직 변경
                string occurEqptValue = (string.IsNullOrEmpty(cboOccurEqpt.SelectedValue.ToString()) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboOccurEqpt };
                string[] sFilter2 = { _AREA, _PROCID };
                string[] sFilter3 = { cboLoss.SelectedValue.ToString(), _EQPTID, occurEqptValue };
                if(occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.ToString().Equals("SELECT") || string.IsNullOrEmpty(cboOccurEqpt.SelectedValue.ToString())))
                {
                    combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "cboOccrLossDetl");
                }
                else
                {
                    combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter2);
                }

                if (!initLoss.Equals(""))
                {
                    cboLoss.SelectedValue = initLoss;
                }

                if (!initLossDetl.Equals(""))
                {
                    cboLossDetl.SelectedValue = initLossDetl;
                }

                cboLoss.SelectedValueChanged += cboLoss_SelectedValueChanged;
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
            //DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "MINUTE", Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")));

            
            DateTime st_dttm = DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM").ToString());
            DateTime ed_dttm = DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM").ToString());            
            string minute = ((int)(ed_dttm.Subtract(st_dttm).TotalMinutes)).ToString();
            DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "MINUTE", minute);

            //DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "EQPTNAME", cboOccurEqpt.Text);
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
            // 2023.06.05 윤지해 CSR ID E20230330-001442 수정(NULL값 허용) PACK이 아닐 경우
            else
            {
                DataTableConverter.SetValue(dgSplitInfo.CurrentRow.DataItem, "EQPTNAME", cboOccurEqpt.SelectedValue.ToString().Equals("SELECT") ? string.Empty : cboOccurEqpt.SelectedValue.ToString());

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
                // 20210308 | 김건식 | ESWA PACK BM 3분 미만 분할 Validataion 기능 추가
                // BM을 3분 미만으로 분할할 경우OEE 데이터에 BM 대신 wait 상태로 보고되므로 데이터 신뢰성 낮아지므로 제한.
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    // EL012 : BM(3분이상)
                    int iMinute = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE"))) ? 0 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")));
                    /*
                    if (cboLoss.SelectedValue.ToString().Equals(Util.ConvertEqptLossLevel2Change("LC008")) && iMinute < 3) 
                    {
                        Util.MessageInfo("SFU8336"); // BM은 3분미만으로 분할할수 없습니다.
                        return false;
                    }
                    */
                }
            }
            // 2023.06.05 윤지해 CSR ID E20230330-001442 NULL 허용으로 주석처리함
            // 기존에 PACK만 -SELECT - 값이 존재 > SELECT에 대한 VALIDATION을 PACK인 경우와 아닌경우 중복으로 처리하고 있음
            //else
            //{
            //    if (!cboLoss.SelectedValue.Equals("RUN"))
            //    {
            //        if (cboOccurEqpt.Text.Equals("-SELECT-"))
            //        {
            //            return false;
            //        }
            //    }
            //}
            if (cboLoss.Text.Equals(""))
            {
                return false;
            }

            if (cboLossDetl.Text.Equals(""))
            {
                return false;
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
            bool chkUpperLossCode = true;

            if (bUseEqptLossAppr)
            {
                if (ValidationChkDDay().Equals("AUTH_ONLY"))
                {
                    for (int i = 0; i < dgSplitInfo.Rows.Count; i++)
                    {
                        string lossCode = Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "LOSS"));
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LOSS_CODE"] = lossCode;
                        RQSTDT.Rows.Add(dr);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                        if (result.Rows.Count != 0)
                        {
                            string upprLossCode = Convert.ToString(result.Rows[0]["UPPR_LOSS_CODE"]);
                            if (upprLossCode.Equals("10000"))
                            {
                                if (LoginInfo.USERTYPE.Equals("P"))
                                {
                                    Util.MessageValidation("SFU5179"); // 공용 PC 사용자권한으로는 Loss 등록이 불가합니다. \n개인권한 사용자로 로그인하여 등록해주시기 바랍니다. 
                                    return;
                                }
                                chkUpperLossCode = false;
                                //Util.MessageValidation("SFU5177"); // 비조업에 해당하는 Loss 분류는 '설비 Loss 수정' 화면을 통해 수정가능합니다. MSG 수정
                                //return;
                            }
                        }
                    }
                }

                if (ValidationChkDDay().Equals("NO_REG"))
                {
                    string strParam = (Convert.ToDouble(strAttr2) + 1).ToString();
                    Util.MessageValidation("SFU5180", ObjectDic.Instance.GetObjectName(strParam));  // strAttr2 + 일이전 설비 Loss는 등록 불가합니다.
                    return;
                }
            }

            if (dgSplitInfo.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU3496"); //추가된 상태가 없습니다.
                return;
            }
            
            if ((DataTableConverter.GetValue(dgSplitInfo.Rows[0].DataItem, "START_DTTM").ToString() != txtRunStartTime.Text) ||
                (DataTableConverter.GetValue(dgSplitInfo.Rows[dgSplitInfo.GetRowCount() - 1].DataItem, "END_DTTM").ToString() != txtRunEndTime.Text))
            {
                Util.MessageValidation("SFU3497"); //총 시간을 다 입력해주세요
                return;
            }

            //int total = 0;
            //for (int i = 0; i < dgSplitInfo.GetRowCount(); i++)
            //{
            //    total += Util.NVC_Int(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "MINUTE"));
            //}
            //if (total != int.Parse(txtTime.Text.ToString()))
            //{
            //    Util.MessageValidation("SFU3497"); //총 시간을 다 입력해주세요
            //    return;
            //}


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
            inLoss.Columns.Add("EQPTNAME", typeof(string)); // 2023.06.05 윤지해 CSR ID E20230330-001442 위치 수정
            if (string.Equals(strAreaType, "P"))
            {
                //inLoss.Columns.Add("EQPTNAME", typeof(string));
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
                row["EQPTNAME"] = Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "EQPTNAME"));  // 2023.06.05 윤지해 CSR ID E20230330-001442 위치 수정
                if (string.Equals(strAreaType, "P"))
                {
                    //row["EQPTNAME"] = Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "EQPTNAME"));
                    row["WORKUSER"] = txtPerson.Tag; //Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "WORKUSER"));
                }                
                inLoss.Rows.Add(row);
            }

            try
            {
                if (chkUpperLossCode)
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
                else
                {
                    inLoss.Columns.Add("WRK_DATE", typeof(string));
                    for (int i = 0; i < inLoss.Rows.Count; i++)
                    {
                        inLoss.Rows[i]["WRK_DATE"] = _WORK_DATE;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = _AREA;
                    Parameters[1] = inLoss;

                    //결재 요청 Popup 창 Open
                    COM001_014_SPLIT_APPR_ASSIGN popup = new COM001_014_SPLIT_APPR_ASSIGN();
                    popup.FrameOperation = FrameOperation;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(Popup_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            COM001_014_SPLIT_APPR_ASSIGN window = sender as COM001_014_SPLIT_APPR_ASSIGN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.Close();
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
            DateTime ed_dttm = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM")));

            if (ed_dttm > InitEndTime)
            {
                Util.Alert("SFU3232"); //종료시간이 기존 종료시간 보다 이후입니다.
                return false;
            }

            if (ed_dttm <= InitStartTime)
            {
                Util.Alert("SFU3231"); //종료시간이 시작시간보다 이전입니다.
                return false;
            }

            if (dgSplitInfo.GetRowCount() > 0)
            {
                if (DataTableConverter.GetValue(dgSplitInfo.Rows[dgSplitInfo.GetRowCount() - 1].DataItem, "END_DTTM").ToString() == txtRunEndTime.Text)
                {
                    Util.MessageValidation("SFU3234"); //더이상 추가할 수 없습니다.
                    return false;
                }
            }
            
            DateTime st_dttm2 = DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "START_DTTM").ToString());
            DateTime ed_dttm2 = DateTime.Parse(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "END_DTTM").ToString());
            int minute = (int)(ed_dttm2.Subtract(st_dttm2).TotalMinutes);

            if (minute < 0)
            {
                Util.MessageValidation("SFU3231"); //종료시간이 시작시간보다 이전입니다.
                return false;
            }

            /*하기 로직은 왜 있는지 모르는 로직으로 DataInsert 오류 유발하는 코드이므로 제외함*/

            //int initMiunte = int.Parse(txtTime.Text);
            //int sumMinute = 0;
            //for (int i = 0; i < dgSplitInfo.GetRowCount(); i++)
            //{
            //    sumMinute += int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplitInfo.Rows[i].DataItem, "MINUTE")));
            //}

            //if (initMiunte == sumMinute)
            //{
            //    Util.MessageValidation("SFU3234"); //더이상 추가할 수 없습니다.
            //    return false;
            //}


            //if ((sumMinute + int.Parse(Util.NVC(DataTableConverter.GetValue(dgSplit.Rows[0].DataItem, "MINUTE")))) != initMiunte)
            //{
            //    if (commttiedFlag == false)
            //    {
            //        Util.MessageValidation("SFU3235"); //경과시간을 설정해주세요
            //        return false;
            //    }
            //}

            //if (dgSplitInfo.GetRowCount() == 0)
            //{
            //    if (commttiedFlag == false)
            //    {
            //        Util.Alert("SFU3235"); //경과시간을 설정해주세요
            //        return false;
            //    }
            //}

            return true;
        }

        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            COM001_014_LOSS_DETL wndLossDetl = new COM001_014_LOSS_DETL();
            wndLossDetl.FrameOperation = FrameOperation;

            // 2023.06.05 윤지해 CSR ID E20230330-001442 Parameter BM/PD 조회내역 제외 또는 포함
            if (wndLossDetl != null)
            {
                #region 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                object[] Parameters = new object[6];
                Parameters[0] = _AREA;//Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = _PROCID;//Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = _EQPTID;//Convert.ToString(cboEquipment.SelectedValue);
                // 2022.08.23 YJH 자동차/ESS 조립 공정, PD(품질부동,EL014)만 적용
                //if (string.Equals(strAreaType, "A") && (LoginInfo.CFG_AREA_ID.ToString().StartsWith("A") || LoginInfo.CFG_AREA_ID.ToString().StartsWith("S")))
                //{
                //    Parameters[3] = cboLoss.SelectedValue.ToString().Equals("EL014") ? cboLoss.SelectedValue.ToString() : "";
                //}
                //else
                //{
                //    Parameters[3] = "";
                //}
                Parameters[3] = (string.IsNullOrEmpty(cboLoss.Text) || cboLoss.SelectedValue.ToString().Equals("SELECT")) ? "" : cboLoss.SelectedValue.ToString();
                Parameters[4] = occurEqptFlag;
                Parameters[5] = (string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                #endregion

                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            COM001_014_LOSS_DETL window = sender as COM001_014_LOSS_DETL;
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

        private string GetNowDate()
        {
            string nowDate = string.Empty;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

            nowDate = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

            return nowDate;
        }

        private string ValidationChkDDay()
        {
            string bDDay = string.Empty;
            DateTime dtNowDay = DateTime.ParseExact(GetNowDate(), "yyyyMMdd", null);
            DateTime dtSearchDay = DateTime.ParseExact(_WORK_DATE, "yyyyMMdd", null);

            if ((dtNowDay - dtSearchDay).TotalDays >= 0 && (dtNowDay - dtSearchDay).TotalDays < Convert.ToDouble(strAttr1))
            {
                bDDay = "ALL";
            }
            else if ((dtNowDay - dtSearchDay).TotalDays >= Convert.ToDouble(strAttr1) && (dtNowDay - dtSearchDay).TotalDays <= Convert.ToDouble(strAttr2))
            {
                bDDay = "AUTH_ONLY";
            }
            else
            {
                bDDay = "NO_REG";
            }

            return bDDay;
        }

        private void GetEqptLossApprAuth()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = _AREA;
                drRQSTDT["COM_TYPE_CODE"] = "EQPT_LOSS_CHG_APPR_USE_SYSTEM";    // 해당 동, 시스템의 '설비 Loss 수정' 화면 사용 여부 확인
                drRQSTDT["COM_CODE"] = LoginInfo.SYSID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (dtRSLTDT != null && dtRSLTDT.Rows.Count > 0)
                {
                    bUseEqptLossAppr = true;

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR1"])))
                    {
                        strAttr1 = Util.NVC(dtRSLTDT.Rows[0]["ATTR1"]);
                    }
                    else
                    {
                        strAttr1 = "1";
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR2"])))
                    {
                        strAttr2 = Util.NVC(dtRSLTDT.Rows[0]["ATTR2"]);
                    }
                    else
                    {
                        strAttr2 = "7";
                    }
                }
                else
                {
                    bUseEqptLossAppr = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            //EQPTNAME.Visibility = Visibility.Collapsed;   // 2023.06.05 윤지해 CSR ID E20230330-001442 주석처리
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

        #region 2023.06.05 윤지해 CSR ID E20230330-001442 원인설비 변경 시 이벤트
        private void cboOccurEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // LOSS LV2 변경
            CommonCombo combo = new CommonCombo();
            // PACK, 소형이 아니고 원인설비별 LOSS 등록 여부 체크
            if (occurEqptFlag.Equals("Y"))
            {
                if (!(string.IsNullOrEmpty(cboOccurEqpt.SelectedValue.ToString()) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))   // 원인설비 선택했을 경우 : BM/PD만 LIST-UP
                {
                    string[] sFilterLoss = { _EQPTID, cboOccurEqpt.SelectedValue.ToString(), string.Empty };
                    combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeOccurEqpt");
                }
                else // BM/PD 제외 LIST-UP
                {
                    string[] sFilterLoss = { _AREA, _PROCID, _EQPTID, occurEqptFlag, string.Empty };
                    combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
                }
            }
        }

        private void cboLoss_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLoss.Text.Equals("-SELECT-") || !string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()))
            {
                string cboLossText = cboLoss.Text.Equals("") || cboLoss.Text.Equals("-SELECT-") ? string.Empty : cboLoss.SelectedValue.ToString();
                string occurEqptValue = (string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.Text.Equals("-SELECT-")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboOccurEqpt };
                string[] sFilter2 = { _AREA, _PROCID };
                string[] sFilter3 = { cboLossText, _EQPTID, occurEqptValue };
                if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.Text.Equals("-SELECT-") || string.IsNullOrEmpty(cboOccurEqpt.Text)))
                {
                    combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "cboOccrLossDetl");
                }
                else
                {
                    combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter2);
                }
            }
            else
            {
                cboLossDetl.SelectedIndex = 0;
            }
        }
        #endregion
        #endregion
    }
}