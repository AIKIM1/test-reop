/*************************************************************************************
 Created Date : 2022.09.05
      Creator : 
   Decription : TimePicker Extension
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.05  조영대 : Initial Created. 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseTimePicker : C1.WPF.DateTimeEditors.C1DateTimePicker
    {
        #region Property

        private DateTime selectedDateTime = System.DateTime.Now;
        [Category("GMES"), Browsable(false), Description("Selected DateTime")]
        public DateTime SelectedDateTime
        {
            get
            {
                DateTime datetime = (DateTime)GetValue(SelectedDateTimeProperty);

                datetime = new DateTime(System.DateTime.MinValue.Year, System.DateTime.MinValue.Month, System.DateTime.MinValue.Day, datetime.Hour, datetime.Minute, datetime.Second, 0);

                return datetime;
            }
            set
            {
                SetValue(SelectedDateTimeProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register("SelectedDateTime", typeof(DateTime), typeof(UcBaseTimePicker), new PropertyMetadata(System.DateTime.Now, SelectedDateTimePropertyChangedCallback));

        private static void SelectedDateTimePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseTimePicker ucBaseTimePicker = d as UcBaseTimePicker;
            ucBaseTimePicker.selectedDateTime = (DateTime)e.NewValue;
            ucBaseTimePicker.DateTime = ucBaseTimePicker.selectedDateTime;
            ucBaseTimePicker.InvalidateVisual();
        }

        public int Hour 
        {
            get
            {
                if (this.DateTime == null)
                {
                    this.DateTime = System.DateTime.MinValue;
                }
                return this.DateTime.Value.Hour; 
            } 
        }

        public int Minute 
        {
            get
            {
                if (this.DateTime == null)
                {
                    this.DateTime = System.DateTime.MinValue;
                }
                return this.DateTime.Value.Minute; 
            }
        }

        public int Second
        {
            get
            {
                if (this.DateTime == null)
                {
                    this.DateTime = System.DateTime.MinValue;
                }
                return this.DateTime.Value.Second;
            }
        }

        #endregion

        #region Declaration & Constructor 

        private ContextMenu selectMenu = null;
        private TextBox timeTextBox = null;

        private string saveFormat = string.Empty;

        private int hourIndex = 0;
        private int minIndex = 0;
        private int secIndex = 0;

        private int showCount = 7;
        private int modeStep = 0;


        public void InitializeControls()
        {
            try
            {
                EditMode = C1.WPF.DateTimeEditors.C1DateTimePickerEditMode.Time;

                if (selectMenu == null)
                {
                    selectMenu = new ContextMenu();
                    selectMenu.PlacementTarget = this;
                    selectMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
                    selectMenu.Width = 80;

                    for (int inx = 0; inx < showCount; inx++)
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.Name = "SelectMenu" + inx.ToString();
                        menuItem.Icon = "T";
                        menuItem.Click += MenuItem_Click;
                        menuItem.MouseWheel += MenuItem_MouseWheel;
                        selectMenu.Items.Add(menuItem);
                    }
                }
                this.ContextMenu = selectMenu;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region Override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            try
            {
                if (e.Key == Key.Enter)
                {
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

                    if (elementWithFocus != null)
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            try
            {
                if (Util.NVC(e.OriginalSource).Contains("TextBoxView"))
                {
                    if (timeTextBox == null)
                    {
                        timeTextBox = this.FindChild<TextBox>("");
                    }

                    e.Handled = true;

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Focus();
                        if (timeTextBox != null)
                        {
                            timeTextBox.Focus();
                            timeTextBox.SelectAll();
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            try
            {
                if (EditMode.Equals(C1.WPF.DateTimeEditors.C1DateTimePickerEditMode.Time))
                {
                    if (CustomTimeFormat.Contains("HH") || CustomTimeFormat.Contains("hh"))
                    {
                        modeStep = 0;

                        hourIndex = DateTime.Value.Hour - (showCount / 2);
                        if (hourIndex < 0) hourIndex = 0;
                        if (hourIndex + showCount > 24) hourIndex = 24 - showCount;

                        DisplayHour();
                    }
                    else if (CustomTimeFormat.Contains("mm"))
                    {
                        modeStep = 1;

                        minIndex = DateTime.Value.Minute - (showCount / 2);
                        if (minIndex < 0) minIndex = 0;
                        if (minIndex + showCount > 60) minIndex = 60 - showCount;

                        DisplayMinute();
                    }
                    else if (CustomTimeFormat.Contains("ss"))
                    {
                        modeStep = 2;

                        secIndex = DateTime.Value.Second - (showCount / 2);
                        if (secIndex < 0) secIndex = 0;
                        if (secIndex + showCount > 60) secIndex = 60 - showCount;

                        DisplaySecond();
                    }

                    this.ContextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            try
            {
                saveFormat = this.CustomTimeFormat;

                this.CustomTimeFormat = this.CustomTimeFormat.Replace(":", "");

                if (timeTextBox == null)
                {
                    timeTextBox = this.FindChild<TextBox>("");
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (timeTextBox != null)
                    {
                        timeTextBox.Focus();
                        timeTextBox.SelectAll();
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            try
            {
                if (timeTextBox != null)
                {
                    string timeTokens = timeTextBox.Text.Replace(":", "").Trim();
                    if (timeTokens.Length == saveFormat.Replace(":", "").Length)
                    {
                        DateTime timeValue;
                        if (System.DateTime.TryParseExact(timeTokens, saveFormat.Replace(":", ""), null, System.Globalization.DateTimeStyles.None, out timeValue))
                        {
                            this.SelectedDateTime = new DateTime(this.DateTime.Value.Year, this.DateTime.Value.Month, this.DateTime.Value.Day,
                                timeValue.Hour, timeValue.Minute, timeValue.Second, timeValue.Millisecond);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.CustomTimeFormat = saveFormat;
            }
        }
        #endregion

        #region Event
        private void MenuItem_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (e.Delta > 0)
                {
                    switch (modeStep)
                    {
                        case 0:
                            hourIndex--;
                            if (hourIndex < 0) hourIndex = 0;
                            if (hourIndex + showCount > 24) hourIndex = 24 - showCount;
                            DisplayHour();
                            break;
                        case 1:
                            minIndex--;
                            if (minIndex < 0) minIndex = 0;
                            if (minIndex + showCount > 60) minIndex = 60 - showCount;
                            DisplayMinute();
                            break;
                        case 2:
                            secIndex--;
                            if (secIndex < 0) secIndex = 0;
                            if (secIndex + showCount > 60) secIndex = 60 - showCount;

                            DisplaySecond();
                            break;
                    }
                }
                else if (e.Delta < 0)
                {
                    switch (modeStep)
                    {
                        case 0:
                            hourIndex++;
                            if (hourIndex + showCount > 24) hourIndex = 24 - showCount;
                            DisplayHour();
                            break;
                        case 1:
                            minIndex++;
                            if (minIndex + showCount > 60) minIndex = 60 - showCount;
                            DisplayMinute();
                            break;
                        case 2:
                            secIndex++;
                            if (secIndex + showCount > 60) secIndex = 60 - showCount;
                            DisplaySecond();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (modeStep++)
                {
                    case 0:
                        if (CustomTimeFormat.Contains("HH") || CustomTimeFormat.Contains("hh"))
                        {
                            int hour = Convert.ToInt16((sender as MenuItem).Header);

                            this.DateTime = new System.DateTime(DateTime.Value.Year, DateTime.Value.Month, DateTime.Value.Day,
                                                                hour, DateTime.Value.Minute, DateTime.Value.Second);
                            if (CustomTimeFormat.Contains("mm"))
                            {
                                minIndex = DateTime.Value.Minute - (showCount / 2);
                                if (minIndex < 0) minIndex = 0;
                                if (minIndex + showCount > 60) minIndex = 60 - showCount;
                                DisplayMinute();
                                this.ContextMenu.IsOpen = true;
                            }
                            else
                            {
                                this.ContextMenu.IsOpen = false;
                            }
                        }
                        break;
                    case 1:
                        if (CustomTimeFormat.Contains("mm"))
                        {
                            int minute = Convert.ToInt16((sender as MenuItem).Header);

                            this.DateTime = new System.DateTime(DateTime.Value.Year, DateTime.Value.Month, DateTime.Value.Day,
                                                                DateTime.Value.Hour, minute, DateTime.Value.Second);
                            if (CustomTimeFormat.Contains("ss"))
                            {
                                secIndex = DateTime.Value.Second - (showCount / 2);
                                if (secIndex < 0) secIndex = 0;
                                if (secIndex + showCount > 60) secIndex = 60 - showCount;

                                DisplaySecond();

                                this.ContextMenu.IsOpen = true;
                            }
                            else
                            {
                                this.ContextMenu.IsOpen = false;
                            }
                        }
                        break;
                    case 2:
                        if (CustomTimeFormat.Contains("ss"))
                        {
                            int second = Convert.ToInt16((sender as MenuItem).Header);

                            this.DateTime = new System.DateTime(DateTime.Value.Year, DateTime.Value.Month, DateTime.Value.Day,
                                                                DateTime.Value.Hour, DateTime.Value.Minute, second);
                        }
                        this.ContextMenu.IsOpen = false;
                        break;
                    default:
                        this.ContextMenu.IsOpen = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private void DisplayHour()
        {
            try
            {
                for (int inx = hourIndex; inx < hourIndex + showCount; inx++)
                {
                    MenuItem menuItem = selectMenu.Items[inx - hourIndex] as MenuItem;
                    menuItem.Header = inx.ToString("00");
                    
                    if (this.DateTime.Value.Hour.Equals(inx))
                    {
                        menuItem.FontWeight = FontWeights.Bold;
                        menuItem.Icon = "▶";
                    }
                    else
                    {
                        menuItem.FontWeight = FontWeights.Normal;
                        menuItem.Icon = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DisplayMinute()
        {
            try
            {
                for (int inx = minIndex; inx < minIndex + showCount; inx++)
                {
                    MenuItem menuItem = selectMenu.Items[inx - minIndex] as MenuItem;
                    menuItem.Header = inx.ToString("00");

                    if (this.DateTime.Value.Minute.Equals(inx))
                    {
                        menuItem.FontWeight = FontWeights.Bold;
                        menuItem.Icon = "▶";
                    }
                    else
                    {
                        menuItem.FontWeight = FontWeights.Normal;
                        menuItem.Icon = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DisplaySecond()
        {
            try
            {
                for (int inx = secIndex; inx < secIndex + showCount; inx++)
                {
                    MenuItem menuItem = selectMenu.Items[inx - secIndex] as MenuItem;
                    menuItem.Header = inx.ToString("00");

                    if (this.DateTime.Value.Second.Equals(inx))
                    {
                        menuItem.FontWeight = FontWeights.Bold;
                        menuItem.Icon = "▶";
                    }
                    else
                    {
                        menuItem.FontWeight = FontWeights.Normal;
                        menuItem.Icon = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
