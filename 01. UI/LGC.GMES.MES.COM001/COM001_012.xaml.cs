/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 이력 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.09     정연준     C20221122-000111     활성화 기능 추가 (Copy of COM001_015 - 변경집합 : 52788)
  2023.03.21     이윤중     E20230322-001797     활성화 ESNB ESS LINE 로직 추가(공통코드 활용) - SetEquipmentSegmentCombo 호출 bizrule(BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO) 변경 
  2023.04.06     이윤중     E20230405-001228     1. 라인/공정 변경시 - 공정/설비 Combobox 가져오지 않는 문제 개선 2. 메인 설비만 조회 가능하도록 변경
  2023.09.08     이윤중     E20230823-000845     메인 설비 CheckBox 추가 (체크시 : Main 설비만 조회, 미체크시 : Main + Machine설비 조회)
  2023.10.24     조영대                          Loaded 이벤트 제거
  2024-08-21     김대현     E20240626-000975     TEST/개발 W/O 자동실적 적용 전후 데이터 조회 기능 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_012 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        string _AREAID = "";
        string _EQSGID = "";
        string _PROCID = "";
        string _EQPTID = "";
        string _SEARCH_FROM_DATE = string.Empty;        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
        string _SEARCH_TO_DATE = string.Empty;          // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
        string _OPEN_TIME;

        bool bPack;
        bool bForm;

        // 동,라인,공정,설비 셋팅
        CommonCombo _combo = new CommonCombo();

        public COM001_012()
        {            
            InitializeComponent();
            InitCombo();

            btnTestLossHist.Visibility = Visibility.Hidden;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bForm = true;
            }

            // 동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            // 2022-12-23 : LINE 조건 전체 선택 불가 하도록 변경 - leeyj
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            C1ComboBox[] cboAreaDailyChild = { cboEquipmentSegmentDaily };
            _combo.SetCombo(cboAreaDaily, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaDailyChild);

            C1ComboBox[] cboAreaSPQChild = { cboEquipmentSegmentSPQ };
            _combo.SetCombo(cboAreaSPQ, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaSPQChild);


            /*if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {*/
            bPack = false;

            if(bForm)
            {
                SetEquipmentSegmentCombo(cboEquipmentSegment);
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
                SetEquipmentSegmentCombo(cboEquipmentSegmentDaily);
                cboEquipmentSegmentDaily.SelectedValueChanged += cboEquipmentSegmentDaily_SelectedItemChanged;
            }
            else
            {
                // 라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
                // 2022-12-23 : LINE 조건 전체 선택 불가 하도록 변경 - leeyj
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "LINE_FCS");
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "LINE_FCS");


                C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
                C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily, cboEquipmentDaily };
                //_combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);
                _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent, sCase: "LINE_FCS");
            }

            // 공정
            /*C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cbProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent);*/

            string sCase2 = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment, cboArea };
            //C1ComboBox[] cbProcessChild = { cboEquipment };
            // 2022-12-23 : 공정 조건 전체 선택 불가 하도록 변경 - leeyj
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent, sCase: sCase2);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE/*, cbChild: cbProcessChild*/, cbParent: cbProcessParent, sCase: sCase2);
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;

            C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily, cboAreaDaily };
            //C1ComboBox[] cbProcessDailyChild = { cboEquipmentDaily };
            //_combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbChild: cbProcessDailyChild, cbParent: cbProcessDailyParent);
            _combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.NONE/*, cbChild: cbProcessDailyChild*/, cbParent: cbProcessDailyParent, sCase: sCase2);
            cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;



            /*
            C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
            C1ComboBox[] cbProcessDailyChild = { cboEquipmentDaily };
            string strProcessDailyCase = string.Empty;

            if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
            {
                strProcessDailyCase = "cboProcessPack";
            }
            else
            {
                strProcessDailyCase = "PROCESS";
            }

            _combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbChild: cbProcessDailyChild, cbParent: cbProcessDailyParent);
            */

            // 설비


            /*
                SetEquipmentCombo(cboEquipment);
                */


            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cbEquipmentParent);

            C1ComboBox[] cbEquipmentDailyParent = { cboEquipmentSegmentDaily, cboProcessDaily };
            _combo.SetCombo(cboEquipmentDaily, CommonCombo.ComboStatus.NONE, sCase: "EQUIPMENT", cbParent: cbEquipmentDailyParent);

            
            //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            SetTroubleUnitColumnDisplay();



            /*}
            else
            {
                bPack = true;

                // 라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
                C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily };
                _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);

                C1ComboBox[] cboEquipmentSegmentSPQParent = { cboAreaSPQ };
                _combo.SetCombo(cboEquipmentSegmentSPQ, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbParent: cboEquipmentSegmentSPQParent);

                // 공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                string strProcessCase = string.Empty;
                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    strProcessCase = "cboProcessPack";
                }
                else
                {
                    strProcessCase = "PROCESS";
                }

                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent);

                C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
                _combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.ALL, sCase: strProcessCase, cbParent: cbProcessDailyParent);

                SetEquipment(cboEquipment, cboProcess);
                SetEquipment(cboEquipmentDaily, cboProcessDaily);

                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                cboEquipmentSegmentDaily.SelectedItemChanged += cboEquipmentSegmentDaily_SelectedItemChanged;
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
                cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            }*/

            //2019.11.20
            SetEioStateCombo(cboEquipmentState);

            SetShiftCombo(cboShift);

            if (!this.GetPackApplyLIneByUI(LoginInfo.CFG_AREA_ID))
            {
                this.tabSPQ.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.tabSPQ.Visibility = Visibility.Visible;
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            string bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue;
            dr["INCLUDE_GROUP"] = "AC";

            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            //inTable.Columns.Add("EQPTLEVEL", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                if (cbo.SelectedIndex < 0)
                    cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        private DateTime GetShopOpenTime()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dt);
            if (dtResult.Rows.Count != 0)
            {
                return Convert.ToDateTime(dtResult.Rows[0]["DATETO"]);
            }

            return System.DateTime.Now;

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime dtTmp = GetShopOpenTime();
            ldpDateFrom.SelectedDateTime = dtTmp;
            ldpDateLossFrom.SelectedDateTime = dtTmp;
            ldpDateWorkDate.SelectedDateTime = dtTmp;

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearValue();
                
                //if (ldpDateFrom.SelectedDateTime.Date > GetShopOpenTime().Date)
                //{
                //    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                //    return;
                //}

                if (grdFromToDate.Visibility == Visibility.Visible && ldpDateTo.SelectedDateTime.Date > System.DateTime.Now.Date)
                {
                    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("STRTTIME", typeof(DateTime));
                RQSTDT.Columns.Add("ENDTIME", typeof(DateTime));
                //2023.09.08 - Main설비 체크박스 추가 
                RQSTDT.Columns.Add("MAIN_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);
                //2023.09.08 - Main설비 체크박스 추가 
                dr["MAIN_FLAG"] = (chkMain.IsChecked == true ? "Y" : "N");

                // 작업조로 조회.                
                DateTime srartTime = DateTime.Now;
                DateTime endTime = DateTime.Now;

                bool bSearchShft = false;

                if (!Util.NVC(cboShift.SelectedValue).Equals("") && GetShiftTimes(out srartTime, out endTime))
                {
                    dr["STRTTIME"] = srartTime;
                    dr["ENDTIME"] = endTime;
                    dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateWorkDate);
                    dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateWorkDate);

                    bSearchShft = true;
                }

                RQSTDT.Rows.Add(dr);
                _AREAID = Util.GetCondition(cboArea);
                _EQSGID = Util.GetCondition(cboEquipmentSegment);
                _PROCID = Util.GetCondition(cboProcess);                
                _EQPTID = Util.GetCondition(cboEquipment);
                this._SEARCH_FROM_DATE = Util.GetCondition(ldpDateFrom);    // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
                this._SEARCH_TO_DATE = Util.GetCondition(ldpDateTo);        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY", "RQSTDT", "RSLTDT", RQSTDT);
                string bzRuleID = string.Empty;
                if (chkTestDev.IsChecked == true)
                {
                    bzRuleID = "DA_EQP_SEL_EQPTLOSS_SUMMARY_TEST_DEV";
                }
                else
                {
                    bzRuleID = "DA_EQP_SEL_EQPTLOSS_SUMMARY";
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleID, "RQSTDT", "RSLTDT", RQSTDT);

                if (!dtResult.Columns.Contains("SHFT_SEARCH"))
                {
                    DataColumn newCol = new DataColumn("SHFT_SEARCH", typeof(string));
                    if (bSearchShft)
                        newCol.DefaultValue = "Y";
                    else
                        newCol.DefaultValue = "N";
                    dtResult.Columns.Add(newCol);
                }
                

                Util.GridSetData(dgLossList, dtResult, FrameOperation, true);

                dgLossDetail.ItemsSource = null;
                SearchEqptRemark();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchDaily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_TO", typeof(string));
                //2019.11.20
                RQSTDT.Columns.Add("EIOSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                string sAREAID = Util.NVC(cboAreaDaily.SelectedValue);
                string sEQSGID = Util.NVC(cboEquipmentSegmentDaily.SelectedValue);
                string sPROCID = Util.NVC(cboProcessDaily.SelectedValue);
                string sEQPTID = Util.NVC(cboEquipmentDaily.SelectedValue);

                dr["LANGID"] = LoginInfo.LANGID;

                if (sAREAID == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    cboAreaDaily.Focus();
                    return;
                }
                else
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaDaily);
                }

                if (sEQSGID == "")
                {
                    dr["EQSGID"] = null;
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentDaily);
                }

                if (sPROCID == "")
                {
                    dr["PROCID"] = null;
                }
                else
                {
                    dr["PROCID"] = Util.GetCondition(cboProcessDaily);
                }

                if (sEQPTID == "")
                {
                    dr["EQPTID"] = null;
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipmentDaily);
                }

                dr["WRK_DATE_FROM"] = ldpDateLossFrom.SelectedDateTime.AddDays(-1).ToString("yyyy-MM-dd");
                dr["WRK_DATE_TO"] = ldpDateLossTo.SelectedDateTime.ToString("yyyy-MM-dd");
                //   dr["WRK_DATE"] = Util.GetCondition(ldpDateFrom_Daily);

                //2019.11.20
                dr["EIOSTAT"] = cboEquipmentState.SelectedValue;

                RQSTDT.Rows.Add(dr);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_DAILY", "RQSTDT", "RSLTDT", RQSTDT);
                string bzRuleId = string.Empty;
                if (chkTestDevDaily.IsChecked == true)
                {
                    bzRuleId = "DA_EQP_SEL_EQPTLOSSRAW_DAILY_TEST_DEV";
                }
                else
                {
                    bzRuleId = "DA_EQP_SEL_EQPTLOSSRAW_DAILY";
                }
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleId, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLossDailyList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchSPQ_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string areaID = Util.NVC(cboAreaSPQ.SelectedValue);
                string equipmentSegmentID = Util.NVC(cboEquipmentSegmentSPQ.SelectedValue);

                // Validation Check...
                if (string.IsNullOrEmpty(areaID) || areaID.Equals("SELECT"))
                {
                    Util.Alert("SFU1499");  // 동을 선택하세요.
                    cboAreaSPQ.Focus();
                    return;
                }

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["FROM_WRK_DATE"] = this.ldpDateFromLossSPQ.SelectedDateTime.Date.ToString("yyyyMMdd");
                drRQSTDT["TO_WRK_DATE"] = this.ldpDateToLossSPQ.SelectedDateTime.Date.AddDays(1).ToString("yyyyMMdd");
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL_WITH_SPQ", "RQSTDT", "RSLTDT", dtRQSTDT);

                Util.GridSetData(this.dgLossSPQ, dt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion Button

        #region Grid
        private void dgLossList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLossList.CurrentRow != null && dgLossList.CurrentColumn.Name.Equals("LOSSCNT"))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("STRTTIME", typeof(DateTime));
                    RQSTDT.Columns.Add("ENDTIME", typeof(DateTime));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "LOSS_CODE"));
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "EQPTID"));
                    dr["FROM_WRK_DATE"] = this._SEARCH_FROM_DATE;
                    dr["TO_WRK_DATE"] = this._SEARCH_TO_DATE;
                    if (Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_SEARCH")).Equals("Y"))
                    {
                        dr["FROM_WRK_DATE"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_WRK_DATE");
                        dr["TO_WRK_DATE"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_WRK_DATE");
                        dr["STRTTIME"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_STRTTIME");
                        dr["ENDTIME"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_ENDTIME");
                    }
                    RQSTDT.Rows.Add(dr);

                    //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                    string bzRuleId = string.Empty;
                    if (chkTestDev.IsChecked == true)
                    {
                        bzRuleId = "DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL_TEST_DEV";
                    }
                    else
                    {
                        bzRuleId = "DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL";
                    }
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleId, "RQSTDT", "RSLTDT", RQSTDT);

                    Util.GridSetData(dgLossDetail, dtResult, FrameOperation, true);

                    //if (dtResult.Rows.Count > 0)
                    //{
                    //    dgLossDetail.ItemsSource = DataTableConverter.Convert(dtResult);
                    //    //Util.GridSetData(dgLossDetail, dtResult, FrameOperation);
                    //}
                    //else
                    //{
                    //    dgLossDetail.ItemsSource = null;
                    //}

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgLossList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOSSCNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue) ;
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgLossDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void dgLossSPQ_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }
        #endregion Grid

        #region Combo

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }
        }

        private void cboEquipmentSegmentDaily_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bForm)
            {
                return;
            }

            if (cboEquipmentSegmentDaily.Items.Count > 0 && cboEquipmentSegmentDaily.SelectedValue != null && !cboEquipmentSegmentDaily.SelectedValue.Equals("SELECT"))
            {
                cboProcessDaily.SelectedItemChanged -= cboProcessDaily_SelectedItemChanged;
                C1ComboBox[] cbEquipmentDailyParent = { cboEquipmentSegmentDaily, cboProcessDaily };
                _combo.SetCombo(cboEquipmentDaily, CommonCombo.ComboStatus.NONE, sCase: "EQUIPMENT", cbParent: cbEquipmentDailyParent);
                cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            }
        }
        #endregion [라인] - 조회 조건

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bForm)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cbEquipmentParent);
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            }
            SetShiftCombo(cboShift);
        }

        private void cboProcessDaily_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bForm)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                cboProcessDaily.SelectedItemChanged -= cboProcessDaily_SelectedItemChanged;
                C1ComboBox[] cbEquipmentDailyParent = { cboEquipmentSegmentDaily, cboProcessDaily };
                _combo.SetCombo(cboEquipmentDaily, CommonCombo.ComboStatus.NONE, sCase: "EQUIPMENT", cbParent: cbEquipmentDailyParent);
                cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            SetTroubleUnitColumnDisplay();

            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cbEquipmentParent);
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            SetTroubleUnitColumnDisplay();
            SetShiftCombo(cboShift);
        }

        private void cboProcessDaily_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
        }

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboShift != null && !Util.NVC(cboShift.SelectedValue).Equals(""))
            {
                grdFromToDate.Visibility = Visibility.Collapsed;
                grdWorkDate.Visibility = Visibility.Visible;
            }
            else
            {
                grdFromToDate.Visibility = Visibility.Visible;
                grdWorkDate.Visibility = Visibility.Collapsed;
            }
        }

        private void SetTroubleUnitColumnDisplay()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["EQPT_LOSS_UNIT_ALARM_DISP_FLAG"].GetString() == "Y")
                    {
                        dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                        dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                    dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                Util.MessageException(ex);

            }
        }

        #endregion [공정] - 조회 조건

        //2019.11.20
        private void SetEioStateCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_BAS_SEL_EIOSTATE_CBO";
            //string[] arrColumn = { "LANGID" };
            //string[] arrCondition = { LoginInfo.LANGID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;
            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOSTATE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow dr_ = dtRslt.NewRow();
                    dr_["CBO_NAME"] = "ALL";
                    dr_["CBO_CODE"] = null;
                    dtRslt.Rows.Add(dr_);

                    dtRslt.AcceptChanges();
                }

                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);

                cbo.SelectedIndex = 5;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        #endregion Combo

        #endregion Event

        #region Mehod

        private void ClearValue()
        {
            _AREAID = "";
            _EQSGID = "";
            _PROCID = "";            
            _EQPTID = "";
            this._SEARCH_FROM_DATE = string.Empty;      // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
            this._SEARCH_TO_DATE = string.Empty;        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
        }

        private void SearchEqptRemark()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = this._SEARCH_FROM_DATE;
                dr["TO_DATE"] = this._SEARCH_TO_DATE;
                dr["EQPTID"] = _EQPTID;
                dr["AREAID"] = _AREAID;
                dr["EQSGID"] = _EQSGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PROCID"] = _PROCID;
                dr["SHFT_ID"] = Util.NVC(cboShift.SelectedValue).Equals("") ? null : Util.NVC(cboShift.SelectedValue);

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_NOTE_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    dgEQPTDetail.ItemsSource = DataTableConverter.Convert(dtResult);
                    //Util.GridSetData(dgEQPTDetail, dtResult, FrameOperation, true);
                }
                else
                {
                    dgEQPTDetail.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [### 설비 정보 가져오기]
        private void SetEquipment(C1ComboBox cbEqpt, C1ComboBox cbProc)
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegment;
                dr["PROCID"] = Util.GetCondition(cbProc);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                cbEqpt.DisplayMemberPath = "CBO_NAME";
                cbEqpt.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-ALL-";
                //drIns["CBO_CODE"] = "";
                //dtResult.Rows.InsertAt(drIns, 0);

                cbEqpt.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbEqpt.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbEqpt.SelectedIndex < 0)
                        cbEqpt.SelectedIndex = 0;
                }
                else
                {
                    cbEqpt.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion  [### 설비 정보 가져오기]

        private bool ChkMainUnit(string sEqptID)
        {
            try
            {
                bool bMainUnit = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VWEQUIPMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("MAIN_EQPTID") && Util.NVC(dtResult.Rows[0]["MAIN_EQPTID"]).Equals(sEqptID))
                {
                    bMainUnit = true;
                }

                return bMainUnit;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetShiftCombo(C1ComboBox cbo)
        {
            try
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow["CBO_CODE"] = "";
                newRow["CBO_NAME"] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);
                cbo.ItemsSource = dtResult.Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                grdFromToDate.Visibility = Visibility.Visible;
                grdWorkDate.Visibility = Visibility.Collapsed;
            }
        }

        private bool GetShiftTimes(out DateTime startTime, out DateTime endTime)
        {
            startTime = DateTime.Now;
            endTime = DateTime.Now;

            try
            {
                bool bRet = false;                

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHFTID", typeof(string));
                RQSTDT.Columns.Add("CALDATE", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["SHFTID"] = Util.NVC(cboShift.SelectedValue);
                dr["CALDATE"] = Util.GetCondition(ldpDateWorkDate);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHFT_TIME_BY_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("STARTTIME") && dtResult.Columns.Contains("ENDTIME") && Util.NVC(dtResult.Rows[0]["VLD_YN"]).Equals("Y"))
                {
                    if (DateTime.TryParse(Util.NVC(dtResult.Rows[0]["STARTTIME"]), out startTime) && DateTime.TryParse(Util.NVC(dtResult.Rows[0]["ENDTIME"]), out endTime))
                        bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion  Mehod

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    dgLossList.Columns["SHFT_WRK_DATE"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_STRTTIME"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_ENDTIME"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_SEARCH"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnTestLossHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1499");
                    return;
                }
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1223");
                    return;
                }
                if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1459");
                    return;
                }
                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1153");
                    return;
                }
                if (cboShift.SelectedIndex < 0 || cboShift.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1844");
                    return;
                }


                /*COM001_015_TEST_LOSS_PRV_INFO wndTestLossInfo = new COM001_015_TEST_LOSS_PRV_INFO();
                wndTestLossInfo.FrameOperation = FrameOperation;

                if (wndTestLossInfo != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = Util.GetCondition(cboArea);
                    Parameters[1] = Util.GetCondition(cboEquipmentSegment);
                    Parameters[2] = Util.GetCondition(cboProcess);
                    Parameters[3] = Util.GetCondition(cboEquipment);
                    Parameters[4] = Util.GetCondition(ldpDateFrom);
                    Parameters[5] = Util.GetCondition(ldpDateTo);
                    Parameters[6] = Util.GetCondition(cboShift);
                    Parameters[7] = "";
                    Parameters[8] = "";

                    DateTime srartTime = DateTime.Now;
                    DateTime endTime = DateTime.Now;

                    if (!Util.NVC(cboShift.SelectedValue).Equals("") && GetShiftTimes(out srartTime, out endTime))
                    {
                        Parameters[7] = srartTime;
                        Parameters[8] = endTime;

                        Parameters[4] = Util.GetCondition(ldpDateWorkDate);
                        Parameters[5] = Util.GetCondition(ldpDateWorkDate);
                    }

                    C1WindowExtension.SetParameters(wndTestLossInfo, Parameters);
                    wndTestLossInfo.Closed += new EventHandler(wndTestLossInfo_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndTestLossInfo.ShowModal()));
                }*/
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void wndTestLossInfo_Closed(object sender, EventArgs e)
        {
            /*COM001_015_TEST_LOSS_PRV_INFO window = sender as COM001_015_TEST_LOSS_PRV_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }*/
        }


        private bool GetPackApplyLIneByUI(string areaID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
                drRQSTDT["CBO_CODE"] = "COM001_012";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        if (drRSLTDT["ATTRIBUTE1"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE2"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE3"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE4"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE5"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }
    }
}
