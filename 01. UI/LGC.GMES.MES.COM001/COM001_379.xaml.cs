/*************************************************************************************
 Created Date : 2023.05.10
      Creator :  
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.10  DEVELOPER : 김대현    Initial Created.
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; 


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_379 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private DataTable _dtMappingList;
        private DataTable _dtMappingClearList;

        public COM001_379()
        {
            InitializeComponent();
            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            ApplyPermissions();
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMapping);
            listAuth.Add(btnInitMapping);
            listAuth.Add(btnMappingClear);
            listAuth.Add(btnInitMappingClear);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitMappingList();
            InitMappingClearList();
        }

        private void InitMappingList()
        {
            _dtMappingList = new DataTable();
            _dtMappingList.Columns.Add("CHK", typeof(bool));
            _dtMappingList.Columns.Add("CheckBoxIsEnabled", typeof(string));
            _dtMappingList.Columns.Add("NO", typeof(int));
            _dtMappingList.Columns.Add("LOTID", typeof(string));
            _dtMappingList.Columns.Add("PRODID", typeof(string));
            _dtMappingList.Columns.Add("PRODNAME", typeof(string));
            _dtMappingList.Columns.Add("MODLID", typeof(string));
            _dtMappingList.Columns.Add("AVL_DAYS", typeof(string));
            _dtMappingList.Columns.Add("AREANAME", typeof(string));
            _dtMappingList.Columns.Add("EQSGNAME", typeof(string));
            _dtMappingList.Columns.Add("PROCID", typeof(string));
            _dtMappingList.Columns.Add("WIPQTY", typeof(decimal));
            _dtMappingList.Columns.Add("CUT_ID", typeof(string));

            Util.GridSetData(dgMappingList, _dtMappingList, null);
        }

        private void InitMappingClearList()
        {
            _dtMappingClearList = new DataTable();
            _dtMappingClearList.Columns.Add("CHK", typeof(bool));
            _dtMappingClearList.Columns.Add("LOTID", typeof(string));
            _dtMappingClearList.Columns.Add("PRODID", typeof(string));
            _dtMappingClearList.Columns.Add("PRODNAME", typeof(string));
            _dtMappingClearList.Columns.Add("MODLID", typeof(string));
            _dtMappingClearList.Columns.Add("AVL_DAYS", typeof(string));
            _dtMappingClearList.Columns.Add("AREANAME", typeof(string));
            _dtMappingClearList.Columns.Add("EQSGNAME", typeof(string));
            _dtMappingClearList.Columns.Add("PROCID", typeof(string));
            _dtMappingClearList.Columns.Add("WIPQTY", typeof(decimal));
            _dtMappingClearList.Columns.Add("CUT_ID", typeof(string));

            Util.GridSetData(dgMappingClearList, _dtMappingClearList, null);
        }
        #endregion

        #region Button Event
        private void btnInitMapping_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgMappingList);
            txtCstId.Text = string.Empty;
            txtLotID.Text = string.Empty;
            txtCstId.IsEnabled = true;
            txtCstId.Focus();
        }

        private void btnInitMappingClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgMappingClearList);
            SetDataGridCheckHeaderInitialize(dgMappingClearList);
            txtMappingCstId.Text = string.Empty;
            txtMappingCstId.Focus();
        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMappingSkidPancake()) return;
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_PRD_REG_LOAD_LOT_ON_CST";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CST_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["USERID"] = LoginInfo.USERID;
                dr["CST_ID"] = txtCstId.Text;
                inTable.Rows.Add(dr);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgMappingList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Convert.ToBoolean(DataTableConverter.GetValue(row.DataItem, "CHK")) == true)
                    {
                        DataRow newRow = inLot.NewRow();
                        newRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        inLot.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnInitMapping_Click(btnInitMapping, null);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMappingClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMappingClearSkidPancake()) return;
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_UNLOAD_LOT_ON_CST";
                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CST_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["USERID"] = LoginInfo.USERID;
                dr["CST_ID"] = txtMappingCstId.Text;
                inTable.Rows.Add(dr);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgMappingClearList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Convert.ToBoolean(DataTableConverter.GetValue(row.DataItem, "CHK")) == true)
                    {
                        DataRow newRow = inLot.NewRow();
                        newRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        inLot.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnInitMappingClear_Click(btnInitMappingClear, null);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region TextBox Event 
        private void txtCstId_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCstId.Text)) return;
            if (e.Key == Key.Enter)
            {
                txtCstId.IsEnabled = false;
                GetMappingCarrierInfo(txtCstId.Text);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtLotID.Text))
                {
                    Util.MessageValidation("SFU1366");
                    return;
                }
                if (string.IsNullOrEmpty(txtCstId.Text))
                {
                    Util.MessageValidation("SFU1244", result =>
                    {
                        if (result != MessageBoxResult.OK) return;
                        txtLotID.Text = string.Empty;
                        txtCstId.Focus();
                    });
                    return;
                }

                if (!ValidationDuplicationSkidPancake(txtLotID.Text))
                {
                    Util.MessageValidation("SFU2051", result =>
                    {
                        if (result != MessageBoxResult.OK) return;
                        txtLotID.Focus();
                    }, txtLotID.Text);
                    return;
                }

                GetMappingPancakeInfo(txtLotID.Text);
                txtLotID.Text = string.Empty;
            }
        }

        private void txtMappingCstId_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);

            if (string.IsNullOrEmpty(txtMappingCstId.Text)) return;
            if (e.Key == Key.Enter)
            {
                GetMappingClearCarrierInfo(txtMappingCstId.Text);
            }
        }
        #endregion

        #region Etc Event
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgMappingClearList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgMappingClearList);
        }
        #endregion

        #region Method
        private void GetMappingCarrierInfo(string cstId)
        {
            const string bizRuleName = "BR_PRD_GET_CARRIER_INFO";
            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["CSTID"] = cstId;
            dr["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, exception) =>
            {
                if (exception != null)
                {
                    Util.MessageException(exception);
                    txtCstId.Text = string.Empty;
                    txtCstId.IsEnabled = true;
                    txtCstId.Focus();
                    return;
                }

                if (CommonVerify.HasTableRow(result) && !string.IsNullOrEmpty(Util.NVC(result.Rows[0][0])))
                {
                    int maxSeq = 1;
                    DataTable dt = ((DataView)dgMappingList.ItemsSource).Table;
                    if (CommonVerify.HasTableRow(dt))
                    {
                        maxSeq = Convert.ToInt32(dt.Compute("max([NO])", string.Empty)) + 1;
                    }

                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["CHK"] = false;
                        newRow["CheckBoxIsEnabled"] = "False";
                        newRow["NO"] = maxSeq;
                        newRow["LOTID"] = result.Rows[i]["LOTID"];
                        newRow["PRODID"] = result.Rows[i]["PRODID"];
                        newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                        newRow["MODLID"] = result.Rows[i]["MODLID"];
                        newRow["AVL_DAYS"] = result.Rows[i]["AVL_DAYS"];
                        newRow["AREANAME"] = result.Rows[i]["AREANAME"];
                        newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                        newRow["PROCID"] = result.Rows[i]["PROCID"];
                        newRow["WIPQTY"] = result.Rows[i]["WIPQTY"];
                        newRow["CUT_ID"] = result.Rows[i]["CUT_ID"];
                        dt.Rows.Add(newRow);
                        maxSeq++;
                    }
                    Util.GridSetData(dgMappingList, dt, null, true);
                    
                }

                txtLotID.Focus();
            });
        }

        private void GetMappingPancakeInfo(string lotId)
        {
            const string bizRuleName = "BR_PRD_SEL_SKID_LOTID_NJ";

            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("LOT_CUT_SEL", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            if (rdoCutId.IsChecked == true)
            {
                dr["LOT_CUT_SEL"] = "C";
            }
            if (rdoLotId.IsChecked == true)
            {
                dr["LOT_CUT_SEL"] = "L";
            }
            dr["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, exception) =>
            {
                if (exception != null)
                {
                    Util.MessageException(exception);
                    txtLotID.Text = string.Empty;
                    txtLotID.IsEnabled = true;
                    txtLotID.Focus();
                    return;
                }
                if (CommonVerify.HasTableRow(result))
                {
                    int maxSeq = 1;
                    DataTable dt = ((DataView)dgMappingList.ItemsSource).Table;
                    if (CommonVerify.HasTableRow(dt))
                    {
                        maxSeq = Convert.ToInt32(dt.Compute("max([NO])", string.Empty)) + 1;
                    }

                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        DataRow[] drRow = dt.Select("LOTID = '" + Util.NVC(result.Rows[i]["LOTID"]) + "'");
                        if (drRow.Length == 0)
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["CHK"] = true;
                            newRow["CheckBoxIsEnabled"] = "True";
                            newRow["NO"] = maxSeq;
                            newRow["LOTID"] = result.Rows[i]["LOTID"];
                            newRow["PRODID"] = result.Rows[i]["PRODID"];
                            newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                            newRow["MODLID"] = result.Rows[i]["MODLID"];
                            newRow["AVL_DAYS"] = result.Rows[i]["AVL_DAYS"];
                            newRow["AREANAME"] = result.Rows[i]["AREANAME"];
                            newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                            newRow["PROCID"] = result.Rows[i]["PROCID"];
                            newRow["WIPQTY"] = result.Rows[i]["WIPQTY"];
                            newRow["CUT_ID"] = result.Rows[i]["CUT_ID"];
                            dt.Rows.Add(newRow);
                            maxSeq++;
                        }
                    }
                    Util.GridSetData(dgMappingList, dt, null, true);
                }
                else
                {
                    Util.MessageInfo("SFU1195");
                }
            });
            txtLotID.Focus();
        }

        private void GetMappingClearCarrierInfo(string cstId)
        {
            const string bizRuleName = "BR_PRD_GET_CARRIER_INFO";
            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("CSTID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["CSTID"] = cstId;
            dr["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, exception) =>
            {
                if (exception != null)
                {
                    Util.MessageException(exception);
                    txtMappingCstId.Text = string.Empty;
                    txtMappingCstId.Focus();
                    return;
                }

                if (!CommonVerify.HasTableRow(result) || string.IsNullOrEmpty(Util.NVC(result.Rows[0][0])))
                {
                    Util.MessageInfo("SFU4564");
                    return;
                }

                DataTable dt = ((DataView)dgMappingClearList.ItemsSource).Table;
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    DataRow newRow = dt.NewRow();
                    newRow["CHK"] = false;
                    newRow["LOTID"] = result.Rows[i]["LOTID"];
                    newRow["PRODID"] = result.Rows[i]["PRODID"];
                    newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                    newRow["MODLID"] = result.Rows[i]["MODLID"];
                    newRow["AVL_DAYS"] = result.Rows[i]["AVL_DAYS"];
                    newRow["AREANAME"] = result.Rows[i]["AREANAME"];
                    newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                    newRow["PROCID"] = result.Rows[i]["PROCID"];
                    newRow["WIPQTY"] = result.Rows[i]["WIPQTY"];
                    newRow["CUT_ID"] = result.Rows[i]["CUT_ID"];
                    dt.Rows.Add(newRow);
                }
                Util.GridSetData(dgMappingClearList, dt, null, true);
            });
        }

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
        #endregion

        #region Validation
        private bool ValidationMappingSkidPancake()
        {
            if (!CommonVerify.HasDataGridRow(dgMappingList))
            {
                Util.MessageValidation("SFU5069");
                return false;
            }

            return true;
        }

        private bool ValidationDuplicationSkidPancake(string lotId)
        {
            if (CommonVerify.HasDataGridRow(dgMappingList))
            {
                DataTable dt = ((DataView)dgMappingList.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<string>("LOTID") == lotId
                                 select t).ToList();

                if (queryEdit.Any())
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidationMappingClearSkidPancake()
        {
            if (!CommonVerify.HasDataGridRow(dgMappingClearList))
            {
                Util.MessageValidation("SFU5069");
                return false;
            }

            if(dgMappingClearList.IsCheckedRow("CHK")== false)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion
    }
}
