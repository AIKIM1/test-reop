/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : 라인별 Stocker 현황 상세
 ------------------------------------------------------------------------------------
  [Change History]
    2021.04.06  정용석 : Initial Created.
    2021.08.19  정용석 : 장기재고 연동내역 추가
    2021.09.25  김길용 : 수동포트배출 호출 비즈 수정, 컬럼 추가 및 Visibility 수정
    2021.10.27  김길용 : C20211013-000001 오경석 책임 요청사항으로 컬럼추가 (WIPHOLD_USERID, WIPHOLD_DTTM)
    2021.11.04  김길용 : ESWA PACK3 적용을 위한 하드코딩 제거
    2022.01.27  김길용 : 다중 키인시 자동 체크기능 추가
    2022.03.21  김길용 : 팩3동 모듈 스토커 내의 비정상 캐리어 정리를 위한 버튼 추가
    2022.06.20  김길용 : 수동포트정보 가져오기 수정 (MCS 비즈호출버전 - MEB7단창고에서 STK - STK 수동명령 개선 요구사항, STK정보 + Manual Port정보 표출)
    2022.12.01  정용석 : 조회조건 & 장표 그리드에 ROUTID 추가
    2023.07.14  김길용 : E20230516-000486 [WA GMES Pack] 팩물류 연관 수동배출명령 시 MES 이력 추가를 위한 기능수정(개선방안)
    2023.08.26  정용석 : E20230510-001124 Additional Hold Name Column, Detail of Stocker Status (Module) - WIPHOLD 및 QMSHOLD 정보 추가
 ************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_024 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private StockerDetailDataHelper dataHelper = new StockerDetailDataHelper();
        private int stockerIDMultiComboBindDataCount = 0;
        private int equipmentSegmentIDMultiComboBindDataCount = 0;
        private int productIDMultiComboBindDataCount = 0;
        private int assyEquipmentIDMultiComboBindDataCount = 0;
        private int elecEquipmentIDMultiComboBindDataCount = 0;
        private int processIDMultiComboBindDataCount = 0;
        private int reqWipTypeCodeMultiComboBindDataCount = 0;
        private int routIDMultiComboBindDataCount = 0;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK003_024()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void SyncSearchCondition(MultiSelectionBox multiSelectionBox, string inputID, string searchKey)
        {
            DataTable dt = DataTableConverter.Convert(multiSelectionBox.ItemsSource);
            if (string.IsNullOrEmpty(inputID))
            {
                multiSelectionBox.Check(-1);
            }
            else
            {
                int index = 0;
                foreach (DataRowView drv in dt.AsDataView())
                {
                    if (inputID.Contains(drv[searchKey].ToString()))
                    {
                        multiSelectionBox.Check(index++);
                    }
                    else
                    {
                        multiSelectionBox.Uncheck(index++);
                    }
                }
            }
        }

        // Initialize
        private void Initialize()
        {
            // Multi Combobox
            PackCommon.SetC1ComboBox(this.dataHelper.GetUseFlagInfo(), this.cboInspectionFlag, "All");
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetStockerInfo(), this.cboMultiStockerID, ref this.stockerIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackLineInfo(), this.cboMultiEquipmentSegmentID, ref this.equipmentSegmentIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetPackProcessInfo(), this.cboMultiProcessID, ref this.processIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetProductInfo(), this.cboMultiProductID, ref this.productIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetAssyEquipmentInfo(), this.cboMultiAssyEquipmentID, ref this.assyEquipmentIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetElecEquipmentInfo(), this.cboMultiElecEquipmentID, ref this.elecEquipmentIDMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRequestWipTypeCodeInfo(), this.cboMultiReqWipTypeCode, ref this.reqWipTypeCodeMultiComboBindDataCount);
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRouteInfo(), this.cboMultiRoutID, ref this.routIDMultiComboBindDataCount);

            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
            Util.gridClear(this.dgLOTInfo);
            this.cboOutPort.IsEnabled = false;
            this.cboOutPort.ItemsSource = null;

            object[] arrFramOperationParameters = FrameOperation.Parameters;
            if (arrFramOperationParameters == null || arrFramOperationParameters.Length <= 0)
            {
                return;
            }

            string processID = arrFramOperationParameters[0].ToString();
            string equipmentSegmentID = arrFramOperationParameters[1].ToString();
            string productID = arrFramOperationParameters[2].ToString();
            string requestWipTypeCode = arrFramOperationParameters[3].ToString();
            string stockerID = arrFramOperationParameters[4].ToString();
            string routID = arrFramOperationParameters[5].ToString();

            // 검색조건 연동
            this.SyncSearchCondition(this.cboMultiStockerID, stockerID, "EQPTID");
            this.SyncSearchCondition(this.cboMultiProcessID, processID, "PROCID");
            this.SyncSearchCondition(this.cboMultiEquipmentSegmentID, equipmentSegmentID, "EQSGID");
            this.SyncSearchCondition(this.cboMultiProductID, productID, "PRODID");
            this.SyncSearchCondition(this.cboMultiReqWipTypeCode, requestWipTypeCode, "REQ_WIP_TYPE_CODE");
            this.SyncSearchCondition(this.cboMultiRoutID, routID, "ROUTID");
            this.cboMultiAssyEquipmentID.Check(-1);
            this.cboMultiElecEquipmentID.Check(-1);

            this.SearchProcess();
        }

        // Validation Check
        private bool ValidationCheck()
        {
            bool returnValue = true;

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }
            if (this.dgLOTInfo == null || this.dgLOTInfo.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }

            if (this.dgLOTInfo.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgLOTInfo.ItemsSource);
            if (!CommonVerify.HasTableRow(dt))
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                this.dgLOTInfo.Focus();
                return false;
            }

            // Validation Check
            var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));

            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                this.dgLOTInfo.Focus();
                return false;
            }

            // Stocker 중복 Check
            var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).GroupBy(x => new
            {
                STOCKER_ID = x.Field<string>("STOCKER_ID")
            }).Select(grp => new
            {
                STOCKER_ID = grp.Key.STOCKER_ID
            });

            if (query.Count() <= 0 || query.Count() > 1)
            {
                Util.Alert("SFU8387");
                return false;
            }

            // Manual Port 선택 여부
            if (this.cboOutPort.SelectedValue == null || string.IsNullOrEmpty(this.cboOutPort.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고포트")); // %1(을)를 선택하세요.
                this.cboOutPort.Focus();
                return false;
            }

            return returnValue;
        }

        // 조회
        private void SearchProcess()
        {
            // Validation of Search Condition
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiStockerID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("호기")); // %1(을)를 선택하세요.
                this.cboMultiStockerID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProductID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PRODID")); // %1(을)를 선택하세요.
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiEquipmentSegmentID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                this.cboMultiEquipmentSegmentID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProcessID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("공정")); // %1(을)를 선택하세요.
                this.cboMultiProcessID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiAssyEquipmentID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("조립설비")); // %1(을)를 선택하세요.
                this.cboMultiAssyEquipmentID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiElecEquipmentID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("전극설비")); // %1(을)를 선택하세요.
                this.cboMultiElecEquipmentID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiReqWipTypeCode.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("재공구분")); // %1(을)를 선택하세요.
                this.cboMultiElecEquipmentID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiRoutID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("경로")); // %1(을)를 선택하세요.
                this.cboMultiElecEquipmentID.Focus();
                return;
            }

            this.cboOutPort.ItemsSource = null;
            this.cboOutPort.IsEnabled = false;
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
            Util.gridClear(this.dgLOTInfo);
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                string stockerID = this.cboMultiStockerID.SelectedItems.Count().Equals(this.stockerIDMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiStockerID.SelectedItemsToString);
                string productID = Convert.ToString(this.cboMultiProductID.SelectedItemsToString);  // 제품 : 필수
                string equipmentSegmentID = Convert.ToString(this.cboMultiEquipmentSegmentID.SelectedItemsToString);    // 라인 : 필수
                string processID = Convert.ToString(this.cboMultiProcessID.SelectedItemsToString);  // 공정 : 필수
                string LOTID = this.txtLOTID.Text.ToString();
                string assyEqptID = this.cboMultiAssyEquipmentID.SelectedItems.Count().Equals(this.assyEquipmentIDMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiAssyEquipmentID.SelectedItemsToString);
                string elecEqptID = this.cboMultiElecEquipmentID.SelectedItems.Count().Equals(this.elecEquipmentIDMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiElecEquipmentID.SelectedItemsToString);
                string inspectionFlag = this.cboInspectionFlag.SelectedValue.ToString();
                string requestWipTypeCode = this.cboMultiReqWipTypeCode.SelectedItems.Count().Equals(this.reqWipTypeCodeMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiReqWipTypeCode.SelectedItemsToString);
                string routID = this.cboMultiRoutID.SelectedItems.Count().Equals(this.routIDMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiRoutID.SelectedItemsToString);
                DataTable dt = this.dataHelper.GetStockerDetailList(stockerID, processID, equipmentSegmentID, productID, elecEqptID, assyEqptID, inspectionFlag, LOTID, requestWipTypeCode, routID);

                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtGridRowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgLOTInfo, dt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회
        private void SearchExcelLoad()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                string LOTIDList = string.Empty;
                this.txtLOTID.Text = string.Empty;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                openFileDialog.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (openFileDialog.ShowDialog() == true)
                {
                    using (Stream stream = openFileDialog.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                        if (dtExcelData != null)
                        {
                            for (int i = 0; i < dtExcelData.Rows.Count; i++)
                            {
                                LOTIDList = LOTIDList + dtExcelData.Rows[i][0].ToString() + ",";
                            }
                        }
                    }
                    this.txtLOTID.Text = LOTIDList.Substring(0, LOTIDList.Length - 1);
                    this.SearchProcess();
                    this.txtLOTID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 조회
        private void SearchClipboardData()
        {
            try
            {
                string[] separators = new string[] { "\r\n" };
                string clipboardText = Clipboard.GetText();
                string[] arrLOTIDList = clipboardText.Split(separators, StringSplitOptions.None);

                if (arrLOTIDList.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                    return;
                }

                for (int i = 0; i < arrLOTIDList.Length; i++)
                {
                    if (string.IsNullOrEmpty(arrLOTIDList[i]))
                    {
                        Util.MessageInfo("SFU1190", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                return;
                            }
                        });
                    }
                }

                this.txtLOTID.Text = string.Join(",", arrLOTIDList);
                this.SearchProcess();
                this.txtLOTID.Text = string.Empty;
                if (this.dgLOTInfo == null || this.dgLOTInfo.ItemsSource == null)
                {
                    return;
                }

                PackCommon.GridCheckAllFlag(this.dgLOTInfo, true, "CHK");
                this.dgLOTInfo_CommittedEdit(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Grid의 Header CheckBox Check 또는 Uncheck했을때
        private void SetGridRowChecked(CheckBox checkBox, bool check)
        {
            if (this.dgLOTInfo == null || this.dgLOTInfo.ItemsSource == null)
            {
                return;
            }

            PackCommon.GridCheckAllFlag(this.dgLOTInfo, check, "CHK");
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnPortOut_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheck())
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(this.dgLOTInfo.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
            DataTable dtPortInfo = DataTableConverter.Convert(this.cboOutPort.ItemsSource);

            string selectedPortID = this.cboOutPort.SelectedValue.ToString();
            string selectedEqptID = ((DataRowView)cboOutPort.SelectedItem).Row.ItemArray[2].ToString();

            this.dataHelper.SetLOTSendOutToManualPort(dt, selectedEqptID, selectedPortID);
        }

        private void DataGridCheckBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked((CheckBox)sender, true);
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked((CheckBox)sender, false);
        }

        private void dgLOTInfo_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (this.dgLOTInfo.ItemsSource == null)
                {
                    return;
                }

                DataTable dt = DataTableConverter.Convert(this.dgLOTInfo.ItemsSource);

                if (!CommonVerify.HasTableRow(dt))
                {
                    this.cboOutPort.IsEnabled = false;
                    this.cboOutPort.ItemsSource = null;
                    return;
                }

                var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).GroupBy(x => new
                {
                    STOCKER_ID = x.Field<string>("STOCKER_ID")
                }).Select(grp => new
                {
                    STOCKER_ID = grp.Key.STOCKER_ID
                });

                if (query.Count() <= 0 || query.Count() > 1)
                {
                    this.cboOutPort.IsEnabled = false;
                    this.cboOutPort.ItemsSource = null;
                    return;
                }

                // 선택한 Stocker에 연관되어 있는 Manual Port 가져오기
                foreach (var item in query)
                {
                    DataTable dtManualPortInfo = this.dataHelper.GetManualPortInfo_MCS(query); //이전 버전 : GetManualPortInfo 사용
                    if (CommonVerify.HasTableRow(dtManualPortInfo))
                    {
                        this.cboOutPort.IsEnabled = true;
                        PackCommon.SetC1ComboBox(dtManualPortInfo, this.cboOutPort, "-SELECT-");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SearchExcelLoad();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.SearchClipboardData();
                e.Handled = true;
            }
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            this.dgLOTInfo.Columns["WIP_HOLD"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["WIP_HOLD_NOTE"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["WIP_HOLD_USERID"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["WIP_HOLD_DTTM"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["QMS_HOLD"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["QMS_HOLD_USERID"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["QMS_HOLD_DTTM"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["WIPSNAME"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["PROD_ELTR_LINE_LIST_NAME"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["PROD_ASSY_LINE_LIST_NAME"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["CUST_LOTID"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["LOTDTTM_CR"].Visibility = Visibility.Visible;
            this.dgLOTInfo.Columns["SMPL_HOLD"].Visibility = (LoginInfo.CFG_AREA_ID == "PA") ? Visibility.Visible : this.dgLOTInfo.Columns["SMPL_HOLD"].Visibility;
            this.dgLOTInfo.Columns["SMPL_DTTM"].Visibility = (LoginInfo.CFG_AREA_ID == "PA") ? Visibility.Visible : this.dgLOTInfo.Columns["SMPL_DTTM"].Visibility;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            this.dgLOTInfo.Columns["WIP_HOLD"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["WIP_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["WIP_HOLD_USERID"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["WIP_HOLD_DTTM"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["QMS_HOLD"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["QMS_HOLD_USERID"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["QMS_HOLD_DTTM"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["WIPSNAME"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["PROD_ELTR_LINE_LIST_NAME"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["PROD_ASSY_LINE_LIST_NAME"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["CUST_LOTID"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["LOTDTTM_CR"].Visibility = Visibility.Collapsed;
            this.dgLOTInfo.Columns["SMPL_HOLD"].Visibility = (LoginInfo.CFG_AREA_ID == "PA") ? Visibility.Collapsed : this.dgLOTInfo.Columns["SMPL_HOLD"].Visibility;
            this.dgLOTInfo.Columns["SMPL_DTTM"].Visibility = (LoginInfo.CFG_AREA_ID == "PA") ? Visibility.Collapsed : this.dgLOTInfo.Columns["SMPL_DTTM"].Visibility;
        }

        private void btnManualCarrierOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK003_024_ABNORMALOUTPUT_POPUP popup = new PACK003_024_ABNORMALOUTPUT_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }

    public class StockerDetailDataHelper
    {
        #region Member Variable Lists...
        private DataTable dtAssyLineInfo = new DataTable();
        private DataTable dtElecLineInfo = new DataTable();
        #endregion

        #region Constructor
        public StockerDetailDataHelper()
        {
            this.GetBasicInfo(ref this.dtAssyLineInfo, Area_Type.ASSY);     // 조립동 설비정보
            this.GetBasicInfo(ref this.dtElecLineInfo, Area_Type.ELEC);     // 전극동 설비정보
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode
        private DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 조립동 및 전극동 Shop, Line, Equipment
        private void GetBasicInfo(ref DataTable dtReturn, string areaTypeCode)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_BASICINFO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = null;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["AREA_TYPE_CODE"] = areaTypeCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - DB Time 가져오기
        private DateTime GetSystemTime()
        {
            string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";
            DateTime dteReturn = DateTime.Now;

            try
            {
                string inDataTableNameList = string.Empty;
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    foreach (DataTable dt in dsOUTDATA.Tables.OfType<DataTable>().Where(x => x.TableName.Equals(outDataSetName)))
                    {
                        if (CommonVerify.HasTableRow(dt))
                        {
                            foreach (DataRowView drv in dt.AsDataView())
                            {
                                dteReturn = Convert.ToDateTime(drv["SYSTIME"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dteReturn;
            }

            return dteReturn;
        }

        // 순서도 호출 - MCS BizActor 접속정보 가져오기
        private DataTable GetMCSBizActorServerInfo()
        {
            String bizRuleName = "DA_MCS_SEL_MCS_CONFIG_INFO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            dtRQSTDT.Columns.Add("KEYGROUPID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["KEYGROUPID"] = "FP_MCS_AP_LOGIS_CONFIG";
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            if (!CommonVerify.HasTableRow(dtRSLTDT))
            {
                return null;
            }

            var query = dtRSLTDT.AsEnumerable().GroupBy(x => x.Field<string>("KEYGROUPID")).Select(grp => new
            {
                BIZACTORIP = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORIP")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPORT = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPORT")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPROTOCOL = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPROTOCOL")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEINDEX = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEINDEX")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEMODE = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEMODE")) ? x.Field<string>("KEYVALUE") : string.Empty)
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 순서도 호출 - Stocker 정보 조회
        public DataTable GetStockerInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_STOCKER_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQGRID", typeof(string));
                dtRQSTDT.Columns.Add("EQPTLEVEL", typeof(string));
                dtRQSTDT.Columns.Add("STOCKERTYPE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQGRID"] = null;
                drRQSTDT["EQPTLEVEL"] = "M";
                drRQSTDT["STOCKERTYPE"] = "OCV1_WAIT_STK,OCV_NG_STK,OCV2_WAIT_STK,OCV_WAIT_STK";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtReturn;
            }

            return dtReturn;
        }

        // 순서도 호출 - Pack Line
        public DataTable GetPackLineInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));  // 반송여부(물류타는라인)
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));         // MEB 라인 여부
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));         // 자동 포장 라인 여부

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = "Y";
                drRQSTDT["PACK_BOX_LINE_FLAG"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        EQSGID = x.Field<string>("CBO_CODE"),
                        EQSGNAME = x.Field<string>("CBO_NAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 제품
        public DataTable GetProductInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_ROUT_MODULE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PRODID = x.Field<string>("PRODID"),
                        PRODNAME = x.Field<string>("PRODID") + " : " + x.Field<string>("PRODNAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Pack 공정 정보
        public DataTable GetPackProcessInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_PROCESS_PACK_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                drRQSTDT["PCSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PROCID = x.Field<string>("CBO_CODE"),
                        PROCNAME = x.Field<string>("CBO_NAME")
                    }).OrderBy(x => x.PROCID);

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Route
        public DataTable GetRouteInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_SHOP_PRDT_ROUTID_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        ROUTID = x.Field<string>("ROUTID"),
                        ROUTNAME = x.Field<string>("ROUTID") + " : " + x.Field<string>("ROUTNAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Stocker 상세 현황 조회
        public DataTable GetStockerDetailList(string stockerID, string processID, string equipmentSegmentID, string productID, string elecEquipmentID, string assyEquipmentID, string inspectionTargetFlag, string LOTID, string requestWipTypeCode, string routID)
        {
            string bizRuleName = "BR_PRD_SEL_LOGIS_STOCK_DETAIL";
            //string bizRuleName = "BR_PRD_SEL_LOGIS_STOCK_DETAIL_COPY";
            DataTable dtINDATA = new DataTable("INDATA");
            DataTable dtOUTDATA = new DataTable("OUTDATA");

            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("AREAID", typeof(string));
            dtINDATA.Columns.Add("EQPTID", typeof(string));
            dtINDATA.Columns.Add("PRODID_LIST", typeof(string));
            dtINDATA.Columns.Add("PROD_PACK_LINE_LIST", typeof(string));
            dtINDATA.Columns.Add("PROCID_LIST", typeof(string));
            dtINDATA.Columns.Add("PROD_ELTR_LINE_LIST", typeof(string));
            dtINDATA.Columns.Add("PROD_ASSY_LINE_LIST", typeof(string));
            dtINDATA.Columns.Add("LOTID_LIST", typeof(string));
            dtINDATA.Columns.Add("INSPECTION_FLAG", typeof(string));
            dtINDATA.Columns.Add("REQ_WIP_TYPE_CODE", typeof(string));
            dtINDATA.Columns.Add("ROUTID_LIST", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
            drINDATA["EQPTID"] = string.IsNullOrEmpty(stockerID) ? null : stockerID;
            drINDATA["PRODID_LIST"] = string.IsNullOrEmpty(productID) ? null : productID;
            drINDATA["PROD_PACK_LINE_LIST"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
            drINDATA["PROCID_LIST"] = string.IsNullOrEmpty(processID) ? null : processID;
            drINDATA["PROD_ELTR_LINE_LIST"] = string.IsNullOrEmpty(elecEquipmentID) ? null : elecEquipmentID;
            drINDATA["PROD_ASSY_LINE_LIST"] = string.IsNullOrEmpty(assyEquipmentID) ? null : assyEquipmentID;
            drINDATA["LOTID_LIST"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
            drINDATA["INSPECTION_FLAG"] = string.IsNullOrEmpty(inspectionTargetFlag) ? null : inspectionTargetFlag;
            drINDATA["REQ_WIP_TYPE_CODE"] = string.IsNullOrEmpty(requestWipTypeCode) ? null : requestWipTypeCode;
            drINDATA["ROUTID_LIST"] = string.IsNullOrEmpty(routID) ? null : routID;
            dtINDATA.Rows.Add(drINDATA);

            dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);
            return dtOUTDATA;
        }

        // 순서도 호출 - 수동포트정보 가져오기 (MCS 비즈호출버전 - MEB7단창고에서 STK - STK 수동명령 개선 요구사항, STK정보 + Manual Port정보 표출)
        public DataTable GetManualPortInfo_MCS(IEnumerable<dynamic> lstStockerID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "BR_GUI_REG_DST_LOC_INFO_BY_MANUAL";
                DataTable dtMCSBizActorServerInfo = this.GetMCSBizActorServerInfo();
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("SRC_EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                foreach (var item in lstStockerID)
                {
                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["SRC_EQPTID"] = item.STOCKER_ID;
                    drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    dtRQSTDT.Rows.Add(drRQSTDT);
                }

                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());

                    dtRSLTDT = clientProxy.ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                }
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PORT_ID = x.Field<string>("PORT_ID"),
                        PORT_NAME = x.Field<string>("PORT_NAME"),
                        EQPTID = x.Field<string>("EQPTID")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 수동포트출고예약
        public bool SetLOTSendOutToManualPort(DataTable dt, string destEquipmentID, string destPortID)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_TRF_JOB_BY_MES_MANUAL";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUT_DATA";
            DateTime dateTime = this.GetSystemTime();
            DataTable dtMCSBizActorServerInfo = this.GetMCSBizActorServerInfo();

            DataTable dtIN_DATA = new DataTable("IN_DATA");
            dtIN_DATA.Columns.Add("SRC_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("SRC_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("DST_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("DST_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIERID", typeof(string));
            dtIN_DATA.Columns.Add("USER", typeof(string));
            dtIN_DATA.Columns.Add("DTTM", typeof(DateTime));
            dtIN_DATA.Columns.Add("PRODID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIER_STRUCT", typeof(string));
            dtIN_DATA.Columns.Add("MDL_TP", typeof(string));
            dtIN_DATA.Columns.Add("STK_ISS_TYPE", typeof(string));

            foreach (DataRowView drv in dt.AsDataView())
            {
                DataRow drIN_DATA = dtIN_DATA.NewRow();
                drIN_DATA["CARRIERID"] = drv["LOTID"].ToString();
                drIN_DATA["SRC_EQPTID"] = drv["STOCKER_ID"].ToString();
                drIN_DATA["SRC_LOCID"] = drv["RACK_ID"].ToString();
                drIN_DATA["DST_EQPTID"] = destEquipmentID;
                drIN_DATA["DST_LOCID"] = destPortID;
                drIN_DATA["USER"] = LoginInfo.USERID;
                drIN_DATA["DTTM"] = dateTime;
                drIN_DATA["PRODID"] = drv["PRODID"].ToString();
                drIN_DATA["CARRIER_STRUCT"] = null;
                drIN_DATA["MDL_TP"] = null;
                drIN_DATA["STK_ISS_TYPE"] = null;
                dtIN_DATA.Rows.Add(drIN_DATA);
            }
            dsINDATA.Tables.Add(dtIN_DATA);

            string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
            try
            {
                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());

                    dsOUTDATA = clientProxy.ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                    if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                    {
                        Util.MessageInfo("SFU8111"); // 이동명령이 예약되었습니다
                        returnValue = true;
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 조립설비
        public DataTable GetAssyEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 전극설비
        public DataTable GetElecEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                return null;
            }

            var query = this.dtElecLineInfo.AsEnumerable().GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                CHK = false,
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // Use Flag
        public DataTable GetUseFlagInfo()
        {
            DataTable dt = this.GetCommonCodeInfo("USE_FLAG");

            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }

            var query = dt.AsEnumerable().Select(x => new
            {
                CHK = false,
                CBO_CODE = x.Field<string>("CBO_CODE"),
                CBO_NAME = x.Field<string>("CBO_CODE") + " : " + x.Field<string>("CBO_CODE")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 반송재공유형코드
        public DataTable GetRequestWipTypeCodeInfo()
        {
            DataTable dt = this.GetCommonCodeInfo("REQ_WIP_TYPE_CODE");

            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }

            var query = dt.AsEnumerable().Select(x => new
            {
                CHK = false,
                REQ_WIP_TYPE_CODE = x.Field<string>("CBO_CODE"),
                REQ_WIP_TYPE_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }
        #endregion
    }
}