/*************************************************************************************
 Created Date : 2016.10.01
      Creator : 김광호C
   Decription : 대차 모니터링 - 대차 Control
--------------------------------------------------------------------------------------
 [Change History]
  2016.10.01  김광호C : Initial Created.
  
**************************************************************************************/
using LGC.GMES.MES.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LGC.GMES.MES.COM001
{

    /// <summary>
    /// cnssalee01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_032_CRTCTL : UserControl
    {
        #region Declaration & Constructor
        Point oldPoint, currentPoint;
        bool isInDrag = false;
        private TranslateTransform transform = new TranslateTransform();

        public COM001_032_CRTCTL()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_032_CRTCTL(string sCartID, string pCartLabel, string pCartType, string pEmptyYN,
                                 string sDisplayType, double dCanvasWidth, double dCanvasHeight)
        {
            String sImgUri;

            if (sDisplayType == "CT")
            {
                if (pEmptyYN == "Y")
                {
                    sImgUri = "Images\\icon_map_pin01.png";

                }
                else
                {
                    sImgUri = "Images\\icon_map_pin02.png";
                }

                //ImgCart.Height = 30 - (dCanvasHeight / 803 * (30 / 29));
                //ImgCart.Width = 23 - (dCanvasWidth / 1838 * (23 / 22));
            }
            else
            {
                sImgUri = "Images\\icon_map_pin03.png";
            }

            //if (sDisplayType == "CT")
            //{
            //    //ImgCart.Stretch = Stretch.Uniform;                
            //}
            //else
            //{
            //    //ImgCart.Stretch = Stretch.None;                
            //}



            InitializeComponent();


            ImageSource imgSrc = new BitmapImage(new Uri(sImgUri, UriKind.Relative));
            ImgCart.Source = imgSrc;

            lblCartID.Content = sCartID;
            lblCartNo.Content = pCartLabel;
            lblCartType.Content = pCartType;

            lblCartNo.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
            lblCartNo.Background = new SolidColorBrush(Colors.Transparent);

            if (sDisplayType == "CT")
            {
                ImgCart.Height = 32;
                ImgCart.Width = 22;
            }
            else
            {
                ImgCart.Height = 25;
                ImgCart.Width = 18;
            }
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void CrtImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;

            oldPoint = e.GetPosition(null);
            element.CaptureMouse();
            isInDrag = true;
            e.Handled = true;

            //COM001_032 popup = sender as COM001_032;
            //popup.FrameOperation = this.FrameOperation;
            //popup.chkAutoRefresh.IsChecked = false;
        }

        private void CrtImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.ReleaseMouseCapture();
            isInDrag = false;
            e.Handled = true;
        }

        private void CrtImg_MouseMove(object sender, MouseEventArgs e)
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
