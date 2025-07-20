/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.C1Schedule;
using C1.WPF;
using LGC.GMES.MES.Common;
//using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_208 : UserControl
    {
        #region Declaration & Constructor 

        private DateTime dtTemp = DateTime.Today;
        public PGM_GUI_208()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            setComboBoxShift();
            cboShift.SelectedIndex = 0;
            cboDelShift.SelectedIndex = 0;
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
            e.Handled = true;
        }
        /// <summary>
        /// Scheduler 더블클릭시
        /// open되는 기본 입력팝업 무시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Scheduler 스타일 변경무시.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_StyleChanged(object sender, RoutedEventArgs e)
        {
            Scheduler.Style = Scheduler.MonthStyle;
            setDisplayScheduler();
        }
        /// <summary>
        /// 일정선택시 삭제 대기 정보에 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_SelectedAppointmentChanged(object sender, C1.WPF.Schedule.PropertyChangeEventArgsBase e)
        {
            C1.C1Schedule.Appointment appointment = Scheduler.SelectedAppointment;
            if (appointment != null)
            {
                dtpDelWorkStartDay.SelectedDateTime = appointment.Start;
                dtpDelWorkEndDay.SelectedDateTime = appointment.End;
                //cboDelShift.SelectedIndex = appointment.Subject;
            }
        }
        #endregion


        private void btnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            dtTemp = dtTemp.AddMonths(-1);
            setDisplayScheduler();
        }

        private void btnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            dtTemp = dtTemp.AddMonths(+1);
            setDisplayScheduler();
        }

        private void btnToDay_Click(object sender, RoutedEventArgs e)
        {
            dtTemp = DateTime.Today;
            setDisplayScheduler();
        }

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //샘플로 하드코딩함. 차후 기준정보 에 의해 조회 후 set하도록 수정해야함.!!
            if ("A" == cboShift.SelectedValue.ToString())
            {
                teWorkStartTime.Value = TimeSpan.Parse("06:00:00");
                teWorkEndTime.Value = TimeSpan.Parse("18:00:00");
            }
            else if ("B" == cboShift.SelectedValue.ToString())
            {
                teWorkStartTime.Value = TimeSpan.Parse("18:00:00");
                teWorkEndTime.Value = TimeSpan.Parse("06:00:00");
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //저장후 조회.
            saveScheduledata(); //저장 구현해야함!!!

            getScheduleInfomation();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Mehod

        private void setDisplayScheduler()
        {
            Scheduler.VisibleDates.BeginUpdate();
            Scheduler.VisibleDates.Clear();
            DateTime firstOfNextMonth = new DateTime(dtTemp.Year, dtTemp.Month, 1).AddMonths(1);
            DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);
            for (int i = 0; i < lastOfThisMonth.Day; i++)
            {
                Scheduler.VisibleDates.Insert(i, new DateTime(dtTemp.Year, dtTemp.Month, i + 1));
            }
            Scheduler.VisibleDates.EndUpdate();
        }

        private void setComboBoxShift()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("NAME", typeof(string));
            dtResult.Columns.Add("CODE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "A", "A" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "B", "B" };
            dtResult.Rows.Add(newRow);

            cboShift.ItemsSource = DataTableConverter.Convert(dtResult);

            cboDelShift.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        private void saveScheduledata()
        {

        }
        private DataTable getScheduleData()
        {
            #region 샘플데이터 향후 조회해야함!!!
            DateTime dStartdate = dtpWorkStartDay.SelectedDateTime;
            DateTime dEnddate = dtpWorkEndDay.SelectedDateTime;

            DateTime dTemptoDay = DateTime.Today;
            string sStartTime = dTemptoDay.Add((TimeSpan)teWorkStartTime.Value).ToString("tt hh:mm"); ;
            string sEndTime = dTemptoDay.Add((TimeSpan)teWorkEndTime.Value).ToString("tt hh:mm"); ;


            DataRow newRow = null;
            DataTable dtReturn = new DataTable();
            dtReturn.Columns.Add("SHIFT");
            dtReturn.Columns.Add("STARTDATE", typeof(DateTime));
            dtReturn.Columns.Add("ENDDATE", typeof(DateTime));
            dtReturn.Columns.Add("STARTTIME", typeof(string));
            dtReturn.Columns.Add("ENDTIME", typeof(string));

            newRow = dtReturn.NewRow();
            newRow.ItemArray = new object[] { "A조", dStartdate, dEnddate, sStartTime, sEndTime };
            dtReturn.Rows.Add(newRow);
            newRow = dtReturn.NewRow();
            newRow.ItemArray = new object[] { "B조", dStartdate, dEnddate, "오후 06:00", "오전 06:00" };
            dtReturn.Rows.Add(newRow);

            #endregion
            return dtReturn;
        }
        private void getScheduleInfomation()
        {

            Color[] colorSet = { Color.FromRgb(169, 208, 142), Color.FromRgb(142, 169, 219), Color.FromRgb(244, 176, 132) };
            int iColorIndex = 0;


            DataTable dtTemp = getScheduleData();

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                C1.C1Schedule.Label label = new C1.C1Schedule.Label();
                //label.Color = (Color)ColorConverter.ConvertFromString(ShiftColor.SelectedValue.ToString());
                label.Color = colorSet[iColorIndex];
                Appointment app = new Appointment(DateTime.Parse(dtTemp.Rows[i]["STARTDATE"].ToString()), DateTime.Parse(dtTemp.Rows[i]["ENDDATE"].ToString()), dtTemp.Rows[i]["SHIFT"].ToString());
                app.AllDayEvent = true;
                app.Label = label;
                app.Location = dtTemp.Rows[i]["STARTTIME"].ToString() + " ~ " + dtTemp.Rows[i]["ENDTIME"].ToString();

                Scheduler.DataStorage.AppointmentStorage.Appointments.Add(app);

                iColorIndex++;
            }
        }



        #endregion
    }
}