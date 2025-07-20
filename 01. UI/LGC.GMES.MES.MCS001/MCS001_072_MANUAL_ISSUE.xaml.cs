/*************************************************************************************
 Created Date : 2021.11.08
      Creator : 곽란영 대리
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
    /// MCS001_018_MANUAL_ISSUE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_072_MANUAL_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        CommonCombo _combo = new CommonCombo();

        private bool _isLoaded;
        private string _equipmentCode;
        private DataTable _dtRowColumnLayer;
        public bool IsUpdated;
        #endregion

        public MCS001_072_MANUAL_ISSUE()
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

            // VD 라인 콤보박스
            //SetVDLineCombo(cboVDEquipmentSegment);

            // VD 설비 콤보박스
            //SetVDEqpCombo(cboVDEquipment);

            // Destination 콤보박스 Setting
            //SetDestinationCombo(cboDestination);
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

            //SelectRowColumnlayerInfo(_equipmentCode);

            //_isLoaded = true;

            /*
            string sCststat = GetCstStat();
            
            // 실 SKID
            if (sCststat == "U")
            {
                lblVDEquipment.Visibility = Visibility.Visible;
                cboVDEquipment.Visibility = Visibility.Visible;
            }
            // 공 SKID
            else
            {
                lblVDEquipment.Visibility = Visibility.Hidden;
                cboVDEquipment.Visibility = Visibility.Hidden;
            }

            lblSubPort.Visibility = Visibility.Hidden;
            cboSubPort.Visibility = Visibility.Hidden;
            */

            if (_equipmentCode == "N1ASTO601")
            {
                lblStockerPort.Visibility = Visibility.Hidden;
                cboStockerPort.Visibility = Visibility.Hidden;

                lblVDEquipment.Visibility = Visibility.Hidden;
                cboVDEquipment.Visibility = Visibility.Hidden;

                lblport.Visibility = Visibility.Hidden;
                cboPort.Visibility = Visibility.Hidden;

                lblSubPort.Visibility = Visibility.Hidden;
                cboSubPort.Visibility = Visibility.Hidden;
            }
            else
            {
                lblLine.Visibility = Visibility.Hidden;
                cboLine.Visibility = Visibility.Hidden;

                SelectRowColumnlayerInfo(_equipmentCode);

                _isLoaded = true;

                string sCststat = GetCstStat();

                // 실 SKID
                if (sCststat == "U")
                {
                    lblVDEquipment.Visibility = Visibility.Visible;
                    cboVDEquipment.Visibility = Visibility.Visible;
                }
                // 공 SKID
                else
                {
                    lblVDEquipment.Visibility = Visibility.Hidden;
                    cboVDEquipment.Visibility = Visibility.Hidden;
                }

                lblSubPort.Visibility = Visibility.Hidden;
                cboSubPort.Visibility = Visibility.Hidden;
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
            if (cboPort.SelectedValue.GetString() == "N1AVWW602P01")
            {
                lblSubPort.Visibility = Visibility.Visible;
                cboSubPort.Visibility = Visibility.Visible;
                SetSubPortCombo(cboSubPort);
            }
            else
            {
                lblSubPort.Visibility = Visibility.Hidden;
                cboSubPort.Visibility = Visibility.Hidden;
                
                SetVDEqpCombo(cboVDEquipment);
            }
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnIssueReservation_Click(object sender, RoutedEventArgs e)
        {
            //string vdeqp = cboVDEquipment.SelectedValue.ToString();

            //if (vdeqp == "SELECT")
            //{
            //    Util.MessageInfo("SFU1733"); //예약 할 설비를 선택해주세요.
            //    return;
            //}

            /*
            // 실 SKID
            if (sCststat == "U")
                IssueReservation();
            // 공 SKID
            else
                IssueReservationEmpty();
            */


            // VD 3층 STK 경우 출고 대상 극성과 출고 포트 극성을 체크하여 다른 경우 팝업 표시
            if (cboStockerPort.SelectedValue.GetString() == "N1ASTO603")
            {
                string strRack = string.Empty;

                DataTable dtRack = new DataTable();
                dtRack.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = dtRack.NewRow();
                dr["RACK_ID"] = Util.NVC(dgRackInfo.GetCell(0, dgRackInfo.Columns["RACK_ID"].Index).Value);
                dtRack.Rows.Add(dr);

                DataTable dtResult_R = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RACK_INFO", "RQSTDT", "RSLTDT", dtRack);

                if (dtResult_R.Rows.Count > 0)
                    strRack = dtResult_R.Rows[0]["ZONE_ID"].ToString();

                DataTable dtPort = new DataTable();
                dtPort.Columns.Add("PORT_ID", typeof(string));

                dr = dtPort.NewRow();
                dr["PORT_ID"] = cboPort.SelectedValue.GetString();
                dtPort.Rows.Add(dr);

                DataTable dtResult_P = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PORT_OPT", "RQSTDT", "RSLTDT", dtPort);

                if (dtResult_P.Rows.Count == 0)
                    return;

                if (dtResult_P.Rows[0]["ELTR_TYPE_CODE"].ToString().Equals("A") || dtResult_P.Rows[0]["ELTR_TYPE_CODE"].ToString().Equals("C"))
                {
                    if (strRack != dtResult_P.Rows[0]["ELTR_TYPE_CODE"].ToString())
                    {
                        // 출고 포트 극성이 다릅니다. 출고 예약 진행 하시겠습니까?
                        Util.MessageConfirm("MCS0011", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                                SetIssueReservation();
                        });
                    }
                    else
                        SetIssueReservation();
                }
                else
                    SetIssueReservation();
            }
            else
                SetIssueReservation();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void cboRow_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRow?.SelectedValue != null)
            {
                GetColumnList();
            }
        }
        

        private void cboColumn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboColumn?.SelectedValue != null)
            {
                GetLayerList();
            }
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

            if (_equipmentCode == "N1ASTO601")
                IssueReservation_TBox();
            else
            {
                // 실 SKID
                if (sCststat == "U")
                    IssueReservation();
                // 공 SKID
                else
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
                //indataSet.Tables["INDATA"].Columns.Add("STK2_PORT_ID", typeof(string));

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
                        //dr["STK2_PORT_ID"] = cboSubPort.SelectedValue;
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
                        dr["EQPTID"] = "N1ASTO601";
                        dr["TO_PORT_ID"] = "N1ASTO601P02";
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
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            //string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "VWW", LoginInfo.CFG_AREA_ID }

            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
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

        private void SetSubPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_CBO_VD_STK2_NJ";
            string[] arrColumn = { "LANGID", "PORTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboPort.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetVDLineCombo(C1ComboBox cbo)
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }

        //private void SetVDEqpCombo(C1ComboBox cbo)
        //{
        //    string Stocker = string.Empty;

        //    if (cboStockerPort.SelectedValue.GetString() == "N1AVWW601")
        //        Stocker = "1";
        //    else if (cboStockerPort.SelectedValue.GetString() == "N1AVWW602")
        //        Stocker = "2";
        //    else if (cboStockerPort.SelectedValue.GetString() == "N1ASTO603")
        //        Stocker = "3";
            
        //    const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO_V2";
        //    string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE2" };
        //    string[] arrCondition = { LoginInfo.LANGID, "VD_EQPT_CODE_FOR_AGV", Stocker };
        //    string selectedValueText = cbo.SelectedValuePath;
        //    string displayMemberText = cbo.DisplayMemberPath;

        //    CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
            
        //}

        private void SetVDEqpCombo(C1ComboBox cbo)
        {
            string Stocker = string.Empty;

            if (cboStockerPort.SelectedValue.GetString() == "N1AVWW601")
                Stocker = "1";
            else if (cboStockerPort.SelectedValue.GetString() == "N1AVWW602")
                Stocker = "2";
            else if (cboStockerPort.SelectedValue.GetString() == "N1ASTO603")
                Stocker = "3";

            string VDSeq = null;

            switch (cboPort.SelectedValue.GetString())
            {
                case "N1ASTO603P02":
                case "N1ASTO603P03":
                    VDSeq = "1";
                    break;
                case "N1ASTO603P04":
                    VDSeq = "2";
                    break;
            }

            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO_V2";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE2", "ATTRIBUTE3" };
            string[] arrCondition = { LoginInfo.LANGID, "VD_EQPT_CODE_FOR_AGV", Stocker, VDSeq };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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
            {
                return (string)dtResult.Rows[0]["CSTSTAT"];
            }
            else
            {
                return "U";
            }
        }

        #endregion
    }
}
 