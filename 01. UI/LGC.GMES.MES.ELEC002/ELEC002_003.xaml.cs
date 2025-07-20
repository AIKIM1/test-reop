/*****************************************
 Created Date : 2019.10.23
      Creator : JEONG
   Decription : 롤프레스 공정진척
------------------------------------------
 [Change History]
 2019-10-23   : BASEFORM UI분리
******************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Globalization;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF;
using C1.WPF.DataGrid;
using System.Linq;
using System.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_003 : UserControl, IWorkArea
    {
        #region Initialize
        private Util _Util = new Util();

        private GridLength ExpandFrame;

        private string _LDR_LOT_IDENT_BAS_CODE;
        private string _UNLDR_LOT_IDENT_BAS_CODE;
        private string _IS_POSTING_HOLD;

        private DataTable _CURRENT_LOTINFO = new DataTable();
        private DataTable _PROD_LOTINFO = new DataTable();

        private int dgLVIndex1 = 0;
        private int dgLVIndex2 = 0;
        private int dgLVIndex3 = 0;

        private bool isDefectLevel = false;
        private bool isChangeReason = false;
        private bool isChangeQuality = false;
        private bool isChangeColorTag = false;
        private bool isChangeRemark = false;

        private bool isDupplicatePopup = false;

        public ELEC002_003()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }
        public DataTable WIPCOLORLEGEND { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            grdWorkOrder.Children.Add(new UC_WORKORDER_CWA());
            InitComboBox();
            InitDataTable();

            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            foreach (Button button in Util.FindVisualChildren<Button>(mainGrid))
                listAuth.Add(button);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitComboBox()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: new string[] { Process.ROLL_PRESSING, null, "A" });

            // 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
            SetWipColorLegendCombo();
        }

        private void InitDataTable()
        {
            _CURRENT_LOTINFO.Columns.Add("LOTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("WIPSEQ", typeof(Int32));
            _CURRENT_LOTINFO.Columns.Add("WIPSTAT", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("LOTID_PR", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CSTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("OUT_CSTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CUT_ID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("PRODID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CUT", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("QA_INSP_TRGT_FLAG", typeof(string));
        }

        private void InitAllClearControl()
        {
            Util.gridClear(((UC_WORKORDER_CWA)grdWorkOrder.Children[0]).dgWorkOrder);
            Util.gridClear(dgProductLot);
            this.InitClearControl();
        }

        private void InitClearControl()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgQuality);
            Util.gridClear(dgColor);
            Util.gridClear(dgRemark);
            Util.gridClear(dgRemarkHistory);
            Util.gridClear(dgWipMerge);
            Util.gridClear(dgWipMerge2);

            _CURRENT_LOTINFO.Clear();

            _IS_POSTING_HOLD = string.Empty;
            txtUnit.Text = string.Empty;

            txtInputQty.Value = 0;
            txtParentQty.Value = 0;
            txtRemainQty.Value = 0;

            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtMergeInputLot.Text = string.Empty;
            txtLaneQty.Value = 0;
            txtCurLaneQty.Value = 0;

            chkExtraPress.IsChecked = false;

            isChangeReason = false;
            isChangeQuality = false;
            isChangeColorTag = false;
            isChangeRemark = false;

            btnSaveWipReason.IsEnabled = false;
            btnPublicWipSave.IsEnabled = false;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }
        }

        private void InitClearHalfSlitterSide()
        {
            txtWorkHalfSlittingSide.Text = string.Empty;
            txtWorkHalfSlittingSide.Tag = string.Empty;

            if (cboEquipment.SelectedIndex > 0)
                GetWorkHalfSlittingSide();
        }
        #endregion

        #region CWA용 LV Filter 로직
        private void ClearDefectLV()
        {
            if (chkDefectFilter.IsChecked == true)
            {
                isDefectLevel = true;
                OnClickDefetectFilter(chkDefectFilter, null);
                isDefectLevel = false;
            }
        }

        private void OnClickDefetectFilter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (dgLotInfo.GetRowCount() < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
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
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV2(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV3(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLVAll()
        {
            DataTable dt = (dgWipReason.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgWipReason.Rows[i].Visibility = Visibility.Visible;
            }
        }
        #endregion
        #region Event Definition
        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataRowView drv = e.NewValue as DataRowView;

            if (drv != null)
            {
                InitAllClearControl();

                GetWorkOrder();                                             // W/O 조회
                GetProductLot();                                            // 생산LOT 조회
                SetIdentInfo();                                             // 로더, 언로더 CARRIER
                SetLotAutoSelected();                                       // LOT자동선택
                GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
                InitClearHalfSlitterSide();                                 // HALF SLITTER SIDE 초기화
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(GetStatus()))
            {
                Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                return;
            }

            InitAllClearControl();

            GetWorkOrder();                                             // W/O 조회
            GetProductLot();                                            // 생산LOT 조회
            SetIdentInfo();                                             // 로더, 언로더 CARRIER
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
            InitClearHalfSlitterSide();                                 // HALF SLITTER SIDE 초기화
        }

        private void RefreshData(bool isConfirm = false)
        {
            if (isConfirm && _CURRENT_LOTINFO.Rows.Count > 0)
            {
                txtEndLotId.Text = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                int iSamplingCount;
                if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                {
                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                    {
                        foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                        {
                            iSamplingCount = 1;
                            string[] sCompany = null;

                            foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                            {
                                iSamplingCount = Util.NVC_Int(items.Key);
                                sCompany = Util.NVC(items.Value).Split(',');
                            }

                            for (int i = 0; i < iSamplingCount; i++)
                                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.ROLL_PRESSING, i > sCompany.Length - 1 ? "" : sCompany[i]);
                        }
                    }
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        if (string.Equals(LoginInfo.CFG_CARD_POPUP, "Y") || string.Equals(LoginInfo.CFG_CARD_AUTO, "Y"))
                            PrintHistoryCard();

                    }
                });
            }

            Thread.Sleep(500);

            InitAllClearControl();

            GetWorkOrder();                                             // W/O 조회
            GetProductLot();                                            // 생산LOT 조회
            SetIdentInfo();                                             // 로더, 언로더 CARRIER
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
            InitClearHalfSlitterSide();                                 // HALF SLITTER SIDE 초기화
        }

        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (chkWait != null && chkRun != null && chkEqpEnd != null && chkConfirm != null && chkWoProduct != null)
            {
                CheckBox cb = sender as CheckBox;

                switch (cb.Name)
                {
                    case "chkWait":
                        if (cb.IsChecked == true)
                        {
                            chkWoProduct.Visibility = Visibility.Visible;

                            chkRun.IsChecked = false;
                            chkEqpEnd.IsChecked = false;
                            chkConfirm.IsChecked = false;
                        }
                        else
                        {
                            chkWoProduct.Visibility = Visibility.Collapsed;

                            chkRun.IsChecked = true;
                            chkEqpEnd.IsChecked = true;
                            chkConfirm.IsChecked = true;
                        }
                        break;

                    case "chkRun":
                    case "chkEqpEnd":
                    case "chkConfirm":
                        if (cb.IsChecked == true)
                        {
                            chkWoProduct.Visibility = Visibility.Collapsed;

                            chkWait.IsChecked = false;
                            chkRun.IsChecked = true;
                            chkEqpEnd.IsChecked = true;
                            chkConfirm.IsChecked = true;
                        }
                        else
                        {
                            chkWoProduct.Visibility = Visibility.Visible;

                            chkWait.IsChecked = true;
                            chkRun.IsChecked = false;
                            chkEqpEnd.IsChecked = false;
                            chkConfirm.IsChecked = false;
                        }
                        break;

                    case "chkWoProduct":
                        if (cb.IsChecked == true)
                        {
                            if (dgProductLot.Rows.Count == 0)
                                return;

                            try
                            {
                                _PROD_LOTINFO = (dgProductLot.ItemsSource as DataView).Table;
                                Util.GridSetData(dgProductLot, (dgProductLot.ItemsSource as DataView).Table.Select("PRODID='" + (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).PRODID + "'").CopyToDataTable(), FrameOperation, true);
                            }
                            catch
                            {
                                dgProductLot.ItemsSource = null;
                                return;
                            }
                        }
                        else
                        {
                            if ( _PROD_LOTINFO.Rows.Count > 0)
                                Util.GridSetData(dgProductLot, _PROD_LOTINFO, FrameOperation, true);
                        }
                        break;
                }

                if (!cb.Name.Equals("chkWoProduct"))
                {
                    if (string.Equals(cb.Name, "chkRun") || string.Equals(cb.Name, "chkEqpEnd")) 
                        return;

                    if (cb.IsChecked == true)
                        RefreshData();
                }

                if (cb.Name.Equals("chkWait"))
                    chkWoProduct.IsChecked = true;
            }
        }

        private void OnGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgProductLot":
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

                                            InitClearControl();

                                            if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                                return;

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            GetLotInfo(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetDefectList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetQualityList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetColorList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetRemark(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetRemarkHistory(dgProductLot.Rows[e.Cell.Row.Index].DataItem);

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            ClearDefectLV();
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            InitClearControl();

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                                            ClearDefectLV();
                                        }
                                        break;
                                }

                                if (dgProductLot.CurrentCell != null)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                                else if (dgProductLot.Rows.Count > 0)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                                return;

                            // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완 
                            if (WIPCOLORLEGEND == null)
                                return;

                            // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                            SolidColorBrush scbZVersionBack = new SolidColorBrush();
                            SolidColorBrush scbZVersionFore = new SolidColorBrush();

                            SolidColorBrush scbCutBack = new SolidColorBrush();
                            SolidColorBrush scbCutFore = new SolidColorBrush();

                            foreach (DataRow dr in WIPCOLORLEGEND.Rows)
                            {
                                if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                                {
                                    continue;
                                }

                                if (dr["CODE"].ToString().Equals("Z_VER"))
                                {
                                    scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                                    scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                                }
                                else if (dr["CODE"].ToString().Equals("CUT"))
                                {
                                    scbCutBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                                    scbCutFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                                }
                            }

                            if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).IsNullOrEmpty() &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = scbCutBack;
                                e.Cell.Presenter.Foreground = scbCutFore;
                            }
                            else if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                            {
                                e.Cell.Presenter.Background = scbZVersionBack;
                                e.Cell.Presenter.Foreground = scbZVersionFore;
                            }
                            else if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                            {
                                e.Cell.Presenter.Background = scbZVersionBack;
                                e.Cell.Presenter.Foreground = scbZVersionFore;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }));
            }
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                            if (dataGrid.Columns["INPUT_VALUE_TYPE"].Index < e.Cell.Column.Index &&
                                dataGrid.Columns["EQPT_END_QTY"].Index > e.Cell.Column.Index && ((e.Cell.Row.Index - dataGrid.TopRows.Count)) == 2)
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

        private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count > 0)
            {
                C1DataGrid dataGrid = sender as C1DataGrid;
                if (dataGrid != null)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                    {
                        // 길이부족 조정 반영 (길이부족을 기점으로 수량 조정하도록 변경)
                        //if (e.Cell.Row.Index == _Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "PROD_QTY_INCR"))
                        if ( string.Equals( DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRCS_ITEM_CODE"), "PROD_QTY_INCR"))
                        {
                            // 로직 변경으로 주석 처리
                            /*
                            decimal dIncrQty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                            decimal dLackQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                            if (Util.NVC_Decimal(e.Cell.Value) > (dIncrQty + dLackQty))
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", dIncrQty + dLackQty);
                                DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY", 0);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY", dLackQty - (Util.NVC_Decimal(e.Cell.Value) - dIncrQty));
                            }
                            */

                            decimal dLackQty = GetSumFirstAutoQty(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK", "FRST_AUTO_RSLT_RESNQTY");
                            decimal dInitQty = GetSumFirstAutoQty(dataGrid, "PRCS_ITEM_CODE", "PROD_QTY_INCR", "FRST_AUTO_RSLT_RESNQTY");
                            decimal dSumQty = GetSumFirstAutoQty(dataGrid, "PRCS_ITEM_CODE", "PROD_QTY_INCR", "RESNQTY");

                            if ((dSumQty - dInitQty) > dLackQty)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", Util.NVC_Decimal(e.Cell.Value) - (dSumQty - (dLackQty + dInitQty)));
                                DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY", 0);
                            }
                            else if (dSumQty == dLackQty)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY", 0);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY", (dLackQty + dInitQty) - dSumQty);
                            }
                        }

                        if (Util.NVC_Decimal(e.Cell.Value) == 0 &&
                            Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK")) == false)
                        {
                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
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
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK", true);

                        GetSumDefectQty();
                        dgLotInfo.Refresh(false);
                    }
                }
            }
        }

        private void dgWipReason_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (string.Equals(e.Column.Name, "COUNTQTY") &&
                    !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    e.Cancel = true;

                if ((string.Equals(e.Column.Name, "COUNTQTY") || string.Equals(e.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Column.Name, "RESNQTY")) &&
                        string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y"))
                    e.Cancel = true;

                if (string.Equals(e.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    e.Cancel = true;

            }
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
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y")))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                // 길이부족 차감 색상표시 추가 [2019-12-09]
                                if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRCS_ITEM_CODE"), "PROD_QTY_INCR")))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D6606D"));
                            }
                        }
                    }
                }));
            }
        }


        private void dgWipReason_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgWipReason_BeganEdit(object sender, DataGridBeganEditEventArgs e)
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
                                    //decimal dIncrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "PROD_QTY_INCR")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                                    decimal dLackQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                                    //decimal dExceedQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_EXCEED")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                    //if (idx == _Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "PROD_QTY_INCR"))
                                    //  DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", dIncrQty + dLackQty);
                                    if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "PRCS_ITEM_CODE"), "PROD_QTY_INCR"))
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "FRST_AUTO_RSLT_RESNQTY")) + dLackQty);
                                    else
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                                        Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")));

                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                                            {
                                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                            }
                                        }
                                    }
                                    GetSumDefectQty();
                                    dgLotInfo.Refresh(false);
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
                            DataTableConverter.SetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK")].DataItem, "RESNQTY",
                                GetSumFirstAutoQty(dataGrid, "PRCS_ITEM_CODE", "LENGTH_LACK", "FRST_AUTO_RSLT_RESNQTY"));

                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
                        }
                    }
                }
            }));
        }

        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;
            
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

                    // 자주검사 USL, LSL 체크
                    DataTable dataCollect = DataTableConverter.Convert(caller.ItemsSource);
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
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
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

                    isChangeQuality = true;
                }
            }
        }

        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        }

                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                            if (e.Cell.Column.Index == dataGrid.Columns["CLSS_NAME1"].Index)
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                            }
                            else if (e.Cell.Column.Index == dataGrid.Columns["CLCTVAL01"].Index) // 측정값
                            {
                                if (presenter.HorizontalAlignment != HorizontalAlignment.Right)
                                    presenter.HorizontalAlignment = HorizontalAlignment.Right;

                                decimal sumValue = 0;
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                {
                                    if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非?字"))
                                    {
                                        foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));


                                        if (sumValue == 0)
                                            presenter.Content = 0;
                                        else
                                            presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dataGrid.Rows.Count - dataGrid.BottomRows.Count), "EA"));
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
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


        private void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
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

        private void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                isDupplicatePopup = true;
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
                        isChangeQuality = true;
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(item.Value)) && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        DataTable dataCollect = DataTableConverter.Convert(grid.ItemsSource);
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
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
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
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount3++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount4++;
                                    }
                                }
                            }
                        }

                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount3 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount4 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }

                        if (grid.BottomRows.Count > 0)
                            grid.BottomRows[0].Refresh(false);
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

                    isChangeQuality = true;
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
                isDupplicatePopup = false;
            }
        }

        private void dgColor_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgColor.Columns[e.Cell.Column.Index].Name)).Equals("") &&
                Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgColor.Columns[e.Cell.Column.Index].Name)) > 0)
                isChangeColorTag = true;
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void dgLevel_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
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
                                            dgLVIndex1 = e.Cell.Row.Index;

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
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
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
                                            if (dgLVIndex1 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex1 = e.Cell.Row.Index;
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
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
                                            dgLVIndex2 = e.Cell.Row.Index;


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
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();

                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
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
                                            if (dgLVIndex2 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex2 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
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
                                            dgLVIndex3 = e.Cell.Row.Index;


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
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
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
                                            if (dgLVIndex3 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex3 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
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
                        dgLVIndex1 = 0;
                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel1.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex2 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel2.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex3 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel3.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;
            }
        }
      
        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            DataRowView drv = dataitem.DataItem as DataRowView;
            string sInputLot;
            string sChildSeq;
            string sLot;

            try
            {
                sInputLot = drv["LOTID_PR"].ToString().Equals(string.Empty) ? drv["LOTID"].ToString() : drv["LOTID_PR"].ToString();
            }
            catch
            {
                sInputLot = string.Empty;
            }

            try
            {
                sChildSeq = string.IsNullOrEmpty(drv["CUT_ID"].ToString()) ? "1" : drv["CUT_ID"].ToString();
            }
            catch
            {
                sChildSeq = "1";
            }

            try
            {
                sLot = drv["LOTID"].ToString();
            }
            catch
            {
                sLot = string.Empty;
            }

            if (!string.IsNullOrEmpty(sInputLot) && !string.IsNullOrEmpty(sChildSeq))
            {
                // 모두 Uncheck 처리 및 동일 자LOT의 경우는 Check 처리.
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    if (dataitem.Index != i)
                    {
                        if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID_PR")) &&
                            sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CUT_ID")))
                        {
                            if (sInputLot.Equals(""))
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                    dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (bUncheckAll)
                                {
                                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                         dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                         (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                }
                                else
                                {

                                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                        dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                }

                            }
                        }
                        else
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            return true;
        }

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

        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
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
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }
        #endregion
        #region User Method
        private string GetStatus()
        {
            var status = new List<string>();

            if (chkWait.IsChecked == true)
                status.Add(chkWait.Tag.ToString());

            if (chkRun.IsChecked == true)
                status.Add(chkRun.Tag.ToString());

            if (chkEqpEnd.IsChecked == true)
                status.Add(chkEqpEnd.Tag.ToString());

            if (chkConfirm.IsChecked == true)
                status.Add(chkConfirm.Tag.ToString());

            return string.Join(",", status);
        }

        private void GetWorkOrder()
        {
            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                wo.FrameOperation = FrameOperation;
                wo.EQPTSEGMENT = Util.NVC(cboEquipmentSegment.SelectedValue);
                wo.EQPTID = Util.NVC(cboEquipment.SelectedValue);
                wo.PROCID = Process.ROLL_PRESSING;
                wo.GetWorkOrder();
            }
        }

        private void SetLotAutoSelected()
        {
            if (dgProductLot != null && dgProductLot.Rows.Count > 0)
            {
                C1.WPF.DataGrid.DataGridCell currCell = dgProductLot.GetCell(0, dgProductLot.Columns["CHK"].Index);

                if (currCell != null)
                {
                    dgProductLot.SelectedIndex = currCell.Row.Index;
                    dgProductLot.CurrentCell = currCell;

                }
            }         
        }

        private void SetWipColorLegendCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow inRow = inTable.NewRow();
            inRow["LANGID"] = LoginInfo.LANGID;
            inRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            inRow["PROCID"] = Process.ROLL_PRESSING;

            inTable.Rows.Add(inRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            WIPCOLORLEGEND = dtResult;
        }

        private bool IsValidDispatcher()
        {
            if (!IsValidGoodQty())
                return false;

            if (!IsValidLimitQty())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (!ValidQualityRequired())
                return false;

            if (!ValidQualitySpecRequired())
                return false;

            if (!CheckRollQASampling())
                return false;

            if (!IsValidCollect())
                return false;

            return true;
        }

        private bool IsValidGoodQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) < 0)
                        {
                            Util.MessageValidation("SFU5129");  //양품량이 0보다 작습니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsValidLimitQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) !=
                            (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) +
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT_SUM")) +
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_LEN_LACK"))))
                        {
                            Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidShift()
        {
            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력하세요.
                return false;
            }

            return true;
        }

        private bool ValidOperator()
        {
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }

            return true;
        }

        private bool ValidQualityRequired()
        {
            List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality };
            foreach (C1DataGrid dg in lst)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    bool isValid = false;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (!string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) && !string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (isValid == false)
                    {
                        Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidQualitySpecRequired()
        {
            List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality };
            foreach (C1DataGrid dg in lst)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    string itemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckQualitySpec()
        {
            DataTable dataCollect = DataTableConverter.Convert(dgQuality.ItemsSource);
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
                        if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                            iHGCount1++;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
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
                else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                    {
                        if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                            iHGCount3++;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                    {
                        if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                            iHGCount4++;
                        }
                    }
                }
            }

            foreach (DataRow row in dataCollect.Rows)
            {
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount1 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue1 / iHGCount1)) > 4))
                        {
                            return true;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount2 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue2 / iHGCount2)) > 4))
                        {
                            return true;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount1 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue1 / iHGCount1)) > 4))
                        {
                            return true;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount2 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue2 / iHGCount2)) > 4))
                        {
                            return true;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount3 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue3 / iHGCount3)) > 4))
                        {
                            return true;
                        }
                    }
                }
                else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && !string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                    {
                        if (iHGCount4 > 0 && (Math.Abs(Util.NVC_Decimal(row["CLCTVAL01"]) - (sumValue4 / iHGCount4)) > 4))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private bool IsValidCollect()
        {
            if (isChangeQuality)
            {
                Util.MessageValidation("SFU1999");  //품질 정보를 저장하세요.
                return false;
            }

            if (isChangeColorTag)
            {
                Util.MessageValidation("SFU3410");  //색지 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }
            return true;
        }

        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

            if (dtMain != null && dtMain.Rows.Count > 0)
                return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                return;

            // 전역변수 SET
            DataRow dataRow = _CURRENT_LOTINFO.NewRow();
            dataRow["LOTID"] = Util.NVC(rowview["LOTID"]);
            dataRow["WIPSEQ"] = Util.NVC_Int(rowview["WIPSEQ"]);
            dataRow["WIPSTAT"] = Util.NVC(rowview["WIPSTAT"]);
            dataRow["LOTID_PR"] = Util.NVC(rowview["LOTID_PR"]);
            dataRow["CSTID"] = Util.NVC(rowview["CSTID"]);
            dataRow["OUT_CSTID"] = Util.NVC(rowview["OUT_CSTID"]);
            dataRow["CUT_ID"] = Util.NVC(rowview["CUT_ID"]);
            dataRow["PRODID"] = Util.NVC(rowview["PRODID"]);
            dataRow["CUT"] = Util.NVC(rowview["CUT"]);
            dataRow["QA_INSP_TRGT_FLAG"] = Util.NVC(rowview["QA_INSP_TRGT_FLAG"]);
            _CURRENT_LOTINFO.Rows.Add(dataRow);

            // SET VERSION
            DataTable versionDt = GetProcessVersion(Util.NVC(rowview["LOTID"]), Util.NVC(rowview["PRODID"]));
            if (versionDt.Rows.Count > 0)
            {
                txtVersion.Text = Util.NVC(versionDt.Rows[0]["PROD_VER_CODE"]);
                txtLaneQty.Value = string.IsNullOrEmpty(Util.NVC(versionDt.Rows[0]["LANE_QTY"])) ? 0 : Convert.ToInt16(Util.NVC(versionDt.Rows[0]["LANE_QTY"]));
            }

            // CWA 전수 불량 추가
            txtCurLaneQty.Value = getCurrLaneQty(Util.NVC(rowview["LOTID"]));

            if (getDefectLane(LoginInfo.CFG_EQSG_ID))
                btnSaveRegDefectLane.Visibility = Visibility.Visible;

            btnSaveRegDefectLane.IsEnabled = true;

            // SET TIME
            txtStartDateTime.Text = Convert.ToDateTime(Util.NVC(rowview["WIPDTTM_ST"])).ToString("yyyy-MM-dd HH:mm");

            if (string.IsNullOrEmpty(Util.NVC(rowview["WIPDTTM_ED"])))
                txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            else
                txtEndDateTime.Text = Convert.ToDateTime(Util.NVC(rowview["WIPDTTM_ED"])).ToString("yyyy-MM-dd HH:mm");

            if (txtWorkDate != null)
                SetCalDate();

            txtUnit.Text = Util.NVC(rowview["UNIT_CODE"]);
            txtMergeInputLot.Text = Util.NVC(rowview["LOTID_PR"]);
            txtParentQty.Value = Convert.ToDouble(rowview["INPUTQTY"]);

            SetExtraPress();    // 2차 압연 모델 추가압연 체크

            // 완공 상태일 경우만 불량/Loss 저장 가능
            if (string.Equals(rowview["WIPSTAT"], Wip_State.END))
            {
                btnSaveWipReason.IsEnabled = true;
                btnPublicWipSave.IsEnabled = true;
            }

            SetUnitFormatted();
            SetResultInfo(rowview);     // SET LOT GRID
        }

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

                if (dgLotInfo.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgLotInfo.Columns.Count; i++)
                        if (dgLotInfo.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgLotInfo.Columns[i].Tag, "N"))
                            // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = sFormatted;

                if (dgWipReason.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgWipReason.Columns.Count; i++)
                        if (dgWipReason.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReason.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)dgWipReason.Columns[i]).Format = sFormatted;

                if (dgQuality.Visibility == Visibility.Visible)
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

        private bool IsWorkOrderValid()
        {
            bool IsValid = true;
            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                if (new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK") != -1)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

                    if (Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WOID")))
                        {
                            Util.MessageValidation("SFU1436");
                            IsValid = false;
                        }
                        else
                        {
                            IsValid = true;
                        }
                    }
                }
            }
            return IsValid;
        }

        private void GetSumDefectQty()
        {
            // LENGTH_LACK : 길이부족, LENGTH_EXCEED : 길이초과 [RSLT_EXCEL_FLAG = 'Y'],  PROD_QTY_INCR :생산수량증가
            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
                return;

            if (_CURRENT_LOTINFO.Rows.Count > 0)
            {
                decimal dInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY"));
                decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LANE_QTY"));
                decimal dDefectQty = GetSumDefectQty(dgWipReason, "DEFECT_LOT");
                decimal dLossQty = GetSumDefectQty(dgWipReason, "LOSS_LOT");
                decimal dChargeProdQty = GetSumDefectQty(dgWipReason, "CHARGE_PROD_LOT");
                decimal dLengthLackQty = GetSumProcItemQty(dgWipReason, "LENGTH_LACK");
                decimal dLengthExceedQty = GetSumProcItemQty(dgWipReason, "LENGTH_EXCEED");
                decimal dDefectTotalQty = dDefectQty + dLossQty + dChargeProdQty;

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", dDefectQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_LOSS", dLossQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_CHARGEPRD", dChargeProdQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT_SUM", dDefectTotalQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_LEN_EXCEED", dLengthExceedQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_LEN_LACK", dLengthLackQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY", dInputQty - (dDefectTotalQty + dLengthLackQty));
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY2", (dInputQty - (dDefectTotalQty + dLengthLackQty)) * dLaneQty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY2",
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) * dLaneQty);
                    }
                }
                // Summary 추가
                txtInputQty.Value = Convert.ToDouble(dInputQty - (dDefectTotalQty + dLengthLackQty));
                txtParentQty.Value = Convert.ToDouble(dInputQty);
                txtRemainQty.Value = Convert.ToDouble(0);
            }
        }

        private decimal GetSumDefectQty(C1DataGrid dataGrid, string sActId)
        {
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE")), "LENGTH_LACK") &&
                                !string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE")), "LENGTH_EXCEED"))
                                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            return dSumQty;
        }


        private decimal GetSumProcItemQty(C1DataGrid dataGrid, string sItemCode)
        {
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), sItemCode))
                                dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            return dSumQty;
        }


        private decimal GetSumFirstAutoQty(C1DataGrid dataGrid, string sColumnName, string sCompareValue, string sColumnValue)
        {
            decimal remainQty = 0;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)).Equals(sCompareValue))
                    remainQty += Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnValue));

            return remainQty;
        }
        #endregion
        #region DA Biz Call
        // Manual Mode 체크 [USERTYPE = 'G'인 경우만 활성화
        private void SetManualMode(List<Button> buttons)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(row["USERTYPE"]), "G"))
                        {
                            foreach (Button button in buttons)
                                if (string.Equals(Util.NVC(button.Tag), Util.NVC(row["USERTYPE"])))
                                    button.Visibility = Visibility.Visible;

                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 동별 공통코드 사용 유무 
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return false;
        }

        private void SetCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "RQSTDT", "OUTDATA", RQSTDT);

                if (result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["CALDATE"])))
                {
                    txtWorkDate.Text = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtWorkDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // OUT LOT 조회
        private void GetProductLot(DataRowView drv = null)
        {
            try
            {
                if (string.IsNullOrEmpty(GetStatus()))
                {
                    Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("LOTID_LARGE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.ROLL_PRESSING;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["WIPSTAT"] = GetStatus();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_ELEC", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgProductLot, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 작업자 정보 SET
        private void GetWrkShftUser()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = Process.ROLL_PRESSING;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", RQSTDT, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                        {
                            txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                        }
                        else
                        {
                            txtShiftStartTime.Text = string.Empty;
                        }

                        if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                        {
                            txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            txtShiftEndTime.Text = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                        {
                            txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                        }
                        else
                        {
                            txtShiftDateTime.Text = string.Empty;
                        }

                        if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                        else
                        {
                            txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                            txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                        }

                        if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                        {
                            txtShift.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                        }
                        else
                        {
                            txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                            txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                        }
                    }
                    else
                    {
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void SetIdentInfo()
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    _LDR_LOT_IDENT_BAS_CODE = string.Empty;
                    _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.ROLL_PRESSING;
                row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _UNLDR_LOT_IDENT_BAS_CODE = result.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();

                    switch (_LDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }

                    switch (_UNLDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

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
                    Indata["PROCID"] = Process.ROLL_PRESSING;
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

        private DataTable GetProcessVersion(string sLotID, string sProdID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PROCSTATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.ROLL_PRESSING;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["LOTID"] = sLotID;
                dr["MODLID"] = sProdID;
                dr["PROCSTATE"] = "Y";
                RQSTDT.Rows.Add(dr);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_V01", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
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
                dr["PROCID"] = Process.ROLL_PRESSING;
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "RQSTDT", "RSLTDT", RQSTDT);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return 0;
        }

        private bool getDefectLane(string sEQSGID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SCRIBE_DEFECT_LANE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (string.Equals(Util.NVC(row["CBO_CODE"]), sEQSGID + ":" + Process.ROLL_PRESSING))
                            return true;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return false;
        }

        private void SetResultInfo(DataRowView rowview)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.NVC(rowview["LOTID"]);
                dr["WIPSEQ"] = Util.NVC_Int(rowview["WIPSEQ"]);
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_INFO_ROLL", "INDATA", "RSLTDT", RQSTDT);
                Util.GridSetData(dgLotInfo, dt, FrameOperation, true);
                _Util.SetDataGridMergeExtensionCol(dgLotInfo, new string[] { "LOTID", "OUT_CSTID", "PR_LOTID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetDefectList(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                    return;

                Util.gridClear(dgWipReason);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgWipReason };
                foreach (C1DataGrid dg in lst)
                {
                    inDataTable.Rows.Clear();

                    DataRow Indata = inDataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.ROLL_PRESSING;
                    Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                    inDataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                    if (dg.Visibility == Visibility.Visible)
                        Util.GridSetData(dg, dt, FrameOperation, true);
                }

                GetSumDefectQty();
                dgLotInfo.Refresh(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetQualityList(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                    return;

                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality };
                foreach (C1DataGrid dg in lst)
                {
                    IndataTable.Rows.Clear();

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.ROLL_PRESSING;
                    Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                    Indata["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                    {
                        Indata["VER_CODE"] = txtVersion.Text;
                        Indata["LANEQTY"] = txtLaneQty.Value;
                    }
                    IndataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (dt.Rows.Count == 0)
                        dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (dg.Visibility == Visibility.Visible)
                    {
                        Util.GridSetData(dg, dt, FrameOperation, true);
                        _Util.SetDataGridMergeExtensionCol(dg, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetColorList(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                    return;

                Util.gridClear(dgColor);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                Indata["PROCID"] = Process.ROLL_PRESSING;
                Indata["EQPTID"] = null;
                Indata["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(dgColor, dt, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgColor, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetRemark(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                    return;

                Util.gridClear(dgRemark);

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(String));
                dt.Columns.Add("REMARK", typeof(String));
                DataRow inDataRow = null;
                inDataRow = dt.NewRow();

                inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

                if (rowview != null)
                {
                    string[] sWipNote = GetRemarkData(Util.NVC(rowview["LOTID"])).Split('|');
                    if (sWipNote.Length > 1)
                        inDataRow["REMARK"] = sWipNote[1];
                }
                dt.Rows.Add(inDataRow);

                inDataRow = dt.NewRow();
                inDataRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                inDataRow["REMARK"] = GetRemarkData(Util.NVC(rowview["LOTID"])).Split('|')[0];
                dt.Rows.Add(inDataRow);

                Util.GridSetData(dgRemark, dt, FrameOperation);
                dgRemark.Rows[0].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetRemarkData(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }

        private void GetRemarkHistory(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                if (string.Equals(rowview["WIPSTAT"], Wip_State.WAIT))
                    return;

                Util.gridClear(dgRemarkHistory);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                inDataTable.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_HISTORY_WIPNOTE", "INDATA", "RSLTDT", inDataTable);

                // 필요정보 변환
                System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                foreach (DataRow row in dtResult.Rows)
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
                Util.GridSetData(dgRemarkHistory, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetMergeList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dataRow["PROCID"] = Process.ROLL_PRESSING;
                dataRow["PRODID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["PRODID"]);
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_LIST_V01", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgWipMerge, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetMergeEndList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dataRow["PROCID"] = Process.ROLL_PRESSING;
                dataRow["LOTID"] = txtMergeInputLot.Text;
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_END_LIST", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgWipMerge2, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private DataTable GetMergeInfo(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_HIST", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return new DataTable();
        }

        private string GetLotProdVerCode(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PROD_VER_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void GetWorkHalfSlittingSide()
        {
            try
            {
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WRK_HALF_SLIT_SIDE", "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                        txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetExtraPress()
        {
            try
            {
                int roll_Count; //압연 횟수
                int roll_Seq;   //압연 차수

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLPRESS_COUNT", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    roll_Count = Int32.Parse(string.Equals(result.Rows[0]["ROLLPRESS_COUNT"], "") ? "0" : Util.NVC(result.Rows[0]["ROLLPRESS_COUNT"]));
                    roll_Seq = Int32.Parse(string.Equals(result.Rows[0]["ROLLPRESS_SEQNO"], "") ? "0" : Util.NVC(result.Rows[0]["ROLLPRESS_SEQNO"]));

                    if (roll_Count <= roll_Seq)
                    {
                        chkExtraPress.IsEnabled = true;
                        chkExtraPress.IsChecked = false;
                    }
                    else
                    {
                        chkExtraPress.IsEnabled = false;
                        chkExtraPress.IsChecked = true;
                    }
                }
                else
                {
                    roll_Count = 0;
                    roll_Seq = 0;

                    chkExtraPress.IsEnabled = true;
                    chkExtraPress.IsChecked = false;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool CheckRollQASampling()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPATTR_QAFLAG", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1382");  //LOT이 WIPATTR테이블에 없습니다. 
                    return false;
                }

                if (Convert.ToString(result.Rows[0]["QA_INSP_TRGT_FLAG"]).Equals("Y"))
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("AREAID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                    IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.ROLL_PRESSING;
                    Indata["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                    Indata["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]); ;

                    Indata["CLCT_PONT_CODE"] = null;
                    IndataTable.Rows.Add(Indata);

                    result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count == 0)
                        result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count > 0)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        DataRow[] inspRows;

                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0)
                        {
                            inspRows = result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'E3000-0001'") : result.Select("INSP_ITEM_ID = 'SI516'");
                        }
                        else
                        {
                            inspRows = result.Select("INSP_ITEM_ID = 'SI022'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'SI022'") : result.Select("INSP_ITEM_ID = 'SI516'");
                        }
                    }
                }

                if (!CheckValidInspectionSpec())
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;
        }

        private bool CheckValidInspectionSpec()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INSP_ITEM_SPEC", "INDATA", "RSLTDT", dt);
            if (result.Rows.Count != 0)
            {
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    if (Convert.ToString(result.Rows[i]["CLCTVAL01"]).Equals("") || Util.NVC_Decimal(result.Rows[i]["CLCTVAL01"]) == 0)
                    {
                        Util.MessageValidation("SFU2886", new object[] { Util.NVC(result.Rows[i]["INSP_CLCTNAME"]) });  //{%1} 품질 값을 넣어주세요
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidConfirmLotCheck()
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            try
            {
                string sLotID = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", IndataTable);

                if (dt != null && dt.Rows.Count > 0 && !string.Equals(Process.ROLL_PRESSING, dt.Rows[0]["PROCID"]) && (string.Equals(INOUT_TYPE.IN, dt.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dt.Rows[0]["WIP_TYPE_CODE"])))
                    return false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }
        #endregion
        #region BR Biz Call
        private void SaveWIPHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS1QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS2QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS3QTY", typeof(decimal));
            inDataTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("OUT_CSTID", typeof(string));

            foreach (DataRow dr in _CURRENT_LOTINFO.Rows)
            {
                DataRow inLotDetailDataRow = null;
                inLotDetailDataRow = inDataTable.NewRow();
                inLotDetailDataRow["LOTID"] = Util.NVC(dr["LOTID"]);
                inLotDetailDataRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                inLotDetailDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                inLotDetailDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                inLotDetailDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                inLotDetailDataRow["LANE_PTN_QTY"] = 1;
                inLotDetailDataRow["LANE_QTY"] = Util.NVC_Decimal(txtLaneQty.Value);
                inLotDetailDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inLotDetailDataRow);
            }

            new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inDataTable, (result, resultEx) =>
            {
                try
                {
                    if (resultEx != null)
                    {
                        Util.MessageException(resultEx);
                        return;
                    }
                    int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (iRow >= 0)
                        DataTableConverter.SetValue(dgProductLot.Rows[iRow].DataItem, "PROD_VER_CODE", Util.NVC(txtVersion.Text));
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        // 불량/Loss/물청 저장
        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = Process.ROLL_PRESSING;
            inDataTable.Rows.Add(inDataRow);

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            inDataRow = null;
            DataTable dtTop = (dg.ItemsSource as DataView).Table;

            foreach (DataRow dataRow in dtTop.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                inDataRow["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                inDataRow["ACTID"] = dataRow["ACTID"];
                inDataRow["RESNCODE"] = dataRow["RESNCODE"];
                inDataRow["RESNQTY"] = dataRow["RESNQTY"].ToString().Equals("") ? 0 : dataRow["RESNQTY"];
                inDataRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(dataRow["DFCT_TAG_QTY"])) ? 0 : dataRow["DFCT_TAG_QTY"];
                inDataRow["LANE_QTY"] = txtLaneQty.Value;
                inDataRow["LANE_PTN_QTY"] = 1;
                inDataRow["COST_CNTR_ID"] = dataRow["COSTCENTERID"];
                inDataRow["WRK_COUNT"] = dataRow["COUNTQTY"].ToString() == "" ? DBNull.Value : dataRow["COUNTQTY"];

                IndataTable.Rows.Add(inDataRow);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 품질항목 저장
        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);
            try
            {
                new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot);
                isChangeQuality = false;
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }


        // 색지정보 저장
        private void SaveColorTag(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);
            try
            {
                new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot);
                isChangeColorTag = false;
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }


        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                inData = IndataTable.NewRow();

                inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inData["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                inData["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();

                inData["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            } 
            return IndataTable;
        }

        // 특이사항 저장
        private void SaveWipNote()
        {
            if (dgRemark.GetRowCount() < 1)
                return;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIP_NOTE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
            DataRow inData = null;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                inData = inTable.NewRow();

                inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                if (dgRemark.Rows[0].Visibility == Visibility.Visible)
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                else
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                inData["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(inData);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable);
                isChangeRemark = false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

        }

        private void SaveMergeData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["LOTID"] = Util.NVC(txtMergeInputLot.Text);
                        row["NOTE"] = string.Empty;
                        row["USERID"] = LoginInfo.USERID;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable formLotID = indataSet.Tables.Add("IN_FROMLOT");
                        formLotID.Columns.Add("FROM_LOTID", typeof(string));

                        DataTable dt = ((DataView)dgWipMerge.ItemsSource).Table;
                        decimal iAddSumQty = 0;

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                row = formLotID.NewRow();

                                iAddSumQty += Util.NVC_Decimal(inRow["WIPQTY"]);
                                row["FROM_LOTID"] = Util.NVC(inRow["LOTID"]);
                                indataSet.Tables["IN_FROMLOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MERGE_LOT", "INDATA,IN_FROMLOT", null, indataSet);

                        Util.MessageInfo("SFU2009");    //합권되었습니다.
                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }
        #endregion
        #region Button Event
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (!IsWorkOrderValid())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", Process.ROLL_PRESSING);
            dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            dicParam.Add("EQSGID", Util.NVC(cboEquipmentSegment.SelectedValue));
            if (new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") != -1)
                dicParam.Add("LOTID", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID_PR")));

            ELEC002_003_LOTSTART _LotStart = new ELEC002_003_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;
            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC002_003_LOTSTART window = sender as ELEC002_003_LOTSTART;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // TO-DO : 롤프레스 착공 취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (!string.Equals(Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"]), Wip_State.PROC))
                {
                    Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                    return;
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow inDataRow = null;
                        for (int i = 0; i < _CURRENT_LOTINFO.Rows.Count; i++)
                        {
                            inDataRow = null;
                            inDataRow = inDataTable.NewRow();

                            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                            inDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                            if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                                inDataRow["OUT_CSTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["OUT_CSTID"]);

                            inDataRow["USERID"] = LoginInfo.USERID;
                            inDataRow["INPUT_LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID_PR"]);

                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                                inDataRow["CSTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CSTID"]);

                            inDataTable.Rows.Add(inDataRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_RP", "INDATA", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.PROC))
            {
                Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPT_END();
            wndEqpComment.FrameOperation = FrameOperation;

            string endLotID = "";
            foreach (DataRow row in _CURRENT_LOTINFO.Rows)
                if (!string.IsNullOrEmpty(Util.NVC(row["LOTID"])))
                    endLotID = Util.NVC(row["LOTID"]) + "," + endLotID;

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = Process.ROLL_PRESSING;
                Parameters[2] = endLotID;
                Parameters[3] = txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(txtStartDateTime.Text);    // 시작시간 추가
                Parameters[5] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CUT_ID"]);
                Parameters[6] = Util.NVC(txtParentQty.Value);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(OnCloseEqptEnd);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        private void OnCloseEqptEnd(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END window = sender as LGC.GMES.MES.CMM001.CMM_COM_EQPT_END;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        private void btnEndCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (!string.Equals(Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"]), Wip_State.END))
                {
                    Util.MessageValidation("SFU5146", new object[] { _CURRENT_LOTINFO.Rows[0]["LOTID"] });  //해당 Lot[%1]은 실적확정 상태가 아니라 취소 할 수 없습니다.
                    return;
                }

                if (_Util.IsCommonCodeUse("ELEC_CNFM_CANCEL_USER", LoginInfo.USERID) == false)
                {
                    Util.MessageValidation("SFU5148", new object[] { LoginInfo.USERID });  //해당 USER[%1]는 실적취소할 권한이 없습니다. (시스템 담당자에게 문의 바랍니다.)
                    return;
                }

                // 해당 확정 처리 된 Lot을 실적취소하시겠습니까?
                Util.MessageConfirm("SFU5147", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        inDataRow["PROCID"] = Process.ROLL_PRESSING;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, inDataSet);
                    }
                });
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDispatch_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 확정 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
            {
                Util.MessageValidation("SFU5131");  //공정이동 대상 Lot 선택 오류 [선택한 Lot이 완공상태 인지 확인 후 처리]
                return;
            }

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            if (!IsValidDispatcher())
                return;

            if (CheckQualitySpec() == true)
            {
                LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = "ELEC_MANA";    // 관리권한

                    C1WindowExtension.SetParameters(authConfirm, Parameters);

                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                    this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                }
            }
            else
            {
                ConfirmDispatcher();
            }
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
                ConfirmDispatcher();
        }

        // TO-DO : 디스패처 로직 구현
        private void ConfirmDispatcher(bool bRealWorkerSelFlag = false)
        {
            // Remark 취합
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU4257"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

            #region 작업자 실명관리 기능 추가
            if (!bRealWorkerSelFlag && CheckRealWorkerCheckFlag())
            {
                CMM001.CMM_COM_INPUT_USER wndRealWorker = new CMM001.CMM_COM_INPUT_USER();

                wndRealWorker.FrameOperation = FrameOperation;
                object[] Parameters2 = new object[0];
                //Parameters2[0] = "";

                C1WindowExtension.SetParameters(wndRealWorker, Parameters2);

                wndRealWorker.Closed -= new EventHandler(wndRealWorker_Closed);
                wndRealWorker.Closed += new EventHandler(wndRealWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndRealWorker.ShowModal()));

                return;
            }
            #endregion

            // 다음 공정으로 이송 하시겠습니까?
            Util.MessageConfirm("SFU4257", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        // DEFECT 자동 저장
                        SaveDefect(dgWipReason);

                        // IN_DATA
                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("IN_DATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WIP_NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("REPROC_YN", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        row["PROCID"] = Process.ROLL_PRESSING;
                        row["SHIFT"] = txtShift.Tag;
                        row["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                        row["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                        row["WIP_NOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "LOTID"))];
                        row["USERID"] = LoginInfo.USERID;
                        row["REPROC_YN"] = chkExtraPress.IsChecked == true ? "Y" : "N";
                        inDataSet.Tables["IN_DATA"].Rows.Add(row);

                        // IN_LOT
                        DataTable InLot = inDataSet.Tables.Add("IN_LOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(decimal));
                        InLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLot.Columns.Add("RESNQTY", typeof(decimal));
                        InLot.Columns.Add("HOLD_YN", typeof(string));

                        for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                        {
                            if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                            {
                                row = InLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "LOTID"));
                                row["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY"));
                                row["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                                row["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT_SUM"))
                                               + Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_LEN_LACK"));
                                row["HOLD_YN"] = _IS_POSTING_HOLD;
                                inDataSet.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NEXT_PROC_MOVE", "IN_DATA,IN_LOT", null, (result, searchException) =>
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            RefreshData(true);
                        }, inDataSet);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            });
        }

        private Dictionary<string, string> GetRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (dgRemark.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < dgRemark.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    sRemark.Append("|");

                    // 3. 조정횟수
                    if (dgWipReason.Visibility == Visibility.Visible && dgWipReason.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < dgWipReason.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 4. 압연횟수
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "ROLLPRESS_SEQNO")));
                    sRemark.Append("|");

                    // 5.색지정보
                    for (int j = 0; j < dgColor.Rows.Count; j++)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01"))) &&
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01")) > 0)
                            sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID")), Process.ROLL_PRESSING);
                    if (mergeInfo.Rows.Count > 0)
                        foreach (DataRow row in mergeInfo.Rows)
                            sRemark.Append(Util.NVC(row["LOTID"]) + " : " + GetUnitFormatted(row["LOT_QTY"]) + txtUnit.Text + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        private void PrintHistoryCard()
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[4];
                Parameters[0] = txtEndLotId.Text; //LOT ID
                Parameters[1] = Process.ROLL_PRESSING; //PROCESS ID
                Parameters[2] = string.Empty;
                Parameters[3] = "Y";    // 실적확정 여부

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnBarcodeLabel_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1559");  //발행할 LOT을 선택하십시오.
                return;
            }

            if (string.IsNullOrEmpty(GetLotProdVerCode(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]))))
            {
                Util.MessageValidation("SFU4561"); // 생산실적 화면의 저장버튼 클릭 후(버전 정보 저장) 바코드 출력 하시기 바랍니다.
                return;
            }

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                return;
            }

            DataTable printDT = GetPrintCount(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]), Process.ROLL_PRESSING);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            int iSamplingCount;
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            {
                                foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                                {
                                    iSamplingCount = 1;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.ROLL_PRESSING, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            }
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.ROLL_PRESSING);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Process.ROLL_PRESSING;
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // 수작업 모드
        private void btnManualMode_Click(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();

            foreach (Button button in Util.FindVisualChildren<Button>(mainGrid))
                listAuth.Add(button);

            btnExtra.IsDropDownOpen = false;
            SetManualMode(listAuth);

            if (btnStart.Visibility != Visibility.Visible)
            {
                Util.MessageValidation("SFU5142");  //수작업 모드를 진행할 권한이 없습니다.(엔지니어에게 문의 바랍니다.)
                return;
            }
        }

        // 설비 특이 사항
        private void btnEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = Process.ROLL_PRESSING;
                Parameters[3] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                Parameters[4] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                Parameters[5] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[6] = Util.NVC(txtShift.Text);
                Parameters[7] = Util.NVC(txtShift.Tag);
                Parameters[8] = Util.NVC(txtWorker.Text);
                Parameters[9] = Util.NVC(txtWorker.Tag);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        // 작업 조건 등록
        private void btnEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            CMM_ELEC_EQPT_COND wndPopup = new CMM_ELEC_EQPT_COND();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.Text);
                Parameters[2] = Process.ROLL_PRESSING;
                Parameters[3] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnWipDataCollect_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipment.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return;
            //}

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM wndLotIssue = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM();
            wndLotIssue.FrameOperation = FrameOperation;

            if (wndLotIssue != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = Process.ROLL_PRESSING;
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndLotIssue, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndLotIssue.ShowModal()));
            }
        }

        // W/O 예약
        private void btnReservation_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION wndReservation = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION();
            wndReservation.FrameOperation = FrameOperation;

            if (wndReservation != null)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];

                if (wo.dgWorkOrder != null && wo.dgWorkOrder.Rows.Count > 0)
                {
                    Parameters[0] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRJT_NAME");
                    Parameters[1] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PROD_VER_CODE");
                    Parameters[2] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "WOID");
                    Parameters[3] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRODID");
                    Parameters[4] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "ELECTYPE");
                    Parameters[5] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "LOTYNAME");
                    Parameters[6] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "EQPTID");
                    Parameters[7] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[8] = Process.ROLL_PRESSING;
                    Parameters[9] = (wo.dgWorkOrder.ItemsSource as DataView).ToTable();
                }

                C1WindowExtension.SetParameters(wndReservation, Parameters);
                wndReservation.Closed += WndReservation_Closed;

                this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
            }
        }

        private void WndReservation_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION window = sender as LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // LOT SPLIT
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LOTCUT wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LOTCUT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = Process.ROLL_PRESSING;

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // 롤프레스 공정 리턴
        private void btnProcReturn_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_RP_RETURN wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_RP_RETURN();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                C1WindowExtension.SetParameters(wndPopup, null);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // 샘플링 대상 팝업 (샘플링 출하 법인 지정)
        private void btnSamplingProdT1_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = Process.ROLL_PRESSING;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // 무지부 방향 설정
        private void btnWorkHalfSlitSide_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_ELEC_WORK_HALF_SLITTING popupWorkHalfSlitting = new CMM_ELEC_WORK_HALF_SLITTING();
            popupWorkHalfSlitting.FrameOperation = FrameOperation;

            if (popupWorkHalfSlitting != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                parameters[1] = txtWorkHalfSlittingSide.Tag;   // 무지부 방향

                C1WindowExtension.SetParameters(popupWorkHalfSlitting, parameters);
                popupWorkHalfSlitting.Closed += popupWorkHalfSlitting_Closed;

                this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitting.ShowModal()));
            }
        }

        private void popupWorkHalfSlitting_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_WORK_HALF_SLITTING window = sender as CMM_ELEC_WORK_HALF_SLITTING;
            if (window.DialogResult == MessageBoxResult.OK)
                GetWorkHalfSlittingSide();
        }

        // 착공 취소 LOT 재 생성
        private void btnCancelDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[3];
                Parameters[0] = Process.ROLL_PRESSING; //PROCESS ID
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(OnCloseCancelDeleteLot);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseCancelDeleteLot(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // 전수 불량 등록
        private void btnSaveRegDefectLane_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                    return;
                }

                if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
                {
                    Util.MessageValidation("SFU3723");  //작업 가능한 상태가 아닙니다.
                    return;
                }

                LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[6];
                    Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CUT_ID"]);

                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();

        }

        // 임시 저장 기능
        private void btnSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                    return;

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                // 진행LOT이 실적 확정 완료이면 저장 전체 방어
                if (ValidConfirmLotCheck() == false)
                {
                    Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                    return;
                }

                if (txtInputQty.Value <= 0)
                {
                    SaveWIPHistory();
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 작업자 선택
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.ROLL_PRESSING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플래그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }

        // 좌측 확장 버튼
        private void btnLeftExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualWidth != 0)
                ExpandFrame = Content.ColumnDefinitions[0].Width;

            if (btnLeftExpandFrame.IsChecked == true)
            {
                Content.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Content.ColumnDefinitions[0].Width = ExpandFrame;
            }
        }

        // 상하 확장 버튼
        private void btnExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualHeight != 0)
                ExpandFrame = Content.RowDefinitions[1].Height;
            if (btnExpandFrame.IsChecked == true)
            {
                Content.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                Content.RowDefinitions[1].Height = ExpandFrame;
            }
        }

        // CWA 불량등록 필터 그리드 사이즈 조절
        private void chkDefectFilter_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (_CURRENT_LOTINFO.Rows.Count < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                //CWA 불량등록 필터 그리드
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }
        }

        // TAB 항목 전체 저장
        private void btnPublicWipSave_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                if (isChangeReason == true)
                    SaveDefect(dgWipReason);

                if (isChangeQuality == true)
                    SaveQuality(dgQuality);

                if( isChangeColorTag == true)
                    SaveColorTag(dgColor);

                if (isChangeRemark == true)
                    SaveWipNote();

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 불량/Loss/물청 저장
        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                SaveDefect(dgWipReason);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 품질항목 저장
        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                SaveQuality(dgQuality);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 색지정보 저장
        private void btnSaveColor_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                SaveColorTag(dgColor);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 특이사항 저장
        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            try
            {
                SaveWipNote();

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 공정 합권 조회
        private void btnSearchMerge_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");    //Lot을 선택 하세요
                return;
            }

            GetMergeList();
            GetMergeEndList();
        }

        // 공정 합권 저장
        private void btnSaveMerge_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));
            dt2.Columns.Add("PR_LOTID", typeof(string));

            DataRow dataRow2 = dt2.NewRow();
            dataRow2["LOTID"] = Util.NVC(txtMergeInputLot.Text);

            dt2.Rows.Add(dataRow2);

            DataTable prodLotresult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt2);

            DataTable dt = ((DataView)dgWipMerge.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["CHK"].ToString().Equals("1"))
                {
                    if (prodLotresult.Rows[0]["LANE_QTY"].ToString() != dt.Rows[i]["LANE_QTY"].ToString())
                    {
                        Util.MessageInfo("SFU5081");
                        return;
                    }
                    if (prodLotresult.Rows[0]["MKT_TYPE_CODE"].ToString() != dt.Rows[i]["MKT_TYPE_CODE"].ToString())
                    {
                        Util.MessageInfo("SFU4271");
                        return;
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[i]["WH_ID"].ToString()))
                    {
                        Util.MessageInfo("SFU2963");
                        return;
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[i]["ABNORM_FLAG"].ToString()) && dt.Rows[i]["ABNORM_FLAG"].ToString().Equals("Y"))
                    {
                        Util.MessageInfo("SFU7029");
                        return;
                    }
                }

                DataTable prodLotresult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt2);
                if (!string.IsNullOrEmpty(prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString()) && prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                {
                    Util.MessageValidation("SFU7029");  //전수불량레인이 존재하여 합권취 불가합니다.
                    return;
                }

                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1381");   //Lot을 선택 하세요
                    return;
                }


                if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.PROC))
                {
                    Util.MessageValidation("SFU3627");  //합권취는 진행 상태에서만 가능합니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtMergeInputLot.Text))
                {
                    Util.MessageValidation("SFU1945");  //투입 LOT이 없습니다.
                    return;
                }

                C1DataGrid dg = dgWipMerge;
                if (Util.gridGetChecked(ref dg, "CHK").Length <= 0)
                {
                    Util.MessageValidation("SFU3628");  //합권취 진행할 대상 Lot들이 선택되지 않았습니다.
                    return;
                }
            }
            SaveMergeData();
        }
        #endregion

        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Process.ROLL_PRESSING;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                    ConfirmDispatcher(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "LOTID"));
                        //newRow["WIPSEQ"] = null;
                        newRow["WORKER_NAME"] = sWrokerName;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                    }
                }
                
                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}