/*************************************************************************************
 Created Date : 2022.06.30
      Creator : 오화백
   Decription : 창고 수동출고 
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.29  오화백 과장 : Initial Created.    
 
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
    /// MCS001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_082 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private double _maxcheckCount = 0;
        private string _selectedRadioButtonValue;
        
        private string _pastDay;
        private string _MtrlID;
        private string _Mtrl_Lot;
        private string _faultyType;
   
        private string _equipmentCode;
        private string _electrodeTypeCode;
        private string _cstTypeCode;
        private string _abNormalReasonCode;
        private string _selectedWipHold;

        private string _dst_eqptID;
        private string _dst_portID;

        private bool _isAdminAuthority;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

       


        public MCS001_082()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 권한
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
           
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);
        }

        /// <summary>
        /// 화면 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitializeCombo();
            rdoFiFo.IsChecked = true;
            rdoPort.IsChecked = true;
            SelectReleaseCount();
            if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == "MWW")
            {
                rdoEmptyCarrier.Visibility = Visibility.Collapsed;
            }
            else
            {
                rdoEmptyCarrier.Visibility = Visibility.Visible;
            }
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

   
        /// <summary>
        ///  그리드 셋팅
        /// </summary>
        private void InitializeGrid()
        {
            dgStore.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStore.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);
         
          

        }
        /// <summary>
        /// 콤보 셋팅
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


        }
        #endregion

        #region Event

        #region 창고 유형 콤보 이벤트  : cboStockerType_SelectedValueChanged()

        /// <summary>
        /// 창고유형
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboStockerType.SelectedValue.ToString() == "MWW")
            {
                rdoEmptyCarrier.Visibility = Visibility.Collapsed;
                if(rdoEmptyCarrier.IsChecked == true)
                {
                    rdoEmptyCarrier.IsChecked = false;
                    rdoFiFo.IsChecked = true;
                }

            }
            else
            {
                rdoEmptyCarrier.Visibility = Visibility.Visible;
            }

            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        #endregion

        #region 극성 콤보 이벤트  : cboElectrodeType_SelectedValueChanged()
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        #endregion

        #region 창고 콤보 이벤트 : cboStocker_SelectedValueChanged()
        /// <summary>
        /// 창고 콤보
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }

        #endregion

        #region 조회 버튼 : btnSearch_Click()
        /// <summary>
        /// 조회 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            // 공캐리어 조회
            if (rdoEmptyCarrier.IsChecked == true)
            {
                SelectWareHouseEmptyCarrier();
            }
            //오류 Rack 조회
            else if (rdoNoReadCarrier.IsChecked == true)
            {
                SelectWareHouseNoReadCarrier();
            }
            //정보 불일치 조회
            else if (rdoAbNormalCarrier.IsChecked == true)
            {
                SelectWareHouseAbNormalCarrier();
            }
            //FIFO, 조건출고 조회
            else
            {
                SelectManualOutInventory();
            }
        }

        #endregion

        #region 수동출고 버튼 : btnManualIssue_Click()
        /// <summary>
        /// 수동출고 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            SaveManualIssueByEsnb();

        }

        #endregion

        #region 수동출고 취소버튼 : btnTransferCancel_Click()
        /// <summary>
        /// 수동출고 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;

            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;
            C1DataGrid dg;
            string _Carrierid = string.Empty;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
                _Carrierid = "CARRIERID";
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
                _Carrierid = "CARRIERID";
            }
            else
            {
                dg = dgIssueTargetInfo;
                _Carrierid = "CARRIERID";
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
                    newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                    inTable.Rows.Add(newRow);
                }
            }

            CMM_MHS_TRANSFER_CANCEL popupTransferCancel = new CMM_MHS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = inTable;
            parameters[1] = string.Empty;
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        #endregion

        #region 수동출고 취소 팝업 닫기 : popupTransferCancel_Closed()
        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MHS_TRANSFER_CANCEL popup = sender as CMM_MHS_TRANSFER_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
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


        #endregion

        #region 유형 Radio 버튼 클릭 : rdoRelease_Checked()
        /// <summary>
        /// 유형 Radio 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            txtReturnSrc.Visibility = Visibility.Visible;
            rdoPort.Visibility = Visibility.Visible;
            rdoWareHouse.Visibility = Visibility.Visible;
            txtSrc.Visibility = Visibility.Visible;
            cboIssuePort.Visibility = Visibility.Visible;
            btnManualIssue.Visibility = Visibility.Visible;
            btnTransferCancel.Visibility = Visibility.Visible;
            
            Left.Visibility = Visibility.Visible;
            Left_NoRead.Visibility = Visibility.Collapsed;

            dgStore.Columns["LOT_HOLD_QTY"].Visibility = Visibility.Collapsed;
            dgStore.Columns["LOT_TOTAL_QTY"].Visibility = Visibility.Collapsed;


            switch (radioButton.Name)
            {
                case "rdoFiFo":
                    spFiFo.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "FIFO";


                    break;
                case "rdoCondition":
                    spCondition.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    dgStore.Columns["LOT_HOLD_QTY"].Visibility = Visibility.Visible;
                    dgStore.Columns["LOT_TOTAL_QTY"].Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NORMAL";

                    break;
                case "rdoEmptyCarrier":
                    dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Visible;
                    dgStoreByEmptyCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "EMPTYCARRIER";

                    break;
                case "rdoNoReadCarrier":
                    dgStoreByNoReadCarrier.Visibility = Visibility.Visible;
                    dgIssueTargetInfoByNoReadCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NOREADCARRIER";
                    // if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;

                    btnManualIssue.Visibility = Visibility.Collapsed;
                    btnTransferCancel.Visibility = Visibility.Collapsed;
                    txtReturnSrc.Visibility = Visibility.Collapsed;
                    rdoPort.Visibility = Visibility.Collapsed;
                    rdoWareHouse.Visibility = Visibility.Collapsed;
                    txtSrc.Visibility = Visibility.Collapsed;
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    cboIssueWareHouse.Visibility = Visibility.Collapsed;

                    Left.Visibility = Visibility.Collapsed;
                    Left_NoRead.Visibility = Visibility.Visible;

                    break;
                case "rdoAbNormalCarrier":
                    dgStoreByAbNormalCarrier.Visibility = Visibility.Visible;
                    dgIssueTargetInfoByAbNormalCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "ABNORMALCARRIER";

                    break;
            }

            ClearControl();
            //SetStockerTypeCombo(cboStockerType);
            //SetStockerCombo(cboStocker);

        }

        #endregion

        #region FIFO 출고 선택시  출고 수량  이벤트  : rowCount_LostFocus(), rowCount_ValueChanged()

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

        #endregion

        #region FIFO 출고, 조건출고  집계 리스트 이벤트 : dgStore_LoadedCellPresenter(), dgStore_UnloadedCellPresenter(),dgStore_MouseLeftButtonUp()

        /// <summary>
        ///  리스트 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "GRADE")), ObjectDic.Instance.GetObjectName("합계")))
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
        ///  리스트 색깔 처리
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
        /// <summary>
        /// 창고 재고 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                _pastDay = null;
                _MtrlID = null;
                _Mtrl_Lot = null;
                _faultyType = null;
                _equipmentCode = null;
                _electrodeTypeCode = null;
                _selectedWipHold = null;



                if (!string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "GRADE").GetString()))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                 }
                else
                {
                    _equipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()) ? null : cboStocker.SelectedValue.GetString();

                }
                if (cell.Column.Name.Equals("LOT_QTY"))
                {
                    _selectedWipHold = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY"))
                {
                    _selectedWipHold = "Y";
                
                }

                _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

                _MtrlID = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "MTRLID").GetString()) ? null : DataTableConverter.GetValue(drv, "MTRLID").GetString();
            
                if (!string.Equals(_selectedRadioButtonValue, "FIFO"))
                {
                    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                    _Mtrl_Lot = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
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

        private void dgIssueTargetInfo_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgIssueTargetInfo.ItemsSource == null) return;

                SelectPortInfo(string.Empty);
                txtEquipmentName.Text = string.Empty;
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);
                DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 공 Carrier 출고 집계 리스트 이벤트 : dgStoreByEmptyCarrier_LoadedCellPresenter(), dgStoreByEmptyCarrier_UnloadedCellPresenter(), dgStoreByEmptyCarrier_MouseLeftButtonUp()

        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD")), ObjectDic.Instance.GetObjectName("합계")))
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
                    else
                    {
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

                }
            }));
        }
        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        ///  창고 재고 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

          
          
                _electrodeTypeCode = string.Empty;

                if (DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                {
                    _electrodeTypeCode = null;
            
             

                }
                else
                {
                    if (cell.Column.Name.Equals("ELTR_TYPE_NAME"))
                    {
                        _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                   
                    }
                    else if (cell.Column.Name.Equals("CSTPROD"))
                    {
                   
                        _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }
                    else
                    {
                  
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

        #endregion

        #region 정보 불일치  집계 리스트 이벤트 : dgStoreByAbNormalCarrier_LoadedCellPresenter(), dgStoreByAbNormalCarrier_UnloadedCellPresenter(), dgStoreByAbNormalCarrier_MouseLeftButtonUp()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
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

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 창고데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (DataTableConverter.GetValue(drv, "EQPTNAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
            {
            }
            else
            {
                if (cell.Column.Name.Equals("EQPTNAME"))
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

        #endregion

        #region 비정상 RACK 집계 리스트 이벤트 : dgStoreByNoReadCarrier_LoadedCellPresenter(), dgStoreByNoReadCarrier_UnloadedCellPresenter(), dgStoreByNoReadCarrier_MouseLeftButtonUp()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
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

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 창고데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (DataTableConverter.GetValue(drv, "EQPTNAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                _cstTypeCode = DataTableConverter.GetValue(drv, "CST_TYPE_CODE").GetString();
            }

            txtEquipmentName.Text = string.Empty;
            SelectWareHouseNoReadCarrierList();

        }

        #endregion

        #region  조건출고 리스트 조회 조건 : cboHoldReason_SelectedValueChanged(), txtLotId_KeyDown(), txtPastDay_KeyDown(), cboFaultyType_SelectedValueChanged()

        /// <summary>
        ///  HOLD 사유
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboHoldReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (!string.IsNullOrEmpty(_projectName))
            //{
            //    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
            //    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
            //    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
            //    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

            //    SelectManualOutInventoryList();
            //}
        }
        /// <summary>
        /// 자재 아이디
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "CARRIERID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }

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
        /// <summary>
        /// 경과일수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPastDay_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _Mtrl_Lot = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectManualOutInventoryList();
            }
        }
        /// <summary>
        /// QA불량유형
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboFaultyType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (!string.IsNullOrEmpty(_projectName))
            //{
            //    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
            //    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
            //    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
            //    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

            //    SelectManualOutInventoryList();
            //}
        }

        #endregion

        #region  FIFO 출고, 조건 출고 리스트 이벤트  : dgIssueTargetInfo_LoadedCellPresenter(), dgIssueTargetInfo_UnloadedCellPresenter(),dgIssueTargetInfo_BeginningEdit(),  dgIssueTargetInfo_MouseDoubleClick()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    if (Convert.ToString(e.Cell.Column.Name) == "HOLD_YN")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetString() == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "VLD_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() <= 7)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
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

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 리스트 내 체크박스 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfo;


                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
   

        #endregion

        #region 공 Carrier 출고 리스트 이벤트 : dgIssueTargetInfoByEmptyCarrier_BeginningEdit(), dgIssueTargetInfoByEmptyCarrier_MergingCells()
        /// <summary>
        /// 공 Carrier 출고 첵크 박스 체크시
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

       
        #endregion

        #region 정보 불일치 리스트 이벤트 : dgIssueTargetInfoByAbNormalCarrier_BeginningEdit(), dgIssueTargetInfoByAbNormalCarrier_MergingCells()
        /// <summary>
        ///  정보불일치 첵크 박스 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


       



        #endregion

        #region Port 리스트 이벤트 : dgPortInfo_LoadedCellPresenter(), dgPortInfo_UnloadedCellPresenter()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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



        #endregion

        #region 목적지 콤보박스 이벤트 : cboIssuePort_SelectedIndexChanged()
        /// <summary>
        /// 목적지 콤보박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    if (rdoPort.IsChecked == true)
                    {
                        _dst_eqptID = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).DataContext.GetString();
                    }
                    else
                    {
                        _dst_portID = ((ContentControl)(cboIssueWareHouse.Items[currentRowIndex])).DataContext.GetString();
                    }

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
        #endregion

        #region Timer 셋팅 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// 타이머 셋 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region PORT 리스트 및 목적지 콤보박스 조회  : SelectWareHousePortInfo()
        /// <summary>
        /// PORT 리스트 및 목적지 콤보박스 조회
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="e"></param>
        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPTNAME" : "EQPTNAME").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrier = DataTableConverter.GetValue(e.Row.DataItem, "CARRIERID").GetString();
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
                           //where t.Field<int>("CHK") == 1 // 2024.11.20. 김영국 - DBType 문제로 인하여 Type변경. int -> long
                             where t.Field<long>("CHK") == 1
                             select t).ToList();

                if (checkValue == "0")
                {
                    if (query.Count() < 1)
                    {

                        if (rdoPort.IsChecked == true)
                        {
                            SetIssuePort(cboIssuePort, currentequipmentCode);
                            SelectPortInfo(currentequipmentCode);
                            txtEquipmentName.Text = currentequipmentName;
                        }
                        else
                        {

                            SetWareHouse(cboIssueWareHouse, carrier);
                        }



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
                        if (cboIssueWareHouse.SelectedItem != null && cboIssueWareHouse.Items.Count > 0)
                        {
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                        }
                    }
                }
            }
        }

        #endregion

        #region LOTID 텍스트 박스 이벤트 : txtLotId_PreviewKeyDown()
        /// <summary>
        /// 랏 아이디 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "CARRIERID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }


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

        #endregion

        #region 반송 목적지 Radio 버튼 이벤트 : rdoType_Checked()
        private void rdoType_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;
            switch (radioButton.Name)
            {

                case "rdoPort":
                    cboIssuePort.Visibility = Visibility.Visible;
                    cboIssueWareHouse.Visibility = Visibility.Collapsed;
                    int lastIdx = -1;
                    if (rdoFiFo.IsChecked == true || rdoCondition.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");
                    }
                    else if (rdoEmptyCarrier.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByEmptyCarrier, "CHK");
                    }
                    else if (rdoAbNormalCarrier.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByAbNormalCarrier, "CHK");
                    }

                    if (lastIdx >= 0)
                    {
                        string selectedequipmentCode = string.Empty;
                        if (rdoFiFo.IsChecked == true || rdoCondition.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                        }
                        else if (rdoEmptyCarrier.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx].DataItem, "EQPTNAME").GetString();

                        }
                        else if (rdoAbNormalCarrier.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx].DataItem, "EQPTNAME").GetString();

                        }

                        SetIssuePort(cboIssuePort, selectedequipmentCode);
                        SelectPortInfo(selectedequipmentCode);
                    }

                    break;
                case "rdoWareHouse":
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    cboIssueWareHouse.Visibility = Visibility.Visible;
                    Util.gridClear(dgPortInfo);
                    txtEquipmentName.Text = string.Empty;
                    int lastIdx1 = -1;

                    if (rdoFiFo.IsChecked == true || rdoCondition.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");
                    }
                    else if (rdoEmptyCarrier.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByEmptyCarrier, "CHK");
                    }
                    else if (rdoAbNormalCarrier.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByAbNormalCarrier, "CHK");
                    }



                    if (lastIdx1 >= 0)
                    {
                        string carrier = string.Empty;

                        if (rdoFiFo.IsChecked == true || rdoCondition.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx1].DataItem, "CARRIERID").GetString();

                        }
                        else if (rdoEmptyCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx1].DataItem, "CARRIERID").GetString();

                        }
                        else if (rdoAbNormalCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx1].DataItem, "CSTID").GetString();

                        }
                        SetWareHouse(cboIssueWareHouse, carrier);

                    }

                    break;

            }

        }

        #endregion

        #endregion

        #region Method

        #region FIFO 출고, 조건출고  집계 및 리스트 조회 : SelectManualOutInventory(), SelectManualOutInventoryList()

        /// <summary>
        /// FIFO 출고, 조건출고 집계 조회
        /// </summary>
        private void SelectManualOutInventory()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_MTRL";
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
                dr["FIFO"] = _selectedRadioButtonValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    decimal LotTotalCount = 0;
                    decimal LotCount = 0;
                    decimal LotHoldCount = 0;
                
                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            LotTotalCount = LotTotalCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_TOTAL_QTY"]);
                            LotCount = LotCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_QTY"]);
                            LotHoldCount = LotHoldCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_HOLD_QTY"]);
                           
                        }

                        DataRow newRow = bizResult.NewRow();
                        newRow["GRADE"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["ELTR_TYPE_CODE"] = string.Empty;
                        newRow["ELTR_TYPE_NAME"] = string.Empty;
                        newRow["EQPTID"] = string.Empty;
                        newRow["EQPTNAME"] = string.Empty;
                        newRow["MTRLID"] = string.Empty;
                        newRow["LOT_TOTAL_QTY"] = LotTotalCount;
                        newRow["LOT_QTY"] = LotCount;
                        newRow["LOT_HOLD_QTY"] = LotHoldCount;
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
        /// <summary>
        ///  FIFO 출고, 조건출고 리스트 조회
        /// </summary>
        /// <param name="isRefresh"></param>
        private void SelectManualOutInventoryList(bool isRefresh = false)
        {
            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("MLOT_ID", typeof(string));
                inTable.Columns.Add("PAST_DAY", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));
                inTable.Columns.Add("MTRL_DFCT_FLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _equipmentCode;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["MTRLID"] = _MtrlID;
                dr["PAST_DAY"] = _pastDay;
                dr["MLOT_ID"] = _Mtrl_Lot;
                dr["FIFO"] = _selectedRadioButtonValue;
                dr["MTRL_DFCT_FLAG"] = _selectedWipHold;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);


                    if (!isRefresh)
                    {
                        if (CommonVerify.HasTableRow(bizResult) && string.Equals(_selectedRadioButtonValue, "FIFO"))
                        {
                            rowCount_ValueChanged(rowCount, null);
                        }
                        else
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                        }
                    }

                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);


                    if (!isRefresh)
                    {
                        if (CommonVerify.HasTableRow(bizResult) && string.Equals(_selectedRadioButtonValue, "FIFO"))
                        {
                            rowCount_ValueChanged(rowCount, null);
                        }
                        else
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                        }
                    }
             

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region 공캐리어 집계 및 리스트 조회 : SelectWareHouseEmptyCarrier(), SelectWareHouseEmptyCarrierList()

        /// <summary>
        /// 공캐리어 집계 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST_MTRL";
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
                        BobinCount = g.Sum(x => x.Field<Int32>("BBN_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                        newRow["ELTR_TYPE_NAME"] = query.ElectrodeTypeName;
                        newRow["BBN_QTY"] = query.BobinCount;
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

        /// <summary>
        /// 공캐리어 리스트 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrierList()
        {

            Util.gridClear(dgPortInfo);
          
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_EMPTY_CST_MTRL";
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
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["EQPTID"] = _selectedRadioButtonValue.Equals("EMPTYCARRIER") ? cboStocker.SelectedValue : _equipmentCode;
            

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);

                    SetIssuePort(cboIssuePort, string.Empty);
                    SetWareHouse(cboIssueWareHouse, string.Empty);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 비정상 Rack 집계 및 리스트 조회 : SelectWareHouseNoReadCarrier(),   SelectWareHouseNoReadCarrierList()

        /// <summary>
        /// 비정상 Rack 집계 조회
        /// </summary>
        private void SelectWareHouseNoReadCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_ABNORM_RACK";
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
                        newRow["EQPTNAME"] = query.EquipmentName;
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

        /// <summary>
        /// 비정상 Rack 리스트 조회
        /// </summary>
        private void SelectWareHouseNoReadCarrierList()
        {
            ShowLoadingIndicator();
            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_RACK";

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

                Util.GridSetData(dgIssueTargetInfoByNoReadCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);

            });

        }


        #endregion

        #region 정보 불일치 집계 및 리스트 조회 : SelectWareHouseAbNormalCarrier(), SelectWareHouseAbNormalCarrierList()

        /// <summary>
        /// 정보불일치 집계 조회
        /// </summary>
        private void SelectWareHouseAbNormalCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_ABNORM_CST_MTRL";
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
                        newRow["EQPTNAME"] = query.EquipmentName;
                        newRow["CST_QTY"] = query.SkidCount;
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



        /// <summary>
        /// 정보 불일치 리스트 조회
        /// </summary>
        private void SelectWareHouseAbNormalCarrierList()
        {
            ShowLoadingIndicator();

            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_CST_MTRL";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
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

                Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);


                if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                {
                    bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                    bizResult.Columns.Add("CARRIERID", typeof(string));
                    bizResult.Columns.Add("REQ_TRFID", typeof(string));
                }

              

                Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);


            });
        }
        #endregion

        #region 출고 수량 셋팅 : SelectReleaseCount()
        /// <summary>
        /// 출고 수량 셋팅  공통코드 관리
        /// </summary>
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
        #endregion

        #region PORT 리스트 조회  : SelectPortInfo()
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

                new ClientProxy().ExecuteService("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
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

        #endregion

        #region 수동 출고 처리 : SaveManualIssueByEsnb()
        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI_2";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("SRC_PORTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                C1DataGrid dg;
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                    _Carrierid = "CARRIERID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dg = dgIssueTargetInfoByAbNormalCarrier;
                    _Carrierid = "CARRIERID";
                }
                else
                {
                    dg = dgIssueTargetInfo;
                    _Carrierid = "CARRIERID";
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_EQPTID").GetString();
                        newRow["SRC_PORTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_PORTID").GetString();

                        if (rdoPort.IsChecked == true)
                        {
                            newRow["DST_EQPTID"] = _dst_eqptID;
                            newRow["DST_PORTID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                        }
                        else
                        {
                            newRow["DST_EQPTID"] = (cboIssueWareHouse.SelectedItem as C1ComboBoxItem).Tag.GetString();
                            newRow["DST_PORTID"] = _dst_portID;
                        }

                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        inTable.Rows.Add(newRow);
                    }
                }

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

        #endregion

        #region 사용자 권한 조회 : IsAdminAuthorityByUserId()
        /// <summary>
        /// 사용자 권한
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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

        #endregion

   

        #region  초기화 : ClearControl()
        private void ClearControl()
        {
         
          
        
            _pastDay = string.Empty;
            _MtrlID = string.Empty;
            _Mtrl_Lot = string.Empty;
            _faultyType = string.Empty;
      
       
            _equipmentCode = string.Empty;
            _electrodeTypeCode = string.Empty;
            _cstTypeCode = string.Empty;
            _abNormalReasonCode = string.Empty;
            txtEquipmentName.Text = string.Empty;
         
   

         
            Util.gridClear(dgStore);
            Util.gridClear(dgStoreByEmptyCarrier);
            Util.gridClear(dgStoreByNoReadCarrier);
            Util.gridClear(dgStoreByAbNormalCarrier);
            Util.gridClear(dgIssueTargetInfo);
            Util.gridClear(dgIssueTargetInfoByEmptyCarrier);
            Util.gridClear(dgIssueTargetInfoByNoReadCarrier);
            Util.gridClear(dgIssueTargetInfoByAbNormalCarrier);
            Util.gridClear(dgPortInfo);

       

            if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
            {
                SetIssuePort(cboIssuePort, string.Empty);
            }
            if (cboIssueWareHouse.SelectedItem != null && cboIssueWareHouse.Items.Count > 0)
            {
                SetWareHouse(cboIssueWareHouse, string.Empty);
            }
        }

        #endregion

        #region FIFO 출고 선택시 출고수량에 따라 선택 :  SetCheckedIssueTargetInfoGrid()
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
                                    DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                    i++;
                                    dgIssueTargetInfo.EndEdit();
                                    dgIssueTargetInfo.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "CARRIERID").GetString();

                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 수동출고 Validation  : ValidationManualIssue(), ValidationTransferCancelByEsnb()
        /// <summary>
        /// 기본 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationManualIssue()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
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

            if (rdoPort.IsChecked == true)
            {
                if (cboIssuePort.SelectedItem == null || string.IsNullOrEmpty((cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
                {
                    Util.MessageValidation("MCS1004");
                    return false;
                }
            }
            else
            {
                if (cboIssueWareHouse.SelectedItem == null || string.IsNullOrEmpty((cboIssueWareHouse.SelectedItem as C1ComboBoxItem).Tag.GetString()))
                {
                    Util.MessageValidation("MCS1004");
                    return false;
                }
            }

            if ((cboIssuePort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            {
                Util.MessageInfo("SFU8137");
                return false;
            }

            return true;
        }


        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    _Carrierid = "CARRIERID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    _Carrierid = "CARRIERID";
                }
                else
                {
                    _Carrierid = "CARRIERID";
                }
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
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


        #endregion

        #region 수동출고 취소 Validation : ValidationTransferCancel()
        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
            }
            else
            {
                dg = dgIssueTargetInfo;
            }
            int test = dg.Rows.Count;
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

            if (!ValidationTransferCancelByEsnb(dg))
            {
                return false;
            }

            return true;


        }

        #endregion

        #region 창고유형 콤보 조회 : SetStockerTypeCombo()
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            string attribute1, attribute2;

            if (_selectedRadioButtonValue == "FIFO" || _selectedRadioButtonValue == "NORMAL" || _selectedRadioButtonValue == "NOREADCARRIER")
            {
                attribute1 = "Y";
                attribute2 = null;
            }
            else
            {
                attribute1 = null;
                attribute2 = "Y";
            }

            const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, attribute1, attribute2, "AREA_EQUIPMENT_MTRL_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);

        }

        #endregion

        #region 창고 콤보 조회 : SetStockerCombo()
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "DA_MHS_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region 극성 콤보 조회 : SetElectrodeTypeCombo()
        private static void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region HOLD 사유 콤보 조회 : SetHoldCombo()
        private static void SetHoldCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ACTIVITIREASON_CBO";
            string[] arrColumn = { "LANGID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, "HOLD_LOT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region QA 불량 유형 콤보 조회 : SetFalutyTypeCombo()
        private static void SetFalutyTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "VD_RESN_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);


        }

        #endregion

        #region 출고 PORT 콤보 조회 : SetIssuePort()
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
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);
                //cbo.ItemsSource = dtResult.AsEnumerable();
                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["PORT_ID"].GetString();
                    comboBoxItem.Name = row["TRF_STAT_CODE"].GetString();

                    comboBoxItem.DataContext = row["DST_EQPTID"].GetString();



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

        #endregion

        #region 대상 창고 콤보 조회 : SetWareHouse()
        private void SetWareHouse(C1ComboBox cbo, string carrier)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;
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

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("IS_EMPTY", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrier;
                dr["IS_EMPTY"] = _selectedRadioButtonValue == "EMPTYCARRIER" ? "Y" : "N";

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_LIST", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["DST_PORTNAME"].GetString();
                    comboBoxItem.Tag = row["DST_PORTID"].GetString();
                    comboBoxItem.Name = row["DST_EQPTID"].GetString();
                    comboBoxItem.DataContext = row["DST_PORTID"].GetString();

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

        #endregion

        #region Timer 관련 : TimerSetting(), _dispatcherTimer_Tick()

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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }


        #endregion

        #region 프로그래스 바  : ShowLoadingIndicator()
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








        #endregion

        #endregion

      
    }
}
