/*************************************************************************************
 Created Date : 2021.04.01
      Creator : 고재영 (jyking)
   Decription : 한계 불량률 조치이력
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.01  고재영 : 최초 생성
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_356 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public COM001_356()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            DateTime systemDateTime = GetSystemTime();

            if (dpStart != null)
                dpStart.SelectedDateTime = systemDateTime;

            if (dpEnd != null)
                dpEnd.SelectedDateTime = systemDateTime.AddDays(+1);

            if (teStart != null)
                teStart.Value = new TimeSpan(0, 0, 0, 0, 0);

            if (teEnd != null)
                teEnd.Value = new TimeSpan(0, 0, 0, 0, 0);
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            // 2020.11.17 오화백 : ALL-> SELECT로 변경
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, null, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);

            if (cboProcess.Items.Count < 1)
                SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControls();

            dpStart.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dpEnd.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetEquipmentAlarmHistoryList();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcess();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
                Util.gridClear(dgEquipmentAlarmHistoryList);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpStart.SelectedDateTime.Year > 1 && dpEnd.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays < 0)
                {
                    dpStart.SelectedDateTime = dpEnd.SelectedDateTime;
                    return;
                }
            }
        }

        private void txtTroubleCode_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            if (1 == 1)
            {
                //해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?

                COM001_356_RELEASE_LIMIT_DFCT wndConfirm = new COM001_356_RELEASE_LIMIT_DFCT();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "EQPT_ALARM_SEQNO"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "EQPTID"));
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "EQPTNAME"));

                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "ACTDTTM"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "EQPT_ALARM_CODE"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgEquipmentAlarmHistoryList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEquipmentAlarmHistoryList, "CHK")].DataItem, "EQPT_ALARM_NAME"));

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndRelease_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
                    
                
            }

        }

        private void wndRelease_Closed(object sender, EventArgs e)
        {
            GetEquipmentAlarmHistoryList();
        }

        private void dgEquipmentAlarmHistoryList_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgEquipmentAlarmHistoryList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));
        }

        private void dgEquipmentAlarmHistoryList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (e.Cell != null &&
                        e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null)
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

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

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

        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string areaCode = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(areaCode))
                    return;

                string processCode = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string equipmentSegmentCode = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = areaCode;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegmentCode) ? null : equipmentSegmentCode;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);
                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static void SetEioStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EIOSTATE_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }


        private void GetEquipmentAlarmHistoryList()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_EQPT_ALARM_HIST_FOR_LIMIT_DFCT";

                DateTime dtStartTime;
                var fromTimeSpan = (TimeSpan)teStart.Value;
                dtStartTime = new DateTime(dpStart.SelectedDateTime.Year, dpStart.SelectedDateTime.Month, dpStart.SelectedDateTime.Day, fromTimeSpan.Hours, fromTimeSpan.Minutes, fromTimeSpan.Seconds, DateTimeKind.Local);

                DateTime dtEndTime;
                var toTimeSpan = (TimeSpan)teEnd.Value;
                dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year, dpEnd.SelectedDateTime.Month, dpEnd.SelectedDateTime.Day, toTimeSpan.Hours, toTimeSpan.Minutes, toTimeSpan.Seconds, DateTimeKind.Local);

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("DATETIME_FROM", typeof(DateTime));
                inTable.Columns.Add("DATETIME_TO", typeof(DateTime));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPT_ALARM_CODE", typeof(string));         //설비알람코드
                inTable.Columns.Add("AREAID", typeof(string));         //동
                inTable.Columns.Add("EQSGID", typeof(string));         //라인
                inTable.Columns.Add("PROCID", typeof(string));         //공정


                DataRow dr = inTable.NewRow();
                dr["DATETIME_FROM"] = dtStartTime;
                dr["DATETIME_TO"] = dtEndTime;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString() == string.Empty ? null : cboEquipment.SelectedValue;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_ALARM_CODE"] = !string.IsNullOrEmpty(txtEquipmentAlarmCode.Text.Trim()) ? txtEquipmentAlarmCode.Text : null;
                //2020 11.17 오화백 : DA 파라미터 추가
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["PROCID"] = cboProcess.SelectedValue;


                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEquipmentAlarmHistoryList, bizResult, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Function

        private bool ValidationSearch()
        {
            //if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays > 31)
            //{
            //    // 기간은 {0}일 이내 입니다.
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

            if ((dpEnd.SelectedDateTime - dpStart.SelectedDateTime).TotalDays > 8)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return false;
            }
            // 2020 11 17 오화백 : 라인 정보 체크
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을선택하세요
                Util.MessageValidation("SFU1499");
                return false;
            }

            //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return false;
            //}

            if (teEnd.Value != null && teStart != null)
            {
                TimeSpan tsEnd = (TimeSpan)teEnd.Value;
                DateTime dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year
                    , dpEnd.SelectedDateTime.Month
                    , dpEnd.SelectedDateTime.Day
                    , tsEnd.Hours
                    , tsEnd.Minutes
                    , tsEnd.Seconds
                    , DateTimeKind.Local);

                TimeSpan tsStart = (TimeSpan)teStart.Value;
                DateTime dtStartTime = new DateTime(dpStart.SelectedDateTime.Year
                    , dpStart.SelectedDateTime.Month
                    , dpStart.SelectedDateTime.Day
                    , tsStart.Hours
                    , tsStart.Minutes
                    , tsStart.Seconds
                    , DateTimeKind.Local);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //Util.MessageValidation("종료시간이 시작시간보다 전 시간 일 수 없습니다.");
                    Util.MessageValidation("SFU3037");
                    return false;
                }
            }

            return true;
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