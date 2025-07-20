/*************************************************************************************
 Created Date : 2021.09.14
      Creator : 이제섭
   Decription : 창고 수동출고 예약
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.14  이제섭 대리 : Initial Created. (MCS001_027 화면 Copy하여 ESNA 2동 전극 Pancake 창고용 화면 생성)
  2022.04.04  정재홍      : C20220309-000071 - GMES  Logistics Mgmt improve

**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_067.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_067 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private double _maxcheckCount = 0;
        private string _userId;
        private string _selectedRadioButtonValue = "FIFO";
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
        private string _cstTypeCode;
        private string _abNormalReasonCode;
        private string _selectedWipHold;
        private string _selectedQmsHold;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        private DataTable _requestTransferInfoTable;
        private bool _isGradeJudgmentDisplay;
        private bool _isAdminAuthority;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private string _FastTrackLot;
        private string _FastFlag;

        // CSR : C20220309-000071
        private bool _isFirskUnchk;

        public MCS001_067()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo();
            InitializeRequestTransferTable();
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            rdoFiFo.IsChecked = true;
            InitializeGrid();
            InitializeCombo();
            SelectReleaseCount();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
 
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();

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

            // SFU5073 Port 출고 하시겠습니까?
            Util.MessageConfirm("SFU5073", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveManualIssue();
                }
            });
        }

        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
            {
                dg = dgIssueTargetInfoByNoReadCarrier;
            }
            else
            {
                dg = dgIssueTargetInfo;
            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("RequestTransferId", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));

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
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                {
                    SelectWareHouseNoReadCarrier();
                    SelectWareHouseNoReadCarrierList();
                }
                else
                {
                    SelectManualOutInventory();
                    SelectManualOutInventoryList(true);
                }
            }
        }

        private void btnDataIssue_Click(object sender, RoutedEventArgs e)
        {

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
            {
                dg = dgIssueTargetInfoByNoReadCarrier;
            }
            else
            {
                dg = dgIssueTargetInfo;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }
            else
            {
                //출고 하시겠습니까?
                Util.MessageConfirm("SFU3121", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            SaveDataIssue();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

           
            }
        }

        private void popupSaveDataissue_Closed(object sender, EventArgs e)
        {
            MCS001_027_SAVE_DATAISSUE popup = sender as MCS001_027_SAVE_DATAISSUE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                {
                    SelectWareHouseNoReadCarrier();
                    SelectWareHouseNoReadCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    SelectWareHouseAbNormalCarrier();
                    SelectWareHouseAbNormalCarrierList();
                }
                else
                {
                    SelectManualOutInventory();
                    SelectManualOutInventoryList(true);
                }
            }
        }


        private void rdoRelease_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;


            spFiFo.Visibility = Visibility.Collapsed;
            spCondition.Visibility = Visibility.Collapsed;
            dgStore.Visibility = Visibility.Collapsed;
            dgStoreByEmptyCarrier.Visibility = Visibility.Collapsed;
            dgStoreByNoReadCarrier.Visibility = Visibility.Collapsed;
            dgStoreByAbNormalCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByNoReadCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByAbNormalCarrier.Visibility = Visibility.Collapsed;

            switch (radioButton.Name)
            {
                case "rdoFiFo":
                    spFiFo.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "FIFO";
                    // 실 SKID는 MES에서 수동출고 가능하기 때문에 기능 Visible 처리
                    grdPriority.Visibility = Visibility.Visible;
                    grdPort.Visibility = Visibility.Visible;
                    btnManualIssue.Visibility = Visibility.Visible;
                    break;
                case "rdoEmptyCarrier":
                    dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Visible;
                    dgStoreByEmptyCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "EMPTYCARRIER";
                    // 공 SKID는 WCS에서 입출고 처리함. MES에서는 기능 Collapsed 처리
                    grdPriority.Visibility = Visibility.Collapsed; // 우선순위 콤보박스 Collapsed 처리
                    grdPort.Visibility = Visibility.Collapsed; // 출고 포트 콤보박스 Collapsed 처리
                    btnManualIssue.Visibility = Visibility.Collapsed; // 수동 출고예약 버튼 Collapsed 처리
                    break;
            }

            ClearControl();
            SetStockerTypeCombo(cboStockerType);
            SetStockerCombo(cboStocker);

            if (string.Equals(_selectedRadioButtonValue, "FIFO") || string.Equals(_selectedRadioButtonValue, "NORMAL"))
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

        private void rowCount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_maxcheckCount < rowCount.Value)
            {
                rowCount.Value = _maxcheckCount;
            }

            SetCheckedIssueTargetInfoGrid();
        }

        private void rowCount_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (_maxcheckCount < rowCount.Value)
                rowCount.Value = _maxcheckCount;

            SetCheckedIssueTargetInfoGrid();
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
                    string[] columnStrings = new[] { "LOT_TOTAL_QTY", "LOT_QTY", "LOT_HOLD_QTY", "LOT_HOLD_QMS_QTY" };

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


                // 선택한 셀의 Row 위치
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
                _isFirskUnchk = true;

                if (!string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "PRJT_NAME").GetString()))
                {
                    // C20220309-000071 - GMES  Logistics Mgmt improve
                    // 조건문 추가
                    if (cell.Column.Name.Equals("PRJT_NAME"))
                    {
                        if (!string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()))
                            _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    else
                    {
                        _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
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

                if (!string.Equals(_selectedRadioButtonValue, "FIFO"))
                {
                    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();
                }

                txtEquipmentName.Text = string.Empty;
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

                txtEquipmentName.Text = string.Empty;
                SelectWareHouseEmptyCarrierList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgStoreByNoReadCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
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

        private void dgStoreByNoReadCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgStoreByNoReadCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            _equipmentCode = null;
            _cstTypeCode = null;

            if (DataTableConverter.GetValue(drv, "EQPT_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                _cstTypeCode = DataTableConverter.GetValue(drv, "CST_TYPE_CODE").GetString();
            }

            txtEquipmentName.Text = string.Empty;
            SelectWareHouseNoReadCarrierList();

        }

        private void dgStoreByAbNormalCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "SKD_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SKD_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
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

        private void dgStoreByAbNormalCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgStoreByAbNormalCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            _equipmentCode = null;
            _abNormalReasonCode = null;

            if (DataTableConverter.GetValue(drv, "EQPT_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
            {
            }
            else
            {
                if (cell.Column.Name.Equals("EQPT_NAME"))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                }
                else
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _abNormalReasonCode = DataTableConverter.GetValue(drv, "ABNORM_TRF_RSN_CODE").GetString();
                }
            }

            txtEquipmentName.Text = string.Empty;
            SelectWareHouseAbNormalCarrierList();
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

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgIssueTargetInfo.Rows.Count; i++)
                        {
                            if (txtLotId.Text.ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "LOTID")).GetString())
                            {
                                DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 1);

                                dgIssueTargetInfo.EndEdit();
                                dgIssueTargetInfo.EndEditRow(true);
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NAME").GetString();
                            SetIssuePort(cboIssuePort, selectedequipmentCode);
                            SelectPortInfo(selectedequipmentCode);
                        }

                        txtLotId.Text = string.Empty;
                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        txtLotId.Text = string.Empty;
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;


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
                        if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
                        if (!string.Equals(_selectedRadioButtonValue, "NORMAL"))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 공 Carrier 출고, 오류 Carrier 출고 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void dgIssueTargetInfoByNoReadCarrier_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfoByNoReadCarrier;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgIssueTargetInfoByAbNormalCarrier_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfoByAbNormalCarrier;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboIssuePort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;

            try
            {
                if (cboIssuePort != null && cboIssuePort.SelectedItem != null)
                {
                    int previousRowIndex = e.OldValue;
                    int currentRowIndex = e.NewValue;

                    //string locationName = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Content.GetString();
                    //string locationId = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Tag.GetString();
                    string transferStateCode = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Name.GetString();

                    if (transferStateCode == "OUT_OF_SERVICE")
                    {
                        Util.MessageInfo("SFU8137");
                        cboIssuePort.SelectedIndex = previousRowIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPortInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_STAT_CODE").GetString() == "OUT_OF_SERVICE")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#BDBDBD");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgPortInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgIssueTargetInfo_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgIssueTargetInfo.ItemsSource == null) return;

                SelectPortInfo(string.Empty);
                txtEquipmentName.Text = string.Empty;
                SetIssuePort(cboIssuePort, string.Empty);

                DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
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

                new ClientProxy().ExecuteService("DA_WCS_SEL_MANUAL_OUT_INVENTORY_PANCAKE_NA", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
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
                        LotTotalCount = g.Sum(x => x.Field<int>("LOT_TOTAL_QTY")),
                        LotCount = g.Sum(x => x.Field<int>("LOT_QTY")),
                        LotHoldCount = g.Sum(x => x.Field<int>("LOT_HOLD_QTY")),
                        LotHoldQMSCount = g.Sum(x => x.Field<int>("LOT_HOLD_QMS_QTY")),
                        HalfSlittingSideCode = string.Empty,
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        bizResult.Columns.Add("PROD_VER_CODE");
                        bizResult.Columns.Add("HALF_SLIT_SIDE");

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

        private void SelectManualOutInventoryList(bool isRefresh = false)
        {
            Util.gridClear(dgPortInfo);

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
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
                dr["PRJT_NAME"] = _projectName;
                dr["PROD_VER_CODE"] = _productVersion;
                dr["PRODID"] = _productCode;
                dr["HALF_SLIT_SIDE"] = _halfSlitterSideCode != "A" ? _halfSlitterSideCode : null;
                dr["LOTID"] = _lotId;
                dr["PAST_DAY"] = _pastDay;
                dr["QA_INSP_JUDG_VALUE"] = _faultyType;
                dr["WIPHOLD"] = _selectedWipHold;
                dr["QMSHOLD"] = _selectedQmsHold;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_WCS_SEL_MANUAL_OUT_INVENTORY_LIST_NA", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);

                    string[] sColumnName = new string[] { "PRJT_NAME", "ELTR_TYPE_NAME", "SKID_ID", "CSTINDTTM", "PAST_DAY", "EQPT_NAME", "RACK_NAME" };

                    _util.SetDataGridMergeExtensionCol(dgIssueTargetInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                    if (!isRefresh)
                    {
                        if (CommonVerify.HasTableRow(bizResult) && string.Equals(_selectedRadioButtonValue, "FIFO"))
                        {
                            rowCount_ValueChanged(rowCount, null);
                        }
                        else
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SelectRequestTransferList()
        {
            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA") return;

            const string bizRuleName = "DA_SEL_MCS_REQ_TRF_MES_GUI";
            DataTable inDataTable = new DataTable("INDATA");
            _requestTransferInfoTable = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, null, "RSLTDT", inDataTable);
        }

        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_WCS_SEL_WAREHOUSE_EMPTY_CARRIER";
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

        private void SelectWareHouseNoReadCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_NOREAD";
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
                        EquipmentCode = string.Empty,
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        SkidCount = g.Sum(x => x.Field<Int32>("CST_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPT_NAME"] = query.EquipmentName;
                        newRow["CST_QTY"] = query.SkidCount;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByNoReadCarrier, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseAbNormalCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_ABNORM_CARRIER";
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
                        EquipmentCode = string.Empty,
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        SkidCount = g.Sum(x => x.Field<Int32>("SKD_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();


                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPT_NAME"] = query.EquipmentName;
                        newRow["SKD_QTY"] = query.SkidCount;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByAbNormalCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrierList()
        {
            Util.gridClear(dgPortInfo);
            string carrierState;

            if (!string.IsNullOrEmpty(_emptybobbinState))
            {
                if (_emptybobbinState == "Y")
                    carrierState = "U";
                else
                    carrierState = "E";
            }
            else
            {
                carrierState = null;
            }


            const string bizRuleName = "DA_WCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
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

                    if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
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

        private void SelectWareHouseNoReadCarrierList()
        {
            ShowLoadingIndicator();
            SelectRequestTransferList();
            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_NOREAD_LIST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("CST_TYPE_CODE", typeof(string));


            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = cboStockerType.SelectedValue;
            dr["EQPTID"] = _equipmentCode;
            dr["CST_TYPE_CODE"] = _cstTypeCode;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();

                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
                {
                    Util.GridSetData(dgIssueTargetInfoByNoReadCarrier, bizResult, null, true);
                    SetIssuePort(cboIssuePort, string.Empty);
                    dgIssueTargetInfoByNoReadCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfoByNoReadCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
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

                        var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                     where t.Field<string>("CARRIERID") == row["MCS_CST_ID"].GetString()
                                     select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                        if (query != null)
                        {
                            row["REQ_TRF_STAT"] = query.RequestTransferState;
                            row["CARRIERID"] = query.CarrierId;
                            row["REQ_TRFID"] = query.RequestTransferId;
                        }
                    }
                    bizResult.AcceptChanges();
                }

                Util.GridSetData(dgIssueTargetInfoByNoReadCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                dgIssueTargetInfoByNoReadCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfoByNoReadCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
            });

        }

        private void SelectWareHouseAbNormalCarrierList()
        {
            ShowLoadingIndicator();

            SelectRequestTransferList();
            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_ABNORM_LIST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = cboStockerType.SelectedValue;
            dr["EQPTID"] = _equipmentCode;
            dr["ABNORM_TRF_RSN_CODE"] = _abNormalReasonCode;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();

                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
                {
                    Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                    SetIssuePort(cboIssuePort, string.Empty);
                    dgIssueTargetInfoByAbNormalCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
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

                        var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                     where t.Field<string>("CARRIERID") == row["MCS_CST_ID"].GetString()
                                     select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                        if (query != null)
                        {
                            row["REQ_TRF_STAT"] = query.RequestTransferState;
                            row["CARRIERID"] = query.CarrierId;
                            row["REQ_TRFID"] = query.RequestTransferId;
                        }
                    }
                    bizResult.AcceptChanges();
                }

                Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                dgIssueTargetInfoByAbNormalCarrier.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRFID"].Visibility = Visibility.Collapsed;
            });
        }

        private void SelectReleaseCount()
        {
            const string bizRuleName = "DA_MCS_SEL_COMMCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CMCDTYPE"] = "CWA_JRW_FIFO_DEFAULT_CNT";
            dr["CMCODE"] = "DEFAULT_VAL";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ATTRIBUTE1"].GetInt() > 3 || dtResult.Rows[0]["ATTRIBUTE1"].GetInt() < 1)
                {
                    _maxcheckCount = 1;
                    rowCount.Maximum = 1;
                    rowCount.Value = 1;
                }
                else
                {
                    _maxcheckCount = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                    rowCount.Maximum = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                    rowCount.Value = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                }
            }
        }

        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPTNAME" : "EQPT_NAME").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();

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
                        SetIssuePort(cboIssuePort, currentequipmentCode);
                        SelectPortInfo(currentequipmentCode);
                        txtEquipmentName.Text = currentequipmentName;
                    }
                }
                else
                {
                    if (query.Count() <= 1)
                    {
                        SelectPortInfo(string.Empty);
                        txtEquipmentName.Text = string.Empty;
                        if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                        }
                    }
                }
            }
        }

        private void SelectPortInfo(string equipmentCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_WCS_SEL_STK_OUT_PORT_INFO", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgPortInfo, result, null, true);
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

        private void SaveManualIssue()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_WCS_REG_LOGIS_CMD_PSR_PSP_NA";

                DataSet ds = new DataSet();
                
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID", typeof(string));
                inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(int));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["TO_PORT_ID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                newRow["LOGIS_CMD_PRIORITY_NO"] = Util.NVC_Int(Convert.ToInt32(cbopriority.SelectedValue.ToString()));
                inTable.Rows.Add(newRow);

                DataTable inCst = ds.Tables.Add("INCST");
                inCst.Columns.Add("CSTID", typeof(string));
                inCst.Columns.Add("FROM_RACK_ID", typeof(string));

                #region Grid 내 SKID 중복제거 후 파라미터 Set
                DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<int>("CHK") == 1
                             select t).ToList();

                DataTable dtChk = query.Any() ? query.CopyToDataTable() : dt.Clone();

                DataRow[] dr = dtChk.DefaultView.ToTable(true, new string[] { "SKID_ID", "RACK_ID"}).Select();
                #endregion

                for (int i = 0; i < dr.Length; i++)
                {
                    DataRow newRow1 = inCst.NewRow();
                    newRow1["CSTID"] = dr[i]["SKID_ID"].ToString();
                    newRow1["FROM_RACK_ID"] = dr[i]["RACK_ID"].ToString();
                    inCst.Rows.Add(newRow1);
                }

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow2= inLot.NewRow();
                        newRow2["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                        newRow2["CSTID"] = DataTableConverter.GetValue(row.DataItem, "SKID_ID").GetString();
                        inLot.Rows.Add(newRow2);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCST,INLOT", null, (result, bizException) =>
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

                        // List 재 조회
                        SelectManualOutInventoryList(true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                C1DataGrid dg;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                }
                else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                {
                    dg = dgIssueTargetInfoByNoReadCarrier;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dg = dgIssueTargetInfoByAbNormalCarrier;
                }
                else
                {
                    dg = dgIssueTargetInfo;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        string carrierId;
                        if (string.Equals(dg.Name, "dgIssueTargetInfoByNoReadCarrier") || string.Equals(dg.Name, "dgIssueTargetInfoByAbNormalCarrier"))
                        {
                            carrierId = "MCS_CST_ID";
                        }
                        else
                        {
                            carrierId = "SKID_ID";
                        }

                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, carrierId).GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        newRow["DST_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        newRow["DST_LOCID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        inTable.Rows.Add(newRow);
                    }
                }

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

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

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {

                            SelectWareHouseEmptyCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                        {
                            SelectWareHouseNoReadCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                        {
                            SelectWareHouseAbNormalCarrierList();
                        }
                        else
                        {
                            SelectManualOutInventoryList(true);
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

        private void SaveDataIssue()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOCATED_ON_THING_DATA_OUT";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataTable inCst = ds.Tables.Add("INCST");
                inCst.Columns.Add("CSTID", typeof(string));

                C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NOREADCARRIER") ? dgIssueTargetInfoByNoReadCarrier : dgIssueTargetInfo;

                if (_selectedRadioButtonValue == "NOREADCARRIER")
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                        {
                            DataRow dr = inCst.NewRow();
                            dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "MCS_CST_ID").GetString();
                            inCst.Rows.Add(dr);
                        }
                    }
                }
                else
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                        {
                            DataRow dr = inLot.NewRow();
                            dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                            inLot.Rows.Add(dr);
                        }
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT,INCST", null, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {
                            SelectWareHouseEmptyCarrier();
                            SelectWareHouseEmptyCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                        {
                            SelectWareHouseNoReadCarrier();
                            SelectWareHouseNoReadCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                        {
                            SelectWareHouseAbNormalCarrier();
                            SelectWareHouseAbNormalCarrierList();
                        }
                        else
                        {
                            SelectManualOutInventory();
                            SelectManualOutInventoryList(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDataIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_UNLOAD_FROM_RACK_MANUAL_UI";

                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "NOREADCARRIER") ? dgIssueTargetInfoByNoReadCarrier : dgIssueTargetInfo;

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["RACK_ID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {
                            SelectWareHouseEmptyCarrier();
                            SelectWareHouseEmptyCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                        {
                            SelectWareHouseNoReadCarrier();
                            SelectWareHouseNoReadCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                        {
                            SelectWareHouseAbNormalCarrier();
                            SelectWareHouseAbNormalCarrierList();
                        }
                        else
                        {
                            SelectManualOutInventory();
                            SelectManualOutInventoryList(true);
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

        private void GetBizActorServerInfo()
        {
            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA") return;

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

        private bool IsAdminAuthorityByUserId(string userId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["USERID"] = userId;
                dr["AUTHID"] = "MESADMIN,MESDEV,LOGIS_MANA";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", inTable);

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

        private void ManualIssueAuthConfirm()
        {
            CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = "LOGIS_MANA";
            C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

            popupAuthConfirm.Closed += popupAuthConfirm_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
        }

        private void popupAuthConfirm_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
            }
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
        }

        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
 
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

            // 우선순위 콤보박스
            SetPriorityComBo(cbopriority);

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
            _cstTypeCode = string.Empty;
            _abNormalReasonCode = string.Empty;
            txtEquipmentName.Text = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _FastTrackLot = string.Empty;
            _FastFlag = string.Empty;
            _isFirskUnchk = false;

            Util.gridClear(dgStore);
            Util.gridClear(dgStoreByEmptyCarrier);
            Util.gridClear(dgStoreByNoReadCarrier);
            Util.gridClear(dgStoreByAbNormalCarrier);
            Util.gridClear(dgIssueTargetInfo);
            Util.gridClear(dgIssueTargetInfoByEmptyCarrier);
            Util.gridClear(dgIssueTargetInfoByNoReadCarrier);
            Util.gridClear(dgIssueTargetInfoByAbNormalCarrier);
            Util.gridClear(dgPortInfo);

            _requestTransferInfoTable.Clear();

            if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
            {
                SetIssuePort(cboIssuePort, string.Empty);
            }
        }

        private void SetCheckedIssueTargetInfoGrid()
        {
            try
            {
                if (Math.Abs(rowCount.Value) >= 0)
                {
                    int selectedCheckCount = (int)rowCount.Value;
                    int i = 0;

                    if (CommonVerify.HasDataGridRow(dgIssueTargetInfo))
                    {
                        if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") >= rowCount.Value) return;

                        foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
                        {
                            if (row.Type == DataGridRowType.Item)
                            {
                                if (i < selectedCheckCount)
                                {
                                    if (cboStockerType.SelectedValue.GetString() == "JRW")
                                    {
                                        if (DataTableConverter.GetValue(row.DataItem, "CWA_JRW_QMS").GetString() == "OK")
                                        {
                                            DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                            i++;
                                        }

                                    }
                                    //else if (cboStockerType.SelectedValue.GetString() == "PCW")
                                    //{
                                    //    if (DataTableConverter.GetValue(row.DataItem, "CWA_PCW_QMS").GetString() == "OK")
                                    //    {
                                    //        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                    //        i++;
                                    //    }
                                    //}
                                    else
                                    {
                                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                        i++;
                                    }

                                    dgIssueTargetInfo.EndEdit();
                                    dgIssueTargetInfo.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NAME").GetString();
                            SetIssuePort(cboIssuePort, selectedequipmentCode);
                            SelectPortInfo(selectedequipmentCode);
                        }

                        // CSR : C20220309-000071
                        // SKID 자동 체크 해제
                        // cboIssuePort ComboBox 데이터 생성 후 체크 해제기능 처리
                        if (_isFirskUnchk)
                            SetFirstUnCheck();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationManualIssue()
        {
            C1DataGrid dg = dgIssueTargetInfo;

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

            if (cboIssuePort.SelectedItem == null || string.IsNullOrEmpty((cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            if ((cboIssuePort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            {
                Util.MessageInfo("SFU8137");
                return false;
            }
            if (cbopriority.SelectedValue == null || cbopriority.SelectedValue.ToString() == "SELECT") 
            {
                // %1(을)를 선택하세요.
                Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("우선순위"));
                return false;
            }

            return true;
        }

        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
            {
                dg = dgIssueTargetInfoByNoReadCarrier;
            }
            else
            {
                dg = dgIssueTargetInfo;
            }

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

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString()) || DataTableConverter.GetValue(row.DataItem, "REQ_TRF_STAT").GetString() != "REQUEST")
                    {
                        Util.MessageInfo("SFU8116", ObjectDic.Instance.GetObjectName("반송요청상태"));
                        return false;
                    }
                }
            }

            return true;

        }

        private bool ValidationDataIssue()
        {
            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
            {
                dg = dgIssueTargetInfoByNoReadCarrier;
            }
            else
            {
                dg = dgIssueTargetInfo;
            }


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

            return true;
        }

        private void SetStockerTypeCombo(C1ComboBox cbo)
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

            const string bizRuleName = "DA_WCS_SEL_LOGIS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private static void SetElectrodeTypeCombo(C1ComboBox cbo)
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

        private static void SetPriorityComBo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "WCS_JOB_PRIORITY" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);


        }

        private void SetIssuePort(C1ComboBox cbo, string equipmentCode)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;

                if (cbo.Items.Count > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = equipmentCode;
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataTable dtResult;

                dtResult = new ClientProxy().ExecuteServiceSync("DA_WCS_SEL_STK_OUT_PORT_CBO", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["LOC_NAME"].GetString();
                    comboBoxItem.Tag = row["LOCID"].GetString();
                    comboBoxItem.Name = row["TRF_STAT_CODE"].GetString();

                    if (row["TRF_STAT_CODE"].GetString() == "OUT_OF_SERVICE")
                    {
                        comboBoxItem.Foreground = new SolidColorBrush(Colors.Red);
                        comboBoxItem.FontWeight = FontWeights.Bold;
                    }
                    cbo.Items.Add(comboBoxItem);
                }

                cboIssuePort.SelectedIndexChanged += cboIssuePort_SelectedIndexChanged;

                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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


        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(null, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }


        #endregion

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {
                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            for (int j = 0; j < dgIssueTargetInfo.Rows.Count; j++)
                            {
                                if (sPasteStrings[i].ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[j].DataItem, "LOTID")).GetString())
                                {
                                    DataTableConverter.SetValue(dgIssueTargetInfo.Rows[j].DataItem, "CHK", 1);

                                    dgIssueTargetInfo.EndEdit();
                                    dgIssueTargetInfo.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NAME").GetString();
                            SetIssuePort(cboIssuePort, selectedequipmentCode);
                            SelectPortInfo(selectedequipmentCode);
                        }

                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }

        }


        private void dgIssueTargetInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgIssueTargetInfo.ItemsSource)
                {
                    if (drv["SKID_ID"].ToString().Equals(item["SKID_ID"].ToString()))
                    {
                        item["CHK"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgIssueTargetInfoChoice_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgIssueTargetInfo.ItemsSource)
                {
                    if (drv["SKID_ID"].ToString().Equals(item["SKID_ID"].ToString()))
                    {
                        item["CHK"] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFirstUnCheck()
        {
            try
            {
                if (dgIssueTargetInfo.ItemsSource == null || dgIssueTargetInfo.Rows.Count < 0)
                    return;

                // 창고재고 클릭시 자동체크 해제 처리
                for (int i = 0; i < dgIssueTargetInfo.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK")))
                    {
                        DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 0);
                    }
                }

                _isFirskUnchk = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
