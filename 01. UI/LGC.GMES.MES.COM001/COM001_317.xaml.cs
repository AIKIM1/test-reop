/*************************************************************************************
 Created Date : 2019.12.25
      Creator : LG CNS 김대근
   Decription : 설비 RFID 스캔 NO READ 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.25  LG CNS 김대근 : 설비 RFID 스캔 NO READ 모니터링
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_317.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_317 : UserControl, IWorkArea
    {
        public COM001_317()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }

        private enum ComboStatus
        {
            ALL,
            SELECT,
            NA,
            NONE
        }

        private string selectedProc = string.Empty;

        private string selectedEqsg = string.Empty;

        private string selectedEqpt = string.Empty;

        private string selectedScanType = string.Empty;

        private void initCombo()
        {
            SetcboProcess();
            SetcboScanType();
            SetCboSummaryProcess();
        }

        private void initText()
        {
            txtArea.Text = LoginInfo.CFG_AREA_ID + " : " + LoginInfo.CFG_AREA_NAME;
            txtArea.Tag = LoginInfo.CFG_AREA_ID;
        }

        private void initDate()
        {
            dtpFrom.SelectedDateTime = DateTime.Today.AddDays(-7);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initText();
            initCombo();
            initDate();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                //공정을 선택해주세요.
                Util.MessageValidation("SFU3207");
                return;
            }
            GetScanHist();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedProc = Util.NVC(cboProcess.SelectedValue);
            SetcboProcessLine();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedEqsg = Util.NVC(cboEquipmentSegment.SelectedValue);
            SetcboEquipment();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedEqpt = Util.NVC(cboEquipment.SelectedValue);
        }

        private void cboScanType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedScanType = Util.NVC(cboScanType.SelectedValue);
        }

        private void GetScanHist()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("SCAN_TYPE", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = Util.NVC(txtArea.Tag);
                row["PROCID"] = string.IsNullOrEmpty(selectedProc) ? null : selectedProc;
                row["EQSGID"] = string.IsNullOrEmpty(selectedEqsg) ? null : selectedEqsg;
                row["EQPTID"] = string.IsNullOrEmpty(selectedEqpt) ? null : selectedEqpt;
                row["SCAN_TYPE"] = string.IsNullOrEmpty(selectedScanType) ? null : selectedScanType;
                row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                row["TO_DTTM"] = dtpTo.SelectedDateTime.Date.AddDays(1); //선택시 시간이 자정임
                inData.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_SCAN_HIST", "INDATE", "OUTDATA", inData);

                Util.GridSetData(dgScanHist, result, this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProcess()
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
                row["PROCTYPE"] = "P";
                row["PCSGID"] = "A,E";

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_ETC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-SELECT-";
                dr["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                cboProcess.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboProcessLine()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["PROCID"] = selectedProc;

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipmentSegment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = selectedEqsg;
                row["PROCID"] = selectedProc;

                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_FOR_RFID", "RQSTDT", "RSLTDT", RQSTDT);

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

        #region [summary data tab]
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
                catch(Exception ex)
                {
                    Util.MessageException(ex);
                    dg.LoadedCellPresenter -= dgEqptReadingRate_LoadedCellPresenter;
                }
            }));
        }

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
                catch(Exception ex)
                {
                    Util.MessageException(ex);
                    dg.LoadedCellPresenter -= dgEqptPstnReadingRate_LoadedCellPresenter;
                }
            }));
        }

        private void ChkSummaryDateDiff(Dictionary<string, object[]> blockMsgId)
        {
            DateTime fromTime = dtpSummaryFrom.SelectedDateTime.Date;
            DateTime toTime = dtpSummaryTo.SelectedDateTime.Date.AddDays(1);
            int diff = (toTime - fromTime).Days;

            if(diff > 30)
            {
                blockMsgId.Add("SFU4466", null);
            }
        }

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
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("FROM_DTTM", typeof(DateTime));
                dtInData.Columns.Add("TO_DTTM", typeof(DateTime));
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("PROCID", typeof(string));

                DataRow drInData = dtInData.NewRow();
                drInData["AREAID"] = LoginInfo.CFG_AREA_ID;
                drInData["FROM_DTTM"] = dtpSummaryFrom.SelectedDateTime.Date;
                drInData["TO_DTTM"] = dtpSummaryTo.SelectedDateTime.Date.AddDays(1);
                drInData["LANGID"] = LoginInfo.LANGID;
                drInData["PROCID"] = string.IsNullOrEmpty(cboSummaryProcess.SelectedValue as string) ? null : cboSummaryProcess.SelectedValue as string;
                dtInData.Rows.Add(drInData);

                dsInData.Tables.Add(dtInData);

                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_EQPT_READING_RATE", "INDATA", "RESULT_BY_EQPT, RESULT_BY_EQPT_PSTN", dsInData);
                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_EQPT_READING_RATE", "INDATA", "RESULT_BY_EQPT, RESULT_BY_EQPT_PSTN", (ds, exception) =>
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
                            new Util().SetDataGridMergeExtensionCol(dgEqptReadingRate, new string[] { "PROCNAME" }, DataGridMergeMode.VERTICAL);
                        }

                        if (dtResultByEqptPstn != null)
                        {
                            Util.GridSetData(dgEqptPstnReadingRate, dtResultByEqptPstn, this.FrameOperation);
                            new Util().SetDataGridMergeExtensionCol(dgEqptPstnReadingRate, new string[] { "PROCNAME" }, DataGridMergeMode.VERTICAL);
                        }
                    }
                    catch(Exception ex)
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
            catch(Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Dispatcher.BeginInvoke(new Action(() => {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }));

            }           
        }

        private void SetCboSummaryProcess()
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
                row["PROCTYPE"] = "P";
                row["PCSGID"] = "A,E";
                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_ETC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //ALL추가
                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(dr, 0);

                //값 세팅
                cboSummaryProcess.DisplayMemberPath = "CBO_NAME";
                cboSummaryProcess.SelectedValuePath = "CBO_CODE";

                //cboSummaryProcess에 할당
                cboSummaryProcess.ItemsSource = dtResult.Copy().AsDataView();
                cboSummaryProcess.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [util]
        private string GetMessageAsOne(Dictionary<string, object[]> msgId)
        {
            string message = string.Empty;
            int cnt = 1;

            foreach(KeyValuePair<string, object[]> pair in msgId)
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
    }
}
