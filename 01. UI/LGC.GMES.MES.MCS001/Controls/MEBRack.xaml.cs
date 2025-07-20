/*************************************************************************************
 Created Date : 2019.05.08
      Creator : 오화백K
   Decription : CWA 물류 - MEB RACK UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2019.05.08   오화백   : MEB Rack Stair UserControl 
**************************************************************************************/
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.MCS001.Controls
{
    /// <summary>
    /// UcRackStair.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MEBRack : UserControl
    {
        #region Declaration & Constructor 

        //private MEBRack[][] _racks;
        public DockPanel RootLayout;
        private Color TypeABackColor = Colors.White;
        private Color TypeAForeColor = Colors.Black;
    
        private Color portColor = (Color)ColorConverter.ConvertFromString("#FFD99694");
        private Color portMaterialColor = (Color)ColorConverter.ConvertFromString("#FFC3D69B");
        private Color unuseColor = (Color)ColorConverter.ConvertFromString("#FF000000");
        private Color checkColor = (Color)ColorConverter.ConvertFromString("#FFFFFF00");
        private Color errorColor = (Color)ColorConverter.ConvertFromString("#FFE46C0A");


        //PORT_ID
        private string _portId = string.Empty;
        // Lot Id
        private string _lotId = string.Empty;
        // prodid
        private string _prodId = string.Empty;
        // procid
        private string _procid = string.Empty;
        // wipstat
        private string _wipstat = string.Empty;
        // WipHold 여부
        private string _wipHold = string.Empty;
        //특별관리
        private string _spcl_Flag = string.Empty;
        //자제출고flag
        private string _mtrlexistflag = string.Empty;
        //포트상태
        private string _portstat = string.Empty;
        //상태값
        private string _qa = string.Empty;
       



        // 사용자정의 필드
        private readonly Dictionary<string, object> _userDic = new Dictionary<string, object>();

        private bool _doubleClicked = false;
         
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> DoubleClick;
      

        public MEBRack()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            rootLayout.Background = new SolidColorBrush(TypeABackColor);
            Foreground = new SolidColorBrush(TypeAForeColor);
             //SetRackBackcolor();
        }
    
        #endregion

        #region Event
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_doubleClicked)
            {
                _doubleClicked = false;
                return;
            }

            Click?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            _doubleClicked = true;
            DoubleClick?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }
        #endregion





        #region Method
          
        /// <summary>
        /// PORT_ID 
        /// </summary>
        public string PORT_ID
        {
            get
            {
                return _portId;
            }
            set
            {
                _portId = value;
                lblPortID.Text = ObjectDic.Instance.GetObjectName(" 포트명 : ") + _portId;
            }
        }

        /// <summary>
        /// LOTID 
        /// </summary>
        public string LotId
        {
            get
            {
                return _lotId;
            }
            set
            {
                _lotId = value;
                lblLotID.Text = ObjectDic.Instance.GetObjectName(" LOT : ") + _lotId;
            }
        }
      
        /// <summary>
        /// 제품ID
        /// </summary>
        public string ProdId
        {
            get
            {
                return _prodId;
            }
            set
            {
                _prodId = value;
            }
        }

        /// <summary>
        /// 공정
        /// </summary>
        public string ProcId
        {
            get
            {
                return _procid;
            }
            set
            {
                _procid = value;
            }
        }

        /// <summary>
        /// 재공상태
        /// </summary>
        public string Wipstate
        {
            get
            {
                return _wipstat;
            }
            set
            {
                _wipstat = value;
            }
        }

        /// <summary>
        /// 재공Hold
        /// </summary>
        public string Wiphold
        {
            get
            {
                return _wipHold;
            }
            set
            {
                _wipHold = value;
            }
        }
        /// <summary>
        /// 특별관리
        /// </summary>
        public string Spcl_Flag
        {
            get
            {
                return _spcl_Flag;
            }
            set
            {
                _spcl_Flag = value;
               
            }
        }
        /// <summary>
        /// 자재출고Flag
        /// </summary>
        public string Mtrlexistflag
        {
            get
            {
                return _mtrlexistflag;
            }
            set
            {
                _mtrlexistflag = value;

            }
        }
        /// <summary>
        /// 포트상태
        /// </summary>
        public string PortStat
        {
            get
            {
                return _portstat;
            }
            set
            {
                _portstat = value;

            }
        }
        ////상태값
        //private string _qa = string.Empty;

        /// <summary>
        /// 상태값
        /// </summary>
        public string QA
        {
            get
            {
                return _qa;
            }
            set
            {
                //lblSkidId.Text = value;
                _qa = value;
                lblState.Text = " "+_qa;
                lblPort.Text = _portstat + "," + _mtrlexistflag;
                //포트에 대한 기본셋팅
                if (_portstat + _mtrlexistflag != "LRN" && _portstat + _mtrlexistflag != "URY")
                {
                    string test = _portstat + _mtrlexistflag;
                    lblPort.Background = new SolidColorBrush(Colors.Red);
                    lblPort.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    lblPort.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#05ba12"));
                    lblPort.Foreground = new SolidColorBrush(Colors.White);
                }
                //기본 Port 설정
                lblLotID.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2dfe5"));
                lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                lblState.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2dfe5"));
                lblState.Foreground = new SolidColorBrush(Colors.Black);

                LineTop.BorderThickness = new Thickness(1, 1, 1, 0);
                LineMid.BorderThickness = new Thickness(1, 0, 1, 0);
                LineBtm.BorderThickness = new Thickness(1, 0, 1, 1);

                LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));

                //LOT상태에 따른 색깔
               if (_qa == ObjectDic.Instance.GetObjectName("TERM")) //LOT 상태가 TERM
                {
                    lblLotID.Background = new SolidColorBrush(Colors.Red);
                    lblLotID.Foreground = new SolidColorBrush(Colors.Yellow);

                    lblState.Background = new SolidColorBrush(Colors.Red);
                    lblState.Foreground = new SolidColorBrush(Colors.Yellow);

                    LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));

                }
                else if (_qa == "HOLD") //LOT이 HOLD상태
                {
                    lblLotID.Background = new SolidColorBrush(Colors.Red);
                    lblLotID.Foreground = new SolidColorBrush(Colors.White);

                    lblState.Background = new SolidColorBrush(Colors.Red);
                    lblState.Foreground = new SolidColorBrush(Colors.White);

                    LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));

                }
                else if (_qa == ObjectDic.Instance.GetObjectName("검사대기")) //LOT이 검사대기
                {

                    lblLotID.Background = new SolidColorBrush(Colors.White);
                    lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                    lblState.Background = new SolidColorBrush(Colors.White);
                    lblState.Foreground = new SolidColorBrush(Colors.Black);

                    LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));

                    ColorAnimation da = new ColorAnimation();
                    da.From = Colors.White;
                    da.To = Colors.Yellow;
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    lblLotID.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                    ColorAnimation da1 = new ColorAnimation();
                    da1.From = Colors.White;
                    da1.To = Colors.Yellow;
                    da1.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da1.AutoReverse = true;
                    da1.RepeatBehavior = RepeatBehavior.Forever;
                    lblState.Background.BeginAnimation(SolidColorBrush.ColorProperty, da1);
                }
                else if (_qa == ObjectDic.Instance.GetObjectName("검사대기경과시간초과")) //LOT이 검사대기시간 초과
                {
                    lblLotID.Background = new SolidColorBrush(Colors.Yellow);
                    lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                    lblState.Background = new SolidColorBrush(Colors.Yellow);
                    lblState.Foreground = new SolidColorBrush(Colors.Black);

                    LineTop.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineTop.BorderThickness = new Thickness(2, 2, 2, 0);
                    LineMid.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineMid.BorderThickness = new Thickness(2, 0, 2, 0);
                    LineBtm.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineBtm.BorderThickness = new Thickness(2, 0, 2, 2);

                    ColorAnimation da = new ColorAnimation();
                    da.From = (Color)ColorConverter.ConvertFromString("#e2dfe5");
                    da.To = Colors.Yellow;
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    lblLotID.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                    ColorAnimation da1 = new ColorAnimation();
                    da1.From = (Color)ColorConverter.ConvertFromString("#e2dfe5");
                    da1.To = Colors.Yellow;
                    da1.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da1.AutoReverse = true;
                    da1.RepeatBehavior = RepeatBehavior.Forever;
                    lblState.Background.BeginAnimation(SolidColorBrush.ColorProperty, da1);
                }
                else if (_qa == ObjectDic.Instance.GetObjectName("검사중"))
                {
                    lblLotID.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0c24c"));
                    lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                    lblState.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0c24c"));
                    lblState.Foreground = new SolidColorBrush(Colors.Black);

                    LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                  

                    ColorAnimation da = new ColorAnimation();
                    da.From = (Color)ColorConverter.ConvertFromString("#e2dfe5");
                    da.To = (Color)ColorConverter.ConvertFromString("#e0c24c");
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    lblLotID.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                    ColorAnimation da1 = new ColorAnimation();
                    da1.From = (Color)ColorConverter.ConvertFromString("#e2dfe5");
                    da1.To = (Color)ColorConverter.ConvertFromString("#e0c24c");
                    da1.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da1.AutoReverse = true;
                    da1.RepeatBehavior = RepeatBehavior.Forever;
                    lblState.Background.BeginAnimation(SolidColorBrush.ColorProperty, da1);
                }
                else if (_qa == ObjectDic.Instance.GetObjectName("검사중경과시간초과")) //검사중 시간 초과
                {
                    lblLotID.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0c24c"));
                    lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                    lblState.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0c24c"));
                    lblState.Foreground = new SolidColorBrush(Colors.Black);

                    LineTop.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineTop.BorderThickness = new Thickness(2, 2, 2, 0);
                    LineMid.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineMid.BorderThickness = new Thickness(2, 0, 2, 0);
                    LineBtm.BorderBrush = new SolidColorBrush(Colors.Red);
                    LineBtm.BorderThickness = new Thickness(2, 0, 2, 2);

                    ColorAnimation da = new ColorAnimation();
                    da.From = Colors.White;
                    da.To = (Color)ColorConverter.ConvertFromString("#e0c24c");
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    lblLotID.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                    ColorAnimation da1 = new ColorAnimation();
                    da1.From = Colors.White;
                    da1.To = (Color)ColorConverter.ConvertFromString("#e0c24c");
                    da1.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da1.AutoReverse = true;
                    da1.RepeatBehavior = RepeatBehavior.Forever;
                    lblState.Background.BeginAnimation(SolidColorBrush.ColorProperty, da1);
                }
                else if (_qa == ObjectDic.Instance.GetObjectName("특별관리")) //특별관리
                {
                    lblLotID.Background = new SolidColorBrush(Colors.Black);
                    lblLotID.Foreground = new SolidColorBrush(Colors.White);

                    lblState.Background = new SolidColorBrush(Colors.Black);
                    lblState.Foreground = new SolidColorBrush(Colors.White);

                    LineTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineMid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                    LineBtm.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
                }
               if (_portstat == "UR" && _mtrlexistflag == "Y" && _lotId == string.Empty) //UR,Y인데 매핑된 LOT이 없음
               {
                    lblState.Text = " " + ObjectDic.Instance.GetObjectName("LOT 없음");
                    lblLotID.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2dfe5"));
                    lblLotID.Foreground = new SolidColorBrush(Colors.Black);

                    lblState.Background = new SolidColorBrush(Colors.Red);
                    lblState.Foreground = new SolidColorBrush(Colors.White);

                    lblPort.Background = new SolidColorBrush(Colors.Red);
                    lblPort.Foreground = new SolidColorBrush(Colors.White);
                }

            }
        }
       

        #endregion
    }
}