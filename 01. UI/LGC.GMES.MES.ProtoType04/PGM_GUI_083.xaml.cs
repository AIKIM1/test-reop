/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_083 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtResult;

        public PGM_GUI_083()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        #endregion
       
        #region Initialize
        private void Initialize()
        {           
            testData();
        }
        #endregion

        #region Event
        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcelActHistory_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();
        }

        private void testGridData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("LOT_ID", typeof(string));
            dtResult.Columns.Add("PALLET_ID", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "31419205T715122801", "pallet001" });
            menulist.Add(new object[] { "31419205T715122802", "pallet001" });
            menulist.Add(new object[] { "31419205T715122803", "pallet002" });
            menulist.Add(new object[] { "31419205T715122804", "pallet003" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = item;
                dtResult.Rows.Add(newRow);
            }

            //SetBinding(dgResult, dtResult);
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion

               

       
    }
}
