/*************************************************************************************
 Created Date : 2019.06.12
      Creator : 신광희
   Decription : 창고간 수동반송 예약
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.12  신광희 차장 : Initial Created.    
  2020.04.03  정문교      : CSR[C20200403-000010] > BR_GUI_REG_TRF_JOB_BYUSER INPUT 파라미터명 USER -> UPDUSER로 변경 
  2021.02.02  신광희      : ESNB 증설 자동차 2동 전극(EF), 조립(AA) 수동출고예약, 출고예약취소, 데이터출고 수정
  2022.06.01  오화백      : 폴란드 3공장 (전극 : EJ, 조립 : AC)  미국 GM (전극 : EI, 조립 : AB)  ==> NB 소스 사용하도록 IF문 조건 추가
  2022.06.06  오화백      : 동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
  2023.02.06  김도형      : CSR[C20221219-000623] 출고대상정보목록(dgIssueTargetInfo) FastTrack 컬럼 추가
  2024.03.13  김도형      : [E20240215-001200] [ESWA PI] Timer Column For Material After Coater Addition
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_028.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_028 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private string _projectName;
        private string _productVersion;
        private string _halfSlitterSideCode;
        private string _productCode;
        private string _holdCode;
        private string _pastDay;
        private string _lotId;
        private string _faultyType;
        private string _skidType;
        private string _emptybobbinState;
        private string _equipmentCode;
        private string _electrodeTypeCode;
        private string _selectedWipHold;
        private string _selectedQmsHold;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        private DataTable _requestTransferInfoTable;
        private string _selectedRadioButtonValue;
        private bool _isGradeJudgmentDisplay;

        //private const string _bizIp = "10.32.169.224";
        //private const string _bizPort = "7865";
        //private const string _bizIndex = "0";
        //private string[] _bizInfo = { _bizIp, _bizPort, _bizIndex };
        private string _Set_Logis_Type;
        private string _Skid_Use_Chk;
        public MCS001_028()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
            InitializeRequestTransferTable();
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitializeGrid();
            InitializeCombo();
            Set_Logis_Sys_Type();
            Chk_Skid_Use();
            rdoCondition.IsChecked = true;
            //무지부/권취방향
            if (_Set_Logis_Type == "RTD")
            {
                dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Header = ObjectDic.Instance.GetObjectName("무지부_");
                dgStoreByEmptyCarrier.Columns["CSTPROD"].Header = ObjectDic.Instance.GetObjectName("CST유형");
                dgStoreByEmptyCarrier.Columns["BBN_QTY"].Header = ObjectDic.Instance.GetObjectName("CST수");

            }
            else
            {
                dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Header = ObjectDic.Instance.GetObjectName("무지부");
                dgStoreByEmptyCarrier.Columns["CSTPROD"].Header = ObjectDic.Instance.GetObjectName("SKID Type");
                dgStoreByEmptyCarrier.Columns["BBN_QTY"].Header = ObjectDic.Instance.GetObjectName("SKID 수");
            }


            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void rdoRelease_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            spCondition.Visibility = Visibility.Collapsed;
            dgStore.Visibility = Visibility.Collapsed;
            dgStoreByEmptyCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Collapsed;


            switch (radioButton.Name)
            {
                case "rdoCondition":
                    spCondition.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NORMAL";
                    
                    break;
                case "rdoEmptyCarrier":
                    dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Visible;
                    dgStoreByEmptyCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "EMPTYCARRIER";
                    break;
            }

            ClearControl();
            SetStockerTypeCombo(cboStockerType);
            SetStockerCombo(cboStocker);

            if (string.Equals(_selectedRadioButtonValue, "NORMAL"))
            {
                if (cboStockerType.SelectedValue.GetString() == "JRW")
                {
                    dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                    dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();

            C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;
            SetDataGridCheckHeaderInitialize(dg);

            if (rdoEmptyCarrier.IsChecked == true)
            {
                SelectWareHouseEmptyCarrier();
            }
            else
            {
                SelectManualOutInventory();
            }
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
            if (_Set_Logis_Type == "RTD")
                SaveManualIssueByEsnb();
            else 
                SaveManualIssue();
        }

        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("RequestTransferId", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));

            //C1DataGrid dg = dgIssueTargetInfo;
            C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["RequestTransferId"] = DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString();
                    newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                    inTable.Rows.Add(newRow);
                }
            }

            CMM_MCS_TRANSFER_CANCEL popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = inTable;
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MCS_TRANSFER_CANCEL popup = sender as CMM_MCS_TRANSFER_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;
                SetDataGridCheckHeaderInitialize(dg);

                if (string.Equals(_selectedRadioButtonValue, "NORMAL"))
                {
                    SelectManualOutInventoryList();
                }
                else
                {
                    SelectWareHouseEmptyCarrierList();
                }
            }
        }


        private void dgStore_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string[] columnStrings = new[] {"LOT_TOTAL_QTY", "LOT_QTY", "LOT_HOLD_QTY", "LOT_HOLD_QMS_QTY"};

                    if (columnStrings.Contains(e.Cell.Column.Name))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRJT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgStore_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgStore_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgStore == null || dgStore.CurrentCell == null || dgStore.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgStore.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dgStore.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;


                _halfSlitterSideCode = null;
                _productVersion = null;
                _productCode = null;
                _holdCode = null;
                _pastDay = null;
                _lotId = null;
                _faultyType = null;
                _selectedWipHold = null;
                _selectedQmsHold = null;
                _equipmentCode = null;
                _electrodeTypeCode = null;
                _projectName = string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "PRJT_NAME").GetString()) ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();

                if (!string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "PRJT_NAME").GetString()))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    _halfSlitterSideCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "HALF_SLIT_SIDE").GetString()) ? null : DataTableConverter.GetValue(drv, "HALF_SLIT_SIDE").GetString();
                }
                else
                {
                    _equipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()) ? null : cboStocker.SelectedValue.GetString();
                    _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();
                }

                if (cell.Column.Name.Equals("LOT_QTY"))
                {
                    _selectedWipHold = "N";
                    _selectedQmsHold = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY"))
                {
                    _selectedWipHold = "Y";
                    _selectedQmsHold = null;
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY"))
                {
                    _selectedQmsHold = "Y";
                    _selectedWipHold = "N";
                }

                _productVersion = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "PROD_VER_CODE").GetString()) ? null : DataTableConverter.GetValue(drv, "PROD_VER_CODE").GetString();
                _productCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "PRODID").GetString()) ? null : DataTableConverter.GetValue(drv, "PRODID").GetString();
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SetDataGridCheckHeaderInitialize(dgIssueTargetInfo);
                SelectManualOutInventoryList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgStoreByEmptyCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "BBN_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "BBN_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgStoreByEmptyCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgStoreByEmptyCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _skidType = string.Empty;
                _emptybobbinState = string.Empty;
                _electrodeTypeCode = string.Empty;

                if (DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                {
                    _electrodeTypeCode = null;
                    _skidType = null;
                    _emptybobbinState = null;
                }
                else
                {
                    if (cell.Column.Name.Equals("ELTR_TYPE_NAME"))
                    {
                        _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        _skidType = null;
                        _emptybobbinState = null;
                    }
                    else if (cell.Column.Name.Equals("CSTPROD"))
                    {
                        _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                        _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }
                    else
                    {
                        _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                        _emptybobbinState = DataTableConverter.GetValue(drv, "EMPTY_BBN_YN").GetString();
                        _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }
                }

                SelectWareHouseEmptyCarrierList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;
            dgIssueTargetInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QA_INSP_JUDG_VALUE_NAME"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QA_INSP_JUDG_VALUE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["A_COATING_NAME"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["E_COATING_NAME"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
            
            if (_Set_Logis_Type == "RTD" && _Skid_Use_Chk == "Y")
            {
                dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Visible;

            }

            if (cboStockerType.SelectedValue.GetString() == "JRW")
            {
                dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["SKID_ID"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["E_COATING_NAME"].Visibility = Visibility.Visible;
            }
            else if (cboStockerType.SelectedValue.GetString() == "PCW")
            {
                dgIssueTargetInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["E_COATING_NAME"].Visibility = Visibility.Visible;

                if (_Set_Logis_Type == "RTD" && _Skid_Use_Chk == "Y")
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Visible;

                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgIssueTargetInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;

                if (_Set_Logis_Type == "RTD" && _Skid_Use_Chk == "Y")
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Visible;

                }
            }

            if (cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgIssueTargetInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["QA_INSP_JUDG_VALUE_NAME"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["QA_INSP_JUDG_VALUE"].Visibility = Visibility.Visible;
                dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;

                if (_Set_Logis_Type == "RTD" && _Skid_Use_Chk == "Y")
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Visible;

                }
            }

            if (cboStockerType.SelectedValue.GetString() == "NWW" || cboStockerType.SelectedValue.GetString() == "MNW" || cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgIssueTargetInfo.Columns["A_COATING_NAME"].Visibility = Visibility.Visible;
            }

            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(_projectName))
                {
                    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                    SelectManualOutInventoryList();
                }
            }
        }

        private void cboHoldReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectManualOutInventoryList();
            }
        }

        private void txtPastDay_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(_projectName))
                {
                    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                    SelectManualOutInventoryList();
                }
            }
        }

        private void cboFaultyType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectManualOutInventoryList();
            }
        }

        private void chkIssueTargetInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            if (cb.IsChecked != null)
            {
                int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

                var query = (from t in ((DataView)dgIssueTargetInfo.ItemsSource).Table.AsEnumerable()
                    where t.Field<int>("CHK") == 1
                    select t).ToList();

                DataRowView drv = cb.DataContext as DataRowView;
                if (drv != null)
                {
                    string equipmentCode = drv["EQPTID"].GetString();
                    string carrier = drv["SKID_ID"].GetString();
                    SetIssuePort(cboIssuePort, equipmentCode, carrier);
                
                }
            }
        }

        private void chkIssueTargetInfo_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            if (cb.IsChecked != null)
            {
                var query = (from t in ((DataView)dgIssueTargetInfo.ItemsSource).Table.AsEnumerable()
                    where t.Field<int>("CHK") == 1
                    select t).ToList();

                //DataTable dt = ((DataView) dgIssueTargetInfo.ItemsSource).Table;
                //string selectedEquipmentCode = string.Join(",", dt.Rows.OfType<DataRow>().Where(s => s["CHK"].ToString().Equals("1")).Select(k => k["EQPTID"].ToString()).ToArray());
                //MessageBox.Show(selectedEquipmentCode);

                if (!query.Any())
                {
                    if (cboIssuePort.ItemsSource != null && cboIssuePort.Items.Count > 0)
                        SetIssuePort(cboIssuePort, string.Empty);
                }
            }
        }

        private void dgIssueTargetInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CWA_JRW_QMS").GetString() == "N/A")
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#8C8C8C");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                        else
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CWA_PCW_QMS").GetString() == "N/A")
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#8C8C8C");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                        else
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                            if (convertFromString != null)
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }));
        }

        private void dgIssueTargetInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgIssueTargetInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfo;

                if (e.Column.Name == "CHK")
                {
                    /*
                    if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        if (DataTableConverter.GetValue(e.Row.DataItem, "CWA_JRW_QMS").GetString() == "OK")
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            e.Cancel = true;
                            return; 
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        if (DataTableConverter.GetValue(e.Row.DataItem, "CWA_PCW_QMS").GetString() == "OK")
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    */

                    if (DataTableConverter.GetValue(e.Row.DataItem, "ABNORM_TRF_RSN_CODE").GetString() != "N")
                    {
                        //정보불일치 스키드 입니다.
                        Util.MessageValidation("SFU8206");
                        e.Cancel = true;
                    }
                    else
                        e.Cancel = false;
                }

                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgIssueTargetInfo;
            int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPTID").GetString();

                    if (DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString() == selectedequipmentCode)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                    }
                    else
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }

                    if (DataTableConverter.GetValue(row.DataItem, "ABNORM_TRF_RSN_CODE").GetString() != "N")
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }
                }
                else
                {
                    if (DataTableConverter.GetValue(row.DataItem, "ABNORM_TRF_RSN_CODE").GetString() != "N")
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }
                    else
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                    }
                }
            }
            dg.EndEdit();
            dg.EndEditRow(true);


            int lastIdx = _util.GetDataGridLastRowIndexByCheck(dg, "CHK");
            if (lastIdx > -1)
            {
                SetIssuePort(cboIssuePort, DataTableConverter.GetValue(dg.Rows[lastIdx].DataItem, "EQPTID").GetString(), DataTableConverter.GetValue(dg.Rows[lastIdx].DataItem, "SKID_ID").GetString());
            }

        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgIssueTargetInfo;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 0);
            }

            dg.EndEdit();
            dg.EndEditRow(true);

            if (cboIssuePort.ItemsSource != null && cboIssuePort.Items.Count > 0)
                SetIssuePort(cboIssuePort, string.Empty);
        }

        private void dgIssueTargetInfoByEmptyCarrier_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfoByEmptyCarrier;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgIssueTargetInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("LOTID"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Method

        private void SelectManualOutInventory()
        {
            const string bizRuleName = "BR_MCS_GET_MANUAL_OUT_INVENTORY";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["FIFO"] = "NORMAL";
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                    {
                        ProjectName = ObjectDic.Instance.GetObjectName("합계"),
                        ProductVersionCode = string.Empty,
                        ElectrodeTypeCode = string.Empty,
                        ElectrodeTypeName = string.Empty,
                        EquipmentCode = string.Empty,
                        EquipmentName = string.Empty,
                        ProductCode = string.Empty,
                        LotTotalCount = g.Sum(x => x.Field<decimal>("LOT_TOTAL_QTY")),
                        LotCount = g.Sum(x => x.Field<decimal>("LOT_QTY")),
                        LotHoldCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QTY")),
                        LotHoldQMSCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QMS_QTY")),
                        HalfSlittingSideCode = string.Empty,
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["PRJT_NAME"] = query.ProjectName;
                        newRow["PROD_VER_CODE"] = query.ProductVersionCode;
                        newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                        newRow["ELTR_TYPE_NAME"] = query.ElectrodeTypeName;
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPTNAME"] = query.EquipmentName;
                        newRow["PRODID"] = query.ProductCode;
                        newRow["LOT_TOTAL_QTY"] = query.LotTotalCount;
                        newRow["LOT_QTY"] = query.LotCount;
                        newRow["LOT_HOLD_QTY"] = query.LotHoldCount;
                        newRow["LOT_HOLD_QMS_QTY"] = query.LotHoldQMSCount;
                        newRow["HALF_SLIT_SIDE"] = query.HalfSlittingSideCode;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStore, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_CARRIER";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        ElectrodeTypeCode = string.Empty,
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        CarrierProductCode = string.Empty,
                        BobinCount = g.Sum(x => x.Field<Int32>("BBN_QTY")),
                        EmptyBobinYn = string.Empty,
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                        newRow["ELTR_TYPE_NAME"] = query.ElectrodeTypeName;
                        newRow["CSTPROD"] = query.CarrierProductCode;
                        newRow["BBN_QTY"] = query.BobinCount;
                        newRow["EMPTY_BBN_YN"] = query.EmptyBobinYn;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByEmptyCarrier, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectManualOutInventoryList()
        {
            SelectRequestTransferList();

            const string bizRuleName = "DA_MCS_SEL_MANUAL_OUT_INVENTORY_LIST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("PAST_DAY", typeof(string));
                inTable.Columns.Add("QA_INSP_JUDG_VALUE", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMSHOLD", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["EQPTID"] = _equipmentCode;
                dr["FIFO"] = "NORMAL";
                dr["PRJT_NAME"] = _projectName;
                dr["PROD_VER_CODE"] = _productVersion;
                dr["PRODID"] = _productCode;
                dr["HALF_SLIT_SIDE"] = _halfSlitterSideCode != "A" ? _halfSlitterSideCode : null;
                dr["LOTID"] = _lotId;
                dr["HOLD_CODE"] = _holdCode;
                dr["PAST_DAY"] = _pastDay;
                dr["QA_INSP_JUDG_VALUE"] = _faultyType;
                dr["WIPHOLD"] = _selectedWipHold;
                dr["QMSHOLD"] = _selectedQmsHold;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                    //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                    //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
                    if (_Set_Logis_Type == "RTD")
                    {
                        Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
                        SetIssuePort(cboIssuePort, string.Empty);
                        dgIssueTargetInfo.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                        dgIssueTargetInfo.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                    {
                        bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                        bizResult.Columns.Add("CARRIERID", typeof(string));
                        bizResult.Columns.Add("REQ_TRFID", typeof(string));
                    }
                    
                    if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    {
                        foreach (DataRow row in bizResult.Rows)
                        {
                            //창고유형 = “JRW” 이면 BOBBIN_ID (MES) = CARRIERID (MCS)
                            if (cboStockerType?.SelectedValue.GetString() == "JWR")
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["BOBBIN_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                            else //창고유형 != “JRW” 이면SKID_ID (MES) = CARRIERID (MCS) 
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["SKID_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                        }
                        bizResult.AcceptChanges();
                    }


                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
                    SetIssuePort(cboIssuePort, string.Empty);
                    dgIssueTargetInfo.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SelectWareHouseEmptyCarrierList()
        {
            SelectRequestTransferList();
            //Util.gridClear(dgPortInfo);
            string carrierState;

            if (!string.IsNullOrEmpty(_emptybobbinState))
            {
                if (_Skid_Use_Chk == "Y")
                {
                    if (_emptybobbinState == "Y")
                        carrierState = "E";
                    else
                        carrierState = "U";
                }
                else
                {
                    if (_emptybobbinState == "Y")
                        carrierState = "U";
                    else
                        carrierState = "E";
                }
            }
            else
            {
                carrierState = null;
            }


            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["EQPTID"] = _selectedRadioButtonValue.Equals("EMPTYCARRIER") ? cboStocker.SelectedValue : _equipmentCode;
                dr["CSTPROD"] = _skidType;
                dr["CSTSTAT"] = carrierState; //string.Equals(_emptybobbinState, "Y") ? "U" : "E";
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                    //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                    //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
                    if (_Set_Logis_Type == "RTD")
                    {
                        Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);
                        SetIssuePort(cboIssuePort, string.Empty);
                        dgIssueTargetInfoByEmptyCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                        dgIssueTargetInfoByEmptyCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                    {
                        bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                        bizResult.Columns.Add("CARRIERID", typeof(string));
                        bizResult.Columns.Add("REQ_TRFID", typeof(string));
                    }

                    if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    {
                        foreach (DataRow row in bizResult.Rows)
                        {
                            //창고유형 = “JRW” 이면 BOBBIN_ID (MES) = CARRIERID (MCS)
                            if (cboStockerType?.SelectedValue.GetString() == "JWR")
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["BOBBIN_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                            else //창고유형 != “JRW” 이면SKID_ID (MES) = CARRIERID (MCS) 
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["SKID_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                        }
                        bizResult.AcceptChanges();
                    }

                    Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);
                    SetIssuePort(cboIssuePort, string.Empty);
                    dgIssueTargetInfoByEmptyCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfoByEmptyCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectRequestTransferList()
        {
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC") return;
            //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
            if (_Set_Logis_Type == "RTD") return;
            const string bizRuleName = "DA_SEL_MCS_REQ_TRF_MES_GUI";
            DataTable inDataTable = new DataTable("INDATA");
            _requestTransferInfoTable = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, null, "RSLTDT", inDataTable);
        }

        private void SaveManualIssue()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));

                //C1DataGrid dg = dgIssueTargetInfo;
                C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "SKID_ID").GetString();
                        newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        newRow["DST_LOCID"] = cboIssuePort.SelectedValue;
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["DTTM"] = dtSystem;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", "OUT_REQ_TRF_INFO", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        SetDataGridCheckHeaderInitialize(dg);
                        if (string.Equals(_selectedRadioButtonValue, "NORMAL"))
                        {
                            SelectManualOutInventoryList();
                        }
                        else
                        {
                            SelectWareHouseEmptyCarrierList();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;
                DataTable dt = DataTableConverter.Convert(cboIssuePort.ItemsSource);
                DataRow dr = dt.Rows[cboIssuePort.SelectedIndex];

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "SKID_ID").GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        newRow["DST_EQPTID"] = dr["DST_EQPTID"].GetString();
                        newRow["DST_LOCID"] = dr["DST_LOC_GROUPID"].GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;

                        inTable.Rows.Add(newRow);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        SetDataGridCheckHeaderInitialize(dg);
                        if (string.Equals(_selectedRadioButtonValue, "NORMAL"))
                        {
                            SelectManualOutInventoryList();
                        }
                        else
                        {
                            SelectWareHouseEmptyCarrierList();
                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCheckedIssueTargetInfoGrid()
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgIssueTargetInfo))
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                            dgIssueTargetInfo.EndEdit();
                            dgIssueTargetInfo.EndEditRow(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetBizActorServerInfo()
        {
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC") return;
            //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
            if (_Set_Logis_Type == "RTD") return;
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

        private void InitializeRequestTransferTable()
        {
            _requestTransferInfoTable = new DataTable();
            _requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRF_STAT", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRFID", typeof(string));
            _requestTransferInfoTable.Columns.Add("SRC_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("DST_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("JOBID", typeof(string));
        }

        private void InitializeGrid()
        {
            dgStore.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStore.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

            if (_isGradeJudgmentDisplay) dgIssueTargetInfo.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;

            //20230206 김도형 : FastTrack 적용여부 체크
            if (ChkFastTrackOWNER())
            {
                dgIssueTargetInfo.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;             
            }

            // [E20240215-001200] [ESWA PI] Timer Column For Material After Coater Addition
            if (IsCountDownTimerUse())
            {
                dgIssueTargetInfo.Columns["COUNTDOWN_TIME"].Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            } 
            return bRet;
        }

        // [E20240215-001200] [ESWA PI] Timer Column For Material After Coater Addition
        private bool IsCountDownTimerUse()
        {
            bool bRet = false; 
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_PROC_AGING_TIME";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);
                
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bRet = true;   
                }
                else
                {
                    bRet = false;
                }                 
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                bRet = false;
                return bRet;
            }
            return bRet;
        }

        private void InitializeCombo()
        {
            // 창고 유형 콤보박스
            SetStockerTypeCombo(cboStockerType);

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // HOLD 사유 콤보박스
            SetHoldCombo(cboHoldReason);

            // QA불량유형
            SetFalutyTypeCombo(cboFaultyType);
        }

        private void ClearControl()
        {
            _projectName = string.Empty;
            _productVersion = string.Empty;
            _halfSlitterSideCode = string.Empty;
            _productCode = string.Empty;
            _holdCode = string.Empty;
            _pastDay = string.Empty;
            _lotId = string.Empty;
            _faultyType = string.Empty;

            _skidType = string.Empty;
            _emptybobbinState = string.Empty;
            _equipmentCode = string.Empty;
            _electrodeTypeCode = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;

            Util.gridClear(dgStore);
            Util.gridClear(dgIssueTargetInfo);
            Util.gridClear(dgStoreByEmptyCarrier);
            Util.gridClear(dgIssueTargetInfoByEmptyCarrier);

            _requestTransferInfoTable.Clear();

            if (cboIssuePort.ItemsSource != null && cboIssuePort.Items.Count > 0)
                SetIssuePort(cboIssuePort, string.Empty);
        }

        private bool ValidationManualIssue()
        {
            //C1DataGrid dg = dgIssueTargetInfo;
            C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboIssuePort.SelectedValue == null || string.IsNullOrEmpty(cboIssuePort.SelectedValue.GetString()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            // CNB2동의 경우 아래의 Valudation 체크로직 Pass
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
            if (_Set_Logis_Type == "RTD")
                return true;


            if (!ValidationJobCreate())
                return false;

            return true;
        }

        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = dgIssueTargetInfo;
            C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NORMAL") ? dgIssueTargetInfo : dgIssueTargetInfoByEmptyCarrier;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            // CNB 2동의 경우 반송요청 관련 BizRule에서 처리 하므로 아래의 조건은 Pass
            //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
            //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
            //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
            if (_Set_Logis_Type == "RTD")
            {
                if (!ValidationTransferCancelByEsnb(dg))
                {
                    return false;
                }

                return true;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString()) || DataTableConverter.GetValue(row.DataItem, "REQ_TRF_STAT").GetString() != "REQUEST")
                    {
                        //Util.MessageValidation("반송요청상태를 확인해 주세요.");
                        Util.MessageInfo("SFU8116", ObjectDic.Instance.GetObjectName("반송요청상태"));
                        return false;
                    }
                }
            }

            return true;

        }

        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_CHK_SEL_TRF_CMD_CANCEL_BY_UI", "IN_DATA", "OUT_DATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["RETVAL"].GetString() != "0")
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsGradeJudgmentDisplay()
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationJobCreate()
        {
            const string bizRuleName = "BR_CHK_CNV_AVAILABILITY_CREATE_JOB";

            try
            {
                DataTable inTable = new DataTable("IN_LOC");
                inTable.Columns.Add("LOCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOCID"] = cboIssuePort.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "IN_LOC", null, inTable);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private static void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "Y", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetHoldCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ACTIVITIREASON_CBO";
            string[] arrColumn = { "LANGID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, "HOLD_LOT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetFalutyTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "VD_RESN_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPTNAME" : "EQPT_NAME").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrierId = DataTableConverter.GetValue(e.Row.DataItem, "SKID_ID").GetString();

                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPTID").GetString();
                    if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EQPTID")).Equals(selectedequipmentCode))
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                var query = (from t in ((DataView)dg.ItemsSource).Table.AsEnumerable()
                             where t.Field<int>("CHK") == 1
                             select t).ToList();

                if (checkValue == "0")
                {
                    if (query.Count() < 1)
                    {
                        SetIssuePort(cboIssuePort, currentequipmentCode, carrierId);
                    }
                }
                else
                {
                    if (query.Count() <= 1)
                    {
                        if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                        }
                    }
                }
            }
        }

        private void SetIssuePort(C1ComboBox cbo, string equipmentCode, string carrierId = null)
        {

            if (cbo.Items.Count > 0)
            {
                cbo.ItemsSource = null;
                for (int i = 0; i < cbo.Items.Count; i++)
                {
                    cbo.Items.RemoveAt(i);
                    i--;
                }
                cbo.Items.Clear();
                cbo.SelectedValue = null;
            }


            if (string.IsNullOrEmpty(equipmentCode) && string.IsNullOrEmpty(carrierId)) return;

            try
            {
                DataTable inTable = new DataTable("IN_EQPT_INFO");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("IS_EMPTY", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = !string.IsNullOrEmpty(equipmentCode) ? equipmentCode : null;
                dr["CARRIERID"] = !string.IsNullOrEmpty(carrierId) ? carrierId : null;
                dr["IS_EMPTY"] = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? "Y" : "N";
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataTable dtResult;

                //2022 06 01 오화백  폴란드 3공장 (전극,조립), GM(전극, 조립) 추가
                //if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA" || LoginInfo.CFG_AREA_ID == "EI" || LoginInfo.CFG_AREA_ID == "AB" || LoginInfo.CFG_AREA_ID == "EJ" || LoginInfo.CFG_AREA_ID == "AC")
                //2022.06.06  오화백  동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
                if (_Set_Logis_Type == "RTD")
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_SEL_TARGET_STK_CBO", "IN_EQPT_INFO", "OUT_DEST_INFO", inTable);
                }
                else
                {
                    dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("BR_GUI_GET_TRF_DEST_BYDFFSTKC", "IN_EQPT_INFO", "OUT_DEST_INFO", inTable);
                }

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 2022.06.06  오화백 동 구분 하드 코딩 부분  동별 공통코드 사용 (LOGIS_SYST_TYPE_CODE)
        /// </summary>
        private void Set_Logis_Sys_Type()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "LOGIS_SYST_TYPE_CODE";
                dr["COM_CODE"] = "RTD";
                dr["USE_FLAG"] = "Y";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    _Set_Logis_Type = dtRslt.Rows[0]["COM_CODE"].ToString();
                }
                else
                {
                    _Set_Logis_Type = string.Empty;
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        /// <summary>
        /// 스키드 사용 체크
        /// </summary>

        private void Chk_Skid_Use()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));
            dt.Columns.Add("ATTRIBUTE1", typeof(string));


            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "MHS_UNCHECK_SKID_BOBBIN_MAPPING";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dr["ATTRIBUTE1"] = "Y";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                _Skid_Use_Chk = "Y";
            }
            else
            {
                _Skid_Use_Chk = "N";
            }

        }


        #endregion


    }
}
