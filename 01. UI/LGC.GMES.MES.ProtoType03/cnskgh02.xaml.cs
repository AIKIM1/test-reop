/*************************************************************************************
 Created Date : 2016.09.19
      Creator : 이슬아D
   Decription : 대차 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.19  이슬아D : Initial Created.
  
**************************************************************************************/
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LGC.GMES.MES.ProtoType03
{

    /// <summary>
    /// cnssalee01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnskgh02 : UserControl
    {

        public cnskgh02()
        {
            InitializeComponent();
        }

        public cnskgh02(string sCartID, string pCartLabel, string pCartType, string pEmptyYN, string sDisplayType)
        {
            String sImgUri;

            if (sDisplayType == "CRT")
            {
                if (pEmptyYN == "Y")
                {
                    sImgUri = "D:/#.Secure Work Folder/문서/Image/map_pin_01.png";
                }
                else
                {
                    sImgUri = "D:/#.Secure Work Folder/문서/Image/map_pin_02.png";
                }
            }
            else
            {
                sImgUri = "D:/#.Secure Work Folder/문서/Image/map_pin_03.png";
            }

            InitializeComponent();

            ////var label = gridRecipe.Children.OfType<Label>()
            ////                .First(i => i.Name == "labelRecipeName");

            //String ImgNameUri = "D:/#.Secure Work Folder/문서/Image/Cart_M_E.png";

            //Image img1 = new Image();
            //img1.Width = 60;
            //img1.Height = 44;
            ImageSource imgSrc = new BitmapImage(new Uri(sImgUri));
            ImgCart.Source = imgSrc;
            //Grid.SetRow(img1, 0);
            //Grid.SetColumn(img1, 0);
            //grid1.Children.Add(img1);

            //Label label1 = new Label();
            //label1.Name = "label1";
            lblCartID.Content = sCartID;
            lblCartNo.Content = pCartLabel;
            lblCartType.Content = pCartType;
            
            //label1.Width = 240;
            //label1.Height = 30;
            //label1.Margin = new Thickness(0, 49, 0, 0);
            lblCartNo.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
            lblCartNo.Background = new SolidColorBrush(Colors.Transparent);

            //Grid.SetRow(label1, 1);
            //Grid.SetColumn(label1, 0);
            //grid1.Children.Add(label1);

        }

        private void lblCartNo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // For Popup Window

            //cnskgh03 dwCart = new cnskgh03();


        }
        
    }


}
