/*************************************************************************************
 Created Date : 2019.03.06
      Creator : 강 민준
   Decription : [조립 - 원형9,10호기 IN-LINE Collect UserControl910]
--------------------------------------------------------------------------------------
 [Change History]
   2019.04.19   INS 강민준C : TRAY조회 팝업화면 추가
   2022.12.30    김린겸 CSR ID C20220822-000365 원통형 9,10호 자동 실적 확정 기능 구현
   2023.01.04    김린겸 CSR ID C20220822-000365 원통형 9,10호 자동 실적 확정 기능 구현 - 품질정보 항목 데이터 없을시, 수집 기준 코드(Time) 에서 예외처리
   2023.03.27    성민식     C20230110-000142     ESNJ 대기PANCAKE 선입선출 시간 / 최대 이전 공정 종료일 컬럼 추가
**************************************************************************************/

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Text;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyDataCollect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyDataCollectInline
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public bool IsSmallType { get; set; }

        public bool IsRework { get; set; }

        public C1DataGrid DgDefect { get; set; }

        public C1DataGrid DgDefectReInput { get; set; }

        public C1DataGrid DgProdCellWinding { get; set; }

        public C1TabItem TabDefectReInput { get; set; }

        public string ProcessCode { get; set; }

        public string EquipmentSegmentCode { get; set; }

        public string EquipmentCode { get; set; }

        public string EquipmentCodeName { get; set; }

        public string ProdLotId { get; set; }

        public string ProdLotState { get; set; }

        public string ProdWorkOrderId { get; set; }

        public string ProdWorkOrderDetailId { get; set; }

        public int ProdSelectedCheckRowIdx { get; set; }

        public string ProdWorkInProcessSequence { get; set; }

        public string CellManagementTypeCode { get; set; }

        public readonly System.Windows.Threading.DispatcherTimer DispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private string _trayId;
        private string _trayTag;
        private string _outLotId;
        private string _wipQty;
        private bool _isWindingSetAutoTime = false;
        private DateTime _dtMinValid;
        private string _maxPeviewProcessEndDay = string.Empty;
        private Int32 _trayCheckSeq;
        int idxCheck = 0;
        string strReloadFlag = "";


        //private DataRow workOrder;
        private string wipqty2 = string.Empty;

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

        public UcAssyDataCollectInline()
        {
            InitializeComponent();
            SetControl();
            InitCombo();
        }
        #endregion

        private void InitCombo()
        {
        }

        #region Initialize
        private void SetControl()
        {
            DgDefect = dgDefect;
            DgDefectReInput = dgDefectReInput;
            DgProdCellWinding = dgProdCellWinding;
            TabDefectReInput = TabReInput;

            //TabDefect.Visibility = Visibility.Collapsed;                // 불량/LOSS/물품청구
            //TabMaterialInput.Visibility = Visibility.Collapsed;         // 자재투입
            TabProdCellWinding.Visibility = Visibility.Collapsed;       // 생산반제품
            TabWashingResult.Visibility = Visibility.Collapsed;         // Washing실적
            TabPancake.Visibility = Visibility.Collapsed;               // 대기Pancake
            TabWaitHalfProduct.Visibility = Visibility.Collapsed;       // 대기반제품
            TabInputHalfProduct.Visibility = Visibility.Collapsed;      // 투입반제품
            TabInputMaterial.Visibility = Visibility.Collapsed;         // 투입자재
            TabHistory.Visibility = Visibility.Collapsed;               // 투입이력
            TabBox.Visibility = Visibility.Collapsed;                   // BOX
            TabReInput.Visibility = Visibility.Collapsed;               // 재투입

            dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;
            dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;
            btnInHalfProductInPutQty.Visibility = Visibility.Collapsed;

            rdoLot.IsChecked = true;
            _trayCheckSeq = 0;
        }

        public void SetControlProperties()
        {
            if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
            {
                TabPancake.Visibility = Visibility.Visible;               // 대기Pancake
                TabHistory.Visibility = Visibility.Visible;               // 투입이력
                if (IsSmallType)
                {
                    TabProdCellWinding.Visibility = Visibility.Visible;   // 생산반제품
                    dgProdCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;
                    dgProdCellWinding.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
                }

                dgDefect.Columns["RSLT_EXCL_FLAG"].Visibility = Visibility.Visible;
                dgDefect.Columns["INPUT_QTY_APPLY_TYPE_CODE_NAME"].Visibility = Visibility.Visible;
                dgWaitPancake.Columns["VALID_DATE"].Visibility = Visibility.Collapsed;

                TabMaterialInput.Header = ObjectDic.Instance.GetObjectName("자재투입/잔량처리");
                TabHistory.Header = ObjectDic.Instance.GetObjectName("전극/자재투입이력"); 
                //전극/자재투입이력
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                TabWashingResult.Visibility = Visibility.Visible;         // Washing실적
                TabWaitHalfProduct.Visibility = Visibility.Visible;       // 대기반제품
                TabInputHalfProduct.Visibility = Visibility.Visible;      // 투입반제품
                TabInputMaterial.Visibility = Visibility.Visible;         // 투입자재
                TabBox.Visibility = Visibility.Visible;                   // BOX 
                dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Visible;
                //dgInHalfProduct.Columns["INPUT_LOTID"].Header = ObjectDic.Instance.GetObjectName("대차LOTID");

                
                if (IsSmallType)
                {
                    dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Visible;
                    dgWaitHalfProduct.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    tbWaitLotId.Text = ObjectDic.Instance.GetObjectName("TRAYID");
                }
                else
                {
                    dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                    dgWaitHalfProduct.Columns["LOTID"].Visibility = Visibility.Visible;
                    tbWaitLotId.Text = ObjectDic.Instance.GetObjectName("LOT ID");
                    btnInHalfProductInPutQty.Visibility = Visibility.Visible;
                }
            }
            else if (string.Equals(ProcessCode, Process.WASHING))
            {
                TabInputMaterial.Header = ObjectDic.Instance.GetObjectName("투입처리");
                if (IsRework)
                {
                    dgWaitHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                }
            }            

            if (TabProdCellWinding.Visibility == Visibility.Visible)
            {
                CommonCombo combo = new CommonCombo();
                string[] filter = { "MOBILE_TRAY_INTERVAL" };
                combo.SetCombo(cboAutoSearchOutWinding, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

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
        #endregion

        #region Event

        private void TcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            if (string.Equals(tabItem, "TabDefect"))
            {
                
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

                    if ((ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH)) && IsSmallType)
                    {
                        if (!CommonVerify.HasDataGridRow(dgProdCellWinding)) return;
                        GetProductCellList(dgProdCellWinding);
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

        #region 불량/LOSS/물품청구 Event
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;
            SaveDefect();
            //Util.MessageConfirm("SFU1587", result =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveDefect();
            //    }
            //});
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
                            if (drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y")
                            {
                                e.Cancel = true;
                            }
                            else
                            {
                                e.Cancel = false;
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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            
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
        #endregion

        #region 초소형 와인더 공정진척 Event
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
            if (!ValidationTrayConfirm(dgProdCellWinding))
                return;

            try
            {
                DispatcherTimer?.Stop();
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTray(dgProdCellWinding);
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
            if (!ValidationConfirmCancel(dgProdCellWinding)) return;
            try
            {
                DispatcherTimer?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTrayCancel(dgProdCellWinding);
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

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDischarge(dgProdCellWinding))
                return;

            try
            {
                DispatcherTimer?.Stop();
                //배출 하시겠습니까?
                Util.MessageConfirm("SFU3613", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DischargeTray(dgProdCellWinding);
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
            if (!ValidationRemoveTray(dgProdCellWinding))
                return;

            RemoveTray();
        }

        private void dgProdCellWinding_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        // 배출완료(#E8F7C8)
                        if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
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
                        if (!string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_TRAY_CNFM_FLAG").GetString(), "Y") && string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                            ((ContentControl)e.Cell.Presenter.Content).Tag = "U";
                            ((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                            ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
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

        private void dgProdCellWinding_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
                {
                    dgProdCellWinding.SelectedItem = row.DataItem;

                    _trayId = DataTableConverter.GetValue(dgProdCellWinding.SelectedItem, "TRAYID").GetString();
                    if (btn != null) _trayTag = btn.Tag.GetString();
                    _outLotId = DataTableConverter.GetValue(dgProdCellWinding.SelectedItem, "OUT_LOTID").GetString();
                    _wipQty = DataTableConverter.GetValue(dgProdCellWinding.SelectedItem, "CELLQTY").GetString();
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
        #endregion

        #region Washing 실적 Event
        private void TbWashingResult_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GetSelectProductRow() != null)
                GetWashingResult();
        }
        #endregion

        #region 자재 투입 Event
        private void dgMaterialInput_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
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

                                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_LOTID").GetString()))
                                        {
                                            btnMaterialInputComplete.IsEnabled = false;
                                        }
                                        

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
                                            _previewValues.PreviewCurentInput = string.Empty;
                                            btnMaterialInputComplete.IsEnabled = true;
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

        private void txtMaterialInputLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!ValidationAutoInputLot())
                        return;

                    string positionId = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string positionName = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                    //string outLotId = txtMaterialInputLotID.Text.Trim();

                    object[] parameters = new object[2];
                    parameters[0] = positionName;
                    parameters[1] = txtMaterialInputLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                            {
                                InputAutoLotWinding(txtMaterialInputLotID.Text.Trim(), positionId);
                            }
                            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                            {
                                if (IsSmallType)
                                {
                                    InputAutoLotAssemblySmallType(txtMaterialInputLotID.Text.Trim(), positionId);
                                }
                                else
                                {
                                    InputAutoLotAssembly(txtMaterialInputLotID.Text.Trim(), positionId);
                                }
                            }
                            else if (string.Equals(ProcessCode, Process.WASHING))
                            {
                                InputAutoLotWashing(txtMaterialInputLotID.Text.Trim(), positionId);
                            }

                            txtMaterialInputLotID.Text = string.Empty;
                        }
                    }, parameters);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        //In-Line 생산반제품 Tray 생성추가 강민준
        private void txtTrayIDInline_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (ValidationCreateTray() == true)
                    {                        
                        btnSave_ClickInline();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //생산반제품-트레이생성 조건확인
        private bool ValidationCreateTray()
        {
            if (string.IsNullOrEmpty(txtTrayIDInline.Text.Trim()) || txtTrayIDInline.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675");                
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtTrayIDInline.Text.ToUpper(), @"[^a-zA-Z0-9가-힣]");
            
            if (chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtTrayIDInline.Text.ToUpper());
                return false;
            }

            return true;
        }        
        //생산반제품-트레이생성
        private void btnSave_ClickInline()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {                    
                    try
                    {
                        string sBizNAme = string.Empty;
                        
                        sBizNAme = "BR_PRD_REG_EQPT_END_OUT_LOT_WN_CST";                       

                        DataSet inDataSet = new DataSet();
                        DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                        DataRow drow = inEQP.NewRow();
                        StringBuilder sb = new StringBuilder();
                        
                        inEQP.Columns.Add("SRCTYPE", typeof(string));
                        inEQP.Columns.Add("IFMODE", typeof(string));
                        inEQP.Columns.Add("EQPTID", typeof(string));
                        inEQP.Columns.Add("USERID", typeof(string));
                        inEQP.Columns.Add("PROD_LOTID", typeof(string));
                        inEQP.Columns.Add("EQPT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUTPUT_QTY", typeof(string));
                        inEQP.Columns.Add("CSTID", typeof(string));
                        

                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;                        
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = EquipmentCode;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = ProdLotId;
                        drow["EQPT_LOTID"] = ProdLotId;
                        drow["OUT_LOTID"] = "";
                        drow["OUTPUT_QTY"] = Convert.ToDecimal(324);
                        drow["CSTID"] = txtTrayIDInline.Text;

                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);                        

                        new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            GetProductCellList(dgProdCellWinding);

                        }, inDataSet);
                        

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });            
        }

        // 생산반제품-저장버튼 추가 강민준
        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationTraySave()) return;
            SaveTray();           
        }
        private bool ValidationTraySave()
        {
            //int idx = _util.GetDataGridCheckFirstRowIndex(dgProdCellWinding, "CHK");
            //if (idx < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            //if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 작업대상이 없습니다.");
            //    Util.MessageValidation("SFU1645");
            //    return false;
            //}            
            return true;
        }
        private void SaveTray()
        {
            try
            {
                //ShowLoadingIndicator();
                DispatcherTimer?.Stop();
                dgProdCellWinding.EndEdit();
                
                const string bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WN_CST";                
                //string specialyn = null;
                //string specialReasonCode = null;
                
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                                
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                for (int i = 0; i < dgProdCellWinding.Rows.Count - dgProdCellWinding.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgProdCellWinding, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["USERID"] = LoginInfo.USERID;

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "TRAYID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "CELLQTY")));

                    inDataTable.Rows.Add(newRow);                   

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_DATA", null, indataSet);
                    inDataTable.Rows.Remove(newRow);
                    
                }

                //HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
                if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                    DispatcherTimer.Start();

                GetProductLot();


            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
                if (DispatcherTimer != null && DispatcherTimer.Interval.TotalSeconds > 0)
                    DispatcherTimer.Start();
            }
        }
        //생산반제품 그리드 체크 
        private bool IsCheckedOnDataGrid()
        {
            if (CommonVerify.HasDataGridRow(dgProdCellWinding))
            {
                DataTable dt = ((DataView)dgProdCellWinding.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int32>("CHK") == 1
                                 select t).ToList();
                if (queryEdit.Any())
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
        //생산반제품 체크
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsCheckedOnDataGrid())
            {
                chkHeaderAll_CheckedInline(null, null);
            }
            else
            {
                chkHeaderAll_UncheckedInline(null, null);
            }
        }
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProdCellWinding);
        }
        private void chkHeaderAll_UncheckedInline(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProdCellWinding);
        }
        private void chkHeaderAll_CheckedInline(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgProdCellWinding))
            {
                DataTable dt = ((DataView)dgProdCellWinding.ItemsSource).Table;
                var sortedTable = dt.Copy().AsEnumerable()                    
                    .OrderBy(r => r.Field<string>("LOTDTTM_CR"))
                    .Take(_trayCheckSeq).ToList();

                foreach (C1.WPF.DataGrid.DataGridRow row in dgProdCellWinding.Rows)
                {
                    if (row.Type != DataGridRowType.Item) continue;

                    decimal cellQty = DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();
                    decimal cellCstQty = DataTableConverter.GetValue(row.DataItem, "CST_CELL_QTY").GetDecimal();
                    string strStatCode = DataTableConverter.GetValue(row.DataItem, "STAT_CODE").GetString();
                    
                    if (cellQty > 0)
                    {
                        if (sortedTable.Any())
                        {
                            if (sortedTable.AsQueryable().Any(x => x.Field<string>("TRAYID") == DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString()))
                            {
                                if (strStatCode.Equals("EQPT_END"))
                                {
                                    if (cellQty <= cellCstQty)
                                    {                                        
                                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                                        btnOutSave.IsEnabled = true;
                                        
                                    }
                                }
                                
                            }
                        }
                    }
                }
                dgProdCellWinding.EndEdit();
                dgProdCellWinding.EndEditRow(true);
            }
        }
        

        private void dgOut_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProdCellWinding.GetCellFromPoint(pnt);

            if (cell != null)
            {
                idxCheck = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK").GetString();
                decimal cellQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CELLQTY").GetDecimal();
                decimal cellCstQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CST_CELL_QTY").GetDecimal();                


                dgProdCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;

                if (string.Equals(moveStateCode, "EQPT_END") && cellQty <= cellCstQty)
                {
                    DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", false);
                    //btnOutSave.IsEnabled = true;

                    if (checkFlag.Equals("0"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", false);
                        btnOutSave.IsEnabled = true;
                        if (cellQty > cellCstQty)
                        {
                            DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", true);
                        }
                        strReloadFlag = "";
                    }

                    if (checkFlag.Equals("1"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", true);
                        if (btnOutSave.IsEnabled == true)
                        {
                            btnOutSave.IsEnabled = true;
                        }else
                        {
                            btnOutSave.IsEnabled = false;
                        }
                        if (cellQty > cellCstQty)
                        {
                            DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", true);
                        }
                        strReloadFlag = "";
                    }
                }
                else
                {
                    if (checkFlag.Equals("0"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", false);
                        btnOutSave.IsEnabled = false;
                        strReloadFlag = "";
                    }

                    if (checkFlag.Equals("1"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", true);
                        if (btnOutSave.IsEnabled == false)
                        {
                            btnOutSave.IsEnabled = true;
                        }
                        strReloadFlag = "";
                    }                    
                }                
            }
        }





        private void btnMaterialInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMaterialInputCancel())
                    return;

                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN")) == "Y")
                        {
                            EqptRemainInputCancel();
                        }
                        else
                        {
                            DataTable inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgMaterialInput, "CHK", i)) continue;

                                if (!Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                                {
                                    DataRow newRow = inDataTable.NewRow();
                                    newRow["WIPNOTE"] = "";
                                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID"));
                                    newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_QTY")));
                                    inDataTable.Rows.Add(newRow);
                                }
                            }
                            MaterialInputCancel(inDataTable);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void EqptRemainInputCancel()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                /*
                if (selectedProductRow == null) return;
                */

                DataSet indataSet = new DataSet();

                DataTable inEqpTable = indataSet.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("SRCTYPE", typeof(string));
                inEqpTable.Columns.Add("IFMODE", typeof(string));
                inEqpTable.Columns.Add("EQPTID", typeof(string));
                inEqpTable.Columns.Add("USERID", typeof(string));
                inEqpTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("EQPT_REMAIN_PSTN", typeof(string));

                DataRow newRow = inEqpTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                if (selectedProductRow != null)
                {
                    newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                }
                inEqpTable.Rows.Add(newRow);

                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "INPUT_LOTID"));
                newRow["EQPT_REMAIN_PSTN"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN"));
                inInputTable.Rows.Add(newRow);

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_EQPT_REMAIN_LOT_WN", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetProductLot();

                    Util.MessageInfo("SFU1275");

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialInputReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMaterialInputReplace()) return;

                if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                {
                    PopupReplaceWinding();
                }
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (IsSmallType)
                    {
                        PopupReplaceAssemblySmallType();
                    }
                    else
                    {
                        PopupReplaceAssembly();
                    }
                }
                else
                {
                    PopupReplaceWashingRework();
                    /*
                    if (IsRework)
                    {
                        PopupReplaceWashingRework();
                    }
                    else
                    {
                        PopuoReplaceWashing();
                    }
                    */

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialInputComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMaterialInputComplete()) return;
                Util.MessageConfirm("SFU1972", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                        {
                            DataTable inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgMaterialInput, "CHK", i)) continue;

                                if (!Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                                {
                                    DataRow newRow = inDataTable.NewRow();
                                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID"));
                                    newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "MTRLID"));
                                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_QTY")));

                                    inDataTable.Rows.Add(newRow);
                                }
                            }
                            MaterialInputCompleteWinding(inDataTable);
                        }
                        else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                        {
                            DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_EQPT_END_INPUT_LOT_AS();
                            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgMaterialInput, "CHK", i)) continue;

                                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                                {
                                    DataRow dr = inDataTable.NewRow();
                                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                    dr["EQPTID"] = EquipmentCode;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID"));
                                    dr["PROD_LOTID"] = ProdLotId;
                                    dr["EQPT_LOTID"] = string.Empty;
                                    inDataTable.Rows.Add(dr);
                                }
                            }
                            MaterialInputCompleteAssembly(inDataTable);
                        }
                        else if (string.Equals(ProcessCode, Process.WASHING))
                        {
                            DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_END_INPUT_LOT_WS();
                            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgMaterialInput, "CHK", i)) continue;

                                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                                {
                                    DataRow dr = inDataTable.NewRow();
                                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                    dr["EQPTID"] = EquipmentCode;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                    dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                    dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID"));
                                    dr["PROD_LOTID"] = ProdLotId;
                                    dr["EQPT_LOTID"] = string.Empty;
                                    inDataTable.Rows.Add(dr);
                                }
                            }
                            MaterialInputCompleteWashing(inDataTable);
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

        #region 대기Pancake Event
        private void cboPancakeMountPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitPancake();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationWaitPanCakeInput())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InputWaitPancake();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            //DataGridCurrentCellChanged(sender, e);
            try
            {
                // 대기 1개만 선택.
                Dispatcher.BeginInvoke(new Action(() =>
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
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null && dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null && checkBox.IsChecked.HasValue && !(bool)checkBox.IsChecked))
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

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(_maxPeviewProcessEndDay).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay;
                        int.TryParse(_maxPeviewProcessEndDay, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitPancakeLot.Text.Trim().Length > 0)
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_dtMinValid.AddDays(iDay) >= dtValid)
                                {
                                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        #endregion

        #region 대기 반제품 Event
        private void cboWaitHalfProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void btnWaitHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitHalfProductList();
        }

        private void btnWaitHalfProductInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationWaitHalfProductInput()) return;

                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        WaitHalfProductInput();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgWaitHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }

        #endregion

        #region 투입 반제품 Event
        private void cboInHalfMountPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetHalfProductList();
        }

        private void btnInHalfProductSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHalfProductList();
        }


        private void btnInHalfProductInPutQty_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInHalfProductInPutQty()) return;

            DataRow selectedProductRow = GetSelectProductRow();
            if (selectedProductRow == null) return;

            int idx = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            CMM_ASSY_MODIFY_INPUT_LOT_QTY popModifyQty = new CMM_ASSY_MODIFY_INPUT_LOT_QTY { FrameOperation = FrameOperation };
            object[] parameters = new object[5];
            parameters[0] = EquipmentCode;
            parameters[1] = selectedProductRow["LOTID"].GetString();
            parameters[2] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_LOTID").GetString();
            parameters[3] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_SEQNO").GetString();
            parameters[4] = DataTableConverter.GetValue(dgInHalfProduct.Rows[idx].DataItem, "INPUT_QTY").GetString();
            C1WindowExtension.SetParameters(popModifyQty, parameters);
            popModifyQty.Closed += popModifyQty_Closed;

            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popModifyQty);
                    popModifyQty.BringToFront();
                    break;
                }
            }
        }

        private void popModifyQty_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_MODIFY_INPUT_LOT_QTY pop = sender as CMM_ASSY_MODIFY_INPUT_LOT_QTY;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
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

        private void btnInHalfProductInPutCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInHalfProductInPutCancel()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        InputHalfProductCancelAssembly();
                    }
                    else
                    {
                        InputHalfProductCancel();
                    }
                }
            });
        }

        private void dgInHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }
        #endregion

        #region 투입자재 Event
        private void cboInputMaterialMountPstsID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetMaterialList();
        }

        private void txtInputMaterialLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMaterialList();
            }
        }

        private void btnInputMaterialtSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMaterialList();
        }

        private void btnInputMaterialCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputMaterialCancel()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputMaterialCancel();
                }
            });
        }

        private void dgInputMaterial_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            CheckInDataGridCheckBox(dg, e);
        }
        #endregion

        #region 투입이력 Event
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

        private void btnInputHistoryCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationInputHistoryCancel(dgInputHist))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        InputHistoryCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 품질정보 Event
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualitySave()) return;
            SaveQuality();
            //Util.MessageConfirm("SFU1241", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveQuality();
            //    }
            //});
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL01", StringComparison.Ordinal) >= 0)
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
            }));
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1DataGrid grid = null;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    if (n != null)
                    {
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                        if (p != null)
                        {
                            rIdx = p.Cell.Row.Index;
                            cIdx = p.Cell.Column.Index;
                            grid = p.DataGrid;
                        }
                    }
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    StackPanel panel = n?.Parent as StackPanel;
                    DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                    if (p != null)
                    {
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                }
                else
                    return;


                if (grid.GetRowCount() > ++rIdx)
                {
                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid?.ScrollIntoView(rIdx + 1, cIdx);
                    }

                    if (grid != null)
                    {
                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
                        StackPanel panel = p.Content as StackPanel;

                        if (panel != null)
                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                    panel.Children[cnt].Focus();
                            }
                    }
                }
                else if (CommonVerify.HasDataGridRow(grid) && grid.GetRowCount() == rIdx)
                {
                    //btnQualitySave.Focus();
                }
            }
        }

        private void CLCTVAL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid = null;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }
                    else
                        return;

                    if (grid.GetRowCount() > ++rIdx)
                    {
                        // Null 오류 Scroll 추가
                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }

                        if (grid != null)
                        {
                            DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                            if (p == null) return;
                            StackPanel panel = p.Content as StackPanel;

                            if (panel != null)
                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid = null;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n?.Parent as StackPanel;
                        DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                        if (p != null)
                        {
                            rIdx = p.Cell.Row.Index;
                            cIdx = p.Cell.Column.Index;
                            grid = p.DataGrid;
                        }
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }
                    else
                        return;

                    if (grid.GetRowCount() > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        // Null 오류 Scroll 추가
                        if (rIdx >= 0)
                        {
                            if (rIdx == 0)
                                grid.ScrollIntoView(rIdx, cIdx);
                            else
                                grid.ScrollIntoView(rIdx - 1, cIdx);
                        }
                        else
                        {
                            return;
                        }

                        if (grid != null)
                        {
                            DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                            if (p == null) return;
                            StackPanel panel = p.Content as StackPanel;

                            if (panel != null)
                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
                if (drv == null) return;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);

                    string usl = drv["USL"].GetString();
                    string lsl = drv["LSL"].GetString();

                    if (n != null)
                    {
                        if (!string.IsNullOrEmpty(usl) && !string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal() || n.Value.GetDecimal() < lsl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(usl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() < usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                    }
                }
                else //if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            //ComboBox cbo = sender as ComboBox;
            //if (cbo != null && cbo.IsVisible)
            //    cbo.SelectedIndex = 0;
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            GetBoxList();
        }
        #endregion

        #region 재투입 Event

        private void btnDefectReInputSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefectReInput()) return;
            SaveDefectReInput();
        }

        private void dgDefectReInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectReInput_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void dgDefectReInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        #endregion

        #endregion

        #region Mehod

        public void ChangeEquipment(string equipmentCode, string equipmentSegmentCode)
        {
            try
            {
                EquipmentSegmentCode = equipmentSegmentCode;
                EquipmentCode = equipmentCode;

                SetComboBox();
                ClearDataCollectControl();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SearchAllDataCollect()
        {
            try
            {
                if (TabMaterialInput.Visibility == Visibility.Visible)
                {
                    GetMaterialInputList();
                }
                if (TabPancake.Visibility == Visibility.Visible)
                {
                    GetWaitPancake();
                }
                if (TabWaitHalfProduct.Visibility == Visibility.Visible)
                {
                    GetWaitHalfProductList();
                }
                if (TabInputHalfProduct.Visibility == Visibility.Visible)
                {
                    GetMaterialList();
                }
                if (TabInputMaterial.Visibility == Visibility.Visible)
                {
                    GetHalfProductList();
                }
                if (TabHistory.Visibility == Visibility.Visible)
                {
                    GetInputHistory();
                }
                if (TabProdCellWinding.Visibility == Visibility.Visible)
                {
                    //생산 반제품 조회
                    GetProductCellList(dgProdCellWinding);
                }
                if (TabQualityInfo.Visibility == Visibility.Visible)
                {
                    GetQualityInfoList();
                }
                if (TabWashingResult.Visibility == Visibility.Visible)
                {
                    GetWashingResult();
                }
                if (TabBox.Visibility == Visibility.Visible)
                {
                    GetBoxList();
                }
                if (TabReInput.Visibility == Visibility.Visible)
                {
                    GetDefectReInputList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetMaterialInputList()
        {
            try
            {
                if (GetSelectProductRow() == null)
                {
                    btnMaterialInputCancel.IsEnabled = false;
                    //btnMaterialInputComplete.IsEnabled = false;
                }
                else
                {
                    btnMaterialInputCancel.IsEnabled = true;
                    //btnMaterialInputComplete.IsEnabled = true;
                }

                string bizRuleName;
                if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                }
                //const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgMaterialInput.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgMaterialInput, searchResult, FrameOperation);

                        if (!_previewValues.PreviewCurentInput.Equals(""))
                        {
                            int idx = _util.GetDataGridRowIndex(dgMaterialInput, "EQPT_MOUNT_PSTN_ID", _previewValues.PreviewCurentInput);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgMaterialInput.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgMaterialInput.SelectedIndex = idx;
                                dgMaterialInput.ScrollIntoView(idx, dgMaterialInput.Columns["CHK"].Index);
                            }
                        }

                        // WINDING 의 경우 컬럼 다르게 보이도록 수정.
                        if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
                        {
                            dgMaterialInput.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            dgMaterialInput.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
                            dgMaterialInput.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgMaterialInput.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            dgMaterialInput.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                            dgMaterialInput.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }

                        if (dgMaterialInput.CurrentCell != null)
                            dgMaterialInput.CurrentCell = dgMaterialInput.GetCell(dgMaterialInput.CurrentCell.Row.Index, dgMaterialInput.Columns.Count - 1);
                        else if (dgMaterialInput.Rows.Count > 0 && dgMaterialInput.GetCell(dgMaterialInput.Rows.Count, dgMaterialInput.Columns.Count - 1) != null)
                            dgMaterialInput.CurrentCell = dgMaterialInput.GetCell(dgMaterialInput.Rows.Count, dgMaterialInput.Columns.Count - 1);
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

        public void GetWaitPancake()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_WN_BY_LV3_CODE";
                
                // UI에서 선택된 양극(CA) 음극(AN) 값만 호출 하는 것으로 수정 처리 함
                //string sInMtrlClssCode = GetInputMtrlClssCode();
                string inputClassCode;
                if (cboPancakeMountPstnID?.SelectedValue == null || cboPancakeMountPstnID.SelectedValue.Equals("SELECT"))
                {
                    inputClassCode = string.Empty;
                }
                else
                {
                    string[] postionStrings = cboPancakeMountPstnID.SelectedValue.GetString().Split('_');
                    inputClassCode = postionStrings.Length > 0 ? postionStrings[0] : string.Empty;
                }

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["WOID"] = ProdWorkOrderId;
                newRow["IN_LOTID"] = txtWaitPancakeLot.Text;
                //newRow["PRDT_CLSS_CODE"] = null;  -- 설비 조건으로 PROD_CLASS_CODE 조회 함..
                newRow["INPUT_MTRL_CLSS_CODE"] = inputClassCode;
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    dgWaitPancake.Columns["MAX_PRE_PROC_END_DAY"].Visibility = Visibility.Visible;
                    dgWaitPancake.Columns["END_VALID_DATE"].Visibility = Visibility.Visible;
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (CommonVerify.HasTableRow(searchResult) && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _dtMinValid);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
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

        private string GetInputMtrlClssCode()
        {
            try
            {
                if (cboPancakeMountPstnID?.SelectedValue == null)
                {
                    return string.Empty;
                }

                const string bizRuleName = "DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID";
                string sInputMtrlClssCode = string.Empty;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EquipmentCode;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                    txtWaitPancakeInputClssCode.Text = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        public void GetProcMtrlInputRule()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_PROC_MTRL_INPUT_RULE";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _maxPeviewProcessEndDay = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWaitHalfProductList()
        {
            try
            {
                Util.gridClear(dgWaitHalfProduct);
                string bizRuleName;

                if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    bizRuleName = IsSmallType ? "DA_PRD_SEL_WAIT_HALFPROD_ASS" : "DA_PRD_SEL_WAIT_HALFPROD_AS";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_WS";
                }

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));

                if (!string.Equals(ProcessCode, Process.ASSEMBLY))
                    indataTable.Columns.Add("WOID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = EquipmentSegmentCode;
                dr["PROCID"] = ProcessCode;
                dr["INPUT_LOTID"] = string.IsNullOrEmpty(txtTrayId.Text.Trim()) ? null : txtTrayId.Text.Trim();
                if (!string.Equals(ProcessCode, Process.ASSEMBLY))
                    dr["WOID"] = ProdWorkOrderId;

                indataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgWaitHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHalfProductList()
        {
            try
            {
                Util.gridClear(dgInHalfProduct);

                string bizRuleName = string.Equals(ProcessCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
                const string materialType = "PROD";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtInHalfProductLot.Text.Trim();
                dr["MTRLTYPE"] = materialType;
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInHalfMountPstnID.SelectedValue.GetString()) ? null : cboInHalfMountPstnID.SelectedValue.GetString();
                dr["EQPTID"] = EquipmentCode;
                dr["PROD_LOTID"] = ProdLotId;

                indataTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                //dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dgInHalfProduct, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefect()
        {
            try
            {
                dgDefect.EndEdit();

                //const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_ALL";
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
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //if (bMsgShow)
                //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                //IsChangeDefect = false;

                GetProductLot();
            }
            catch (Exception ex)
            {
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
                //string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //IsChangeDefect = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //=====================================================================================================================
        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        //=====================================================================================================================
        public void SetAutoRsltCnfmAddDefectQtyBeforeConfirm(int AutoRsltCnfmAddDfctQty)
        {
            try
            {
                //원격에서 호출시 에러 발생...SaveDefectBeforeAutoConfirm(double AutoRsltCnfmAddDfctQty)으로 대체 
                int iRowRun = _util.GetDataGridFirstRowIndexByEquiptmentEnd(dgDefect, "RESNCODE", "PL99LF3");
                if(iRowRun > 0)
                {
                    DataTableConverter.SetValue(dgDefect.Rows[iRowRun].DataItem, "RESNQTY", AutoRsltCnfmAddDfctQty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //=====================================================================================================================
        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        //=====================================================================================================================
        public void SaveDefectBeforeAutoConfirm(double AutoRsltCnfmAddDfctQty)
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

                string ResnCode = string.Empty;
                string Acid = string.Empty;
                double RsnQty = 0;

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;

                    //추가불량 수량 -기타추가불량으로 저장 
                    Acid = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    ResnCode = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    if(Acid.Equals("LOSS_LOT") && ResnCode.Equals("PL99LF3"))
                    {
                        RsnQty = AutoRsltCnfmAddDfctQty;
                    }
                    else
                    {
                        RsnQty = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    }
                    
                    newRow["ACTID"] = Acid;
                    newRow["RESNCODE"] = ResnCode;
                    newRow["RESNQTY"] = RsnQty;

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
                //string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //IsChangeDefect = false;
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
                //string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        ClearDataCollectControl();
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
                //int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
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
                //string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }
                        ClearDataCollectControl();
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
                
        private void DischargeTray(C1DataGrid dg)
        {
            try
            {
                ShowParentLoadingIndicator();
                //const string bizRuleName = "BR_PRD_CHK_CONFIRM_TRAY_WNS";
                const string bizRuleName = "BR_PRD_CHK_CONFIRM_TRAY_WN_CST";

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

                //string xmlText = indataSet.GetXml();

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
                        ClearDataCollectControl();
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

        private void RemoveTray()
        {
            C1DataGrid dg = dgProdCellWinding;
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

                if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
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

                //string xmlText = indataSet.GetXml();

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
                        ClearDataCollectControl();
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

        public void GetWashingResult()
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

        private void GetMaterialList()
        {
            try
            {
                Util.gridClear(dgInputMaterial);

                string bizRuleName = string.Equals(ProcessCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_MATERIAL_AS" : "DA_PRD_SEL_INPUT_MATERIAL_WS";
                const string materialType = "MTRL";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));
                indataTable.Columns.Add("MTRLTYPE", typeof(string));
                indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUT_LOTID"] = txtInputMaterialLotID.Text.Trim();
                dr["MTRLTYPE"] = materialType;
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInputMaterialMountPstsID.SelectedValue.GetString()) ? null : cboInputMaterialMountPstsID.SelectedValue.GetString();
                dr["EQPTID"] = EquipmentCode;
                dr["PROD_LOTID"] = ProdLotId;
                indataTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInputMaterial.ItemsSource = DataTableConverter.Convert(dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["PROD_WIPSEQ"] = string.IsNullOrEmpty(ProdWorkInProcessSequence) ? 1 : Convert.ToDecimal(ProdWorkInProcessSequence);
                newRow["INPUT_LOTID"] = string.IsNullOrEmpty(txtHistLotID.Text) ? null : txtHistLotID.Text.Trim();
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

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

        private void GetBoxList()
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
                //string xml = ds.GetXml();

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
                        dgBox.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private void GetDefectReInputList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWorkInProcessSequence;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefectReInput.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefectReInput.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWinding(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;
                inInputTable.Rows.Add(newRow);

                //string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                GetProductLot();
                //GetMaterialInputList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotAssembly(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_AS();

                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                row["EQPT_LOTID"] = string.Empty;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                row = inInputTable.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = positionId;
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["PRODID"] = string.Empty;
                row["WINDING_RUNCARD_ID"] = inputLot;
                inInputTable.Rows.Add(row);

                //string xmlText = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_EQP", indataSet);
                GetProductLot();
                //GetMaterialInputList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotAssemblySmallType(string inputLot, string positionId)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_LOT_ASS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_AS();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                newRow["EQPT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["CSTID"] = inputLot;
                newRow["OUT_LOTID"] = string.Empty;
                inTable.Rows.Add(newRow);

                //string xmlText = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                GetProductLot();
                //GetMaterialInputList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
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
                //string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                GetProductLot();
                //GetMaterialInputList();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCancel(DataTable dt)
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                string bizRuleName = string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH) ? "BR_PRD_REG_CANCEL_INPUT_IN_LOT_WN" : "BR_PRD_REG_CANCEL_INPUT_IN_LOT";

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

                //string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetProductLot();
                    //GetMaterialInputList();
                    Util.MessageInfo("SFU1275");

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCompleteWinding(DataTable dt)
        {
            try
            {
                string prodLotId = string.Empty;

                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow != null) prodLotId = selectedProductRow["LOTID"].GetString();

                const string bizRuleName = "BR_PRD_REG_END_INPUT_IN_LOT_WN";

                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_COMPLETE_LM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = prodLotId;

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

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetProductLot();
                    //GetMaterialInputList();
                    Util.MessageInfo("SFU1275");
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCompleteAssembly(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_EQPT_END_INPUT_LOT_AS";
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
                    GetProductLot();
                    //GetMaterialInputList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCompleteWashing(DataTable dt)
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
                    GetProductLot();
                    //GetMaterialInputList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputWaitPancake()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);
                
                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetProductLot();
                        GetWaitPancake();
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

        private void WaitHalfProductInput()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;

                ShowParentLoadingIndicator();
                // 추후 분리작업이 필요할 수 있음

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgWaitHalfProduct, "CHK");

                DataSet inDataSet = new DataSet();
                string bizRuleName = string.Empty;
                string outData = null;

                if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (IsSmallType)
                    {
                        bizRuleName = "BR_PRD_REG_START_INPUT_LOT_ASS";

                        DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));
                        inDataTable.Columns.Add("OUT_LOTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = EquipmentCode;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                        dr["EQPT_LOTID"] = null;
                        dr["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                        dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                        dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "TRAYID"));
                        dr["OUT_LOTID"] = null;
                        inDataTable.Rows.Add(dr);
                    }
                    else
                    {
                        bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";

                        DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = EquipmentCode;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                        dr["EQPT_LOTID"] = null;
                        inDataTable.Rows.Add(dr);

                        DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        ininput.Columns.Add("PRODID", typeof(string));
                        ininput.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
                        ininput.Columns.Add("INPUT_QTY", typeof(decimal));

                        DataRow newRow = ininput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "PRODID"));
                        newRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                        newRow["INPUT_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "WIPQTY2"));

                        ininput.Rows.Add(newRow);
                        outData = "OUT_EQP";
                    }
                }
                else if (string.Equals(ProcessCode, Process.WASHING))
                {
                    bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                    DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                    inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = EquipmentCode;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                    dr["EQPT_LOTID"] = null;
                    inDataTable.Rows.Add(dr);

                    DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    ininput.Columns.Add("PRODID", typeof(string));
                    ininput.Columns.Add("INPUT_LOTID", typeof(string));
                    ininput.Columns.Add("INPUT_QTY", typeof(decimal));

                    DataRow newRow = ininput.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboWaitHalfProduct.SelectedValue;
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "PRODID"));
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                    newRow["INPUT_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "WIPQTY2"));

                    ininput.Rows.Add(newRow);
                }


                //string xml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", outData, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //GetProductLot();
                    GetWaitHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputHalfProductCancel()
        {
            try
            {
                ShowParentLoadingIndicator();
                string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_TERMINATE_LOT_WS" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInHalfProduct.Rows.Count - dgInHalfProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //GetHalfProductList();
                        GetProductLot();
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
                Util.MessageException(ex);

            }
        }

        private void InputHalfProductCancelAssembly()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;
                ShowParentLoadingIndicator();

                //const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                string bizRuleName = IsSmallType ? "BR_PRD_REG_CANCEL_INPUT_LOT_ASS" : "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                
                DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_LOT_AS();
                DataTable inDataTable = inDataSet.Tables["INDATA"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = selectedProductRow["LOTID"].GetString();
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["INLOT"];
                for (int i = 0; i < dgInHalfProduct.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgInHalfProduct, "CHK", i))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_LOTID"));
                        row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "INPUT_QTY")).GetDecimal();
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        inInputTable.Rows.Add(row);
                    }
                }
                //string xmlText = inDataSet.GetXml();
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //GetHalfProductList();
                    GetProductLot();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputMaterialCancel()
        {
            try
            {
                DataRow selectedProductRow = GetSelectProductRow();
                if (selectedProductRow == null) return;
                ShowParentLoadingIndicator();

                //string bizRuleName = string.Equals(ProcessCode, Process.WASHING) ? "BR_PRD_REG_CANCEL_INPUT_LOT_WS" : "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                string bizRuleName;
                if (string.Equals(ProcessCode, Process.WASHING))
                {
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_WS";
                }
                else
                {
                    if (IsSmallType)
                    {
                        bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_ASS";
                    }
                    else
                    {
                        bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_AS";
                    }
                }

                DataSet inDataSet = _bizDataSet.GetBR_PRD_DEL_INPUT_LOT_AS();
                DataTable inDataTable = inDataSet.Tables["INDATA"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = ProdLotId;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = inDataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputMaterial.GetRowCount(); i++)
                {
                    // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                    if (_util.GetDataGridCheckValue(dgInputMaterial, "CHK", i) && !string.Equals(Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                    {
                        row = inInputTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "INPUT_LOTID"));
                        row["WIPQTY"] = 0;
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));

                        inInputTable.Rows.Add(row);
                    }
                }

                //string xmlText = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetMaterialList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void InputHistoryCancel()
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

        private void PopupReplaceWinding()
        {
            CMM_WINDING_PAN_REPLACE popPanReplace = new CMM_WINDING_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegmentCode;
            parameters[1] = EquipmentCode;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += popWindingReplace_Closed;

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void popWindingReplace_Closed(object sender, EventArgs e)
        {
            CMM_WINDING_PAN_REPLACE pop = sender as CMM_WINDING_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //GetMaterialInputList();
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

        private void PopupReplaceAssemblySmallType()
        {
            CMM_ASSY_CSH_PAN_REPLACE popPanReplace = new CMM_ASSY_CSH_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegmentCode;
            parameters[1] = EquipmentCode;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += popReplaceAssemblySmallType_Closed;

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void popReplaceAssemblySmallType_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CSH_PAN_REPLACE pop = sender as CMM_ASSY_CSH_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //GetMaterialInputList();
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

        private void PopupReplaceAssembly()
        {
            CMM_ASSY_WG_PAN_REPLACE popPanReplace = new CMM_ASSY_WG_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegmentCode;
            parameters[1] = EquipmentCode;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += popAssemblyReplace_Closed;

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void popAssemblyReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WG_PAN_REPLACE pop = sender as CMM_ASSY_WG_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //GetMaterialInputList();
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

        private void PopupReplaceWashingRework()
        {
            CMM_ASSY_WSH_REWORK_PAN_REPLACE popReplace = new CMM_ASSY_WSH_REWORK_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");

            object[] parameters = new object[6];
            parameters[0] = EquipmentCode;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            parameters[5] = IsRework;
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += popWashingReworkReplace_Closed;

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
                GetProductLot();
                //GetMaterialInputList();
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

        private void SaveQuality()
        {
            try
            {
                dgQuality.EndEdit();
                DataRow selectedProductRow = GetSelectProductRow();
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Int16));
                inTable.Columns.Add("CLCTSEQ", typeof(Int16));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("CLCTMAX", typeof(string));
                inTable.Columns.Add("CLCTMIN", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTSEQ_ORG", typeof(Int16));

                foreach (DataGridRow row in dgQuality.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = selectedProductRow["LOTID"].GetString();
                    dr["WIPSEQ"] = selectedProductRow["WIPSEQ"].GetString();
                    if (rdoLot.IsChecked == true)
                    {
                        dr["CLCTSEQ"] = 1;
                    }
                    dr["CLCTITEM"] = DataTableConverter.GetValue(row.DataItem, "CLCTITEM").GetString();
                    dr["CLCTVAL01"] = DataTableConverter.GetValue(row.DataItem, "CLCTVAL01").GetString();

                    if(DataTableConverter.GetValue(row.DataItem, "MAND_INSP_ITEM_FLAG").GetString().ToUpper() == "Y")
                    {
                        if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                        {
                            object[] parameters = new object[1];
                            string clssName1 = DataTableConverter.GetValue(row.DataItem, "CLSS_NAME1").GetString();
                            string clssName2 = DataTableConverter.GetValue(row.DataItem, "CLSS_NAME2").GetString();

                            if (String.IsNullOrEmpty(clssName2) == true)
                            {
                                parameters[0] = (clssName1);
                            }
                            else
                            {
                                parameters[0] = (clssName1 + " - " + clssName2);
                            }
                            //parameters[0] = (dr["CLSS_NAME1"].GetString() + dr["CLSS_NAME2"].GetString()).Length > 0 ? " - " + dr["CLSS_NAME2"].GetString(): string.Empty;
                            Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.
                            return;
                        }
                    }
                    dr["CLCTMAX"] = DataTableConverter.GetValue(row.DataItem, "USL");
                    dr["CLCTMIN"] = DataTableConverter.GetValue(row.DataItem, "LSL");
                    dr["EQPTID"] = EquipmentCode;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CLCTSEQ_ORG"] = DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    inTable.Rows.Add(dr);
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xmlText = ds.GetXml();

                string serviceName = "BR_QCA_REG_WIP_DATA_CLCT";
                if (rdoTime.IsChecked == true)
                {
                    serviceName = "BR_QCA_REG_WIP_DATA_CLCT_TIME_FOR_LOT";
                }

                if (inTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync(serviceName, "INDATA", null, inTable);
                    if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                    {
                        Util.MessageInfo("SFU1270");      //저장되었습니다.
                        //===================================================================
                        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
                        // 7.  [자동실적확정]호출(버튼x,타 이벤트0) :  
                        //      3.2 <품질검사 입력> 우측하단
                        //===================================================================
                        // 원통형 9,10호 Line, Winding 공정이면 품질검사 후 자동실적확정 처리 시작
                        if (IsSmallType ==true && (EquipmentSegmentCode.Equals("M2C09") || EquipmentSegmentCode.Equals("M2C10")) && ProcessCode.Equals("A2000"))
                        {
                            AutoConfirm_Call();
                        }
                        
                        GetQualityInfoList();
                    }
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }
                //HiddenParentLoadingIndicator();
            }
            catch (Exception ex)
            {
                //HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        //=====================================================================================================================
        // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        //  - 자동실적확정 호출
        //=====================================================================================================================
        protected virtual void AutoConfirm_Call()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("AutoConfirm_Call");
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

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

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

        private void GetTrayInfo(C1DataGrid dg, out string returnMessage, out string messageCode)
        {
            try
            {
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                //const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WNS";
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WN_CST";

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", indataTable);

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
        //생산반제품 조회
        public void GetProductCellList(C1DataGrid dg, bool isAsync = true)
        {
            try
            {
                if (isAsync)
                {
                    ShowParentLoadingIndicator();
                }

                SetDataGridCheckHeaderInitialize(dg);
                //string bizRuleName = ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH) ? "DA_PRD_SEL_OUT_LOT_LIST_WNS" : "DA_PRD_SEL_OUT_LOT_LIST_WS";
                string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WN_CST";

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
                //string xmlText = ds.GetXml();

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
                    _trayCheckSeq = 0;

                    if (dg.Rows.Count > 0)
                        _trayCheckSeq = dg.Rows.Count;
                        dg.GetCell(dg.Rows.Count, 1);
                        
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

        private void GetQualityInfoList()
        {
            try
            {
                Util.gridClear(dgQuality);
                if (GetSelectProductRow() == null) return;
                
                ShowParentLoadingIndicator();

                string bizRuleName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
                if (rdoTime.IsChecked == true)
                {
                    bizRuleName = "BR_QCA_SEL_SELF_INSP_CLCTITEM_TIME";

                    if (LoginInfo.CFG_SHOP_ID.Equals("A010") && LoginInfo.CFG_AREA_ID.Equals("M2") && (LoginInfo.CFG_EQSG_ID.Equals("M2C09") || LoginInfo.CFG_EQSG_ID.Equals("M2C10")) && LoginInfo.CFG_PROC_ID.Equals("A2000"))
                    {
                        bizRuleName = "BR_QCA_SEL_SELF_INSP_CLCTITEM_TIMEZONE_LOT";
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_BAS_CODE", typeof(string));
                inTable.Columns.Add("CLCT_ITVL", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = ProcessCode;
                dr["LOTID"] = ProdLotId;
                dr["WIPSEQ"] = ProdWorkInProcessSequence;
                dr["EQPTID"] = EquipmentCode;
                dr["CLCT_BAS_CODE"] = "LOT";
                if (rdoTime.IsChecked == true)
                {
                    dr["CLCT_BAS_CODE"] = "TIME";
                }
                if (rdoTime.IsChecked == true && cboTime.Items.Count > 0)
                {
                    dr["CLCT_ITVL"] = cboTime.SelectedValue;
                }
                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, ex) =>
                {
                    HiddenParentLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    Util.GridSetData(dgQuality, result, FrameOperation, true);

                    if (LoginInfo.CFG_SHOP_ID.Equals("A010") && LoginInfo.CFG_AREA_ID.Equals("M2") && (LoginInfo.CFG_EQSG_ID.Equals("M2C09") || LoginInfo.CFG_EQSG_ID.Equals("M2C10")) && LoginInfo.CFG_PROC_ID.Equals("A2000"))
                    {
                        if (rdoTime.IsChecked == true)
                        {
                            List<int> list = new List<int>();
                            DateTime dt_Start_DTTM = DateTime.Now;
                            for (int r = 0; r < result.Rows.Count; r++)
                            {
                                DataRow row = result.Rows[r];
                                dt_Start_DTTM = (DateTime)row["START_DTTM"];
                                string sStart_DTTM = dt_Start_DTTM.ToString("HH");
                                int nHH = Util.NVC_Int(sStart_DTTM);
                                list.Add(nHH);
                            }

                            if (list.Count != 0)
                            {
                                dgQuality.Columns["CLCTVAL01"].Header = list.Max().ToString();
                            }
                            else
                            {
                                dgQuality.Columns["CLCTVAL01"].Header = ObjectDic.Instance.GetObjectName("측정값");
                            }
                        }
                        else
                        {
                            dgQuality.Columns["CLCTVAL01"].Header = ObjectDic.Instance.GetObjectName("측정값");
                        }
                    }

                    if (rdoTime.IsChecked == true && cboTime.Items.Count < 1)
                    {   
                        MakeTimeCombo(result.Copy());
                    }

                    _util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "CLSS_NAME1", "CLSS_NAME2", "CLSS_NAME3" }, DataGridMergeMode.VERTICALHIERARCHI);
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MakeTimeCombo(DataTable dt)
        {
            var query = dt.Copy().AsEnumerable()
                .OrderBy(o => o.Field<Int32>("CLCT_ITVL"))
                .GroupBy(g => new { clctVal = g.Field<Int32>("CLCT_ITVL") })
                .Select(s => s.Key.clctVal).ToList();

            DataTable dtTime = new DataTable();
            dtTime.Columns.Add("CODE", typeof(string));
            dtTime.Columns.Add("CODENAME", typeof(string));

            foreach (var item in query)
            {
                DataRow dr = dtTime.NewRow();
                dr["CODE"] = item.GetString();
                dr["CODENAME"] = item.GetString();
                dtTime.Rows.Add(dr);
            }

            DataRow newRow = dtTime.NewRow();
            newRow["CODE"] = null;
            newRow["CODENAME"] = "-ALL-";
            dtTime.Rows.InsertAt(newRow, 0);

            cboTime.DisplayMemberPath = "CODENAME";
            cboTime.SelectedValue = "CODE";
            cboTime.ItemsSource = dtTime.Copy().AsDataView();
            cboTime.SelectedIndex = 0;

            cboTime.SelectedValueChanged += cboTime_SelectedValueChanged;
        }

        private void cboTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetQualityInfoList();
        }

        private void SaveDefectReInput()
        {
            try
            {
                dgDefectReInput.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_FOR_REINPUT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefectReInput.Rows.Count - dgDefectReInput.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = ProdWorkInProcessSequence;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNNOTE"));
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //if (bMsgShow)
                //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                //IsChangeDefect = false;
                GetProductLot();
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    //object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
                    return (DataRow)methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
                return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    return (DataRow) methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
                return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public void ClearDataCollectControl()
        {
            Util.gridClear(dgDefect);
            Util.gridClear(dgProdCellWinding);
            Util.gridClear(dgWashingResult);
            Util.gridClear(dgMaterialInput);
            Util.gridClear(dgWaitPancake);
            Util.gridClear(dgWaitHalfProduct);
            Util.gridClear(dgInHalfProduct);
            Util.gridClear(dgInputMaterial);
            Util.gridClear(dgInputHist);
            Util.gridClear(dgQuality);
            Util.gridClear(dgDefectReInput);

            txtMaterialInputLotID.Text = string.Empty;
            txtWaitPancakeLot.Text = string.Empty;
            txtTrayId.Text = string.Empty;
            txtInHalfProductLot.Text = string.Empty;
            txtInputMaterialLotID.Text = string.Empty;
            txtHistLotID.Text = string.Empty;

            _trayId = string.Empty;
            _trayTag = string.Empty;
            _outLotId = string.Empty;
            _wipQty = string.Empty;

            ProdWorkOrderId = string.Empty;
            ProdWorkOrderDetailId = string.Empty;
            ProdLotState = string.Empty;
            ProdLotId = string.Empty;
            ProdWorkInProcessSequence = string.Empty;
            CellManagementTypeCode = string.Empty;
            EquipmentCodeName = string.Empty;
            ProdSelectedCheckRowIdx = -1;
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

        private bool ValidationTrayConfirm(C1DataGrid dg)
        {
            try
            {
                if (GetSelectProductRow() == null)
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

        private bool ValidationConfirmCancel(C1DataGrid dg)
        {
            try
            {

                if (GetSelectProductRow() == null)
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

        private bool ValidationDischarge(C1DataGrid dg)
        {
            try
            {
                if (GetSelectProductRow() == null)
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
                if (GetSelectProductRow() == null)
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

        private bool ValidationTrayDelete(C1DataGrid dg)
        {

            try
            {
                if (GetSelectProductRow() == null)
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

        private bool ValidationAutoInputLot()
        {
            if (GetSelectProductRow() == null)
            {
                Util.MessageValidation("SFU1664");
                return false;
            }

            if (string.IsNullOrEmpty(txtMaterialInputLotID.Text.Trim()))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtMaterialInputLotID.Text.Trim()))
                    {
                        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidationMaterialInputCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (GetSelectProductRow() == null)
            {
                Util.MessageValidation("SFU1640");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialInputReplace()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialInputComplete()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationWaitPanCakeInput()
        {
            
            if (GetSelectProductRow() == null)
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return false;
            }

            if (cboPancakeMountPstnID.SelectedValue == null || cboPancakeMountPstnID.SelectedValue.Equals("") || cboPancakeMountPstnID.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitPancake, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }


            int rowIndex = _util.GetDataGridRowIndex(dgMaterialInput, "EQPT_MOUNT_PSTN_ID", cboPancakeMountPstnID.SelectedValue.ToString());
            if (rowIndex >= 0)
            {
                string classCode = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "PRDT_CLSS_CODE"));

                if (classCode != "C")
                {
                    //string sInPancake = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "INPUT_LOTID"));
                    string sInState = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "WIPSTAT"));
                    string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                    if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
                    {
                        //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[rowIndex].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                        return false;
                    }
                }
            }

            //if (iRow >= 0)
            //{
            //    string sInPancake = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID"));
            //    string sInState = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT"));
            //    string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "MOUNT_MTRL_TYPE_CODE"));

            //    if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
            //    {
            //        //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
            //        Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
            //        return false;
            //    }
            //}


            return true;
        }

        private bool ValidationWaitHalfProductInput()
        {
            if (cboWaitHalfProduct.SelectedValue == null || cboWaitHalfProduct.SelectedValue.GetString() == "SELECT" || string.IsNullOrEmpty(cboWaitHalfProduct.SelectedValue.GetString()))
            {
                //투입 위치를 선택하세요.
                Util.MessageValidation("SFU1957");
                return false;
            }

            if (_util.GetDataGridCheckCnt(dgWaitHalfProduct, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationInHalfProductInPutCancel()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInHalfProductInPutQty()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInHalfProduct, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInHalfProduct.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInputMaterialCancel()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputMaterial, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputMaterial.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationInputHistoryCancel(C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationQualitySave()
        {
            if (GetSelectProductRow() == null)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgQuality))
            {
                //Data가 존재하지 않습니다.
                Util.MessageValidation("SFU1331");
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefectReInput()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectReInput))
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

        private void CheckInDataGridCheckBox(C1DataGrid dg, DataGridBeganEditEventArgs e)
        {
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

        private void SetComboBox()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { EquipmentCode, "PROD" };
                String[] sFilter2 = { EquipmentCode, null }; // 자재,제품 전체
                String[] sFilter3 = { EquipmentCode, "MTRL" }; // 자재,제품 전체
                //MTRL

                combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboWaitHalfProduct, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboInHalfMountPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboInputMaterialMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
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

        private void ShowParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
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
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private decimal GetSumDefectQty()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
                return 0;

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            decimal defectqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("DEFECT_LOT") && !w.Field<string>("RSLT_EXCL_FLAG").Equals("Y")).Sum(s => s.Field<decimal>("RESNQTY"));
            decimal lossqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("LOSS_LOT")).Sum(s => s.Field<decimal>("RESNQTY"));
            decimal chargeprdqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("CHARGE_PROD_LOT")).Sum(s => s.Field<decimal>("RESNQTY"));

            return defectqty + lossqty + chargeprdqty;
        }

        #endregion

        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SetTimeControl();
                GetQualityInfoList();
            }
        }

        private void rdoTime_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SetTimeControl();
                GetQualityInfoList();
            }
        }

        private void SetTimeControl()
        {
            if (rdoTime.IsChecked == true)
            {
                txtTime.Visibility = Visibility.Visible;
                cboTime.Visibility = Visibility.Visible;
            }
            else
            {
                txtTime.Visibility = Visibility.Collapsed;
                cboTime.Visibility = Visibility.Collapsed;
            }

        }

        private void dgDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                switch (Convert.ToString(e.Cell.Column.Name))
                {
                    case "CELLQTY":
                        SetParentQty();                
                        break;
                }

                if (strReloadFlag.Equals("Y"))
                {
                    GetProductCellList(dg, true);
                }
            }
        }
        private void SetParentQty()
        {
            string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "STAT_CODE"));
            string checkFlag = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK").GetString();
            decimal cellQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CELLQTY").GetDecimal();
            decimal cellCstQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CST_CELL_QTY").GetDecimal();                

            if (cellQty > cellCstQty)
            {
                Util.MessageValidation("SFU1500", txtTrayIDInline.Text.ToUpper());
                btnOutSave.IsEnabled = false;
                DataTableConverter.SetValue(dgProdCellWinding.Rows[idxCheck].DataItem, "CHK", false);
                strReloadFlag = "Y";
            }else
            {
                strReloadFlag = "";
            }
        }

        // Tray 조회 화면 팝업 2019.04.18
        private void btnSearchTray_Click(object sender, RoutedEventArgs e)
        {
            CMM_WINDING_TRAY_SEARCH popPanReplace = new CMM_WINDING_TRAY_SEARCH { FrameOperation = FrameOperation };
            popPanReplace.Show();

            // 추후 pram 으로 넘길시 아래 소스 참조
            //CMM_WINDING_TRAY_SEARCH _InputProductray = sender as CMM_WINDING_TRAY_SEARCH;
            //_InputProductray.FrameOperation = FrameOperation;

            //if (_InputProductray != null)
            //{
            //    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView; // 돋보기 버튼 클릭한 Row
            //    //string sElectrodeCode = dataRow.Row["ELECTRODECODE"].ToString();
            //    string sElectrodeCode = "";
            //    int rowIndex = -1;

            //    for (int i = 0; i < dgProdCellWinding.Rows.Count; i++)
            //    {
            //        if (string.Equals(sElectrodeCode, dgProdCellWinding[i, 0].Text))
            //        {
            //            rowIndex = i;
            //            break;
            //        }
            //    }

            //    // SET PARAMETER
            //    object[] parameters = new object[11];
            //    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[0].DataItem, "WOID"));                    // WOID
            //    parameters[1] = Util.NVC(lineID);                                                                               // Line
            //    parameters[2] = Util.NVC(procID);                                                                               // 공정코드
            //    parameters[3] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "ELECTRODECODE")); // 전극
            //    parameters[4] = "1";                                                                                            //     
            //    parameters[5] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "LOTID"));         // 기능추가로 LOTID 추가
            //    parameters[6] = "D";                                                                                            // 페이지 구분자
            //    parameters[7] = Util.NVC(eqptID);                                                                               // 설비코드
            //    parameters[8] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODID"));        // 제품ID
            //    parameters[9] = false;                                                                                          // 원각/초소형 구분
            //    parameters[10] = "N";  //엔터/조회버튼 클릭여부 
            //    C1WindowExtension.SetParameters(_InputProductray, parameters);

            //    _InputProductray.Closed += new EventHandler(InputHalfProduct_Closed);
            //    this.Dispatcher.BeginInvoke(new Action(() => _InputProductray.ShowModal()));
                
            //}
        }
        private void InputHalfProduct_Closed(object sender, EventArgs e)
        {
            CMM_WAITING_PANCAKE_SEARCH _InputProductray = new CMM_WAITING_PANCAKE_SEARCH();

            DataRow selectRow = null;

            if (_InputProductray.DialogResult == MessageBoxResult.OK)
            {
                selectRow = _InputProductray.SELECTEDROW;

                for (int i = 0; i < dgProdCellWinding.Rows.Count; i++)
                {
                    if (string.Equals(_InputProductray.ELECTRODETYPE, dgProdCellWinding[i, 0].Text))
                    {
                        //DataTableConverter.SetValue(dgProdCellWinding.Rows[i].DataItem, "LOTID", Convert.ToString(selectRow["LOTID"]));
                        //DataTableConverter.SetValue(dgProdCellWinding.Rows[i].DataItem, "PRODID", Convert.ToString(selectRow["PRODID"]));
                        //DataTableConverter.SetValue(dgProdCellWinding.Rows[i].DataItem, "PRODNAME", Convert.ToString(selectRow["PRODNAME"]));
                        //DataTableConverter.SetValue(dgProdCellWinding.Rows[i].DataItem, "WIPQTY", Convert.ToString(selectRow["WIPQTY"]));
                        //DataTableConverter.SetValue(dgProdCellWinding.Rows[i].DataItem, "PRJT_NAME", Convert.ToString(selectRow["PRJT_NAME"]));

                        break;
                    }
                }
            }

            //this.BringToFront();
        }
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
                    btnInputHistoryCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInputHistoryCancel.Visibility = Visibility.Collapsed;
                }
                */

                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE5"]).Trim().Equals("Y"))
                {
                    btnInputHistoryCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInputHistoryCancel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }

   

}
