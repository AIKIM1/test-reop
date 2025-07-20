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
    /// COM001_109_NOWORK_SMS_EXCEPTION.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_109_NOWORK_SMS_EXCEPTION : C1Window, IWorkArea
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
        public COM001_109_NOWORK_SMS_EXCEPTION()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if(teStart != null)
                teStart.Value = new TimeSpan(GetSystemTime().Hour, GetSystemTime().Minute, GetSystemTime().Second);

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
            if (!ValidationSaveNoWorkException()) return;
            SaveNoWorkException();            
        }

        #endregion

        #region Mehod

        private void InitCombo()
        {
            SetAreaCombo(cboArea);
            SetEquipmentSegment(cboEquipmentSegment, cboArea.SelectedValue.GetString());
            SetDayofWeek(cboDayofWeek);
        }


        #region [BizCall]
        private void SaveNoWorkException()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_EQPT_INS_NOWORK_SMS_EXCEPTION";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("DAY_OF_WEEK", typeof(string));
                inDataTable.Columns.Add("STRT_TIME", typeof(string));
                inDataTable.Columns.Add("END_TIME", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["DAY_OF_WEEK"] = cboDayofWeek.SelectedValue;
                dr["STRT_TIME"] = teStart.Value.GetString().Replace(":","");
                dr["END_TIME"] = teEnd.Value.GetString().Replace(":", "");
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

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

        private void SetDayofWeek(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "WEEK_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, GetToDay(GetSystemTime()));
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

        private static string GetToDay(DateTime dt)
        {
            string todayCode = string.Empty;

            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    todayCode = "2";
                    break;
                case DayOfWeek.Tuesday:
                    todayCode = "3";
                    break;
                case DayOfWeek.Wednesday:
                    todayCode = "4";
                    break;
                case DayOfWeek.Thursday:
                    todayCode = "5";
                    break;
                case DayOfWeek.Friday:
                    todayCode = "6";
                    break;
                case DayOfWeek.Saturday:
                    todayCode = "7";
                    break;
                case DayOfWeek.Sunday:
                    todayCode = "1";
                    break;
            }
            return todayCode;
        }

        #endregion

        #region [Validation]

        private bool ValidationSaveNoWorkException()
        {
            
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (teStart.Value == null)
            {
                //시작시간이 없습니다.
                Util.MessageValidation("SFU1697");
                return false;
            }

            if (teEnd.Value == null)
            {
                //종료시간을 확인 하세요.
                Util.MessageValidation("SFU1912");
                return false;
            }

            if (teStart.Value != null && teEnd.Value != null)
            {
                TimeSpan tsStart = (TimeSpan)teStart.Value;
                DateTime dtStartTime = new DateTime(GetSystemTime().Year
                    , GetSystemTime().Month
                    , GetSystemTime().Day
                    , tsStart.Hours
                    , tsStart.Minutes
                    , tsStart.Seconds
                    , DateTimeKind.Local
                    );

                TimeSpan tsEnd = (TimeSpan)teEnd.Value;
                DateTime dtEndTime = new DateTime(GetSystemTime().Year
                    , GetSystemTime().Month
                    , GetSystemTime().Day
                    , tsEnd.Hours
                    , tsEnd.Minutes
                    , tsEnd.Seconds
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

        private void teStart_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {

        }

        private void teEnd_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {

        }
    }
}
