/*************************************************************************************
 Created Date : 2022.06.01
      Creator : 최도훈
   Decription : 활성화 창고 수동출고 예약
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.01  최도훈 선임 : Initial Created. (활성화 4동 분리)  
  2024.07.09  임정훈 사원 : 공Pallet 선택 시 세정후 컬럼 및 수동세정완료 버튼 기능 추가
  2024.08.20  임정훈 사원 : E20240805-000350 활성화 #4 MES 수동출고 예약 화면에 창고 선택 시 층별 구분 반영
  2025.07.09  이지은      : MES2.0 Rebulding / DB Convesion 과 Solace 도입으로 수정
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
    /// MCS001_078.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_078 : UserControl, IWorkArea
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
        private string _selectedRadioButtonValue;
        private string _projectName;
        private string _productVersion;
        private string _halfSlitterSideCode;
        private string _productCode;
        private string _holdCode;
        private string _pastDay;
        private string _lotId;
        private string _faultyType;
        private string _trayType;
        private string _emptyTrayState;
        private string _emptyClean;
        private string _equipmentCode;
        private string _electrodeTypeCode;
        private string _cstTypeCode;
        private string _abNormalReasonCode;
        private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedProdID = string.Empty;
        private string _selectedSearchCode = string.Empty;
        private bool _IsPalletSearch = false;
        private bool _IsCarrierSearch = false;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;

        private DataTable _requestTransferInfoTable;
        private DataTable _requestOutRackInfoTable;
        private DataTable _requestCleanerInfoTable;
        private bool _isGradeJudgmentDisplay;
        private bool _isAdminAuthority;
        private bool _isFAAuthority;

        public MCS001_078()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeRequestTransferTable();
            InitializeRequestOutRackTable();
            InitializeRequestCleanerTable();
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);
            _isFAAuthority = IsFAAuthorityByUserId(LoginInfo.USERID);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel, btnDataIssue};
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            // 데이터출고는 물류단에서 처리
            //if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;

            // FA권한이 있다면 창고간 반송 허용
            if (_isFAAuthority)
            {
                txtDstType.Visibility = Visibility.Visible;
                cboDstType.Visibility = Visibility.Visible;
            }
            else
            {
                txtDstType.Visibility = Visibility.Collapsed;
                cboDstType.Visibility = Visibility.Collapsed;
            }

            rdoRealPallet.IsChecked = true;
            InitializeGrid();
            InitializeCombo();
            SelectReleaseCount();

            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgRealPalletDetail.Viewport.HorizontalOffset;
        }

        private void dgRealPalletDetail_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            SetFloorCombo(cboFloor);

            //cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            //cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        private void cboFloor_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            if (cboFloor.SelectedItem == null)
            {
                cboFloor.SelectedIndex = 0;
            }
            SetStockerCombo(cboStocker);
        }
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            if (cboStocker.SelectedItem == null)
            {
                cboStocker.SelectedIndex = 0;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            //SelectOutRackList(false, null, null, null);
            //SelectCleanerList(false, null, null);
            SelectPortInfo(cboArea.SelectedValue.ToString(), null, "", "");
            SelectStkInfo(cboArea.SelectedValue.ToString());

            this._IsPalletSearch = false;
            this._IsCarrierSearch = false;
            string strCstType = string.Empty;

            // txtPalletId.Text 데이터 있으면 Pallet Search
            if (!string.IsNullOrEmpty(txtPalletId.Text.Trim()))
            {
                this._IsPalletSearch = true;
                this.rdoRealPallet.IsChecked = true;
            }
            else if (!string.IsNullOrEmpty(txtCarrierId.Text.Trim()))
            {
                this._IsCarrierSearch = true;
                strCstType = this.GetCstType(txtCarrierId.Text.Trim());

                if (strCstType == "U")
                {
                    this.rdoRealPallet.IsChecked = true;
                }
                else if (strCstType == "E" || strCstType == "T")
                {
                    this.rdoNoPallet.IsChecked = true;
                }
                else if (strCstType == "ABNORM")
                {
                    this.rdoAbnormalPallet.IsChecked = true;
                }
                else if (strCstType == "ERROR")
                {
                    this.rdoErrorPallet.IsChecked = true;
                }
            }

            if (rdoNoPallet.IsChecked == true)
            {
                SelectNoPallet();

                if (this._IsCarrierSearch)
                {
                    SelectNoPalletList();
                }
            }
            else if (rdoAbnormalPallet.IsChecked == true)
            {
                SelectAbnormalPallet();

                if (this._IsCarrierSearch)
                {
                    SelectAbnormalPalletList();
                }
            }
            else if (rdoErrorPallet.IsChecked == true)
            {
                SelectErrorPallet();

                if (this._IsCarrierSearch)
                {
                    SelectErrorPalletList();
                }
            }
            else
            {
                SelectRealPallet(dgRealPallet);

                if (this._IsPalletSearch || this._IsCarrierSearch)
                {
                    SelectRealPalletList(dgRealPalletDetail);
                }
            }
        }

        private string GetCstType(string cstId)
        {
            const string bizRuleName = "DA_MCS_SEL_CARRIER_INFO_BY_STK_FORMATION";
            DataTable inDataTable = new DataTable("RQSTDT");

            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("BLDGCODE", typeof(string));

            string[] arrData = cstId.Split(',');
            string strTempData = string.Empty;

            if (arrData.Length > 0)
            {
                strTempData = arrData[0];
            }

            DataRow dr = inDataTable.NewRow();

            //dr["CSTID"] = cstId;
            dr["CSTID"] = strTempData;
            dr["BLDGCODE"] = cboArea.SelectedValue.ToString();

            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            string strReturnType = string.Empty;

            if (dtResult.Rows.Count > 0)
            {
                if (dtResult.Rows[0]["ABNORM_TRF_RSN_CODE"].ToString() != "")
                {
                    strReturnType = "ABNORM";
                }
                else
                {
                    strReturnType = dtResult.Rows[0]["CSTSTAT"].ToString();
                }
            }

            return strReturnType;
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            // 반송하시겠습니까?
            Util.MessageConfirm("SFU8018", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 물류로부터 bizRule 회신 예정
                    this.SaveManualIssue(sender);
                }
            });
        }

        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationTransferCancel()) return;

                C1DataGrid dg;

                if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                {
                    dg = dgNoPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                {
                    dg = dgAbnormalPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                {
                    dg = dgErrorPalletDetail;
                }
                else
                {
                    dg = dgRealPalletDetail;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CMD_SEQNO", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CMD_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CMD_SEQNO").GetString();
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                //CMM_MCS_TRANSFER_CANCEL popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
                //object[] parameters = new object[1];
                //parameters[0] = inTable;
                //C1WindowExtension.SetParameters(popupTransferCancel, parameters);

                CMM_MCS_TRANSFER_CANCEL_BY_FP2 popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL_BY_FP2 { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = inTable;
                parameters[1] = cboArea.SelectedValue.ToString();
                parameters[2] = null;
                parameters[3] = "Port";
                C1WindowExtension.SetParameters(popupTransferCancel, parameters);

                popupTransferCancel.Closed += popupTransferCancel_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine(Logger.OPERATION_R + "btnTransferCancel_Click", ex, LogCategory.UI);
                Util.MessageException(ex);
            }
        }

        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MCS_TRANSFER_CANCEL_BY_FP2 popup = sender as CMM_MCS_TRANSFER_CANCEL_BY_FP2;
            if (popup != null && popup.IsUpdated)
            {
                if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                {
                    SelectNoPallet();
                    SelectNoPalletList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                {
                    SelectAbnormalPallet();
                    SelectAbnormalPalletList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                {
                    SelectErrorPallet();
                    SelectErrorPalletList();
                }
                else
                {
                    SelectRealPallet(dgRealPallet);
                    SelectRealPalletList(dgRealPalletDetail);
                }

                SelectPortInfo(cboArea.SelectedValue.ToString(), null, "", "");
                SelectStkInfo(cboArea.SelectedValue.ToString());
            }
        }

        private void btnDataIssue_Click(object sender, RoutedEventArgs e)
        {
            // 데이터출고는 물류단에서 처리
            //if (!ValidationDataIssue()) return;

            // ※주의! 데이터만 출고처리 됩니다. 출고 하시겠습니까?
            //Util.MessageConfirm("SFU8253", (result) =>
            //{
            //if (result == MessageBoxResult.OK)
            //{
            //this.SaveDataIssue();
            //}
            //});
        }

        private void rdoPalletType_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            spRealPallet.Visibility = Visibility.Collapsed;
            spAbnormalPallet.Visibility = Visibility.Collapsed;
            dgRealPallet.Visibility = Visibility.Collapsed;
            dgNoPallet.Visibility = Visibility.Collapsed;
            dgAbnormalPallet.Visibility = Visibility.Collapsed;
            dgErrorPallet.Visibility = Visibility.Collapsed;
            dgRealPalletDetail.Visibility = Visibility.Collapsed;
            dgNoPalletDetail.Visibility = Visibility.Collapsed;
            dgAbnormalPalletDetail.Visibility = Visibility.Collapsed;
            dgErrorPalletDetail.Visibility = Visibility.Collapsed;
            // 데이터출고는 물류단에서 처리
            //btnDataIssue.Visibility = Visibility.Collapsed;

            txtShipType.Visibility = Visibility.Collapsed;
            cboShipType.Visibility = Visibility.Collapsed;
            // 세정기 항목 보류. (20220601) 
            btnManualClean.Visibility = Visibility.Collapsed;
            BottomArea.ColumnDefinitions[5].Width = new GridLength(0, GridUnitType.Star);

            switch (radioButton.Name)
            {
                case "rdoRealPallet":
                    //spRealPallet.Visibility = Visibility.Visible;
                    dgRealPalletDetail.Visibility = Visibility.Visible;
                    dgRealPallet.Visibility = Visibility.Visible;
                    btnManualIssue.Visibility = Visibility.Visible;
                    btnTransferCancel.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "REALPALLET";

                    //txtShipType.Visibility = Visibility.Visible;
                    //cboShipType.Visibility = Visibility.Visible;
                    BottomArea.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);


                    //if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;
                    break;
                case "rdoNoPallet":
                    dgNoPallet.Visibility = Visibility.Visible;
                    dgNoPalletDetail.Visibility = Visibility.Visible;
                    btnManualIssue.Visibility = Visibility.Visible;
                    btnTransferCancel.Visibility = Visibility.Visible;
                    btnManualClean.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NOPALLET";

                    //if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;
                    break;
                case "rdoAbnormalPallet":
                    //spAbnormalPallet.Visibility = Visibility.Visible;
                    dgAbnormalPallet.Visibility = Visibility.Visible;
                    dgAbnormalPalletDetail.Visibility = Visibility.Visible;
                    btnManualIssue.Visibility = Visibility.Visible;
                    btnTransferCancel.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "ABNORMALPALLET";

                    //if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;
                    break;
                case "rdoErrorPallet":
                    dgErrorPallet.Visibility = Visibility.Visible;
                    dgErrorPalletDetail.Visibility = Visibility.Visible;
                    btnManualIssue.Visibility = Visibility.Hidden;
                    btnTransferCancel.Visibility = Visibility.Hidden;
                    btnManualClean.Visibility = Visibility.Hidden;
                    _selectedRadioButtonValue = "ERRORPALLET";

                    //if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;
                    break;
            }

            ClearControl();
        }

        private void rowCount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_maxcheckCount < rowCount.Value)
            {
                rowCount.Value = _maxcheckCount;
            }

            SetCheckedRealPalletInfoGrid();
        }

        private void rowCount_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (_maxcheckCount < rowCount.Value)
                rowCount.Value = _maxcheckCount;

            SetCheckedRealPalletInfoGrid();
        }

        private void dgRealPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "SHIP_PLT_QTY")
                        || string.Equals(e.Cell.Column.Name, "HOLD_PLT_QTY")
                        || string.Equals(e.Cell.Column.Name, "INSP_WAIT_PLT_QTY")
                        || string.Equals(e.Cell.Column.Name, "NG_PLT_QTY")
                        || string.Equals(e.Cell.Column.Name, "VLD_DATE_PLT_QTY")
                        || string.Equals(e.Cell.Column.Name, "SHIP_CELL_QTY")
                        || string.Equals(e.Cell.Column.Name, "HOLD_CELL_QTY")
                        || string.Equals(e.Cell.Column.Name, "INSP_WAIT_CELL_QTY")
                        || string.Equals(e.Cell.Column.Name, "NG_CELL_QTY")
                        || string.Equals(e.Cell.Column.Name, "VLD_DATE_CELL_QTY")
                        || string.Equals(e.Cell.Column.Name, "OUT_PLT_CNT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MDLLOT_ID")), ObjectDic.Instance.GetObjectName("합계")))
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

        private void dgRealPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRealPallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;
                //if (cell == null
                //    || cell.Column.Name.Equals("SHIP_CELL_QTY")
                //    || cell.Column.Name.Equals("HOLD_CELL_QTY")
                //    || cell.Column.Name.Equals("INSP_WAIT_CELL_QTY")
                //    || cell.Column.Name.Equals("NG_CELL_QTY"))
                //{
                //    return;
                //}

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedProdID = DataTableConverter.GetValue(drv, "PRODID").GetString();

                if (cell.Column.Name.Equals("MDLLOT_ID") || cell.Column.Name.Equals("PRJT_NAME"))
                {
                    _selectedSearchCode = "ALL";
                }
                else if (cell.Column.Name.Equals("SHIP_PLT_QTY") || cell.Column.Name.Equals("SHIP_CELL_QTY"))
                {
                    _selectedSearchCode = "OK";
                }
                else if (cell.Column.Name.Equals("HOLD_PLT_QTY") || cell.Column.Name.Equals("HOLD_CELL_QTY"))
                {
                    _selectedSearchCode = "HOLD";
                }
                else if (cell.Column.Name.Equals("INSP_WAIT_PLT_QTY") || cell.Column.Name.Equals("INSP_WAIT_CELL_QTY"))
                {
                    _selectedSearchCode = "INSP_WAIT";
                }
                else if (cell.Column.Name.Equals("NG_PLT_QTY") || cell.Column.Name.Equals("NG_CELL_QTY"))
                {
                    _selectedSearchCode = "NG";
                }
                else if (cell.Column.Name.Equals("VLD_DATE_PLT_QTY") || cell.Column.Name.Equals("VLD_DATE_CELL_QTY"))
                {
                    _selectedSearchCode = "VLD_DATE";
                }

                if (!cell.Column.Name.Equals("OUT_PLT_CNT"))
                {
                    Util.gridClear(dgOutRacktDetail);
                    tabItemRackList.IsSelected = true;
                    SelectRealPalletList(dgRealPalletDetail);
                }
                else
                {
                    Util.gridClear(dgRealPalletDetail);
                    //Util.gridClear(dgPortInfo);
                    tabItemOutRackList.IsSelected = true;
                    //txtEquipmentName.Text = string.Empty;
                    SelectOutRackList(true, "U", null, _selectedProdID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgNoPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (string.Equals(e.Cell.Column.Name, "E_PLT_CNT")
                    || string.Equals(e.Cell.Column.Name, "T_PLT_CLEAN_BEFORE_CNT")
                    || string.Equals(e.Cell.Column.Name, "T_PLT_CLEAN_AFTER_CNT")
                    || string.Equals(e.Cell.Column.Name, "OUT_PLT_CNT")
                    || string.Equals(e.Cell.Column.Name, "C_PLT_E_CNT")
                    || string.Equals(e.Cell.Column.Name, "C_PLT_T_CNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD_NAME")), ObjectDic.Instance.GetObjectName("합계")))
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
            }));
        }

        private void dgNoPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgNoPallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

                _trayType = null;
                _emptyTrayState = null;
                _emptyClean = null;

                //if (DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
                //{
                if (cell.Column.Name.Equals("CSTPROD_NAME") &&
                    DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
                {
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                }
                else if (cell.Column.Name.Equals("E_PLT_CNT"))
                {
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    _emptyTrayState = "E";//DataTableConverter.GetValue(drv, "CSTSTAT").GetString();
                }
                else if (cell.Column.Name.Equals("T_PLT_CLEAN_BEFORE_CNT"))
                {
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    _emptyTrayState = "T";//DataTableConverter.GetValue(drv, "CSTSTAT").GetString();
                    _emptyClean = "N";
                }
                else if (cell.Column.Name.Equals("T_PLT_CLEAN_AFTER_CNT"))
                {
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    _emptyTrayState = "T";// DataTableConverter.GetValue(drv, "CSTSTAT").GetString();
                    _emptyClean = "Y";
                }

                if (cell.Column.Name.Equals("OUT_PLT_CNT"))
                {
                    Util.gridClear(dgNoPalletDetail);
                    //Util.gridClear(dgPortInfo);
                    Util.gridClear(dgCleanerDetail);
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    tabItemOutRackList.IsSelected = true;
                    //txtEquipmentName.Text = string.Empty;
                    SelectOutRackList(true, "T", _trayType, null);
                }
                else if (cell.Column.Name.Equals("C_PLT_E_CNT"))
                {
                    Util.gridClear(dgNoPalletDetail);
                    //Util.gridClear(dgPortInfo);
                    Util.gridClear(dgOutRacktDetail);
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    tabItemCleanerList.IsSelected = true;
                    //txtEquipmentName.Text = string.Empty;
                    SelectCleanerList(true, "E", _trayType);
                }
                else if (cell.Column.Name.Equals("C_PLT_T_CNT"))
                {
                    Util.gridClear(dgNoPalletDetail);
                    //Util.gridClear(dgPortInfo);
                    Util.gridClear(dgOutRacktDetail);
                    _trayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    tabItemCleanerList.IsSelected = true;
                    //txtEquipmentName.Text = string.Empty;
                    SelectCleanerList(true, "T", _trayType);
                }
                else
                {
                    Util.gridClear(dgOutRacktDetail);
                    Util.gridClear(dgCleanerDetail);
                    tabItemRackList.IsSelected = true;
                    //txtEquipmentName.Text = string.Empty;
                    SelectNoPalletList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAbnormalPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "PLT_CNT")
                     || string.Equals(e.Cell.Column.Name, "OUT_PLT_CNT"))

                    {
                        //if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PLT_CNT").GetInt() > 0)
                        //{
                        //	e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //	e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //}
                        //else
                        //{
                        //	e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        //	e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //}

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
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

        private void dgAbnormalPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgAbnormalPallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            if (DataTableConverter.GetValue(drv, "EQPT_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                _abNormalReasonCode = DataTableConverter.GetValue(drv, "ABNORM_TRF_RSN_CODE").GetString();
            }
            else
            {
                _equipmentCode = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
            }

            if (!cell.Column.Name.Equals("OUT_PLT_CNT"))
            {
                Util.gridClear(dgOutRacktDetail);
                tabItemRackList.IsSelected = true;
                //txtEquipmentName.Text = string.Empty;
                SelectAbnormalPalletList();
            }
            else
            {
                Util.gridClear(dgAbnormalPalletDetail);
                //Util.gridClear(dgPortInfo);
                tabItemOutRackList.IsSelected = true;
                //txtEquipmentName.Text = string.Empty;
                SelectOutRackList(true, "ABNORM", null, null);
            }
        }

        private void dgErrorPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "PLT_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PLT_CNT").GetInt() > 0)
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

        private void dgErrorPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgErrorPallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            if (DataTableConverter.GetValue(drv, "EQPT_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
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
                    //_abNormalReasonCode = DataTableConverter.GetValue(drv, "CST_TYPE_CODE").GetString();
                    _cstTypeCode = DataTableConverter.GetValue(drv, "CST_TYPE_CODE").GetString();
                }
            }

            Util.gridClear(dgOutRacktDetail);
            tabItemRackList.IsSelected = true;
            //txtEquipmentName.Text = string.Empty;
            SelectErrorPalletList();
        }

        private void cboHoldReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectRealPalletList(dgRealPalletDetail);
            }
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

                    SelectRealPalletList(dgRealPalletDetail);
                }
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

                    SelectRealPalletList(dgRealPalletDetail);
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

                SelectRealPalletList(dgRealPalletDetail);
            }
        }

        /// <summary>
        /// 출하가능여부 관련 색깔 표현
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRealPalletDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "SHIPPING_YN")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHIPPING_YN").ToString() == "N")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        else
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                    }
                    else if (e.Cell.Column.Name == "BOXID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Column.Width = C1.WPF.DataGrid.DataGridLength.Auto;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                    if (_isscrollToHorizontalOffset)
                    {
                        dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgRealPalletDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRealPalletDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgRealPalletDetail;
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
        private void dgNoPalletDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgNoPalletDetail;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAbnormalPalletDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgAbnormalPalletDetail;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgErrorPalletDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgErrorPalletDetail;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboOutPort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;

            try
            {
                if (cboOutPort != null && cboOutPort.SelectedItem != null)
                {
                    int previousRowIndex = e.OldValue;
                    int currentRowIndex = e.NewValue;

                    //string locationName = ((ContentControl)(cboOutPort.Items[currentRowIndex])).Content.GetString();
                    //string locationId = ((ContentControl)(cboOutPort.Items[currentRowIndex])).Tag.GetString();
                    string transferStateCode = ((ContentControl)(cboOutPort.Items[currentRowIndex])).Name.GetString();

                    if (transferStateCode == "OUT_OF_SERVICE")
                    {
                        Util.MessageInfo("SFU8137");
                        cboOutPort.SelectedIndex = previousRowIndex;
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_STAT_CODE").GetString() == "OUT_OF_SERVICE" ||
                       (/*DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOC_DETL_TP").GetString() == "MANUAL_OUT_PORT" &&*/
                        DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_STAT_CODE").GetString() == "TRANSFER_BLOCKED"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#BDBDBD");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "REQ_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        //if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_CNT").GetInt() > 0)
                        //{
                        //	e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //	e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //}
                        //else
                        //{
                        //	e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        //}
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

        private void dgStkInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "MOVE_IN_QTY")
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
            }));
        }

        private void dgStkInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRealPalletDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("BOXID"))
                {
                    //object[] parameters = new object[1];
                    //parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "BOXID"));
                    //this.FrameOperation.OpenMenu("SFU010160050", true, parameters);

                    string sAREAID = LoginInfo.CFG_AREA_ID;
                    string sSHOPID = LoginInfo.CFG_SHOP_ID;
                    string sPalletid = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "BOXID"));

                    loadingIndicator.Visibility = Visibility.Visible;
                    string[] sParam = { sAREAID, sSHOPID, sPalletid };
                    // 기간별 Pallet 확정 이력 정보 조회
                    this.FrameOperation.OpenMenu("SFU010736080", true, sParam);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRealPalletDetail_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgRealPalletDetail.ItemsSource == null) return;

                //SelectPortInfo(string.Empty, string.Empty, string.Empty, string.Empty);
                //txtEquipmentName.Text = string.Empty;
                SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);

                DataTable dt = ((DataView)dgRealPalletDetail.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupAuthConfirm_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
            }
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                /*
				if (string.IsNullOrEmpty(textBox.Text.Trim()))
				{
					Util.MessageValidation("10013", (result) =>// Pallet ID를 입력하세요.
					{
						if (result == MessageBoxResult.OK)
						{
							textBox.SelectAll();
							textBox.Focus();
						}
					}, ObjectDic.Instance.GetObjectName("Pallet ID"));

					return;
				}
				*/

                this.btnSearch_Click(null, null);
            }
        }

        private void dgAbnormalPallet_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgAbnormalPallet;

                string columnNameBase = "OUT_PLT_CNT";
                int columnIdxBase = dg.Columns[columnNameBase].Index;

                if (dg.GetRowCount() > 1)
                {
                    e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dg.GetCell(0, columnIdxBase), dg.GetCell(dg.GetRowCount() - 2, columnIdxBase)));
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message.ToString());
            }
        }

        private void btnManualClean_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationClean()) return;

            // 수동세정완료 하시겠습니까?
            Util.MessageConfirm("SFU8267", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.SaveManualClean();
                }
            });
        }

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                /*
				if (string.IsNullOrEmpty(textBox.Text.Trim()))
				{
					Util.MessageValidation("10013", (result) =>// Pallet ID를 입력하세요.
					{
						if (result == MessageBoxResult.OK)
						{
							textBox.SelectAll();
							textBox.Focus();
						}
					}, ObjectDic.Instance.GetObjectName("Pallet ID"));

					return;
				}
				*/

                this.btnSearch_Click(null, null);
            }
        }

        private void popPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            PopupFindControl textBox = sender as PopupFindControl;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                /*
				if (string.IsNullOrEmpty(textBox.Text.Trim()))
				{
					Util.MessageValidation("10013", (result) =>// Pallet ID를 입력하세요.
					{
						if (result == MessageBoxResult.OK)
						{
							textBox.SelectAll();
							textBox.Focus();
						}
					}, ObjectDic.Instance.GetObjectName("Pallet ID"));

					return;
				}
				*/

                this.btnSearch_Click(null, null);
            }
        }

        private void txtPalletId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    string strData = string.Empty;

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (sPasteStrings[i].Trim() == "") continue;
                        strData += sPasteStrings[i].Trim() + ",";
                    }

                    if (strData.Length > 0)
                        strData = strData.Substring(0, strData.Length - 1);

                    this.txtPalletId.Text = strData;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCarrierId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    string strData = string.Empty;

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (sPasteStrings[i].Trim() == "") continue;
                        strData += sPasteStrings[i].Trim() + ",";
                    }

                    if (strData.Length > 0)
                        strData = strData.Substring(0, strData.Length - 1);

                    this.txtCarrierId.Text = strData;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgPortInfo);
                SelectPortInfo(cboArea.SelectedValue.ToString(), null, "", "");
                SelectStkInfo(cboArea.SelectedValue.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgPortInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            if (!cell.Column.Name.Equals("REQ_CNT"))
            {
                return;
            }

            string strPortId = string.Empty;

            strPortId = DataTableConverter.GetValue(drv, "PORT_ID").GetString();

            //DataTable inTable = new DataTable();
            //inTable.Columns.Add("ORDID", typeof(string));

            //foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            //{
            //	if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
            //	{
            //		DataRow newRow = inTable.NewRow();
            //		newRow["ORDID"] = DataTableConverter.GetValue(row.DataItem, "ORDID").GetString();
            //		inTable.Rows.Add(newRow);
            //	}
            //}


            CMM_MCS_TRANSFER_CANCEL_BY_FP2 popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL_BY_FP2 { FrameOperation = FrameOperation };
            object[] parameters = new object[4];
            parameters[0] = null;
            parameters[1] = cboArea.SelectedValue.ToString();
            parameters[2] = strPortId;
            parameters[3] = "Port";
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        private void dgStkInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            if (!cell.Column.Name.Equals("MOVE_IN_QTY"))
            {
                return;
            }

            string strPortId = string.Empty;

            strPortId = DataTableConverter.GetValue(drv, "PORT_ID").GetString();

            //DataTable inTable = new DataTable();
            //inTable.Columns.Add("ORDID", typeof(string));

            //foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            //{
            //	if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
            //	{
            //		DataRow newRow = inTable.NewRow();
            //		newRow["ORDID"] = DataTableConverter.GetValue(row.DataItem, "ORDID").GetString();
            //		inTable.Rows.Add(newRow);
            //	}
            //}


            CMM_MCS_TRANSFER_CANCEL_BY_FP2 popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL_BY_FP2 { FrameOperation = FrameOperation };
            object[] parameters = new object[4];
            parameters[0] = null;
            parameters[1] = cboArea.SelectedValue.ToString();
            parameters[2] = strPortId;
            parameters[3] = "Stocker";
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        private void dgNoPalletDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgAbnormalPalletDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgErrorPalletDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgNoPalletDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgAbnormalPalletDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgErrorPalletDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRealPalletDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.ViewCheckingQty(dgRealPalletDetail);
        }

        private void dgNoPalletDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.ViewCheckingQty(dgNoPalletDetail);
        }

        private void dgAbnormalPalletDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.ViewCheckingQty(dgAbnormalPalletDetail);
        }

        private void dgErrorPalletDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.ViewCheckingQty(dgErrorPalletDetail);
        }
        #endregion

        #region Method
        private void SelectOutRackList(bool IsBinding, string cstStat, string cstProd, string prodId)
        {
            //const string bizRuleName = "DA_SEL_MCS_OUTRACK_BY_MES_GUI";
            //DataTable inDataTable = new DataTable("RQSTDT");

            //inDataTable.Columns.Add("LANGID", typeof(string));
            //inDataTable.Columns.Add("BLDGOCDE", typeof(string));
            //inDataTable.Columns.Add("CSTSTAT", typeof(string));
            //inDataTable.Columns.Add("CSTPROD", typeof(string));
            //inDataTable.Columns.Add("PRODID", typeof(string));

            //DataRow dr = inDataTable.NewRow();
            //dr["LANGID"] = LoginInfo.LANGID;
            //dr["BLDGOCDE"] = cboArea.SelectedValue.ToString();
            //dr["CSTSTAT"] = (string.IsNullOrEmpty(cstStat)) ? null : cstStat;
            //dr["CSTPROD"] = (string.IsNullOrEmpty(cstProd)) ? null : cstProd;
            //dr["PRODID"] = (string.IsNullOrEmpty(prodId)) ? null : prodId;

            //inDataTable.Rows.Add(dr);

            //_requestOutRackInfoTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            //if (IsBinding)	Util.GridSetData(dgOutRacktDetail, _requestOutRackInfoTable, FrameOperation, true);
        }

        private void SelectCleanerList(bool IsBinding, string cstStat, string cstProd)
        {
            const string bizRuleName = "DA_MCS_SEL_FORMATION_CLEANER_CNT";

            DataTable inDataTable = new DataTable("RQSTDT");

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("BLDG_CODE", typeof(string));
            inDataTable.Columns.Add("CSTSTAT", typeof(string));
            inDataTable.Columns.Add("CSTPROD", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["BLDG_CODE"] = cboArea.SelectedValue.ToString();
            dr["CSTSTAT"] = (string.IsNullOrEmpty(cstStat)) ? null : cstStat;
            dr["CSTPROD"] = (string.IsNullOrEmpty(cstProd)) ? null : cstProd;

            inDataTable.Rows.Add(dr);

            _requestCleanerInfoTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (IsBinding) Util.GridSetData(dgCleanerDetail, _requestCleanerInfoTable, FrameOperation, true);
        }

        private void SelectRealPalletList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MCS_SEL_STO_FORMATION_REALPALLET_DETAIL";
            try
            {
                ShowLoadingIndicator();

                //Util.gridClear(dgPortInfo);

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SEARCH_CODE", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));
                inTable.Columns.Add("WH_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));
                //inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("PALLETID_LIST", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTID_LIST", typeof(string));
                inTable.Columns.Add("FLOORNUM", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (_selectedProdID != string.Empty)
                {
                    dr["PRODID"] = _selectedProdID;
                }
                if (_selectedSearchCode != string.Empty)
                {
                    dr["SEARCH_CODE"] = _selectedSearchCode;
                }
                else
                {
                    dr["SEARCH_CODE"] = "ALL";
                }

                dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                dr["WH_ID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();

                //dr["BOXID"] = string.IsNullOrWhiteSpace(txtPalletId.Text.Trim()) ? null : txtPalletId.Text.Trim();
                dr["PALLETID_LIST"] = string.IsNullOrWhiteSpace(txtPalletId.Text.Trim()) ? null : ConvertData(txtPalletId.Text.Trim());
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);

                //if (txtLocation.Text != string.Empty)
                //{
                //    string _Location = string.Empty;
                //    DataRow[] drLocation = _dtLocation.Select("CBO_CODE ='" + cboArea.SelectedValue.ToString() + "'");
                //    _Location = drLocation[0]["WH_PHYS_PSTN_CODE"].ToString() + txtLocation.Text;

                //    dr["RACK_ID"] = _Location;
                //}
                //if (txtPalletID.Text != string.Empty)
                //{

                //    dr["BOXID"] = txtPalletID.Text;
                //}

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    SelectRequestTransferList(bizResult);
                    HiddenLoadingIndicator();

                    bizResult.Columns.Add("ORD_STAT", typeof(string));
                    bizResult.Columns.Add("ORD_STAT_NAME", typeof(string));
                    bizResult.Columns.Add("CARRIERID", typeof(string));
                    bizResult.Columns.Add("ORDID", typeof(string));
                    //bizResult.Columns.Add("STK_ISS_TYPE", typeof(string));
                    bizResult.Columns.Add("STK_ISS_TYPE_NAME", typeof(string));
                    bizResult.Columns.Add("DST_LOCNAME", typeof(string));
                    bizResult.Columns.Add("CMD_SEQNO", typeof(string));

                    if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    {
                        foreach (DataRow row in bizResult.Rows)
                        {
                            var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                         where t.Field<string>("CARRIERID") == row["CSTID"].GetString()
                                         //select new { RequestTransferState = t.Field<string>("ORD_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("ORDID"), STKIssueType = t.Field<string>("STK_ISS_TYPE"), STKIssueTypeName = t.Field<string>("STK_ISS_TYPE_NAME"), DSTLocName = t.Field<string>("DST_LOCNAME") }).FirstOrDefault();
                                         select new
                                         {
                                             RequestTransferState = t.Field<string>("ORD_STAT"),
                                             RequestTransferStateName = t.Field<string>("ORD_STAT_NAME"),
                                             CarrierId = t.Field<string>("CARRIERID"),
                                             RequestTransferId = t.Field<string>("ORDID"),
                                             STKIssueTypeName = t.Field<string>("STK_ISS_TYPE_NAME"),
                                             DSTLocName = t.Field<string>("DST_LOCNAME"),
                                             CmdSeqNo = t.Field<decimal>("CMD_SEQNO").ToString()
                                         }).FirstOrDefault();

                            if (query != null)
                            {
                                row["ORD_STAT"] = query.RequestTransferState;
                                row["ORD_STAT_NAME"] = query.RequestTransferStateName;
                                row["CARRIERID"] = query.CarrierId;
                                row["ORDID"] = query.RequestTransferId;
                                //row["STK_ISS_TYPE"] = query.STKIssueType;
                                row["STK_ISS_TYPE_NAME"] = query.STKIssueTypeName;
                                row["DST_LOCNAME"] = query.DSTLocName;
                                row["CMD_SEQNO"] = query.CmdSeqNo;
                            }
                        }
                        bizResult.AcceptChanges();
                    }

                    Util.GridSetData(dg, bizResult, FrameOperation, true);
                    SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                    dgRealPalletDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgRealPalletDetail.Columns["ORDID"].Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectRequestTransferList(DataTable dt)
        {
            string strCstIdList = string.Empty;

            const string bizRuleName = "DA_MHS_SEL_TRF_CMD_BY_CSTID";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = string.IsNullOrWhiteSpace(row["CSTID"].ToString()) ? null : row["CSTID"].ToString();
                inDataTable.Rows.Add(dr);
            }

            if (inDataTable.Rows.Count <= 0) return;

            _requestTransferInfoTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
        }

        private void SelectNoPallet()
        {
            const string bizRuleName = "DA_MCS_SEL_FORMATION_STO_EMPTY_PALLET";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTID_LIST", typeof(string));
                inTable.Columns.Add("FLOORNUM", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "STO";   //cboStockerType.SelectedValue;
                dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                dr["EQPTID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("OUT_PLT_CNT", typeof(int));
                    bizResult.Columns.Add("C_PLT_E_CNT", typeof(int));
                    bizResult.Columns.Add("C_PLT_T_CNT", typeof(int));

                    foreach (DataRow drData in bizResult.Rows)
                    {
                        DataRow[] drs = _requestOutRackInfoTable.Select("MDL_TP = '" + drData["CSTPROD"].ToString() + "' AND CARRIER_STRUCT = 'T'");
                        DataRow[] drs_C1 = _requestCleanerInfoTable.Select("CSTPROD = '" + drData["CSTPROD"].ToString() + "' AND CSTSTAT = 'E'");
                        DataRow[] drs_C2 = _requestCleanerInfoTable.Select("CSTPROD = '" + drData["CSTPROD"].ToString() + "' AND CSTSTAT = 'T'");
                        drData["OUT_PLT_CNT"] = drs.Length;
                        drData["C_PLT_E_CNT"] = drs_C1.Length;
                        drData["C_PLT_T_CNT"] = drs_C2.Length;
                    }

                    // 기준정보를 기반으로한 전체 조회 방식으로 변경후 이동중인 수량을 들고 와서 모든 데이터가 0인것만 제거하는 로직
                    DataTable dtTempResult = bizResult.DefaultView.ToTable();
                    dtTempResult.Clear();

                    DataRow[] drs2 = bizResult.DefaultView.ToTable().Select("E_PLT_CNT <> 0 OR T_PLT_CLEAN_BEFORE_CNT <> 0 OR T_PLT_CLEAN_AFTER_CNT <> 0 OR OUT_PLT_CNT <> 0 OR C_PLT_E_CNT <> 0 OR C_PLT_T_CNT <> 0", "CSTPROD ASC");

                    foreach (DataRow dr2 in drs2)
                    {
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtTempResult.Rows.Add(dr2.ItemArray);
                        dtTempResult.AddDataRow(dr2);
                    }

                    bizResult = dtTempResult;

                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    { }).Select(g => new
                    {
                        CstProd = string.Empty,
                        CstProdName = ObjectDic.Instance.GetObjectName("합계"),
                        E_PLTQty = g.Sum(x => x.Field<Int32>("E_PLT_CNT")),
                        T_PLT_Clean_Before_Qty = g.Sum(x => x.Field<Int32>("T_PLT_CLEAN_BEFORE_CNT")),
                        T_PLT_Clean_After_Qty = g.Sum(x => x.Field<Int32>("T_PLT_CLEAN_AFTER_CNT")),
                        OUT_PLT_CNT_Qty = g.Sum(x => x.Field<Int32>("OUT_PLT_CNT")),
                        Cleaner_PLT_E_Qty = g.Sum(x => x.Field<Int32>("C_PLT_E_CNT")),
                        Cleaner_PLT_T_Qty = g.Sum(x => x.Field<Int32>("C_PLT_T_CNT")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["CSTPROD"] = query.CstProd;
                        newRow["CSTPROD_NAME"] = query.CstProdName;
                        newRow["E_PLT_CNT"] = query.E_PLTQty;
                        newRow["T_PLT_CLEAN_BEFORE_CNT"] = query.T_PLT_Clean_Before_Qty;
                        newRow["T_PLT_CLEAN_AFTER_CNT"] = query.T_PLT_Clean_After_Qty;
                        newRow["OUT_PLT_CNT"] = query.OUT_PLT_CNT_Qty;
                        newRow["C_PLT_E_CNT"] = query.Cleaner_PLT_E_Qty;
                        newRow["C_PLT_T_CNT"] = query.Cleaner_PLT_T_Qty;
                        bizResult.Rows.Add(newRow);
                    }

                    /*
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
                    */

                    Util.GridSetData(dgNoPallet, bizResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectAbnormalPallet()
        {
            const string bizRuleName = "DA_MCS_SEL_FORMATION_STK_ABNORM_PALLET";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                //inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTID_LIST", typeof(string));
                inTable.Columns.Add("FLOORNUM", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "STO";
                //dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                dr["EQPTID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("OUT_PLT_CNT", typeof(int));

                    foreach (DataRow drData in bizResult.Rows)
                    {
                        DataRow[] drs = _requestOutRackInfoTable.Select("CARRIER_STRUCT = 'ABNORM'");
                        drData["OUT_PLT_CNT"] = drs.Length;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EquipmentCode = string.Empty,
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        PLTQty = g.Sum(x => x.Field<Int32>("PLT_CNT")),
                        OUT_PLT_CNT_Qty = (g.First() == null) ? 0 : Convert.ToInt32(g.First()["OUT_PLT_CNT"]),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPT_NAME"] = query.EquipmentName;
                        newRow["PLT_CNT"] = query.PLTQty;
                        newRow["OUT_PLT_CNT"] = query.OUT_PLT_CNT_Qty;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgAbnormalPallet, bizResult, FrameOperation, true);
                    //string[] columnName = new string[] { "OUT_PLT_CNT" };
                    //_util.SetDataGridMergeExtensionCol(dgAbnormalPallet, columnName, DataGridMergeMode.VERTICAL);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectErrorPallet()
        {
            const string bizRuleName = "DA_MCS_SEL_FORMATION_STK_ERROR_PALLET";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                //inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTID_LIST", typeof(string));
                inTable.Columns.Add("FLOORNUM", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "STO";
                //dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                dr["EQPTID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);
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
                        PLTQty = g.Sum(x => x.Field<Int32>("PLT_CNT")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPT_NAME"] = query.EquipmentName;
                        newRow["PLT_CNT"] = query.PLTQty;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgErrorPallet, bizResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectNoPalletList()
        {
            //Util.gridClear(dgPortInfo);
            string carrierState;

            if (!string.IsNullOrEmpty(_emptyTrayState))
            {
                carrierState = _emptyTrayState;
                /*
                if (_emptybobbinState == "Y")
                    carrierState = "T";     // 공Tray Pallet
                else
                    carrierState = "E";     // 공Pallet
                */
            }
            else
            {
                carrierState = null;
            }

            const string bizRuleName = "DA_MCS_SEL_FORMATION_STO_EMPTY_PALLET_LIST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("EQGRID", typeof(string));
                //inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("BLDGCODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTID_LIST", typeof(string));
                inTable.Columns.Add("FLOORNUM", typeof(string));

                bool isCarrierSearch = !string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim());

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["EQGRID"] = cboStockerType.SelectedValue;
                //dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["CSTPROD"] = _trayType == "" ? null : _trayType;
                dr["CSTSTAT"] = carrierState == "" ? null : carrierState;
                dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
                dr["EQPTID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();

                if (isCarrierSearch)
                {
                    //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                    dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                }
                else
                {
                    dr["CST_CLEAN_FLAG"] = _emptyClean;
                }

                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    SelectRequestTransferList(bizResult);
                    HiddenLoadingIndicator();

                    bizResult.Columns.Add("ORD_STAT", typeof(string));
                    bizResult.Columns.Add("ORD_STAT_NAME", typeof(string));
                    bizResult.Columns.Add("CARRIERID", typeof(string));
                    bizResult.Columns.Add("ORDID", typeof(string));
                    //bizResult.Columns.Add("STK_ISS_TYPE", typeof(string));
                    bizResult.Columns.Add("STK_ISS_TYPE_NAME", typeof(string));
                    bizResult.Columns.Add("DST_LOCNAME", typeof(string));
                    bizResult.Columns.Add("CMD_SEQNO", typeof(string));


                    if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    {
                        foreach (DataRow row in bizResult.Rows)
                        {
                            var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                         where t.Field<string>("CARRIERID") == row["CSTID"].GetString()
                                         select new
                                         {
                                             RequestTransferState = t.Field<string>("ORD_STAT"),
                                             RequestTransferStateName = t.Field<string>("ORD_STAT_NAME"),
                                             CarrierId = t.Field<string>("CARRIERID"),
                                             RequestTransferId = t.Field<string>("ORDID"),
                                             STKIssueTypeName = t.Field<string>("STK_ISS_TYPE_NAME"),
                                             DSTLocName = t.Field<string>("DST_LOCNAME"),
                                             CmdSeqNo = t.Field<decimal>("CMD_SEQNO").ToString()
                                         }).FirstOrDefault();

                            if (query != null)
                            {
                                row["ORD_STAT"] = query.RequestTransferState;
                                row["ORD_STAT_NAME"] = query.RequestTransferStateName;
                                row["CARRIERID"] = query.CarrierId;
                                row["ORDID"] = query.RequestTransferId;
                                //row["STK_ISS_TYPE"] = query.STKIssueType;
                                row["STK_ISS_TYPE_NAME"] = query.STKIssueTypeName;
                                row["DST_LOCNAME"] = query.DSTLocName;
                                row["CMD_SEQNO"] = query.CmdSeqNo;
                            }
                        }
                        bizResult.AcceptChanges();
                    }

                    Util.GridSetData(dgNoPalletDetail, bizResult, FrameOperation, true);
                    SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                    dgNoPalletDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                    dgNoPalletDetail.Columns["ORDID"].Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectAbnormalPalletList()
        {
            ShowLoadingIndicator();

            //Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MCS_SEL_FORMATION_STO_ABNORM_PALLET_LIST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("BLDGCODE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
            //inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("CSTID_LIST", typeof(string));
            inTable.Columns.Add("FLOORNUM", typeof(string));

            bool isCarrierSearch = !string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim());

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = "STO";
            dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();

            if (isCarrierSearch)
            {
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
            }
            else
            {
                dr["EQPTID"] = _equipmentCode;
                dr["ABNORM_TRF_RSN_CODE"] = _abNormalReasonCode;
            }

            dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);

            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(bizException);
                    return;
                }

                SelectRequestTransferList(bizResult);
                HiddenLoadingIndicator();

                bizResult.Columns.Add("ORD_STAT", typeof(string));
                bizResult.Columns.Add("ORD_STAT_NAME", typeof(string));
                bizResult.Columns.Add("CARRIERID", typeof(string));
                bizResult.Columns.Add("ORDID", typeof(string));
                //bizResult.Columns.Add("STK_ISS_TYPE", typeof(string));
                bizResult.Columns.Add("STK_ISS_TYPE_NAME", typeof(string));
                bizResult.Columns.Add("DST_LOCNAME", typeof(string));
                bizResult.Columns.Add("CMD_SEQNO", typeof(string));

                if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                {
                    foreach (DataRow row in bizResult.Rows)
                    {
                        var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                     where t.Field<string>("CARRIERID") == row["CSTID"].GetString()
                                     select new
                                     {
                                         RequestTransferState = t.Field<string>("ORD_STAT"),
                                         RequestTransferStateName = t.Field<string>("ORD_STAT_NAME"),
                                         CarrierId = t.Field<string>("CARRIERID"),
                                         RequestTransferId = t.Field<string>("ORDID"),
                                         STKIssueTypeName = t.Field<string>("STK_ISS_TYPE_NAME"),
                                         DSTLocName = t.Field<string>("DST_LOCNAME"),
                                         CmdSeqNo = t.Field<decimal>("CMD_SEQNO").ToString()
                                     }).FirstOrDefault();

                        if (query != null)
                        {
                            row["ORD_STAT"] = query.RequestTransferState;
                            row["ORD_STAT_NAME"] = query.RequestTransferStateName;
                            row["CARRIERID"] = query.CarrierId;
                            row["ORDID"] = query.RequestTransferId;
                            //row["STK_ISS_TYPE"] = query.STKIssueType;
                            row["STK_ISS_TYPE_NAME"] = query.STKIssueTypeName;
                            row["DST_LOCNAME"] = query.DSTLocName;
                            row["CMD_SEQNO"] = query.CmdSeqNo;
                        }
                    }
                    bizResult.AcceptChanges();
                }

                Util.GridSetData(dgAbnormalPalletDetail, bizResult, FrameOperation, true);
                SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                dgAbnormalPalletDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                dgAbnormalPalletDetail.Columns["ORDID"].Visibility = Visibility.Collapsed;
            });
        }

        private void SelectErrorPalletList()
        {
            ShowLoadingIndicator();

            //Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MCS_SEL_FORMATION_STK_ERROR_PALLET_LIST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("BLDGCODE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("CST_TYPE_CODE", typeof(string));
            //inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("CSTID_LIST", typeof(string));
            inTable.Columns.Add("FLOORNUM", typeof(string));

            bool isCarrierSearch = !string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim());

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = "STO";
            dr["BLDGCODE"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();

            if (isCarrierSearch)
            {
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
            }
            else
            {
                dr["EQPTID"] = _equipmentCode;
                dr["CST_TYPE_CODE"] = _cstTypeCode;
            }

            dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);

            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(bizException);
                    return;
                }

                SelectRequestTransferList(bizResult);
                HiddenLoadingIndicator();

                bizResult.Columns.Add("ORD_STAT", typeof(string));
                bizResult.Columns.Add("ORD_STAT_NAME", typeof(string));
                bizResult.Columns.Add("CARRIERID", typeof(string));
                bizResult.Columns.Add("ORDID", typeof(string));
                //bizResult.Columns.Add("STK_ISS_TYPE", typeof(string));
                bizResult.Columns.Add("STK_ISS_TYPE_NAME", typeof(string));
                bizResult.Columns.Add("DST_LOCNAME", typeof(string));
                bizResult.Columns.Add("CMD_SEQNO", typeof(string));

                if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                {
                    foreach (DataRow row in bizResult.Rows)
                    {
                        var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                     where t.Field<string>("CARRIERID") == row["CSTID"].GetString()
                                     select new
                                     {
                                         RequestTransferState = t.Field<string>("ORD_STAT"),
                                         RequestTransferStateName = t.Field<string>("ORD_STAT_NAME"),
                                         CarrierId = t.Field<string>("CARRIERID"),
                                         RequestTransferId = t.Field<string>("ORDID"),
                                         STKIssueTypeName = t.Field<string>("STK_ISS_TYPE_NAME"),
                                         DSTLocName = t.Field<string>("DST_LOCNAME"),
                                         CmdSeqNo = t.Field<decimal>("CMD_SEQNO").ToString()
                                     }).FirstOrDefault();

                        if (query != null)
                        {
                            row["ORD_STAT"] = query.RequestTransferState;
                            row["ORD_STAT_NAME"] = query.RequestTransferStateName;
                            row["CARRIERID"] = query.CarrierId;
                            row["ORDID"] = query.RequestTransferId;
                            //row["STK_ISS_TYPE"] = query.STKIssueType;
                            row["STK_ISS_TYPE_NAME"] = query.STKIssueTypeName;
                            row["DST_LOCNAME"] = query.DSTLocName;
                            row["CMD_SEQNO"] = query.CmdSeqNo;
                        }
                    }
                    bizResult.AcceptChanges();
                }

                Util.GridSetData(dgErrorPalletDetail, bizResult, FrameOperation, true);
                SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                dgErrorPalletDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
                dgErrorPalletDetail.Columns["ORDID"].Visibility = Visibility.Collapsed;
            });

        }

        private void SelectReleaseCount()
        {
            _maxcheckCount = 1;
            rowCount.Maximum = 1;
            rowCount.Value = 1;

            /*
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
            */
        }

        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentRackId = DataTableConverter.GetValue(e.Row.DataItem, "RACK_ID").GetString();
                string currentCstStat = string.Empty;
                string currentCstId = DataTableConverter.GetValue(e.Row.DataItem, "CSTID").GetString();

                if (dg != dgErrorPalletDetail)
                {
                    currentCstStat = DataTableConverter.GetValue(e.Row.DataItem, "CSTSTAT").GetString();
                }

                //string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgErrorPalletDetail") ? "EQPTNAME" : "EQPTNAME").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, "EQPTNAME").GetString();
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
                             where t.Field<Int64>("CHK") == 1
                             select t).ToList();

                if (checkValue == "0")
                {
                    if (query.Count() < 1)
                    {
                        SetOutPort(cboOutPort, currentequipmentCode, currentRackId, currentCstStat, currentCstId);
                        //SelectPortInfo(string.Empty, currentequipmentCode, currentRackId, currentCstStat);
                        //txtEquipmentName.Text = currentequipmentName;
                    }
                }
                else
                {
                    if (query.Count() <= 1)
                    {
                        //SelectPortInfo(string.Empty, string.Empty, string.Empty, string.Empty);
                        //txtEquipmentName.Text = string.Empty;
                        if (cboOutPort.SelectedItem != null && cboOutPort.Items.Count > 0)
                        {
                            SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                        }
                    }
                }
            }
        }

        private void SelectPortInfo(string bldgCode, string equipment, string portTypeCodeList, string portWrkMode)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MHS_SEL_STO_PORT_LIST_FOR_PLT";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                //inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PORT_TYPE_CODE_LIST", typeof(string));
                inTable.Columns.Add("PORT_WRK_MODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["EQPTID"] = equipment;
                dr["PORT_TYPE_CODE_LIST"] = "'MGV'";        // MGV : 작업자가 대차로 Carrier를 투입/배출할 수 있는 포트
                                                            // MP  : Stocker에서 Rack에 적재된 Carrier를 확인하기 위한 포트
                dr["PORT_WRK_MODE"] = 'M';

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
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

        private void SelectStkInfo(string equipment)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MHS_SEL_STO_LIST_BY_STO_TYPE";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("MES_SYSTEM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MES_SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                dr["EQPT_GR_TYPE_CODE"] = "PLT";
                dr["EQPTID"] = null;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgStkInfo, result, null, true);
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

        private void SaveManualIssue(object sender)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI_2";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("SRC_PORTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("PRIORITY_NO", typeof(decimal));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));


                C1DataGrid dg;

                bool IsProdIdNull = false;
                bool IsCstStatNull = false;
                bool IsTrayTypeNull = false;
                bool IsSTKIssueTypeNull = false;

                if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                {
                    dg = dgNoPalletDetail;
                    IsProdIdNull = true;
                    IsSTKIssueTypeNull = true;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                {
                    dg = dgAbnormalPalletDetail;
                    IsProdIdNull = true;
                    IsSTKIssueTypeNull = true;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                {
                    dg = dgErrorPalletDetail;
                    IsProdIdNull = true;
                    IsCstStatNull = true;
                    IsTrayTypeNull = true;
                    IsSTKIssueTypeNull = true;
                }
                else
                {
                    dg = dgRealPalletDetail;
                }

                // 창고간 반송은 실팔레트도 출고유형 제거함.
                if (!cboDstType.SelectedValue.ToString().ToUpper().Equals("PORT"))
                {
                    IsSTKIssueTypeNull = true;
                }

                string strOutPortTag = (cboOutPort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                string[] strOutPortTagList = strOutPortTag.Split('|');

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        //DataTable inTable1 = new DataTable("RQSTDT");
                        //inTable1.Columns.Add("LANGID", typeof(string));
                        //inTable1.Columns.Add("EQPTID", typeof(string));

                        //DataRow dr1 = inTable1.NewRow();
                        //dr1["LANGID"] = LoginInfo.LANGID;
                        //dr1["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        //inTable1.Rows.Add(dr1);

                        //string strDstPortID = string.Empty;
                        //DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_CARRIER_BY_CSTID", "RQSTDT", "RSLTDT", inTable1);

                        //if (dtResult1.Rows.Count > 0)
                        //{
                        //    strDstPortID = dtResult1.Rows.GetValue("PORT_ID").ToString();
                        //}


                        DataRow dr = inTable.NewRow();
                        dr["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        dr["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        dr["SRC_PORTID"] = DataTableConverter.GetValue(row.DataItem, "PORT_CUR").GetString();
                        dr["DST_EQPTID"] = strOutPortTagList[0];
                        dr["DST_PORTID"] = strOutPortTagList[1];
                        dr["PRIORITY_NO"] = 50;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["TRF_CAUSE_CODE"] = null;
                        dr["MANL_TRF_CAUSE_CNTT"] = null;
                        inTable.Rows.Add(dr);

                        //Util.GetCondition(cboOutPort, sMsg: "FM_ME_0340");  //배출할 위치를 선택해주세요.
                        //if (string.IsNullOrEmpty(cboOutPort.ToString())) return;
                        //  drIn["DST_EQPTID"] = Util.GetCondition(cboToEqp);
                        //dr["DST_EQPTID"] = Util.NVC(cboToEqp.Tag);

                        //DataRow newRow = inTable.NewRow();
                        //newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        //newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        //newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        //newRow["DST_EQPTID"] = strOutPortTagList[0];
                        //newRow["DST_LOCID"] = strOutPortTagList[1];
                        //newRow["USER"] = LoginInfo.USERID;
                        //newRow["DTTM"] = dtSystem;
                        //newRow["PRODID"] = IsProdIdNull ? null : DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();

                        //if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                        //{
                        //	newRow["CARRIER_STRUCT"] = "ABNORM";
                        //}
                        //else
                        //{
                        //	newRow["CARRIER_STRUCT"] = IsCstStatNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
                        //}

                        //                  newRow["MDL_TP"] = IsTrayTypeNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTPROD").GetString();
                        //newRow["STK_ISS_TYPE"] = IsSTKIssueTypeNull ? null : cboShipType.SelectedValue.ToString();

                        //inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                        {
                            SelectNoPalletList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                        {
                            SelectAbnormalPalletList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                        {
                            SelectErrorPalletList();
                        }
                        else
                        {
                            SelectRealPalletList(dgRealPalletDetail);
                        }

                        SelectPortInfo(cboArea.SelectedValue.ToString(), "", "", "");
                        SelectStkInfo(cboArea.SelectedValue.ToString());
                    }
                    catch (Exception e)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(e);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveManualClean()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_STK_FORMATION_MANUAL_CLEAN";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("UPDDTTM", typeof(DateTime));

                C1DataGrid dg = dgNoPalletDetail;

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["UPDDTTM"] = dtSystem;

                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
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
                        SelectNoPalletList();
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
                const string bizRuleName = "BR_MCS_REG_STK_FORMATION_DATA_OUT";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inCst = ds.Tables.Add("INCST");
                inCst.Columns.Add("CSTID", typeof(string));

                C1DataGrid dg;

                if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                {
                    dg = dgNoPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                {
                    dg = dgAbnormalPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                {
                    dg = dgErrorPalletDetail;
                }
                else
                {
                    dg = dgRealPalletDetail;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow dr = inCst.NewRow();
                        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        inCst.Rows.Add(dr);
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

                        if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                        {
                            SelectNoPallet();
                            SelectNoPalletList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                        {
                            SelectAbnormalPallet();
                            SelectAbnormalPalletList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                        {
                            SelectErrorPallet();
                            SelectErrorPalletList();
                        }
                        else
                        {
                            SelectRealPallet(dgRealPallet);
                            //SelectRealPalletList(true);
                            SelectRealPalletList(dgRealPalletDetail);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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
                dr["AUTHID"] = "MESADMIN,MESDEV";
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

        private bool IsFAAuthorityByUserId(string userId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["USERID"] = userId;
                dr["AUTHID"] = "MESADMIN,FA_MNGR";
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

        private void InitializeRequestTransferTable()
        {
            _requestTransferInfoTable = new DataTable();
            _requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
            _requestTransferInfoTable.Columns.Add("ORD_STAT_NAME", typeof(string));
            _requestTransferInfoTable.Columns.Add("STK_ISS_TYPE_NAME", typeof(string));
            _requestTransferInfoTable.Columns.Add("DST_LOCNAME", typeof(string));
            _requestTransferInfoTable.Columns.Add("ORD_STAT", typeof(string));
            _requestTransferInfoTable.Columns.Add("ORDID", typeof(string));
            _requestTransferInfoTable.Columns.Add("CMD_SEQNO", typeof(string));
        }

        private void InitializeRequestOutRackTable()
        {
            _requestOutRackInfoTable = new DataTable();
            _requestOutRackInfoTable.Columns.Add("CARRIERID", typeof(string));
            _requestOutRackInfoTable.Columns.Add("JOB_CREATE_TP", typeof(string));
            _requestOutRackInfoTable.Columns.Add("CARRIER_STRUCT", typeof(string));
            _requestOutRackInfoTable.Columns.Add("MDL_TP", typeof(string));
            _requestOutRackInfoTable.Columns.Add("SRC_NAME", typeof(string));
            _requestOutRackInfoTable.Columns.Add("DST_NAME", typeof(string));
            _requestOutRackInfoTable.Columns.Add("CURR_NAME", typeof(string));
            _requestOutRackInfoTable.Columns.Add("PRODID", typeof(string));
            _requestOutRackInfoTable.Columns.Add("MACRO_EXETIME", typeof(string));
        }

        private void InitializeRequestCleanerTable()
        {
            _requestCleanerInfoTable = new DataTable();
            _requestCleanerInfoTable.Columns.Add("PORT_ID", typeof(string));
            _requestCleanerInfoTable.Columns.Add("EQPTID", typeof(string));
            _requestCleanerInfoTable.Columns.Add("EQPTNAME", typeof(string));
            _requestCleanerInfoTable.Columns.Add("PORT_NAME", typeof(string));
            _requestCleanerInfoTable.Columns.Add("CSTID", typeof(string));
            _requestCleanerInfoTable.Columns.Add("CSTPROD", typeof(string));
            _requestCleanerInfoTable.Columns.Add("CSTPROD_NAME", typeof(string));
            _requestCleanerInfoTable.Columns.Add("CSTSTAT", typeof(string));
            _requestCleanerInfoTable.Columns.Add("CSTSTAT_NAME", typeof(string));
            _requestCleanerInfoTable.Columns.Add("UPDDTTM", typeof(DateTime));
        }

        private void InitializeGrid()
        {
            //dgRealPallet.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            //dgRealPallet.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

            //if (_isGradeJudgmentDisplay) dgRealPalletDetail.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
        }

        private void InitializeCombo()
        {
            // Area 콤보박스
            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");
            //SetAreaCombo(cboArea);

            SetFloorCombo(cboFloor);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // HOLD 사유 콤보박스
            SetHoldCombo(cboHoldReason);

            // QA불량유형
            SetFalutyTypeCombo(cboFaultyType);

            // 출고유형
            string[] sFilter2 = { "STK_ISS_TYPE", "UI" };
            _combo.SetCombo(cboShipType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODEATTRS");

            // MCS 서버의 출고 Port 콤보박스
            // SetOutPort(cboOutPort);

            string[] sFilter3 = { "STK_DST_TYPE" };
            _combo.SetCombo(cboDstType, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            SetEquipmentPopupFindCombo(popPalletId);
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
            _trayType = string.Empty;
            _emptyTrayState = string.Empty;
            _emptyClean = string.Empty;
            _equipmentCode = string.Empty;
            _electrodeTypeCode = string.Empty;
            _cstTypeCode = string.Empty;
            _abNormalReasonCode = string.Empty;
            //txtEquipmentName.Text = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;

            Util.gridClear(dgRealPallet);
            Util.gridClear(dgNoPallet);
            Util.gridClear(dgAbnormalPallet);
            Util.gridClear(dgErrorPallet);
            Util.gridClear(dgRealPalletDetail);
            Util.gridClear(dgNoPalletDetail);
            Util.gridClear(dgAbnormalPalletDetail);
            Util.gridClear(dgErrorPalletDetail);
            Util.gridClear(dgPortInfo);
            Util.gridClear(dgOutRacktDetail);

            _requestTransferInfoTable.Clear();
            _requestOutRackInfoTable.Clear();

            if (cboOutPort.SelectedItem != null && cboOutPort.Items.Count > 0)
            {
                SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
            }

            txtCheckingQtyData.Text = "0";
        }

        private void SetStockerCombo(C1ComboBox stockerMSB)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("BLDGCODE", typeof(string));
            dtRQSTDT.Columns.Add("FLOORNUM", typeof(string));

            DataRow drNewrow = dtRQSTDT.NewRow();
            drNewrow["LANGID"] = LoginInfo.LANGID;
            drNewrow["BLDGCODE"] = cboArea.SelectedValue.ToString();
            drNewrow["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);

            dtRQSTDT.Rows.Add(drNewrow);

            new ClientProxy().ExecuteService("DA_MCS_SEL_FORMATION_STO_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_MCS_SEL_FORMATION_STO_CBO", Exception.Message, Exception.ToString());
                    return;
                }

                DataTable dtTemp = new DataTable();
                dtTemp = result.Copy();
                //cbo.ItemsSource = DataTableConverter.Convert(dtTemp);

                //ComboStatus cs = ComboStatus
                stockerMSB.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "EQPTID", "EQPTNAME").Copy().AsDataView();
                stockerMSB.SelectedIndex = 0;
            }
            );
        }

        private void SetFloorCombo(C1ComboBox cboFloor)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("BLDGCODE", typeof(string));

                DataRow drNewrow = dtRQSTDT.NewRow();
                drNewrow["BLDGCODE"] = cboArea.SelectedValue.ToString();
                dtRQSTDT.Rows.Add(drNewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_FORMATION_STO_FLOOR_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboFloor.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                cboFloor.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SelectRealPallet(C1DataGrid dg)
        {
            try
            {
                if (cboArea.SelectedValue == null) return;

                ShowLoadingIndicator();

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("BLDGCODE", typeof(string));
                dt.Columns.Add("WH_ID", typeof(string));
                //dt.Columns.Add("PALLETID", typeof(string));
                dt.Columns.Add("PALLETID_LIST", typeof(string));
                //dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("CSTID_LIST", typeof(string));
                dt.Columns.Add("FLOORNUM", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BLDGCODE"] = cboArea.SelectedValue;
                //dr["WH_ID"] = cboArea.SelectedValue.ToString();
                dr["WH_ID"] = cboStocker.SelectedValue.ToString() == "" ? null : cboStocker.SelectedValue.ToString();
                //dr["PALLETID"] = string.IsNullOrWhiteSpace(txtPalletId.Text.Trim()) ? null : txtPalletId.Text.Trim();
                dr["PALLETID_LIST"] = string.IsNullOrWhiteSpace(txtPalletId.Text.Trim()) ? null : ConvertData(txtPalletId.Text.Trim());
                //dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : txtCarrierId.Text.Trim();
                dr["CSTID_LIST"] = string.IsNullOrWhiteSpace(txtCarrierId.Text.Trim()) ? null : ConvertData(txtCarrierId.Text.Trim());
                dr["FLOORNUM"] = Util.GetCondition(cboFloor, bAllNull: true);
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_MCS_SEL_STO_FORMATION_REALPALLET", "INDATA", "OUTDATA", dt, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    /*
                    if (result.Rows.Count > 0)
                    {
                        double Ship_cell_Qty = 0;
                        double Hold_cell_Qty = 0;
                        double Insp_wait_cell_Qty = 0;
                        double Ng_cell_Qty = 0;

                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            Ship_cell_Qty = Ship_cell_Qty + Convert.ToDouble(result.Rows[i]["SHIP_CELL_QTY"].ToString());
                            Hold_cell_Qty = Hold_cell_Qty + Convert.ToDouble(result.Rows[i]["HOLD_CELL_QTY"].ToString());
                            Insp_wait_cell_Qty = Insp_wait_cell_Qty + Convert.ToDouble(result.Rows[i]["INSP_WAIT_CELL_QTY"].ToString());
                            Ng_cell_Qty = Ng_cell_Qty + Convert.ToDouble(result.Rows[i]["NG_CELL_QTY"].ToString());

                            //txtRealCarrierCount.Text = String.Format("{0:#,##0}", Ship_cell_Qty + Hold_cell_Qty + Insp_wait_cell_Qty + Ng_cell_Qty);
                        }
                    }
                    else
                    {
                        //txtRealCarrierCount.Text = "0";
                    }
                    */

                    //OUT_PLT_CNT

                    result.Columns.Add("OUT_PLT_CNT", typeof(int));

                    foreach (DataRow drData in result.Rows)
                    {
                        DataRow[] drs = _requestOutRackInfoTable.Select("PRODID = '" + drData["PRODID"].ToString() + "' AND CARRIER_STRUCT = 'U'");
                        drData["OUT_PLT_CNT"] = drs.Length;
                    }

                    var query = result.AsEnumerable().GroupBy(x => new
                    { }).Select(g => new
                    {
                        ModelLotID = ObjectDic.Instance.GetObjectName("합계"),
                        ProjectName = string.Empty,
                        ShipPLTQty = g.Sum(x => x.Field<double>("SHIP_PLT_QTY")),
                        HoldPLTQty = g.Sum(x => x.Field<double>("HOLD_PLT_QTY")),
                        InspWaitPLTQty = g.Sum(x => x.Field<double>("INSP_WAIT_PLT_QTY")),
                        NGPLTQty = g.Sum(x => x.Field<double>("NG_PLT_QTY")),
                        VldDatePLTQty = g.Sum(x => x.Field<double>("VLD_DATE_PLT_QTY")),
                        ShipCellQty = g.Sum(x => x.Field<double>("SHIP_CELL_QTY")),
                        HoldCellQty = g.Sum(x => x.Field<double>("HOLD_CELL_QTY")),
                        InspWaitCellQty = g.Sum(x => x.Field<double>("INSP_WAIT_CELL_QTY")),
                        NGCellQty = g.Sum(x => x.Field<double>("NG_CELL_QTY")),
                        VldDateCellQty = g.Sum(x => x.Field<double>("VLD_DATE_CELL_QTY")),
                        OUT_PLT_CNT_Qty = g.Sum(x => x.Field<Int32>("OUT_PLT_CNT")),
                        ProdiId = string.Empty,
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = result.NewRow();
                        newRow["MDLLOT_ID"] = query.ModelLotID;
                        newRow["PRJT_NAME"] = query.ProjectName;
                        newRow["SHIP_PLT_QTY"] = query.ShipPLTQty;
                        newRow["HOLD_PLT_QTY"] = query.HoldPLTQty;
                        newRow["INSP_WAIT_PLT_QTY"] = query.InspWaitPLTQty;
                        newRow["NG_PLT_QTY"] = query.NGPLTQty;
                        newRow["VLD_DATE_PLT_QTY"] = query.VldDatePLTQty;
                        newRow["SHIP_CELL_QTY"] = query.ShipCellQty;
                        newRow["HOLD_CELL_QTY"] = query.HoldCellQty;
                        newRow["INSP_WAIT_CELL_QTY"] = query.InspWaitCellQty;
                        newRow["NG_CELL_QTY"] = query.NGCellQty;
                        newRow["VLD_DATE_CELL_QTY"] = query.VldDateCellQty;
                        newRow["OUT_PLT_CNT"] = query.OUT_PLT_CNT_Qty;
                        newRow["PRODID"] = query.ProdiId;
                        result.Rows.Add(newRow);
                    }

                    Util.GridSetData(dg, result, FrameOperation, true);

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private static void SetEquipmentPopupFindCombo(CMM001.PopupFindControl pop)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));

            dt.Rows.Add(new object[] { "AAA", "AAA_NAME" });
            dt.Rows.Add(new object[] { "BBB", "BBB_NAME" });
            dt.Rows.Add(new object[] { "CCC", "CCC_NAME" });

            pop.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void SetCheckedRealPalletInfoGrid()
        {
            try
            {
                if (Math.Abs(rowCount.Value) >= 0)
                {
                    int selectedCheckCount = (int)rowCount.Value;
                    int i = 0;

                    if (CommonVerify.HasDataGridRow(dgRealPalletDetail))
                    {
                        if (_util.GetDataGridRowCountByCheck(dgRealPalletDetail, "CHK") >= rowCount.Value) return;

                        foreach (C1.WPF.DataGrid.DataGridRow row in dgRealPalletDetail.Rows)
                        {
                            if (row.Type == DataGridRowType.Item)
                            {
                                if (i < selectedCheckCount)
                                {
                                    /*
                                    if (cboStockerType.SelectedValue.GetString() == "JRW")
                                    {
                                        if (DataTableConverter.GetValue(row.DataItem, "CWA_JRW_QMS").GetString() == "OK")
                                        {
                                            DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                            i++;
                                        }
                                    }
                                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                                    {
                                        if (DataTableConverter.GetValue(row.DataItem, "CWA_PCW_QMS").GetString() == "OK")
                                        {
                                            DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                        i++;
                                    }
                                    */

                                    dgRealPalletDetail.EndEdit();
                                    dgRealPalletDetail.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgRealPalletDetail, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgRealPalletDetail.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string selectedRackId = DataTableConverter.GetValue(dgRealPalletDetail.Rows[lastIdx].DataItem, "RACK_ID").GetString();
                            string selectedCstStat = DataTableConverter.GetValue(dgRealPalletDetail.Rows[lastIdx].DataItem, "CSTSTAT").GetString();
                            string selectedCstId = DataTableConverter.GetValue(dgRealPalletDetail.Rows[lastIdx].DataItem, "CSTID").GetString();
                            //txtEquipmentName.Text = DataTableConverter.GetValue(dgRealPalletDetail.Rows[lastIdx].DataItem, "EQPT_NAME").GetString();

                            SetOutPort(cboOutPort, selectedequipmentCode, selectedRackId, selectedCstStat, selectedCstId);
                            //SelectPortInfo(string.Empty, selectedequipmentCode, selectedRackId, selectedCstStat);
                        }
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
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
            {
                dg = dgNoPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
            {
                dg = dgAbnormalPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
            {
                dg = dgErrorPalletDetail;
            }
            else
            {
                dg = dgRealPalletDetail;
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

            if (cboShipType.SelectedItem == null || string.IsNullOrEmpty(cboShipType.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU4925", this.txtShipType.Text);
                return false;
            }

            if (cboOutPort.SelectedItem == null || string.IsNullOrEmpty((cboOutPort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            if ((cboOutPort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            {
                Util.MessageInfo("SFU8137");
                return false;
            }

            return true;
        }

        private bool ValidationClean()
        {
            C1DataGrid dg = dgNoPalletDetail;

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

        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;

            if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
            {
                dg = dgNoPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
            {
                dg = dgAbnormalPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
            {
                dg = dgErrorPalletDetail;
            }
            else
            {
                dg = dgRealPalletDetail;
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
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ORDID").GetString())
                          //                || !(DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() == "ORD_CREATE"
                          //|| DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() == "JOB_FAIL"
                          //|| DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() == "JOB_WAIT"
                          //|| DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() == "TRANSFER_WAIT")
                          )
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

            if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
            {
                dg = dgNoPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
            {
                dg = dgAbnormalPalletDetail;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
            {
                dg = dgErrorPalletDetail;
            }
            else
            {
                dg = dgRealPalletDetail;
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

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_BY_SHOP_CBO";
            string[] arrColumn = { "SYSTEM_ID", "LANGID", "SHOPID", "USERID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID, "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
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

        private void SetOutPort(C1ComboBox cbo, string equipmentCode, string rackId, string cstStat, string cstId = null)
        {
            try
            {
                cboOutPort.SelectedIndexChanged -= cboOutPort_SelectedIndexChanged;

                if (cbo.Items.Count > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                }

                if (cboDstType.SelectedValue.ToString().ToUpper().Equals("PORT"))
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PORT_TYPE_CODE_LIST", typeof(string));
                    inTable.Columns.Add("PORT_WRK_MODE", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = equipmentCode;
                    dr["PORT_TYPE_CODE_LIST"] = "'MGV'";        // MGV : 작업자가 대차로 Carrier를 투입/배출할 수 있는 포트
                                                                // MP  : Stocker에서 Rack에 적재된 Carrier를 확인하기 위한 포트
                    dr["PORT_WRK_MODE"] = 'M';

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);

                    foreach (DataRow row in dtResult.Rows)
                    {
                        C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                        comboBoxItem.Content = row["PORT_NAME2"].GetString();
                        //comboBoxItem.Tag = row["DST_LOC_GROUPID"].GetString();
                        comboBoxItem.Tag = equipmentCode + "|" + row["PORT_ID"].GetString();

                        cbo.Items.Add(comboBoxItem);
                    }
                }
                else
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("IS_EMPTY", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = cstId;
                    dr["IS_EMPTY"] = cstStat == "" ? null : cstStat;

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_LIST_FOR_MANUAL_OUTPUT", "INDATA", "OUTDATA", inTable);

                    foreach (DataRow row in dtResult.Rows)
                    {
                        C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                        comboBoxItem.Content = row["DST_PORTNAME"].GetString();
                        //comboBoxItem.Tag = row["DST_LOC_GROUPID"].GetString();
                        comboBoxItem.Tag = row["DST_EQPTID"].GetString() + "|" + row["DST_PORTID"].GetString();

                        cbo.Items.Add(comboBoxItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                cboOutPort.SelectedIndexChanged += cboOutPort_SelectedIndexChanged;

                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
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

        private void ViewCheckingQty(C1DataGrid dg)
        {
            int iSelectedCount = 0;
            DataTable dt = ((DataView)dg.ItemsSource).Table;
            //if (!dt.Columns.Contains("CHK")) return;

            iSelectedCount = dt.Select("CHK = 1").ToList<DataRow>().Count;
            this.txtCheckingQtyData.Text = iSelectedCount.ToString("#,##0");
        }

        private string ConvertData(string data)
        {
            string strReturn = string.Empty;

            string[] list = data.Split(',');
            foreach (string alist in list)
            {
                strReturn += string.Format("'{0}',", alist);
            }

            if (strReturn.Length > 0)
                strReturn = strReturn.Substring(0, strReturn.Length - 1);

            return strReturn;
        }
        #endregion

        private void cboDstType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboDstType.SelectedValue.ToString().ToUpper().Equals("PORT"))
                {
                    cboShipType.IsEnabled = true;
                    this.txtPortInfo.Text = ObjectDic.Instance.GetObjectName("목적지 포트 정보");
                    dgPortInfo.Visibility = Visibility.Visible;
                    dgStkInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cboShipType.IsEnabled = false;
                    this.txtPortInfo.Text = ObjectDic.Instance.GetObjectName("목적지 창고 정보");
                    dgPortInfo.Visibility = Visibility.Collapsed;
                    dgStkInfo.Visibility = Visibility.Visible;
                }

                C1DataGrid dg;

                if (string.Equals(_selectedRadioButtonValue, "NOPALLET"))
                {
                    dg = dgNoPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALPALLET"))
                {
                    dg = dgAbnormalPalletDetail;
                }
                else if (string.Equals(_selectedRadioButtonValue, "ERRORPALLET"))
                {
                    dg = dgErrorPalletDetail;
                }
                else
                {
                    dg = dgRealPalletDetail;
                }

                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                if (idx >= 0)
                {
                    C1.WPF.DataGrid.DataGridRow row = dg.Rows[idx];
                    string currentequipmentCode = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                    string currentRackId = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                    string currentequipmentName = DataTableConverter.GetValue(row.DataItem, "EQPTNAME").GetString();
                    string checkValue = DataTableConverter.GetValue(row.DataItem, "CHK").GetString();
                    string currentCstStat = string.Empty;

                    if (dg != dgErrorPalletDetail)
                    {
                        currentCstStat = DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
                    }

                    var query = (from t in ((DataView)dg.ItemsSource).Table.AsEnumerable()
                                 where t.Field<Int64>("CHK") == 1
                                 select t).ToList();

                    if (checkValue != "0")
                    {
                        if (query.Count() > 0)
                        {
                            SetOutPort(cboOutPort, currentequipmentCode, currentRackId, currentCstStat);
                            //SelectPortInfo(string.Empty, currentequipmentCode, currentRackId, currentCstStat);
                            //txtEquipmentName.Text = currentequipmentName;
                        }
                    }
                }
                else
                {
                    SetOutPort(cboOutPort, string.Empty, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
    }
}