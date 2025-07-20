/*************************************************************************************
 Created Date : 2018.10.03
      Creator : 신광희 차장
   Decription : CWA 전극 수동출고 팝업
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001_PANCAKE_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_MANUAL_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        private bool _isLoaded;
        private string _equipmentCode;
        private string _fromPortCode;
        private string _toPortCode;
        private string _rackStateCode;
        private DataTable _dtRowColumnLayer;
        private readonly Util _util = new Util();
        private string _authConfirmUserId;
        public bool IsUpdated;

        #endregion

        public MCS001_011_MANUAL_ISSUE()
        {
            InitializeComponent();
        }

        #region Initialize 

        private void InitializeControls()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // Destination 콤보박스 Setting
            SetDestinationCombo(cboDestination);

            // Port 출고 그리드(Roll-Press, Coater 방향) 셀의 상태 콤보
            //SetDataGridPortStateCombo(dgPortRollPress.Columns["CURR_INOUT_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
            //SetDataGridPortStateCombo(dgPortCoater.Columns["CURR_INOUT_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
        }


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();

            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataRow drRackInfo = parameters[1] as DataRow;
                _authConfirmUserId = Util.NVC(parameters[2]);
                if (drRackInfo != null)
                    SetRackInfo(drRackInfo);

            }

            // Stocker 정보 Setting
            if (!string.IsNullOrEmpty(_equipmentCode))
            {
                for (int i = 0; i < cboStocker.Items.Count; i++)
                {
                    if (string.Equals(_equipmentCode, ((DataRowView) cboStocker.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboStocker.SelectedIndex = i;
                        cboStocker.IsEnabled = false;
                        break;
                    }
                }
            }

            SelectRowColumnlayerInfo();

            SelectJumboRollPort();
            _isLoaded = true;
        }

        private void SetRackInfo(DataRow drRackInfo)
        {
            txtRackId.Text = drRackInfo["RACK_ID"].GetString();
            txtProjectName.Text = drRackInfo["PRJT_NAME"].GetString();
            txtProductId.Text = drRackInfo["PRODID"].GetString();
            txtProductName.Text = drRackInfo["PRODNAME"].GetString();
            txtStartTime.Text = drRackInfo["WH_RCV_DTTM"].GetString();
            txtLotId.Text = drRackInfo["LOTID"].GetString();
            txtQty.Text = drRackInfo["WIPQTY"].GetString();
            txtvalidDt.Text = drRackInfo["VLD_DATE"].GetString();
            txtPastDay.Text = drRackInfo["PAST_DAY"].GetString();
            txtWipHold.Text = drRackInfo["WIPHOLD"].GetString();

            if (drRackInfo["WIPHOLD"].GetString() == "Y")
            {
                txtWipHold.FontWeight = FontWeights.Bold;
                txtWipHold.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                txtWipHold.FontWeight = FontWeights.Normal;
                txtWipHold.Foreground = new SolidColorBrush(Colors.Black);
            }

            _rackStateCode = drRackInfo["RACK_STAT_CODE"].GetString();
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    string tabItem = ((C1TabItem) ((ItemsControl) sender).Items.CurrentItem).Name.GetString();

                    if (string.Equals(tabItem, "TabRackMove"))
                    {
                        btnPortChange.Visibility = Visibility.Collapsed;
                        btnPortIssue.Visibility = Visibility.Collapsed;
                        //btnPortIssueMove.Visibility = Visibility.Collapsed;
                        btnRackMove.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnPortChange.Visibility = Visibility.Visible;
                        btnPortIssue.Visibility = Visibility.Visible;
                        //btnPortIssueMove.Visibility = Visibility.Visible;
                        btnRackMove.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnRackMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRackMove()) return;
            //Rack 이동 하시겠습니까?
            Util.MessageConfirm("SFU5072", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveRackMove();
                }
            });
        }

        private void btnPortChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationPortChange()) return;

                MCS001_011_PORT_SETUP popupPortSetup = new MCS001_011_PORT_SETUP {FrameOperation = FrameOperation};
                object[] parameters = new object[2];
                parameters[0] = cboStocker.SelectedValue;
                parameters[1] = null;

                C1WindowExtension.SetParameters(popupPortSetup, parameters);

                popupPortSetup.Closed += popupPortSetup_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupPortSetup.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupPortSetup_Closed(object sender, EventArgs e)
        {
            MCS001_011_PORT_SETUP popup = sender as MCS001_011_PORT_SETUP;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                IsUpdated = true;
            }
        }

        private void btnPortIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPortIssue()) return;
            //Port 출고 하시겠습니까?
            Util.MessageConfirm("SFU5073", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePortIssue();
                }
            });
        }

        private void btnPortIssueMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPortIssueMove()) return;

            //Port 출고 및 이동 하시겠습니까?
            Util.MessageConfirm("SFU5074", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePortIssueMove();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //SelectJumboRollPort();
        }


        private void cboRow_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRow?.SelectedValue != null)
            {
                GetColumnList();
            }
        }
        

        private void cboColumn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboColumn?.SelectedValue != null)
            {
                GetLayerList();
            }
        }


        private void dgPortRollPress_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgPortRollPress.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    int rowIndex = 0;
                    foreach (var item in dgPortRollPress.Rows)
                    {
                        if (drv["CHK"].GetString() == "True")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }

                    foreach (C1.WPF.DataGrid.DataGridRow row in dgPortCoater.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        }
                    }

                    if (drv["CHK"].GetString() == "True")
                    {
                        //_fromPortCode = drv["PORT_ID"].GetString();
                        _fromPortCode = drv["TO_PORT_ID"].GetString();
                        SetDestinationCombo(cboDestination);
                    }

                    dgPortCoater.Refresh();

                    if (_util.GetDataGridRowCountByCheck(dgPortRollPress, "CHK", true) < 1)
                    {
                        _fromPortCode = string.Empty;
                        SetDestinationCombo(cboDestination);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPortRollPress_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            /* TODO HOLD 제품이 Roll-Press 방향 포트로도 수동출고 가능하게 임시로 Validation 해제 2월경에 Validation 재적용 필요 
            if (e?.Column != null)
            {
                if (txtWipHold.Text == "Y")
                {
                    Util.MessageValidation("SFU6004");
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
            */
        }

        private void dgPortCoater_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgPortCoater.CurrentRow.DataItem as DataRowView;

                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    int rowIndex = 0;
                    foreach (var item in dgPortCoater.Rows)
                    {
                        if (drv["CHK"].GetString() == "True")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }

                    foreach (var item in dgPortRollPress.Rows)
                    {
                        DataTableConverter.SetValue(item.DataItem, "CHK", false);
                    }

                    if (drv["CHK"].GetString() == "True")
                    {
                        _fromPortCode = drv["PORT_ID"].GetString();
                        SetDestinationCombo(cboDestination);
                    }

                    dgPortRollPress.Refresh();

                    if (_util.GetDataGridRowCountByCheck(dgPortCoater, "CHK", true) < 1)
                    {
                        _fromPortCode = string.Empty;
                        SetDestinationCombo(cboDestination);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgPortCoater_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (txtWipHold.Text == "Y")
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    if (!DataTableConverter.GetValue(e.Row.DataItem, "PORT_TYPE_CODE").Equals("STK_JR_COT_OUT"))
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        e.Cancel = false;
                    }
                }
            }
        }

        #endregion

        private void SaveRackMove()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_JSR_JSR";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_RACK_ID", typeof(string));
                inTable.Columns.Add("X_PSTN", typeof(string));
                inTable.Columns.Add("Y_PSTN", typeof(string));
                inTable.Columns.Add("Z_PSTN", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["FROM_RACK_ID"] = txtRackId.Text;
                newRow["X_PSTN"] = cboRow.SelectedValue;
                newRow["Y_PSTN"] = cboColumn.SelectedValue;
                newRow["Z_PSTN"] = cboLayer.SelectedValue;
                newRow["USERID"] = _authConfirmUserId;
                inTable.Rows.Add(newRow);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, bizException) =>
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


        private void SavePortIssue()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_JSR_JSP";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID", typeof(string));
                inTable.Columns.Add("FROM_RACK_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _equipmentCode;
                newRow["TO_PORT_ID"] = _toPortCode; // TO_PORT_ID
                newRow["FROM_RACK_ID"] = txtRackId.Text;
                newRow["USERID"] = _authConfirmUserId;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA,OUTMSG", (result, bizException) =>
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
                        SelectJumboRollPort();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                },ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SavePortIssueMove()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_JSR_JEP";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_RACK_ID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID_EQPT", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["FROM_RACK_ID"] = txtRackId.Text;
                newRow["TO_PORT_ID"] = _toPortCode;
                newRow["TO_PORT_ID_EQPT"] = cboDestination.SelectedValue;
                newRow["USERID"] = _authConfirmUserId;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, bizException) =>
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
                        SelectJumboRollPort();
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

        #region Mehod

        private void GetColumnList()
        {


            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN")}).Distinct().ToList();

            cboColumn.ItemsSource = queryColumn.ToDataTable().Copy().AsDataView();
            cboColumn.SelectedIndex = 0;
        }

        private void GetLayerList()
        {
            var queryLayer = (from t in _dtRowColumnLayer.AsEnumerable()
                              where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString() 
                              && t.Field<string>("Y_PSTN") == cboColumn.SelectedValue.GetString()
                              orderby t.Field<string>("Z_PSTN").GetInt()
                              select new { CBO_NAME = t.Field<string>("Z_PSTN"), CBO_CODE = t.Field<string>("Z_PSTN") }).ToList();

            cboLayer.ItemsSource = queryLayer.ToDataTable().Copy().AsDataView();
            cboLayer.SelectedIndex = 0;
        }

        private void SelectJumboRollPort()
        {
            SelectJumboRollPortRollPress();
            SelectJumboRollPortCoater();
        }

        private void SelectJumboRollPortRollPress()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_PORT_ROLLPRESS";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgPortRollPress, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollPortCoater()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_PORT_COATER";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //Util.GridSetData(dgPortCoater, bizResult, null, true);
                    Util.GridSetData(dgPortCoater, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectRowColumnlayerInfo()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_BY_XYZ_PSTN";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    _dtRowColumnLayer = searchResult.Copy();

                    var queryRow = (from t in _dtRowColumnLayer.AsEnumerable()
                            orderby t.Field<string>("X_PSTN").GetInt()
                            select new { CBO_CODE = t.Field<string>("X_PSTN")
                            , CBO_NAME = t.Field<string>("X_PSTN") }).Distinct().ToList()
                        ;
                    cboRow.ItemsSource = queryRow.ToDataTable().Copy().AsDataView();
                    cboRow.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationRackMove()
        {
            if (string.IsNullOrEmpty(txtRackId.Text))
            {
                Util.MessageValidation("70003");
                return false;
            }

            if (string.IsNullOrEmpty(txtLotId.Text))
            {
                Util.MessageValidation("SFU1643");
                return false;
            }

            //if (_rackStateCode != "USING")
            if (_rackStateCode.Equals("USING") || _rackStateCode.Equals("NOREAD"))
            {
                return true;
            }

            Util.MessageValidation("SFU1643");
            return false;
        }

        private bool ValidationPortChange()
        {
            return true;
        }

        private bool ValidationPortIssue()
        {
            if (string.IsNullOrEmpty(txtRackId.Text))
            {
                Util.MessageValidation("70003");
                return false;
            }

            if (!_rackStateCode.Equals("USING") && !_rackStateCode.Equals("NOREAD"))
            {
                Util.MessageValidation("SFU1643");
                return false;
            }
            /*
            if (string.IsNullOrEmpty(cboDestination?.SelectedValue?.GetString()))
            {   //PORT가 설정되지 않았습니다.
                Util.MessageValidation("SFU5061");
                return false;
            }
            */
            
            string selectedRollPressPortId = string.Empty;
            string selectedRollPressInoutTypeCode = string.Empty;

            string selectedCoaterPortId = string.Empty;
            string selectedCoaterInputTypeCode = string.Empty;

            if (_util.GetDataGridRowCountByCheck(dgPortRollPress, "CHK", true) > 0)
            {
                selectedRollPressPortId = _util.GetDataGridFirstRowBycheck(dgPortRollPress, "CHK", true).Field<string>("PORT_ID").GetString();
                selectedRollPressInoutTypeCode = _util.GetDataGridFirstRowBycheck(dgPortRollPress, "CHK", true).Field<string>("CURR_INOUT_TYPE_CODE").GetString();
            }

            if (_util.GetDataGridRowCountByCheck(dgPortCoater, "CHK", true) > 0)
            {
                selectedCoaterPortId = _util.GetDataGridFirstRowBycheck(dgPortCoater, "CHK", true).Field<string>("PORT_ID").GetString();
                selectedCoaterInputTypeCode = _util.GetDataGridFirstRowBycheck(dgPortCoater, "CHK", true).Field<string>("CURR_INOUT_TYPE_CODE").GetString();
            }

            if (string.IsNullOrEmpty(selectedRollPressPortId) && string.IsNullOrEmpty(selectedCoaterPortId))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            string selectedInoutTypeCode = string.IsNullOrEmpty(selectedRollPressInoutTypeCode) ? selectedCoaterInputTypeCode : selectedRollPressInoutTypeCode;

            if (selectedInoutTypeCode != "OUT")
            {
                Util.MessageValidation("SFU5075");
                return false;
            }


            _toPortCode = string.IsNullOrEmpty(selectedRollPressPortId) ? selectedCoaterPortId : selectedRollPressPortId;

            return true;
        }

        private bool ValidationPortIssueMove()
        {
            if (string.IsNullOrEmpty(txtRackId.Text))
            {
                Util.MessageValidation("70003");
                return false;
            }

            if (string.IsNullOrEmpty(cboDestination?.SelectedValue?.GetString()))
            {   //PORT가 설정되지 않았습니다.
                Util.MessageValidation("SFU5061");
                return false;
            }

            string selectedRollPressPortId = string.Empty;
            string selectedCoaterPortId = string.Empty;

            if (_util.GetDataGridRowCountByCheck(dgPortRollPress, "CHK", true) > 0)
            {
                selectedRollPressPortId = _util.GetDataGridFirstRowBycheck(dgPortRollPress, "CHK", true).Field<string>("PORT_ID").GetString();
            }

            if (_util.GetDataGridRowCountByCheck(dgPortCoater, "CHK", true) > 0)
            {
                selectedCoaterPortId = _util.GetDataGridFirstRowBycheck(dgPortCoater, "CHK", true).Field<string>("PORT_ID").GetString();
            }

            if (string.IsNullOrEmpty(selectedRollPressPortId) && string.IsNullOrEmpty(selectedCoaterPortId))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            _toPortCode = string.IsNullOrEmpty(selectedRollPressPortId) ? selectedCoaterPortId : selectedRollPressPortId;
            return true;
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "JRW", LoginInfo.CFG_AREA_ID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetDestinationCombo(C1ComboBox cbo)
        {
            //if (string.IsNullOrEmpty(_fromPortCode)) return;

            const string bizRuleName = "DA_MCS_SEL_PORT_RELATION_CBO";
            string[] arrColumn = { "LANGID", "FROM_PORT_ID" };
            string[] arrCondition = { LoginInfo.LANGID, _fromPortCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetDataGridPortStateCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "CMCODE" };
            string[] arrCondition = { LoginInfo.LANGID, "MCS_CURR_INOUT_TYPE_CODE", null };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
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
