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
using System.Windows.Input;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_091 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;

        public PGM_GUI_091()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize
        private void Initialize()
        {
            testData();
            
            //오늘 날자로 고정            
            dtpPalletConfig.SelectedDateTime = (DateTime)System.DateTime.Now;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

       

        private void dgPallethistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgPallethistory.CurrentRow.Index;


            if (dgPallethistory.CurrentColumn.Index == 1)
            {
                //string selectLot = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "LOT_ID").ToString();

                //string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                //string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

                //PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            }
            else if (dgPallethistory.CurrentColumn.Index == 0)
            {
                string selectPallet = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "BOX_ID").ToString();

                txtPalleyIdR.Text = selectPallet;
            }
        }
        #endregion

        #region Mehod

        private void testData()
        {
            testGridData();            
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("PALLET_ID", typeof(string));
            dtSearchResult.Columns.Add("PALLETCFG_DATE", typeof(string));
            dtSearchResult.Columns.Add("BOX_ID", typeof(string));
            dtSearchResult.Columns.Add("BOXMAKE_DATE", typeof(string));
            dtSearchResult.Columns.Add("OUPUT_ID", typeof(string));
          

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "PP00077431", "2016-07-15 오전 08:30:10", "31453487T716070718", "2016-07-15 오전 08:30:10", "" });
            menulist.Add(new object[] { "PP00077431", "2016-07-15 오전 08:31:13", "31453487T716071801", "2016-07-15 오전 08:31:13", "" });
            menulist.Add(new object[] { "PP00077431", "2016-07-15 오전 08:33:13", "31453487T716071802", "2016-07-15 오전 08:31:13", "" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgPallethistory, dtSearchResult);
        }       

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }


        #endregion

        private void btnConfigCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPalletLabel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkPalletId_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkPalletId.IsChecked)
            {
                txtPalletId.IsReadOnly = true;
            }
            else
            {
                txtPalletId.IsReadOnly = false;
            }
        }

        private void dgPalletId_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
