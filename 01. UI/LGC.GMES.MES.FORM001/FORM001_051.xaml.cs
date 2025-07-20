/*************************************************************************************
 Created Date : 2017.06.29
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 대기창고 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.29  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
 **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_051.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_051 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        //private string _BaseProcess = Process.PolymerDSF;
        private string _BaseProcess = LoginInfo.CFG_PROC_ID;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        #region Popup
        FORM001_051_HISTORY wndHistroy = null;
        FORM001_051_MONITORING wndMonitoring = null;
        FORM001_051_RERECEPTION wndReReception = null;
        FORM001_051_LABEL_PRINT wndLabelPrint = null;
        #endregion Popup

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_051()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            teTimeFrom.Value = new TimeSpan(8, 0, 0);
            teTimeTo.Value = new TimeSpan(20, 0, 0);

            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboDsfWarehouseChild = { cboDsfWarehouseRack };
            String[] sDsfWarehouseFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboDsfWarehouse, CommonCombo.ComboStatus.SELECT, cbChild: cboDsfWarehouseChild, sFilter: sDsfWarehouseFilter);

            C1ComboBox[] cboDsfWarehouseRackParent = { cboDsfWarehouse };
            String[] sDsfWarehouseRackFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboDsfWarehouseRack, CommonCombo.ComboStatus.ALL, cbParent: cboDsfWarehouseRackParent, sFilter: sDsfWarehouseRackFilter);

            String[] sEquipmentSegmentFilter = { LoginInfo.CFG_AREA_ID };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sEquipmentSegmentFilter);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sEquipmentSegmentFilter, sCase: "cboEquipmentSegmentForm");

            if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0)
                cboEquipmentSegment.SelectedIndex = 0;

            chkWip.IsChecked = false;

            #region DSF 재공 Tray 구분콤보
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));
            DataRow dr = dt.NewRow();
            dr["CBO_CODE"] = "WIP";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("재공");
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["CBO_CODE"] = "IN";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("입고");
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["CBO_CODE"] = "OUT";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("출고");
            dt.Rows.Add(dr);
            cboSelect.ItemsSource = dt.Copy().AsDataView();
            cboSelect.SelectedIndex = 0;

            cboSelect.SelectedValueChanged += cboSelect_SelectedValueChanged;
            #endregion
        }

        #endregion

        #region Event

        #region [Main Window]

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _BaseProcess = LoginInfo.CFG_PROC_ID;
            txtProcess.Text = LoginInfo.CFG_PROC_ID + ":" + LoginInfo.CFG_PROC_NAME;
            txtProcess.Padding = new Thickness(20, 0, 20, 0);

            #region Popup
            if (wndHistroy != null)
                wndHistroy.BringToFront();

            if (wndMonitoring != null)
                wndMonitoring.BringToFront();

            if (wndReReception != null)
                wndReReception.BringToFront();

            if (wndLabelPrint != null)
                wndLabelPrint.BringToFront();
            #endregion Popup

        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            // btnExtra.IsDropDownOpen = false;
        }

        private void btnHistorySerch_Click(object sender, RoutedEventArgs e)
        {
            if (wndHistroy != null)
                wndHistroy = null;

            string sWarehouse = Util.NVC(cboDsfWarehouse.SelectedValue);
            if (sWarehouse.Length < 1 || sWarehouse.ToUpper().Equals("SELECT") || sWarehouse.ToUpper().Equals("ALL"))
            {
                Util.MessageValidation("SFU2961"); // 창고를 먼저 선택해주세요
                return;
            }

            wndHistroy = new FORM001_051_HISTORY();
            wndHistroy.FrameOperation = FrameOperation;

            if (wndHistroy != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _BaseProcess;
                Parameters[1] = string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);
                Parameters[2] = string.Format("{0} {1:0#}:59:59", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours);
                Parameters[3] = Util.NVC(cboDsfWarehouse.SelectedValue);
                Parameters[4] = Util.NVC(cboEquipmentSegment.SelectedValue);

                string _WarehouseID = string.Empty;

                C1WindowExtension.SetParameters(wndHistroy, Parameters);
                wndHistroy.Closed += new EventHandler(wndHistroy_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndHistroy.ShowModal()));
            }
        }

        private void btnMonitoring_Click(object sender, RoutedEventArgs e)
        {
            if (wndMonitoring != null)
                wndMonitoring = null;

            string sWarehouse = Util.NVC(cboDsfWarehouse.SelectedValue);
            if (sWarehouse.Length < 1 || sWarehouse.ToUpper().Equals("SELECT") || sWarehouse.ToUpper().Equals("ALL"))
            {
                Util.MessageValidation("SFU2961"); // 창고를 먼저 선택해주세요
                return;
            }

            wndMonitoring = new FORM001_051_MONITORING();
            wndMonitoring.FrameOperation = FrameOperation;

            if (wndMonitoring != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _BaseProcess;
                Parameters[1] = string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);
                Parameters[2] = string.Format("{0} {1:0#}:59:59", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours);
                Parameters[3] = Util.NVC(cboDsfWarehouse.SelectedValue);

                C1WindowExtension.SetParameters(wndMonitoring, Parameters);
                wndMonitoring.Closed += new EventHandler(wndMonitoring_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndMonitoring.ShowModal()));
            }
        }

        private void btnReReception_Click(object sender, RoutedEventArgs e)
        {
            if (wndReReception != null)
                wndReReception = null;

            string sSelLotID = string.Empty;
            if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx >= 0)
                {
                    sSelLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }
            }

            wndReReception = new FORM001_051_RERECEPTION();
            wndReReception.FrameOperation = FrameOperation;

            if (wndReReception != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = _BaseProcess;
                Parameters[1] = sSelLotID;

                C1WindowExtension.SetParameters(wndReReception, Parameters);
                wndReReception.Closed += new EventHandler(wndReReception_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndReReception.ShowModal()));
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (wndLabelPrint != null)
                wndLabelPrint = null;

            string sWarehouse = Util.NVC(cboDsfWarehouse.SelectedValue);
            if (sWarehouse.Length < 1 || sWarehouse.ToUpper().Equals("SELECT") || sWarehouse.ToUpper().Equals("ALL"))
            {
                Util.MessageValidation("SFU2961"); // 창고를 먼저 선택해주세요
                return;
            }

            string sLotID = string.Empty;

            if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx >= 0)
                {
                    sLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }
            }
            if (sLotID.Length < 1)
            {
                Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
                return;
            }

            wndLabelPrint = new FORM001_051_LABEL_PRINT();
            wndLabelPrint.FrameOperation = FrameOperation;

            if (wndLabelPrint != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = _BaseProcess;
                Parameters[1] = Util.NVC(cboDsfWarehouse.SelectedValue);
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridRowIndex(dgProductLot, "LOTID", sLotID)].DataItem, "RACK_ID"));
                Parameters[3] = sLotID;
                Parameters[4] = string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);    // FROM DATE
                Parameters[5] = string.Format("{0} {1:0#}:59:59", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours); // TO DATE
                Parameters[6] = Util.NVC(cboEquipmentSegment.SelectedValue);    // Line ID
                Parameters[7] = Util.NVC(chkWip.IsChecked);    // WIP YN


                C1WindowExtension.SetParameters(wndLabelPrint, Parameters);
                wndLabelPrint.Closed += new EventHandler(wndLabelPrint_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndLabelPrint.ShowModal()));
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);
                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                string dateFrom = string.Format("{0}{1:0#}", dtPik.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, true))
                {
                    dtPik.Text = dtpDateTo.SelectedDateTime.ToLongDateString();
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    teTimeFrom.Value = new TimeSpan(teTimeTo.Value.Value.Ticks);
                    return;
                }
            }));
        }
        private void teTimeFrom_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                C1.WPF.DateTimeEditors.C1TimeEditor teTimePik = (sender as C1.WPF.DateTimeEditors.C1TimeEditor);
                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimePik.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, true))
                {
                    teTimePik.Value = new TimeSpan(teTimeTo.Value.Value.Ticks);
                    return;
                }
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);
                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtPik.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, false))
                {
                    dtPik.Text = dtpDateFrom.SelectedDateTime.ToLongDateString();
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    teTimeTo.Value = new TimeSpan(teTimeFrom.Value.Value.Ticks);
                    return;
                }
            }));
        }
        private void teTimeTo_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                C1.WPF.DateTimeEditors.C1TimeEditor teTimePik = (sender as C1.WPF.DateTimeEditors.C1TimeEditor);
                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimePik.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, false))
                {
                    teTimePik.Value = new TimeSpan(teTimeFrom.Value.Value.Ticks);
                    return;
                }
            }));
        }

        private void chkWip_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    spnSearchPeriod.IsEnabled = !((bool)(sender as CheckBox).IsChecked);
                }
            }
        }
        private void chkWip_Unchecked(object sender, RoutedEventArgs e)
        {
            spnSearchPeriod.IsEnabled = !((bool)(sender as CheckBox).IsChecked);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                return;
            }

            GetProductLot();
            GetTrayInfo();
            //GetRemark();
        }

        #endregion

        #region [Product Lot]

        private void dgProductLot_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;
                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content is RadioButton)
                            {
                                RadioButton rdoButton = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;
                                if (rdoButton != null)
                                {
                                    if (rdoButton.DataContext == null)
                                        return;

                                    if (!(bool)rdoButton.IsChecked && (rdoButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                                    {
                                        for (int i = 0; i < dg.Rows.Count; i++)
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", e.Cell.Row.Index == i);

                                        dg.SelectedIndex = e.Cell.Row.Index;

                                        //GetRemark();
                                        GetTrayInfo();
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && !((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1")))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                dgProductLot.SelectedIndex = idx;

                //GetRemark();
                GetTrayInfo();
            }
        }
        #endregion

        #region [Tary]

        #region Grid All CheckBox
        private C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkTrayAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgTray_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkTrayAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkTrayAll.Checked -= new RoutedEventHandler(chkTrayAll_Checked);
                        chkTrayAll.Unchecked -= new RoutedEventHandler(chkTrayAll_Unchecked);
                        chkTrayAll.Checked += new RoutedEventHandler(chkTrayAll_Checked);
                        chkTrayAll.Unchecked += new RoutedEventHandler(chkTrayAll_Unchecked);
                    }
                }
            }));
        }

        private void dgTray_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            chkTrayAll.Checked -= new RoutedEventHandler(chkTrayAll_Checked);
                            chkTrayAll.Unchecked -= new RoutedEventHandler(chkTrayAll_Unchecked);

                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                            if (!chk.IsChecked.Value)
                            {
                                chkTrayAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkTrayAll.IsChecked = true;
                            }

                            chkTrayAll.Checked += new RoutedEventHandler(chkTrayAll_Checked);
                            chkTrayAll.Unchecked += new RoutedEventHandler(chkTrayAll_Unchecked);
                            break;
                    }
                }
            }
        }

        private void chkTrayAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgTray == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgTray.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                if (Util.NVC(dt.Rows[idx]["WIPDTTM_ED"]).Length > 0)
                    continue;

                dt.Rows[idx]["CHK"] = true;
            }
            dgTray.ItemsSource = DataTableConverter.Convert(dt);

            GetRemark();
        }

        private void chkTrayAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgTray == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgTray.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgTray.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void cboSelect_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboSelect.SelectedValue == null)
                return;

            if (dgProductLot.Rows.Count - dgProductLot.FrozenBottomRowsCount == 0)
                return;

            if (Util.NVC(cboSelect.SelectedValue).Equals("WIP"))
                dgTray.Columns["OUT_TYPE"].Visibility = Visibility.Collapsed;
            else
                dgTray.Columns["OUT_TYPE"].Visibility = Visibility.Visible;

            GetTrayInfo();
        }

        #endregion

        private void dgTray_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPDTTM_ED")).Length > 0)
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));

                        CheckBox chk = dgTray.GetCell(e.Cell.Row.Index, dgTray.Columns["CHK"].Index).Presenter.Content as CheckBox;
                        if (chk != null && chk.IsChecked.HasValue)
                        {
                            chk.IsChecked = false;
                            DataTableConverter.SetValue(dgTray.Rows[e.Cell.Row.Index].DataItem, "CHK", (bool)(chk.IsChecked));
                        }
                    }
                    else
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgTray_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            {
                                if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null)
                                    break;

                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPDTTM_ED")).Length > 0)
                                    break;

                                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                                if (chk == null || !(chk.IsChecked.HasValue))
                                    break;

                                chk.IsChecked = !((bool)(chk.IsChecked));
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", (bool)(chk.IsChecked));

                                if ((bool)(chk.IsChecked))
                                {
                                    dg.SelectedIndex = e.Cell.Row.Index;
                                }

                                GetRemark();
                            }
                            break;
                    }

                    if (dg.CurrentCell != null)
                        //dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void txtInputTrayID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtInputTrayID.Text = txtInputTrayID.Text.Trim();
                    txtInputTrayID.Select(txtInputTrayID.Text.Length, 0);

                    if (txtInputTrayID.Text.Length < 1)
                        return;

                    bool isExistTrayID = false;
                    for (int i = dgTray.TopRows.Count; i < dgTray.Rows.Count - dgTray.BottomRows.Count; i++)
                    {
                        if (txtInputTrayID.Text.Equals(Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "TRAYID")))
                            || txtInputTrayID.Text.Equals(Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "TRAYSEQ"))))
                        {
                            isExistTrayID = true;

                            if (Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "WIPDTTM_ED")).Length > 0)
                                continue;

                            CheckBox chk = dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Presenter.Content as CheckBox;
                            if (chk == null || !(chk.IsChecked.HasValue))
                                continue;

                            chk.IsChecked = !((bool)(chk.IsChecked));
                            DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "CHK", (bool)(chk.IsChecked));
                            dgTray.SelectedIndex = i;
                            break;
                        }
                    }
                    if (!isExistTrayID)
                    {
                        Util.MessageValidation("SFU1430"); // Tray ID 가 없습니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTrayStockOut_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayStockOut())
                return;

            Util.MessageConfirm("SFU3121", (result) => // 출고 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayStockOut();
                }
            });
        }

        #endregion

        #region [Tabs]

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            //string sLotID = string.Empty;

            //if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
            //{
            //    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            //    if (idx >= 0)
            //    {
            //        sLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
            //    }
            //}
            //if (sLotID.Length < 1)
            //{
            //    Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
            //    return;
            //}

            //Util.MessageConfirm("SFU1241", (result) => // 저장하시겠습니까?
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SetRemark(sLotID);
            //    }
            //});

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgTray, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
            }

            Util.MessageConfirm("SFU1241", (result) => // 저장하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemark();
                }
            });
        }

        #endregion

        #region [Popup]

        private void wndHistroy_Closed(object sender, EventArgs e)
        {
            wndHistroy = null;
            C1Window window = sender as C1Window;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndMonitoring_Closed(object sender, EventArgs e)
        {
            wndMonitoring = null;
            C1Window window = sender as C1Window;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndReReception_Closed(object sender, EventArgs e)
        {
            wndReReception = null;
            C1Window window = sender as C1Window;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetProductLot();
            GetTrayInfo();
            //GetRemark();
        }

        private void wndLabelPrint_Closed(object sender, EventArgs e)
        {
            wndLabelPrint = null;
            C1Window window = sender as C1Window;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetProductLot();
            GetTrayInfo();
            //GetRemark();
        }

        #endregion [Popup]

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetProductLot(bool bSelPreviousInfo = true)
        {
            try
            {
                ShowLoadingIndicator();

                string sSelPreviousInfo = string.Empty;
                if (bSelPreviousInfo && dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sSelPreviousInfo = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                Util.gridClear(dgProductLot);

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_STOCK();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _BaseProcess;
                dr["DATE_FROM"] = (bool)(chkWip.IsChecked) ? null : string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);
                dr["DATE_TO"] = (bool)(chkWip.IsChecked) ? null : string.Format("{0} {1:0#}:00:00", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours);
                dr["WH_ID"] = Util.NVC(cboDsfWarehouse.SelectedValue).Equals("") ? null : Util.NVC(cboDsfWarehouse.SelectedValue);
                dr["RACK_ID"] = Util.NVC(cboDsfWarehouseRack.SelectedValue).Equals("") ? null : Util.NVC(cboDsfWarehouseRack.SelectedValue);
                dr["LINEID"] = Util.NVC(cboEquipmentSegment.SelectedValue).Equals("") ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PRODID"] = !string.IsNullOrEmpty(txtProductId.Text) ? txtProductId.Text : null;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_DSF_STOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && !(searchResult.Columns.Contains("CHK")))
                            searchResult.Columns.Add("CHK", typeof(decimal));

                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (bSelPreviousInfo && sSelPreviousInfo.Length > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sSelPreviousInfo);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                dgProductLot.SelectedIndex = idx;
                                dgProductLot.CurrentCell = dgProductLot.GetCell(idx, dgProductLot.Columns["CHK"].Index);

                                GetTrayInfo();
                                //GetRemark();
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo(bool bSelPreviousInfo = false)
        {
            try
            {
                txtInputTrayID.Text = string.Empty;

                int idxChoiceLot = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idxChoiceLot < 0)
                {
                    Util.gridClear(dgTray);
                    return;
                }

                List<string> sSelPreviousInfo = new List<string>();
                if (bSelPreviousInfo && dgTray.ItemsSource != null && dgTray.Rows.Count > 0)
                {
                    for (int i = dgTray.TopRows.Count; i < dgTray.Rows.Count - dgTray.BottomRows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "CHK")))
                        {
                            string sTrayID = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "TRAYID"));
                            if (sTrayID.Length > 0)
                                sSelPreviousInfo.Add(sTrayID);
                        }
                    }
                }

                Util.gridClear(dgTray);

                //DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_STOCK();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WH_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                if (!Util.NVC(cboSelect.SelectedValue).Equals("WIP"))
                {
                    inTable.Columns.Add("IN_Y", typeof(string));
                    inTable.Columns.Add("OUT_Y", typeof(string));
                    inTable.Columns.Add("DATE_FROM", typeof(string));
                    inTable.Columns.Add("DATE_TO", typeof(string));
                }

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; ;
                dr["PROCID"] = _BaseProcess;
                dr["WH_ID"] = DataTableConverter.GetValue(dgProductLot.Rows[idxChoiceLot].DataItem, "WH_ID").ToString();
                dr["LOTID"] = DataTableConverter.GetValue(dgProductLot.Rows[idxChoiceLot].DataItem, "LOTID").ToString();
                if (!Util.NVC(cboSelect.SelectedValue).Equals("WIP"))
                {
                    dr["IN_Y"] = Util.NVC(cboSelect.SelectedValue).Equals("IN") ? "Y" : null;
                    dr["OUT_Y"] = Util.NVC(cboSelect.SelectedValue).Equals("OUT") ? "Y" : null;
                    dr["DATE_FROM"] = string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);                //                spn = ((TimeSpan)teTimeTo.Value);
                    dr["DATE_TO"] = string.Format("{0} {1:0#}:00:00", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours);
                }
                inTable.Rows.Add(dr);

                string BizRuleName = string.Empty;
                if (Util.NVC(cboSelect.SelectedValue).Equals("WIP"))
                {
                    BizRuleName = "DA_PRD_SEL_TRAY_DSF_STOCK_WIP";
                }
                else
                {
                    BizRuleName = "DA_PRD_SEL_TRAY_DSF_STOCK_INOUT";
                }

                new ClientProxy().ExecuteService(BizRuleName, "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        rtxRemark.Document.Blocks.Clear();

                        if (searchResult != null && !(searchResult.Columns.Contains("CHK")))
                            searchResult.Columns.Add("CHK", typeof(decimal));

                        Util.GridSetData(dgTray, searchResult, FrameOperation, true);

                        DataGridAggregate.SetAggregateFunctions(dgTray.Columns["TRAYID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = searchResult.Rows.Count.ToString("###,###") } });

                        if (bSelPreviousInfo && sSelPreviousInfo.Count > 0)
                        {
                            foreach (string selectionInfo in sSelPreviousInfo)
                            {
                                int idx = _Util.GetDataGridRowIndex(dgTray, "TRAYID", selectionInfo);
                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgTray.Rows[idx].DataItem, "CHK", true);
                            }
                            GetRemark();
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TrayStockOut()
        {
            try
            {
                ShowLoadingIndicator();

                DataRow newRow = null;
                DataSet inDataSet = _Biz.GetBR_PRD_REG_TRAY_STOCK_OUT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = null;
                DataTable inputLOT = inDataSet.Tables["INLOT"];
                for (int i = dgTray.TopRows.Count; i < dgTray.Rows.Count - dgTray.BottomRows.Count; i++)
                {
                    if (!Convert.ToBoolean(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "CHK")))
                        continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "WIPDTTM_ED")).Length > 0)
                        continue;

                    newRow = inputLOT.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "TRAYID"));

                    inputLOT.Rows.Add(newRow);
                }
                //string xml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TRAY_STOCK_OUT", "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        GetTrayInfo(false);
                        //GetRemark();

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageValidation("SFU1275");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetRemark()
        {
            try
            {
                rtxRemark.Document.Blocks.Clear();

                string sLotID = string.Empty;

                //if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                //{
                //    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                //    if (idx >= 0)
                //    {
                //        sLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                //    }
                //}
                //if (sLotID.Length < 1)
                //{
                //    return;
                //}

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgTray, "CHK");

                if (idx < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "LOTID"));
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "WIPSEQ"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

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

        private void SetRemark()//string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_UPD_LOT_REMARK();

                for (int i = 0; i < dgTray.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgTray, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[i].DataItem, "WIPSEQ"));
                    newRow["WIP_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                //DataRow newRow = inTable.NewRow();
                //newRow["LOTID"] = sLotID;
                //newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridRowIndex(dgProductLot, "LOTID", sLotID)].DataItem, "WIPSEQ"));
                //newRow["WIP_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                //newRow["USERID"] = LoginInfo.USERID;

                //inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1270"); // 저장되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
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

        #region [Validation]

        private bool CanSearchDateTime(string dateFrom, string dateTo, bool byFromDate)
        {
            bool bRet = false;

            if (Convert.ToDecimal(dateFrom) > Convert.ToDecimal(dateTo))
            {
                Util.MessageValidation(byFromDate ? "SFU3231" : "SFU3231"); // [SFU3231:종료시간이 시작시간보다 이전입니다.], [SFU1698:시작일자 이전 날짜는 선택할 수 없습니다.]
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSearch()
        {
            bool bRet = false;
            
            if (cboDsfWarehouse.SelectedIndex < 0 || cboDsfWarehouse.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU2961"); // 창고를 먼저 선택해주세요
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayStockOut()
        {
            bool bRet = false;

            int idxChoice = _Util.GetDataGridCheckFirstRowIndex(dgTray, "CHK");
            if (idxChoice < 0)
            {
                Util.MessageValidation("SFU3017"); // 출고 대상이 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]

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
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnHistorySerch);
            listAuth.Add(btnMonitoring);
            listAuth.Add(btnReReception);
            listAuth.Add(btnPrint);

            listAuth.Add(btnTrayStockOut);
            listAuth.Add(btnSaveRemark);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #endregion

        private void dgProductLot_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {

            var i = e.AddedRanges.Rows;
            var j = e.RemovedRanges.Rows;

            return;
        }
    }
}
