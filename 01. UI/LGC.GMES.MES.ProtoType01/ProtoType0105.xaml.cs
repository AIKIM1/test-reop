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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType01
{
    /// <summary>
    /// MNT_selEquipmentProcess_TS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProtoType0105 : UserControl
    {
        #region Declaration & Constructor 

        public ProtoType0105()
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
            DataGrid02(dgData02);
        }

        #endregion

        #region Event

        private void btnColAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01ColAdd(dgData01);
        }

        private void btnRowAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowAdd(dgData01);
        }

        private void btnRowDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgData01.CurrentRow != null)
            {
                dgData01.IsReadOnly = false;
                //dgData01.EndNewRow(true);
                dgData01.RemoveRow(dgData01.CurrentRow.Index);
                dgData01.IsReadOnly = true;
                txtRowCount.Text = dgData01.IsAddingNewRowsAllowed.ToString() + " GetRowCount : " + dgData01.GetRowCount() + " Rows.Count : " + dgData01.Rows.Count + " TopRows.Count : " + dgData01.TopRows.Count + " BottomRows.Count : " + dgData01.BottomRows.Count;
            }
        }

        #endregion

        #region Mehod

        private void DataGrid01ColAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.Columns.Clear();

            List<string> sHeadNames01 = new List<string>();
            sHeadNames01.Add("Lot ID");
            sHeadNames01.Add("Lot ID");

            List<string> sHeadNames02 = new List<string>();
            sHeadNames02.Add("Lot Name");
            sHeadNames02.Add("Lot Name");

            //LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dg, "LOTID", sHeadNames01, null, false, false, false, false, 200, HorizontalAlignment.Center, Visibility.Visible);
            //LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dg, "LOTNAME", sHeadNames02, null, false, false, false, false, 200, HorizontalAlignment.Center, Visibility.Visible);

            LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dg, "LOTID", sHeadNames01, null, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1,C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
            LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dg, "LOTNAME", sHeadNames02, null, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);

            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dg.BeginEdit();
            dg.ItemsSource = DataTableConverter.Convert(dt);
            dg.EndEdit();

            txtRowCount.Text = dg.IsAddingNewRowsAllowed.ToString() + " GetRowCount : " + dg.GetRowCount() + " Rows.Count : " + dg.Rows.Count + " TopRows.Count : " + dg.TopRows.Count + " BottomRows.Count : " + dg.BottomRows.Count;
        }

        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.IsReadOnly = false;
            dg.BeginNewRow();
            dg.EndNewRow(true);
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "LOTID", "11111");
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "LOTNAME", "11111");
            dg.IsReadOnly = true;
            txtRowCount.Text = dg.IsAddingNewRowsAllowed.ToString() + " GetRowCount : " + dg.GetRowCount() + " Rows.Count : " + dg.Rows.Count + " TopRows.Count : " + dg.TopRows.Count + " BottomRows.Count : " + dg.BottomRows.Count;
        }

        private void DataGrid02(C1.WPF.DataGrid.C1DataGrid dg)
        {
            Set_DataGrid02_Combo_Item(dg.Columns["COL01"], 1);
            Set_DataGrid02_Combo_Item(dg.Columns["COL02"], 2);

            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }

            DataRow dr;

            dr = dt.NewRow();
            dr["COL01"] = "CD01";
            dr["COL02"] = "CD0102";
            dr["COL03"] = "CD010202";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["COL01"] = "CD02";
            dr["COL02"] = "CD0203";
            dr["COL03"] = "CD020303";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["COL01"] = "CD03";
            dr["COL02"] = "CD0303";
            dr["COL03"] = "CD030303";
            dt.Rows.Add(dr);

            dg.BeginEdit();
            dg.ItemsSource = DataTableConverter.Convert(dt);
            dg.EndEdit();
        }

        private void Set_DataGrid02_Combo_Item(C1.WPF.DataGrid.DataGridColumn col, int colidx )
        {
            DataTable dt = new DataTable();
            DataRow newRow = null;

            switch (colidx)
            {
                case 1:
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD01", "CD01-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD02", "CD02-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD03", "CD03-A" };
                    dt.Rows.Add(newRow);
                    break;
                case 2:
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0101", "CD0101-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0102", "CD0102-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0103", "CD0103-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0201", "CD0201-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0202", "CD0202-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0203", "CD0203-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0301", "CD0301-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0302", "CD0302-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0303", "CD0303-A" };
                    dt.Rows.Add(newRow);

                    break;
            }

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        private void dgData02_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;

            if (cbo == null)
            {
                return;
            }
            if (Convert.ToString(e.Column.Name) == "COL01")
            {
                return;
            }

            string tmpcol01 = Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "COL01"));
            string tmpcol02 = Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "COL02"));

            DataTable dt = new DataTable();
            DataRow newRow = null;

            switch (tmpcol01)
            {
                case "CD01":
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0101", "CD0101-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0102", "CD0102-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0103", "CD0103-A" };
                    dt.Rows.Add(newRow);

                    cbo.ItemsSource = DataTableConverter.Convert(dt);
                    cbo.SelectedValue = tmpcol02;
                    break;
                case "CD02":
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0201", "CD0201-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0202", "CD0202-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0203", "CD0203-A" };
                    dt.Rows.Add(newRow);

                    cbo.ItemsSource = DataTableConverter.Convert(dt);
                    cbo.SelectedValue = tmpcol02;
                    break;
                case "CD03":
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0301", "CD0301-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0302", "CD0302-A" };
                    dt.Rows.Add(newRow);

                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { "CD0303", "CD0303-A" };
                    dt.Rows.Add(newRow);

                    cbo.ItemsSource = DataTableConverter.Convert(dt);
                    cbo.SelectedValue = tmpcol02;
                    break;
            }
        }
    }
}

