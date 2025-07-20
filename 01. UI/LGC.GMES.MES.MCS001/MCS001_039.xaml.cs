/*************************************************************************************
 Created Date : 2019.12.30
      Creator : Shin Kwang Hee
   Decription : 물류설비 알람 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.30  신 광희 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_039 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        public MCS001_039()
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

        private void Initialize()
        {
            List<Button> listAuth = new List<Button> { btnSearch};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeCombo();
        }

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
            SetCommonCodeCombo(cboArea, "BLDG_CODE", CommonCombo.ComboStatus.NONE, "SITE", string.Empty);           // 동
            SetCommonCombo(cboEquipmentType, "TSC_TP", CommonCombo.ComboStatus.NONE, "Y", string.Empty);            // TSC 유형
            SetEquipmentCombo(cboEquipment);                                                                        // TSC 명
            SetCommonCodeCombo(cboEquipmentAlarmState, "ALARM_STAT", CommonCombo.ComboStatus.ALL, null, string.Empty);   // 알람상태
            SetCommonCodeCombo(cboEquipmentAlarmLevel, "ALARMLEVEL", CommonCombo.ComboStatus.ALL, null, string.Empty);   // 알람상태
            SetCommonCombo(cboMachineType, "MACHINE_TP", CommonCombo.ComboStatus.ALL, "Y", string.Empty);           // Machine유형 
            SetMachineCombo(cboMachineName);                                                                        // Machine 명 
            SetCommonCombo(cboUnitType, "UNIT_TP", CommonCombo.ComboStatus.ALL, "Y", string.Empty);                 // Unit 유형
            SetUnitCombo(cboUnit);                                                                                  // Unit 명
            SetCommonCombo(cboAssemblyType, "ASSY_TP", CommonCombo.ComboStatus.ALL, "Y", string.Empty);             // Unit 유형
            SetAssemblyCombo(cboAssemblyName);                                                                      // Assy 명
        }

        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            InitializeCombo();

            Loaded -= UserControl_Loaded;
            Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch())
                    return;

                SelectEquipmentAlarmHistoryList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTroubleCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);    
        }

        private void cboEquipmentType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMachineCombo(cboMachineName);
        }

        private void cboMachineType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMachineCombo(cboMachineName);
        }

        private void cboMachineName_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetUnitCombo(cboUnit);
        }

        private void cboUnitType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetUnitCombo(cboUnit);
        }

        private void cboUnit_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetAssemblyCombo(cboAssemblyName);
        }

        private void cboAssemblyType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetAssemblyCombo(cboAssemblyName);
        }

        private void chkTscAlarm_Checked(object sender, RoutedEventArgs e)
        {
            cboMachineType.IsEnabled = false;
            cboMachineName.IsEnabled = false;
            cboUnitType.IsEnabled = false;
            cboUnit.IsEnabled = false;
            cboAssemblyType.IsEnabled = false;
            cboAssemblyName.IsEnabled = false;
        }

        private void chkTscAlarm_Unchecked(object sender, RoutedEventArgs e)
        {
            cboMachineType.IsEnabled = true;
            cboMachineName.IsEnabled = true;
            cboUnitType.IsEnabled = true;
            cboUnit.IsEnabled = true;
            cboAssemblyType.IsEnabled = true;
            cboAssemblyName.IsEnabled = true;
        }

        #endregion

        #region Mehod

        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

        }

        private void SelectEquipmentAlarmHistoryList()
        {

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentAlarmHistory);

                DateTime dtStartTime;
                var fromTimeSpan = (TimeSpan)teStart.Value;
                dtStartTime = new DateTime(dpStart.SelectedDateTime.Year, dpStart.SelectedDateTime.Month, dpStart.SelectedDateTime.Day, fromTimeSpan.Hours, fromTimeSpan.Minutes, fromTimeSpan.Seconds, DateTimeKind.Local);

                DateTime dtEndTime;
                var toTimeSpan = (TimeSpan)teEnd.Value;
                dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year, dpEnd.SelectedDateTime.Month, dpEnd.SelectedDateTime.Day, toTimeSpan.Hours, toTimeSpan.Minutes, toTimeSpan.Seconds, DateTimeKind.Local);

                const string bizRuleName = "DA_SEL_MCS_ALARM_HIST_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("BLDG_CODE", typeof(string));
                inDataTable.Columns.Add("TSC_TP", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CIM_ALARM", typeof(string));
                inDataTable.Columns.Add("ALARMID", typeof(string));
                inDataTable.Columns.Add("ALARM_STAT", typeof(string));
                inDataTable.Columns.Add("ALARM_LEVEL", typeof(string));
                inDataTable.Columns.Add("MACHINE_TP", typeof(string));
                inDataTable.Columns.Add("MACHINE_ID", typeof(string));
                inDataTable.Columns.Add("UNIT_TP", typeof(string));
                inDataTable.Columns.Add("UNITID", typeof(string));
                inDataTable.Columns.Add("ASSY_TP", typeof(string));
                inDataTable.Columns.Add("ASSY_ID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROM_DATE"] = dtStartTime;
                inData["TO_DATE"] = dtEndTime;
                inData["BLDG_CODE"] = cboArea.SelectedValue;
                inData["TSC_TP"] = cboEquipmentType.SelectedValue;
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["ALARMID"] = !string.IsNullOrEmpty(txtEquipmentAlarmCode.Text) ? txtEquipmentAlarmCode.Text : null;
                inData["ALARM_STAT"] = cboEquipmentAlarmState.SelectedValue;
                inData["ALARM_LEVEL"] = cboEquipmentAlarmLevel.SelectedValue;

                if (chkTscAlarm != null && chkTscAlarm.IsChecked == true)
                {
                    inData["CIM_ALARM"] = "1";
                }
                else
                {
                    inData["MACHINE_TP"] = cboMachineType.SelectedValue;
                    inData["MACHINE_ID"] = cboMachineName.SelectedValue;
                    inData["UNIT_TP"] = cboUnitType.SelectedValue;
                    inData["UNITID"] = cboUnit.SelectedValue;
                    inData["ASSY_TP"] = cboAssemblyType.SelectedValue;
                    inData["ASSY_ID"] = cboAssemblyName.SelectedValue;
                }
                inDataTable.Rows.Add(inData);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgEquipmentAlarmHistory, result, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetCommonCodeCombo(C1ComboBox cbo, string codeType, CommonCombo.ComboStatus status, string keyId = null, string selectedValue = null)
        {
            const string bizRuleName = "DA_SEL_MMD_COMMONCODE_NAME_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("KEYID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CMCDTYPE"] = codeType;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["KEYID"] = keyId;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { cbo.SelectedValuePath, cbo.DisplayMemberPath });

            DataRow newRow = dtBinding.NewRow();

            switch (status)
            {
                case CommonCombo.ComboStatus.ALL:
                    newRow[cbo.SelectedValuePath] = null;
                    newRow[cbo.DisplayMemberPath] = "-ALL-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    newRow[cbo.SelectedValuePath] = "SELECT";
                    newRow[cbo.DisplayMemberPath] = "-SELECT-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    newRow[cbo.SelectedValuePath] = string.Empty;
                    newRow[cbo.DisplayMemberPath] = "-N/A-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NONE:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(selectedValue)) cbo.SelectedValue = selectedValue;
        }

        private DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_COR_SEL_SYSTIME_G";

            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private bool ValidationSearch()
        {
            if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

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

        private void SetCommonCombo(C1ComboBox cbo, string codeType, CommonCombo.ComboStatus status, string useFlag = null, string selectedValue = null)
        {
            const string bizRuleName = "DA_SEL_MMD_MCS_COMMONCODE_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCDIUSE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = codeType;
            dr["CMCDIUSE"] = useFlag;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { cbo.SelectedValuePath, cbo.DisplayMemberPath });

            DataRow newRow = dtBinding.NewRow();

            switch (status)
            {
                case CommonCombo.ComboStatus.ALL:
                    newRow[cbo.SelectedValuePath] = null;
                    newRow[cbo.DisplayMemberPath] = "-ALL-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    newRow[cbo.SelectedValuePath] = "SELECT";
                    newRow[cbo.DisplayMemberPath] = "-SELECT-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    newRow[cbo.SelectedValuePath] = string.Empty;
                    newRow[cbo.DisplayMemberPath] = "-N/A-";
                    dtBinding.Rows.InsertAt(newRow, 0);
                    break;

                case CommonCombo.ComboStatus.NONE:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(selectedValue)) cbo.SelectedValue = selectedValue;
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            string buildingCode = string.IsNullOrEmpty(cboArea?.SelectedValue.GetString()) ? null : cboArea?.SelectedValue.GetString();

            const string bizRuleName = "DA_SEL_MMD_TSC_NAME_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("TSC_TP", typeof(string));
            inTable.Columns.Add("BLDG_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TSC_TP"] = equipmentType;
            dr["BLDG_CODE"] = buildingCode;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-ALL-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetMachineCombo(C1ComboBox cbo)
        {
            string machineType = string.IsNullOrEmpty(cboMachineType?.SelectedValue.GetString()) ? null : cboMachineType?.SelectedValue.GetString();
            string equipmentCode = string.IsNullOrEmpty(cboEquipment?.SelectedValue.GetString()) ? null : cboEquipment?.SelectedValue.GetString();

            const string bizRuleName = "DA_SEL_MONITORING_ALARM_STATUS_MACHINE_NAME_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("MACHINE_TP", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["MACHINE_TP"] = machineType;
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-ALL-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetUnitCombo(C1ComboBox cbo)
        {
            string unitType = string.IsNullOrEmpty(cboUnitType?.SelectedValue.GetString()) ? null : cboUnitType?.SelectedValue.GetString();
            string machineCode = string.IsNullOrEmpty(cboMachineName?.SelectedValue.GetString()) ? null : cboMachineName?.SelectedValue.GetString();

            const string bizRuleName = "DA_SEL_MONITORING_ALARM_STATUS_UNIT_NAME_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("UNIT_TP", typeof(string));
            inTable.Columns.Add("MACHINEID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["UNIT_TP"] = unitType;
            dr["MACHINEID"] = machineCode;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-ALL-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetAssemblyCombo(C1ComboBox cbo)
        {
            string assemblyType = string.IsNullOrEmpty(cboAssemblyType?.SelectedValue.GetString()) ? null : cboAssemblyType?.SelectedValue.GetString();
            string unitCode = string.IsNullOrEmpty(cboUnit?.SelectedValue.GetString()) ? null : cboUnit?.SelectedValue.GetString();

            const string bizRuleName = "DA_SEL_MONITORING_ALARM_STATUS_ASSY_NAME_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("ASSY_TP", typeof(string));
            inTable.Columns.Add("UNITID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ASSY_TP"] = assemblyType;
            dr["UNITID"] = unitCode;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-ALL-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
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