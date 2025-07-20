/*************************************************************************************
 Created Date : 2021.10.20
      Creator : 
   Decription : TextBox Extension
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.20  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseTextBox : TextBox, IControlValidation
    {
        #region EventHandler
        public event ClipboardPastedEventHandler ClipboardPasted;
        public delegate void ClipboardPastedEventHandler(object sender, DataObjectPastingEventArgs e, string text);
        #endregion

        #region Property

        private bool isClearButton = true;
        [Category("GMES"), DefaultValue(true), Description("Clear Button 보기 유무")]
        public bool IsClearButton
        {
            get { return isClearButton; }
            set { isClearButton = value; }
        }

        private bool isPasteCommaConvert = false;
        [Category("GMES"), DefaultValue(false), Description("true 일때 붙여넣기 시 콤마문자열화 합니다.")]
        public bool IsPasteCommaConvert
        {
            get { return isPasteCommaConvert; }
            set
            {
                isPasteCommaConvert = value;

                if (isPasteCommaConvert)
                {
                    WaterText = "CSV";
                    if (!allowSpecialCharacter.Contains(",")) allowSpecialCharacter += ",";
                }                
            }
        }

        private bool isSpecialCharacter = false;
        [Category("GMES"), DefaultValue(false), Description("특수문자 입력 가능 유무")]
        public bool IsSpecialCharacter
        {
            get { return isSpecialCharacter; }
            set { isSpecialCharacter = value; }
        }

        private string allowSpecialCharacter = string.Empty;
        [Category("GMES"), DefaultValue(""), Description("허용 특수문자")]
        public string AllowSpecialCharacter
        {
            get { return allowSpecialCharacter; }
            set 
            {
                allowSpecialCharacter = value;
                if (allowSpecialCharacter != string.Empty)
                {
                    for (int i = 0; i < allowSpecialCharacter.Length; i++)
                    {
                        noSpecialCharactor = noSpecialCharactor.Replace(allowSpecialCharacter[i].ToString(), "");
                    }
                }
            }
        }

        private string waterText = string.Empty;
        [Category("GMES"), DefaultValue(""), Description("Water 마크")]
        public string WaterText
        {
            get { return waterText; }
            set 
            {
                waterText = ObjectDic.Instance.GetObjectName(value);
            }
        }

        private Visibility validationVisibility = Visibility.Collapsed;
        [Category("GMES"), Browsable(false), DefaultValue(Visibility.Collapsed), Description("Validation Visibility")]
        public Visibility ValidationVisibility
        {
            get
            {
                return (Visibility)GetValue(ValidationVisibilityProperty);
            }
            set
            {
                SetValue(ValidationVisibilityProperty, value);
            }
        }
        public static readonly DependencyProperty ValidationVisibilityProperty =
            DependencyProperty.Register("ValidationVisibility", typeof(Visibility), typeof(UcBaseTextBox), new PropertyMetadata(Visibility.Collapsed, ValidationVisibilityPropertyChangedCallback));
         
        private static void ValidationVisibilityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseTextBox ucBaseTextBox = d as UcBaseTextBox;
            ucBaseTextBox.validationVisibility = (Visibility)e.NewValue;
            ucBaseTextBox.InvalidateVisual();
        }
        
        private string text = string.Empty;
        [Category("GMES"), DefaultValue(""), Description("Text")]
        new public string Text
        {
            get 
            {
                if (base.Text.Equals(waterText))
                {
                    return string.Empty;
                }
                else
                {
                    return base.Text.Trim();
                }
            }
            set 
            {
                base.Text = value;

                if (!string.IsNullOrEmpty(waterText) && string.IsNullOrEmpty(value) && !IsFocused)
                {
                    base.Text = waterText;
                    this.Foreground = waterBrush;
                }
                else
                {
                    this.Foreground = forgroundBrush;
                }                
            }
        }

        #endregion

        #region Declaration & Constructor 
        private string noSpecialCharactor = "`-=~!@#$%^&*()_+[]\\{}|;':\",./<>?";
        private System.Timers.Timer validationTimer;
        private ToolTip validationToolTip = null;

        private Style originalStyle = null;

        private SolidColorBrush buttonBrush = new SolidColorBrush(Colors.WhiteSmoke);

        private Brush borderBrush = Brushes.Gray;
        private Brush crossInBrush = Brushes.Red;
        private Brush crossOutBrush = Brushes.Gray;

        private Brush forgroundBrush = Brushes.Black;
        private Brush waterBrush = Brushes.Gainsboro;

        private Pen borderPen;
        private Pen crossPen;

        private int buttonSize = 9;
        private int termSize = 1;

        private bool isButtonVisible = false;
        private bool isButtonIn = false;        

        public UcBaseTextBox()
        {
            this.CharacterCasing = CharacterCasing.Upper;
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (originalStyle == null)
            {
                if (this.Style == null)
                {
                    this.Style = Application.Current.Resources["TextBoxBaseStyle"] as Style;
                }

                originalStyle = this.Style;

                if (!Application.Current.Resources.Contains("UcBaseTextBoxStyle"))
                {
                    ResourceDictionary resourceDic = new ResourceDictionary();
                    resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                    Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                }

                if (Application.Current.Resources.Contains("UcBaseTextBoxStyle"))
                {
                    this.Style = Application.Current.Resources["UcBaseTextBoxStyle"] as Style;
                    this.ApplyTemplate();
                }

                foreach (Setter setter in originalStyle.Setters)
                {
                    switch (setter.Property.ToString())
                    {
                        case "Background":
                        case "Foreground":
                        case "FontSize":
                        case "FontFamily":
                            this.SetValue(setter.Property, setter.Value);
                            break;
                    }
                }
            }

            crossPen = new Pen(crossOutBrush, 1);
            borderPen = new Pen(borderBrush, 1);

            DataObject.AddPastingHandler(this, OnPaste);

            if (!string.IsNullOrEmpty(waterText) && base.Text.Equals(string.Empty) && !IsFocused)
            {
                base.Text = waterText;
                this.Foreground = waterBrush;
            }
        }

        #endregion

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (isClearButton && isButtonVisible && !IsReadOnly && IsEnabled)
            {
                Point pointX = new Point(ActualWidth + termSize, termSize);
                Point pointY = new Point(ActualWidth + termSize + buttonSize, buttonSize + termSize);

                Rect rect = new Rect(pointX, pointY);

                int term2 = 2;

                Point point1 = new Point(rect.Right - term2, rect.Top + term2);
                Point point2 = new Point(rect.Left + term2, rect.Bottom - term2);
                Point point3 = new Point(rect.Left + term2, rect.Top + term2);
                Point point4 = new Point(rect.Right - term2, rect.Bottom - term2);

                drawingContext.DrawRoundedRectangle(buttonBrush, borderPen, rect, buttonSize * 0.15, buttonSize * 0.15);

                drawingContext.DrawLine(crossPen, point1, point2);
                drawingContext.DrawLine(crossPen, point3, point4);
            }
        }

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);

            if (isClearButton && isButtonVisible && isButtonIn)
            {
                ClearValidation();
                Text = string.Empty;
                Tag = null;
                Focus();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            MouseButtonPosition(e);
        }
                
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            
            crossPen = new Pen(crossOutBrush, 1);
            this.Cursor = null;

            isButtonIn = false;
    
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MouseButtonPosition(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!string.IsNullOrEmpty(waterText) && base.Text.Equals(string.Empty) && !IsFocused)
            {
                base.Text = waterText;
                this.Foreground = waterBrush;
            }

            if (isClearButton)
            {
                if (Text.Equals(string.Empty))
                {
                    isButtonVisible = false;
                }
                else
                {
                    isButtonVisible = true;
                }

                InvalidateVisual();
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if (!isSpecialCharacter && !string.IsNullOrEmpty(e.Text))
            {
                if (noSpecialCharactor.Contains(e.Text) &&
                    !allowSpecialCharacter.Contains(e.Text)) e.Handled = true;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (!string.IsNullOrEmpty(waterText) && base.Text.Equals(waterText))
            {
                base.Text = string.Empty;
                this.Foreground = forgroundBrush;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (!string.IsNullOrEmpty(waterText) && base.Text.Equals(string.Empty))
            {
                base.Text = waterText;
                this.Foreground = waterBrush;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!isSpecialCharacter)
            {
                string removeSpecial = Clipboard.GetText();
                if (!string.IsNullOrEmpty(removeSpecial))
                {
                    for (int inx = 0; inx < noSpecialCharactor.Length; inx++)
                    {
                        if (allowSpecialCharacter.Contains(noSpecialCharactor[inx].ToString())) continue;

                        if (removeSpecial.Contains(noSpecialCharactor[inx].ToString()))
                        {
                            removeSpecial = removeSpecial.Replace(noSpecialCharactor[inx].ToString(), "");
                        }
                    }
                    e.CancelCommand();
                    Text = removeSpecial;
                }
            }

            if (isPasteCommaConvert)
            {
                e.CancelCommand();
                PasteRebuildComma();
            }
            this.SelectionStart = this.Text.Length;

            ClipboardPasted?.Invoke(this, e, Text);
        }
        
        #endregion

        #region Event
        private void MouseButtonPosition(MouseEventArgs e)
        {
            bool saveButtonIn = isButtonIn;

            Point pointLeftTop = new Point(ActualWidth + termSize, termSize);
            Point pointRightBottom = new Point(ActualWidth + termSize + buttonSize, buttonSize + termSize);

            Point mousePoint = e.GetPosition(this);

            if ((int)mousePoint.X >= (int)pointLeftTop.X && (int)mousePoint.Y >= (int)pointLeftTop.Y &&
                (int)mousePoint.X <= (int)pointRightBottom.X && (int)mousePoint.Y <= (int)pointRightBottom.Y)
            {
                crossPen = new Pen(crossInBrush, 1);
                this.Cursor = Cursors.Arrow;

                isButtonIn = true;
            }
            else
            {
                crossPen = new Pen(crossOutBrush, 1);
                this.Cursor = null;

                isButtonIn = false;
            }

            if (isButtonIn != saveButtonIn) InvalidateVisual();
        }
        #endregion

        #region Public Method

        #region Validation 관련
        public void ClearValidation()
        {
            try
            {
                if (validationToolTip == null)
                {
                    validationToolTip = new ToolTip();
                    validationToolTip.PlacementTarget = this;
                    validationToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                }

                validationTimer?.Stop();

                if (validationVisibility.Equals(Visibility.Visible))
                {
                    if (validationToolTip.IsOpen) validationToolTip.IsOpen = false;

                    this.ToolTip = null;

                    ValidationVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        
        public void SetValidation(string messageID, params object[] parameters)
        {
            SetValidation(MessageDic.Instance.GetMessage(messageID, parameters), true);
        }

        public void SetValidation(string message, bool showToolTip = true)
        {
            try
            {
                if (validationToolTip == null)
                {
                    validationToolTip = new ToolTip();
                    validationToolTip.PlacementTarget = this;
                    validationToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                }

                if (validationTimer == null)
                {
                    validationTimer = new System.Timers.Timer(5000);
                    validationTimer.Elapsed += validationTimer_Elapsed;
                    validationTimer.AutoReset = true;
                }
                validationTimer.Stop();

                ValidationVisibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(message))
                {
                    string convertMessage = MessageDic.Instance.GetMessage(message).Replace("[#]", "").Trim();

                    validationToolTip.Content = convertMessage;

                    this.ToolTip = validationToolTip;

                    if (showToolTip && !validationToolTip.IsOpen)
                    {
                        validationToolTip.IsOpen = true;
                        if (this.ActualHeight > 30)
                        {
                            validationToolTip.HorizontalOffset = (this.ActualWidth - validationToolTip.ActualWidth) / 2 * -1 + 20;
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
        private void PasteRebuildComma()
        {
            try
            {
                string convertText = Clipboard.GetText();

                convertText = convertText.Replace(" ", ",");
                convertText = convertText.Replace("\r", ",");
                convertText = convertText.Replace("\n", ",");
                convertText = convertText.Replace("\t", ",");

                while (convertText.IndexOf(",,") > -1)
                {
                    convertText = convertText.Replace(",,", ",");
                }

                if (convertText.Length > 0 && convertText.Substring(convertText.Length - 1, 1).Equals(","))
                {
                    convertText = convertText.Substring(0, convertText.Length - 1);
                }

                Text = convertText;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

    }
}
