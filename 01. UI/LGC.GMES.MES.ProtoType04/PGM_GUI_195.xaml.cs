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

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_195 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_195()
        {
            InitializeComponent();

            setGridLoction();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod
        private void setGridLoction()
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

                    gLayout.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < iRowCnt + 1; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(50);
                    gLayout.RowDefinitions.Add(gridRow1);

                }


                for (int iCol = 0; iCol < iColCnt; iCol++)
                {
                    Label _lable = new Label();
                    _lable.Content = "header" + iCol.ToString();
                    _lable.FontSize = 10;
                    _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                    _lable.VerticalContentAlignment = VerticalAlignment.Center;
                    _lable.Margin = new Thickness(0, 0, 0, 0);
                    _lable.Padding = new Thickness(0, 0, 0, 0);
                    _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                    _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                    _lable.Width = 100;
                    _lable.Height = 50;

                    _lable.Background = new SolidColorBrush(Colors.Gray);


                    //_lable.Background = new SolidColorBrush(Colors.Red);
                    Grid.SetColumn(_lable, iCol);
                    Grid.SetRow(_lable, 0);
                    gLayout.Children.Add(_lable);
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
                        _lable.Height = 50;

                        if ((iCol + iRow) % 3 == 0)
                        {
                            _lable.Background = new SolidColorBrush(Colors.Red);
                        }

                        //_lable.Background = new SolidColorBrush(Colors.Red);
                        Grid.SetColumn(_lable, iCol);


                        Grid.SetRow(_lable, iRow +1);
                        gLayout.Children.Add(_lable);
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
