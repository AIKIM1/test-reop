/*************************************************************************************
 Created Date : 2021.07.21
      Creator : 강동희
   Decription : 롤프레스 대기 모델별 재공 수량 설정
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.21  강동희 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_066 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        private readonly Util _util = new Util();

        public MCS001_066()
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
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
            SetCombo();
        }

        private void SetCombo()
        {
            try
            {
                //설비 ID Set
                const string bizRuleName = "DA_SEL_MMD_MCS_COMMONCODE_CBO";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CMCDTYPE", typeof(string));
                inDataTable.Columns.Add("CMCDUSE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ROL_WAIT_QTY_SET_STK";
                dr["CMCDUSE"] = "Y";
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
                cboEqpId.DisplayMemberPath = cboEqpId.DisplayMemberPath;
                cboEqpId.SelectedValuePath = cboEqpId.SelectedValuePath;
                cboEqpId.ItemsSource = dtResult.Copy().AsDataView();
                cboEqpId.SelectedIndex = 0;

                //사용 여부 Set
                SetDataGridUseFlagCombo(dgModelList.Columns["USE_FLAG"]);

                //제품 Set
                SetDataGridProdCombo(dgModelList.Columns["PRODID"], _bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            DataTable bindingTable = Util.MakeDataTable(dgModelList, true);
            dgModelList.ItemsSource = DataTableConverter.Convert(bindingTable);


            Initialize();
            Loaded -= UserControl_Loaded;
        }

        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                    {
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    }
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                    {
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    }
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                    {
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    }
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                    {
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    }
                    else
                    {
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                    }
                }
            }
        }
        #endregion

        #region Event
        private void cboStkId_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowAdd(dgModelList, int.Parse(Util.NVC(numAddCount.Value)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridRowDelete(dgModelList, int.Parse(Util.NVC(numAddCount.Value)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;
            SaveRolWaitMdlSetQty();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!ValidationSelect()) return;

                GetRolWaitMdlSetQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgModelList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgModelList);
        }

        private void dgModelList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("INSUSER", LoginInfo.USERID);
            e.Item.SetValue("UPDUSER", LoginInfo.USERID);
        }

        private void dgModelList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                string[] exceptionColumn = { "CHK" };
                Util.DataGridRowEditByCheckBoxColumn(e, exceptionColumn);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModelList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        ////////////////////////////////////////////  default 색상 및 Cursor
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        ///////////////////////////////////////////////////////////////////////////////////

                        if (!e.Cell.Column.Name.ToString().Equals("CHK") && !e.Cell.Column.Name.ToString().Equals("USE_FLAG") && 
                            !e.Cell.Column.Name.ToString().Equals("PRODID") && !e.Cell.Column.Name.ToString().Equals("SET_QTY"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }

                        if (e.Cell.Column.Name.ToString().Equals("CHK") || e.Cell.Column.Name.ToString().Equals("USE_FLAG") ||
                            e.Cell.Column.Name.ToString().Equals("SET_QTY"))
                        {
                            e.Cell.Presenter.IsEnabled = true;
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
        // 데이터 조회
        private void GetRolWaitMdlSetQty()
        {
            try
            {
                Util.gridClear(dgModelList);

                const string bizRuleName = "BR_GET_ROL_WAIT_MDL_SET_QTY";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEqpId.GetBindValue();
                inDataTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgModelList, bizResult, FrameOperation, false);
                    }

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 데이터 저장
        private void SaveRolWaitMdlSetQty()
        {
            try
            {
                const string bizRuleName = "BR_REG_ROL_WAIT_STK_SET_QTY";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("SET_QTY", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgModelList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        if (cboEqpId.GetBindValue() == null)
                        {
                            //%1(을)를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("EQPTID"));
                            return;
                        }

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "USE_FLAG").GetString()))
                        {
                            //%1(을)를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("USE_FLAG"));
                            return;
                        }

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "PRODID").GetString()))
                        {
                            //%1(을)를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PRODUCT"));
                            return;
                        }

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SET_QTY").GetString()))
                        {
                            //%1(을)를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("SETTING_QTY"));
                            return;
                        }

                        DataRow dr = inDataTable.NewRow();
                        dr["EQPTID"] = cboEqpId.GetBindValue();
                        dr["PRODID"] = DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();
                        dr["SET_QTY"] = DataTableConverter.GetValue(row.DataItem, "SET_QTY").GetString();
                        dr["USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "USE_FLAG").GetString();
                        dr["USER"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                ShowLoadingIndicator();
                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "", inDataTable, (bizResult, bizException) =>
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
                        SetDataGridCheckHeaderInitialize(dgModelList);
                        btnSearch_Click(null, null);
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

        // Grid Head CheckBox 초기화
        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        // Grid 사용 여부 컬럼 Set
        private static void SetDataGridUseFlagCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "CMCDTYPE", "LANGID" };
            string[] arrCondition = { "IUSE", LoginInfo.LANGID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dgcol, selectedValueText, displayMemberText);
        }

        // Grid 제품 컬럼 Set
        private static void SetDataGridProdCombo(C1.WPF.DataGrid.DataGridColumn dgcol, string bizRuleIp, string bizRuleProtocol, string bizRulePort, string bizRuleServiceMode, string bizRuleServiceIndex)
        {
            const string bizRuleName = "DA_SEL_PROD_BY_ROL_WIAT_STK";
            string[] arrColumn = { "LANGID", "CLSS2_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, "APC" };
            string selectedValueText = dgcol.SelectedPopupFindDataColumnValuePath();
            string displayMemberText = dgcol.DisplayPopupFindDataColumnMemberPath();
            CommonCombo.SetDataGridPopupFindDataColumnComboItem(bizRuleIp, bizRuleProtocol, bizRulePort, bizRuleServiceMode, bizRuleServiceIndex, bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dgcol, selectedValueText, displayMemberText);
        }

        // 조회 Validation
        private bool ValidationSelect()
        {
            if (cboEqpId.GetBindValue() == null)
            {
                //%1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", lblEqpId.Text);
                return false;
            }
            return true;
        }

        // 저장 Validation
        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgModelList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgModelList, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
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