/*************************************************************************************
 Created Date : 2023.02.27
      Creator : 
   Decription : UcBaseDateTimePicker Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.27  조영대 : Initial Created. 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Controls
{

    public partial class UcBaseDateTimePicker : UserControl
    {
        #region Internal Enum & Class 
        public enum SelectedTargetType
        {
            FromDate, 
            ToDate,
            FromTime,
            ToTime
        }
        public class DateTimeChangedEventArgs : EventArgs
        {
            public DateTimeChangedEventArgs(SelectedTargetType selectedTarget, DateTime fromDateTime, DateTime toDateTime)
            {
                SelectedTarget = selectedTarget;
                SelectedFromDate = fromDateTime;
                SelectedToDate = toDateTime;
            }

            public SelectedTargetType SelectedTarget { get; set; }
            public DateTime SelectedFromDate { get; set; }
            public DateTime SelectedToDate { get; set; }
        }
        #endregion

        #region EventHandler
        public delegate void DateTimeChangedEventHandler(object sender, DateTimeChangedEventArgs e);

        public event DateTimeChangedEventHandler DateTimeChanged;
        #endregion

        #region Property
        [Category("GMES"), Browsable(true), DefaultValue("조회일자"), Description("조회일자")]        
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(UcBaseDateTimePicker), new PropertyMetadata("조회일자", TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseDateTimePicker ucBaseDateTimePicker = d as UcBaseDateTimePicker;
            ucBaseDateTimePicker.txtLabel.Text = (string)e.NewValue;
            if (string.IsNullOrEmpty(ucBaseDateTimePicker.txtLabel.Text.Trim()))
            {
                ucBaseDateTimePicker.txtLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ucBaseDateTimePicker.txtLabel.Visibility = Visibility.Visible;
            }
            ucBaseDateTimePicker.InvalidateVisual();
        }

        private bool isFromTo = false;
        [Category("GMES"), Browsable(true), DefaultValue(false), Description("From To 사용여부")]
        public bool IsFromTo
        {
            get
            {
                return isFromTo;
            }
            set
            {
                isFromTo = value;

                if (isFromTo)
                {
                    txtTild.Visibility = Visibility.Visible;
                    dtpToDate.Visibility = Visibility.Visible;
                    if (isTimeView) dtpToTime.Visibility = Visibility.Visible;                    
                }
                else
                {
                    txtTild.Visibility = Visibility.Collapsed;
                    dtpToDate.Visibility = Visibility.Collapsed;
                    dtpToTime.Visibility = Visibility.Collapsed;
                }

                tglFromTo.OnOff = isFromTo;
            }
        }

        private bool isFromToToggleView = true;
        [Category("GMES"), Browsable(true), DefaultValue(true), Description("From To 사용여부")]
        public bool IsFromToToggleView
        {
            get
            {
                return isFromToToggleView;
            }
            set
            {
                isFromToToggleView = value;

                if (isFromToToggleView)
                {
                    tglFromTo.Visibility = Visibility.Visible;
                }
                else
                {
                    tglFromTo.Visibility = Visibility.Collapsed;
                }
            }
        }


        private bool isTimeView = false;
        [Category("GMES"), Browsable(true), DefaultValue(false), Description("Time 보기여부")]
        public bool IsTimeView
        {
            get
            {
                return isTimeView;
            }
            set
            {
                isTimeView = value;
                if (isTimeView)
                {
                    dtpFromTime.Visibility = Visibility.Visible;
                    if (isFromTo) dtpToTime.Visibility = Visibility.Visible;
                }
                else
                {
                    dtpFromTime.Visibility = Visibility.Collapsed;
                    dtpToTime.Visibility = Visibility.Collapsed;
                }
            }
        }

        private LGCDataPickerType dateFormatType = LGCDataPickerType.Date;
        [Category("GMES"), Browsable(true), DefaultValue(LGCDataPickerType.Date), Description("Date Type")]
        public LGCDataPickerType DateFormatType
        {
            get { return dateFormatType; }
            set
            {
                dateFormatType = value;
                dtpFromDate.DatepickerType = dateFormatType;
                dtpToDate.DatepickerType = dateFormatType;
                this.InvalidateVisual();
            }
        }
   
        private string timeFormat = "HH:mm";
        [Category("GMES"), Browsable(true), DefaultValue("HH:mm"), Description("Time Format")]
        public string TimeFormat
        {
            get { return timeFormat; }
            set 
            {
                timeFormat = value;
                dtpFromTime.CustomTimeFormat = timeFormat;
                dtpToTime.CustomTimeFormat = timeFormat;
                this.InvalidateVisual();
            }
        }

        private DateTime selectedFromDateTime = DateTime.Now;
        [Category("GMES"), Browsable(false), Description("Selected FromDate")]
        public DateTime SelectedFromDateTime
        {
            get
            {
                DateTime datetime = (DateTime)GetValue(SelectedFromDateTimeProperty);
                if (!IsTimeView)
                {
                    datetime = new DateTime(datetime.Year, datetime.Month, datetime.Day, 0, 0, 0, 0);
                }
                else
                {
                    datetime = new DateTime(datetime.Year, datetime.Month, datetime.Day, dtpFromTime.Hour, dtpFromTime.Minute, dtpFromTime.Second);
                }

                return datetime;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    dtpFromDate.SelectedDateTime = selectedFromDateTime;
                }
                else
                {
                    SetValue(SelectedFromDateTimeProperty, value);
                }
            }
        }
        public static readonly DependencyProperty SelectedFromDateTimeProperty =
            DependencyProperty.Register("SelectedFromDateTime", typeof(DateTime), typeof(UcBaseDateTimePicker), new PropertyMetadata(DateTime.Now, SelectedFromDateTimePropertyChangedCallback));

        private static void SelectedFromDateTimePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {   
            UcBaseDateTimePicker ucBaseDateTimePicker = d as UcBaseDateTimePicker;
            if (!ucBaseDateTimePicker.isSelectedDateChanged) return;

            ucBaseDateTimePicker.selectedFromDateTime = (DateTime)e.NewValue;

            ucBaseDateTimePicker.dtpFromDate.SelectedDateTime = ucBaseDateTimePicker.selectedFromDateTime;
            ucBaseDateTimePicker.dtpFromTime.SelectedDateTime = ucBaseDateTimePicker.selectedFromDateTime;
        }

        private DateTime selectedToDateTime = DateTime.Now;
        [Category("GMES"), Browsable(false), Description("Selected ToDate")]
        public DateTime SelectedToDateTime
        {
            get
            {
                DateTime datetime = (DateTime)GetValue(SelectedToDateTimeProperty);
                if (isFromTo)
                {
                    if (!IsTimeView)
                    {
                        datetime = datetime.Date.AddDays(1).AddMilliseconds(-1);
                    }
                    else
                    {
                        datetime = new DateTime(datetime.Year, datetime.Month, datetime.Day, dtpToTime.Hour, dtpToTime.Minute, dtpToTime.Second);
                    }
                }
                else
                {
                    datetime = SelectedFromDateTime;
                }
                return datetime;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    dtpToDate.SelectedDateTime = selectedToDateTime;
                }
                else
                {
                    SetValue(SelectedToDateTimeProperty, value);
                }
                
            }
        }
        public static readonly DependencyProperty SelectedToDateTimeProperty =
            DependencyProperty.Register("SelectedToDateTime", typeof(DateTime), typeof(UcBaseDateTimePicker), new PropertyMetadata(DateTime.Now, SelectedToDateTimePropertyChangedCallback));

        private static void SelectedToDateTimePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseDateTimePicker ucBaseDateTimePicker = d as UcBaseDateTimePicker;
            if (!ucBaseDateTimePicker.isSelectedDateChanged) return;

            ucBaseDateTimePicker.selectedToDateTime = (DateTime)e.NewValue;

            ucBaseDateTimePicker.dtpToDate.SelectedDateTime = ucBaseDateTimePicker.selectedToDateTime;
            ucBaseDateTimePicker.dtpToTime.SelectedDateTime = ucBaseDateTimePicker.selectedToDateTime;
        }

        #endregion

        private System.Timers.Timer validationTimer;
        private ToolTip validationToolTip = null;
        private System.Windows.Controls.Primitives.Popup popupFrom = null;
        private System.Windows.Controls.Primitives.Popup popupTo = null;

        #region Constructor & Initialize

        private bool isSelectedDateChanged = true;

        public UcBaseDateTimePicker()
        {
            InitializeComponent();

            InitializeControls();
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
        }
        #endregion

        #region Event
        private void dtpFromDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSelectedDateChanged) return;

            isSelectedDateChanged = false;
            if (dtpFromDate.SelectedDateTime == DateTime.MinValue) dtpFromDate.SelectedDateTime = DateTime.Now;
            if (dtpToDate.SelectedDateTime == DateTime.MinValue) dtpToDate.SelectedDateTime = DateTime.Now;

            if (dtpFromDate.SelectedDateTime > dtpToDate.SelectedDateTime)
            {
                dtpToDate.SelectedDateTime = dtpFromDate.SelectedDateTime;
                SelectedToDateTime = GetDateTimeCombination(dtpToDate, dtpToTime);
            }
            SelectedFromDateTime = GetDateTimeCombination(dtpFromDate, dtpFromTime);
            isSelectedDateChanged = true;

            if (popupFrom == null)
            {
                popupFrom = dtpFromDate.FindChild<System.Windows.Controls.Primitives.Popup>("CalendarPopUp");
                if (popupFrom != null)
                {
                    popupFrom.Closed += PopupFrom_Closed;
                }
            }

            if (!popupFrom.IsOpen)
            {
                DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.FromDate, SelectedFromDateTime, SelectedToDateTime));
            }
        }

        private void PopupFrom_Closed(object sender, EventArgs e)
        {
            DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.FromDate, SelectedFromDateTime, SelectedToDateTime));
        }

        private void dtpFromTime_SelectionCommitted(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (!isSelectedDateChanged) return;

            isSelectedDateChanged = false;
            SelectedFromDateTime = GetDateTimeCombination(dtpFromDate, dtpFromTime);
            isSelectedDateChanged = true;

            if (popupFrom == null)
            {
                popupFrom = dtpFromDate.FindChild<System.Windows.Controls.Primitives.Popup>("CalendarPopUp");
                if (popupFrom != null)
                {
                    popupFrom.Closed += PopupFrom_Closed;
                }
            }
            DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.FromTime, SelectedFromDateTime, SelectedToDateTime));

        }

        private void dtpToDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSelectedDateChanged) return;

            isSelectedDateChanged = false;

            if (dtpFromDate.SelectedDateTime == DateTime.MinValue) dtpFromDate.SelectedDateTime = DateTime.Now;
            if (dtpToDate.SelectedDateTime == DateTime.MinValue) dtpToDate.SelectedDateTime = DateTime.Now;

            if (dtpToDate.SelectedDateTime < dtpFromDate.SelectedDateTime)
            {
                dtpFromDate.SelectedDateTime = dtpToDate.SelectedDateTime;
                SelectedFromDateTime = GetDateTimeCombination(dtpFromDate, dtpFromTime);
            }
            SelectedToDateTime = GetDateTimeCombination(dtpToDate, dtpToTime);
            isSelectedDateChanged = true;

            if (popupTo == null)
            {
                popupTo = dtpToDate.FindChild<System.Windows.Controls.Primitives.Popup>("CalendarPopUp");
                if (popupTo != null)
                {
                    popupTo.Closed += PopupTo_Closed;
                }
            }
            DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.ToDate, SelectedFromDateTime, SelectedToDateTime));
        }

        private void PopupTo_Closed(object sender, EventArgs e)
        {
            DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.ToDate, SelectedFromDateTime, SelectedToDateTime));
        }

        private void dtpToTime_SelectionCommitted(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (!isSelectedDateChanged) return;

            isSelectedDateChanged = false;
            SelectedToDateTime = GetDateTimeCombination(dtpToDate, dtpToTime);

            if (popupFrom == null) popupFrom = dtpFromDate.FindChild<System.Windows.Controls.Primitives.Popup>("CalendarPopUp");

            DateTimeChanged?.Invoke(this, new DateTimeChangedEventArgs(SelectedTargetType.ToTime, SelectedFromDateTime, SelectedToDateTime));

            isSelectedDateChanged = true;
        }

        private void tglFromTo_OnOffValueChanged(object sender, bool onoff)
        {
            this.IsFromTo = onoff;
        }

        #region Validation 관련
        public void SetValidation(string messageID, params object[] parameters)
        {
            SetValidation(MessageDic.Instance.GetMessage(messageID, parameters));
        }

        public void SetValidation(string message)
        {
            try
            {
                if (validationToolTip == null)
                {
                    validationToolTip = new ToolTip();
                    validationToolTip.PlacementTarget = this;
                    validationToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                }

                if (validationTimer == null)
                {
                    validationTimer = new System.Timers.Timer(5000);
                    validationTimer.Elapsed += validationTimer_Elapsed;
                    validationTimer.AutoReset = true;
                }
                validationTimer.Stop();

                if (!string.IsNullOrEmpty(message))
                {
                    string convertMessage = MessageDic.Instance.GetMessage(message).Replace("[#]", "").Trim();

                    validationToolTip.Content = convertMessage;

                    this.ToolTip = validationToolTip;

                    if (!validationToolTip.IsOpen)
                    {
                        validationToolTip.IsOpen = true;
                        if (this.ActualHeight > 30)
                        {
                            validationToolTip.HorizontalOffset = (this.ActualWidth - validationToolTip.ActualWidth) / 2 * -1 + 50;
                            validationToolTip.VerticalOffset = 0;
                        }
                        else
                        {
                            validationToolTip.HorizontalOffset = (this.ActualWidth - validationToolTip.ActualWidth) / 2 * -1;
                            validationToolTip.VerticalOffset = (this.ActualHeight / 2) + (validationToolTip.ActualHeight / 2) + 1;
                        }
                        validationTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void validationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                validationTimer?.Stop();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (this.ToolTip is ToolTip)
                    {
                        ToolTip tp = this.ToolTip as ToolTip;
                        if (tp.IsOpen) tp.IsOpen = false;

                        this.ToolTip = null;
                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region Method
        private DateTime GetDateTimeCombination(LGCDatePicker date, UcBaseTimePicker time)
        {
            return new DateTime(date.SelectedDateTime.Year, date.SelectedDateTime.Month, date.SelectedDateTime.Day, time.Hour, time.Minute, time.Second);
        }
        #endregion

    }

}
