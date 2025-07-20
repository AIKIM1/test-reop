/*************************************************************************************
 Created Date : 2020.12.01
      Creator : 조영대
   Decription : 투입요청서 - Hopper 정보 사용자 컨트롤
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.01  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using LGC.GMES.MES.Common;
using System.Windows.Data;
using LGC.GMES.MES.CMM001.Extensions;
using System.ComponentModel;
using System;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ELEC003.Controls
{

    /// <summary>
    /// UcHopper.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcHopper : UserControl
    {
        #region Declaration

        public enum ViewMode
        {
            NORMAL_OPERATION,
            INPUT_REQUEST,
            UNUSED_HOPPER
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public event HopperDoubleClickEventHandler HopperDoubleClick;
        public delegate void HopperDoubleClickEventHandler(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId);

        private static Color fillNormalOperation = Color.FromArgb(255, 34, 177, 76);
        private static Color fillInputRequest = Color.FromArgb(255, 255, 192, 0);
        private static Color fillUnusedHopper = Color.FromArgb(255, 127, 127, 127);

        private static LinearGradientBrush fillNormalOperationGradientBrush = 
            new LinearGradientBrush(fillNormalOperation, Color.FromArgb(180, fillNormalOperation.R, fillNormalOperation.G, fillNormalOperation.B), 50);
        private static LinearGradientBrush fillInputRequestGradientBrush = 
            new LinearGradientBrush(fillInputRequest, Color.FromArgb(180, fillInputRequest.R, fillInputRequest.G, fillInputRequest.B), 50);
        private static LinearGradientBrush fillUnusedHopperGradientBrush = 
            new LinearGradientBrush(fillUnusedHopper, Color.FromArgb(180, fillUnusedHopper.R, fillUnusedHopper.G, fillUnusedHopper.B), 50);

        private LinearGradientBrush fillGradientBrush = new LinearGradientBrush(fillNormalOperation, Color.FromArgb(100, fillNormalOperation.R, fillNormalOperation.G, fillNormalOperation.B), 50);


        private static SolidColorBrush fillNormalOperationSolidBrush = new SolidColorBrush(fillNormalOperation);
        private static SolidColorBrush fillInputRequestSolidBrush = new SolidColorBrush(fillInputRequest);
        private static SolidColorBrush fillUnusedHopperSolidBrush = new SolidColorBrush(fillUnusedHopper);

        private SolidColorBrush fillSolidBrush = new SolidColorBrush(fillNormalOperation);
        private SolidColorBrush fillAlarmBrush = new SolidColorBrush(fillInputRequest);
        private LinearGradientBrush fillGradientAlarmBrush = fillInputRequestGradientBrush;

        private bool alarmOnOff = false;

        [DefaultValue(false)]
        public bool UseGradient { get; set; }

        [DefaultValue(false)]
        public bool UseAlarm { get; set; }

        private ViewMode mode;

        public ViewMode Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                
                alarmOnOff = false;
                switch (mode)
                {
                    case ViewMode.NORMAL_OPERATION:
                        fillGradientBrush = fillNormalOperationGradientBrush;
                        fillSolidBrush = fillNormalOperationSolidBrush;
                        txtHopperWeight.Foreground = new SolidColorBrush(Colors.Black);
                        txtHopperTime.Foreground = new SolidColorBrush(Colors.White);
                        break;
                    case ViewMode.INPUT_REQUEST:
                        fillGradientBrush = fillInputRequestGradientBrush;
                        fillSolidBrush = fillInputRequestSolidBrush;
                        txtHopperWeight.Foreground = new SolidColorBrush(Colors.Black);
                        txtHopperTime.Foreground = new SolidColorBrush(Colors.Red);

                        if (UseAlarm)
                        {
                            alarmOnOff = true;

                            ColorAnimation colorAni = new ColorAnimation()
                            {
                                From = Color.FromArgb(255, 255, 192, 0),
                                To = Colors.Red,
                                Duration = TimeSpan.FromSeconds(2),                                
                                AutoReverse = true,
                                RepeatBehavior = RepeatBehavior.Forever
                            };

                            Random rand = new Random(DateTime.Now.Millisecond);
                            int stratTime = rand.Next(0, 2000);
                            colorAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);
                            
                            BorderThickness = new Thickness(0);
                            BorderBrush = new SolidColorBrush(Colors.White);
                            BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);                            
                        }
                        break;
                    case ViewMode.UNUSED_HOPPER:
                        fillGradientBrush = fillUnusedHopperGradientBrush;
                        fillSolidBrush = fillUnusedHopperSolidBrush;
                        txtHopperWeight.Foreground = new SolidColorBrush(Colors.Gray);
                        txtHopperTime.Foreground = new SolidColorBrush(Colors.Gray);
                        break;
                }

                this.InvalidateVisual();
            }       
        }

        public string EquipmentSegment { get; set; }

        public string Process { get; set; }

        public string Equipment { get; set; }

        public string HopperId { get; set; }

        private string hopperName = string.Empty;
        public string HopperName
        {
            get
            {
                return (string)GetValue(HopperNameProperty);
            }
            set
            {
                SetValue(HopperNameProperty, value);

                if (!value.Equals(hopperName))
                {
                    OnPropertyChanged("Name");
                }
                hopperName = value;

                txtHopperName.Text = hopperName;
                txtHopperName.ToolTip = hopperName;
            }
        }

        public static readonly DependencyProperty HopperNameProperty =
            DependencyProperty.Register("HopperName", typeof(string), typeof(UcHopper), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        private string hopperWeight = string.Empty;
        public string HopperWeight
        {
            get
            {
                return (string)GetValue(HopperWeightProperty);
            }
            set
            {
                SetValue(HopperWeightProperty, value);

                if (!value.Equals(hopperWeight))
                {
                    OnPropertyChanged("Weight");
                }
                hopperWeight = value;

                if (mode.Equals(ViewMode.UNUSED_HOPPER))
                {
                    txtHopperWeight.Text = string.Empty;
                }
                else
                {
                    txtHopperWeight.Text = hopperWeight;
                }
                
            }
        }

        public static readonly DependencyProperty HopperWeightProperty =
            DependencyProperty.Register("HopperWeight", typeof(string), typeof(UcHopper), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        private string hopperTime = string.Empty;
        public string HopperTime
        {
            get
            {
                return (string)GetValue(HopperTimeProperty);
            }
            set
            {
                SetValue(HopperTimeProperty, value);

                if (!value.Equals(hopperTime))
                {
                    OnPropertyChanged("Time");
                }
                hopperTime = value;

                if (mode.Equals(ViewMode.UNUSED_HOPPER))
                {
                    txtHopperTime.Text = string.Empty;
                }
                else
                {
                    txtHopperTime.Text = hopperTime;
                }          
            }
        }

        public static readonly DependencyProperty HopperTimeProperty =
            DependencyProperty.Register("HopperTime", typeof(string), typeof(UcHopper), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public string MaterialId { get; set; }

        private string hopperMaterial = string.Empty;
        public string HopperMaterial
        {
            get
            {
                return (string)GetValue(HopperTimeProperty);
            }
            set
            {
                SetValue(HopperTimeProperty, value);

                if (!value.Equals(hopperMaterial))
                {
                    OnPropertyChanged("Time");
                }
                hopperMaterial = value;

                if (mode.Equals(ViewMode.UNUSED_HOPPER))
                {
                    txtMaterialCode.Text = string.Empty;
                }
                else
                {
                    txtMaterialCode.Text = hopperMaterial;
                }
           
            }
        }

        public static readonly DependencyProperty HopperMaterialProperty =
            DependencyProperty.Register("HopperMaterial", typeof(string), typeof(UcHopper), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        private bool isProperWeight = true;
        public bool IsProperWeight
        {
            get
            {
                return (bool)GetValue(IsProperWeightProperty);
            }
            set
            {
                SetValue(IsProperWeightProperty, value);

                if (!value.Equals(hopperWeight))
                {
                    OnPropertyChanged("Weight");
                }
                isProperWeight = value;

                if (isProperWeight)
                {
                    txtHopperName.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    txtHopperName.Foreground = new SolidColorBrush(Colors.Red);
                }

            }
        }

        public static readonly DependencyProperty IsProperWeightProperty =
            DependencyProperty.Register("IsProperWeight", typeof(bool), typeof(UcHopper), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public UcHopper()
        {
            InitializeComponent();
            
            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            txtMaterialCode.FontSize = 10;
            this.Width = 80;
        }


        #endregion

        #region Override
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            if (e.Property.Name.Equals("BorderBrush"))
            {
                InvalidateVisual();
            }
        }

        private void OnPropertyChanged(string v)
        {
        
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Brush backGround = Parent.GetValue(BackgroundProperty) as Brush;
            if (backGround == null)
            {
                backGround = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            drawingContext.DrawRectangle(backGround, new Pen(backGround, 0), new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            const double LINE_THICKNESS = 1;

            double TopBottomMargin = 3;
            double hopperLeft = 3;
            double hopperTop = this.ActualHeight / 5 + TopBottomMargin;
            double hopperWidth = hopperCanvas.ActualWidth;
            double hopperHeight = hopperCanvas.ActualHeight - TopBottomMargin;
            
            Point[] pointArray = new Point[5];

            pointArray[0] = new Point(hopperLeft, hopperTop);
            pointArray[1] = new Point(hopperLeft + hopperWidth, hopperTop);
            pointArray[2] = new Point(hopperLeft + hopperWidth, hopperTop + (hopperHeight / 3 * 2));
            pointArray[3] = new Point(hopperLeft + hopperWidth / 2, hopperTop + hopperHeight);
            pointArray[4] = new Point(hopperLeft, hopperTop + (hopperHeight / 3 * 2));

            Pen pen = new Pen(Brushes.Black, LINE_THICKNESS);

            
            if (UseGradient)
            {
                if (UseAlarm && alarmOnOff)
                {
                    fillGradientAlarmBrush = 
                        new LinearGradientBrush(((SolidColorBrush)BorderBrush).Color, Color.FromArgb(180, fillInputRequest.R, fillInputRequest.G, fillInputRequest.B), 100);
                    drawingContext.DrawPolygon(fillGradientAlarmBrush, pen, pointArray, FillRule.EvenOdd);                    
                }
                else
                {
                    drawingContext.DrawPolygon(fillGradientBrush, pen, pointArray, FillRule.EvenOdd);
                }
            }
            else
            {
                if (UseAlarm && alarmOnOff)
                {
                     fillAlarmBrush = (SolidColorBrush)BorderBrush;
                    drawingContext.DrawPolygon(fillAlarmBrush, pen, pointArray, FillRule.EvenOdd);                    
                }
                else
                {
                    drawingContext.DrawPolygon(fillSolidBrush, pen, pointArray, FillRule.EvenOdd);
                }                
            }
            
            if (this.IsFocused)
            {
                SolidColorBrush outLineBrush = new SolidColorBrush(Color.FromArgb(255, 0, 204, 153));
                Pen outLinePen = new Pen(outLineBrush, 1);

                SolidColorBrush fillBrush = new SolidColorBrush(Color.FromArgb(150, 218, 255, 246));
                drawingContext.DrawRectangle(fillBrush, outLinePen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            }            
        }

        #endregion

        #region Event
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsFocused)
            {
                this.Focus();
                this.InvalidateVisual();
            }
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            this.InvalidateVisual();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HopperDoubleClick?.Invoke(this, EquipmentSegment, Process, Equipment, MaterialId, HopperId);
            
            this.InvalidateVisual();
        }
        
        #endregion

        #region Mehod

        #endregion
    }
}
