/*************************************************************************************
 Created Date : 2017.06.20
      Creator : 신 광희
   Decription : [조립 - 원각 및 초소형 생산영역 UserControl]
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridColumn = C1.WPF.DataGrid.DataGridColumn;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyProduction.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyProduction
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;
        public readonly System.Windows.Threading.DispatcherTimer DispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private readonly SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);

        private string _trayId;
        private string _trayTag;
        private string _outLotId;
        private string _wipQty;
        private DateTime _dtCaldate;
        private bool _isAutoSelectTime = false;
        private bool _isWindingSetAutoTime = false;

        public string EquipmentSegmentCode { get; set; }

        public string EquipmentCode { get; set; }

        public string EquipmentCodeName { get; set; }

        public string ProcessCode { get; set; }

        public string ProdWorkOrderId { get; set; }

        public string ProdWorkOrderDetailId { get; set; }

        public string ProdLotState { get; set; }

        public string ProdLotId { get; set; }

        public string ProdWorkInProcessSequence { get; set; }

        public int ProdSelectedCheckRowIdx { get; set; }

        public bool IsSmallType { get; set; }

        public bool IsRework { get; set; }

        public string CellManagementTypeCode { get; set; }

        public bool IsChangeMaterial { get; set; }

        public bool IsChangeProduct { get; set; }

        public bool IsChangeDefect { get; set; }

        public C1DataGrid DgDefectDetail { get; set; }

        public C1DataGrid DgDefect { get; set; }

        public C1DataGrid DgProductionCellWinding { get; set; }

        public C1DataGrid DgOut { get; set; }

        public Button ButtonCellWindingLocation { get; set; }

        public Button ButtonSaveWipHistory { get; set; }

        public Button ButtonBoxCreate { get; set; }
        //추가
        public Button ButtonVersion { get; set; }

        public LGCDatePicker DtpCaldate { get; set; }

        public RichTextBox TextRemark { get; set; }

        public TextBox TextAddDefectQty { get; set; }

        public TextBox TextWipNote { get; set; }
        //추가
        public TextBox TxtProdVerCode { get; set; }

        public C1NumericBox NumericBoxQty { get; set; }

        private struct PreviewValues
        {
            public string PreviewTray;
            public string PreviewCurentInput;
            private string _previewOutBox;

            public PreviewValues(string tray, string currentInput, string box)
            {
                PreviewTray = tray;
                PreviewCurentInput = currentInput;
                this._previewOutBox = box;
            }
        }

        private PreviewValues _previewValues = new PreviewValues("", "", "");




        public UcAssyProduction()
        {
            InitializeComponent();
            SetControl();
            this.RegisterName("myAnimatedBrush", myAnimatedBrush);
        }
        #endregion

        #region Event

        private void TbDefectDetail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CalculateDefectQty();
        }

        private void dgProductionCellWinding_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    //위치정보 오류(#FFE400)
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_NG")).Equals("NG"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#FFE400");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color) convertFromString);
                    }
                    else
                    {
                        // 배출완료(#E8F7C8)
                        if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color) convertFromString);
                        }
                        else
                        {
                            // 확정(#E6F5FB)
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_TRAY_CNFM_FLAG").GetString() == "Y")
                            {
                                var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                            else
                            {
                                // 미확정(#F8DAC0)
                                var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                    }

                    if (e.Cell.Column.Name.Equals("btnModify"))
                    {
                        if (!string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_TRAY_CNFM_FLAG").GetString(), "Y") && string.IsNullOrEmpty( DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            ((ContentControl) e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                            ((ContentControl) e.Cell.Presenter.Content).Tag = "U";
                            ((ContentControl) e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                            ((ContentControl) e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("조회");
                            ((ContentControl)e.Cell.Presenter.Content).Tag = string.Empty;
                            ((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Black);
                            ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                            ((ContentControl)e.Cell.Presenter.Content).Tag = "X";
                        }
                    }
                }
            }));
        }

        private void dgProductionCellWinding_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_P"].Index)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_M"].Index)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    //else if (e.Cell.Column.Index == dataGrid.Columns["BOXQTY"].Index)
                    //{
                    //    var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                    //    if (convertFromString != null)
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    //}
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;
            SaveDefect();
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //Util.MessageConfirm("SFU1587", result =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveDefect();
            //    }
            //});
        }


        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            try
            {
                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["RESNQTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumDefectQty().GetInt();
                                }
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

        private void btnCellWindingCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DispatcherTimer?.Stop();
                _trayId = string.Empty;
                _trayTag = "C";
                _outLotId = string.Empty;
                _wipQty = string.Empty;
                GetTrayFormLoad();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWindingConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm(dgProductionCellWinding))
                return;

            try
            {
                DispatcherTimer?.Stop();
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTray(dgProductionCellWinding);
                    }
                    else
                    {
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWindingConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirmCancel(dgProductionCellWinding)) return;
            try
            {
                DispatcherTimer?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTrayCancel(dgProductionCellWinding);
                    }
                    else
                    {
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWindingDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemoveTray(dgProductionCellWinding))
                return;

            RemoveTray();
        }

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDischarge(dgProductionCellWinding))
                return;

            try
            {
                DispatcherTimer?.Stop();
                //배출 하시겠습니까?
                Util.MessageConfirm("SFU3613", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DischargeTray(dgProductionCellWinding);
                    }
                    else
                    {
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnGridTraySearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DispatcherTimer?.Stop();

                Button btn = sender as Button;

                DataGridRow row = new DataGridRow();
                IList<FrameworkElement> ilist = btn.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }

                if (ProcessCode.Equals(Process.WINDING))
                {
                    dgProductionCellWinding.SelectedItem = row.DataItem;

                    _trayId = DataTableConverter.GetValue(dgProductionCellWinding.SelectedItem, "TRAYID").GetString();
                    _trayTag = btn.Tag.GetString();
                    _outLotId = DataTableConverter.GetValue(dgProductionCellWinding.SelectedItem, "OUT_LOTID").GetString();
                    _wipQty = DataTableConverter.GetValue(dgProductionCellWinding.SelectedItem, "CELLQTY").GetString();
                }
                else if (ProcessCode.Equals(Process.WASHING))
                {
                    //dgProductionCellWashing.SelectedItem = row.DataItem;
                    //_trayId = DataTableConverter.GetValue(dgProductionCellWashing.SelectedItem, "TRAYID").GetString();
                    //_trayTag = btn.Tag.GetString();
                    //_outLotId = DataTableConverter.GetValue(dgProductionCellWashing.SelectedItem, "OUT_LOTID").GetString();
                    //_wipQty = DataTableConverter.GetValue(dgProductionCellWinding.SelectedItem, "CELLQTY").GetString();
                }

                GetTrayFormLoad();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAutoSearchOutWinding_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (DispatcherTimer != null)
                {
                    DispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutWinding?.SelectedValue != null && !cboAutoSearchOutWinding.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWinding.SelectedValue.ToString());

                    if (iSec == 0 && _isWindingSetAutoTime)
                    {
                        DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isWindingSetAutoTime = true;
                        return;
                    }

                    DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    DispatcherTimer.Start();

                    if (_isWindingSetAutoTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        if (cboAutoSearchOutWinding != null)
                            Util.MessageValidation("SFU1605", cboAutoSearchOutWinding.SelectedValue.GetString());
                    }

                    _isWindingSetAutoTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;
                    if (ProdSelectedCheckRowIdx < 0) return;

                    if (ProcessCode.Equals(Process.WINDING))
                    {
                        if (!CommonVerify.HasDataGridRow(DgProductionCellWinding)) return;
                        GetProductCellList(dgProductionCellWinding);
                    }

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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                IsChangeDefect = true;
                //CalculateDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgProductionCellWinding);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionCellWinding);
        }

        private void dgCurrIn_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        _previewValues.PreviewCurentInput = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (
                                                        box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            _previewValues.PreviewCurentInput = "";
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCurrInLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!ValidationCurrentAutoInputLot())
                        return;

                    string positionId = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string positionName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                    string outLotId = txtCurrInLotID.Text.Trim();

                    object[] parameters = new object[2];
                    parameters[0] = positionName;
                    parameters[1] = txtCurrInLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (string.Equals(ProcessCode, Process.WINDING))
                            {
                                //InputAutoLot(txtCurrInLotID.Text.Trim(), positionId, string.Empty, string.Empty);
                            }
                            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            {
                                //InputAutoLotAssembly(txtCurrInLotID.Text.Trim(), positionId);
                            }
                            else
                            {
                                //TODO 와싱공정진척 KeyDown Event 
                                InputAutoLotWashing(txtCurrInLotID.Text.Trim(), positionId);
                            }

                            txtCurrInLotID.Text = string.Empty;
                        }
                    }, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationCurrentInputCancell())
                    return;

                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable ininDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                DataRow newRow = ininDataTable.NewRow();
                                newRow["WIPNOTE"] = "";
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));

                                ininDataTable.Rows.Add(newRow);
                            }
                        }
                        CurrentInputCancel(ininDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationCurrentInputComplete())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입완료 처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1972", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_END_INPUT_LOT_WS();
                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                            {
                                DataRow dr = inDataTable.NewRow();
                                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                dr["EQPTID"] = EquipmentCode;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                dr["PROD_LOTID"] = ProdLotId;
                                dr["EQPT_LOTID"] = string.Empty;
                                inDataTable.Rows.Add(dr);
                            }
                        }
                        CurrentInputCompleteWashing(inDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationCurrentInputReplace())
                    return;

                if (string.Equals(ProcessCode, Process.WASHING))
                {
                    PopupReplaceWashingRework();
                    /*
                    if (IsRework)
                    {
                        PopupReplaceWashingRework();
                    }
                    else
                    {
                        // Whshing 잔량처리 팝업호출
                        PopupReplaceWashing();
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHistMountPstsID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHistLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInputHistory();
            }
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHistory();
        }

        private void btnInBoxInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationBoxInputCancel(dgInputHist))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInBoxInputSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationBoxInputSave(dgInputHist)) return;
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputSave();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    if (e.Cell.Column.Name.Equals("GOODQTY"))
                    {
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double eqptqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTQTY").GetDouble();
                        double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                        //double adddefectqty = 0;
                        //    adddefectqty = eqptqty - (goodqty + defectqty + lossqty + chargeprdqty) < 0
                        //        ? 0
                        //        : eqptqty - (goodqty + defectqty + lossqty + chargeprdqty - GetEtcDefect().GetDouble());

                        double adddefectqty = 0;
                        adddefectqty = eqptqty - (goodqty + defectqty + lossqty + chargeprdqty - GetEtcDefect().GetDouble());
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);

                        if (string.Equals(ProcessCode, Process.WINDING))
                        {

                            if (adddefectqty.Equals(0))
                                txtAddDefectQty.Text = "0";
                            else
                                txtAddDefectQty.Text = adddefectqty.ToString("##,###");
                        }
                    }
                }
                else
                {
                    double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                    double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                    double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                    double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                    double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);
                }
            }
        }

        private void btnAddDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveAddDefect()) return;

            //추가불량 수량을 반영하시겠습니까?
            Util.MessageConfirm("SFU3658", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveAddDefect();
                }
            });
        }

        private void TbWashingResult_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProdLotId))
                GetWashingResult();
        }

        private void TbBox_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetBoxList();
        }

        private void dgBox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }

        private void btnBoxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxCreate()) return;
            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateBox();
                }
            });
        }

        private void btnBoxInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxInput()) return;
            //투입 하시겠습니까?
            Util.MessageConfirm("SFU1248", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputBox();
                }
            });
        }

        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxDelete()) return;
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteBox();
                }
            });
        }

        private void btnBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidationBoxPrint()) return;
            //발행하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PrintBox();
                }
            });
        }
        private void dgBox_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgBox.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    string checkFlag = DataTableConverter.GetValue(drv, "CHK").GetString();
                    SetBoxButtonEnable(checkFlag == "True" ? e.Cell.Row : null);

                    int rowIndex = 0;
                    foreach (var item in dgBox.Rows)
                    {
                        if (drv["CHK"].GetString() == "True")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
        private void SetControl()
        {
            DgDefect = dgDefect;
            DgDefectDetail = dgDefectDetail;
            DgProductionCellWinding = dgProductionCellWinding;
            //DtpCaldate = dtpCaldate;
            TextRemark = txtRemark;
            TextAddDefectQty = txtAddDefectQty;
            ButtonSaveWipHistory = btnSaveWipHistory;
            dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;
            TextWipNote = txtWipNote;
            NumericBoxQty = BoxQty;
            ButtonBoxCreate = btnBoxCreate;
            TxtProdVerCode = txtProdVerCode;
            ButtonVersion = btnVersion;

            btnInBoxInputSave.Visibility = Visibility.Collapsed;
            dgInputHist.Columns["INPUT_NOTE"].Visibility = Visibility.Collapsed;


        }

        public void SetTabVisibility()
        {
            if (ProcessCode.Equals(Process.WINDING))
            {
                TabWashingResult.Visibility = Visibility.Collapsed;
                TabCurrIn.Visibility = Visibility.Collapsed;
                TabInputHistory.Visibility = Visibility.Collapsed;
                TabBox.Visibility = Visibility.Collapsed;

                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["DEFECTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Collapsed;
                dgProductionCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;
                dgProductionCellWinding.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;

                txtErpSendYn.Visibility = Visibility.Collapsed;
                tbErpSendYn.Visibility = Visibility.Collapsed;
                if (!IsSmallType)
                    tabProdCellWinding.Visibility = Visibility.Collapsed;
                else
                    dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
            }
            else if (ProcessCode.Equals(Process.ASSEMBLY))
            {
                TabCurrIn.Visibility = Visibility.Collapsed;
                TabInputHistory.Visibility = Visibility.Collapsed;
                tabProdCellWinding.Visibility = Visibility.Collapsed;
                TabBox.Visibility = Visibility.Collapsed;

                txtErpSendYn.Visibility = Visibility.Collapsed;
                tbErpSendYn.Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Collapsed;

                tbAddDefect.Visibility = Visibility.Collapsed;
                txtAddDefectQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;
                btnAddDefect.Visibility = Visibility.Collapsed;
            }
            else if (ProcessCode.Equals(Process.WASHING))
            {
                //grdAddDefect.Visibility = Visibility.Collapsed;
                TabWashingResult.Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                
                txtErpSendYn.Visibility = Visibility.Collapsed;
                tbErpSendYn.Visibility = Visibility.Collapsed;
                grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                tbAddDefect.Visibility = Visibility.Collapsed;
                txtAddDefectQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;
                btnAddDefect.Visibility = Visibility.Collapsed;

                btnInBoxInputSave.Visibility = Visibility.Visible;
                dgInputHist.Columns["INPUT_NOTE"].Visibility = Visibility.Visible;

                TabBox.Visibility = Visibility.Visible;
                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;

                if (IsRework)
                {
                    tabProdCellWinding.Visibility = Visibility.Collapsed;
                    TabCurrIn.Visibility = Visibility.Collapsed;
                    TabInputHistory.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TabCurrIn.Visibility = Visibility.Visible;
                    TabInputHistory.Visibility = Visibility.Visible;
                    tabProdCellWinding.Visibility = Visibility.Collapsed;
                }
                /*
                if (IsSmallType)
                {
                    //TabBox.Visibility = Visibility.Collapsed;
                    //TabBox.Visibility = Visibility.Visible;
                    dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Collapsed;

                }
                else
                {
                    //TabBox.Visibility = Visibility.Visible;
                    dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;
                }
                */
            }

            if (tabProdCellWinding.Visibility == Visibility.Visible)
            {
                CommonCombo combo = new CommonCombo();
                
                // 자동 조회 시간 Combo
                String[] sFilter3 = { "MOBILE_TRAY_INTERVAL" };
                combo.SetCombo(cboAutoSearchOutWinding, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

                //String[] sFilter2 = { EquipmentCode, null }; // 자재,제품 전체
                //combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                if (cboAutoSearchOutWinding?.Items != null && cboAutoSearchOutWinding.Items.Count > 0)
                    cboAutoSearchOutWinding.SelectedIndex = 0;

                if (DispatcherTimer != null)
                {
                    int iSec = 0;

                    if (cboAutoSearchOutWinding?.SelectedValue != null && !cboAutoSearchOutWinding.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWinding.SelectedValue.ToString());

                    DispatcherTimer.Tick += _dispatcherTimer_Tick;
                    DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                }
            }

        }

        public void ChangeEquipment(string equipmentCode, string equipmentSegmentCode)
        {
            try
            {
                EquipmentSegmentCode = equipmentSegmentCode;
                EquipmentCode = equipmentCode;

                SetComboBox();
                ClearProductionControl();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearProductionControl()
        {
            Util.gridClear(dgDefectDetail);
            Util.gridClear(dgDefect);
            Util.gridClear(dgProductionCellWinding);
            Util.gridClear(dgWashingResult);
            Util.gridClear(dgCurrIn);
            Util.gridClear(dgInputHist);
            Util.gridClear(dgBox);

            txtAddDefectQty.Text = string.Empty;
            txtWorkOrder.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtWorkMinute.Text = string.Empty;
            txtRemark.Document.Blocks.Clear();
            txtProdId.Text = string.Empty;
            txtProdVerCode.Text = string.Empty;

            BoxQty.Value = 0;
            txtWipNote.Text = string.Empty;

            _trayId = string.Empty;
            _trayTag = string.Empty;
            _outLotId = string.Empty;
            _wipQty = string.Empty;

            IsChangeDefect = false;
            IsChangeMaterial = false;
            IsChangeProduct = false;

            //EquipmentSegmentCode = string.Empty;
            //EquipmentCode = string.Empty;
            ProcessCode = string.Empty;
            ProdWorkOrderId = string.Empty;
            ProdWorkOrderDetailId = string.Empty;
            ProdLotState = string.Empty;
            ProdLotId = string.Empty;
            ProdWorkInProcessSequence = string.Empty;
            CellManagementTypeCode = string.Empty;
            EquipmentCodeName = string.Empty;
            ProdSelectedCheckRowIdx = -1;

            //_prvVlaues = new PrveviewValues("", "", "");
        }

        private void SetComboBox()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { EquipmentCode, "PROD" };
                String[] sFilter2 = { EquipmentCode, null }; // 자재,제품 전체
                combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetCurrInList()
        {
            try
            {
                // 메인 LOT이 없는 경우 disable 처리..
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    btnCurrInCancel.IsEnabled = false;
                    btnCurrInComplete.IsEnabled = false;
                }
                else
                {
                    btnCurrInCancel.IsEnabled = true;
                    btnCurrInComplete.IsEnabled = true;
                }

                string bizRuleName;
                if (ProcessCode.Equals(Process.WINDING))
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                }

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgCurrIn.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                        if(!string.IsNullOrEmpty(_previewValues.PreviewCurentInput))
                        {
                            int idx = _util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", _previewValues.PreviewCurentInput);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgCurrIn.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgCurrIn.SelectedIndex = idx;
                                dgCurrIn.ScrollIntoView(idx, dgCurrIn.Columns["CHK"].Index);
                            }
                        }

                        // WINDING 의 경우 컬럼 다르게 보이도록 수정.
                        if (ProcessCode.Equals(Process.WINDING))
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }

                        if (dgCurrIn.CurrentCell != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.CurrentCell.Row.Index, dgCurrIn.Columns.Count - 1);
                        else if (dgCurrIn.Rows.Count > 0 && dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1) != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void SearchAll()
        {
            try
            {
                if (TabCurrIn.Visibility == Visibility.Visible)
                {
                    GetCurrInList();
                }
                if (TabInputHistory.Visibility == Visibility.Visible)
                {
                    GetInputHistory();
                }
                if (TabBox.Visibility == Visibility.Visible)
                {
                    GetBoxList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetResultDetailControl(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtWorkOrder.Text = rowview["WOID"].GetString();
            txtLotId.Text = rowview["LOTID"].GetString();
            txtProdId.Text = rowview["PRODID"].GetString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].GetString();
            txtEndTime.Text = rowview["WIPDTTM_ED"].GetString();
            txtProdVerCode.Text = rowview["PROD_VER_CODE"].GetString();

            DateTime dTmpEnd;
            DateTime dTmpStart;

            if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                {
                    txtWorkMinute.Text = Math.Truncate(DateTime.Now.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                }
            }

            else if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && !string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtEndTime.Text, out dTmpEnd) && DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
            }

            //if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) &&
            //    !string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            //{
            //    DateTime dTmpEnd;
            //    DateTime dTmpStart;

            //    if (DateTime.TryParse(txtEndTime.Text, out dTmpEnd) && DateTime.TryParse(txtStartTime.Text, out dTmpStart))
            //        txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
            //}

            txtRemark.AppendText(rowview["REMARK"].GetString());
            //dtpCaldate.Text = rowview["CALDATE"].GetString();
            //dtpCaldate.SelectedDateTime = Convert.ToDateTime(rowview["CALDATE"].GetString());
            //_dtCaldate = Convert.ToDateTime(rowview["CALDATE"].GetString());
        }

        protected virtual void GetDefectInfo()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetDefectInfo");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = ProdLotId;
                parameterArrys[1] = ProdWorkInProcessSequence;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void CalculateDefectQty()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("CalculateDefectQty");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void GetTrayFormLoad()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetTrayFormLoad");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    parameterArrys[0] = _trayId;
                    parameterArrys[1] = _trayTag;
                    parameterArrys[2] = _outLotId;
                    parameterArrys[3] = _wipQty;

                    methodInfo.Invoke(UcParentControl, parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetInputMaterialList()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    //Util.GridSetData(dgInputMaterial, searchResult, FrameOperation);

                    /*
                    if (!_prvVlaues.PreviewCurrIn.Equals(""))
                    {
                        int idx = _util.GetDataGridRowIndex(dgInputMaterial, "EQPT_MOUNT_PSTN_ID", _prvVlaues.PreviewCurrIn);

                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgInputMaterial.Rows[idx].DataItem, "CHK", true);

                            //row 색 바꾸기
                            dgInputMaterial.SelectedIndex = idx;
                            dgInputMaterial.ScrollIntoView(idx, dgInputMaterial.Columns["CHK"].Index);
                        }
                    }
                    

                    dgInputMaterial.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                    //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                    dgInputMaterial.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                    dgInputMaterial.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                    */

                    //if (dgInputMaterial.CurrentCell != null)
                    //    dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.CurrentCell.Row.Index, dgInputMaterial.Columns.Count - 1);
                    //else if (dgInputMaterial.Rows.Count > 0 && dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1) != null)
                    //    dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1);

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetMaterialList(C1DataGrid dg, string materialType)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_ASSY_INPUT_MTRL";
                Util.gridClear(dg);
                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_ASSY_INPUT_MTRL();
                DataRow indata = indataTable.NewRow();

                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = ProdLotId;
                indata["MTRLTYPE"] = materialType;
                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        public bool GetHalfProdList()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_HALFPROD_DATA_WN";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_HALFPROD_DATA_WN();
                DataRow indata = indataTable.NewRow();

                indata["LANGID"] = LoginInfo.LANGID;
                indata["PROCID"] = Process.WINDING;
                //indata["LOTID"] = txtProductLotId.Text;
                indata["LOTID"] = string.Empty;
                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

                if (dt.Rows.Count == 1)
                {
                    SetHalfProdAddGrid(dt);
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

        public DataTable GetHalfProductList(string electrodeType, string lotId)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_HALFPROD_LIST_WN";
                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_HALFPROD_LIST_WN();
                DataRow indata = indataTable.NewRow();

                indata["LANGID"] = LoginInfo.LANGID;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["PROCID"] = ProcessCode;
                if (!string.IsNullOrEmpty(electrodeType))
                    indata["ELECTRODETYPE"] = Util.NVC(electrodeType);

                indata["LOTID"] = Util.NVC(lotId);
                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
        }

        public void SetHalfProdAddGrid(DataTable dt)
        {
            /*
            // ADD GRID
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bool isDupplicate = false;
                for (int j = 0; j < dgInputProduct.Rows.Count; j++)
                {
                    if (string.Equals(Convert.ToString(dt.Rows[i]["LOTID"]), Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[j].DataItem, "LOTID"))))
                    {
                        isDupplicate = true;
                        break;
                    }
                }

                if (isDupplicate == false)
                {
                    List<string> bindValue = new List<string>();
                    bindValue.Add("True");
                    bindValue.Add("0"); // SEQ지정해야 함 (현재는 0으로 초기화
                    bindValue.Add(Convert.ToString(dt.Rows[i]["LOTID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["RUNCARDID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["ELECTRODECODE"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["ELECTRODETYPE"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRJT_NAME"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRODID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRODNAME"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["WIPQTY"]));
                    bindValue.Add("0");
                    bindValue.Add("");
                    bindValue.Add(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    GridDataBinding(dgInputProduct, bindValue, false);
                }
            }
            if (!string.IsNullOrEmpty(txtProductLotId.Text)) txtProductLotId.Text = string.Empty;
            */
        }

        public void GetProductCellList(C1DataGrid dg, bool isAsync = true)
        {
            try
            {
                if (isAsync)
                {
                    ShowParentLoadingIndicator();
                }

                SetDataGridCheckHeaderInitialize(dg);
                string bizRuleName = ProcessCode.Equals(Process.WINDING) ? "DA_PRD_SEL_OUT_LOT_LIST_WNS" : "DA_PRD_SEL_OUT_LOT_LIST_WS";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = ProdLotId;
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["EQPTID"] = EquipmentCode;
                indataTable.Rows.Add(indata);

                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", indataTable, (result, ex) =>
                {
                    if (isAsync)
                    {
                        HiddenParentLoadingIndicator();
                    }
                    
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }

                    Util.GridSetData(dg, result, null, true);

                    CalculateDefectQty();

                    if (dg.Rows.Count > 0)
                        dg.GetCell(dg.Rows.Count,1);
                });
            }
            catch (Exception ex)
            {
                if (isAsync)
                {
                    HiddenParentLoadingIndicator();
                }
                
                Util.MessageException(ex);
            }
        }

        private void GetWashingResult()
        {
            try
            {
                Util.gridClear(dgWashingResult);
                ShowParentLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WASHING_LOT_RSLT";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["PROD_LOTID"] = ProdLotId;
                dr["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWashingResult, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }   
        }

        public void GetBoxList()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "DA_PRD_SEL_WAIT_BOX_LIST_WS";
                ShowParentLoadingIndicator();

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LOTID"] = selectedProductRow["LOTID"].GetString();
                indataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        dgBox.ItemsSource = DataTableConverter.Convert(Util.CheckBoxColumnAddTable(searchResult,false));
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void SaveDefectBeforeConfirm()
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }
                string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                IsChangeDefect = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefect(bool bMsgShow = true)
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDefectLot.Rows.Add(newRow);
                }
                string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //if (bMsgShow)
                //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                IsChangeDefect = false;
                GetProductLot();
                //GetDefectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveAddDefect()
        {
            try
            {
                
                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = ((DataView)dgDefect.ItemsSource).Table;

                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("RESNCODE") == "P999000AJR"
                             select new
                             {
                                 ActCode = t.Field<string>("ACTID"),
                                 ResnCode = t.Field<string>("RESNCODE"),
                                 ResnQty = t.Field<decimal>("RESNQTY"),
                                 CostCenterCode = t.Field<string>("COST_CNTR_ID")
                             }).FirstOrDefault();

                if (query == null) return;

                DataTable inDefectLot = indataSet.Tables["INRESN"];
                newRow = inDefectLot.NewRow();
                newRow["LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWorkInProcessSequence;
                newRow["ACTID"] = query.ActCode;
                newRow["RESNCODE"] = query.ResnCode;
                newRow["RESNQTY"] = txtAddDefectQty.Text.GetDecimal();
                newRow["RESNCODE_CAUSE"] = "";
                newRow["PROCID_CAUSE"] = "";
                newRow["RESNNOTE"] = "";
                newRow["DFCT_TAG_QTY"] = 0;
                newRow["LANE_QTY"] = 1;
                newRow["LANE_PTN_QTY"] = 1;
                if (Util.NVC(query.ActCode).Equals("CHARGE_PROD_LOT"))
                {
                    newRow["COST_CNTR_ID"] = query.CostCenterCode;
                }
                else
                {
                    newRow["COST_CNTR_ID"] = "";
                }
                inDefectLot.Rows.Add(newRow);

                string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                IsChangeDefect = false;
                GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();
                //const string bizRuleName = "DA_PRD_UPD_TRAY_CONFIRM_WNS";
                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_WNS";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        ClearProductionControl();
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmTrayCancel(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_CANCEL_WNS";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }
                        ClearProductionControl();
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetMaterialItem(string workOrderId, string itemCode)
        {
            try
            {
                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_ITEM_LIST();
                DataRow indata = indataTable.NewRow();

                indata["WOID"] = Util.NVC(workOrderId);

                if (!string.IsNullOrEmpty(itemCode))
                    indata["ITEMCODE"] = itemCode;

                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ITEM_LIST", "INDATA", "RSLTDT", indataTable);

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
        }

        private void GetTrayInfo(C1DataGrid dg, out string returnMessage, out string messageCode)
        {
            try
            {
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = ProdLotId;
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["EQPTID"] = EquipmentCode;
                indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                indataTable.Rows.Add(indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_WNS", "INDATA", "OUTDATA", indataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    //if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    if (dtResult.Rows[0]["PROC_TRAY_CNFM_FLAG"].GetString() != "Y")
                    {
                        returnMessage = "OK";
                        messageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        messageCode = "SFU1431";
                    }
                }
                else
                {
                    returnMessage = "NG";
                    //sMsg = "존재하지 않습니다.";
                    messageCode = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnMessage = "EXCEPTION";
                messageCode = ex.Message;
            }
        }

        private void SaveTray(C1DataGrid dg)
        {
            try
            {
                DispatcherTimer?.Stop();

                dg.EndEdit();

                const string bizRuleName = "BR_PRD_REG_UPD_OUT_LOT_WS";
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                string specialyn = null;
                string specialDescription = null;
                string specialReasonCode = null;

                if (!ProcessCode.Equals(Process.WINDING))
                {
                    specialyn = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPECIALYN"));
                    specialDescription = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPECIALDESC"));
                    specialReasonCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPCL_RSNCODE"));

                    if (specialyn.Equals("Y"))
                    {
                        if (string.IsNullOrWhiteSpace(specialDescription))
                        {
                            //Util.Alert("특별관리내역을 입력하세요.");
                            Util.MessageValidation("SFU1990");
                            return;
                        }
                    }
                    else if (specialyn.Equals("N"))
                    {
                        if (!string.IsNullOrWhiteSpace(specialDescription))
                        {
                            //Util.Alert("특별관리내역을 삭제하세요.");
                            Util.MessageValidation("SFU1989");
                            return;
                        }
                    }
                }
                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_UPD_OUT_LOT_WS();
                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                newRow = null;

                for (int i = 0; i < dg.Rows.Count - dg.BottomRows.Count; i++)
                {
                    // Tray 정보 DataTable             
                    newRow = inLot.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")));
                    inLot.Rows.Add(newRow);

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specialyn;
                    newRow["SPCL_CST_NOTE"] = specialDescription;
                    newRow["SPCL_CST_RSNCODE"] = specialReasonCode;

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");
                        ClearProductionControl();
                        GetProductLot();
                        //GetProductCellList(dg);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                    DispatcherTimer.Start();
            }
        }

        private void RemoveTray()
        {
            C1DataGrid dg = GetDataGrid();
            if (!ValidationTrayDelete(dg)) return;

            try
            {
                DispatcherTimer?.Stop();

                //string sMsg = "삭제 하시겠습니까?";
                string messageCode = "SFU1230";
                double dCellQty = 0;

                string cellQty = Util.NVC(DataTableConverter.GetValue(dg.Rows[_util.GetDataGridCheckFirstRowIndex(dg, "CHK")].DataItem, "CELLQTY"));

                if (!string.IsNullOrEmpty(cellQty))
                    double.TryParse(cellQty, out dCellQty);

                if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                {
                    //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                    messageCode = "SFU1320";
                }

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteTray(dg);
                    }
                    else
                    {
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DischargeTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();

                const string bizRuleName = "BR_PRD_CHK_CONFIRM_TRAY_WNS";


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("INLOT");
                inInput.Columns.Add("SEQ", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                inInput.Columns.Add("PROD_LOTID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                int seq = 1;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["SEQ"] = seq.GetString();
                        row["CSTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "TRAYID").GetString();
                        row["PROD_LOTID"] = ProdLotId;
                        inInput.Rows.Add(row);
                        seq = seq + 1;
                    }
                }

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,INLOT", "OUT_EQP", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ClearProductionControl();
                        GetWorkOrder();
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();

                string bizRuleName;

                if (ProcessCode.Equals(Process.WINDING))
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WNS";
                else
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS";


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("OUT_LOTID", typeof(string));
                inInput.Columns.Add("TRAYID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["PROD_LOTID"] = ProdLotId;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);


                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["OUT_LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        row["TRAYID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "TRAYID").GetString();
                        inInput.Rows.Add(row);
                    }
                }

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ClearProductionControl();
                        GetWorkOrder();
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateBox()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CREATE_BOX_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CELL_QTY", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["PROD_LOTID"] = ProdLotId;
                dr["USERID"] = LoginInfo.USERID;
                dr["WIPNOTE"] = txtWipNote.Text;
                dr["SHIFT"] = string.Empty;
                dr["WRK_USERID"] = string.Empty;
                dr["WRK_USER_NAME"] = string.Empty;
                dr["CELL_QTY"] = BoxQty.Value;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, ex) =>
                {
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    txtWipNote.Text = string.Empty;
                    BoxQty.Value = 0;
                    GetBoxList();
                    
                });

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputBox()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_INPUT_BOX_LOT_WS";
                DataTable dtEquipment = new DataTable("IN_EQP");
                dtEquipment.Columns.Add("SRCTYPE", typeof(string));
                dtEquipment.Columns.Add("IFMODE", typeof(string));
                dtEquipment.Columns.Add("EQPTID", typeof(string));
                dtEquipment.Columns.Add("USERID", typeof(string));
                dtEquipment.Columns.Add("PROD_LOTID", typeof(string));
                dtEquipment.Columns.Add("EQPT_LOTID", typeof(string));
                DataRow dr = dtEquipment.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = ProdLotId;
                dr["EQPT_LOTID"] = string.Empty;
                dtEquipment.Rows.Add(dr);

                DataTable dtInput = new DataTable("IN_INPUT");
                dtInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                dtInput.Columns.Add("PRODID", typeof(string));
                dtInput.Columns.Add("INPUT_LOTID", typeof(string));
                dtInput.Columns.Add("INPUT_QTY", typeof(decimal));
                DataRow dataRow = dtInput.NewRow();
                dataRow["EQPT_MOUNT_PSTN_ID"] = string.Empty;
                dataRow["EQPT_MOUNT_PSTN_STATE"] = string.Empty;
                dataRow["PRODID"] = string.Empty;
                dataRow["INPUT_LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<string>("LOTID").GetString();
                dataRow["INPUT_QTY"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<decimal>("WIPQTY").GetDecimal();
                dtInput.Rows.Add(dataRow);
                
                DataSet ds = new DataSet();
                ds.Tables.Add(dtEquipment);
                ds.Tables.Add(dtInput);
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetBoxList();
                    Util.MessageInfo("SFU1275");
                }, ds);

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void DeleteBox()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DELETE_BOX_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK",true).Field<string>("LOTID").GetString();
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, ex) =>
                {
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    txtWipNote.Text = string.Empty;
                    BoxQty.Value = 0;
                    GetBoxList();

                });

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PrintBox()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_BOX_RUNCARD_DATA_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<string>("LOTID").GetString();
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    CMM_ASSY_BOXCARD_PRINT poopupHistoryCard = new CMM_ASSY_BOXCARD_PRINT { FrameOperation = this.FrameOperation };
                    object[] parameters = new object[1];
                    parameters[0] = result;
                    C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                    poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);

                    foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (grid.Name == "grdMain")
                        {
                            grid.Children.Add(poopupHistoryCard);
                            poopupHistoryCard.BringToFront();
                            break;
                        }
                    }

                    //Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    //txtWipNote.Text = string.Empty;
                    //BoxQty.Value = 0;
                    //GetBoxList();

                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_BOXCARD_PRINT popup = sender as CMM_ASSY_BOXCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
                    
                
            }
        }

        private void GetInputHistory()
        {
            try
            {
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "DA_PRD_SEL_INPUT_MTRL_HIST_WS" : "DA_PRD_SEL_INPUT_MTRL_HIST";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["PROD_WIPSEQ"] = ProdWorkInProcessSequence.Equals("") ? 1 : Convert.ToDecimal(ProdWorkInProcessSequence);
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.GetString().Equals("") ? null : cboHistMountPstsID.SelectedValue.GetString();

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputHist.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputHist, searchResult, FrameOperation);

                        if (dgInputHist.CurrentCell != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PopupReplaceWashing()
        {
            CMM_ASSY_WSH_PAN_REPLACE popReplace = new CMM_ASSY_WSH_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[5];
            parameters[0] = EquipmentCode;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += new EventHandler(popReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popReplace);
                    popReplace.BringToFront();
                    break;
                }
            }
        }

        private void popReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WSH_PAN_REPLACE pop = sender as CMM_ASSY_WSH_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                    tmp.Children.Remove(pop);
            }
        }

        private void PopupReplaceWashingRework()
        {
            CMM_ASSY_WSH_REWORK_PAN_REPLACE popReplace = new CMM_ASSY_WSH_REWORK_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[6];
            parameters[0] = EquipmentCode;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            parameters[5] = IsRework;
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += new EventHandler(popWashingReworkReplace_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popReplace);
                    popReplace.BringToFront();
                    break;
                }
            }
        }

        private void popWashingReworkReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WSH_REWORK_PAN_REPLACE pop = sender as CMM_ASSY_WSH_REWORK_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetCurrInList();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
            }
        }

        private void CurrentInputCancel(DataTable dt)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT";

                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_LM();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in dt.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CurrentInputCompleteWashing(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_END_INPUT_LOT_WS";
            ShowParentLoadingIndicator();
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetProductLot();
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BoxInputCancel()
        {

            try
            {
                //const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputHist.Rows.Count - dgInputHist.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputHist, "CHK", i)) continue;
                    newRow = null;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetInputHistory();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void BoxInputSave()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "DA_BAS_UPD_INPUT_NOTE_WS";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("INPUT_SEQNO", typeof(string));
                inTable.Columns.Add("INPUT_NOTE", typeof(string));

                for (int i = 0; i < dgInputHist.Rows.Count - dgInputHist.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputHist, "CHK", i)) continue;
                    DataRow dr = inTable.NewRow();
                    dr["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")));
                    dr["INPUT_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_NOTE"));
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, ex) =>
                {
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    GetInputHistory();
                });

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWashing(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WS();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["EQPT_LOTID"] = string.Empty;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;

                inInputTable.Rows.Add(newRow);
                string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                //GetProductLot();
                GetCurrInList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataRow GetSelectProductRow()
        {
            if (UcParentControl == null)
                return null;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetSelectProductRow");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }
                return (DataRow)methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void GetWorkOrder()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetWorkOrder");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                //object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataRow GetSelectWorkOrderRow()
        {
            if (UcParentControl == null)
                return null;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetSelectWorkOrderRow");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                //object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                return (DataRow) methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private decimal GetEtcDefect()
        {
            if (UcParentControl == null)
                return 0;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetEtcDefect");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                //object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                return (decimal)methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (ProdLotId.Length < 1)
            {
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }

        private bool ValidationSaveAddDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (ProdLotId.Length < 1)
            {
                Util.MessageValidation("SFU1195");
                return false;
            }

            if (txtAddDefectQty.Text.GetDecimal() < 0)
            {
                //수량이 0보다 작습니다.
                Util.MessageValidation("SFU1232");
                return false;
            }

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("RESNCODE") == "P999000AJR"
                         select t).ToList();

            if (!query.Any())
            {
                //추가불량 기타 항목이 존재하지 않습니다.
                Util.MessageValidation("SFU3662");
                return false;
            }

            return true;
        }

        private bool ValidationTrayConfirm(C1DataGrid dg)
        {
            try
            {
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                Util.MessageValidation("SFU3616");
                                return false;
                            }

                            // 확정 여부 확인
                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                Util.MessageValidation("SFU1235");
                                return false;
                            }

                            if (item["LOCATION_NG"].GetString() == "NG")
                            {
                                Util.MessageValidation("SFU3638");
                                return false;
                            }

                            double dTmp;
                            if (double.TryParse(Util.NVC(item["CELLQTY"]), out dTmp))
                            {
                                if (dTmp.Equals(0))
                                {
                                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                                    Util.MessageValidation("SFU1685");
                                    return false;
                                }
                            }
                            else
                            {
                                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                                Util.MessageValidation("SFU1687");
                                return false;
                            }

                            string returnMessage;
                            string messageCode;

                            // Tray 현재 작업중인지 여부 확인.
                            GetTrayInfo(dg, out returnMessage, out messageCode);

                            if (returnMessage.Equals("NG"))
                            {
                                Util.MessageValidation(messageCode);
                                return false;
                            }
                            else if (returnMessage.Equals("EXCEPTION"))
                                return false;
                        }
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

        private bool ValidationTrayDelete(C1DataGrid dg)
        {

            try
            {
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataRow selectedWorkOrderRow = GetSelectWorkOrderRow();
                if (selectedWorkOrderRow == null)
                {
                    //Util.Alert("선택된 W/O가 없습니다.");
                    Util.MessageValidation("SFU1635");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 삭제할 수 없습니다.
                                Util.MessageValidation("SFU3619");
                                return false;
                            }

                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 건은 삭제하실 수 없습니다
                                Util.MessageValidation("SFU3621");
                                return false;
                            }
                        }
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

        private bool ValidationDischarge(C1DataGrid dg)
        {
            try
            {
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 Tray만 배출가능 합니다.
                                Util.MessageValidation("SFU3614");
                                return false;
                            }

                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건이 존재합니다.
                                Util.MessageValidation("SFU3620");
                                return false;
                            }

                            if (!string.Equals(Util.NVC(item["LOCATION_NG"]).GetString(), "OK"))
                            {
                                //투입위치 정보를 확인 하세요.
                                Util.MessageValidation("SFU1980");
                                return false;
                            }
                        }
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

        private bool ValidationRemoveTray(C1DataGrid dg)
        {
            try
            {
                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 삭제할 수 없습니다.
                                Util.MessageValidation("SFU3619");
                                return false;
                            }

                            //// 확정 여부 확인
                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 건은 삭제하실 수 없습니다.
                                Util.MessageValidation("SFU3621");
                                return false;
                            }
                        }
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

        private bool ValidationConfirmCancel(C1DataGrid dg)
        {
            try
            {

                if (string.IsNullOrEmpty(ProdLotId))
                {
                    //Util.Alert("선택된 작업대상이 없습니다.");
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                
                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int32>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 확정취소 할 수 없습니다.
                                Util.MessageValidation("SFU3617");
                                return false;
                            }

                            // 확정 여부 확인
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정 상태만 확정 취소할 수 있습니다.
                                Util.MessageValidation("SFU3618");
                                return false;
                            }
                        }
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

        private bool ValidationTrayCellInfo(C1DataGrid dg)
        {
            if (string.IsNullOrEmpty(ProdLotId))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationCurrentInputCancell()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!ProcessCode.Equals(Process.PACKAGING))
            {
                if (Util.NVC(ProdLotId).Equals(""))
                {
                    //Util.Alert("선택된 실적정보가 없습니다.");
                    Util.MessageValidation("SFU1640");
                    return false;
                }
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationCurrentAutoInputLot()
        {
            if (txtCurrInLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if (ProdLotId.Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (ProcessCode.Equals(Process.LAMINATION) || ProcessCode.Equals(Process.STACKING_FOLDING))
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                    {
                        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // %1 에 이미 투입되었습니다.
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                        {
                            Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidationCurrentInputReplace()
        {
            //int iRow = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgCurrIn, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationCurrentInputComplete()
        {
            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgCurrIn, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[rowIndex].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationBoxInputCancel(C1.WPF.DataGrid.C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationBoxInputSave(C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationBoxCreate()
        {
            if (GetSelectProductRow() == null)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }
            return true;
        }

        private bool ValidationBoxInput()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK",true);
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgBox.Rows[rowIndex].DataItem, "LOTID").GetString()))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationBoxDelete()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK",true) < 0 || !CommonVerify.HasDataGridRow(dgBox))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationBoxPrint()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK",true) < 0 || !CommonVerify.HasDataGridRow(dgBox))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private void ShowParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HiddenParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }
                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private C1DataGrid GetDataGrid()
        {
            //if (ProcessCode.Equals(Process.WINDING))
            //{
            //    return dgProductionCellWinding;
            //}
            //else
            //{
            //    return dgProductionCellWashing;
            //}
            return dgProductionCellWinding;
        }

        public static void GridDataBinding(C1DataGrid dataGrid, List<String> bindValues, bool isNewFlag)
        {
            if (dataGrid.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                foreach (DataGridColumn col in dataGrid.Columns)
                    colDt.Columns.Add(col.Name);

                dataGrid.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable inputDt = ((DataView)dataGrid.ItemsSource).Table;
            DataRow inputRow = inputDt.NewRow();

            for (int i = 0; i < inputDt.Columns.Count; i++)
                inputRow[inputDt.Columns[i].Caption] = bindValues[i];

            // ADD DATA
            inputDt.Rows.Add(inputRow);

            if (isNewFlag)
                dataGrid.ItemsSource = DataTableConverter.Convert(inputDt);
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= this.chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += this.chkHeaderAll_Unchecked;
            }
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            if (dg != null)
                            {
                                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                         checkBox.IsChecked.HasValue &&
                                                         !(bool)checkBox.IsChecked))
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;
                                    SetBoxButtonEnable(e.Cell.Row);

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                if (chk != null) chk.IsChecked = false;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                        box.IsChecked.HasValue &&
                                                        (bool)box.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                        SetBoxButtonEnable(null);
                                    }
                                }
                            }
                            break;
                    }
                    if (dg?.CurrentCell != null)
                        //dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, 1);
                    else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }


        private void SetBoxButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    btnBoxInput.IsEnabled = true;
                    btnBoxDelete.IsEnabled = true;

                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "MODE")).Equals("DEL"))
                    {
                        btnBoxInput.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "MODE")).Equals("INS"))
                    {
                        btnBoxDelete.IsEnabled = false;
                    }
                    else
                    {
                        btnBoxInput.IsEnabled = true;
                        btnBoxDelete.IsEnabled = true;
                    }
                }
                else
                {
                    btnBoxInput.IsEnabled = true;
                    btnBoxDelete.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Decimal GetSumDefectQty()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
                return 0;

            decimal defectqty = 0;
            decimal lossqty = 0;
            decimal chargeprdqty = 0;

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            defectqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("DEFECT_LOT") && !w.Field<string>("RSLT_EXCL_FLAG").Equals("Y")).Sum(s => s.Field<Decimal>("RESNQTY"));
            lossqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("LOSS_LOT")).Sum(s => s.Field<Decimal>("RESNQTY"));
            chargeprdqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("CHARGE_PROD_LOT")).Sum(s => s.Field<Decimal>("RESNQTY"));

            return defectqty + lossqty + chargeprdqty;
        }


        #endregion
        public void SetInputHistButtonControls()
        {
            try
            {
                bool bRet = false;
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("ATTRIBUTE1", typeof(string));
                dt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_LOT_CANCEL_TERM_USE";
                dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
                dr["ATTRIBUTE2"] = ProcessCode;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", dt);
                /*
                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE3"]).Trim().Equals("Y"))
                {
                    btnInBoxInputCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                }
                */

                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE5"]).Trim().Equals("Y"))
                {
                    btnInBoxInputCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
