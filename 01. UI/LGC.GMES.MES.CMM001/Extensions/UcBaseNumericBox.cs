/*************************************************************************************
 Created Date : 2023.10.30
      Creator : 
   Decription : NumericBox Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.30  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseNumericBox : C1.WPF.C1NumericBox, IControlValidation
    {
        #region EventHandler
        public event ClipboardPastedEventHandler ClipboardPasted;
        public delegate void ClipboardPastedEventHandler(object sender, DataObjectPastingEventArgs e, string text);
        #endregion

        #region Property

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
            DependencyProperty.Register("ValidationVisibility", typeof(Visibility), typeof(UcBaseNumericBox), new PropertyMetadata(Visibility.Collapsed, ValidationVisibilityPropertyChangedCallback));

        private static void ValidationVisibilityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseNumericBox ucBaseNumericBox = d as UcBaseNumericBox;
            ucBaseNumericBox.validationVisibility = (Visibility)e.NewValue;
            ucBaseNumericBox.InvalidateVisual();
        }

        #endregion

        #region Declaration & Constructor 
        private System.Timers.Timer validationTimer;
        private ToolTip validationToolTip = null;

        private Style originalStyle = null;


        public UcBaseNumericBox()
        {
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (originalStyle == null)
            {
                if (this.Style == null)
                {
                    this.Style = Application.Current.Resources["C1NumericBoxStyle"] as Style;
                }

                originalStyle = this.Style;

                if (!Application.Current.Resources.Contains("UcBaseNumericBoxStyle"))
                {
                    ResourceDictionary resourceDic = new ResourceDictionary();
                    resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                    Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                }

                if (Application.Current.Resources.Contains("UcBaseNumericBoxStyle"))
                {
                    this.Style = Application.Current.Resources["UcBaseNumericBoxStyle"] as Style;
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
        }

        #endregion

        #region Override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }
        #endregion

        #region Event
   
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

        #endregion

    }
}
