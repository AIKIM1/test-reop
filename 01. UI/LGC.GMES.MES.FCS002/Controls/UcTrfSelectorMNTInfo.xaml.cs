/*************************************************************************************
 Created Date : 2021.06.29
      Creator : 강동희
   Decription : 사용자정의 컨트롤 - Rack Info.
--------------------------------------------------------------------------------------
 [Change History]
   2021.06.29  강동희 : Initial Created.
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

namespace LGC.GMES.MES.FCS002.Controls
{
    public partial class UcTrfSelectorMNTInfo : UserControl
    {
        #region Declaration

        private static Color fillStep0 = Colors.White;
        private static Color fillStep1 = Colors.Red;
        private static Color fillStep2 = Colors.Orange;
        private static Color fillStep3 = Colors.LightGreen;
        private static Color fillStep4 = Colors.Yellow;
        private static Color fillStep5 = Colors.Gray;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public delegate void EqpClickEventHandler(object sender, string Port_ID, string Lot_ID, string Tray_ID, string Wip_Qty);
        public event EqpClickEventHandler EqpClick;

        private string Eqp_ID = string.Empty;
        private string Port_ID = string.Empty;
        private string Tray_ID = string.Empty;
        private string Lot_ID = string.Empty;
        private string Wip_Qty = string.Empty;

        private int Row = 0;
        private int Col = 0;


        [Browsable(false)]
        public string EQP_ID
        {
            get { return Eqp_ID; }
            set
            {
                Eqp_ID = value;
            }
        }

        public string PORT_ID
        {
            get { return Port_ID; }
            set
            {
                Port_ID = value;
            }
        }

        public string TRAY_ID
        {
            get { return Tray_ID; }
            set
            {
                Tray_ID = value;
            }
        }

        public string LOT_ID
        {
            get { return Lot_ID; }
            set
            {
                Lot_ID = value;
            }
        }

        public string WIP_QTY
        {
            get { return Wip_Qty; }
            set
            {
                Wip_Qty = value;
            }
        }

        public int ROW
        {
            get { return Row; }
            set
            {
                Row = value;
            }
        }

        public int COL
        {
            get { return Col; }
            set
            {
                Col = value;
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

        private string setColor = string.Empty;
        [Browsable(false)]
        public string SetColor
        {
            get { return setColor; }
            set
            {
                setColor = value.ToUpper();

                if (setColor.Equals("WHITE"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep0);
                }
                else if (setColor.Equals("RED"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep1);
                }
                else if (setColor.Equals("ORANGE"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep2);
                }
                else if (setColor.Equals("GREEN"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep3);

                    ColorAnimation colorAni = new ColorAnimation()
                    {
                        From = fillStep4,
                        To = Colors.LightGreen,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    Random rand = new Random(DateTime.Now.Millisecond);
                    int stratTime = rand.Next(0, 1000);
                    colorAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                    bdrEqpInfo.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
                }
                else if (setColor.Equals("YELLOW"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep4);
                }
                else if (setColor.Equals("GRAY"))
                {
                    bdrEqpInfo.Background = new SolidColorBrush(fillStep5);
                }

                //else if (setColor.Equals(0))
                //{
                //    bdrRackInfo.Background = new SolidColorBrush(fillStep4);

                //    ColorAnimation colorAni = new ColorAnimation()
                //    {
                //        From = fillStep4,
                //        To = Colors.Red,
                //        Duration = TimeSpan.FromMilliseconds(1000),
                //        AutoReverse = true,
                //        RepeatBehavior = RepeatBehavior.Forever
                //    };

                //    Random rand = new Random(DateTime.Now.Millisecond);
                //    int stratTime = rand.Next(0, 1000);
                //    colorAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                //    bdrRackInfo.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
                //}
            }
        }

        public UcTrfSelectorMNTInfo()
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
            EqpClick?.Invoke(sender, Port_ID, Lot_ID, Tray_ID, Wip_Qty);
        }
        #endregion

        #region Mehod
        
        #endregion    
    }
}
