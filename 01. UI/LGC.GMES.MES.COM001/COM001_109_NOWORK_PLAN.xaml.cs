/*************************************************************************************
 Created Date : 2017.03.04
      Creator : 신광희C
   Decription : 전지 5MEGA-GMES 구축 - 부동계획 등록 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.25  신광희C : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Forms.VisualStyles;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_109_NOWORK_PLAN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_109_NOWORK_PLAN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _selectedAreaCode = string.Empty;
        private string _selectedEquipmentSegmentCode = string.Empty;
        private DataTable _dtArea = null;
        private DataTable _dtEquipmentSegment = null;


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_109_NOWORK_PLAN()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (dpStart != null)
                dpStart.SelectedDateTime = GetSystemTime();

            if(teStart != null)
                teStart.Value = new TimeSpan(GetSystemTime().Hour, GetSystemTime().Minute, GetSystemTime().Second);

            if(dpEnd != null)
                dpEnd.SelectedDateTime = GetSystemTime();

            if(teEnd != null)
                teEnd.Value = new TimeSpan(GetSystemTime().Hour, GetSystemTime().Minute, GetSystemTime().Second);

            cboArea.SelectedValue = _selectedAreaCode;
            cboEquipmentSegment.SelectedValue = _selectedEquipmentSegmentCode;
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _selectedAreaCode = tmps[0].GetString();
                    _selectedEquipmentSegmentCode = tmps[1].GetString();
                    _dtArea = tmps[2] as DataTable;
                    _dtEquipmentSegment = tmps[3] as DataTable;
                }

                ApplyPermissions();
                InitializeControls();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveNoWorkPlan()) return;
            SaveNoWorkPlan();            
        }

        #endregion

        #region Mehod

        private void InitCombo()
        {
            SetAreaCombo(cboArea);
            SetEquipmentSegment(cboEquipmentSegment, cboArea.SelectedValue.GetString());
        }



        #region [BizCall]
        private void SaveNoWorkPlan()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_EQSG_INS_NOWORK_PLAN";

                DateTime dtStartTime;
                if (teStart.Value.HasValue)
                {
                    var spn = (TimeSpan)teStart.Value;
                    dtStartTime = new DateTime(dpStart.SelectedDateTime.Year, dpStart.SelectedDateTime.Month, dpStart.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtStartTime = new DateTime(dpStart.SelectedDateTime.Year, dpStart.SelectedDateTime.Month, dpStart.SelectedDateTime.Day);
                }

                DateTime dtEndTime;
                if (teEnd.Value.HasValue)
                {
                    var spn = (TimeSpan)teEnd.Value;
                    dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year, dpEnd.SelectedDateTime.Month, dpEnd.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year, dpEnd.SelectedDateTime.Month, dpEnd.SelectedDateTime.Day);
                }


                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("NOWORK_STRT_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("NOWORK_END_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("NOWORK_PLAN_NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["NOWORK_STRT_DTTM"] = dtStartTime;
                dr["NOWORK_END_DTTM"] = dtEndTime;
                dr["NOWORK_PLAN_NOTE"] = txtNote.Text;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

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
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
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

        private void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "SYSTEM_ID", "USERID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.SYSID, LoginInfo.USERID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetEquipmentSegment(C1ComboBox cbo, string areaId)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, areaId };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
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

        #region [Validation]

        private bool ValidationSaveNoWorkPlan()
        {
            
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (teEnd.Value != null && teStart != null)
            {
                TimeSpan tsEnd = (TimeSpan) teEnd.Value;
                DateTime dtEndTime = new DateTime(dpEnd.SelectedDateTime.Year
                    , dpEnd.SelectedDateTime.Month
                    , dpEnd.SelectedDateTime.Day
                    , tsEnd.Hours
                    , tsEnd.Minutes
                    , tsEnd.Seconds
                    , DateTimeKind.Local);

                TimeSpan tsStart = (TimeSpan) teStart.Value;
                DateTime dtStartTime = new DateTime(dpStart.SelectedDateTime.Year
                    , dpStart.SelectedDateTime.Month
                    , dpStart.SelectedDateTime.Day
                    , tsStart.Hours
                    , tsStart.Minutes
                    , tsStart.Seconds
                    , DateTimeKind.Local);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //Util.MessageValidation("종료시간이 시작시간보다 전 시간 일 수 없습니다.");
                    Util.MessageValidation("SFU3037");
                    return false;
                }
            }


            return true;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> {btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentSegment(cboEquipmentSegment, cboArea.SelectedValue.GetString());
        }

        private void dpStart_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void teStart_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {

        }

        private void dpEnd_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void teEnd_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {

        }
    }
}
