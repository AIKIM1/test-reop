/*************************************************************************************
 Created Date : 2022.07.20
      Creator : 조영대
   Decription : UcProgress
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.20  조영대 : Initial Created.
**************************************************************************************/

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System;
using System.Windows.Media.Animation;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.CMM001.Controls
{
    public partial class UcProgress : UserControl
    {
        #region Declaration
        
        public enum eProgressViewType
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

        public enum eLabelViewType
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            Center
        }

        #region EventHandler

        public event ClickProgressEventHandler ClickProgress;
        public delegate void ClickProgressEventHandler(object sender);

        public event PercentValueChangedEventHandler PercentValueChanged;
        public delegate void PercentValueChangedEventHandler(object sender, double percent);

        public class WorkProcessEventArgs : EventArgs
        {
            public WorkProcessEventArgs(BackgroundWorker worker, object arguments, object result, Exception ex)
            {
                Worker = worker;
                Arguments = arguments;
                Result = result;
                Exception = ex;                
            }

            public BackgroundWorker Worker { get; set; }
            public object Arguments { get; set; }
            public object Result { get; set; }
            public Exception Exception { get; set; }
        }

        public event WorkProcessEventHandler WorkProcess;
        public delegate object WorkProcessEventHandler(object sender, WorkProcessEventArgs e);

        public event WorkProcessChangedEventHandler WorkProcessChanged;
        public delegate void WorkProcessChangedEventHandler(object sender, int percent, WorkProcessEventArgs e);

        public event WorkProcessCompletedEventHandler WorkProcessCompleted;
        public delegate void WorkProcessCompletedEventHandler(object sender, WorkProcessEventArgs e);
        
        #endregion


        #region Property

        private eLabelViewType labelViewType = eLabelViewType.None;
        [Category("GMES"), DefaultValue(eLabelViewType.None), Description("라벨 보기 유형")]
        public eLabelViewType LabelViewType
        {
            get { return labelViewType; }
            set
            {
                labelViewType = value;

                lblMsgLeft.Visibility = Visibility.Collapsed;
                lblMsgTop.Visibility = Visibility.Collapsed;
                lblMsgRight.Visibility = Visibility.Collapsed;
                lblMsgBottom.Visibility = Visibility.Collapsed;
                lblMsgCenter.Visibility = Visibility.Collapsed;

                switch (labelViewType)
                {
                    case eLabelViewType.Left:
                        lblMsgLeft.Visibility = Visibility.Visible;
                        break;
                    case eLabelViewType.Top:
                        lblMsgTop.Visibility = Visibility.Visible;
                        break;
                    case eLabelViewType.Right:
                        lblMsgRight.Visibility = Visibility.Visible;
                        break;
                    case eLabelViewType.Bottom:
                        lblMsgBottom.Visibility = Visibility.Visible;
                        break;
                    case eLabelViewType.Center:
                        lblMsgCenter.Visibility = Visibility.Visible;
                        break;
                }
                InvalidateVisual();
            }
        }

        private eProgressViewType toggleType = eProgressViewType.Rounded;
        [Category("GMES"), DefaultValue(eProgressViewType.Rounded), Description("버튼 보기 유형")]
        public eProgressViewType ToggleType
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
                onGradientBrush = new LinearGradientBrush(Color.FromArgb(120, onColor.R, onColor.G, onColor.B), onColor, 50);

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

        private double buttonSize = 3;
        [Category("GMES"), DefaultValue(3), Description("버튼의 크기, 폭의 %")]
        public double ButtonSize
        {
            get { return buttonSize; }
            set
            {
                buttonSize = value;
                InvalidateVisual();
            }
        }

        private double slideSpeed = 100;
        [Category("GMES"), DefaultValue(100), Description("버튼 움직임 속도(Milliseconds)")]
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

        private bool flowReverse = false;
        [Category("GMES"), DefaultValue(false), Description("진행방향 반전")]
        public bool FlowReverse
        {
            get
            {
                return flowReverse;
            }
            set
            {
                flowReverse = value;

                if (flowReverse)
                {
                    onSolidBrush = new SolidColorBrush(offColor);
                    onGradientBrush = new LinearGradientBrush(Color.FromArgb(120, offColor.R, offColor.G, offColor.B), offColor, 50);

                    offSolidBrush = new SolidColorBrush(onColor);
                    offGradientBrush = new LinearGradientBrush(onColor, Color.FromArgb(180, onColor.R, onColor.G, onColor.B), 50);
                }
                else
                {
                    onSolidBrush = new SolidColorBrush(onColor);
                    onGradientBrush = new LinearGradientBrush(Color.FromArgb(120, onColor.R, onColor.G, onColor.B), onColor, 50);

                    offSolidBrush = new SolidColorBrush(offColor);
                    offGradientBrush = new LinearGradientBrush(offColor, Color.FromArgb(180, offColor.R, offColor.G, offColor.B), 50);
                }
                Clear();
            }
        }

        private bool labelReverse = false;
        [Category("GMES"), DefaultValue(false), Description("세로모드일때 라벨 문장 방향 반전")]
        public bool LabelReverse
        {
            get
            {
                return labelReverse;
            }
            set
            {
                labelReverse = value;
                InvalidateVisual();
            }
        }

        #endregion


        BackgroundWorker bgWorker = null;
      
        private double buttonPadding = 2;
        private double labelPadding = 5;

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
        
        private DoubleAnimation moveAni;

        #region DependencyProperty

        private double percent = 0d;
        [Category("GMES"), DefaultValue(0d), Description("% 값")]
        public double Percent
        {
            get
            {
                return percent;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;

                if (flowReverse)
                {
                    value = 100 - value;
                }

                if (percent != value)
                {
                    moveAni.From = percent;
                    moveAni.To = value;

                    moveAni.Duration = new Duration(TimeSpan.FromMilliseconds(100));

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        BeginAnimation(PercentProperty, moveAni);
                    }));

                }
                
            }
        }
        
        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register("Percent", typeof(double), typeof(UcProgress), new PropertyMetadata(0d, UcProgressPropertyChangedCallback));


        private string progressText = string.Empty;
        [Category("GMES"), DefaultValue(0d), Description("알림 문장")]
        public string ProgressText
        {
            get
            {
                return progressText;
            }
            set
            {
                if (progressText != value)
                {   
                    SetValue(ProgressTextProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(UcProgress), new PropertyMetadata(string.Empty, UcProgressPropertyChangedCallback));


        private static void UcProgressPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcProgress ucProgress = d as UcProgress;
            switch (e.Property.Name)
            {
                case "Percent":
                    ucProgress.percent = Convert.ToDouble(e.NewValue);
                    if (ucProgress.percent < 0) ucProgress.percent = 0;
                    if (ucProgress.percent > 100) ucProgress.percent = 100;
                    ucProgress.InvalidateVisual();

                    ucProgress.PercentValueChanged?.Invoke(ucProgress, ucProgress.percent);
                    break;
                case "ProgressText":
                    ucProgress.progressText = e.NewValue == null ? string.Empty : e.NewValue.ToString();
                    ucProgress.InvalidateVisual();
                    break;
            }
        }
        #endregion

        public UcProgress()
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

            onGradientBrush = new LinearGradientBrush(Color.FromArgb(120, onColor.R, onColor.G, onColor.B), onColor, 50);
            offGradientBrush = new LinearGradientBrush(offColor, Color.FromArgb(180, offColor.R, offColor.G, offColor.B), 50);
            btnGradientBrush = new LinearGradientBrush(btnColor, Colors.White, 230);
            disableGradientBrush = new LinearGradientBrush(disableColor, Colors.White, 230);

            moveAni = new DoubleAnimation();
            moveAni.Duration = new Duration(TimeSpan.FromMilliseconds(slideSpeed));
            moveAni.AutoReverse = false;
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
                        
            Rect rectAll = new Rect();
            Rect rectButton = new Rect();
            Rect rectBefore = new Rect();
            Rect rectAfter = new Rect();

            Point point1, point2, point3, point4;

            eLabelViewType printMsgType = labelViewType;

            double btnLocation = 0;
            double btnSize = 0;
            double radious = 0;
            double progressLeft = 0;
            double progressTop = 0;
            double progressWidth = 0;
            double progressHeight = 0;

            if (ActualHeight > ActualWidth)
            {
                switch (labelViewType)
                {
                    case eLabelViewType.None:
                    case eLabelViewType.Center:
                    case eLabelViewType.Top:
                    case eLabelViewType.Bottom:
                        progressWidth = ActualWidth;
                        progressHeight = ActualHeight;
                        printMsgType = eLabelViewType.Center;
                        break;
                    case eLabelViewType.Left:                    
                        progressLeft = lblMsgLeft.ActualWidth + labelPadding;
                        progressWidth = ActualWidth - lblMsgLeft.ActualWidth - labelPadding;
                        progressHeight = ActualHeight;
                        break;
                    case eLabelViewType.Right:
                        progressWidth = ActualWidth - lblMsgRight.ActualWidth - labelPadding;
                        progressHeight = ActualHeight;
                        break;
                }
                if (progressWidth < 10) progressWidth = 10;
                rectAll = new Rect(progressLeft, progressTop, progressWidth, progressHeight);

                switch (toggleType)
                {
                    case eProgressViewType.Circle:
                        radious = progressWidth / 2;
                        break;
                    case eProgressViewType.Rounded:
                        radious = progressWidth * 0.15;
                        break;
                }

                btnSize = rectAll.Height * (buttonSize / 100);
                if (btnSize < buttonPadding * 2) btnSize = buttonPadding * 2;

                btnLocation = percent * (rectAll.Height - btnSize - buttonPadding) / 100 + buttonPadding;

                rectButton = new Rect(progressLeft + buttonPadding, progressTop + btnLocation, progressWidth - (buttonPadding * 2), btnSize - buttonPadding);

                rectBefore = new Rect(progressLeft, progressTop, progressWidth, btnLocation + (btnSize / 2) + (radious / 2));
                rectAfter = new Rect(progressLeft, progressTop + btnLocation + (btnSize / 2) - (radious / 2), progressWidth, rectAll.Height - (btnLocation + (btnSize / 2)) + (radious / 2));

                point1 = new Point(rectBefore.Left, rectBefore.Bottom - (radious * 0.8));
                point2 = new Point(rectAfter.Left, rectAfter.Top + (radious * 0.8));
                point3 = new Point(rectBefore.Right, rectBefore.Bottom - (radious * 0.8));
                point4 = new Point(rectAfter.Right, rectAfter.Top + (radious * 0.8));
            }
            else
            {
                switch (labelViewType)
                {
                    case eLabelViewType.None:
                    case eLabelViewType.Center:
                    case eLabelViewType.Left:
                    case eLabelViewType.Right:
                        progressWidth = ActualWidth;
                        progressHeight = ActualHeight;
                        printMsgType = eLabelViewType.Center;
                        break;                    
                    case eLabelViewType.Top:
                        progressTop = lblMsgTop.ActualHeight + labelPadding;
                        progressWidth = ActualWidth;
                        progressHeight = ActualHeight - lblMsgTop.ActualHeight - labelPadding;
                        break;
                    case eLabelViewType.Bottom:
                        progressWidth = ActualWidth;
                        progressHeight = ActualHeight - lblMsgBottom.ActualHeight - labelPadding;
                        break;                    
                }
                if (progressHeight < 10) progressHeight = 10;
                rectAll = new Rect(progressLeft, progressTop, progressWidth, progressHeight);

                switch (toggleType)
                {
                    case eProgressViewType.Circle:
                        radious = progressHeight / 2;
                        break;
                    case eProgressViewType.Rounded:
                        radious = progressHeight * 0.15;
                        break;
                }

                btnSize = rectAll.Width * (buttonSize / 100);
                if (btnSize < buttonPadding * 2) btnSize = buttonPadding * 2;

                btnLocation = percent * (rectAll.Width - btnSize - buttonPadding) / 100 + buttonPadding;

                rectButton = new Rect(progressLeft + btnLocation, progressTop + buttonPadding, btnSize - buttonPadding, progressHeight - (buttonPadding * 2));

                rectBefore = new Rect(progressLeft, progressTop, btnLocation + (btnSize / 2) + (radious / 2), progressHeight);
                rectAfter = new Rect(progressLeft + btnLocation + (btnSize / 2) - (radious / 2), progressTop, rectAll.Width - (btnLocation + (btnSize / 2)) + (radious / 2), progressHeight);

                point1 = new Point(rectBefore.Right - (radious * 0.8), rectBefore.Top);
                point2 = new Point(rectAfter.Left + (radious * 0.8), rectBefore.Top);
                point3 = new Point(rectBefore.Right - (radious * 0.8), rectBefore.Bottom);
                point4 = new Point(rectAfter.Left + (radious * 0.8), rectBefore.Bottom);
            }

            if (IsEnabled)
            {
                if (useGradient)
                {
                    if (percent.Equals(0))
                    {
                        drawingContext.DrawRoundedRectangle(offGradientBrush, outLinePen, rectAll, radious, radious);
                    }
                    else if (percent.Equals(100))
                    {
                        drawingContext.DrawRoundedRectangle(onGradientBrush, outLinePen, rectAll, radious, radious);
                    }
                    else
                    {
                        drawingContext.DrawRoundedRectangle(onGradientBrush, outLinePen, rectBefore, radious, radious);
                        drawingContext.DrawRoundedRectangle(offGradientBrush, outLinePen, rectAfter, radious, radious);

                        drawingContext.DrawLine(outLinePen, point1, point2);
                        drawingContext.DrawLine(outLinePen, point3, point4);
                    }
                    drawingContext.DrawRoundedRectangle(btnGradientBrush, outLineButtonPen, rectButton, radious, radious);
                }
                else
                {
                    if (percent.Equals(0))
                    {
                        drawingContext.DrawRoundedRectangle(offSolidBrush, outLinePen, rectAll, radious, radious);
                    }
                    else if (percent.Equals(100))
                    {
                        drawingContext.DrawRoundedRectangle(onSolidBrush, outLinePen, rectAll, radious, radious);
                    }
                    else
                    {
                        drawingContext.DrawRoundedRectangle(onSolidBrush, outLinePen, rectBefore, radious, radious);
                        drawingContext.DrawRoundedRectangle(offSolidBrush, outLinePen, rectAfter, radious, radious);

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

            if (!labelViewType.Equals(eLabelViewType.None) && !string.IsNullOrEmpty(progressText))
            {
                Point messagePos = new Point();
                double rotateAngel = 0;
                switch (printMsgType)
                {
                    case eLabelViewType.Left:
                        if (ActualWidth - rectAll.Width - labelPadding > 0)
                        {
                            messagePos = new Rect(0, 0, ActualWidth - rectAll.Width - labelPadding, rectAll.Height).GetCenterPoint();
                        }
                        else
                        {
                            messagePos = new Point();
                        }

                        if (labelReverse)
                        {
                            rotateAngel = -90;
                        }
                        else
                        {
                            rotateAngel = 90;
                        }                        
                        break;
                    case eLabelViewType.Top:
                        if (ActualHeight - rectAll.Height - labelPadding > 0)
                        {
                            messagePos = new Rect(0, 0, rectAll.Width, ActualHeight - rectAll.Height - labelPadding).GetCenterPoint();
                        }
                        else
                        {
                            messagePos = new Point();
                        }
                        break;
                    case eLabelViewType.Right:
                        if (ActualWidth - rectAll.Width - labelPadding > 0)
                        {
                            messagePos = new Rect(rectAll.Width + labelPadding, 0, ActualWidth - rectAll.Width - labelPadding, rectAll.Height).GetCenterPoint();
                        }
                        else
                        {
                            messagePos = new Point();
                        }

                        if (labelReverse)
                        {
                            rotateAngel = -90;
                        }
                        else
                        {
                            rotateAngel = 90;
                        }
                        break;
                    case eLabelViewType.Bottom:
                        if (ActualHeight - rectAll.Height - labelPadding > 0)
                        {
                            messagePos = new Rect(0, rectAll.Height + labelPadding, rectAll.Width, ActualHeight - rectAll.Height - labelPadding).GetCenterPoint();
                        }
                        else
                        {
                            messagePos = new Point();
                        }
                        break;
                    case eLabelViewType.Center:
                        messagePos = rectAll.GetCenterPoint();
                        if (ActualHeight > ActualWidth)
                        {
                            if (labelReverse)
                            {
                                rotateAngel = -90;
                            }
                            else
                            {
                                rotateAngel = 90;
                            }
                        }
                        break;
                }

                drawingContext.DrawString(
                    messagePos,
                    progressText, TextAlignment.Center,
                    Foreground,
                    FontFamily, FontStyle, FontWeights.Bold, FontStretch, FontSize,
                    VerticalAlignment.Center, TextAlignment.Center,
                    rotateAngel);


            }
        }

        #endregion

        #region Event

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ClickProgress?.Invoke(sender);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            InvalidateVisual();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object result = WorkProcess?.Invoke(sender, new WorkProcessEventArgs(bgWorker, e.Argument, null, null));

                object[] arguments = new object[2];
                arguments[0] = e.Argument;
                arguments[1] = result;

                e.Result = arguments;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }
        
        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkProcessChanged?.Invoke(sender, e.ProgressPercentage, new WorkProcessEventArgs(bgWorker, e.UserState, null, null));
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                Util.MessageException((Exception)e.Result);
                WorkProcessCompleted?.Invoke(this, new WorkProcessEventArgs(bgWorker, null, null, (Exception)e.Result));
                return;
            }

            object[] argumentsWork = e.Result as object[];
            object arguemnts = argumentsWork[0];
            object result = argumentsWork[1];

            try
            {
                WorkProcessCompleted?.Invoke(this, new WorkProcessEventArgs(bgWorker, arguemnts, result, null));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                WorkProcessCompleted?.Invoke(this, new WorkProcessEventArgs(bgWorker, arguemnts, result, (Exception)e.Result));
            }
        }

        #endregion

        #region Public Method

        public void Clear()
        {
            if (flowReverse)
            {
                percent = 100;
            }
            else
            {
                percent = 0;
            }
            progressText = string.Empty;

            InvalidateVisual();
        }

        public void RunWorker()
        {
            RunWorker(null);
        }

        public void RunWorker(object arguments)
        {
            if (bgWorker == null)
            {
                bgWorker = new BackgroundWorker();

                bgWorker.WorkerSupportsCancellation = true;
                bgWorker.WorkerReportsProgress = true;

                bgWorker.DoWork += BgWorker_DoWork;
                bgWorker.ProgressChanged += BgWorker_ProgressChanged;
                bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            }
            else
            {
                if (bgWorker.IsBusy) return;
            }

            bgWorker.RunWorkerAsync(arguments);
        }

        public void CancelWorkProcess()
        {
            if (bgWorker != null && bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }
        #endregion

    }
    
}
