/*************************************************************************************
 Created Date : 2017.09.18
      Creator : 신광희C
   Decription : 설비 부동 SMS 발송 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.18  DEVELOPER : Initial Created.
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_109 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_109()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            SetAreaCombo(cboArea);
            SetEquipmentSegment(cboEquipmentSegment, cboArea.SelectedValue.GetString());
            SetAreaCombo(cboAreaException);
            SetEquipmentSegment(cboEquipmentSegmentException, cboAreaException.SelectedValue.GetString());
        }

        private void InitializeControls()
        {
            dtpDateTo.SelectedDateTime = GetSystemTime();
            dtpDateFrom.SelectedDateTime = GetSystemTime().AddMonths(-1);
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitializeControls();
            this.Loaded -= UserControl_Loaded;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentSegment(cboEquipmentSegment, cboArea.SelectedValue.GetString());
        }

        private void cboAreaException_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentSegment(cboEquipmentSegmentException, cboAreaException.SelectedValue.GetString());
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationSelect()) return;
            SelectNoworkPlan();

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationAdd()) return;

            COM001_109_NOWORK_PLAN popupNoworkPlan = new COM001_109_NOWORK_PLAN { FrameOperation = FrameOperation };

            object[] parameters = new object[4];
            parameters[0] = cboArea.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = DataTableConverter.Convert(cboArea.ItemsSource);
            parameters[3] = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            C1WindowExtension.SetParameters(popupNoworkPlan, parameters);

            popupNoworkPlan.Closed += new EventHandler(popupNoworkPlan_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupNoworkPlan.ShowModal()));
        }

        private void popupNoworkPlan_Closed(object sender, EventArgs e)
        {
            COM001_109_NOWORK_PLAN popup = sender as COM001_109_NOWORK_PLAN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SelectNoworkPlan();
            }

            // 이력 팝업 종료후 처리
            //this.grdMain.Children.Remove(popup);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            dgNoWorkPlan.EndEdit();
            dgNoWorkPlan.EndEditRow(true);

            if (!ValidationDelete()) return;
            SaveNoworkPlan();
        }

        private void btnExceptionSelect_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationExceptionSelect()) return;
            SelectNoworkException();
        }

        private void btnExceptionAdd_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationExceptionAdd()) return;

            COM001_109_NOWORK_SMS_EXCEPTION popupNoworkException = new COM001_109_NOWORK_SMS_EXCEPTION { FrameOperation = FrameOperation };

            object[] parameters = new object[4];
            parameters[0] = cboArea.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = DataTableConverter.Convert(cboArea.ItemsSource);
            parameters[3] = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            C1WindowExtension.SetParameters(popupNoworkException, parameters);

            popupNoworkException.Closed += new EventHandler(popupNoworkException_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupNoworkException.ShowModal()));
        }

        private void popupNoworkException_Closed(object sender, EventArgs e)
        {
            COM001_109_NOWORK_SMS_EXCEPTION popup = sender as COM001_109_NOWORK_SMS_EXCEPTION;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SelectNoworkException();
            }
        }

        private void btnExceptionDelete_Click(object sender, RoutedEventArgs e)
        {
            dgNoWorkException.EndEdit();
            dgNoWorkException.EndEditRow(true);

            if (!ValidationExceptionDelete()) return;
            SavetNoworkException();
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgNoWorkPlan);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgNoWorkPlan);
        }

        private void chkHeaderAllException_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgNoWorkException);
        }

        private void chkHeaderAllException_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgNoWorkException);
        }

        #endregion

        #region Mehod

        private void SelectNoworkPlan()
        {
            try
            {
                SetDataGridCheckHeaderInitialize(dgNoWorkPlan);

                ShowLoadingIndicator();
                Util.gridClear(dgNoWorkPlan);
                const string bizRuleName = "DA_EQSG_SEL_NOWORK_PLAN"; // 설비 부동 SMS 발송 관리
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("NOWORK_STRT_DTTM", typeof(string));
                inTable.Columns.Add("NOWORK_END_DTTM", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.GetString() == "SELECT" ? null : cboArea.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.GetString() == "SELECT" ? null : cboEquipmentSegment.SelectedValue;
                dr["NOWORK_STRT_DTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["NOWORK_END_DTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();

                inTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        dgNoWorkPlan.ItemsSource = DataTableConverter.Convert(CheckBoxColumnAddTable(searchResult, false));
                    }
                    catch (Exception ex)
                    {
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

        private void SelectNoworkException()
        {
            try
            {
                SetDataGridCheckHeaderInitialize(dgNoWorkException);
                ShowLoadingIndicator();
                Util.gridClear(dgNoWorkException);
                const string bizRuleName = "DA_EQPT_SEL_NOWORK_SMS_EXCEPTION";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaException.SelectedValue.GetString() == "SELECT" ? null : cboAreaException.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegmentException.SelectedValue.GetString() == "SELECT" ? null : cboEquipmentSegmentException.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        dgNoWorkException.ItemsSource = DataTableConverter.Convert(CheckBoxColumnAddTable(searchResult, false));
                    }
                    catch (Exception ex)
                    {
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

        private void SaveNoworkPlan()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_EQSG_UPD_NOWORK_PLAN";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("NOWORK_PLAN_SEQNO", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                
                foreach (C1.WPF.DataGrid.DataGridRow row in dgNoWorkPlan.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
                    {
                        DataRow param = inDataTable.NewRow();
                        param["NOWORK_PLAN_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "NOWORK_PLAN_SEQNO");
                        param["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(param);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        SelectNoworkPlan();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SavetNoworkException()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_EQPT_UPD_NOWORK_SMS_EXCEPTION";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SEQNO", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgNoWorkException.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
                    {
                        DataRow param = inDataTable.NewRow();
                        param["SEQNO"] = DataTableConverter.GetValue(row.DataItem, "SEQNO");
                        param["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(param);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        SelectNoworkException();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationSelect()
        {
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationAdd()
        {
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationDelete()
        {
            if (!CommonVerify.HasDataGridRow(dgNoWorkPlan))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgNoWorkPlan, "CHK",true) < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationExceptionSelect()
        {
            if (cboEquipmentSegmentException.SelectedValue == null || cboEquipmentSegmentException.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationExceptionAdd()
        {
            if (cboEquipmentSegmentException.SelectedValue == null || cboEquipmentSegmentException.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationExceptionDelete()
        {
            if (!CommonVerify.HasDataGridRow(dgNoWorkException))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgNoWorkException, "CHK", true) < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnAdd,
                btnDelete,
                btnSelect,
                btnExceptionAdd,
                btnExceptionDelete,
                btnExceptionSelect
            };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "SYSTEM_ID", "USERID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.SYSID,LoginInfo.USERID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetEquipmentSegment(C1ComboBox cbo, string areaId)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID"};
            string[] arrCondition = { LoginInfo.LANGID,  areaId};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
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

        private static DataTable CheckBoxColumnAddTable(DataTable dt, bool isChecked)
        {
            try
            {
                var dtBinding = dt.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "CHK", DataType = typeof(bool) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["CHK"] = isChecked;
                }
                dtBinding.AcceptChanges();

                return dtBinding;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
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
        #endregion


    }
}
