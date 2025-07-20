/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class bojhs02 : UserControl
    {
        #region Declaration & Constructor 
        public bojhs02()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            #region Sample Table
            DataRow newRow = null;
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("LINEID");
            dtTemp.Columns.Add("LINENAME");
            dtTemp.Columns.Add("PROCNAME");
            dtTemp.Columns.Add("PROCID");
            dtTemp.Columns.Add("PROCSEQ");

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1","2호기", "CELL 검사", "CT201",1};
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CELL 컷팅밴딩", "CM202", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CELL 시퀀스", "CM203", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 조립", "MA201", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 용접", "MW201", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 검사", "MT201", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CELL 검사", "CTP01", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CELL 폴딩밴딩", "CMP02", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CMA 조립", "MAP01", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CMA 용접및검사", "MTP01", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "BMA 조립", "BTP01", 5 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 검사", "CT301", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 컷팅밴딩", "CM302", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 시퀀스", "CM303", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 조립", "MA301", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 용접", "MW301", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 검사", "MT301", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 검사", "CT401", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 컷팅밴딩", "CM402", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 시퀀스", "CM403", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 조립", "MA401", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 용접", "MW401", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 검사", "MT401", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 검사", "CT501", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 컷팅밴딩", "CM502", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 시퀀스", "CM503", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 조립", "MA501", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 용접", "MW501", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 검사", "MT501", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 검사", "CT601", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 컷팅밴딩", "CM602", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 시퀀스", "CM603", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 조립", "MA601", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 용접", "MW601", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 검사", "MT601", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 검사", "CT701", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 컷팅밴딩", "CM702", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 시퀀스", "CM703", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CMA 조립", "MA701", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CMA 용접", "MW701", 5 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 검사", "CT801", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 컷팅밴딩", "CM802", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 시퀀스", "CM803", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 조립", "MA801", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 용접", "MW801", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 검사", "MT801", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 검사", "CT901", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 컷팅밴딩", "CM902", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 시퀀스", "CM903", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 조립", "MA901", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 용접", "MW901", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 검사", "MT901", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 검사", "CTA01", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 컷팅밴딩", "CMA02", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 시퀀스", "CMA03", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CMA 조립", "MAA01", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CMA 용접", "MWA01", 5 };
            dtTemp.Rows.Add(newRow);
            #endregion
            //DataRow[] dtRow = dtTemp.Select("", "PROCSEQ");

            var list = dtTemp.AsEnumerable().GroupBy(r => new
            {
                PRODGROUP = r.Field<string>("LINEID")
            }).Select(g => g.First());
            DataTable dtLineList = list.CopyToDataTable();

            

            for (int i=0; i < dtLineList.Rows.Count; i++)
            {
                var row = new RowDefinition();
                row.Height = new System.Windows.GridLength(70);
                sp.RowDefinitions.Add(row);
                



                Grid grdNew = new Grid();
                grdNew.Name = dtLineList.Rows[i]["LINEID"].ToString();
                grdNew.Tag = dtLineList.Rows[i]["LINEID"];
                //grdNew.HorizontalAlignment = HorizontalAlignment.Left;
                //grdNew.Margin = new Thickness(100, 0, 100, 0);

                //Grid grdRowNew = (Grid)this.FindName(dtLineList.Rows[i]["LINEID"].ToString());
                //Grid grdRowNew = FindChild<Grid>(Application.Current.MainWindow, dtLineList.Rows[i]["LINEID"].ToString());

                DataTable myNewTable = dtTemp.Select("LINEID = '"+ dtLineList.Rows[i]["LINEID"].ToString() + "'").CopyToDataTable();

                if(myNewTable.Rows.Count > 0)
                {
                    var column = new ColumnDefinition();
                    grdNew.ColumnDefinitions.Add(column);
                    column.Width = new System.Windows.GridLength(100);

                    System.Windows.Controls.TextBlock newTextBlock = new TextBlock();

                    newTextBlock.Text = dtLineList.Rows[i]["LINENAME"].ToString();                    
                    newTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    newTextBlock.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                    newTextBlock.SetValue(Grid.RowProperty, 0);
                    grdNew.Children.Add(newTextBlock);
                }
                for (int c = 0; c < myNewTable.Rows.Count; c++)
                {
                    var column = new ColumnDefinition();
                    if (c - 1 == myNewTable.Rows.Count)
                    {
                        new GridLength(1, GridUnitType.Star);
                    }
                    else
                    {
                        column.Width = new System.Windows.GridLength(200);
                    }
                    grdNew.ColumnDefinitions.Add(column);

                    System.Windows.Controls.Button newBtn = new Button();                    
                    newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                    newBtn.Name = "Button" + myNewTable.Rows[c]["PROCID"].ToString();
                    newBtn.Tag = myNewTable.Rows[c];//myNewTable.Rows[c]["PROCID"].ToString();
                    newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle");
                    newBtn.Click += btnProcess_Click;
                    newBtn.SetValue(Grid.ColumnProperty, grdNew.ColumnDefinitions.Count-1);
                    grdNew.Children.Add(newBtn);

                    var columnEmpty = new ColumnDefinition();
                    columnEmpty.Width = new System.Windows.GridLength(20);
                    grdNew.ColumnDefinitions.Add(columnEmpty);
                }
                grdNew.SetValue(Grid.RowProperty, sp.RowDefinitions.Count - 1);
                sp.Children.Add(grdNew);

                var border = new Border();
                border.BorderBrush = Brushes.Gray;
                border.BorderThickness = new Thickness(1);
                border.SetValue(Grid.RowProperty, sp.RowDefinitions.Count - 1);
                sp.Children.Add(border);
                

                var rowEmpty = new RowDefinition();
                rowEmpty.Height = new System.Windows.GridLength(20);
                sp.RowDefinitions.Add(rowEmpty);
            }
            

            /*

            for (int i = 0; i < 2; i++)
            {

                var column = new ColumnDefinition();
                //column.
                sp.ColumnDefinitions.Add(column);
                //sp.ColumnDefinitions = column;

                //System.Windows.Controls.Border newborder = new Border();
                System.Windows.Controls.Button newBtn = new Button();

                //newborder.Background = Brushes.Green;
                //newborder.Name = "Border" + i.ToString();
                newBtn.Content = i.ToString();
                newBtn.Name = "Button" + i.ToString();
                newBtn.Tag = "";

                //newBtn.Style = (Style)Application.Current.Resources["Content_MainButtonNextStyle"];
                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle");


                //newBtn.
                newBtn.SetValue(Grid.ColumnProperty, i);
                //sp.Children.Add(newborder);
                sp.Children.Add(newBtn);
            }
            */
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            DataRow drProc = (DataRow)clicked.Tag;
            txtSelectedLine.Text = drProc["LINENAME"].ToString();
            txtSelectedProcess.Text = drProc["PROCNAME"].ToString();
        }

        private static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
