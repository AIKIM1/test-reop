/*************************************************************************************
 Created Date : 2022.07.05
      Creator : 오화백K
   Decription : 수동반송 예약 취소
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.05  오화백      : Initial Created.
 
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_MCS_TRANSFER_CANCEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_MHS_TRANSFER_CANCEL : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public bool IsUpdated;
        private DataTable _dtTransferCancel;
        private string _sProc = string.Empty;
    
        private readonly Util _util = new Util();

    
        public CMM_MHS_TRANSFER_CANCEL()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    

        private void Initialize()
        {
            ApplyPermissions();
            InitializeControls();
            InitializeCombo();
         
        }

        private void InitializeControls()
        {
            gdTransferInfo.Visibility = Visibility.Collapsed;
            TextBlockEquipmentType.Text = ObjectDic.Instance.GetObjectName("EQP_TYPE");
            dgManualTransferCancel.Columns["JOBID"].Visibility = Visibility.Collapsed;

            dgManualTransferCancel.Columns["REQ_TRFID"].Header = ObjectDic.Instance.GetObjectName("반송지시번호");
            dgManualTransferCancel.Columns["INSDTTM"].Header = ObjectDic.Instance.GetObjectName("반송지시일시");
            dgManualTransferCancel.Columns["REQ_TRF_STAT"].Header = ObjectDic.Instance.GetObjectName("반송지시상태");
            dgManualTransferCancel.Columns["SRC_EQPT_TP"].Header = ObjectDic.Instance.GetObjectName("출발지 설비군");
            dgManualTransferCancel.Columns["DST_EQPT_TP"].Header = ObjectDic.Instance.GetObjectName("목적지 설비군");

            dgManualTransferCancel.Columns["CHK"].DisplayIndex = 0;
            dgManualTransferCancel.Columns["REQ_TRFID"].DisplayIndex = 1;
            dgManualTransferCancel.Columns["INSDTTM"].DisplayIndex = 2;
            dgManualTransferCancel.Columns["CARRIERID"].DisplayIndex = 3;
            dgManualTransferCancel.Columns["REQ_TRF_STAT"].DisplayIndex = 4;
            dgManualTransferCancel.Columns["SRC_EQPT_TP"].DisplayIndex = 5;
            dgManualTransferCancel.Columns["SRC_EQPTID"].DisplayIndex = 6;
            dgManualTransferCancel.Columns["SRC_EQPTNAME"].DisplayIndex = 7;

            dgManualTransferCancel.Columns["SRC_LOCID"].DisplayIndex = 8;
            dgManualTransferCancel.Columns["SRC_LOCNAME"].DisplayIndex = 9;

            dgManualTransferCancel.Columns["DST_EQPT_TP"].DisplayIndex = 10;
            dgManualTransferCancel.Columns["DST_EQPTID"].DisplayIndex = 11;
            dgManualTransferCancel.Columns["DST_EQPTNAME"].DisplayIndex = 12;

            SearchArea.Visibility = Visibility.Collapsed;

            DateTime systemDateTime = GetSystemTime();

            if (dtpDateFrom != null)
                dtpDateFrom.SelectedDateTime = systemDateTime;

            if (dtpDateTo != null)
                dtpDateTo.SelectedDateTime = systemDateTime.AddDays(+1);

        }

        private void InitializeCombo()
        {
            //설비군
            SetEquipmentTypeCombo(cboEquipmentType);

            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
       
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();
               
                object[] tmps = C1WindowExtension.GetParameters(this);
                _dtTransferCancel = tmps[0] as DataTable;   // 
                _sProc = tmps[1] as string;   // 

                if (CommonVerify.HasTableRow(_dtTransferCancel))
                {
                    SelectFirstManualTransferCancelListByEsnb();
                }

                Loaded -= C1Window_Loaded;
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDataGridCheckHeaderInitialize(dgManualTransferCancel);

                SelectManualTransferCancelListByEsnb();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void btnManualTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualTransferCancel()) return;

            if(_sProc == "proc")
            {
                SaveManualTransferCancel_Proc();
            }
            else
            {
                SaveManualTransferCancelByEsnb();
            }
           
        }

        #endregion

        #region Mehod
        private void SelectFirstManualTransferCancelListByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "BR_MHS_SEL_TRF_CMD_CANCEL_BY_UI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                foreach (DataRow row in _dtTransferCancel.Rows)
                {
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["CSTID"] = row["CARRIERID"].ToString();
                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);

                    if (CommonVerify.HasTableRow(result))
                    {
                        C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgManualTransferCancel.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                        StackPanel allPanel = allColumn?.Header as StackPanel;
                        CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                        allCheck.IsChecked = true;
                    }

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectManualTransferCancelListByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                const string bizRuleName = "BR_MHS_SEL_TRF_CMD_CANCEL_BY_UI";

                DataTable inDataTable = new DataTable("IN_DATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("EQPT_TP", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROM_DATE"] = dtpDateFrom.SelectedDateTime;
                inData["TO_DATE"] = dtpDateTo.SelectedDateTime;
                inData["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text.Trim()) ? txtCarrierId.Text : null;
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["EQPT_TP"] = cboEquipmentType.SelectedValue;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
  

        private void SaveManualTransferCancelByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_CANCEL_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgManualTransferCancel.Rows)
                {
                    if ((row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                        || (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")) // 2024.11.04. 김영국 - CHK 값이 0 또는 1로 넘어오는 문제로 인해 해당 로직 구현.
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnSearch_Click(btnSearch, null);

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveManualTransferCancel_Proc()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_MHS_REG_ALL_CANCEL_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgManualTransferCancel.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnSearch_Click(btnSearch, null);

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            DataTable inDataTable = new DataTable("INDATA");

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private bool ValidationManualTransferCancel()
        {
            C1DataGrid dg = dgManualTransferCancel;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

    

        private void SetEquipmentCombo(C1ComboBox cbo, string equipmentType = null)
        {
            try
            {
                string bizRuleName;
                DataTable dtResult;

                bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_FOR_MHS";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = equipmentType;
                inTable.Rows.Add(dr);
                dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                DataRow newRow = dtResult.NewRow();
                newRow[cbo.SelectedValuePath.GetString()] = null;
                newRow[cbo.DisplayMemberPath.GetString()] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MHS_SEL_EQUIPMENTGROUP_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

       
        #endregion


    }
}