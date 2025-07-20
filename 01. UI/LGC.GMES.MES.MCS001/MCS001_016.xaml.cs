/*************************************************************************************
 Created Date : 2018.12.04
      Creator : 신광희
   Decription : 물류 반송 명령 취소
--------------------------------------------------------------------------------------
 [Change History]
  2018.12.04  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_016 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

        public MCS001_016()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        private void InitializeCombo()
        {
            // 설비구분
            SetEquipmentTypeCombo(cboEquipmentType);
            // 설비
            SetEquipmentCombo(cboEquipment);
        }


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (!ValidationSelectLogisticsCommand()) return;

                SetDataGridCheckHeaderInitialize(dgLotList);
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_GET_CANCEL_LOGIS_CMD_LIST";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQPT_TYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQPT_TYPE"] = cboEquipmentType.SelectedValue;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["LOGIS_CMD_ID"] = string.IsNullOrEmpty(txtLogisticsCommandId.Text.Trim()) ? null : txtLogisticsCommandId.Text.Trim();
                dr["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? null : txtLotId.Text.Trim();
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

                        //dgLotList.ItemsSource = DataTableConverter.Convert(result);
                        Util.GridSetData(dgLotList, Util.CheckBoxColumnAddTable(result, false), null, true);
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

        private void btnCancelLogisticsCommand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!ValidationCancelLogisticsCommand()) return;

                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_CANCEL";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
                inTable.Columns.Add("CANCEL_TYPE_CODE", typeof(string));    //취소 유형 코드 ("C"ancel, "R"eserve)
                inTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLotList.Rows)
                {

                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True")
                    {
                        DataRow dr = inTable.NewRow();
                        dr["LOGIS_CMD_ID"] = DataTableConverter.GetValue(row.DataItem, "LOGIS_CMD_ID");

                        if (DataTableConverter.GetValue(row.DataItem, "EQPT_TYPE").GetString() == "AGVC")
                        {
                            if (DataTableConverter.GetValue(row.DataItem, "LOGIS_CMD_STAT_CODE").GetString() == "CREATED")
                            {
                                dr["CANCEL_TYPE_CODE"] = "C";
                            }
                            else
                            {
                                // LOGIS_CMD_STAT_CODE : CMD_RCV, TRSFING
                                dr["CANCEL_TYPE_CODE"] = "R";
                            }
                        }
                        else
                        {
                            // EQPT_TYPE : CRN , LOGIS_CMD_STAT_CODE : CREATED, CMD_RCV, TRSFING
                            dr["CANCEL_TYPE_CODE"] = "C";
                        }
                        dr["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(dr);
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
                        btnSearch_Click(btnSearch, null);
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

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cboEquipmentType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgLotList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgLotList);
        }

        #endregion

        #region Mehod

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

        private void SetEquipmentTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "LOGIS_EQPT_DETL_TYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            string equipmentType = cboEquipmentType.SelectedValue?.GetString();

            const string bizRuleName = "DA_MCS_SEL_EQPTID";
            string[] arrColumn = { "LANGID", "EQPTTYPE", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, equipmentType, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private bool ValidationSelectLogisticsCommand()
        {
            //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            //{
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

            return true;
        }

        private bool ValidationCancelLogisticsCommand()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgLotList, "CHK",true) < 0 || !CommonVerify.HasDataGridRow(dgLotList))
            {
                Util.MessageValidation("SFU1651");
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