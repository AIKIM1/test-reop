/*************************************************************************************
 Created Date : 2023.12.20
      Creator : 이원열
   Decription : NTC/VD 대기 창고 모니터링 수동출고 예약 팝업 - 호출대상 UI(NTC/VD대기 창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
    22023.12.20 이원열 : NJ N2 ESS NTC/VD 대기 창고 모니터링 수동출고 예약 팝업  Create

**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_091_MANUAL_RESERVATION.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_091_MANUAL_RESERVATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        CommonCombo _combo = new CommonCombo();

        private bool _isLoaded;
        private string _equipmentCode;
        private DataTable _dtRowColumnLayer;
        public bool IsUpdated;

        private string _SkidCode;
        private string _Cststat;

        #endregion

        public MCS001_091_MANUAL_RESERVATION()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataTable dtRackInfo = parameters[1] as DataTable;
                _SkidCode = Util.NVC(parameters[2]);

                if (dtRackInfo != null)
                    SetRackInfo(dtRackInfo);
            }

            InitializeControls();

            SelectRowColumnlayerInfo(_equipmentCode);

            _isLoaded = true;
        }

        private void InitializeControls()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            SetCboCombo();
        }
        #endregion

        #region Event       

        private void SetRackInfo(DataTable dtRackInfo)
        {
            Util.GridSetData(dgRackInfo, dtRackInfo, null, true);
        }

        private void SetCboCombo()
        {
            _Cststat = GetCstStat();

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);
            SetStockerCombo(cboStockerPort);

            if (_SkidCode.Equals("N") && _Cststat != "E")
            {
                SetPortCombo(cboPort, Skid_Chk(_SkidCode));
                lblVDEquipment.Visibility = Visibility.Hidden;
                cboVDEquipment.Visibility = Visibility.Hidden;
            }
            else if (_SkidCode.Equals("V") && _Cststat != "E")
            {
                SetPortCombo(cboPort, Skid_Chk(_SkidCode));

                SetVDEqpCombo(cboVDEquipment);

                lblVDEquipment.Visibility = Visibility.Visible;
                cboVDEquipment.Visibility = Visibility.Visible;
            }
            else if (_Cststat == "E")
            {
                SetPortCombo(cboPort, Skid_Chk(_Cststat));

                lblVDEquipment.Visibility = Visibility.Hidden;
                cboVDEquipment.Visibility = Visibility.Hidden;
            }
            else { };
        }

        private void SetPortCombo(C1ComboBox cbo, string sPortType)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_OUT_CBO";

            string[] arrColumn = { "LANGID", "EQPTID", "PORT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboStockerPort.SelectedValue.GetString(), sPortType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void cboStockerPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //
        }

        private void cboPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetVDEqpCombo(cboVDEquipment);
        }

        private void btnIssueReservation_Click(object sender, RoutedEventArgs e)
        {
            SetIssueReservation();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cboRow_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRow?.SelectedValue != null)
                GetColumnList();
        }

        private void cboColumn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboColumn?.SelectedValue != null)
                GetLayerList();
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboStocker?.SelectedValue == null)
                return;

            SelectRowColumnlayerInfo(cboStocker.SelectedValue.GetString());
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //
        }

        #endregion

        #region Mehod

        private void GetColumnList()
        {
            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN") }).Distinct().ToList();

            cboColumn.ItemsSource = queryColumn.ToDataTable().Copy().AsDataView();
            cboColumn.SelectedIndex = 0;
        }

        private void GetLayerList()
        {
            var queryLayer = (from t in _dtRowColumnLayer.AsEnumerable()
                              where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                              && t.Field<string>("Y_PSTN") == cboColumn.SelectedValue.GetString()
                              orderby t.Field<string>("Z_PSTN").GetInt()
                              select new { CBO_NAME = t.Field<string>("Z_PSTN"), CBO_CODE = t.Field<string>("Z_PSTN") }).ToList();

            cboLayer.ItemsSource = queryLayer.ToDataTable().Copy().AsDataView();
            cboLayer.SelectedIndex = 0;
        }

        private void SetIssueReservation()
        {
            if (_Cststat == "U")    // 실 SKID
                IssueReservation();
            else                   // 공 SKID
                IssueReservationEmpty();
        }

        private void IssueReservation()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                bizRuleName = "BR_MCS_REG_LOGIS_CMD_VD_PSR_PSP_NJ";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");
                indataSet.Tables.Add("IN_RACK");

                indataSet.Tables["INDATA"].Columns.Add("SRCTYPE", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("TO_PORT_ID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("VD_EQPTID", typeof(string));

                indataSet.Tables["IN_RACK"].Columns.Add("FROM_RACK_ID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgRackInfo.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        DataRow dr = indataSet.Tables["INDATA"].NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["EQPTID"] = cboStockerPort.SelectedValue;
                        dr["TO_PORT_ID"] = cboPort.SelectedValue;

                        if (_SkidCode.Equals("N"))
                            dr["VD_EQPTID"] = cboPort.SelectedValue;  // NTC 는 PORT 에서 요청
                        else if (_SkidCode.Equals("V"))
                            dr["VD_EQPTID"] = cboVDEquipment.SelectedValue;

                        dr["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(dr);
                    }

                    if (row.Type == DataGridRowType.Item)
                    {
                        DataRow dr = indataSet.Tables["IN_RACK"].NewRow();
                        dr["FROM_RACK_ID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID");

                        indataSet.Tables["IN_RACK"].Rows.Add(dr);
                    }
                }
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_RACK", "OUTDATA", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void IssueReservationEmpty()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                //bizRuleName = "BR_MCS_REG_LOGIS_CMD_E_SKID_OUT_VD_NJ";
                bizRuleName = "BR_MCS_REG_LOGIS_CMD_E_SKID_OUT_ESS";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");

                indataSet.Tables["INDATA"].Columns.Add("SRCTYPE", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("CSTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("FROM_RACK_ID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("TO_PORT_ID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgRackInfo.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        DataRow dr = indataSet.Tables["INDATA"].NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["EQPTID"] = cboStockerPort.SelectedValue;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "MCS_CST_ID");
                        dr["FROM_RACK_ID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID");
                        dr["TO_PORT_ID"] = cboPort.SelectedValue;
                        dr["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectRowColumnlayerInfo(string equipmentCode)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_BY_XYZ_PSTN";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = equipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    _dtRowColumnLayer = searchResult.Copy();

                    var queryRow = (from t in _dtRowColumnLayer.AsEnumerable()
                                    orderby t.Field<string>("X_PSTN").GetInt()
                                    select new
                                    {
                                        CBO_CODE = t.Field<string>("X_PSTN")
                                    ,
                                        CBO_NAME = t.Field<string>("X_PSTN")
                                    }).Distinct().ToList()
                        ;
                    cboRow.ItemsSource = queryRow.ToDataTable().Copy().AsDataView();
                    cboRow.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            //동별 공통코드
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "VD_STK_CBO_BIZ";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

            string scboBiz = dtResult.Rows[0]["COM_CODE"].ToString();

            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(scboBiz, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }
        
        private void SetVDEqpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE";
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR1", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "VD_STK_REQ_VD_EQPT_LIST", cboPort.SelectedValue.GetString(), cboStockerPort.SelectedValue.GetString() };
            string selectedValueText = "COM_CODE";
            string displayMemberText = "COM_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetOutLineEqpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_CBO_VD_NJ";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboStockerPort.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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

        private string GetCstStat()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CST_ID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CST_ID"] = Util.NVC(dgRackInfo.GetCell(0, dgRackInfo.Columns["MCS_CST_ID"].Index).Value);
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EMPTY_CST", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count > 0 && !string.IsNullOrWhiteSpace((string)dtResult.Rows[0]["CSTSTAT"]))
                return (string)dtResult.Rows[0]["CSTSTAT"];
            else
                return "U";
        }

        private string Skid_Chk(string sComCode)
        {
            try
            {
                string sComTypeCode = "PORT_TYPE_CODE";

                DataTable inTable = new DataTable();

                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                
                DataRow row = inTable.NewRow();
                row["COM_TYPE_CODE"] = sComTypeCode;
                row["COM_CODE"] = sComCode;
                row["USE_FLAG"] = "Y";

                inTable.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", inTable);

                if (dtResult.Rows.Count > 0 && !string.IsNullOrWhiteSpace((string)dtResult.Rows[0]["ATTR1"]))
                    return (string)dtResult.Rows[0]["ATTR1"];
                else
                    return "";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion
    }
}

