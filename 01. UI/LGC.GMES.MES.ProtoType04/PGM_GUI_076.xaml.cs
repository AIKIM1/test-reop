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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_076 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_076()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private DataView dvRootNodes;

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetInfomation();
            }
        }

        private void btnSearchLotId_Click(object sender, RoutedEventArgs e)
        {
            SetInfomation();
        }

        #region Event - ContextMenu ItemClick
        private void Item_AllCopy_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Item_Copy_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Item_Change_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_LOTCHANGE";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_Scrap_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_LOTSCRAP";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_AddKeyPart_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_ADDKEYPART";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_AddInspectionDate_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_ADDINSPECTIONDATA";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_AddDetailData_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_ADDDETAILDATA";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_ChangeHistory_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_076_CHANGEHISTORY";

            PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        }
        private void Item_LabelPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Event - Excel버튼
        private void btnExcelActHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcelInspectionData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcelDetailData_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        void popup_Closed(object sender, EventArgs e)
        {
            C1Window popup = sender as C1Window;
            if (popup.DialogResult == MessageBoxResult.OK)
            {

            }
            else
            {

            }
        }

        #endregion

        #region Mehod

        private void SetInfomation()
        {
            DataSet dsLotInfo = null;
            setLotInfoText(dsLotInfo);
            setTreeView(dsLotInfo);
            setLotActHistory(dsLotInfo);
            setInspectionData(dsLotInfo);
            setDetailData(dsLotInfo);

        }

        private void setTreeView(DataSet dsLotInfo)
        {
            DataSet ds = GetData(txtSearchLotId.Text);
            dvRootNodes = ds.Tables["TB_DATA"].DefaultView;
            dvRootNodes.RowFilter = "PARENT_KEY IS NULL";
            trvKeypartList.ItemsSource = dvRootNodes;
            TreeItemExpandAll();
        }

        private void setLotInfoText(DataSet dsLotInfo)
        {
            txtLotInfoCreateDate.Text = "2016-07-13 16:28:16";
            txtLotInfoLotType.Text = "양산 (PROD)";
            txtLotInfoProductId.Text = "EVPVPCMPL65B0";
            txtLotInfoProductName.Text = "Audi_37h CMA(9호기)";
            txtLotInfoWipState.Text = "양품";
            txtLotInfoModel.Text = "37AH";
            txtLotInfoWipProcess.Text = "9호기 CMA 이송 공정[MAAB09] (MA903)";
        }
        private void setLotActHistory(DataSet dsLotInfo)
        {

        }
        private void setInspectionData(DataSet dsLotInfo)
        {

        }
        private void setDetailData(DataSet dsLotInfo)
        {

        }

        private DataSet GetData(string sLotId)
        {
            DataTable dt = new DataTable("TB_DATA");
            dt.Columns.Add("KEY", typeof(string));
            dt.Columns.Add("PARENT_KEY", typeof(string));
            dt.Columns.Add("ITEM_NAME", typeof(string));
            dt.Columns.Add("VISIBLE_CHKECK", typeof(bool));

            dt.Rows.Add(sLotId, null, sLotId, false);

            dt.Rows.Add("PF11M10963", sLotId, "PF11M10963", true);
            dt.Rows.Add("PF09M10236", sLotId, "PF09M10236", true);
            dt.Rows.Add("PF11M20141", sLotId, "PF11M20141", true);
            dt.Rows.Add("PF11M10869", sLotId, "PF11M10869", true);
            dt.Rows.Add("PF11M20237", sLotId, "PF11M20237", true);
            dt.Rows.Add("PF11M20005", sLotId, "PF11M20005", true);
            dt.Rows.Add("PF11M20142", sLotId, "PF11M20142", true);
            dt.Rows.Add("PF11M14941", sLotId, "PF11M14941", true);
            dt.Rows.Add("EABS00121AC:X310H071605250299A", sLotId, "EABS00121AC:X310H071605250299A", true);

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.Relations.Add("Relations", ds.Tables["TB_DATA"].Columns["KEY"], ds.Tables["TB_DATA"].Columns["PARENT_KEY"]);
            return ds;
        }

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

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

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        #endregion
    }
}