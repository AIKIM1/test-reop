/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_PRODTREE : C1Window, IWorkArea
    {
        #region Declaration & Constructor      
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        DataSet DS = new DataSet();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string SHOPID
        {
            get
            {
                return sSHOPID;
            }

            set
            {
                sSHOPID = value;
            }
        }

        public string PRODID
        {
            get
            {
                return sPRODID;
            }

            set
            {
                sPRODID = value;
            }
        }

        private string sSHOPID = "";
        private string sPRODID = "";      

        public COM001_035_PRODTREE()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    string dtText = tmps[0] as string;
                    string dtText1 = tmps[1] as string;

                    if(dtText.Length > 0)
                    {
                        sSHOPID = dtText;
                    }

                    if (dtText1.Length > 0)
                    {
                        sPRODID = dtText1;
                    }

                    setPRODTREE();                    
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void trvData_ItemExpanded(object sender, SourcedEventArgs e)
        {
            rb_Checked();
        }

        private void rbCheck_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            string prodid = (rb.DataContext as DataRowView).Row["PRODID"].ToString();
            string from_prodid = (rb.DataContext as DataRowView).Row["FROM_PRODID"].ToString();

            string[] spot_prodid = prodid.Split('.');
            PRODID = spot_prodid[0];

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        #endregion

        #region Mehod

        private void setPRODTREE()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = sSHOPID;
                Indata["PRODID"] = sPRODID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_MBOM_EXPLOSION", "RQSTDT", "TREEDATA", IndataTable);

                DS.Tables.Add("TREEDATA");
                DS.Tables["TREEDATA"].Merge(dtResult);            

                DataSet TEMP_DS = new DataSet(); //PRIMARY 키 중복때문에 RELATIONS에서 ERROR 나서 중복 변형 후 RELATIONS 수행              

                DataView dvRootNodes;
                //중복키 변형
                TEMP_DS = MODY_DATASET();

                TEMP_DS.Relations.Add("Relations", TEMP_DS.Tables["TREEDATA"].Columns["PRODID"], TEMP_DS.Tables["TREEDATA"].Columns["FROM_PRODID"]);             
                dvRootNodes = DS.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_PRODID  IS NULL";

                trvData.ItemsSource = dvRootNodes;
                TreeItemExpandAll();    
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet MODY_DATASET()
        {
            try
            {
                DataSet dsModi = new DataSet();
                dsModi = DS;
                string sProdid = string.Empty;
                int cnt = 0;

                for (int i = 0; i < dsModi.Tables["TREEDATA"].Rows.Count; i++)
                {
                    sProdid = dsModi.Tables["TREEDATA"].Rows[i]["PRODID"].ToString();

                    for (int j = 0; j < dsModi.Tables["TREEDATA"].Rows.Count; j++)
                    {
                        if (i != j)
                        {
                            if (sProdid == dsModi.Tables["TREEDATA"].Rows[j]["PRODID"].ToString())
                            {
                                if (cnt == 0)
                                {
                                    dsModi.Tables["TREEDATA"].Rows[j]["PRODID"] = sProdid + "                                                            .";
                                }
                                else if (cnt == 1)
                                {
                                    dsModi.Tables["TREEDATA"].Rows[j]["PRODID"] = sProdid + "                                                            ..";
                                }
                                else if (cnt == 3)
                                {
                                    dsModi.Tables["TREEDATA"].Rows[j]["PRODID"] = sProdid + "                                                            ...";
                                }
                                cnt++;
                            }
                        }
                    }
                    cnt = 0;
                }

                return dsModi;
            }
            catch(Exception ex)
            {                
                throw ex;                
            }
        }

        public DataTable modifyTable(DataTable dt)
        {
            dt.Columns.Add("FROM_PRODID", typeof(string));
            dt.Columns["BOM_PATH"].ColumnName = "PATH1";
            string base_prod = string.Empty;

            for(int i = 0; i < dt.Rows.Count; i++)
            {                
                string[] bom_path = dt.Rows[i]["PATH1"].ToString().Split('/');

                if(bom_path.Length ==1)
                {
                    if(i==0) base_prod = dt.Rows[i]["PRODID"].ToString().Trim();

                    dt.Rows[i]["FROM_PRODID"] = "";
                    dt.Rows[i]["PATH1"] = "/";
                }
                else if(bom_path.Length == 2)
                {
                    if(dt.Rows[i]["PRODID"].Equals(bom_path[bom_path.Length-1].Trim()))
                    {
                        dt.Rows[i]["FROM_PRODID"] = bom_path[bom_path.Length - 2].Trim();
                        dt.Rows[i]["PATH1"] = bom_path[1].ToString().Trim();
                    }                  
                }
                else if (bom_path.Length == 3)
                {
                    if (dt.Rows[i]["PRODID"].Equals(bom_path[bom_path.Length - 1].Trim()))
                    {
                        dt.Rows[i]["FROM_PRODID"] = bom_path[bom_path.Length - 2].Trim();
                        dt.Rows[i]["PATH1"] = bom_path[1].ToString().Trim() + "/" + bom_path[2].ToString().Trim(); ;
                    }
                }
                else
                {
                    if (dt.Rows[i]["PRODID"].Equals(bom_path[bom_path.Length - 1].Trim()))
                    {
                        dt.Rows[i]["FROM_PRODID"] = bom_path[bom_path.Length - 2].Trim();
                        dt.Rows[i]["PATH1"] = bom_path[1].ToString().Trim() + "/" + bom_path[2].ToString().Trim() + "/" + bom_path[3].ToString().Trim();
                    }
                }
            }
            return dt;
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
                    if ((childItem.DataContext as DataRowView).Row.ItemArray[0].Equals(sPRODID))
                    {
                        childItem.IsSelected = true;
                    }

                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        private void rb_Checked()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNode(item);
            }
        }

        public void TreeItemExpandNode(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem childItem in items)
            {
                if ((childItem.DataContext as DataRowView).Row.ItemArray[0].Equals(sPRODID))
                {
                    childItem.IsSelected = true;
                }
                TreeItemExpandNode(childItem);
            }
        }


        #endregion


    }
}
