/*************************************************************************************
 Created Date : 2023.09.18
      Creator : 박생규
   Decription : VD 대기 창고 모니터링 수동출고 예약 팝업 - 호출대상 UI(VD 대기 창고 모니터링)
--------------------------------------------------------------------------------------
 [Change History]
    0000.00.00  홍길동 차장 : 수정 내용 작성.
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
    /// MCS001_072_MANUAL_RESERVATION.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_072_MANUAL_RESERVATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        CommonCombo _combo = new CommonCombo();

        private bool _isLoaded;
        private string _equipmentCode;
        private DataTable _dtRowColumnLayer;
        public bool IsUpdated;

        private DataTable _dtStkInfo;    // STK 설비 기준 정보
        private string _sStkType;       // TBOX, VD STK 구분
        #endregion

        public MCS001_072_MANUAL_RESERVATION()
        {
            InitializeComponent();
        }

        #region Initialize 

        private void InitializeControls()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            SetStockerCombo(cboStockerPort);

            SetLine(cboLine);
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataTable dtRackInfo = parameters[1] as DataTable;

                if (dtRackInfo != null)
                    SetRackInfo(dtRackInfo);
            }

            //동별 공통코드
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "VD_STK_GRID_COL_NAME";
            dr["COM_CODE"] = _equipmentCode;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

            _dtStkInfo = dtResult.Copy();
            _sStkType = _dtStkInfo.Rows[0]["ATTR5"].ToString();
            
            if (_sStkType.Equals("TBOX"))
            {
                lblStockerPort.Visibility = Visibility.Hidden;
                cboStockerPort.Visibility = Visibility.Hidden;

                lblVDEquipment.Visibility = Visibility.Hidden;
                cboVDEquipment.Visibility = Visibility.Hidden;

                lblport.Visibility = Visibility.Hidden;
                cboPort.Visibility = Visibility.Hidden;
            }
            else
            {
                lblLine.Visibility = Visibility.Hidden;
                cboLine.Visibility = Visibility.Hidden;

                SelectRowColumnlayerInfo(_equipmentCode);

                _isLoaded = true;

                string sCststat = GetCstStat();
                                
                if (sCststat == "U")    // 실 SKID
                {
                    lblVDEquipment.Visibility = Visibility.Visible;
                    cboVDEquipment.Visibility = Visibility.Visible;
                }
                else                   // 공 SKID
                {
                    lblVDEquipment.Visibility = Visibility.Hidden;
                    cboVDEquipment.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SetRackInfo(DataTable dtRackInfo)
        {
            Util.GridSetData(dgRackInfo, dtRackInfo, null, true);
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    string tabItem = ((C1TabItem) ((ItemsControl) sender).Items.CurrentItem).Name.GetString();

                    if (string.Equals(tabItem, "TabRackMove"))
                    {

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboStockerPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetPortCombo(cboPort);
            SetVDEqpCombo(cboVDEquipment);
        }

        private void cboPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetVDEqpCombo(cboVDEquipment);
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

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

        #endregion

        #region Mehod

        private void GetColumnList()
        {
            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN")}).Distinct().ToList();

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
            string sCststat = GetCstStat();

            if (_sStkType.Equals("TBOX"))
                IssueReservation_TBox();
            else
            {
                if (sCststat == "U")    // 실 SKID
                    IssueReservation();
                else                   // 공 SKID
                    IssueReservationEmpty();
            }
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

                bizRuleName = "BR_MCS_REG_LOGIS_CMD_E_SKID_OUT_VD_NJ";

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

        /// <summary>
        /// T-Box STK 출고
        /// </summary>
        private void IssueReservation_TBox()
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
                        dr["EQPTID"] = _dtStkInfo.Rows[0]["COM_CODE"].ToString();   // STK 설비 ID
                        dr["TO_PORT_ID"] = _dtStkInfo.Rows[0]["ATTR2"].ToString();  // STK 출고 포트
                        dr["VD_EQPTID"] = cboLine.SelectedValue.ToString();
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
                            select new { CBO_CODE = t.Field<string>("X_PSTN")
                            , CBO_NAME = t.Field<string>("X_PSTN") }).Distinct().ToList()
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

        private bool ValidationIssueReservation()
        {
            return true;
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            /// 6동 모니터링 화면을 타동에서 같이 사용
            /// STK 설비 구조가 변경되어 조회하는 BIZ가 분리
            /// 동별 호출 BIZ를 동별 공통코드 관리를 통해 처리
            /// COM_TYPE_CODE : VD_STK_CBO_BIZ

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
            
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            //string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private static void SetLine(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_EQUIPMENTSEGMENT_NJ";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_CBO_VD_NJ";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboStockerPort.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetVDLineCombo(C1ComboBox cbo)
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }

        private void SetVDEqpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE";
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "VD_STK_REQ_VD_EQPT_LIST", cboStockerPort.SelectedValue.GetString() };
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

        #endregion
    }
}
 