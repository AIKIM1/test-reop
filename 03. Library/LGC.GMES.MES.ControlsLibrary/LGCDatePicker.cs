/*************************************************************************************
 Created Date : Unknown
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  Unknown  DEVELOPER : Initial Created.
  2024.06.27 조영대 : 날짜 키인 기능 적용.
                      최초 선택시 블럭설정, 다시 클릭 혹은 좌우 화살표 사용으로 편집
                      Ctrl + Left, Ctrl + Right 키로 년, 월, 일 블록 설정.
                      Up 키, Down 키로 커서 위치 블럭 증가 혹은 감소.
                      일, 월의 경우 입력시 두자리 필수. ex) 2024년 1월 2일의 경우 20240102-(O), 202412-(X)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    #region Public Class

    public enum LGCDataPickerType
    {
        Date,
        Month,
        Year
    }

    #endregion

    public class LGCDatePicker : Control
    {
        //Week의 시작요일 : 월요일
        //주의 기준 : 목요일

        #region Internal Class

        protected enum ClickedControlType
        {
            Title,
            Preview,
            Next,
            Date,
            Month,
            Year,
            None
        }

        protected class DatePart
        {
            public enum DatePartType
            {
                Day,
                Month,
                Year
            }

            public DatePart(DatePartType selectDatePart, int startIndex, int length)
            {
                SelectDatePart = selectDatePart;
                StartIndex = startIndex;
                Length = length;
            }

            public DatePartType SelectDatePart { get; set; }
            public int StartIndex { get; set; }
            public int Length { get; set; }

        }

        #endregion

        #region Declaration & Constructor 

        private const string ElementLayoutRoot = "LayoutRoot";
        private const string ControlTemplateDate = "imgDate";
        private const string ControlTemplateWeek = "imgWeek";
        private const string ControlTemplateMonth = "imgMonth";
        private const string ControlTemplateYear = "imgYear";

        private const string ElementtxtDate = "txtDate";
        private const string ElementbtnCalendar = "btnCalendar";
        private const string Elementcalendar1 = "calendar1";
        private const string Elementcalendar2 = "calendar2";
        private const string ElementCalendarPopUp = "CalendarPopUp";
        private const string ElementValidationPopUp = "ValidationPopUp";
        private const string ElementlblValedation = "lblValedation";

        private string _dateFormat = "d";

        private DateTime tmpDateTime;
        private LGCDataPickerType _datePickerType = LGCDataPickerType.Date;
        private ClickedControlType clickControl = ClickedControlType.None;
        private System.Globalization.CultureInfo currentCulture = System.Globalization.CultureInfo.CurrentCulture;
        private Dictionary<int, DatePart> partDic = null;
        private int selectPart = 0;
        private bool isEditting = false;
        private DateTime saveEditDate = DateTime.MinValue;

        private string _textData;
        private bool _IsReadOnly = false;
        private string _validationMessage;

        private C1MaskedTextBox txtDate;
        private Button btnCalendar;
        private Calendar calendar1;
        private Popup CalendarPopUp;
        private Popup ValidationPopUp;
        private FrameworkElement layoutRoot;
        private Label lblValedation;

        private ControlTemplate CTimgDate;
        private ControlTemplate CTimgWeek;
        private ControlTemplate CTimgMonth;
        private ControlTemplate CTimgYear;
              
        public event SelectionChangedEventHandler SelectedDataTimeChanged;

        #region Property

        public string DateFormat
        {
            get { return _dateFormat; }
            set { _dateFormat = value; }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                 typeof(String),
                                                                 typeof(LGCDatePicker),
                                                                 new PropertyMetadata(OnTextPropertyChanged));

        [Category("GMES"), Description("사용 금지 : 이름 변경 IsReadOnly")]
        public bool IsReanOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        [Category("GMES"), Description("Gets or sets ReadOnly.")]
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        [Category("GMES"), Description("Gets or sets an Text.")]
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string ValidationMessage
        {
            get { return _validationMessage; }
            set { _validationMessage = value; }
        }

        public bool IsNullInitValue { get; set; }

        public static readonly DependencyProperty SelectedDateTimeProperty = DependencyProperty.Register("SelectedDateTime",
                                                                                                          typeof(DateTime),
                                                                                                          typeof(LGCDatePicker),
                                                                                                          new PropertyMetadata(OnSelectedDateTimePropertyPropertyChanged));

        [Category("GMES"), Description("Gets or sets an SelectedDateTime.")]
        public DateTime SelectedDateTime
        {
            get { return (DateTime)GetValue(SelectedDateTimeProperty); }
            set { SetValue(SelectedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty DatepickerTypeProperty = DependencyProperty.Register(
                                                                           "DatepickerType",
                                                                           typeof(LGCDataPickerType),
                                                                           typeof(LGCDatePicker),
                                                                           new PropertyMetadata(OnDatepickerTypePropertyPropertyPropertyChanged));


        [Category("GMES"), Description("Gets or sets an LGCDataPickerType.")]
        public LGCDataPickerType DatepickerType
        {
            get { return (LGCDataPickerType)GetValue(DatepickerTypeProperty); }
            set { SetValue(DatepickerTypeProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly",
                                                                                                    typeof(bool),
                                                                                                    typeof(LGCDatePicker),
                                                                                                    new PropertyMetadata(OnIsReadOnlyPropertyPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LGCDatePicker lGCDatePicker = (LGCDatePicker)d;

            if (lGCDatePicker != null)
                if (lGCDatePicker.txtDate != null)
                    lGCDatePicker.txtDate.Text = ((string)e.NewValue);
        }

        private static void OnIsReadOnlyPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LGCDatePicker lGCDatePicker = (LGCDatePicker)d;

            if (lGCDatePicker != null)
            {
                if (lGCDatePicker.txtDate != null)
                {
                    lGCDatePicker._IsReadOnly = ((bool)e.NewValue);
                    lGCDatePicker.txtDate.IsReadOnly = ((bool)e.NewValue);
                }
            }
        }

        private static void OnSelectedDateTimePropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LGCDatePicker lGCDatePicker = (LGCDatePicker)d;

            if (lGCDatePicker != null)
            {
                if (lGCDatePicker.txtDate != null)
                {
                    lGCDatePicker.calendar1.SelectedDate = ((DateTime)e.NewValue);
                    lGCDatePicker.calendar1.DisplayDate = ((DateTime)e.NewValue);

                    lGCDatePicker.SetDateText(e.NewValue);

                    if (!lGCDatePicker.isEditting && lGCDatePicker.SelectedDataTimeChanged != null)
                    {
                        System.Collections.IList lstRemoved = new List<object>();
                        lstRemoved.Add(e.OldValue);
                        System.Collections.IList lstAdded = new List<object>();
                        lstAdded.Add(e.NewValue);
                        SelectionChangedEventArgs selectedArg = new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, lstRemoved, lstAdded);
                        lGCDatePicker.SelectedDataTimeChanged(lGCDatePicker, selectedArg);
                    }
                }
            }
        }

        private static void OnDatepickerTypePropertyPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LGCDatePicker lGCDatePicker = (LGCDatePicker)d;

            if (lGCDatePicker != null)
            {
                lGCDatePicker._datePickerType = ((LGCDataPickerType)e.NewValue);

                if (lGCDatePicker.txtDate != null)
                {
                    switch (lGCDatePicker._datePickerType)
                    {
                        case (LGCDataPickerType.Month):
                            lGCDatePicker.calendar1.Visibility = Visibility.Visible;
                            lGCDatePicker._dateFormat = lGCDatePicker.currentCulture.DateTimeFormat.YearMonthPattern;
                            lGCDatePicker.calendar1.DisplayMode = CalendarMode.Year;
                            lGCDatePicker.btnCalendar.Template = lGCDatePicker.CTimgMonth;
                            break;

                        case (LGCDataPickerType.Date):
                            lGCDatePicker.calendar1.Visibility = Visibility.Visible;
                            lGCDatePicker._dateFormat = lGCDatePicker.currentCulture.DateTimeFormat.ShortDatePattern;
                            lGCDatePicker.calendar1.DisplayMode = CalendarMode.Month;
                            lGCDatePicker.btnCalendar.Template = lGCDatePicker.CTimgDate;

                            // 시작 요일 일요일로 지정
                            lGCDatePicker.calendar1.FirstDayOfWeek = DayOfWeek.Sunday;
                            break;

                        case (LGCDataPickerType.Year):
                            lGCDatePicker.calendar1.Visibility = Visibility.Visible;
                            lGCDatePicker.calendar1.DisplayMode = CalendarMode.Decade;
                            lGCDatePicker.btnCalendar.Template = lGCDatePicker.CTimgYear;
                            lGCDatePicker._dateFormat = "yyyy";
                            break;
                    }
                }
            }
        }

        #endregion

        public LGCDatePicker()
        {
            this.Name = "lGCDatePicker";
            this.DatepickerType = LGCDataPickerType.Date;
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            #region 2020.11.26 : 조영대 - 디지인 모드에서 날짜 컨트롤 개체참조 오류로 보이지 않는 현상 수정
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            #endregion

            layoutRoot = this.GetTemplateChild(ElementLayoutRoot) as FrameworkElement;

            if (layoutRoot != null)
            {
                CTimgDate = layoutRoot.Resources[ControlTemplateDate] as ControlTemplate;
                CTimgWeek = layoutRoot.Resources[ControlTemplateWeek] as ControlTemplate;
                CTimgMonth = layoutRoot.Resources[ControlTemplateMonth] as ControlTemplate;
                CTimgYear = layoutRoot.Resources[ControlTemplateYear] as ControlTemplate;

                txtDate = layoutRoot.FindName(ElementtxtDate) as C1MaskedTextBox;

                SetDateText(SelectedDateTime);

                txtDate.GotFocus += TxtDate_GotFocus;
                txtDate.LostFocus += TxtDate_LostFocus;
                txtDate.PreviewMouseDown += TxtDate_PreviewMouseDown;

                btnCalendar = layoutRoot.FindName(ElementbtnCalendar) as Button;
                btnCalendar.Click += btnCalendar_Click;

                CalendarPopUp = layoutRoot.FindName(ElementCalendarPopUp) as Popup;
                CalendarPopUp.Opened += CalendarPopUp_Opened;
                CalendarPopUp.Closed += CalendarPopUp_Closed;

                ValidationPopUp = layoutRoot.FindName(ElementValidationPopUp) as Popup;

                lblValedation = layoutRoot.FindName(ElementlblValedation) as Label;

                calendar1 = layoutRoot.FindName(Elementcalendar1) as Calendar;
                calendar1.SelectedDatesChanged += Calendar1_SelectedDatesChanged;
                calendar1.DisplayModeChanged += Calendar1_DisplayModeChanged;
                calendar1.PreviewMouseDown += Calendar1_PreviewMouseDown;
                calendar1.Background = new SolidColorBrush(Colors.White);

                if (!IsNullInitValue) SelectedDateTime = System.DateTime.Now;

            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (!this.CalendarPopUp.IsOpen) txtDate.Focus();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            this.CalendarPopUp.IsOpen = false;
            this.ValidationPopUp.IsOpen = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            try
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        this.CalendarPopUp.IsOpen = false;
                        this.ValidationPopUp.IsOpen = false;
                        break;
                    case Key.Enter:
                        if (Keyboard.FocusedElement is C1MaskedTextBox)
                        {
                            MoveNextFocusable(this);

                            if (!SelectedDateTime.Equals(saveEditDate) && SelectedDataTimeChanged != null)
                            {
                                System.Collections.IList lstRemoved = new List<object>();
                                lstRemoved.Add(saveEditDate);
                                System.Collections.IList lstAdded = new List<object>();
                                lstAdded.Add(SelectedDateTime);
                                SelectionChangedEventArgs selectedArg = new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, lstRemoved, lstAdded);
                                SelectedDataTimeChanged(this, selectedArg);
                            }

                            saveEditDate = SelectedDateTime;
                        }
                        break;
                    case Key.Left:
                        if (Keyboard.IsKeyDown(Key.LeftCtrl)) SelectPartLeft();
                        break;
                    case Key.Right:
                        if (Keyboard.IsKeyDown(Key.LeftCtrl)) SelectPartRight();
                        break;
                    case Key.Up:
                        SelectPartUp();
                        break;
                    case Key.Down:
                        SelectPartDown();
                        break;
                }

                if (txtDate.ToolTip == null)
                {
                    switch (e.Key)
                    {
                        case Key.Left:
                        case Key.Right:
                        case Key.Up:
                        case Key.Down:
                            txtDate.ToolTip = MessageDic.Instance.GetMessage("FM_ME_0586");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                txtDate.Text = "";
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (e.Delta > 0)
            {
                SelectPartUp();
            }
            else if (e.Delta < 0)
            {
                SelectPartDown();
            }
        }

        #endregion

        #region Event

        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsReadOnly) return;

            if (this.CalendarPopUp.IsOpen)
            {
                this.CalendarPopUp.IsOpen = false;
            }
            else
            {
                switch (this.DatepickerType)
                {
                    case LGCDataPickerType.Month:
                        calendar1.Visibility = Visibility.Visible;
                        calendar1.DisplayMode = CalendarMode.Year;
                        _dateFormat = currentCulture.DateTimeFormat.YearMonthPattern;
                        break;

                    case LGCDataPickerType.Date:
                        calendar1.Visibility = Visibility.Visible;
                        calendar1.DisplayMode = CalendarMode.Month;
                        _dateFormat = currentCulture.DateTimeFormat.ShortDatePattern;
                        break;

                    case LGCDataPickerType.Year:
                        calendar1.Visibility = Visibility.Visible;
                        calendar1.DisplayMode = CalendarMode.Decade;
                        _dateFormat = "yyyy";
                        break;
                }

                this.CalendarPopUp.IsOpen = true;
            }
        }

        private void RootMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CalendarPopUp.IsOpen)
            {
                this.CalendarPopUp.IsOpen = false;
                this.ValidationPopUp.IsOpen = false;
            }
        }

        private void CalendarPopUp_Opened(object sender, EventArgs e)
        {
            try
            {
                //현재 선택된 날짜와 동일한 날짜 선택시에도 팝업을 닫는다.
                if (calendar1.SelectedDate != null)
                {
                    calendar1.SelectedDatesChanged -= new EventHandler<SelectionChangedEventArgs>(Calendar1_SelectedDatesChanged);
                    calendar1.SelectedDate = ((DateTime)calendar1.SelectedDate).AddMilliseconds(1);
                    calendar1.DisplayDate = SelectedDateTime;
                    calendar1.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(Calendar1_SelectedDatesChanged);
                }


                GeneralTransform gt = this.TransformToVisual(Application.Current.MainWindow as UIElement);
                Point offset = gt.Transform(new Point(0, 0));
                double controlTop = offset.Y;
                double controlLeft = offset.X;

                double widthOfDialog = 179;
                double heightOfDialog = 169;

                if (Application.Current.MainWindow.ActualHeight < controlTop + this.ActualHeight + heightOfDialog)
                    ((Popup)sender).VerticalOffset = -1 * heightOfDialog;

                if (Application.Current.MainWindow.ActualWidth < controlLeft + widthOfDialog)
                    ((Popup)sender).HorizontalOffset = Application.Current.MainWindow.ActualWidth - (controlLeft + widthOfDialog);
            }
            catch
            {
                txtDate.Text = "";
            }

            this.Focus();

            Application.Current.MainWindow.MouseLeftButtonDown -= RootMouseLeftButtonDown;
            Application.Current.MainWindow.MouseLeftButtonDown += RootMouseLeftButtonDown;
        }

        private void CalendarPopUp_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.MouseLeftButtonDown -= RootMouseLeftButtonDown;
        }

        private void Calendar1_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            switch (DatepickerType)
            {
                case LGCDataPickerType.Year:
                    if (calendar1.DisplayMode == CalendarMode.Year)
                    {
                        this.CalendarPopUp.IsOpen = false;
                        SelectedDateTime = calendar1.DisplayDate;
                    }
                    break;
                case LGCDataPickerType.Month:
                    if (calendar1.DisplayMode == CalendarMode.Month)
                    {
                        this.CalendarPopUp.IsOpen = false;
                        SelectedDateTime = calendar1.DisplayDate;
                    }
                    break;
            }
        }

        private void Calendar1_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems != e.AddedItems)
                {
                    if (calendar1 != null)
                    {
                        switch (this.DatepickerType)
                        {
                            case LGCDataPickerType.Year:
                                if (calendar1.DisplayMode != CalendarMode.Decade)
                                    calendar1.DisplayMode = CalendarMode.Decade;
                                break;
                            case (LGCDataPickerType.Month):
                                if (calendar1.DisplayMode != CalendarMode.Year)
                                    calendar1.DisplayMode = CalendarMode.Year;
                                break;
                        }

                        SelectedDateTime = (DateTime)e.AddedItems[0];
                        SetDateText(SelectedDateTime);
                    }
                }

                if (calendar1 != null)
                    calendar1.Focus();
            }
            catch (Exception)
            {
                txtDate.Text = "";
            }
        }

        private void Calendar1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                clickControl = ClickedControlType.None;
                if (e.OriginalSource is Border)
                {
                    Border border = (Border)e.OriginalSource;
                    if (border.Name.Equals("NextBtnClickArea"))
                    {
                        clickControl = ClickedControlType.Next;
                    }
                    else if (border.Name.Equals("PreBtnClickArea"))
                    {
                        clickControl = ClickedControlType.Preview;
                    }
                    else if (border != null && border.Child is ContentControl)
                    {
                        ContentControl contentControl = (ContentControl)border.Child;
                        if (contentControl.DataContext is DateTime)
                        {
                            switch (calendar1.DisplayMode)
                            {
                                case CalendarMode.Decade:
                                    clickControl = ClickedControlType.Year;
                                    break;
                                case CalendarMode.Year:
                                    clickControl = ClickedControlType.Month;
                                    break;
                                case CalendarMode.Month:
                                    clickControl = ClickedControlType.Date;
                                    break;
                            }

                        }
                    }
                }
                else if (e.OriginalSource is TextBlock)
                {
                    int number;
                    TextBlock textBlock = (TextBlock)e.OriginalSource;
                    if (textBlock != null && textBlock.Text != null)
                    {
                        if (int.TryParse(textBlock.Text, out number))
                        {
                            if (number > 50)
                            {
                                if (calendar1.DisplayMode == CalendarMode.Decade)
                                {
                                    clickControl = ClickedControlType.Year;
                                }
                                else
                                {
                                    clickControl = ClickedControlType.Title;
                                }
                            }
                            else
                            {
                                switch (calendar1.DisplayMode)
                                {
                                    case CalendarMode.Year:
                                        clickControl = ClickedControlType.Month;
                                        break;
                                    case CalendarMode.Month:
                                        clickControl = ClickedControlType.Date;
                                        break;
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(textBlock.Text))
                        {
                            string title = string.Empty;

                            switch (calendar1.DisplayMode)
                            {
                                case CalendarMode.Year:
                                    if (currentCulture.DateTimeFormat.AbbreviatedMonthNames.Contains(textBlock.Text))
                                    {
                                        clickControl = ClickedControlType.Month;
                                    }
                                    else
                                    {
                                        title = calendar1.DisplayDate.Year.ToString();
                                        if (textBlock.Text.Equals(title)) clickControl = ClickedControlType.Title;
                                    }
                                    break;
                                case CalendarMode.Month:
                                    title = calendar1.DisplayDate.ToString(currentCulture.DateTimeFormat.YearMonthPattern, currentCulture);
                                    if (textBlock.Text.Equals(title)) clickControl = ClickedControlType.Title;
                                    break;
                            }
                        }
                    }
                }
                else if (e.OriginalSource is Image)
                {
                    Image image = (Image)e.OriginalSource;
                    if (image != null && image.TemplatedParent != null)
                    {
                        Button button = (Button)image.TemplatedParent;
                        if (button != null)
                        {
                            if (button.Name.Equals("PART_PreviousButton"))
                            {
                                clickControl = ClickedControlType.Preview;
                            }
                            else if (button.Name.Equals("PART_NextButton"))
                            {
                                clickControl = ClickedControlType.Next;
                            }
                        }
                    }
                }
                else if (e.OriginalSource is Run)
                {
                    Run run = (Run)e.OriginalSource;
                    if (run != null)
                    {
                        string title = string.Empty;

                        switch (calendar1.DisplayMode)
                        {
                            case CalendarMode.Year:
                                title = calendar1.DisplayDate.Year.ToString();
                                break;
                            case CalendarMode.Month:
                                title = calendar1.DisplayDate.ToString(currentCulture.DateTimeFormat.YearMonthPattern, currentCulture);
                                break;
                        }

                        if (!string.IsNullOrEmpty(run.Text) && run.DataContext.Equals(title)) clickControl = ClickedControlType.Title;
                    }
                }

                switch (clickControl)
                {
                    case ClickedControlType.Title:
                    case ClickedControlType.Preview:
                    case ClickedControlType.Next:
                    case ClickedControlType.Year:
                    case ClickedControlType.Month:
                        break;
                    case ClickedControlType.Date:
                        this.CalendarPopUp.IsOpen = false;
                        this.ValidationPopUp.IsOpen = false;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch { txtDate.Text = ""; }
        }

        private void TxtDate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CalendarPopUp.IsOpen)
            {
                this.CalendarPopUp.IsOpen = false;
                this.ValidationPopUp.IsOpen = false;
            }

            if (!txtDate.IsFocused)
            {
                e.Handled = true;

                txtDate.Focus();
                txtDate.SelectAll();
            }
        }

        private void TxtDate_GotFocus(object sender, RoutedEventArgs e)
        {
            isEditting = true;
            saveEditDate = SelectedDateTime;

            SetDateMask(SelectedDateTime);

            this.Dispatcher.BeginInvoke(new Action(() => txtDate.SelectAll()));
        }

        private void TxtDate_LostFocus(object sender, RoutedEventArgs e)
        {
            SetDateText(txtDate.Text);
            txtDate.Mask = null;
            SetDateText(SelectedDateTime);

            //편집 후 포커스 이동시 SelectedDataTimeChanged 이벤트 처리를 위해 편집 전 날짜와 다르면 호출
            if (!SelectedDateTime.Equals(saveEditDate) && SelectedDataTimeChanged != null)
            {
                System.Collections.IList lstRemoved = new List<object>();
                lstRemoved.Add(saveEditDate);
                System.Collections.IList lstAdded = new List<object>();
                lstAdded.Add(SelectedDateTime);
                SelectionChangedEventArgs selectedArg = new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, lstRemoved, lstAdded);
                SelectedDataTimeChanged(this, selectedArg);
            }

            isEditting = false;
            saveEditDate = SelectedDateTime;
        }

        #endregion

        #region Method

        protected void SetDateMask(DateTime dateTime)
        {
            try
            {
                if (txtDate != null)
                {
                    string stringFormatValue = currentCulture.DateTimeFormat.ShortDatePattern.ToUpper();

                    string dateSeparator = currentCulture.DateTimeFormat.DateSeparator;
                    string[] dateSplit = currentCulture.DateTimeFormat.ShortDatePattern.Split(new string[1] { dateSeparator }, StringSplitOptions.None);
                    string dateMask = string.Empty;

                    switch (DatepickerType)
                    {
                        case LGCDataPickerType.Year:
                            txtDate.Mask = "0000";

                            for (int i = 0; i < dateSplit.Length; i++)
                            {
                                switch (dateSplit[i].ToUpper())
                                {
                                    case "YYYY":
                                    case "YY":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Year.ToString("0000"));
                                        break;
                                }
                            }
                            break;
                        case LGCDataPickerType.Month:
                            if (currentCulture.DateTimeFormat.YearMonthPattern.Contains("MMMM"))
                            {
                                dateMask = currentCulture.DateTimeFormat.YearMonthPattern.Replace("'", "").Replace(" ", dateSeparator).ToUpper();
                            }
                            else
                            {
                                dateMask = currentCulture.DateTimeFormat.YearMonthPattern.Replace("'", "").ToUpper();
                            }
                            dateMask = dateMask.Replace("YYYY", "0000").Replace("YY", "0000");
                            dateMask = dateMask.Replace("MMMM", "00").Replace("MM", "00").Replace("M", "00");

                            // 날짜 구분자를 - 로 통일
                            txtDate.Mask = dateMask.Replace(dateSeparator, "-");

                            if (LoginInfo.LANGID.Equals("pl-PL")) stringFormatValue = "DD" + dateSeparator + "MM" + dateSeparator + "YYYY";

                            for (int i = 0; i < dateSplit.Length; i++)
                            {
                                switch (dateSplit[i].ToUpper())
                                {
                                    case "YYYY":
                                    case "YY":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Year.ToString("0000"));
                                        break;
                                    case "MMMM":
                                    case "MMM":
                                    case "MM":
                                    case "M":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Month.ToString("00"));
                                        break;
                                }
                            }
                            break;
                        case LGCDataPickerType.Date:
                            dateMask = currentCulture.DateTimeFormat.ShortDatePattern.ToUpper();
                            dateMask = dateMask.Replace("YYYY", "0000").Replace("YY", "0000");
                            dateMask = dateMask.Replace("MMMM", "00").Replace("MM", "00").Replace("M", "00");
                            dateMask = dateMask.Replace("DDDD", "00").Replace("DD", "00").Replace("D", "00");

                            // 날짜 구분자를 - 로 통일
                            txtDate.Mask = dateMask.Replace(dateSeparator, "-");

                            for (int i = 0; i < dateSplit.Length; i++)
                            {
                                switch (dateSplit[i].ToUpper())
                                {
                                    case "YYYY":
                                    case "YY":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Year.ToString("0000"));
                                        break;
                                    case "MMMM":
                                    case "MMM":
                                    case "MM":
                                    case "M":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Month.ToString("00"));
                                        break;
                                    case "DD":
                                    case "D":
                                        stringFormatValue = stringFormatValue.Replace(dateSplit[i].ToUpper(), dateTime.Day.ToString("00"));
                                        break;
                                }
                            }
                            break;
                    }

                    txtDate.Text = stringFormatValue;

                    if (!string.IsNullOrEmpty(txtDate.Mask))
                    {
                        partDic = new Dictionary<int, DatePart>();
                        string formatMask = txtDate.Mask + "X";

                        int startInx = -1, length = 0;
                        for (int i = 0; i < formatMask.Length; i++)
                        {
                            if (startInx == -1 && formatMask[i] == '0')
                            {
                                startInx = i;
                                length = 1;
                            }
                            else
                            {
                                if (startInx > -1)
                                {
                                    if (formatMask[i] == '0')
                                    {
                                        length++;
                                    }
                                    else
                                    {
                                        DatePart.DatePartType selectPart = DatePart.DatePartType.Day;
                                        switch (DatepickerType)
                                        {
                                            case LGCDataPickerType.Year:
                                                stringFormatValue = "YYYY";
                                                break;
                                            case LGCDataPickerType.Month:
                                                if (currentCulture.DateTimeFormat.YearMonthPattern.Contains("MMMM"))
                                                {
                                                    stringFormatValue = currentCulture.DateTimeFormat.YearMonthPattern.Replace("MMMM", "MM").ToUpper();
                                                }
                                                else
                                                {
                                                    stringFormatValue = currentCulture.DateTimeFormat.YearMonthPattern.Replace("M", "MM").ToUpper();
                                                }
                                                break;
                                            case LGCDataPickerType.Date:
                                                stringFormatValue = currentCulture.DateTimeFormat.ShortDatePattern.ToUpper();
                                                if (!stringFormatValue.Contains("MM")) stringFormatValue = stringFormatValue.Replace("M", "MM");
                                                if (!stringFormatValue.Contains("DD")) stringFormatValue = stringFormatValue.Replace("D", "DD");
                                                break;
                                        }

                                        switch (stringFormatValue.Replace("'", "").Substring(startInx, length))
                                        {
                                            case "YYYY":
                                            case "YY":
                                                selectPart = DatePart.DatePartType.Year;
                                                break;
                                            case "MMMM":
                                            case "MMM":
                                            case "MM":
                                            case "M":
                                                selectPart = DatePart.DatePartType.Month;
                                                break;
                                            case "DD":
                                            case "D":
                                                selectPart = DatePart.DatePartType.Day;
                                                break;
                                        }
                                        DatePart datePart = new DatePart(selectPart, startInx, length);
                                        partDic.Add(partDic.Count, datePart);

                                        startInx = -1; length = 0;
                                    }
                                }
                                else
                                {
                                    startInx = -1; length = 0;
                                }
                            }
                        }
                        selectPart = 0;
                    }
                }
            }
            catch { txtDate.Text = ""; }
        }

        protected void SetDateText(object date)
        {
            try
            {
                if (date is DateTime)
                {
                    DateTime dateTime = (DateTime)date;

                    string dateSeparator = currentCulture.DateTimeFormat.DateSeparator;
                    string stringFormatValue = currentCulture.DateTimeFormat.ShortDatePattern.ToUpper();

                    switch (DatepickerType)
                    {
                        case LGCDataPickerType.Year:
                            stringFormatValue = dateTime.ToString("yyyy");
                            break;
                        case LGCDataPickerType.Month:
                            if (txtDate.IsFocused)
                            {
                                if (currentCulture.DateTimeFormat.YearMonthPattern.Contains("MMMM"))
                                {
                                    stringFormatValue = dateTime.ToString(currentCulture.DateTimeFormat.YearMonthPattern.Replace("MMMM", "MM"), currentCulture);
                                }
                                else
                                {
                                    stringFormatValue = dateTime.ToString(currentCulture.DateTimeFormat.YearMonthPattern.Replace("M", "MM"), currentCulture);
                                }
                            }
                            else
                            {
                                stringFormatValue = dateTime.ToString(currentCulture.DateTimeFormat.YearMonthPattern, currentCulture);
                            }

                            if (txtDate.IsFocused)
                            {
                                stringFormatValue = stringFormatValue.Replace(" ", currentCulture.DateTimeFormat.DateSeparator);
                            }
                            break;
                        case LGCDataPickerType.Date:
                            if (txtDate.IsFocused)
                            {
                                string datePattern = currentCulture.DateTimeFormat.ShortDatePattern;
                                if (!datePattern.Contains("MM")) datePattern = datePattern.Replace("M", "MM");
                                if (!datePattern.Contains("dd")) datePattern = datePattern.Replace("d", "dd");
                                stringFormatValue = dateTime.ToString(datePattern, currentCulture);
                            }
                            else
                            {
                                stringFormatValue = dateTime.ToString(currentCulture.DateTimeFormat.ShortDatePattern, currentCulture);
                            }
                            break;
                    }
                    
                    // 날짜 구분자를 - 로 통일
                    txtDate.Text = stringFormatValue.Replace(dateSeparator, "-");
                }
                else if (date is string)
                {
                    if (date.ToString().Contains("_"))
                    {
                        SetDateText(this.SelectedDateTime);
                        return;
                    }

                    DateTime dateTime;

                    switch (DatepickerType)
                    {
                        case LGCDataPickerType.Year:
                            dateTime = new DateTime(Convert.ToInt16(date), 1, 1);
                            if (dateTime.Year >= 1900 && dateTime.Year <= 9900)
                            {
                                if (this.SelectedDateTime != dateTime) SelectedDateTime = dateTime;
                            }
                            else
                            {
                                SetDateText(this.SelectedDateTime);
                            }
                            break;
                        case LGCDataPickerType.Month:
                        case LGCDataPickerType.Date:
                            if (DateTime.TryParse(date.ToString(), currentCulture, System.Globalization.DateTimeStyles.None, out dateTime))
                            {
                                if (this.SelectedDateTime != dateTime)
                                {
                                    if (txtDate.Text.Length == txtDate.SelectionLength)
                                    {
                                        SelectedDateTime = dateTime;
                                        txtDate.SelectAll();
                                    }
                                    else
                                    {
                                        SelectedDateTime = dateTime;
                                    }
                                }
                            }
                            else
                            {
                                SetDateText(this.SelectedDateTime);
                            }
                            break;
                    }
                }
            }
            catch { txtDate.Text = ""; }
        }

        private void SelectPartLeft()
        {
            try
            {
                if (this.IsReadOnly) return;

                foreach (KeyValuePair<int, DatePart> item in partDic)
                {
                    if (txtDate.SelectionStart >= item.Value.StartIndex &&
                        txtDate.SelectionStart <= item.Value.StartIndex + item.Value.Length)
                    {
                        selectPart = item.Key;
                        break;
                    }
                }

                selectPart--;
                if (selectPart < 0) selectPart = 0;

                this.Dispatcher.BeginInvoke(new Action(() => txtDate.Select(partDic[selectPart].StartIndex, partDic[selectPart].Length)));
            }
            catch { txtDate.Text = ""; }
        }

        private void SelectPartRight()
        {
            try
            {
                if (this.IsReadOnly) return;

                foreach (KeyValuePair<int, DatePart> item in partDic)
                {
                    if (txtDate.SelectionStart >= item.Value.StartIndex &&
                        txtDate.SelectionStart <= item.Value.StartIndex + item.Value.Length)
                    {
                        selectPart = item.Key;
                        break;
                    }
                }

                selectPart++;
                if (selectPart >= partDic.Count) selectPart = partDic.Count - 1;

                this.Dispatcher.BeginInvoke(new Action(() => txtDate.Select(partDic[selectPart].StartIndex, partDic[selectPart].Length)));
            }
            catch { txtDate.Text = ""; }
        }

        private void SelectPartUp()
        {
            try
            {
                if (this.IsReadOnly) return;
                if (string.IsNullOrEmpty(txtDate.Mask)) return;

                SetDateText(txtDate.Text);

                DatePart.DatePartType part = DatePart.DatePartType.Day;
                switch (DatepickerType)
                {
                    case LGCDataPickerType.Year:
                        part = DatePart.DatePartType.Year;
                        break;
                    case LGCDataPickerType.Month:
                        part = DatePart.DatePartType.Month;
                        break;
                    case LGCDataPickerType.Date:
                        part = DatePart.DatePartType.Day;
                        break;
                }

                if (!txtDate.SelectionLength.Equals(txtDate.Mask.Length))
                {
                    foreach (KeyValuePair<int, DatePart> item in partDic)
                    {
                        if (txtDate.SelectionStart >= item.Value.StartIndex &&
                            txtDate.SelectionStart <= item.Value.StartIndex + item.Value.Length)
                        {
                            part = item.Value.SelectDatePart;
                            break;
                        }
                    }
                }

                switch (part)
                {
                    case DatePart.DatePartType.Year:
                        if (SelectedDateTime.Year < DateTime.MaxValue.Year) SelectedDateTime = SelectedDateTime.AddYears(1);
                        break;
                    case DatePart.DatePartType.Month:
                        if (SelectedDateTime.Year < DateTime.MaxValue.Year || SelectedDateTime.Month < DateTime.MaxValue.Month)
                        {
                            SelectedDateTime = SelectedDateTime.AddMonths(1);
                        }
                        break;
                    case DatePart.DatePartType.Day:
                        if (SelectedDateTime.Year < DateTime.MaxValue.Year || SelectedDateTime.Month < DateTime.MaxValue.Month || SelectedDateTime.Day < DateTime.MaxValue.Day)
                        {
                            SelectedDateTime = SelectedDateTime.AddDays(1);
                        }
                        break;
                }

                foreach (KeyValuePair<int, DatePart> item in partDic)
                {
                    if (part == item.Value.SelectDatePart)
                    {
                        selectPart = item.Key;
                        this.Dispatcher.BeginInvoke(new Action(() => txtDate.Select(item.Value.StartIndex, item.Value.Length)));
                        break;
                    }
                }
            }
            catch { txtDate.Text = ""; }
        }

        private void SelectPartDown()
        {
            try
            {
                if (this.IsReadOnly) return;
                if (string.IsNullOrEmpty(txtDate.Mask)) return;

                SetDateText(txtDate.Text);

                DatePart.DatePartType part = DatePart.DatePartType.Day;
                switch (DatepickerType)
                {
                    case LGCDataPickerType.Year:
                        part = DatePart.DatePartType.Year;
                        break;
                    case LGCDataPickerType.Month:
                        part = DatePart.DatePartType.Month;
                        break;
                    case LGCDataPickerType.Date:
                        part = DatePart.DatePartType.Day;
                        break;
                }

                if (!txtDate.SelectionLength.Equals(txtDate.Mask.Length))
                {
                    foreach (KeyValuePair<int, DatePart> item in partDic)
                    {
                        if (txtDate.SelectionStart >= item.Value.StartIndex &&
                            txtDate.SelectionStart <= item.Value.StartIndex + item.Value.Length)
                        {
                            part = item.Value.SelectDatePart;
                            break;
                        }
                    }
                }

                switch (part)
                {
                    case DatePart.DatePartType.Year:
                        if (SelectedDateTime.Year > DateTime.MinValue.Year) SelectedDateTime = SelectedDateTime.AddYears(-1);
                        break;
                    case DatePart.DatePartType.Month:
                        if (SelectedDateTime.Year > DateTime.MinValue.Year || SelectedDateTime.Month > DateTime.MinValue.Month)
                        {
                            SelectedDateTime = SelectedDateTime.AddMonths(-1);
                        }
                        break;
                    case DatePart.DatePartType.Day:
                        if (SelectedDateTime.Year > DateTime.MinValue.Year || SelectedDateTime.Month > DateTime.MinValue.Month || SelectedDateTime.Day > DateTime.MinValue.Day)
                        {
                            SelectedDateTime = SelectedDateTime.AddDays(-1);
                        }
                        break;
                }

                foreach (KeyValuePair<int, DatePart> item in partDic)
                {
                    if (part == item.Value.SelectDatePart)
                    {
                        selectPart = item.Key;
                        this.Dispatcher.BeginInvoke(new Action(() => txtDate.Select(item.Value.StartIndex, item.Value.Length)));
                        break;
                    }
                }
            }
            catch { txtDate.Text = ""; }
        }
                
        private void MoveNextFocusable(DependencyObject baseObject)
        { 
            if (baseObject == null) return;

            bool retVal = true;
            UIElement element = Keyboard.FocusedElement as UIElement;
            Control control = element as Control;
            while (retVal && element != null && control != null)
            {
                retVal = element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                element = Keyboard.FocusedElement as UIElement;
                if (element is Button || element is Grid || element is StackPanel) continue;
                if (element is C1TabItem || element is MenuItem) break;

                if (element != null)
                {
                    control = element as Control;
                    if (control != null)
                    {
                        if (control.Focusable) break;
                    }
                }
            }
            return;
        }

        #endregion
    }
}