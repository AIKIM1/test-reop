/*************************************************************************************
 Created Date : 2020.12.01
      Creator : 조영대
   Decription : UcToggleOnOff
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.01  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Controls
{

    /// <summary>
    /// UcToggleOnOff.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcToggleOnOff : UserControl
    {
        #region Declaration

        public enum eToggleControlType
        {
            /// <summary>
            /// CheckBox 형식
            /// </summary>
            CheckControl,
            /// <summary>
            /// RadioButton 형식
            /// </summary>
            RadioControl
        }

        public enum eToggleViewType
        {
            /// <summary>
            /// 원형
            /// </summary>
            Circle,
            /// <summary>
            /// 라운드형
            /// </summary>
            Rounded
        }

        public event OnOffValueChangedEventHandler OnOffValueChanged;
        public delegate void OnOffValueChangedEventHandler(object sender, bool onoff);

        private double btnPadding = 2;

        private Brush borderBrush = Brushes.Gray;
 
        private Color btnColor = Colors.Gainsboro;
        private Color disableColor = Colors.Silver;

        private Pen outLinePen;
        private Pen outLineButtonPen;

        private SolidColorBrush onSolidBrush;
        private SolidColorBrush offSolidBrush;
        private SolidColorBrush btnSolidBrush;
        private SolidColorBrush disableSolidBrush;

        private LinearGradientBrush onGradientBrush;
        private LinearGradientBrush offGradientBrush;
        private LinearGradientBrush btnGradientBrush;
        private LinearGradientBrush disableGradientBrush;

        DoubleAnimation moveAni;

        #region Property

        private eToggleControlType toggleControlType = eToggleControlType.CheckControl;
        [Category("GMES"), DefaultValue(eToggleControlType.CheckControl), Description("버튼 유형")]
        public eToggleControlType ToggleControlType
        {
            get { return toggleControlType; }
            set
            {
                toggleControlType = value;
                InvalidateVisual();
            }
        }

        private eToggleViewType toggleType = eToggleViewType.Circle;
        [Category("GMES"), DefaultValue(eToggleViewType.Circle), Description("버튼 보기 유형")]
        public eToggleViewType ToggleType
        {
            get { return toggleType; }
            set
            {
                toggleType = value;
                InvalidateVisual();
            }
        }

        private Color onColor = Colors.Green;
        [Category("GMES"), DefaultValue(typeof(Color), "System.Windows.Media.Colors.Green"), Description("On Color")]
        public Color OnColor
        {
            get { return onColor; }
            set
            {
                onColor = value;

                onSolidBrush = new SolidColorBrush(onColor);
                onGradientBrush = new LinearGradientBrush(onColor, Color.FromArgb(180, onColor.R, onColor.G, onColor.B), 50);

                InvalidateVisual();
            }
        }

        private Color offColor = Colors.WhiteSmoke;
        [Category("GMES"), DefaultValue(typeof(Color), "System.Windows.Media.Colors.WhiteSmoke"), Description("Off Color")]
        public Color OffColor
        {
            get { return offColor; }
            set
            {
                offColor = value;
                
                offSolidBrush = new SolidColorBrush(offColor);
                offGradientBrush = new LinearGradientBrush(offColor, Color.FromArgb(180, offColor.R, offColor.G, offColor.B), 50);

                InvalidateVisual();
            }
        }

        private bool useGradient = false;
        [Category("GMES"), DefaultValue(false), Description("Gradient 사용 여부")]
        public bool UseGradient
        {
            get { return useGradient; }
            set
            {
                useGradient = value;
                InvalidateVisual();
            }
        }

        private double buttonSize = 40;
        [Category("GMES"), DefaultValue(40), Description("버튼의 크기, 전체 길이의 %")]
        public double ButtonSize
        {
            get { return buttonSize; }
            set
            {
                buttonSize = value;
                InvalidateVisual();
            }
        }

        private double slideSpeed = 50;
        [Category("GMES"), DefaultValue(50), Description("버튼 움직임 속도(Milliseconds)")]
        public double SlideSpeed
        {
            get { return slideSpeed; }
            set
            {
                slideSpeed = value;
                if (moveAni != null)
                {
                    moveAni.Duration = new Duration(TimeSpan.FromMilliseconds(slideSpeed));
                }
            }
        }

        private string groupName = "DefaultGroup";
        [Category("GMES"), DefaultValue("DefaultGroup"), Description("RadioControl 일때 그룹이름")]
        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
            }
        }

        #endregion

        #region DependencyProperty

        private double buttonPosition = 0d;
        [Category("GMES"), DefaultValue(0d), Description("버튼의 위치, 전체 길이의 %")]
        public double ButtonPositon
        {
            get { return buttonPosition; }
            set
            {
                buttonPosition = value;
                InvalidateVisual();
            }
        }

        public static readonly DependencyProperty ButtonPositonProperty =
            DependencyProperty.Register("ButtonPositon", typeof(double), typeof(UcToggleOnOff), new PropertyMetadata(0d, UcToggleButtonPositonPropertyChangedCallback));

        //private bool eventCheck = true;
        private bool onoff = false;
        [Category("GMES"), DefaultValue(false), Description("버튼 On/Off 값")]
        public bool OnOff
        {
            get
            {
                return onoff;
            }
            set
            {
                if (onoff != value)
                {
                    if (toggleControlType.Equals(eToggleControlType.RadioControl))
                    {
                        ClearOtherToggleOnOff();
                    }

                    SetValue(OnOffProperty, value);

                    if (!onoff)
                    {
                        if (!toggleControlType.Equals(eToggleControlType.RadioControl))
                        {
                            moveAni.From = 100d;
                            moveAni.To = 0d;

                            buttonPosition = 100d;
                        }
                    }
                    else
                    {
                        moveAni.From = 0d;
                        moveAni.To = 100d;

                        buttonPosition = 0d;
                    }

                    BeginAnimation(ButtonPositonProperty, moveAni);
                }
                
            }
        }
        
        public static readonly DependencyProperty OnOffProperty =
            DependencyProperty.Register("OnOff", typeof(bool), typeof(UcToggleOnOff), new PropertyMetadata(false, UcToggleOnOffPropertyChangedCallback));
        
        #endregion

        public UcToggleOnOff()
        {
            InitializeComponent();
            
            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            outLinePen = new Pen(borderBrush, 1);
            outLineButtonPen = new Pen(borderBrush, 1.5);

            onSolidBrush = new SolidColorBrush(onColor);
            offSolidBrush = new SolidColorBrush(offColor);
            btnSolidBrush = new SolidColorBrush(btnColor);
            disableSolidBrush = new SolidColorBrush(disableColor);

            onGradientBrush = new LinearGradientBrush(onColor, Color.FromArgb(180, onColor.R, onColor.G, onColor.B), 50);
            offGradientBrush = new LinearGradientBrush(offColor, Color.FromArgb(180, offColor.R, offColor.G, offColor.B), 50);
            btnGradientBrush = new LinearGradientBrush(btnColor, Colors.White, 230);
            disableGradientBrush = new LinearGradientBrush(disableColor, Colors.White, 230);

            moveAni = new DoubleAnimation();
            moveAni.Duration = new Duration(TimeSpan.FromMilliseconds(slideSpeed));
            moveAni.AutoReverse = false;
            moveAni.Completed += MoveAni_Completed;
        }

        #endregion

        #region Override

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.Property.Name)
            {
                case "IsEnabled":
                    InvalidateVisual();
                    break;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ActualWidth <= btnPadding * 2 || ActualHeight <= btnPadding * 2) return;
            
            Rect rectAll = new Rect(new Size(ActualWidth, ActualHeight));
            Rect rectButton = new Rect();
            Rect rectOn = new Rect();
            Rect rectOff = new Rect();

            Point point1, point2, point3, point4;

            double btnLocation = 0;
            double btnSize = 0;
            double radious = 0;

            if (ActualHeight > ActualWidth)
            {
                switch (toggleType)
                {
                    case eToggleViewType.Circle:
                        radious = ActualWidth / 2;
                        break;
                    case eToggleViewType.Rounded:
                        radious = ActualWidth * 0.15;
                        break;
                }

                btnSize = ActualHeight * (buttonSize / 100);
                btnLocation = buttonPosition * (ActualHeight - btnSize - btnPadding) / 100 + btnPadding;

                rectButton = new Rect(btnPadding, btnLocation, ActualWidth - (btnPadding * 2), btnSize - btnPadding);
                rectOn = new Rect(0, 0, ActualWidth, btnLocation + (btnSize / 2) + (radious / 2));
                rectOff = new Rect(0, btnLocation + (btnSize / 2) - (radious / 2), ActualWidth, ActualHeight - (btnLocation + (btnSize / 2)) + (radious / 2));

                point1 = new Point(rectOn.Left, rectOn.Bottom - (radious * 0.8));
                point2 = new Point(rectOff.Left, rectOff.Top + (radious * 0.8));
                point3 = new Point(rectOn.Right, rectOn.Bottom - (radious * 0.8));
                point4 = new Point(rectOff.Right, rectOff.Top + (radious * 0.8));
            }
            else
            {
                switch (toggleType)
                {
                    case eToggleViewType.Circle:
                        radious = ActualHeight / 2;
                        break;
                    case eToggleViewType.Rounded:
                        radious = ActualHeight * 0.15;
                        break;
                }

                btnSize = ActualWidth * (buttonSize / 100);
                btnLocation = buttonPosition * (ActualWidth - btnSize - btnPadding) / 100 + btnPadding;

                rectButton = new Rect(btnLocation, btnPadding, btnSize - btnPadding, ActualHeight - (btnPadding * 2));
                rectOn = new Rect(0, 0, btnLocation + (btnSize / 2) + (radious / 2), ActualHeight);
                rectOff = new Rect(btnLocation + (btnSize / 2) - (radious / 2), 0, ActualWidth - (btnLocation + (btnSize / 2)) + (radious / 2), ActualHeight);

                point1 = new Point(rectOn.Right - (radious * 0.8), rectOn.Top);
                point2 = new Point(rectOff.Left + (radious * 0.8), rectOn.Top);
                point3 = new Point(rectOn.Right - (radious * 0.8), rectOn.Bottom);
                point4 = new Point(rectOff.Left + (radious * 0.8), rectOn.Bottom);
            }

            if (IsEnabled)
            {
                if (useGradient)
                {
                    if (buttonPosition.Equals(0))
                    {
                        drawingContext.DrawRoundedRectangle(offGradientBrush, outLinePen, rectAll, radious, radious);
                    }
                    else if (buttonPosition.Equals(100))
                    {
                        drawingContext.DrawRoundedRectangle(onGradientBrush, outLinePen, rectAll, radious, radious);
                    }
                    else
                    {
                        drawingContext.DrawRoundedRectangle(onGradientBrush, outLinePen, rectOn, radious, radious);
                        drawingContext.DrawRoundedRectangle(offGradientBrush, outLinePen, rectOff, radious, radious);

                        drawingContext.DrawLine(outLinePen, point1, point2);
                        drawingContext.DrawLine(outLinePen, point3, point4);
                    }
                    drawingContext.DrawRoundedRectangle(btnGradientBrush, outLineButtonPen, rectButton, radious, radious);
                }
                else
                {
                    if (buttonPosition.Equals(0))
                    {
                        drawingContext.DrawRoundedRectangle(offSolidBrush, outLinePen, rectAll, radious, radious);
                    }
                    else if (buttonPosition.Equals(100))
                    {
                        drawingContext.DrawRoundedRectangle(onSolidBrush, outLinePen, rectAll, radious, radious);
                    }
                    else
                    {
                        drawingContext.DrawRoundedRectangle(onSolidBrush, outLinePen, rectOn, radious, radious);
                        drawingContext.DrawRoundedRectangle(offSolidBrush, outLinePen, rectOff, radious, radious);

                        drawingContext.DrawLine(outLinePen, point1, point2);
                        drawingContext.DrawLine(outLinePen, point3, point4);
                    }
                    drawingContext.DrawRoundedRectangle(btnSolidBrush, outLineButtonPen, rectButton, radious, radious);
                }
            }
            else
            {
                if (useGradient)
                {
                    drawingContext.DrawRoundedRectangle(disableGradientBrush, outLinePen, rectAll, radious, radious);
                    drawingContext.DrawRoundedRectangle(btnGradientBrush, outLineButtonPen, rectButton, radious, radious);
                }
                else
                {
                    drawingContext.DrawRoundedRectangle(disableSolidBrush, outLinePen, rectAll, radious, radious);
                    drawingContext.DrawRoundedRectangle(btnSolidBrush, outLineButtonPen, rectButton, radious, radious);
                }
            }
        }
        
        #endregion

        #region Event
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            if (!IsFocused) Focus();

            if (Parent != null)
            {
                if ((Parent as UIElement).IsEnabled.Equals(false)) return;

                if (Parent is C1.WPF.DataGrid.DataGridCellPresenter &&
                    (Parent as C1.WPF.DataGrid.DataGridCellPresenter).Column.IsReadOnly.Equals(true)) return;
            }
            
            if (toggleControlType.Equals(eToggleControlType.RadioControl))
            {
                if (OnOff != true) OnOff = true;
            }
            else
            {
                OnOff = !OnOff;
            }
        }

        private void MoveAni_Completed(object sender, EventArgs e)
        {
      
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            InvalidateVisual();
        }


        #endregion

        #region Method
        private void ClearOtherToggleOnOff()
        {
            if (Parent is C1.WPF.DataGrid.DataGridCellPresenter)
            {
                C1.WPF.DataGrid.DataGridCellPresenter dgCell = Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                
                foreach (UcToggleOnOff ucOnOff in FindVisualChildren<UcToggleOnOff>(dgCell.DataGrid))
                {
                    if (ucOnOff.GroupName.Equals(groupName))
                    {
                        ucOnOff.SetValue(OnOffProperty, false);
                    }
                }
            }
            else
            {
                foreach (UcToggleOnOff ucOnOff in FindVisualChildren<UcToggleOnOff>(Parent))
                {
                    if (ucOnOff.GroupName.Equals(groupName) && !ucOnOff.Equals(this))
                    {
                        ucOnOff.SetValue(OnOffProperty, false);
                    }
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private static void UcToggleOnOffPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcToggleOnOff ucToggleOnOff = d as UcToggleOnOff;
            switch (e.Property.Name)
            {
                case "OnOff":
                    ucToggleOnOff.onoff = e.NewValue.Equals(true);
                    if (ucToggleOnOff.onoff)
                    {
                        ucToggleOnOff.buttonPosition = 100;
                    }
                    else
                    {
                        ucToggleOnOff.buttonPosition = 0;
                    }

                    ucToggleOnOff.InvalidateVisual();

                    ucToggleOnOff.OnOffValueChanged?.Invoke(ucToggleOnOff, ucToggleOnOff.onoff);
                    break;
            }    
        }

        private static void UcToggleButtonPositonPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcToggleOnOff ucToggleOnOff = d as UcToggleOnOff;
            ucToggleOnOff.buttonPosition = (double)e.NewValue;
            ucToggleOnOff.InvalidateVisual();
        }
        #endregion
    }
}
