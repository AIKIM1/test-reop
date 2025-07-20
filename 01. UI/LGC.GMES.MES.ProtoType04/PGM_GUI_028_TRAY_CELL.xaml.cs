/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_028_TRAY_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PGM_GUI_028_TRAY_CELL()
        {
            InitializeComponent();
            setTryLoction();
        }



        #endregion

        #region Initialize

        #endregion

        #region Event
        private void button_Click(object sender, RoutedEventArgs e)
        {
         

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Mehod
        private void setTryLoction()
        {
            try
            {

                //_grid.Children.Clear();
                //_grid.ColumnDefinitions.Clear();
                //_grid.RowDefinitions.Clear();

                int iRowCnt = 30;
                int iColCnt = 5;

                for (int i = 0; i < iColCnt; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    gridCol1.Width = new GridLength(100); 

                    gTrayLayout.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < iRowCnt+1; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(10);
                    gTrayLayout.RowDefinitions.Add(gridRow1);

                }

                for (int iRow = 0; iRow < iRowCnt / 2; iRow++)
                {
                    for (int iCol = 0; iCol < iColCnt; iCol++)
                    {
                        Label _lable = new Label();
                        _lable.Content = iRow.ToString() + iCol.ToString();
                        _lable.FontSize = 10;
                        _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                        _lable.VerticalContentAlignment = VerticalAlignment.Center;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        _lable.Width = 100;
                        _lable.Height = 20;

                        if ((iCol + iRow) % 3 == 0) {
                            _lable.Background = new SolidColorBrush(Colors.Red);
                        }

                        //_lable.Background = new SolidColorBrush(Colors.Red);
                        Grid.SetColumn(_lable, iCol);

                        if (iCol % 2 == 0)
                        {
                            Grid.SetRow(_lable, iRow * 2);
                        }
                        else
                        {
                            Grid.SetRow(_lable, iRow * 2 + 1);
                        }
                        Grid.SetRowSpan(_lable, 2);

                        gTrayLayout.Children.Add(_lable);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //commMessage.Show(ex.Message);
            }
        }
        #endregion


    }
}
