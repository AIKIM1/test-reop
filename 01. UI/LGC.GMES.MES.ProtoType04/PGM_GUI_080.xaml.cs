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
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_080 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtPrintHistory;        
        public PGM_GUI_080()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        private void btnTestOut_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAceept_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();
            testComboSet();
        }

        private void testGridData()
        {
            dtPrintHistory = new DataTable();
            dtPrintHistory.Columns.Add("BOX_ID", typeof(string));
            dtPrintHistory.Columns.Add("PRODUCT_NAME", typeof(string));
            dtPrintHistory.Columns.Add("LINE", typeof(string));
            dtPrintHistory.Columns.Add("PRODUCT_CNT", typeof(string));
            dtPrintHistory.Columns.Add("DATE", typeof(string));
            

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "JH3TTE4003PG15TA0003", "14S4P_BMA", "E4", "3", "2016-07-13 19:30:18" });
            menulist.Add(new object[] { "JH3TTE4003PG15TA0003", "14S4P_BMA", "E4", "3", "2016-07-13 19:30:20" });
            menulist.Add(new object[] { "JH3TTE4003PG15TA0003", "14S4P_BMA", "E4", "3", "2016-07-13 19:30:23" });
            menulist.Add(new object[] { "JH3TTE4003PG15TA0003", "14S4P_BMA", "E4", "3", "2016-07-13 19:30:23" });            

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtPrintHistory.NewRow();
                newRow.ItemArray = item;
                dtPrintHistory.Rows.Add(newRow);
            }

            SetBinding(dgPrintHistory, dtPrintHistory);
        }
        private void testComboSet()
        {
            testSet_Cbo(cboLabelType,   "1", "CARRER");
            testSet_Cbo(cboVersion,     "1", "TYPE1");
            testSet_Cbo(cboProduct,     "1", "14S4P_BMA");
            testSet_Cbo(cboAera,        "1", "ESS4호기_BMA");
            testSet_Cbo(cboWorkGroup,   "1", "A");
            testSet_Cbo(cboPrintConnet, "1", "COM");

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전체", "0" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + key, "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "2", "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "3", "3" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count > 0)
            {
                //CheckAll();
            }
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        
    }
}
