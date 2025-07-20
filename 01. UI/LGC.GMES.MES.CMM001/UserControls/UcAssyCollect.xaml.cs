/*************************************************************************************
 Created Date : 2017.05.23
      Creator : 신 광희
   Decription : [조립 - 원각 및 초소형 Collect UserControl]
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyCollect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyCollect
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

        private bool _isWindingSetAutoTime = false;

        private bool _isWashingSetAutoTime = false;

        public string PreviewValues { get; set; }

        public string EquipmentSegmentCode { get; set; }

        public string EquipmentCode { get; set; }

        public string ProcessCode { get; set; }

        public string ProdWorkOrderId { get; set; }

        public string ProdWorkOrderDetailId { get; set; }

        public string ProdLotState { get; set; }

        public string ProdLotId { get; set; }

        public string ProdWipSeq { get; set; }

        public int ProdSelectedCheckRowIdx { get; set; }

        public bool IsSmallType { get; set; }

        public bool IsChangeMaterial { get; set; }

        public bool IsChangeProduct { get; set; }

        public bool IsChangeDefect { get; set; }

        public string ProdVerCode { get; set; }

        public C1TabItem TabDefect { get; set; }
        public C1TabItem TabInputMaterial { get; set; }
        public C1TabItem TabInputProduct { get; set; }
        public C1TabItem TabProdCellWinding { get; set; }
        public C1TabItem TabProdCellWashing { get; set; }
        public C1TabItem TabReInputWashing { get; set; }
        public C1DataGrid DgDefect { get; set; }
        public C1DataGrid DgInputMaterial { get; set; }
        public C1DataGrid DgInputProduct { get; set; }
        public C1DataGrid DgProductionCell { get; set; }
        public C1DataGrid DgProductionCellWinding { get; set; }
        public C1DataGrid DgProductionCellWashing { get; set; }

        public Button ButtonDefectRefresh { get; set; }
        public Button ButtonDefectSave { get; set; }
        public Button ButtonMaterialSearch { get; set; }
        public Button ButtonMaterialDelete { get; set; }
        public Button ButtonMaterialSave { get; set; }
        public Button ButtonProductSearch { get; set; }
        public Button ButtonProductWaitLot { get; set; }
        public Button ButtonProductCancel { get; set; }
        public Button ButtonProductSave { get; set; }
        public Button ButtonCellWashingTrayCreate { get; set; }
        public Button ButtonCellWashingTrayRemove { get; set; }
        public Button ButtonCellWashingTrayCancel { get; set; }
        public Button ButtonCellWashingTrayConfirm { get; set; }
        public Button ButtonCellWashingOutSave { get; set; }
        public Button ButtonCellWindingLocation { get; set; }
        public Button ButtonCellWashingTrayCell { get; set; }
        public C1ComboBox ComboAutoSearchWinding { get; set; }

        public TextBox TextProductLotId { get; set; }

        private struct PrveviewValues
        {
            public string PreviewOutTray;
            public string PreviewCurrIn;
            public string PreviewOutBox;

            public PrveviewValues(string sTray, string sIn, string sBox)
            {
                this.PreviewOutTray = sTray;
                this.PreviewCurrIn = sIn;
                this.PreviewOutBox = sBox;
            }
        }

        private PrveviewValues _prvVlaues = new PrveviewValues("", "", "");

        public UcAssyCollect()
        {
            InitializeComponent();
            SetControl();
            PreviewValues = string.Empty;
            this.RegisterName("myAnimatedBrush", myAnimatedBrush);
        }
        #endregion

        #region Event

        private void dgProductionCellWinding_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductionCellWinding_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            if (e?.Row != null)
            {
                if (e.Row.Index < 0 || e.Column.Name != "CHK") return;

                int rowIndex = e.Row.Index;

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (i == rowIndex && !Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK")))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                    }
                }

            }
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
                    //LOCATION_NG
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_NG")).Equals("NG"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color) convertFromString);
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("PROC"))
                        {
                            var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("EQPT_END"))
                        {
                            var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("PROC"))
                    //{
                    //    var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
                    //    if (convertFromString != null)
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    //}
                    //else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("EQPT_END"))
                    //{
                    //    var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                    //    if (convertFromString != null)
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Background = null;
                    //}

                    /*
                    if (e.Cell.Column.Name.Equals("btnModify"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("EQPT_END"))
                        {
                            //((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                            //((ContentControl)e.Cell.Presenter.Content).Tag = "U";
                            //((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                            //((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            //((ContentControl)e.Cell.Presenter.Content).Content = string.Empty;
                            //((ContentControl)e.Cell.Presenter.Content).Tag = string.Empty;
                            //((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                            //((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("조회");
                            //((ContentControl)e.Cell.Presenter.Content).Tag = "X";
                            //((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        }
                    }
                    */
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

        private void dgProductionCellWinding_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
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

        private void dgProductionCellWashing_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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
                                var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                    box.IsChecked.HasValue &&
                                                    !(bool)box.IsChecked))
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        // 이전 값 저장.
                                        PreviewValues = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));
                                        SetOutTrayButtonEnable(e.Cell.Row);

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var checkBox = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (checkBox != null)
                                                        checkBox.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             (bool)checkBox.IsChecked))
                                    {
                                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                            PreviewValues = string.Empty;
                                            // 확정 시 저장, 삭제 버튼 비활성화
                                            SetOutTrayButtonEnable(null);
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                }
            }));
        }

        private void btnMaterialSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMaterialList(dgInputMaterial, "MTRL");
        }

        private void btnMaterialDelete_Click(object sender, RoutedEventArgs e)
        {
            int selectedRowCount = _util.GetDataGridCheckCnt(dgInputMaterial, "CHK");

            if (selectedRowCount == 0)
            {
                //선택된 자재 정보가 없습니다.
                Util.MessageValidation("SFU1643");
                return;
            }

            RemoveMaterialList();
        }

        private void btnMaterialSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveMaterial()) return;
            SaveMaterialList();
        }

        private void txtMaterialLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox txtBox = sender as TextBox;

                if (txtBox != null && string.IsNullOrEmpty(txtBox.Text.Trim()))
                    return;

                for (int i = 0; i < dgInputMaterial.Rows.Count; i++)
                {
                    if (txtBox != null && string.Equals(txtBox.Text, Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "LOTID"))))
                    {
                        //동일한 자재LOT ID가 이미 입력되어 있습니다.
                        Util.MessageValidation("SFU1507");
                        dgInputMaterial.SelectedIndex = i;
                        return;
                    }
                }

                List<string> bindSet = new List<string>(new string[] { "True", "0", "", "", "", txtBox.Text, "0", "", DateTime.Now.ToString(CultureInfo.InvariantCulture) }); // SEQ는 현재 0으로 처리

                GridDataBinding(dgInputMaterial, bindSet, false);
                txtBox.Text = string.Empty;
            }
        }

        private void dgInputMaterial_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            if (e?.Row != null)
            {
                if (e.Row.Index < 0 || e.Column.Name != "CHK") return;

                int rowIndex = e.Row.Index;

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (i == rowIndex && !Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK")))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
        }

        private void dgInputMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            if (CommonVerify.HasDataGridRow(dgInputMaterial))
            {
                IsChangeMaterial = true;

                if (e != null && string.Equals(e.Cell.Column.Name, "ITEMID") && !string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                {
                    DataTable dt = GetMaterialItem(ProdWorkOrderId, Util.NVC(e.Cell.Value));

                    if (dt.Rows.Count > 0)
                    {
                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "ITEMNAME", dt.Rows[0]["ITEMDESC"]);
                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "UNIT", dt.Rows[0]["UNIT"]);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "ITEMNAME", "");
                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "UNIT", "");
                    }
                    dg.EndEditRow(true);
                }
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect();
                }
            });
        }

        private void btnProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHalfProductList(dgInputProduct, "PROD");
        }

        private void btnProductCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemoveHalfProduct()) return;
            RemoveHalfProdList();
        }

        private void btnProductSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveHalfProduct()) return;
            SaveHalfProdList();
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

        private void btnCellWindingDelete_Click(object sender, RoutedEventArgs e)
        {
            RemoveTray();
        }

        private void btnCellWashingTrayCell_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = GetDataGrid();
            if (!ValidationTrayCellInfo(dg)) return;

            DispatcherTimer?.Stop();

            int rowIdx = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
            _trayId = DataTableConverter.GetValue(dg.Rows[rowIdx], "TRAYID").GetString();
            _trayTag = "U";
            _outLotId = DataTableConverter.GetValue(dg.Rows[rowIdx], "OUT_LOTID").GetString();
            _wipQty = DataTableConverter.GetValue(dg.Rows[rowIdx], "CELLQTY").GetString();
                
            GetTrayFormLoad();
        }

        private void btnOutTraySplSaveWashing_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveSpecialTray()) return;
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSpecialTray();
                }
            });
        }

        private void btnCellWashingTrayConfirm_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = GetDataGrid();

            if (!ValidationTrayConfirm(dg))
                return;

            try
            {
                DispatcherTimer?.Stop();

                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTray(dg);
                    }
                    else
                    {
                        if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                            DispatcherTimer.Start();
                    }
                });
                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //finally
            //{
            //    if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
            //        DispatcherTimer.Start();
            //}
        }

        private void btnGridTraySearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DispatcherTimer?.Stop();

                Button btn = sender as Button;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
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
                    dgProductionCellWashing.SelectedItem = row.DataItem;

                    _trayId = DataTableConverter.GetValue(dgProductionCellWashing.SelectedItem, "TRAYID").GetString();
                    _trayTag = btn.Tag.GetString();
                    _outLotId = DataTableConverter.GetValue(dgProductionCellWashing.SelectedItem, "OUT_LOTID").GetString();
                    _wipQty = DataTableConverter.GetValue(dgProductionCellWinding.SelectedItem, "CELLQTY").GetString();
                }

                GetTrayFormLoad();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWashingTrayCreate_Click(object sender, RoutedEventArgs e)
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

        private void btnCellWashingOutSave_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = GetDataGrid();
            if (!ValidationSaveTray(dg)) return;
            SaveTray(dg);
        }

        private void btnCellWashingTrayRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveTray();
        }

        private void btnCellWashingTrayCancel_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = GetDataGrid();
            if(!ValidationConfirmCancel(dg)) return;

            try
            {
                DispatcherTimer?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTrayCancel(dg);
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

        private void btnProductWaitLot_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = GetDataGrid();
            //if (!ValidationTrayDelete(dg)) return;
            if (string.IsNullOrEmpty(ProdLotId))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
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

        private void cboAutoSearchOutWashing_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (DispatcherTimer != null)
                {
                    DispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutWashing?.SelectedValue != null && !cboAutoSearchOutWashing.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWashing.SelectedValue.ToString());

                    if (iSec == 0 && _isWashingSetAutoTime)
                    {
                        DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isWashingSetAutoTime = true;
                        return;
                    }

                    DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    DispatcherTimer.Start();

                    if (_isWashingSetAutoTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        if (cboAutoSearchOutWashing != null)
                            Util.MessageValidation("SFU1605", cboAutoSearchOutWashing.SelectedValue.GetString());
                    }

                    _isWashingSetAutoTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkOutTraySplWashing_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrkWashing != null)
            {
                txtOutTrayReamrkWashing.Text = string.Empty;
            }
        }

        private void dgInputProduct_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            if (e?.Row != null)
            {
                if (e.Row.Index < 0 || e.Column.Name != "CHK") return;

                int rowIndex = e.Row.Index;

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (i == rowIndex && !Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK")))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                    }
                }
                dg.EndEdit();
                dg.EndEditRow(true);
            }

        }

        private void dgInputProduct_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e?.Column != null)
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                if (e.Column.Name == "CHK")
                {
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        e.Cancel = e.Column.Name != "CHK" && drv["CHK"].GetString() != "True";
                    }
                    else
                    {
                        if (e.Column.Name == "CHK" && (drv != null && drv["ELECTRODECODE"].GetString() != "C"))
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }

            }
        }

        private void dgInputProduct_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name != "CHK")
                {
                    IsChangeProduct = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
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

        private void dgProductionCellWashing_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
                /*
                if (e.Cell.Column.Name.Equals("btnModify"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        // Tray 수정가능
                        ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                        ((ContentControl)e.Cell.Presenter.Content).Tag = "U";
                        ((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                        ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        //((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content)
                    }
                    else
                    {
                        // Tray 조회만
                        ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("조회");
                        ((ContentControl)e.Cell.Presenter.Content).Tag = "X";
                        ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                    }
                }
                */
            }));
        }

        private void dgProductionCellWashing_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

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

        private void dgProductionCellWashing_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
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
                    else if (ProcessCode.Equals(Process.WASHING))
                    {
                        if (!CommonVerify.HasDataGridRow(DgProductionCellWashing)) return;
                        GetProductCellList(dgProductionCellWashing);
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

        #endregion

        #region Mehod
        private void SetControl()
        {
            TabDefect = tabDefect;
            TabInputMaterial = tabInputMaterial;
            TabInputProduct = tabInputProduct;
            TabProdCellWinding = tabProdCellWinding;
            TabProdCellWashing = tabProdCellWashing;
            TabReInputWashing = tabReInputWashing;

            DgDefect = dgDefect;
            DgInputMaterial = dgInputMaterial;
            DgInputProduct = dgInputProduct;
            DgProductionCellWinding = dgProductionCellWinding;
            DgProductionCellWashing = dgProductionCellWashing;

            ButtonDefectRefresh = btnDefectRefresh;
            ButtonDefectSave = btnDefectSave;
            ButtonMaterialSearch = btnMaterialSearch;
            ButtonMaterialDelete = btnMaterialDelete;
            ButtonMaterialSave = btnMaterialSave;
            ButtonProductSearch = btnProductSearch;
            ButtonProductWaitLot = btnProductWaitLot;
            ButtonProductCancel = btnProductCancel;
            ButtonProductSave = btnProductSave;
            ButtonCellWashingTrayCreate = btnCellWashingTrayCreate;
            ButtonCellWashingTrayRemove = btnCellWashingTrayRemove;
            ButtonCellWashingTrayCancel = btnCellWashingTrayCancel;
            ButtonCellWashingTrayConfirm = btnCellWashingTrayConfirm;
            ButtonCellWashingOutSave = btnCellWashingOutSave;
            ButtonCellWindingLocation = btnCellWindingLocation;
            ButtonCellWashingTrayCell = btnCellWashingTrayCell;

            ComboAutoSearchWinding = cboAutoSearchOutWinding;
            TextProductLotId = txtProductLotId;
        }

        public void SetTabVisibility()
        {
            if (ProcessCode.Equals(Process.WINDING))
            {
                tabProdCellWashing.Visibility = Visibility.Collapsed;
                tabReInputWashing.Visibility = Visibility.Collapsed;

                dgInputProduct.Columns["RUNCARDID"].Visibility = Visibility.Collapsed;
                dgInputProduct.Columns["SLOTNO"].Visibility = Visibility.Collapsed;
                dgInputMaterial.Columns["SLOTNO"].Visibility = Visibility.Collapsed;
                dgProductionCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;
                dgProductionCellWinding.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
            }
            else if (ProcessCode.Equals(Process.ASSEMBLY))
            {
                tabInputMaterial.Visibility = Visibility.Collapsed;
                tabProdCellWinding.Visibility = Visibility.Collapsed;
                tabProdCellWashing.Visibility = Visibility.Collapsed;
                tabReInputWashing.Visibility = Visibility.Collapsed;

                dgInputProduct.Columns["RUNCARDID"].Visibility = Visibility.Collapsed;
                dgInputProduct.Columns["SLOTNO"].Visibility = Visibility.Collapsed;
            }
            else if (ProcessCode.Equals(Process.WASHING))
            {
                tabInputMaterial.Visibility = Visibility.Collapsed;
                tabInputProduct.Visibility = Visibility.Collapsed;
                tabProdCellWinding.Visibility = Visibility.Collapsed;

                dgInputProduct.Columns["RUNCARDID"].Visibility = Visibility.Collapsed;
                dgInputProduct.Columns["SLOTNO"].Visibility = Visibility.Collapsed;
            }

            if (tabProdCellWinding.Visibility == Visibility.Visible)
            {
                CommonCombo combo = new CommonCombo();
                
                // 자동 조회 시간 Combo
                String[] sFilter3 = { "MOBILE_TRAY_INTERVAL" };
                combo.SetCombo(cboAutoSearchOutWinding, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

                if (cboAutoSearchOutWinding?.Items != null && cboAutoSearchOutWinding.Items.Count > 0)
                    cboAutoSearchOutWinding.SelectedIndex = 0;

                if (DispatcherTimer != null)
                {
                    int iSec = 0;

                    if (cboAutoSearchOutWinding?.SelectedValue != null && !cboAutoSearchOutWinding.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWinding.SelectedValue.ToString());

                    DispatcherTimer.Tick += _dispatcherTimer_Tick;
                    DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    //dispatcherTimer.Start();
                }
            }
            //Washing 공정진척
            if (tabProdCellWashing.Visibility == Visibility.Visible)
            {
                CommonCombo combo = new CommonCombo();

                // 특별 TRAY  사유 Combo
                String[] sFilter3 = { "SPCL_RSNCODE" };
                combo.SetCombo(cboOutTraySplReasonWashing, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

                if (cboOutTraySplReasonWashing?.Items != null && cboOutTraySplReasonWashing.Items.Count > 0)
                    cboOutTraySplReasonWashing.SelectedIndex = 0;

                // 자동 조회 시간 Combo
                String[] sFilter4 = { "MOBILE_TRAY_INTERVAL" };
                combo.SetCombo(cboAutoSearchOutWashing, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

                if (cboAutoSearchOutWashing?.Items != null && cboAutoSearchOutWashing.Items.Count > 0)
                    cboAutoSearchOutWashing.SelectedIndex = 0;

                if (DispatcherTimer != null)
                {
                    int iSec = 0;

                    if (cboAutoSearchOutWashing?.SelectedValue != null && !cboAutoSearchOutWashing.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWashing.SelectedValue.ToString());

                    DispatcherTimer.Tick += _dispatcherTimer_Tick;
                    DispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                }
            }
        }

        public void ClearCollectControl()
        {
            Util.gridClear(dgDefect);
            Util.gridClear(dgInputMaterial);
            Util.gridClear(dgInputProduct);
            Util.gridClear(dgProductionCellWinding);
            Util.gridClear(dgProductionCellWashing);

            txtMaterialLotId.Text = string.Empty;
            txtProductLotId.Text = string.Empty;
            txtOutTrayReamrkWashing.Text = string.Empty;

            _trayId = string.Empty;
            _trayTag = string.Empty;
            _outLotId = string.Empty;
            _wipQty = string.Empty;

            IsChangeDefect = false;
            IsChangeMaterial = false;
            IsChangeProduct = false;

            EquipmentSegmentCode = string.Empty;
            EquipmentCode = string.Empty;
            ProcessCode = string.Empty;
            ProdWorkOrderId = string.Empty;
            ProdWorkOrderDetailId = string.Empty;
            ProdLotState = string.Empty;
            ProdLotId = string.Empty;
            ProdWipSeq = string.Empty;
            ProdSelectedCheckRowIdx = -1;
            ProdWorkOrderId = string.Empty;
            ProdVerCode = string.Empty;

            _prvVlaues = new PrveviewValues("", "", "");
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
                parameterArrys[1] = ProdWipSeq;

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
                ParameterInfo[] parameters = methodInfo.GetParameters();
                object[] parameterArrys = new object[parameters.Length];

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
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
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = _trayId;
                parameterArrys[1] = _trayTag;
                parameterArrys[2] = _outLotId;
                parameterArrys[3] = _wipQty;

                methodInfo.Invoke(UcParentControl, parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetInputMaterialList()
        {
            if (string.IsNullOrEmpty(ProdLotId))
            {
                btnMaterialDelete.IsEnabled = false;
            }
            else
            {
                btnMaterialDelete.IsEnabled = true;
            }

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

                    Util.GridSetData(dgInputMaterial, searchResult, FrameOperation);

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

                    if (dgInputMaterial.CurrentCell != null)
                        dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.CurrentCell.Row.Index, dgInputMaterial.Columns.Count - 1);
                    else if (dgInputMaterial.Rows.Count > 0 && dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1) != null)
                        dgInputMaterial.CurrentCell = dgInputMaterial.GetCell(dgInputMaterial.Rows.Count, dgInputMaterial.Columns.Count - 1);

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

                //if (CommonVerify.HasTableRow(dt))
                //{
                //    Util.GridSetData(dg, dt, null, true);
                //}
                //else
                //{
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //}
                
                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RemoveMaterialList()
        {
            Util.MessageConfirm("SFU2974", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowParentLoadingIndicator();

                        string bizRuleName = string.Empty;
                        if (string.Equals(ProcessCode, Process.WINDING))
                            bizRuleName = "BR_PRD_DEL_INPUT_MTRL_WN";
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            bizRuleName = "BR_PRD_DEL_INPUT_MTRL_AS";
                        else if (string.Equals(ProcessCode, Process.WASHING))
                            bizRuleName = "BR_PRD_DEL_INPUT_MTRL_WS";

                        DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_ITEM_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(EquipmentCode);
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = ProdLotId;
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < dgInputMaterial.GetRowCount(); i++)
                        {
                            // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                            if (_util.GetDataGridCheckValue(dgInputMaterial, "CHK", i) == true && !string.Equals(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                            {
                                row = inInputTable.NewRow();
                                row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "LOTID"));

                                inInputTable.Rows.Add(row);
                            }
                        }

                        if (inInputTable.Rows.Count > 0)
                        {
                            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                            {
                                HiddenParentLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                //RemoveGridRow(dgInputMaterial);
                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                                GetMaterialList(dgInputMaterial, "MTRL");


                            }, inDataSet);
                        }
                        else
                        {
                            //RemoveGridRow(dgInputMaterial);
                            GetMaterialList(dgInputMaterial, "MTRL");
                        }

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
            });
        }

        private void SaveMaterialList()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 정보를 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2976", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowParentLoadingIndicator();

                        string bizRuleName = string.Empty;

                        if (string.Equals(ProcessCode, Process.WINDING))
                            bizRuleName = "BR_PRD_REG_INPUT_MTRL_WN";
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            bizRuleName = "BR_PRD_REG_INPUT_MTRL_AS";
                        else if (string.Equals(ProcessCode, Process.WASHING))
                            bizRuleName = "BR_PRD_REG_INPUT_MTRL_WS";

                        DataSet inDataSet = _bizDataSet.GetBR_PRD_REG_INPUT_MTRL_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = EquipmentCode;
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = ProdLotId; //Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < dgInputMaterial.GetRowCount(); i++)
                        {
                            row = inInputTable.NewRow();
                            row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "SLOTNO"));
                            row["EQPT_MOUNT_PSTN_STATE"] = "A";
                            row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "ITEMID"));
                            row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "LOTID"));
                            row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUTQTY"));

                            inInputTable.Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
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
                                GetMaterialList(dgInputMaterial, "MTRL");

                                IsChangeMaterial = false;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenParentLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
        }

        private void RemoveGridRow(C1DataGrid dg)
        {
            try
            {
                // REMOVE ROW
                for (int i = dg.GetRowCount(); i >= 0; i--)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    if (_util.GetDataGridCheckValue(dg, "CHK", i) == true)
                    {
                        dt.Rows[i].Delete();
                    }
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetHalfProductList(C1DataGrid dg, string materialType)
        {
            try
            {
                string bizRuleName;
                Util.gridClear(dg);
                if (string.Equals(ProcessCode, Process.WINDING))
                    bizRuleName = "DA_PRD_SEL_INPUT_HALFPROD_WN";
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                    bizRuleName = "DA_PRD_SEL_INPUT_HALFPROD_AS";
                else
                    bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_INPUT_HALFPROD();

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = ProdLotId;
                indata["MTRLTYPE"] = materialType;
                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

                dg.ItemsSource = DataTableConverter.Convert(dt);

                //if (CommonVerify.HasTableRow(dt))
                //{
                //    Util.GridSetData(dg, dt, null, true);
                //}
                //else
                //{
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //}

                //if (dt.Rows.Count < 1)
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //else
                //    Util.GridSetData(dg, dt, null, true);


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
                indata["LOTID"] = txtProductLotId.Text;
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
        }

        private void RemoveHalfProdList()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 삭제하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2970", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowParentLoadingIndicator();

                        string bizRuleName = string.Empty;
                        if (string.Equals(ProcessCode, Process.WINDING))
                            bizRuleName = "BR_PRD_DEL_INPUT_LOT_WN";
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            bizRuleName = "BR_PRD_DEL_INPUT_LOT_AS";
                        else if (string.Equals(ProcessCode, Process.WASHING))
                            bizRuleName = "BR_PRD_DEL_INPUT_LOT_WS";

                        DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_ITEM_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = EquipmentCode;
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = ProdLotId;
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
                        {
                            // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                            if (_util.GetDataGridCheckValue(dgInputProduct, "CHK", i) == true &&
                                 !string.Equals(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                            {
                                row = inInputTable.NewRow();
                                row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUT_SEQNO"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "LOTID"));

                                inInputTable.Rows.Add(row);
                            }
                        }
                        
                        if (inInputTable.Rows.Count > 0)
                        {
                            string xmlText = inDataSet.GetXml();

                            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                            {
                                HiddenParentLoadingIndicator();
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                //RemoveGridRow(dgInputProduct);
                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                                btnProductSearch_Click(btnProductSearch,null);
                            }, inDataSet);
                        }
                        else
                        {
                            btnProductSearch_Click(btnProductSearch, null);
                            //RemoveGridRow(dgInputProduct);
                        }

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
            });
        }

        private void SaveHalfProdList()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2972", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowParentLoadingIndicator();

                        string bizRuleName = "";
                        if (string.Equals(ProcessCode, Process.WINDING))
                            bizRuleName = "BR_PRD_REG_INPUT_LOT_WN";
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";
                        else if (string.Equals(ProcessCode, Process.WASHING))
                            bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                        DataSet inDataSet = null;
                        string outData = string.Empty;
                        
                        if (string.Equals(ProcessCode, Process.WINDING))
                        {
                            inDataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WN();

                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                            DataRow row = inDataTable.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["EQPTID"] = EquipmentCode;
                            row["USERID"] = LoginInfo.USERID;
                            row["PROD_LOTID"] = ProdLotId;
                            inDataTable.Rows.Add(row);

                            DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                            for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
                            {
                                row = inInputTable.NewRow();
                                row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "SLOTNO"));
                                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "LOTID"));
                                row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUTQTY"));

                                inInputTable.Rows.Add(row);
                            }
                            outData = null;
                        }
                        else if(string.Equals(ProcessCode, Process.ASSEMBLY))
                        {
                            inDataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_AS();

                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                            DataRow row = inDataTable.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["EQPTID"] = EquipmentCode;
                            row["USERID"] = LoginInfo.USERID;
                            row["PROD_LOTID"] = ProdLotId;
                            inDataTable.Rows.Add(row);

                            DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                            for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
                            {
                                row = inInputTable.NewRow();
                                row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "SLOTNO"));
                                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"));
                                row["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "RUNCARDID"));
                                row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUTQTY"));

                                inInputTable.Rows.Add(row);
                            }
                            outData = "OUT_EQP";
                        }
                        else //(string.Equals(ProcessCode, Process.WASHING))
                        {
                            inDataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WS();

                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                            DataRow row = inDataTable.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["EQPTID"] = EquipmentCode;
                            row["USERID"] = LoginInfo.USERID;
                            row["PROD_LOTID"] = ProdLotId;
                            row["EQPT_LOTID"] = string.Empty;
                            inDataTable.Rows.Add(row);

                            DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                            for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
                            {
                                row = inInputTable.NewRow();
                                row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "SLOTNO"));
                                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                                //row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "LOTID"));
                                row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUTQTY"));

                                inInputTable.Rows.Add(row);
                            }
                            outData = null;
                        }

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", outData, (bizResult, bizException) =>
                        {
                            HiddenParentLoadingIndicator();
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                            GetHalfProductList(dgInputProduct, "PROD");

                            IsChangeProduct = false;
                        }, inDataSet);
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
            });
        }

        public void GetProductCellList(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();
                string bizRuleName = ProcessCode.Equals(Process.WINDING) ? "DA_PRD_SEL_OUT_LOT_LIST_WNM" : "DA_PRD_SEL_OUT_LOT_LIST_WS";

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
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    Util.GridSetData(dg, AddVisibilityColumn(result), null, true);
                    //dg.ItemsSource = DataTableConverter.Convert(result);
                    CalculateDefectQty();

                    if (dg.Rows.Count > 0)
                        dg.GetCell(dg.Rows.Count,1);

                    if (ProcessCode.Equals(Process.WINDING))
                        return;

                    // 특별TRAY 콤보
                    DataTable dtcbo = new DataTable();
                    dtcbo.Columns.Add("CODE");
                    dtcbo.Columns.Add("NAME");

                    dtcbo.Rows.Add("N", "N");
                    dtcbo.Rows.Add("Y", "Y");

                    var dataGridComboBoxColumn = dg.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                    if (dataGridComboBoxColumn != null)
                        dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dtcbo.Copy());

                    if (!ProcessCode.Equals(Process.WINDING))
                    {
                        // 사유 콤보
                        DataTable dtReason = SetOutTraySplReasonCommonCode();
                        var gridComboBoxColumn = dg.Columns["SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                        if (gridComboBoxColumn != null)
                            gridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dtReason.Copy());
                    }
                });

                /*
                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

                //Util.GridSetData(dg, dt, null, true);
                dg.ItemsSource = DataTableConverter.Convert(dt);

                if (dg.Rows.Count > 0)
                    dg.GetCell(0, 0);

                if (ProcessCode.Equals(Process.WINDING))
                    return;

                // 특별TRAY 콤보
                DataTable dtcbo = new DataTable();
                dtcbo.Columns.Add("CODE");
                dtcbo.Columns.Add("NAME");

                dtcbo.Rows.Add("N", "N");
                dtcbo.Rows.Add("Y", "Y");

                var dataGridComboBoxColumn = dg.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dtcbo.Copy());

                // 사유 콤보
                DataTable dtReason = SetOutTraySplReasonCommonCode();
                var gridComboBoxColumn = dg.Columns["SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                if (gridComboBoxColumn != null)
                    gridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dtReason.Copy());

                */
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private static DataTable AddVisibilityColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "VisibilityText", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "VisibilityButton", DataType = typeof(string) });

            foreach (DataRow row  in dtBinding.Rows)
            {
                if (row["FORM_MOVE_STAT_CODE"].GetString() == "EQPT_END")
                {
                    row["VisibilityText"] = "Collapsed";
                    row["VisibilityButton"] = "Visible";
                }
                else
                {
                    row["VisibilityText"] = "Visible";
                    row["VisibilityButton"] = "Collapsed";
                }
            }
            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private void SaveDefect(bool bMsgShow = true)
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_ALL";
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
                    newRow = null;

                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWipSeq;
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

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                IsChangeDefect = false;
                GetDefectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveSpecialTray()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_EIOATTR_SPCL_CST";
                string reasonCode = cboOutTraySplReasonWashing.SelectedValue.GetString();
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = ProcessCode;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_LOT_GNRT_FLAG"] = chkOutTraySplWashing.IsChecked != null && (bool)chkOutTraySplWashing.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = chkOutTraySplWashing.IsChecked != null && (bool)chkOutTraySplWashing.IsChecked ? reasonCode : string.Empty;
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrkWashing.Text;
                newRow["SPCL_PROD_LOTID"] = chkOutTraySplWashing.IsChecked != null && (bool)chkOutTraySplWashing.IsChecked ? ProdLotId : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfo();

                        if (chkOutTraySplWashing.IsChecked != null && (bool)chkOutTraySplWashing.IsChecked)
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTray();
                        }
                        else
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                        }

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

        private void ConfirmTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

                string bizRuleName;
                if (ProcessCode.Equals(Process.WINDING))
                    bizRuleName = "BR_PRD_REG_END_OUT_LOT_WNM";
                else
                    bizRuleName = "BR_PRD_REG_END_OUT_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_OUT_LOT_WS();

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                indataTable.Rows.Add(newRow);

                DataTable inCstTable = indataSet.Tables["IN_CST"];
                newRow = inCstTable.NewRow();

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] =
                    Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "SPCL_RSNCODE"));
                inCstTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null,(searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        ClearCollectControl();
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

        private void ConfirmTrayCancel(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS();

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                indataTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

                DataTable inCstTable = indataSet.Tables["IN_CST"];
                newRow = inCstTable.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                inCstTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null,(searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        ClearCollectControl();
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

        private DataTable SetOutTraySplReasonCommonCode()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                DataTable indataTable = new DataTable { TableName = "RQSTDT" };
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SPCL_RSNCODE";
                indataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", indataTable);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetMaterialItem(string workOrderId, string itemCode)
        {
            // SELECT MATERIAL ITEM
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

        public DataTable GetSpecialTrayInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(EquipmentCode)) return null;
                const string bizRuleName = "DA_BAS_SEL_EIOATTR_SPCL_LOT";

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    chkOutTraySplWashing.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    txtOutTrayReamrkWashing.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutTraySplReasonWashing?.Items != null && cboOutTraySplReasonWashing.Items.Count > 0 && cboOutTraySplReasonWashing.Items.CurrentItem != null)
                    {
                        var dataRowView = cboOutTraySplReasonWashing.Items.CurrentItem as DataRowView;
                        DataView dtview = dataRowView?.DataView;
                        if (dtview?.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
                                {
                                    cboOutTraySplReasonWashing.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutTraySplReasonWashing.Items.Count > 0)
                                cboOutTraySplReasonWashing.SelectedIndex = 0;
                        }
                    }

                    //cboOutTraySplReason.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);

                    if ((bool)chkOutTraySplWashing.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutTraySplWashing.IsChecked = false;
                    txtOutTrayReamrkWashing.Text = string.Empty;

                    if (cboOutTraySplReasonWashing.Items.Count > 0)
                        cboOutTraySplReasonWashing.SelectedIndex = 0;

                    if ((bool)chkOutTraySplWashing.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_WS", "INDATA", "OUTDATA", indataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
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

                    //if ((grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceNotUse.IsChecked.HasValue &&
                    //    (bool)(grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceNotUse.IsChecked)
                    //    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")));

                    inLot.Rows.Add(newRow);
                    newRow = null;

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
                        ClearCollectControl();
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

        private void DeleteTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();

                string bizRuleName;

                if (ProcessCode.Equals(Process.WINDING))
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WNM";
                else
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS";

                DataTable indataTable = _bizDataSet.GetBR_PRD_REG_DELETE_OUT_LOT_WS();

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                DataRow selectedWorkOrderRow = GetSelectWorkOrderRow();

                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                newRow["WO_DETL_ID"] = Util.NVC(selectedWorkOrderRow["WO_DETL_ID"]);
                newRow["USERID"] = LoginInfo.USERID;

                indataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "IN_EQP", null, indataTable,(searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ClearCollectControl();
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
                    }
                });
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

        private bool ValidationSaveMaterial()
        {
            // INPUT MATERIAL SAVE
            if (dgInputMaterial.GetRowCount() == 0)
            {
                //입력된 투입 자재정보가 없습니다.
                Util.MessageValidation("SFU1809");
                return false;
            }

            for (int i = 0; i < dgInputMaterial.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "ITEMID"))))
                {
                    //자재ID가 누락되었습니다.
                    Util.MessageValidation("SFU1821");
                    dgInputMaterial.SelectedIndex = i;
                    return false;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "ITEMNAME"))))
                {
                    //자재명이 누락되었습니다.
                    Util.MessageValidation("SFU1830");
                    dgInputMaterial.SelectedIndex = i;
                    return false;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "LOTID"))))
                {
                    //자재LOT이 누락되었습니다.
                    Util.MessageValidation("SFU1823");
                    dgInputMaterial.SelectedIndex = i;
                    return false;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUTQTY"))) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUTQTY")) < 0)
                {
                    //사용량이 누락되었거나 0보다 커야 합니다.
                    Util.MessageValidation("SFU1591");
                    dgInputMaterial.SelectedIndex = i;
                    return false;
                }
            }
            return true;
        }

        private bool ValidationRemoveHalfProduct()
        {
            if (_util.GetDataGridCheckCnt(dgInputProduct, "CHK") == 0)
            {
                //선택된 반제품 정보가 없습니다.
                Util.MessageValidation("SFU1638");
                return false;
            }
            return true;
        }

        private bool ValidationSaveHalfProduct()
        {
            if (dgInputProduct.GetRowCount() == 0)
            {
                //입력된 투입 반제품정보가 없습니다.
                Util.MessageValidation("SFU1808");
                return false;
            }

            for (int i = 0; i < dgInputProduct.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUTQTY"))) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUTQTY")) < 0)
                {
                    //투입량이 누락되었거나 0보다 커야 합니다.
                    Util.MessageValidation("SFU1970");
                    dgInputProduct.SelectedIndex = i;
                    return false;
                }
            }
            return true;
        }

        private bool ValidationSaveSpecialTray()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductionCellWashing, "CHK") < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (chkOutTraySplWashing.IsChecked.HasValue && (bool)chkOutTraySplWashing.IsChecked)
            {
                if (cboOutTraySplReasonWashing.SelectedValue == null || cboOutTraySplReasonWashing.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return false;
                }

                if (txtOutTrayReamrkWashing.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return false;
                }
            }
            else
            {
                if (!txtOutTrayReamrkWashing.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return false;
                }
            }



            return true;
        }

        private bool ValidationSaveTray(C1DataGrid dg)
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

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            return true;
        }

        private bool ValidationTrayConfirm(C1DataGrid dg)
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

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "CELLQTY")), out dTmp))
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
            GetTrayInfo(dg,out returnMessage, out messageCode);

            if (returnMessage.Equals("NG"))
            {
                Util.MessageValidation(messageCode);
                return false;
            }
            else if (returnMessage.Equals("EXCEPTION"))
                return false;

            return true;
        }

        private bool ValidationTrayDelete(C1DataGrid dg)
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

            return true;
        }

        private bool ValidationConfirmCancel(C1DataGrid dg)
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

        public void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnCellWashingTrayCreate.IsEnabled = true;
                        btnCellWashingTrayRemove.IsEnabled = true;
                        btnCellWashingTrayCancel.IsEnabled = false;
                        btnCellWashingTrayConfirm.IsEnabled = true;
                        btnCellWashingTrayCell.IsEnabled = true;
                        btnCellWashingOutSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnCellWashingTrayCreate.IsEnabled = true;
                        btnCellWashingTrayRemove.IsEnabled = false;
                        btnCellWashingTrayCancel.IsEnabled = true;
                        btnCellWashingTrayConfirm.IsEnabled = false;
                        btnCellWashingTrayCell.IsEnabled = true;
                        btnCellWashingOutSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        btnCellWashingTrayCreate.IsEnabled = true;
                        btnCellWashingTrayRemove.IsEnabled = false;
                        btnCellWashingTrayCancel.IsEnabled = false;
                        btnCellWashingTrayConfirm.IsEnabled = false;
                        btnCellWashingTrayCell.IsEnabled = true;
                        btnCellWashingOutSave.IsEnabled = false;
                    }
                    else
                    {
                        btnCellWashingTrayCreate.IsEnabled = true;
                        btnCellWashingTrayRemove.IsEnabled = true;
                        btnCellWashingTrayCancel.IsEnabled = true;
                        btnCellWashingTrayConfirm.IsEnabled = true;
                        btnCellWashingTrayCell.IsEnabled = true;
                        btnCellWashingOutSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnCellWashingTrayCreate.IsEnabled = true;
                    btnCellWashingTrayRemove.IsEnabled = true;
                    btnCellWashingTrayCancel.IsEnabled = true;
                    btnCellWashingTrayConfirm.IsEnabled = true;
                    btnCellWashingTrayCell.IsEnabled = true;
                    btnCellWashingOutSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ColorAnimationInSpecialTray()
        {
            recSpcTray.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
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
            if (ProcessCode.Equals(Process.WINDING))
            {
                return dgProductionCellWinding;
            }
            else
            {
                return dgProductionCellWashing;
            }
        }

        public static void GridDataBinding(C1DataGrid dataGrid, List<String> bindValues, bool isNewFlag)
        {
            if (dataGrid.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                    colDt.Columns.Add(dataGrid.Columns[i].Name);

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








        #endregion


    }
}
