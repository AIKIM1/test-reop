/*************************************************************************************
 Created Date : 2018.01.02
      Creator : 신광희
   Decription : 이동 Tray 활성화 입고 
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.02  DEVELOPER : Initial Created.
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public ASSY002_022()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            combo.SetCombo(cboFromArea, CommonCombo.ComboStatus.SELECT, sCase: "MOVETOAREA");

            //원각 : CR , 초소형 : CS
            const string gubun = "CR";
            String[] sFilter = { LoginInfo.CFG_AREA_ID, gubun, Process.WASHING };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");

            // 특별 TRAY  사유 Combo
            string[] filterTraySplReason = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboTrayMoveSplReason, CommonCombo.ComboStatus.SELECT, sFilter: filterTraySplReason, sCase: "SpecialResonCodebyAreaCode");

            if (cboTrayMoveSplReason?.Items != null && cboTrayMoveSplReason.Items.Count > 0)
                cboTrayMoveSplReason.SelectedIndex = 0;
            
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateTo.SelectedDateTime = GetSystemTime();
            dtpDateFrom.SelectedDateTime = GetSystemTime().AddDays(-7);
        }        
        #endregion

        #region Event
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }        



        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            GetMoveLotList();
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

                        //대상 Tray 리스트 조회
                        GetMoveTrayList(idx);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMoveTrayConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirmCancel()) return;
            MoveTrayConfirmCancel();
        }

        private void btnMoveTrayConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm()) return;
            MoveTrayConfirm();
        }

        private void dgMoveTray_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgMoveTray_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderWait.Tag))
                    {
                        e.Cell.Presenter.Background = borderWait.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderAssyOut.Tag))
                    {
                        e.Cell.Presenter.Background = borderAssyOut.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderFormIn.Tag))
                    {
                        e.Cell.Presenter.Background = borderFormIn.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
                
            }));
        }

        private void dgMoveTray_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgMoveTray_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgMoveTray_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgMoveTray_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgMoveTray);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgMoveTray);
        }

        private void chkTrayMoveSpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtTrayMoveRemark != null)
            {
                txtTrayMoveRemark.Text = string.Empty;
            }
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch()) return;
                GetMoveLotList();
            }
        }

        private void txtTray_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch()) return;
                GetMoveLotList();
            }
        }
        #endregion

        #region Method

        private void GetMoveLotList()
        {
            try
            {
                ShowLoadingIndicator();

                string selectedMoveOrderCode = string.Empty;

                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");

                    if (rowIdx >= 0)
                    {
                        selectedMoveOrderCode = DataTableConverter.GetValue(dgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    }
                }

                const string bizRuleName = "DA_BAS_SEL_AREA_MOVE_LOT_LIST_WS";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("TO_AREAID", typeof(string));
                inTable.Columns.Add("FROM_AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));


                string fromMonth = dtpDateFrom.SelectedDateTime.Month.GetString();
                if (dtpDateFrom.SelectedDateTime.Month.GetString().Length < 2)
                    fromMonth = "0" + dtpDateFrom.SelectedDateTime.Month.GetString();

                string fromDay = dtpDateFrom.SelectedDateTime.Day.GetString();
                if(dtpDateFrom.SelectedDateTime.Day.GetString().Length < 2)
                    fromDay = "0" + dtpDateFrom.SelectedDateTime.Day.GetString();

                string fromDate = dtpDateFrom.SelectedDateTime.Year.GetString() + "-" + fromMonth + "-" + fromDay;

                string toMonth = dtpDateTo.SelectedDateTime.Month.GetString();
                if (dtpDateTo.SelectedDateTime.Month.GetString().Length < 2)
                    toMonth = "0" + dtpDateTo.SelectedDateTime.Month.GetString();

                string toDay = dtpDateTo.SelectedDateTime.Day.GetString();
                if (dtpDateTo.SelectedDateTime.Day.GetString().Length < 2)
                    toDay = "0" + dtpDateTo.SelectedDateTime.Day.GetString();

                string toDate = dtpDateTo.SelectedDateTime.Year.GetString() + "-" + toMonth + "-" + toDay;

                DataRow dr = inTable.NewRow();
                dr["FROM_DATE"] = fromDate;
                dr["TO_DATE"] = toDate;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_AREAID"] = cboFromArea.SelectedValue;
                dr["LOTID"] = txtLot.Text.Trim();
                dr["TRAYID"] = txtTray.Text.Trim();
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                    }
                    else
                    {
                        Util.GridSetData(dgProductLot, bizResult, FrameOperation, false);

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            if (!string.IsNullOrEmpty(selectedMoveOrderCode))
                            {
                                int idx = _util.GetDataGridRowIndex(dgProductLot, "LOTID", selectedMoveOrderCode);
                                if (idx >= 0)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                    dgProductLot.SelectedIndex = idx;
                                    GetMoveTrayList(idx);
                                }
                            }
                            else
                            {
                                const int idx = 0;
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                dgProductLot.SelectedIndex = idx;
                                GetMoveTrayList(idx);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetMoveTrayList(int rowIndex)
        {
            if (rowIndex < 0 || !_util.GetDataGridCheckValue(dgProductLot, "CHK", rowIndex)) return;
            SetDataGridCheckHeaderInitialize(dgMoveTray);
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_BAS_SEL_AREA_MOVE_TRAY_LIST_WS";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MOVE_ORD_ID"));
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                    }
                    else
                    {
                        dgMoveTray.ItemsSource = DataTableConverter.Convert(bizResult);
                        //특별TRAY 콤보
                        DataTable dt = new DataTable();
                        dt.Columns.Add("CODE");
                        dt.Columns.Add("NAME");

                        dt.Rows.Add("N", "N");
                        dt.Rows.Add("Y", "Y");

                        var dataGridComboBoxColumn = dgMoveTray.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                        if (dataGridComboBoxColumn != null)
                            dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dt.Copy());

                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MoveTrayConfirmCancel()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS_FOR_AREA_MOVE";

                DataSet ds = new DataSet();

                DataTable inEqpTable = ds.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("SRCTYPE", typeof(string));
                inEqpTable.Columns.Add("IFMODE", typeof(string));
                inEqpTable.Columns.Add("EQSGID", typeof(string));
                inEqpTable.Columns.Add("USERID", typeof(string));

                DataTable inCstTable = ds.Tables.Add("IN_CST");
                inCstTable.Columns.Add("OUT_LOTID", typeof(string));
                inCstTable.Columns.Add("CSTID", typeof(string));
                inCstTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                DataTable inSpclTable = ds.Tables.Add("IN_SPCL");
                inSpclTable.Columns.Add("SPCL_FLAG", typeof(string));
                inSpclTable.Columns.Add("SPCL_RSNCODE", typeof(string));
                inSpclTable.Columns.Add("SPCL_NOTE", typeof(string));

                DataRow drEqpt = inEqpTable.NewRow();
                drEqpt["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drEqpt["IFMODE"] = IFMODE.IFMODE_OFF;
                drEqpt["EQSGID"] = cboEquipmentSegment.SelectedValue;
                drEqpt["USERID"] = LoginInfo.USERID;
                inEqpTable.Rows.Add(drEqpt);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgMoveTray.Rows)
                {
                    if (row.Type != DataGridRowType.Item || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;
                    DataRow drCst = inCstTable.NewRow();
                    drCst["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                    drCst["CSTID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                    drCst["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetString())  ? 0 : DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();
                    inCstTable.Rows.Add(drCst);
                }

                //if (chkTrayMoveSpl.IsChecked == true)
                //{
                //    DataRow drSpcl = inSpclTable.NewRow();
                //    drSpcl["SPCL_FLAG"] = "Y";
                //    drSpcl["SPCL_RSNCODE"] = cboTrayMoveSplReason.SelectedValue;
                //    drSpcl["SPCL_NOTE"] = txtTrayMoveRemark.Text;
                //    inSpclTable.Rows.Add(drSpcl);
                //}

                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST,IN_SPCL", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetMoveLotList();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                }, ds);


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MoveTrayConfirm()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_END_OUT_LOT_WS_FOR_AREA_MOVE";

                DataSet ds = new DataSet();

                DataTable inEqpTable = ds.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("SRCTYPE", typeof(string));
                inEqpTable.Columns.Add("IFMODE", typeof(string));
                inEqpTable.Columns.Add("EQSGID", typeof(string));
                inEqpTable.Columns.Add("USERID", typeof(string));

                DataTable inCstTable = ds.Tables.Add("IN_CST");
                inCstTable.Columns.Add("OUT_LOTID", typeof(string));
                inCstTable.Columns.Add("CSTID", typeof(string));
                inCstTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                DataTable inSpclTable = ds.Tables.Add("IN_SPCL");
                inSpclTable.Columns.Add("SPCL_FLAG", typeof(string));
                inSpclTable.Columns.Add("SPCL_RSNCODE", typeof(string));
                inSpclTable.Columns.Add("SPCL_NOTE", typeof(string));

                DataRow drEqpt = inEqpTable.NewRow();
                drEqpt["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drEqpt["IFMODE"] = IFMODE.IFMODE_OFF;
                drEqpt["EQSGID"] = cboEquipmentSegment.SelectedValue;
                drEqpt["USERID"] = LoginInfo.USERID;
                inEqpTable.Rows.Add(drEqpt);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgMoveTray.Rows)
                {
                    if (row.Type != DataGridRowType.Item || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;
                    DataRow drCst = inCstTable.NewRow();
                    drCst["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                    drCst["CSTID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                    drCst["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetString()) ? 0 : DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();
                    inCstTable.Rows.Add(drCst);
                }

                if (chkTrayMoveSpl.IsChecked == true)
                {
                    DataRow drSpcl = inSpclTable.NewRow();
                    drSpcl["SPCL_FLAG"] = "Y";
                    drSpcl["SPCL_RSNCODE"] = cboTrayMoveSplReason.SelectedValue;
                    drSpcl["SPCL_NOTE"] = txtTrayMoveRemark.Text;
                    inSpclTable.Rows.Add(drSpcl);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST,IN_SPCL", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetMoveLotList();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                }, ds);


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

        private bool ValidationSearch()
        {

            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                Util.MessageValidation("SFU3569");
                return false;
            }

            if (cboFromArea.SelectedIndex < 0 || cboFromArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // "인계동 (을)를 선택하여 주십시오."
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("인계동"));
                return false;
            }

            return true;
        }

        private bool ValidationTrayConfirm()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }


            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgMoveTray, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (chkTrayMoveSpl.IsChecked.HasValue && (bool)chkTrayMoveSpl.IsChecked)
            {
                if (cboTrayMoveSplReason.SelectedValue == null || cboTrayMoveSplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return false;
                }

                if (txtTrayMoveRemark.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return false;
                }
            }

            /*
            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgMoveTray.Rows[rowIndex].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgMoveTray.Rows[rowIndex].DataItem, "CELLQTY")), out dTmp))
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
            */

            return true;
        }

        private bool ValidationTrayConfirmCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgMoveTray, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }



            return true;
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


    }
}
