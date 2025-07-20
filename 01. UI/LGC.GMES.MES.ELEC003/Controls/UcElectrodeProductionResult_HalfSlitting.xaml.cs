/*************************************************************************************
 Created Date : 2021.02.19
      Creator : 정문교
   Decription : 전극 공정진척 - 생산실적 (E2500 Half Slitting)
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeProductionResult_HalfSlitting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeProductionResult_HalfSlitting : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable DtEquipment { get; set; }
        public DataRowView DvProductLot { get; set; }
        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public string UnldrLotIdentBasCode { get; set; }

        public bool bProductionUpdate { get; set; }

        public bool bChangeQuality
        {
            get { return _isChangeQuality; }
        }
        public bool bChangeRemark
        {
            get { return _isChangeRemark; }
        }
        public decimal ExceedLengthQty
        {
            get { return _exceedLengthQty; }
        }
        public string ItemCodeLenLack
        {
            get { return _itemCodeLenLack; }
        }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        bool _isResnCountUse = false;
        bool _isDupplicatePopup = false;

        // DataCollect 변경 여부
        bool _isChangeWipReason = false;                      // 불량/LOSS/물품청구
        bool _isChangeQuality = false;                        // 품질정보
        bool _isChangeRemark = false;                         // 특이사항

        decimal _exceedLengthQty;
        decimal _convRate;

        private const string _itemCodeLenLack = "LENGTH_LACK";
        private const string _itemCodeLenExceed = "LENGTH_EXCEED";

        private DataTable dtWipReasonBak;       // WIPREASONCOLLECT의 이전값을 보관하기 위한 DataTable(C20190404_67447) [2019-04-11]

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));


        public UcElectrodeProductionResult_HalfSlitting()
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

            InitializeControls();
            //SetControl();
            SetButtons();
            //SetControlVisibility();
            SetPrivateVariable();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            this.RegisterName("greenBrush", greenBrush);
        }

        private void SetButtons()
        {
        }

        private void SetControl()
        {
            recEquipment.Fill = greenBrush;
            txtEquipment.Text = "[" + EquipmentCode + "] " + EquipmentName;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "greenBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);

            // 슬리터 기본 설정 (소형 체크로직을 Setting에서 불러와서 사용)
            if (LoginInfo.CFG_ETC != null && LoginInfo.CFG_ETC.Rows.Count > 0)
                chkSum.IsChecked = Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_SMALLTYPE]);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetControlClear()
        {
            txtVersion.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtUnit.Text = string.Empty;
            txtLaneQty.Value = 0;
            txtLanePatternQty.Value = 0;

            txtInputQty.Value = 0;
            txtParentQty.Value = 0;
            txtRemainQty.Value = 0;

            _isResnCountUse = false;
            _isDupplicatePopup = false;

            // DataCollect 변경 여부
            _isChangeWipReason = false;                      // 불량/LOSS/물품청구
            _isChangeQuality = false;                        // 품질정보
            _isChangeRemark = false;                         // 특이사항

            bProductionUpdate = false;                       // 실적 저장, 불량/LOSS/물품청구 저장시 True

            _exceedLengthQty = 0;

            Util.gridClear(dgWipReason);
            Util.gridClear(dgWipMerge);
            Util.gridClear(dgWipMerge2);
            Util.gridClear(dgQuality);
            Util.gridClear(dgRemark);
            Util.gridClear(dgRemarkHistory);
        }

        private void SetControlVisibility()
        {
            // 이전 LotId
            if (LdrLotIdentBasCode.Equals("CST_ID") || LdrLotIdentBasCode.Equals("RF_ID") ||
                UnldrLotIdentBasCode.Equals("CST_ID") || UnldrLotIdentBasCode.Equals("RF_ID"))
            {
                dgProductResult.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgProductResult.Columns["PR_LOTID"].Visibility = Visibility.Visible;
            }

            if (UnldrLotIdentBasCode.Equals("CST_ID") || UnldrLotIdentBasCode.Equals("RF_ID"))
            {
                dgProductResult.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductResult.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
            }

            if (LdrLotIdentBasCode.Equals("CST_ID") || LdrLotIdentBasCode.Equals("RF_ID"))
            {
                dgProductResult.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductResult.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }
        }

        private void SetPrivateVariable()
        {
            _isResnCountUse = IsAreaCommonCodeUse("RESN_COUNT_USE_YN", ProcessCode);
        }

        #endregion

        #region Event

        private void tcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.RemovedItems.Count > 0)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiWipReason"))
                    {
                        dgWipReason.EndEdit(true);
                    }
                }
            }

        }

        private void dgProductResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (dataGrid.GetRowCount() > 0)
                                {
                                    if (e.Cell.Column.Visibility == Visibility.Visible)
                                    {
                                        TextBlock sContents = e.Cell.Presenter.Content as TextBlock;

                                        int iSourceIdx = e.Cell.Row.Index - (dataGrid.Rows.Count - dataGrid.BottomRows.Count) + dataGrid.TopRows.Count;

                                        if (DataTableConverter.Convert(dataGrid.ItemsSource).Columns.Contains(e.Cell.Column.Name))
                                        {
                                            string sValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[iSourceIdx].DataItem, e.Cell.Column.Name));

                                            if (string.Equals(e.Cell.Column.Name, "LANE_QTY"))
                                            {
                                                sContents.Text = sValue;
                                            }
                                            else
                                            {
                                                if (e.Cell.Column.GetType() == typeof(DataGridNumericColumn))
                                                    sContents.Text = GetUnitFormatted(Convert.ToDouble(Util.NVC_Decimal(string.IsNullOrEmpty(sValue) ? "0" : sValue) * _convRate), "EA");
                                                else
                                                    sContents.Text = sValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgProductResult_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }
        
        /// <summary>
        /// 저장 
        /// </summary>
        private void btnProductionUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductionUpdate()) return;

            if (txtInputQty.Value <= 0)
            {
                SaveProductionUpdate();
            }
        }

        #region **불량/LOSS/물품청구

        /// <summary>
        /// 소형 전극 체크 박스
        /// </summary>
        protected virtual void OnSumReasonGridChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb != null && (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) > dgProductResult.TopRows.Count)
            {
                dgWipReason.Refresh(false);
                SelectDefect();
            }
        }

        /// <summary>
        /// Lane 선택
        /// </summary>
        protected virtual void OnLaneChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                if (string.Equals(checkBox.Tag, "ALL"))
                {
                    foreach (CheckBox _checkBox in cboLaneNum.Items)
                        _checkBox.IsChecked = checkBox.IsChecked;
                }
                else
                {
                    SetVisibilityWipReasonGrid(Util.NVC(checkBox.Tag), checkBox.IsChecked);
                }
            }
        }

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //if (e.Cell != null && e.Cell.Presenter != null)
                    //{
                    //    if (e.Cell.Row.Type == DataGridRowType.Item)
                    //    {
                    //        if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    //        {
                    //            if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                    //            if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                    //                    (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                    //            if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                    //            // 길이부족 차감 색상표시 추가 [2019-12-09]
                    //            if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                    //                    (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRCS_ITEM_CODE"), "PROD_QTY_INCR")))
                    //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D6606D"));
                    //        }
                    //    }
                    //}
                }));
            }


        }

        private void dgWipReason_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e != null && e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgWipReason_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (dataGrid.Columns["ALL"].Index < e.Column.Index && dataGrid.Columns["COSTCENTERID"].Index > e.Column.Index)
                {
                    if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK") ||
                        string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                    {
                        e.Cancel = true;
                        dataGrid.BeginEdit(e.Row.Index, dataGrid.Columns["ALL"].Index);
                    }
                }

                if (_isResnCountUse == true)
                {
                    if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "DEFECT_LOT") ||
                        string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "LOSS_LOT"))
                    {
                        if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y") &&
                            string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "LINK_DETL_RSN_CODE_TYPE"))))
                            return;
                    }

                    if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true)
                        e.Cancel = true;
                }
            }
        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (ValidateDefect(dg))
            {
                DefectChange(dg, dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index);

                SetExceedLength();
                dgProductResult.Refresh(false);
            }

            _isChangeWipReason = true;
        }

        private void dgWipReason_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null && dg.CurrentCell != null && dg.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dg.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, 0);
                        dg.BeginEdit(dg.CurrentCell);
                        dg.EndEdit(true);

                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, DBNull.Value);

                        if (dg.CurrentCell != null && dg.CurrentCell.Presenter != null)
                        {
                            dg.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dg.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dg.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }

                        // 식제시 길이초과 재산출
                        SetExceedLength();
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false && dg.CurrentCell.IsEditable == false)
                        dg.BeginEdit(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index);
                }
            }
        }

        /// <summary>
        /// 전체 저장
        /// </summary>
        private void btnSaceAllWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQuality(true)) return;

            // 불량/LOSS/물품청구
            SaveDefect(dgWipReason, true);

            // 품질정보
            SaveQuality(dgQuality, true);

            // 특이사항
            SaveWipNote(dgRemark);
        }

        /// <summary>
        /// 저장
        /// </summary>
        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefect()) return;

            SaveDefect(dgWipReason);
        }
        #endregion

        #region **품질정보
        /// <summary>
        /// 측정값 
        /// </summary>
        private void OnKeyDownMesualQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgProductResult.GetRowCount() < 1)
                    return;

                C1NumericBox numericBox = sender as C1NumericBox;
                if (numericBox != null)
                {
                    for (int i = 0; i < dgQuality.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgQuality.Rows[i].DataItem, "CLCTVAL01", numericBox.Value);

                        decimal iUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgQuality.Rows[i].DataItem, "USL"));
                        decimal iLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgQuality.Rows[i].DataItem, "LSL"));

                        if (iLSL == 0 && iUSL == 0)
                            continue;

                        if (dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter != null)
                        {
                            if (iLSL > Util.NVC_Decimal(numericBox.Value) || (iUSL > 0 && iUSL < Util.NVC_Decimal(numericBox.Value)))
                            {
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    _isChangeQuality = true;
                    numericBox.Value = 0;
                }
            }
        }

        private void OnKeyLostMesualQty(object sender, RoutedEventArgs e)
        {
            C1NumericBox numericBox = sender as C1NumericBox;

            if (numericBox != null)
                if (numericBox.Value > 0)
                    OnKeyDownMesualQty(numericBox, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // 자동차 2동 요구사항으로 인하여 Event재 정의를 함으로써 Focus가 정확히 이동 안하는 현상 때문에 해당 이벤트 추가 [2019-05-01]
                if (Convert.ToBoolean(e.NewValue) == true)
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    int iRowIdx = p.Cell.Row.Index;
                    int iColIdx = p.Cell.Column.Index;
                    C1.WPF.DataGrid.C1DataGrid grid = p.DataGrid;

                    if (grid.CurrentCell.Column.Index != iColIdx)
                        grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);

                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        // 액셀파일 PASTE시 공란PASS없이 전체 붙여넣기 추가 [2019-01-28]
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line.Trim());

                            iRowIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (_isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                _isDupplicatePopup = true;
                int iRowIdx = 0;
                int iColIdx = 0;
                int iMeanColldx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    iMeanColldx = dgQuality.Columns["MEAN"].Index;

                    grid = p.DataGrid;

                    C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);

                    string sLSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "LSL"));
                    string sCSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "CSL"));
                    string sUSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "USL"));


                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        if (sLSL != "" && Util.NVC_Decimal(item.Value) < Util.NVC_Decimal(sLSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        else if (sUSL != "" && Util.NVC_Decimal(item.Value) > Util.NVC_Decimal(sUSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            currentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        _isChangeQuality = true;
                    }
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    _isChangeQuality = true;
                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _isDupplicatePopup = false;
            }
        }

        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null)
            {
                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                                C1.WPF.DataGrid.C1DataGrid grid;
                                grid = p.DataGrid;

                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
                                string sCSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSL"));
                                string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));

                                if (panel != null)
                                {
                                    if (string.Equals(sCode, "NUM"))
                                    {
                                        C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                        // 재설정
                                        if (string.Equals(txtUnit.Text, "EA"))
                                            numeric.Format = "F1";
                                        else
                                            numeric.Format = GetUnitFormatted();

                                        // SRS요청으로 동별 LIMIT값 설정 [2017-11-30]
                                        if (!string.IsNullOrEmpty(sLSL_Limit) && Util.NVC_Decimal(sLSL_Limit) > 0)
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            // 액셀 붙여넣기 기능으로 빈칸이 입력될 경우 Convert클래스 이용 시 오류 발생 문제로 체크용 Function 교체 [2019-01-28]
                                            if (!string.IsNullOrWhiteSpace(sValue) && !string.Equals(sValue, "NaN"))
                                            {
                                                //소수점Separator에 따라 분기(우크라이나 언어)
                                                if (sValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.CurrentCulture.NumberFormat);
                                                else
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture.NumberFormat);
                                            }

                                            if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }

                                            else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }
                                            else
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                            }
                                        }
                                        numeric.IsKeyboardFocusWithinChanged -= OnDataCollectGridFocusChanged;
                                        numeric.IsKeyboardFocusWithinChanged += OnDataCollectGridFocusChanged;
                                        numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.LostKeyboardFocus -= OnDataCollectGridGotKeyboardLost;
                                        numeric.LostKeyboardFocus += OnDataCollectGridGotKeyboardLost;
                                    }
                                }
                            }
                            else if (string.Equals(e.Cell.Column.Name, "MEAN"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }

                    }
                }));
            }

        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e != null && e.Cell != null && e.Cell.Presenter != null)
                        {
                            e.Cell.Presenter.Background = null;

                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }
            }));

        }

        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            //if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            //{
            //    sValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTVAL02"));
            //    sCLCITEM = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
            //    sCLCNAME = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
            //               Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
            //               Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
            //}
            //else
            //{
                sValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTVAL01"));
            //}
            string sCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        //Util.MessageValidation("SFU1806");  //입력값이 하한값 보다 작습니다
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                    {
                        //Util.MessageValidation("SFU1805");  //입력값이 상한값 보다 큽니다
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    _isChangeQuality = true;

                    DataTable dataCollect = DataTableConverter.Convert(dg.ItemsSource);
                    int iHGCount1 = 0;  // H/G
                    int iHGCount2 = 0;  // M/S
                    int iHGCount3 = 0;  // 1차 H/G
                    int iHGCount4 = 0;  // 1차 M/S
                    decimal sumValue1 = 0;
                    decimal sumValue2 = 0;
                    decimal sumValue3 = 0;
                    decimal sumValue4 = 0;
                    foreach (DataRow row in dataCollect.Rows)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount3++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount4++;
                            }
                        }
                    }

                    if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                }
            }
        }

        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg != null && dg.CurrentCell != null && dg.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dg.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, 0);
                        dg.BeginEdit(dg.CurrentCell);
                        dg.EndEdit(true);

                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, DBNull.Value);

                        if (dg.CurrentCell != null && dg.CurrentCell.Presenter != null)
                        {
                            dg.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dg.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dg.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false && dg.CurrentCell.IsEditable == false)
                        dg.BeginEdit(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index);
                }
            }
        }

        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQuality()) return;

            SaveQuality(dgQuality);
        }

        #endregion

        #region **특이사항
        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 1)
            {
                Grid grid = e.Cell.Presenter.Content as Grid;

                if (grid != null)
                {
                    TextBox remarkText = grid.Children[0] as TextBox;

                    if (remarkText != null)
                    {
                        remarkText.LostKeyboardFocus -= OnRemarkLostKeyboardFocus;
                        remarkText.LostKeyboardFocus += OnRemarkLostKeyboardFocus;
                    }
                }
            }
            else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 1)
            {
                Grid grid = e.Cell.Presenter.Content as Grid;

                if (grid != null)
                {
                    TextBox remarkText = grid.Children[0] as TextBox;

                    if (remarkText != null)
                    {
                        remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                        remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;
                    }
                }
            }
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
            {
                _isChangeRemark = true;
            }
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                _isChangeRemark = true;
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemark(dgRemark)) return;

            SaveWipNote(dgRemark);
        }
        #endregion

        #region **합권취
        private void btnSearchMerge_Click(object sender, RoutedEventArgs e)
        {
            SelectMergeFrom();
            SelectMergeTo();
        }

        private void btnSaveMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMerge()) return;

            SaveMerge();
        }
        #endregion

        #endregion

        #region Mehod

        #region [외부호출]
        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnProductionUpdate);                // 저장
            listAuth.Add(btnSaceAllWipReason);                // 불량/LOSS/물품청구 : 전체저장
            listAuth.Add(btnSaveWipReason);                   // 불량/LOSS/물품청구 : 저장
            listAuth.Add(btnSaveQuality);                     // 품질정보 : 저장
            listAuth.Add(btnSaveRemark);                      // 특이사항 : 저장
            listAuth.Add(btnSearchMerge);                     // 합권취 : 조회
            listAuth.Add(btnSaveMerge);                       // 합권취 : 저장

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductionResult()
        {
            SetControl();
            SetControlClear();
            SetControlVisibility();

            // 실적
            SetProductionResult();

            // 불량/LOSS/물품청구
            SelectDefect();
            // 합권취                    =========================> 합권취 탭의 조회 버튼 클릭시 조회됨
            // 품질정보
            SelectQuality();
            //// 특이사항                =========================> 실적 조회시 조회됨
            //SelectRemark();
            // 이전특이사항
            SelectRemarkPrevious();

            //this.Cursor = Cursors.Arrow;
        }

        #endregion

        #region [BizCall]


        /// <summary>
        /// 작업일
        /// </summary>
        private void SetCalDate(TextBox tb)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = Util.NVC(DvProductLot["EQPTID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(bizResult.Rows[0]["CALDATE"])))
                        {
                            tb.Text = Convert.ToDateTime(bizResult.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                            tb.Tag = Convert.ToDateTime(bizResult.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            tb.Text = DateTime.Now.ToString("yyyy-MM-dd");
                            tb.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetProcessVersion()
        {
            // VERSION, LANE수를 룰에 따라 가져옴 [2017-02-17]
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MODLID", typeof(string));
                inTable.Columns.Add("PROCSTATE", typeof(string));
                inTable.Columns.Add("TOPLOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["PROCID"] = ProcessCode;
                indata["EQPTID"] = EquipmentCode;
                indata["LOTID"] = DvProductLot["LOTID"];
                indata["MODLID"] = DvProductLot["PRODID"]; 
                indata["PROCSTATE"] = "Y";
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        /// <summary>
        /// 불량 Count 사용여부
        /// </summary>
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = sCodeType;
                newRow["COM_CODE"] = sCodeName;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }

        private void SetParentQty()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = Util.NVC(DvProductLot["LOTID"]); 
                indata["WIPSTAT"] = Util.NVC(DvProductLot["WIPSTAT"]);

                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QTY", "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    if (DvProductLot["WIPSTAT"].ToString().Equals(Wip_State.EQPT_END))
                        txtInputQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_OUT"]);

                    txtParentQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_IN"].ToString());
                    SetParentRemainQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetSaveProductQty()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR_FOR_PROD_QTY", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                    if (Util.NVC_Decimal(result.Rows[0]["PROD_QTY"]) > 0)
                        txtInputQty.Value = Convert.ToDouble(result.Rows[0]["PROD_QTY"]);
            }
            catch (Exception ex) { }
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = prodID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                        if (string.Equals(row["PROD_VER_CODE"], txtVersion.Text) && !string.IsNullOrEmpty(Util.NVC(row["PTN_LEN"])))
                            return Util.NVC_Decimal(row["PTN_LEN"]);

                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private void SetResultInfo()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID_PR"] = DvProductLot["WIPSTAT"].ToString() == Wip_State.EQPT_END ? null : DvProductLot["LOTID"];
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["WIPSTAT"] = DvProductLot["WIPSTAT"];
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_INFO_RP", "INDATA", "RSLTDT", inTable);

                // LOT 실적
                Util.GridSetData(dgProductResult, dtResult, null, false);

                // 모LOT투입량 산출
                SetParentQty();
                // 특이사항
                DataTable dtCopy = dtResult.Copy();
                BindingWipNote(dtCopy);

                // 해당 설비 완공 시점에서는 설비완공 시점에서 투입량을 수량으로 변경한다 [2017-02-14]
                if (string.Equals(DvProductLot["WIPSTAT"].ToString(), Wip_State.EQPT_END) && txtInputQty.IsReadOnly == false)
                    txtInputQty.Value = Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "EQPT_END_QTY"));

                // 절연코터, BACK WINDER는 자동 입력 후 수정 못하게 변경 (믹서는 투입자재 총수량 = 생산량)
                // 믹서 공정 다시 설비완공수량을 생산량으로 자동입력하게 변경, 또한 표면검사는 투입량 -> 생산량 자동입력 및 수정 가능하게 변경 요청
                // 백와인더, INS슬리터 코터는 모LOT 투입 기준 수정X, 나머지 공정들은 모LOT 투입 기준 수정 O
                txtInputQty.Value = txtParentQty.Value;

                // 저장되어 있는 수량이 있으면 그 수량을 최선책으로 지정 [2017-04-21]
                SetSaveProductQty();

                SetInputQty();
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        /// <summary>
        /// 실적 확정 여부 체크
        /// </summary>
        public bool CheckConfirmLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && !string.Equals(ProcessCode, dtResult.Rows[0]["PROCID"]) && (string.Equals(INOUT_TYPE.IN, dtResult.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dtResult.Rows[0]["WIP_TYPE_CODE"])))
                {
                    Util.MessageValidation("SFU5066");     // 이미 실적 확정 된 LOT입니다.
                    return false;
                }

            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        /// <summary>
        /// 저장 
        /// </summary>
        private void SaveProductionUpdate()
        {
            try
            {
                // 작업조, 작업자
                DataRow[] drShift = DtEquipment.Select("EQPTID = '" + EquipmentCode + "' And SEQ = 2");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                inTable.Columns.Add("LANE_QTY", typeof(decimal));
                inTable.Columns.Add("PROD_QTY", typeof(decimal));
                inTable.Columns.Add("SRS1QTY", typeof(decimal));
                inTable.Columns.Add("SRS2QTY", typeof(decimal));
                inTable.Columns.Add("SRS3QTY", typeof(decimal));
                inTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                for (int i = dgProductResult.TopRows.Count; i < (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count); i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOTID").ToString();
                    newRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                    newRow["SHIFT"] = string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()) ? null : drShift[0]["SHFT_ID"].ToString();
                    newRow["WIPNOTE"] = null;
                    newRow["WRK_USER_NAME"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["VAL002"].ToString();
                    newRow["WRK_USERID"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["WRK_USERID"].ToString();
                    newRow["PROD_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY"));

                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SaveDefect(dgWipReason, true);
                        _isChangeWipReason = false;

                        bProductionUpdate = true;

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
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

        #region **불량/LOSS/물품청구 조회

        /// <summary>
        /// 불량/LOSS/물품청구 조회
        /// </summary>
        private void SelectDefect()
        {
            try
            {
                string childLotId;

                // 소형/자동차 전극 사용을 체크박스로 분리 [2017-01-23] CR-55
                bool isElecProdType = chkSum.IsChecked == true ? true : false;

                Util.gridClear(dgWipReason);

                // SET LANE COMBO 설정
                cboLaneNum.Items.Clear();

                CheckBox allCheck = new CheckBox();
                allCheck.IsChecked = true;
                allCheck.Content = Util.NVC("-ALL-");
                allCheck.Tag = "ALL";
                allCheck.Checked += OnLaneChecked;
                allCheck.Unchecked += OnLaneChecked;
                cboLaneNum.Items.Add(allCheck);

                for (int i = dgProductResult.TopRows.Count; i < (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count); i++)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.IsChecked = true;
                    checkBox.Content = Util.NVC(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_NUM"));
                    checkBox.Tag = Util.NVC(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOTID"));
                    checkBox.Checked += OnLaneChecked;
                    checkBox.Unchecked += OnLaneChecked;
                    cboLaneNum.Items.Add(checkBox);
                }
                cboLaneNum.Text = ObjectDic.Instance.GetObjectName("Lane선택");

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = ProcessCode;
                Indata["EQPTID"] = EquipmentCode;
                IndataTable.Rows.Add(Indata);

                //C20210222-000365 적용으로 DA_PRD_SEL_DEFECT -> BR_PRD_SEL_DEFECT 변경
                new ClientProxy().ExecuteService("BR_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable totalWipReason = searchResult.DataSet.Tables["OUTDATA"];
                        DataTable dsWipReason = totalWipReason.DefaultView.ToTable(false, "RESNCODE", "ACTID", "ACTNAME", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "TAG_CONV_RATE");

                        Util.GridSetData(dgWipReason, dsWipReason, FrameOperation);

                        if (dgWipReason.Rows.Count > 0)
                        {
                            if (dgProductResult.GetRowCount() > 0)
                            {
                                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                                int iCount = _isResnCountUse == true ? 1 : 0;

                                // CONV_RATE기준으로 반영된 컬럼 삭제 [2017-02-15]
                                for (int i = dgWipReason.Columns.Count - 1; i > dgWipReason.Columns["TAG_CONV_RATE"].Index; i--)
                                    dgWipReason.Columns.RemoveAt(i);

                                if (dgProductResult.GetRowCount() > 0)
                                {
                                    string sMessageDic = ObjectDic.Instance.GetObjectName("태그수");
                                    string sMessageNum = ObjectDic.Instance.GetObjectName("횟수");
                                    double defectQty = 0;
                                    double lossQty = 0;
                                    double chargeQty = 0;

                                    for (int i = dgProductResult.TopRows.Count; i < (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count); i++)
                                    {
                                        childLotId = Util.NVC(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOTID"));

                                        // 소형 ( SUM, 태그수 X ), 자동차 ( ALL, 태그수 O ) (2017-01-23) CR-54
                                        if (i == dgProductResult.TopRows.Count)
                                        {
                                            if (isElecProdType)
                                                Util.SetGridColumnNumeric(dgWipReason, "ALL", null, "SUM", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                            else
                                                Util.SetGridColumnNumeric(dgWipReason, "ALL", null, "ALL", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                        }

                                        if (_isResnCountUse == true)
                                        {
                                            Util.SetGridColumnNumeric(dgWipReason, childLotId + "NUM", null, sMessageNum,
                                                true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);
                                        }

                                        Util.SetGridColumnNumeric(dgWipReason, childLotId + "CNT", null, sMessageDic,
                                            true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);

                                        // HALF SLITTING의 경우 8번째 자리 값으로 표시해달라는 요청으로 추가 [2017-05-08]
                                        Util.SetGridColumnNumeric(dgWipReason, childLotId, null, childLotId.Length > 8 ? childLotId.Substring(7, 1) : Util.NVC(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_NUM")),
                                            true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);

                                        DataGridAggregate.SetAggregateFunctions(dgWipReason.Columns[childLotId], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});

                                        if (dgWipReason.Rows.Count == 0)
                                            continue;

                                        DataTable dt = GetDefectDataByLot(Util.NVC(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOTID")));

                                        if (dt != null)
                                        {
                                            if (_isResnCountUse == true)
                                            {
                                                for (int j = 0; j < dt.Rows.Count; j++)
                                                {
                                                    if (dt.Rows[j]["COUNTQTY"].ToString().Equals(""))
                                                        BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 2, null);
                                                    else
                                                        BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 2, dt.Rows[j]["COUNTQTY"]);
                                                }
                                            }

                                            for (int j = 0; j < dt.Rows.Count; j++)
                                            {
                                                if (dt.Rows[j]["DFCT_TAG_QTY"].ToString().Equals(""))
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 1, 0);
                                                else
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 1, dt.Rows[j]["DFCT_TAG_QTY"]);
                                            }

                                            for (int j = 0; j < dt.Rows.Count; j++)
                                            {
                                                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count, 0);
                                                else
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                            }
                                        }

                                        defectQty = 0;
                                        lossQty = 0;
                                        chargeQty = 0;

                                        DataTable distinctDt = DataTableConverter.Convert(dgWipReason.ItemsSource).DefaultView.ToTable(true, "ACTID");
                                        foreach (DataRow _row in distinctDt.Rows)
                                        {
                                            if (string.Equals(_row["ACTID"], "DEFECT_LOT"))
                                                defectQty = SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "LOSS_LOT"))
                                                lossQty = SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "CHARGE_PROD_LOT"))
                                                chargeQty = SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                        }

                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_LOSS", lossQty);
                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY", (defectQty + lossQty + chargeQty));
                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY")) - Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY")));
                                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_QTY")));
                                    }
                                }

                                // COST CENTER 컬럼 맨 뒤로 오게하는 요청으로 해당 컬럼 제일 하위에서 생성 [2017-02-07]
                                // 전 공정 횟수 관리를 위하여 컬럼 추가 (C20190416_75868 ) [2019-04-17]
                                Util.SetGridColumnText(dgWipReason, "COSTCENTERID", null, "COSTCENTERID", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "COSTCENTER", null, "COSTCENTER", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "TAG_ALL_APPLY_FLAG", null, "TAG_ALL_APPLY_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);

                                if (_isResnCountUse == true)
                                {
                                    Util.SetGridColumnText(dgWipReason, "WRK_COUNT_MNGT_FLAG", null, "WRK_COUNT_MNGT_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                    Util.SetGridColumnText(dgWipReason, "LINK_DETL_RSN_CODE_TYPE", null, "LINK_DETL_RSN_CODE_TYPE", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                }

                                for (int i = 0; i < totalWipReason.Rows.Count; i++)
                                {
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["COSTCENTERID"].Index + 1, totalWipReason.Rows[i]["COSTCENTERID"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["COSTCENTER"].Index + 1, totalWipReason.Rows[i]["COSTCENTER"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["TAG_ALL_APPLY_FLAG"].Index + 1, totalWipReason.Rows[i]["TAG_ALL_APPLY_FLAG"]);

                                    if (_isResnCountUse == true)
                                    {
                                        BindingDataGrid(dgWipReason, i, dgWipReason.Columns["WRK_COUNT_MNGT_FLAG"].Index + 1, totalWipReason.Rows[i]["WRK_COUNT_MNGT_FLAG"]);
                                        BindingDataGrid(dgWipReason, i, dgWipReason.Columns["LINK_DETL_RSN_CODE_TYPE"].Index + 1, totalWipReason.Rows[i]["LINK_DETL_RSN_CODE_TYPE"]);
                                    }
                                }

                                // ROW별 합산 수량 반영 [2017-02-15]
                                double rowSum = 0;
                                for (int i = 0; i < dgWipReason.Rows.Count; i++)
                                {
                                    rowSum = 0;
                                    for (int j = dgWipReason.Columns["ALL"].Index + (2 + iCount); j < dgWipReason.Columns["COSTCENTERID"].Index; j += (2 + iCount))
                                        rowSum += Convert.ToDouble(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[j].Name));

                                    DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNTOTQTY", rowSum);
                                }
                                Util.GridSetData(dgWipReason, DataTableConverter.Convert(dgWipReason.ItemsSource), FrameOperation, true);
                                SetExceedLength();
                                dgWipReason.Refresh(false);
                                dgWipReason.FrozenColumnCount = dgWipReason.Columns["ALL"].Index + 1;

                                // S/L공정의 경우 TAG 불량/LOSS 자동 반영을 위하여 하기와 같이 이전 값을 DataTable로 보관(C20190404_67447)  [2019-04-11]
                                dtWipReasonBak = DataTableConverter.Convert(dgWipReason.ItemsSource);
                            }
                        }
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));
                IndataTable.Columns.Add("CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;

                if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                    Indata["CODE"] = "BAS";

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 불량/LOSS/물품청구 저장 
        /// </summary>
        public void SaveDefect(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                int iCount = _isResnCountUse == true ? 1 : 0;

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable InResn = inDataSet.Tables.Add("INRESN");
                InResn.Columns.Add("LOTID", typeof(string));
                InResn.Columns.Add("WIPSEQ", typeof(Int32));
                InResn.Columns.Add("ACTID", typeof(string));
                InResn.Columns.Add("RESNCODE", typeof(string));
                InResn.Columns.Add("RESNQTY", typeof(double));
                InResn.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                InResn.Columns.Add("COST_CNTR_ID", typeof(string));
                InResn.Columns.Add("WRK_COUNT", typeof(Int16));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable dtDefect = DataTableConverter.Convert(dg.ItemsSource);
                int iLotCount = 0;

                for (int iCol = dg.Columns["ALL"].Index + (2 + iCount); iCol < dg.Columns["COSTCENTERID"].Index; iCol += (2 + iCount))
                {
                    string sublotid = dg.Columns[iCol].Name;

                    foreach (DataRow _iRow in dtDefect.Rows)
                    {
                        newRow = InResn.NewRow();

                        newRow["LOTID"] = sublotid;
                        newRow["WIPSEQ"] = DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "WIPSEQ");
                        newRow["ACTID"] = _iRow["ACTID"];
                        newRow["RESNCODE"] = _iRow["RESNCODE"];
                        newRow["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                        newRow["DFCT_TAG_QTY"] = _iRow[sublotid + "CNT"].ToString().Equals("") ? 0 : _iRow[sublotid + "CNT"];
                        newRow["LANE_QTY"] = DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count + iLotCount].DataItem, "LANE_QTY");
                        newRow["LANE_PTN_QTY"] = DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count + iLotCount].DataItem, "LANE_PTN_QTY");
                        newRow["COST_CNTR_ID"] = _iRow["COSTCENTERID"];
                        if (_isResnCountUse == true)
                            newRow["WRK_COUNT"] = string.IsNullOrEmpty(_iRow[sublotid + "NUM"].ToString()) ? 0 : _iRow[sublotid + "NUM"];

                        InResn.Rows.Add(newRow);
                    }
                    iLotCount++;
                }

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                    dg.EndEdit(true);

                    _isChangeWipReason = false;
                }
                catch (Exception ex) { Util.MessageException(ex); }

                if (!bAllSave)
                    Util.MessageInfo("SFU3532");     // 저장 되었습니다

                //bProductionUpdate = true;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region **품질정보
        /// <summary>
        /// 품질정보 조회
        /// </summary>
        private void SelectQuality()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                inTable.Columns.Add("VER_CODE", typeof(string));
                inTable.Columns.Add("LANEQTY", typeof(Int16));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                if (!string.IsNullOrWhiteSpace(txtVersion.Text))
                    newRow["VER_CODE"] = txtVersion.Text;
                if (txtLaneQty.Value != double.NaN && txtLaneQty.Value > 0)
                    newRow["LANEQTY"] = txtLaneQty.Value;

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            bizResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", inTable);
                        }

                        Util.GridSetData(dgQuality, bizResult, null, true);

                        _util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                        _util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "MEAN" }, DataGridMergeMode.VERTICALHIERARCHI);
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
        /// 품질정보 저장 
        /// </summary>
        private void SaveQuality(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET     SetWCLCTSeq
                DataTable dtQuality = DataTableConverter.Convert(dg.ItemsSource);

                foreach (DataRow row in dtQuality.Rows)
                {
                    DataRow newRow = inTable.NewRow();

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = DvProductLot["LOTID"];
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = row["CLCTITEM"];

                    decimal tmp;
                    if (Decimal.TryParse(row["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                        newRow["CLCTVAL01"] = Util.NVC(row["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(row["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    else
                        newRow["CLCTVAL01"] = Util.NVC(row["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(row["CLCTVAL01"]).Trim().ToString();

                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"];
                    newRow["CLCTSEQ"] = 1;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isChangeQuality = false;

                        if (!bAllSave)
                            Util.MessageInfo("SFU1998");     // 품질 정보가 저장되었습니다.
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

        #region **합권취
        /// <summary>
        /// 합권취 대상 Lot 조회
        /// </summary>
        private void SelectMergeFrom()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["PRODID"] = Util.NVC(DvProductLot["PRODID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_LOT_LIST_V01", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWipMerge, bizResult, null, true);
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
        /// 합권취 Lot 조회
        /// </summary>
        private void SelectMergeTo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = txtMergeInputLot.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_LOT_END_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWipMerge2, bizResult, null, true);
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
        /// 투입 Lot 속성 조회
        /// </summary>
        private DataTable SelectMergeLotAttr()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PR_LOTID", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = Util.NVC(txtMergeInputLot.Text);
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", inTable);
        }

        /// <summary>
        /// 합권취 저장 
        /// </summary>
        private void SaveMerge()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable InFromLot = inDataSet.Tables.Add("IN_FROMLOT");
                InFromLot.Columns.Add("FROM_LOTID", typeof(string));
                ////////////////////////////////////////////////////////////////////

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LOTID"] = Util.NVC(txtMergeInputLot.Text); 
                newRow["NOTE"] = string.Empty;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = Util.gridGetChecked(ref dgWipMerge, "CHK");
                newRow = null;

                foreach (DataRow row in dr)
                {
                    newRow = InFromLot.NewRow();
                    newRow["FROM_LOTID"] = Util.NVC(row["LOTID"]);
                    InFromLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_LOT", "IINDATA,IN_FROMLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        bProductionUpdate = true;

                        Util.MessageInfo("SFU2009");     // 합권되었습니다.

                        btnSearchMerge_Click(null, null);
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
        #endregion

        #region **특이사항
        /// <summary>
        /// 특이사항 조회
        /// </summary>
        private string GetRemarkData(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = sLotID;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dtResult.Rows.Count > 0)
            {
                return Util.NVC(dtResult.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 특이사항 저장 
        /// </summary>
        private void SaveWipNote(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                // 0 Row는 공통특이사항
                for (int row = 1; row < dt.Rows.Count; row++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(dt.Rows[row]["LOTID"]);

                    if (dg.Rows[0].Visibility == Visibility.Visible)
                        newRow["WIP_NOTE"] = Util.NVC(dt.Rows[row]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                    else
                        newRow["WIP_NOTE"] = Util.NVC(dt.Rows[row]["REMARK"]);

                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isChangeRemark = false;

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
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

        #region **이전특이사항
        /// <summary>
        /// 이전특이사항 조회
        /// </summary>
        private void SelectRemarkPrevious()
        {
            try
            {
                String sLotID = String.Empty;
                if (string.IsNullOrWhiteSpace(DvProductLot["LOTID_PR"].ToString()))
                    sLotID = DvProductLot["LOTID"].ToString();
                else
                    sLotID = DvProductLot["LOTID_PR"].ToString();


                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = sLotID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_HISTORY_WIPNOTE", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 필요정보 변환
                        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                        foreach (DataRow row in bizResult.Rows)
                        {
                            strBuilder.Clear();
                            string[] wipNotes = Util.NVC(row["WIPNOTE"]).Split('|');

                            for (int i = 0; i < wipNotes.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(wipNotes[i]))
                                {
                                    if (i == 0)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                                    else if (i == 1)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                                    else if (i == 2)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                                    else if (i == 3)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                                    else if (i == 4)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                                    else if (i == 5)
                                        strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                                    strBuilder.Append("\n");
                                }
                            }
                            row["WIPNOTE"] = strBuilder.ToString();
                        }
                        Util.GridSetData(dgRemarkHistory, bizResult, FrameOperation, true);
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

        #endregion

        #region [Func]

        /// <summary>
        /// 단위에 따른 숫자 포멧
        /// </summary>
        private void SetUnitFormatted()
        {
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                string sFormatted = string.Empty;
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }

                txtInputQty.Format = sFormatted;
                txtParentQty.Format = sFormatted;
                txtRemainQty.Format = sFormatted;

                if (string.Equals(txtUnit.Text, "EA"))
                    txtMesualQty.Format = "F1";
                else
                    txtMesualQty.Format = sFormatted;

                for (int i = 0; i < dgProductResult.Columns.Count; i++)
                    if (dgProductResult.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgProductResult.Columns[i].Tag, "N"))
                        ((DataGridNumericColumn)dgProductResult.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgWipReason.Columns.Count; i++)
                    if (dgWipReason.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReason.Columns[i].Tag, "N"))
                        ((DataGridNumericColumn)dgWipReason.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgQuality.Columns.Count; i++)
                    if (dgQuality.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQuality.Columns[i].Tag, "N"))
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgQuality.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgQuality.Columns[i]).Format = sFormatted;
            }
        }

        private string GetUnitFormatted()
        {
            string sFormatted = "0";
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        public string GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (txtUnit.Text)
            {
                case "KG":
                    sFormatted = "{0:#,##0.000}";
                    break;

                case "M":
                    sFormatted = "{0:#,##0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:#,##0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private string GetUnitFormatted(object obj, string pattern)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (pattern)
            {
                case "KG":
                    sFormatted = "{0:###0.000}";
                    break;

                case "M":
                    sFormatted = "{0:###0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:###0.0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        /// 특이사항
        /// </summary>
        private void BindingWipNote(DataTable dt)
        {
            if (dgRemark.GetRowCount() > 0) return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

            if (dt.Rows.Count > 0)
            {
                string[] sWipNote = GetRemarkData(Util.NVC(dt.Rows[0]["LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    inDataRow["REMARK"] = sWipNote[1];
            }
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                inDataRow["REMARK"] = GetRemarkData(Util.NVC(_row["LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(dgRemark, dtRemark, FrameOperation);
        }

        private void SetProductionResult()
        {
            if (DvProductLot["WIPSTAT"].ToString() == "WAIT") return;

            // 버전, Lane수
            DataTable dtVersion = new DataTable();
            string sVersion = string.Empty;
            string sLaneQty = string.Empty;

            dtVersion = GetProcessVersion();

            if (dtVersion != null && dtVersion.Rows.Count > 0)
            {
                sVersion = Util.NVC(dtVersion.Rows[0]["PROD_VER_CODE"]);
                sLaneQty = string.IsNullOrWhiteSpace(Util.NVC(dtVersion.Rows[0]["LANE_QTY"])) ? "0" : Util.NVC(dtVersion.Rows[0]["LANE_QTY"]);
            }

            txtVersion.Text = sVersion;
            txtLaneQty.Value = string.IsNullOrEmpty(sLaneQty) ? 0 : Convert.ToDouble(sLaneQty);
            txtLanePatternQty.Value = 1;               // 패턴 사용을 안하지만 다른곳에서 패턴을 사용할 것을 대비하여 1로 SET

            txtStartDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ST"]).ToString("yyyy-MM-dd HH:mm");

            if (string.IsNullOrWhiteSpace(DvProductLot["WIPDTTM_ED"].ToString()))
                txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            else
                txtEndDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ED"]).ToString("yyyy-MM-dd HH:mm");

            // 작업일
            if (txtWorkDate != null)
                SetCalDate(txtWorkDate);

            txtWipstat.Text = DvProductLot["WIPSTAT_NAME"].ToString();
            txtUnit.Text = DvProductLot["UNIT_CODE"].ToString();
            // 합권취용 투입 LOT SET
            txtMergeInputLot.Text = DvProductLot["LOTID_PR"].ToString();
            //txtParentQty.Value = Convert.ToDouble(DvProductLot["INPUTQTY"]);

            // 청주 소형전극에서만 패턴에서만 변환해서 값 입력 [COATER에서만 사용]
            //if (string.Equals(txtUnit.Text, "EA") && string.Equals(LoginInfo.CFG_AREA_ID, "E1") && string.Equals(procId, Process.COATING))
            if (string.Equals(txtUnit.Text, "EA"))
                _convRate = GetPatternLength(Util.NVC(DvProductLot["PRODID"]));

            //if (string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
            //{
            //    btnSaceAllWipReason.IsEnabled = true;
            //    btnSaveWipReason.IsEnabled = true;
            //}
            //else
            //{
            //    btnSaceAllWipReason.IsEnabled = false;
            //    btnSaveWipReason.IsEnabled = false;
            //}

            SetResultInfo();

            // UNIT별로 FORMAT
            SetUnitFormatted();
        }

        private void DefectChange(C1DataGrid dg, int iRow, int iCol)
        {
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = _isResnCountUse == true ? 1 : 0;

            if (iCol == dg.Columns["ALL"].Index)
            {
                // 소형 전극은 SUM으로 분배 [2017-01-24]
                int iLaneQty = 0;

                for (int i = 1; i < cboLaneNum.Items.Count; i++)
                    if (((CheckBox)cboLaneNum.Items[i]).IsChecked == true)
                        iLaneQty++;

                if (iLaneQty == 0)
                    return;

                decimal iTarget = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                decimal iLastCol = iTarget % iLaneQty;

                for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i++)
                {
                    if ((i + iCol) % (2 + iCount) == 0)
                    {
                        if (dg.Columns[i].Visibility == Visibility.Collapsed)
                            continue;

                        string _ValueToFind = string.Empty;

                        // 길이초과, 길이부족의 경우는 SUM도 분배가 아닌 일괄 등록
                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK") ||
                            string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                        {
                            _ValueToFind = Util.NVC(iTarget);
                        }
                        else
                        {
                            if (string.Equals(dg.Columns["ALL"].GetColumnText(), ObjectDic.Instance.GetObjectName("SUM")))
                                _ValueToFind = Util.NVC(Math.Truncate(iTarget / iLaneQty) + (iLastCol > 1 ? 1 : iLastCol));
                            else
                                _ValueToFind = Util.NVC(iTarget);
                        }

                        DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name, _ValueToFind);

                        bool isDefectValid = true;
                        isDefectValid = GetSumDefectQty(dg, iRow, i);

                        if (isDefectValid == false)
                            continue;

                        iLastCol = iLastCol > 1 ? iLastCol - 1 : 0;
                    }
                }
            }
            else if (iCol >= dg.Columns["ALL"].Index + 1 && (iCol - (dg.Columns["ALL"].Index - 1)) % (2 + iCount) == 1)
            {
                GetSumDefectQty(dg, iRow, iCol);
            }
        }

        private bool GetSumDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            string subLotId = dg.Columns[colIdx].Name;
            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));
            double inputQty = 0;
            double actSum = 0;
            double rowSum = 0;
            double totalSum = 0;
            int iLotIdx = Util.gridFindDataRow(ref dgProductResult, "LOTID", subLotId, false);

            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = _isResnCountUse == true ? 1 : 0;

            inputQty = Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[iLotIdx].DataItem, "INPUTQTY"));

            for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));

            actSum = SumDefectQty(dg, colIdx, subLotId, actId);

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[iLotIdx].DataItem, "DTL_DEFECT"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[iLotIdx].DataItem, "DTL_LOSS"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[iLotIdx].DataItem, "DTL_CHARGEPRD"));

            /*
            if (inputQty < totalSum)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1608"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //생산량 보다 불량이 크게 입력될 수 없습니다.
                DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, dg.Columns[colIdx].Name, null);
                return false;
            }
            */

            DataTableConverter.SetValue(dgProductResult.Rows[iLotIdx].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DTL_DEFECT" : string.Equals(actId, "LOSS_LOT") ? "DTL_LOSS" : "DTL_CHARGEPRD", actSum);
            DataTableConverter.SetValue(dgProductResult.Rows[iLotIdx].DataItem, "LOSSQTY", totalSum);
            DataTableConverter.SetValue(dgProductResult.Rows[iLotIdx].DataItem, "GOODQTY", inputQty - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            return true;
        }

        private void SetParentRemainQty()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty - _exceedLengthQty));
            txtInputQty.Value = 0;
        }

        private void SetExceedLength()
        {
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = _isResnCountUse == true ? 1 : 0;

            for (int i = 0; i < dgWipReason.Rows.Count; i++)
            {
                if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), _itemCodeLenExceed))
                {
                    if ((dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) == dgProductResult.TopRows.Count + 1)
                        _exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNQTY"));
                    else
                        _exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[dgWipReason.Columns["ALL"].Index + (2 + iCount)].Name));
                    break;
                }
            }

            if (_exceedLengthQty >= 0)
            {
                decimal inputQty = Util.NVC_Decimal(txtParentQty.Value);
                decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

                if (prodQty > 0)
                    txtRemainQty.Value = Convert.ToDouble(inputQty - (prodQty - Util.NVC_Decimal(_exceedLengthQty)));
            }
        }

        public bool SetLossLot(C1DataGrid dg, string sItemCode, decimal iLossQty)
        {
            bool isLossValid = false;
            DataTable dt = (dg.ItemsSource as DataView).Table;
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = _isResnCountUse == true ? 1 : 0;

            for (int iCol = dg.Columns["ALL"].Index + (1 + iCount); iCol < dg.Columns["COSTCENTERID"].Index; iCol += (2 + iCount))
            {
                string sublotid = dg.Columns[iCol + 1].Name;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, sublotid, iLossQty);
                        DefectChange(dg, i, iCol + 1);
                        isLossValid = true;
                        break;
                    }
                }
            }

            if (isLossValid == false)
                Util.MessageValidation("SFU3196", new object[] { string.Equals(sItemCode, _itemCodeLenLack) ?
                    ObjectDic.Instance.GetObjectName("길이부족") : ObjectDic.Instance.GetObjectName("길이초과") }); //해당 MMD에 {%1}에 관련된 속성이 지정되지 않아 자동Loss를 등록할 수 없습니다.

            return isLossValid;
        }

        private void SetInputQty()
        {
            decimal inputQty = Util.NVC_Decimal(txtInputQty.Value);
            decimal lossQty = 0;
            int laneqty = 0;

            for (int i = 0 + dgProductResult.TopRows.Count; i < (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count); i++)
            {
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY"));
                laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_QTY"));

                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY", inputQty);
                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);

                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * laneqty);
            }

            SetParentRemainQty();
        }

        private void SetVisibilityWipReasonGrid(string sLotID, bool? isVisibility)
        {
            for (int i = dgWipReason.Columns["TAG_CONV_RATE"].Index; i < dgWipReason.Columns.Count; i++)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                if (string.Equals(sLotID, dgWipReason.Columns[i].Name) ||
                    (chkSum.IsChecked == false && (string.Equals(sLotID + "NUM", dgWipReason.Columns[i].Name) || string.Equals(sLotID + "CNT", dgWipReason.Columns[i].Name))))
                    dgWipReason.Columns[i].Visibility = isVisibility == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BindingDataGrid(C1DataGrid dg, int iRow, int iCol, object sValue)
        {
            try
            {
                if (dg.ItemsSource == null)
                {
                    return;
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                    if (dt.Columns.Count < dg.Columns.Count)
                        for (int i = dt.Columns.Count; i < dg.Columns.Count; i++)
                            dt.Columns.Add(dg.Columns[i].Name);

                    if (sValue.Equals("") || sValue.Equals(null))
                        sValue = 0;

                    dt.Rows[iRow][iCol - 1] = sValue;

                    dg.BeginEdit();
                    Util.GridSetData(dg, dt, FrameOperation, false);
                    dg.EndEdit();
                }
            }
            catch { }
        }

        private double SumDefectQty(C1DataGrid dg, int index, string lotId, string actId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)) * 1);

            return sum;
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

        #endregion;

        #region[[Validation]

        private bool ValidationProductionUpdate()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            return true;
        }

        private bool ValidateDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return false;
            }

            //슬리터 공정진척 길이초과 값이 존재하면 길이부족 입력 불가 처리
            if (string.Equals(DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
            {
                int iCount = _isResnCountUse == true ? 1 : 0;
                decimal iLackQty = 0;

                iLackQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "ALL"));

                if (iLackQty > 0)
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                        {
                            if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RESNTOTQTY")) > 0)
                            {
                                Util.MessageValidation("SFU8315");  // 길이초과와 길이부족을 동시 입력 불가
                                DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, "ALL", 0);

                                return true;
                            }
                        }
                    }
                }
            }

            if (string.Equals(DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                int iCount = _isResnCountUse == true ? 1 : 0;

                decimal inputQty = 0;
                decimal inputLengthQty = 0;

                inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "ALL"));

                if (inputLengthQty > 0)
                {
                    inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

                    if (Util.NVC_Decimal(txtParentQty.Value) > inputQty)
                    {
                        Util.MessageValidation("SFU3424");     // FINAL CUT이 아닌 경우 길이초과 입력 불가
                        DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, dg.CurrentCell.Column.Name, null);

                        for (int i = dgWipReason.Columns["ALL"].Index + (2 + iCount); i < dgWipReason.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                            DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, dg.Columns[i].Name, 0);

                        _exceedLengthQty = 0;
                        return false;
                    }

                    if (inputLengthQty > (inputQty - Util.NVC_Decimal(txtParentQty.Value)))
                    {
                        Util.MessageValidation("SFU3422", (inputQty - Util.NVC_Decimal(txtParentQty.Value)) + txtUnit.Text);     // 길이초과수량을 초과하였습니다.[현재 실적에서 길이초과는 %1까지 입력 가능합니다.] 
                        DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, dg.CurrentCell.Column.Name, null);

                        for (int i = dgWipReason.Columns["ALL"].Index + (2 + iCount); i < dgWipReason.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                            DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, dg.Columns[i].Name, 0);

                        _exceedLengthQty = 0;
                        return false;
                    }
                }

                //길이부족 입력 후 길이초과 입력되면 길이부족 초기화
                for (int k = 0; k < dg.Rows.Count; k++)
                {
                    if (string.Equals(DataTableConverter.GetValue(dg.Rows[k].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[k].DataItem, "RESNTOTQTY")) > 0)
                        {
                            if (SetLossLot(dg, _itemCodeLenLack, 0) == false)
                                return true;
                            DataTableConverter.SetValue(dg.Rows[k].DataItem, "ALL", 0);
                        }
                    }
                }

            }

            return true;
        }

        private bool ValidationDefect()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            if (dgWipReason.GetRowCount() <= 0)
            {
                Util.MessageInfo("SFU3552");    // 저장 할 DATA가 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationQuality(bool bAllSave = false)
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            if (bAllSave == false && dgQuality.GetRowCount() <= 0)
            {
                Util.MessageInfo("SFU3552");    // 저장 할 DATA가 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationRemark(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3552");     // 저장 할 DATA가 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationMerge()
        {
            //// 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            //if (CheckConfirmLot() == false) return false;

            if (DvProductLot["WIPSTAT"].ToString() != Wip_State.PROC)
            {
                Util.MessageValidation("SFU3627");     // 합권취는 진행 상태에서만 가능합니다.
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMergeInputLot.Text))
            {
                Util.MessageValidation("SFU1945");     // 투입 LOT이 없습니다.
                return false;
            }

            DataRow[] drMerge = DataTableConverter.Convert(dgWipMerge.ItemsSource).Select("CHK = 1");

            if (drMerge.Length == 0)
            {
                Util.MessageValidation("SFU3628");    // 합권취 진행할 대상 Lot들이 선택되지 않았습니다.
                return false;
            }

            DataTable dtLotAttr = SelectMergeLotAttr();
            //DataTable dtWipAttr = SelectMergeWipAttr();

            foreach (DataRow row in drMerge)
            {
                if (dtLotAttr.Rows[0]["LANE_QTY"].ToString() != row["LANE_QTY"].ToString())
                {
                    Util.MessageInfo("SFU5081");     // LANE수가 다릅니다.
                    return false;
                }
                if (dtLotAttr.Rows[0]["MKT_TYPE_CODE"].ToString() != row["MKT_TYPE_CODE"].ToString())
                {
                    Util.MessageInfo("SFU4271");     // 동일한 시장유형이 아닙니다.
                    return false;
                }
                if (!string.IsNullOrEmpty(row["WH_ID"].ToString()))
                {
                    Util.MessageInfo("SFU2963");     // 창고에서 출고되지 않았습니다.
                    return false;
                }
                //if (!string.IsNullOrWhiteSpace(row["ABNORM_FLAG"].ToString()) && row["ABNORM_FLAG"].ToString().Equals("Y"))
                //{
                //    Util.MessageInfo("SFU7029");     // 전수불량레인이 존재하여 합권취 불가합니다.
                //    return false;
                //}

                //if (!string.IsNullOrWhiteSpace(dtWipAttr.Rows[0]["ABNORM_FLAG"].ToString()) && dtWipAttr.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                //{
                //    Util.MessageValidation("SFU7029");     // 전수불량레인이 존재하여 합권취 불가합니다.
                //    return false;
                //}
            }

            return true;
        }


        #endregion

        #region [팝업]
        #endregion

        #endregion

    }


}
