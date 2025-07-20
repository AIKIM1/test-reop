/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class cnsjhjeen03 : UserControl
    {
        #region Declaration & Constructor 
        DataRow newRow = null;
        DataTable dtMain = null;
       



        DataTable dtDetail = new DataTable();

        public cnsjhjeen03()
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

        private void dgDetail_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

        }

        private void btnTestPrint_Click(object sender, RoutedEventArgs e)
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("BoxId", typeof(string));
            dtMain.Columns.Add("Product", typeof(string));
            dtMain.Columns.Add("Line", typeof(string));
            dtMain.Columns.Add("Quantity", typeof(string));
            dtMain.Columns.Add("PrintTime", typeof(string));

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "Coating", "V6_음극", "XA2", "", DateTime.Now.ToString() };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "Coating", "56_음극", "XA2", "", DateTime.Now.ToString() };
            dtMain.Rows.Add(newRow);

            dgMain.ItemsSource = DataTableConverter.Convert(dtMain);


          






        }
    }
}
