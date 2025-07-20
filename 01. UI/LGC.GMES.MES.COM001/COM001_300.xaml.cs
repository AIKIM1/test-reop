/*************************************************************************************
 Created Date : 2019.04.22
      Creator : 정문교
   Decription : 월력관리 화면
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.22  정문교 : Initial Created.
  2019.12.01  정문교 : 공정콤보 조립 공정만 조회 되게 수정
**************************************************************************************/

using C1.C1Schedule;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_300 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        /// <summary>
        /// 스케줄러 표시되는 월 임시저장용 변수
        /// </summary>
        private DateTime datetimeTemp = DateTime.Today;
        private string sStartTime = string.Empty;
        private string sEndTime = string.Empty;
        private bool _load = true;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public COM001_300()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            #region 월력 등록 현황
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정 
            C1ComboBox[] cboProcessParent = { cboArea };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_BY_AREAID_PCSG");
            #endregion

            #region 월력 등록
            //동
            C1ComboBox[] cboArea_RChild = { cboEquipmentSegment_R };
            combo.SetCombo(cboArea_R, CommonCombo.ComboStatus.SELECT, cbChild: cboArea_RChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegment_RParent = { cboArea_R };
            C1ComboBox[] cboEquipmentSegment_RChild = { cboProcess_R };
            combo.SetCombo(cboEquipmentSegment_R, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegment_RChild, cbParent: cboEquipmentSegment_RParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcess_RParent = { cboArea_R };
            //combo.SetCombo(cboProcess_R, CommonCombo.ComboStatus.SELECT, cbParent: cboProcess_RParent, sCase: "PROCESS");
            combo.SetCombo(cboProcess_R, CommonCombo.ComboStatus.SELECT, cbParent: cboProcess_RParent, sCase: "PROCESS_BY_AREAID_PCSG");
            #endregion

            #region 작업자 등록 현황
            //동
            C1ComboBox[] cboArea_WChild = { cboEquipmentSegment_W };
            combo.SetCombo(cboArea_W, CommonCombo.ComboStatus.SELECT, cbChild: cboArea_WChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegment_WParent = { cboArea_W };
            C1ComboBox[] cboEquipmentSegment_WChild = { cboProcess_W };
            combo.SetCombo(cboEquipmentSegment_W, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegment_WChild, cbParent: cboEquipmentSegment_WParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcess_WParent = { cboArea_W };
            //combo.SetCombo(cboProcess_W, CommonCombo.ComboStatus.ALL, cbParent: cboProcess_WParent, sCase: "PROCESS");
            combo.SetCombo(cboProcess_W, CommonCombo.ComboStatus.ALL, cbParent: cboProcess_WParent, sCase: "PROCESS_BY_AREAID_PCSG");
            #endregion
        }

        private void SetControls()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(-3);
            //dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(3);

            //dtpDateFrom_W.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(-3);
            //dtpDateTo_W.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(3);

            //dtpMonth_R.SelectedDateTime = DateTime.Now;

            //월력의 language 환경설정의 language로 변경.
            Scheduler.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControls();

            this.Loaded -= UserControl_Loaded;
        }

        private void tabCalendar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tabCalendar.SelectedItem).Name.Equals("ctbRegister"))
            {
                if (_load)
                {
                    dtpMonth_R.SelectedDataTimeChanged += dtpMoth_R_SelectedDataTimeChanged;
                    _load = false;
                }
            }
            else 
            {
                //btnSave.IsEnabled = false;
            }
        }

        #region 월력 등록 현황
        private void dgRegisterList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                switch (e.Cell.Column.Name)
                {
                    case "EQSGNAME":
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        break;
                    case "REGISTER_COUNT":
                        if (e.Cell.Text == "0")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }

                        break;

                    default:
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        break;
                }
            }));
        }

        private void dgRegisterList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgRegisterList.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Column.Name == "EQSGNAME")
                {
                    DataRowView dr = dgRegisterList.Rows[cell.Row.Index].DataItem as DataRowView;
                    TabMove_Schedule(dr);

                    SetDisplayScheduler();
                    SetScheduleInfomation();
                }
            }
        }

        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchProcess();
        }
        #endregion

        #region 월력 등록

        /// <summary>
        /// 월 변경시 Scheduler 해당월로 변경
        /// </summary>
        private void dtpMoth_R_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //SetDisplayScheduler();
            //SetScheduleInfomation();
        }

        private void Scheduler_StyleChanged(object sender, RoutedEventArgs e)
        {
            Scheduler.Style = Scheduler.MonthStyle;

        }

        /// <summary>
        /// Scheduler 더블클릭시
        /// open되는 기본 입력팝업 무시
        /// <summary>
        private void Scheduler_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Scheduler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// drop 무시
        /// </summary>
        private void Scheduler_BeforeAppointmentDrop(object sender, C1.WPF.Schedule.AppointmentActionEventArgs e)
        {
            e.Handled = true;
        }

        private void Scheduler_KeyDown(object sender, KeyEventArgs e)
        {
            //key입력으로 작업조 이름을 변경하지못하도록 함.
            e.Handled = true;
        }

        /// <summary>
        /// 일정선택시 삭제 대기 정보에 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_SelectedAppointmentChanged(object sender, C1.WPF.Schedule.PropertyChangeEventArgsBase e)
        {
        }

        private void Scheduler_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Point pnt = e.GetPosition(null);

            C1.C1Schedule.Appointment appointment = Scheduler.SelectedAppointment;

            if (appointment == null) return;

            popUpDelete(appointment);
        }

        /// <summary>
        /// 전월 조회
        /// </summary>
        private void btnBefore_Click(object sender, RoutedEventArgs e)
        {
            dtpMonth_R.SelectedDateTime = (dtpMonth_R.SelectedDateTime).AddMonths(-1);
            SetDisplayScheduler();
            SetScheduleInfomation();
        }

        /// <summary>
        /// 당월 조회
        /// </summary>
        private void btnMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpMonth_R.SelectedDateTime = DateTime.Today;
            SetDisplayScheduler();
            SetScheduleInfomation();
        }

        /// <summary>
        /// 다음월 조회
        /// </summary>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            dtpMonth_R.SelectedDateTime = (dtpMonth_R.SelectedDateTime).AddMonths(1);
            SetDisplayScheduler();
            SetScheduleInfomation();
        }

        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_R_Click(object sender, RoutedEventArgs e)
        {
            SetDisplayScheduler();
            SetScheduleInfomation();
        }

        /// <summary>
        /// 월력 등록
        /// </summary>
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            COM001_300_REGISTER popupRegister = new COM001_300_REGISTER { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRegister.Name.ToString()) == false)
                return;

            object[] parameters = new object[3];
            parameters[0] = Util.NVC(cboArea_R.SelectedValue);
            parameters[1] = Util.NVC(cboEquipmentSegment_R.SelectedValue);
            parameters[2] = Util.NVC(cboProcess_R.SelectedValue);
            C1WindowExtension.SetParameters(popupRegister, parameters);

            popupRegister.Closed += new EventHandler(popupRegister_Closed);
            grdMain.Children.Add(popupRegister);
            popupRegister.BringToFront();
        }

        private void popupRegister_Closed(object sender, EventArgs e)
        {
            COM001_300_REGISTER popup = sender as COM001_300_REGISTER;
            if (popup != null && popup.Save == true)
            {
                //다시 월력 조회
                cboArea_R.SelectedValue = popup.Area;
                cboEquipmentSegment_R.SelectedValue = popup.EquipmentSegment;
                cboProcess_R.SelectedValue = popup.Process;
                dtpMonth_R.SelectedDateTime = new DateTime(int.Parse(popup.YearMonth.Substring(0,4)), int.Parse(popup.YearMonth.Substring(5, 2)), 1);

                SetDisplayScheduler();
                SetScheduleInfomation();
            }
            grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 월력 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            popUpDelete(null);
        }

        #endregion

        #region 작업자 등록 현황
        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_W_Click(object sender, RoutedEventArgs e)
        {
            Search_WProcess();
        }
        #endregion

        #endregion

        #region Mehod
        /// <summary>
        /// 월력 등록 현황 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WORKCALENDAR_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("MONTHFROM", typeof(string));
                dtRqst.Columns.Add("MONTHTO", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MONTHFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM");
                dr["MONTHTO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM");
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgRegisterList, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 월력 등록 조회
        /// </summary>
        private DataTable SearchScheduleData()
        {
            try
            {
                if (!ValidationSearchScheduleData()) return null;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("YM", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["YM"] = dtpMonth_R.SelectedDateTime.ToString("yyyy-MM");
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea_R, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_R, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess_R, bAllNull: true);
                dtRqst.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKCALENDAR_L", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetScheduleInfomation()
        {
            try
            {
                Color[] colorSet = { Color.FromRgb(226, 140, 5), Color.FromRgb(86, 170, 28), Color.FromRgb(214, 96, 109), Color.FromRgb(21, 154, 182), Color.FromRgb(149, 80, 144) };
                DataTable dtTemp = SearchScheduleData();

                if (dtTemp == null) return;

                Scheduler.DataStorage.AppointmentStorage.Appointments.Clear();

                int iColorNumber = 0;
                for (int i = 0; i < dtTemp.Rows.Count; i++)
                {
                    C1.C1Schedule.Label label = new C1.C1Schedule.Label();

                    bool bAppCreateFlag = true;

                    //동일한 Appointments 존재시 End Time에 추가
                    for (int iApp = 0; iApp < Scheduler.DataStorage.AppointmentStorage.Appointments.Count; iApp++)
                    {
                        bAppCreateFlag = true;
                        if (Util.NVC(Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].Tag)
                                == (Util.NVC(dtTemp.Rows[i]["AREAID"]) + "," +
                                    Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "," +
                                    Util.NVC(dtTemp.Rows[i]["PROCID"]) + "," +
                                    Util.NVC(dtTemp.Rows[i]["SHFT_ID"]) + "," +
                                    Util.NVC(dtTemp.Rows[i]["WRK_GR_ID"]) + "," +
                                    dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + 
                                    dtTemp.Rows[i]["WRK_END_HMS"].ToString())
                            && Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].End
                                == Util.StringToDateTime(Util.NVC(dtTemp.Rows[i]["CALDATE_DATE"]), "yyyyMMdd"))
                        {
                            Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].End = Util.StringToDateTime(Util.NVC(dtTemp.Rows[i]["CALDATE_DATE"]), "yyyyMMdd").AddDays(1);
                            bAppCreateFlag = false;
                            break;
                        }
                    }
                    //동일한 Appointments 없는경우 신규 추가
                    if (bAppCreateFlag)
                    {
                        string sSubject = "(" + Util.NVC(dtTemp.Rows[i]["SHFT_ID"])  + ")" + Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + " " + dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString();
                        string sLocation = "";

                        #region Color 설정
                        bool bSameColorFlag = true;
                        for (int iApp = 0; iApp < Scheduler.DataStorage.AppointmentStorage.Appointments.Count; iApp++)
                        {
                            if (Util.NVC(Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].Tag)
                                    == (Util.NVC(dtTemp.Rows[i]["AREAID"]) + "," +
                                        Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "," +
                                        Util.NVC(dtTemp.Rows[i]["PROCID"]) + "," +
                                        Util.NVC(dtTemp.Rows[i]["SHFT_ID"]) + "," +
                                        Util.NVC(dtTemp.Rows[i]["WRK_GR_ID"]) + "," +
                                        dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + 
                                        dtTemp.Rows[i]["WRK_END_HMS"].ToString())
                                    )
                            {
                                //동일한 라인,조 와 같은 색 표시
                                label.Color = Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].Label.Color;
                                bSameColorFlag = false;
                            }

                        }
                        if (bSameColorFlag)
                        {
                            label.Color = colorSet[iColorNumber];
                        }
                        if (iColorNumber >= 4)
                        {
                            iColorNumber = 0;
                        }
                        else
                        {
                            iColorNumber++;
                        }
                        #endregion

                        Appointment app = new Appointment(Util.StringToDateTime(dtTemp.Rows[i]["CALDATE_DATE"].ToString(), "yyyyMMdd"), Util.StringToDateTime(dtTemp.Rows[i]["CALDATE_DATE"].ToString(), "yyyyMMdd"), sSubject);
                        app.BusyStatus = this.Scheduler.DataStorage.StatusStorage.Statuses[C1.C1Schedule.StatusTypeEnum.Tentative];
                        app.AllDayEvent = true;
                        app.Label = label;
                        app.Location = sLocation;
                        app.Tag = Util.NVC(dtTemp.Rows[i]["AREAID"]) + "," +
                                  Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "," +
                                  Util.NVC(dtTemp.Rows[i]["PROCID"]) + "," +
                                  Util.NVC(dtTemp.Rows[i]["SHFT_ID"]) + "," +
                                  Util.NVC(dtTemp.Rows[i]["WRK_GR_ID"]) + "," +
                                  dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + 
                                  dtTemp.Rows[i]["WRK_END_HMS"].ToString();

                        Scheduler.DataStorage.AppointmentStorage.Appointments.Add(app);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업자 등록 현황 조회
        /// </summary>
        private void Search_WProcess()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_CALDATE_EQSG_PROC_WRKR_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = dtpDateFrom_W.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["DATE_TO"] = dtpDateTo_W.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea_W, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_W, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess_W, bAllNull: true);
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgWorkerList, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearchScheduleData()
        {
            if (cboArea_R.SelectedIndex < 0 || cboArea_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment_R.SelectedIndex < 0 || cboEquipmentSegment_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcess_R.SelectedIndex < 0 || cboProcess_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            //if (cboArea_R.SelectedIndex < 0 || cboArea_R.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 동을 선택하세요.
            //    Util.MessageValidation("SFU1499");
            //    return false;
            //}

            //if (cboEquipmentSegment_R.SelectedIndex < 0 || cboEquipmentSegment_R.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 라인을 선택하세요.
            //    Util.MessageValidation("SFU1223");
            //    return false;
            //}

            //if (cboProcess_R.SelectedIndex < 0 || cboProcess_R.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 공정을 선택하세요.
            //    Util.MessageValidation("SFU1459");
            //    return false;
            //}

            return true;
        }

        private bool ValidationDelete()
        {
            if (cboArea_R.SelectedIndex < 0 || cboArea_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment_R.SelectedIndex < 0 || cboEquipmentSegment_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcess_R.SelectedIndex < 0 || cboProcess_R.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            return true;
        }
        #endregion

        #region [팝업]
        private void popUpDelete(C1.C1Schedule.Appointment appointment)
        {
            if (!ValidationDelete())
                return;

            COM001_300_DELETE popupDelete = new COM001_300_DELETE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupDelete.Name.ToString()) == false)
                return;

            object[] parameters = new object[7];

            if (appointment == null)
            {
                // 버튼 Click
                parameters[0] = Util.NVC(cboArea_R.SelectedValue);
                parameters[1] = Util.NVC(cboEquipmentSegment_R.SelectedValue);
                parameters[2] = Util.NVC(cboProcess_R.SelectedValue);
                parameters[3] = null;
                parameters[4] = null;
                parameters[5] = DateTime.Now;
                parameters[6] = DateTime.Now;
            }
            else
            {
                // 스케쥴 Click
                String[] Para = appointment.Tag.ToString().Split(',');

                DateTime dtStart = appointment.Start.Date;
                DateTime dtEnd = appointment.End.Date.AddDays(-1);

                parameters[0] = Para[0];           // AREAID
                parameters[1] = Para[1];           // EQSGID
                parameters[2] = Para[2];           // PROCID
                parameters[3] = Para[3];           // SHFT_ID
                parameters[4] = Para[4];           // WRK_GR_ID
                parameters[5] = dtStart;           // 시작일자
                parameters[6] = dtEnd;             // 종료일자
            }
            C1WindowExtension.SetParameters(popupDelete, parameters);

            popupDelete.Closed += new EventHandler(popupDelete_Closed);
            grdMain.Children.Add(popupDelete);
            popupDelete.BringToFront();
        }

        private void popupDelete_Closed(object sender, EventArgs e)
        {
            COM001_300_DELETE popup = sender as COM001_300_DELETE;
            if (popup != null && popup.Save == true)
            {
                //다시 월력 조회
                dtpMonth_R.SelectedDateTime = new DateTime(int.Parse(popup.YearMonth.Substring(0, 4)), int.Parse(popup.YearMonth.Substring(5, 2)), 1);
                SetDisplayScheduler();
                SetScheduleInfomation();
            }
            grdMain.Children.Remove(popup);

            Scheduler.SelectedAppointment = null;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnRegister,
                btnDelete
            };

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

        /// <summary>
        /// Scheduler 화면표시 일자 설정
        /// </summary>
        private void SetDisplayScheduler()
        {
            try
            {
                Scheduler.VisibleDates.BeginUpdate();
                Scheduler.VisibleDates.Clear();
                DateTime firstOfNextMonth = new DateTime(dtpMonth_R.SelectedDateTime.Year, dtpMonth_R.SelectedDateTime.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                for (int i = 0; i < lastOfThisMonth.Day; i++)
                {
                    Scheduler.VisibleDates.Insert(i, new DateTime(dtpMonth_R.SelectedDateTime.Year, dtpMonth_R.SelectedDateTime.Month, i + 1));
                }
                Scheduler.VisibleDates.EndUpdate();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TabMove_Schedule(DataRowView dr)
        {
            try
            {
                string Area = Util.NVC(dr["AREAID"]);
                string EquipmentSegment = Util.NVC(dr["EQSGID"]);
                string Process = Util.NVC(dr["PROCID"]);
                string YM = Util.NVC(dr["YM"]);

                cboArea_R.SelectedValue = Area;
                cboEquipmentSegment_R.SelectedValue = EquipmentSegment;
                cboProcess_R.SelectedValue = Process;

                int Year = int.Parse(YM.Substring(0, 4));
                int Month = int.Parse(YM.Substring(5, 2));
                DateTime SelectMonth = new DateTime(Year, Month, 1);

                dtpMonth_R.SelectedDateTime = SelectMonth;

                // 탭 전환
                tabCalendar.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }





        #endregion

        #endregion

    }
}