/*************************************************************************************
 Created Date : 2019.09.09
      Creator : 신광희C
   Decription : Port별 Skid Type 설정
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.09  신광희C : Initial Created.
 
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_SKIDTYPE_SETTING_PORT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SKIDTYPE_SETTING_PORT : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public bool IsUpdated;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        public CMM_ELEC_SKIDTYPE_SETTING_PORT()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    

        private void Initialize()
        {
            ApplyPermissions();
            InitializeCombo();
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            GetBizActorServerInfo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                Loaded -= C1Window_Loaded;
                Initialize();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectSkidTypeSettingByPortList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgSkidTypeSettingByPort)) return;

                dgSkidTypeSettingByPort.EndEdit();
                dgSkidTypeSettingByPort.EndEditRow(true);

                if (!ValidationForSave()) return;
                SaveSkidTypeSettingByPort();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectConveyorCombo(cboConveyor);
        }

        private void cboConveyor_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
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
                var queryIp = (from t in dtResult.AsEnumerable() where t.Field<string>("KEYID") == "BizActorIP" select new { bizActorIp = t.Field<string>("KEYVALUE") }).FirstOrDefault();
                if (queryIp != null) _bizRuleIp = queryIp.bizActorIp;

                var queryPort = (from t in dtResult.AsEnumerable() where t.Field<string>("KEYID") == "BizActorPort" select new { bizActorPort = t.Field<string>("KEYVALUE") }).FirstOrDefault();
                if (queryPort != null) _bizRulePort = queryPort.bizActorPort;

                var queryProtocol = (from t in dtResult.AsEnumerable() where t.Field<string>("KEYID") == "BizActorProtocol" select new { bizActorProtocol = t.Field<string>("KEYVALUE") }).FirstOrDefault();
                if (queryProtocol != null) _bizRuleProtocol = queryProtocol.bizActorProtocol;

                var queryServiceMode = (from t in dtResult.AsEnumerable() where t.Field<string>("KEYID") == "BizActorServiceMode" select new { bizActorServiceMode = t.Field<string>("KEYVALUE") }).FirstOrDefault();
                if (queryServiceMode != null) _bizRuleServiceMode = queryServiceMode.bizActorServiceMode;

                var queryServiceIndex = (from t in dtResult.AsEnumerable() where t.Field<string>("KEYID") == "BizActorServiceIndex" select new { bizActorServiceIndex = t.Field<string>("KEYVALUE") }).FirstOrDefault();
                if (queryServiceIndex != null) _bizRuleServiceIndex = queryServiceIndex.bizActorServiceIndex;
            }
        }

        private void SelectSkidTypeSettingByPortList()
        {
            try
            {

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgSkidTypeSettingByPort);

                const string bizRuleName = "DA_SEL_MCS_LOC_SKID_TYPE_SCV";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("POLARITY", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["POLARITY"] = cboElectrodeType.SelectedValue;
                inData["EQPTID"] = cboConveyor.SelectedValue;
                inDataTable.Rows.Add(inData);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgSkidTypeSettingByPort, result, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SaveSkidTypeSettingByPort()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_GUI_REG_LOC_MDL_TP";

                DataTable inDataTable = new DataTable("IN_LOC_INFORM");
                inDataTable.Columns.Add("LOCID", typeof(string));
                inDataTable.Columns.Add("MDL_TP", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOCID"] = DataTableConverter.GetValue(dgSkidTypeSettingByPort.SelectedItem, "PORT_ID");
                dr["MDL_TP"] = cboSkidType.SelectedValue;
                dr["UPDUSER"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_LOC_INFORM", "OUT_RETURN", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    IsUpdated = true;
                    Util.MessageInfo("MCS0003");
                    SelectSkidTypeSettingByPortList();
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void InitializeCombo()
        {
            SetCommonCombo(cboElectrodeType, "POLARITY_TP");
            SetCommonCombo(cboSkidType, "P_MDL_TP");

            SelectConveyorCombo(cboConveyor);
        }

        private void SetCommonCombo(C1ComboBox cbo, string codeType)
        {

            try
            {
                const string bizRuleName = "DA_SEL_MMD_MCS_COMMONCODE_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CMCDIUSE"] = "Y";
                inTable.Rows.Add(dr);
                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (string.Equals(codeType, "POLARITY_TP"))
                {
                    DataRow newRow = dtResult.NewRow();

                    newRow[cbo.SelectedValuePath.GetString()] = null;
                    newRow[cbo.DisplayMemberPath.GetString()] = "-ALL-";
                    dtResult.Rows.InsertAt(newRow, 0);
                }

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorCombo(C1ComboBox cbo)
        {
            try
            {
                const string bizRuleName = "DA_SEL_MMD_EQPT_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("POLARITY_TP", typeof(string));
                inTable.Columns.Add("EQPT_GROUP", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["POLARITY_TP"] = cboElectrodeType.SelectedValue;
                dr["EQPT_GROUP"] = "SCV";
                dr["USE_FLAG"] = "Y";

                inTable.Rows.Add(dr);
                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                DataRow newRow = dtResult.NewRow();

                newRow[cbo.SelectedValuePath.GetString()] = null;
                newRow[cbo.DisplayMemberPath.GetString()] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool ValidationForSave()
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgSkidTypeSettingByPort))
                {
                    Util.MessageInfo("MCS0007");
                    return false;
                }

                if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgSkidTypeSettingByPort.SelectedItem, "PORT_ID").GetString()))
                {
                    Util.MessageInfo("MCS0007");
                    return false;
                }

                if (cboSkidType.SelectedValue == null || string.IsNullOrEmpty(cboSkidType.SelectedValue.GetString()))
                {
                    Util.MessageInfo("MCS0005", ObjectDic.Instance.GetObjectName("SKID Type"));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnSave };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

    }
}