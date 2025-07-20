/*************************************************************************************
 Created Date : 2018.10.25
      Creator : 신광희C
   Decription : CWA 물류 - 점보롤 창고 모니터링 UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2018.10.25   신광희C   : Rack Stair UserControl 
   2021.06.01   강동희C   : HALF 슬리팅 면 컬럼 추가 대응.
   2021.06.28   강동희    : QMS HOLD 여부 컬럼 추가 대응.
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

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcRackStair.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRackStair : UserControl
    {
        #region Declaration & Constructor 

        private UcRackStair[][] _racks;
        public DockPanel RootLayout;

        // Lot Id
        private string _lotId = string.Empty;
        // Rack Id
        // 경과일수
        // WipHold 여부
        private string _wipHold = string.Empty;
        // Rack State Code
        private string _rackStateCode = string.Empty;
        // 극성
        private string _productClassCode = string.Empty;
        // 배경 색
        private string _legendColor = string.Empty;
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
        public event EventHandler<RoutedEventArgs> MouseEnter;

        public event EventHandler ColChanged;
        public event EventHandler RowChanged;

        public UcRackStair()
        {
            InitializeComponent();
            RootLayout = rootLayout;
            rootLayout.Background = new SolidColorBrush(IssueDayTypeABackColor);
            Foreground = new SolidColorBrush(IssueDayTypeAForeColor);
            grdRackStat.Visibility = Visibility.Hidden;
            SetRackBackcolor();
        }

        public void SetRackBackcolor()
        {

        }


        /// <summary>
        /// TypeA (3일이내) 배경
        /// </summary>
        public Color IssueDayTypeABackColor { get; set; } = Colors.White;

        /// <summary>
        /// TypeA (3일이내) 글자
        /// </summary>
        public Color IssueDayTypeAForeColor { get; set; } = Colors.Black;

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
                lblLotId.Text = _lotId;
            }
        }

        public string RackId { get; set; } = string.Empty;

        /// <summary>
        /// Rack Id
        /// </summary>
        //public string RackId { get; set; } = string.Empty;

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
                //else if (_rackStateCode.Equals("USING"))
                //{
                //    rootLayout.Background = string.IsNullOrEmpty(_lotId) ? new SolidColorBrush(Colors.AliceBlue) : new SolidColorBrush(Colors.White);
                //}

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
        /// 재공 순번
        /// </summary>
        public decimal WipSeq { get; set; }

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
        /// Workorder ID
        /// </summary>
        public string WorkOrderId { get; set; }

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
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// 프로세스명
        /// </summary>
        public string ProcessName { get; set; }


        /// <summary>
        /// 설비 세그먼트명
        /// </summary>
        public string EquipmentSegmentName { get; set; }

        /// <summary>
        /// 제품버전
        /// </summary>
        public string ProductVersionCode { get; set; }

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

        public string ProductClassCode
        {
            get { return _productClassCode; }
            set
            {
                _productClassCode = value;
                if (_wipHold == "N")
                {
                    if (_productClassCode == "C")
                    {
                        Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (_productClassCode == "A")
                    {
                        Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        Foreground = new SolidColorBrush(Colors.SandyBrown);
                    }
                    lblLotId.Background = new SolidColorBrush(Colors.Transparent);
                }
                else if(_wipHold == "Y")
                {
                    lblLotId.Background = new SolidColorBrush(Colors.White);
                    Foreground = new SolidColorBrush(Colors.Red);
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
                    if (convertFromString != null) rootLayout.Background = new SolidColorBrush((Color)convertFromString);
                }
                else
                {
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        public string HaltSlitSide { get; set; } = string.Empty; //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응

        public string QmsHoldFlag  { get; set; } = string.Empty; //2021.06.28 QMS HOLD 여부 컬럼 추가 대응

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
            LotId = string.Empty;
            lblLotId.Text = string.Empty;
            _userDic.Clear();
        }


        /// <summary>
        /// UcRackStair 찾음
        /// </summary>
        /// <param name="row">열 index</param>
        /// <param name="col">연 index</param>
        /// <returns></returns>
        public UcRackStair this[int row, int col] => _racks[row][col];

        /// <summary>
        /// 이름으로 Rack 찾음(rXcXX ex:r0c03)
        /// </summary>
        /// <param name="name">이름(rXcXX ex:r0c03)</param>
        /// <returns></returns>
        public UcRackStair this[string name]
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