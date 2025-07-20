/*************************************************************************************
 Created Date : 2020.11.10
      Creator : 김길용
   Decription : CWA 물류 - 창고모니터링 UserControl
--------------------------------------------------------------------------------------
 [Change History]
      Date         Author      CSR         Description...
   2020.11.10      김길용       SI         Rack Stair UserControl 
   2021.02.04      정용석       SI         보관일자에 따른 배경색 지정 부분 수정
   2021.04.02      김길용       SI         전극 설비정보(COT,PKG) 분할하여 항목 추가
   2024.09.09      권성혁     E20240822-001209 활성화에서 전송해준 편차(GAP 일수)에 따른 색상 추가
   2025.04.14      김선준       SI         ReTray일 경우 상단에 'R'표시   
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001.Controls
{
    /// <summary>
    /// UcRackStair.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Rack_CheckBox : UserControl
    {
        #region Declaration & Constructor 

        private Rack_CheckBox[][] _racks;
        public DockPanel RootLayout;
        private Color TypeABackColor = Colors.White;
        private Color TypeAForeColor = Colors.Black;
        private Color TypeRed = Colors.Red;
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

        // Rack Id
        private string _rackId = string.Empty;
        // Pallet ID
        private string palletID = string.Empty;
        // 구루마ID
        private string cstID = string.Empty;
        // 제품ID
        private string prodID = string.Empty;
        // 라인ID
        private string _lineID = string.Empty;
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
        //Rack인치 Port인지 구분
        private string _Check = string.Empty;
        //극성 호기(전극)
        private string _cot_abbr_code = string.Empty;
        //극성 호기(조립)
        private string _pkg_abbr_code = string.Empty;
        // 열
        private int _row = -1;
        // 연
        private int _col = -1;
        // 단
        private int _stair = -1;
        //편차 일수
        private string _gap_Date = string.Empty;
        //ReTray표시
        private string _RETRAY_DISP = string.Empty;

        // 사용자정의 필드
        private readonly Dictionary<string, object> _userDic = new Dictionary<string, object>();

        private bool _doubleClicked = false;
        private bool _mouseEnter = false;
        public event EventHandler<RoutedEventArgs> Checked;
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> DoubleClick;

        public event EventHandler ColChanged;
        public event EventHandler RowChanged;

        public Rack_CheckBox()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            //rootLayout.Background = new SolidColorBrush(TypeABackColor);
            //lblProject.Foreground = new SolidColorBrush(Colors.Black);
            //lblLotId1.Foreground = new SolidColorBrush(Colors.Black);
            //lblLotId2.Foreground = new SolidColorBrush(Colors.Black);
            //Foreground = new SolidColorBrush(TypeAForeColor);
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
        /// Pallet ID
        /// </summary>
        public string PalletID
        {
            get
            {
                return palletID;
            }
            set
            {
                palletID = value;

                if (palletID.Trim().Length > 0)
                {
                    this.txtPalletID.Text = palletID;
                }
            }
        }
        /// <summary>
        /// 제품 ID
        /// </summary>
        public string ProdID
        {
            get
            {
                return prodID;
            }
            set
            {
                prodID = value;

                if (prodID.Trim().Length > 0)
                {
                    this.txtProdID.Text = prodID;
                }

            }
        }
        /// <summary>
        /// 라인ID
        /// </summary>
        public string LineID
        {
            get
            {
                return _lineID;
            }
            set
            {
                _lineID = value;

                if (_lineID.Trim().Length > 0)
                {
                    this.txtLineID.Text = "/ " + _lineID;
                }

            }
        }
        /// <summary>
        /// 구루마 ID
        /// </summary>
        public string CSTID
        {
            get
            {
                return this.cstID;
            }
            set
            {
                this.cstID = value;

                if (this.cstID.Trim().Length > 0)
                {
                    this.txtCSTID.Text = "(" + this.cstID + ")";
                }
            }
        }

        /// <summary>
        /// 전극설비정보
        /// </summary>
        public string CAbbr_Code
        {
            get
            {
                return _cot_abbr_code;
            }
            set
            {
                _cot_abbr_code = value;
                if (_cot_abbr_code.Trim().Length > 0 && palletID.Trim().Length > 0)
                {
                    this.CotAbbr_code.Text = _cot_abbr_code;

                }
            }
        }
        /// <summary>
        /// 조립설비정보
        /// </summary>
        public string PAbbr_Code
        {
            get
            {
                return _pkg_abbr_code;
            }
            set
            {
                _pkg_abbr_code = value;
                if (_pkg_abbr_code.Trim().Length > 0 && palletID.Trim().Length > 0)
                {
                    this.PkgAbbr_code.Text = "/ " + _pkg_abbr_code;

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
                if (_wip_Remarks == "N")
                {
                    this.check.Visibility = Visibility.Hidden;
                }
            }
        }
        /// <summary>
        /// PORT 여부 설정
        /// </summary>
        public string SkidID
        {
            get
            {
                return _skidID;
            }
            set
            {
                _skidID = value;
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

            }
        }
        /// <summary>
        /// Rack State Code
        /// </summary>
        /// <returns></returns>
        public string RackStateCode
        {
            get
            {
                return _rackStateCode;
            }
            set
            {
                _rackStateCode = value;
                //if (_rackStateCode == "HOLD")
                //{
                //    rootLayout.Background = new SolidColorBrush(TypeRed);
                //    ColorAnimation da = new ColorAnimation();
                //    da.From = TypeRed;
                //    da.To = Colors.White;
                //    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                //    da.AutoReverse = true;
                //    da.RepeatBehavior = RepeatBehavior.Forever;
                //    rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                //}
                //else
                //{
                //    try
                //    {
                //        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                //    }
                //    catch
                //    {
                //    }
                //}
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

                if (_spcl_Flag == "Y")
                {
                    //lstLegendElement.Add(new Tuple<string, Brush, Color>(ObjectDic.Instance.GetObjectName("정상")
                    //                                                   , (SolidColorBrush)(new BrushConverter().ConvertFrom("#90EE90"))
                    //                                                   //, (Color)System.Windows.Media.ColorConverter.ConvertFromString("#90EE90")));    // Green
                    //                                                   , Colors.Black));
                    //lstLegendElement.Add(new Tuple<string, Brush, Color>(ObjectDic.Instance.GetObjectName("D+3미만")
                    //                                                   , (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA500"))
                    //                                                   //, (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500")));    // 귤색
                    //                                                   , Colors.Black));
                    //lstLegendElement.Add(new Tuple<string, Brush, Color>(ObjectDic.Instance.GetObjectName("D+3이상")
                    //                                                   , (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000"))
                    //                                                   //, (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000")));    // 빨강색
                    //                                                   , Colors.Black));

                    if (_elapseday < 3)
                    {
                        rootLayout.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA500"));
                    }
                    if (_elapseday >= 3)
                    {
                        rootLayout.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000"));
                    }
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
                if (_spcl_Flag == "N")
                {
                    rootLayout.Background = new SolidColorBrush(Colors.LightGreen);
                }
            }
        }

        public string Gap_Date
        {
            get
            {
                return _gap_Date;
            }
            set
            {
                _gap_Date = value;

                if (_spcl_Flag == "N")
                {
                    if (_gap_Date == "Y")
                    {
                        rootLayout.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D3D3D3"));
                    }
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
                //if (_wip_Remarks == "Y")
                //{
                //
                //    rootLayout.Background = new SolidColorBrush(TypeRed);
                //
                //    if (_wipHold == "Y")
                //    {
                //        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                //    }
                //    else if (_wipHold == "N")
                //    {
                //        rootLayout.Background = new SolidColorBrush(checkColor);
                //        ColorAnimation da = new ColorAnimation();
                //        da.From = checkColor;
                //        da.To = Colors.White;
                //        da.Duration = new Duration(TimeSpan.FromSeconds(1));
                //        da.AutoReverse = true;
                //        da.RepeatBehavior = RepeatBehavior.Forever;
                //        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                //    }
                //    else
                //    {
                //        try
                //        {
                //            rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                //        }
                //        catch
                //        {
                //        }
                //    }
                //}
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
        /// Retray여부
        /// </summary>
        public string RETRAY_DISP
        {
            get
            {
                return _RETRAY_DISP;
            }
            set
            {
                _RETRAY_DISP = value;
                this.tbTray.Text = _RETRAY_DISP;
                if (!string.IsNullOrEmpty(_RETRAY_DISP))
                {
                    this.vbTray.Visibility = Visibility.Visible;
                    this.tbTray.Foreground = (_RETRAY_DISP.Equals("B")) ? Brushes.Red : Brushes.Blue;
                }
                else
                {
                    this.vbTray.Visibility = Visibility.Collapsed;
                }
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
            ProdID = string.Empty;
            PalletID = string.Empty;
            this.txtPalletID.Text = string.Empty;
            this.txtProdID.Text = string.Empty;
            this.tbTray.Text = string.Empty;
            this.vbTray.Visibility = Visibility.Collapsed;
            _userDic.Clear();
        }
        #endregion
    }
}