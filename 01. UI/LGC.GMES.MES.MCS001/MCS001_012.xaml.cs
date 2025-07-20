/*************************************************************************************
 Created Date : 2018.09.28
      Creator : 신광희
   Decription : 점보롤 창고 입출고 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.09  DEVELOPER : Initial Created.
  2024.06.25  PARK7142  : ESNJ 7동 전극 점보롤 경우 공SKID 입출고 가능하여 분기 처리
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_012 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public MCS001_012()
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
            SetStockerCombo(cboStocker);
            SetInoutTypeCombo(cboInoutType);
            SetPortCombo(cboPortId);
            
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
                if (!ValidationSelectJumboRollInout())
                    return;

                ShowLoadingIndicator();

                string bizRuleName;

                // ESNJ 7동 전극 점보롤 경우 공SKID 입출고 가능하여 분기 처리
                if(LoginInfo.CFG_AREA_ID == "EE")
                    bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_INOUT_LIST_WITH_E_SKID";
                else
                    bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_INOUT_LIST";

                string autoYn = string.Empty;

                if (cboInoutType.SelectedValue == null)
                    autoYn = null;
                else
                {
                    switch (cboInoutType.SelectedValue.GetString())
                    {
                        case "COMMAND":
                            autoYn = "Y";
                            break;
                        case "MANUAL":
                            autoYn = "N";
                            break;
                    }
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PORTID", typeof(string));
                inTable.Columns.Add("RACKID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("AUTOYN", typeof(string));
                
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["PORTID"] = cboPortId.SelectedValue;
                dr["RACKID"] = string.IsNullOrEmpty(txtRackId.Text.Trim()) ? null : txtRackId.Text.Trim();
                dr["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? null : txtLotId.Text.Trim();
                dr["AUTOYN"] = autoYn;
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

                        Util.GridSetData(dgLotList, result, null, true);
                        //dgLotList.ItemsSource = DataTableConverter.Convert(result);
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

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetPortCombo(cboPortId);
        }

        #endregion

        #region Mehod

        private void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "JRW", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetInoutTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE"};
            string[] arrCondition = { LoginInfo.LANGID, "MCS_CMD_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_BY_EQPTID";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboStocker.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private bool ValidationSelectJumboRollInout()
        {
            //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            //{
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

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