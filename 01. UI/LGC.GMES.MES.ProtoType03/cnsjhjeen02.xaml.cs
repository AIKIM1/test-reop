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
    public partial class cnsjhjeen02 : UserControl
    {

        DataTable dtMain = null;


        #region Declaration & Constructor 
        public cnsjhjeen02()
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
          

            if (cboProductType.SelectedItem == null || cboProcess.SelectedItem == null || cboProduct.SelectedItem == null)
            {
                MessageBox.Show("빠진게 있습니다.");
            }else
            {
                string prodType = cboProductType.SelectedItem.ToString();
                string process = cboProcess.SelectedItem.ToString();
                string product = cboProduct.SelectedItem.ToString();
                dtMain = new DataTable();
                dtMain.Columns.Add("LotId", typeof(string));
                dtMain.Columns.Add("Product", typeof(string));
                dtMain.Columns.Add("ProdcutionDate", typeof(string));
                dtMain.Columns.Add("Process", typeof(string));
                dtMain.Columns.Add("Status", typeof(string));

                DataRow newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LGC-KOR13.07.16A4MD0028", "Audi BEV A4MF", DateTime.Now.ToString(), process, "작업중" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LGC-KOR13.07.16A4MD0026", "Audi BEV A4MF", DateTime.Now.ToString(), process, "작업중" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LGC-KOR13.07.16A4MD0027", "Audi BEV A4MF", DateTime.Now.ToString(), process, "작업중" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LGC-KOR13.07.16A4MD0029", "Audi BEV A4MF", DateTime.Now.ToString(), process, "작업중" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LGC-KOR13.07.16A4MD0028", "Audi BEV A4MF", DateTime.Now.ToString(), process, "작업중" };
                dtMain.Rows.Add(newRow);

                dgMain.ItemsSource = DataTableConverter.Convert(dtMain);
            }
        }
    }
}
