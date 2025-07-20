/*************************************************************************************
 Created Date : 2020.09.14
      Creator : 정문교
   Decription : 전극 공정진척 - 생산실적 (E1000 : Mixing)
                                          E0500 : 선분산 Mixing
                                          E0400 : Binder Solution      
                                          E0410 : CMC 공정진척
--------------------------------------------------------------------------------------
 [Change History]
 2023-07-19     김태우     DAM MIXING 추가
 2024.05.22     백상우 : [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부 
 2024.08.10     배현우 : [E20240807-000861] Dam Mixer 공정이  WO 공정으로 변경됨에따라 비즈 분기 변경
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

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeProductionResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeProductionResult_Mixing : UserControl
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

        public bool bProductionUpdate { get; set; }

        public bool bChangeQuality
        {
            get { return _isChangeQuality; }
        }
        public bool bChangeMaterial
        {
            get { return _isChangeMaterial; }
        }
        public bool bChangeRemark
        {
            get { return _isChangeRemark; }
        }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        bool _isResnCountUse = false;
        bool _isDupplicatePopup = false;
        bool _isSoldContAutoCalAreaProc = false;

        // DataCollect 변경 여부
        bool _isChangeWipReason = false;                      // 불량/LOSS/물품청구
        bool _isChangeQuality = false;                        // 품질정보
        bool _isChangeMaterial = false;                       // 투입자재
        bool _isChangeRemark = false;                         // 특이사항 

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));

        public UcElectrodeProductionResult_Mixing()
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
        }

        private void SetControlClear()
        {
            txtLotID.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtWorkTime.Value = 0;
            txtWorkDate.Text = string.Empty;
            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtUnit.Text = string.Empty;
            txtLaneQty.Value = 0;
            txtBeadMillCount.Value = 0;

            txtEquipmentQty.Value = 0;
            txtProductionQty.Value = 0;
            txtGoodQty.Value = 0;

            _isResnCountUse = false;
            _isDupplicatePopup = false;

            // DataCollect 변경 여부
            _isChangeWipReason = false;                      // 불량/LOSS/물품청구
            _isChangeQuality = false;                        // 품질정보
            _isChangeMaterial = false;                       // 투입자재
            _isChangeRemark = false;                         // 특이사항

            bProductionUpdate = false;                       // 실적 저장, 불량/LOSS/물품청구 저장시 True

            Util.gridClear(dgWipReason);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgQuality);
            Util.gridClear(dgRemark);
        }

        private void SetControlVisibility()
        {
            tbWorkTime.Visibility = Visibility.Collapsed;            // 가동시간
            txtWorkTime.Visibility = Visibility.Collapsed;           // 가동시간
            tbBeadMillCount.Visibility = Visibility.Collapsed;       // BeadMill횟수
            txtBeadMillCount.Visibility = Visibility.Collapsed;      // BeadMill횟수
            tbLaneQty.Visibility = Visibility.Collapsed;             // 실물Lane수
            txtLaneQty.Visibility = Visibility.Collapsed;            // 실물Lane수

            grdWipReasonHeader.Visibility = Visibility.Collapsed;    // 불량/LOSS/물품청구 탭 : 수동, 간이세정, Full세정
            btnEqptMaterial.Visibility = Visibility.Collapsed;       // 투입자재 탭 : 설비IF현황

            if (ProcessCode == Process.MIXING)
            {
                tbWorkTime.Visibility = Visibility.Visible;
                txtWorkTime.Visibility = Visibility.Visible;

                grdWipReasonHeader.Visibility = Visibility.Visible;
                btnEqptMaterial.Visibility = Visibility.Visible;
            }
            else if (ProcessCode == Process.PRE_MIXING)
            {
                tbBeadMillCount.Visibility = Visibility.Visible;
                txtBeadMillCount.Visibility = Visibility.Visible;
            }

            if (Util.NVC(DvProductLot["WIPSTAT"]) == Wip_State.WAIT)
                txtProductionQty.IsEnabled = false;
            else
                txtProductionQty.IsEnabled = true;

            _isSoldContAutoCalAreaProc = IsAreaCommonCodeUse("SOLID_CONT_AUTO_CALC_PROC", ProcessCode);
            if (_isSoldContAutoCalAreaProc)
                btnSolidContRate.Visibility = Visibility.Visible;
            else
                btnSolidContRate.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// 버전 팝업 
        /// </summary>
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            PopupVersion();
        }

        /// <summary>
        /// 생산수량
        /// </summary>
        private void txtProductionQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SumDefectQty();
            }
        }

        private void txtProductionQty_LostFocus(object sender, RoutedEventArgs e)
        {
            C1NumericBox cnb = sender as C1NumericBox;

            if (cnb.Value > 0)
                txtProductionQty_KeyDown(cnb, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));

        }

        /// <summary>
        /// 저장
        /// </summary>
        private void btnProductionUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductionUpdate()) return;

            SaveProductionUpdate();
        }

        #region **불량/LOSS/물품청구
        /// <summary>
        /// 세정, 간이세정, Full세정 RadioButton
        /// </summary>
        private void rbManual_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            string sCode = string.Empty;

            if (rb.Name == "rbSimpClean")
                sCode = "SIMP";
            else if (rb.Name == "rbFullClean")
                sCode = "FULL";
            else
                sCode = "BAS";

            SelectDefect(null, sCode);
        }

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        //if (e.Cell.Row.Type == DataGridRowType.Item)
                        //{
                        //    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                        //    {
                        //        if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                        //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        //        if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                        //                (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                        //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        //        if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                        //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        //        // 길이부족 차감 색상표시 추가 [2019-12-09]
                        //        if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                        //                (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRCS_ITEM_CODE"), "PROD_QTY_INCR")))
                        //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D6606D"));
                        //    }
                        //}
                    }
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

            if (dataGrid == null) return;

        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            SumDefectQty();

            _isChangeWipReason = true;
        }

        private void dgWipReason_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Presenter == null) return;


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

        /// <summary>
        /// 전체 저장
        /// </summary>
        private void btnSaceAllWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefect()) return;

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

            // 불량/LOSS/물품청구
            SaveDefect(dgWipReason);
        }
        #endregion

        #region **품질정보
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

        private bool IsElecPermissionGrpArea(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = "Y";
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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


                    //if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString())) // 2024.10.11. 김영국  - 0값이 입력된 경우에도 SPEC CHECK를 하도록 수정함.
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

            if (dg == null) return;

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
                        else if (string.Equals(e.Cell.Column.Name, "CLSS_NAME1"))
                        {
                            // 필수 검사 항목 여부 색상 표시
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MAND_INSP_ITEM_FLAG")) == "Y")
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            else
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);

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
                            string isEnable = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ISENABLE"));

                            if (panel != null)
                            {
                                if (string.Equals(sCode, "NUM"))
                                {
                                    C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                    // 재설정
                                    numeric.Format = GetUnitFormatted(txtUnit.Text);

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

                                    if (isEnable.Equals("False"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#EBEBEB"));
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
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    //if (e.Cell.Row.Type == DataGridRowType.Bottom)
                    //{
                    //    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    //    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    //    if (e.Cell.Column.Index == dg.Columns["CLSS_NAME1"].Index)
                    //    {
                    //        if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                    //            presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                    //    }
                    //    else if (e.Cell.Column.Index == dg.Columns["CLCTVAL01"].Index) // 측정값
                    //    {
                    //        if (presenter.HorizontalAlignment != HorizontalAlignment.Right)
                    //            presenter.HorizontalAlignment = HorizontalAlignment.Right;

                    //        decimal sumValue = 0;
                    //        if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                    //            if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非数字"))
                    //            {
                    //                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    //                    if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                    //                        sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));

                    //                if (sumValue == 0)
                    //                    presenter.Content = 0;
                    //                else
                    //                    presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dg.Rows.Count - dg.BottomRows.Count), "EA"));
                    //            }
                    //    }
                    //}
                }
            }));

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

            if (dg == null) return;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            {
                sValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTVAL02"));
                //sCLCITEM = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-').Count() == 3)
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0] + "-" +
                        Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[1];
                }
                else
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                }
                sCLCNAME = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
                //Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTNAME")).ToString().Replace("Back", "");
            }
            else
            {
                sValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "CLCTVAL01"));
            }
            string sCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        //Util.MessageValidation("SFU1806");     // 입력값이 하한값 보다 작습니다
                        //DataTableConverter.SetValue(dgQualityTop.Rows[dgQualityTop.CurrentRow.Index].DataItem, "CLCTVAL01", null);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                    {
                        //Util.MessageValidation("SFU1805");     // 입력값이 상한값 보다 큽니다
                        //DataTableConverter.SetValue(dgQualityTop.Rows[dgQualityTop.CurrentRow.Index].DataItem, "CLCTVAL01", null);
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
                }
            }
            else if (e.Cell.Presenter != null)
            {
            }
        }

        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Presenter == null) return;

            // CLCTVAL02 ACTION 재 정의 [2019-03-27]
            if (string.Equals(dg.CurrentCell.Column.Name, "CLCTVAL02"))
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;

                    int iRowIdx = dg.CurrentCell.Row.Index;
                    if ((dg.CurrentCell.Row.Index + 1) < dg.GetRowCount())
                        iRowIdx++;

                    C1.WPF.DataGrid.DataGridCell currentCell = dg.GetCell(iRowIdx, dg.CurrentCell.Column.Index);
                    Util.SetDataGridCurrentCell(dg, currentCell);
                    dg.CurrentCell = currentCell;
                    dg.Focus();
                }
                else if (e.Key == Key.Delete)
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false)
                    {
                        // 이동중 DEL키 입력 시는 측정값 초기화하도록 변경 [2019-04-22]
                        if (dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter != null && dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter.Content != null &&
                            dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Value != null)
                        {
                            ((C1NumericBox)dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter.Content).Value = 0;
                        }
                        else
                        {
                            DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, "CLCTVAL01", null);
                        }
                    }
                }
            }
            else
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

        #region **투입자재
        private void btnEqptMaterial_Click(object sender, RoutedEventArgs e)
        {
            PopupEqptMaterial();
        }

        private void btnInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            PopupInputMaterial();
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

        #endregion

        #region Mehod

        #region [외부호출]
        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnProductionUpdate);                // 저장
            listAuth.Add(btnSaceAllWipReason);                // 불량/LOSS/물품청구 : 전체저장
            listAuth.Add(btnSaveWipReason);                   // 불량/LOSS/물품청구 : 저장
            listAuth.Add(btnEqptMaterial);                    // 투입자재 : 설비IF현황
            listAuth.Add(btnInputMaterial);                   // 투입자재 : 투입자재등록
            listAuth.Add(btnSaveQuality);                     // 품질정보 : 저장
            listAuth.Add(btnSaveRemark);                      // 특이사항 : 저장

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
            // 투입자재
            if (ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing)
                SelectInputMaterialListBS();
            else
                SelectInputMaterialList();

            // 품질정보
            SelectQuality(dgQuality);
            // 특이사항
            SelectRemark(dgRemark);

            //this.Cursor = Cursors.Arrow;
        }

        #endregion

        #region [BizCall]

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

        /// <summary>
        /// 버전, Lane수 
        /// </summary>
        private DataTable SetProductVersion()
        {
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

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = Util.NVC(DvProductLot["EQPTID"]);
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["MODLID"] = Util.NVC(DvProductLot["PRODID"]);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 버전 Mixing
        /// </summary>
        private string SetProductVersionMixing()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PRODID"] = DvProductLot["PRODID"];
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["PROD_VER_CODE"].ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 생산 수량
        /// </summary>
        private void SetSaveProductQty()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_SEL_WIPATTR_FOR_PROD_QTY_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dt = bizResult.Clone();
                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            if (Util.NVC_Decimal(bizResult.Rows[0]["PROD_QTY"]) > 0)
                                txtProductionQty.Value = Convert.ToDouble(bizResult.Rows[0]["PROD_QTY"]);

                            // 설비수량
                            txtEquipmentQty.Value = Convert.ToDouble(bizResult.Rows[0]["EQPT_END_QTY"].ToString());

                            // 생산수량 : 해당 설비 완공 시점에서는 설비완공 시점에서 투입량을 수량으로 변경한다 [2017-02-14]
                            //if (string.Equals(DvProductLot["WIPSTAT"].ToString(), Wip_State.EQPT_END))
                            //  txtProductionQty.Value = Convert.ToDouble(DvProductLot["EQPT_END_QTY"].ToString());
                            
                            if (string.Equals(DvProductLot["WIPSTAT"].ToString(), Wip_State.EQPT_END))
                            {
                                // [E20241014-000213] NJ电极Mixer设备计量值上传MES投入履历 NJ 전극 Mixer 설비 계량치 MES 투입 이력 업로드
                                string sComTypeCode = "PERMISSIONS_PER_PROC_PROD_QTY_DRB";   // 전극 공정진척 생산수량 권한
                                string[] sAttrbute = { null, ProcessCode, "PROD_QTY_W" };
                                if (IsElecPermissionGrpArea(sComTypeCode, "", sAttrbute))    // 공정 생산수량 권한체크 적용여부
                                {
                                    if (txtProductionQty.Value == 0)
                                    {
                                        txtProductionQty.Value = Convert.ToDouble(DvProductLot["EQPT_END_QTY"].ToString());
                                    }
                                }
                                else
                                {
                                    txtProductionQty.Value = Convert.ToDouble(DvProductLot["EQPT_END_QTY"].ToString());
                                }
                            }
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
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                newRow["SHIFT"] = string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()) ? null : drShift[0]["SHFT_ID"].ToString();
                newRow["WIPNOTE"] = null;
                newRow["WRK_USER_NAME"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["VAL002"].ToString();
                newRow["WRK_USERID"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["WRK_USERID"].ToString();
                newRow["PROD_QTY"] = txtProductionQty.Value;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

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
        private void SelectDefect(string Resnposition = null, string sCode = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("RESNPOSITION", typeof(string));          // TOP/BACK
                inTable.Columns.Add("CODE", typeof(string));                  // MIX 세정 Option

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);

                if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                    newRow["CODE"] = "BAS";

                if (!string.IsNullOrWhiteSpace(sCode))
                {
                    newRow["CODE"] = sCode;
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWipReason, bizResult, null, true);

                        SumDefectQty();
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
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                if (_isResnCountUse == true)
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

                foreach (DataRow row in dtDefect.Rows)
                {
                    newRow = InResn.NewRow();
                    newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                    newRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                    newRow["ACTID"] = row["ACTID"];
                    newRow["RESNCODE"] = row["RESNCODE"];
                    newRow["RESNQTY"] = row["RESNQTY"].ToString().Equals("") ? 0 : row["RESNQTY"];
                    newRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(row["DFCT_TAG_QTY"])) ? 0 : row["DFCT_TAG_QTY"];
                    newRow["LANE_QTY"] = txtLaneQty.Value;
                    newRow["LANE_PTN_QTY"] = 1;
                    newRow["COST_CNTR_ID"] = row["COSTCENTERID"];

                    //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                    // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
                    if (_isResnCountUse == true)
                        newRow["WRK_COUNT"] = row["COUNTQTY"].ToString() == "" ? DBNull.Value : row["COUNTQTY"];

                    InResn.Rows.Add(newRow);
                }

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                    dg.EndEdit(true);
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
        private void SelectQuality(C1DataGrid dg, string ClctPontCode = null)
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
                newRow["CLCT_PONT_CODE"] = ClctPontCode;
                if (!string.IsNullOrWhiteSpace(txtVersion.Text))
                {
                    newRow["VER_CODE"] = txtVersion.Text;
                }

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

                        Util.GridSetData(dg, bizResult, null, true);

                        _util.SetDataGridMergeExtensionCol(dg, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                        //_util.SetDataGridMergeExtensionCol(dg, new string[] { "MEAN" }, DataGridMergeMode.VERTICALHIERARCHI);
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

        #region **투입자재
        /// <summary>
        /// 투입자재 List
        /// </summary>
        private void SelectInputMaterialList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["WOID"] = Util.NVC(DvProductLot["WOID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgMaterial, bizResult, null, true);

                        // 믹서공정은 투입자재 총사용량 = 생산량 [2017-03-02]
                        double inputMtrlSumQty = 0;
                        foreach (DataRow row in bizResult.Rows)
                        {
                            inputMtrlSumQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));
                        }

                        if (inputMtrlSumQty > 0)
                        {
                            if (txtProductionQty.Value == 0)
                            {
                                txtProductionQty.Value = inputMtrlSumQty;
                                SumDefectQty();
                            }
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

        /// <summary>
        /// 투입자재 List(BS, CMC)
        /// </summary>
        private void SelectInputMaterialListBS()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["PRODID"] = Util.NVC(DvProductLot["PRODID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY_SOL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgMaterial, bizResult, null, true);

                        // 믹서공정은 투입자재 총사용량 = 생산량 [2017-03-02]
                        double inputMtrlSumQty = 0;
                        foreach (DataRow row in bizResult.Rows)
                        {
                            inputMtrlSumQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));
                        }

                        if (inputMtrlSumQty > 0)
                        {
                            if (txtProductionQty.Value == 0)
                            {
                                txtProductionQty.Value = inputMtrlSumQty;
                                SumDefectQty();
                            }
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

        #endregion

        #region [Func]
        /// <summary>
        /// 단위에 따른 숫자 포멧
        /// </summary>
        private void SetUnitFormatted()
        {
            if (string.IsNullOrWhiteSpace(DvProductLot["UNIT_CODE"].ToString())) return;

            string sFormatted = GetUnitFormatted(DvProductLot["UNIT_CODE"].ToString());

            txtEquipmentQty.Format = sFormatted;
            txtProductionQty.Format = sFormatted;
            txtGoodQty.Format = sFormatted;

            // 불량/LOSS/물품청구 
            for (int col = 0; col < dgWipReason.Columns.Count; col++)
                if (dgWipReason.Columns[col].GetType() == typeof(DataGridNumericColumn))
                    ((DataGridNumericColumn)dgWipReason.Columns[col]).Format = sFormatted;
            // 품질정보
            for (int col = 0; col < dgQuality.Columns.Count; col++)
                if (dgQuality.Columns[col].GetType() == typeof(DataGridNumericColumn))
                    ((DataGridNumericColumn)dgQuality.Columns[col]).Format = sFormatted;

            // 투입자재
            for (int col = 0; col < dgMaterial.Columns.Count; col++)
                if (dgMaterial.Columns[col].GetType() == typeof(DataGridNumericColumn))
                    ((DataGridNumericColumn)dgMaterial.Columns[col]).Format = sFormatted;

        }

        private string GetUnitFormatted(string sUnit)
        {
            string sFormatted = "0";
            if (!string.IsNullOrWhiteSpace(sUnit))
            {
                switch (sUnit)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                        sFormatted = "F1";
                        break;
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
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
        private void SelectRemark(C1DataGrid dg)
        {
            try
            {
                DataTable dtRemark = new DataTable();
                dtRemark.Columns.Add("LOTID", typeof(String));
                dtRemark.Columns.Add("REMARK", typeof(String));

                DataRow newRow = dtRemark.NewRow();
                newRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

                string[] WipNote = GetRemarkData(Util.NVC(DvProductLot["LOTID"])).Split('|');
                if (WipNote.Length > 1)
                    newRow["REMARK"] = WipNote[1];

                dtRemark.Rows.Add(newRow);

                newRow = dtRemark.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["REMARK"] = GetRemarkData(Util.NVC(DvProductLot["LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(newRow);

                Util.GridSetData(dg, dtRemark, FrameOperation);

                // SLITTER가 아닌 경우 공통특이사항은 숨김
                dg.Rows[0].Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 생산실적
        /// <summary>
        private void SetProductionResult()
        {
            if (DvProductLot["WIPSTAT"].ToString() == "WAIT") return;

            // 버전, Lane수
            DataTable dtVersion = new DataTable();
            string sVersion = string.Empty;
            string sLaneQty = string.Empty;

            dtVersion = SetProductVersion();

            if (dtVersion != null && dtVersion.Rows.Count > 0)
            {
                sVersion = Util.NVC(dtVersion.Rows[0]["PROD_VER_CODE"]);
                sLaneQty = string.IsNullOrWhiteSpace(Util.NVC(dtVersion.Rows[0]["LANE_QTY"])) ? "0" : Util.NVC(dtVersion.Rows[0]["LANE_QTY"]);
            }

            // MIXER공정에서 CONVRATE에 버전정보 1개만 존재 시 해당 버전 SETUP [2017-05-10]
            if (string.IsNullOrWhiteSpace(sVersion))
                sVersion = SetProductVersionMixing();

            txtVersion.Text = sVersion;
            //txtLaneQty.Value = Convert.ToInt16(sLaneQty);

            txtStartDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ST"]).ToString("yyyy-MM-dd HH:mm");

            if (string.IsNullOrWhiteSpace(DvProductLot["WIPDTTM_ED"].ToString()))
                txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            else
                txtEndDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ED"]).ToString("yyyy-MM-dd HH:mm");

            if (txtWorkTime != null)
                txtWorkTime.Value = (Convert.ToDateTime(txtEndDateTime.Text) - Convert.ToDateTime(txtStartDateTime.Text)).TotalMinutes;

            // 작업일
            if (txtWorkDate != null)
                SetCalDate(txtWorkDate);

            txtLotID.Text = DvProductLot["LOTID"].ToString();
            txtWipstat.Text = DvProductLot["WIPSTAT_NAME"].ToString();
            txtUnit.Text = DvProductLot["UNIT_CODE"].ToString();

            // BEACMILL 횟수 SET
            if (ProcessCode == Process.PRE_MIXING)
                txtBeadMillCount.Value = Util.NVC_Int(DvProductLot["MILL_COUNT"]);

            SetLotDefectSumGrid();                    // Lot의 불량Sum그리드
            ////SetParentQty();                           // txtProductionQty, txtParentQty, 잔량산출

            // 조회시 Product List이 EQPT_END_QTY는 최종이 아닐수 있어 다시 조회 한다.
            //txtEquipmentQty.Value = Convert.ToDouble(DvProductLot["EQPT_END_QTY"].ToString());

            //// 해당 설비 완공 시점에서는 설비완공 시점에서 투입량을 수량으로 변경한다 [2017-02-14]
            //if (string.Equals(DvProductLot["WIPSTAT"].ToString(), Wip_State.EQPT_END) && txtProductionQty.IsReadOnly == false)
            //    txtProductionQty.Value = Convert.ToDouble(DvProductLot["EQPT_END_QTY"].ToString());

            // 저장되어 있는 수량이 있으면 그 수량을 최선책으로 지정 [2017-04-21]
            SetSaveProductQty();

            // UNIT별로 FORMAT
            SetUnitFormatted();
        }

        /// <summary>
        /// 실적 : Lot의 불량 Grid
        /// </summary>
        private void SetLotDefectSumGrid()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ITEM", typeof(string));
            dt.Columns.Add("DEFECT_SUM", typeof(string));
            dt.Columns.Add("DTL_DEFECT", typeof(string));
            dt.Columns.Add("DTL_LOSS", typeof(string));
            dt.Columns.Add("DTL_CHARGEPRD", typeof(string));

            DataRow newRow = dt.NewRow();
            newRow["ITEM"] = ObjectDic.Instance.GetObjectName("발생수량");
            for (int col = 1; col < dt.Columns.Count; col++)
                newRow[col] = "0";

            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow["ITEM"] = ObjectDic.Instance.GetObjectName("비율");
            for (int col = 1; col < dt.Columns.Count; col++)
                newRow[col] = "%";

            dt.Rows.Add(newRow);

            Util.GridSetData(dgProductResult, dt, null);
        }

        /// <summary>
        /// 불량 합산
        /// </summary>
        private void SumDefectQty()
        {
            try
            {
                DataTable dtDefect = DataTableConverter.Convert(dgWipReason.ItemsSource);

                double dInputQty;
                double dGoodQty;
                double dDefectSum = 0;
                double dDefect = 0;
                double dLoss = 0;
                double dChargeProd = 0;

                // Null인 경우 "NaN" 또는 "非数字"로 발생
                if (txtProductionQty.Value.ToString() == double.NaN.ToString())
                    dInputQty = 0;
                // 생산수량 0 입력 방지 생산수량 입력 이벤트 호출
                else if (txtProductionQty.Value == 0)
                {
                    SetSaveProductQty();
                    dInputQty = txtProductionQty.Value;
                }
                else
                    dInputQty = txtProductionQty.Value;

                foreach (DataRow dr in dtDefect.Rows)
                {
                    if (dr["RSLT_EXCL_FLAG"].ToString() != "Y" && !dr["PRCS_ITEM_CODE"].Equals("UNIDENTIFIED_QTY"))
                    {
                        if (dr["ACTID"].Equals("DEFECT_LOT"))
                            dDefect += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else if (dr["ACTID"].Equals("LOSS_LOT"))
                            dLoss += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else
                            dChargeProd += double.Parse(Util.NVC(dr["RESNQTY"]));
                    }
                }

                dDefectSum = dDefect + dLoss + dChargeProd;

                // 불량수
                DataTableConverter.SetValue(dgProductResult.Rows[0].DataItem, "DEFECT_SUM", dDefectSum);
                DataTableConverter.SetValue(dgProductResult.Rows[0].DataItem, "DTL_DEFECT", dDefect);
                DataTableConverter.SetValue(dgProductResult.Rows[0].DataItem, "DTL_LOSS", dLoss);
                DataTableConverter.SetValue(dgProductResult.Rows[0].DataItem, "DTL_CHARGEPRD", dChargeProd);

                // 불량율
                if (dInputQty != 0)
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[1].DataItem, "DEFECT_SUM", (dDefectSum / dInputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgProductResult.Rows[1].DataItem, "DTL_DEFECT", (dDefect / dInputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgProductResult.Rows[1].DataItem, "DTL_LOSS", (dLoss / dInputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgProductResult.Rows[1].DataItem, "DTL_CHARGEPRD", (dChargeProd / dInputQty).ToString("#0.##%"));
                }

                // Mixing 양품수량 = 생산수량 – 불량 – Loss - 물청
                dGoodQty = dInputQty - dDefect - dLoss - dChargeProd;

                txtGoodQty.Value = dGoodQty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        #endregion;

        #region[[Validation]

        private bool ValidationProductionUpdate()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            return true;
        }

        private bool ValidationDefect()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            return true;
        }

        private bool ValidationQuality()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            return true;
        }

        private bool ValidationRemark(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationVersion()
        {
            //if (string.IsNullOrWhiteSpace(txtVersion.Text))
            //{
            //    return false;
            //}

            return true;
        }

        private bool ValidationPopupInputMaterial()
        {
            if (ProcessCode == Process.MIXING && string.IsNullOrWhiteSpace(txtVersion.Text))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
        private void PopupVersion()
        {
            if (!ValidationVersion()) return;

            CMM_ELECRECIPE popupVersion = new CMM_ELECRECIPE { FrameOperation = FrameOperation };

            if (popupVersion != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(DvProductLot["PRODID"]);
                Parameters[1] = ProcessCode;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = EquipmentCode;
                Parameters[4] = Util.NVC(DvProductLot["LOTID"]);
                C1WindowExtension.SetParameters(popupVersion, Parameters);

                popupVersion.Closed += new EventHandler(PopupVersion_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupVersion.ShowModal()));
            }

        }

        private void PopupVersion_Closed(object sender, EventArgs e)
        {
            CMM_ELECRECIPE popup = sender as CMM_ELECRECIPE;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (Util.NVC_Decimal(txtLaneQty.Value) != Util.NVC_Decimal(popup._ReturnLaneQty))
                {
                    txtVersion.Text = popup._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(popup._ReturnLaneQty);

                    SumDefectQty();
                }
                else
                {
                    txtVersion.Text = popup._ReturnRecipeNo;
                }
            }
        }

        private void PopupEqptMaterial()
        {
            CMM_EQPT_MATERIAL popupEqptMaterial = new CMM_EQPT_MATERIAL { FrameOperation = FrameOperation };

            if (popupEqptMaterial != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Util.NVC(DvProductLot["LOTID"]);
                Parameters[1] = Util.NVC(DvProductLot["WOID"]);
                Parameters[2] = EquipmentCode;
                Parameters[3] = EquipmentName;
                C1WindowExtension.SetParameters(popupEqptMaterial, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupEqptMaterial.ShowModal()));
            }
        }

        private void PopupInputMaterial()
        {
            if (!ValidationPopupInputMaterial()) return;

            CMM_INPUT_MATERIAL popupInputMaterial = new CMM_INPUT_MATERIAL { FrameOperation = FrameOperation };

            if (popupInputMaterial != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = Util.NVC(DvProductLot["LOTID"]);
                Parameters[1] = Util.NVC(DvProductLot["WOID"]);
                Parameters[2] = EquipmentCode;
                Parameters[3] = EquipmentName;
                Parameters[4] = ProcessCode;
                Parameters[5] = Util.NVC(DvProductLot["PRODID"]);
                Parameters[6] = txtVersion.Text;
                Parameters[7] = txtProductionQty.Value;
                C1WindowExtension.SetParameters(popupInputMaterial, Parameters);

                popupInputMaterial.Closed += new EventHandler(PopupInputMaterial_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupInputMaterial.ShowModal()));
            }
        }

        private void PopupInputMaterial_Closed(object sender, EventArgs e)
        {
            CMM_INPUT_MATERIAL popup = sender as CMM_INPUT_MATERIAL;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            if (ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing)
                SelectInputMaterialListBS();
            else
                SelectInputMaterialList();

            //}
        }




        #endregion

        #endregion

        private void btnSolidContRate_Click(object sender, RoutedEventArgs e)
        {
            CMM_HOPR_SCLE_SOLD_CONT_RATE _popUpSolid = new CMM_HOPR_SCLE_SOLD_CONT_RATE();
            _popUpSolid.FrameOperation = FrameOperation;

            if (_popUpSolid != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Util.NVC(DvProductLot["EQPTID"]);
                Parameters[1] = Util.NVC(DvProductLot["PROCID"]);

                C1WindowExtension.SetParameters(_popUpSolid, Parameters);

                _popUpSolid.Closed += new EventHandler(popUpSolid_Closed);
                _popUpSolid.ShowModal();
                _popUpSolid.CenterOnScreen();
            }
        }
        private void popUpSolid_Closed(object sender, EventArgs e)
        {
            CMM_HOPR_SCLE_SOLD_CONT_RATE runStartWindow = sender as CMM_HOPR_SCLE_SOLD_CONT_RATE;
            if (runStartWindow.DialogResult == MessageBoxResult.Cancel)
            {
                SelectQuality(dgQuality);
                dgQuality.Refresh(false);
            }
        }

        #region ===[E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        /// <summary>
        /// 실적확정시 미투입자재 Pop에서 투입자재 저장시 투입자재 목록 재조회
        /// </summary>
        public void SelectInputMaterialResult()
        {
            // 투입자재
            if (ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing)
                SelectInputMaterialListBS();
            else
                SelectInputMaterialList();

        }
        #endregion

    }
}
