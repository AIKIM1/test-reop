/*************************************************************************************
 Created Date : 2021.06.03
      Creator : 조영대
   Decription : 사용자정의 컨트롤 - Rack Info.
--------------------------------------------------------------------------------------
 [Change History]
   2021.06.03  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System;

namespace LGC.GMES.MES.FCS001.Controls
{
    public partial class UcWearhouseRackInfo : UserControl
    {
        #region Declaration

        private static Color fillStep0 = Colors.White;
        private static Color fillStep1 = Colors.LightGreen;
        private static Color fillStep2 = Colors.LightSkyBlue;
        private static Color fillStep3 = Colors.Yellow;
        private static Color fillStep4 = Colors.LightCoral;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public delegate void RackClickEventHandler(object sender, string rackId);
        public event RackClickEventHandler RackClick;

        private string rackId = string.Empty;
        [Browsable(false)]
        public string RackId
        {
            get { return rackId; }
            set
            {
                rackId = value;
            }
        }


        private string info1 = string.Empty;
        [Browsable(false)]
        public string Info1
        {
            get { return info1; }
            set
            {
                txtInfo1.Text= info1 = value;                
            }
        }

        private string info2 = string.Empty;
        [Browsable(false)]
        public string Info2
        {
            get { return info2; }
            set
            {
                txtInfo2.Text = info2 = value;
            }
        }

        private string info3 = string.Empty;
        [Browsable(false)]
        public string Info3
        {
            get { return info3; }
            set
            {
                txtInfo3.Text = info3 = value;
            }
        }

        private string info4 = string.Empty;
        [Browsable(false)]
        public string Info4
        {
            get { return info4; }
            set
            {
                txtInfo4.Text = info4 = value;
            }
        }

        private float loadRate = -1;
        [Browsable(false)]
        public float LoadRate
        {
            get { return loadRate; }
            set
            {
                loadRate = value;

                if (loadRate < 0)
                {
                    bdrRackInfo.Background = new SolidColorBrush(fillStep0);
                }
                else if (loadRate >= 0 && loadRate < 34)
                {
                    bdrRackInfo.Background = new SolidColorBrush(fillStep1);
                }
                else if (loadRate >= 34 && loadRate < 66)
                {
                    bdrRackInfo.Background = new SolidColorBrush(fillStep2);
                }
                else if (loadRate >= 66 && loadRate < 100)
                {
                    bdrRackInfo.Background = new SolidColorBrush(fillStep3);
                }
                else if (loadRate >= 100)
                {
                    bdrRackInfo.Background = new SolidColorBrush(fillStep4);

                    ColorAnimation colorAni = new ColorAnimation()
                    {
                        From = fillStep4,
                        To = Colors.Red,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    Random rand = new Random(DateTime.Now.Millisecond);
                    int stratTime = rand.Next(0, 1000);
                    colorAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                    bdrRackInfo.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
                }
            }
        }

        public UcWearhouseRackInfo()
        {
            InitializeComponent();

            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            txtInfo1.Text = string.Empty;
            txtInfo2.Text = string.Empty;
            txtInfo3.Text = string.Empty;
            txtInfo4.Text = string.Empty;
        }


        #endregion

        #region Override

        #endregion

        #region Event
        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RackClick?.Invoke(sender, rackId);
        }
        #endregion

        #region Mehod
        
        #endregion    
    }
}
