/*************************************************************************************
 Created Date : 2022.09.26
      Creator : 강동희
   Decription : 설비 알람 이력 조회(활성화)
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.26  강동희 : Initial Created.   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_141 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS001_141()
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

        private void InitializeControls()
        {
            DateTime systemDateTime = GetSystemTime();

            if (dpStart != null)
                dpStart.SelectedDateTime = systemDateTime;

            if (dpEnd != null)
                dpEnd.SelectedDateTime = systemDateTime.AddDays(+1);
            
            if (teStart != null)
                teStart.Value = new TimeSpan(0, 0, 0, 0, 0);

            if (teEnd != null)
                teEnd.Value = new TimeSpan(0, 0, 0, 0, 0);
        }

        private void InitializeCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            //동
            C1ComboBox[] cboPlantChild = { cboEquipmentSegment };
            ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "PLANT", cbChild: cboPlantChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE_SHOPID", cbParent: cboLineParent);

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode);

            // 공정
            SetProcessCombo(cboProcess);

            //설비
            SetEquipmentCombo(cboEquipment);

            //string[] sFilter = { null, "M,C" };
            //C1ComboBox[] cboEqpParent = { cboEquipmentSegment };
            //ComCombo.SetCombo(cboEquipment, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTFORM", sFilter: sFilter, cbParent: cboEqpParent);

            SetEioStateCombo(cboEioState);
            SetEquipmentAlarmLevel(cboEquipmentAlarmLevel);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcGrpCode.SelectedItemChanged += cboProcGrpCode_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
        }
 
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControls();

            dpStart.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dpEnd.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetEquipmentAlarmHistoryList();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcessGroupCombo(cboProcGrpCode);
            }
        }

        private void cboProcGrpCode_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcGrpCode.Items.Count > 0 && cboProcGrpCode.SelectedValue != null && !cboProcGrpCode.SelectedValue.Equals("SELECT"))
            {
                SetProcessCombo(cboProcess);
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipmentCombo(cboEquipment);
                Util.gridClear(dgEquipmentAlarmHistoryList);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpStart.SelectedDateTime.Year > 1 && dpEnd.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays < 0)
                {
                    dpStart.SelectedDateTime = dpEnd.SelectedDateTime;
                    return;
                }
            }
        }

        private void txtTroubleCode_KeyDown(object sender, KeyEventArgs e)
        {

        }

        #endregion

        #region Mehod

        /// <summary>
        /// 공정그룹
        /// </summary>
        private void SetProcessGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(), "PROC_GR_CODE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(), cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null); //2021.03.31 기본 공정이 없을 경우 에러 발생에 대한 조치

            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 START
            DataTable dtcbo = DataTableConverter.Convert(cbo.ItemsSource);
            if (dtcbo == null || dtcbo.Rows.Count == 0)
            {
                const string bizRuleName1 = "DA_BAS_SEL_ALL_OP_CBO";
                string[] arrColumn1 = { "LANGID", "PROC_GR_CODE" };
                string[] arrCondition1 = { LoginInfo.LANGID, cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
                string selectedValueText1 = "CBO_CODE";
                string displayMemberText1 = "CBO_NAME";

                CommonCombo_Form.CommonBaseCombo(bizRuleName1, cbo, arrColumn1, arrCondition1, CommonCombo_Form.ComboStatus.NONE, selectedValueText1, displayMemberText1, null);
            }
            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 END

        }

        // 설비
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            cbo.Clear();
            cbo.Text = string.Empty;
            cbo.ItemsSource = null;

            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_EQP_BY_PROCESS";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCGRID", "PROCID", "EQPTLEVEL" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,
                cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(),
                cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode),
                cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString(),
                "M,C" };

            cbo.SetDataComboItem(bizRuleName, arrColumn, arrCondition, string.Empty, CommonCombo.ComboStatus.NONE, true, null);
        }

        private static void SetEioStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EIOSTATE_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentAlarmLevel(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "EQPT_ALARM_LEVEL_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_CUS_GET_SYSTIME";

            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if(!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string areaCode = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(areaCode))
                    return;

                string processCode = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string equipmentSegmentCode = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = areaCode;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegmentCode) ? null : equipmentSegmentCode;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);
                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if(!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetEquipmentAlarmHistoryList()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_EQPT_ALARM_HIST";

                DateTime dtStartTime;
                var fromTimeSpan = (TimeSpan)teStart.Value;
                dtStartTime = new DateTime(dpStart.SelectedDateTime.Year, dpStart.SelectedDateTime.Month, dpStart.SelectedDateTime.Day, fromTimeSpan.Hours, fromTimeSpan.Minutes, fromTimeSpan.Seconds, DateTimeKind.Local);

                DateTime dtEndTime;
                var toTimeSpan = (TimeSpan)teEnd.Value;
                dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year, dpEnd.SelectedDateTime.Month, dpEnd.SelectedDateTime.Day, toTimeSpan.Hours, toTimeSpan.Minutes, toTimeSpan.Seconds, DateTimeKind.Local);

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("DATETIME_FROM", typeof(DateTime));
                inTable.Columns.Add("DATETIME_TO", typeof(DateTime));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EIOSTAT", typeof(string));
                inTable.Columns.Add("EQPT_ALARM_LEVEL_CODE", typeof(string));   //설비알람레벨코드
                inTable.Columns.Add("EQPT_ALARM_CODE", typeof(string));         //설비알람코드
                //2020 11.17 오화백 : DA 파라미터 추가
                inTable.Columns.Add("AREAID", typeof(string));         //동
                inTable.Columns.Add("EQSGID", typeof(string));         //라인
                inTable.Columns.Add("PROCID", typeof(string));         //공정


                DataRow dr = inTable.NewRow();
                dr["DATETIME_FROM"] = dtStartTime;
                dr["DATETIME_TO"] = dtEndTime;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString() == string.Empty ? null : cboEquipment.SelectedValue;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EIOSTAT"] = cboEioState.SelectedValue;
                dr["EQPT_ALARM_LEVEL_CODE"] = cboEquipmentAlarmLevel.SelectedValue;
                dr["EQPT_ALARM_CODE"] = !string.IsNullOrEmpty(txtEquipmentAlarmCode.Text.Trim()) ? txtEquipmentAlarmCode.Text : null;
                //2020 11.17 오화백 : DA 파라미터 추가
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["PROCID"] = cboProcess.SelectedValue;


                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEquipmentAlarmHistoryList, bizResult, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Function

        private bool ValidationSearch()
        {
            //if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays > 31)
            //{
            //    // 기간은 {0}일 이내 입니다.
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

            if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays > 8)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return false;
            }
            // 2020 11 17 오화백 : 라인 정보 체크
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을선택하세요
                Util.MessageValidation("SFU1499");
                return false;
            }

            //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return false;
            //}

            if (teEnd.Value != null && teStart != null)
            {
                TimeSpan tsEnd = (TimeSpan)teEnd.Value;
                DateTime dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year
                    , dpEnd.SelectedDateTime.Month
                    , dpEnd.SelectedDateTime.Day
                    , tsEnd.Hours
                    , tsEnd.Minutes
                    , tsEnd.Seconds
                    , DateTimeKind.Local);

                TimeSpan tsStart = (TimeSpan)teStart.Value;
                DateTime dtStartTime = new DateTime(dpStart.SelectedDateTime.Year
                    , dpStart.SelectedDateTime.Month
                    , dpStart.SelectedDateTime.Day
                    , tsStart.Hours
                    , tsStart.Minutes
                    , tsStart.Seconds
                    , DateTimeKind.Local);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //Util.MessageValidation("종료시간이 시작시간보다 전 시간 일 수 없습니다.");
                    Util.MessageValidation("SFU3037");
                    return false;
                }
            }

            return true;
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
