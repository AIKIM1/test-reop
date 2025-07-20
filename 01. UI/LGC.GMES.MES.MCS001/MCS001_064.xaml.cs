/*************************************************************************************
 Created Date : 2021.05.28
      Creator : 조영대
   Decription : 물류설비 알람이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.28  조영대 : Initial Created. (Copy by COM001_315)
  2021.06.17  조영대 : EMS 설비명, EMS 상세 설비명 적용
  2021.09.10  안인효 : C20210831-000510 : 설비명 초기화 변경(SELECT -> ALL)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_064 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public MCS001_064()
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

        private void InitializeControls()
        {
            DateTime systemDateTime = GetSystemTime();

            if (dtpStart != null)
                dtpStart.SelectedDateTime = systemDateTime;

            if (dtpEnd != null)
                dtpEnd.SelectedDateTime = systemDateTime.AddDays(+1);
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            //동
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");

            //설비군
            string[] arrColumn1 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition1 = { LoginInfo.LANGID, cboArea.GetStringValue(), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            cboEqpType.SetDataComboItem("DA_MHS_SEL_EQUIPMENTGROUP_EMS_CBO", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT, false);

            //UNIT 구분
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQGRID", "EQPTID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(), cboEmsDetlEquipment.GetStringValue(), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            cboEmsEquipment.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_EMS_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL);

            // 알람레벨
            cboEquipmentAlarmLevel.SetCommonCode("EQPT_ALARM_LEVEL_CODE", CommonCombo.ComboStatus.ALL, true);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControls();

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetEquipmentAlarmHistoryList();
        }

        private void cboEqpType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비명
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQGRID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            cboEmsDetlEquipment.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_EMS_DETL_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);
            
        }

        private void cboEmsDetlEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //UNIT 구분
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQGRID", "EQPTID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(), cboEmsDetlEquipment.GetStringValue(), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            cboEmsEquipment.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_EMS_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL);
        }

        private void dtpStart_SelectedDateTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpStart.SelectedDateTime > dtpEnd.SelectedDateTime)
            {
                dtpEnd.SelectedDateTime = dtpStart.SelectedDateTime;
            }
            else
            {
                if (dtpStart.SelectedDateTime.AddDays(7) <= dtpEnd.SelectedDateTime)
                {
                    dtpEnd.SelectedDateTime = dtpStart.SelectedDateTime.AddDays(6);
                }
            }
        }

        private void dtpEnd_SelectedDateTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDateTime < dtpStart.SelectedDateTime)
            {
                dtpStart.SelectedDateTime = dtpEnd.SelectedDateTime;
            }
            else
            {
                if (dtpStart.SelectedDateTime.AddDays(7) <= dtpEnd.SelectedDateTime)
                {
                    dtpStart.SelectedDateTime = dtpEnd.SelectedDateTime.AddDays(-6);
                }
            }
        }
        #endregion

        #region Mehod

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

        private bool ValidationSearch()
        {          
            if (cboArea.GetBindValue() == null)
            {
                // 동을선택하세요
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEqpType.GetBindValue() == null)
            {
                // 설비그룹(을)를 선택하세요.
                Util.MessageValidation("SFU4925", lblEqpType.Text);
                return false;
            }
            
            //if (cboEmsDetlEquipment.GetBindValue() == null)
            //{
            //    // 설비를 선택하세요.
            //    Util.MessageValidation("SFU4925", lblEmsDetlEquipment.Text);
            //    return false;
            //}

            if ((dtpEnd.SelectedDateTime - dtpStart.SelectedDateTime).TotalDays > 8)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return false;
            }

            return true;
        }

        private void GetEquipmentAlarmHistoryList()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_EQPT_ALARM_HIST_FOR_MHS";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EMS_EQPT_NAME", typeof(string));
                inTable.Columns.Add("EQPT_ALARM_LEVEL_CODE", typeof(string));
                inTable.Columns.Add("EQPT_ALARM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.GetBindValue();
                dr["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpEnd.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQGRID"] = cboEqpType.GetBindValue();
                if (cboEmsDetlEquipment.GetBindValue() != null)
                {
                    dr["EQPTID"] = cboEmsDetlEquipment.GetBindValue() + "%";
                }
                else
                {
                    dr["EQPTID"] = "%";
                }
                dr["EMS_EQPT_NAME"] = cboEmsEquipment.GetBindValue();
                dr["EQPT_ALARM_LEVEL_CODE"] = cboEquipmentAlarmLevel.GetBindValue();
                dr["EQPT_ALARM_CODE"] = txtEquipmentAlarmCode.GetBindValue();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    dgEquipmentAlarmHistoryList.SetItemsSource(bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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