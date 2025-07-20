/*************************************************************************************
 Created Date : 2021.01.06
      Creator : 김태균
   Decription : 설비 LOSS 등록
--------------------------------------------------------------------------------------
 [Change History]
 2022.01.24 강동희 : 원인설비 Setting 조건 변경
 2022.05.25 조영대 : 설비콤보 SUB EQUIPMENT 선택시 처리.
 2022.07.13 조영대 : 메인설비에서 대표설비로 수정
 2022.08.08 조영대 : 작업조 있을경우 정렬 수정(날짜 DESC)
 2022.08.17 이정미 : GetEqptLossDetailList INDATA 수정 - 전극조립 공통 BIZ 사용하여 LOSS 시간비교와 정렬 INDATA 추가
 2022.08.23 조영대 : 비즈명 변경 - DA_EQP_SEL_EQPTLOSSDETAIL ===> DA_EQP_SEL_EQPTLOSSDETAIL_FOR_UNIT
 2022.08.31 강동희 : Loss Code 상세 조회 조건 추가
 2022.09.29 조영대 : Loss 목록 선택시 두줄이상시 처리
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_066 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sStartTime = string.Empty;
        private string RunSplit = string.Empty; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분 - 활성화는 사용 안함
        string sSearchDay = string.Empty;

        Hashtable hash_loss_color = new Hashtable();

        DataTable dtMainList = new DataTable();
        DataTable AreaTime;
        DataTable dtShift;

        string sMainEqptID;
        DataSet dsEqptTimeList = null;
        Util _Util = new Util();
        Hashtable org_set;

        List<string> liProcId;

        int iEqptCnt;

        public FCS002_066()
        {
            InitializeComponent();

            InitCombo();
            InitGrid();
            GetLossColor();
            GetStartTime();
            GetShiftTime();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLaneChild = { cboEqp };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", cbChild: cboLaneChild);

            //string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "5,D" };  //설정된 설비만 Loss 모니터링 가능
            //C1ComboBox[] cboEqpKindChild = { cboEqp, cboFcrGroup };
            //_combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            string[] sFilterEqpType = { "DEG,EOL" };  //설정된 설비만 Loss 모니터링 가능(DEGAS, EOL)
            C1ComboBox[] cboEqpKindChild = { cboEqp, cboFcrGroup };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
            C1ComboBox[] cboEqpChild = { cboLossDetl, cboLatest, cboCauEqp };

            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "LOSSEQP", cbParent: cboEqpParent, cbChild: cboEqpChild);

            //string[] sFilterShift = { "COMBO_SHIFT" };
            _combo.SetCombo(cboShift, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_SHIFT");

            _combo.SetCombo(cboFailure, CommonCombo_Form.ComboStatus.NA, sCase: "FCR", sFilter: new string[] { "F" });
            _combo.SetCombo(cboCause, CommonCombo_Form.ComboStatus.NA, sCase: "FCR", sFilter: new string[] { "C" });
            _combo.SetCombo(cboResolution, CommonCombo_Form.ComboStatus.NA, sCase: "FCR", sFilter: new string[] { "R" });

            C1ComboBox[] cboLossChild = { cboLossDetl };
            _combo.SetCombo(cboLoss, CommonCombo_Form.ComboStatus.SELECT, sCase: "EQPLOSS", cbChild: cboLossChild);

            C1ComboBox[] cboLossDetlParent = { cboLoss };
            _combo.SetCombo(cboLossDetl, CommonCombo_Form.ComboStatus.NA, sCase: "EQPLOSSDETAIL", cbParent: cboLossDetlParent);

            C1ComboBox[] cboLatestParent = { cboEqp };
            _combo.SetCombo(cboLatest, CommonCombo_Form.ComboStatus.NA, sCase: "LASTLOSSCODE", cbParent: cboLatestParent);

            cboLatest.SelectedIndexChanged += cboLatest_SelectedIndexChanged;

            //20220124_원인설비 Setting 조건 변경 START
            //C1ComboBox[] cboCauEqpParent = { cboEqp };
            C1ComboBox[] cboCauEqpParent = { cboEqp, cboLane };
            _combo.SetCombo(cboCauEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "OCCUREQP", cbParent: cboCauEqpParent);
            //20220124_원인설비 Setting 조건 변경 END

            C1ComboBox[] cboFcrGroupParent = { cboEqpKind };
            _combo.SetCombo(cboFcrGroup, CommonCombo_Form.ComboStatus.NA, sCase: "FCRGROUP", cbParent: cboFcrGroupParent);

            //cboFcrGroup.SelectedIndexChanged += cboFcrGroup_SelectedIndexChanged
            cboFcrGroup.SelectedValueChanged += cboFcrGroup_SelectedValueChanged;

        }

        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {
                int gridRowCount = 500;

                _grid.Width = 3000;

                _grid.Height = gridRowCount * 15;

                for (int i = 0; i < 361; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = GridLength.Auto;
                    }
                    else { gridCol1.Width = new GridLength(3); }

                    _grid.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < gridRowCount; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow1);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetAreaTime();
        }

        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border aa = sender as Border;

            org_set = aa.Tag as Hashtable;

            if (e.ChangedButton == MouseButton.Right)
            {
                if (!org_set["STATUS"].ToString().Equals("R"))
                {
                    ContextMenu menu = this.FindResource("_gridMenu") as ContextMenu;
                    menu.PlacementTarget = sender as Border;
                    menu.IsOpen = true;

                    for (int i = 0; i < menu.Items.Count; i++)
                    {
                        MenuItem item = menu.Items[i] as MenuItem;

                        switch (item.Name.ToString())
                        {
                            case "LossDetail":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss내역보기");
                                item.Click -= lossDetail_Click;
                                item.Click += lossDetail_Click;
                                break;

                                //case "LossSplit":
                                //    item.Header = ObjectDic.Instance.GetObjectName("Loss분할");
                                //    item.Click -= lossSplit_Click;
                                //    item.Click += lossSplit_Click;
                                //    break;
                        }
                    }
                }
                if (RunSplit.Equals("Y"))
                {
                    if (org_set["STATUS"].ToString().Equals("R")) //추가
                    {
                        string startTime = txtStartHidn.Text;
                        string endTime = txtEndHidn.Text;
                        if (startTime.Equals("") || endTime.Equals(""))
                        {
                            return;
                        }

                        DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime).CopyToDataTable();
                        if (dt.Select("EIOSTAT <> 'R'").Count() > 0)
                        {
                            Util.MessageValidation("SFU3204"); //운영설비 사이에 Loss가 존재합니다.
                            btnReset_Click(null, null);
                            return;
                        }

                        FCS002_066_RUN_SPLIT wndPopup = new FCS002_066_RUN_SPLIT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[9];
                            Parameters[0] = org_set["EQPTID"].ToString();
                            Parameters[1] = org_set["TIME"].ToString();
                            Parameters[2] = startTime;
                            Parameters[3] = endTime;
                            Parameters[4] = LoginInfo.CFG_AREA_ID;
                            Parameters[5] = Util.GetCondition(ldpDatePicker); //ldpDatePicker.SelectedDateTime.ToShortDateString();
                            Parameters[6] = this;
                            //Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                            //Parameters[8] = cboProcess.SelectedValue.ToString();

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }
            }
            else
            {
                // 2022-07-08 : 메세지창 막음(기경원책임 요청)
                //if (!org_set["EQPTID"].ToString().Equals(sMainEqptID))//메인설비 아닌경우 선택안되도록
                //{
                //    Util.AlertInfo("SFU2863");
                //}

                if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
                {
                    btnReset_Click(null, null);
                }
                else
                {
                    setMapColor(org_set["TIME"].ToString(), "MAP");
                }
            }
        }

        private void lossSplit_Click(object sender, RoutedEventArgs e)
        {
            if (!org_set["STATUS"].ToString().Equals("R")) //추가
            {
                string startTime = txtStartHidn.Text;
                string endTime = txtEndHidn.Text;
                if (startTime.Equals("") || endTime.Equals(""))
                {
                    return;
                }

                DataTable tmp = new DataTable();
                tmp.Columns.Add("EQPTID", typeof(string));
                tmp.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));

                DataRow dr = tmp.NewRow();
                dr["EQPTID"] = org_set["EQPTID"].ToString().Substring(1);
                dr["STRT_DTTM_YMDHMS"] = startTime;
                tmp.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_END_DTTM", "RQSTDT", "RSLTDT", tmp);
                if (result.Rows.Count != 0)
                {
                    if (Convert.ToString(result.Rows[0]["END_DTTM"]).Equals(""))
                    {
                        Util.MessageValidation("SFU4244"); // 부동이 끝나지 않아 데이터를 분할 할 수 없습니다.
                        return;
                    }
                }

                DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime).CopyToDataTable();
                if (dt.Select("EIOSTAT = 'R'").Count() > 0)
                {
                    Util.MessageValidation("SFU3511"); //Run상태가 존재합니다
                    btnReset_Click(null, null);
                    return;
                }

                if (dt.Select().Count() > 1)
                {
                    Util.MessageValidation("SFU3512"); //하나의 부동상태만 선택해주세요
                    btnReset_Click(null, null);
                    return;
                }

                FCS002_066_LOSS_SPLIT wndPopup = new FCS002_066_LOSS_SPLIT();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = org_set["EQPTID"].ToString();
                    Parameters[1] = org_set["TIME"].ToString();
                    Parameters[2] = startTime;
                    Parameters[3] = endTime;
                    Parameters[4] = LoginInfo.CFG_AREA_ID;
                    Parameters[5] = Util.GetCondition(ldpDatePicker);
                    Parameters[6] = this;
                    //Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                    //Parameters[8] = cboProcess.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopup_Closed2);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_066_SPLIT window = sender as FCS002_066_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void wndPopup_Closed2(object sender, EventArgs e)
        {
            FCS002_066_LOSS_SPLIT window = sender as FCS002_066_LOSS_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void wndFCR_Closed(object sender, EventArgs e)
        {
            FCS002_066_FCR window = sender as FCS002_066_FCR;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            resetMapColor();
            txtEqpName.Text = string.Empty;
            txtStart.Text = string.Empty;
            txtEnd.Text = string.Empty;
            txtStartHidn.Text = string.Empty;
            txtEndHidn.Text = string.Empty;
            txtLossNote.Text = string.Empty;

            cboLoss.SelectedIndex = 0;
            cboCauEqp.SelectedIndex = 0;
            cboLossDetl.SelectedIndex = 0;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLatest.SelectedIndex = 0;

        }

        private void lossDetail_Click(object sender, RoutedEventArgs e)
        {
            FCS002_066_TROUBLE wndPopup = new FCS002_066_TROUBLE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = org_set["EQPTID"].ToString();
                Parameters[1] = org_set["TIME"].ToString();
                Parameters[2] = cboShift.SelectedValue.ToString().Equals("") ? 20 : 10;
                Parameters[3] = Util.GetCondition(ldpDatePicker);//ldpDatePicker.SelectedDateTime.ToShortDateString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.Show()));
            }
        }

        private void cboLatest_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLatest.SelectedIndex > 0)
            {
                string[] sLastLoss = cboLatest.SelectedValue.ToString().Split('-');

                cboLoss.SelectedValue = sLastLoss[0];

                if (!string.IsNullOrEmpty(sLastLoss[1]))
                {
                    cboLossDetl.SelectedValue = sLastLoss[1];
                }
            }
        }

        private void cboFcrGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboFcrGroup.SelectedIndex > 0)
            {
                string[] sFCR = cboFcrGroup.SelectedValue.ToString().Split('|');

                if (sFCR.Length == 3)
                {
                    cboFailure.SelectedValue = sFCR[0];
                    cboCause.SelectedValue = sFCR[1];
                    cboResolution.SelectedValue = sFCR[2];
                }
            }
        }

        private void cboFcrGroup_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cboFcrGroup.SelectedValue != null && !string.IsNullOrEmpty(cboFcrGroup.SelectedValue.ToString()))
            {
                string[] sFCR = cboFcrGroup.SelectedValue.ToString().Split('|');

                if (sFCR.Length == 3)
                {
                    cboFailure.SelectedValue = sFCR[0];
                    cboCause.SelectedValue = sFCR[1];
                    cboResolution.SelectedValue = sFCR[2];
                }
            }
        }


        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sEqpt = Util.GetCondition(cboEqp, "SFU1153"); //설비를 선택하세요
            if (sEqpt.Equals("")) return;

            //초기화
            InitInsertCombo();
            ClearGrid();
            ClearControl();

            SelectProcess();
            GetEqptLossDetailList();
            GetEqptLossRawList();

            if (chkSub.IsChecked.Equals(true))
            {
                sMainEqptID = "A" + cboEqp.GetStringValue("MAIN_EQPTID");
            }
            else
            {
                sMainEqptID = "A" + Util.GetCondition(cboEqp);
            }

            sSearchDay = ldpDatePicker.SelectedDateTime.ToString("yyyyMMdd");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") > 0)
            {
                Util.MessageValidation("SFU3490"); //하나의 부동내역을 저장 할 경우 check box선택을 모두 해제 후 \r\n 한개의 행만 더블클릭  해주세요
                return;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return;
            }

            if (cboCauEqp.Text.ToString().Equals("-SELECT-")) // C20200728 - 000321 원인설비 - SELECT - 초기화 설정(PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return;
            }

            ValidateNonRegisterLoss("ONE");

            DataRow[] dtRow = dtMainList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_END <= '" + txtEndHidn.Text + "'", "");

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            string msg = "";

            if (!(string.IsNullOrEmpty(txtLossNote.Text)) && cboLoss.Text.Equals("-SELECT-") && cboLossDetl.Text.Equals("-SELECT-"))
            {
                msg = "SFU3441";//"해당 Loss의 비고 항목만 저장하시겠습니까?";

                //dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU1153"); //설비를 선택하세요

                // 대표설비로 저장.
                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");

                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
                dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
                dr["LOSS_CODE"] = null;
                //if (dr["LOSS_CODE"].Equals("")) return;
                dr["LOSS_DETL_CODE"] = null;
                dr["LOSS_NOTE"] = Util.GetCondition(txtLossNote);
                dr["SYMP_CODE"] = null;
                dr["CAUSE_CODE"] = null;
                dr["REPAIR_CODE"] = null;
                dr["OCCR_EQPTID"] = null;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {

                    if (result.ToString().Equals("OK"))
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK", "RQSTDT", "RSLTDT", RQSTDT);

                        btnSearch_Click(null, null);
                        chkT.IsChecked = false;
                        chkW.IsChecked = false;
                        chkU.IsChecked = false;

                        // 설비로스 변경 이력 저장
                        try
                        {
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        }
                        catch (Exception ex9)
                        {
                            Util.MessageException(ex9);
                        }

                        Util.AlertInfo("SFU1270");  //저장되었습니다.
                    }
                });
            }
            else
            {

                //dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU1153"); // 설비를 선택하세요

                // 대표설비로 저장.
                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");

                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
                dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
                dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); //LOSS는필수항목입니다
                if (dr["LOSS_CODE"].Equals("")) return;

                dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                if (dr["LOSS_DETL_CODE"].Equals(""))
                {
                    if (cboLossDetl.Items.Count > 1)
                    {
                        // 부동내용을 입력하세요.
                        Util.MessageValidation("SFU3631");
                        return;
                    }
                }

                dr["LOSS_NOTE"] = Util.GetCondition(txtLossNote);
                dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                dr["OCCR_EQPTID"] = Util.GetCondition(cboCauEqp);
                dr["USERID"] = LoginInfo.USERID;

                try
                {

                    RQSTDT.Rows.Add(dr);

                    if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true) //일괄등록이 하나라도 체크되어 있으면 Run 은 살린 상태로 개별 저장
                    {
                        if (chkT.IsChecked == true)
                        {
                            dr["CHKT"] = "T";
                        }
                        else
                        {
                            dr["CHKT"] = "";
                        }

                        if (chkW.IsChecked == true)
                        {
                            dr["CHKW"] = "W";
                        }
                        else
                        {
                            dr["CHKW"] = "";
                        }

                        if (chkU.IsChecked == true)
                        {
                            dr["CHKU"] = "U";
                        }
                        else
                        {
                            dr["CHKU"] = "";
                        }

                        // UPD 조건 다름..
                        // 설비로스 변경 이력 저장
                        try
                        {
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        }
                        catch (Exception ex9)
                        {
                            Util.MessageException(ex9);
                        }

                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_EACH", "RQSTDT", "RSLTDT", RQSTDT);

                    }
                    else
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS", "RQSTDT", "RSLTDT", RQSTDT);

                        // 설비로스 변경 이력 저장
                        try
                        {
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        }
                        catch (Exception ex9)
                        {
                            Util.MessageException(ex9);
                        }

                    }

                    //UPDATE 처리후 재조회
                    btnSearch_Click(null, null);
                    chkT.IsChecked = false;
                    chkW.IsChecked = false;
                    chkU.IsChecked = false;

                    dgDetail.ScrollIntoView(idx, 0);

                    Util.AlertInfo("SFU1270");  //저장되었습니다.
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnTotalSave_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Empty;

            if (cboCauEqp.Text.ToString().Equals("-SELECT-")) // C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return;
            }

            if (cboLoss.Text.ToString().Equals("-SELECT-") && cboLossDetl.Text.ToString().Equals("-SELECT-") && string.IsNullOrEmpty(txtLossNote.Text))
            {
                Util.MessageValidation("SFU3485"); //저장내역을 입력해주세요
                return;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") == 1)
            {
                Util.MessageValidation("SFU3487"); //일괄등록의 경우 한개 이상의 부동내역을 선택해주세요
                return;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");


            ValidateNonRegisterLoss("TOTAL");

            //해당Loss를 일괄로 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {

                    DataSet ds = new DataSet();
                    DataTable RQSTDT = ds.Tables.Add("INDATA");
                    //RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                    RQSTDT.Columns.Add("END_DTTM", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
                    RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CHKW", typeof(string));
                    RQSTDT.Columns.Add("CHKT", typeof(string));
                    RQSTDT.Columns.Add("CHKU", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    DataRow dr = null;

                    if (!(string.IsNullOrEmpty(txtLossNote.Text)) && cboLoss.Text.Equals("-SELECT-") && cboLossDetl.Text.Equals("-SELECT-"))
                    {
                        msg = "SFU3441";

                        for (int i = 0; i < dgDetail.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dr = RQSTDT.NewRow();

                                //dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU3514"); //설비는필수입니다.

                                // 대표설비로 저장.
                                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");

                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                                dr["LOSS_CODE"] = null;
                                dr["LOSS_DETL_CODE"] = null;
                                dr["LOSS_NOTE"] = Util.GetCondition(txtLossNote);
                                dr["SYMP_CODE"] = null;
                                dr["CAUSE_CODE"] = null;
                                dr["REPAIR_CODE"] = null;
                                dr["OCCR_EQPTID"] = null;
                                dr["USERID"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(dr);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL", "INDATA", null, ds);

                        // 설비로스 변경 이력 저장
                        try
                        {
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        }
                        catch (Exception ex9)
                        {
                            Util.MessageException(ex9);
                        }

                        btnSearch_Click(null, null);
                        chkT.IsChecked = false;
                        chkW.IsChecked = false;
                        chkU.IsChecked = false;

                        dgDetail.ScrollIntoView(idx, 0);

                        Util.MessageInfo("SFU1270");  //저장되었습니다.

                    }
                    else
                    {
                        dr = null;

                        for (int i = 0; i < dgDetail.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dr = RQSTDT.NewRow();

                                //dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU3514"); //설비는필수입니다.

                                // 대표설비로 저장.
                                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");

                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                                dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
                                if (dr["LOSS_CODE"].Equals("")) return;

                                dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                                if (dr["LOSS_DETL_CODE"].Equals(""))
                                {
                                    if (cboLossDetl.Items.Count > 1)
                                    {
                                        // 부동내용을 입력하세요.
                                        Util.MessageValidation("SFU3631");
                                        return;
                                    }
                                }

                                dr["LOSS_NOTE"] = Util.GetCondition(txtLossNote);
                                dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                                dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                                dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                                dr["OCCR_EQPTID"] = Util.GetCondition(cboCauEqp);
                                dr["USERID"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(dr);
                            }
                        }

                        try
                        {
                            if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                            {
                                Util.MessageValidation("SFU3489");//개별등록일 경우 일괄저장 기능 사용 불가
                                return;
                            }
                            else
                            {
                                new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL", "RQSTDT", null, ds);

                                // 설비로스 변경 이력 저장
                                try
                                {
                                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                                }
                                catch (Exception ex9)
                                {
                                    Util.MessageException(ex9);
                                }

                            }

                            //UPDATE 처리후 재조회
                            btnSearch_Click(null, null);
                            chkT.IsChecked = false;
                            chkW.IsChecked = false;
                            chkU.IsChecked = false;

                            dgDetail.ScrollIntoView(idx, 0);

                            Util.MessageInfo("SFU1270");  //저장되었습니다.
                        }

                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                }
            });
        }

        private void btnLossCode_Click(object sender, RoutedEventArgs e)
        {
            FCS002_066_LOSS_DETL wndLossDetl = new FCS002_066_LOSS_DETL();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = Util.NVC(Util.GetCondition(cboEqpKind)); // 공정
                Parameters[2] = string.Empty; //Util.NVC(Util.GetCondition(cboLoss)); //설비
                Parameters[3] = Util.NVC(Util.GetCondition(cboLoss)); //Loss Code

                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            FCS002_066_LOSS_DETL window = sender as FCS002_066_LOSS_DETL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                cboLossDetl.SelectedValue = window._LOSS_DETL_CODE;
            }
        }

        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            svMap.ScrollToVerticalOffset(10);
            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            if (dgDetail.CurrentRow != null)
            {
                if (RunSplit.Equals("Y"))
                {
                    if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("Y"))
                    {
                        //가동 데이터를 분할 한 데이터이므로 추가된 Loss가 초기화 됩니다. 그래도 삭제하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3205"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {

                            if (result.ToString().Equals("OK"))
                            {
                                try
                                {
                                    DataSet ds = new DataSet();
                                    DataTable dt = ds.Tables.Add("INDATA");

                                    dt.Columns.Add("STRT_DTTM", typeof(DateTime));
                                    dt.Columns.Add("END_DTTM", typeof(DateTime));
                                    dt.Columns.Add("EQPTID", typeof(string));
                                    dt.Columns.Add("WRK_DATE", typeof(string));
                                    dt.Columns.Add("AREAID", typeof(string));
                                    dt.Columns.Add("USERID", typeof(string));
                                    dt.Columns.Add("START_DTTM_YMDHMS", typeof(string));

                                    DataRow row = dt.NewRow();
                                    row["STRT_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["END_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                    row["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "WRK_DATE"));
                                    row["AREAID"] = LoginInfo.CFG_AREA_ID;
                                    row["USERID"] = LoginInfo.USERID;
                                    row["START_DTTM_YMDHMS"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                    dt.Rows.Add(row);

                                    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RUN_SPLT_RESET", "INDATA", null, ds);

                                    btnSearch_Click(null, null);

                                    if (dgDetail.GetRowCount() != 0)
                                    {
                                        dgDetail.ScrollIntoView(idx, 0);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }
                        }
                        );
                    }
                }

                if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "CHECK_DELETE")).Equals("DELETE") && dgDetail.CurrentColumn != null && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("N")) //삭제 더블클릭시에 실행
                {
                    //삭제하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("EQPTID", typeof(string));
                                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                                RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                                RQSTDT.Columns.Add("END_DTTM", typeof(string));
                                RQSTDT.Columns.Add("USERID", typeof(string));

                                DataRow dr = RQSTDT.NewRow();
                                //dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU3514"); //설비는필수입니다.
                                // 대표설비로 저장.
                                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;

                                //if (dr["EQPTID"].Equals("") || dr["LOSS_CODE"].Equals("")) return;

                                RQSTDT.Rows.Add(dr);
                                DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_RESET", "RQSTDT", "RSLTDT", RQSTDT);

                                // 설비로스 변경 이력 저장
                                try
                                {
                                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                                }
                                catch (Exception ex9)
                                {
                                    Util.MessageException(ex9);
                                }

                                //UPDATE 처리후 재조회
                                btnSearch_Click(null, null);
                                if (dgDetail.GetRowCount() != 0)
                                {
                                    dgDetail.ScrollIntoView(idx, 0);
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("SPLIT") && dgDetail.CurrentColumn != null) //분할 더블클릭시에 실행
                {
                    //분할하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3120"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            FCS002_066_SPLIT wndPopup = new FCS002_066_SPLIT();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[5];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO"));
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                Parameters[4] = LoginInfo.CFG_AREA_ID;

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("MERGE") && dgDetail.CurrentColumn != null) //병합 더블클릭시에 실행
                {
                    //병합하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2876"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet dsData = new DataSet();

                                DataTable dtIn = dsData.Tables.Add("INDATA");
                                dtIn.Columns.Add("EQPTID", typeof(string));
                                dtIn.Columns.Add("FROM_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("TO_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("USERID", typeof(string));

                                DataRow row = null;
                                row = dtIn.NewRow();
                                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                row["FROM_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100;
                                row["TO_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100 + 99;
                                row["USERID"] = LoginInfo.USERID;

                                dtIn.Rows.Add(row);

                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_SPLIT_RESET", "INDATA", "OUTDATA", dsData);

                                if (Convert.ToInt16(dsRslt.Tables["OUTDATA"].Rows[0]["CNT"]) > 0)
                                {
                                    Util.AlertInfo("SFU1516");  //등록된 데이터를 지우고 병합해주세요.
                                    return;
                                }
                                else
                                {
                                    //UPDATE 처리후 재조회
                                    btnSearch_Click(null, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    );
                }
                else //선택처리
                {
                    btnReset_Click(null, null);
                    setMapColor(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);
                }
            }
        }

        private void btnRegiFcr_Click(object sender, RoutedEventArgs e)
        {
            FCS002_066_FCR wndFCR = new FCS002_066_FCR();
            wndFCR.FrameOperation = FrameOperation;

            if (wndFCR != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = Util.GetCondition(cboEqpKind);
                //Parameters[2] = Convert.ToString(cboEquipmentSegment.SelectedValue);

                C1WindowExtension.SetParameters(wndFCR, Parameters);

                wndFCR.Closed += new EventHandler(wndFCR_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCR.ShowModal()));
                wndFCR.BringToFront();
            }
        }

        #endregion

        #region Method

        private void setMapColor(string sTime, string sType, C1.WPF.DataGrid.DataGridRow row = null)
        {
            //DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END > '" + sTime + "'", "");
            DataRow[] dtRow = null;
            // Row 더블클릭시 값이 여러개일경우 못찾아 분리함..
            if (row == null)
            {
                if (chkSub.IsChecked.Equals(true))
                {
                    dtRow = dtMainList.Select("EQPTID = '" + sMainEqptID.Substring(1) + "' AND HIDDEN_START <= '" + sTime + "' AND HIDDEN_END > '" + sTime + "'", "");
                }
                else
                {
                    dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' AND HIDDEN_END > '" + sTime + "'", "HIDDEN_START ASC");
                }
            }
            else
            {
                if (chkSub.IsChecked.Equals(true))
                {
                    dtRow = dtMainList.Select("EQPTID = '" + sMainEqptID.Substring(1) + "' AND HIDDEN_START = '" + sTime + "'", "");
                }
                else
                {
                    dtRow = dtMainList.Select("HIDDEN_START = '" + sTime + "'", "HIDDEN_START ASC");
                }
            }

            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");

            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            try
            {
                if (dtRow.Length > 0)
                {
                    dtRow[0]["CHK"] = "1";

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                    {
                        if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                        }
                        else
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRow[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                        }
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow.Max(m => m.Field<string>("HIDDEN_END"))) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        //txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        //txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

                        txtEnd.Text = dtRow.Max(m => m.Field<string>("END_TIME"));
                        txtEndHidn.Text = dtRow.Max(m => m.Field<string>("HIDDEN_END"));

                        if (row != null)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME")).Equals(""))
                            {
                                txtEqpName.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                            {
                                cboCauEqp.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                            }

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            {
                                cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            {
                                cboLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                            {
                                cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                            {
                                cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                            {
                                cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                            {
                                //new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                                txtLossNote.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                            }
                        }
                    }

                    Border borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        //  dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;  (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString();
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    if (borderS == null || borderS.Tag == null || borderE == null || borderE.Tag == null)
                    { }
                    else
                    {
                        Hashtable hashStart = borderS.Tag as Hashtable;
                        Hashtable hashEnd = borderE.Tag as Hashtable;

                        DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                        Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                        Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                        Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                        Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                        Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                        //색칠해야할 셀갯수 =  row 차이 / (설비갯수 + 시간디스플레이) * 컬럼수 + 종료컬럼 - 시작컬럼
                        int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));


                        for (int j = 0; j < cellCnt; j++)
                        {
                            Border _border = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                            _border.Background = new SolidColorBrush(Colors.Blue);
                        }

                        //마지막 칸 정리
                        Border borderEndMinusOne = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                        Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                        if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                        {
                            borderE.Background = new SolidColorBrush(Colors.Blue);
                        }

                        int iRow = Grid.GetRow(borderS);

                        txtEqpName.Text = GetEqptName(hashStart["EQPTID"].ToString(), "M").Rows[0][1].ToString();

                        if (sType.Equals("LIST"))
                        {
                            svMap.ScrollToVerticalOffset((15 * iRow - 8));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable GetEqptName(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return RSLTDT;
        }

        private void ValidateNonRegisterLoss(string saveType)
        {
            try
            {
                int idx = -1;
                if (saveType.Equals("TOTAL"))
                {
                    //제일 마지막에 클릭한 index찾기
                    for (int i = dgDetail.GetRowCount() - 1; i > 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            idx = i;
                            break;
                        }
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = Convert.ToString(cboEqp.SelectedValue);
                //row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) :  ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + txtStart.Text;
                //우크라이나어 세팅시 날짜 포맷형식 에러로 인한 수정 2019.07.19.
                row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) : ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + txtStart.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSETAIL_VALID", "RQSTDT", "RSLT", dt);

                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU3515", Convert.ToString(result.Rows[0]["STRT_DTTM"]));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string VadliationERPEnd()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("WRKDATE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["WRKDATE"] = Util.GetCondition(ldpDatePicker);
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_LOSS_CLOSE", "RQSTDT", "RSLT", dt);

            if (result.Rows.Count != 0)
            {
                return Convert.ToString(result.Rows[0]["ERP_CLOSING_FLAG"]);
            }

            return "OPEN";
        }

        private void resetMapColor()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            DataRow[] dtRow = dtMainList.Select("CHK = '1'", "");

            foreach (DataRow dr in dtRow)
            {
                dr["CHK"] = "0";
            }

        }

        private void GetEqptLossRawList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEqp, "SFU3514"); //설비는필수입니다
                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                dtMainList = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_RAW", "RQSTDT", "RSLTDT", RQSTDT);
                //dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_RAW", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게 
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetEqptLossDetailList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string)); 
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                //dr["EQPTID"] = Util.GetCondition(cboEqp);
                // 대표설비로 저장.
                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");

                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["REVERSE_CHECK"] = 'Y';
                dr["MIN_SECONDS"] = 60;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_FOR_UNIT", "RQSTDT", "RSLTDT", RQSTDT);

                if (cboShift.SelectedValue != null && !string.IsNullOrEmpty(cboShift.SelectedValue.GetString()))
                {
                    DateTime dJobDate_st = new DateTime();
                    DateTime dJobDate_ed = new DateTime();

                    DataRow[] drShift = dtShift.Select("SHIFT='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        string sShift_st = drShift[0]["START_TIME"].ToString();
                        string sShift_ed = drShift[0]["END_TIME"].ToString();

                        dJobDate_st = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_st.Substring(0, 2) + ":" + sShift_st.Substring(2, 2) + ":" + sShift_st.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        dJobDate_ed = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                        //작업조의 end시간이 기준시간 보다 작을때
                        if (TimeSpan.Parse(sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2)) < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
                        {
                            dJobDate_ed = DateTime.ParseExact(ldpDatePicker.SelectedDateTime.AddDays(1).ToString("yyyyMMdd") + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        }
                    }

                    try
                    {
                        RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")", "HIDDEN_START DESC").CopyToDataTable();
                    }
                    catch (Exception ex)
                    {
                        DataTable dt = new DataTable();
                        foreach (DataColumn col in RSLTDT.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.ColumnName));
                        }

                        RSLTDT = dt;
                    }
                }

                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);

                txtRequireCnt.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtInputCnt.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();


                // 2019-09-30 황기근 사원 수정
                restrictSave();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void restrictSave()
        {

            try
            {
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.Columns.Add("LANGID", typeof(string));
                RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));

                DataRow row = RQSTDT1.NewRow();
                row["LANGID"] = LoginInfo.USERID;
                row["CMCDTYPE"] = "LOSS_MODIFY_RESTRICT_SHOP";
                RQSTDT1.Rows.Add(row);

                DataTable shopList = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT1);
                DataRow[] shop = shopList.Select("CBO_Code = '" + LoginInfo.CFG_SHOP_ID + "'");

                if (shop.Count() > 0)   // LOSS 수정 제한하는 PLANT일 때
                {
                    DateTime pickedDate = ldpDatePicker.SelectedDateTime;
                    if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.ToString("yyyy-MM-dd")))
                        return;

                    DateTime dtCaldate = Convert.ToDateTime(AreaTime.Rows[0]["JOBDATE_YYYYMMDD"]);
                    string sCaldate = dtCaldate.ToString("yyyy-MM-dd");
                    btnSave.IsEnabled = true;
                    //btnTotalSave.IsEnabled = true;

                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.Columns.Add("USERID", typeof(string));
                    RQSTDT2.Columns.Add("AUTHID", typeof(string));

                    DataRow row2 = RQSTDT2.NewRow();
                    row2["USERID"] = LoginInfo.USERID;
                    row2["AUTHID"] = "EQPTLOSS_MGMT";
                    RQSTDT2.Rows.Add(row2);

                    DataTable auth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT2);

                    if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")))
                    {
                        TimeSpan due = DateTime.Parse(Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(0, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(2, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(4, 2)).TimeOfDay;
                        TimeSpan searchTime = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;
                        if (searchTime >= due && auth.Rows.Count <= 0)
                        {
                            btnSave.IsEnabled = false;
                            //btnTotalSave.IsEnabled = false;
                        }
                    }
                    else
                    {
                        if (auth.Rows.Count <= 0)
                        {
                            btnSave.IsEnabled = false;
                            //btnTotalSave.IsEnabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectProcess()
        {
            try
            {
                string sEqptID = Util.GetCondition(cboEqp);
                string sEqptType = (chkSub.IsChecked.Equals(true)) ? "A" : "M";
                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);
                string sLaneID = Util.GetCondition(cboLane);

                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();

                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sLaneID, sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                //spdMList.ActiveSheet.RowCount = (hash_title.Count) + 1;

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정                    
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                //for (int i = 0; i < 1000; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);


                        _grid.Children.Add(_lable);
                    }

                    //spdMList.ActiveSheet.Cells[nRow, nCol].HorizontalAlignment = CellHorizontalAlignment.Left;

                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];
                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정


                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;


                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);

                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                        if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        {
                            //spdMList.ActiveSheet.RowCount = spdMList.ActiveSheet.RowCount + (hash_title.Count) + 1;
                        }
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private DataSet GetEqptTimeList(string sJobDate, string sShiftCode)
        {
            DataSet ds = new DataSet();
            DataSet dsInData = new DataSet();
            DataSet dsOutData = new DataSet();
            ArrayList sColumnList = new ArrayList();
            ArrayList sDataList = new ArrayList();

            try
            {
                DataTable RSLTDT = new DataTable("RSLTDT");
                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();
                if (string.IsNullOrEmpty(sShiftCode))
                {
                    dJobDate = DateTime.ParseExact(sJobDate + " " + _sStartTime, "yyyyMMdd HH:mm:ss", null);
                    iTerm = 20;
                    iIncrease = 20;
                }
                else
                {
                    DataRow[] drShift = dtShift.Select("SHIFT='" + sShiftCode + "'", "");

                    String sShift = drShift[0]["START_TIME"].ToString();
                    dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                    iTerm = int.Parse(drShift[0]["TERM"].ToString()) * 10;
                    iIncrease = 10;
                }

                DataTable dtGetDate = new ClientProxy().ExecuteServiceSync("COR_SEL_GETDATE", null, "RSLTDT", null);

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));
                }

                DataRow[] drs = RSLTDT.Select("STARTTIME <=" + Convert.ToDateTime(dtGetDate.Rows[0]["SYSTIME"]).ToString("yyyyMMddHHmmss"), "");
                if (drs.Length > 0)
                {
                    DataTable RSLTDT1 = drs.CopyToDataTable();
                    RSLTDT1.TableName = "RSLTDT";
                    ds.Tables.Add(RSLTDT1);
                }
                return ds;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return ds;
            }
        }

        private DataTable GetEqptLossFirstList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return RSLTDT;
        }

        private DataTable GetEqptLossList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_PROC_LOSS_MAP", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return RSLTDT;

        }

        private DataTable GetEqptName(string sLaneID, string sEqptId, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dr["EQPTTYPE"] = sEqptType;
                dr["LANE_ID"] = sLaneID;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_EQPT_TITLE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return RSLTDT;
        }

        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Hashtable hash_rs = new Hashtable();
                    for (int j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        hash_rs.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                    }
                    hash_return.Add(dt.Rows[i]["STARTTIME"].ToString(), hash_rs);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                hash_return = null;
            }
            return hash_return;
        }

        private void GetStartTime()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(dtRslt.Rows[0]["DAY_CLOSE_HMS"].ToString()))
                    {
                        Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                        return;
                    }
                    else
                    {
                        _sStartTime = dtRslt.Rows[0]["DAY_CLOSE_HMS"].ToString().Substring(0, 2) + ":" + dtRslt.Rows[0]["DAY_CLOSE_HMS"].ToString().Substring(2, 2) + ":" + dtRslt.Rows[0]["DAY_CLOSE_HMS"].ToString().Substring(4, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetShiftTime()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                dtShift = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIFT_LIST", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetAreaTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("JOBDATE", typeof(string));

                DataRow row = dt.NewRow();

                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd");
                dt.Rows.Add(row);

                AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
                if (AreaTime.Rows.Count == 0) { }
                if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
                {
                    Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearGrid()
        {
            try
            {
                foreach (Border _border in _grid.Children.OfType<Border>())
                {
                    _grid.UnregisterName(_border.Name);
                }

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearControl()
        {
            txtEqpName.Text = string.Empty;
            txtStart.Text = string.Empty;
            txtEnd.Text = string.Empty;
            txtTroubleName.Text = string.Empty;
            txtLossNote.Text = string.Empty;
            txtStartHidn.Text = string.Empty;
            txtEndHidn.Text = string.Empty;

            chkW.IsChecked = false;
            chkT.IsChecked = false;
            chkU.IsChecked = false;

            if (cboLoss.Items.Count > 0)
                cboLoss.SelectedIndex = 0;
            if (cboLossDetl.Items.Count > 0)
                cboLossDetl.SelectedIndex = 0;
            if (cboLatest.Items.Count > 0)
                cboLatest.SelectedIndex = 0;
            if (cboFailure.Items.Count > 0)
                cboFailure.SelectedIndex = 0;
            if (cboCause.Items.Count > 0)
                cboCause.SelectedIndex = 0;
            if (cboResolution.Items.Count > 0)
                cboResolution.SelectedIndex = 0;
            if (cboFcrGroup.Items.Count > 0)
                cboFcrGroup.SelectedIndex = 0;

            txtInputCnt.Text = "0";
            txtRequireCnt.Text = "0";
        }

        private void InitInsertCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLossChild = { cboLossDetl };
            _combo.SetCombo(cboLoss, CommonCombo_Form.ComboStatus.SELECT, sCase: "EQPLOSS", cbChild: cboLossChild);

            C1ComboBox[] cboLossDetlParent = { cboLoss };
            _combo.SetCombo(cboLossDetl, CommonCombo_Form.ComboStatus.NA, sCase: "EQPLOSSDETAIL", cbParent: cboLossDetlParent);

            C1ComboBox[] cboLatestParent = { cboEqp };
            _combo.SetCombo(cboLatest, CommonCombo_Form.ComboStatus.NA, sCase: "LASTLOSSCODE", cbParent: cboLatestParent);

            //20220124_원인설비 Setting 조건 변경 START
            //C1ComboBox[] cboCauEqpParent = { cboEqp };
            C1ComboBox[] cboCauEqpParent = { cboEqp, cboLane };
            _combo.SetCombo(cboCauEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "OCCUREQP", cbParent: cboCauEqpParent);
            //20220124_원인설비 Setting 조건 변경 END

            C1ComboBox[] cboFcrGroupParent = { cboEqpKind };
            _combo.SetCombo(cboFcrGroup, CommonCombo_Form.ComboStatus.NA, sCase: "FCRGROUP", cbParent: cboFcrGroupParent);
        }

        private void GetLossColor()
        {
            try
            {
                C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
                cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
                cboColorLegend.Items.Add(cbItemTitle);

                C1ComboBoxItem cbItemRun = new C1ComboBoxItem();
                cbItemRun.Content = "Run";
                cbItemRun.Background = ColorToBrush(GridBackColor.R);
                cboColorLegend.Items.Add(cbItemRun);

                C1ComboBoxItem cbItemWait = new C1ComboBoxItem();
                cbItemWait.Content = "Wait";
                cbItemWait.Background = ColorToBrush(GridBackColor.W);
                cboColorLegend.Items.Add(cbItemWait);

                C1ComboBoxItem cbItemTrouble = new C1ComboBoxItem();
                cbItemTrouble.Content = "Trouble";
                cbItemTrouble.Background = ColorToBrush(GridBackColor.T);
                cboColorLegend.Items.Add(cbItemTrouble);

                C1ComboBoxItem cbItemOff = new C1ComboBoxItem();
                cbItemOff.Content = "OFF";
                cbItemOff.Background = ColorToBrush(GridBackColor.F);
                cboColorLegend.Items.Add(cbItemOff);

                C1ComboBoxItem cbItemUserStop = new C1ComboBoxItem();
                cbItemUserStop.Content = "UserStop";
                cbItemUserStop.Background = ColorToBrush(GridBackColor.U);
                cboColorLegend.Items.Add(cbItemUserStop);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtRslt);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["LOSS_NAME"];
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["DISPCOLOR"].ToString()));
                    cboColorLegend.Items.Add(cbItem);
                }

                cboColorLegend.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);
        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.WhiteSmoke;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType.Equals(""))
                        {
                            color = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                color = System.Drawing.Color.WhiteSmoke;
            }
            return color;
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

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

        #endregion


    }
}
