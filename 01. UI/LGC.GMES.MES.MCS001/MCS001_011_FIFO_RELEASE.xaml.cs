/*************************************************************************************
 Created Date : 2019.04.24
      Creator : 신광희 차장
   Decription : FIFO 수동출고 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2019.04.24  DEVELOPER : Initial Created.
 2022.04.13  정재홍    : [C20220316-000571] - GMES(ESWA 전극)시스템의 J/R Manual 출고 개선 요청의 건

**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_011_FIFO_RELEASE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_FIFO_RELEASE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }

        private DataTable _dtJumboRoll;
        private string _electrodeType;
        private string _processCode;
        private string _projectName;
        private string _halfSlitterSideCode;
        private string _productVersion;
        private string _workOrderModel;
        private string _electrodeparameter;
        private long _selectedReleaseCount;
        private readonly Util _util = new Util();

        public bool IsUpdated;

        private bool isManualChk = false;
        #endregion

        public MCS001_011_FIFO_RELEASE()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void InitializeControls()
        {
            dtpDateTo.SelectedDateTime = GetSystemTime();
            dtpDateFrom.SelectedDateTime = GetJUmboRollMinCalDate();
            SelectReleaseCount();
            InitializeComboBox();
        }

        private void InitializeJumboRollByProject()
        {
            _dtJumboRoll = new DataTable();
            _dtJumboRoll.Columns.Add("PRJT_NAME", typeof(string));
            _dtJumboRoll.Columns.Add("PROD_VER_CODE", typeof(string));
            _dtJumboRoll.Columns.Add("QTY_SUM", typeof(Int32));         //수량( IN WAREHOUSE),합계
            _dtJumboRoll.Columns.Add("QTY_A", typeof(Int32));           //수량( IN WAREHOUSE), A
            _dtJumboRoll.Columns.Add("QTY_L", typeof(Int32));           //수량( IN WAREHOUSE), L
            _dtJumboRoll.Columns.Add("QTY_R", typeof(Int32));           //수량( IN WAREHOUSE), R
        }

        private void InitializeComboBox()
        {
            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;
            SetProcessCombo(cboProcess);
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;

            cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            SetEquipmentCombo(cboEquipment);
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;

            GetWorkOrderInfoByEquipment();

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStocker(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _electrodeparameter = Util.NVC(parameters[1]);
            }
            InitializeControls();
            InitializeJumboRollByProject();
            SelectReturnCommand();

            // CSR : [C20220316-000571]
            GetRadioBtnVisible();

            btnSearch_Click(btnSearch,null);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            GetWorkOrderInfoByEquipment();
            SelectJumboRollInfoByProject();
            SetDestinationCombo(cboDestination);

            // CSR : [C20220316-000571]
            GetRadioBtnVisible();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            SetEquipmentCombo(cboEquipment);
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;

            GetWorkOrderInfoByEquipment();
            SelectJumboRollInfoByProject();
            SetDestinationCombo(cboDestination);
            SelectReturnCommand();

            // CSR : [C20220316-000571]
            GetRadioBtnVisible();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            GetWorkOrderInfoByEquipment();
            SelectJumboRollInfoByProject();
            SetDestinationCombo(cboDestination);
            SelectReturnCommand();
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgJumboRollInfo);
            SelectJumboRollInfoByProject();
        }

        private void dgJumboRollByProject_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (dgJumboRollByProject == null || dgJumboRollByProject.CurrentCell == null || dgJumboRollByProject.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgJumboRollByProject.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Column.Name.Equals("QTY_A") || cell.Column.Name.Equals("QTY_L") || cell.Column.Name.Equals("QTY_R"))
                {
                    if (!CommonVerify.IsInt(cell.Value.GetString()) || cell.Value.Equals(0)) return;

                    // 선택한 셀의 위치
                    int rowIdx = cell.Row.Index;
                    //int colIdx = cell.Column.Index;

                    DataRowView drv = dgJumboRollByProject.Rows[rowIdx].DataItem as DataRowView;
                    if (drv == null) return;

                    _projectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();

                    if (cell.Column.Name.Equals("QTY_A"))
                    {
                        _halfSlitterSideCode = "A";
                    }
                    else if (cell.Column.Name.Equals("QTY_L"))
                    {
                        _halfSlitterSideCode = "L";
                    }
                    else if (cell.Column.Name.Equals("QTY_R"))
                    {
                        _halfSlitterSideCode = "R";
                    }
                    _productVersion = DataTableConverter.GetValue(drv, "PROD_VER_CODE").GetString();
                    
                    SelectJumboRollInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgJumboRollByProject_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("QTY_A"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "QTY_A").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("QTY_L"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "QTY_L").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("QTY_R"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "QTY_R").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("PRJT_NAME"))
                        {
                            if (string.Equals(_workOrderModel, DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRJT_NAME").GetString()))
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
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
            }));
        }

        private void dgJumboRollByProject_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (!e.Cell.Column.Name.Contains("QTY_"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
            }));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            #region 기존 소스 주석처리
            //int receiveCommandCount = ValidationJumboRollReceiveCommandCount();

            //if (receiveCommandCount > 0)
            //{
            //    object[] parameters = new object[1];
            //    parameters[0] = receiveCommandCount;

            //    Util.MessageConfirm("SFU6043", result =>
            //    {
            //        if (result == MessageBoxResult.OK)
            //        {
            //            SaveReceiveCommand();
            //        }
            //    }, parameters);
            //}
            //else
            //{
            //    SaveReceiveCommand();
            //}
            #endregion

            if (isManualChk == true)
            {
                if (rdoRelease4.IsChecked == true)
                {
                    MCS001_011_AUTH_CONFIRM authConfirm = new MCS001_011_AUTH_CONFIRM();
                    authConfirm.FrameOperation = this.FrameOperation;
                    if (authConfirm != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = "AREA_USER_AUTH_CHK";   //동별 공통코드
                        Parameters[1] = "lgchem.com";
                        C1WindowExtension.SetParameters(authConfirm, Parameters);
                        authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);

                        this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                    }
                }
                else
                {
                    GetReleaseOrderCreate();
                }
            }
            else
            {
                GetReleaseOrderCreate();
            }
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            MCS001_011_AUTH_CONFIRM window = sender as MCS001_011_AUTH_CONFIRM;
            
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetReleaseOrderCreate();
            }
        }

        private void rdoRelease_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            switch (radioButton.Name)
            {
                case "rdoRelease1":
                    _selectedReleaseCount = 1;
                    break;
                case "rdoRelease2":
                    _selectedReleaseCount = 2;
                    break;
                case "rdoRelease3":
                    _selectedReleaseCount = 3;
                    break;
                case "rdoRelease4":
                    _selectedReleaseCount = 3;
                    break;
            }
            //SetCheckedJumboRollInfoGrid();

            // CSR : [C20220316-000571]
            if (radioButton.Name != "rdoRelease4")
            {
                dgJumboRollInfo.IsReadOnly = true;
                dgJumboRollInfo.Columns["CHK"].IsReadOnly = true;
                SetCheckedJumboRollInfoGrid();
            }
            else
            {
                dgJumboRollInfo.IsReadOnly = false;
                dgJumboRollInfo.Columns["CHK"].IsReadOnly = false;
                _util.gridSelectionManual(dgJumboRollInfo, "CHK", false);
            }
        }

        private void btnReturnCommandCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReturnCommandCancel()) return;
            SaveReturnCommandCancel();
        }

        private void btnReturnCommandSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectReturnCommand();
        }

        private void checkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgReturnCommand);
        }

        private void checkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgReturnCommand);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (dgJumboRollInfo.Rows.Count <= 0)
                return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (rdoRelease4.IsChecked != true)
            {
                if (cb.IsChecked != null)
                    DataTableConverter.SetValue(dgJumboRollInfo.Rows[idx].DataItem, "CHK", false);

                SetCheckedJumboRollInfoGrid();
                return;
            }

            if (_util.GetDataGridCheckCnt(dgJumboRollInfo, "CHK") > _selectedReleaseCount)
            {
                DataTableConverter.SetValue(dgJumboRollInfo.Rows[idx].DataItem, "CHK", false);
                Util.MessageValidation("SFU8483");
                return;
            }
        }

        #endregion

        #region Mehod

        private void ClearControl()
        {
            try
            {
                txtWorkOrder.Text = string.Empty;
                _dtJumboRoll?.Clear();
                Util.gridClear(dgJumboRollInfo);
                Util.gridClear(dgJumboRollByProject);
                cboDestination.ItemsSource = null;

                _electrodeType = string.Empty;
                _processCode = string.Empty;
                _projectName = string.Empty;
                _workOrderModel = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetWorkOrderInfoByEquipment()
        {
            const string bizRuleName = "DA_MCS_SEL_WO_PJT_BY_EQPTID";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                _electrodeType = dtResult.Rows[0]["ELTR_TYPE_CODE"].GetString();
                _processCode = dtResult.Rows[0]["PROCID"].GetString();
                txtWorkOrder.Text = dtResult.Rows[0]["WO_DETL_ID"].GetString() + " : " + dtResult.Rows[0]["PRJT_NAME"].GetString();
                _projectName = dtResult.Rows[0]["PRJT_NAME"].GetString();
                _workOrderModel = dtResult.Rows[0]["PRJT_NAME"].GetString();
            }
        }

        private void SelectJumboRollInfoByProject()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_SUMMARY_BY_PROJECT";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["ELTR_TYPE_CODE"] = _electrodeType;
                //dr["PROCID"] = _processCode;
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue ?? GetAllItemsByComboBox();
                dr["PRJT_NAME"] = _projectName;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (!CommonVerify.HasTableRow(bizResult))
                    {
                        HiddenLoadingIndicator();
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        projectName = x.Field<string>("PRJT_NAME"),version = x.Field<string>("PROD_VER_CODE"),
                    }).Select(g => new{ProjectName = g.Key.projectName,Version = g.Key.version,Count = g.Count()}).ToList();

                    if (query.Any())
                    {
                        foreach (var item in query)
                        {
                            DataRow newRow = _dtJumboRoll.NewRow();

                            newRow["PRJT_NAME"] = item.ProjectName;
                            newRow["PROD_VER_CODE"] = item.Version;
                            newRow["QTY_SUM"] = bizResult.AsEnumerable().Where(w => w.Field<string>("PRJT_NAME").Equals(item.ProjectName) && w.Field<string>("PROD_VER_CODE").Equals(item.Version) ).Sum(s => s.Field<Int32>("QTY"));
                            newRow["QTY_A"] = bizResult.AsEnumerable().Where(w => w.Field<string>("PRJT_NAME").Equals(item.ProjectName) && w.Field<string>("PROD_VER_CODE").Equals(item.Version) && w.Field<string>("HALF_SLIT_SIDE").Equals("A")).Sum(s => s.Field<Int32>("QTY"));
                            newRow["QTY_L"] = bizResult.AsEnumerable().Where(w => w.Field<string>("PRJT_NAME").Equals(item.ProjectName) && w.Field<string>("PROD_VER_CODE").Equals(item.Version) && w.Field<string>("HALF_SLIT_SIDE").Equals("L")).Sum(s => s.Field<Int32>("QTY"));
                            newRow["QTY_R"] = bizResult.AsEnumerable().Where(w => w.Field<string>("PRJT_NAME").Equals(item.ProjectName) && w.Field<string>("PROD_VER_CODE").Equals(item.Version) && w.Field<string>("HALF_SLIT_SIDE").Equals("R")).Sum(s => s.Field<Int32>("QTY"));
                            _dtJumboRoll.Rows.Add(newRow);
                        }
                    }
                    HiddenLoadingIndicator();
                    Util.GridSetData(dgJumboRollByProject, _dtJumboRoll, null);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollInfo()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_LIST_BY_PROJECT";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue ?? GetAllItemsByComboBox();
                //dr["PROCID"] = _processCode;
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["PRJT_NAME"] = _projectName;
                dr["PROD_VER_CODE"] = _productVersion;
                dr["HALF_SLIT_SIDE"] = _halfSlitterSideCode;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["ELTR_TYPE_CODE"] = _electrodeType;
                
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgJumboRollInfo, bizResult, null, true);
                    //SetCheckedJumboRollInfoGrid();

                    if (rdoRelease4.IsChecked == false)
                    {
                        dgJumboRollInfo.IsReadOnly = true;
                        dgJumboRollInfo.Columns["CHK"].IsReadOnly = true;
                        SetCheckedJumboRollInfoGrid();
                    }
                    else
                    {
                        dgJumboRollInfo.IsReadOnly = false;
                        dgJumboRollInfo.Columns["CHK"].IsReadOnly = false;
                        _util.gridSelectionManual(dgJumboRollInfo, "CHK", false);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectReleaseCount()
        {
            const string bizRuleName = "DA_MCS_SEL_COMMCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CMCDTYPE"] = "CWA_JRW_FIFO_DEFAULT_CNT";
            dr["CMCODE"] = "DEFAULT_VAL";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ATTRIBUTE1"].GetInt() > 3 || dtResult.Rows[0]["ATTRIBUTE1"].GetInt() < 1)
                {
                    rdoRelease1.IsChecked = true;
                    _selectedReleaseCount = 1;
                }
                else
                {
                    switch (dtResult.Rows[0]["ATTRIBUTE1"].GetString())
                    {
                        case "1":
                            rdoRelease1.IsChecked = true;
                            _selectedReleaseCount = 1;
                            break;
                        case "2":
                            rdoRelease2.IsChecked = true;
                            _selectedReleaseCount = 2;
                            break;
                        case "3":
                            rdoRelease3.IsChecked = true;
                            _selectedReleaseCount = 3;
                            break;
                        case "4":
                            rdoRelease4.IsChecked = true;
                            _selectedReleaseCount = 3;
                            break;
                    }
                }
            }
        }

        private void SaveReturnCommandCancel()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_CANCEL";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
                inTable.Columns.Add("CANCEL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgReturnCommand.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOGIS_CMD_ID"] = DataTableConverter.GetValue(row.DataItem, "LOGIS_CMD_ID").GetString();
                        newRow["CANCEL_TYPE_CODE"] = DataTableConverter.GetValue(row.DataItem, "CANCEL_TYPE_CODE").GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        SelectReturnCommand();
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

        private void SelectReturnCommand()
        {
            SetDataGridCheckHeaderInitialize(dgReturnCommand);

            loadingIndicator1.Visibility = Visibility.Visible;

            const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_RSV_CMD";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("INOUT_TYPE", typeof(string));
            inTable.Columns.Add("PRJT_NAME", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = "EB";
            dr["INOUT_TYPE"] = null;
            dr["PRJT_NAME"] = null;
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(dr);


            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                loadingIndicator1.Visibility = Visibility.Collapsed;

                if (bizException != null)
                {
                    loadingIndicator1.Visibility = Visibility.Collapsed;
                    Util.MessageException(bizException);
                    return;
                }

                Util.GridSetData(dgReturnCommand, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
            });

        }

        private void SaveReceiveCommand()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_FIFO_ROLLPRESS";

                DataTable dt = DataTableConverter.Convert(cboDestination.ItemsSource);
                DataRow dr = dt.Rows[cboDestination.SelectedIndex];

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("PORT_WRK_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgJumboRollInfo.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        newRow["PORT_ID"] = cboDestination.SelectedValue;
                        newRow["PORT_WRK_MODE"] = dr["PORT_WRK_MODE"].GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["RACK_ID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA,OUTMSG", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        btnSearch_Click(btnSearch, null);
                        IsUpdated = true;

                        SelectReleaseCount();
                        SelectReturnCommand();
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
                Util.MessageException(ex);
            }
        }

        private void SetCheckedJumboRollInfoGrid()
        {
            if (Math.Abs(_selectedReleaseCount) >= 0)
            {
                if (CommonVerify.HasDataGridRow(dgJumboRollInfo))
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgJumboRollInfo.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", Util.NVC(DataTableConverter.GetValue(row.DataItem, "ROW_NUM")).GetInt() <= _selectedReleaseCount ? 1 : 0);
                            dgJumboRollInfo.EndEdit();
                            dgJumboRollInfo.EndEditRow(true);
                        }
                    }
                }
            }
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= checkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += checkHeaderAll_Unchecked;
            }
        }

        private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PROCESS_FIFO_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, cboProcess.SelectedValue.GetString(), _electrodeparameter };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetStocker(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_FIFO_WH_LIST";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            dr["ELTR_TYPE_CODE"] = _electrodeType;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = null;
            newRow["CBO_NAME"] = "-ALL-";
            newRow["ELTR_TYPE_CODE"] = _electrodeType;
            dtResult.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetDestinationCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_RELATION_FIFO_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ELTR_TYPE_CODE"] = _electrodeType;
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgJumboRollInfo))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgJumboRollInfo, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationReturnCommandCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgReturnCommand))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgReturnCommand, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private int ValidationJumboRollReceiveCommandCount()
        {
            const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_RSV_CMD_COUNT";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                return (int) dtResult.Rows[0]["CNT"].GetInt();
            }

            return 0;
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

        private static DateTime GetJUmboRollMinCalDate()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_MIN_CALDATE";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
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

        private string GetAllItemsByComboBox()
        {
            string comboBoxItemsText = string.Empty;

            foreach (object item in cboStocker.Items)
            {
                if (!string.IsNullOrEmpty(((DataRowView)item).Row.ItemArray[0].ToString()))
                {
                    comboBoxItemsText += ((DataRowView)item).Row.ItemArray[0] + ",";
                }
            }

            return comboBoxItemsText;
        }

        private bool IsCommoncodeAttrUse(string sCodeType, string sCmCode, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CBO_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsCommoncodeUse(string sCodeType, string sCmCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = sCmCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetRadioBtnVisible()
        {
            try
            {
                string [] sCMCODE = { LoginInfo.CFG_AREA_ID, cboProcess.SelectedValue.ToString() };

                if (IsCommoncodeAttrUse("JUMBO_ROLL_MANUAL_OUTPUT", string.Empty, sCMCODE))
                {
                    rdoRelease4.Visibility = Visibility.Visible;
                    isManualChk = true;
                }
                else
                {
                    rdoRelease4.Visibility = Visibility.Collapsed;
                    isManualChk = false;
                    if (rdoRelease4.IsChecked == true)
                        SelectReleaseCount();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetReleaseOrderCreate()
        {
            int receiveCommandCount = ValidationJumboRollReceiveCommandCount();

            if (receiveCommandCount > 0)
            {
                object[] parameters = new object[1];
                parameters[0] = receiveCommandCount;

                Util.MessageConfirm("SFU6043", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveReceiveCommand();
                    }
                }, parameters);
            }
            else
            {
                SaveReceiveCommand();
            }
        }

        #endregion
    }
}
