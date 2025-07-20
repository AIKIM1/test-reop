/*************************************************************************************
 Created Date : 2020.04.28
      Creator : 이상준
 Decription   : CNJ 전극 수동출고 팝업
--------------------------------------------------------------------------------------
     2020.05.11  이상준 차장 : CSR[C20200311-000027] Lot ID가 없는경우에도 SKID만 출고 가능하도록 함.
     2020.05.18  이상준 차장 : CSR[C20200311-000027] Rack 이동시 우선순위 파라미터 추가 
     2020.05.27  이제섭      : CSR[C20200311-000027] Port  출고 호출시 출고대상설비 파라미터 추가
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
    public partial class MCS001_043_MANUAL_ISSUE : C1Window, IWorkArea
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

        public MCS001_043_MANUAL_ISSUE()
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
            _isLoaded = true;
        }

        private void SetRackInfo(DataRow drRackInfo)
        {
            txtRackId.Text = drRackInfo["RACK_ID"].GetString();
            txtProjectName.Text = drRackInfo["PRJT_NAME"].GetString();
            txtSkid.Text = drRackInfo["MCS_CST_ID"].GetString();
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
                        btnPortIssue.Visibility = Visibility.Collapsed;
                        btnRackMove.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnPortIssue.Visibility = Visibility.Visible;
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
        #endregion

        private void SaveRackMove()
        {
            try
            {
                Int32 iPriorityNo = 0;  // UI 호출시 0 으로 고정

                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_JSR_JSR_NJ";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_RACK_ID", typeof(string));
                inTable.Columns.Add("X_PSTN", typeof(string));
                inTable.Columns.Add("Y_PSTN", typeof(string));
                inTable.Columns.Add("Z_PSTN", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(Int32));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["FROM_RACK_ID"] = txtRackId.Text;
                newRow["X_PSTN"] = cboRow.SelectedValue;
                newRow["Y_PSTN"] = cboColumn.SelectedValue;
                newRow["Z_PSTN"] = cboLayer.SelectedValue;
                newRow["USERID"] = _authConfirmUserId;
                newRow["LOGIS_CMD_PRIORITY_NO"] = iPriorityNo;
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
            string sCststat = GetCstStat();

            // 실 SKID
            if (sCststat == "U")
                IssueReservation();
            // 공 SKID
            else
                IssueReservationEmpty();
        }

        private string GetCstStat()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CST_ID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CST_ID"] = txtSkid.Text;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EMPTY_CST", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count > 0 && !string.IsNullOrWhiteSpace((string)dtResult.Rows[0]["CSTSTAT"]))
                return (string)dtResult.Rows[0]["CSTSTAT"];
            else
                return "U";
        }

        private void IssueReservation()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_JSR_JSP_MULTI_NJ";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");
                indataSet.Tables.Add("IN_RACK");

                indataSet.Tables["INDATA"].Columns.Add("SRCTYPE", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("TO_PORT_ID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("REQ_EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("AREAID", typeof(string));

                indataSet.Tables["IN_RACK"].Columns.Add("FROM_RACK_ID", typeof(string));

                DataRow dr = indataSet.Tables["INDATA"].NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["EQPTID"] = _equipmentCode;
                dr["TO_PORT_ID"] = cboDestination.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                dr["REQ_EQPTID"] = null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                indataSet.Tables["INDATA"].Rows.Add(dr);

                dr = indataSet.Tables["IN_RACK"].NewRow();
                dr["FROM_RACK_ID"] = txtRackId.Text;

                indataSet.Tables["IN_RACK"].Rows.Add(dr);
                                
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_RACK", null, (result, bizException) =>
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
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void IssueReservationEmpty()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                bizRuleName = "BR_MCS_REG_LOGIS_JUMBOROLL_E_SKID_OUT_NJ";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");

                indataSet.Tables["INDATA"].Columns.Add("SRCTYPE", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("AREAID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("CSTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("FROM_RACK_ID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("TO_PORT_ID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));

                DataRow dr = indataSet.Tables["INDATA"].NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = _equipmentCode;
                dr["CSTID"] = txtSkid.Text;
                dr["FROM_RACK_ID"] = txtRackId.Text;
                dr["TO_PORT_ID"] = cboDestination.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
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
                        Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #region Mehod

        private void GetColumnList()
        {
            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN") }).Distinct().ToList();

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

        private void SelectRowColumnlayerInfo()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_BY_XYZ_PSTN";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = cboStocker.SelectedValue;
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
            //const string bizRuleName = "DA_MCS_SEL_PORT_RELATION_FIFO_CBO_NJ";
            const string bizRuleName = "DA_MCS_SEL_PORT_CBO_VD_NJ";
            string[] arrColumn = { "LANGID", "EQPTID" };

            string[] arrCondition = { LoginInfo.LANGID, cboStocker.SelectedValue.ToString() };
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
        
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboStocker?.SelectedValue != null)
            {
                SetDestinationCombo(cboDestination);
            }
        }
        #endregion


    }
}
