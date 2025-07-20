/*************************************************************************************
 Created Date : 2022.08.29
      Creator : 오화백
   Decription : 물류설비 RFID 스캔 NO READ 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.29  오화백 : Init
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.MCS001
{
 
    /// <summary>
    /// MCS001_084.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_084 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string selectedEqgrID = string.Empty; //설비그룹ID
        private string selectedEqpt = string.Empty; // 설비ID
        private string selectedPort = string.Empty; // PORT
        private string selectedScanType = string.Empty;

        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_084()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        /// <summary>
        /// 화면로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            initText();
            initCombo();
            initDate();
        }
        /// <summary>
        /// 텍스트 박스 초기화
        /// </summary>
        private void initText()
        {
            txtArea.Text = LoginInfo.CFG_AREA_ID + " : " + LoginInfo.CFG_AREA_NAME;
            txtArea.Tag = LoginInfo.CFG_AREA_ID;
        }
        /// <summary>
        /// 콤보박스 초기화
        /// </summary>
        private void initCombo()
        {
            SetcboEqgrname();
            SetcboScanType();
            SetcboSummaryEqgrname();
        }
    
        /// <summary>
        /// 날짜 초기화 : 7일간 표시
        /// </summary>
        private void initDate()
        {
            dtpFrom.SelectedDateTime = DateTime.Today.AddDays(-7);
        }

        #endregion

        #region Event

        #region [ROW DATA 탭]

        #region 설비군 콤보 이벤트 : cboEqgrname_SelectedValueChanged()
        /// <summary>
        /// 설비군 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEqgrname_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedEqgrID = Util.NVC(cboEqgrname.SelectedValue);
            SetcboEquipment();
        }

        #endregion

        #region 설비 콤보 이벤트 : cboEquipment_SelectedValueChanged()
        /// <summary>
        /// 설비 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedEqpt = Util.NVC(cboEquipment.SelectedValue);
            SetcboPort();
        }

        #endregion

        #region Port 콤보 이벤트 : cboPort_SelectedValueChanged()

        /// <summary>
        /// Port 콤보 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedPort = Util.NVC(cboPort.SelectedValue);

        }

        #endregion

        #region 스캔방법 콤보 이벤트 : cboScanType_SelectedValueChanged()

        /// <summary>
        /// 스캔방법 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboScanType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedScanType = Util.NVC(cboScanType.SelectedValue);
        }

        #endregion

        #region 조회버튼 클릭 : btnSearch_Click()
        /// <summary>
        ///  조회 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEqgrname.SelectedValue.ToString().Equals("SELECT"))
            {
                //%1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("설비군"));
                return;
            }
            GetScanHist();
        }

        #endregion


        #endregion

        #region [SUMMARY DATA 탭]

        #region Summary 조회 버튼 클릭 : btnSummarySearch_Click()
        /// <summary>
        /// Summary 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSummarySearch_Click(object sender, RoutedEventArgs e)
        {
            //조회자체가 되지 않는 Validation
            Dictionary<string, object[]> blockMsgId = new Dictionary<string, object[]>();
            ChkSummaryDateDiff(blockMsgId);

            if (!MessageBlock(blockMsgId))
            {
                return;
            }

            //조회는 되지만 경고성 팝업
            Dictionary<string, object[]> warningMsgId = new Dictionary<string, object[]>();

            MessageWarning(warningMsgId, () => {
                GetSummaryData();
            });
        }

        #endregion

        #region 설비별 리스트 RFID 비율에 따른 색깔지정 : dgEqptReadingRate_LoadedCellPresenter()

        /// <summary>
        /// 설비별 조회 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEqptReadingRate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e == null || sender == null)
                    return;

                C1DataGrid dg = sender as C1DataGrid;
                C1.WPF.DataGrid.DataGridCell cell = e.Cell as C1.WPF.DataGrid.DataGridCell;

                if (dg == null || cell == null || cell.Presenter == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(e.Cell.Column.Name)))
                {
                    return;
                }

                if (e.Cell.Row.Index < e.Cell.DataGrid.TopRows.Count)
                {
                    return;
                }

                try
                {
                    SolidColorBrush scbRFID_RATE = new SolidColorBrush();
                    SolidColorBrush scbRFID_UTILIZATION = new SolidColorBrush();

                    if (Util.NVC(e.Cell.Column.Name).Equals("RFID_RATE"))
                    {
                        decimal rfidRate = Util.NVC_Decimal(e.Cell.Value);
                        if (rfidRate < 98 && rfidRate > 95)
                        {
                            scbRFID_RATE = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rfidRate <= 95)
                        {
                            scbRFID_RATE = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            scbRFID_RATE = null;
                        }

                        e.Cell.Presenter.Background = scbRFID_RATE;
                        e.Cell.Presenter.SelectedBackground = scbRFID_RATE;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if (Util.NVC(e.Cell.Column.Name).Equals("RFID_UTILIZATION"))
                    {
                        var rfidUtilization = Util.NVC_Decimal(e.Cell.Value);
                        if (rfidUtilization < 98 && rfidUtilization > 95)
                        {
                            scbRFID_UTILIZATION = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rfidUtilization <= 95)
                        {
                            scbRFID_UTILIZATION = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            scbRFID_UTILIZATION = null;
                        }

                        e.Cell.Presenter.Background = scbRFID_UTILIZATION;
                        e.Cell.Presenter.SelectedBackground = scbRFID_UTILIZATION;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    dg.LoadedCellPresenter -= dgEqptReadingRate_LoadedCellPresenter;
                }
            }));
        }

        #endregion

        #region 설비 포트별 리스트 RFID 비율에 따른 색깔지정 : dgEqptPstnReadingRate_LoadedCellPresenter()
        /// <summary>
        /// 설비포트별 조회 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEqptPstnReadingRate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e == null || sender == null)
                    return;

                C1DataGrid dg = sender as C1DataGrid;
                C1.WPF.DataGrid.DataGridCell cell = e.Cell as C1.WPF.DataGrid.DataGridCell;

                if (dg == null || cell == null || cell.Presenter == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(e.Cell.Column.Name)))
                {
                    return;
                }

                if (e.Cell.Row.Index < e.Cell.DataGrid.TopRows.Count)
                {
                    return;
                }

                try
                {
                    SolidColorBrush scbRFID_RATE = new SolidColorBrush();
                    SolidColorBrush scbRFID_UTILIZATION = new SolidColorBrush();

                    if (Util.NVC(e.Cell.Column.Name).Equals("RFID_RATE"))
                    {
                        decimal rfidRate = Util.NVC_Decimal(e.Cell.Value);
                        if (rfidRate < 98 && rfidRate > 95)
                        {
                            scbRFID_RATE = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rfidRate <= 95)
                        {
                            scbRFID_RATE = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            scbRFID_RATE = null;
                        }

                        e.Cell.Presenter.Background = scbRFID_RATE;
                        e.Cell.Presenter.SelectedBackground = scbRFID_RATE;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if (Util.NVC(e.Cell.Column.Name).Equals("RFID_UTILIZATION"))
                    {
                        var rfidUtilization = Util.NVC_Decimal(e.Cell.Value);
                        if (rfidUtilization < 98 && rfidUtilization > 95)
                        {
                            scbRFID_UTILIZATION = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rfidUtilization <= 95)
                        {
                            scbRFID_UTILIZATION = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            scbRFID_UTILIZATION = null;
                        }

                        e.Cell.Presenter.Background = scbRFID_UTILIZATION;
                        e.Cell.Presenter.SelectedBackground = scbRFID_UTILIZATION;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    dg.LoadedCellPresenter -= dgEqptPstnReadingRate_LoadedCellPresenter;
                }
            }));
        }

        #endregion

        #endregion

        #endregion

        #region Mehod

        #region [ROW DATA 탭]

        #region 설비군 콤보 조회 : SetcboEqgrname()

        /// <summary>
        /// 설비군 콤보
        /// </summary>
        private void SetcboEqgrname()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCTYPE", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));


                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_EQUIPMENTGROUP_EQPTLEVEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEqgrname.DisplayMemberPath = "CBO_NAME";
                cboEqgrname.SelectedValuePath = "CBO_CODE";

                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-SELECT-";
                dr["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboEqgrname.ItemsSource = dtResult.Copy().AsDataView();
                cboEqgrname.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region 설비 콤보 조회 : SetcboEquipment()

        /// <summary>
        /// 설비조회 
        /// </summary>
        private void SetcboEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                row["EQGRID"] = selectedEqgrID;

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_EQUIPMENT_EQPTLEVEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region PORT 콤보 조회 : SetcboPort()
        /// <summary>
        /// Port 정보 조회
        /// </summary>
        private void SetcboPort()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));


                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = selectedEqpt;


                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MCS_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboPort.DisplayMemberPath = "CBO_NAME";
                cboPort.SelectedValuePath = "CBO_CODE";
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                cboPort.ItemsSource = dtResult.Copy().AsDataView();
                cboPort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 스캔방법 콤보 조회 : SetcboScanType()
        /// <summary>
        /// 스캔방법 조회
        /// </summary>
        private void SetcboScanType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["CMCDTYPE"] = "EQPT_SCAN_TYPE";

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboScanType.DisplayMemberPath = "CBO_NAME";
                cboScanType.SelectedValuePath = "CBO_CODE";
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                cboScanType.ItemsSource = dtResult.Copy().AsDataView();
                cboScanType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region 스캔 방법 이력 조회 : GetScanHist()

        /// <summary>
        /// 스캔 방법 조회 
        /// </summary>
        private void GetScanHist()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("PORT_ID", typeof(string));
                inData.Columns.Add("EQGRID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("SCAN_TYPE", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));
                inData.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = Util.NVC(txtArea.Tag);
                row["PORT_ID"] = string.IsNullOrEmpty(selectedPort) ? null : selectedPort;
                row["EQGRID"] = string.IsNullOrEmpty(selectedEqgrID) ? null : selectedEqgrID;
                row["EQPTID"] = string.IsNullOrEmpty(selectedEqpt) ? null : selectedEqpt;
                row["SCAN_TYPE"] = string.IsNullOrEmpty(selectedScanType) ? null : selectedScanType;
                row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                row["TO_DTTM"] = dtpTo.SelectedDateTime.Date.AddDays(1); //선택시 시간이 자정임
                row["SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                inData.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_EQPT_SCAN_HIST", "INDATE", "OUTDATA", inData);

                Util.GridSetData(dgScanHist, result, this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        #region [summary data tab]

        #region 날짜 조회 30일 경과 체크  : ChkSummaryDateDiff()

        /// <summary>
        /// 날짜 조회시 30일 초과 체크
        /// </summary>
        /// <param name="blockMsgId"></param>
        private void ChkSummaryDateDiff(Dictionary<string, object[]> blockMsgId)
        {
            DateTime fromTime = dtpSummaryFrom.SelectedDateTime.Date;
            DateTime toTime = dtpSummaryTo.SelectedDateTime.Date.AddDays(1);
            int diff = (toTime - fromTime).Days;

            if (diff > 30)
            {
                blockMsgId.Add("SFU4466", null);
            }
        }

        #endregion

        #region SUMMARY 설비 그룹 콤보 조회 : SetcboSummaryEqgrname()

        /// <summary>
        /// 서머리 설비 그룹 콤보조회
        /// </summary>
        private void SetcboSummaryEqgrname()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCTYPE", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));


                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_EQUIPMENTGROUP_EQPTLEVEL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //ALL추가
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                //값 세팅
                cboSummaryEqgrname.DisplayMemberPath = "CBO_NAME";
                cboSummaryEqgrname.SelectedValuePath = "CBO_CODE";

                //cboSummaryProcess에 할당
                cboSummaryEqgrname.ItemsSource = dtResult.Copy().AsDataView();
                cboSummaryEqgrname.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 설비별, 설비 Port 별  Summary Data 조회 : GetSummaryData()
        /// <summary>
        /// 스캔 Summary Data 조회 
        /// </summary>
        private void GetSummaryData()
        {
            try
            {
                loadingIndicator.Dispatcher.BeginInvoke(new Action(() => {
                    loadingIndicator.Visibility = Visibility.Visible;
                }));

                DataSet dsInData = new DataSet();

                DataTable dtInData = new DataTable();
                dtInData.TableName = "INDATA";
                dtInData.Columns.Add("FROM_DTTM", typeof(DateTime));
                dtInData.Columns.Add("TO_DTTM", typeof(DateTime));
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("EQGRID", typeof(string));
                dtInData.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));

                DataRow drInData = dtInData.NewRow();
                drInData["FROM_DTTM"] = dtpSummaryFrom.SelectedDateTime.Date;
                drInData["TO_DTTM"] = dtpSummaryTo.SelectedDateTime.Date.AddDays(1);
                drInData["LANGID"] = LoginInfo.LANGID;
                drInData["EQGRID"] = string.IsNullOrEmpty(cboSummaryEqgrname.SelectedValue as string) ? null : cboSummaryEqgrname.SelectedValue as string;
                drInData["SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                dtInData.Rows.Add(drInData);

                dsInData.Tables.Add(dtInData);

                new ClientProxy().ExecuteService_Multi("BR_MHS_SEL_EQPT_READING_RATE", "INDATA", "RESULT_BY_EQPT, RESULT_BY_EQPT_PSTN", (ds, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            throw exception;
                        }
                        DataTable dtResultByEqpt = null;
                        DataTable dtResultByEqptPstn = null;

                        if (ds != null)
                        {
                            for (int i = 0; i < ds.Tables.Count; i++)
                            {
                                if (ds.Tables[i].TableName.Trim().Equals("RESULT_BY_EQPT"))
                                {
                                    dtResultByEqpt = ds.Tables[i];
                                }
                                else if (ds.Tables[i].TableName.Trim().Equals("RESULT_BY_EQPT_PSTN"))
                                {
                                    dtResultByEqptPstn = ds.Tables[i];
                                }
                            }
                        }

                        if (dtResultByEqpt != null)
                        {
                            Util.GridSetData(dgEqptReadingRate, dtResultByEqpt, this.FrameOperation);
                            new Util().SetDataGridMergeExtensionCol(dgEqptReadingRate, new string[] { "EQGRNAME" }, DataGridMergeMode.VERTICAL);
                        }

                        if (dtResultByEqptPstn != null)
                        {
                            Util.GridSetData(dgEqptPstnReadingRate, dtResultByEqptPstn, this.FrameOperation);
                            new Util().SetDataGridMergeExtensionCol(dgEqptPstnReadingRate, new string[] { "EQGRNAME" }, DataGridMergeMode.VERTICAL);
                            new Util().SetDataGridMergeExtensionCol(dgEqptPstnReadingRate, new string[] { "EQPTID" }, DataGridMergeMode.VERTICAL);
                            new Util().SetDataGridMergeExtensionCol(dgEqptPstnReadingRate, new string[] { "EQPTNAME" }, DataGridMergeMode.VERTICAL);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Dispatcher.BeginInvoke(new Action(() => {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }));
                    }
                }, dsInData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Dispatcher.BeginInvoke(new Action(() => {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }));

            }
        }


        #endregion


        #endregion

        #region 메세지 관련 Utill
        private string GetMessageAsOne(Dictionary<string, object[]> msgId)
        {
            string message = string.Empty;
            int cnt = 1;

            foreach (KeyValuePair<string, object[]> pair in msgId)
            {
                string tmpMsg = MessageDic.Instance.GetMessage(pair.Key, pair.Value);
                tmpMsg = tmpMsg.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                tmpMsg = cnt.ToString() + ". " + tmpMsg;
                if (msgId.Count > cnt)
                {
                    tmpMsg += Environment.NewLine;
                }

                message += tmpMsg;
                cnt++;
            }
            return string.IsNullOrEmpty(message) ? message : message.Substring(0, message.Length - 1);
        }

        private bool MessageBlock(Dictionary<string, object[]> blockMsgId)
        {
            bool result = true;
            string message = GetMessageAsOne(blockMsgId);

            if (!string.IsNullOrEmpty(message))
            {
                ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                result = false;
            }

            return result;
        }

        private void MessageWarning(Dictionary<string, object[]> warningMsgId, Action callback)
        {
            string message = GetMessageAsOne(warningMsgId);

            if (!string.IsNullOrEmpty(message))
            {
                ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, (result) => { callback(); });
            }
            else
            {
                callback();
            }
        }
        #endregion

        #endregion

    }
}
