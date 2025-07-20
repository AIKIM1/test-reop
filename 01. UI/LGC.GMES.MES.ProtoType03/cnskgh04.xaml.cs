/*************************************************************************************
 Created Date : 2016.10.01
      Creator : 김광호C
   Decription : 대차 모니터링 - Location Control
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.19  이슬아D : Initial Created.
  
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType03
{

    /// <summary>
    /// cnskgh04.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnskgh04 : UserControl
    {
        private string _MntType;

        public cnskgh04()
        {
            InitializeComponent();
        }

        public cnskgh04(string sZoneID, string sZoneName, string sCartCnt, string sVisibleCD, string sMntType)
        {
            InitializeComponent();
            
            lblZoneID.Content = sZoneID;
            lblZoneNM.Content = sZoneName;
            _MntType = sMntType;

            if (sVisibleCD == "C")
            {
                lblZoneID.Visibility = Visibility.Visible;
                lblZoneNM.Visibility = Visibility.Hidden;
            }
            else
            {
                lblZoneID.Visibility = Visibility.Hidden;
                lblZoneNM.Visibility = Visibility.Visible;
            }

            if (Convert.ToInt16(sCartCnt) > 0)
            {
                lblCartCnt.Content = "Cart Cnt : " + sCartCnt;
                lblCartCnt.Visibility = Visibility.Visible;
            }
            else
            {
                lblCartCnt.Visibility = Visibility.Hidden;
            }

            lblZoneID.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
        }

        private void lblZoneID_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cnskgh05 wndLocationDetail = new cnskgh05();
            
            if (wndLocationDetail != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = _MntType;
                Parameters[1] = lblZoneID.Content.ToString();   // Location_Code
                Parameters[2] = lblZoneNM.Content.ToString();   // Location_Name
                C1WindowExtension.SetParameters(wndLocationDetail, Parameters);

                //wndLocationDetail.Closed += new EventHandler(wndLocationDetail_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndLocationDetail.ShowModal()));
            }
        }
    }


}
