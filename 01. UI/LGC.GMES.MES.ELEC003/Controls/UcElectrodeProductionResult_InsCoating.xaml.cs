/*************************************************************************************
 Created Date : 2021.07.01
      Creator : 조영대
   Decription : 전극 공정진척 - 생산실적 (E2300 Ins Coating)
--------------------------------------------------------------------------------------
 [Change History]
 2021.07.01  조영대 : Initial Created.
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
using System.Linq;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ELEC003.Controls
{
    public partial class UcElectrodeProductionResult_InsCoating : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Button ButtonSaveRegDefectLane { get; set; }
        public Button ButtonSaveCarrier { get; set; }

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
        public bool bChangeMaterial
        {
            get { return _isChangeMaterial; }
        }
        public bool bChangeRemark
        {
            get { return _isChangeRemark; }
        }
        public decimal ExceedLengthQty
        {
            get { return _exceedLengthQty; }
        }

        private string coatSide = string.Empty;

        public string CoatSide
        {
            get { return coatSide; }
            set
            {
                coatSide = value;

                if (coatSide.Equals("B"))
                {
                    dgWipReasonBack.Visibility = Visibility.Visible;
                    lblBack.Visibility = Visibility.Visible;
                    spltWipReason.Visibility = Visibility.Visible;
                }
                else
                {
                    dgWipReasonBack.Visibility = Visibility.Collapsed;
                    lblBack.Visibility = Visibility.Collapsed;
                    spltWipReason.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        bool _isResnCountUse = false;
        bool _isDupplicatePopup = false;

        // DataCollect 변경 여부
        bool _isChangeWipReason = false;                      // 불량/LOSS/물품청구
        bool _isChangeQuality = false;                        // 품질정보
        bool _isChangeMaterial = false;                       // 투입자재
        bool _isChangeRemark = false;                         // 특이사항
        bool _isChangeInputFocus = false;

        bool _isDefectLevel = false;

        decimal _inputOverrate;                               // 전극 투입 제한율Control Load시 공통코그정보 셋팅)

        decimal _exceedLengthQty;
        decimal _convRate;

        private const string _itemCodeLenLack = "LENGTH_LACK";
        private const string _itemCodeLenExceed = "LENGTH_EXCEED";

        private int _dgLVIndex1 = 0;
        private int _dgLVIndex2 = 0;
        private int _dgLVIndex3 = 0;

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));

        public UcElectrodeProductionResult_InsCoating()
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
            
            // 투입량의 초과입력률 체크하기 위하여 추가
            string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            _inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));
        }

        private void SetButtons()
        {
            ButtonSaveCarrier = btnSaveCarrier;
            ButtonSaveRegDefectLane = btnSaveRegDefectLane;
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

        /// <summary>
        /// 
        /// </summary>
        private void SetControlClear()
        {
            txtLotID.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtUnit.Text = string.Empty;
            //txtCurLaneQty.Value = 0;
            txtLaneQty.Value = 0;

            chkFinalCut.IsChecked = false;

            txtInputQty.Value = 0;
            txtParentQty.Value = 0;
            txtRemainQty.Value = 0;

            _isResnCountUse = false;
            _isDupplicatePopup = false;
            
            // DataCollect 변경 여부
            _isChangeWipReason = false;                      // 불량/LOSS/물품청구
            _isChangeQuality = false;                        // 품질정보
            _isChangeMaterial = false;                       // 투입자재
            _isChangeRemark = false;                         // 특이사항
            _isChangeInputFocus = false;

            _exceedLengthQty = 0;
            _convRate = 1;

            bProductionUpdate = false;                       // 실적 저장, 불량/LOSS/물품청구 저장시 True

            _dgLVIndex1 = 0;
            _dgLVIndex2 = 0;
            _dgLVIndex3 = 0;

            Util.gridClear(dgWipReasonTop);
            Util.gridClear(dgWipReasonBack);            
            Util.gridClear(dgQualityTop);
            Util.gridClear(dgQualityBack);
            Util.gridClear(dgRemark);
            Util.gridClear(dgRemarkHistory);
        }

        private void SetControlVisibility()
        {
            btnSaveRegDefectLane.Visibility = Visibility.Collapsed;
            btnSaveCarrier.Visibility = Visibility.Collapsed;

            tbOutCstID.Visibility = Visibility.Collapsed;
            txtOutCstID.Visibility = Visibility.Collapsed;

            switch (UnldrLotIdentBasCode)
            {
                case "CST_ID":
                case "RF_ID":
                    btnSaveCarrier.Visibility = Visibility.Visible;

                    tbOutCstID.Visibility = Visibility.Visible;
                    txtOutCstID.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
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
                        dgWipReasonTop.EndEdit(true);
                        dgWipReasonBack.EndEdit(true);
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

        private void dgProductResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (string.Equals(e.Cell.Column.Tag, "N"))
                            {
                                if ((e.Cell.Row.Index - dataGrid.TopRows.Count) > 0)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Transparent);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }

                            //if (dataGrid.Columns["INPUT_VALUE_TYPE"].Index < e.Cell.Column.Index &&
                            //    dataGrid.Columns.Count > e.Cell.Column.Index && ((e.Cell.Row.Index - dataGrid.TopRows.Count)) == 2)
                            //{
                            //    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name,
                            //                 Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) -
                            //                 Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)));

                            //    if (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) !=
                            //        Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)))
                            //    {
                            //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            //    }
                            //    else
                            //    {
                            //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            //    }
                            //}
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
        /// Carrier 연계
        /// </summary>
        private void btnSaveCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveCarrier()) return;

            SaveCarrier();
        }

        /// <summary>
        /// 저장
        /// </summary>
        private void btnProductionUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductionUpdate()) return;

            if (txtInputQty.Value <= 0)
            {
                if (IsCoaterProdVersion() == true && !string.Equals(GetCoaterMaxVersion(), txtVersion.Text))
                {
                    // 작업지시 최신 Version과 상이합니다! 그래도 저장하시겠습니까?
                    Util.MessageConfirm("SFU4462", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                        {
                            SaveProductionUpdate();
                        }
                    }, new object[] { GetCoaterMaxVersion(), txtVersion.Text });
                }
                else
                {
                    SaveProductionUpdate();
                }
            }
        }

        #region **불량/LOSS/물품청구
        #region *LVFilter
        private void chkDefectFilter_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;

                //CWA 불량등록 필터 그리드
                GetDefectLevel();
            }
            else
            {
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }

        }

        private void dgLevel_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dgReason = dgWipReasonTop;

            switch ((sender as C1DataGrid).Name)
            {
                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            _dgLVIndex1 = e.Cell.Row.Index;

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                            {
                                                                dgReason.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }

                                                    if (dgWipReasonBack.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReasonBack.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                            Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 1, true);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }

                                                    if (dgWipReasonBack.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReasonBack.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, true);
                                                    }

                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (_dgLVIndex1 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            _dgLVIndex1 = e.Cell.Row.Index;
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel1.CurrentCell != null)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.CurrentCell.Row.Index, dgLevel1.Columns.Count - 1);
                                else if (dgLevel1.Rows.Count > 0)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.Rows.Count, dgLevel1.Columns.Count - 1);

                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            _dgLVIndex2 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }

                                                    if (dgWipReasonBack.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReasonBack.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, true);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();

                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }

                                                    if (dgWipReasonBack.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReasonBack.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, true);
                                                    }
                                                }
                                            }
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (_dgLVIndex2 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            _dgLVIndex2 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel2.CurrentCell != null)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.CurrentCell.Row.Index, dgLevel2.Columns.Count - 1);
                                else if (dgLevel2.Rows.Count > 0)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.Rows.Count, dgLevel2.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            _dgLVIndex3 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }

                                                    if (dgWipReasonBack.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReasonBack.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, true);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, true);
                                                    }
                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (_dgLVIndex3 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            _dgLVIndex3 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel3.CurrentCell != null)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.CurrentCell.Row.Index, dgLevel3.Columns.Count - 1);
                                else if (dgLevel3.Rows.Count > 0)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.Rows.Count, dgLevel3.Columns.Count - 1);

                            }
                        }
                    }));
                    break;
            }
        }

        private void dgLevel_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        _dgLVIndex1 = 0;
                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        _dgLVIndex2 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        _dgLVIndex3 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;
            }
        }
        #endregion

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if ((dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) > dgProductResult.TopRows.Count)
            {
                C1DataGrid caller = sender as C1DataGrid;

                if (ValidateDefect(sender as C1DataGrid))
                {
                    if ((dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) > dgProductResult.TopRows.Count + 1)
                    {
                        Util.MessageValidation("DefectChange() 실행됨 확인!");
                    }
                    else
                    {
                        // 청주1동 특화 M -> P 변환 로직 [2017-05-15]
                        if (string.Equals(caller.CurrentCell.Column.Name, "CONVRESNQTY") && string.Equals(txtUnit.Text, "EA"))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY",
                                Convert.ToDouble(GetIntFormatted(Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CONVRESNQTY")) / _convRate)));

                        GetSumDefectQty();
                    }

                    _isChangeWipReason = true;

                    SetExceedLength();
                    dgProductResult.Refresh(false);

                }
            }
        }

        private void GetSumDefectQty()
        {
            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            if ((dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) == dgProductResult.TopRows.Count + 1)
            {
                defectQty = SumDefectQty("DEFECT_LOT");
                LossQty = SumDefectQty("LOSS_LOT");
                chargeQty = SumDefectQty("CHARGE_PROD_LOT");

                totalSum = defectQty + LossQty + chargeQty;

                DataTable dt = (dgProductResult.ItemsSource as DataView).Table;

                for (int i = dgProductResult.TopRows.Count; i < dt.Rows.Count + dgProductResult.TopRows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_LOSS", LossQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY", totalSum);

                    if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                    else
                        DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) + totalSum);


                    laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_QTY"));

                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * laneqty);
                }


                SetParentRemainQty();
            }
        }

        private double SumDefectQty(string actId)
        {
            double sum = 0;

            if (dgWipReasonTop.Rows.Count > 0)
                for (int i = 0; i < dgWipReasonTop.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "RESNQTY")) * 1);

            if (dgWipReasonBack.Visibility == Visibility.Visible && dgWipReasonBack.Rows.Count > 0)
                for (int i = 0; i < dgWipReasonBack.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += Convert.ToDouble(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "RESNQTY"));

            return sum;
        }

        private bool ValidateDefect(C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return false;
            }
            

                if (string.Equals(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                {
                    // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                    int iCount = _isResnCountUse == true ? 1 : 0;

                    decimal inputQty = 0;
                    decimal inputLengthQty = 0;

                   
                        inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "RESNQTY"));

                    if (inputLengthQty > 0)
                    {
                        inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

                        if (Util.NVC_Decimal(txtParentQty.Value) > inputQty)
                        {
                            Util.MessageValidation("SFU3424");  // FINAL CUT이 아닌 경우 길이초과 입력 불가
                            DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);
                        
                            _exceedLengthQty = 0;
                            return false;
                        }

                        if (inputLengthQty > (inputQty - Util.NVC_Decimal(txtParentQty.Value)))
                        {
                            Util.MessageValidation("SFU3422", (inputQty - Util.NVC_Decimal(txtParentQty.Value)) + txtUnit.Text);    // 길이초과수량을 초과하였습니다.[현재 실적에서 길이초과는 %1까지 입력 가능합니다.] 
                            DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);
                        
                            _exceedLengthQty = 0;
                            return false;
                        }
                    }                    
                }
            
            return true;
        }
        
        private void dgWipReason_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                    //삭제시 길이초과 재산출
                    SetExceedLength();
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }

        private void dgWipReasonBack_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                        // BACK단선은 전체 체크 시 설비에서 올라온 단선BASE수량만 변경하도록 변경 (실수로 체크 시 TOP/BACK수량이 투입량에 반영되어 크게 왜곡 발생) [2019-12-04]
                                        // 코터 공정 단선 조정 시 투입량 변경으로 전체 불량 등록 시 단선 수 차감하고 등록하도록 수정 [2019-01-13]
                                        decimal dWebBreakQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                    if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    {
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                                        DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                                    }
                                    else
                                    {
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                                        Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_BACK_QTY")) - dWebBreakQty);
                                    }

                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                        }
                                    }
                                    SumDefectQty();
                                    dgProductResult.Refresh(false);
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESN_TOT_CHK", false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                            SumDefectQty();
                            dgProductResult.Refresh(false);
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 전체 저장
        /// </summary>
        private void btnSaceAllWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefect()) return;

            // 불량/LOSS/물품청구
            SaveDefect(dgWipReasonTop, true);
            SaveDefect(dgWipReasonBack, true);

            // 품질정보
            SaveQuality(dgQualityTop, true);
            SaveQuality(dgQualityBack, true);
            
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
            SaveDefect(dgWipReasonTop, true);
            SaveDefect(dgWipReasonBack);
        }
        #endregion

        #region **품질정보
        private void dgQualityTop_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }

                            if (string.Equals(e.Cell.Column.Name, "INSP_CONV_RATE"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;

                                if (Convert.ToDouble(e.Cell.Value) == 1)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                else
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                            }
                        }
                    }
                }));
            }
        }

        private void dgQualityTop_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (string.Equals(e.Cell.Column.Name, "INSP_CONV_RATE"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }

                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }));
            }

        }

        private void dgQualityTop_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02"));
                //sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-').Count() == 3)
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0] + "-" +
                        Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[1];
                }
                else
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                }
                sCLCNAME = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
            }
            else
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            }
            string sCode = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
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
                    _isChangeQuality = true;
                }

                if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
                {
                    if (dgQualityBack.Visibility == Visibility.Visible)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = null;

                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", null);

                        if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM") && Convert.ToDouble(sValue) != Double.NaN)
                        {
                            double inputRate = Convert.ToDouble(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_CONV_RATE"));
                            double input = Convert.ToDouble(GetUnitFormatted(sValue)) * inputRate;

                            // Unloading량은 원래 BACK로딩량 - TOP로딩량임, 하지만 현재 요청한 구조로는 처리가 어려워서 오창 자동차동만 하기 LOGIC을 사용한다고 하여 하기와 같은 로직을 내부적으로 반영
                            // TOP 로딩량 존재 시 : BACK LOADING - (TOP LOADING INPUT VALUE * 환산값 * 2) [소수점을 사용안하고 반을 감안하기 위하여 하기와 같이 적용 [2019-03-18]
                            bool isValueComplete = false;
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "E2000-0002"))
                            {
                                DataRow[] rows = (dgQualityTop.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}'",
                                    new object[] { "E2000-0001", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                    else
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));

                                    isValueComplete = true;
                                }
                            }
                            else if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "SI017"))
                            {
                                DataRow[] rows = (dgQualityTop.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}'",
                                    new object[] { "SI016", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                    else
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));

                                    isValueComplete = true;
                                }
                            }

                            if (isValueComplete == false)
                                DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);

                            C1.WPF.DataGrid.DataGridCell inputCell = dgQualityBack.GetCell(caller.CurrentRow.Index, caller.Columns["CLCTVAL01"].Index);

                            if (sLSL != "" && Util.NVC_Decimal(input) < Util.NVC_Decimal(sLSL))
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.FontWeight = FontWeights.Bold;
                            }

                            else if (sUSL != "" && Util.NVC_Decimal(input) > Util.NVC_Decimal(sUSL))
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                inputCell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            _isChangeQuality = true;
                        }
                    }
                }
            }
        }

        private void dgQualityTop_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (string.Equals(dataGrid.CurrentCell.Column.Name, "CLCTVAL02"))
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        int iRowIdx = dataGrid.CurrentCell.Row.Index;
                        if ((dataGrid.CurrentCell.Row.Index + 1) < dataGrid.GetRowCount())
                            iRowIdx++;

                        C1.WPF.DataGrid.DataGridCell currentCell = dataGrid.GetCell(iRowIdx, dataGrid.CurrentCell.Column.Index);
                        Util.SetDataGridCurrentCell(dataGrid, currentCell);
                        dataGrid.CurrentCell = currentCell;
                        dataGrid.Focus();
                    }
                    else if (e.Key == Key.Delete)
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                        {
                            // 이동중 DEL키 입력 시는 측정값 초기화하도록 변경
                            if (dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter != null && dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter.Content != null &&
                                dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Value != null)
                            {
                                ((C1NumericBox)dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter.Content).Value = 0;
                            }
                            else
                            {
                                DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, "CLCTVAL01", null);
                            }
                        }
                    }
                }
                else
                {
                    if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                    {
                        dataGrid.EndEdit(true);
                    }
                    else if (e.Key == Key.Delete)
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                        {
                            DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                            dataGrid.BeginEdit(dataGrid.CurrentCell);
                            dataGrid.EndEdit(true);

                            DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                            if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                            {
                                dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                                dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                            dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                    }
                }
            }
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
                    iMeanColldx = dgQualityTop.Columns["MEAN"].Index;

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

        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQuality()) return;

            SaveQuality(dgQualityTop, true);
            SaveQuality(dgQualityBack);
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
            listAuth.Add(btnSaveRegDefectLane);               // 전수불량Lane등록
            listAuth.Add(btnSaveCarrier);                     // Carrier 연계
            listAuth.Add(btnProductionUpdate);                // 저장
            listAuth.Add(btnSaveAllWipReason);                // 불량/LOSS/물품청구 : 전체저장
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

            // LV Filter Clear
            ClearDefectLV();

            // 실적
            SetProductionResult();

            // 불량/LOSS/물품청구
            SelectDefect(dgWipReasonTop, "DEFECT_TOP");
            SelectDefect(dgWipReasonBack, "DEFECT_BACK");
            
            // 품질정보
            SelectQuality(dgQualityTop, "T");
            SelectQuality(dgQualityBack, "B");

            // 특이사항
            SelectRemark(dgRemark);

            // 이전특이사항
            SelectRemarkPrevious();
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

        private string GetCoaterMaxVersion()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PRODID"] = DvProductLot["PRODID"].ToString();
                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_MAX_VERSION", "INDATA", "RSLTDT", inTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return Util.NVC(dtMain.Rows[0][0]);

            }
            catch (Exception ex) { }

            return "";
        }

        private DataTable SetProcessVersion()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MODLID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["MODLID"] = DvProductLot["PRODID"];
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return null;
        }

        private Int32 SetCurrLaneQty()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "RQSTDT", "RSLTDT", inTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return 0;
        }

        private Int32 getCurrLaneQty(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotID;
                dr["PROCID"] = Process.COATING;
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "RQSTDT", "RSLTDT", RQSTDT);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return 0;
        }

        private void GetResultInfo()
        {
            try
            {
                dgProductResult.ClearRows();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LOTID_PR"] = Util.NVC(DvProductLot["WIPSTAT"]) == Wip_State.EQPT_END ? null : DvProductLot["LOTID"];
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["WIPSTAT"] = DvProductLot["WIPSTAT"];
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_CHILD_TP", "INDATA", "RSLTDT", inTable);

                // LOT정보
                Util.GridSetData(dgProductLotInfo, dtResult, null);
                
                if (dtResult.Rows.Count > 0)
                {
                    txtPrLotId.Text = Util.NVC(dtResult.Rows[0]["PR_LOTID"]);
                }

                // 실적수량
                Util.GridSetData(dgProductResult, dtResult, null, false);


                // 모LOT투입량 산출
                SetParentQty(Util.NVC(DvProductLot["LOTID"]), Util.NVC(DvProductLot["WIPSTAT"]));

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

        private void SetParentQty(string lotid, string status)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = lotid;
                indata["WIPSTAT"] = status;

                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QTY", "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    if (status.Equals(Wip_State.EQPT_END))
                        txtInputQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_OUT"]);

                    txtParentQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_IN"].ToString());
                    SetParentRemainQty();

                    // 절연코터는 투입량 = 생산량이어야 해서 자동 입력 후 수정 못하게 변경
                    txtInputQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_IN"].ToString());
                    SetInputQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetParentRemainQty()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            txtRemainQty.Value = 0;
            txtInputQty.Value = 0;
        }

        private void SetExceedLength()
        {
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = _isResnCountUse == true ? 1 : 0;

            for (int i = 0; i < dgWipReasonTop.Rows.Count; i++)
            {
                if (string.Equals(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "PRCS_ITEM_CODE"), _itemCodeLenExceed))
                {
                    if ((dgProductResult.Rows.Count - dgProductResult.BottomRows.Count) == dgProductResult.TopRows.Count + 1)
                        _exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "RESNQTY"));
                    else
                        _exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, dgWipReasonTop.Columns[dgWipReasonTop.Columns["ALL"].Index + (2 + iCount)].Name));
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

            // SLITTER가 아닌 경우 공통특이사항은 숨김
            dgRemark.Rows[0].Visibility = Visibility.Collapsed;
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

        private void SetInputQtyBack()
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

                if (txtLaneQty != null)
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * Util.NVC_Decimal(txtLaneQty.Value));
            }

            SetParentRemainQty();
        }

        void SetInputQty()
        {
            if (dgProductResult.GetRowCount() < 1)
                return;

            decimal inputQty = Util.NVC_Decimal(txtInputQty.Value);
            decimal lossQty = 0;
            int laneqty = 0;

            for (int i = 0 + dgProductResult.TopRows.Count; i < (dgProductResult.Rows.Count - dgProductResult.BottomRows.Count); i++)
            {
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY"));
                laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_QTY"));

                if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY", inputQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                }
                else
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY", inputQty + lossQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", inputQty);
                }

                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * laneqty);
            }

            SetParentRemainQty();
        }

        #region *Carrier 연계
        /// <summary>
        /// Check Lot
        /// </summary>
        private bool CheckLotID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU7000", sLotID);     // LOTID[%1]에 해당하는 LOT이 없습니다.
                    return false;
                }

                if (!string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CSTID"])))
                {
                    Util.MessageValidation("SFU5126", Util.NVC(searchResult.Rows[0]["CSTID"]), sLotID);    // Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                    return false;
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;

        }

        /// <summary>
        /// Check CST
        /// </summary>
        private bool CheckCstID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = sCstID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    //CSTID[%1]에 해당하는 CST가 없습니다.
                    Util.MessageValidation("SFU7001", sCstID);
                    return false;
                }

                //캐리어 상태 Check
                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("U"))
                {
                    if (Util.NVC(searchResult.Rows[0]["CURR_LOTID"]) == sLotID)
                    {
                        Util.MessageValidation("SFU5126", sCstID, sLotID);     // Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                        return false;
                    }
                    else
                    {
                        Util.MessageValidation("SFU7002", Util.NVC(searchResult.Rows[0]["CSTID"]), Util.NVC(searchResult.Rows[0]["CSTSNAME"]));     // CSTID[%1] 이 상태가 %2 입니다.
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;
        }

        /// <summary>
        /// Carrier 연계
        /// </summary>
        private void SaveCarrier()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("SRCTYPE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = DvProductLot["LOTID"].ToString();
            newRow["CSTID"] = txtOutCstID.Text;
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inTable.Rows.Add(newRow);

            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                                bProductionUpdate = true;
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                        finally
                        {
                        }
                    });
                }
            });
        }

        #endregion

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
                newRow["LANE_PTN_QTY"] = 1;
                newRow["LANE_QTY"] = Util.NVC_Decimal(txtLaneQty.Value);
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

                        SaveDefect(dgWipReasonTop, true);
                        SaveDefect(dgWipReasonBack, true);

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
        #region *LVFilter
        /// <summary>
        /// LVFilter
        /// </summary>
        private void GetDefectLevel()
        {
            try
            {
                string[] Level = { "LV1", "LV2", "LV3" };

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LV_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();


                DataTable dtAddAll = new DataTable();
                dtAddAll.Columns.Add("CHK", typeof(string));
                dtAddAll.Columns.Add("LV_NAME", typeof(string));
                dtAddAll.Columns.Add("LV_CODE", typeof(string));

                DataRow AddData = dtAddAll.NewRow();

                for (int i = 0; i < Level.Count(); i++)
                {
                    AddData["CHK"] = 0;
                    AddData["LV_NAME"] = "ALL";
                    AddData["LV_CODE"] = "ALL";
                    dtAddAll.Rows.Add(AddData);

                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = ProcessCode;
                    Indata["LV_CODE"] = Level[i];

                    IndataTable.Rows.Add(Indata);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC_LEVEL", "RQSTDT", "RSLTDT", IndataTable);

                    dtAddAll.Merge(dtResult);

                    if (i == 0)
                        Util.GridSetData(dgLevel1, dtAddAll, FrameOperation, true);
                    else if (i == 1)
                        Util.GridSetData(dgLevel2, dtAddAll, FrameOperation, true);
                    else if (i == 2)
                        Util.GridSetData(dgLevel3, dtAddAll, FrameOperation, true);

                    IndataTable.Clear();
                    dtAddAll.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        /// <summary>
        /// 불량/LOSS/물품청구 조회
        /// </summary>
        private void SelectDefect(C1DataGrid dg, string Resnposition = null)
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
                newRow["RESNPOSITION"] = Resnposition;
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

                        Util.GridSetData(dg, bizResult, null, true);

                        SetCauseTitle(dg);
                        SumDefectQty();
                        GetSumDefectQty();
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
                if (!dg.Visibility.Equals(Visibility.Visible)) return;

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
        /// 품질정보 Seq 조회
        /// </summary>
        private string[] GetWipSeq(string sLotID, string sCLCTITEM)
        {
            string[] RetrunSeq = new string[2];

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = ProcessCode;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_SL", "INDATA", "RSLTDT", IndataTable);

            if (dtResult.Rows.Count == 0)
            {
                RetrunSeq[0] = null;
                RetrunSeq[1] = null;
            }
            else
            {
                RetrunSeq[0] = dtResult.Rows[0]["WIPSEQ"].ToString();
                RetrunSeq[1] = dtResult.Rows[0]["CLCTSEQ"].ToString();
            }

            return RetrunSeq;
        }

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
                    newRow["LANEQTY"] = txtLaneQty.Value;
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

                        if (string.Equals(dg.Name, dgQualityBack.Name))
                            bizResult.Columns.Add("CLCTVAL02", typeof(double));

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

        #endregion

        #region [Func]
        #region #LVFilter
        private void ClearDefectLV()
        {
            if (chkDefectFilter.IsChecked == true)
            {
                _isDefectLevel = true;
                OnClickDefetectFilter(chkDefectFilter, null);
                _isDefectLevel = false;
            }
        }

        private void OnClickDefetectFilter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                GetDefectLevel();
            }
            else
            {
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }
        }

        private void DefectVisibleLV(DataTable dt, int LV, bool chk)
        {
            if (LV == 1)
            {
                DefectVisibleLV1(dt, chk);
            }
            else if (LV == 2)
            {
                DefectVisibleLV2(dt, chk);
            }
            else if (LV == 3)
            {
                DefectVisibleLV3(dt, chk);
            }
        }

        private void DefectVisibleLV1(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (_dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[_dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[_dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (_dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[_dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[_dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV2(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (_dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[_dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[_dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (_dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[_dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[_dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV3(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (_dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[_dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonTop.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[_dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReasonTop.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonTop.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            if (chk == true)
            {
                if (_dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[_dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (_dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReasonBack.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[_dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReasonBack.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReasonBack.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLVAll()
        {
            DataTable dt = (dgWipReasonTop.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgWipReasonTop.Rows[i].Visibility = Visibility.Visible;
            }

            DataTable dt2 = (dgWipReasonBack.ItemsSource as DataView).Table;

            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                dgWipReasonBack.Rows[i].Visibility = Visibility.Visible;
            }
        }

        #endregion

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

                for (int i = 0; i < dgProductResult.Columns.Count; i++)
                    if (dgProductResult.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgProductResult.Columns[i].Tag, "N"))
                        // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgProductResult.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgProductResult.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgWipReasonTop.Columns.Count; i++)
                    if (dgWipReasonTop.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReasonTop.Columns[i].Tag, "N"))
                        ((DataGridNumericColumn)dgWipReasonTop.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgWipReasonBack.Columns.Count; i++)
                    if (dgWipReasonBack.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReasonBack.Columns[i].Tag, "N"))
                        ((DataGridNumericColumn)dgWipReasonBack.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgQualityTop.Columns.Count; i++)
                    if (dgQualityTop.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQualityTop.Columns[i].Tag, "N"))
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgQualityTop.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgQualityTop.Columns[i]).Format = sFormatted;

                for (int i = 0; i < dgQualityBack.Columns.Count; i++)
                    if (dgQualityBack.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQualityBack.Columns[i].Tag, "N"))
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgQualityBack.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgQualityBack.Columns[i]).Format = sFormatted;

            }
        }

        /// <summary>
        /// 단위에 따른 숫자 포멧
        /// </summary>
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

        private string GetIntFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = "{0:#,##0}";
            double dFormat = 0;

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private void SetCauseTitle(C1DataGrid dg)
        {
            int causeqty = 0;

            if (dg.ItemsSource != null)
            {
                DataTable dt = (dg.ItemsSource as DataView).Table;
                for (int i = dg.TopRows.Count; i < dt.Rows.Count + dg.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(dg.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }
                if (causeqty > 0)
                {
                    if (dg.Name.ToString() != "dgWipReasonBack")
                    {
                        if (lblTop.Visibility == Visibility.Visible)
                        {
                            lblTop.Text = ObjectDic.Instance.GetObjectName("Top(*는 타공정 귀속)");
                        }
                        else
                        {
                            lblTop.Visibility = Visibility.Visible;
                            lblTop.Text = ObjectDic.Instance.GetObjectName("(*는 타공정 귀속)");
                        }
                    }
                    else
                    {
                        lblBack.Text = ObjectDic.Instance.GetObjectName("Back(*는 타공정 귀속)");
                    }
                }
            }

        }

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

                dgRemark.Rows[0].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProductionResult()
        {
            if (DvProductLot["WIPSTAT"].ToString() == "WAIT") return;

            // 버전, Lane수
            DataTable dtVersion = new DataTable();
            string sVersion = string.Empty;
            string sLaneQty = string.Empty;

            dtVersion = SetProcessVersion();

            if (dtVersion != null && dtVersion.Rows.Count > 0)
            {
                sVersion = Util.NVC(dtVersion.Rows[0]["PROD_VER_CODE"]);
                sLaneQty = string.IsNullOrWhiteSpace(Util.NVC(dtVersion.Rows[0]["LANE_QTY"])) ? "0" : Util.NVC(dtVersion.Rows[0]["LANE_QTY"]);
            }

            txtVersion.Text = sVersion;
            if (!Util.IsNVC(sLaneQty))
            {
                txtLaneQty.Value = Convert.ToInt16(sLaneQty);
            }

            // CWA 전수 불량 추가
            //txtCurLaneQty.Value = SetCurrLaneQty();
            txtStartDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ST"]).ToString("yyyy-MM-dd HH:mm");

            if (string.IsNullOrWhiteSpace(DvProductLot["WIPDTTM_ED"].ToString()))
                txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            else
                txtEndDateTime.Text = Convert.ToDateTime(DvProductLot["WIPDTTM_ED"]).ToString("yyyy-MM-dd HH:mm");

            // 작업일
            if (txtWorkDate != null)
                SetCalDate(txtWorkDate);

            txtLotID.Text = DvProductLot["LOTID"].ToString();
            txtWipstat.Text = DvProductLot["WIPSTAT_NAME"].ToString();
            txtUnit.Text = DvProductLot["UNIT_CODE"].ToString();
            txtOutCstID.Text = DvProductLot["OUT_CSTID"].ToString();

            // 합권취용 투입 LOT SET
            txtMergeInputLot.Text = DvProductLot["LOTID_PR"].ToString();

            if (string.Equals(DvProductLot["FINAL_CUT_FLAG"], "Y"))
                chkFinalCut.IsChecked = true;

            //if (string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
            //{
            //    btnSaveAllWipReason.IsEnabled = true;
            //    btnSaveWipReason.IsEnabled = true;
            //}
            //else
            //{
            //    btnSaveAllWipReason.IsEnabled = false;
            //    btnSaveWipReason.IsEnabled = false;
            //}

            if (string.Equals(txtUnit.Text, "EA"))
                _convRate = GetPatternLength(Util.NVC(DvProductLot["PRODID"]));

            GetResultInfo();

            // UNIT별로 FORMAT
            SetUnitFormatted();
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
        
        private void SumDefectQty()
        {
            if (!string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
                return;

            decimal dTopInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
            decimal dBackInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
            decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "LANE_QTY"));
            decimal dTopDefectQty = GetSumDefectQty(dgWipReasonTop, "DEFECT_LOT");
            decimal dTopLossQty = GetSumDefectQty(dgWipReasonTop, "LOSS_LOT");
            decimal dTopChargeProdQty = GetSumDefectQty(dgWipReasonTop, "CHARGE_PROD_LOT");
            decimal dTopTotalQty = dTopDefectQty + dTopLossQty + dTopChargeProdQty;
            decimal dBackDefectQty = GetSumDefectQty(dgWipReasonBack, "DEFECT_LOT");
            decimal dBackLossQty = GetSumDefectQty(dgWipReasonBack, "LOSS_LOT");
            decimal dBackChargeProdQty = GetSumDefectQty(dgWipReasonBack, "CHARGE_PROD_LOT");
            decimal dBackWebBreakQty = GetDiffWebBreakQty(dgWipReasonBack, "DEFECT_LOT", "BACK");
            decimal dBackTotalQty = dBackDefectQty + dBackLossQty + dBackChargeProdQty;

            for (int i = 0; i < dgProductResult.GetRowCount(); i++)
            {
                if (!string.Equals(DataTableConverter.GetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_TOP_DEFECT", dTopDefectQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_TOP_LOSS", dTopLossQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_TOP_CHARGEPRD", dTopChargeProdQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_TOP_DEFECT_SUM", dTopTotalQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_BACK_DEFECT", dBackDefectQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_BACK_LOSS", dBackLossQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_BACK_CHARGEPRD", dBackChargeProdQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM", dBackTotalQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "GOODQTY", ((dBackInputQty - dBackWebBreakQty) - dBackTotalQty));
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "GOODQTY2", (((dBackInputQty - dBackWebBreakQty) - dBackTotalQty) * dLaneQty));
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "INPUT_TOP_QTY", dTopTotalQty);
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "INPUT_BACK_QTY", dBackInputQty - dBackWebBreakQty);
                }
                else
                {
                    DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "GOODQTY2",
                        Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "GOODQTY")) * dLaneQty);
                }
            }
            // Summary 추가
            txtInputQty.Value = Convert.ToDouble(dBackInputQty);                                            // 투입량
            txtParentQty.Value = Convert.ToDouble(dBackInputQty - dBackTotalQty);                             // 양품량
            txtRemainQty.Value = Convert.ToDouble(dBackInputQty - (dBackInputQty - dBackTotalQty));         // 잔량
        }
        
        private double SumDefectQty(C1DataGrid dataGrid, string actId)
        {
            double sum = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            return sum;
        }

        private decimal GetSumDefectQty(C1DataGrid dataGrid, string sActId)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            return dSumQty;
        }

        private decimal GetDiffWebBreakQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                                        Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
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
            if (string.IsNullOrWhiteSpace(txtVersion.Text))
            {
                return false;
            }

            if (!string.Equals(DvProductLot["WIPSTAT"], Wip_State.PROC))
            {
                Util.MessageValidation("SFU7353", ObjectDic.Instance.GetObjectName("완공"));     // {%1} 상태에서는 버전을 변경할 수 없습니다.  완공
                return false;
            }

            return true;
        }

        private bool ValidationSaveCarrier()
        {
            if (string.IsNullOrWhiteSpace(txtOutCstID.Text))
            {
                Util.MessageValidation("SFU6051");     // 입력오류 : Carrier ID를 입력 하세요.
                return false;
            }

            if (!CheckCstID(DvProductLot["LOTID"].ToString(), txtOutCstID.Text))
            {
                return false;
            }
            else
            {
                if (!CheckLotID(DvProductLot["LOTID"].ToString(), txtOutCstID.Text))
                    return false;
            }

            return true;
        }

        private bool IsCoaterProdVersion()
        {
            // 1. LOT이 선택되었는지 확인
            if (DvProductLot == null)
                return false;

            // 2. 입력된 VERSION 체크
            if (string.IsNullOrWhiteSpace(txtVersion.Text))
                return false;

            // 3. 양산버전 이외는 체크 안함
            System.Text.RegularExpressions.Regex engRegex = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]");
            if (engRegex.IsMatch(txtVersion.Text.Substring(0, 1)) == true)
                return false;

            // 4. 1번 CUT인지 확인
            string sCut = Util.NVC(DvProductLot["CUT"]);
            if (string.IsNullOrEmpty(sCut) || !string.Equals(sCut, "1"))
                return false;

            return true;
        }

        private bool ValidationMerge()
        {
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
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(DvProductLot["PRODID"]);
                Parameters[1] = ProcessCode;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = EquipmentCode;
                Parameters[4] = Util.NVC(DvProductLot["LOTID"]);
                Parameters[5] = "Y";    // 전극 버전 확정 여부
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
                    //txtCurLaneQty.Value = getCurrLaneQty(Util.NVC(DvProductLot["LOTID"]));

                    if (dgProductResult.GetRowCount() > 0)
                        for (int i = 0; i < dgProductResult.GetRowCount(); i++)
                            DataTableConverter.SetValue(dgProductResult.Rows[i + dgProductResult.TopRows.Count].DataItem, "LANE_QTY", txtLaneQty.Value);

                    SumDefectQty();
                    dgProductResult.Refresh(false);
                }
                else
                {
                    txtVersion.Text = popup._ReturnRecipeNo;
                }
            }
        }

        #endregion

        #endregion

        private void txtInputQty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isChangeInputFocus == false && txtInputQty.Value > 0)
                txtInputQty_KeyDown(txtInputQty, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        private void txtInputQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _isChangeInputFocus = true;

                if (IsFinalProcess())
                {
                    if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                    {
                        decimal diffQty = Math.Abs(Util.NVC_Decimal(txtParentQty.Value) - Util.NVC_Decimal(txtInputQty.Value));

                        // 투입량의 제한률 이상 초과하면 입력 금지, 단 INPUT_OVER_RATE가 등록되어있지 않으면 SKIP [2017-03-02]
                        decimal inputRateQty = Util.NVC_Decimal(Util.NVC_Decimal(txtParentQty.Value) * _inputOverrate);

                        if (inputRateQty > 0 && diffQty > inputRateQty)
                        {
                            Util.MessageValidation("SFU3195", new object[] { Util.NVC(_inputOverrate * 100) + "%" });    // 투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
                            return;
                        }

                        //  차이수량(생산량-투입량) %1 만큼 길이초과로 등록 하시겠습니까?
                        Util.MessageConfirm("SFU1921", (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                if (dgWipReasonTop.Visibility.Equals(Visibility.Visible) &&  SetLossLot(dgWipReasonTop, _itemCodeLenExceed, diffQty) == false) return;
                                if (dgWipReasonBack.Visibility.Equals(Visibility.Visible) && SetLossLot(dgWipReasonBack, _itemCodeLenExceed, diffQty) == false) return;

                                _exceedLengthQty = diffQty;
                                _isChangeWipReason = true;

                                SetInputQty();

                                dgWipReasonTop.Refresh();
                                dgWipReasonBack.Refresh();

                                dgProductResult.Refresh(false);
                            }
                        }, new object[] { diffQty + txtUnit.Text });

                    }
                    else
                    {
                        // 그전 길이 초과수량 Clear
                        if (dgWipReasonTop.Visibility.Equals(Visibility.Visible)) SetLossLot(dgWipReasonTop, _itemCodeLenExceed, 0);
                        if (dgWipReasonBack.Visibility.Equals(Visibility.Visible)) SetLossLot(dgWipReasonBack, _itemCodeLenExceed, 0);

                        SetInputQty();
                    }

                }
                else
                {
                    if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                    {
                        Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                        return;
                    }
                    else
                    {
                        SetInputQty();
                    }
                }

                _isChangeInputFocus = false; // FOCUS 초기화

                dgProductResult.Refresh(false);
            }
        }

        private bool IsFinalProcess()
        {
            // 현재 작업중인 LOT이 마지막 공정인지 체크 [2017-02-16]
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = Util.NVC(DvProductLot["LOTID_PR"]);
            indata["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
            indata["PROCID"] = ProcessCode;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                return true;

            return false;
        }

        private string GetCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return Util.NVC(row["ATTRIBUTE1"]);
            }
            catch (Exception ex) { }

            return "";
        }

        public bool SetLossLot(C1DataGrid dg, string sItemCode, decimal iLossQty)
        {
            bool isLossValid = false;
            DataTable dt = (dg.ItemsSource as DataView).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "RESNQTY", iLossQty);
                    GetSumDefectQty(dg, false);
                    isLossValid = true;
                    break;
                }
            }

            if (isLossValid == false)
                Util.MessageValidation("SFU3196", new object[] { string.Equals(sItemCode, _itemCodeLenLack) ?
                    ObjectDic.Instance.GetObjectName("길이부족") : ObjectDic.Instance.GetObjectName("길이초과") }); //해당 MMD에 {%1}에 관련된 속성이 지정되지 않아 자동Loss를 등록할 수 없습니다.

            return isLossValid;
        }

        private void GetSumDefectQty(C1DataGrid dg, bool bRemainQty = true)
        {
            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            defectQty = SumDefectQty(dg, "DEFECT_LOT");
            LossQty = SumDefectQty(dg, "LOSS_LOT");
            chargeQty = SumDefectQty(dg, "CHARGE_PROD_LOT");

            totalSum = defectQty + LossQty + chargeQty;

            DataTable dt = (dgProductResult.ItemsSource as DataView).Table;

            for (int i = dgProductResult.TopRows.Count; i < dt.Rows.Count + dgProductResult.TopRows.Count; i++)
            {
                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_LOSS", LossQty);
                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "LOSSQTY", totalSum);

                DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "INPUTQTY")) - totalSum);

                laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "LANE_QTY"));

                if (txtLaneQty != null)
                    DataTableConverter.SetValue(dgProductResult.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(dgProductResult.Rows[i].DataItem, "GOODQTY")) * txtLaneQty.Value);
            }

            if (bRemainQty)
                SetParentRemainQty();
        }

    }
}
