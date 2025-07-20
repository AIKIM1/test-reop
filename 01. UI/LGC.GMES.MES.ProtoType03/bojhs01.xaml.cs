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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class bojhs01 : UserControl
    {
        #region Declaration & Constructor 
        public bojhs01()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnInputInputProduct_Click(object sender, RoutedEventArgs e)
        {
            setDgInputProduct();
        }
        private void tbInputLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key ==Key.Enter)
            {
                setDgInputProduct();
            }
        }

        private void btnInputInputKeyPart_Click(object sender, RoutedEventArgs e)
        {
            setDgInputKeyPart();
        }

        private void btnDeleteInputKeyPart_Click(object sender, RoutedEventArgs e)
        {

            DataTable dtDetail = null;
            if (dgInputKeyPart.ItemsSource != null)
            {
                dtDetail = ((DataView)dgInputKeyPart.ItemsSource).ToTable();
            }
            
            if (dgInputKeyPart.SelectedIndex > 0)
            {
                //dgInputKeyPart.RemoveRow(dgInputKeyPart.SelectedIndex);
                if (dtDetail != null)
                {
                    if (dtDetail.Rows.Count > 0)
                    {
                        dtDetail.Rows[dgInputKeyPart.SelectedIndex].Delete();

                        dgInputKeyPart.ItemsSource = DataTableConverter.Convert(dtDetail);
                    }
                }
            }
            else
            {
                //dgInputKeyPart.RemoveRow(dgInputKeyPart.Rows.Count-2);
                if (dtDetail != null)
                {
                    if (dtDetail.Rows.Count > 0)
                    {
                        dtDetail.Rows[dtDetail.Rows.Count - 1].Delete();

                        dgInputKeyPart.ItemsSource = DataTableConverter.Convert(dtDetail);
                    }
                }
            }
        }

        private void tbInputKeyPart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                setDgInputKeyPart();
            }
        }


        #endregion

        #region Mehod

        private void setGridItemsSource()
        {
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dgInputKeyPart.Rows where row.Visibility == Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState != DataRowState.Added && (row.DataItem as DataRowView).Row.RowState != DataRowState.Detached select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dgInputKeyPart.ItemsSource as DataView).Table.Clone();
            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }
            table.AcceptChanges();
            dgInputKeyPart.ItemsSource = DataTableConverter.Convert(table);
        }

        private void setDgInputProduct()
        {
            if (tbInputLotID.Text != "")
            {
                DataTable dtDetail = null;
                if (dgInputProduct.ItemsSource != null)
                {
                    dtDetail = ((DataView)dgInputProduct.ItemsSource).ToTable();
                }
                if (dtDetail == null)
                {
                    dtDetail = new DataTable();
                    dtDetail.Columns.Add("PRODUCT", typeof(string));
                    dtDetail.Columns.Add("LOTID", typeof(string));
                    dtDetail.Columns.Add("ACTDTTM", typeof(string));
                }

                DataRow newRow = dtDetail.NewRow();
                newRow["PRODUCT"] = "TEST" + dtDetail.Rows.Count;
                newRow["LOTID"] = tbInputLotID.Text;
                newRow["ACTDTTM"] = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                dtDetail.Rows.Add(newRow);
                dgInputProduct.ItemsSource = DataTableConverter.Convert(dtDetail);

                tbInputLotID.Text = "";
            }
        }

        private void setDgInputKeyPart()
        {
            if (tbInputKeyPart.Text != "")
            {

                //dgInputKeyPart.BeginNewRow();
                //dgInputKeyPart.EndNewRow(true);

                DataTable dtDetail = null;
                if (dgInputKeyPart.ItemsSource != null)
                {
                    dtDetail = ((DataView)dgInputKeyPart.ItemsSource).ToTable();
                }
                if (dtDetail == null)
                {
                    dtDetail = new DataTable();
                    dtDetail.Columns.Add("LOTID", typeof(string));
                    dtDetail.Columns.Add("COUNT", typeof(string));
                    dtDetail.Columns.Add("SEQ", typeof(string));
                }

                DataRow newRow = dtDetail.NewRow();
                newRow["LOTID"] = tbInputKeyPart.Text;
                newRow["COUNT"] = "1";
                newRow["SEQ"] = dtDetail.Rows.Count;
                dtDetail.Rows.Add(newRow);
                dgInputKeyPart.ItemsSource = DataTableConverter.Convert(dtDetail);
                ((DataView)dgInputKeyPart.ItemsSource).RowStateFilter = DataViewRowState.Added;
                tbInputKeyPart.Text = "";
            }
        }


        #endregion

        
    }
}
