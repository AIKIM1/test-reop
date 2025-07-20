/*************************************************************************************
 Created Date : 2022.10.21
      Creator : 조영대
   Decription : UcPageNavigation
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.21  조영대 : Initial Created.
**************************************************************************************/

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace LGC.GMES.MES.CMM001.Controls
{
    public partial class UcPageNavigation : UserControl
    {
        #region Declaration
        
        public event PageChangedEventHandler PageChanged;
        [Category("GMES"), Description("Page 변경시 발생하는 Event")]
        public delegate void PageChangedEventHandler(object sender, int pageNumber);
        
        #region Property

        private int rowCountPerPage = 1000;
        [Category("GMES"), DefaultValue(1000), Description("Page 당 Row 수")]
        public int RowCountPerPage
        {
            get { return rowCountPerPage; }
            set { rowCountPerPage = value; }
        }

        private int maxRowCount = 0;
        [Category("GMES"), DefaultValue(12345), Description("최대 Row 수")]
        public int MaxRowCount
        {
            get { return maxRowCount; }
            set
            {
                maxRowCount = value;
                
                DisplayPageNo();
            }
        }

        private int currentPage = 0;
        [Category("GMES"), DefaultValue(0), Description("현재 Page 번호"), Browsable(false)]
        public int CurrentPage
        {
            get { return currentPage + 1; }
        }

        private List<TextBlock> pageLists = new List<TextBlock>();
        private List<TextBlock> blankLists = new List<TextBlock>();

        private int startPageNo = 0;
        #endregion

        #region DependencyProperty

        #endregion

        public UcPageNavigation()
        {
            InitializeComponent();
            
            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            pageLists.Add(lblPage01); blankLists.Add(lblPage01_B);
            pageLists.Add(lblPage02); blankLists.Add(lblPage02_B);
            pageLists.Add(lblPage03); blankLists.Add(lblPage03_B);
            pageLists.Add(lblPage04); blankLists.Add(lblPage04_B);
            pageLists.Add(lblPage05); blankLists.Add(lblPage05_B);
            pageLists.Add(lblPage06); blankLists.Add(lblPage06_B);
            pageLists.Add(lblPage07); blankLists.Add(lblPage07_B);
            pageLists.Add(lblPage08); blankLists.Add(lblPage08_B);
            pageLists.Add(lblPage09); blankLists.Add(lblPage09_B);
            pageLists.Add(lblPage10); blankLists.Add(lblPage10_B);

            pageLists[0].Foreground = Brushes.Red;

            DisplayPageNo();
        }

        #endregion

        #region Override

        #endregion

        #region Event

        private void PageNavigator_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button btnNavi = (Button)sender;
            
            switch (btnNavi.Name)
            {
                case "btnPageDown":
                    PageDown();
                    break;
                case "btnPagePreview":
                    PagePreview();

                    if (maxRowCount > 0)
                    {
                        PageChanged?.Invoke(this, currentPage + 1);
                    }
                    break;
                case "btnPageNext":
                    PageNext();

                    if (maxRowCount > 0)
                    {
                        PageChanged?.Invoke(this, currentPage + 1);
                    }
                    break;
                case "btnPageUp":
                    PageUp();
                    break;
            }
        }

        private void Page_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBlock)) return;
            if (currentPage < 0)
            {
                currentPage = 0;
                return;
            }

            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;

            currentPage = Convert.ToInt16((sender as TextBlock).Text.Trim()) - 1;

            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Red;

            if (currentPage >= 0 && btnPagePreview.IsEnabled.Equals(false))
            {
                btnPagePreview.IsEnabled = true;
            }

            if (currentPage >= (maxRowCount / rowCountPerPage) && btnPageNext.IsEnabled.Equals(true))
            {
                btnPageNext.IsEnabled = false;
            }

            if (currentPage < 1 && btnPagePreview.IsEnabled.Equals(true))
            {
                btnPagePreview.IsEnabled = false;
            }

            if (currentPage < (maxRowCount / rowCountPerPage) && btnPageNext.IsEnabled.Equals(false))
            {
                btnPageNext.IsEnabled = true;
            }

            if (maxRowCount > 0)
            {
                PageChanged?.Invoke(this, currentPage + 1);
            }
        }

        #endregion

        #region Method

        public void Clear()
        {
            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;

            startPageNo = 0;
            currentPage = 0;

            DisplayPageNo();

            InvalidateVisual();
        }

        private void PageNext()
        {
            if (currentPage >= maxRowCount / rowCountPerPage) return;

            if (currentPage >= 0 && btnPagePreview.IsEnabled.Equals(false))
            {
                btnPagePreview.IsEnabled = true;
            }

            if (currentPage >= (maxRowCount / rowCountPerPage - 1)  && btnPageNext.IsEnabled.Equals(true))
            {
                btnPageNext.IsEnabled = false;
            }
            
            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;

            int pageGroup = currentPage / 10;
            currentPage++;
            if (!pageGroup.Equals(currentPage/10))
            {
                PageUp();
            }

            if (currentPage < startPageNo) currentPage = startPageNo;

            if (currentPage > startPageNo + 10)
            {
                startPageNo += 10;
                DisplayPageNo();
            }
            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Red;
        }

        private void PagePreview()
        {

            if (currentPage <= 0) return;

            if (currentPage <= 1 && btnPagePreview.IsEnabled.Equals(true))
            {
                btnPagePreview.IsEnabled = false;
            }

            if (currentPage <= (maxRowCount / rowCountPerPage) && btnPageNext.IsEnabled.Equals(false))
            {
                btnPageNext.IsEnabled = true;
            }

            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;
            
            int pageGroup = currentPage / 10;

            currentPage--;

            if (!pageGroup.Equals(currentPage / 10))
            {
                PageDown();
            }

            if (currentPage > startPageNo + 9)
            {
                currentPage = startPageNo + 9;
            }

            if (currentPage < startPageNo)
            {
                startPageNo -= 10;
                DisplayPageNo();
            }

            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Red;
        }

        private void PageUp()
        {
            if (startPageNo + 10 >= maxRowCount / rowCountPerPage) return;

            startPageNo += 10;
            //currentPage += 10;

            DisplayPageNo();

        }

        private void PageDown()
        {
            startPageNo -= 10;
            //currentPage -= 10;

            if (startPageNo < 0)
            {
                startPageNo = 0;
                return;
            }

            DisplayPageNo();
        }

        private void DisplayPageNo()
        {
            pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;

            for (int inx = 0; inx < pageLists.Count; inx++)
            {
                if (startPageNo + inx > maxRowCount / rowCountPerPage)
                {
                    pageLists[inx].Text = string.Empty;
                    pageLists[inx].Visibility = System.Windows.Visibility.Collapsed;

                    blankLists[inx].Text = string.Empty;
                    blankLists[inx].Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    pageLists[inx].Text = " " + (startPageNo + inx + 1).ToString() + " ";
                    pageLists[inx].Visibility = System.Windows.Visibility.Visible;

                    blankLists[inx].Text = "";
                    blankLists[inx].Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (startPageNo + 10 > maxRowCount / rowCountPerPage)
            {
                btnPageUp.IsEnabled = false;
            }
            else
            {
                btnPageUp.IsEnabled = true;
            }

            if (startPageNo < 10)
            {
                btnPageDown.IsEnabled = false;
            }
            else
            {
                btnPageDown.IsEnabled = true;
            }

            if (currentPage > maxRowCount / rowCountPerPage)
            {
                pageLists[currentPage % pageLists.Count].Foreground = Brushes.Black;
                currentPage = maxRowCount / rowCountPerPage;
                pageLists[currentPage % pageLists.Count].Foreground = Brushes.Red;
            }
            else
            {
                if (pageLists[currentPage % pageLists.Count].Text.Trim().Equals((currentPage + 1).ToString()))
                {
                    pageLists[currentPage % pageLists.Count].Foreground = Brushes.Red;
                }
            }

            if (currentPage >= 0 && btnPagePreview.IsEnabled.Equals(false))
            {
                btnPagePreview.IsEnabled = true;
            }

            if (currentPage >= (maxRowCount / rowCountPerPage) && btnPageNext.IsEnabled.Equals(true))
            {
                btnPageNext.IsEnabled = false;
            }

            if (currentPage <= 1 && btnPagePreview.IsEnabled.Equals(true))
            {
                btnPagePreview.IsEnabled = false;
            }

            if (currentPage < (maxRowCount / rowCountPerPage) && btnPageNext.IsEnabled.Equals(false))
            {
                btnPageNext.IsEnabled = true;
            }
        }

        #endregion

        
    }

}