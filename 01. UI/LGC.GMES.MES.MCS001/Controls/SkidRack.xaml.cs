/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.MCS001.Controls
{
	/// <summary>
	/// SkidRack.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SkidRack : UserControl
	{
        #region Declaration & Constructor 
        // 입고금지
        public static Color lockBackColor = Colors.Black;
		public static Color lockForeColor = Colors.White;
		// Disable
		public static Color disableBackColor = Color.FromRgb( 0xEB, 0xEB, 0xEB );
		public static Color disableForeColor = SystemColors.GrayTextColor;

		// TypeA 3일이내
		private Color issueDayTypeABackColor = Colors.White;
		private Color issueDayTypeAForeColor = Colors.Black;
		private int issueDayTypeADay = 3;
		// TypeB 7일이내
		private Color issueDayTypeBBackColor = Colors.LightGreen;
		private Color issueDayTypeBForeColor = Colors.Black;
		private int issueDayTypeBDay = 7;
		// TypeC 30일이내
		private Color issueDayTypeCBackColor = Colors.Yellow;
		private Color issueDayTypeCForeColor = Colors.Black;
		private int issueDayTypeCDay = 30;
		// TypeD 30일이상
		private Color issueDayTypeDBackColor = Colors.LightCoral;
		private Color issueDayTypeDForeColor = Colors.White;
		private int issueDayTypeDDay = 30;


        private Color portColor = Colors.Pink;
        private Color portMaterialColor = Colors.Green;
        private Color unuseColor = Colors.Black;
        private Color checkColor = Colors.Blue;
        private Color errorColor = Colors.Red;     

		private bool crane = false;

		// Port Position
		private bool isPort = false;
		// 입고금지
		private bool lockWH = false;
		// 입고경과일
		private int elapseday = 0;
		// skid id
		private string skidId = string.Empty;
        // pancake id 1
        private string pancakeId1 = string.Empty;
        // pancake id 2
        private string pancakeId2 = string.Empty;
        // pancake id 3
        private string pancakeId3 = string.Empty;
        // pancake id 4
        private string pancakeId4 = string.Empty;
        // pancake id 5
        private string pancakeId5 = string.Empty;
        // pancake id 6
        private string pancakeId6 = string.Empty;
        // pancake QA 1
        private string pancakeQA1 = string.Empty;
        // pancake QA 2
        private string pancakeQA2 = string.Empty;
        // pancake QA 3
        private string pancakeQA3 = string.Empty;
        // pancake QA 4
        private string pancakeQA4 = string.Empty;
        // pancake QA 5
        private string pancakeQA5 = string.Empty;
        // pancake QA 6
        private string pancakeQA6 = string.Empty;
        // project
        private string project = string.Empty;
		// Rack id
		private string rackId = string.Empty;
		// Rack State
		private string rackStat = string.Empty;

        private string zoneId = string.Empty;


        private string prdt_CLSS_CODE = string.Empty;


        private string spcl_Flag = string.Empty;
        private string spcl_RsnCode = string.Empty;
        private string wip_Remarks = string.Empty;

        private string wipdttm_ED = string.Empty;


        private string wiphold1 = string.Empty;
        private string wiphold2 = string.Empty;
        private string wiphold3 = string.Empty;
        private string wiphold4 = string.Empty;
        private string wiphold5 = string.Empty;
        private string wiphold6 = string.Empty;

        // 열
        private int row = -1;
		// 연
		private int col = -1;
		// 단
		private int stair = -1;

		// 사용자정의 필드
		private Dictionary<string, object> userDic = new Dictionary<string, object>();

		private bool doubleClicked = false;
        private bool mouseEnter = false;

		public event EventHandler<RoutedEventArgs> Checked = null;
		public event EventHandler<RoutedEventArgs> Click = null;
		public event EventHandler<RoutedEventArgs> DoubleClick = null;
        public event EventHandler<RoutedEventArgs> MouseEnter = null;

        public event EventHandler ColChanged = null;
		public event EventHandler RowChanged = null;

		public SkidRack() {
			InitializeComponent();

			rootLayout.Background = new SolidColorBrush( issueDayTypeABackColor );
			this.Foreground = new SolidColorBrush( issueDayTypeAForeColor );

			grdRackStat.Visibility = Visibility.Hidden;

            SetRackBackcolor();
		}

        public void SetRackBackcolor()
        {
            DataTable RQDT = new DataTable("RQSTDT");
            RQDT.Columns.Add("LANGID", typeof(string));

            DataRow drColor = RQDT.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;

            RQDT.Rows.Add(drColor);

            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COLOR_LEGEND", "RQSTDT", "RSLTDT", RQDT);
            foreach (DataRow row in dtColorResult.Rows)
            {
                if (row["KEYID"].ToString() == "VD3")
                {
                    issueDayTypeABackColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "VD7")
                {
                    issueDayTypeBBackColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "VD30")
                {
                    issueDayTypeCBackColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "VD30_O")
                {
                    issueDayTypeDBackColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "PORT")
                {
                    portColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "PORT_MTRL")
                {
                    portMaterialColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "UNUSE")
                {
                    unuseColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "CHECK")
                {
                    checkColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
                else if (row["KEYID"].ToString() == "ERRORSKID")
                {
                    errorColor = (Color)ColorConverter.ConvertFromString(row["KEYVALUE"].ToString());
                }
            }
        }


		/// <summary>
		/// TypeA (3일이내) 배경
		/// </summary>
		public Color IssueDayTypeABackColor {
			get {
				return issueDayTypeABackColor;
			}
			set {
				issueDayTypeABackColor = value;
			}
		}

		/// <summary>
		/// TypeA (3일이내) 글자
		/// </summary>
		public Color IssueDayTypeAForeColor {
			get {
				return issueDayTypeAForeColor;
			}
			set {
				issueDayTypeAForeColor = value;
			}
		}
		/// <summary>
		/// TypeA (3일이내) 날짜
		/// </summary>
		public int IssueDayTypeADay {
			get {
				return issueDayTypeADay;
			}
			set {
				issueDayTypeADay = value;
			}
		}

		/// <summary>
		/// TypeB (7일이내) 배경
		/// </summary>
		public Color IssueDayTypeBBackColor {
			get {
				return issueDayTypeBBackColor;
			}
			set {
				issueDayTypeBBackColor = value;
			}
		}
		/// <summary>
		/// TypeB (7일이내) 글자
		/// </summary>
		public Color IssueDayTypeBForeColor {
			get {
				return issueDayTypeBForeColor;
			}
			set {
				issueDayTypeBForeColor = value;
			}
		}

		/// <summary>
		/// TypeB (7일이내) 날짜
		/// </summary>
		public int IssueDayTypeBDay {
			get {
				return issueDayTypeBDay;
			}
			set {
				issueDayTypeBDay = value;
			}
		}

		/// <summary>
		/// TypeC (30일이내) 배경
		/// </summary>
		public Color IssueDayTypeCBackColor {
			get {
				return issueDayTypeCBackColor;
			}
			set {
				issueDayTypeCBackColor = value;
			}
		}

		/// <summary>
		/// TypeC (30일이내) 글자
		/// </summary>
		public Color IssueDayTypeCForeColor {
			get {
				return issueDayTypeCForeColor;
			}
			set {
				issueDayTypeCForeColor = value;
			}
		}
		/// <summary>
		/// TypeC (30일이내) 날짜
		/// </summary>
		public int IssueDayTypeCDay {
			get {
				return issueDayTypeCDay;
			}
			set {
				issueDayTypeCDay = value;
			}
		}
		/// <summary>
		/// TypeD (30일이상) 배경
		/// </summary>
		public Color IssueDayTypeDBackColor {
			get {
				return issueDayTypeDBackColor;
			}
			set {
				issueDayTypeDBackColor = value;
			}
		}

		/// <summary>
		/// TypeD (30일이상) 글자
		/// </summary>
		public Color IssueDayTypeDForeColor {
			get {
				return issueDayTypeDForeColor;
			}
			set {
				issueDayTypeDForeColor = value;
			}
		}
		/// <summary>
		/// TypeD (30일이상) 날짜
		/// </summary>
		public int IssueDayTypeDDay {
			get {
				return issueDayTypeDDay;
			}
			set {
				issueDayTypeDDay = value;
			}
		}

        #endregion

        #region Event
        protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e ) {
			//base.OnMouseLeftButtonUp( e );
			if( doubleClicked ) {
				doubleClicked = false;
				return;
			}
			if( IsPortPosition == false ) {
				Click?.Invoke( this, new RoutedEventArgs() );
			}
			e.Handled = true;
		}

		protected override void OnMouseDoubleClick( MouseButtonEventArgs e ) {

            ////////////if (lblSkidId.Text == "PORT")
            ////////////{
            ////////////    return;
            ////////////}
                //base.OnMouseDoubleClick( e );
                doubleClicked = true;

			if( IsPortPosition == false ) {
				DoubleClick?.Invoke( this, new RoutedEventArgs() );
			}
			e.Handled = true;
		}


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (IsPortPosition == false)
            {
                MouseEnter?.Invoke(this, new RoutedEventArgs());
            }
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
        /// 입고 경과일
        /// </summary>
        public int ElapseDay {
			get {
				return elapseday;
			}
			set {
				elapseday = value;
                if (lockWH == false)
                {
                    if (elapseday <= IssueDayTypeADay)
                    {
                        rootLayout.Background = new SolidColorBrush(issueDayTypeABackColor);
                        this.Foreground = new SolidColorBrush(issueDayTypeAForeColor);
                    }
                    else if (elapseday <= IssueDayTypeBDay)
                    {
                        rootLayout.Background = new SolidColorBrush(issueDayTypeBBackColor);
                        this.Foreground = new SolidColorBrush(issueDayTypeBForeColor);
                    }
                    else if (elapseday <= IssueDayTypeCDay)
                    {
                        rootLayout.Background = new SolidColorBrush(issueDayTypeCBackColor);
                        this.Foreground = new SolidColorBrush(issueDayTypeCForeColor);
                    }
                    else if (elapseday > IssueDayTypeDDay)
                    {
                        rootLayout.Background = new SolidColorBrush(issueDayTypeDBackColor);
                        this.Foreground = new SolidColorBrush(issueDayTypeDForeColor);
                    }
                    else {
                        rootLayout.Background = new SolidColorBrush(Colors.White);
                        this.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
		}

		/// <summary>
		/// Check 여부
		/// </summary>
		public bool IsChecked {
			get {
				return check.IsChecked.Value;
			}
			internal set {
				check.IsChecked = value;
			}
		}

		/// <summary>
		/// Check box 비활성화 여부
		/// </summary>
		public bool IsCheckEnabled {
			get {
				return check.IsEnabled;
			}
			set {
				check.IsEnabled = value;
			}
		}

		public new bool IsEnabled {
			get {
				return base.IsEnabled;
			}
			set {
				base.IsEnabled = value;
				if( !base.IsEnabled ) {
					// disable
					if( LockWarehousing == false ) {
						rootLayout.Background = new SolidColorBrush( disableBackColor );
						this.Foreground = new SolidColorBrush( disableForeColor );
					}

				} else {
					// enable
					if( LockWarehousing == false ) {
						// 입고금지와 같은 로직으로 구성
						ElapseDay = elapseday;

						checkArea.Visibility = Visibility.Visible;
						lblProjectName.Visibility = Visibility.Visible;
                        lblPancake1.Visibility = Visibility.Visible;
                        lblPancake2.Visibility = Visibility.Visible;
                        lblPancake3.Visibility = Visibility.Visible;
                        lblPancake4.Visibility = Visibility.Visible;
                        lblPancake5.Visibility = Visibility.Visible;
                        lblPancake6.Visibility = Visibility.Visible;
                        //vbLock.Visibility = Visibility.Hidden;

                        rootLayout.Background = new SolidColorBrush(Colors.White);
                    }
				}
			}
		}

		public bool IsPortPosition {
			get {
				return isPort;
			}
			set {
				isPort = value;
				if( isPort ) {
					lblPortTypeName.Visibility = Visibility.Visible;
					check.Visibility = Visibility.Collapsed;
					/*
					check.Visibility = Visibility.Hidden;
					lblProjectName.Visibility = Visibility.Hidden;
					lblPancake1.Visibility = Visibility.Hidden;
					lblPancake2.Visibility = Visibility.Hidden;
					vbLock.Visibility = Visibility.Hidden;
					LinearGradientBrush linear = new LinearGradientBrush();
					linear.StartPoint = new Point( 0, 0 );
					linear.EndPoint = new Point( 1, 1 );
					linear.GradientStops.Add( new GradientStop( Colors.White, 0.0 ) );
					linear.GradientStops.Add( new GradientStop( Color.FromRgb( 150, 150, 150 ), 0.5 ) );
					linear.GradientStops.Add( new GradientStop( Colors.White, 1.0 ) );

					rootLayout.Background = linear;
					*/
				} else {
					check.Visibility = Visibility.Visible;
					lblPortTypeName.Visibility = Visibility.Collapsed;
					/*
					check.Visibility = Visibility.Visible;
					lblProjectName.Visibility = Visibility.Visible;
					lblPancake1.Visibility = Visibility.Visible;
					lblPancake2.Visibility = Visibility.Visible;
					vbLock.Visibility = Visibility.Hidden;
					*/
				}
			}
		}

		/// <summary>
		/// 임고 금지 여부
		/// </summary>
		public bool LockWarehousing {
			get {
				return lockWH;
			}
			set {
				if( lockWH != value ) {
					lockWH = value;
					if( lockWH ) {
						// lock
						rootLayout.Background = new SolidColorBrush( lockBackColor );
						this.Foreground = new SolidColorBrush( lockForeColor );

						checkArea.Visibility = Visibility.Hidden;
						lblProjectName.Visibility = Visibility.Hidden;
						pancakeInfo.Visibility = Visibility.Hidden;
						//vbLock.Visibility = Visibility.Visible;
					} else {
						// 새로 색상설정
						ElapseDay = elapseday;

						checkArea.Visibility = Visibility.Visible;
						lblProjectName.Visibility = Visibility.Visible;
						pancakeInfo.Visibility = Visibility.Visible;
						//vbLock.Visibility = Visibility.Hidden;

					}

				}
			}
		}

		/// <summary>
		/// Stacker Crane 이 현재위치에 있는지 여부
		/// </summary>
		////public bool CraneLocated {
		////	get {
		////		return crane;
		////	}
		////	set {
		////		crane = value;
		////		if( crane ) {
		////			this.Background = new SolidColorBrush( Colors.Orange );
		////		} else {
		////			this.Background = null;
		////		}
		////	}
		////}

		/// <summary>
		/// 열 번호
		/// </summary>
		public int Row {
			get {
				return row;
			}
			set {
				if( row != value ) {
					row = value;
					RowChanged?.Invoke( this, EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// 연 번호
		/// </summary>
		public int Col {
			get {
				return col;
			}
			set {
				if( col != value ) {
					col = value;
					ColChanged?.Invoke( this, EventArgs.Empty );
				}
			}
		}
		/// <summary>
		/// 단 
		/// </summary>
		public int Stair {
			get {
				return stair;
			}
			set {
				stair = value;
			}
		}

		//public string LockName {
		//	get {
		//		return lblLock.Text;
		//	}
		//	set {
		//		lblLock.Text = value;
		//	}
		//}

		public string SkidID {
			get {
                //return lblSkidId.Text;
                return skidId;
            }
			set {
                //lblSkidId.Text = value;
                skidId = value;

                if (skidId == "PORT")
                {
                    Grid.SetColumnSpan(Row0, 2); ;
                    Grid.SetRowSpan(Row0, 3); ; 
                    
                }else if(!string.IsNullOrEmpty(skidId))
                {
                    Grid.SetColumnSpan(Row0, 2);
                    Grid.SetColumnSpan(Row1, 2);  
                    Grid.SetColumnSpan(Row2, 2);  
                    Grid.SetRowSpan(Row0, 1); 
                    

                }
                 
                if (skidId == "PORT")
                {
                    lblSkidId.Text = "PORT";


                    if (wip_Remarks == "Y")
                    {
                        rootLayout.Background = new SolidColorBrush(portMaterialColor);
                    }
                    else
                    {
                        rootLayout.Background = new SolidColorBrush(portColor);
                    }
                    this.Foreground = new SolidColorBrush(Colors.Black);

                    this.check.Visibility = Visibility.Hidden;

                    if (project == "T" || project == "U" || project == "F")
                    {
                        ColorAnimation da = new ColorAnimation();

                        if (wip_Remarks == "Y")
                        {
                            da.From = portMaterialColor;
                        }
                        else
                        {
                            da.From = portColor;
                        }


                        //da.From = portColor;
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
                else if (skidId == "UNUSE")
                {
                    lblSkidId.Text = "UNUSE";
                   
                    
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }

                    rootLayout.Background = new SolidColorBrush(unuseColor);
                }
                else if (skidId == "ERROR")
                {
                    lblSkidId.Text = "ERROR";
                   
                   
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
                else if (skidId == "CHECK")
                {
                    lblSkidId.Text = "CHECK";
                   

                              
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }

                    rootLayout.Background = new SolidColorBrush(checkColor);

                    this.check.Visibility = Visibility.Hidden;



                    ColorAnimation da = new ColorAnimation();
                    da.From = checkColor;
                    da.To = Colors.White;
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);



                    //if (this.stair.ToString() == "2")
                    //{
                    //    DoubleAnimation da = new DoubleAnimation();
                    //    da.From = this.Height;
                    //    da.To = 0;

                    //    da.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); // 500ms == 0.5s
                    //    da.AutoReverse = true;
                    //    da.RepeatBehavior = RepeatBehavior.Forever;

                    //    this.BeginAnimation(SkidRack.HeightProperty, da);
                    //}
                    //else {
                    //    DoubleAnimation da = new DoubleAnimation();
                    //    da.From = this.Width;
                    //    da.To = 0;

                    //    da.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); // 500ms == 0.5s
                    //    da.AutoReverse = true;
                    //    da.RepeatBehavior = RepeatBehavior.Forever;

                    //    this.BeginAnimation(SkidRack.WidthProperty, da);
                    //}
                }
                else if (skidId == "")
                {
                    lblSkidId.Text = "";

                   
                              
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }

                    this.check.Visibility = Visibility.Hidden;
                    rootLayout.Background = new SolidColorBrush(Colors.White);
                    this.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if(project == "" && skidId != "" )                    
                {
                    lblSkidId.Text = skidId;
                   
                               
                    try
                    {
                        rootLayout.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }
                    this.check.Visibility = Visibility.Visible;
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
                    this.check.Visibility = Visibility.Visible;
                }
            }
		}
        #region #SetPancakeID
        public string PancakeID1
        {
            get
            {
                return pancakeId1;
            }
            set
            {
                pancakeId1 = value;

                if (skidId == "PORT")
                {
                    lblPancake1.Text = pancakeId1;
                }

                
            }
        }
        public string PancakeID2
        {
            get
            {
                return pancakeId2;
            }
            set
            {
                pancakeId2 = value;

                
            }
        }
        public string PancakeID3
        {
            get
            {
                return pancakeId3;
            }
            set
            {
                pancakeId3 = value;

                
            }
        }

        public string PancakeID4
        {
            get
            {
                return pancakeId4;
            }
            set
            {
                pancakeId4 = value; 
                
                 if(string.IsNullOrEmpty(pancakeId4))
                {
                    Grid.SetColumnSpan(Row0, 2);
                    Grid.SetColumnSpan(Row1, 2);
                    Grid.SetColumnSpan(Row2, 2);
                }
                else
                {
                    Grid.SetColumnSpan(Row0, 1);
                    Grid.SetColumnSpan(Row1, 1);
                    Grid.SetColumnSpan(Row2, 1);
                }

                //if (string.IsNullOrEmpty(pancakeId5))
                //{
                //    Grid.SetColumnSpan(Row1, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row1, 1);
                //}

                //if (string.IsNullOrEmpty(pancakeId6))
                //{
                //    Grid.SetColumnSpan(Row2, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row2, 1);
                //}

            }
        }
        public string PancakeID5
        {
            get
            {
                return pancakeId5;
            }
            set
            {
                pancakeId5 = value;
                if (string.IsNullOrEmpty(pancakeId4))
                {
                    Grid.SetColumnSpan(Row0, 2);
                    Grid.SetColumnSpan(Row1, 2);
                    Grid.SetColumnSpan(Row2, 2);
                }
                else
                {
                    Grid.SetColumnSpan(Row0, 1);
                    Grid.SetColumnSpan(Row1, 1);
                    Grid.SetColumnSpan(Row2, 1);
                }

                //if (string.IsNullOrEmpty(pancakeId5))
                //{
                //    Grid.SetColumnSpan(Row1, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row1, 1);
                //}

                //if (string.IsNullOrEmpty(pancakeId6))
                //{
                //    Grid.SetColumnSpan(Row2, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row2, 1);
                //}
            }
        }

        public string PancakeID6
        {
            get
            {
                return pancakeId6;
            }
            set
            {
                pancakeId6 = value;

                if (string.IsNullOrEmpty(pancakeId4))
                {
                    Grid.SetColumnSpan(Row0, 2);
                    Grid.SetColumnSpan(Row1, 2);
                    Grid.SetColumnSpan(Row2, 2);
                }
                else
                {
                    Grid.SetColumnSpan(Row0, 1);
                    Grid.SetColumnSpan(Row1, 1);
                    Grid.SetColumnSpan(Row2, 1);
                }

                //if (string.IsNullOrEmpty(pancakeId5))
                //{
                //    Grid.SetColumnSpan(Row1, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row1, 1);
                //}

                //if (string.IsNullOrEmpty(pancakeId6))
                //{
                //    Grid.SetColumnSpan(Row2, 2);
                //}
                //else
                //{
                //    Grid.SetColumnSpan(Row2, 1);
                //}
            }
        }
        #endregion

        #region #SetPancakeQA
        public string PancakeQA1
        {
            get
            {
                return pancakeQA1;
            }
            set
            {
                pancakeQA1 = value;

                if (skidId == "PORT")
                {

                }
                else
                {
                    lblPancake1.Text = string.IsNullOrEmpty(pancakeQA1) ? "" : pancakeQA1;
                }
            }
        }
        public string PancakeQA2
        {
            get
            {
                return pancakeQA2;
            }
            set
            {
                pancakeQA2 = value;

                lblPancake2.Text = "";

                lblPancake2.Text = string.IsNullOrEmpty(pancakeQA2) ? "" : pancakeQA2;
            }
        }
        public string PancakeQA3
        {
            get
            {
                return pancakeQA3;
            }
            set
            {
                pancakeQA3 = value;

                lblPancake3.Text = "";

                lblPancake3.Text = string.IsNullOrEmpty(pancakeQA3) ? "" : pancakeQA3;
            }
        }
        public string PancakeQA4
        {
            get
            {
                return pancakeQA2;
            }
            set
            {
                pancakeQA4 = value;

                lblPancake4.Text = "";

                lblPancake4.Text = string.IsNullOrEmpty(pancakeQA4) ? "" : pancakeQA4;
            }
        }
        public string PancakeQA5
        {
            get
            {
                return pancakeQA1;
            }
            set
            {
                pancakeQA5 = value;

                lblPancake5.Text = "";

                lblPancake5.Text = string.IsNullOrEmpty(pancakeQA5) ? "" : pancakeQA5;
            }
        }
        public string PancakeQA6
        {
            get
            {
                return pancakeQA2;
            }
            set
            {
                pancakeQA6 = value;

                lblPancake6.Text = "";

                lblPancake6.Text = string.IsNullOrEmpty(pancakeQA6) ? "" : pancakeQA6;
            }
        }
        #endregion

        #region #SetWipHold
        public string WIPHOLD1
        {
            get
            {
                return wiphold1;
            }
            set
            {
                wiphold1 = value;
                if (wiphold1 == "Y")
                { 
                    lblPancake1.Background = new SolidColorBrush(Colors.Red); 
                }
                else
                { 
                    lblPancake1.Background = new SolidColorBrush(Colors.Transparent); 
                }
            }
        }
        public string WIPHOLD2
        {
            get
            {
                return wiphold2;
            }
            set
            {
                wiphold2 = value;
                if (wiphold2 == "Y")
                {
                    lblPancake2.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblPancake2.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        public string WIPHOLD3
        {
            get
            {
                return wiphold3;
            }
            set
            {
                wiphold3 = value;
                if (wiphold3 == "Y")
                {
                    lblPancake3.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblPancake3.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        public string WIPHOLD4
        {
            get
            {
                return wiphold4;
            }
            set
            {
                wiphold4 = value;
                if (wiphold4 == "Y")
                {
                    lblPancake4.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblPancake4.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        public string WIPHOLD5
        {
            get
            {
                return wiphold5;
            }
            set
            {
                wiphold5 = value;
                if (wiphold5 == "Y")
                {
                    lblPancake5.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblPancake5.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        public string WIPHOLD6
        {
            get
            {
                return wiphold6;
            }
            set
            {
                wiphold6 = value;
                if (wiphold6 == "Y")
                {
                    lblPancake6.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    lblPancake6.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        #endregion

        
        /// <summary>
        /// Project 명
        /// </summary>
        public string ProjectName {
			get {
				return project;
			}
			set {
				project = value;

                //try
                //{
                //    this.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                //}
                //catch
                //{
                //}

                if (skidId != "PORT" || skidId != "UNUSE" || skidId != "CHECK" || skidId != "ERROR")
                {
                    if (project.Trim().Length > 0)
                    {
                        this.lblSkidId.Text = project;
                    }
                }
                //else if (skidId == "PORT" && (project == "T" || project == "U" || project == "F"))
                //{
                //    ColorAnimation da = new ColorAnimation();
                //    da.From = portColor;
                //    da.To = Colors.White;
                //    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                //    da.AutoReverse = true;
                //    da.RepeatBehavior = RepeatBehavior.Forever;
                //    this.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                //}                
            }
		}
		public string RackId {
			get {
				return rackId;
			}
			set {
				rackId = value;
			}
		}

        public string PRDT_CLSS_CODE
        {
            get
            {
                return prdt_CLSS_CODE;
            }
            set
            {
                prdt_CLSS_CODE = value;

                if (prdt_CLSS_CODE == "C")
                {
                    lblSkidId.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    lblSkidId.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        public string ZoneId
        {
            get { return zoneId; }
            set { zoneId = value; }
        }

        public string RackStat {
			get {
				return rackStat;
			}
			set {
				rackStat = value;
				if( rackStat.Equals( "C" ) ) {
					// 상태확인요청
					grdRackStat.Visibility = Visibility.Visible; 
				} else if( rackStat.Equals( "R" ) ) {
					// 입고 예약
					grdRackStat.Visibility = Visibility.Visible; 
				} else {
					grdRackStat.Visibility = Visibility.Hidden;
				}
			}
		}

        public string Spcl_Flag
        {
            get
            {
                return spcl_Flag;
            }
            set
            {
                spcl_Flag = value;
                if (spcl_Flag == "Y")
                {
                    lblSkidId.Background = new SolidColorBrush(Colors.Yellow);

                    //lblPancake1.Background = (wiphold1 == "Y") ? lblPancake1.Background : new SolidColorBrush(Colors.Yellow);
                    //lblPancake2.Background = (wiphold2 == "Y") ? lblPancake2.Background : new SolidColorBrush(Colors.Yellow);
                    //lblPancake3.Background = (wiphold3 == "Y") ? lblPancake3.Background : new SolidColorBrush(Colors.Yellow);
                    //lblPancake4.Background = (wiphold4 == "Y") ? lblPancake4.Background : new SolidColorBrush(Colors.Yellow);
                    //lblPancake5.Background = (wiphold5 == "Y") ? lblPancake5.Background : new SolidColorBrush(Colors.Yellow);
                    //lblPancake6.Background = (wiphold6 == "Y") ? lblPancake6.Background : new SolidColorBrush(Colors.Yellow);
                }
                else 
                {
                    lblSkidId.Background =   new SolidColorBrush(Colors.Transparent);
                    lblPancake1.Background = (wiphold1 == "Y") ? lblPancake1.Background : new SolidColorBrush(Colors.Transparent);
                    lblPancake2.Background = (wiphold2 == "Y") ? lblPancake2.Background : new SolidColorBrush(Colors.Transparent);
                    lblPancake3.Background = (wiphold3 == "Y") ? lblPancake3.Background : new SolidColorBrush(Colors.Transparent);
                    lblPancake4.Background = (wiphold4 == "Y") ? lblPancake4.Background : new SolidColorBrush(Colors.Transparent);
                    lblPancake5.Background = (wiphold5 == "Y") ? lblPancake5.Background : new SolidColorBrush(Colors.Transparent);
                    lblPancake6.Background = (wiphold6 == "Y") ? lblPancake6.Background : new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        public string Spcl_RsnCode
        {
            get
            {
                return spcl_RsnCode;
            }
            set
            {
                spcl_RsnCode = value;
            }
        }

        public string Wip_Remarks
        {
            get
            {
                return wip_Remarks;
            }
            set
            {
                wip_Remarks = value;
            }
        }

        public string Wipdttm_ED
        {
            get
            {
                return wipdttm_ED;
            }
            set
            {
                wipdttm_ED = value;
            }
        }

        public string PortTypeName {
			get {
				return lblPortTypeName.Content.ToString();
			}
			set {
				lblPortTypeName.Content = value;
			}
		}

		/// <summary>
		/// 사용자 정의 데이터
		/// </summary>
		public Dictionary<string, object> UserData {
			get {
				return userDic;
			}
		}

		

		public void Clear() {
			this.IsChecked = false;
			this.RackStat = "";
			this.IsPortPosition = false;

			check.IsEnabled = true;

			row = -1;
			col = -1;
			stair = -1;
			RackId = "";
			ProjectName = "";

			SkidID = "";
			pancakeId1 = "";
			pancakeId2 = "";
            pancakeId3 = "";
            pancakeId4 = "";
            pancakeId5 = "";
            pancakeId6 = "";
            pancakeQA1 = "";
			pancakeQA2 = "";
            pancakeQA3 = "";
            pancakeQA4 = "";
            pancakeQA5 = "";
            pancakeQA6 = "";
            lblPancake1.Text = "";
			lblPancake2.Text = "";
            lblPancake3.Text = "";
            lblPancake4.Text = "";
            lblPancake5.Text = "";
            lblPancake6.Text = "";
            PortTypeName = "";
			userDic.Clear();
		}
        #endregion
    }
}
