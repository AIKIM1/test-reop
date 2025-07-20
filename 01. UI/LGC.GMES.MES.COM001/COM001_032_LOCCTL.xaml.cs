/*************************************************************************************
 Created Date : 2016.10.01
      Creator : 김광호C
   Decription : 대차 모니터링 - Location Control
--------------------------------------------------------------------------------------
 [Change History]
  2016.10.01  김광호C : Initial Created.
  
**************************************************************************************/
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    /// <summary>
    /// cnskgh04.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_032_LOCCTL : UserControl
    {
        #region Declaration & Constructor 

        private string _MntType = string.Empty;
        private string _EmptyYN = string.Empty;
        private string _CartNo = string.Empty;
        private string _CartType = string.Empty;
        private string _ProdDttmSt = string.Empty;
        private string _ProdDttmEt = string.Empty;
        private string _LotID = string.Empty;

        Point oldPoint, currentPoint;
        bool isInDrag = false;
        private TranslateTransform transform = new TranslateTransform();

        public COM001_032_LOCCTL()
        {
            InitializeComponent();
        }

        public COM001_032_LOCCTL(string sZoneID, string sZoneName, string sCartCnt, string sVisibleCD, 
                                 string sMntType, string sEmptyYn, string sCartType, string sCartNo, 
                                 string sProdDttmSt, string sProdDttmEt, string pLotId)
        {
            InitializeComponent();
            
            lblZoneID.Content = sZoneID;
            lblZoneNM.Content = sZoneName;
            _MntType = sMntType;
            _EmptyYN = sEmptyYn;
            _CartNo = sCartNo;
            _CartType = sCartType;
            _LotID = pLotId;
            _ProdDttmSt = sProdDttmSt;
            _ProdDttmEt = sProdDttmEt;

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
                lblCartCnt.Content = "*Qty : " + sCartCnt;
                lblCartCnt.Visibility = Visibility.Visible;
            }
            else
            {
                lblCartCnt.Visibility = Visibility.Hidden;
            }

            lblZoneID.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void LocCtl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;

            oldPoint = e.GetPosition(null);
            element.CaptureMouse();
            isInDrag = true;
            e.Handled = true;
        }

        private void LocCtl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.ReleaseMouseCapture();
            isInDrag = false;
            e.Handled = true;
        }

        private void lblCartCnt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            COM001_032_LOCPOPUP wndLocationDetail = new COM001_032_LOCPOPUP();

            if (wndLocationDetail != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = _MntType;
                Parameters[1] = _EmptyYN;
                Parameters[2] = _CartType;
                Parameters[3] = _CartNo;
                Parameters[4] = lblZoneID.Content.ToString();   // Location_Code
                Parameters[5] = lblZoneNM.Content.ToString();   // Location_Name
                Parameters[6] = _LotID;
                Parameters[7] = _ProdDttmSt;
                Parameters[8] = _ProdDttmEt;

                C1WindowExtension.SetParameters(wndLocationDetail, Parameters);

                //wndLocationDetail.Closed += new EventHandler(wndLocationDetail_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndLocationDetail.ShowModal()));
            }
        }

        private void LocCtl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isInDrag)
            {
                var element = sender as FrameworkElement;
                currentPoint = e.GetPosition(null);

                var transform = new TranslateTransform
                {
                    X = (currentPoint.X - oldPoint.X),
                    Y = (currentPoint.Y - oldPoint.Y)
                };
                this.RenderTransform = transform;
                //oldPoint = currentPoint;
            }
        }

        #endregion

        #region Mehod

        #endregion

    }


}
