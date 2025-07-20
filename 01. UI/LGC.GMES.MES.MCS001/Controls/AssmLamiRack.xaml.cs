/*************************************************************************************
 Created Date : 2018.01.16
      Creator : 오화백
   Decription : 조립 Rack 
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
	public partial class AssmLamiRack : UserControl
	{
        #region Declaration & Constructor 
  
            
        private Color TypeABackColor = Colors.White;
        private Color TypeAForeColor = Colors.Black;
        private Color rcv_reserveColor = Colors.Yellow;
        private Color iss_reserveColor = Colors.Orange;
        private Color unuseColor = Colors.Blue;
        private Color checkColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#9C9C9C");
        private Color holdColor = Colors.Red;


        private Color usingColor = Colors.White;
        private Color usableColor = Colors.White;

       
      
        // Rack State
		private string rackStat = string.Empty;

        // Border 
        private string border = string.Empty;

      

        public Button ButtonUsControl { get; set; }

        //생성자
        public AssmLamiRack()
        {
            InitializeComponent();
            this.Foreground = new SolidColorBrush(TypeAForeColor);
            btnUserControl.Background = new SolidColorBrush(TypeABackColor);
            btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
            RackStatColor(string.Empty);
            SetButtons();
        }
		/// <summary>
		/// 배경 색
		/// </summary>
		public Color P_TypeABackColor {
			get {
				return TypeABackColor;
			}
			set {
                TypeABackColor = value;
			}
		}
		/// <summary>
		/// ForeColor
		/// </summary>
		public Color P_TypeAForeColor{
			get {
				return TypeAForeColor;
			}
			set {
                TypeAForeColor = value;
			}
		}
        /// <summary>
        /// 버튼셋팅
        /// </summary>
        private void SetButtons()
        {
            ButtonUsControl = btnUserControl;
        }




        #endregion

        #region Event 

      
    

        #endregion

        #region Method

        #region Lot 정보 셋팅 : P_LotID {}
        /// <summary>
        /// LOT 정보 셋팅
        /// </summary>
        public string P_STAT
        {
            get
            {
                return rackStat;
            }
            set
            {
                rackStat = value;
                RackStatColor(rackStat);
            }
        }

        #endregion

        #region Rack 상태 : RackStatColor()

        /// <summary>
        /// Rack 상태 Color
        /// </summary>
        public void RackStatColor(string Stat)
        {
            if(Stat != string.Empty)
            {

                if(Stat == "RCV_RESERVE")
                {
                 
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(rcv_reserveColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "USABLE")
                {
                 
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(usingColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "USING")
                {
                   
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(usingColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "UNUSE")
                {
                 
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(unuseColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "ISS_RESERVE")
                {
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(iss_reserveColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "CHECK")
                {
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(checkColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
                else if (Stat == "HOLD")
                {
                    this.Foreground = new SolidColorBrush(TypeAForeColor);
                    btnUserControl.Background = new SolidColorBrush(holdColor);
                    btnUserControl.Foreground = new SolidColorBrush(TypeAForeColor);
                }
            }
        }


        #endregion






        #endregion


    }
}
