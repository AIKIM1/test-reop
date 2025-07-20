/*************************************************************************************
 Created Date : 2021.01.25
      Creator : 조영대
   Decription : 설비 반송요청현황
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.25  조영대 : Initial Created.
  2021.04.12  조영대 : 요청 반송유형, 반송요청상태, 요청설비를 멀티선택콤보로 수정
  2021.04.30  조영대 : TB_MHS_TRF_CMD_TRGT_CST Join 추가
  2023.10.10  김태우 : NFF 보빈ID, 캐리어ID 대신 대표랏(REP_LOT)사용 하는경우 적용
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_054 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private System.Windows.Threading.DispatcherTimer refreshTimer = null;

        private readonly Util _util = new Util();

        public MCS001_054()
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

        private void Initialize()
        {
            ApplyPermissions();

            InitializeControls();
            InitializeCombo();
        }

        private void InitializeControls()
        {
        }

        private void InitializeCombo()
        {
            CommonCombo comboSet = new CommonCombo();
            String[] sFilter = { string.Empty };

            //동
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 요청 반송유형
            cboReqReturnType.SetCommonCode("MHS_PORT_TYPE_CODE", true);

            // 상태
            cboState.SetCommonCode("MHS_REQ_STAT_CODE", true);

            // 조회 주기
            cboInquiryCycle.SetCommonCode("INTERVAL_MIN", CommonCombo.ComboStatus.NA, false);
        }

        #endregion

        #region Event
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            Loaded -= UserControl_Loaded;
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            SetReqEqptCombo();
            RepLotUseForm();
        }

        private void cboReqReturnType_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        private void cboState_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        private void cboInquiryCycle_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }

        private void cboReqEquipment_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        private void btnRtdRunLog_Click(object sender, RoutedEventArgs e)
        {
            if (!dgStatus.IsCheckedRow("CHK"))
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return;
            }

            int selectRow = dgStatus.GetCheckedRowIndex("CHK").First();

            // 팝업 호출
            MCS001_054_RTD_RUN_LOG popup = new MCS001_054_RTD_RUN_LOG { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = Util.NVC(dgStatus.GetValue(selectRow, "RTD_EXEC_LOG_CNTT"));
            C1WindowExtension.SetParameters(popup, parameters);

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }

        private void btnRtdRunLogHist_Click(object sender, RoutedEventArgs e)
        {
            if (dgHistory.CurrentRow == null || dgHistory.SelectedIndex < 0)
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return;
            }

            // 팝업 호출
            MCS001_054_RTD_RUN_LOG popup = new MCS001_054_RTD_RUN_LOG { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = Util.NVC(dgHistory.GetValue("RTD_EXEC_LOG_CNTT"));
            C1WindowExtension.SetParameters(popup, parameters);

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }
        
        private void popupBatchUse_Closed(object sender, EventArgs e)
        {
            MCS001_054_BATCH_USE popup = sender as MCS001_054_BATCH_USE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnReqAdd_Click(object sender, RoutedEventArgs e)
        {
            // 팝업 호출
            MCS001_054_ADD_REQUEST popup = new MCS001_054_ADD_REQUEST { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = "EQPT_REQ";
            C1WindowExtension.SetParameters(popup, parameters);

            popup.Closed += popupAddRequest_Closed;

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }

        private void popupAddRequest_Closed(object sender, EventArgs e)
        {
            MCS001_054_ADD_REQUEST popup = sender as MCS001_054_ADD_REQUEST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnReqDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!dgStatus.IsCheckedRow("CHK"))
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return;
            }

            int selectRow = dgStatus.GetCheckedRowIndex("CHK").First();

            // 팝업 호출
            MCS001_054_DEL_REQUEST popup = new MCS001_054_DEL_REQUEST { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = dgStatus.GetDataRow(selectRow);
            C1WindowExtension.SetParameters(popup, parameters);

            popup.Closed += popupDelRequest_Closed;

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }

        private void popupDelRequest_Closed(object sender, EventArgs e)
        {
            MCS001_054_DEL_REQUEST popup = sender as MCS001_054_DEL_REQUEST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void dtpStart_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //SetReqEqptCombo();
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                dtpStart.IsEnabled = false;
            }
            else
            {
                dtpStart.IsEnabled = true;
            }

            //SetReqEqptCombo();
        }

        private void dgStatus_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgStatus.CurrentRow == null || dgStatus.SelectedIndex < 0) return;
            if (!dgStatus.IsClickedCell()) return;

            tabMain.SelectedIndex = 1;

            SelectHistoryList(true);
        }

        private void dgStatus_CheckedChanged(object sender, RoutedEventArgs e)
        {
            dgStatus.SelectedIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;
        }


        private void dgStatus_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid grid = sender as C1DataGrid;
                if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                {
                    var _mergeList = new List<DataGridCellsRange>();

                    int saveCmdSeq = Util.StringToInt(grid.GetValue(0, "REQ_SEQNO").ToString());
                    int mergeStart = 0, mergeEnd = 0;
                    for (int row = 0; row < grid.Rows.Count; row++)
                    {
                        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) &&
                            Util.NVC(grid.GetValue(row - 1, "REQ_SEQNO")) != Util.NVC(grid.GetValue(row, "REQ_SEQNO")))
                        {
                            break;
                        }

                        if (!saveCmdSeq.Equals(Util.StringToInt(grid.GetValue(row, "REQ_SEQNO").ToString())) ||
                            row.Equals(grid.Rows.Count - 1))
                        {
                            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                            if (mergeStart < mergeEnd)
                            {
                                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                                {
                                    switch (dgCol.Name)
                                    {
                                        case "CST_LOAD_LOCATION_CODE":
                                        case "CSTID":
                                        case "LOTID":
                                            break;
                                        default:
                                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                                            break;
                                    }

                                }
                            }
                            mergeStart = row;
                            saveCmdSeq = Util.StringToInt(grid.GetValue(row, "REQ_SEQNO").ToString());
                        }
                    }
                    foreach (var range in _mergeList)
                    {
                        e.Merge(range);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid grid = sender as C1DataGrid;
                if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                {
                    var _mergeList = new List<DataGridCellsRange>();

                    string saveCmdSeq = Util.NVC(grid.GetValue(0, "REQ_SEQNO")) + Util.NVC(grid.GetValue(0, "HIST_SEQNO"));
                    
                    int mergeStart = 0, mergeEnd = 0;
                    for (int row = 1; row < grid.Rows.Count; row++)
                    {
                        string beforeCmdSeq = Util.NVC(grid.GetValue(row - 1, "REQ_SEQNO")) + Util.NVC(grid.GetValue(row - 1, "HIST_SEQNO"));
                        string currentCmdSeq = Util.NVC(grid.GetValue(row, "REQ_SEQNO")) + Util.NVC(grid.GetValue(row, "HIST_SEQNO"));

                        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) && beforeCmdSeq != currentCmdSeq)
                        {
                            break;
                        }

                        if (!saveCmdSeq.Equals(currentCmdSeq) ||
                            row.Equals(grid.Rows.Count - 1))
                        {
                            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                            if (mergeStart < mergeEnd)
                            {
                                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                                {
                                    switch (dgCol.Name)
                                    {
                                        case "CST_LOAD_LOCATION_CODE":
                                        case "CSTID":
                                        case "LOTID":
                                            break;
                                        default:
                                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                                            break;
                                    }

                                }
                            }
                            mergeStart = row;
                            saveCmdSeq = currentCmdSeq;
                        }
                    }
                    foreach (var range in _mergeList)
                    {
                        e.Merge(range);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnReqAdd,
                btnReqDelete
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void ClearDataGrid()
        {
            dgStatus.ClearRows();
            dgHistory.ClearRows();
        }

        private void TimerSetting()
        {
            if (cboInquiryCycle.GetBindValue() == null)
            {
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer = null;
                }
                return;
            }

            if (refreshTimer == null)
            {
                refreshTimer = new System.Windows.Threading.DispatcherTimer();

                int interval = Convert.ToInt32(cboInquiryCycle.SelectedValue);

                refreshTimer.Interval = TimeSpan.FromSeconds(interval);
                refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            }
            
            refreshTimer.Start();

            Refresh();
        }

        private void Refresh()
        {
            try
            {
                if (!ChkValidation())
                {
                    if (refreshTimer != null) refreshTimer.Stop();
                    cboInquiryCycle.SelectedIndex = 0;
                    return;
                }

                if (tabMain.SelectedIndex == 0)
                {
                    SelectStatusList();
                }
                else
                {
                    SelectHistoryList(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReqEqptCombo()
        {

            try
            {
                cboReqEquipment.ItemsSource = null;

                DataTable dtInData = new DataTable();
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("REQ_DTTM_FROM", typeof(string));
                dtInData.Columns.Add("REQ_DTTM_TO", typeof(string));

                DataRow inData = dtInData.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = Util.NVC(cboArea.SelectedValue);
                inData["REQ_DTTM_FROM"] = dtpStart.IsEnabled ? dtpStart.SelectedDateTime.ToString("yyyyMMdd") : null;
                inData["REQ_DTTM_TO"] = dtpStart.IsEnabled ? dtpStart.SelectedDateTime.ToString("yyyyMMdd") : null;
                dtInData.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_REQ_EQPT_CBO", "INDATA", "OUTDATA", dtInData);

                if (dtResult != null)
                {
                    cboReqEquipment.DisplayMemberPath = "CBO_NAME";
                    cboReqEquipment.SelectedValuePath = "CBO_CODE";
                    
                    cboReqEquipment.ItemsSource = dtResult.Copy().AsDataView();

                    cboReqEquipment.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkValidation()
        {
            if (cboArea.SelectedValue==null || cboArea.SelectedValue.Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                cboArea.Focus();
                return false;
            }

            //if ((dtpEnd.SelectedDateTime - dtpStart.SelectedDateTime).TotalDays > 31)
            //{
            //    // 기간은 {0}일 이내 입니다.
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

            if (!Util.IsNVC(txtCarrierId.Text) && txtCarrierId.Text.Length < 3)
            {
                // [%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");
                txtCarrierId.Focus();
                txtCarrierId.SelectAll();
                return false;
            }
            return true;
        }

        private void SelectStatusList()
        {
            try
            {
                ShowLoadingIndicator();

                dgStatus.ClearRows();

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PRCS_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("REQ_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("REQ_STAT_CODE", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.SelectedValue;
                inData["EQPTID"] = cboReqEquipment.GetBindValue();
                inData["PRCS_TYPE_CODE"] = "EQPT_REQ";
                inData["REQ_TYPE_CODE"] = cboReqReturnType.GetBindValue();
                inData["REQ_STAT_CODE"] = cboState.GetBindValue();
                inData["CSTID"] = txtCarrierId.GetBindValue();
                inData["LOTID"] = txtLotId.GetBindValue();

                INDATA.Rows.Add(inData);

                new ClientProxy().ExecuteService("DA_MHS_SEL_TRF_REQ_STATUS_LIST", "RQSTDT", "RSLTDT", INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    if (!result.Columns.Contains("CHK"))
                    {
                        result.Columns.Add("CHK", typeof(bool));
                        result.Select().ToList().ForEach(r => r["CHK"] = false);
                    }

                    //DataTable dt1 = result.Copy();
                    //dt1.Select().ToList().ForEach(r => r["CST_LOAD_LOCATION_CODE"] = 1);
                    //DataTable dt2 = result.Copy();
                    //dt2.Select().ToList().ForEach(r => r["CST_LOAD_LOCATION_CODE"] = 2);
                    //dt1.Merge(dt2.Copy());

                    //DataView dv = dt1.DefaultView;
                    //dv.Sort = "REQ_SEQNO, CST_LOAD_LOCATION_CODE";
                    //result = dv.ToTable();

                    dgStatus.SetItemsSource(result, FrameOperation, true);

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectHistoryList(bool isDoubleClick)
        {
            try
            {                
                ShowLoadingIndicator();

                dgHistory.ClearRows();

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PRCS_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("REQ_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("REQ_STAT_CODE", typeof(string));
                INDATA.Columns.Add("REQ_DTTM_FROM", typeof(string));
                INDATA.Columns.Add("REQ_DTTM_TO", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("REQ_SEQNO", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.SelectedValue;
                inData["EQPTID"] = cboReqEquipment.GetBindValue();
                inData["PRCS_TYPE_CODE"] = "EQPT_REQ";
                inData["REQ_TYPE_CODE"] = cboReqReturnType.GetBindValue();
                inData["REQ_STAT_CODE"] = cboState.GetBindValue();
                inData["REQ_DTTM_FROM"] = dtpStart.IsEnabled ? dtpStart.SelectedDateTime.ToString("yyyyMMdd") : null;
                inData["REQ_DTTM_TO"] = dtpStart.IsEnabled ? dtpStart.SelectedDateTime.ToString("yyyyMMdd") : null;
                inData["CSTID"] = txtCarrierId.GetBindValue();
                inData["LOTID"] = txtLotId.GetBindValue();
                if (isDoubleClick && dgStatus.CurrentRow != null)
                {
                    inData["REQ_SEQNO"] = dgStatus.GetValue("REQ_SEQNO");            
                }
                INDATA.Rows.Add(inData);

                new ClientProxy().ExecuteService("DA_MHS_SEL_TRF_REQ_HISTORY_LIST", "RQSTDT", "RSLTDT", INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    //DataTable dt1 = result.Copy();
                    //dt1.Select().ToList().ForEach(r => r["CST_LOAD_LOCATION_CODE"] = 1);
                    //DataTable dt2 = result.Copy();
                    //dt2.Select().ToList().ForEach(r => r["CST_LOAD_LOCATION_CODE"] = 2);
                    //dt1.Merge(dt2.Copy());

                    //DataView dv = dt1.DefaultView;
                    //dv.Sort = "REQ_SEQNO, HIST_SEQNO, CST_LOAD_LOCATION_CODE";
                    //result = dv.ToTable();

                    dgHistory.SetItemsSource(result, FrameOperation, true);

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        private void RepLotUseForm()
        {

            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", cboArea.SelectedValue.ToString()))  //NFF 추가
            {
                dgStatus.Columns["CSTID"].Header = "CARRIER_REP_LOTID"; // ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");
                dgHistory.Columns["CSTID"].Header = "CARRIER_REP_LOTID"; //ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");
                txtBlockCarrierId.Text = ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");

            }
            else
            {
                dgStatus.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                dgHistory.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                txtBlockCarrierId.Text = ObjectDic.Instance.GetObjectName("Carrier ID");

            }
        }
        #endregion

    }
}