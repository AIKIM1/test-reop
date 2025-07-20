/*************************************************************************************
 Created Date : 2019.07.11
      Creator : 정문교
   Decription : 원자재관리 - 원자재 반송 적재 보고 Scan
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08  정문교 : Biz 오류 메시지 다국어 처리 안됨
                       Biz 오류 메시지 Util.MessageValidation -> MessageException Call Back으로 변경

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Windows.Threading;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_003 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        DataTable _dtLoading;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_003()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            if (((System.Windows.FrameworkElement)tabLoading.SelectedItem).Name.Equals("ctbLoading"))
            {
                cboEquipmentSegment.SelectedIndex = 0;

                txtPalletID.Text = string.Empty;
                txtMLotID.Text = string.Empty;
            }
            else
            {
                txtCancelMLotID.Text = string.Empty;
            }
        }

        private void InitializeGrid()
        {
            if (((System.Windows.FrameworkElement)tabLoading.SelectedItem).Name.Equals("ctbLoading"))
            {
                Util.gridClear(dgRequest);
                Util.gridClear(dgRequestMtrl);
            }
            else
            {
                Util.gridClear(dgCancel);
            }
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "cboAreaAll");

            //라인
            EquipmentSegmentCombo(cboEquipmentSegment);
            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void SetControl(bool isVisibility = false)
        {
            _dtLoading = new DataTable();
            _dtLoading.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
            _dtLoading.Columns.Add("PLLT_ID", typeof(string));
            _dtLoading.Columns.Add("MTRLID", typeof(string));
            _dtLoading.Columns.Add("MLOTID", typeof(string));
            _dtLoading.Columns.Add("MTRL_QTY", typeof(decimal));
            _dtLoading.Columns.Add("DEL_FLAG", typeof(string));
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            // 조회
            SearchProcess();

            this.Loaded -= UserControl_Loaded;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null) return;

            //라인
            EquipmentSegmentCombo(cboEquipmentSegment);
            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);

            // 조회
            SearchProcess();
        }
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedValue == null) return;

            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);
        }
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null) return;

            //설비
            EquipmentCombo(cboEquipment);
        }

        #region 적재
        private void dgRequestChoice_Checked(object sender, RoutedEventArgs e)
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

                        // row 색 바꾸기
                        dgRequest.SelectedIndex = idx;

                        // 요청 자재 조회
                        SearchMTRL();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        private void dgRequestMtrl_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_SPLY_REQ_QTY") || e.Cell.Column.Name.Equals("LOAD_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        if (e.Cell.Column.Name.Equals("LOAD_QTY"))
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        else
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        /// <summary>
        /// Pallet Scan
        /// /// </summary>
        private void txtPalletID_GotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
                DispatcherPriority.ContextIdle,
                new Action
                (
                    delegate {(sender as TextBox).SelectAll();}
                )
            );
        }
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationScan(txtPalletID)) return;

                CheckScanValuePLT();
            }
        }

        /// <summary>
        /// 자재LOT Scan
        /// /// </summary>
        private void txtMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationScan(txtMLotID)) return;

                CheckScanValueMLOT();
            }
        }

        /// <summary>
        /// Scan 자료 삭제
        /// /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;

                    // 적재된 자재 LOT 적재 취소
                    DataRow[] drCancel = _dtLoading.Select("MTRLID = '" + dtRow["MTRLID"] + "' And DEL_FLAG = 'N'");
                    if (drCancel.Length > 0)
                    {
                        CancelProcess(drCancel);
                    }

                    _dtLoading.Select("MTRLID = '" + dtRow["MTRLID"] + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    _dtLoading.AcceptChanges();

                    CountLoadMtrl(dtRow["MTRLID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _dtLoading.Clear();

            DataTable dt = DataTableConverter.Convert(dgRequestMtrl.ItemsSource);
            dt.Select("").ToList<DataRow>().ForEach(r => r["LOAD_QTY"] = 0);
            dt.AcceptChanges();
            Util.GridSetData(dgRequestMtrl, dt, null);
        }

        /// <summary>
        /// 적재
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            // %1 (을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            }, ObjectDic.Instance.GetObjectName("적재"));
        }
        #endregion

        #region 적재취소
        private void txtCancelMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtCancelMLotID.Text)) return;

                CheckScanValueCancelMLOT();
            }
        }

        /// <summary>
        /// Scan 자료 삭제
        /// /// </summary>
        private void btnCancelDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;
                    DataTable dt = DataTableConverter.Convert(dgCancel.ItemsSource);

                    dt.Select("MLOTID = '" + dtRow["MLOTID"] + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();
                    Util.GridSetData(dgCancel, dt, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            // %1 (을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcess();
                }
            }, ObjectDic.Instance.GetObjectName("적재취소"));
        }


        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 라인
        /// </summary>
        private void EquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID"};
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void ProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 설비
        /// </summary>
        private void EquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(),
                                                        cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString(), null };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 요청 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                // Clear
                _dtLoading.Clear();
                InitializeControls();
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Request;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgRequest, bizResult, FrameOperation, true);

                        // 첫번쨰 선택및 요청 자재 조회
                        if (bizResult.Rows.Count > 0)
                        {
                            DataTableConverter.SetValue(dgRequest.Rows[0].DataItem, "CHK", true);
                            // row 색 바꾸기
                            dgRequest.SelectedIndex = 0;
                            // 요청 자재 조회
                            SearchMTRL();
                        }
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 요청 지재 조회
        /// </summary>
        private void SearchMTRL()
        {
            try
            {
                _dtLoading.Clear();

                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");
                string ReqID = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = ReqID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST_MTRL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgRequestMtrl, bizResult, null);

                        // 적재취소후 적재된 자재lLOT 조회
                        SearchMLOT(ReqID);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// MLOT 조회
        /// </summary>
        private void SearchMLOT(string ReqID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = ReqID;
                newRow["DEL_FLAG"] = "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_LOADING_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        foreach (DataRow row in bizResult.Rows)
                        {
                            DataRow addRow = _dtLoading.NewRow();
                            addRow["MTRL_SPLY_REQ_ID"] = ReqID;
                            addRow["PLLT_ID"] = row["PLLT_ID"];
                            addRow["MTRLID"] = row["MTRLID"];
                            addRow["MLOTID"] = row["MLOTID"];
                            addRow["MTRL_QTY"] = row["MTRL_QTY"];
                            addRow["DEL_FLAG"] = row["DEL_FLAG"];
                            _dtLoading.Rows.Add(addRow);
                        }
                        _dtLoading.AcceptChanges();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Scan Value 체크
        /// </summary>
        private DataTable SearchScanValue(bool isPallet)
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
            inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
            inTable.Columns.Add("PLLT_ID", typeof(string));
            inTable.Columns.Add("S_BOX_ID", typeof(string));
            inTable.Columns.Add("ISPLAAET", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "AREAID").ToString());
            newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
            newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Loaded;
            newRow["PLLT_ID"] = txtPalletID.Text;
            newRow["S_BOX_ID"] = txtMLotID.Text;
            newRow["ISPLAAET"] = isPallet == true ? "Y" : "N";
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("BR_MTR_CHK_PALLET_MLOT", "INDATA", "OUTDATA", inTable);
        }

        /// <summary>
        /// 적재
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AGV_ID", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("CNFM_QTY", typeof(string));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("MTRLID", typeof(string));
                inMLot.Columns.Add("MTRL_QTY", typeof(decimal));
                inMLot.Columns.Add("PLLT_ID", typeof(string));

                /////////////////////////////////////////////////////////////////
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Loaded;
                newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "EQPTID").ToString());
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                foreach (DataRow row in _dtLoading.Rows)
                {
                    newRow = inMLot.NewRow();
                    newRow["MLOTID"] = Util.NVC(row["MLOTID"]);
                    newRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                    newRow["MTRL_QTY"] = row["MTRL_QTY"];
                    newRow["PLLT_ID"] = Util.NVC(row["PLLT_ID"]);
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REQUEST_LOADING", "INDATA,INMTRL,INMLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 적재 되었습니다.
                        Util.MessageInfo("SFU8002");

                        // 재조회
                        SearchProcess();
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

        /// <summary>
        /// 적재취소 Scan Value 체크
        /// </summary>
        private DataTable SearchCancelScanValue()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
            inTable.Columns.Add("S_BOX_ID", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Cancel;
            newRow["S_BOX_ID"] = txtCancelMLotID.Text;
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("BR_MTR_CHK_PALLET_MLOT", "INDATA", "OUTDATA", inTable);
        }

        /// <summary>
        /// MLOT 조회
        /// </summary>
        private void SearchCancelMLOT(string ReqID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MLOTID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = ReqID;
                newRow["MLOTID"] = txtCancelMLotID.Text;
                newRow["DEL_FLAG"] = "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_LOADING_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgCancel.ItemsSource);
                        dt.Merge(bizResult);

                        Util.GridSetData(dgCancel, dt, null);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 적재취소
        /// </summary>
        private void CancelProcess(DataRow[] drCancel = null)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inReq = inDataSet.Tables.Add("INREQ");
                inReq.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inMLot.Columns.Add("MLOTID", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr;
                if (drCancel == null)
                    dr = DataTableConverter.Convert(dgCancel.ItemsSource).Select();
                else
                    dr = drCancel;

                // 요청 번호별 Grouping
                var summarydata = from row in dr
                                  group row by new
                                  {
                                      REQ_ID = row.Field<string>("MTRL_SPLY_REQ_ID"),
                                  } into grp
                                  select new
                                  {
                                      ReqID = grp.Key.REQ_ID,
                                      ReqCount = grp.Count()
                                  };

                foreach (var data in summarydata)
                {
                    newRow = inReq.NewRow();
                    newRow["MTRL_SPLY_REQ_ID"] = data.ReqID;
                    inReq.Rows.Add(newRow);
                }

                // 자재 LOT별
                foreach (DataRow row in dr)
                {
                    newRow = inMLot.NewRow();
                    newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(row["MTRL_SPLY_REQ_ID"]);
                    newRow["MLOTID"] = Util.NVC(row["MLOTID"]);
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REQUEST_LOADING_CANCEL", "INDATA,INREQ,INMLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        if (drCancel == null)
                        {
                            //  취소되었습니다.
                            Util.MessageInfo("SFU5032");

                            // clear
                            InitializeGrid();

                            // 적재 탭 재조회
                            SearchProcess();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            //if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    // 라인을 선택해주세요
            //    Util.MessageValidation("SFU4050");
            //    return false;
            //}

            //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    // 설비를 선택하세요
            //    Util.MessageValidation("SFU1153");
            //    return false;
            //}

            return true;
        }

        private bool ValidationScan(TextBox tb)
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            if (rowIndex < 0 || dgRequestMtrl.Rows.Count == 0)
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                // 스캔된 항목이 없습니다.
                Util.MessageValidation("SFU2916");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            if (rowIndex < 0 || dgRequestMtrl.Rows.Count == 0)
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return false;
            }

            //DataTable dt = DataTableConverter.Convert(dgRequestMtrl.ItemsSource);
            //DataRow[] dr = dt.Select("MTRL_SPLY_REQ_QTY <> LOAD_QTY");

            //if (dr.Length > 0)
            //{
            //    // 요청수량과 적재수량이 틀립니다.
            //    Util.MessageValidation("SFU8001");
            //    return false;
            //}

            if (_dtLoading == null || _dtLoading.Rows.Count == 0)
            {
                // %1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("적재LOT"));
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            if (dgCancel.Rows.Count == 0)
            {
                // 선택된 LOT이 없습니다.
                Util.MessageValidation("SFU1261");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
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

        /// <summary>
        /// Pallet Scan 체크
        /// </summary>
        private void CheckScanValuePLT()
        {
            try
            {
                DataTable dt = SearchScanValue(true);
                txtMLotID.Focus();
            }
            catch (Exception ex)
            {
                // 스캔한 데이터가 없습니다.
                Util.MessageException(ex, result =>
                {
                    txtPalletID.Text = string.Empty;
                    txtPalletID.Focus();
                });

                return;
            }
        }

        /// <summary>
        /// 자재LOT Scan 체크
        /// </summary>
        private void CheckScanValueMLOT()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    // 선택된 Pallet가 없습니다.
                    Util.MessageValidation("SFU3425", result =>
                    {
                        txtPalletID.Text = string.Empty;
                        txtPalletID.Focus();
                    });
                    return;
                }

                DataRow[] dr = _dtLoading.Select("MLOTID = '" + txtMLotID.Text + "'");

                if (dr.Length > 0)
                {
                    // 이미 바코드 스캔 완료하였습니다.
                    Util.MessageValidation("SFU5114", result =>
                    {
                        txtMLotID.Text = string.Empty;
                        txtMLotID.Focus();
                    });
                    return;
                }

                DataTable dt = SearchScanValue(false);
                /////////////////////////////////////////////////////////////////////////////////////////////////

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow[] drMtrl = DataTableConverter.Convert(dgRequestMtrl.ItemsSource).Select("MTRLID = '" + dt.Rows[0]["MTRLID"].ToString() + "'");

                    if (int.Parse(Util.NVC(drMtrl[0]["MTRL_SPLY_REQ_QTY"])) == int.Parse(Util.NVC(drMtrl[0]["LOAD_QTY"])))
                    {
                        // 요청수량 보다 적재수량이 큽니다.
                        Util.MessageValidation("SFU8004", result =>
                        {
                            txtMLotID.Text = string.Empty;
                            txtMLotID.Focus();
                        });
                        return;
                    }

                    if (Util.NVC(dt.Rows[0]["PLLT_DISAGREEMENT"]) == "Y")
                    {
                        // %1에 적재된 소포장 LOT이 아닙니다.  그래도 적재 하시겠습니까?
                        Util.MessageConfirm("SFU8123", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetScanValue(dt);
                            }
                            else
                            {
                                txtMLotID.Text = string.Empty;
                                txtMLotID.Focus();
                            }
                        }, txtPalletID.Text);
                    }
                    else
                    {
                        SetScanValue(dt);
                    }

                    //int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

                    //DataRow newrow = _dtLoading.NewRow();
                    //newrow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                    //newrow["PLLT_ID"] = txtPalletID.Text;
                    //newrow["MTRLID"] = dt.Rows[0]["MTRLID"];
                    //newrow["MLOTID"] = txtMLotID.Text;
                    //newrow["MTRL_QTY"] = dt.Rows[0]["MTRL_QTY"];
                    //_dtLoading.Rows.Add(newrow);
                    //_dtLoading.AcceptChanges();

                    //// 자재별 합산 조회
                    //CountLoadMtrl(dt.Rows[0]["MTRLID"].ToString());
                }
                else
                {
                    txtMLotID.Text = string.Empty;
                    txtMLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, result =>
                {
                    txtMLotID.Text = string.Empty;
                    txtMLotID.Focus();
                });

                return;
            }
        }

        /// <summary>
        /// Scan 값 Setting
        /// </summary>
        private void SetScanValue(DataTable dtScan)
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            DataRow newrow = _dtLoading.NewRow();
            newrow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
            newrow["PLLT_ID"] = txtPalletID.Text;
            newrow["MTRLID"] = dtScan.Rows[0]["MTRLID"];
            newrow["MLOTID"] = txtMLotID.Text;
            newrow["MTRL_QTY"] = dtScan.Rows[0]["MTRL_QTY"];
            _dtLoading.Rows.Add(newrow);
            _dtLoading.AcceptChanges();

            // 자재별 합산 조회
            CountLoadMtrl(dtScan.Rows[0]["MTRLID"].ToString());

            txtMLotID.Text = string.Empty;
            txtPalletID.Text = string.Empty;
            txtPalletID.Focus();
        }

        /// <summary>
        /// 적재 취소 자재LOT Scan 체크
        /// </summary>
        private void CheckScanValueCancelMLOT()
        {
            try
            {
                if (dgCancel.Rows.Count > 0)
                {
                    DataRow[] dr = DataTableConverter.Convert(dgCancel.ItemsSource).Select("MLOTID = '" + txtCancelMLotID.Text + "'");
                    if (dr.Length > 0)
                    {
                        // 이미 바코드 스캔 완료하였습니다.
                        Util.MessageValidation("SFU5114", result =>
                        {
                            txtCancelMLotID.Text = string.Empty;
                            txtCancelMLotID.Focus();
                        });
                        return;
                    }
                }

                DataTable dt = SearchCancelScanValue();
                /////////////////////////////////////////////////////////////////////////////////////////////////

                if (dt != null && dt.Rows.Count > 0)
                {
                    SearchCancelMLOT(dt.Rows[0]["MTRL_SPLY_REQ_ID"].ToString());
                }

                txtCancelMLotID.Text = string.Empty;
                txtCancelMLotID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, result =>
                {
                    txtCancelMLotID.Text = string.Empty;
                    txtCancelMLotID.Focus();
                });

                return;
            }
        }

        private void CountLoadMtrl(string MtrlID)
        {
            DataTable dtMtrl = DataTableConverter.Convert(dgRequestMtrl.ItemsSource);

            // 자재별 Count
            int ScanCount = int.Parse(_dtLoading.Compute("COUNT(MLOTID)", "MTRLID = '" + MtrlID + "'").ToString());

            // 적재수량에 Updte
            dtMtrl.Select("MTRLID = '" + MtrlID + "'").ToList<DataRow>().ForEach(r => r["LOAD_QTY"] = ScanCount);
            dtMtrl.AcceptChanges();
            Util.GridSetData(dgRequestMtrl, dtMtrl, null);
        }

        #endregion

        #endregion

        #endregion

    }
}
