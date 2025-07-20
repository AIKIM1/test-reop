/*************************************************************************************
 Created Date : 2020.10.26
      Creator : 신광희
   Decription : 작업지시 팝업 (UserControl(UC_WORKORDER_LINE) 복사하여 팝업(CMM_WORKORDER_LINE_DRB)으로 생성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.26  신광희 : Initial Created.   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// CMM_WORKORDER_LINE_DRB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_083_WORKORDER_NH_MTRL : C1Window, IWorkArea
    {
        #region Declaration & Constructor        
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _coatingSideType = string.Empty;
        private string _workOrderId = string.Empty;
        private string _workOrderDetailId = string.Empty;

        private string _foctoryPlanReferenceProcessCode = string.Empty;
        private string _processErpUseYN = string.Empty;                             // Workorder 사용 공정 여부.
        private string _processPlanLevelCode = string.Empty;                        // 계획 Level 코드. (EQPT, PROC .. )
        private string _processPlanManagementTypeCode = string.Empty;               // 계획 관리 유형 (WO, MO, REF..)
        private string _previousEquipmentSegmentCode = string.Empty;                // 타라인 이전 선택 값.
        private string _unloaderLotType = string.Empty;                             // 언로더 기준 생성 LOT 유형 (LOT_ID / CST_ID)
        private string _productId = string.Empty;

        int iCntInline = 0;
        private bool _isShowEquipmentName = false;
        private bool _isLoaded = false;


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        //private CMM_WORKORDER_WIPLIST workInProcessList;
        //private CMM_ASSY_PRDT_GPLM assyProductgplm;


        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public MCS001_083_WORKORDER_NH_MTRL()
        {
            InitializeComponent();

            this.Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Input, (System.Threading.ThreadStart)(() =>
                {
                    SetChangeDatePlan();
                }
            ));
        }

        private void InitializeGridColumns()
        {
            if (dgWorkOrder == null) return;

            if (_processCode.Equals(Process.TOP_COATING) || _processCode.Equals(Process.INS_COATING) || _processCode.Equals(Process.SRS_COATING) || _processCode.Equals(Process.HALF_SLITTING) ||
                _processCode.Equals(Process.ROLL_PRESSING) || _processCode.Equals(Process.TAPING) || _processCode.Equals(Process.REWINDER) || _processCode.Equals(Process.BACK_WINDER))
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                }
            }

            // 조립 공정인 경우 버전 컬럼 HIDDEN.
            if (_processCode.Equals(Process.NOTCHING) || _processCode.Equals(Process.LAMINATION) || _processCode.Equals(Process.STACKING_FOLDING) || _processCode.Equals(Process.PACKAGING) 
                //||
                //_processCode.Equals(Process.WINDING) || _processCode.Equals(Process.ASSEMBLY) || _processCode.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("PROD_VER_CODE"))
                {
                    dgWorkOrder.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
            }

            // 패키지 공정인 경우만 모델랏 정보 표시
            if (dgWorkOrder.Columns.Contains("MDLLOT_ID"))
            {
                if (_processCode.Equals(Process.PACKAGING) || _processCode.Equals(Process.WINDING) || _processCode.Equals(Process.ASSEMBLY))
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                }
            }

            // 라미 공정일 경우 Cell Type (CLSS_NAME : 분류명) 컬럼 표시 -> 극성 컬럼 Hidden
            if (_processCode.Equals(Process.LAMINATION))
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;

                // 남경 라미인 경우 CLSS_NAME 대신에 PRODNAME으로 표시 처리.
                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Visible;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            }

            // 노칭을 제외한 조립 공정 극성 컬럼 Hidden
            if (_processCode.Equals(Process.STACKING_FOLDING) ||
                _processCode.Equals(Process.SRC) ||
                _processCode.Equals(Process.STP) ||
                _processCode.Equals(Process.SSC_BICELL) ||
                _processCode.Equals(Process.SSC_FOLDED_BICELL) ||
                _processCode.Equals(Process.PACKAGING) ||
                _processCode.Equals(Process.WINDING) ||
                _processCode.Equals(Process.ASSEMBLY) ||
                _processCode.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeCombo()
        {
            DataTable dtTemp = null;

            if (_processCode.Equals(Process.VD_LMN))
                dtTemp = GetEquipmentSegmentComboForVD();
            else
                dtTemp = GetEquipmentSegmentCombo();

            // Inline정보 확인
            if (_processCode.Equals("A2000"))
            {
                CheckInline(_equipmentSegmentCode);
                if (iCntInline > 0)
                {
                    _unloaderLotType = "CST_ID";
                }
                else
                {
                    _unloaderLotType = "LOT_ID";
                }
                dtTemp = GetEquipmentSegmentComboInline();
            }

            if (dtTemp != null)
            {
                cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();
                cboEquipmentSegment.SelectedIndex = 0;
                if (!Util.NVC(_previousEquipmentSegmentCode).Equals("") && dtTemp?.Select("CBO_CODE = '" + _previousEquipmentSegmentCode + "'").Length > 0)
                    cboEquipmentSegment.SelectedValue = _previousEquipmentSegmentCode;
                else
                    cboEquipmentSegment.SelectedIndex = 0;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            }
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _equipmentSegmentCode = Util.NVC(tmps[0]);
            _processCode = Util.NVC(tmps[1]);
            _equipmentCode = Util.NVC(tmps[2]);
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeCombo();
            InitializeGridColumns();
            btnSelectCancel.IsEnabled = false;
            SetParameters();

            GetWorkOrder();

            // 선택된 행이 있으면 WO BOM 조회
            int idx = _util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx > -1)
            {
                SelectWorkOrderBOM(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID").GetString());
            }


            //dtpDateFrom_SelectedDataTimeChanged
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            _isLoaded = true;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboEquipmentSegment?.SelectedValue != null)
                    _previousEquipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
                else
                    _previousEquipmentSegmentCode = string.Empty;

                if (_isLoaded == false) return;

                btnSearch_Click(btnSearch, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                // BASETIME 기준설정
                DateTime currentDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string currentTime = string.Empty;
                string baseTime = string.Empty;

                GetChangeDatePlan(out currentDate, out currentTime, out baseTime);

                if (Util.NVC_Decimal(currentTime) - Util.NVC_Decimal(baseTime) < 0)
                    baseDate = currentDate.AddDays(-1);

                // W/O 공정인 경우에만 체크.
                if (_processPlanManagementTypeCode.Equals("WO"))
                {
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }
                else
                {
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }

                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
                    return;
                }

                if (_isLoaded == false) return;

                btnSearch_Click(btnSearch, null);
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                // BASETIME 기준설정
                DateTime currentDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string currentTime = string.Empty;
                string baseTime = string.Empty;

                GetChangeDatePlan(out currentDate, out currentTime, out baseTime);

                if (Util.NVC_Decimal(currentTime) - Util.NVC_Decimal(baseTime) < 0)
                    baseDate = currentDate.AddDays(-1);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    baseDate = dtpDateFrom.SelectedDateTime;

                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                    return;
                }

                if (_isLoaded == false) return;
                btnSearch_Click(btnSearch, null);
            }));
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = idx;

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "EIO_WO_SEL_STAT").GetString();
                    btnSelectCancel.IsEnabled = workState == "Y";
                }

                // WO BOM 조회
                SelectWorkOrderBOM(DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "WOID").GetString());
            }
        }

        private void dgWorkOrder_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                }
            }));
        }

        private void dgWorkOrder_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        
                    }
                }
            }));
        }

        private void dgWorkOrder_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    // WO BOM 조회
                    SelectWorkOrderBOM(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WOID").GetString());

                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;
                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content is RadioButton)
                            {
                                RadioButton rdoButton = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

                                if (rdoButton != null)
                                {
                                    if (rdoButton.DataContext == null)
                                        return;

                                    // 부모 조회 없으므로 로직 수정..
                                    if (!(bool)rdoButton.IsChecked && (rdoButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                                    {
                                        DataRow dtRow = (rdoButton.DataContext as DataRowView).Row;

                                        for (int i = 0; i < dg.Rows.Count; i++)
                                            if (e.Cell.Row.Index == i)   // Mode = OneWay 이므로 Set 처리.
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                                            else
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                        //row 색 바꾸기
                                        dgWorkOrder.SelectedIndex = e.Cell.Row.Index;

                                        //선택 취소 버튼 Enabled 속성 설정
                                        if (CommonVerify.HasDataGridRow(dgWorkOrder))
                                        {
                                            string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rdoButton.Parent).DataGrid.Rows[e.Cell.Row.Index].DataItem, "EIO_WO_SEL_STAT").GetString();
                                            btnSelectCancel.IsEnabled = workState == "Y";
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    try
            //    {
            //        if (workInProcessList != null) return;


            //        C1DataGrid dg = sender as C1DataGrid;

            //        if (dg == null) return;

            //        C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

            //        if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

            //        switch (Convert.ToString(currCell.Column.Name))
            //        {
            //            case "PRODID":
            //                if (_processCode.Equals(Process.LAMINATION) || _processCode.Equals(Process.STACKING_FOLDING) || _processCode.Equals(Process.PACKAGING))
            //                {
            //                    workInProcessList = new CMM_WORKORDER_WIPLIST();
            //                    workInProcessList.FrameOperation = FrameOperation;

                                
            //                    C1.WPF.DataGrid.DataGridRow currRow = dg.CurrentRow; DataRow dtRow = (dgWorkOrder.Rows[currRow.Index].DataItem as DataRowView).Row;
            //                    string valueToWorkOrderId = dtRow["WOID"].ToString();

            //                    string valueToProductId = dtRow["PRODID"].ToString();

            //                    object[] parameters = new object[7];
            //                    parameters[0] = valueToWorkOrderId;
            //                    parameters[1] = LoginInfo.CFG_AREA_ID;
            //                    parameters[2] = _equipmentSegmentCode;
            //                    parameters[3] = _processCode;
            //                    parameters[4] = Area_Type.ASSY;
            //                    parameters[5] = valueToProductId;
            //                    C1WindowExtension.SetParameters(workInProcessList, parameters);

            //                    workInProcessList.Closed += new EventHandler(workInProcessList_Closed);
            //                    Dispatcher.BeginInvoke(new Action(() => workInProcessList.ShowModal()));
            //                }
            //                break;

            //            case "PRJT_NAME":
            //                if (_processCode.Equals(Process.WINDING) || _processCode.Equals(Process.ASSEMBLY) || _processCode.Equals(Process.WASHING))
            //                    assyProductgplm = new CMM_ASSY_PRDT_GPLM();
            //                assyProductgplm.FrameOperation = FrameOperation;

            //                if (assyProductgplm != null)
            //                {
            //                    object[] parameters = new object[2];
            //                    parameters[0] = DataTableConverter.GetValue(currCell.Row.DataItem, "PRODID");

            //                    if (string.Equals(_processCode, Process.WINDING))
            //                        parameters[1] = Gplm_Process_Type.WINDING;
            //                    else if (string.Equals(_processCode, Process.ASSEMBLY))
            //                        parameters[1] = Gplm_Process_Type.ASSEMBLY;
            //                    else
            //                        parameters[1] = Gplm_Process_Type.WASHING;

            //                    C1WindowExtension.SetParameters(assyProductgplm, parameters);
            //                    this.Dispatcher.BeginInvoke(new Action(() => assyProductgplm.ShowModal()));
            //                }
            //                break;

            //        }

            //        if (dg.CurrentCell != null)
            //            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
            //        else if (dg.Rows.Count > 0)
            //            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
            //    }
            //    catch (Exception){}
            //}));
        }

        private void workInProcessList_Closed(object sender, EventArgs e)
        {
            //CMM_WORKORDER_WIPLIST pop = sender as CMM_WORKORDER_WIPLIST;
            //if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            //{
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_equipmentSegmentCode.Equals("") || _equipmentSegmentCode.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                if (_equipmentCode.Equals("") || _equipmentCode.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int idx = _util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
                return;

            DataRow dtRow = (dgWorkOrder.Rows[idx].DataItem as DataRowView).Row;
            _productId = dtRow["PRODID"].GetString();

            if (!CanChangeWorkOrder(dtRow))
                return;

            if (string.Equals(_processCode, Process.WASHING))
            {
                if (!ValidationWashingReWork())
                    return;
            }

            string stringMessageCode = string.Empty;
            if (WipProcessCheckForWorkOrderChange() == true)
            {
                // 설비에 진행중인 LOT 이 존재합니다. 진행중인 LOT 이 존재할 때 WO 변경시 실적저장에 이상이 발생할 수 있습니다. WO 를 변경하시겠습니까?
                stringMessageCode = "SFU3730";
            }
            else
            {
                // 작업지시를 변경하시겠습니까?
                stringMessageCode = "SFU2943";
            }

            // 작업지시를 변경하시겠습니까?
            Util.MessageConfirm(stringMessageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(dtRow);
                }
            });
        }

        private bool WipProcessCheckForWorkOrderChange()
        {
            bool bResult = false;

            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_WIP_PROC_FOR_WO_CHG", "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    bResult = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return bResult;
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_equipmentCode) || string.IsNullOrEmpty(_processCode) || string.IsNullOrEmpty(_equipmentSegmentCode) || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;

                DataRowView drv = dgWorkOrder.Rows[idx].DataItem as DataRowView;
                if (drv != null)
                {
                    DataRow dr = drv.Row;
                    _productId = dr["PRODID"].ToString();

                    // 작업지시를 선택취소 하시겠습니까?
                    Util.MessageConfirm("SFU2944", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SelectWorkInProcessStatus(_equipmentCode, _processCode, _equipmentSegmentCode, (table, ex) =>
                            {
                                if (CommonVerify.HasTableRow(table))
                                {
                                    if (table.Rows[0]["WIPSTAT"].GetString() == "PROC")
                                    {
                                        Util.MessageValidation("SFU1917");     //진행중인 LOT이 있습니다.
                                        return;
                                    }
                                    else
                                    {
                                        WorkOrderChange(dr, false);
                                    }
                                }
                                else
                                {
                                    WorkOrderChange(dr, false);
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Method

        #region [BizCall]
        private void GetProcessFPInfo()
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["PROCID"] = _processCode;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (!CommonVerify.HasTableRow(dtRslt))
                {
                    _foctoryPlanReferenceProcessCode = string.Empty;
                    _processErpUseYN = string.Empty;
                    _processPlanLevelCode = string.Empty;
                    return;
                }

                // WorkOrder 사용여부, 계획LEVEL 코드.
                _processErpUseYN = Util.NVC(dtRslt.Rows[0]["ERPRPTIUSE"]);
                _processPlanLevelCode = Util.NVC(dtRslt.Rows[0]["PLAN_LEVEL_CODE"]);
                _processPlanManagementTypeCode = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);
                _foctoryPlanReferenceProcessCode = string.Empty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWorkOrderInfo(string workOrderId, out string returnMessage, out string returnMessageCode)
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WO();

                DataRow newRow = inTable.NewRow();
                newRow["WOID"] = workOrderId;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKORDER", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("20") ||
                        Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("40"))
                    {
                        returnMessage = "OK";
                        returnMessageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        returnMessageCode = "SFU3058";    // 선택 가능한 상태의 작업지시가 아닙니다.
                    }
                }
                else
                {
                    returnMessage = "NG";
                    returnMessageCode = "SFU2881";// "존재하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                returnMessage = "NG";
                returnMessageCode = ex.Message;
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private bool ChkFPDtlInfoByMonth(string workOrderDetailId, string sCalDateYMD, out string sOutMsg)
        {
            sOutMsg = "";

            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = workOrderDetailId;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FP_DETL_PLAN", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult) && dtResult.Columns.Contains("STRT_DTTM") && dtResult.Columns.Contains("END_DTTM"))
                {
                    DateTime dtStrtDate;
                    DateTime dtEndDate;
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["STRT_DTTM"]), out dtStrtDate);
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["END_DTTM"]), out dtEndDate);

                    if (dtEndDate != null)
                    {
                        // W/O 공정인 경우에만 체크.
                        if (_processPlanManagementTypeCode.Equals("WO"))
                        {
                            if (Util.NVC_Int(dtEndDate.ToString("yyyyMMdd")) >= Util.NVC_Int(sCalDateYMD))
                            {
                                bRet = true;
                            }
                            else
                                sOutMsg = "SFU3517";    // 계획일자가 이미 지난 WO는 선택할 수 없습니다.
                        }
                        else
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckCalDateByMonth(LGCDatePicker dtPik, out DateTime dtCaldate, out string sCalDateYMD, out string sCalDateYYYY, out string sCalDateMM, out string sCalDateDD)
        {
            try
            {
                bool bRet = false;

                dtCaldate = System.DateTime.Now;
                sCalDateYMD = "";
                sCalDateYYYY = "";
                sCalDateMM = "";
                sCalDateDD = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("CALDATE"))
                    {
                        if (Util.NVC(dtResult.Rows[0]["CALDATE"]).Equals(""))
                            return bRet;

                        DateTime.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE"]), out dtCaldate);
                    }
                    if (dtResult.Columns.Contains("CALDATE_YMD"))
                        sCalDateYMD = Util.NVC(dtResult.Rows[0]["CALDATE_YMD"]);
                    if (dtResult.Columns.Contains("CALDATE_YYYY"))
                        sCalDateYYYY = Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]);
                    if (dtResult.Columns.Contains("CALDATE_MM"))
                        sCalDateMM = Util.NVC(dtResult.Rows[0]["CALDATE_MM"]);
                    if (dtResult.Columns.Contains("CALDATE_DD"))
                        sCalDateDD = Util.NVC(dtResult.Rows[0]["CALDATE_DD"]);

                    if (dtResult.Columns.Contains("CALDATE_YYYY") && dtResult.Columns.Contains("CALDATE_MM"))
                    {
                        int iYM = 0;
                        int.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]) + Util.NVC(dtResult.Rows[0]["CALDATE_MM"]), out iYM);
                        if (dtPik != null && iYM != Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                        {
                            bRet = true;
                        }
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                dtCaldate = System.DateTime.Now;
                sCalDateYMD = string.Empty;
                sCalDateYYYY = string.Empty;
                sCalDateMM = string.Empty;
                sCalDateDD = string.Empty;

                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="dr">선택 한 작업지시 정보 DataRow</param>
        /// <param name="isSelectFlag">선택 처리:true 선택 취소:fals</param>
        private void SetWorkOrderSelect(DataRow dr, bool isSelectFlag = true)
        {
            try
            {
                DataTable inTable = _bizDataSet.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = _equipmentCode;
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(dr["WO_DETL_ID"]) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                if (isSelectFlag)
                    Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.
                else
                    Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        private void GetWorkOrder(bool isSelectFlag = true)
        {
            try
            {
                InitializeCombo();

                if (_processCode.Length < 1 || _equipmentCode.Length < 1 || _equipmentSegmentCode.Length < 1)
                    return;

                // 일자 설정이 안된경우 RETURN.
                if (dtpDateFrom.SelectedDateTime.Year < 2000 || dtpDateTo.SelectedDateTime.Year < 2000)
                    return;

                // Process 정보 조회
                GetProcessFPInfo();

                string previousWorkOrderDetail = string.Empty;

                if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
                {
                    int idx = _util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                    if (idx >= 0)
                    {
                        previousWorkOrderDetail = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
                    }
                }

                // 취소인 경우에는 선택 없애도록..
                if (!isSelectFlag)
                    previousWorkOrderDetail = string.Empty;

                InitializeGridColumns();
                ClearWorkOrderInfo();

                btnSelectCancel.IsEnabled = false;
                DataTable searchResult = null;

                searchResult = GetWorkOrderListByEquipmentSegment();

                if (searchResult == null) return;
                Util.GridSetData(dgWorkOrder, searchResult, FrameOperation, true);

                // 3D 제품이 존재하는 경우
                if (dgWorkOrder.Columns.Contains("CELL_3DTYPE"))
                {
                    if (searchResult.Columns.Contains("CELL_3DYN"))
                    {
                        DataRow[] drTmp = searchResult.Select("CELL_3DYN = 'Y'");
                        if (drTmp.Length > 0)
                        {
                            dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                    }
                }

                // 현 작업지시 정보 Top Row 처리 및 고정..
                if (searchResult.Rows.Count > 0)
                {
                    if (!Util.NVC(searchResult.Rows[0]["EIO_WO_DETL_ID"]).Equals(""))
                        dgWorkOrder.FrozenTopRowsCount = 1;
                    else
                        dgWorkOrder.FrozenTopRowsCount = 0;
                }

                // 이전 선택 작지 선택
                if (!previousWorkOrderDetail.Equals(""))
                {
                    int idx = _util.GetDataGridRowIndex(dgWorkOrder, "WO_DETL_ID", previousWorkOrderDetail);

                    if (idx >= 0)
                    {
                        for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                            if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                            else
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);

                        DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                        // 재조회 처리.
                        ReChecked(idx);

                        _productId = DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID").ToString();
                    }
                }
                else // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
                {
                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            dgWorkOrder.SelectedIndex = i;
                            _productId = DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PRODID").ToString();
                            break;
                        }
                    }
                }

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    int idx = _util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                    if (idx != -1)
                    {
                        string workState = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"));
                        btnSelectCancel.IsEnabled = workState == "Y";
                    }
                }

                if (cboEquipmentSegment != null && Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Collapsed;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Visible;
                    if (_isShowEquipmentName) dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static void SelectWorkInProcessStatus(string equipmentCode, string processCode, string equipmentSegmentCode, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = equipmentCode;
                inData["PROCID"] = processCode;
                inData["EQSGID"] = equipmentSegmentCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    actionCompleted?.Invoke(result, null);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SelectProcessStateLot()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = _equipmentCode;
                inData["PROCID"] = _processCode;
                inData["EQSGID"] = _equipmentSegmentCode;
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("WIPSTAT"))
                {
                    if (dtRslt.Rows[0]["WIPSTAT"].Equals("PROC"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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

        private void SetGridData(C1DataGrid dg)
        {
            dg.Columns["CHK"].DisplayIndex = 0;
            dg.Columns["EIO_WO_SEL_STAT"].DisplayIndex = 1;
            dg.Columns["PRJT_NAME"].DisplayIndex = 2;
            dg.Columns["PROD_VER_CODE"].DisplayIndex = 3;
            dg.Columns["WOID"].DisplayIndex = 4;
            dg.Columns["MDLLOT_ID"].DisplayIndex = 5;
            dg.Columns["PRODID"].DisplayIndex = 6;
            dg.Columns["MKT_TYPE_NAME"].DisplayIndex = 7;
            dg.Columns["CELL_3DTYPE"].DisplayIndex = 8;
            dg.Columns["PRODNAME"].DisplayIndex = 9;
            dg.Columns["ELECTYPE"].DisplayIndex = 10;

            dg.Columns["MODLID"].DisplayIndex = 11;
            dg.Columns["LOTYNAME"].DisplayIndex = 12;
            dg.Columns["INPUT_QTY"].DisplayIndex = 13;
            dg.Columns["C_ROLL_QTY"].DisplayIndex = 14;
            dg.Columns["S_ROLL_QTY"].DisplayIndex = 15;
            dg.Columns["LANE_QTY"].DisplayIndex = 16;
            dg.Columns["OUTQTY"].DisplayIndex = 17;
            dg.Columns["UNIT_CODE"].DisplayIndex = 18;
            dg.Columns["STRT_DTTM"].DisplayIndex = 19;
            dg.Columns["END_DTTM"].DisplayIndex = 20;

            dg.Columns["WO_STAT_NAME"].DisplayIndex = 21;
            dg.Columns["WO_STAT_CODE"].DisplayIndex = 22;
            dg.Columns["WO_DETL_ID"].DisplayIndex = 23;
            dg.Columns["EQSGID"].DisplayIndex = 24;
            dg.Columns["EQSGNAME"].DisplayIndex = 25;
            dg.Columns["EQPTID"].DisplayIndex = 26;
            dg.Columns["EQPTNAME"].DisplayIndex = 27;
            dg.Columns["CLSS_ID"].DisplayIndex = 28;
            dg.Columns["CLSS_NAME"].DisplayIndex = 29;
            dg.Columns["PLAN_TYPE_NAME"].DisplayIndex = 30;

            dg.Columns["PLAN_TYPE"].DisplayIndex = 31;
            dg.Columns["WOTYPE"].DisplayIndex = 32;
            dg.Columns["EIO_WO_DETL_ID"].DisplayIndex = 33;
            dg.Columns["PRDT_CLSS_CODE"].DisplayIndex = 34;
            dg.Columns["DEMAND_TYPE"].DisplayIndex = 35;

            dg.FrozenColumnCount = 5;

        }

        private void SetChangeDatePlan(bool isInitFlag = true)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (isInitFlag)
            {
                if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateFrom.Tag = "CHANGE";

                    dtpDateTo.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateTo.Tag = "CHANGE";
                }
            }
            else
            {
                if (Util.NVC_Decimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate;
                    dtpDateFrom.Tag = "CHANGE";
                }

                if (Util.NVC_Decimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateTo.SelectedDateTime = currDate;
                    dtpDateTo.Tag = "CHANGE";
                }
            }
        }

        private void GetChangeDatePlan(out DateTime currentDate, out string currentTime, out string baseTime)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            currentDate = GetCurrentTime();
            currentTime = currentDate.ToString("HHmmss");
            baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);
        }

        private DataTable GetWorkOrderListByEquipmentSegment()
        {
            try
            {
                string bizRuleName = string.Equals(_processCode, Process.WASHING) ? "DA_PRD_SEL_WORKORDER_LIST_WASH_RW" : "DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE_DRB";

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = string.IsNullOrEmpty(_foctoryPlanReferenceProcessCode) ? _processCode : _foctoryPlanReferenceProcessCode;
                searchCondition["EQPTID"] = _equipmentCode;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0 &&
                    !Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    searchCondition["OTHER_EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    searchCondition["EQSGID"] = string.Empty;
                    searchCondition["PROC_EQPT_FLAG"] = "LINE";
                }
                else
                {
                    searchCondition["OTHER_EQSGID"] = string.Empty;
                    searchCondition["EQSGID"] = _equipmentSegmentCode;
                    searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();
                }
                inTable.Rows.Add(searchCondition);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentSegmentCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _processCode;
                dr["EQPTID"] = _equipmentCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;

                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception)
            {
                //Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentSegmentComboInline()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("OUT_LOT_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _processCode;
                dr["EQPTID"] = _equipmentCode;
                dr["OUT_LOT_TYPE"] = _unloaderLotType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR_WND", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;

                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private DataTable GetEquipmentSegmentComboForVD()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = "A1000," + Process.VD_LMN + "," + Process.VD_ELEC;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FOR_VD", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;
                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();
                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetFpPlanGnrtBasCode()
        {
            try
            {
                string planType = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _processCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_PLAN_GNRT_BAS_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("FP_PLAN_GNRT_BAS_CODE"))
                {
                    if (Util.NVC(dtResult.Rows[0]["FP_PLAN_GNRT_BAS_CODE"]).Equals("E"))
                    {
                        planType = "EQPT";
                        _isShowEquipmentName = true;
                    }
                    else
                    {
                        planType = "LINE";
                        _isShowEquipmentName = false;
                    }
                }

                return planType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "LINE";
            }
        }

        public void CheckInline(string line)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_UNLDR_FLAG";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["EQSGID"] = line;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            iCntInline = searchResult.Rows.Count;
        }

        private void SelectWorkOrderBOM(string workOrderId)
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WOID"] = workOrderId;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WOMTRL_BY_WOID", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListMaterial, bizResult, FrameOperation, false);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool ValidationWashingReWork()
        {
            if (string.IsNullOrEmpty(_equipmentCode))
                return false;

            const string bizRuleName = "DA_BAS_SEL_PROD_LOT_BY_EQPTID_WS";
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["EQPTID"] = _equipmentCode;
            dr["PR_LOTID"] = null;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                //진행중인 LOT이 있습니다.\r\nLOT ID : {%1}
                Util.MessageValidation("SFU3199", searchResult.Rows[0]["LOTID"]);
                return false;
            }
            return true;
        }
        #endregion

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSelectCancel);

            if (FrameOperation != null)
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 작업지시 선택 가능 Validation 처리
        /// </summary>
        /// <param name="iRow">선택한 작업지시 정보 Row Number</param>
        /// <returns></returns>
        private bool CanChangeWorkOrder(DataRow dtRow)
        {
            bool bRet = false;

            if (dtRow == null)
                return bRet;

            if (_equipmentCode.Trim().Equals("") || _processCode.Trim().Equals("") || _equipmentSegmentCode.Trim().Equals(""))
                return bRet;

            if (Util.NVC(dtRow["EIO_WO_SEL_STAT"]).Equals("Y"))
            {
                Util.MessageValidation("SFU3061");  //이미 선택된 작업지시 입니다.
                return bRet;
            }

            if (_processCode.Equals(Process.LAMINATION) || _processCode.Equals(Process.STACKING_FOLDING))
            {
                if (SelectProcessStateLot())
                {
                    Util.MessageValidation("SFU1917");
                    return bRet;
                }
            }

            // Workorder 내려오는 공정만 체크 필요.
            if (_processErpUseYN.Equals("Y"))
            {
                // 선택 가능한 작지 여부 확인.
                string sRet = string.Empty;
                string sMsg = string.Empty;

                GetWorkOrderInfo(Util.NVC(dtRow["WOID"]), out sRet, out sMsg);
                if (sRet.Equals("NG"))
                {
                    Util.MessageValidation(sMsg);
                    return bRet;
                }
            }

            // 해당 월의 W/O만 선택 가능
            DateTime dtCaldate;
            string sCalDateYMD = "";
            string sCalDateYYYY = "";
            string sCalDateMM = "";
            string sCalDateDD = "";
            string sOutMsg = "";

            CheckCalDateByMonth(null, out dtCaldate, out sCalDateYMD, out sCalDateYYYY, out sCalDateMM, out sCalDateDD);
            if (!ChkFPDtlInfoByMonth(Util.NVC(dtRow["WO_DETL_ID"]), sCalDateYMD, out sOutMsg))
            {
                Util.MessageValidation(sOutMsg);
                return bRet;
            }

            // W/O Rolling에 따라 자동 Over Completion을 위하여 하기 Validation 적용 [2017-09-18]
            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr = dt?.Select("EIO_WO_SEL_STAT = 'Y'");
            if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("MODLID"))
            {
                foreach (DataRow drTmp in dr)
                {
                    if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && Util.NVC(dtRow["MODLID"]).Equals(Util.NVC(drTmp["MODLID"])))
                    {
                        Util.MessageValidation("SFU4117"); // 동일한 모델, WO Type의 WO가 이미 선택되어 있습니다.
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        /// <summary>
        /// 작업지시 선택 처리
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isSelectFlag"> 선택 처리:true 선택 취소:false </param>
        private void WorkOrderChange(DataRow dr, bool isSelectFlag = true)
        {
            if (dr == null) return;

            SetWorkOrderSelect(dr, isSelectFlag);
            GetWorkOrder(isSelectFlag);

            if (isSelectFlag)
            {
                // WO 선택
                _workOrderId = Util.NVC(dr["WOID"]);
                _workOrderDetailId = Util.NVC(dr["WO_DETL_ID"]);

                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                // WO 선택 취소
                GetWorkOrder(isSelectFlag);
            }
        }

        /// <summary>
        /// 작업지시 Datagrid Clear
        /// </summary>
        private void ClearWorkOrderInfo()
        {
            Util.gridClear(dgWorkOrder);
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = iRow;
            }
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

        #endregion

        #endregion


    }
}