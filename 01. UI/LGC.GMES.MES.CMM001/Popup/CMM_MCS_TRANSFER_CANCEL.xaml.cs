/*************************************************************************************
 Created Date : 2019.10.21
      Creator : 신광희C
   Decription : 수동반송 예약 취소
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.21  신 광희 : Initial Created.
  2021.02.02  신광희      : ESNB 증설 자동차 2동 전극(EF), 조립(AA) 수동출고예약, 출고예약취소, 데이터출고 수정
  2022.06.01  오화백      : 폴란드 3공장 및 GM 공장에서 수동출고예약, 출고예약취소, 데이터출고  사용할수 있도록 AREA 정보 추가
  2022.06.06  오화백      : 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 ( LOGIS_SYST_TYPE_CODE  ) 

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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_MCS_TRANSFER_CANCEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_MCS_TRANSFER_CANCEL : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public bool IsUpdated;
        private DataTable _dtTransferCancel;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        private readonly Util _util = new Util();

        private string _LOGIS_Type_Code; //  2022.06.06  오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정
        public CMM_MCS_TRANSFER_CANCEL()
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
            LogisType();
            InitializeControls();
            InitializeCombo();
         
        }

        private void InitializeControls()
        {
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD")
            {
                gdTransferInfo.Visibility = Visibility.Collapsed;
                TextBlockEquipmentType.Text = ObjectDic.Instance.GetObjectName("EQP_TYPE");
                dgManualTransferCancel.Columns["JOBID"].Visibility = Visibility.Collapsed;

                dgManualTransferCancel.Columns["REQ_TRFID"].Header = ObjectDic.Instance.GetObjectName("반송지시번호");
                dgManualTransferCancel.Columns["INSDTTM"].Header = ObjectDic.Instance.GetObjectName("반송지시일시");
                dgManualTransferCancel.Columns["REQ_TRF_STAT"].Header = ObjectDic.Instance.GetObjectName("반송지시상태");
                dgManualTransferCancel.Columns["SRC_EQPT_TP"].Header = ObjectDic.Instance.GetObjectName("출발지 설비군");
                dgManualTransferCancel.Columns["DST_EQPT_TP"].Header = ObjectDic.Instance.GetObjectName("목적지 설비군");

                dgManualTransferCancel.Columns["CHK"].DisplayIndex = 0;
                dgManualTransferCancel.Columns["REQ_TRFID"].DisplayIndex = 1;
                dgManualTransferCancel.Columns["INSDTTM"].DisplayIndex = 2;
                dgManualTransferCancel.Columns["CARRIERID"].DisplayIndex = 3;
                dgManualTransferCancel.Columns["REQ_TRF_STAT"].DisplayIndex = 4;
                dgManualTransferCancel.Columns["SRC_EQPT_TP"].DisplayIndex = 5;
                dgManualTransferCancel.Columns["SRC_EQPTID"].DisplayIndex = 6;
                dgManualTransferCancel.Columns["SRC_EQPTNAME"].DisplayIndex = 7;
                dgManualTransferCancel.Columns["DST_EQPT_TP"].DisplayIndex = 8;
                dgManualTransferCancel.Columns["DST_EQPTID"].DisplayIndex = 9;
                dgManualTransferCancel.Columns["DST_EQPTNAME"].DisplayIndex = 10;

                SearchArea.Visibility = Visibility.Collapsed;
            }


            DateTime systemDateTime = GetSystemTime();

            if (dtpDateFrom != null)
                dtpDateFrom.SelectedDateTime = systemDateTime;

            if (dtpDateTo != null)
                dtpDateTo.SelectedDateTime = systemDateTime.AddDays(+1);

        }

        private void InitializeCombo()
        {
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD")
            {
                //설비군
                SetEquipmentTypeCombo(cboEquipmentType);
            }
            else
            {
                //설비군
                SetCommonCombo(cboEquipmentType, "EQPT_TP");
            }

            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);
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
                Initialize();
               
                object[] tmps = C1WindowExtension.GetParameters(this);
                _dtTransferCancel = tmps[0] as DataTable;   // 

                if (CommonVerify.HasTableRow(_dtTransferCancel))
                {
                    //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                    //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                    //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
                    if (_LOGIS_Type_Code == "RTD")
                        SelectFirstManualTransferCancelListByEsnb();
                    else
                        SelectFirstManualTransferCancelList();
                }

                Loaded -= C1Window_Loaded;
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDataGridCheckHeaderInitialize(dgManualTransferCancel);

                //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
                if (_LOGIS_Type_Code == "RTD")
                {
                    SelectManualTransferCancelListByEsnb();
                }
                else
                {
                    SelectManualTransferCancelList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
                if (_LOGIS_Type_Code == "RTD")
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                else
                    DataTableConverter.SetValue(row.DataItem, "CHK", "1");
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
                if (_LOGIS_Type_Code == "RTD")
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                else
                    DataTableConverter.SetValue(row.DataItem, "CHK", "0");
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void btnManualTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualTransferCancel()) return;

            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD")
            {
                SaveManualTransferCancelByEsnb();
            }
            else
            {
                SaveManualTransferCancel();
            }
        }

        #endregion

        #region Mehod

        private void GetBizActorServerInfo()
        {
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC") return;
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD") return;
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

        private void SelectFirstManualTransferCancelList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "DA_SEL_MCS_REQ_TRF_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("REQ_TRFID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));

                DateTime systemDateTime = GetSystemTime();

                foreach (DataRow row in _dtTransferCancel.Rows)
                {
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["REQ_TRFID"] = row["RequestTransferId"].ToString();
                    newRow["FROM_DATE"] = systemDateTime.AddMonths(-1).ToString("yyyyMMdd");
                    newRow["TO_DATE"] = systemDateTime.ToString("yyyyMMdd");
                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);

                    if (CommonVerify.HasTableRow(result))
                    {
                        C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgManualTransferCancel.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                        StackPanel allPanel = allColumn?.Header as StackPanel;
                        CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                        allCheck.IsChecked = true;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SelectFirstManualTransferCancelListByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "BR_MHS_SEL_TRF_CMD_CANCEL_BY_UI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                foreach (DataRow row in _dtTransferCancel.Rows)
                {
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["CSTID"] = row["CARRIERID"].ToString();
                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);

                    if (CommonVerify.HasTableRow(result))
                    {
                        C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgManualTransferCancel.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                        StackPanel allPanel = allColumn?.Header as StackPanel;
                        CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                        allCheck.IsChecked = true;
                    }

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectManualTransferCancelList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "DA_SEL_MCS_REQ_TRF_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("REQ_TRFID", typeof(string));
                inDataTable.Columns.Add("CARRIERID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("EQPT_TP", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                inData["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inData["REQ_TRFID"] = !string.IsNullOrEmpty(txtRequestTransferId.Text.Trim()) ? txtRequestTransferId.Text : null;
                inData["CARRIERID"] = !string.IsNullOrEmpty(txtCarrierId.Text.Trim()) ? txtCarrierId.Text : null;
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["EQPT_TP"] = cboEquipmentType.SelectedValue;
                inDataTable.Rows.Add(inData);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SelectManualTransferCancelListByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "BR_MHS_SEL_TRF_CMD_CANCEL_BY_UI";

                DataTable inDataTable = new DataTable("IN_DATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("EQPT_TP", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROM_DATE"] = dtpDateFrom.SelectedDateTime;
                inData["TO_DATE"] = dtpDateTo.SelectedDateTime;
                inData["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text.Trim()) ? txtCarrierId.Text : null;
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["EQPT_TP"] = cboEquipmentType.SelectedValue;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveManualTransferCancel()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER_CHG_STAT";

                DataSet ds = new DataSet();
                DataTable requestTransferInfoTable = ds.Tables.Add("IN_REQ_TRF_INFO");
                requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
                requestTransferInfoTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgManualTransferCancel.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = requestTransferInfoTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        requestTransferInfoTable.Rows.Add(newRow);
                    }
                }

                DataTable changeStateTable = ds.Tables.Add("IN_CHG_STAT");
                changeStateTable.Columns.Add("CHG_STAT", typeof(string));
                DataRow dr = changeStateTable.NewRow();
                dr["CHG_STAT"] = "CANCEL";
                changeStateTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService_Multi(bizRuleName, "IN_REQ_TRF_INFO,IN_CHG_STAT", null, (bizResult, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnSearch_Click(btnSearch, null);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SaveManualTransferCancelByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_CANCEL_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgManualTransferCancel.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnSearch_Click(btnSearch, null);

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            DataTable inDataTable = new DataTable("INDATA");

            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD")
            {
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
                }

                return systemDateTime;
            }
            else
            {
                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("BR_COR_SEL_SYSTIME_G", "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
                }

                return systemDateTime;
            }
        }

        private bool ValidationManualTransferCancel()
        {
            C1DataGrid dg = dgManualTransferCancel;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
            if (_LOGIS_Type_Code == "RTD")
            {
                if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }
            }
            else
            {
                if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }
            }

            return true;
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

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentCombo(C1ComboBox cbo, string equipmentType = null)
        {
            try
            {
                string bizRuleName;
                DataTable dtResult;

                //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                //2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 (LOGIS_SYST_TYPE_CODE)
                if (_LOGIS_Type_Code == "RTD")
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_FOR_MHS";

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("EQGRID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["EQGRID"] = equipmentType;
                    inTable.Rows.Add(dr);
                    dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                }
                else
                {
                    bizRuleName = "DA_SEL_MMD_EQPT_CBO";

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQPT_TP", typeof(string));
                    inTable.Columns.Add("USE_FLAG", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPT_TP"] = equipmentType;
                    dr["USE_FLAG"] = "Y";
                    inTable.Rows.Add(dr);
                    dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                }

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

        private void SetEquipmentTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MHS_SEL_EQUIPMENTGROUP_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        ///  2022 06 06 오화백 동 구분을 하드코딩에서 동별공통코드를 조회하여 처리하도록 수정 ( LOGIS_SYST_TYPE_CODE  )
        /// </summary>
        private void LogisType()
        {

            //동별 공통코드
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("COM_CODE", typeof(string));
            RQSTDT.Columns.Add("USE_FLAG", typeof(string));
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "LOGIS_SYST_TYPE_CODE";
            dr["COM_CODE"] = "RTD";
            dr["USE_FLAG"] = "Y";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                _LOGIS_Type_Code = dtResult.Rows[0]["COM_CODE"].ToString();
            }
            else
            {
                _LOGIS_Type_Code = string.Empty;
            }

        }
        #endregion


    }
}