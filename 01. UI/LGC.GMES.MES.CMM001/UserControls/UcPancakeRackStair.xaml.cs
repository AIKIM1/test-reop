/*************************************************************************************
 Created Date : 2018.10.25
      Creator : 신광희C
   Decription : CWA 물류 - Pancake 모니터링 UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2019.01.08   신광희C   : Pancake Rack Stair UserControl Initial Created.
   2023.12.18   오수현    : E20231023-000294 T-BOX Stocker Rack별 전극 코터 설비 호기 표시 요청
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcPancakeRackStair.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPancakeRackStair : UserControl
    {
        #region Declaration & Constructor 

        private UcPancakeRackStair[][] _racks;
        public DockPanel RootLayout;

        // Lot Id
        private string _projectName = string.Empty;
        // Rack Id
        // 경과일수
        // WipHold 여부
        private string _wipHold = string.Empty;
        // Rack State Code
        private string _rackStateCode = string.Empty;
        // 가용 Lot Count
        private string _lotNormalCount = string.Empty;
        // Hold Lot Count
        private string _lotHoldCount = string.Empty;
        // 극성
        private string _productClassCode = string.Empty;

        private string _legendColor = string.Empty;

        private string _qms = string.Empty;
        // 전극 COATER 설비 호기 정보 - E20231023-000294
        private string _coatingEqptUnit = string.Empty;

        // 열
        private int _row = -1;
        // 연
        private int _col = -1;
        // 단

        // 사용자정의 필드
        private readonly Dictionary<string, object> _userDic = new Dictionary<string, object>();

        private bool _doubleClicked = false;
        private bool _mouseEnter = false;
        public event EventHandler<RoutedEventArgs> Checked;
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> DoubleClick;
        public event EventHandler<RoutedEventArgs> MouseEnter;

        public event EventHandler ColChanged;
        public event EventHandler RowChanged;

        public UcPancakeRackStair()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            rootLayout.Background = new SolidColorBrush(Colors.White);
            Foreground = new SolidColorBrush(Colors.Black);
            grdRackStat.Visibility = Visibility.Hidden;
            SetRackBackcolor();
        }

        public void SetRackBackcolor()
        {

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


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, new RoutedEventArgs());
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
        public int Stair { get; set; } = -1;

        public string RackId { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Rack State Code
        /// </summary>
        public string RackStateCode
        {
            get
            {
                return _rackStateCode;
            }
            set
            {
                _rackStateCode = value;
                
                /*
                // 출고예약
                if (_rackStateCode.Equals("ISS_RESERVE"))
                {
                    rootLayout.Background = new SolidColorBrush(Colors.Orange);
                }
                // 입고예약
                else if (_rackStateCode.Equals("RCV_RESERVE"))
                {
                    rootLayout.Background = new SolidColorBrush(Colors.Yellow);
                }
                // 상태확인
                //else if (_rackStateCode.Equals("STATUSCHECK"))
                //{
                //    rootLayout.Background = new SolidColorBrush(Colors.Blue);
                //}

                // CHECK 상태확인
                else if (_rackStateCode.Equals("CHECK"))
                {
                    var convertFromString = ColorConverter.ConvertFromString("#9C9C9C");
                    if (convertFromString != null)
                        rootLayout.Background = new SolidColorBrush((Color)convertFromString);

                }
                // 입고금지
                else if (_rackStateCode.Equals("UNUSE"))
                {
                    rootLayout.Background = new SolidColorBrush(Colors.LightGray); 
                }
                // 포트
                else if (_rackStateCode.Equals("PORT"))
                {
                    
                }
                // 사용중
                else if (_rackStateCode.Equals("USING"))
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                }
                // NO READ
                else if (_rackStateCode.Equals("NOREAD"))
                {
                    rootLayout.Background = new SolidColorBrush(Colors.AliceBlue);
                }
                else
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                }
                */
            }
        }

        /// <summary>
        /// 연결 세그먼트 ID
        /// </summary>
        public string LinqEquipmentSegmentCode { get; set; } = string.Empty;

        /// <summary>
        /// 입고 우선 순위
        /// </summary>
        public Int32 ReceivePriority { get; set; } = 0;

        /// <summary>
        /// 존 ID
        /// </summary>
        public string ZoneId { get; set; } = string.Empty;

        /// <summary>
        /// MCS캐리어 아이디
        /// </summary>
        public string DistributionCarrierId { get; set; } = string.Empty;

        /// <summary>
        /// 사용여부
        /// </summary>
        public string UseFlag { get; set; } = string.Empty;

        /// <summary>
        /// 설비 Id
        /// </summary>
        public string EquipmentCode { get; set; } = string.Empty;

        /// <summary>
        /// 유효일자
        /// </summary>
        public string ValidDate { get; set; } = string.Empty;

        /// <summary>
        /// 생산일자
        /// </summary>
        public string CalculationDate { get; set; } = string.Empty;

        /// <summary>
        /// 창고 입고 일시
        /// </summary>
        public string WarehouseReceiveDate { get; set; } = string.Empty;

        /// <summary>
        /// 제품 ID
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 프로세스 세그먼트 ID
        /// </summary>
        public string ProcessSegmentCode { get; set; }

        /// <summary>
        /// 프로세스 ID
        /// </summary>
        public string ProcessCode { get; set; }

        /// <summary>
        /// 재공 상태
        /// </summary>
        public string WipState { get; set; }

        /// <summary>
        /// 재공 생산 시작 일시
        /// </summary>
        public string WipStartDateTime { get; set; }

        /// <summary>
        /// 재공 수량 (투입)
        /// </summary>
        public decimal WipQty { get; set; }


        /// <summary>
        /// 재공 수량 (완성)
        /// </summary>
        public decimal WipQty2 { get; set; }

        /// <summary>
        /// 설비 세그먼트 ID
        /// </summary>
        public string EquipmentSegmentCode { get; set; } = string.Empty;

        /// <summary>
        /// 제품명
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 모델 ID
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// 단위
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// 프로젝트명
        /// </summary>
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                _projectName = value;
                lblProject.Text = _projectName;
            }
        }

        public string LotCount { get; set; }


        public string LotNormalCount
        {
            get { return _lotNormalCount; }
            set
            {
                _lotNormalCount = value;
                if (!string.IsNullOrEmpty(_lotNormalCount))
                {
                    lblNormalLotCount.Text = _lotNormalCount;
                    lblNormalLotCount.Foreground = new SolidColorBrush(Colors.Black);

                    parenthesisLeft.Text = "(";
                    parenthesisLeft.Foreground = new SolidColorBrush(Colors.Black);
                    parenthesisRight.Text = ")";
                    parenthesisRight.Foreground = new SolidColorBrush(Colors.Black);
                    slash.Text = "/";
                    slash.Foreground = new SolidColorBrush(Colors.Black);

                }
            }
        }

        public string LotHoldCount
        {
            get { return _lotHoldCount; }
            set
            {
                _lotHoldCount = value;
                if (!string.IsNullOrEmpty(_lotHoldCount))
                {
                    lblHoldLotCount.Text = _lotHoldCount;

                    //lblHoldLotCount.Foreground = _lotHoldCount.GetInt() > 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
                    if (_lotHoldCount.GetInt() > 0)
                    {
                        lblHoldLotCount.Foreground = new SolidColorBrush(Colors.Red);
                        lblHoldLotCount.Background = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        lblHoldLotCount.Foreground = new SolidColorBrush(Colors.Black);
                        lblHoldLotCount.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }

        public string ProductClassCode
        {
            get { return _productClassCode; }
            set
            {
                _productClassCode = value;

                if (string.IsNullOrEmpty(_productClassCode))
                {
                    lblProject.Foreground = new SolidColorBrush(Colors.SandyBrown);
                }
                else
                {
                    if (_productClassCode == "C")
                    {
                        lblProject.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (_productClassCode == "A")
                    {
                        lblProject.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }

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
                    if (convertFromString != null) rootLayout.Background = new SolidColorBrush((Color) convertFromString);
                }
                else
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        public string QMS
        {
            get { return _qms; }
            set
            {
                _qms = value;

                if (_qms == "FAIL")
                {
                    lblQMS.Text = "Q_F";
                    lblQMS.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblQMS.Text = "Q_F";
                    lblQMS.Foreground = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        public string CoatingEqptUnit
        {
            get { return _coatingEqptUnit; }
            set
            {
                _coatingEqptUnit = value;

                if (!string.IsNullOrEmpty(_coatingEqptUnit))
                {
                    lblCoatingEqptUnit.Text = _coatingEqptUnit;
                    lblCoatingEqptUnit.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        /// <summary>
        /// 프로세스명
        /// </summary>
        public string ProcessName { get; set; }


        /// <summary>
        /// 설비 세그먼트명
        /// </summary>
        public string EquipmentSegmentName { get; set; }

        /// <summary>
        /// 특별관리 여부
        /// </summary>
        public string SpecialFlag { get; set; }

        /// <summary>
        /// 경과 일수 (창고 입고)
        /// </summary>

        public int ElapseDay { get; set; } = 0;

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

        /// <summary>
        /// 사용자 정의 데이터
        /// </summary>
        public Dictionary<string, object> UserData => _userDic;


        public void Clear()
        {
            IsChecked = false;
            RackStateCode = string.Empty;
            check.IsEnabled = true;
            _row = -1;
            _col = -1;
            Stair = -1;
            RackId = string.Empty;
            ProjectName = string.Empty;
            lblProject.Text = string.Empty;
            lblNormalLotCount.Text = string.Empty;
            lblHoldLotCount.Text = string.Empty;
            _userDic.Clear();

            parenthesisLeft.Text = string.Empty;
            parenthesisRight.Text = string.Empty;
            slash.Text = string.Empty;
        }


        /// <summary>
        /// UcPancakeRackStair 찾음
        /// </summary>
        /// <param name="row">열 index</param>
        /// <param name="col">연 index</param>
        /// <returns></returns>
        public UcPancakeRackStair this[int row, int col] => _racks[row][col];

        /// <summary>
        /// 이름으로 Rack 찾음(rXcXX ex:r0c03)
        /// </summary>
        /// <param name="name">이름(rXcXX ex:r0c03)</param>
        /// <returns></returns>
        public UcPancakeRackStair this[string name]
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