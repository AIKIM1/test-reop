/*************************************************************************************
 Created Date : 2019.12.24
      Creator : 신광희
   Decription : BOBBIN-SKID 정보관리
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.24  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_037.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_037 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private DataTable _bindingTable;

        private string _UserID = string.Empty; //직접 실행하는 USerID

        public MCS001_037()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> {btnConnectionSave, btnConnectionClearSave };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControl();
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete()) return;
            C1DataGrid dg;

            if (TabItemConnection.IsSelected)
                dg = dgConnectionList;
            else
                dg = dgConnectionClearList;

            for (int i = dg.GetRowCount() - 1; i >= 0; i--)
            {
                DataRowView drv = dg.Rows[i].DataItem as DataRowView;
                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").GetString() == "True")
                {
                    dg.RemoveRow(i);
                }
            }
            _bindingTable.Clear();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if (LoginInfo.USERTYPE == "P") //공정PC만
                {
                    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                    authConfirm.FrameOperation = FrameOperation;
                    if (authConfirm != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = LoginInfo.CFG_AREA_ID == "A7" ? "ASSYAU_MANA" : "ELEC_MANA";
                        Parameters[1] = "lgchem.com";
                        C1WindowExtension.SetParameters(authConfirm, Parameters);
                        authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Save);

                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(authConfirm);
                                authConfirm.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else //공정 PC가 아니면
                {

                    Util.MessageConfirm("SFU1621", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveCarrierDetailInfo();
                        }
                    });

                }
            }
            else // 폴란드 조립3동, 전극2동을 제외한 나머지
            {
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveCarrierDetailInfo();
                    }
                });
            }

        }


        // <summary>
        // LOT 이력저장 인증 팝업 닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_Save(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                SaveCarrierDetailInfo();
            }


            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }




        private void txtConnectionBobbinId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtConnectionBobbinId.Text.Trim()))
                    {
                        if (!ValidationSelect(dgConnectionList, txtConnectionBobbinId.Text)) return;

                        SelectCarrierDetailInfo(txtConnectionBobbinId);
                        //txtConnectionBobbinId.Text = string.Empty;
                        txtConnectionBobbinId.Focus();
                        txtConnectionBobbinId.SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtConnectionSkidId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtConnectionSkidId.Text.Trim()))
                    {
                        if (!ValidationSelect(dgConnectionList, txtConnectionSkidId.Text)) return;

                        SelectCarrierDetailInfo(txtConnectionSkidId);
                        txtConnectionSkidId.Text = string.Empty;
                        txtConnectionSkidId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtConnectionClearBobbinId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtConnectionClearBobbinId.Text.Trim()))
                    {
                        if (!ValidationSelect(dgConnectionClearList, txtConnectionClearBobbinId.Text)) return;

                        SelectCarrierDetailInfo(txtConnectionClearBobbinId);
                        txtConnectionClearBobbinId.Text = string.Empty;
                        txtConnectionClearBobbinId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtConnectionClearSkidId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtConnectionClearSkidId.Text.Trim()))
                    {
                        if (!ValidationSelect(dgConnectionClearList, txtConnectionClearSkidId.Text)) return;

                        SelectCarrierDetailInfo(txtConnectionClearSkidId);
                        txtConnectionClearSkidId.Text = string.Empty;
                        txtConnectionClearSkidId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tabTray_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((C1TabItem) ((ItemsControl) sender).Items.CurrentItem).Name;

            switch (tabItem)
            {
                case "TabItemConnection":
                    
                    break;
                case "TabItemClear":
                    
                    break;
                default:

                    break;
            }
        }


        private void chkConnectionClearHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgConnectionClearList;
            Util.DataGridCheckAllChecked(dg);
        }

        private void chkConnectionClearHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgConnectionClearList;
            Util.DataGridCheckAllUnChecked(dg);
        }

        private void chkConnectionHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgConnectionList;
            Util.DataGridCheckAllChecked(dg);
        }

        private void chkConnectionHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgConnectionList;
            Util.DataGridCheckAllUnChecked(dg);
        }


        #endregion

        #region Method


        private void InitializeControl()
        {
            _bindingTable = Util.MakeDataTable(dgConnectionList, true);
            dgConnectionList.ItemsSource = DataTableConverter.Convert(_bindingTable);
            dgConnectionClearList.ItemsSource = DataTableConverter.Convert(_bindingTable);

            TabItemConnection.IsSelected = true;
            txtConnectionBobbinId.Focus();
            txtConnectionSkidId.IsReadOnly = true;
        }

        private void SelectCarrierDetailInfo(TextBox textBox)
        {
            try
            {
                string bizRuleName;
                C1DataGrid dataGrid;
                string keyValue = textBox.Text;

                if (textBox.Name == "txtConnectionClearBobbinId" || textBox.Name == "txtConnectionClearSkidId")
                    dataGrid = dgConnectionClearList;
                else
                    dataGrid = dgConnectionList;

                SetDataGridCheckHeaderInitialize(dataGrid);

                switch (textBox.Name)
                {
                    case "txtConnectionBobbinId":
                    case "txtConnectionClearBobbinId":
                        bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO";
                        break;
                    case "txtConnectionSkidId":
                        bizRuleName = "DA_MCS_SEL_CARRIER_SKID_INFO";
                        break;
                    case "txtConnectionClearSkidId":
                        bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO_BY_SKID";
                        break;
                    default:
                        bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO";
                        break;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = textBox.Text;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        if (CommonVerify.HasTableRow(result))
                        {
                            if (TabItemConnection.IsSelected)
                            {
                                if (textBox.Name == "txtConnectionBobbinId")
                                {
                                    if (!string.IsNullOrEmpty(result.Rows[0]["OUTER_CSTID"].GetString()))
                                    {
                                        txtConnectionSkidId.Clear();
                                        txtConnectionSkidId.IsReadOnly = true;

                                        //이미 연계된 BOBBIN은 연계할 수 없습니다.
                                        Util.MessageInfo("SFU8138", (messageresult) =>
                                        {
                                            textBox.Focus();
                                        });
                                        return;
                                    }
                                }
                                else
                                {
                                    if (result.Rows[0]["CSTSTAT"].GetString() == "E" && !string.IsNullOrEmpty(result.Rows[0]["BOBBIN"].GetString()))
                                    {
                                        //실 SKID는 연계할 수 없습니다.	
                                        Util.MessageInfo("SFU8139", (messageresult) =>
                                        {
                                            textBox.Focus();
                                        });
                                        return;
                                    }
                                }

                                if (textBox.Name == "txtConnectionBobbinId")
                                {
                                    _bindingTable = result;

                                    txtConnectionSkidId.Clear();
                                    txtConnectionSkidId.IsReadOnly = false;
                                    txtConnectionSkidId.Focus();
                                }

                                else //txtConnectionSkidId
                                {
                                    BindingDataGrid(dataGrid, _bindingTable, keyValue);
                                    txtConnectionSkidId.Clear();
                                    txtConnectionSkidId.IsReadOnly = true;
                                    txtConnectionBobbinId.Focus();
                                }

                            }
                            else
                            {
                                if (string.IsNullOrEmpty(result.Rows[0]["OUTER_CSTID"].GetString()))
                                {
                                    //BOBBIN에 연계된 SKID 정보가 없습니다.
                                    Util.MessageInfo("SFU8140", (messageresult) =>
                                    {
                                        textBox.Focus();
                                    });
                                    return;
                                }

                                BindingDataGrid(dataGrid, result);
                            }
                        }
                        else
                        {
                            Util.MessageInfo("601", (messageresult) =>
                            {
                                textBox.Focus();
                            });
                            return;

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
                Util.MessageException(ex);
            }
        }


        private void SaveCarrierDetailInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                C1DataGrid dg;
                string activeType;

                if (TabItemConnection.IsSelected)
                {
                    dg = dgConnectionList;
                    activeType = "M";
                }
                else
                {
                    dg = dgConnectionClearList;
                    activeType = "R";
                }

                const string bizRuleName = "BR_MCS_REG_BOBBIN_SKID_INFO";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("ACT_TYPE", typeof(string));
                inTable.Columns.Add("BOBBIN_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["ACT_TYPE"] = activeType;
                        newRow["BOBBIN_ID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        newRow["SKID_ID"] = DataTableConverter.GetValue(row.DataItem, "OUTER_CSTID").GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", null, inTable);
                        inTable.Rows.Remove(newRow);
                    }
                }
                HiddenLoadingIndicator();

                Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                SetDataGridCheckHeaderInitialize(dg);
                Util.gridClear(dg);

                /*
                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["ACT_TYPE"] = activeType;
                        newRow["BOBBIN_ID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        newRow["SKID_ID"] = DataTableConverter.GetValue(row.DataItem, "OUTER_CSTID").GetString();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                    SetDataGridCheckHeaderInitialize(dg);
                    Util.gridClear(dg);
                });
                */
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BindingDataGrid(C1DataGrid dg, DataTable dt, string mappingCarrierId = null)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            dg.BeginNewRow();
            dg.EndNewRow(true);
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CHK", true);
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "AREANAME", dt.Rows[0]["AREANAME"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTID", dt.Rows[0]["CSTID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTTYPE_NAME", dt.Rows[0]["CSTTYPE_NAME"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTPROD_NAME", dt.Rows[0]["CSTPROD_NAME"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTSTAT_NAME", dt.Rows[0]["CSTSTAT_NAME"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "OUTER_CSTID", string.IsNullOrEmpty(mappingCarrierId) ? dt.Rows[0]["OUTER_CSTID"].ToString() : mappingCarrierId);
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "USERNAME", dt.Rows[0]["USERNAME"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "UPDDTTM", dt.Rows[0]["UPDDTTM"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTTYPE", dt.Rows[0]["CSTTYPE"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTPROD", dt.Rows[0]["CSTPROD"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CSTSTAT", dt.Rows[0]["CSTSTAT"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "PROCID", dt.Rows[0]["PROCID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "EQPTID", dt.Rows[0]["EQPTID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "PORT_ID", dt.Rows[0]["PORT_ID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "RACK_ID", dt.Rows[0]["RACK_ID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CURR_AREAID", dt.Rows[0]["CURR_AREAID"].ToString());
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "CST_DFCT_FLAG", dt.Rows[0]["CST_DFCT_FLAG"].ToString());

            DataTable resultTable = ((DataView)dg.ItemsSource).Table;
            Util.GridSetData(dg, resultTable, null, true);

        }

        private bool ValidationSelect(C1DataGrid dg, string scanId)
        {
            if (CommonVerify.HasDataGridRow(dg))
            {
                DataTable dt = ((DataView)dg.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                    where t.Field<string>("CSTID") == scanId
                    select t).ToList();

                if (query.Any())
                {
                    Util.MessageValidation("SFU2051", scanId);
                    return false;
                }
                return true;
            }
            return true;
        }

        private bool ValidationSave()
        {
            C1DataGrid dg;

            if (TabItemConnection.IsSelected)
                dg = dgConnectionList;
            else
                dg = dgConnectionClearList;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            C1DataGrid dg;

            if (TabItemConnection.IsSelected)
                dg = dgConnectionList;
            else
                dg = dgConnectionClearList;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                if (dg.Name == "dgConnectionList")
                {
                    allCheck.Unchecked -= chkConnectionHeaderAll_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkConnectionHeaderAll_Unchecked;
                }
                else
                {
                    allCheck.Unchecked -= chkConnectionClearHeaderAll_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkConnectionClearHeaderAll_Unchecked;
                }
            }
        }


        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }




        #endregion


    }
}
