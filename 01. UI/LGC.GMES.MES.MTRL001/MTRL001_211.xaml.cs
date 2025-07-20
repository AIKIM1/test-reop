/*************************************************************************************
 Created Date : 2024.09.11
      Creator : 오화백
   Decription : STK 출고 지시서 
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.11  오화백 과장 : Initial Created.    
  2025.04.30  이민형 차장 : [MI2_OSS_0220] R 권한을 가진 사용자에게 조회 버튼까지 숨김 처리됨 btnSearch 를 설정 목록에서 제거
  2025.05.26  이민형 차장 : [MI_LS_OSS_0123] 자재창고 - 자재STO 수동출고 에서 목적지 창고에 따라 Port정보 표기
  2025.07.11  이민형 차장 : [MI2_OSS_0409] 자재창고 - 자재STO 수동출고 창고Combo 동별코드ATTR4->ATTR5 변경
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

namespace LGC.GMES.MES.MTRL001
{
    /// <summary>
    /// MTRL001_211.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MTRL001_211 : UserControl, IWorkArea
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
     
        private bool _isAdminAuthority;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

       


        public MTRL001_211()
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
            List<Button> listAuth = new List<Button> { btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitializeCombo();
            rdoCarrier.IsChecked = true;
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

            // 목적지창고
            SetTagetCombo(cboTagetStk);




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
                    rdoCarrier.IsChecked = true;
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
            ChkOutput();
            //SaveManualIssueByEsnb();

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
           
            try
            {
                if (!ValidationTransferCancel()) return;

                //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;
                C1DataGrid dg;
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                    _Carrierid = "MTRL_CSTID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dg = dgIssueTargetInfoByAbNormalCarrier;
                    _Carrierid = "MTRL_CSTID";
                }
                else
                {
                    dg = dgIssueTargetInfo;
                    _Carrierid = "MTRL_CSTID";
                }

                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_TRF_CMD_CANCEL";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

             
                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
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
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

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

            //txtReturnSrc.Visibility = Visibility.Collapsed;

            txtReturnSrc.Visibility = Visibility.Visible;
            cboTagetStk.Visibility = Visibility.Visible;
            txtSrc.Visibility = Visibility.Visible;
            cboIssuePort.Visibility = Visibility.Visible;
            btnManualIssue.Visibility = Visibility.Visible;
            btnTransferCancel.Visibility = Visibility.Visible;
            
            Left.Visibility = Visibility.Visible;
            Left_NoRead.Visibility = Visibility.Collapsed;

            dgStore.Columns["HOLD_QTY_1"].Visibility = Visibility.Collapsed;
            dgStore.Columns["TOTAL_QTY"].Visibility = Visibility.Collapsed;


            switch (radioButton.Name)
            {
                   //break;
                case "rdoCarrier":
                    spCondition.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    dgStore.Columns["HOLD_QTY_1"].Visibility = Visibility.Visible;
                    dgStore.Columns["TOTAL_QTY"].Visibility = Visibility.Visible;
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
                    txtSrc.Visibility = Visibility.Collapsed;
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    Left.Visibility = Visibility.Collapsed;
                    Left_NoRead.Visibility = Visibility.Visible;
                    cboTagetStk.Visibility = Visibility.Collapsed;

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
                    string[] columnStrings = new[] { "TOTAL_QTY", "AVAIL_QTY_1", "HOLD_QTY_1" };

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
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                 }
                else
                {
                    _equipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()) ? null : cboStocker.SelectedValue.GetString();

                }
                if (cell.Column.Name.Equals("AVAIL_QTY_1"))
                {
                    _selectedWipHold = "N";
                }
                else if (cell.Column.Name.Equals("HOLD_QTY_1"))
                {
                    _selectedWipHold = "Y";
                
                }

                _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

                _MtrlID = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "MATERIAL_CD").GetString()) ? null : DataTableConverter.GetValue(drv, "MATERIAL_CD").GetString();
            
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
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NM")), ObjectDic.Instance.GetObjectName("합계")))
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
                _equipmentCode = string.Empty;


                if (!string.Equals(ObjectDic.Instance.GetObjectName(""), DataTableConverter.GetValue(drv, "EQPT_NM").GetString()))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                }
                else
                {
                    _equipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()) ? null : cboStocker.SelectedValue.GetString();

                }


                if (DataTableConverter.GetValue(drv, "ELTR_TYPE_NM").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                {
                    _electrodeTypeCode = null;
            
             

                }
                else
                {
                    if (cell.Column.Name.Equals("ELTR_TYPE_NM"))
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
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CNT").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NM")), ObjectDic.Instance.GetObjectName("합계")))
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

            if (DataTableConverter.GetValue(drv, "EQPT_NM").GetString() == ObjectDic.Instance.GetObjectName("합계"))
            {
            }
            else
            {
                if (cell.Column.Name.Equals("EQPT_NM"))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                }
                else
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
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
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CNT").GetInt() > 0)
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

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NM")), ObjectDic.Instance.GetObjectName("합계")))
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

            if (DataTableConverter.GetValue(drv, "EQPT_NM").GetString() != ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                _cstTypeCode = DataTableConverter.GetValue(drv, "RACK_ID").GetString();
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
                        
                         if (txtLotId.Text.ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "MLOT_ID")).GetString() || txtLotId.Text.ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "PLLT_ID")).GetString())
                            {
                                DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 1);

                                dgIssueTargetInfo.EndEdit();
                                dgIssueTargetInfo.EndEditRow(true);
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_ID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "MTRL_CSTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NM").GetString();
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
                    else if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
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
                   
                    string transferStateCode = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Name.GetString();
                    _dst_eqptID = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).DataContext.GetString();

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
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPT_ID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPT_NM" : "EQPT_NM").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrier = DataTableConverter.GetValue(e.Row.DataItem, "MLOT_ID").GetString();
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_ID").GetString();
                    if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EQPT_ID")).Equals(selectedequipmentCode))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                DataTable dt = ((DataView)dg.ItemsSource).Table;
                int chkCount = 0;
                for(int i=0; i< dt.Rows.Count; i++)
                {
                    if(dt.Rows[i]["CHK"].ToString() == "1")
                    {
                        chkCount = chkCount + 1;
                    }
                }

                if (checkValue == "0")
                {
                    if (chkCount < 1)
                    {

                        SetIssuePort(cboIssuePort, currentequipmentCode);
                        SelectPortInfo(currentequipmentCode);
                        txtEquipmentName.Text = currentequipmentName;
                    }
                }
                else
                {
                    if (chkCount <= 1)
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
                                if (sPasteStrings[i].ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[j].DataItem, "PLLT_ID")).GetString())
                                {
                                    for (int k = 0; k < dgIssueTargetInfo.Rows.Count; k++)
                                    {
                                       if(Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[j].DataItem, "REP_PROCESSING_GROUP_ID")).GetString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[k].DataItem, "REP_PROCESSING_GROUP_ID")).GetString())
                                        {
                                            DataTableConverter.SetValue(dgIssueTargetInfo.Rows[k].DataItem, "CHK", 1);

                                            dgIssueTargetInfo.EndEdit();
                                            dgIssueTargetInfo.EndEditRow(true);
                                            break;
                                        }
                                    }

                                 
                                }
                            }
                        }
                        
                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_ID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "MTRL_CSTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NM").GetString();
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

        #endregion

  

        #endregion

        #region Method
        
        #region FIFO 출고, 조건출고  집계 및 리스트 조회 : SelectManualOutInventory(), SelectManualOutInventoryList()

        /// <summary>
        /// FIFO 출고, 조건출고 집계 조회
        /// </summary>
        private void SelectManualOutInventory()
        {
            const string bizRuleName = "DA_INV_SEL_SUMMARY_WAREHOUSE_GROUP_MTRL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = cboStocker.SelectedValue;
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
                            LotTotalCount = LotTotalCount + Convert.ToDecimal(bizResult.Rows[i]["TOTAL_QTY"]);
                            LotCount = LotCount + Convert.ToDecimal(bizResult.Rows[i]["AVAIL_QTY_1"]);
                            LotHoldCount = LotHoldCount + Convert.ToDecimal(bizResult.Rows[i]["HOLD_QTY_1"]);
                           
                        }

                        DataRow newRow = bizResult.NewRow();
                        newRow["GRADE"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["EQPT_ID"] = string.Empty;
                        newRow["EQPT_NM"] = string.Empty;
                        newRow["MATERIAL_NAME"] = string.Empty;
                        newRow["TOTAL_QTY"] = LotTotalCount;
                        newRow["AVAIL_QTY_1"] = LotCount;
                        newRow["HOLD_QTY_1"] = LotHoldCount;
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
            const string bizRuleName = "DA_INV_SEL_NORM_MTRL";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("WH_TYPE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("HOLD_PLT_YN", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["WH_TYPE"] = cboStockerType.SelectedValue;
                dr["STK_ID"] = _equipmentCode;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["MTRLID"] = _MtrlID;
                dr["HOLD_PLT_YN"] = _selectedWipHold;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["ROWNUMBER"] = i + 1;
                        }
                    }
                    if (!isRefresh)
                    {
                        SetIssuePort(cboIssuePort, string.Empty);
                    }

                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
              

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
            const string bizRuleName = "DA_INV_SEL_SUMMARY_EMPTY_CST_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = cboStocker.SelectedValue;
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
                        BobinCount = g.Sum(x => x.Field<double>("CST_CNT")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                        newRow["ELTR_TYPE_NM"] = query.ElectrodeTypeName;
                        newRow["CST_CNT"] = query.BobinCount;
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
          
            const string bizRuleName = "DA_INV_SEL_SUMMARY_EMPTY_CST_MTRL";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = _equipmentCode;
      
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
            const string bizRuleName = "DA_INV_SEL_ABNORM_RACK_TOTAL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    decimal CstCount = 0;


                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            CstCount = CstCount + Convert.ToDecimal(bizResult.Rows[i]["CST_CNT"]);
                        }

                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPT_NM"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["ABNORM_STAT_NM"] = string.Empty;
                        newRow["CST_CNT"] = CstCount;
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
            const string bizRuleName = "DA_INV_SEL_ABNORM_RACK";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("FACILITY_CODE", typeof(string));
            inTable.Columns.Add("STK_ID", typeof(string));
            inTable.Columns.Add("RACK_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            dr["STK_ID"] = _equipmentCode;
            dr["RACK_ID"] = _cstTypeCode;
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
            const string bizRuleName = "DA_INV_SEL_ABNORM_CST_MTRL_TOTAL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                  
                    decimal CstCount = 0;
                

                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            CstCount = CstCount + Convert.ToDecimal(bizResult.Rows[i]["CST_CNT"]);
                        }

                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPT_NM"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["ABNORM_STAT_NM"] = string.Empty;
                        newRow["CST_CNT"] = CstCount;
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
            const string bizRuleName = "DA_INV_SEL_ABNORM_CST_MTRL";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("FACILITY_CODE", typeof(string));
            inTable.Columns.Add("STK_ID", typeof(string));
            inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            dr["STK_ID"] = _equipmentCode;
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
               

               if(equipmentCode == string.Empty)
               {
                    Util.gridClear(dgPortInfo);
                    //return;
                }

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WH_LOCATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;
                dr["WH_LOCATE"] = string.Empty;

                if (cboTagetStk.Items != null
                    && !cboTagetStk.SelectedValue.ToString().Equals("SELECT"))
                {
                    dr["WH_LOCATE"] = cboTagetStk.SelectedValue;
                }

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_INV_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
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

        #region 수동 출고 처리 : ChkOutput(),SaveManualIssueByEsnb()
        private void ChkOutput()
        {
            try
            {

                DataTable dtChkDurable = new DataTable();
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dtChkDurable = DataTableConverter.Convert(dgIssueTargetInfoByEmptyCarrier.ItemsSource);

                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dtChkDurable = DataTableConverter.Convert(dgIssueTargetInfoByAbNormalCarrier.ItemsSource);

                }
                else
                {
                    dtChkDurable = DataTableConverter.Convert(dgIssueTargetInfo.ItemsSource);

                }

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("UI_YN", typeof(string));


                DataTable inDurableID = inDataSet.Tables.Add("DURABLE_LIST");
                inDurableID.Columns.Add("DURABLE_ID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PORT_ID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                newRow["UI_YN"] = "Y";

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dtChkDurable.Rows.Count; i++)
                {

                    if (dtChkDurable.Rows[i]["CHK"].ToString() == "1")
                    {


                        newRow = inDurableID.NewRow();
                        // newRow["DURABLE_ID"] = dtChkDurable.Rows[i]["MLOT_ID"].GetString();
                        newRow["DURABLE_ID"] = dtChkDurable.Rows[i]["MLOT_ID"].GetString();

                        inDurableID.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_INV_MTRL_OUTSTK_VALIDATION", "INDATA,DURABLE_LIST", "OUTDATA", (bizResult, bizException) =>
                {


                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 여러건의 데이터 중 하나라도 PERMIT_FLAG가 1로 나오면 출고 안됨
                        if (bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            for (int i = 0; i < bizResult.Tables["OUTDATA"].Rows.Count; i++)
                            {
                                if (bizResult.Tables["OUTDATA"].Rows[i]["PERMIT_FLAG"].ToString() == "1")
                                {    //[%1] status is not available to STKOUT
                                    //[%1]상태는 출고불가합니다.
                                    Util.MessageValidation("SUF9051", bizResult.Tables["OUTDATA"].Rows[i]["DURABLE_ID"].ToString());
                                    return;
                                }

                            }
                            SaveManualIssueByEsnb();

                        }

                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);
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
                const string bizRuleName = "BR_INV_REG_TRF_CMD";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("SRC_PORTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));
                inTable.Columns.Add("TARGET_STO", typeof(string));
                inTable.Columns.Add("MLOT_ID", typeof(string));
                inTable.Columns.Add("MTRL_TYPE", typeof(string));

                DataTable dtCopy = new DataTable();
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dtCopy = DataTableConverter.Convert(dgIssueTargetInfoByEmptyCarrier.ItemsSource);
                    _Carrierid = "MTRL_CSTID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dtCopy = DataTableConverter.Convert(dgIssueTargetInfoByAbNormalCarrier.ItemsSource);
                    _Carrierid = "MTRL_CSTID";
                }
                else
                {
                    dtCopy = DataTableConverter.Convert(dgIssueTargetInfo.ItemsSource);
                    _Carrierid = "MTRL_CSTID";
                }

                DataTable dtTo = dtCopy.Copy();

                //for (int i = 0; i < dtCopy.Rows.Count; i++)
                //{
                //    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                //    {
                //        for (int j = 0; j < dtTo.Rows.Count; j++)
                //        {
                //            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["MLOT_ID"].ToString())
                //            {
                //                dtTo.Rows[j]["CHK"] = 1;
                //            }
                //        }
                //    }
                //}


                for (int i = 0; i < dtTo.Rows.Count; i++)
                {

                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {

                        if (rdoCarrier.IsChecked == true)
                        {
                            if (cboTagetStk.SelectedValue.Equals(GetStandbyWarehouseCode()) && dtTo.Rows[i]["HOLD_YN"].GetString() == "Y")
                            {
                                //HOLD자재는 자재창고를 선택하세요
                                Util.MessageValidation("SUF9053");
                                HiddenLoadingIndicator();
                                return;
                            }
                        }
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = dtTo.Rows[i]["MTRL_CSTID"].GetString();
                        newRow["SRC_EQPTID"] = dtTo.Rows[i]["SRC_EQPTID"].GetString();
                        newRow["SRC_PORTID"] = dtTo.Rows[i]["SRC_PORTID"].GetString();
                        newRow["DST_EQPTID"] = _dst_eqptID;
                        newRow["DST_PORTID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();

                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        newRow["TARGET_STO"] = cboTagetStk.SelectedValue;
                        newRow["MLOT_ID"] = dtTo.Rows[i]["MLOT_ID"].GetString();
                        newRow["MTRL_TYPE"] = dtTo.Rows[i]["MTRL_TYPE"].GetString();
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

                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                            SetIssuePort(cboIssuePort, selectedequipmentCode);
                            SelectPortInfo(selectedequipmentCode);


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
            if (cboTagetStk.SelectedIndex == 0)
            {
                //"목적지창고를 선택하세요
                Util.MessageInfo("SUF9052");
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
                    _Carrierid = "MTRL_CSTID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    _Carrierid = "MTRL_CSTID";
                }
                else
                {
                    _Carrierid = "MTRL_CSTID";
                }
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("MLOT_ID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                        newRow["MLOT_ID"] = DataTableConverter.GetValue(row.DataItem, "MLOT_ID").GetString();
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

            if ( _selectedRadioButtonValue == "NORMAL" || _selectedRadioButtonValue == "NOREADCARRIER")
            {
                attribute1 = "Y";
                attribute2 = null;
            }
            else
            {
                attribute1 = null;
                attribute2 = "Y";
            }
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "ATTR5", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, attribute1, attribute2, null, "Y", "AREA_EQUIPMENT_MTRL_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region 창고 콤보 조회 : SetStockerCombo()
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
        
            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };
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

        #region 목적지창고 조회 : SetTagetCombo()
        private static void SetTagetCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE6" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,"INV_WH_LOCATE","Y","Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
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

                if (equipmentCode == string.Empty)
                {
                    if (cbo.Items.Count > 0)
                    {
                        for (int i = 0; i < cbo.Items.Count; i++)
                        {
                            cbo.Items.RemoveAt(i);
                            i--;
                        }
                    }
                    Util.gridClear(dgPortInfo);
                    //return;
                }


                DataTable inTable = new DataTable("RQSTDT");  // 목적지 창고 정보 인자값 추가.
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WH_LOCATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;
                dr["WH_LOCATE"] = string.Empty;

                if (cboTagetStk.Items != null
                    && !cboTagetStk.SelectedValue.ToString().Equals("SELECT"))                   
                {
                    dr["WH_LOCATE"] = cboTagetStk.SelectedValue;
                }
               
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INV_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);
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


        private void dgIssueTargetInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfo.TopRows.Count; i < dgIssueTargetInfo.Rows.Count; i++)
                {

                    if (dgIssueTargetInfo.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfo.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CHK"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CMD_STAT_CODE"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CMD_STAT_CODE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["GRADE"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["GRADE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["REP_PROCESSING_GROUP_ID"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CHK"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["CMD_STAT_CODE"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["CMD_STAT_CODE"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["GRADE"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["GRADE"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo.GetCell(idxS, dgIssueTargetInfo.Columns["REP_PROCESSING_GROUP_ID"].Index), dgIssueTargetInfo.GetCell(idxE, dgIssueTargetInfo.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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


        private string GetStandbyWarehouseCode()
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
            inDataTable.Columns.Add("BUSINESS_USAGE_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE14", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            newRow["BUSINESS_USAGE_TYPE_CODE"] = "INV_WH_LOCATE";
            newRow["USE_FLAG"] = "Y";
            newRow["ATTRIBUTE14"] = "Y";

            inDataTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO", "INDATA", "OUTDATA", inDataTable);

            string ProductionStandbyWarehouseCode = string.Empty;
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                ProductionStandbyWarehouseCode = Util.NVC(dtResult.Rows[0]["CBO_CODE"]);
            }

            return ProductionStandbyWarehouseCode;

        }

        #endregion

        private void dgIssueTargetInfoByEmptyCarrier_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfoByEmptyCarrier.TopRows.Count; i < dgIssueTargetInfoByEmptyCarrier.Rows.Count; i++)
                {

                    if (dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfoByEmptyCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_CODE"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_CODE"].Index)));
                                    
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_CODE"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_CODE"].Index)));
                                

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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

        private void cboTagetStk_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

            if (lastIdx >= 0)
            {
                string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_ID").GetString();
                string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "MTRL_CSTID").GetString();
                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPT_NM").GetString();
                SetIssuePort(cboIssuePort, selectedequipmentCode);                
            }
            else
            {
                SetIssuePort(cboIssuePort, string.Empty);
            }
            
        }
    }
}
