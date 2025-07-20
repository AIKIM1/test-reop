/*************************************************************************************
 Created Date : 2019.07.05
      Creator : 정문교
   Decription : 원자재관리 - 원자재 요청
--------------------------------------------------------------------------------------
 [Change History]
  2019.07.05  정문교 : Initial Created.
  2020.02.05  정문교 : IWMS <-> GMES I/F 변경에 따른 수정 
                       1.Biz 변경
                       2.요청 상태 코드 MTRL_SPLY_REQ_STAT_CODE -> MTRL_SPLY_REQ_STAT_CODE_ASSY 변경


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_001()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            if (((System.Windows.FrameworkElement)tabRequest.SelectedItem).Name.Equals("ctbRequest"))
            {
                chkEmrgReqFlag.IsChecked = false;
                txtRequestUser.Tag = string.Empty;
                txtRequestUser.Text = string.Empty;
                txtNote.Text = string.Empty;
            }
            else
            {
                txtRequestUserHis.Tag = string.Empty;
                txtRequestUserHis.Text = string.Empty;
                txtNoteHis.Text = string.Empty;

                //if (!(bool)chkMlot.IsChecked)
                //    dgHistory.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                //else
                    dgHistory.AlternatingRowBackground = null;
            }
        }

        private void InitializeGrid()
        {
            if (((System.Windows.FrameworkElement)tabRequest.SelectedItem).Name.Equals("ctbRequest"))
            {
                Util.gridClear(dgRequest);
                Util.gridClear(dgWorkOrderMtrl);
            }
            else
            {
                Util.gridClear(dgHistory);
                Util.gridClear(dgHistoryMlot);
            }
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            /////////////////////////////////////////// 요청
            //라인
            string[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);
            cboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;

            /////////////////////////////////////////// 요청 이력
            //동
            C1ComboBox[] cboAreaHisChild = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboAreaHis, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaHisChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentHisParent = { cboAreaHis };
            C1ComboBox[] cboEquipmentSegmentHisChild = { cboProcessHis, cboEquipmentHis };
            _combo.SetCombo(cboEquipmentSegmentHis, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentHisChild, cbParent: cboEquipmentSegmentHisParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessHisParent = { cboEquipmentSegmentHis };
            C1ComboBox[] cboProcessHisChild = { cboEquipmentHis };
            _combo.SetCombo(cboProcessHis, CommonCombo.ComboStatus.ALL, cbChild: cboProcessHisChild, cbParent: cboProcessHisParent, sCase: "PROCESS");

            //설비
            C1ComboBox[] cboEquipmentHisParent = { cboEquipmentSegmentHis, cboProcessHis };
            _combo.SetCombo(cboEquipmentHis, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentHisParent, sCase: "EQUIPMENT");
        }

        private void SetControl()
        {
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRequest);
            listAuth.Add(btnCancelHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();
            SearchWorkOrder();

            this.Loaded -= UserControl_Loaded;
        }

        #region [요청]

        /// <summary>
        /// 설비가 변경 되면 W/O 재조회
        /// </summary>
        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchWorkOrder();
        }

        private void dgWOChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgWorkOrder.SelectedIndex = idx;

                        InitializeGrid();

                        // WO BOM 조회
                        SearchBOM();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// WO BOM 체크시 요청 그리드에 반영
        /// </summary>
        private void dgWorkOrderMtrl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgWorkOrderMtrl.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                int row = cell.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgWorkOrderMtrl.ItemsSource);

                if (dt.Rows[row]["CHK"].Equals(1))
                    dt.Rows[row]["CHK"] = 0;
                else
                    dt.Rows[row]["CHK"] = 1;

                dt.AcceptChanges();
                Util.GridSetData(dgWorkOrderMtrl, dt, null, true);

                SetRequest(dt.Rows[row]);
            }
        }

        private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_SPLY_REQ_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgRequest_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                switch (Convert.ToString(e.Cell.Column.Name))
                {
                    case "MTRL_SPLY_REQ_QTY":

                        double UnitQty = 0;
                        double ReqQty = 0;
                        double ConvQty = 0;
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_SPLY_REQ_QTY")), out ReqQty);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "UNIT_QTY")), out UnitQty);

                        ConvQty = ReqQty * UnitQty;
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "CONV_QTY", ConvQty.ToString());

                        break;
                }
            }

        }

        /// <summary>
        /// 요청자 조회
        /// </summary>
        private void btnRequestUser_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }

        private void txtRequestUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popupUser();
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchWorkOrder();
        }

        /// <summary>
        /// 요청
        /// </summary>
        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            dgRequest.EndEdit(true);

            if (!ValidationRequest())
                return;

            // 요청하시겠습니까?
            Util.MessageConfirm("SFU2924", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RequestProcess();
                }
            });
        }

        #endregion

        #region [요청 이력]

        /// <summary>
        /// 자재 Lot Visibility
        /// </summary>
        private void chkMlot_Checked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(true);
        }
        private void chkMlot_Unchecked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(false);
        }

        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (!dg.Columns["MTRL_SPLY_REQ_ID2"].Width.Value.Equals(0))
                    {
                        dg.Columns["MTRL_SPLY_REQ_ID2"].Width = new C1.WPF.DataGrid.DataGridLength(0);
                    }
                }

                if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name == "MTRL_SPLY_REQ_STAT_NAME")
                {
                    if (DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString() != Mtrl_Request_StatCode.Cancel)
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
            }));
        }

        private void dgHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void dgHistoryChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgHistory.SelectedIndex = idx;

                        // 자재 Lot 조회
                        InitializeControls();
                        SearchMLOT();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistory_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgHistory.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataRowView dr = dgHistory.Rows[cell.Row.Index].DataItem as DataRowView;

                if (dr["MTRL_SPLY_REQ_STAT_CODE"].ToString() != Mtrl_Request_StatCode.Cancel)
                {
                    // 팝업호출 TakeOver
                    popTakeOver(Util.NVC(dr["MTRL_SPLY_REQ_ID"]));
                }
            }
        }


        private void dgHistoryMlot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Foreground 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "RCPT_CMPL_FLAG") == null ||
                        string.IsNullOrWhiteSpace(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "RCPT_CMPL_FLAG").ToString()))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));

        }

        /// <summary>
        /// 요청자 조회
        /// </summary>
        private void btnRequestUserHis_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }

        private void txtRequestUserHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popupUser();
            }
        }

        /// <summary>
        /// 요청 이력 조회
        /// </summary>
        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearchHis())
                return;

            SearchHisProcess();
        }

        /// <summary>
        /// 요청 취소
        /// </summary>
        private void btnCancelHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel())
                return;

            // % 1 취소 하시겠습니까?
            Util.MessageConfirm("SFU4620", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcess();
                }
            }, ObjectDic.Instance.GetObjectName("요청"));
        }

        /// <summary>
        /// 엑셀
        /// </summary>
        private void btnExcelHis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcel())
                    return;

                new ExcelExporter().Export(dgHistory);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// W/O 조회
        /// </summary>
        private void SearchWorkOrder()
        {
            try
            {
                // Grid Clear
                InitializeControls();
                InitializeGrid();

                if (cboProcess.SelectedValue == null || 
                    cboProcess.SelectedValue.ToString().Equals("SELECT") ||
                    cboEquipment.SelectedValue == null ||
                    cboEquipment.SelectedValue.ToString().Equals("SELECT"))
                {
                    Util.gridClear(dgWorkOrder);
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));
                inTable.Columns.Add("OTHER_EQSGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROC_EQPT_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
                    newRow["EQSGID"] = "";
                else
                    newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROC_EQPT_FLAG"] = "LINE";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWorkOrder, bizResult, null, true);

                        // 현 작업지시 정보 Top Row 처리 및 고정..
                        if (bizResult.Rows.Count > 0)
                        {
                            if (!Util.NVC(bizResult.Rows[0]["EIO_WO_DETL_ID"]).Equals(""))
                                dgWorkOrder.FrozenTopRowsCount = 1;
                            else
                                dgWorkOrder.FrozenTopRowsCount = 0;
                        }

                        // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
                        for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                dgWorkOrder.SelectedIndex = i;
                                break;
                            }
                        }

                        SetWorkOrderGridColumn();

                        // WO BOM 조회
                        SearchBOM();
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

        /// <summary>
        /// W/O BOM 조회
        /// </summary>
        private void SearchBOM()
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgWorkOrder, "CHK");

                if (rowIndex < 0) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WOID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[rowIndex].DataItem, "WOID").ToString());
                newRow["PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_WO_BOM", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWorkOrderMtrl, bizResult, null, true);
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

        /// <summary>
        /// 최대 요청수량 조회   LoginInfo.CFG_AREA_ID;
        /// </summary>
        private double SearchMaxQty(string mtrlID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["MTRLID"] = mtrlID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTR_SEL_TB_MMD_AREA_MTRL_SPLY_ATTR", "INDATA", "OUTDATA", inTable);

                if (dtResult != null &&  dtResult.Rows.Count > 0)
                {
                    return double.Parse(dtResult.Rows[0]["REQ_MAX_QTY"].ToString());
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        /// <summary>
        /// 요청
        /// </summary>
        private void RequestProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_USERID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EMRG_REQ_FLAG", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));

                /////////////////////////////////////////////////////////////////
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgWorkOrder, "CHK");

                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Request;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["MTRL_SPLY_REQ_USERID"] = txtRequestUser.Tag.ToString();
                newRow["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[rowIndex].DataItem, "WO_DETL_ID").ToString());
                newRow["NOTE"] = txtNote.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Request;
                newRow["EMRG_REQ_FLAG"] = (bool)chkEmrgReqFlag.IsChecked ? "Y" : "N";
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[rowIndex].DataItem, "MKT_TYPE_CODE").ToString());
                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgRequest.Rows)
                {
                    newRow = inMtrl.NewRow();
                    newRow["MTRLID"] = DataTableConverter.GetValue(row.DataItem, "MTRLID");
                    newRow["MTRL_SPLY_REQ_QTY"] = DataTableConverter.GetValue(row.DataItem, "MTRL_SPLY_REQ_QTY");
                    inMtrl.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REQUEST_NEW", "INDATA,INMTRL", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 요청되었습니다.
                        Util.MessageInfo("SFU1747");

                        // 재조회
                        InitializeControls();
                        InitializeGrid();
                        SearchBOM();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 요청 이력 조회
        /// </summary>
        private void SearchHisProcess()
        {
            try
            {
                string bizRuleName = (bool)chkMlot.IsChecked ? "DA_MTR_SEL_MATERIAL_REQUEST_HISTORY_MLOT" : "DA_MTR_SEL_MATERIAL_REQUEST_HISTORY";

                // Clear
                InitializeControls();
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("DIFF_QTY", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);
                newRow["AREAID"] = cboAreaHis.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegmentHis.SelectedValue.ToString()) ? null : cboEquipmentSegmentHis.SelectedValue.ToString();
                newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessHis.SelectedValue.ToString()) ? null : cboProcessHis.SelectedValue.ToString();
                newRow["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipmentHis.SelectedValue.ToString()) ? null : cboEquipmentHis.SelectedValue.ToString();
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Request;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);
                        GridColumnVisibility((bool)chkMlot.IsChecked);

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

        /// <summary>
        /// 지재 LOT 조회
        /// </summary>
        private void SearchMLOT()
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgHistory, "CHK");

                if (rowIndex < 0) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                newRow["DEL_FLAG"] = "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_LOADING_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryMlot, bizResult, null);

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

        /// <summary>
        /// 요청 취소
        /// </summary>
        private void CancelProcess()
        {
            try
            {
                try
                {
                    DataSet inDataSet = new DataSet();
                    /////////////////////////////////////////////////////////////////  Table 생성
                    DataTable inTable = inDataSet.Tables.Add("INDATA");
                    inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                    inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("MTRL_SPLY_REQ_USERID", typeof(string));
                    inTable.Columns.Add("WO_DETL_ID", typeof(string));
                    inTable.Columns.Add("NOTE", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("EMRG_REQ_FLAG", typeof(string));
                    inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                    DataTable inMLot = inDataSet.Tables.Add("INMTRL");
                    inMLot.Columns.Add("MTRLID", typeof(string));
                    inMLot.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));

                    /////////////////////////////////////////////////////////////////
                    int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgHistory, "CHK");

                    DataRow newRow = inTable.NewRow();
                    newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                    newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Cancel;
                    newRow["MTRL_SPLY_REQ_USERID"] = txtRequestUserHis.Tag.ToString();
                    newRow["NOTE"] = txtNoteHis.Text;
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);

                    /////////////////////////////////////////////////////////////////

                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REQUEST_NEW", "INDATA,INMTRL", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            // 취소되었습니다.
                            Util.MessageInfo("SFU5032");

                            // 재조회
                            SearchHisProcess();
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 라인을 선택해주세요
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 설비를 선택하세요
                Util.MessageValidation("SFU1153");
                return false;
            }

            return true;
        }

        private bool ValidationRequest()
        {
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 라인을 선택해주세요
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 설비를 선택하세요
                Util.MessageValidation("SFU1153");
                return false;
            }

            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgWorkOrder, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 W/O가 없습니다.
                Util.MessageValidation("SFU1635");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgRequest.ItemsSource).Select();

            if (dr.Length == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            //int rowChkCount = dr.AsEnumerable().Count(r => r.Field<string>("MTRL_SPLY_REQ_QTY") == "0");
            //if (rowChkCount > 0)
            //{
            //    // 요청수량을 입력 하세요.
            //    Util.MessageValidation("SFU4135");
            //    return false;
            //}

            foreach (DataRow row in dr)
            {
                int ReqQty = 0;
                double MaxQty = 0;

                int.TryParse(row["MTRL_SPLY_REQ_QTY"].ToString(), out ReqQty);
                double.TryParse(row["REQ_MAX_QTY"].ToString(), out MaxQty);

                if (ReqQty == 0)
                {
                    // 요청수량을 입력 하세요.
                    Util.MessageValidation("SFU4135");
                    return false;
                }

                if (MaxQty != 0 && ReqQty > MaxQty)
                {
                    // 최대 적재 수량을 초과할 수 없습니다.
                    Util.MessageValidation("SFU5110");
                    return false;
                }
            }

            if (txtRequestUser.Tag == null || string.IsNullOrWhiteSpace(txtRequestUser.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationSearchHis()
        {
            if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboAreaHis.SelectedValue == null || cboAreaHis.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgHistory, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 요청서가 없습니다.
                Util.MessageValidation("SFU1641");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString()).Equals(Mtrl_Request_StatCode.Request))
            {
                string[] Parameters = new string[2];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString());

                // 요청번호 [%1] 상태는 [%2] 입니다. \r\n 요청인 상태만 가능 합니다.
                Util.MessageValidation("SFU8000", Parameters);
                return false;
            }

            if (txtRequestUserHis.Tag == null || string.IsNullOrWhiteSpace(txtRequestUserHis.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationExcel()
        {
            if (dgHistory.Rows.Count - dgHistory.TopRows.Count - dgHistory.BottomRows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
        private void popTakeOver(string ReqID)
        {
            MTRL001_001_TAKEOVER popTakeOver = new MTRL001_001_TAKEOVER();
            popTakeOver.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popTakeOver.Name.ToString()) == false)
                return;

            object[] parameters = new object[1];
            parameters[0] = ReqID;
            C1WindowExtension.SetParameters(popTakeOver, parameters);

            popTakeOver.Closed += new EventHandler(popTakeOver_Closed);
            grdMain.Children.Add(popTakeOver);
            popTakeOver.BringToFront();
        }

        private void popTakeOver_Closed(object sender, EventArgs e)
        {
            MTRL001_001_TAKEOVER popup = sender as MTRL001_001_TAKEOVER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 요청자 팝업
        /// </summary>
        private void popupUser()
        {
            CMM_PERSON popUser = new CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popUser.Name.ToString()) == false)
                return;

            object[] Parameters = new object[1];

            if (((System.Windows.FrameworkElement)tabRequest.SelectedItem).Name.Equals("ctbRequest"))
            {
                Parameters[0] = txtRequestUser.Text;
            }
            else
            {
                Parameters[0] = txtRequestUserHis.Text;
            }

            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);
            grdMain.Children.Add(popUser);
            popUser.BringToFront();
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)tabRequest.SelectedItem).Name.Equals("ctbRequest"))
                {
                    txtRequestUser.Text = popup.USERNAME;
                    txtRequestUser.Tag = popup.USERID;
                    txtNote.Focus();
                }
                else
                {
                    txtRequestUserHis.Text = popup.USERNAME;
                    txtRequestUserHis.Tag = popup.USERID;
                    txtNoteHis.Focus();
                }
            }
            grdMain.Children.Remove(popup);
        }

        #endregion

        #region [Func]

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

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        private void GridColumnVisibility(bool isVisibility = false)
        {
            if (isVisibility)
            {
                dgHistory.Columns["MLOTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgHistory.Columns["MLOTID"].Visibility = Visibility.Collapsed;
            }
        }

        private void SetWorkOrderGridColumn()
        {
            if (dgWorkOrder.Columns.Contains("MDLLOT_ID"))
            {
                if (cboProcess.SelectedValue.ToString().Equals(Process.PACKAGING))
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                else
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
            }

            if (dgWorkOrder.Columns.Contains("ELECTYPE"))
            {
                if (cboProcess.SelectedValue.ToString().Equals(Process.NOTCHING))
                    dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Visible;
                else
                    dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
            }
        }

        private void SetRequest(DataRow drrow)
        {
            DataTable dt = DataTableConverter.Convert(dgRequest.ItemsSource);
            if (dt.Columns.Count == 0)
            {
                // 칼럼 생성
                for (int col = 0; col < dgRequest.Columns.Count; col++ )
                {
                    dt.Columns.Add(dgRequest.Columns[col].Name);
                }
            }

            if (drrow["CHK"].Equals(1))
            {
                // Check
                double ReqQty = 0;
                double ConvQty = 0;
                double DiffQty = 0;
                double UnitQty = 0;
                double MaxQty = 0;

                double.TryParse(Util.NVC(drrow["DIFF_QTY"]), out DiffQty);
                double.TryParse(Util.NVC(drrow["UNIT_QTY"]), out UnitQty);

                DataRow newrow = dt.NewRow();
                newrow["MTRLID"] = drrow["MTRLID"];
                newrow["MTRLDESC"] = drrow["MTRLDESC"];
                newrow["REQ_UNIT_CODE"] = drrow["REQ_UNIT_CODE"];

                MaxQty = SearchMaxQty(drrow["MTRLID"].ToString());

                if (UnitQty > 0)
                {
                    if (DiffQty % UnitQty == 0)
                    {
                        // 나머지가 없다.
                        ReqQty = DiffQty / UnitQty;
                    }
                    else
                    {
                        ReqQty = Math.Round((DiffQty / UnitQty) + 0.5, 0);
                    }

                    if (ReqQty < 0)
                        ReqQty = 0;

                    // 요청수량 > 최대요청수량 이면 최대 요청 수량으로 적용
                    if (MaxQty != 0 && ReqQty > MaxQty)
                        ReqQty = MaxQty;

                    ConvQty = ReqQty * UnitQty;
                }

                newrow["MTRL_SPLY_REQ_QTY"] = ReqQty.ToString();
                newrow["UNIT_QTY"] = drrow["UNIT_QTY"];
                newrow["CONV_QTY"] = ConvQty.ToString();
                newrow["REQ_MAX_QTY"] = MaxQty.ToString();

                dt.Rows.Add(newrow);
            }
            else
            {
                // UnCheck
                dt.Select("MTRLID = '" + drrow["MTRLID"].ToString() + "'").ToList<DataRow>().ForEach(delrow => delrow.Delete());
            }

            dt.AcceptChanges();
            Util.GridSetData(dgRequest, dt, null, true);
        }

        #endregion

        #endregion

        #endregion

    }
}
