/*************************************************************************************
 Created Date : 2019.01.07
      Creator : 신광희 차장
   Decription : CWA 전극 수동출고 예약 팝업 - 호출대상 UI(전극 Pancake 창고 모니터링)
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.07  신광희 차장 : Initial Created.
    
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_018_MANUAL_ISSUE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_018_MANUAL_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        private bool _isLoaded;
        private string _equipmentCode;
        private DataTable _dtRowColumnLayer;
        public bool IsUpdated;
        private const string AuthGroup = "LOGIS_MANA";// 물류관리자
        #endregion

        public MCS001_018_MANUAL_ISSUE()
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

            SetStockerCombo(cboStockerPort);

            // Destination 콤보박스 Setting
            //SetDestinationCombo(cboDestination);

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
                DataTable dtRackInfo = parameters[1] as DataTable;

                if (dtRackInfo != null)
                    SetRackInfo(dtRackInfo);
            }

            //SelectRowColumnlayerInfo(_equipmentCode);
            SelectRowColumnlayerInfo(cboStocker.SelectedValue.GetString());
            _isLoaded = true;
        }

        private void SetRackInfo(DataTable dtRackInfo)
        {
            Util.GridSetData(dgRackInfo, dtRackInfo, null, true);
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

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboStockerPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetPortCombo(cboPort);
        }


        private void btnIssueReservation_Click(object sender, RoutedEventArgs e)
        {
            if ((TabLotControl.SelectedItem as C1TabItem)?.Name == "TabRackMove")
            {
                if (dgRackInfo.GetRowCount() > 1)
                {
                    Util.MessageValidation("SFU6006");
                    return;
                }
                IssueReservation();
            }
            else if ((TabLotControl.SelectedItem as C1TabItem)?.Name == "TabPortIssue")
            {
                if (SelectUserLimitInfo())
                {
                    CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

                    object[] parameters = new object[1];
                    parameters[0] = AuthGroup;
                    C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

                    popupAuthConfirm.Closed += popupChangeRackState_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
                }
                else
                {
                    IssueReservation();
                }
            }
        }        

        private void popupChangeRackState_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                IssueReservation();
            }
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
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


        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboStocker?.SelectedValue == null) return;

            SelectRowColumnlayerInfo(cboStocker.SelectedValue.GetString());
        }

        #endregion




        #region Mehod

        private void SelectRowColumnlayerInfo(string equipmentCode)
        {
            try
            {
                //cboRow.ItemsSource = null;
                cboRow.SelectedItem = null;

                const string bizRuleName = "DA_MCS_SEL_RACK_BY_XYZ_PSTN";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = equipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                _dtRowColumnLayer = searchResult.Copy();

                var queryRow = (from t in _dtRowColumnLayer.AsEnumerable()
                    orderby t.Field<string>("X_PSTN").GetInt()
                    select new
                    {
                        CBO_CODE = t.Field<string>("X_PSTN"),
                        CBO_NAME = t.Field<string>("X_PSTN")
                    }).Distinct().ToList();

                cboRow.ItemsSource = queryRow.ToDataTable().Copy().AsDataView();
                cboRow.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SelectUserLimitInfo()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_PORT_BY_USER_AUTH";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PORT_ID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PORT_ID"] = cboPort.SelectedValue;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                string result = searchResult.Rows[0]["CNT"].ToString();

                if (result.Equals("0"))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetColumnList()
        {
            //cboColumn.ItemsSource = null;
            cboColumn.SelectedItem = null;

            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN")}).Distinct().ToList();
            
            cboColumn.ItemsSource = queryColumn.ToDataTable().Copy().AsDataView();
            cboColumn.SelectedIndex = 0;
        }

        private void GetLayerList()
        {
            //cboLayer.ItemsSource = null;
            cboLayer.SelectedItem = null;

            var queryLayer = (from t in _dtRowColumnLayer.AsEnumerable()
                              where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString() 
                              && t.Field<string>("Y_PSTN") == cboColumn.SelectedValue.GetString()
                              orderby t.Field<string>("Z_PSTN").GetInt()
                              select new { CBO_NAME = t.Field<string>("Z_PSTN"), CBO_CODE = t.Field<string>("Z_PSTN") }).ToList();

            cboLayer.ItemsSource = queryLayer.ToDataTable().Copy().AsDataView();
            cboLayer.SelectedIndex = 0;
        }

        private void IssueReservation()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                if ((TabLotControl.SelectedItem as C1TabItem)?.Name == "TabRackMove")
                {
                    bizRuleName = "BR_MCS_REG_LOGIS_CMD_PSR_PSR";

                    DataTable inDataTable = new DataTable("RQSTDT");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("FROM_RACK_ID", typeof(string));
                    inDataTable.Columns.Add("TO_EQPTID", typeof(string));
                    inDataTable.Columns.Add("TO_X_PSTN", typeof(string));
                    inDataTable.Columns.Add("TO_Y_PSTN", typeof(string));
                    inDataTable.Columns.Add("TO_Z_PSTN", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["FROM_RACK_ID"] = DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_ID").GetString();
                    newRow["TO_EQPTID"] = cboStocker.SelectedValue;
                    newRow["TO_X_PSTN"] = cboRow.SelectedValue;
                    newRow["TO_Y_PSTN"] = cboColumn.SelectedValue;
                    newRow["TO_Z_PSTN"] = cboLayer.SelectedValue;
                    newRow["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
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
                else if ((TabLotControl.SelectedItem as C1TabItem)?.Name == "TabPortIssue")
                {
                    bizRuleName = "BR_MCS_REG_LOGIS_CMD_PSR_PSP";

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("FROM_RACK_ID", typeof(string));
                    inDataTable.Columns.Add("TO_PORT_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    foreach (C1.WPF.DataGrid.DataGridRow row in dgRackInfo.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["FROM_RACK_ID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID");
                            dr["TO_PORT_ID"] = cboPort.SelectedValue;
                            dr["USERID"] = LoginInfo.USERID;
                            inDataTable.Rows.Add(dr);
                        }
                    }

                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(inDataTable);
                    //string xml = ds.GetXml();

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
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
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private static void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "PCW", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_OUT_PORT";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboStockerPort.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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
