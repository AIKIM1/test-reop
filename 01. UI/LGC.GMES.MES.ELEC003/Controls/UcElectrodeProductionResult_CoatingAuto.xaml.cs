/*************************************************************************************
 Created Date : 2020.09.14
      Creator : 정문교
   Decription : 전극 공정진척 - 생산실적 (E2000 Coating)
--------------------------------------------------------------------------------------
 [Change History]
  2021-11-23  김지은     SI       Fast Track 체크박스 추가
  2022-03-12  김지은     SI       TEST CUT 실적반영 기능 추가
  2022.06.19  윤기업     SI       RollMap 적용 
  2022.08.12  정문교     SI       TOP_LOSS_CODE 바인딩 오류 수정
  2022.11.25  방민재     SI       Loading량 측정값 자동계산시 음수값 입력 수정
  2023.01.13  윤기업     SI       롤맵 수불의 경우 TOP_LOSS 입력 제한 제거 및 롤맵수정 적용
  2023.02.02  윤기업     SI       롤맵 수불의 경우에도 TEST CUT 기능 적용
  2023.02.21  윤기업     SI       롤맵 수불 조회모드 추가에 따른 PARA 추가
  2023.02.24  윤기업     SI       TEST CUT DEFAULT LOSS 반영으로 변경
  2023.08.28  김도형              [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
  2024.02.16  김도형              [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
  2024.08.07  유명환              [E20240701-000286] MES 시스템의 전극 LOT별 로딩량 데이터 연동
  2025.04.15  이민형              [MI2_OSS_0179] Gird 입력 시 그리드 Cell 사이즈 자동 조정
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeProductionResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeProductionResult_CoatingAuto : UserControl
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

        // RollMap 대상여부
        private bool _isRollMapEquipment = false;
        public bool IsRollMapEquipment
        {
            get { return _isRollMapEquipment; }
            set { _isRollMapEquipment = value; }
        }

        // RollMap Lot 여부
        private bool _isRollMapLot = false;
        public bool IsRollMapLot
        {
            get { return _isRollMapLot; }
            set { _isRollMapLot = value; }
        }

        private bool _isRollMapSBL = false;
        public bool IsRollMapSBL
        {
            get { return _isRollMapSBL; }
            set { _isRollMapSBL = value; }
        }

        //황석동 2024.12.02 롤맵 코터 수불 - 불량/로스 변경 감지
        decimal _setDefectLen = 0;
        public decimal SetDefectLen
        {
            get { return _setDefectLen; }
        }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        bool _isResnCountUse = false;
        bool _isDupplicatePopup = false;
        bool _isTestCutUse = false;                           // Test Cut 사용여부

        // DataCollect 변경 여부
        bool _isChangeWipReason = false;                      // 불량/LOSS/물품청구
        bool _isChangeQuality = false;                        // 품질정보
        bool _isChangeMaterial = false;                       // 투입자재
        bool _isChangeRemark = false;                         // 특이사항
        bool _isTestCutApply = false;                         // Test Cut

        bool _isDefectLevel = false;

        private int _dgLVIndex1 = 0;
        private int _dgLVIndex2 = 0;
        private int _dgLVIndex3 = 0;
         
        private DataTable _dtRollmapEqptDectModifyTarget = new DataTable();    // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private string _ProcRollmapEqptDectModifyApplyFlag = "N";              // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선 

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));

        //private bool _isRollMapLot;

        public UcElectrodeProductionResult_CoatingAuto()
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

            // 잔량 안보이게 
            tbRemainQty.Visibility = Visibility.Collapsed;
            txtRemainQty.Visibility = Visibility.Collapsed;

            // TEST CUT 사용 사이트만 TEST CUT Tab 보이도록 함
            _isTestCutUse = _util.IsCommonCodeUse("TEST_CUT_HIST_AREA", LoginInfo.CFG_AREA_ID);
            if (_isTestCutUse)
                tiTestcut.Visibility = Visibility.Visible;
            else
                tiTestcut.Visibility = Visibility.Collapsed;
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
            chkFastTrack.IsChecked = false;

            txtInputQty.Value = 0;
            txtGoodQty.Value = 0;
            txtRemainQty.Value = 0;

            _isResnCountUse = false;
            _isDupplicatePopup = false;

            // DataCollect 변경 여부
            _isChangeWipReason = false;                      // 불량/LOSS/물품청구
            _isChangeQuality = false;                        // 품질정보
            _isChangeMaterial = false;                       // 투입자재
            _isChangeRemark = false;                         // 특이사항
            _isTestCutApply = false;                         // Test Cut
            bProductionUpdate = false;                       // 실적 저장, 불량/LOSS/물품청구 저장시 True

            // Test Cut 세부사항
            txtTCTopLossQty.Text = "0";
            txtTCBackLossQty.Text = "0";
            cboTCTopLoss.SelectedIndex = 0;
            cboTCBackLoss.SelectedIndex = 0;
            txtConfirmTime.Text = string.Empty;

            _dgLVIndex1 = 0;
            _dgLVIndex2 = 0;
            _dgLVIndex3 = 0;

            Util.gridClear(dgWipReasonTop);
            Util.gridClear(dgWipReasonBack);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgQualityTop);
            Util.gridClear(dgQualityBack);
            Util.gridClear(dgRemark);
            Util.gridClear(dgTestCut);
        }

        private void SetControlVisibility()
        {
            btnSaveRegDefectLane.Visibility = Visibility.Collapsed;
            btnSaveCarrier.Visibility = Visibility.Collapsed;

            tbOutCstID.Visibility = Visibility.Collapsed;
            txtOutCstID.Visibility = Visibility.Collapsed;

            if (ChkFastTrackOWNER())
            {
                tbFastTrack.Visibility = Visibility.Visible;
                chkFastTrack.Visibility = Visibility.Visible;
                chkFastTrack.IsChecked = CheckFastTrackLot();

            }
            else
            {
                tbFastTrack.Visibility = Visibility.Collapsed;
                chkFastTrack.Visibility = Visibility.Collapsed;
            }

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

            if (_isRollMapEquipment && (_isRollMapLot || _isRollMapSBL) && DvProductLot["WIPSTAT"].ToString() == "END")
            {
                if (_isRollMapSBL && _isRollMapLot == false)
                {
                    btnRollMapUpdate.Content = ObjectDic.Instance.GetObjectName("롤맵실적조회");
                }
                else
                {
                    btnRollMapUpdate.Content = ObjectDic.Instance.GetObjectName("롤맵수정");
                }
                        

                btnRollMapUpdate.Visibility = Visibility.Visible;
                btnSaveTestCut.Visibility = Visibility.Visible;
                btnAddMaterial.Visibility = Visibility.Collapsed;
                btnDeleteMaterial.Visibility = Visibility.Collapsed;
                btnSaveMaterial.Visibility = Visibility.Collapsed;
                dgMaterial.IsReadOnly = true;
            }
            else
            {
                btnRollMapUpdate.Visibility = Visibility.Collapsed;
                btnSaveTestCut.Visibility = Visibility.Visible;
                btnAddMaterial.Visibility = Visibility.Visible;
                btnDeleteMaterial.Visibility = Visibility.Visible;
                btnSaveMaterial.Visibility = Visibility.Visible;
                dgMaterial.IsReadOnly = false;
            }
        }

        private void SetPrivateVariable()
        {
            _isResnCountUse = IsAreaCommonCodeUse("RESN_COUNT_USE_YN", ProcessCode);
        }

        private void SetControlTestCut()
        {
            SelectLossCombo(cboTCTopLoss, "DEFECT_TOP");
            SelectLossCombo(cboTCBackLoss, "DEFECT_BACK");
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
                            ////////////// 코팅 공정은 강제 칼럼 Width 조정
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(86.5);

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

                            if (dataGrid.Columns["INPUT_VALUE_TYPE"].Index < e.Cell.Column.Index &&
                                dataGrid.Columns.Count > e.Cell.Column.Index && ((e.Cell.Row.Index - dataGrid.TopRows.Count)) == 2)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name,
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) -
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)));

                                if (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) !=
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
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

            if (txtGoodQty.Value <= 0)
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

        /// <summary>
        /// FastTrack 설정 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkFastTrack_Click(object sender, RoutedEventArgs e)
        {
            if (chkFastTrack.IsChecked == true)
            {
                // FastTrack Lot으로 등록 하시겠습니까?
                Util.MessageConfirm("SFU7354", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(true);
                        chkFastTrack.IsChecked = true;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = false;
                    }
                });
            }
            else
            {
                // FastTrack Lot 등록 취소 하시겠습니까?
                Util.MessageConfirm("SFU7355", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(false);
                        chkFastTrack.IsChecked = false;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = true;
                    }
                });
            }
        }

        /// <summary>
        /// RollMap 수불 PopUp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // RollMap 실적 수정 Popup Call
                CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                if (popupRollMapUpdate != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DvProductLot["PROCID"]);
                    Parameters[1] = EquipmentSegmentCode;
                    Parameters[2] = EquipmentCode;
                    Parameters[3] = Util.NVC(DvProductLot["LOTID"]);
                    Parameters[4] = Util.NVC(DvProductLot["WIPSEQ"]);
                    Parameters[5] = Util.NVC_Decimal(txtLaneQty.Value);
                    Parameters[6] = EquipmentName;
                    Parameters[7] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                    Parameters[8] = (_isRollMapSBL && _isRollMapLot == false) ? "Y" : "N";//Test Cut Visible false
                    //Parameters[9] = "N"; //Search Mode False
                    Parameters[9] = (_isRollMapSBL && _isRollMapLot == false) ? "Y" : "N";

                    popupRollMapUpdate.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);
                    
                    if (popupRollMapUpdate != null)
                    {
                        popupRollMapUpdate.ShowModal();
                        popupRollMapUpdate.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void PopupRollMapUpdate_Closed(object sender, EventArgs e)
        {
            ///////////////////// ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회
            CMM_RM_CT_RESULT popup = sender as CMM_RM_CT_RESULT;
            if (popup.IsUpdated)
            {
                SelectDefectSync(dgWipReasonTop, "DEFECT_TOP");
                SelectDefectSync(dgWipReasonBack, "DEFECT_BACK");

                SetResultInfo();
                SumDefectQty();
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

        private void dgWipReasonTop_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
  
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e != null && e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                       
                        if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                        
                        // RollMap용 수량 변경 금지 처리 
                        if (_isRollMapEquipment && _isRollMapLot && string.Equals(e.Cell.Column.Name, "RESNQTY") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선 
                        if ( string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y") && string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT") )
                        {
                            if (string.Equals(GetProcRollmapEqptDectModifyExceptFlag(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"))), "Y"))
                            {
                               e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                            }
                        }
                    }
                }
            }));
        }

        private void dgWipReasonTop_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        }
                    }
                }));
            }

        }

        private void dgWipReasonTop_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (string.Equals(e.Column.Name, "COUNTQTY") &&
                    !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    e.Cancel = true;

                if ((string.Equals(e.Column.Name, "COUNTQTY") || string.Equals(e.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Column.Name, "RESNQTY")) &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    e.Cancel = true;

                if (string.Equals(e.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    e.Cancel = true;

                // RollMap용 수량 변경 금지 처리
                if (_isRollMapEquipment && _isRollMapLot &&
                    string.Equals(e.Column.Name, "RESNQTY") &&
                    (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST") ||
                     string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y")))
                    e.Cancel = true;

                // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
                if ( string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y") && string.Equals(e.Column.Name, "RESN_TOT_CHK") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT") )
                {
                    if ( string.Equals(GetProcRollmapEqptDectModifyExceptFlag(Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RESNCODE"))), "Y") )
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void dgWipReasonTop_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                // Top Loss 기본 수량 변경 로직 적용 (단순에 차감, 증가분 만큼만 움직이도록 변경)
                // RollMap 수불의 경우 Top Loss 입력 제한 없음
                //if (_isRollMapEquipment && _isRollMapLot && string.Equals(dataGrid.Tag, "DEFECT_TOP") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WEB_BREAK_FLAG"), "Y"))
                if ((!_isRollMapEquipment || !_isRollMapLot) && string.Equals(dataGrid.Tag, "DEFECT_TOP") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WEB_BREAK_FLAG"), "Y"))
                {
                    decimal dTopLossQty = Util.NVC_Decimal(DvProductLot["INPUT_TOP_QTY"]) - Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                    decimal dRemainQty = SumDefectQty(dataGrid, e.Cell.Row.Index);
                    //decimal dWebBreakQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                    //                       Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "RESNQTY"));

                    if (Util.NVC_Decimal(e.Cell.Value) > (dTopLossQty - dRemainQty))
                    {
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", (dTopLossQty - dRemainQty));
                        if (_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y") >= 0)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y")].DataItem, "RESNQTY", 0);
                        }
                    }
                    else
                    {
                        if (_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y") >= 0)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[_util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y")].DataItem, "RESNQTY", (dTopLossQty - dRemainQty) - (Util.NVC_Decimal(e.Cell.Value)));
                        }
                    }
                }

                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    if (_isRollMapEquipment && _isRollMapLot)
                    {
                        SetWipReasonCommittedEdit(sender, e);
                    }


                    if (Util.NVC_Decimal(e.Cell.Value) == 0 &&
                        Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK")) == false)
                    {
                        SumDefectQty();
                        dgProductResult.Refresh(false);
                        return;
                    }

                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                            if (e.Cell.Row.Index != i)
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    }

                    if (Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY")) ==
                        Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_BACK_QTY")))
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK", true);

                    _isChangeWipReason = true;

                    SumDefectQty();
                    dgProductResult.Refresh(false);

                    dataGrid.AllColumnsWidthAuto();
                }
            }
        }

        private void dgWipReasonTop_PreviewKeyDown(object sender, KeyEventArgs e)
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
                    // RollMap 수정 불가 Cell 삭제 방지 
                    if (_isRollMapEquipment == true && _isRollMapLot && string.Equals(DataTableConverter.GetValue(dataGrid.CurrentCell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                    {
                        return;
                    }

                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        //DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

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
        private void btnSaveAllWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefect()) return;

            // 불량/LOSS/물품청구
            SaveDefect(dgWipReasonTop, true);
            SaveDefect(dgWipReasonBack, true);

            // RollMap Defect 좌표 반영
            if (_isRollMapEquipment && _isRollMapLot)
            {
                SaveDefectForRollMap(true);
            }

            // 품질정보
            SaveQuality(dgQualityTop, true);
            SaveQuality(dgQualityBack, true);

            // 투입자재
            SaveMaterial(dgMaterial, "A");

            // 특이사항
            if (!ValidationRemark(dgRemark)) // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
                return;
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

            // RollMap Defect 좌표 반영
            if (_isRollMapEquipment && _isRollMapLot)
            {
                SaveDefectForRollMap(true);
            }
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

                            if (string.Equals(e.Cell.Column.Name, "CLSS_NAME1"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
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

        private void dgQualityTop_CLCTVAL01_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void dgQualityBack_CLCTVAL01_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void dgQualityTop_CLCTVAL01_LostFocus(object sender, RoutedEventArgs e)
        {
            DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;

            if (sender.GetType().Name == "C1NumericBox")
            {
                C1NumericBox n = sender as C1NumericBox;
                
                string sinspitemid = drv["INSP_ITEM_ID"].GetString();
                string sclctval01  = drv["CLCTVAL01"].GetString();

                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (n != null && sinspitemid == "E2000-0001")
                {
                    if (!string.IsNullOrEmpty(sclctval01))
                    {
                        if (sclctval01.GetDecimal() < 0)
                        {
                            Util.MessageInfo("SFU8532");
                            n.Value = 0.00;
                        }
                    }
                }
                else if (n != null && sinspitemid == "SI016")
                {
                    if (!string.IsNullOrEmpty(sclctval01))
                    {
                        if (sclctval01.GetDecimal() < 0)
                        {
                            Util.MessageInfo("SFU8532");
                            n.Value = 0.00;
                        }
                    }
                }
            }
        }

        private void dgQualityBack_CLCTVAL01_LostFocus(object sender, RoutedEventArgs e)
        {
            DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;

            if (sender.GetType().Name == "C1NumericBox")
            {
                C1NumericBox n = sender as C1NumericBox;

                string sinspitemid = drv["INSP_ITEM_ID"].GetString();
                string sclctval01 = drv["CLCTVAL01"].GetString();

                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (n != null && sinspitemid == "E2000-0002")
                {
                    if (!string.IsNullOrEmpty(sclctval01))
                    {
                        if (sclctval01.GetDecimal() < 0)
                        {
                            Util.MessageInfo("SFU8532");
                            n.Value = 0.00;
                        }
                    }
                }
                else if (n != null && sinspitemid == "SI017")
                {
                    if (!string.IsNullOrEmpty(sclctval01))
                    {
                        if (sclctval01.GetDecimal() < 0)
                        {
                            Util.MessageInfo("SFU8532");
                            n.Value = 0.00;
                        }
                    }
                }
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

                            // TOP 로딩량 계산 오류 수정 (CLSS_NAME3 조건 추가)
                            // TOP 로딩량 계산 후 음수값 나올 시 팝업(측정값이 0보다 작습니다. 재확인 바랍니다.) 2022.11.25 고해선 사원 요청
                            bool isValueComplete = false;
                            //05.02 - 검사분류 코드 임시 하드코팅 처리
                            if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "E2000-0002"))
                            {
                                DataRow[] rows = (dgQualityTop.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}' AND CLSS_NAME3 = '{2}'",
                                    new object[] { "E2000-0001", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2"), DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                    {
                                        if ((input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]) < 0)
                                        {
                                            Util.MessageInfo("SFU8532");     // 측정값이 0보다 작습니다. 재확인 바랍니다.
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                        }

                                    }
                                    else
                                    {
                                        if (input - Convert.ToDouble(rows[0]["CLCTVAL01"]) < 0)
                                        {
                                            Util.MessageInfo("SFU8532");     // 측정값이 0보다 작습니다. 재확인 바랍니다.
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                        }

                                    }

                                    isValueComplete = true;
                                }

                            }
                            if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "SI017"))
                            {
                                DataRow[] rows = (dgQualityTop.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}' AND CLSS_NAME3 = '{2}'",
                                    new object[] { "SI016", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2"), DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                    {
                                        if((input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]) < 0)
                                        {
                                            Util.MessageInfo("SFU8532");     // 측정값이 0보다 작습니다. 재확인 바랍니다.
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (input - Convert.ToDouble(rows[0]["CLCTVAL01"]) < 0)
                                        {
                                            Util.MessageInfo("SFU8532");     // 측정값이 0보다 작습니다. 재확인 바랍니다.
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                        }
                                        
                                    }

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

        #region **투입자재
        private void dgMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentCell == null) return;

                if (!dg.CurrentCell.IsEditing)
                {
                    if (dg.CurrentCell.Column.Name.Equals("MTRLID"))
                    {
                        string sMTRLNAME;
                        string vMTRLID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MTRLID"));

                        if (vMTRLID.Equals(""))
                        {
                            return;
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("MTRLID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["MTRLID"] = vMTRLID;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_MTRLDESC", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count > 0)
                            {
                                sMTRLNAME = result.Rows[0]["MTRLDESC"].ToString();
                                DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, "MTRLDESC", sMTRLNAME);

                                DataTable dt2 = (dg.ItemsSource as DataView).Table;
                                Util.GridSetData(dg, dt2, FrameOperation, true);

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }

        }

        private void btnAddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["CHK"] = true;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        private void btnDeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs != null)
            {
                //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                Util.MessageConfirm("SFU1815", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                        SaveMaterial(dgMaterial, "D");
                });
            }

        }

        private void btnSaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMaterial(dgMaterial)) return;

            SaveMaterial(dgMaterial, "A");

        }
        #endregion

        #region **특이사항
        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 2) // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox  1->2
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
            else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 2) // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox  1->2
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

        // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
        private void dgRemark_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null)
                _isChangeRemark = true;

            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell.Column.Name.Contains("POST_HOLD"))
                {
                    if (e.Cell.Row.Index.Equals(0))
                    {
                        DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
                        DataRow dr = dt.Select().FirstOrDefault();
                        string sColumnName = e.Cell.Column.Name;
                        bool hold = Convert.ToBoolean(dr[sColumnName]);
                        if (dr != null)
                        {
                            foreach (DataRow dRow in dt.Rows)
                            {
                                dRow[sColumnName] = hold;
                            }
                        }
                        Util.GridSetData(dgRemark, dt, FrameOperation);
                    }
                }

            }
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemark(dgRemark)) return;

            // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
            DataTable dt = DataTableConverter.Convert(dgRemark.ItemsSource);
            if (dt.Select("POST_HOLD = 'True'").Length > 0 && dt != null)
            {
                //HOLD 하시겠습니까?
                Util.MessageConfirm("SFU1345", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                        SaveWipNote(dgRemark);
                    else
                        return;
                });
            }
            else
            {
                SaveWipNote(dgRemark);
            }
            //SaveWipNote(dgRemark);
        }
        #endregion

        #region **TEST CUT
        private void dgTestCut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;
        }

        private void btnSaveTestCut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTestCut(dgTestCut)) return;

            if (rdoTCApplyY.IsChecked == true)
            {
                // 선택한 항목을 Loss로 반영하시겠습니까?
                Util.MessageConfirm("SFU5173", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTestCut(dgTestCut);
                }
            });
            }
            else if (rdoTCApplyN.IsChecked == true)
            {
                // 전체 항목을 Loss 미반영 하시겠습니까?
                Util.MessageConfirm("SFU5174", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveTestCut(dgTestCut);
                    }
                });
            }
        }

        private void dgTestCut_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            List<DataRow> drList = dgTestCut.GetCheckedDataRow("CHK");

            //decimal dSumTopLoss = drList.Sum(row => row.Field<decimal>("DTL_TOP_LOSS"));
            //decimal dSumBackLoss = drList.Sum(row => row.Field<decimal>("DTL_BACK_LOSS"));
            // 2024.10.16. 김영국 DTL_TOP_LOSS, DTL_BACK_LOSS의 값이 소수점 반영으로 double Type으로 넘온다. 이에 형변환 변경.
            double dSumTopLoss = drList.Sum(row => row.Field<double>("DTL_TOP_LOSS"));
            double dSumBackLoss = drList.Sum(row => row.Field<double>("DTL_BACK_LOSS"));

            txtTCTopLossQty.Text = dSumTopLoss.ToString();
            txtTCBackLossQty.Text = dSumBackLoss.ToString();
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
            listAuth.Add(btnAddMaterial);                     // 투입자재 : 행추가
            listAuth.Add(btnDeleteMaterial);                  // 투입자재 : 삭제
            listAuth.Add(btnSaveMaterial);                    // 투입자재 : 저장
            listAuth.Add(btnSaveQuality);                     // 품질정보 : 저장
            listAuth.Add(btnSaveRemark);                      // 특이사항 : 저장
            listAuth.Add(btnSaveTestCut);                     // Test Cut : 저장

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductionResult()
        {
            SetControl();
            SetControlClear();
            SetControlVisibility();

            // LV Filter Clear
            ClearDefectLV();

            // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
            //공정 설비 불량코드 수정 제외 목록 정보           
            SetProcRollmapEqptDectModifyApplyFlag();
            if (string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y"))
            {
                SetRollmapEqptDectModifyTarget();  
            }             
             

            // 실적
            SetProductionResult();
            // 불량/LOSS/물품청구
            SelectDefect(dgWipReasonTop, "DEFECT_TOP");
            SelectDefect(dgWipReasonBack, "DEFECT_BACK");
            // 투입자재
            SetGridComboMaterial(dgMaterial.Columns["MTRLID"]);
            SelectInputMaterial();
            // 품질정보
            SelectQuality(dgQualityTop, "T");
            SelectQuality(dgQualityBack, "B");
            // 특이사항
            // SelectRemark(dgRemark); // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox : BindingWipNote()로 변경
            // Test Cut
            SetControlTestCut();
            SelectTestCut(dgTestCut);
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

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_V01", "RQSTDT", "RSLTDT", inTable);
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

        private void SetResultInfo()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DvProductLot["LOTID"];
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"];
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_INFO_CT", "INDATA", "RSLTDT", inTable);

                // 설비수량
                if (dtResult != null && dtResult.Rows.Count > 1)
                {
                    DataRow[] dr = dtResult.Select("ORDER_NO = 1");
                    Util.GridSetData(dgProductEquipment, dr.CopyToDataTable(), null);
                }
                else
                {
                    Util.GridSetData(dgProductEquipment, dtResult, null);
                }
                // 실적수량
                Util.GridSetData(dgProductResult, dtResult, null, false);

                // 특이사항  
                DataTable dtCopy = dtResult.Copy();
                BindingWipNote(dtCopy); // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox

                //_util.SetDataGridMergeExtensionCol(dgProductResult, new string[] { "LOTID", "OUT_CSTID", "PR_LOTID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex) { Util.MessageException(ex); }
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
                        // RollMap Defect 좌표 반영
                        if (_isRollMapEquipment && _isRollMapLot)
                        {
                            SaveDefectForRollMap(true);
                        }

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

        /// <summary>
        /// FastTrack 설정여부
        /// </summary>
        private void SetFastTrace(bool fasttrackFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FAST_TRACK_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LOTID"] = DvProductLot["LOTID"];
                if (fasttrackFlag == true)
                {
                    dr["FAST_TRACK_FLAG"] = "Y";
                }
                else
                {
                    dr["FAST_TRACK_FLAG"] = string.Empty;
                }
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_FAST_TRACK_LOT", "INDATA", null, dtRqst);

                if (fasttrackFlag)
                {
                    Util.MessageInfo("SFU1518");    //등록하였습니다.
                }
                else
                {
                    Util.MessageInfo("SFU1937");    //취소되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {
            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dr["ATTRIBUTE1"] = "Y";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count > 0)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// FastTrack 적용여부 체크
        /// </summary>
        private bool CheckFastTrackLot()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = DvProductLot["LOTID"];
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["FAST_TRACK_FLAG"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        public decimal GetDefectLen()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = DvProductLot["LOTID"];
            dr["WIPSEQ"] = DvProductLot["WIPSEQ"];
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_DEFECT_SUM", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult.Rows.Count != 0)
            {
                //return (decimal)dtResult.Rows[0]["DEFECT_LEN"];
                return dtResult.Rows[0]["DEFECT_LEN"].GetDecimal();
            }
            return 0;
        }

        public bool CheckDefectLen()
        {
            bool bResult = true;
            decimal newDefectLen = GetDefectLen();
            if (newDefectLen > 0 && SetDefectLen != newDefectLen)
            {
                SelectDefect(dgWipReasonTop, "DEFECT_TOP");
                SelectDefect(dgWipReasonBack, "DEFECT_BACK");     //차이 발생시에 자동으로 수량 갱신
                SetResultInfo();
                bResult = false;
            }

            return bResult;
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

                        if (_isRollMapEquipment && _isRollMapLot)
                        {
                            _setDefectLen = GetDefectLen();  //황석동 2024.12.01 롤맵 코터 수불 - 실적수량 변경 감지
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

        private void SelectDefectSync(C1DataGrid dg, string Resnposition = null)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dg, dtResult, null, true);

                SetCauseTitle(dg);
                SumDefectQty();

            }
            catch (Exception ex)
            {
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

        /// <summary>
        /// Defect 정보에 따른  RollMap 상대좌표 보정
        /// </summary>
        public void SaveDefectForRollMap(bool bAllSave = false)
        {
            try
            {
                if (dgWipReasonTop.GetRowCount() <= 0 && dgWipReasonBack.GetRowCount() <= 0) return;

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;
                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataRow["EQPTID"] = EquipmentCode;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataTable IndataTable = inDataSet.Tables.Add("IN_LOT");
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(Int32));

                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inDataRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                IndataTable.Rows.Add(inDataRow);

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DATACOLLECT_DEFECT_CT", "IN_EQP,IN_LOT", null, inDataSet);
                }
                catch (Exception ex) { Util.MessageException(ex); }

                if (!bAllSave)
                    Util.MessageInfo("SFU1270");     // 저장 되었습니다

            }
            catch (Exception ex)
            {
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

                string sBizName = "DA_QCA_SEL_WIPDATACOLLECT_LOT";
                if (IsAreaCommonCodeUse("ELEC_LOT_QCA_INFO_ADD_TWS", "USE_YN") && ProcessCode.Equals(Process.COATING))
                {
                    sBizName = "DA_QCA_SEL_PROC_CLCTITEM_ADD_TWS";
                }
             
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
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

        #region **투입자재
        /// <summary>
        /// 투입자재 조회
        /// </summary>
        private void SelectInputMaterial()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                if (_isRollMapEquipment && _isRollMapLot)
                {
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("WIPSEQ", typeof(string));
                }

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);

                if (_isRollMapEquipment && _isRollMapLot)
                {
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]); 
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                string bizRuleName;
                if (_isRollMapEquipment && _isRollMapLot)
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2_RM"; // DA_PRD_SEL_ROLLMAP_CONSUME_SLURRY
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2"; 
                }


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

                        Util.GridSetData(dgMaterial, bizResult, null, true);

                        // RollMap 대상 자재 변경 방지
                        if (_isRollMapEquipment && _isRollMapLot)
                        {
                            //dgMaterial.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;  // 숨김처리 06.06
                            //dgMaterial.Columns["STRT_PSTN"].Visibility = Visibility.Collapsed;  // 숨김처리 06.06
                            //dgMaterial.Columns["END_PSTN"].Visibility = Visibility.Collapsed;  // 숨김처리 06.06

                            dgMaterial.Columns["CHK"].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            dgMaterial.Columns["CHK"].Visibility = Visibility.Visible;
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
        /// 투입자재 데이터 그리드 콤보
        /// </summary>
        private void SetGridComboMaterial(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WOID"] = Util.NVC(DvProductLot["WOID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_COM_SEL_TB_SFC_WO_MTRL2", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(bizResult);
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
        /// 투입자재 추가,삭제 
        /// </summary>
        private void SaveMaterial(C1DataGrid dg, string PROC_TYPE, bool bAllSave = false)
        {
            try
            {
                DataRow[] dr = Util.gridGetChecked(ref dg, "CHK");

                if (dr == null || dr.Length == 0) return;

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = DvProductLot["LOTID"].ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = null;

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                InInput.Columns.Add("MTRLID", typeof(string));
                InInput.Columns.Add("INPUT_QTY", typeof(decimal));
                InInput.Columns.Add("PROC_TYPE", typeof(string));
                //InInput.Columns.Add("INPUT_SEQNO", typeof(Int32));
                InInput.Columns.Add("INPUT_SEQNO", typeof(string)); // 2024.11.06. 김영국 - INPUT SEQ Type변경 int32 -> string

                DataTable dt = ((DataView)dg.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals(""))
                        {
                            newRow = InInput.NewRow();
                            newRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                            newRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            newRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                            newRow["PROC_TYPE"] = PROC_TYPE;
                            //newRow["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                            newRow["INPUT_SEQNO"] = row["INPUT_SEQNO"]; // 2024.11.06. 김영국 - INPUT SEQ Type변경 int32 -> string
                            InInput.Rows.Add(newRow);
                        }
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "IINDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SelectInputMaterial();
                        _isChangeMaterial = false;

                        if (!bAllSave)
                        {
                            if (PROC_TYPE == "D")
                                Util.MessageInfo("SFU1273");     // 삭제되었습니다.
                            else
                                Util.MessageInfo("SFU3532");     // 저장 되었습니다
                        }

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

                //2021.09.09 김대근 : 이력카드에서 WIP_NOTE가 조회되려면 "|"이 반드시 추가되어야 함.
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int row = 1; row < dt.Rows.Count; row++)
                {
                    // 1. 특이사항
                    sRemark.Append(dt.Rows[row]["REMARK"]);
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    if (dg.Rows[0].Visibility == Visibility.Visible)
                    {
                        sRemark.Append(Util.NVC(dt.Rows[0]["REMARK"]));
                    }
                    sRemark.Append("|");

                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(dt.Rows[row]["LOTID"]);
                    newRow["WIP_NOTE"] = sRemark;
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

                        #region [HOLD 처리]  // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
                        string[] HOLD_CHK = new string[1];
                        HOLD_CHK[0] = "POST_HOLD_001";

                        DataSet inDataSet = new DataSet();
                        DataTable dtpostHold = (dg.ItemsSource as DataView).Table;

                        DataRow inDataRow = null;
                        DataTable IndataTable = inDataSet.Tables.Add("INDATA");
                        IndataTable.Columns.Add("WIPSEQ", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        inDataRow = IndataTable.NewRow();
                        inDataRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                        inDataRow["USERID"] = LoginInfo.USERID;

                        IndataTable.Rows.Add(inDataRow);

                        DataRow inDataRow2 = null;
                        DataTable IndataTable2 = inDataSet.Tables.Add("IN_LOT");
                        IndataTable2.Columns.Add("LOTID", typeof(string));
                        IndataTable2.Columns.Add("HOLD_CHK_ITEM_CODE", typeof(string));
                        IndataTable2.Columns.Add("CHK_RSLT", typeof(string));
                        IndataTable2.Columns.Add("HOLD", typeof(string));
                        IndataTable2.Columns.Add("NOTE", typeof(string));
                        for (int k = 0; k < dtpostHold.Rows.Count; k++)
                        {
                            if (!string.Equals(Util.NVC(dtpostHold.Rows[k]["LOTID"]), ObjectDic.Instance.GetObjectName("공통특이사항")))
                            {
                                for (int i = 0; i < HOLD_CHK.Length; i++)
                                {
                                    inDataRow2 = IndataTable2.NewRow();
                                    inDataRow2["LOTID"] = Util.NVC(dtpostHold.Rows[k]["LOTID"]);
                                    inDataRow2["HOLD_CHK_ITEM_CODE"] = HOLD_CHK[i].ToString();
                                    inDataRow2["CHK_RSLT"] = "N";
                                    inDataRow2["HOLD"] = string.Equals(Util.NVC(dtpostHold.Rows[k]["POST_HOLD"]), "True") ? "Y" : "N";

                                    if (dg.Rows[0].Visibility == Visibility.Visible)
                                        inDataRow2["NOTE"] = Util.NVC(dtpostHold.Rows[k]["REMARK"]) + "|" + Util.NVC(dtpostHold.Rows[0]["REMARK"]);
                                    else
                                        inDataRow2["NOTE"] = Util.NVC(dtpostHold.Rows[k]["REMARK"]);

                                    IndataTable2.Rows.Add(inDataRow2);
                                }
                            }
                        }
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_POST_HOLD", "INDATA,IN_LOT", null, inDataSet);
                        dg.EndEdit(true);
                        #endregion


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

        #region **TEST CUT
        /// <summary>
        /// TEST CUT 존재 및 실적 반영 확정 여부 확인
        /// </summary>
        public bool CheckTestCut()
        {
            try
            {
                //TEST CUT 이력 존재하는데 LOSS 반영 여부 미확정 일 때
                if (!_isTestCutApply)
                {
                    Util.MessageValidation("SFU5172");     // Test Cut 이력이 존재합니다. Loss 반영 여부를 선택해 주세요.
                    tcDataCollect.SelectedItem = tiTestcut;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }

        /// <summary>
        /// Test Cut Loss 반영 여부 저장
        /// </summary>
        /// <param name="dg"></param>
        private void SaveTestCut(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataSet ds = new DataSet();

                DataTable dtData = new DataTable("IN_DATA");
                dtData.Columns.Add("OUTPUT_LOTID", typeof(string));
                dtData.Columns.Add("WIPSEQ", typeof(decimal));
                dtData.Columns.Add("TOP_LOSS_CODE", typeof(string));
                dtData.Columns.Add("BACK_LOSS_CODE", typeof(string));
                dtData.Columns.Add("REMARKS", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("CNFM_USERID", typeof(string));
                dtData.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtData.NewRow();
                dr["OUTPUT_LOTID"] = DvProductLot["LOTID"];
                dr["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0)
                    dr["TOP_LOSS_CODE"] = Util.GetCondition(cboTCTopLoss);
                else
                    dr["TOP_LOSS_CODE"] = string.Empty;
                if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0)
                    dr["BACK_LOSS_CODE"] = Util.GetCondition(cboTCBackLoss);
                else
                    dr["BACK_LOSS_CODE"] = string.Empty;        // dr["TOP_LOSS_CODE"] = string.Empty;

                dr["USERID"] = LoginInfo.USERID;
                dr["CNFM_USERID"] = LoginInfo.USERID;
                dr["EQPTID"] = EquipmentCode;
                dtData.Rows.Add(dr);

                DataTable dtCut = new DataTable("IN_CUT");
                dtCut.Columns.Add("CUT_RPT_DTTM", typeof(DateTime));
                dtCut.Columns.Add("LOSS_APPLY_FLAG", typeof(string));

                for (int i = 2; i < dg.Rows.Count; i++)
                {
                    dr = dtCut.NewRow();
                    dr["CUT_RPT_DTTM"] = dg.GetValue(i, "CUT_RPT_DTTM");
                    dr["LOSS_APPLY_FLAG"] = (rdoTCApplyN.IsChecked == true) ? "N" : (dg.GetValue(i, "CHK").ToString().Equals("True") || dg.GetValue(i, "CHK").ToString().Equals("1")) ? "Y" : "N";
                    dtCut.Rows.Add(dr);
                }

                ds.Tables.Add(dtData);
                ds.Tables.Add(dtCut);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TEST_CUT_APPLY_LOSS", "IN_DATA,IN_CUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isTestCutApply = true;

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
                        SelectTestCut(dg);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Loss Combo 조회
        /// </summary>
        private void SelectLossCombo(C1ComboBox cb, string sResnposition)
        {
            try
            {
                cb.ItemsSource = null;

                const string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_ELEC";
                string[] arrColumn = { "LANGID", "AREAID", "LOTID", "ACTID", "RESNPOSITION" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Util.NVC(DvProductLot["LOTID"]), "LOSS_LOT", sResnposition };
                string selectedValueText = "RESNCODE";
                string displayMemberText = "RESNNAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cb, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, selectedValue: "SELECT");
            }
            catch (Exception ex)
            {
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
                txtGoodQty.Format = sFormatted;
                txtRemainQty.Format = sFormatted;

                for (int i = 0; i < dgProductEquipment.Columns.Count; i++)
                    if (dgProductEquipment.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgProductEquipment.Columns[i].Tag, "N"))
                        // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgProductEquipment.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgProductEquipment.Columns[i]).Format = sFormatted;

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

                for (int i = 0; i < dgMaterial.Columns.Count; i++)
                    if (dgMaterial.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgMaterial.Columns[i].Tag, "N"))
                        if (string.Equals(txtUnit.Text, "EA"))
                            ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = "F1";
                        else
                            ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = sFormatted;
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

        private string GetUnitFormatted(object obj)
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

        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private void SetProcRollmapEqptDectModifyApplyFlag()
        { 
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "COM_TYPE_CODE";
            sCmCode = "COATER_DEFECT_LOSS_CHARGE_REG_ALL"; // COATER_DEFECT_LOSS_CHARGE_REG_ALL

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
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ProcRollmapEqptDectModifyApplyFlag = "Y";   

                }
                else
                {
                    _ProcRollmapEqptDectModifyApplyFlag = "N";
                }

                return;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                return;
            }
        }
        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private void SetRollmapEqptDectModifyTarget()
        {
             
            string sCodeType;
            string sCmCode;
            string[] sAttribute = { ProcessCode };

            sCodeType = "COATER_DEFECT_LOSS_CHARGE_REG_ALL";  // COATER_DEFECT_LOSS_CHARGE_REG_ALL
            sCmCode = null;

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
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                _dtRollmapEqptDectModifyTarget = null;

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _dtRollmapEqptDectModifyTarget = dtResult;
                }
              

                return  ;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return  ;
            }
        }

        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private string GetProcRollmapEqptDectModifyExceptFlag(string ResnCode)
        {
            string sResnCodeFlag = "Y";
            try
            {
                if (_dtRollmapEqptDectModifyTarget != null && _dtRollmapEqptDectModifyTarget.Rows.Count > 0)
                {
                    for (int i = 0; i < _dtRollmapEqptDectModifyTarget.Rows.Count; i++)
                    {
                        if ( string.Equals( Util.NVC(ResnCode), Util.NVC(_dtRollmapEqptDectModifyTarget.Rows[i]["ATTR2"].ToString()) ) )  // ResnCode
                        {
                            sResnCodeFlag = "N";
                            break;
                        }
                        else
                        {
                            sResnCodeFlag = "Y";
                        }
                    }
                } else
                {
                    sResnCodeFlag = "Y";
                }

                return sResnCodeFlag;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return sResnCodeFlag;
            }
        }

        // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
        private void BindingWipNote(DataTable dt)
        {
            try
            {
                if (dgRemark.GetRowCount() > 0) return;

                 #region [Hold 정보 조회]  // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
                 DataTable IndataTable = new DataTable();
                 IndataTable.Columns.Add("LOTID", typeof(string));
                 IndataTable.Columns.Add("CUT_ID", typeof(string));
                
                 DataRow Indata = IndataTable.NewRow();
                 Indata["LOTID"] = DvProductLot["LOTID"];
                 Indata["CUT_ID"] = Util.NVC(DvProductLot["CUT_ID"]);
                 IndataTable.Rows.Add(Indata);
                
                 DataTable _postholdDT = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_POST_HOLD_ELTR", "INDATA", "RSLTDT", IndataTable);
                
                 DataTable dtRemark = new DataTable();
                 dtRemark.Columns.Add("LOTID", typeof(string));
                 dtRemark.Columns.Add("POST_HOLD", typeof(string));
                 dtRemark.Columns.Add("REMARK", typeof(string));
                
                 var remark = new List<string>();
                 DataRow dRow = null;
                 for (int i = 0; i < _postholdDT.Rows.Count; i++)
                 {
                     dRow = dtRemark.NewRow();
                     dRow["LOTID"] = Util.NVC(_postholdDT.Rows[i]["LOTID"]);
                     dRow["POST_HOLD"] = Util.NVC(_postholdDT.Rows[i]["POST_HOLD"]);
                
                     if (!string.IsNullOrEmpty(Util.NVC(_postholdDT.Rows[i]["HOLD_DESC"]).Split('|')[0]))
                         remark.Add(Util.NVC(_postholdDT.Rows[i]["HOLD_DESC"]).Split('|')[0]);
                     else
                         remark.Add(GetRemarkData(Util.NVC(_postholdDT.Rows[i]["LOTID"])).Split('|')[0]);
                
                     dRow["REMARK"] = remark[0];
                     dtRemark.Rows.Add(dRow);
                     remark.Clear();
                 }
                 #endregion
                
                 DataRow inDataRow = null;
                 inDataRow = dtRemark.NewRow();
                 inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");
                
                 if (dt.Rows.Count > 0)
                 {
                     string[] sWipNote = GetRemarkData(Util.NVC(dt.Rows[0]["LOTID"])).Split('|');
                     if (sWipNote.Length > 1)
                         inDataRow["REMARK"] = sWipNote[1];
                 }
                 dtRemark.Rows.InsertAt(inDataRow, 0);
                
                 Util.GridSetData(dgRemark, dtRemark, FrameOperation);

                // SLITTER가 아닌 경우 공통특이사항은 숨김
                dgRemark.Rows[0].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                
                 dgRemark.Rows[0].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Test Cut 이력 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectTestCut(C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("OUTPUT_LOTID", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUTPUT_LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                dt.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_TEST_CUT_HIST", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dg, dtResult, FrameOperation);
                    DataRow dr = dtResult.Rows[0];
                    if (dr["LOSS_APPLY_CNFM_FLAG"].Equals("Y"))
                    {
                        _isTestCutApply = true;
                        txtTCTopLossQty.Text = (string.IsNullOrEmpty(dr["SUM_TOP_LOSS_QTY"].ToString())) ? "0" : dr["SUM_TOP_LOSS_QTY"].ToString();
                        txtTCBackLossQty.Text = (string.IsNullOrEmpty(dr["SUM_BACK_LOSS_QTY"].ToString())) ? "0" : dr["SUM_BACK_LOSS_QTY"].ToString();
                        txtConfirmTime.Text = dr["CNFM_DTTM"].ToString();
                        cboTCTopLoss.SelectedValue = (string.IsNullOrEmpty(dr["TOP_LOSS_CODE"]?.ToString())) ? "SELECT" : dr["TOP_LOSS_CODE"].ToString();
                        cboTCBackLoss.SelectedValue = (string.IsNullOrEmpty(dr["BACK_LOSS_CODE"]?.ToString())) ? "SELECT" : dr["BACK_LOSS_CODE"].ToString();

                        DataRow[] drN = dtResult.Select("LOSS_APPLY_FLAG = 'N'");
                        if (dtResult.Rows.Count == drN.Length)
                            rdoTCApplyN.IsChecked = true;
                        else
                            rdoTCApplyY.IsChecked = true;
                    }
                    else
                    {
                        rdoTCApplyY.IsChecked = true;
                    }
                }
                else
                {
                    rdoTCApplyY.IsChecked = true;
                    _isTestCutApply = true; //Test Cut 이력 없을 경우 true 처리
                }
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
            txtLaneQty.Value = Convert.ToInt16(sLaneQty);

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

            if (string.Equals(DvProductLot["FINAL_CUT_FLAG"], "Y"))
                chkFinalCut.IsChecked = true;

            if (string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
            {
                btnSaveAllWipReason.IsEnabled = true;
                btnSaveWipReason.IsEnabled = true;
                btnSaveTestCut.IsEnabled = true;
            }
            else
            {
                btnSaveAllWipReason.IsEnabled = false;
                btnSaveWipReason.IsEnabled = false;
                btnSaveTestCut.IsEnabled = false;
            }

            SetResultInfo();

            // UNIT별로 FORMAT
            SetUnitFormatted();
        }

        private decimal SumDefectQty(C1DataGrid dataGrid, int iRow)
        {
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (iRow != i)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                            if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y"))
                                if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));

            return dSumQty;
        }

        private void SumDefectQty()
        {
            if (!string.Equals(DvProductLot["WIPSTAT"], Wip_State.END))
                return;

            decimal dTopInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
            decimal dBackInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
            decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProductResult.Rows[dgProductResult.TopRows.Count].DataItem, "LANE_QTY"));
            decimal dTopDefectQty = GetSumDefectQty(dgWipReasonTop, "DEFECT_LOT", "TOP");
            decimal dTopLossQty = GetSumDefectQty(dgWipReasonTop, "LOSS_LOT", "TOP");
            decimal dTopChargeProdQty = GetSumDefectQty(dgWipReasonTop, "CHARGE_PROD_LOT", "TOP");
            decimal dTopTotalQty = dTopDefectQty + dTopLossQty + dTopChargeProdQty;
            decimal dBackDefectQty = GetSumDefectQty(dgWipReasonBack, "DEFECT_LOT", "BACK");
            decimal dBackLossQty = GetSumDefectQty(dgWipReasonBack, "LOSS_LOT", "BACK");
            decimal dBackChargeProdQty = GetSumDefectQty(dgWipReasonBack, "CHARGE_PROD_LOT", "BACK");
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
            txtGoodQty.Value = Convert.ToDouble(dBackInputQty - dBackTotalQty);                             // 양품량
            txtRemainQty.Value = Convert.ToDouble(dBackInputQty - (dBackInputQty - dBackTotalQty));         // 잔량
        }

        private decimal GetSumDefectQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
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

        private void SetWipReasonCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    // 전체 체크가 없어서 주석 처리
                    /* for (int i = 0; i < dataGrid.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                            if (e.Cell.Row.Index != i)
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    } */

                    // 재 조건 조정 배분 로직 추가 [2021-07-27]
                    DataRow[] row = DataTableConverter.Convert(dataGrid.ItemsSource).Select("DFCT_CODE='" +
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_CODE")) + "' AND PRCS_ITEM_CODE='GRP_QTY_DIST'");

                    if (row.Length > 0)
                    {
                        decimal iCurrQty = 0;
                        decimal iResQty = 0;
                        decimal iInitQty = row.Sum(g => g.Field<decimal>("FRST_AUTO_RSLT_RESNQTY"));

                        decimal iDistQty = DataTableConverter.Convert(dataGrid.ItemsSource).AsEnumerable()
                                                .Where(g => g.Field<string>("DFCT_CODE") == Util.NVC(row[0]["DFCT_CODE"]) &&
                                                                   g.Field<string>("PRCS_ITEM_CODE") != "GRP_QTY_DIST" &&
                                                                   g.Field<string>("RESNCODE") != Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE")))
                                                .Sum(g => Util.NVC_Decimal(g.Field<string>("RESNQTY")));

                        if (iInitQty < (iDistQty + Util.NVC_Decimal(e.Cell.Value)))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", iInitQty - iDistQty);

                        for (int i = 0; i < dataGrid.Rows.Count; i++)
                        {
                            iCurrQty = 0;
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "DFCT_CODE"), row[0]["DFCT_CODE"]) &&
                                string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST"))
                            {
                                iCurrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                if (iCurrQty <= (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty))
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                    iResQty += iCurrQty;
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", iCurrQty - (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty));
                                    iResQty = iDistQty + Util.NVC_Decimal(e.Cell.Value);
                                }
                            }
                        }
                    }
                    
                }

                dataGrid.EndEdit();
            }
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

        private bool ValidationMaterial(C1DataGrid dg)
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            DataRow[] drs = Util.gridGetChecked(ref dg, "CHK");

            if (drs == null)
            {
                Util.MessageValidation("SFU1662");     // 선택한 자재가 없습니다.
                return false;
            }

            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU1984");     // 투입자재 LOT ID를 입력하세요.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationRemark(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }

            // [E20240130-000386] [ESWA PI] 20240129_ElectrodeProcessProgressRemarksHOLDcheckBox
            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
            DataRow[] dr = dt.Select("POST_HOLD = 'True'");

            foreach (DataRow dRow in dr)
            {
                if (string.IsNullOrEmpty(Util.NVC(dRow["REMARK"]).Split('|')[0]))
                {
                    Util.MessageValidation("SFU1993");  //특이사항을 입력하세요
                    return false;
                }
            }


            return true;
        }

        private bool ValidationTestCut(C1DataGrid dg)
        {
            // Loss 반영 – YES : 체크박스 선택유무 확인, Loss Code 선택 확인 후 체크한 항목을 Loss로 반영하시겠습니까? 메시지
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }

            // Loss 반영
            if (rdoTCApplyY.IsChecked == true)
            {
                List<DataRow> drList = dg.GetCheckedDataRow("CHK");
                if (drList.Count == 0)
                {
                    Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                    return false;
                }

                if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0 && cboTCTopLoss.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                    return false;
                }

                if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0 && cboTCBackLoss.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                    return false;
                }
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

    }
}
