/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType01
{
    public partial class ProtoType0103 : UserControl
    {
        #region Declaration & Constructor 

        private DataView dvRootNodes;

        public ProtoType0103()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            DataSet ds = GetData();
            dvRootNodes = ds.Tables["TB_DATA"].DefaultView;
            dvRootNodes.RowFilter = "PARENT_KEY IS NULL";
            trvData.ItemsSource = dvRootNodes;
            TreeItemExpandAll();
        }

        #endregion

        #region Event

        private void trvData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            System.Data.DataRowView dr = trvData.SelectedItem.DataContext as System.Data.DataRowView;
            txtFrom.Text = "KEY : " + Util.NVC(DataTableConverter.GetValue(dr, "KEY"));
            txtSubject.Text = "PARENT_KEY : " + Util.NVC(DataTableConverter.GetValue(dr, "PARENT_KEY"));
            txtReceived.Text = "ITEM_NAME : " + Util.NVC(DataTableConverter.GetValue(dr, "ITEM_NAME"));
            txtSize.Text = "VISIBLE_CHKECK : " + Util.NVC(DataTableConverter.GetValue(dr, "VISIBLE_CHKECK"));

            
            C1TreeViewItem parentitem = trvData.SelectedItem.FindParent<C1TreeViewItem>();
            System.Data.DataRowView drparent = parentitem.DataContext as System.Data.DataRowView;

            txtRead.Text = string.Format("Parent => {0} : {1} : {2} : {3}",
                         Util.NVC(DataTableConverter.GetValue(drparent, "KEY")),
                         Util.NVC(DataTableConverter.GetValue(drparent, "PARENT_KEY")),
                         Util.NVC(DataTableConverter.GetValue(drparent, "ITEM_NAME")),
                         Util.NVC(DataTableConverter.GetValue(drparent, "VISIBLE_CHKECK")));



        }

        private void Item_Add_Click(object sender, RoutedEventArgs e)
        {
            Tree_Item_Add(sender);
        }

        private void Item_Del_Click(object sender, RoutedEventArgs e)
        {
            Tree_Item_Del(sender);
        }

        #endregion

        #region Mehod

        private DataSet GetData()
        {
            DataTable dt = new DataTable("TB_DATA");
            dt.Columns.Add("KEY", typeof(string));
            dt.Columns.Add("PARENT_KEY", typeof(string));
            dt.Columns.Add("ITEM_NAME", typeof(string));
            dt.Columns.Add("VISIBLE_CHKECK", typeof(bool));

            dt.Rows.Add("Asia", null, "Asia", false);

            dt.Rows.Add("China", "Asia", "China", false);
            dt.Rows.Add("Japan", "Asia", "Japan", false);
            dt.Rows.Add("India", "Asia", "India", false);
            dt.Rows.Add("South Korea", "Asia", "South Korea", false);

            dt.Rows.Add("Seoul", "South Korea", "Seoul", true);
            dt.Rows.Add("Busan", "South Korea", "Busan", true);

            dt.Rows.Add("USA", null, "USA", false);

            dt.Rows.Add("Europe", null, "Europe", false);
            dt.Rows.Add("Austria", "Europe", "Austria", false);
            dt.Rows.Add("England", "Europe", "England", false);
            dt.Rows.Add("France", "Europe", "France", false);
            dt.Rows.Add("Germay", "Europe", "Germay", false);
            dt.Rows.Add("Spain", "Europe", "Spain", false);

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.Relations.Add("Relations", ds.Tables["TB_DATA"].Columns["KEY"], ds.Tables["TB_DATA"].Columns["PARENT_KEY"]);
            return ds;
        }

        private void Tree_Item_Add(object sender)
        {
            C1TreeViewItem treeViewItem = sender as C1TreeViewItem;

            //MessageBox.Show(DataTableConverter.Convert(trvData.SelectedItem, "");




        }

        private void Tree_Item_Del(object sender)
        {



        }

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        #endregion
    }
}
