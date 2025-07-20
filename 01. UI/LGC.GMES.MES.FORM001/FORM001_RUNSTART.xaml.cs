/*************************************************************************************
 Created Date : 2017.12.07
      Creator : 
   Decription : 파활성화 후공정 파우치 : 작업시작
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        private Util _util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _processName = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _divisionCode = string.Empty;
        private string _inputType = string.Empty;
        private CheckBoxHeaderType _inBoxHeaderType;
        private DataTable _inboxList;

        private bool _finalExternal;
        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public string InspectorCode { get; set; }
        public string ProdLotId { get; set; }
        public string ProdCartId { get; set; }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                object[] parameters = C1WindowExtension.GetParameters(this);
                _processCode = parameters[0] as string;
                _processName = parameters[1] as string;
                _equipmentCode = parameters[2] as string;
                _equipmentName = parameters[3] as string;
                _divisionCode = parameters[4] as string;
                _equipmentSegmentCode = parameters[5] as string;

                if (_processName != null) txtProcess.Text = _processName;
                if (_equipmentName != null) txtEquipment.Text = _equipmentName;

                InitializeUserControls();
                SetControl();
                SetControlProcess();
                SetControlVisibility();
                _load = false;
            }

            if (cboFormWorkType.IsEnabled)
            {
                cboFormWorkType.Focus();
            }
            else
            {
                txtStartTray.Focus();
            }
        }

        private void InitializeUserControls(bool IsAll = true)
        {
            Util.gridClear(dgAssyLot);
            Util.gridClear(dgProductionInbox);

            if (IsAll)
                txtStartTray.Text = string.Empty;

            txtAssyLotID.Text = string.Empty;
            //cboLottype.IsEnabled = false;
            //cboLottype.SelectedIndex = 0;
        }
        private void SetControl()
        {
            // 작업구분 : 반품재작업 제외
            CommonCombo combo = new CommonCombo();
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _equipmentSegmentCode };
            combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE_LINE", sFilter: sFilter);

            if (_processCode.Equals(Process.CELL_BOXING) || _processCode.Equals(Process.CELL_BOXING_RETURN) || _processCode.Equals(Process.CELL_BOXING_RETURN_RMA))
            {
                // 포장공정은 제외(Cell포장, 물류반품, RMA반품)
            }
            else
            {
                DataTable dt = DataTableConverter.Convert(cboFormWorkType.ItemsSource);
                dt.Select("CBO_CODE = '" + "FORM_WORK_RT" + "'").ToList<DataRow>().ForEach(row => row.Delete());
                cboFormWorkType.ItemsSource = dt.Copy().AsDataView();
            }
            cboFormWorkType.SelectedValueChanged += cboFormWorkType_SelectedValueChanged;

            // 불량그룹
            SetDefectGroupCombo();
            cboDefectGroup.SelectedValueChanged += cboDefectGroup_SelectedValueChanged;

            rdoTray.IsChecked = true;
            txtAssyLotID.IsEnabled = true;
            btnAssyLot.IsEnabled = true;

            rdoTray.Checked += rdoTray_Checked;
            rdoPallet.Checked += rdoPallet_Checked;

            //// Lot type
            //SetLotType();
            ////cboLottype.SelectedValueChanged += cboLottype_SelectedValueChanged;
        }
        private void SetControlProcess()
        {
        }

        private void SetControlVisibility()
        {
            if (cboFormWorkType.Items == null || cboFormWorkType.Items.Count == 0)
                return;

            if (_processCode.Equals(Process.PolymerSideTaping) ||
                _processCode.Equals(Process.CELL_BOXING) ||
                _processCode.Equals(Process.CELL_BOXING_RETURN) ||
                _processCode.Equals(Process.CELL_BOXING_RETURN_RMA))
            {
                rdoPallet.IsChecked = true;
                rdoTray.IsEnabled = false;
                rdoPallet.IsEnabled = false;
            }
            else  if (ChkProcess("FORM_INSP_PROCID"))
            {
                //// 최종외관 공정 작업구분을 재선별(FORM_WORK_RS)로 고정 -> 정상, 재선별 선택 가능하게
                //cboFormWorkType.SelectedValue = "FORM_WORK_RS";
                //cboFormWorkType.IsEnabled = false;

                cboFormWorkType.SelectedValueChanged -= cboFormWorkType_SelectedValueChanged;

                DataTable dt = DataTableConverter.Convert(cboFormWorkType.ItemsSource);
                dt.Select("CBO_CODE <> 'SELECT' And CBO_CODE <> 'FORM_WORK_NO' And CBO_CODE <> 'FORM_WORK_RS'").ToList<DataRow>().ForEach(row => row.Delete());
                cboFormWorkType.ItemsSource = dt.Copy().AsDataView();

                cboFormWorkType.SelectedValueChanged += cboFormWorkType_SelectedValueChanged;

                rdoPallet.IsChecked = true;
                rdoTray.IsEnabled = false;
                rdoPallet.IsEnabled = false;

                _finalExternal = true;
            }
            else if (ChkProcess("FORM_REWORK_PROCID"))
            {
                // 양품화 공정 작업구분을 재작업(FORM_WORK_RW)로 고정
                cboFormWorkType.SelectedValue = "FORM_WORK_RW";
                cboFormWorkType.IsEnabled = false;
                rdoPallet.IsChecked = true;
                rdoTray.IsEnabled = false;
                rdoPallet.IsEnabled = false;
            }

            // WO 공정 체크후 WO TYPE이 REWORK (일반재작업, 재선별작업), REWORK이 아니면 (정상작업:FORM_WORK_NO)
            DataTable dtProcessWO = ChkWotkOrder();
            if (dtProcessWO != null && dtProcessWO.Rows.Count > 0)
            {
                if (Util.NVC(dtProcessWO.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("WO"))
                {
                    cboFormWorkType.SelectedValueChanged -= cboFormWorkType_SelectedValueChanged;

                    DataTable dt = DataTableConverter.Convert(cboFormWorkType.ItemsSource);
                    if (Util.NVC(dtProcessWO.Rows[0]["REWORK_YN"]).Equals("Y"))
                    {
                        dt.Select("CBO_CODE <> 'SELECT' And CBO_CODE <> 'FORM_WORK_RW' And CBO_CODE <> 'FORM_WORK_RS'").ToList<DataRow>().ForEach(row => row.Delete());
                        cboFormWorkType.ItemsSource = dt.Copy().AsDataView();
                    }
                    else
                    {
                        dt.Select("CBO_CODE <> 'SELECT' And CBO_CODE <> 'FORM_WORK_NO'").ToList<DataRow>().ForEach(row => row.Delete());
                        cboFormWorkType.ItemsSource = dt.Copy().AsDataView();
                        cboFormWorkType.SelectedValue = "FORM_WORK_NO";
                    }

                    cboFormWorkType.SelectedValueChanged += cboFormWorkType_SelectedValueChanged;
                }
            }

            // DSF 공정인 경우 작업 모드 활성화
            if (_processCode.Equals(Process.PolymerDSF))
            {
                grdMode.Visibility = Visibility.Visible;
            }
            else
            {
                grdMode.Visibility = Visibility.Collapsed;
            }

        }

        private void SetDataGridColumnVisibility(string WipQltyType)
        {
            if (WipQltyType.Equals("G"))
            {
                cboDefectGroup.IsEnabled = false;
                dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("InBox ID");

                // 양품
                dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
                dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Collapsed;

                dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;
                dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Collapsed;
            }
            else
            {
                cboDefectGroup.IsEnabled = true;
                dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("불량그룹LOT");

                // 불량
                dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Visible;

                dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Visible;
            }
        }


        #endregion

        #region [작업구분 변경 cboFormWorkType_SelectedValueChanged]
        private void cboFormWorkType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeUserControls();
            txtStartTray.Focus();
        }
        #endregion

        #region Tray, Pallet RadioButton Click
        private void rdoTray_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            InitializeUserControls();

            dgAssyLot.Columns["CSTID"].Visibility = Visibility.Visible;
            dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
            dgAssyLot.Columns["CELL_QTY"].Visibility = Visibility.Collapsed;

            txtAssyLotID.IsEnabled = true;
            btnAssyLot.IsEnabled = true;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작 Tray");
            txtStartTray.Focus();

            // Lot 정보 
            gdLot.IsEnabled = false;
        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            InitializeUserControls();

            dgAssyLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
            dgAssyLot.Columns["CELL_QTY"].Visibility = Visibility.Visible;

            txtAssyLotID.IsEnabled = false;
            btnAssyLot.IsEnabled = false;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작대차INBOX");
            txtStartTray.Focus();

            // Lot 정보 
            gdLot.IsEnabled = true;
        }
        #endregion

        #region [시작 Tray KeyDown시 조립 Lot 정보 조회]
        private void txtStartTray_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtStartTray_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 시작 Tray, inbox, 대차 투입가능 체크
                CheckPallet();
            }
        }
        #endregion

        #region [dgLot 색상 dgAssyLot_LoadedCellPresenter]
        private void dgAssyLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("FORM_WRK_TYPE_NAME"))
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(120);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
            }));
        }
        #endregion

        #region [LotType 변경 cboLottype_SelectedValueChanged]
        private void cboLottype_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgAssyLot.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgAssyLot.Rows[0].DataItem, "LOTTYPE", cboLottype.SelectedValue ?? cboLottype.SelectedValue.ToString());
                DataTableConverter.SetValue(dgAssyLot.Rows[0].DataItem, "LOTYNAME", cboLottype.Text);
            }

        }
        #endregion

        #region AssyLot 팝업
        private void txtAssyLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AssyLotPopUp();
            }
        }

        private void btnAssyLot_Click(object sender, RoutedEventArgs e)
        {
            AssyLotPopUp();
        }

        private void AssyLotPopUp()
        {
            if (txtAssyLotID.Text.Length < 4)
            {
                // Lot ID는 4자리 이상 넣어 주세요.
                Util.MessageValidation("SFU3450");
                return;
            }

            FORM001_ASSYLOT popupAssyLot = new FORM001_ASSYLOT { FrameOperation = this.FrameOperation };

            C1WindowExtension.SetParameters(popupAssyLot, null);
            popupAssyLot.AssyLot = txtAssyLotID.Text;
            popupAssyLot.PolymerYN = "Y";

            popupAssyLot.Closed += new EventHandler(popupAssyLot_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupAssyLot);
                    popupAssyLot.BringToFront();
                    break;
                }
            }
        }

        private void popupAssyLot_Closed(object sender, EventArgs e)
        {
            Util.gridClear(dgAssyLot);

            FORM001_ASSYLOT popup = sender as FORM001_ASSYLOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtAssyLotID.Text = popup.AssyLot;
                GetAssyLotInfoPopup();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

            txtAssyLotID.Focus();
        }
        #endregion

        #region 조립Lot CHeck
        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Point pnt = e.GetPosition(null);
            //C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            //if (cell != null)
            //{
            //    if (!cell.Column.Name.Equals("CHK"))
            //        return;

            //    if (!_inputType.Equals("C"))
            //        return;

            //    DataTable dt = new DataTable();
            //    dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

            //    dt.Select().ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            //    dt.Rows[cell.Row.Index]["CHK"] = 1;
            //    // 완성 Inbox 조회
            //    SetGridProductInboxList(Util.NVC(dt.Rows[cell.Row.Index]["CTNR_ID"]), Util.NVC(dt.Rows[cell.Row.Index]["ASSY_LOTID"]));

            //    dt.AcceptChanges();
            //    Util.GridSetData(dgAssyLot, dt, null, true);
            //}

        }
        private void dgAssyLot_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (!e.Cell.Column.Name.Equals("CHK"))
                return;

            if (!_inputType.Equals("C"))
                return;

            // 완성 Inbox 조회
            SetGridProductInboxList();
        }

        #endregion

        #region Inbox Header CHeck
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgProductionInbox;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        #endregion

        #region Inbox 체크
        private void dgProductionInbox_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (!e.Cell.Column.Name.Equals("CHK"))
                return;

            // Inbox Check, UNCheck시 조립Lot 체크 재설정
            DataTable dtAssy = DataTableConverter.Convert(dgAssyLot.ItemsSource);
            dtAssy.Select().ToList<DataRow>().ForEach(r => r["CHK"] = 0);

            DataRow[] drCheckInbox = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");
            if (drCheckInbox.Length != 0)
            {
                DataTable dtCheckInbox = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1").CopyToDataTable<DataRow>();
                DataTable dtAssyDup = dtCheckInbox.DefaultView.ToTable(true, "ASSY_LOTID");

                foreach (DataRow dr in dtAssyDup.Rows)
                {
                    dtAssy.Select("ASSY_LOTID = '" + dr["ASSY_LOTID"] + "'").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                }
            }

            dtAssy.AcceptChanges();
            Util.GridSetData(dgAssyLot, dtAssy, null, true);
        }
        #endregion

        #region [불량구분 변경 cboFormWorkType_SelectedValueChanged]
        private void cboDefectGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboDefectGroup.SelectedValue == null)
            //    return;

            SetIboxDefectGroup();
        }
        #endregion

        #region [작업시작]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_finalExternal)
                    {
                        // 최종외관 작업 시작
                        StartRunProcessFinalExternal();
                    }
                    else
                    {
                        StartRunProcess();
                    }
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        private void SetDefectGroupCombo()
        {
            const string bizRuleName = "DA_QCA_SEL_DEFECT_GROUP_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQPTID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _processCode, _equipmentCode, "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT" };
            string selectedValueText = cboDefectGroup.SelectedValuePath;
            string displayMemberText = cboDefectGroup.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboDefectGroup, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
        }

        private bool ChkProcess(string cmcdtype)
        {
            bool IsProc = false;

            try
            {
                // FORM_INSP_PROCID(최종외관), FORM_REWORK_PROCID((양품화)

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = cmcdtype;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + _processCode + "'");

                    if (dr.Length > 0)
                    {
                        IsProc = true;
                    }
                }

                return IsProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsProc;
            }
        }

        private DataTable ChkWotkOrder()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_CHK_SEL_WORLORDER_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// Lot Type
        /// </summary>
        private void SetLotType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTYPE_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr[cboLottype.DisplayMemberPath.ToString()] = "-SELECT-";
                dr[cboLottype.SelectedValuePath.ToString()] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboLottype.DisplayMemberPath = cboLottype.DisplayMemberPath.ToString();
                cboLottype.SelectedValuePath = cboLottype.SelectedValuePath.ToString();
                cboLottype.ItemsSource = dtResult.Copy().AsDataView();
                cboLottype.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 시작 Pallet 체크
        /// </summary>
        private void CheckPallet()
        {
            try
            {
                if (cboFormWorkType.SelectedValue == null || Util.NVC(cboFormWorkType.SelectedValue).Equals("SELECT"))
                {
                    // 작업 구분을 선택 하세요.
                    Util.MessageValidation("SFU4002");
                    return;
                }

                ShowLoadingIndicator();

                // Clear
                InitializeUserControls(false);

                string bizName;

                //if (rdoTray.IsChecked != null && (string.Equals(_divisionCode, "Tray") || (bool)rdoTray.IsChecked))
                if (rdoTray.IsChecked != null && (bool)rdoTray.IsChecked)
                        bizName = "BR_PRD_CHK_INPUT_LOT_TRAY";
                else
                    //bizName = "BR_PRD_CHK_INPUT_LOT_INBOX";
                    bizName = "BR_PRD_CHK_INPUT_LOT_CTNR";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                if ((bool)rdoPallet.IsChecked)
                {
                    inTable.Columns.Add("PROCID", typeof(string));
                }

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                if ((bool)rdoPallet.IsChecked)
                {
                    newRow["PROCID"] = _processCode;
                }
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtStartTray.Text;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                        else
                        {
                            if (bizResult.Tables["OUTDATA"].Rows.Count > 0)
                            {
                                if ((bool)rdoTray.IsChecked)
                                {
                                    txtAssyLotID.Text = bizResult.Tables["OUTDATA"].Rows[0]["ASSY_LOTID"].ToString();
                                }
                                _inputType = bizResult.Tables["OUTDATA"].Rows[0]["INPUT_TYPE"].ToString();  // (*)T:Tray, P:Inbox,  C:Container

                                // 조립 Lot 정보 조회
                                GetAssyLotInfo();
                            }
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
                Util.gridClear(dgAssyLot);
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// Lot 정보 조회
        /// </summary>
        private void GetAssyLotInfo()
        {
            try
            {
                DataTable inTable = new DataTable();

                string bizName = string.Empty;

                if (_inputType.Equals("T"))
                {
                    // Tray
                    if (string.IsNullOrWhiteSpace(txtAssyLotID.Text))
                    {
                        // 조립 Lot 정보가 없습니다.
                        Util.MessageValidation("SFU4001");
                        return;
                    }
                    bizName = "DA_PRD_SEL_INPUT_TRAY_ASSY_LOT_INFO_PC";

                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    // Data Row
                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = _equipmentCode;
                    newRow["LOTID"] = txtAssyLotID.Text;
                    newRow["CSTID"] = txtStartTray.Text;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inTable.Rows.Add(newRow);
                }
                else if (_inputType.Equals("P"))
                {
                    // Inbox
                    bizName = "DA_PRD_SEL_RUNSTART_INBOX_PC";

                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    // Data Row
                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["LOTID"] = txtStartTray.Text;
                    inTable.Rows.Add(newRow);
                }
                else
                {
                    // 대차
                    bizName = "DA_PRD_SEL_ASSYLOT_CART_LOAD";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("CTNR_ID", typeof(string));
                    // Data Row
                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["CTNR_ID"] = txtStartTray.Text;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                //Util.GridSetData(dgAssyLot, dtResult, null, true);
                //if (CommonVerify.HasDataGridRow(dgAssyLot) && dtResult.Rows.Count == 1)
                //{
                //    DataTableConverter.SetValue(dgAssyLot.Rows[0].DataItem, "CHK", true);
                //}

                SetdgLot(dtResult);

                if (_inputType.Equals("T"))
                    return;

                if (CommonVerify.HasDataGridRow(dgAssyLot) && dtResult.Rows.Count > 0)
                {
                    SetDataGridColumnVisibility(Util.NVC(dtResult.Rows[0]["WIP_QLTY_TYPE_CODE"]));
                }

                if (CommonVerify.HasDataGridRow(dgAssyLot) && dtResult.Rows.Count == 1)
                {
                    if (_inputType.Equals("P"))
                    {
                        // Inbox시
                        Util.GridSetData(dgProductionInbox, dtResult, FrameOperation, true);

                        _inBoxHeaderType = CheckBoxHeaderType.Two;
                        _inboxList = dtResult;
                        dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);
                    }
                    else
                    { 
                        //SetGridProductInboxList(Util.NVC(dtResult.Rows[0]["CTNR_ID"]), Util.NVC(dtResult.Rows[0]["ASSY_LOTID"]));
                        SetGridProductInboxList();
                    }
                }
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

        /// <summary>
        /// 조립LOT 완성 Inbox List
        /// </summary>
        private void SetGridProductInboxList()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Two;

                string CartID = string.Empty;
                string AssyLotID = string.Empty;

                DataRow[] drChk = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 1");

                if (drChk.Length == 0)
                {
                    Util.gridClear(dgProductionInbox);
                    return;
                }

                CartID = Util.NVC(drChk[0]["CTNR_ID"]);

                foreach (DataRow dr in drChk)
                {
                    AssyLotID += dr["ASSY_LOTID"] + ",";
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("LOTSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtStartTray.Text;
                newRow["ASSY_LOTID"] = AssyLotID.Substring(0, AssyLotID.Length-1);
                newRow["LOTSTAT"] = "RELEASED";
                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD", "INDATA", "OUTDATA", inTable);

                if (_inboxList != null && _inboxList.Rows.Count > 0)
                {
                    _inboxList.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                }

                Util.GridSetData(dgProductionInbox, _inboxList, FrameOperation, true);

                if (_inboxList != null && _inboxList.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetAssyLotInfoPopup()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtAssyLotID.Text))
                {
                    // 조립 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4001");
                    return;
                }

                ShowLoadingIndicator();

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["LOTID"] = txtAssyLotID.Text;
                inTable.Rows.Add(newRow);

                _inputType = "T";
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_ASSY_LOT_INFO_PC", "INDATA", "OUTDATA", inTable);

                //Util.GridSetData(dgAssyLot, dtResult, null, true);
                //if (CommonVerify.HasDataGridRow(dgAssyLot))
                //{
                //    DataTableConverter.SetValue(dgAssyLot.Rows[0].DataItem, "CHK", true);
                //}

                SetdgLot(dtResult);
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

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                string bizName = "BR_PRD_REG_START_PROD_LOT_MCP";

                DataRow[] drChk = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 1");

                // DATA Set
                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INPUT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));             // (*)활성화 작업 유형
                inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("LOT_INPUT_FLAG", typeof(string));
                inTable.Columns.Add("LOT_OUTPUT_FLAG", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["PROCID"] = Util.NVC(_processCode);
                //newRow["INPUT_TYPE"] = _inputType; //rdoTray.IsChecked != null && (bool)rdoTray.IsChecked ? "T" : "P";
                newRow["INPUT_TYPE"] = _inputType.Equals("C") ? "P" : _inputType;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();

                if (string.Equals(_divisionCode, "Tray"))
                {
                    newRow["CTNR_TYPE_CODE"] = "T";
                    // Degas Tray는 완성 LOT은 없다
                    newRow["LOT_OUTPUT_FLAG"] = "N";
                }
                else
                {
                    newRow["CTNR_TYPE_CODE"] = "P";
                    newRow["LOT_OUTPUT_FLAG"] = "Y";
                }

                // Tray 투입인 경우 투입 LOT은 없다
                if ((bool)rdoTray.IsChecked)
                {
                    newRow["LOT_INPUT_FLAG"] = "N";
                }
                else
                {
                    newRow["LOT_INPUT_FLAG"] = "Y";
                }

                // DSF 공정인 경우 작업 Mode를 넣어 준다
                if (_processCode.Equals(Process.PolymerDSF))
                {
                    if ((bool)rdoBoxMode.IsChecked)
                        newRow["CTNR_TYPE_CODE"] = "B";
                    else
                        newRow["CTNR_TYPE_CODE"] = "CART";
                }

                if ((bool)rdoPallet.IsChecked && cboFormWorkType.SelectedValue.Equals("FORM_WORK_RW"))
                {
                    // 재작업
                    newRow["CTNR_ID"] = Util.NVC(drChk[0]["CTNR_ID"]);
                }

                inTable.Rows.Add(newRow);

                // 투입 LOT
                DataTable inTableInput = ds.Tables.Add("IN_INPUT");
                inTableInput.Columns.Add("INPUT_LOTID", typeof(string));
                inTableInput.Columns.Add("ASSY_LOTID", typeof(string));

                if (_inputType.Equals("T"))
                {
                    newRow = inTableInput.NewRow();
                    newRow["INPUT_LOTID"] = string.IsNullOrWhiteSpace(Util.NVC(drChk[0]["CSTID"])) ? "NA" : Util.NVC(drChk[0]["CSTID"]);
                    newRow["ASSY_LOTID"] = Util.NVC(drChk[0]["ASSY_LOTID"]);
                    inTableInput.Rows.Add(newRow);
                }
                else
                {
                    DataRow[] drSelect = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");
                    foreach (DataRow dr in drSelect)
                    {
                        newRow = inTableInput.NewRow();
                        newRow["INPUT_LOTID"] = dr["LOTID"];
                        newRow["ASSY_LOTID"] = dr["ASSY_LOTID"];
                        inTableInput.Rows.Add(newRow);
                    }
                }

                // 투입 조립 LOT
                DataTable inTableAssy = ds.Tables.Add("IN_ASSY");
                inTableAssy.Columns.Add("ASSY_LOTID", typeof(string));
                inTableAssy.Columns.Add("PRODID", typeof(string));
                inTableAssy.Columns.Add("LOTTYPE", typeof(string));

                foreach (DataRow dr in drChk)
                {
                    newRow = inTableAssy.NewRow();
                    newRow["ASSY_LOTID"] = Util.NVC(dr["ASSY_LOTID"]);
                    newRow["PRODID"] = Util.NVC(dr["PRODID"]);
                    newRow["LOTTYPE"] = Util.NVC(dr["LOTTYPE"]);
                    inTableAssy.Rows.Add(newRow);
                }

                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,IN_INPUT,IN_ASSY", "OUTDATA", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTDATA"] != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            ProdLotId = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        private void StartRunProcessFinalExternal()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
                inTable.Columns.Add("LOT_INPUT_FLAG", typeof(string));
                inTable.Columns.Add("LOT_OUTPUT_FLAG", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("INASSY");
                inInput.Columns.Add("ASSY_LOTID", typeof(string));
                inInput.Columns.Add("LOTTYPE", typeof(string));
                inInput.Columns.Add("PRODID", typeof(string));
                inInput.Columns.Add("MKT_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgAssyLot.Rows[0].DataItem, "CTNR_ID"));
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["VISL_INSP_USERID"] = InspectorCode;

                // Tray 투입인 경우 투입 LOT은 없다
                if ((bool)rdoTray.IsChecked)
                    newRow["LOT_INPUT_FLAG"] = "N";
                else
                    newRow["LOT_INPUT_FLAG"] = "Y";

                // Degas Tray는 완성 LOT은 없다
                if (string.Equals(_divisionCode, "Tray"))
                    newRow["LOT_OUTPUT_FLAG"] = "N";
                else
                    newRow["LOT_OUTPUT_FLAG"] = "Y";

                inTable.Rows.Add(newRow);

                newRow = null;
                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgAssyLot.Rows)
                {
                    newRow = inInput.NewRow();
                    newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "ASSY_LOTID"));
                    newRow["LOTTYPE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTTYPE"));
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "PRODID"));
                    newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "MKT_TYPE_CODE"));
                    inInput.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_FV", "INDATA,INASSY", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        ProdCartId = Util.NVC(DataTableConverter.GetValue(dgAssyLot.Rows[0].DataItem, "CTNR_ID"));

                        this.DialogResult = MessageBoxResult.OK;
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

        #region[[Validation]
        private bool ValidateStartRun()
        {
            if (cboFormWorkType.SelectedValue == null || cboFormWorkType.SelectedValue.GetString().Equals("SELECT"))
            {
                // 작업 구분을 선택 하세요.
                Util.MessageValidation("SFU4002");
                return false;
            }

            if (dgAssyLot.Rows.Count <= 0)
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            if (_finalExternal.Equals(false))
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgAssyLot, "CHK");
                if (rowIndex < 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (!_inputType.Equals("T"))
                {
                    rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgProductionInbox, "CHK");

                    if (rowIndex < 0)
                    {
                        // 선택된 항목이 없습니다.
                        Util.MessageValidation("SFU1651");
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region [Func]
        private void SetdgLot(DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                // 선택 작업구분으로 변경
                string[] FromWorkTypeSplit = cboFormWorkType.Text.Split(':');
                if (FromWorkTypeSplit.Length > 1)
                    dt.Select().ToList<DataRow>().ForEach(r => r["FORM_WRK_TYPE_NAME"] = FromWorkTypeSplit[1]);
                else
                    dt.Select().ToList<DataRow>().ForEach(r => r["FORM_WRK_TYPE_NAME"] = "");

                // 작업구분이 반품 재작업인 경우 LOT 유형은 반품(N)으로 고정
                if (Util.NVC(cboFormWorkType.SelectedValue).Equals("FORM_WORK_RT"))
                {
                    dt.Select().ToList<DataRow>().ForEach(r => r["LOTTYPE"] = "N");
                }

                if (dt.Rows.Count == 1)
                {
                    dt.Rows[0]["CHK"] = 1;
                    dt.AcceptChanges();
                }
                Util.GridSetData(dgAssyLot, dt, null, true);

                //if (CommonVerify.HasDataGridRow(dgAssyLot))
                //{
                //    DataTableConverter.SetValue(dgAssyLot.Rows[0].DataItem, "CHK", true);
                //}
            }
            else
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return;
            }

        }

        private void SetIboxDefectGroup()
        {
            DataTable dtinbox = _inboxList.Copy();
            if (!string.IsNullOrWhiteSpace(Util.NVC(cboDefectGroup.SelectedValue)))
            {
                dtinbox.Select("DFCT_RSN_GR_ID <> '" + Util.NVC(cboDefectGroup.SelectedValue) + "'").ToList<DataRow>().ForEach(row => row.Delete());
                dtinbox.AcceptChanges();
            }

            Util.GridSetData(dgProductionInbox, dtinbox, FrameOperation, true);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
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
