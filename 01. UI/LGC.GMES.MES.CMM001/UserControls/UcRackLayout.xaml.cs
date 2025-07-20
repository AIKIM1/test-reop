/*************************************************************************************
 Created Date : 2020.07.22
      Creator : 신광희C
   Decription : 물류관리 - STK 재공현황 LOT Layout 탭 UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2020.07.22   신광희C   : Initial Created.
   2022.08.26   오화백    : RACK 금지단 표시 추가
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcRackLayout.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRackLayout : UserControl
    {
        #region Declaration & Constructor 

        private UcRackLayout[][] _racks;
        public DockPanel RootLayout;

        // Lot Id
        private string _lotId = string.Empty;

        // WipHold 여부
        private string _wipHold = string.Empty;

        // Carrier 불량여부
        private string _carrierDefectFlag = string.Empty;

        // Rack State Code
        private string _rackStatus = string.Empty;

        // 배경 색
        private string _legendColor = string.Empty;

        // SKID 구분 1.실보빈(LOT존재),2.공보빈스키드,3.공스키드
        private string _skidType = string.Empty;

        private string _legendColorType = string.Empty;

        private string _holdFlag = string.Empty;

        // 열
        private int _row = -1;
        // 연
        private int _col = -1;
        // 단
        private int _stair = -1;

        // 사용자정의 필드
        private readonly Dictionary<string, object> _userDic = new Dictionary<string, object>();

        private bool _doubleClicked = false;

        public event EventHandler<RoutedEventArgs> Checked;
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> DoubleClick;

        public event EventHandler ColChanged;
        public event EventHandler RowChanged;

        public UcRackLayout()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            rootLayout.Background = new SolidColorBrush(RootLayoutBackColor);
            Foreground = new SolidColorBrush(RootLayoutForegroundColor);
            //grdHoldLot.Visibility = Visibility.Hidden;
           
        }



        public Color RootLayoutBackColor { get; set; } = Colors.White;

        public Color RootLayoutForegroundColor { get; set; } = Colors.Black;

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


        public string LotId
        {
            get
            {
                return _lotId;
            }
            set
            {
                _lotId = value;
                //lblLotId.Text = _lotId;
            }
        }


        /// <summary>
        /// Rack Id
        /// </summary>
        public string RackId { get; set; } = string.Empty;

        public string SkidCarrierCode { get; set; } = string.Empty;

        public string SkidCarrierProductCode { get; set; } = string.Empty;

        public string SkidCarrierProductName { get; set; } = string.Empty;

        public string BobbinCarrierCode { get; set; } = string.Empty;

        public string BobbinCarrierProductCode { get; set; } = string.Empty;

        public string BobbinCarrierProductName { get; set; } = string.Empty;

        public string AbnormalTransferReasonCode { get; set; } = string.Empty;

        //public string LegendColorType { get; set; } = string.Empty;

        public string LegendColorType
        {
            get { return _legendColorType; }
            set
            {
                _legendColorType = value;
                if (_legendColorType == "0")
                {
                    check.Visibility = Visibility.Collapsed;
                }
                else
                    check.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// Rack State Code
        /// </summary>
        public string RackStateCode
        {
            get
            {
                return _rackStatus;
            }
            set
            {
                if (value.ToString().ToUpper() == "DISABLE")

                {

                    bd.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    bd.BorderThickness = new Thickness(3, 3, 3, 3);

                }
             

                _rackStatus = value;

            }
        }

        public string LegendColor
        {
            get { return _legendColor; }
            set
            {
                _legendColor = value;

                if (!string.IsNullOrEmpty(_legendColor))
                {
                    var convertFromString = ColorConverter.ConvertFromString(_legendColor);
                    if (convertFromString != null) rootLayout.Background = new SolidColorBrush((Color)convertFromString);
                }
                else
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        public string SkidType
        {
            get { return _skidType; }
            set
            {
                _skidType = value;
                //1.실보빈(LOT존재),2.공보빈스키드,3.공스키드

                if (_skidType.Equals("1"))
                {
                    //실보빈스키드 : PJT, LOTID 표시
                    lblProject.Text = ProjectName;
                    lblLotId.Text = LotId;
                }
                else if (_skidType.Equals("2"))
                {
                    //공보빈스키드 : 보빈의 CSTPROD 및 보빈ID 표시
                    lblProject.Text = BobbinCarrierProductCode;
                    lblLotId.Text = BobbinCarrierCode;
                }
                else if (_skidType.Equals("3"))
                {
                    //공스키드 : 스키드의 CSTPROD 및 스키드ID 표시 
                    lblProject.Text = SkidCarrierProductCode;
                    lblLotId.Text = SkidCarrierCode;
                }
                else
                {
                    lblProject.Text = string.Empty;
                    lblLotId.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Carrier 불량여부
        /// </summary>
        public string CarrierDefectFlag
        {
            get { return _carrierDefectFlag; }
            set
            {
                _carrierDefectFlag = value;
                Foreground = _carrierDefectFlag == "Y" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }
        }


        /// <summary>
        /// 프로젝트명
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;


        /// <summary>
        /// HOLD 여부
        /// </summary>
        public string WipHold
        {
            get { return _wipHold; }
            set
            {
                _wipHold = value;
                //Foreground = _wipHold == "Y" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }
        }

        public string HoldFlag
        {
            get { return _holdFlag; }
            set
            {
                //'' 정상, 'M' MES HOLD, 'Q' QMS HOLD
                _holdFlag = value;
                lblHoldLot.Text = _holdFlag;
                Foreground = (_holdFlag == "M" || _holdFlag == "Q") ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// 사용자 정의 데이터
        /// </summary>
        public Dictionary<string, object> UserData
        {
            get
            {
                return _userDic;
            }
        }



        public void Clear()
        {
            IsChecked = false;
            RackStateCode = string.Empty;
            check.IsEnabled = true;
            _row = -1;
            _col = -1;
            _stair = -1;
            RackId = string.Empty;
            ProjectName = string.Empty;
            lblProject.Text = string.Empty;
            lblHoldLot.Text = string.Empty;
            HoldFlag = string.Empty;
            LotId = string.Empty;
            lblLotId.Text = string.Empty;
            _userDic.Clear();
        }


        /// <summary>
        /// UcRackLayout 찾음
        /// </summary>
        /// <param name="row">열 index</param>
        /// <param name="col">연 index</param>
        /// <returns></returns>
        public UcRackLayout this[int row, int col] => _racks[row][col];

        /// <summary>
        /// 이름으로 Rack 찾음(rXcXX ex:r0c03)
        /// </summary>
        /// <param name="name">이름(rXcXX ex:r0c03)</param>
        /// <returns></returns>
        public UcRackLayout this[string name]
        {
            get
            {
                for (int r = 0; r < _racks.Length; r++)
                {
                    for (int c = 0; c < _racks[r].Length; c++)
                    {
                        if (_racks[r][c].Name.Equals(name))
                        {
                            return _racks[r][c];
                        }
                    }
                }
                return null;
            }
        }

        #endregion
    }
}