/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : 월력관리 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.06.11  손우석  CSR ID 3715740 180618_GMES 작업자 정보관리 기능추가 요청건 [요청번호 C20180618_15740]
  2018.07.17  손우석  CSR ID 3758429 180711_GMES 작업자 정보 등록 기능 개선요청 [요청번호 C20180806_58429]
  2019.12.11  손우석  CSR ID 6274 라인사용률 Key-in 기능 추가 件 [요청번호 C20191120-000031]
  2020.04.30  김길용
  2020.05.04  손우석 라인별사용률 동 선택 변경시 이벤트 추가, 저장시 체크한 항목이 없는 경우 메시지 추가, 
  2020.05.28  손우석 서비스 번호 65285 라인사용비율 수정 시 에러 수정 요청 [요청번호] C20200528-000305
  2020.06.04  김준겸 [라인 사용 비율 관리 TAP] Site,동,라인 매핑 오류 수정
  2020.06.09  김준겸 서비스 번호 65285 라인사용비율 수정 시 에러 수정 요청 [요청번호] C20200528-000305
  2020.07.09  김준겸 라인사용비율 사용 비율 추가   
**************************************************************************************/

using C1.C1Schedule;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using LGC.GMES.MES.CMM001.Class;
//2020.05.28
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_033 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 

        /// <summary>
        /// 스케줄러 표시되는 월 임시저장용 변수
        /// </summary>
        private DateTime datetimeTemp = DateTime.Today;
        private string sStartTime = string.Empty;
        private string sEndTime = string.Empty;
        //2019.12.11
        private int nRate = 0;
        
        public COM001_033()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentLeft.RowDefinitions[2].Height = new GridLength(175);
                dgInput.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                dgInput.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                dgInput.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                dgInput.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                dgInput.RowDefinitions[5].Height = new GridLength(0);
                dgInput.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

                ContentLeftRate.RowDefinitions[2].Height = new GridLength(175);
                dgInputRate.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                dgInputRate.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                dgInputRate.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                dgInputRate.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                dgInputRate.RowDefinitions[5].Height = new GridLength(0);
                dgInputRate.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

                DataTable dtDGTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);
                DataTable dtTemp = new DataTable();
                dtTemp.Columns.Add("CBO_CODE", typeof(string));
                dtTemp.Columns.Add("CBO_NAME", typeof(string));
                dtTemp.Columns.Add("CBO_DESC", typeof(string));
                dtTemp.Columns.Add("SHFT_STRT_HMS", typeof(string));
                dtTemp.Columns.Add("SHFT_END_HMS", typeof(string));
                
                dgTimeList.ItemsSource = DataTableConverter.Convert(dtTemp);

                DataTable dtDGTimeListRate = DataTableConverter.Convert(dgTimeListRate.ItemsSource);
                DataTable dtTempRate = new DataTable();
                dtTempRate.Columns.Add("CBO_CODE", typeof(string));
                dtTempRate.Columns.Add("CBO_NAME", typeof(string));
                dtTempRate.Columns.Add("CBO_DESC", typeof(string));
                dtTempRate.Columns.Add("SHFT_STRT_HMS", typeof(string));
                dtTempRate.Columns.Add("SHFT_END_HMS", typeof(string));

                dgTimeListRate.ItemsSource = DataTableConverter.Convert(dtTempRate);

                InitCombo();
                getWorkCalendarList();

                //월력의 language 환경설정의 language로 변경.
                Scheduler.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

                btnWorker.Visibility = Visibility.Collapsed; // 2024.12.10. 김영국 - 해당 버튼 Hidden 처리.
                ScheduleWorkerTab.Visibility = Visibility.Collapsed; // 2024.12.10. 김영국 - 해당 Tab Hidden 처리.
                ManagementLineUseRateTab.Visibility = Visibility.Collapsed; // 2024.12.10. 김영국 - 해당 Tab Hidden 처리.
                btnShftTimeChage.Visibility = Visibility.Collapsed;  // 2024.12.10. 김영국 - 해당 버튼 Hidden 처리.

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region Event - Scheduler
        /// <summary>
        /// Scheduler 더블클릭시 
        /// open되는 기본 입력팝업 무시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_UserAddingAppointment(object sender, C1.WPF.Schedule.AddingAppointmentEventArgs e)
        {
            try
            {
                //e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// Scheduler 더블클릭시
        /// open되는 기본 입력팝업 무시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Scheduler 스타일 변경무시.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_StyleChanged(object sender, RoutedEventArgs e)
        {
            
            try
            {
                Scheduler.Style = Scheduler.MonthStyle;
                setDisplayScheduler();
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// drop 무시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_BeforeAppointmentDrop(object sender, C1.WPF.Schedule.AppointmentActionEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// 일정선택시 삭제 대기 정보에 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_SelectedAppointmentChanged(object sender, C1.WPF.Schedule.PropertyChangeEventArgsBase e)
        {
            
            try
            {
                C1.C1Schedule.Appointment appointment = Scheduler.SelectedAppointment;
                if (appointment != null)
                {
                    dtpDelWorkStartDay.SelectedDateTime = appointment.Start;
                    dtpDelWorkEndDay.SelectedDateTime = appointment.End;
                    //cboDelShift.SelectedIndex = appointment.Subject;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Scheduler_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            e.Handled = true;
            
            //dtpDelWorkEndDay.SelectedDateTime = Scheduler.SelectedDateTime;
        }

        private void Scheduler_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //dtpDelWorkStartDay.SelectedDateTime = e.
        }

        private void Scheduler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Scheduler_KeyDown(object sender, KeyEventArgs e)
        {
            //key입력으로 작업조 이름을 변경하지못하도록 함.
            e.Handled = true;
        }
        #endregion Event - Scheduler

        #region Event - Button
        private void btnPrevMonth_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                dtpMoth.SelectedDateTime = (dtpMoth.SelectedDateTime).AddMonths(-1);
                //datetimeTemp = datetimeTemp.AddMonths(-1);
                setDisplayScheduler();
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnNextMonth_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                dtpMoth.SelectedDateTime = (dtpMoth.SelectedDateTime).AddMonths(+1);
                //datetimeTemp = datetimeTemp.AddMonths(+1);
                setDisplayScheduler();
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnToDay_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                dtpMoth.SelectedDateTime = DateTime.Today;
                //datetimeTemp = DateTime.Today;
                setDisplayScheduler();
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkSave())
                {
                    //cboWorkEquipmentSegment.SelectedValuePath = "CBO_NAME";
                    //string sSelectedLineName = cboWorkEquipmentSegment.SelectedItemsToString;
                    //cboWorkEquipmentSegment.SelectedValuePath = "CBO_CODE";

                    string sSelectedLineName = Util.NVC(cboWorkEquipmentSegment.SelectedValue);

                    cboShift.SelectedValuePath = "CBO_NAME";
                    string sSelectedShiftName = cboShift.SelectedItemsToString;
                    cboShift.SelectedValuePath = "CBO_CODE";

                    if (chkSaveEqsg_Exist())
                    {
                        //이미 등록된 정보가 존재합니다.!\r\n선택한 정보로 변경하시겠습니까?\r\n[LINE: {0}]\r\n[작업조: {1}]\r\n[월력시작일: {2}]\r\n[월력종료일: {3}]
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2927", new object[] { sSelectedLineName, sSelectedShiftName, dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"), dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                //저장
                                saveScheduledata();

                                //조회
                                getScheduleInfomation();
                            }
                        });
                    }
                    else
                    {
                        //선택한 정보로 등록하시겠습니까?\r\n[LINE: {0}]\r\n[작업조: {1}]\r\n[월력시작일: {2}]\r\n[월력종료일: {3}]
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1667", new object[] { sSelectedLineName, sSelectedShiftName, dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"), dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                //저장
                                saveScheduledata();

                                //조회
                                getScheduleInfomation();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkDelete())
                {
                    //2020.05.28
                    if (chkRelation())
                    {
                        cboDelWorkEquipmentSegment.SelectedValuePath = "CBO_NAME";
                        string sSelectedLineName = cboDelWorkEquipmentSegment.SelectedItemsToString;
                        cboDelWorkEquipmentSegment.SelectedValuePath = "CBO_CODE";

                        //cboDelShift.SelectedValuePath = "CBO_NAME";
                        //string sSelectedDelShiftName = cboDelShift.SelectedItemsToString;
                        //cboDelShift.SelectedValuePath = "CBO_CODE";

                        //선택한 정보로 삭제하시겠습니까?\r\n[LINE: {0}]\r\n[작업조: {1}]\r\n[월력시작일: {2}]\r\n[월력종료일: {3}]
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1668", new object[] { sSelectedLineName, sSelectedDelShiftName, dtpDelWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"), dtpDelWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1668", new object[] { sSelectedLineName, "ALL", dtpDelWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"), dtpDelWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                            //삭제
                            delScheduledata();

                            //조회
                            getScheduleInfomation();
                            }
                        });
                    }
                }
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
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchtab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getWorkCalendarList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnShftTimeChage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContentLeft.RowDefinitions[2].Height.Value == 175)
                {
                    ContentLeft.RowDefinitions[2].Height = new GridLength(280);

                    dgInput.RowDefinitions[1].Height = new GridLength(31);
                    dgInput.RowDefinitions[2].Height = new GridLength(31);
                    dgInput.RowDefinitions[3].Height = new GridLength(31);
                    dgInput.RowDefinitions[4].Height = new GridLength(31);
                    dgInput.RowDefinitions[5].Height = new GridLength(1, GridUnitType.Star);
                    dgInput.RowDefinitions[6].Height = new GridLength(31);
                }
                else
                {
                    ContentLeft.RowDefinitions[2].Height = new GridLength(175);

                    dgInput.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                    dgInput.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                    dgInput.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                    dgInput.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                    dgInput.RowDefinitions[5].Height = new GridLength(0);
                    dgInput.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);
                }

                //dgInput.RowDefinitions[5].Height = new GridLength(0, GridUnitType.Star);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.06.11
        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                COM001_033_WORKER popup = new COM001_033_WORKER();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = cboShop.SelectedValue.ToString();
                    Parameters[1] = cboWorkAREA.SelectedValue.ToString();
                    Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();

                    Parameters[3] = dtpWorkStartDay.SelectedDateTime.ToString();
                    Parameters[4] = dtpWorkEndDay.SelectedDateTime.ToString();

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWorkerSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(SHOP.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    SHOP.Focus();
                    return;
                }

                if (Util.NVC(cboAreaByAreaType.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    cboAreaByAreaType.Focus();
                    return;
                }

                if (Util.NVC(EQUIPMENTSEGMENT.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    EQUIPMENTSEGMENT.Focus();
                    return;
                }

                getScheduleWorkerInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
}

        //2018.08.07
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWorkerList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.12.11
        private void btnRateSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgEquipmentRate == null)
                {
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable INDATA = new DataTable();
                        INDATA.TableName = "INDATA";
                        INDATA.Columns.Add("CALDATE", typeof(string));

                        INDATA.Columns.Add("SHOPID", typeof(string));
                        INDATA.Columns.Add("AREAID", typeof(string));
                        INDATA.Columns.Add("EQSGID", typeof(string));
                        INDATA.Columns.Add("SHFT_ID", typeof(string));
                        INDATA.Columns.Add("EQSG_USE_RATE", typeof(string));
                        INDATA.Columns.Add("UPDUSER", typeof(string));

                        DataTable dtResult = DataTableConverter.Convert(dgEquipmentRate.ItemsSource);

                        for (int i = 0; i < dgEquipmentRate.GetRowCount(); i++)
                        {
                            // MES 2.0 CHK 컬럼 Bool 오류 Patch
                            if (DataTableConverter.GetValue(dgEquipmentRate.Rows[i].DataItem, "CHK").Equals(true) || 
                                DataTableConverter.GetValue(dgEquipmentRate.Rows[i].DataItem, "CHK").ToString() == "1" ||
                                DataTableConverter.GetValue(dgEquipmentRate.Rows[i].DataItem, "CHK").ToString() == "True")
                            {
                                DataRow drv = dtResult.Rows[i] as DataRow;

                                DataRow drINDATA = INDATA.NewRow();
                                drINDATA["CALDATE"] = Convert.ToDateTime(drv["CALDATE"]).ToString("yyyy/MM/dd 00:00:00");
                                drINDATA["SHOPID"] = drv["SHOPID"].ToString();
                                drINDATA["AREAID"] = drv["AREAID"].ToString();
                                drINDATA["EQSGID"] = drv["EQSGID"].ToString();
                                drINDATA["SHFT_ID"] = drv["SHFT_ID"].ToString();
                                drINDATA["EQSG_USE_RATE"] = drv["EQSG_USE_RATE"];
                                drINDATA["UPDUSER"] = LoginInfo.USERID;

                                INDATA.Rows.Add(drINDATA);
                            }
                        }

                        if (INDATA.Rows.Count > 0)
                        {
                            DataTable dtSave = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE", "RQSTDT", "RSLTDT", INDATA);

                            Util.AlertInfo("SFU1270");  //저장되었습니다.

                            RateSearch();
                        }
                        else
                        {
                            Util.AlertInfo("SFU3538");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirmRate_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_REG_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE
            try
            {
                // MES 2.0 오류 Patch - Validation 추가 (남도우 책임님 요청)
                if (Util.NVC(cboWorkShopRate.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    cboWorkShopRate.Focus();
                    return ;
                }

                if (Util.NVC(cboWorkAREARate.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    cboWorkAREARate.Focus();
                    return;
                }

                if (Util.NVC(cboWorkEquipmentSegmentRate.SelectedValue) == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    cboWorkEquipmentSegmentRate.Focus();
                    return;
                }

                if (cboShiftRate.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1844");  //작업조를 선택하세요.
                    cboShiftRate.Focus();
                    return;
                }

                string sShop = Util.NVC(cboWorkShopRate.SelectedValue);
                string sArea = Util.NVC(cboWorkAREARate.SelectedValue);
                //int iGapDays = Util.DateTimeGap(dtpWorkStartDay.SelectedDateTime, dtpWorkEndDay.SelectedDateTime);

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE");
                INDATA.Columns.Add("LANGID");
                INDATA.Columns.Add("SHOPID");
                INDATA.Columns.Add("AREAID");
                INDATA.Columns.Add("EQSGID");
                INDATA.Columns.Add("FROMDATE");
                INDATA.Columns.Add("TODATE");
                INDATA.Columns.Add("SHFT_ID");
                INDATA.Columns.Add("USERID");
                INDATA.Columns.Add("USAGERATIO");

                DataRow drINDATA = null;
                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SHOPID"] = sShop;
                drINDATA["AREAID"] = sArea;
                drINDATA["EQSGID"] = cboWorkEquipmentSegmentRate.SelectedValue;
                drINDATA["FROMDATE"] = dtpWorkStartDayRate.SelectedDateTime.ToString("yyyyMMdd");
                drINDATA["TODATE"] = dtpWorkEndDayRate.SelectedDateTime.ToString("yyyyMMdd");
                drINDATA["SHFT_ID"] = cboShiftRate.SelectedItemsToString;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["USAGERATIO"] = Convert.ToInt32(numUsageRatio.Value);
                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE", "INDATA", "", INDATA, null);

                Util.AlertInfo("SFU1270");  //저장되었습니다.

                RateSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RateSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.05.28
        private void btnDeleteRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkRateDelete())
                {
                    cboDelWorkEquipmentSegmentRate.SelectedValuePath = "CBO_NAME";
                    string sSelectedLineName = cboDelWorkEquipmentSegmentRate.SelectedItemsToString;
                    cboDelWorkEquipmentSegmentRate.SelectedValuePath = "CBO_CODE";

                    //선택한 정보로 삭제하시겠습니까?\r\n[LINE: {0}]\r\n[작업조: {1}]\r\n[월력시작일: {2}]\r\n[월력종료일: {3}]
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1668", new object[] { sSelectedLineName, sSelectedDelShiftName, dtpDelWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"), dtpDelWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1668", new object[] { sSelectedLineName, "ALL", dtpDelWorkStartDayRate.SelectedDateTime.ToString("yyyy-MM-dd"), dtpDelWorkEndDayRate.SelectedDateTime.ToString("yyyy-MM-dd") }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //삭제
                            delRateedata();

                            //조회
                            RateSearch();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Event - Button

        #region Event - ComboBox
        private void cboDelShift_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboWorkAREA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (tabControlMain.SelectedIndex == 1)
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (cboWorkEquipmentSegment != null)
                        {
                            //SetCboEQSG(cboWorkEquipmentSegment, Util.NVC(cboWorkAREA.SelectedValue));
                            //cboWorkEquipmentSegment.isAllUsed = true;
                            //SetCboShift(cboShift, Util.NVC(cboWorkShop.SelectedValue), Util.NVC(cboWorkAREA.SelectedValue), Util.NVC(cboWorkEquipmentSegment.SelectedValue));
                            //cboShift.isAllUsed = false;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboDelWorkArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (tabControlMain.SelectedIndex == 1)
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (cboDelWorkEquipmentSegment != null)
                        {
                            SetCboEQSG(cboDelWorkEquipmentSegment, Util.NVC(cboDelWorkArea.SelectedValue));
                            cboDelWorkEquipmentSegment.isAllUsed = true;
                            //SetCboShift(cboDelShift, Util.NVC(cboDelWorkShop.SelectedValue), Util.NVC(cboDelWorkArea.SelectedValue), null);
                            //cboDelShift.isAllUsed = false;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboWorkEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetCboShift(cboShift, Util.NVC(cboWorkShop.SelectedValue), Util.NVC(cboWorkAREA.SelectedValue), Util.NVC(cboWorkEquipmentSegment.SelectedValue));
                cboShift.isAllUsed = false;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.MessageException(ex);
            }
        }

        private void cboShift_SelectionChanged(object sender, EventArgs e)
        {
            
            try
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTimeList.ItemsSource);
                DataTable dtResult = dtBefore.Clone();


                System.Collections.Generic.IList<object> list = cboShift.SelectedItems;

                DataTable dtShift = DataTableConverter.Convert(cboShift.ItemsSource);


                for (int i = 0; list.Count > i; i++)
                {
                    DataRow[] drBefore = dtBefore.Select("CBO_CODE = '" + list[i].ToString() + "'");
                    if (!(drBefore.Length > 0))
                    {
                        DataRow[] dr = dtShift.Select("CBO_CODE = '" + list[i].ToString() + "'");
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtResult.Rows.Add(dr[0].ItemArray);
                        dtResult.AddDataRow(dr[0]);
                    }
                }

                dtBefore.Merge(dtResult);
                dgTimeList.ItemsSource = DataTableConverter.Convert(dtBefore);

                dtBefore = DataTableConverter.Convert(dgTimeList.ItemsSource);


                for (int i = dtBefore.Rows.Count - 1; 0 <= i; i--)
                {
                    string sTemp = Util.NVC(dtBefore.Rows[i]["CBO_CODE"]); // Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["CBO_CODE"].Index).Value);
                    bool bCheck = true;

                    for (int iIdx = 0; list.Count > iIdx; iIdx++)
                    {
                        if (sTemp == list[iIdx].ToString())
                        {
                            bCheck = false;
                            break;
                        }
                    }
                    if (bCheck)
                    {
                        dtBefore.Rows.Remove(dtBefore.Rows[i]);
                        //dgTimeList.EndNewRow(true);
                        //dgTimeList.RemoveRow(i);
                        //dgTimeList.Refresh();
                    }
                }

                dgTimeList.ItemsSource = DataTableConverter.Convert(dtBefore);

                /*
                for (int i = dgTimeList.Rows.Count - 1; 0 <= i; i--)
                {
                    string sTemp = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["CBO_CODE"].Index).Value);
                    bool bCheck = true;

                    for (int iIdx = 0; list.Count > iIdx; iIdx++)
                    {
                        if (sTemp == list[iIdx].ToString())
                        {
                            bCheck = false;
                            break;
                        }
                    }
                    if (bCheck)
                    {

                        //dgTimeList.EndNewRow(true);
                        dgTimeList.RemoveRow(i);
                        dgTimeList.Refresh();
                    }
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
           
        }

        //2018.06.11
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                Set_Combo_Area(cboAreaByAreaType);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                Set_Combo_EquipmentSegmant(EQUIPMENTSEGMENT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.12.11
        private void cboShiftRate_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTimeListRate.ItemsSource);
                DataTable dtResult = dtBefore.Clone();


                System.Collections.Generic.IList<object> list = cboShiftRate.SelectedItems;

                DataTable dtShift = DataTableConverter.Convert(cboShiftRate.ItemsSource);


                for (int i = 0; list.Count > i; i++)
                {
                    DataRow[] drBefore = dtBefore.Select("CBO_CODE = '" + list[i].ToString() + "'");
                    if (!(drBefore.Length > 0))
                    {
                        DataRow[] dr = dtShift.Select("CBO_CODE = '" + list[i].ToString() + "'");
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtResult.Rows.Add(dr[0].ItemArray);
                        dtResult.AddDataRow(dr[0]);
                    }
                }

                dtBefore.Merge(dtResult);
                dgTimeListRate.ItemsSource = DataTableConverter.Convert(dtBefore);

                dtBefore = DataTableConverter.Convert(dgTimeListRate.ItemsSource);


                for (int i = dtBefore.Rows.Count - 1; 0 <= i; i--)
                {
                    string sTemp = Util.NVC(dtBefore.Rows[i]["CBO_CODE"]); // Util.NVC(dgTimeListRate.GetCell(i, dgTimeListRate.Columns["CBO_CODE"].Index).Value);
                    bool bCheck = true;

                    for (int iIdx = 0; list.Count > iIdx; iIdx++)
                    {
                        if (sTemp == list[iIdx].ToString())
                        {
                            bCheck = false;
                            break;
                        }
                    }
                    if (bCheck)
                    {
                        dtBefore.Rows.Remove(dtBefore.Rows[i]);
                    }
                }

                dgTimeListRate.ItemsSource = DataTableConverter.Convert(dtBefore);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboWorkAREARate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (tabControlMain.SelectedIndex == 3)
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (cboWorkEquipmentSegmentRate != null)
                        {

                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboWorkEquipmentSegmentRate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetCboShift(cboShiftRate, Util.NVC(cboWorkShopRate.SelectedValue), Util.NVC(cboWorkAREARate.SelectedValue), Util.NVC(cboWorkEquipmentSegmentRate.SelectedValue));
                cboShiftRate.isAllUsed = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAreaRate_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                Set_Combo_EquipmentSegmantRate(cboEquipmentSegmentRate);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.05.28
        private void cboDelWorkAreaRate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (tabControlMain.SelectedIndex == 3)
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (cboDelWorkEquipmentSegmentRate != null)
                        {
                            SetCboEQSG(cboDelWorkEquipmentSegmentRate, Util.NVC(cboDelWorkAreaRate.SelectedValue));
                            cboDelWorkEquipmentSegmentRate.isAllUsed = true;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Event - ComboBox

        #region Event - DataGrid
        private void dgWorkCalendarList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
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
                        case "RECORDCOUNT":
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
                            
                        //case "RECORDFLAG":
                        //    if (e.Cell.Text == "N")
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //    }
                        //    else if (e.Cell.Text == "Y")
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //    }
                        //    else
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //    }

                        //    break;

                        //case "SHIFTFLAG":
                        //    if (e.Cell.Text == "N")
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //    }
                        //    else if (e.Cell.Text == "Y")
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //    }
                        //    else
                        //    {
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //    }
                        //    break;

                        default:
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            break;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkCalendarList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWorkCalendarList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "EQSGNAME")
                    {
                        DataRowView dr = dgWorkCalendarList.Rows[cell.Row.Index].DataItem as DataRowView;
                        TabMove_Schedule(dr,false);
                    }
                    else if (cell.Column.Name == "YEARMONTH"
                        || cell.Column.Name == "TOTALCOUNT"
                        || cell.Column.Name == "RECORDCOUNT"
                        )
                    {
                        DataRowView dr = dgWorkCalendarList.Rows[cell.Row.Index].DataItem as DataRowView;
                        TabMove_Schedule(dr,true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (tabControlMain.SelectedIndex == 0)
                {
                    getWorkCalendarList();
                }
                else if (tabControlMain.SelectedIndex == 1)
                {
                    cboWorkAREA_SelectedValueChanged(null, null);
                    cboDelWorkArea_SelectedValueChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpMoth_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                setDisplayScheduler();
                getScheduleInfomation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTimeList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                //if(dgTimeList.Rows.Count == 1)
                //{
                //    return;
                //}
                


                //DateTime dEditValue = DateTime.Parse(e.Cell.Value.ToString());

                //int iEditIndex_Column = e.Cell.Column.Index;
                //int iEditIndex_Row = e.Cell.Row.Index;

                //string sEditColumnName = e.Cell.Column.Name;
                //string sChkTagetName = sEditColumnName == "SHFT_STRT_HMS" ? "SHFT_END_HMS" : "SHFT_STRT_HMS";

                //DataTable dtTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);

                //C1.WPF.DataGrid.DataGridCell cell = e.Cell;                
                //string sShft =Util.NVC( DataTableConverter.GetValue(dgTimeList.Rows[e.Cell.Row.Index].DataItem, "CBO_CODE"));

                ////DataTable dtBefore = DataTableConverter.Convert(dgTimeList.ItemsSource);
                //DataRow[] drBefore = dtBeForeShift.Select("CBO_CODE = '"+ sShft + "'");
                //string sBeforeValue = Util.NVC(drBefore[0][sEditColumnName]);


                //if (sEditColumnName == "SHFT_STRT_HMS")
                //{
                //    if (iEditIndex_Row == 0)
                //    {
                //        string sTempTaget = dtTimeList.Rows[dtTimeList.Rows.Count - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //}
                //else
                //{
                //    if (iEditIndex_Row == dtTimeList.Rows.Count - 1)
                //    {
                //        string sTempTaget = dtTimeList.Rows[0][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row + 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.12.11
        private void dgEquipmentRate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgEquipmentRate.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "EQSG_USE_RATE")
                    {
                        //this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentRate_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //2020.05.04
                    //switch (e.Cell.Column.Name)
                    //{
                    //    case "EQSG_USE_RATE":
                    //        nRate = Int32.Parse(Util.NVC(e.Cell.Text));
                    //        //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //        //e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //        break;

                    //    default:
                    //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    //        break;
                    //}
                    if (e.Cell.Column.Name == "EQSG_USE_RATE")
                    {
                        nRate = Int32.Parse(Util.NVC(e.Cell.Text));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipmentRate_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                setRate_Display(e.Cell);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.05.28
        private void dgEquipmentRate_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null || e.Cell == null)
                    return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //C1.WPF.DataGrid.DataGridCell dgCELL = dgEquipmentRate.GetCell(e.Cell.Row.Index, dgEquipmentRate.Columns["EQSG_USE_RATE"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (e.Cell.Column.Name.Equals("EQSG_USE_RATE"))
                    {
                        e.Cell.Column.IsReadOnly = false;
                        //setRate_Display(dgEquipmentRate.GetCell(e.Cell.Row.Index, dgEquipmentRate.Columns["EQSG_USE_RATE"].Index) as C1.WPF.DataGrid.DataGridCell);
                    }
                    else
                    {
                        e.Cell.Column.IsReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Event - DataGrid

        #endregion Event

        #region Mehod
        private void InitCombo()
        {
            try
            {
                dtpMoth.SelectedDateTime = DateTime.Today;
                dtpMoth.IsNullInitValue = true;
                dtpWorkStartDay.SelectedDateTime = DateTime.Today;
                dtpWorkStartDay.IsNullInitValue = true;
                dtpWorkEndDay.SelectedDateTime = DateTime.Today;
                dtpWorkEndDay.IsNullInitValue = true;
                dtpDelWorkStartDay.SelectedDateTime = DateTime.Today;
                dtpDelWorkStartDay.IsNullInitValue = true;
                dtpDelWorkEndDay.SelectedDateTime = DateTime.Today;
                dtpDelWorkEndDay.IsNullInitValue = true;

                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { Area_Type.PACK };

                //------------------------------------------------------ 월력 등록 -------------------------------------------------------------------
                //shop
                C1ComboBox[] cboShopChild = { cboArea };
                //2020.06.04
                //_combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter ,sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                //동
                C1ComboBox[] cboAreaParent = { cboShop };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, cbParent: cboAreaParent, sFilter: sFilter ,sCase: "AREA_AREATYPE");

                //라인
                C1ComboBox[] cboLineParent = { cboArea };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);

                //shop
                C1ComboBox[] cboWorkShopChild = { cboWorkAREA };
                //_combo.SetCombo(cboWorkShop, CommonCombo.ComboStatus.SELECT, cbChild: cboWorkShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboWorkShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                //동
                C1ComboBox[] cboWorkAREAParent = { cboWorkShop };
                C1ComboBox[] cboWorkAREAChild = { cboWorkEquipmentSegment };
                _combo.SetCombo(cboWorkAREA, CommonCombo.ComboStatus.SELECT, cbParent: cboWorkAREAParent , cbChild: cboWorkAREAChild, sFilter: sFilter, sCase: "AREA_AREATYPE");

                //라인
                C1ComboBox[] cboWorkLineParent = { cboWorkAREA };
                _combo.SetCombo(cboWorkEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboWorkLineParent ,sCase : "cboEquipmentSegment");

                //작업조
                //C1ComboBox cboTempShop = new C1ComboBox();
                //cboTempShop.SelectedValue = LoginInfo.CFG_SHOP_ID;
                //C1ComboBox[] cboShiftParent = { cboTempShop , cboWorkAREA};
                //_combo.SetCombo(cboShift, CommonCombo.ComboStatus.NONE, cbParent: cboShiftParent, sCase: "SHIFT_AREA");

                //shop
                C1ComboBox[] cboDelWorkShopChild = { cboArea };
                //_combo.SetCombo(cboDelWorkShop, CommonCombo.ComboStatus.SELECT, cbChild: cboDelWorkShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboDelWorkShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                //동
                C1ComboBox[] cboDelWorkAreaParent = { cboDelWorkShop };
                //C1ComboBox[] cboDelWorkAREAChild = { cboDelShift };
                _combo.SetCombo(cboDelWorkArea, CommonCombo.ComboStatus.SELECT, cbParent: cboDelWorkAreaParent, sFilter: sFilter, sCase: "AREA_AREATYPE");

                //C1ComboBox[] cboDelShiftParent = { cboTempShop, cboDelWorkArea};
                //_combo.SetCombo(cboDelShift, CommonCombo.ComboStatus.NONE, cbParent: cboDelShiftParent, sCase: "SHIFT_AREA");

                //------------------------------------------------------ 월력 등록 현황 -------------------------------------------------------------------
                dtpSearchMoth_start.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(-3);
                dtpSearchMoth_end.SelectedDateTime = (DateTime)System.DateTime.Now.AddMonths(3);

                //shop
                C1ComboBox[] cboSearchShopChild = { cboSearchArea };
                //2020.06.04
                //_combo.SetCombo(cboSearchShop, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboSearchShop, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchShopChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                //동
                C1ComboBox[] cboSearchAreaParent = { cboSearchShop };
                C1ComboBox[] cboSearchAreaChild = { cboSearchEquipmentSegment };
                _combo.SetCombo(cboSearchArea, CommonCombo.ComboStatus.ALL, cbChild: cboSearchAreaChild, cbParent: cboSearchAreaParent, sFilter: sFilter, sCase: "AREA_AREATYPE");

                //라인
                C1ComboBox[] cboSearchEquipmentSegmentParent = { cboSearchArea };
                _combo.SetCombo(cboSearchEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboSearchEquipmentSegmentParent , sCase: "cboEquipmentSegment");

                cboSearchArea.SelectedIndex = 0;
                //cboSearchEquipmentSegment.SelectedIndex = 0;

                //2018.06.11
                //작업자 현황 탭
                Set_Combo_Shop(SHOP);
                Set_Combo_Area(cboAreaByAreaType);
                Set_Combo_EquipmentSegmant(EQUIPMENTSEGMENT);

                //------------------------------------------------------ 라인사용비율관리 -------------------------------------------------------------------
                //2019.12.11
                //조회
                C1ComboBox[] cboShopRateChild = { cboAreaRate };
                //2020.06.04
                //_combo.SetCombo(cboShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                //2020.06.04
                //_combo.SetCombo(cboShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboShopRateChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboShopRateChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                C1ComboBox[] cboAreaRateParent = { cboShopRate };
                C1ComboBox[] cboAreaRateChild = { cboEquipmentSegmentRate };
                //2020.06.04
                //_combo.SetCombo(cboAreaRate, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, cbParent: cboAreaParent, sFilter: sFilter, sCase: "AREA_AREATYPE");
                _combo.SetCombo(cboAreaRate, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaRateChild, cbParent: cboAreaRateParent, sFilter: sFilter, sCase: "AREA_AREATYPE");

                C1ComboBox[] cboLineRateParent = { cboAreaRate };
                //2020.06.04
                _combo.SetCombo(cboEquipmentSegmentRate, CommonCombo.ComboStatus.NONE, cbParent: cboLineRateParent, sFilter: sFilter, sCase: "cboEquipmentSegment");
                _combo.SetCombo(cboEquipmentSegmentRate, CommonCombo.ComboStatus.SELECT, cbParent: cboLineRateParent, sFilter: sFilter, sCase: "cboEquipmentSegment");

                //등록
                C1ComboBox[] cboWorkShopRateChild = { cboWorkAREARate };
                //2020.06.04
                //_combo.SetCombo(cboWorkShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboWorkShopRateChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboWorkShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboWorkShopRateChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                C1ComboBox[] cboWorkAREARateParent = { cboWorkShopRate };
                C1ComboBox[] cboWorkAREARateChild = { cboWorkEquipmentSegmentRate };
                _combo.SetCombo(cboWorkAREARate, CommonCombo.ComboStatus.SELECT, cbParent: cboWorkAREARateParent, cbChild: cboWorkAREARateChild, sFilter: sFilter, sCase: "AREA_AREATYPE");

                C1ComboBox[] cboWorkLineRateParent = { cboWorkAREARate };
                _combo.SetCombo(cboWorkEquipmentSegmentRate, CommonCombo.ComboStatus.NONE, cbParent: cboWorkLineRateParent, sCase: "cboEquipmentSegment");

                dtpMoth_Start.SelectedDateTime = DateTime.Today;
                dtpMoth_End.SelectedDateTime = DateTime.Today;

                dtpWorkStartDayRate.SelectedDateTime = DateTime.Today;
                dtpWorkEndDayRate.SelectedDateTime = DateTime.Today;

                dtpWorkStartDayRate.IsNullInitValue = true;
                dtpWorkEndDayRate.IsNullInitValue = true;

                dtpWorkStartDayRate.IsNullInitValue = true;
                dtpWorkEndDayRate.IsNullInitValue = true;

                //2020.05.28
                C1ComboBox[] cboDelWorkShopRateChild = { cboArea };
                //2020.06.04
                //_combo.SetCombo(cboDelWorkShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboDelWorkShopRateChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
                _combo.SetCombo(cboDelWorkShopRate, CommonCombo.ComboStatus.SELECT, cbChild: cboDelWorkShopRateChild, sFilter: sFilter, sCase: "SHOP_AUTH");

                C1ComboBox[] cboDelWorkAreaRateParent = { cboDelWorkShopRate };
                _combo.SetCombo(cboDelWorkAreaRate, CommonCombo.ComboStatus.SELECT, cbParent: cboDelWorkAreaRateParent, sFilter: sFilter, sCase: "AREA_AREATYPE");

                cboDelWorkAreaRate.SelectedIndex = 0;

                dtpDelWorkStartDayRate.SelectedDateTime = DateTime.Today;
                dtpDelWorkStartDayRate.IsNullInitValue = true;
                dtpDelWorkEndDayRate.SelectedDateTime = DateTime.Today;
                dtpDelWorkEndDayRate.IsNullInitValue = true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Scheduler 화면표시 일자 설정
        /// </summary>
        private void setDisplayScheduler()
        {
            try
            {
                
                Scheduler.VisibleDates.BeginUpdate();
                Scheduler.VisibleDates.Clear();
                //DateTime firstOfNextMonth = new DateTime(datetimeTemp.Year, datetimeTemp.Month, 1).AddMonths(1);
                DateTime firstOfNextMonth = new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);
                for (int i = 0; i < lastOfThisMonth.Day; i++)
                {
                    //Scheduler.VisibleDates.Insert(i, new DateTime(datetimeTemp.Year, datetimeTemp.Month, i + 1));
                    Scheduler.VisibleDates.Insert(i, new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, i + 1));
                }
                Scheduler.VisibleDates.EndUpdate();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void saveScheduledata()
        {
            try
            {
                string sShop = Util.NVC(cboWorkShop.SelectedValue);
                string sArea = Util.NVC(cboWorkAREA.SelectedValue);
                int iGapDays = Util.DateTimeGap(dtpWorkStartDay.SelectedDateTime, dtpWorkEndDay.SelectedDateTime);

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE");
                INDATA.Columns.Add("LANGID");
                INDATA.Columns.Add("SHOPID");
                INDATA.Columns.Add("AREAID");
                INDATA.Columns.Add("EQSGID");
                INDATA.Columns.Add("FROMDATE");
                INDATA.Columns.Add("TODATE");
                INDATA.Columns.Add("SHFT_ID");
                INDATA.Columns.Add("USERID");

                DataRow drINDATA = null;
                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SHOPID"] = sShop;
                drINDATA["AREAID"] = sArea;
                drINDATA["EQSGID"] = cboWorkEquipmentSegment.SelectedValue; //cboWorkEquipmentSegment.SelectedItemsToString;
                drINDATA["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                drINDATA["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                drINDATA["SHFT_ID"] = cboShift.SelectedItemsToString;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataTable dtTIME = new DataTable();
                dtTIME.TableName = "TIME";
                dtTIME.Columns.Add("SHFT_ID");
                dtTIME.Columns.Add("FROM_TIME");
                dtTIME.Columns.Add("TO_TIME");

                for (int i = 0; i < dgTimeList.Rows.Count ; i++)
                {
                    string sStartTime = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_STRT_HMS"].Index).Value);
                    string sEndTime = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["SHFT_END_HMS"].Index).Value);
                    DataRow drTIME = null;
                    drTIME = dtTIME.NewRow();
                    drTIME["SHFT_ID"] = Util.NVC(dgTimeList.GetCell(i, dgTimeList.Columns["CBO_CODE"].Index).Value);
                    drTIME["FROM_TIME"] = (DateTime.Parse(sStartTime)).ToString("HH:mm:ss");
                    drTIME["TO_TIME"] = (DateTime.Parse(sEndTime)).ToString("HH:mm:ss");
                    //drTIME["TO_TIME"] = (DateTime.Parse(sEndTime).AddSeconds(-1)).ToString("HH:mm:ss");
                    dtTIME.Rows.Add(drTIME);
                }
                dsInput.Tables.Add(dtTIME);
                
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TB_MMD_CALDATE_SHFT", "INDATA,TIME", "", dsInput, null);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_TB_MMD_CALDATE_SHFT", ex.Message, ex.ToString());
            }
        }

        private void delScheduledata()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROMDATE");
                RQSTDT.Columns.Add("TODATE");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("SHFT_ID");

                DataRow dr = null;
                dr = RQSTDT.NewRow();
                dr["FROMDATE"] = dtpDelWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDelWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = cboDelWorkEquipmentSegment.SelectedItemsToString;
                dr["SHFT_ID"] = null;// cboDelShift.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_BAS_DEL_TB_MMD_CALDATE_SHFT_BY_FROMTO", "RQSTDT", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable getScheduleData()
        {
            DataTable RQSTDT = new DataTable();
            DataTable dtResult = null;
            try
            {
                if (Scheduler.VisibleDates.Count > 0)
                {
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("FROMDATE", typeof(string));
                    RQSTDT.Columns.Add("TODATE", typeof(string));

                    int iSchedulerDates = Scheduler.VisibleDates.Count;
                    DateTime dateTimeStart = Scheduler.VisibleDates[0];
                    DateTime dateTimeEnd = Scheduler.VisibleDates[iSchedulerDates - 1];

                    DateTime firstOfThisMonth = new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, 1);
                    DateTime firstOfNextMonth = new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, 1).AddMonths(1);
                    DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID"] = Util.NVC(cboShop.SelectedValue);
                    dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["FROMDATE"] = firstOfThisMonth.ToString("yyyyMMdd");//dateTimeStart.ToString("yyyyMMdd");
                    dr["TODATE"] = lastOfThisMonth.ToString("yyyyMMdd");//dateTimeEnd.ToString("yyyyMMdd");
                    RQSTDT.Rows.Add(dr);

                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKCALENDAR", "RQSTDT", "RSLTDT", RQSTDT);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void getScheduleInfomation()
        {
            try
            {
                Color[] colorSet = { Color.FromRgb(226, 140, 5), Color.FromRgb(86, 170, 28), Color.FromRgb(214, 96, 109), Color.FromRgb(21, 154, 182), Color.FromRgb(149, 80, 144) };
                DataTable dtTemp = getScheduleData();
                if (dtTemp != null)
                {
                    Scheduler.DataStorage.AppointmentStorage.Appointments.Clear();
                    
                    int iColorNumber = 0;
                    for (int i = 0; i < dtTemp.Rows.Count; i++)
                    {
                        C1.C1Schedule.Label label = new C1.C1Schedule.Label();
                        
                        bool bAppCreateFlag = true;

                        //동일한 Appointments 존재시 End Time에 추가
                        for (int iApp=0; iApp < Scheduler.DataStorage.AppointmentStorage.Appointments.Count  ; iApp++)
                        {
                            bAppCreateFlag = true;
                            if (Util.NVC(Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].Tag) 
                                    == (Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "," + Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + "," + dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString())
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
                            string sSubject = Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + " " + dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString();
                            //string sSubject = Util.NVC(cboEquipmentSegment.SelectedValue)=="" 
                            //    ? Util.NVC(dtTemp.Rows[i]["EQSGNAME"]) + " " + Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + " " 
                            //    : Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + " ";

                            string sLocation = "";// dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString();

                            #region Color 설정
                            bool bSameColorFlag = true;
                            for (int iApp = 0; iApp < Scheduler.DataStorage.AppointmentStorage.Appointments.Count; iApp++)
                            {
                                if (Util.NVC(Scheduler.DataStorage.AppointmentStorage.Appointments[iApp].Tag)
                                     == (Util.NVC(dtTemp.Rows[i]["EQSGID"]) + "," + Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) + "," + dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString())
                                     )
                                {
                                    //sSubject = "";
                                    //sLocation = "";
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
                            app.Tag = Util.NVC(dtTemp.Rows[i]["EQSGID"])+","+ Util.NVC(dtTemp.Rows[i]["SHFT_NAME"]) +","+ dtTemp.Rows[i]["WRK_STRT_HMS"].ToString() + " ~ " + dtTemp.Rows[i]["WRK_END_HMS"].ToString();

                            Scheduler.DataStorage.AppointmentStorage.Appointments.Add(app);
                            
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setEqsg_inputDel()
        {
            try
            {
                cboWorkEquipmentSegment.ItemsSource = cboEquipmentSegment.ItemsSource;
                cboDelWorkEquipmentSegment.ItemsSource = cboEquipmentSegment.ItemsSource;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void SetCboEQSG(MultiSelectionBox cboMulti,string sSelectedValue)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sSelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMulti.DisplayMemberPath = "CBO_NAME";
                cboMulti.SelectedValuePath = "CBO_CODE";
                cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

                
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == Util.NVC(cboEquipmentSegment.SelectedValue))
                    {
                        cboMulti.Check(i);
                    }
                }
            }
            catch(Exception ex)
            {
                //throw ex;
            }
        }

        private void SetCboShift(MultiSelectionBox cboMulti, string sSelectedShop, string sSelectedArea, string sSelectedEqsg)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sSelectedShop;
                dr["AREAID"] = sSelectedArea;
                dr["EQSGID"] = sSelectedEqsg;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMulti.DisplayMemberPath = "CBO_DESC";
                cboMulti.SelectedValuePath = "CBO_CODE";
                
                cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        //2018.06.11
        private void Set_Combo_Shop(C1ComboBox cbo)
        {
            try
            {
                //DataTable dtRQSTDT = new DataTable();
                //dtRQSTDT.TableName = "RQSTDT";
                //dtRQSTDT.Columns.Add("LANGID", typeof(string));
                //dtRQSTDT.Columns.Add("USERID", typeof(string));
                //dtRQSTDT.Columns.Add("SYSID", typeof(string));

                //DataRow drnewrow = dtRQSTDT.NewRow();
                //drnewrow["LANGID"] = LoginInfo.LANGID;
                //drnewrow["USERID"] = LoginInfo.USERID;
                //drnewrow["SYSID"] = LGC.GMES.MES.Common.LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
                //dtRQSTDT.Rows.Add(drnewrow);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                //cbo.DisplayMemberPath = "CBO_DESC";
                //cbo.SelectedValuePath = "CBO_CODE";

                //cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                CommonCombo _combo = new CommonCombo();

                String[] sFilter = { Area_Type.PACK };
                C1ComboBox[] cboShopChild = { cboAreaByAreaType };
                _combo.SetCombo(SHOP, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        //2018.06.11
        private void Set_Combo_Area(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { Util.NVC(SHOP.SelectedValue), Area_Type.PACK };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        //2018.06.11
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { Util.NVC(cboAreaByAreaType.SelectedValue) };
                _combo.SetCombo(EQUIPMENTSEGMENT, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_EquipmentSegmantRate(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { Util.NVC(cboAreaRate.SelectedValue) };
                _combo.SetCombo(cboEquipmentSegmentRate, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "cboEquipmentSegment");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private bool chkSave()
        {
            bool bRetrun = true;
            try
            {
                if(Util.NVC(cboWorkShop.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    bRetrun = false;
                    cboWorkShop.Focus();
                    return bRetrun;
                }
                if (Util.NVC(cboWorkAREA.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    bRetrun = false;
                    cboWorkAREA.Focus();
                    return bRetrun;
                }
                //if (cboWorkEquipmentSegment.SelectedItemsToString == "")
                //{
                //    Util.Alert("SFU1223");  //라인을 선택하세요.
                //    bRetrun = false;
                //    cboWorkEquipmentSegment.Focus();
                //    return bRetrun;
                //}
                if (Util.NVC(cboWorkEquipmentSegment.SelectedValue )== "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    bRetrun = false;
                    cboWorkEquipmentSegment.Focus();
                    return bRetrun;
                }
                if (cboShift.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1844");  //작업조를 선택하세요.
                    bRetrun = false;
                    cboShift.Focus();
                    return bRetrun;
                }
                if (chkSelectedShift(cboShift))
                {
                    Util.Alert("SFU1447");     //같은작업조를선택하세요\n예)2-1조,2-2조
                    bRetrun = false;
                    cboShift.Focus();
                    return bRetrun;
                }
                if (DateTime.Parse(dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                    bRetrun = false;
                    dtpWorkStartDay.Focus();
                    return bRetrun;
                }

                if (chkTime())
                {
                    Util.Alert("SFU1694"); //시간을 잘못 조정하셨습니다.
                    bRetrun = false;
                    return bRetrun;
                }
                
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return bRetrun;

        }

        private bool chkTime()
        {
            bool bChk = false;
            try
            {
                if (dgTimeList.Rows.Count == 0)
                {
                    bChk = true;
                    return bChk;
                }
                if (dgTimeList.Rows.Count == 1)
                {
                    bChk = false;
                    return bChk;
                }
                //SHFT_STRT_HMS
                //SHFT_END_HMS
                int idx_StartColumn=dgTimeList.Columns["SHFT_STRT_HMS"].Index;
                int idx_EndColumn = dgTimeList.Columns["SHFT_END_HMS"].Index;

                for(int i=0 ; i<dgTimeList.Rows.Count; i++)
                {
                    if(chkTime_DataTable(i, idx_StartColumn))
                    {
                        bChk = true;
                        return bChk;
                    }
                    if (chkTime_DataTable(i, idx_EndColumn))
                    {
                        bChk = true;
                        return bChk;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return bChk;
        }
        private bool chkTime_DataTable(int iChk_Idx_Row, int iChk_Idx_Column)
        {
            bool bChk = false;
            try
            {

                string sChk_ColumnName = dgTimeList.Columns[iChk_Idx_Column].Name;
                string sChkTagetName = sChk_ColumnName == "SHFT_STRT_HMS" ? "SHFT_END_HMS" : "SHFT_STRT_HMS";

                DateTime dChk_Value = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[iChk_Idx_Row].DataItem, sChk_ColumnName)));
                DataTable dtTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);
                

                if (sChk_ColumnName == "SHFT_STRT_HMS")
                {
                    if (iChk_Idx_Row == 0)
                    {
                        string sTempTaget = dtTimeList.Rows[dtTimeList.Rows.Count - 1][sChkTagetName].ToString();
                        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                        int iCheck = DateTime.Compare(dChk_Value, dTempTaget);
                        if (iCheck < 0)
                        {
                            bChk = true;
                            return bChk;
                        }
                    }
                    else
                    {
                        string sTempTaget = dtTimeList.Rows[iChk_Idx_Row - 1][sChkTagetName].ToString();
                        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                        int iCheck = DateTime.Compare(dChk_Value, dTempTaget);
                        if (iCheck < 0)
                        {
                            bChk = true;
                            return bChk;
                        }
                    }
                }
                else
                {
                    if (iChk_Idx_Row == dtTimeList.Rows.Count - 1)
                    {
                        string sTempTaget = dtTimeList.Rows[0][sChkTagetName].ToString();
                        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                        int iCheck = DateTime.Compare(dTempTaget, dChk_Value);
                        if (iCheck < 0)
                        {
                            bChk = true;
                            return bChk;
                        }
                    }
                    else
                    {
                        string sTempTaget = dtTimeList.Rows[iChk_Idx_Row + 1][sChkTagetName].ToString();
                        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                        int iCheck = DateTime.Compare(dTempTaget, dChk_Value);
                        if (iCheck < 0)
                        {
                            bChk = true;
                            return bChk;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

            return bChk;
        }

        private bool chkDelete()
        {
            bool bRetrun = true;
            try
            {
                if (Util.NVC(cboDelWorkShop.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    bRetrun = false;
                    cboDelWorkShop.Focus();
                    return bRetrun;
                }

                if (Util.NVC(cboDelWorkArea.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    bRetrun = false;
                    cboDelWorkArea.Focus();
                    return bRetrun;
                }

                if (cboDelWorkEquipmentSegment.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    bRetrun = false;
                    cboDelWorkEquipmentSegment.Focus();
                    return bRetrun;
                }

                //if (cboDelShift.SelectedItemsToString == "")
                //{
                //    Util.Alert("SFU1844");  //작업조를 선택하세요.
                //    bRetrun = false;
                //    cboDelShift.Focus();
                //    return bRetrun;
                //}

                if (DateTime.Parse(dtpDelWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpDelWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                    bRetrun = false;
                    dtpDelWorkStartDay.Focus();
                    return bRetrun;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRetrun;
        }

        private bool chkSelectedShift(MultiSelectionBox cboMulti)
        {
            bool bReturnValue = false;
            try
            {
                string[] sShiftList = cboMulti.SelectedItemsToString.Split(',');
                string sTemp_shft = "";
                int iShiftCount = 0;
                for(int i = 0; i < sShiftList.Length ; i++)
                {
                    if(sTemp_shft != Util.NVC(sShiftList[i]).Substring(0, 1))
                    {
                        iShiftCount++;
                    }
                    sTemp_shft = Util.NVC(sShiftList[i]).Substring(0, 1);
                }
                if (iShiftCount > 1)
                {
                    bReturnValue = true;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return bReturnValue;
        }

        private bool chkSaveEqsg_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable dtResult = null;

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

               


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(cboWorkShop.SelectedValue);
                dr["AREAID"] = Util.NVC(cboWorkAREA.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboWorkEquipmentSegment.SelectedValue);//cboWorkEquipmentSegment.SelectedItemsToString;
                dr["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKCALENDAR_BY_EQSGIDS", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        bReturn = true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void getWorkCalendarList()
        {
            DataTable RQSTDT = new DataTable();
            DataTable dtResult = null;
            try
            {
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MONTHFROM", typeof(string));
                RQSTDT.Columns.Add("MONTHTO", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MONTHFROM"] = dtpSearchMoth_start.SelectedDateTime.ToString("yyyyMM");
                dr["MONTHTO"] = dtpSearchMoth_end.SelectedDateTime.ToString("yyyyMM");
                dr["SHOPID"] = Util.NVC(cboSearchShop.SelectedValue);
                dr["AREAID"] = Util.NVC(cboSearchArea.SelectedValue) == "" ? null : Util.NVC(cboSearchArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboSearchEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboSearchEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKCALENDAR_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWorkCalendarList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgWorkCalendarList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TabMove_Schedule(DataRowView dr , bool bDateSelect)
        {
            try
            {
                string sSelectedShop = Util.NVC(dr["SHOPID"]);
                string sSelectedArea = Util.NVC(dr["AREAID"]);
                string sSelectedEqsg = Util.NVC(dr["EQSGID"]);
                string sYEARMONTH = Util.NVC(dr["YEARMONTH"]);

                cboShop.SelectedValue = sSelectedShop;
                cboWorkShop.SelectedValue = sSelectedShop;
                cboDelWorkShop.SelectedValue = sSelectedShop;

                cboArea.SelectedValue = sSelectedArea;
                cboWorkAREA.SelectedValue = sSelectedArea;
                cboDelWorkArea.SelectedValue = sSelectedArea;
                //2017.03.24 ==============================================
                cboWorkEquipmentSegment.SelectedValue = sSelectedEqsg;
                //2017.03.24 ==============================================
                cboEquipmentSegment.SelectedValue = sSelectedEqsg;
                
                if (bDateSelect)
                {
                    int iYear = int.Parse(sYEARMONTH.Substring(0, 4));
                    int iMonth = int.Parse(sYEARMONTH.Substring(5, 2));

                    DateTime firstOfsMonth = new DateTime(iYear, iMonth, 1);
                    
                    dtpMoth.SelectedDateTime = firstOfsMonth;
                }
                else
                {
                    dtpMoth.SelectedDateTime = DateTime.Now;
                }

                DateTime firstOfThisMonth = new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, 1);
                DateTime firstOfNextMonth = new DateTime(dtpMoth.SelectedDateTime.Year, dtpMoth.SelectedDateTime.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                dtpWorkStartDay.SelectedDateTime = firstOfThisMonth;
                dtpWorkEndDay.SelectedDateTime = lastOfThisMonth;

                dtpDelWorkStartDay.SelectedDateTime = firstOfThisMonth;
                dtpDelWorkEndDay.SelectedDateTime = lastOfThisMonth;


                tabControlMain.SelectedIndex = 1;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkSetTime(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                int iRow = cell.Row.Index;
                if (cell.Text == "")
                {
                    return;
                }

                if (cell.Column.Name == "SHFT_STRT_HMS" || cell.Column.Name == "SHFT_END_HMS")
                {
                    
                    cell.Value.ToString();
                    string dCLCTLSL = Util.NVC(DataTableConverter.GetValue(dgTimeList.Rows[iRow].DataItem, "SHFT_STRT_HMS"));
                    Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgTimeList.Rows[iRow].DataItem, "SHFT_END_HMS"));

                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_033_WORKER popup = sender as COM001_033_WORKER;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                   
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.06.11
        private void getScheduleWorkerInfomation()
        {
            DataTable RQSTDT = new DataTable();
            DataTable dtResult = null;
            try
            {
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(SHOP.SelectedValue);
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);
                dr["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT.SelectedValue) == "" ? null : Util.NVC(EQUIPMENTSEGMENT.SelectedValue);
                dr["SHFT_ID"] = null;
                dr["FROMDATE"] = dtpSearch_DateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpSearch_DateTo.SelectedDateTime.ToString("yyyyMMdd");
                
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CALDATE_WRKR", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgWorkerList, dtResult, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //2019.12.11
        private void RateSearch()
        {
            DataTable RQSTDT = new DataTable();
            DataTable dtResult = null;
            try
            {
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(cboShopRate.SelectedValue);
                dr["AREAID"] = Util.NVC(cboAreaRate.SelectedValue) == "" ? null : Util.NVC(cboAreaRate.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegmentRate.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegmentRate.SelectedValue);
                dr["FROM_DATE"] = dtpMoth_Start.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpMoth_End.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgEquipmentRate, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        
        private void setRate_Display(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                int iRow = cell.Row.Index;

                if (cell.Text == "")
                {
                    return;
                }

                if (cell.Column.Name == "EQSG_USE_RATE")
                {
                    int minRate = 0;
                    int maxRate = 0;

                    int iRate = Int32.Parse(Util.NVC(cell.Text));

                    DataTable result = new DataTable();
                    result.Columns.Add("LANGID", typeof(string));
                    result.Columns.Add("CMCDTYPE", typeof(string));
                    result.Columns.Add("CMCODE", typeof(string));

                    DataRow dr = result.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "PACK_UI_EQSG_USE_RATE";
                    dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

                    result.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", result);
                    //사용비율은 100보다 작아야 합니다.
                    if (dtResult.Rows.Count > 0)
                    {
                        minRate = Int16.Parse(dtResult.Rows[0]["ATTRIBUTE1"].ToString());
                        maxRate = Int16.Parse(dtResult.Rows[0]["ATTRIBUTE2"].ToString());

                        if (iRate > maxRate)
                        {
                            Util.MessageInfo("SFU8229", maxRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "EQSG_USE_RATE", nRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", false);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                            return;
                        }
                        else if (iRate < minRate)
                        {
                            Util.MessageInfo("SFU8230", minRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "EQSG_USE_RATE", nRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", false);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                            return;
                        }
                        else if ((minRate < iRate) || (iRate >= maxRate))
                        {
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", true);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                        }
                    }else
                    {
                        if (iRate > 100)
                        {
                            Util.Alert("SFU8143");
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "EQSG_USE_RATE", nRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", false);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                            return;
                        }
                        else if (iRate < 0)
                        {
                            Util.Alert("SFU8142");
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "EQSG_USE_RATE", nRate);
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", false);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                            return;
                        }
                        else if ((0 < iRate) || (iRate >= 100))
                        {
                            DataTableConverter.SetValue(dgEquipmentRate.Rows[iRow].DataItem, "CHK", true);// MES 2.0 CHK 컬럼 Bool 오류 Patch
                        }
                    }
                                            
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.05.28
        private bool chkRateDelete()
        {
            bool bRetrun = true;

            try
            {
                if (Util.NVC(cboDelWorkShopRate.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    bRetrun = false;
                    cboDelWorkShopRate.Focus();
                    return bRetrun;
                }
                if (Util.NVC(cboDelWorkAreaRate.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    bRetrun = false;
                    cboDelWorkAreaRate.Focus();
                    return bRetrun;
                }
                if (cboDelWorkEquipmentSegmentRate.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    bRetrun = false;
                    cboDelWorkEquipmentSegmentRate.Focus();
                    return bRetrun;
                }

                if (DateTime.Parse(dtpDelWorkStartDayRate.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpDelWorkEndDayRate.SelectedDateTime.ToString("yyyy-MM-dd")))
                {
                    Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                    bRetrun = false;
                    dtpDelWorkStartDayRate.Focus();
                    return bRetrun;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRetrun;
        }

        private void delRateedata()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROMDATE");
                RQSTDT.Columns.Add("TODATE");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("SHFT_ID");

                DataRow dr = null;
                dr = RQSTDT.NewRow();
                dr["FROMDATE"] = dtpDelWorkStartDayRate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDelWorkEndDayRate.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = cboDelWorkEquipmentSegmentRate.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_BAS_DEL_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE_BY_FROMTO", "RQSTDT", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool chkRelation()
        {
            bool bRelationRetrun = true;

            try
            {
                if(GetWRKR())
                {
                    Util.Alert("SFU2084");  //해당 기간에 월력-작업자 등록 현황 데이터가 존해하여 삭제할 수 없습니다.
                    bRelationRetrun = false;
                    return bRelationRetrun;
                }

                if(GetUseRate())
                {
                    Util.Alert("SFU2085");  //해당 기간에 월력-라인사용비율 관리 데이터가 존해하여 삭제할 수 없습니다.
                    bRelationRetrun = false;
                    return bRelationRetrun;
                }

                return bRelationRetrun;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool GetWRKR()
        {
            bool bRWRKRetrun = true;

            try
            {
                DataTable dtResult = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROMDATE");
                RQSTDT.Columns.Add("TODATE");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("SHFT_ID");

                DataRow dr = null;
                dr = RQSTDT.NewRow();
                dr["FROMDATE"] = dtpDelWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDelWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = cboDelWorkEquipmentSegment.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CALDATE_WRKR_BY_FROMTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    int iCnt = Convert.ToInt32(dtResult.Rows[0]["CNT"].ToString());

                    if (iCnt > 0)
                    {
                        bRWRKRetrun = true;
                    }
                    else
                    {
                        bRWRKRetrun = false;
                    }
                }

                return bRWRKRetrun;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool GetUseRate()
        {
            bool bUseRateRetrun = true;

            try
            {
                DataTable dtResult = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROMDATE");
                RQSTDT.Columns.Add("TODATE");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("SHFT_ID");

                DataRow dr = null;
                dr = RQSTDT.NewRow();
                dr["FROMDATE"] = dtpDelWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDelWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = cboDelWorkEquipmentSegment.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_CALDATE_SHFT_EQSG_USE_RATE_BY_FROMTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    int iCnt = Convert.ToInt32(dtResult.Rows[0]["CNT"].ToString());

                    if (iCnt > 0)
                    {
                        bUseRateRetrun = true;
                    }
                    else
                    {
                        bUseRateRetrun = false;
                    }
                }

                return bUseRateRetrun;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgTimeListRate_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //if(dgTimeList.Rows.Count == 1)
                //{
                //    return;
                //}



                //DateTime dEditValue = DateTime.Parse(e.Cell.Value.ToString());

                //int iEditIndex_Column = e.Cell.Column.Index;
                //int iEditIndex_Row = e.Cell.Row.Index;

                //string sEditColumnName = e.Cell.Column.Name;
                //string sChkTagetName = sEditColumnName == "SHFT_STRT_HMS" ? "SHFT_END_HMS" : "SHFT_STRT_HMS";

                //DataTable dtTimeList = DataTableConverter.Convert(dgTimeList.ItemsSource);

                //C1.WPF.DataGrid.DataGridCell cell = e.Cell;                
                //string sShft =Util.NVC( DataTableConverter.GetValue(dgTimeList.Rows[e.Cell.Row.Index].DataItem, "CBO_CODE"));

                ////DataTable dtBefore = DataTableConverter.Convert(dgTimeList.ItemsSource);
                //DataRow[] drBefore = dtBeForeShift.Select("CBO_CODE = '"+ sShft + "'");
                //string sBeforeValue = Util.NVC(drBefore[0][sEditColumnName]);


                //if (sEditColumnName == "SHFT_STRT_HMS")
                //{
                //    if (iEditIndex_Row == 0)
                //    {
                //        string sTempTaget = dtTimeList.Rows[dtTimeList.Rows.Count - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row - 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dEditValue, dTempTaget);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;                            
                //        }
                //    }
                //}
                //else
                //{
                //    if (iEditIndex_Row == dtTimeList.Rows.Count - 1)
                //    {
                //        string sTempTaget = dtTimeList.Rows[0][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //    else
                //    {
                //        string sTempTaget = dtTimeList.Rows[iEditIndex_Row + 1][sChkTagetName].ToString();
                //        DateTime dTempTaget = DateTime.Parse(sTempTaget);
                //        int iCheck = DateTime.Compare(dTempTaget, dEditValue);
                //        if (iCheck < 0)
                //        {
                //            Util.Alert("SFU1694");
                //            e.Cell.Value = sBeforeValue;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnShftTimeChageRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContentLeftRate.RowDefinitions[2].Height.Value == 175)
                {
                    ContentLeftRate.RowDefinitions[2].Height = new GridLength(280);

                    dgInputRate.RowDefinitions[1].Height = new GridLength(31);
                    dgInputRate.RowDefinitions[2].Height = new GridLength(31);
                    dgInputRate.RowDefinitions[3].Height = new GridLength(31);
                    dgInputRate.RowDefinitions[4].Height = new GridLength(31);
                    dgInputRate.RowDefinitions[5].Height = new GridLength(1, GridUnitType.Star);
                    dgInputRate.RowDefinitions[6].Height = new GridLength(31);
                }
                else
                {
                    ContentLeftRate.RowDefinitions[2].Height = new GridLength(175);

                    dgInputRate.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                    dgInputRate.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                    dgInputRate.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                    dgInputRate.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                    dgInputRate.RowDefinitions[5].Height = new GridLength(0);
                    dgInputRate.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}