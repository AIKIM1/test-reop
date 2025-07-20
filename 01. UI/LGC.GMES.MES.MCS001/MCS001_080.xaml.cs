/*************************************************************************************
 Created Date : 2022.06.21
      Creator : 오화백
   Decription : 믹서 원자재 출고
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.21  오화백 :      Initial Created.    
 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_080.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_080 : UserControl, IWorkArea
    {

        #region ************************* Declaration & Constructor ************************* 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        private bool _isAdminAuthority;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private string _dst_eqptID;

        private string _mtrlid = string.Empty;

        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_080()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 권한 설정
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);

        }
      
        /// <summary>
        /// 화면 로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            dtpDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            InitializeCombo();

            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }


        #endregion

        #region    ************************* Event *************************

        #region 조회버튼 클릭  : btnSearch_Click()
        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            if (cboEquipment.Items.Count == 0)
            {
                Util.MessageValidation("SFU2016");   //해당 라인에 설비가 존재하지 않습니다.
                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                return;
            }
            SelectRequestList();
        }
        #endregion

        #region 수동출고예약 버튼 : btnManualIssue_Click()
        /// <summary>
        /// 수동출고 예약
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;
            SaveManualIssueByEsnb();

        }
        #endregion

        #region 수동출고 예약 취소  버튼  : btnTransferCancel_Click()
        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;
            DataTable inTable = new DataTable();
            inTable.Columns.Add("RequestTransferId", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));

            foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["RequestTransferId"] = DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString();
                    newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                    inTable.Rows.Add(newRow);
                }
            }

            CMM_MHS_TRANSFER_CANCEL popupTransferCancel = new CMM_MHS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = inTable;
            parameters[1] = string.Empty;
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }
        #endregion

        #region 수동출고 예약 취소 팝업닫기 : popupTransferCancel_Closed()

        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MHS_TRANSFER_CANCEL popup = sender as CMM_MHS_TRANSFER_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                //사용자재 리스트 조회
                SelectManualOutInventoryList();

            }
        }
        #endregion

        #region 타이머 콤보박스 이벤트 : cboTimer_SelectedValueChanged(), _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 콤보박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 타이머 관련
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(null, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }
        #endregion

        #region 투입요청상세 리스트 조회 이벤트 : dgRequestDetailList_LoadedCellPresenter(), dgRequestDetailList_UnloadedCellPresenter(), dgRequestDetailList_MouseLeftButtonUp()
        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRequestDetailList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "MTRLID")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                }
            }));
        }
        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRequestDetailList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

       

        // <summary>
        /// 투입요청서 상세 조회 LIST 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgReqListDetailChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgRequestDetailList.SelectedIndex = idx;

                _mtrlid = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[idx].DataItem, "MTRLID"));

                //사용가능 자재 리스트 조회
                SelectManualOutInventoryList();
            }
        }


        #endregion

        #region 투입 요청서 리스트 선택 : dgReqListChoice_Checked()
        /// <summary>
        /// 투입요청서 조회 LIST 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgReqListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgRequestList.SelectedIndex = idx;
                GetRequestDetail(Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_ID")));

            }
        }

        #endregion

        #region 출고포트 이벤트 : cboIssuePort_SelectedIndexChanged()
        /// <summary>
        /// 출고 PORT 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboIssuePort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;

            try
            {
                if (cboIssuePort != null && cboIssuePort.SelectedItem != null)
                {
                    int previousRowIndex = e.OldValue;
                    int currentRowIndex = e.NewValue;

                    string transferStateCode = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Name.GetString();
                    _dst_eqptID = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).DataContext.GetString();

                    if (transferStateCode == "OUT_OF_SERVICE")
                    {
                        Util.MessageInfo("SFU8137");
                        cboIssuePort.SelectedIndex = previousRowIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region  출고 리스트 이벤트  : dgIssueTargetInfo_LoadedCellPresenter(), dgIssueTargetInfo_UnloadedCellPresenter(),dgIssueTargetInfo_BeginningEdit(),  dgIssueTargetInfo_MouseDoubleClick()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                    if (convertFromString != null)
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                }

            }));
        }

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        /// <summary>
        /// 리스트 내 체크박스 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfo;


                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgIssueTargetInfo_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgIssueTargetInfo.ItemsSource == null) return;

                SetIssuePort(cboIssuePort, string.Empty);
                DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region PORT 리스트 및 목적지 콤보박스 조회  : SelectWareHousePortInfo()
        /// <summary>
        /// PORT 리스트 및 목적지 콤보박스 조회
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="e"></param>
        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPTNAME" : "EQPTNAME").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrier = DataTableConverter.GetValue(e.Row.DataItem, "CARRIERID").GetString();
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPTID").GetString();
                    if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EQPTID")).Equals(selectedequipmentCode))
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                var query = (from t in ((DataView)dg.ItemsSource).Table.AsEnumerable()
                             where t.Field<int>("CHK") == 1
                             select t).ToList();

                if (checkValue == "0")
                {
                    if (query.Count() < 1)
                    {

                        SetIssuePort(cboIssuePort, currentequipmentCode);



                    }
                }
                else
                {
                    if (query.Count() <= 1)
                    {
                        SetIssuePort(cboIssuePort, string.Empty);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region  ************************* Method *************************

        #region 투입요청서 조회 : SelectRequestList()
        /// <summary>
        /// 투입요청서 조회
        /// </summary>
        private void SelectRequestList()
        {

            try
            {
                if (cboEquipment.Items.Count < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요
                    return;
                }

                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                Util.gridClear(dgIssueTargetInfo);
                

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("REQ_DATE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["REQ_DATE"] = Convert.ToDateTime(dtpDate.SelectedDateTime).ToString("yyyyMMdd");     //dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue.ToString()) == "" ? null : Util.NVC(cboEquipment.SelectedValue.ToString());
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_LIST_CWA", "INDATA", "RSLTDT", IndataTable);
                if (dtMain == null)
                {
                    return;
                }
                Util.GridSetData(dgRequestList, dtMain, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #region 투입요청서 상세 조회  : GetRequestDetail()

        /// <summary>
        /// 투입요청서 상세 조회
        /// </summary>
        /// <param name="sReqID"></param>
        private void GetRequestDetail(string sReqID)
        {
            try
            {
                Util.gridClear(dgRequestDetailList);
                Util.gridClear(dgIssueTargetInfo);
                

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                //IndataTable.Columns.Add("REQ_DATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["REQ_ID"] = sReqID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                //Indata["REQ_DATE"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_DETAIL_CWA", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgRequestDetailList, dtMain, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #region 사용가능 자재리스트 조회  : SelectManualOutInventoryList()
        /// <summary>
        /// 사용가능 자재리스트 조회
        /// </summary>
        /// <param name="isRefresh"></param>
        private void SelectManualOutInventoryList(bool isRefresh = false)
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
            

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "MWW";
                dr["MTRLID"] = _mtrlid;
              
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
                    if (!isRefresh)
                    {
                        SetIssuePort(cboIssuePort, string.Empty);
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region  수동출고 예약  : SaveManualIssueByEsnb()
        /// <summary>
        /// 수동출고 예약 함수
        /// </summary>
        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI_2";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("SRC_PORTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                C1DataGrid dg = dgIssueTargetInfo;
              
                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_EQPTID").GetString();
                        newRow["SRC_PORTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_PORTID").GetString();

                        newRow["DST_EQPTID"] = _dst_eqptID;
                        newRow["DST_PORTID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();

                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                         SelectManualOutInventoryList(true);
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

        #endregion

        #region 수동출고 예약 Validation  : ValidationManualIssue()
        /// <summary>
        /// 수동출고예약 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationManualIssue()
        {

            if (!CommonVerify.HasDataGridRow(dgIssueTargetInfo))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboIssuePort.SelectedItem == null || string.IsNullOrEmpty((cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            if ((cboIssuePort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            {
                Util.MessageInfo("SFU8137");
                return false;
            }

            return true;
        }

        #endregion

        #region 수동출고 예약 취소 Validation : ValidationTransferCancel(), ValidationTransferCancelByEsnb()

        /// <summary>
        /// 수동출고 예약 취소 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationTransferCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgIssueTargetInfo))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (!ValidationTransferCancelByEsnb(dgIssueTargetInfo))
            {
                return false;
            }



            return true;

        }

        /// <summary>
        /// 수동출고 예약 취소 Validation2
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_CHK_SEL_TRF_CMD_CANCEL_BY_UI", "IN_DATA", "OUT_DATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["RETVAL"].GetString() != "0")
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region 초기화 : ClearControl()
        /// <summary>
        /// 초기화
        /// </summary>
        private void ClearControl()
        {
            _mtrlid = string.Empty;
            Util.gridClear(dgRequestList);
            Util.gridClear(dgRequestDetailList);
            Util.gridClear(dgIssueTargetInfo);

            if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
            {
                SetIssuePort(cboIssuePort, string.Empty);
            }
        }

        #endregion

        #region 권한 셋팅  : IsAdminAuthorityByUserId()
        /// <summary>
        /// 권한
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private bool IsAdminAuthorityByUserId(string userId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["USERID"] = userId;
                dr["AUTHID"] = "MESADMIN,MESDEV,LOGIS_MANA";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region 조회 콤보 셋팅 : InitializeCombo()
        /// <summary>
        /// 콤보셋팅
        /// </summary>
        private void InitializeCombo()
        {

            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };

            //ESNB에서도 본 메뉴를 사용 중인데 절연액 공정도 나올 수 있도록 해달라는 요청사항이 발생
            //ProcessCWA는 TOP4만 조회하여 절연액이 조회되지 않고 있음
            if (LoginInfo.CFG_AREA_ID.Equals("EC"))
            {
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "MixingProcess");
            }
            else
            {
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "ProcessCWA");
            }

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);
        }

        #endregion

        #region 출고 Port 셋팅  : SetIssuePort
        /// <summary>
        /// 출고PORT 셋팅
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="equipmentCode"></param>
        private void SetIssuePort(C1ComboBox cbo, string equipmentCode)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;

                if (cbo.Items.Count > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);
                //cbo.ItemsSource = dtResult.AsEnumerable();
                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["PORT_ID"].GetString();
                    comboBoxItem.Name = row["TRF_STAT_CODE"].GetString();

                    comboBoxItem.DataContext = row["DST_EQPTID"].GetString();



                    if (row["TRF_STAT_CODE"].GetString() == "OUT_OF_SERVICE")
                    {
                        comboBoxItem.Foreground = new SolidColorBrush(Colors.Red);
                        comboBoxItem.FontWeight = FontWeights.Bold;
                    }
                    cbo.Items.Add(comboBoxItem);
                }

                cboIssuePort.SelectedIndexChanged += cboIssuePort_SelectedIndexChanged;

                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 타이머 셋팅  : TimerSetting()
        /// <summary>
        /// 타이머 셋팅
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }


        #endregion


        #region 기타 : ShowLoadingIndicator(), HiddenLoadingIndicator()

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        /// <summary>
        /// 프로그래스바 설정 - 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 프로그래스바 설정 - 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        #endregion

        #endregion

    }
}
