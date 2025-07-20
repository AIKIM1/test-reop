/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Reflection;
using System.Data;



using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MCS001.Controls
{
	/// <summary>
	/// SkidStair.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SkidStair : UserControl
	{
        #region Declaration & Constructor 
        //private static int MAX_ROW = 2;
        //private static int MAX_COL = 41;
        //private static int skidstartColIndex = 2;

        private static int MAX_ROW = 3;
        private static int MAX_COL = 23;
        private static int skidstartColIndex = 3;

        // 물리적 Row 수
        //private int physicalRow = 1;
        // 물리적 Col 수
        //private int physicalCol = 17;

        // 한줄에 보여질 최대 컬럼수 ==> 최대 41연 (14 + 14 + 13 ) 총 두줄로 보여짐
        //private int colPerRow = 14;

        private int colPerRow = 23;

        private SkidRack[][] racks = null;
		private TextBlock[][] rackLabels = null;
		private string[] rowNames = null;

		public event EventHandler<SkidRackEventArgs> SkidRackChecked = null;
		public event EventHandler<SkidRackEventArgs> SkidRackClick = null;
		public event EventHandler<SkidRackEventArgs> SkidRackDoubleClick = null;
        public event EventHandler<SkidRackEventArgs> SkidRackMouseEnter = null;


        public SkidStair() {
			InitializeComponent();

            RowCount = 3;
            //ColumnCount = 22;
            this.GetColumnCount();

			PrepareSkid();
			//PrepareLayoutScroll( 2 );
			PrepareLayoutNoScroll();
		}


        public void RackSetBackColor()
        {
            //racks.setba
        }

		/// <summary>
		/// 실제 연 갯수
		/// </summary>
		public int ColumnCount {
			get;
			protected set;
		}

		/// <summary>
		/// 실제 행 갯수
		/// </summary>
		public int RowCount {
			get;
			protected set;
		}

		/// <summary>
		/// TypeA (3일이내) 배경
		/// </summary>
		public Color IssueDayTypeABackColor {
			get {
				return racks[0][0].IssueDayTypeABackColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeABackColor = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeA (3일이내) 글자
		/// </summary>
		public Color IssueDayTypeAForeColor {
			get {
				return racks[0][0].IssueDayTypeAForeColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeAForeColor = value;
					}
				}
			}
		}
		/// <summary>
		/// TypeA (3일이내) 날짜
		/// </summary>
		public int IssueDayTypeADay {
			get {
				return racks[0][0].IssueDayTypeADay;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeADay = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeB (7일이내) 배경
		/// </summary>
		public Color IssueDayTypeBBackColor {
			get {
				return racks[0][0].IssueDayTypeBBackColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeBBackColor = value;
					}
				}
			}
		}
		/// <summary>
		/// TypeB (7일이내) 글자
		/// </summary>
		public Color IssueDayTypeBForeColor {
			get {
				return racks[0][0].IssueDayTypeBForeColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeBForeColor = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeB (7일이내) 날짜
		/// </summary>
		public int IssueDayTypeBDay {
			get {
				return racks[0][0].IssueDayTypeBDay;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeBDay = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeC (30일이내) 배경
		/// </summary>
		public Color IssueDayTypeCBackColor {
			get {
				return racks[0][0].IssueDayTypeCBackColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeCBackColor = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeC (30일이내) 글자
		/// </summary>
		public Color IssueDayTypeCForeColor {
			get {
				return racks[0][0].IssueDayTypeCForeColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeCForeColor = value;
					}
				}
			}
		}
		/// <summary>
		/// TypeC (30일이내) 날짜
		/// </summary>
		public int IssueDayTypeCDay {
			get {
				return racks[0][0].IssueDayTypeCDay;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeCDay = value;
					}
				}
			}
		}
		/// <summary>
		/// TypeD (30일이상) 배경
		/// </summary>
		public Color IssueDayTypeDBackColor {
			get {
				return racks[0][0].IssueDayTypeDBackColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeDBackColor = value;
					}
				}
			}
		}

		/// <summary>
		/// TypeD (30일이상) 글자
		/// </summary>
		public Color IssueDayTypeDForeColor {
			get {
				return racks[0][0].IssueDayTypeDForeColor;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeDForeColor = value;
					}
				}
			}
		}
		/// <summary>
		/// TypeD (30일이상) 날짜
		/// </summary>
		public int IssueDayTypeDDay {
			get {
				return racks[0][0].IssueDayTypeDDay;
			}
			set {
				for( int r = 0; r < RowCount; r++ ) {
					for( int c = 0; c < ColumnCount; c++ ) {
						racks[r][c].IssueDayTypeDDay = value;
					}
				}
			}
		}

        #endregion

        #region Event
        private void OnScrollViewerSizeChanged( object sender, SizeChangedEventArgs e ) {
			/*
			if( viewbox.MinWidth > e.NewSize.Width ) {
				scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
				viewbox.Width = viewbox.MinWidth;
			} else {
				scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
				viewbox.Width = double.NaN;
			}
			if( viewbox.MinHeight > e.NewSize.Height ) {
				scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
				viewbox.Height = viewbox.MinHeight;
			} else {
				scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
				viewbox.Height = double.NaN;
			}
			*/
		}


		/// <summary>
		/// SkidRack Checkbox check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSkidRackChecked( object sender, RoutedEventArgs e ) {
			SkidRack rack = sender as SkidRack;
			if( rack != null ) {
				//////if( rack.IsChecked == true ) {
				//////	// 이전에 check된 rack 해제
				//////	for( int r = 0; r < racks.Length; r++ ) {
				//////		for( int c = 0; c < racks[r].Length; c++ ) {
				//////			if( racks[r][c].IsChecked && !racks[r][c].Equals( rack ) ) {
				//////				// 자기자신이 아닌 check된 rack은 check 해제
				//////				racks[r][c].IsChecked = false;
				//////			}
				//////		}
				////////	}
				////////}
			}

			SkidRackChecked?.Invoke( this, new SkidRackEventArgs( rack ) );
		}

		/// <summary>
		/// SkidRack Mouse Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSkidRackClick( object sender, RoutedEventArgs e ) {
			SkidRack rack = sender as SkidRack;
			SkidRackClick?.Invoke( this, new SkidRackEventArgs( rack ) );
		}

		/// <summary>
		/// SkidRack Mouse Double Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSkidRackDoubleClick( object sender, RoutedEventArgs e ) {
			SkidRack rack = sender as SkidRack;
			SkidRackDoubleClick?.Invoke( this, new SkidRackEventArgs( rack ) );
		}

		private void PrepareSkid() {
			racks = new SkidRack[MAX_ROW][];
			rackLabels = new TextBlock[MAX_ROW][];
			rowNames = new string[MAX_ROW];

			for( int r = 0; r < racks.Length; r++ ) {
				racks[r] = new SkidRack[MAX_COL];
				rackLabels[r] = new TextBlock[MAX_COL];

				for( int c = 0; c < racks[r].Length; c++ ) {
					SkidRack sr = new SkidRack();
					sr.Name = string.Format( "r{0:0}c{1:00}", r, c );
					sr.ProjectName = "";
					sr.PancakeID1 = "";
					sr.PancakeID2 = "";
					sr.PancakeQA1 = "";
					sr.PancakeQA2 = "";

					//sr.Height = 75;
					sr.Checked += OnSkidRackChecked;
					sr.Click += OnSkidRackClick;
					sr.DoubleClick += OnSkidRackDoubleClick;
                    //sr.MouseEnter += OnSkidRackMouseEnter;
					sr.IsVisibleChanged += OnSkidRackVisibleChanged;
                    
					sr.ColChanged += OnSkidRackColChanged;
					//sr.RowChanged += OnSkidRackRowChanged;
					racks[r][c] = sr;

					TextBlock tb = new TextBlock();
					tb.Name = string.Format( "lblr{0:0}c{1:00}", r, c );
					tb.TextAlignment = TextAlignment.Center;

					rackLabels[r][c] = tb;

				} // for( int c = 0; c < racks[r].Length; c++ )
			} // for( int r = 0; r < racks.Length; r++ )
		}

        private void OnSkidRackMouseEnter(object sender, MouseEventArgs e)
        {
            SkidRack rack = sender as SkidRack;
            SkidRackMouseEnter?.Invoke(this, new SkidRackEventArgs(rack));
        }


       
       

        private void OnSkidRackColChanged( object sender, EventArgs e ) {
			SkidRack sr = sender as SkidRack;
			if( sr != null ) {
				TextBlock tb = (TextBlock)stair.FindName( "lbl" + sr.Name );
				if( tb != null ) {
					LGC.GMES.MES.ControlsLibrary.ObjectDicConverter dicConverter = new ControlsLibrary.ObjectDicConverter();
                    //tb.Text = string.Format( "{0}{1} ({2})", sr.Col, LGC.GMES.MES.Common.ObjectDic.Instance.GetObjectName( "연" ), sr.RackId );
                    //tb.Text = string.Format("{0}{1}", sr.Col, LGC.GMES.MES.Common.ObjectDic.Instance.GetObjectName("연"));
                    tb.Text = "";
                }
			}
		}

		private void OnSkidRackRowChanged( object sender, EventArgs e ) {
			/*
			SkidRack sr = sender as SkidRack;
			if( sr != null ) {
				TextBlock tb = (TextBlock)stair.FindName( "lbl" + sr.Name );
				if( tb != null ) {
					LGC.GMES.MES.ControlsLibrary.ObjectDicConverter dicConverter = new ControlsLibrary.ObjectDicConverter();
					tb.Text = sr.Col + (string)dicConverter.Convert( tb, typeof( string ), "연", System.Globalization.CultureInfo.CurrentCulture );
				}
			}
			*/
		}

		private void OnSkidRackVisibleChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			SkidRack sr = sender as SkidRack;
			if( sr != null && e.Property.Name.Equals( "IsVisible" ) ) {
				TextBlock tb = (TextBlock)stair.FindName( "lbl" + sr.Name );
				if( tb != null ) {
					tb.Visibility = sr.Visibility;
				}
			}
		}

        #endregion

        #region Method
        /// <summary>
        /// scroll 없는 layout 준비
        /// </summary>
        private void PrepareLayoutNoScroll() {
			stair.Children.Clear();

			// 행/열 전체 삭제
			if( stair.ColumnDefinitions.Count > 0 ) {
				stair.ColumnDefinitions.Clear();
			}
			if( stair.RowDefinitions.Count > 0 ) {
				stair.RowDefinitions.Clear();
			}

			// column 정의
			ColumnDefinition coldef = null;

			// left margin
			coldef = new ColumnDefinition();
			coldef.Width = new GridLength( 4 );
			stair.ColumnDefinitions.Add( coldef );

			// 열 이름
			coldef = new ColumnDefinition();
			coldef.Width = GridLength.Auto;
			stair.ColumnDefinitions.Add( coldef );

			// Skid rack
			for( int i = 0; i < colPerRow; i++ ) {
				coldef = new ColumnDefinition();
				coldef.Width = new GridLength( 1, GridUnitType.Star );
				stair.ColumnDefinitions.Add( coldef );
			}
			// right margin
			coldef = new ColumnDefinition();
			coldef.Width = new GridLength( 4 );
			stair.ColumnDefinitions.Add( coldef );


			// 연배열당열의수
			int rowsPerRowCnt = (int)( ( ColumnCount / (double)colPerRow ) + 0.5 );
			int needRows = ( rowsPerRowCnt * 2 ); // 한열당 표시될 열의수
			for( int r = 0; r < RowCount; r++ ) {
               
                if (r == 1)
                {
                    RowDefinition rowdef;
                    rowdef = new RowDefinition();
                    stair.RowDefinitions.Add(rowdef);

                    RowDefinition rowdef2;
                    rowdef2 = new RowDefinition();
                    stair.RowDefinitions.Add(rowdef2);
                }
                if (r == 2)
                {
                    RowDefinition rowdef;
                    rowdef = new RowDefinition();
                    stair.RowDefinitions.Add(rowdef);

                    RowDefinition rowdef2;
                    rowdef2 = new RowDefinition();
                    stair.RowDefinitions.Add(rowdef2);
                }

                for (int i = 0; i < needRows; i++)
                {
                    if (1 == 0)
                    {
                        RowDefinition rowdef = new RowDefinition();
                        //rowdef.Height = new GridLength(1);
                        stair.RowDefinitions.Add(rowdef);
                    }
                    else {
                        RowDefinition rowdef = new RowDefinition();
                        rowdef.Height = GridLength.Auto;
                        stair.RowDefinitions.Add(rowdef);
                    }
                }
            } // for( int r = 0; r < RowCount; r++ ) {


			for( int i = 0; i < RowCount; i++ ) {
				int idx = 0;
				for( int r = 0; r < needRows; r++ ) {
					if( idx >= racks[i].Length ) {
						break;
					}

					if( r % 2 == 0 ) {
						// 연 표시 Label 열
						continue;
					}
					int realrow = ( i * ( needRows + 2 ) ) + r;

					bool bVisible = false;
					for( int c = skidstartColIndex; c < ( skidstartColIndex + colPerRow ); c++ ) {
						if( idx >= racks[i].Length ) {
							break;
						}

                        
                        Grid.SetColumn( racks[i][idx], c );
						Grid.SetRow( racks[i][idx], realrow );
						stair.Children.Add( racks[i][idx] );
						stair.RegisterName( racks[i][idx].Name, racks[i][idx] );

						// 연표시열
						rackLabels[i][idx].Visibility = racks[i][idx].Visibility;
						
						//////////////Grid.SetColumn( rackLabels[i][idx], c );
						//////////////Grid.SetRow( rackLabels[i][idx], realrow - 1 );
						//////////////stair.Children.Add( rackLabels[i][idx] );18
						//////////////stair.RegisterName( rackLabels[i][idx].Name, rackLabels[i][idx] );

						if( racks[i][idx].Visibility == Visibility.Visible ) {
							bVisible = true;
						} else {
							// 화면상에 보이지 않음.
							racks[i][idx].IsChecked = false;
						}
						idx++;
					} // for( int c = skidstartColIndex; c < ( skidstartColIndex + colPerRow ); c++ )

					
				}
			} // for( int i = 0; i < RowCount; i++ ) {

			// parent 변경
			if( stair.Parent != this.noscroll ) {
				scrollViewer.Content = null;
				noscroll.Child = stair;
				scrollViewer.Visibility = Visibility.Collapsed;
			}
			noscroll.Visibility = Visibility.Visible;
		}

		private void PrepareLayoutScroll( int rowCount ) {
			stair.Children.Clear();

			// 행/열 전체 삭제
			if( stair.ColumnDefinitions.Count > 0 ) {
				stair.ColumnDefinitions.Clear();
			}
			if( stair.RowDefinitions.Count > 0 ) {
				stair.RowDefinitions.Clear();
			}

			// column 정의
			ColumnDefinition coldef = null;

			// left margin
			coldef = new ColumnDefinition();
			coldef.Width = new GridLength( 4 );
			stair.ColumnDefinitions.Add( coldef );

			// 열 이름
			coldef = new ColumnDefinition();
			coldef.Width = GridLength.Auto;
			stair.ColumnDefinitions.Add( coldef );

			// Skid rack
			int cpr = (int)( ( ColumnCount / (double)rowCount ) + .5 );
			for( int i = 0; i < cpr; i++ ) {
				coldef = new ColumnDefinition();
				coldef.Width = new GridLength( 108 );
				stair.ColumnDefinitions.Add( coldef );
			}
			// right margin
			coldef = new ColumnDefinition();
			coldef.Width = new GridLength( 4 );
			stair.ColumnDefinitions.Add( coldef );

			// 연배열당열의수
			int rowsPerRowCnt = (int)( ( ColumnCount / (double)cpr ) + 0.5 );
			int needRows = ( rowsPerRowCnt * 2 ); // 한열당 표시될 열의수
			for( int r = 0; r < RowCount; r++ ) {
                if (r == 1)
                {
                    // 두번째 열이면 사이에 공백을 표시하기 위해 여백을 주고
                    // 홀짝수 구분으로 연 표시문자열과 rack표시를 하므로 두개의 여백줄을 삽입
                    RowDefinition rowdef;
                    rowdef = new RowDefinition();
                    stair.RowDefinitions.Add(rowdef);
                }
                for ( int i = 0; i < needRows; i++ ) {
					RowDefinition rowdef = new RowDefinition();
					rowdef.Height = GridLength.Auto;
					stair.RowDefinitions.Add( rowdef );
				}
			} // for( int r = 0; r < RowCount; r++ ) {

			//LGC.GMES.MES.ControlsLibrary.ObjectDicConverter dicConverter = new ControlsLibrary.ObjectDicConverter();
			for( int i = 0; i < RowCount; i++ ) {
				int idx = 0;
				for( int r = 0; r < needRows; r++ ) {
					if( idx >= racks[i].Length ) {
						break;
					}
					// 열표시
					if( r % 2 == 0 ) {
						continue;
					}

					bool bVisible = false;
					for( int c = skidstartColIndex; c < ( skidstartColIndex + cpr ); c++ ) {
						if( idx >= racks[i].Length ) {
							break;
						}
						// Rack
						int realrow = ( i * ( needRows + 2 ) ) + r;
						Grid.SetColumn( racks[i][idx], c );
						Grid.SetRow( racks[i][idx], realrow );
						stair.Children.Add( racks[i][idx] );
						stair.RegisterName( racks[i][idx].Name, racks[i][idx] );

						// 연표시열
						rackLabels[i][idx].Visibility = racks[i][idx].Visibility;

						Grid.SetColumn( rackLabels[i][idx], c );
						Grid.SetRow( rackLabels[i][idx], realrow - 1 );
						stair.Children.Add( rackLabels[i][idx] );
						stair.RegisterName( rackLabels[i][idx].Name, rackLabels[i][idx] );
						if( rackLabels[i][idx].Visibility == Visibility.Visible ) {
							bVisible = true;
						} else {
							// 화면상에 보이지 않음.
							racks[i][idx].IsChecked = false;
						}

						idx++;
					} // for( int c = skidstartColIndex; c < ( skidstartColIndex + colPerRow ); c++ )

					// 열표시
					if( bVisible ) {
						int realrow = ( i * ( needRows + 2 ) ) + r;
						TextBlock rowLabel = new TextBlock();
						rowLabel.VerticalAlignment = VerticalAlignment.Center;
						rowLabel.Margin = new Thickness( 0, 0, 0, 0 );
						rowLabel.Text = rowNames[i];
						Grid.SetColumn( rowLabel, 1 );
						Grid.SetRow( rowLabel, realrow );
						stair.Children.Add( rowLabel );
					}
				}
			} // for( int i = 0; i < RowCount; i++ ) {

			// parent 변경
			if( stair.Parent != this.scrollViewer ) {
				noscroll.Child = null;
				this.scrollViewer.Content = stair;
				noscroll.Visibility = Visibility.Collapsed;
			}
			this.scrollViewer.Visibility = Visibility.Visible;
		}
		
		/// <summary>
		/// SkidRack 찾음
		/// </summary>
		/// <param name="row">열 index</param>
		/// <param name="col">연 index</param>
		/// <returns></returns>
		public SkidRack this[int row, int col] {
			get {
				return racks[row][col];
			}
		}

		/// <summary>
		/// 이름으로 Rack 찾음(rXcXX ex:r0c03)
		/// </summary>
		/// <param name="name">이름(rXcXX ex:r0c03)</param>
		/// <returns></returns>
		public SkidRack this[string name] {
			get {
				for( int r = 0; r < racks.Length; r++ ) {
					for( int c = 0; c < racks[r].Length; c++ ) {
						if( racks[r][c].Name.Equals( name ) ) {
							return racks[r][c];
						}
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Check 된 SkidRack을 찾음
		/// </summary>
		/// <returns>Check 된 SkidRack or null</returns>
		public SkidRack GetCheckedRack() {
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					if( racks[r][c].IsChecked ) {
						return racks[r][c];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 모든 Rack을 표시하지 않음.
		/// </summary>
		public void HideAllRack() {
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					racks[r][c].Visibility = Visibility.Hidden;
					racks[r][c].IsVisibleChanged -= OnSkidRackVisibleChanged;

					rackLabels[r][c].Visibility = Visibility.Hidden;
				}
			}
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					racks[r][c].IsVisibleChanged += OnSkidRackVisibleChanged;
				}
			}

		}

		/// <summary>
		/// 모든 Rack을 표시하지 않음.
		/// </summary>
		public void HideAndClearAllRack() {
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					racks[r][c].Visibility = Visibility.Hidden;
					racks[r][c].IsVisibleChanged -= OnSkidRackVisibleChanged;

					rackLabels[r][c].Visibility = Visibility.Hidden;

					racks[r][c].Clear();
				}
			}
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					racks[r][c].IsVisibleChanged += OnSkidRackVisibleChanged;
				}
			}

		}

		/// <summary>
		/// Skid 창고 단 화면 배치
		/// </summary>
		/// <param name="rowCount">보여질 열의 개수(양극/음극 열)</param>
		/// <param name="scrollable">스크롤 사용여부</param>
		public void SetSkidLayout( int rowCount, bool scrollable ) {
			if( scrollable ) {
				PrepareLayoutScroll( 2 );
			} else {
				PrepareLayoutNoScroll();
			}
		}

		public void SetRowName( int rowIndex, string name ) {
			rowNames[rowIndex] = name;
		}

		/// <summary>
		/// Check된 SkidRack 해제
		/// </summary>
		public void UncheckRack() {
			for( int r = 0; r < racks.Length; r++ ) {
				for( int c = 0; c < racks[r].Length; c++ ) {
					if( racks[r][c].IsChecked ) {
						racks[r][c].IsChecked = false;
						//return;
					}
				}
			}
		}

        private void GetColumnCount()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = "A5A102";

            RQSTDT.Rows.Add(dr);
            DataTable dtResult = null;
            dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAXY", "RQSTDT", "RSLTDT", RQSTDT);

            ColumnCount = Convert.ToInt16(dtResult.Rows[0][0]);
        }

        #endregion
    }

	public class SkidRackEventArgs : RoutedEventArgs
	{
		public SkidRack SkidRack {
			get;
			internal set;
		}
		internal SkidRackEventArgs( SkidRack sr ) {
			this.SkidRack = sr;
		}
	}
}
