/*************************************************************************************
Created Date : 2019.10.13
Creator : 신광희
Decription : 설비 알람 이력 조회
--------------------------------------------------------------------------------------
[Change History]
2019.10.13  신광희 : Initial Created.
2019.10.18  신광희 : 설비 알람 이력 조회 조건 변경(설비알람 레벨 콤보박스 조건 추가 및 메인설비 여부 체크 삭제 변경된 BizRule 적용)
2019.11.07  신광희 : 설비 알람 이력 조회 결과 컬럼 추가(해제일시 추가)
2020.11.17  오화백 : 라인정보는 ALL-> SELECT  변경,  설비정보는 SELECT -> ALL로 변경, DA_PRD_SEL_EQPT_ALARM_HIST에 AREAID, EQSGID, PROCID 추가
2022.08.26  안유수S  C20220826-000405  설비 알람 이력 조회 화면 데이터 엑셀 파일로 저장 시 날짜 형식 표기 오류 수정
2022.11.24  정용석 : C20221028-000488  단동 설비 리스트 분리 (팩동)
2024.03.04  이동주 : E20240126-001976  라인 속성 및 설비 다중 선택 적용 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_315 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private bool isSingleEquipmentMode = false;
        private DataTable dtSearchConditionData = new DataTable("RSLTDT");

        public COM001_315()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeEquipmentType();
            InitializeComboBox();
            InitializeDateTimeControl();
            AddControlEvent();
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetEquipmentAlarmHistoryList();
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetBaseInfoWithEquipmentSingle();
            if (cboArea.Items.Count > 0 && cboArea.SelectedValue != null && !cboArea.SelectedValue.Equals("SELECT"))
            {
                SetEquipmentSegmentComboBox();
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcessComboBox();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipmentComboBox();
                Util.gridClear(dgEquipmentAlarmHistoryList);
            }
        }

        private void datePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFromDate.SelectedDateTime.Year > 1 && dtpToDate.SelectedDateTime.Year > 1)
            {
                if ((dtpToDate.SelectedDateTime - dtpFromDate.SelectedDateTime).TotalDays < 0)
                {
                    dtpFromDate.SelectedDateTime = dtpToDate.SelectedDateTime;
                    return;
                }
            }
        }

        private void txtTroubleCode_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void rdoEquipmentAll_Checked(object sender, RoutedEventArgs e)
        {
            isSingleEquipmentMode = false;
            ChangeSearchConditionComboBox();
        }

        private void rdoEquipmentSingle_Checked(object sender, RoutedEventArgs e)
        {
            isSingleEquipmentMode = true;
            ChangeSearchConditionComboBox();
        }
        #endregion

        #region Member Function Lists...
        private void GetBaseInfoWithEquipmentSingle()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_EQUIPMENT_INFO_FOR_SINGLE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("CFG_SYSTEM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("SINGLE_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID; 
                drRQSTDT["AREAID"] = this.cboArea.SelectedValue.ToString();
                drRQSTDT["CFG_SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtSearchConditionData = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtSearchConditionData.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeEquipmentType()
        {
            if ("P7,P8,PA".Contains(LoginInfo.CFG_AREA_ID))
            {
                defColumnEquipmentType.Width = new GridLength(1, GridUnitType.Auto);
                defColumnEquipmentTypeSpace.Width = new GridLength(10, GridUnitType.Pixel);
            }
            else
            {
                defColumnEquipmentType.Width = new GridLength(0, GridUnitType.Pixel);
                defColumnEquipmentTypeSpace.Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private void AddComboBoxControlEvent()
        {
            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
        }

        private void AddControlEvent()
        {
            rdoEquipmentAll.IsChecked = true;
            rdoEquipmentAll.Checked += rdoEquipmentAll_Checked;
            rdoEquipmentSingle.Checked += rdoEquipmentSingle_Checked;
        }

        private void InitializeDateTimeControl()
        {
            DateTime systemTime = GetSystemTime();

            if (dtpFromDate != null)
            {
                dtpFromDate.SelectedDateTime = systemTime;
            }

            if (dtpToDate != null)
            {
                dtpToDate.SelectedDateTime = systemTime.AddDays(+1);
            }

            if (timFromTime != null)
            {
                timFromTime.Value = new TimeSpan(0, 0, 0, 0, 0);
            }

            if (timToTime != null)
            {
                timToTime.Value = new TimeSpan(0, 0, 0, 0, 0);
            }

            dtpFromDate.SelectedDataTimeChanged += datePicker_SelectedDataTimeChanged;
            dtpToDate.SelectedDataTimeChanged += datePicker_SelectedDataTimeChanged;
        }

        private void InitializeComboBox()
        {
            try
            {
                AddComboBoxControlEvent();
                SetAreaComboBox();
                SetEquipmentSegmentComboBox();
                SetProcessComboBox();
                SetEquipmentComboBox();
                SetEIOStateComboBox(cboEioState);
                SetEquipmentAlarmLevelComboBox(cboEquipmentAlarmLevel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeSearchConditionComboBox()
        {
            try
            {
                SetEquipmentSegmentComboBox();
                SetProcessComboBox();
                SetEquipmentComboBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAreaComboBox()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["SYSTEM_ID"] = LoginInfo.SYSID;
                drRQSTDT["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtSearchConditionData.TableName, dtRQSTDT);

                this.cboArea.DisplayMemberPath = "CBO_NAME";
                this.cboArea.SelectedValuePath = "CBO_CODE";
                this.cboArea.ItemsSource = dtRSLTDT.AsDataView();

                if (!LoginInfo.CFG_AREA_ID.Equals(""))
                {
                    this.cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                    if (this.cboArea.SelectedIndex < 0)
                    {
                        this.cboArea.SelectedIndex = 0;
                    }
                }
                else
                {
                    this.cboArea.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegmentComboBox()
        {
            // 동을 선택하세요.
            string areaID = Util.GetCondition(cboArea);
            if (string.IsNullOrWhiteSpace(areaID))
            {
                return;
            }

            // 설비 구성 유형 구분이 전체냐 단동이냐에 따른 Binding Data 분리
            var query = dtSearchConditionData.AsEnumerable().Where(x => x.Field<string>("SINGLE_FLAG").Equals((isSingleEquipmentMode == true) ? "Y" : x.Field<string>("SINGLE_FLAG")))
                                                            .GroupBy(x => x.Field<string>("EQSGID")).Select(x => new
                                                            {
                                                                EQSGID = x.Key,
                                                                EQSGNAME = x.Max(y => y.Field<string>("EQSGNAME")),
                                                            });
            DataTable dt = queryToDataTable(query.ToList());
            if (dt == null)
            {
                dt = new DataTable();
                dt.Columns.Add("EQSGID");
                dt.Columns.Add("EQSGNAME");

                DataRow dr = dt.NewRow();
                dr["EQSGID"] = string.Empty;
                dr["EQSGNAME"] = "-SELECT-";
                dt.Rows.Add(dr);
            }
            else
            {
                DataRow dr = dt.NewRow();
                dr["EQSGNAME"] = "-SELECT-";
                dr["EQSGID"] = string.Empty;
                dt.Rows.InsertAt(dr, 0);
            }

            cboEquipmentSegment.SelectedValuePath = "EQSGID";
            cboEquipmentSegment.DisplayMemberPath = "EQSGNAME";
            cboEquipmentSegment.ItemsSource = dt.AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
                if (cboEquipmentSegment.SelectedIndex < 0)
                {
                    cboEquipmentSegment.SelectedIndex = 0;
                }
            }
            else
            {
                cboEquipmentSegment.SelectedIndex = 0;
            }

            // 설비유형구분이 단동선택이고 , Binding Row 갯수가 1개일 경우 EquipmentSegment ComboBox는 숨기기
            if (isSingleEquipmentMode && query.Count() == 1)
            {
                cboEquipmentSegment.SelectedIndex = 1;
                cboEquipmentSegment.Visibility = Visibility.Collapsed;
                lblEquipmentSegment.Visibility = Visibility.Collapsed;
            }
            else
            {
                cboEquipmentSegment.Visibility = Visibility.Visible;
                lblEquipmentSegment.Visibility = Visibility.Visible;
            }
        }

        private void SetProcessComboBox()
        {
            // 동을 선택하세요.
            string areaID = Util.GetCondition(cboArea);
            if (string.IsNullOrWhiteSpace(areaID))
            {
                return;
            }

            string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

            // 설비 구성 유형 구분이 전체냐 단동이냐에 따른 Binding Data 분리
            var query = dtSearchConditionData.AsEnumerable().Where(x => x.Field<string>("EQSGID").Equals(string.IsNullOrEmpty(equipmentSegment) ? x.Field<string>("EQSGID") : equipmentSegment) &&
                                                                        x.Field<string>("SINGLE_FLAG").Equals((isSingleEquipmentMode == true) ? "Y" : x.Field<string>("SINGLE_FLAG")))
                                                            .GroupBy(x => x.Field<string>("PROCID")).Select(x => new
                                                            {
                                                                PROCID = x.Key,
                                                                PROCNAME = x.Max(y => y.Field<string>("PROCNAME")),
                                                                PROCID_ORDER = x.Max(y => y.Field<long>("PROCID_ORDER"))
                                                            }).OrderBy(x => x.PROCID_ORDER).ThenBy(x => x.PROCID);
            DataTable dt = queryToDataTable(query.ToList());
            if (dt == null)
            {
                dt = new DataTable();
                dt.Columns.Add("PROCID");
                dt.Columns.Add("PROCNAME");

                DataRow dr = dt.NewRow();
                dr["PROCID"] = string.Empty;
                dr["PROCNAME"] = "-SELECT-";
                dt.Rows.Add(dr);
            }
            else
            {
                // 기존 화면 : 최초로드시에는 -SELECT-가 보이는데, 라인 콤보값 바뀌면 SELECT 없어짐. --> 최초로드시에도 SELECT 없어지게 만듬.
                //DataRow dr = dt.NewRow();
                //dr["PROCID"] = string.Empty;
                //dr["PROCNAME"] = "-SELECT-";
                //dt.Rows.InsertAt(dr, 0);
            }

            cboProcess.SelectedValuePath = "PROCID";
            cboProcess.DisplayMemberPath = "PROCNAME";
            cboProcess.ItemsSource = dt.AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cboProcess.SelectedIndex < 0)
                    cboProcess.SelectedIndex = 0;
            }
            else
            {
                if (cboProcess.Items.Count > 0)
                    cboProcess.SelectedIndex = 0;
            }
        }

        private void SetEquipmentComboBox()
        {
            try
            {
                // 동을 선택하세요.
                string areaID = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(areaID))
                {
                    return;
                }
                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);
                string processID = Util.GetCondition(cboProcess);

                // 설비 구성 유형 구분이 전체냐 단동이냐에 따른 Binding Data 분리
                var query = dtSearchConditionData.AsEnumerable().Where(x => x.Field<string>("EQSGID").Equals(string.IsNullOrEmpty(equipmentSegment) ? x.Field<string>("EQSGID") : equipmentSegment) &&
                                                                            x.Field<string>("PROCID").Equals(string.IsNullOrEmpty(processID) ? x.Field<string>("PROCID") : processID) &&
                                                                            x.Field<string>("SINGLE_FLAG").Equals((isSingleEquipmentMode == true) ? "Y" : x.Field<string>("SINGLE_FLAG")))
                                                                .GroupBy(x => x.Field<string>("EQPTID")).Select(x => new
                                                                {
                                                                    EQPTID = x.Key,
                                                                    EQPTNAME = x.Max(y => y.Field<string>("EQPTNAME")),
                                                                    EQPTID_ORDER = x.Max(y => y.Field<Int64>("EQPTID_ORDER"))
                                                                }).OrderBy(x => x.EQPTID_ORDER).ThenBy(x => x.EQPTID);

                DataTable dt = queryToDataTable(query.ToList());
                //if (dt == null || dt.Columns.Count <= 0)
                //{
                //    dt = new DataTable();
                //    dt.Columns.Add("EQPTID");
                //    dt.Columns.Add("EQPTNAME");

                //    DataRow dr = dt.NewRow();
                //    dr["EQPTID"] = string.Empty;
                //    dr["EQPTNAME"] = "-ALL-";
                //    dt.Rows.Add(dr);
                //}
                //else
                //{
                //    DataRow dr = dt.NewRow();
                //    dr["EQPTID"] = string.Empty;
                //    dr["EQPTNAME"] = "-ALL-";
                //    dt.Rows.InsertAt(dr, 0);
                //}

                //cboEquipment.DisplayMemberPath = "EQPTNAME";
                //cboEquipment.SelectedValuePath = "EQPTID";
                //cboEquipment.ItemsSource = dt.AsDataView();

                //if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                //{
                //    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                //    if (cboEquipment.SelectedIndex < 0)
                //        cboEquipment.SelectedIndex = 0;
                //}
                //else
                //{
                //    cboEquipment.SelectedIndex = 0;
                //}

                cboEquipment.ItemsSource = DataTableConverter.Convert(dt);
                cboEquipment.Check(-1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEIOStateComboBox(C1ComboBox c1ComboBox)
        {
            const string bizRuleName = "DA_BAS_SEL_EIOSTATE_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = c1ComboBox.SelectedValuePath;
            string displayMemberText = c1ComboBox.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, c1ComboBox, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentAlarmLevelComboBox(C1ComboBox c1ComboBox)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "EQPT_ALARM_LEVEL_CODE" };
            string selectedValueText = c1ComboBox.SelectedValuePath;
            string displayMemberText = c1ComboBox.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, c1ComboBox, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void GetEquipmentAlarmHistoryList()
        {
            try
            {
                string bizRuleName = isSingleEquipmentMode ? "DA_PRD_SEL_EQPT_ALARM_HIST_FOR_PACK_SINGLE" : "DA_PRD_SEL_EQPT_ALARM_HIST";
                ShowLoadingIndicator();

                var fromTimeSpan = (TimeSpan)timFromTime.Value;
                DateTime dtStartTime = new DateTime(dtpFromDate.SelectedDateTime.Year, dtpFromDate.SelectedDateTime.Month, dtpFromDate.SelectedDateTime.Day, fromTimeSpan.Hours, fromTimeSpan.Minutes, fromTimeSpan.Seconds, DateTimeKind.Local);

                var toTimeSpan = (TimeSpan)timToTime.Value;
                DateTime dtEndTime = new DateTime(dtpToDate.SelectedDateTime.Year, dtpToDate.SelectedDateTime.Month, dtpToDate.SelectedDateTime.Day, toTimeSpan.Hours, toTimeSpan.Minutes, toTimeSpan.Seconds, DateTimeKind.Local);

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("DATETIME_FROM", typeof(string));
                dtRQSTDT.Columns.Add("DATETIME_TO", typeof(string));
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EIOSTAT", typeof(string));
                dtRQSTDT.Columns.Add("EQPT_ALARM_LEVEL_CODE", typeof(string));   // 설비알람레벨코드
                dtRQSTDT.Columns.Add("EQPT_ALARM_CODE", typeof(string));         // 설비알람코드
                // 2020 11.17 오화백 : DA 파라미터 추가
                dtRQSTDT.Columns.Add("AREAID", typeof(string));         // 동
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));         // 라인
                dtRQSTDT.Columns.Add("PROCID", typeof(string));         // 공정

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["DATETIME_FROM"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + fromTimeSpan.Hours.ToString("00") + fromTimeSpan.Minutes.ToString("00") + fromTimeSpan.Seconds.ToString("00"); // dtStartTime;
                drRQSTDT["DATETIME_TO"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + toTimeSpan.Hours.ToString("00") + toTimeSpan.Minutes.ToString("00") + toTimeSpan.Seconds.ToString("00"); //dtEndTime;
                //drRQSTDT["EQPTID"] = cboEquipment.SelectedValue.ToString() == string.Empty ? null : cboEquipment.SelectedValue;
                drRQSTDT["EQPTID"] = cboEquipment.SelectedItemsToString;
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EIOSTAT"] = cboEioState.SelectedValue;
                drRQSTDT["EQPT_ALARM_LEVEL_CODE"] = cboEquipmentAlarmLevel.SelectedValue;
                drRQSTDT["EQPT_ALARM_CODE"] = !string.IsNullOrEmpty(txtEquipmentAlarmCode.Text.Trim()) ? txtEquipmentAlarmCode.Text : null;
                // 2020 11.17 오화백 : DA 파라미터 추가
                drRQSTDT["AREAID"] = cboArea.SelectedValue;
                drRQSTDT["EQSGID"] = cboEquipmentSegment.SelectedValue;
                drRQSTDT["PROCID"] = cboProcess.SelectedValue;
                dtRQSTDT.Rows.Add(drRQSTDT);

                new ClientProxy().ExecuteService(bizRuleName, dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEquipmentAlarmHistoryList, bizResult, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationSearch()
        {
            int validateDateTerm = 8;
            if ((dtpToDate.SelectedDateTime - dtpFromDate.SelectedDateTime).TotalDays > validateDateTerm)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return false;
            }

            // 2020 11 17 오화백 : 라인 정보 체크
            if (cboEquipmentSegment.SelectedValue == null || string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()) || cboEquipmentSegment.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");  // 라인을 선택하세요.
                return false;
            }

            if (cboProcess.SelectedValue == null || string.IsNullOrEmpty(cboProcess.SelectedValue.ToString()) || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboArea.SelectedValue == null || string.IsNullOrEmpty(cboArea.SelectedValue.ToString()) || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을선택하세요
                Util.MessageValidation("SFU1499");
                return false;
            }

            //if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString() == "SELECT")
            //{
            //    Util.MessageValidation("SFU1673");  // 설비를 선택하세요.
            //    return false;
            //}

            if (timToTime.Value != null && timFromTime != null)
            {
                TimeSpan tsEnd = (TimeSpan)timToTime.Value;
                DateTime dtEndTime = new DateTime(dtpToDate.SelectedDateTime.Year
                , dtpToDate.SelectedDateTime.Month
                , dtpToDate.SelectedDateTime.Day
                , tsEnd.Hours
                , tsEnd.Minutes
                , tsEnd.Seconds
                , DateTimeKind.Local);

                TimeSpan tsStart = (TimeSpan)timFromTime.Value;
                DateTime dtStartTime = new DateTime(dtpFromDate.SelectedDateTime.Year
                , dtpFromDate.SelectedDateTime.Month
                , dtpFromDate.SelectedDateTime.Day
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

        private DateTime GetSystemTime()
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

        private DataTable queryToDataTable(IEnumerable<dynamic> records)
        {
            DataTable dt = new DataTable();

            try
            {
                var firstRow = records.FirstOrDefault();
                if (firstRow == null)
                {
                    return null;
                }

                PropertyInfo[] propertyInfos = firstRow.GetType().GetProperties();
                foreach (var propertyinfo in propertyInfos)
                {
                    Type propertyType = propertyinfo.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dt.Columns.Add(propertyinfo.Name, Nullable.GetUnderlyingType(propertyType));
                    }
                    else
                    {
                        dt.Columns.Add(propertyinfo.Name, propertyinfo.PropertyType);
                    }
                }

                foreach (var record in records)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = propertyInfos[i].GetValue(record) != null ? propertyInfos[i].GetValue(record) : DBNull.Value;
                    }

                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dt;
        }
        #endregion
    }
}