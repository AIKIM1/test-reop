/*************************************************************************************
 Created Date : 2017.08.29
      Creator : 
   Decription : X-Ray 재작업 공정진척
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;


namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_008 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UcAssyCommand UcAssyCommand { get; set; }
        public UcAssyShift UcAssyShift { get; set; }

        //public C1DataGrid DgProductLot { get; set; }
        //public C1DataGrid DgDefect { get; set; }
        //public C1DataGrid DgDefectDetail { get; set; }

        private string _processCode;
        private bool _isSmallType;
        private bool _isLoaded = false;

        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private struct PreviewValues
        {
            private string _previewTray;
            public string PreviewCurentInput;
            private string _previewOutBox;

            public PreviewValues(string tray, string currentInput, string box)
            {
                _previewTray = tray;
                PreviewCurentInput = currentInput;
                _previewOutBox = box;
            }
        }

        private PreviewValues _previewValues = new PreviewValues("", "", "");

        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY002_008()
        {
            InitializeComponent();
        }
        
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeUserControls();
            SetComboBox();
            SetEventInUserControls();
        }

        private void SetEventInUserControls()
        {
            if (UcAssyCommand != null)
            {
                UcAssyCommand.ButtonStart.Click += ButtonStart_Click;
                UcAssyCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcAssyCommand.ButtonConfirm.Click += ButtonConfirm_Click;
            }

            if (UcAssyShift != null)
            {
                UcAssyShift.ButtonShift.Click += ButtonShift_Click;
            }
        }

        private void InitializeUserControls()
        {
            UcAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            UcAssyShift = grdShift.Children[0] as UcAssyShift;

            if (UcAssyCommand != null)
            {
                UcAssyCommand.ProcessCode = _processCode;
                UcAssyCommand.IsSmallType = _isSmallType;
                UcAssyCommand.SetButtonVisibility();
            }

            if (UcAssyShift != null)
            {
                UcAssyShift.ProcessCode = _processCode;
                UcAssyShift.SetControlProperties();
            }
        }

        private void SetComboBox()
        {
            SetEquipmentCombo(cboEquipmentAssy);
        }

        private void SetMountPositionComboBox()
        {
            CommonCombo combo = new CommonCombo();

            String[] sFilter1 = { cboEquipmentAssy.SelectedValue.GetString(), "PROD" };
            combo.SetCombo(cboWaitPalletPstnID, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            combo.SetCombo(cboInputPalletPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.XRAY_REWORK;
            _isSmallType = false;

            ApplyPermissions();
            Initialize();

            if (_isLoaded == false)
            {
                if (!cboEquipmentAssy.SelectedValue.GetString().Equals("SELECT"))
                    btnSearch_Click(null, null);
            }
            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            GetProductLot();
            GetEqptWrkInfo();
        }

        private void cboEquipmentAssy_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                ClearControls();
                UcAssyShift.ClearShiftControl();

                SetMountPositionComboBox();
                // 설비 선택 시 자동 조회 처리
                if (cboEquipmentAssy != null && cboEquipmentAssy.SelectedIndex > 0 && cboEquipmentAssy.Items.Count > cboEquipmentAssy.SelectedIndex)
                {
                    if (cboEquipmentAssy.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                ////Grid Data Binding 이용한 Background 색 변경
                //if (e.Cell.Row.Type == DataGridRowType.Item)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                //    {
                //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                //        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                //    {
                //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                //        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                //    }
                //}
            }));
        }

        private void dgOutPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgOutPallet);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllUnChecked(dgOutPallet);
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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //IsChangeDefect = true;
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

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgProductLot_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            /*
            if (sender == null || !CommonVerify.HasDataGridRow(dgProductLot))
                return;

            if (dgProductLot.SelectedItem != null)
            {
                DataTableConverter.SetValue(dgProductLot.SelectedItem, "CHK", true);
                int idx = dgProductLot.SelectedIndex;

                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", idx == i);
                }

                ClearControls();
                //상세 정보 조회
                ProdListClickedProcess(idx);
            }
            */
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
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
                        //row 색 바꾸기
                        dgProductLot.SelectedIndex = idx;

                        //ClearControls();

                        // 생산영역 Control Clear
                        ClearDataCollectControls();
                        // 완성Pallet 영역 Clear
                        ClearOutPalletControls();

                        //상세 정보 조회
                        ProdListClickedProcess(idx);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirm()) return;

            // 불량, 투입자재, 투입 반제품 저장여부 체크
            if (!ValidationDataCollect()) return;

            Util.MessageConfirm("SFU4219", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            ASSY002_008_RUNSTART popupRunStart = new ASSY002_008_RUNSTART { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = Util.NVC(cboEquipmentAssy.SelectedValue);
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY002_008_RUNSTART popup = sender as ASSY002_008_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcAssyShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcAssyShift.TextWorker.Tag);
            parameters[6] = Util.NVC(cboEquipmentAssy.SelectedValue);
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);
            grdMain.Children.Add(popupShiftUser);
            popupShiftUser.BringToFront();
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void TbDefectDetail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

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

                    if (!ValidationCurrAutoInputLot())
                        return;

                    string positionId = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string positionName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

                    object[] parameters = new object[2];
                    parameters[0] = positionName;
                    parameters[1] = txtCurrInLotID.Text.Trim();

                    //%1 위치에 %2 을 투입 하시겠습니까?
                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            InputAutoLot(txtCurrInLotID.Text.Trim(), positionId);
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
            if (!ValidationCurrentInputCancell()) return;

            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CurrentInputCancel();
                }
            });
        }

        private void btnCurrInReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCurrInReplace()) return;
            CMM_ASSY_XRAY_REWORK_PAN_REPLACE popReplace = new CMM_ASSY_XRAY_REWORK_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            object[] parameters = new object[6];
            parameters[0] = cboEquipmentAssy.SelectedValue;
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[3] = ProcessCode;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            parameters[5] = true;
            C1WindowExtension.SetParameters(popReplace, parameters);

            popReplace.Closed += new EventHandler(popReplace_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popReplace.ShowModal()));

            //grdMain.Children.Add(popReplace);
            //popReplace.BringToFront();
        }

        private void popReplace_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_XRAY_REWORK_PAN_REPLACE pop = sender as CMM_ASSY_XRAY_REWORK_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetCurrInList();
            }
            //this.grdMain.Children.Remove(pop);
        }

        private void btnCurrInComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCurrInComplete()) return;

            //투입완료 처리 하시겠습니까?
            Util.MessageConfirm("SFU1972", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputCompleteXRay();
                }
            });
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

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void cboWaitPalletPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }


        private void btnWaitPalletSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationWaitPalletSearch()) return;
            GetWaitPallet();
        }

        private void btnWaitPalletInPut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWaitPalletInput()) return;
            //투입 하시겠습니까?
            Util.MessageConfirm("SFU1248", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WaitPalletInPut();
                }
            });
        }

        private void dgWaitPallet_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
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
                                    if (checkBox != null && (dg?.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null && dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null && checkBox.IsChecked.HasValue && !(bool)checkBox.IsChecked))
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

        private void dgWaitPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgWaitPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void cboInputPalletPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetInputPallet();
        }

        private void btnInputPalletSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationInputPalletSearch()) return;
            GetInputPallet();
        }

        private void btnInputPalletCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputPalletCancel()) return;

            //투입취소 하시겠습니까?
            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputPalletCancel();
                }
            });
        }

        private void btnPalletCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletCreate()) return;

            //Pallet를 생성 하시겠습니까?
            Util.MessageConfirm("SFU4006", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletCreate();
                }
            });
        }

        private void btnPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletDelete()) return;

            //Pallet를 삭제 하시겠습니까?
            Util.MessageConfirm("SFU4008", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletDelete();
                }
            });
        }

        private void btnPalletPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletPrint()) return;

            //발행하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletPrint();
                }
            });
            
        }

        private void dgOutPallet_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgOutPallet.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    int rowIndex = 0;
                    foreach (var item in dgOutPallet.Rows)
                    {
                        if (drv["CHK"].GetString() == "True" || drv["CHK"].GetString() == "1")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }

                    //dgOutPallet.EndEdit();
                    //dgOutPallet.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletQty_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                || e.Key == Key.Back))
            {
                e.Handled = true;
            }
        }

        private void btnSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveWipHistory()) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveWipHistory();
                }
            });
        }
        #endregion

        #region Mehod

        private void GetProductLot()
        {
            try
            {
                string selectedLot = string.Empty;

                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");

                    if (rowIdx >= 0)
                    {
                        selectedLot = DataTableConverter.GetValue(dgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    }
                }

                ClearControls();
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_XR";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        dgProductLot.ItemsSource = DataTableConverter.Convert(result);

                        if (string.IsNullOrEmpty(selectedLot))
                        {
                            if (result.Rows.Count > 0)
                            {
                                int iRowRun = _util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (dgProductLot.TopRows.Count > 0)
                                        iRowRun = dgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;
                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;
                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInList();
                            }
                        }
                        else
                        {
                            int idx = _util.GetDataGridRowIndex(dgProductLot, "LOTID", selectedLot);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;
                                ProdListClickedProcess(idx);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                dgProductLot.CurrentCell = dgProductLot.GetCell(idx, dgProductLot.Columns.Count - 1);
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInList();
                            }
                        }
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ProdListClickedProcess(int rowIdx)
        {
            try
            {
                if (rowIdx < 0 || !_util.GetDataGridCheckValue(dgProductLot, "CHK", rowIdx)) return;

                string lotId = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIdx].DataItem, "LOTID"));
                string wipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIdx].DataItem, "WIPSEQ"));

                // 생산실적, 불량/LOSS/물청
                SetDefectDetail();
                SetResultDetailControl(dgProductLot.Rows[rowIdx].DataItem);
                GetDefectInfo(lotId, wipSeq);

                //투입처리
                GetCurrInList();
                //대기Pallet 리스트
                GetWaitPallet();
                //투입Pallet 리스트        
                GetInputPallet();
                //완성Pallet 리스트        
                GetOutPallet();

                //실적 상세 계산
                CalculateDefectQty();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ClearControls()
        {
            try
            {
                // 작업대상 영역 Control Clear
                Util.gridClear(dgProductLot);
                // 생산영역 Control Clear
                ClearDataCollectControls();
                // 완성Pallet 영역 Clear
                ClearOutPalletControls();
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ClearDataCollectControls()
        {
            Util.gridClear(dgDefectDetail);
            Util.gridClear(dgDefect);
            Util.gridClear(dgCurrIn);
            Util.gridClear(dgWaitPallet);
            Util.gridClear(dgInputPallet);

            txtDefectQty.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtProdId.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtWorkOrder.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtWorkMinute.Text = string.Empty;
            txtRemark.Document.Blocks.Clear();
            txtCurrInLotID.Text = string.Empty;
            txtWaitPalletId.Text = string.Empty;
            txtInputPalletId.Text = string.Empty;
        }

        private void ClearOutPalletControls()
        {
            Util.gridClear(dgOutPallet);
            txtPalletQty.Text = string.Empty;
            txtNote.Text = string.Empty;
        }

        private void SelectAllDataCollect()
        {
            if (TabDefectDetail.Visibility == Visibility.Visible)
            {
                //생산실적
            }
            if (TabDefect.Visibility == Visibility.Visible)
            {
                //불량/LOSS/물청
            }
            if (TabCurrIn.Visibility == Visibility.Visible)
            {
                //자재투입
                GetCurrInList();
            }
            if (TabWaitPallet.Visibility == Visibility.Visible)
            {
                //대기Pallet
                GetWaitPallet();
            }
            if (TabInputPallet.Visibility == Visibility.Visible)
            {
                //투입Pallet
                GetInputPallet();
            }
        }

        private void SetResultDetailControl(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtWorkOrder.Text = rowview["WOID"].GetString();
            txtLotId.Text = rowview["LOTID"].GetString();
            txtProdId.Text = rowview["PRODID"].GetString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].GetString();
            txtEndTime.Text = rowview["WIPDTTM_ED"].GetString();

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

            txtRemark.AppendText(rowview["REMARK"].GetString());

            if (string.IsNullOrEmpty(rowview["CALDATE"].GetString()))
            {
                dtpCaldate.Text = DateTime.Now.ToShortDateString();
                dtpCaldate.SelectedDateTime = DateTime.Now;
            }
            else
            {
                dtpCaldate.Text = rowview["CALDATE"].GetString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(rowview["CALDATE"].GetString());
            }

        }

        private void ConfirmProcess()
        {

            try
            {
                SaveDefectBeforeConfirm();

                ShowLoadingIndicator();
                dgDefectDetail.EndEdit();
                dgDefectDetail.EndEditRow(true);

                const string bizRuleName = "BR_PRD_REG_END_LOT_ASSY_XR";

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("IFMODE", typeof(string));
                dtIndata.Columns.Add("EQPTID", typeof(string));
                dtIndata.Columns.Add("SHIFT", typeof(string));
                dtIndata.Columns.Add("WIPDTTM_ED", typeof(string));
                dtIndata.Columns.Add("WIPNOTE", typeof(string));
                dtIndata.Columns.Add("WRK_USER_NAME", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));
                dtIndata.Columns.Add("PROD_LOTID", typeof(string));
                dtIndata.Columns.Add("INPUT_QTY", typeof(decimal));
                dtIndata.Columns.Add("OUTPUT_QTY", typeof(decimal));
                dtIndata.Columns.Add("RESNQTY", typeof(decimal));

                DataRow dr = dtIndata.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                dr["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                dr["WIPNOTE"] = new TextRange(txtRemark.Document.ContentStart, txtRemark.Document.ContentEnd).Text;
                dr["WRK_USER_NAME"] = UcAssyShift.TextWorker.Text;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["INPUT_QTY"] = 0;
                dr["OUTPUT_QTY"] = 0;
                dr["RESNQTY"] = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                dtIndata.Rows.Add(dr);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
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
                        btnSearch_Click(btnSearch, null);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();
                // 원각 초소형 공통사용 BizRule
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_ASSY_XR";
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    GetProductLot();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void SaveDefectBeforeConfirm()
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
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                    newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDefectDetail()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));
            inDataTable.Columns.Add("REINPUTQTY", typeof(int));
            inDataTable.Columns.Add("BOXQTY", typeof(int));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;
            dtRow["REINPUTQTY"] = 0;
            dtRow["BOXQTY"] = 0;
            inDataTable.Rows.Add(dtRow);

            dgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        private void GetDefectInfo(string lotId, string wipSeq)
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCurrInList()
        {
            try
            {

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                // 메인 LOT이 없는 경우 disable 처리..
                if (rowIndex < 0)
                {
                    btnCurrInCancel.IsEnabled = false;
                    btnCurrInComplete.IsEnabled = false;
                }
                else
                {
                    btnCurrInCancel.IsEnabled = true;
                    btnCurrInComplete.IsEnabled = true;
                }

                
                const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgCurrIn.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation,true);

                        if (!string.IsNullOrEmpty(_previewValues.PreviewCurentInput))
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

                        dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;

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
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetWaitPallet()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WAIT_PALLET_LIST_XR";
                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("EQPTID", typeof(string));
                dtInTable.Columns.Add("PALLETID", typeof(string));
                dtInTable.Columns.Add("PRODID", typeof(string));
                DataRow dr = dtInTable.NewRow();
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["PALLETID"] = txtWaitPalletId.Text;
                dr["PRODID"] = null;

                dtInTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtInTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    dgWaitPallet.ItemsSource = DataTableConverter.Convert(searchResult);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputPallet()
        {
            try
            {

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                if (rowIndex < 0)
                    return;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_INPUT_PALLET_LIST_XR";

                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("PROD_LOTID", typeof(string));
                dtInTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtInTable.Columns.Add("PALLETID", typeof(string));
                
                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["EQPT_MOUNT_PSTN_ID"] = string.IsNullOrEmpty(cboInputPalletPstnID.SelectedValue.GetString()) ? null : cboInputPalletPstnID.SelectedValue.GetString();
                dr["PALLETID"] = txtInputPalletId.Text;
                dtInTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtInTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    //dgInputPallet.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgInputPallet, searchResult, null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputPalletCancel()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_ASSY_XR";
                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["USERID"] = LoginInfo.USERID;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                inTable.Rows.Add(dr);
                
                DataTable inlotTable = ds.Tables.Add("INLOT");
                inlotTable.Columns.Add("INPUT_SEQNO", typeof(string));
                inlotTable.Columns.Add("INPUT_LOTID", typeof(string));
                inlotTable.Columns.Add("WIPQTY", typeof(decimal));
                inlotTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                for (int i = 0; i < dgInputPallet.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgInputPallet, "CHK", i))
                    {
                        DataRow row = inlotTable.NewRow();
                        row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "INPUT_SEQNO"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "PALLETID"));
                        row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "INPUT_QTY")).GetDecimal();
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        inlotTable.Rows.Add(row);
                    }
                }
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");
                    GetProductLot();
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WaitPalletInPut()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_ASSY_XR";
                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_LOTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["EQPT_LOTID"] = null;
                inTable.Rows.Add(dr);

                DataTable inlotTable = ds.Tables.Add("IN_INPUT");
                inlotTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inlotTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inlotTable.Columns.Add("PRODID", typeof(string));
                inlotTable.Columns.Add("INPUT_LOTID", typeof(string));
                inlotTable.Columns.Add("INPUT_QTY", typeof(decimal));

                for (int i = 0; i < dgWaitPallet.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgWaitPallet, "CHK", i))
                    {
                        DataRow row = inlotTable.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = cboWaitPalletPstnID.SelectedValue;
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPallet.Rows[i].DataItem, "PRODID"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPallet.Rows[i].DataItem, "PALLETID"));
                        row["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitPallet.Rows[i].DataItem, "WIPQTY")).GetDecimal();
                        inlotTable.Rows.Add(row);
                    }
                }
                string xmlText = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");
                    GetProductLot();

                }, ds);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetOutPallet()
        {
            try
            {

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                if (rowIndex < 0)
                    return;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_OUT_PALLET_LIST_XR";

                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("PROD_LOTID", typeof(string));
                DataRow dr = dtInTable.NewRow();
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dtInTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtInTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    //dgOutPallet.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgOutPallet, searchResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalculateDefectQty()
        {
            SumDefectQty();

            if (!CommonVerify.HasDataGridRow(dgDefectDetail)) return;

            double goodQty = GetGoodQty().GetDouble();
            double inputQty = GetInputQty().GetDouble();
            double defect = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
            double loss = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
            double chargeprd = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
            double defectQty = defect + loss + chargeprd;

            //양품수량
            DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
            //투입수량
            DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", inputQty);
            //생산수량
            DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", goodQty + defectQty);

            if (Math.Abs(inputQty - (goodQty + defectQty)) > 0)
            {
                txtDefectQty.Text = (inputQty - (goodQty + defectQty)).ToString("##,###");
                txtDefectQty.FontWeight = FontWeights.Bold;
                txtDefectQty.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                txtDefectQty.Text = "0";
                txtDefectQty.FontWeight = FontWeights.Normal;
                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                if (convertFromString != null)
                    txtDefectQty.Foreground = new SolidColorBrush((Color)convertFromString);
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

                if (CommonVerify.HasTableRow(dtTmp))
                {
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString()));
                }
                else
                {
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private decimal GetGoodQty()
        {
            decimal returnGoodQty = 0;

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            if (rowIndex < 0) return 0;

            const string bizRuleName = "DA_PRD_SEL_OUT_PALLET_LIST_XR";
            DataTable dtInTable = new DataTable("RQSTDT");
            dtInTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = dtInTable.NewRow();
            dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            dtInTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                returnGoodQty = searchResult.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY"));
            }
            else
            {
                returnGoodQty = 0;
            }
            return returnGoodQty;
        }

        private decimal GetInputQty()
        {
            decimal returnInputQty = 0;

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            if (rowIndex < 0) return 0;

            ShowLoadingIndicator();
            const string bizRuleName = "DA_PRD_SEL_INPUT_PALLET_LIST_XR";

            DataTable dtInTable = new DataTable("RQSTDT");
            dtInTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = dtInTable.NewRow();
            dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            dtInTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                returnInputQty = searchResult.AsEnumerable().Sum(s => s.Field<Decimal>("INPUT_QTY"));
            }
            else
            {
                returnInputQty = 0;
            }
            return returnInputQty;
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
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                    newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
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

                if (bMsgShow)
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveWipHistory()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "BR_ACT_REG_SAVE_LOT";

            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            dr["PROD_VER_CODE"] = null;
            dr["SHIFT"] = UcAssyShift.TextShift.Tag;
            dr["WIPNOTE"] = new TextRange(txtRemark.Document.ContentStart, txtRemark.Document.ContentEnd).Text;
            dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);
            dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);
            //dr["LANE_PTN_QTY"] = 0;
            //dr["LANE_QTY"] = 0;
            dr["PROD_QTY"] = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
            dr["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(dr);

            DataSet ds = new DataSet();
            ds.Tables.Add(inDataTable);
            string xml = ds.GetXml();

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    GetProductLot();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            });
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable indataTable = new DataTable("RQSTDT");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("SHOPID", typeof(string));
                indataTable.Columns.Add("AREAID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["EQPTID"] = Util.NVC(cboEquipmentAssy.SelectedValue);
                indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                indata["PROCID"] = _processCode;

                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcAssyShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcAssyShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcAssyShift.TextShiftEndTime.Text))
                                {
                                    UcAssyShift.TextShiftDateTime.Text = UcAssyShift.TextShiftStartTime.Text + " ~ " + UcAssyShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcAssyShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcAssyShift.TextWorker.Text = string.Empty;
                                    UcAssyShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcAssyShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcAssyShift.TextShift.Tag = string.Empty;
                                    UcAssyShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcAssyShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcAssyShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PalletCreate()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CREATE_PALLET_ASSY_XR";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CELL_QTY", typeof(decimal));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["USERID"] = LoginInfo.USERID;
                dr["WIPNOTE"] = txtNote.Text;
                dr["SHIFT"] = UcAssyShift.TextShift.Tag;// UcAssyShift.TextShift.Tag;
                dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);// Util.NVC(UcAssyShift.TextWorker.Tag);
                dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);//Util.NVC(UcAssyShift.TextWorker.Text);
                dr["CELL_QTY"] = txtPalletQty.Text.GetDecimal();
                inDataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetProductLot();
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

        private void PalletDelete()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_DELETE_PALLET_ASSY_XR";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTID"] = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                inDataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetProductLot();
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

        private void PalletPrint()
        {
            try
            {
                
                //string palletId = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                const string bizRuleName = "DA_PRD_SEL_PALLET_RUNCARD_DATA_ASSY_XR";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PALLET_ID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["PALLET_ID"] = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        CMM_ASSY_PALLET_PRINT poopupPallet = new CMM_ASSY_PALLET_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[3];
                        parameters[0] = result;
                        parameters[1] = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                        parameters[2] = _processCode;
                        C1WindowExtension.SetParameters(poopupPallet, parameters);
                        poopupPallet.Closed += new EventHandler(poopupPallet_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupPallet.ShowModal()));
                    }
                    else
                    {
                        //데이터가 없습니다.
                        Util.MessageValidation("SFU1498");
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupPallet_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PALLET_PRINT popup = sender as CMM_ASSY_PALLET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetOutPallet();
            }
        }

        private void InputAutoLot(string inputLot, string positionId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_ASSY_XR";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WS();
                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
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
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
                GetCurrInList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputCompleteXRay()
        {
            const string bizRuleName = "BR_PRD_REG_END_INPUT_LOT_ASSY_XR";
            ShowLoadingIndicator();

            DataSet ds = new DataSet();
            DataTable inDataTable = _bizDataSet.GetBR_PRD_REG_END_INPUT_LOT_WS();

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID").GetString()))
                {
                    DataRow dr = inDataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                    dr["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                    dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                    dr["EQPT_LOTID"] = string.Empty;
                    inDataTable.Rows.Add(dr);
                }
            }

            ds.Tables.Add(inDataTable);

            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetCurrInList();
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CurrentInputCancel()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT";
                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("IFMODE", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));
                dtIndata.Columns.Add("EQPTID", typeof(string));
                dtIndata.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["USERID"] = LoginInfo.USERID;
                dr["EQPTID"] = cboEquipmentAssy.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dtIndata.Rows.Add(dr);

                DataTable dtInlot = ds.Tables.Add("IN_INPUT");
                dtInlot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtInlot.Columns.Add("INPUT_LOTID", typeof(string));
                dtInlot.Columns.Add("WIPNOTE", typeof(string));
                dtInlot.Columns.Add("INPUT_SEQNO", typeof(Int64));
                
                for (int i = 0; i < dgCurrIn.GetRowCount(); i++)
                {
                    if (_util.GetDataGridCheckValue(dgCurrIn, "CHK", i))
                    {
                        DataRow row = dtInlot.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID").GetString();
                        row["INPUT_LOTID"] = DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID").GetString();
                        //row["WIPNOTE"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).GetDecimal();
                        dtInlot.Rows.Add(row);
                    }
                }
                string xmlText = ds.GetXml();
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");
                    GetProductLot();
                }, ds);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetConfirmDate()
        {
            string confirmDate;

            const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO";
            string prodLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = prodLotId;
            dr["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).Trim()))
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]).GetString();
            }
            else
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).GetString();
            }

            return confirmDate;
        }

        private bool CheckInputHistoryInfo(string lotId, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST_CNT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
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

        private bool CheckOutTrayInfo(string lotid, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_CNT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["EQPTID"] = cboEquipmentAssy.SelectedValue;
                newRow["PROD_LOTID"] = lotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
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

        private bool ValidationSearch()
        {
            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationConfirm()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }
            /*
            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }
            */
            if (string.IsNullOrEmpty(UcAssyShift.TextShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetString() == "0")
            {
                //양품 수량을 확인하십시오.
                Util.MessageValidation("SFU1722");
                return false;
            }
            return true;
        }

        private bool ValidationDataCollect()
        {

            //if (UcAssyProduction.IsChangeDefect)
            //{
            //    //불량 정보를 저장하세요.     
            //    Util.MessageValidation("SFU1577");
            //    return false;
            //}

            //if (UcAssyProduction.IsChangeMaterial)
            //{
            //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 정보를 저장하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            //    Util.MessageValidation("SFU2975");
            //    return false;
            //}

            //if (UcAssyProduction.IsChangeProduct)
            //{
            //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 저장하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            //    Util.MessageValidation("SFU2971");
            //    return false;
            //}

            if (!string.IsNullOrEmpty(UcAssyShift.TextShiftEndTime.Text))
            {
                DateTime shiftEndDateTime = Convert.ToDateTime(UcAssyShift.TextShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();
                int result = DateTime.Compare(shiftEndDateTime, systemDateTime);

                if (result < 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                    UcAssyShift.ClearShiftControl();
                    return false;
                }
            }

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationRunStart()
        {
            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationCancelRun()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");

            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            string lotid = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            string wipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            // 투입 이력 정보 존재여부 확인            
            if (CheckInputHistoryInfo(lotid, wipSeq))
            {
                Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                return false;
            }

            // 완성 이력 정보 존재여부 확인
            if (CheckOutTrayInfo(lotid, wipSeq))
            {
                Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationSaveWipHistory()
        {

            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgDefectDetail))
            {
                Util.MessageValidation("SFU3552");
                return false;
            }

            return true;
        }

        private bool ValidationInputPalletCancel()
        {

            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgInputPallet, "CHK");
            if (rowIndex < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationPalletCreate()
        {
            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK");
            if (rowIndex < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (string.IsNullOrEmpty(txtPalletQty.Text) || txtPalletQty.Text.GetDecimal() < 1)
            {
                //양품 수량을 확인하십시오.
                Util.MessageValidation("SFU1722");
                return false;
            }

            return true;
        }

        private bool ValidationPalletDelete()
        {
            if (cboEquipmentAssy.SelectedIndex < 0 || cboEquipmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPallet, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPalletPrint()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPallet, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationWaitPalletInput()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgWaitPallet, "CHK");
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

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationCurrAutoInputLot()
        {
            if (string.IsNullOrEmpty(txtCurrInLotID.Text.Trim()))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if(_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK") < 0)
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

            return true;
        }

        private bool ValidationCurrInReplace()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID").GetString()))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationCurrInComplete()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if(string.IsNullOrEmpty(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID").GetString()))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID, _processCode };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            //////////////////// 설비가 N건인 경우 Select 추가
            if (cboEquipmentAssy.Items.Count > 1 || cboEquipmentAssy.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int index = 0;

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                    if (drIndex.Length > 0)
                    {
                        index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                }

                cbo.SelectedIndex = index;
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

        private void ApplyPermissions()
        {
            var ucAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            if (ucAssyCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    ucAssyCommand.ButtonStart,
                    ucAssyCommand.ButtonCancel,
                    ucAssyCommand.ButtonConfirm
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }





        #endregion

    }

}
