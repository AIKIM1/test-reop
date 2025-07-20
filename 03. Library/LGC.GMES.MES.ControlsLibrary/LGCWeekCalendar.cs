using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class LGCWeekCalendar : Control
    {
        #region private property
        private ScrollViewer WeekPresenter;
        private ToggleButton NextButton;
        private TextBlock HeaderText;
        private ToggleButton PreviousButton;
        private Grid LayoutRoot;
        private ToggleButton _CheckedBtn;
        private Brush _backupBrush;
        private ToggleButton _lastWeek;
        private Style btnWeekDayStyle;
        private bool isInit = false;
        private DateTime? oldValue;
        #endregion

        #region event
        public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged;
        public event EventHandler ItemDoubleClick;
        #endregion

        #region Dependency Property
        #region YearProperty
        public static readonly DependencyProperty YearProperty =
                DependencyProperty.Register("Year", typeof(int), typeof(LGCWeekCalendar),
                new PropertyMetadata(new PropertyChangedCallback((s, a) =>
                {
                    LGCWeekCalendar sender = s as LGCWeekCalendar;
                    int oldValue = (int)a.OldValue;
                    int newValue = (int)a.NewValue;

                    // Handle for changed property
                    if (sender.isInit)
                    {
                        sender.forceRefreshYear();
                    }
                })));

        public int Year
        {
            get { return (int)this.GetValue(YearProperty); }
            set { this.SetValue(YearProperty, value); }
        }
        #endregion

        public int SelectedWeekNo { get; set; }

        #region SelectedDateTimeProperty
        public static readonly DependencyProperty SelectedDateTimeProperty =
                DependencyProperty.Register("SelectedDateTime", typeof(DateTime?), typeof(LGCWeekCalendar),
                new PropertyMetadata(new DateTime?(), new PropertyChangedCallback((s, a) =>
                {
                    LGCWeekCalendar sender = s as LGCWeekCalendar;
                    DateTime? oldValue = (DateTime?)a.OldValue;
                    DateTime? newValue = (DateTime?)a.NewValue;

                    if (newValue.HasValue && !newValue.Equals(oldValue))
                    {
                        DateTime? refinedDate = sender.refiningDate(newValue);
                        if (!newValue.Equals(refinedDate))
                        {
                            sender.oldValue = oldValue;
                            sender.SelectedDateTime = refinedDate;
                            return;
                        }
                        else // 선택된 날짜가 몇년 몇번째 주간인지 찾는다
                        {
                            DateTime? firstWeek = sender.GetFirstWeek(refinedDate.Value.Year + 1);
                            if (firstWeek.Equals(refinedDate)) // 다음년도의 첫번째 주간 시작 날짜와 선택된 날짜의 주간 시작 날짜가 같으면 선택된 날짜가 속한 주간은 다음년도의 첫번째 주간이다
                            {
                                sender.Year = refinedDate.Value.Year + 1;
                                sender.SelectedWeekNo = 1;
                            }
                            else // 다음년도의 첫번째 주간 시작 날짜와 선택된 날짜의 주간 시작 날짜가 다르다면
                                 // 선택된 날짜의 주간은 해당년도 첫번째 주간의 시작일자와의 차이로 계산된다
                            {
                                sender.Year = refinedDate.Value.Year;
                                firstWeek = sender.GetFirstWeek(refinedDate.Value.Year);
                                TimeSpan timeSpan = refinedDate.Value - firstWeek.Value;
                                sender.SelectedWeekNo = ((int)timeSpan.TotalDays) / 7 + 1;
                            }

                            if (sender.isInit)
                            {
                                sender.refreshBtnSelection();

                                if (sender.oldValue != null)
                                {
                                    sender.fireSelectedDateTimeChangedEvent(sender.oldValue, newValue);
                                }
                                else
                                {
                                    sender.fireSelectedDateTimeChangedEvent(oldValue, newValue);
                                }

                                sender.oldValue = null;
                            }
                        }
                    }
                })));

        public DateTime? SelectedDateTime
        {
            get { return (DateTime?)this.GetValue(SelectedDateTimeProperty); }
            set { this.SetValue(SelectedDateTimeProperty, value); }
        }
        #endregion


        #region StartDayOfWeekProperty
        public static readonly DependencyProperty StartDayOfWeekProperty =
        DependencyProperty.Register("StartDayOfWeek", typeof(DayOfWeek), typeof(LGCWeekCalendar),
        new PropertyMetadata(DayOfWeek.Monday, new PropertyChangedCallback((s, a) =>
        {
            LGCWeekCalendar sender = s as LGCWeekCalendar;
            DayOfWeek oldValue = (DayOfWeek)a.OldValue;
            DayOfWeek newValue = (DayOfWeek)a.NewValue;

            // Handle for changed property
            if (sender.SelectedDateTime != null && sender.SelectedDateTime.HasValue)
                sender.SelectedDateTime = sender.SelectedDateTime;
        })));

        public DayOfWeek StartDayOfWeek
        {
            get { return (DayOfWeek)this.GetValue(StartDayOfWeekProperty); }
            set { this.SetValue(StartDayOfWeekProperty, value); }
        }
        #endregion

        #endregion

        public LGCWeekCalendar()
        {
            this.DefaultStyleKey = typeof(LGCWeekCalendar);

            this.Loaded += new RoutedEventHandler(LGCWeekCalendar_Loaded);
        }

        void LGCWeekCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(LGCWeekCalendar_Loaded);

            ApplyTemplate();
        }

        private void forceRefreshYear()
        {
            DateTime fweek = GetFirstWeek(Year);
            DateTime lweek = GetFirstWeek(Year + 1).AddDays(-7);
            TimeSpan dspan = lweek - fweek;
            int weekDiff = ((int)dspan.TotalDays) / 7 + 1;
            if (weekDiff > 52)
            {
                _lastWeek.Visibility = Visibility.Visible;
            }
            else
            {
                _lastWeek.Visibility = Visibility.Collapsed;
            }

            HeaderText.Text = Year.ToString();

            refreshBtnSelection();
        }

        public override void OnApplyTemplate()
        {
            // get UIElements
            LayoutRoot = (Grid)this.GetTemplateChild("LayoutRoot");
            PreviousButton = (ToggleButton)this.GetTemplateChild("PreviousButton");
            HeaderText = (TextBlock)this.GetTemplateChild("HeaderText");
            NextButton = (ToggleButton)this.GetTemplateChild("NextButton");
            WeekPresenter = (ScrollViewer)this.GetTemplateChild("WeekPresenter");
            btnWeekDayStyle = (Style)LayoutRoot.Resources["btnWeekDayStyle"];

            base.OnApplyTemplate();

            // initialize
            PreviousButton.Click += new RoutedEventHandler(PreviousButton_Click);
            NextButton.Click += new RoutedEventHandler(NextButton_Click);

            LGCWeekCalendar_Initialize();
        }

        public void ScrollIntoView()
        {
            if (SelectedDateTime.HasValue)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ToggleButton btn = findBtn();
                        //WeekPresenter.ScrollIntoView(btn);
                    }
                    catch
                    {
                    }
                }));
                //this.Dispatcher.BeginInvoke(delegate ()
                //{
                //    try
                //    {
                //        ToggleButton btn = findBtn();
                //        WeekPresenter.ScrollIntoView(btn);
                //    }
                //    catch
                //    {
                //    }
                //});
            }
        }

        public void LGCWeekCalendar_Initialize()
        {
            // initialize
            isInit = true;
            StackPanel verticalStack = new StackPanel() { Orientation = Orientation.Vertical };
            for (int inx = 0; inx < 13; inx++)
            {
                StackPanel horizontalStack = new StackPanel() { Orientation = Orientation.Horizontal };

                for (int jnx = 1; jnx <= 4; jnx++)
                {
                    ToggleButton btn = new ToggleButton() { Width = 36.5, Height = 30, Content = "W" + (4 * inx + jnx).ToString("00") };
                    btn.DataContext = 4 * inx + jnx;
                    btn.Style = btnWeekDayStyle;
                    btn.Click += new RoutedEventHandler(btn_Click);
                    horizontalStack.Children.Add(btn);
                }
                verticalStack.Children.Add(horizontalStack);
            }

            StackPanel lastStack = new StackPanel() { Orientation = Orientation.Horizontal };
            _lastWeek = new ToggleButton() { Width = 36.5, Height = 30, Content = "W53", Visibility = Visibility.Collapsed };
            _lastWeek.Style = btnWeekDayStyle;
            _lastWeek.Click += new RoutedEventHandler(btn_Click);
            lastStack.Children.Add(_lastWeek);
            verticalStack.Children.Add(lastStack);

            WeekPresenter.Content = verticalStack;

            if (SelectedDateTime.HasValue)
            {
                DateTime? selected = SelectedDateTime;
                if (selected != null && selected.HasValue)
                {
                    forceRefreshYear();
                    SelectedDateTime = SelectedDateTime.Value.AddDays(7);
                    SelectedDateTime = selected;
                }
            }
            else
            {
                Year = DateTime.Now.Year;
            }
        }

        void mouseHelper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemDoubleClick != null)
            {
                ItemDoubleClick(this, new EventArgs());
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton s = sender as ToggleButton;

            SelectedDateTime = findDateTime(s);

            refreshBtnSelection();

            //if (MouseDoubleClickHelper.IsDoubleClick(sender, e))
            //{
            //    if (ItemDoubleClick != null)
            //    {
            //        ItemDoubleClick(this, new EventArgs());
            //    }
            //}
        }

        private void fireSelectedDateTimeChangedEvent(DateTime? oldData, DateTime? newData, bool force = false)
        {
            if (!force)
            {
                if (oldData == null && newData == null)
                    return;

                if (oldData != null && oldData.Equals(newData))
                    return;
            }

            if (SelectedDatesChanged != null)
            {
                List<DateTime?> oldList = new List<DateTime?>();
                oldList.Add(oldData == null ? new DateTime?() : oldData);
                List<DateTime?> newList = new List<DateTime?>();
                newList.Add(newData == null ? new DateTime?() : newData);

                //SelectedDatesChanged(this, new SelectionChangedEventArgs(oldList, newList));
            }
        }

        private void checkBtn(ToggleButton btn)
        {
            if (btn != null)
            {
                uncheckBtn();
                btn.IsChecked = true;
                _CheckedBtn = btn;

                _backupBrush = _CheckedBtn.BorderBrush;
                _CheckedBtn.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void uncheckBtn()
        {
            if (_CheckedBtn != null)
            {
                _CheckedBtn.BorderBrush = _backupBrush;
                _CheckedBtn.IsChecked = false;
                _CheckedBtn = null;
            }
        }

        private ToggleButton findBtn()
        {
            if (SelectedWeekNo > 0)
            {
                DateTime fweek = GetFirstWeek(Year);
                DateTime fweek2 = SelectedDateTime.Value.AddDays(-((SelectedWeekNo - 1) * 7)).Date;
                if (fweek.Equals(fweek2))
                {
                    int inx = (SelectedWeekNo - 1) / 4;
                    int jnx = (SelectedWeekNo - 1) % 4;

                    return ((WeekPresenter.Content as StackPanel).Children[inx] as StackPanel).Children[jnx] as ToggleButton;
                }
            }

            return null;
        }

        private DateTime? findDateTime(ToggleButton btn)
        {
            if (btn != null)
            {
                DateTime fWeek = GetFirstWeek(Year);
                string strWeekDiff = btn.Content.ToString().Substring(1, 2);
                int weekDiff = int.Parse(strWeekDiff) - 1;
                return new DateTime?(fWeek.AddDays(7 * weekDiff));
            }

            return new DateTime?();
        }

        private DateTime GetFirstWeek(int year)
        {
            DateTime Date = new DateTime(year, 1, 1);

            switch (Date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                    while (Date.DayOfWeek != DayOfWeek.Monday)
                    {
                        Date = Date.AddDays(-1);
                    }
                    break;
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    while (Date.DayOfWeek != DayOfWeek.Monday)
                    {
                        Date = Date.AddDays(+1);
                    }
                    break;
            }

            return Date;
        }

        private DateTime? refiningDate(DateTime? date)
        {
            if (date.HasValue)
            {
                while (date.Value.DayOfWeek != this.StartDayOfWeek)
                {
                    date = date.Value.AddDays(-1);
                }

                return date;
            }
            return date;
        }

        private void refreshBtnSelection()
        {
            uncheckBtn();

            if (SelectedDateTime.HasValue)
            {
                ToggleButton btn = findBtn();
                if (btn != null && !btn.Equals(_CheckedBtn))
                    checkBtn(btn);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Year++;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Year--;
        }
    }
}
