/*************************************************************************************
 Created Date : 2019.01.08
      Creator : 오화백K
   Decription : CWA 물류 - 라미대기창고모니터링 UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2019.01.08   오화백   : 라미 Rack Stair UserControl 
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
    public partial class AssmLamiRack_CheckBox : UserControl
    {
        #region Declaration & Constructor 

        private AssmLamiRack_CheckBox[][] _racks;
        public DockPanel RootLayout;
        private Color TypeABackColor = Colors.White;
        private Color TypeAForeColor = Colors.Black;
        // TypeA 3일이내
        private Color issueDayTypeABackColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
        private Color issueDayTypeAForeColor = Colors.Black;
        private int issueDayTypeADay = 3;
        // TypeB 7일이내
        private Color issueDayTypeBBackColor = (Color)ColorConverter.ConvertFromString("#FFF2F2F2");
        private Color issueDayTypeBForeColor = Colors.Black;
        private int issueDayTypeBDay = 7;
        // TypeC 30일이내
        private Color issueDayTypeCBackColor = (Color)ColorConverter.ConvertFromString("#FF558ED5");
        private Color issueDayTypeCForeColor = Colors.Black;
        private int issueDayTypeCDay = 30;
        // TypeD 30일이상
        private Color issueDayTypeDBackColor = (Color)ColorConverter.ConvertFromString("#FF8EB4E3");
        private Color issueDayTypeDForeColor = Colors.White;
        private int issueDayTypeDDay = 30;

        private Color portColor = (Color)ColorConverter.ConvertFromString("#FFD99694");
        private Color portMaterialColor = (Color)ColorConverter.ConvertFromString("#FFC3D69B");
        private Color unuseColor = (Color)ColorConverter.ConvertFromString("#FF000000");
        private Color checkColor = (Color)ColorConverter.ConvertFromString("#FFFFFF00");
        private Color errorColor = (Color)ColorConverter.ConvertFromString("#FFE46C0A");



        // Lot Id
        private string _lotId = string.Empty;

        // 
        private string _judg = string.Empty;
        // Rack Id
        private string _rackId = string.Empty;
        // 경과일수
        private int _elapseday = 0;
        // WipHold 여부
        private string _wipHold = string.Empty;
        // Rack State Code
        private string _rackStateCode = string.Empty;
        //극성정보
        private string _prdt_CLSS_CODE = string.Empty;
        //특별관리
        private string _spcl_Flag = string.Empty;
        //코드관리
        private string _skidID = string.Empty;
        //재공노트
        private string _wip_Remarks = string.Empty;
        //프로젝트명
        private string _project = string.Empty;

        //Rack인치 Port인지 구분
        private string _Check = string.Empty;

        // 열
        private int _row = -1;
        // 연
        private int _col = -1;
        // 단
        private int _stair = -1;

 
        
        // 사용자정의 필드
        private readonly Dictionary<string, object> _userDic = new Dictionary<string, object>();

        private bool _doubleClicked = false;
        private bool _mouseEnter = false;
        public event EventHandler<RoutedEventArgs> Checked;
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> DoubleClick;
       

        public event EventHandler ColChanged;
        public event EventHandler RowChanged;

        public AssmLamiRack_CheckBox()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            rootLayout.Background = new SolidColorBrush(TypeABackColor);
            Foreground = new SolidColorBrush(TypeAForeColor);
             //SetRackBackcolor();
        }
        /// <summary>
        /// TypeA (3일이내) 배경
        /// </summary>
        public Color IssueDayTypeABackColor
        {
            get
            {
                return issueDayTypeABackColor;
            }
            set
            {
                issueDayTypeABackColor = value;
            }
        }

        /// <summary>
        /// TypeA (3일이내) 글자
        /// </summary>
        public Color IssueDayTypeAForeColor
        {
            get
            {
                return issueDayTypeAForeColor;
            }
            set
            {
                issueDayTypeAForeColor = value;
            }
        }
        /// <summary>
        /// TypeA (3일이내) 날짜
        /// </summary>
        public int IssueDayTypeADay
        {
            get
            {
                return issueDayTypeADay;
            }
            set
            {
                issueDayTypeADay = value;
            }
        }

        /// <summary>
        /// TypeB (7일이내) 배경
        /// </summary>
        public Color IssueDayTypeBBackColor
        {
            get
            {
                return issueDayTypeBBackColor;
            }
            set
            {
                issueDayTypeBBackColor = value;
            }
        }
        /// <summary>
        /// TypeB (7일이내) 글자
        /// </summary>
        public Color IssueDayTypeBForeColor
        {
            get
            {
                return issueDayTypeBForeColor;
            }
            set
            {
                issueDayTypeBForeColor = value;
            }
        }

        /// <summary>
        /// TypeB (7일이내) 날짜
        /// </summary>
        public int IssueDayTypeBDay
        {
            get
            {
                return issueDayTypeBDay;
            }
            set
            {
                issueDayTypeBDay = value;
            }
        }

        /// <summary>
        /// TypeC (30일이내) 배경
        /// </summary>
        public Color IssueDayTypeCBackColor
        {
            get
            {
                return issueDayTypeCBackColor;
            }
            set
            {
                issueDayTypeCBackColor = value;
            }
        }

        /// <summary>
        /// TypeC (30일이내) 글자
        /// </summary>
        public Color IssueDayTypeCForeColor
        {
            get
            {
                return issueDayTypeCForeColor;
            }
            set
            {
                issueDayTypeCForeColor = value;
            }
        }
        /// <summary>
        /// TypeC (30일이내) 날짜
        /// </summary>
        public int IssueDayTypeCDay
        {
            get
            {
                return issueDayTypeCDay;
            }
            set
            {
                issueDayTypeCDay = value;
            }
        }
        /// <summary>
        /// TypeD (30일이상) 배경
        /// </summary>
        public Color IssueDayTypeDBackColor
        {
            get
            {
                return issueDayTypeDBackColor;
            }
            set
            {
                issueDayTypeDBackColor = value;
            }
        }

        /// <summary>
        /// TypeD (30일이상) 글자
        /// </summary>
        public Color IssueDayTypeDForeColor
        {
            get
            {
                return issueDayTypeDForeColor;
            }
            set
            {
                issueDayTypeDForeColor = value;
            }
        }
        /// <summary>
        /// TypeD (30일이상) 날짜
        /// </summary>
        public int IssueDayTypeDDay
        {
            get
            {
                return issueDayTypeDDay;
            }
            set
            {
                issueDayTypeDDay = value;
            }
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


     

        /// <summary>
		/// Check box check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChecked(object sender, RoutedEventArgs e)
        {
            Checked?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }

        #endregion

        #region Method
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
            }
        }
        /// <summary>
        /// JUDG_VALUE 
        /// </summary>
        public string JUDG_VALUE
        {
            get
            {
                return _judg;
            }
            set
            {
                _judg = value;
                if (_lotId.Length > 10)
                {
                    lblLotId1.Text = _judg.Substring(0, _judg.ToString().IndexOf(","));
                    lblLotId2.Text = _judg.Substring(_judg.ToString().IndexOf(",") + 1, (_judg.Length - 1) - _judg.ToString().IndexOf(","));
                }
                else
                {
                    lblLotId1.Text = _judg;
                }


            }
        }
        /// <summary>
        /// 프로젝트명
        /// </summary>
        public string ProjectName
        {
            get
            {
                return _project;
            }
            set
            {
                _project = value;

                if (_project.Trim().Length > 0)
                {
                    this.lblProject.Text = _project;
                }

            }
        }

        /// <summary>
        /// WIP_REMARKS
        /// </summary>
        public string Wip_Remarks
        {
            get
            {
                return _wip_Remarks;
            }
            set
            {
                _wip_Remarks = value;
            }
        }
        /// <summary>
        /// PORT 여부 설정
        /// </summary>
        public string SkidID
        {
            get
            {
                //return lblSkidId.Text;
                return _skidID;
            }
            set
            {
                //lblSkidId.Text = value;
                _skidID = value;
                if (_skidID != string.Empty&&_skidID.Substring(0,4) == "PORT")
                {
                    lblProject.Text = _skidID;
                    if (_wip_Remarks == "Y")
                    {
                        rootLayout.Background = new SolidColorBrush(portMaterialColor);
                    }
                    else
                    {
                        rootLayout.Background = new SolidColorBrush(portColor);
                    }
                    this.Foreground = new SolidColorBrush(Colors.Black);

                    this.check.Visibility = Visibility.Hidden;

                    if (_project == "T" || _project == "U" || _project == "F")
                    {
                        ColorAnimation da = new ColorAnimation();

                        if (_wip_Remarks == "Y")
                        {
                            da.From = portMaterialColor;
                        }
                        else
                        {
                            da.From = portColor;
                        }
   
                        da.To = Colors.White;
                        da.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da.AutoReverse = true;
                        da.RepeatBehavior = RepeatBehavior.Forever;
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                    }
                    else
                    {
                        try
                        {
                            rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        }
                        catch
                        {
                        }
                    }
                }
                else if (_skidID == "UNUSE")
                {
                    lblProject.Text = "UNUSE";
                    rootLayout.Background = new SolidColorBrush(unuseColor);
                }
                else if (_skidID == "ERROR")
                {
                    lblProject.Text = "ERROR";
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }

                    this.check.Visibility = Visibility.Visible;
                    rootLayout.Background = new SolidColorBrush(errorColor);
                    this.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (_skidID == "CHECK")
                {
                    lblProject.Text = "CHECK";
                    rootLayout.Background = new SolidColorBrush(checkColor);
                    this.check.Visibility = Visibility.Hidden;
                    ColorAnimation da = new ColorAnimation();
                    da.From = checkColor;
                    da.To = Colors.White;
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                }
               
                else if (_project == "")
                {
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }
                    this.check.Visibility = Visibility.Collapsed;
                }
             
            }
        }
        /// <summary>
        /// 극성
        /// </summary>
        public string PRDT_CLSS_CODE
        {
            get
            {
                return _prdt_CLSS_CODE;
            }
            set
            {
                _prdt_CLSS_CODE = value;

                if (_skidID == "PORT")
                    return;
                if (_prdt_CLSS_CODE == "C")
                {
                    lblProject.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    lblProject.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        /// <summary>
        /// 입고 경과일
        /// </summary>
        public int ElapseDay
        {
            get
            {
                return _elapseday;
            }
            set
            {
                _elapseday = value;
                if (_elapseday <= issueDayTypeADay)
                {
                    rootLayout.Background = new SolidColorBrush(issueDayTypeABackColor);
                    this.Foreground = new SolidColorBrush(issueDayTypeAForeColor);
                }
                else if (_elapseday <= IssueDayTypeBDay)
                {
                    rootLayout.Background = new SolidColorBrush(issueDayTypeBBackColor);
                    this.Foreground = new SolidColorBrush(issueDayTypeBForeColor);
                }
                else if (_elapseday <= IssueDayTypeCDay)
                {
                    rootLayout.Background = new SolidColorBrush(issueDayTypeCBackColor);
                    this.Foreground = new SolidColorBrush(issueDayTypeCForeColor);
                }
                else if (_elapseday > IssueDayTypeDDay)
                {
                    rootLayout.Background = new SolidColorBrush(issueDayTypeDBackColor);
                    this.Foreground = new SolidColorBrush(issueDayTypeDForeColor);
                }
                else
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                    this.Foreground = new SolidColorBrush(Colors.Black);
                }
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
                if (_spcl_Flag == "Y")
                {
                    lblProject.Background = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    lblProject.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        /// <summary>
        /// HOLD 여부
        /// </summary>
        public string WipHold
        {
            get { return _wipHold; }
            set
            {
                _wipHold = value;
                if (_wipHold == "Y")
                {
                    lblLotId1.Background = new SolidColorBrush(Colors.Red);
                    lblLotId2.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblLotId1.Background = new SolidColorBrush(Colors.Transparent);
                    lblLotId2.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        /// <summary>
        /// RACKID관리
        /// </summary>
        public string RackId
        {
            get { return _rackId; }
            set
            {
                _rackId = value;
            }
        }

        /// <summary>
        /// Rack인지 Port인지 체크
        /// </summary>
        public string Check
        {
            get { return _Check; }
            set
            {
                _Check = value;
            }
        }

        /// <summary>
        /// Check 여부
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return check.IsChecked != null && check.IsChecked.Value;
            }
            set
            {
                check.IsChecked = value;
            }
        }

        /// <summary>
        /// Check box 비활성화 여부
        /// </summary>
        public bool IsCheckEnabled
        {
            get
            {
                return check.IsEnabled;
            }
            set
            {
                check.IsEnabled = value;
            }
        }


        public new bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
                if (!base.IsEnabled)
                {

                }
                else
                {

                }
            }
        }


        /// <summary>
        /// 열 번호
        /// </summary>
        public int Row
        {
            get
            {
                return _row;
            }
            set
            {
                if (_row != value)
                {
                    _row = value;
                    RowChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 연 번호
        /// </summary>
        public int Col
        {
            get
            {
                return _col;
            }
            set
            {
                if (_col != value)
                {
                    _col = value;
                    ColChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// 단 
        /// </summary>
        public int Stair
        {
            get
            {
                return _stair;
            }
            set
            {
                _stair = value;
            }
        }
   

        public void Clear()
        {
            IsChecked = false;
            check.IsEnabled = true;
            _row = -1;
            _col = -1;
            _stair = -1;
            RackId = string.Empty;
            ProjectName = string.Empty;
            LotId = string.Empty;
            lblLotId1.Text = string.Empty;
            lblLotId2.Text = string.Empty;
            _userDic.Clear();
        }


    


     
       
       

        #endregion
    }
}