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
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class nondle02 : UserControl
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        DataTable dtMain = new DataTable();
        CommonDataSet _Com = new CommonDataSet();
        DataRow newRow = null;

        public nondle02()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
        }

        private void dgLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            _Util.gridLoadedRowHeaderPresenter(sender, e);
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Get_WorkOrderData();
            Get_MainData();
        }
        private void LotCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotList.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;            
            
            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (rowIndex != i)
                    (dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
            }

            Get_ReadyBasketData();

            Get_InputBasketData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_OutTrayData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }

        private void InProdCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReadyCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OutTrayCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Mehod
        private void InitializeControls()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE", typeof(string));
            dt.Columns.Add("NAME", typeof(string));

            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("N1APKG431", "17라인 PKG 1호기");
            dt.Rows.Add("N1APKG432", "17라인 PKG 2호기");
            
            cboEquipment.ItemsSource = dt.Copy().AsDataView();

            dt.Clear();
            
            dt.Rows.Add("64", "64");
            dt.Rows.Add("66", "66");
            dt.Rows.Add("88", "88");
            dt.Rows.Add("108", "108");
            dt.Rows.Add("110", "110");
            dt.Rows.Add("128", "128");
            dt.Rows.Add("132", "132");
            dt.Rows.Add("151", "151");
            dt.Rows.Add("1024", "1024");

            cboTrayKind.ItemsSource = dt.Copy().AsDataView();

            cboTrayKind.SelectedIndex = 0;
            cboEquipment.SelectedIndex = 0;

            txtModel.Text = "XPQ";
            txtModelAlias.Text = "P2.7S";
            txtType.Text = "A";
            txtTrayType.Text = "XFHA";
        }

        private void Get_WorkOrderData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("WORKDATE", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));
            dtMain.Columns.Add("OPERCODE", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("ORDERQTY", typeof(Int32));
            dtMain.Columns.Add("WORKQTY", typeof(Int32));

            Random rnd = new Random();

            if (rnd.Next(0, 2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "20160630", "2886316", "0010", "MBEV3601AM", 1000, 200 };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "20120530", "2886317", "0020", "MBEV3601AP", 2000, 0 };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "20120501", "2886186", "9000", "MBEV3801AB", 1000000, 0 };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "20100331", "V4-1_1000A", "0010", "MBSLURRYAA2", 10000, 0 };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "20100331", "V4-1_1000A", "0010", "MBSLURRYAA2", 10000, 0 };
                dtMain.Rows.Add(newRow);
            }
            
            dgWorkorder.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_MainData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("LOTID", typeof(string));
            dtMain.Columns.Add("MODELID", typeof(string));
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("STATUS", typeof(string));
            dtMain.Columns.Add("INQTY", typeof(Int32));
            dtMain.Columns.Add("WIPQTY", typeof(Int32));
            dtMain.Columns.Add("REALEQPQTY", typeof(Int32));
            dtMain.Columns.Add("STARTTIME", typeof(string));
            dtMain.Columns.Add("EQPENDTIME", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));
            dtMain.Columns.Add("OPERCODE", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            
            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CCNPF30LL1", "CCN", "523275L1 양극", "작업중", 0, 219, 0, "2016-06-30 10:36", "", "1333256", "0010", "ENP523275AVPK", "BATTERY 523275L1 1582mAh  Packaged Cell" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CFSPF08LL1", "CFS", "3449109L1양극", "장비완료", 0, 0, 0, "2016-06-08 14:51", "2016-06-10 23:07", "1320027", "0010", "ENP3449A9AVPK", "NP3449109 AV PACKAGING" };
            dtMain.Rows.Add(newRow);


            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CFSPF07LL1", "CFS", "3449109L1양극", "장비완료", 0, 0, 0, "2016-06-07 9:29", "2016-06-12 22:37", "1320027", "0010", "ENP3449A9AVPK", "NP3449109 AV PACKAGING" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CELPF07LL1", "CEL", "343993L1 양극", "장비완료", 0, 0, 0, "2016-06-07 8:10", "2016-06-07 9:39", "1320021", "0010", "ENP343993APPK", "BATTERY 343993L1 1970mAh  Packaged Cell" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CELPF06LL1", "CEL", "343993L1 양극", "장비완료", 0, 0, 0, "2016-06-06 15:53", "2016-06-07 17:24", "1320021", "0010", "ENP343993APPK", "BATTERY 343993L1 1970mAh  Packaged Cell" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CCAPC07LL1", "CCA", "506274L1 양극", "장비완료", 0, 901, 0, "2016-03-07 8:08", "2016-03-07 12:25", "1286582", "0010", "ENP506274APPK", "BATTERY 506274L1 4030mAh Packaged Cell" };
            dtMain.Rows.Add(newRow);
            
            dgLotList.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_ReadyBasketData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("LARGELOT", typeof(string));
            dtMain.Columns.Add("FOLDLOT", typeof(string));
            dtMain.Columns.Add("SUBLOTID", typeof(string));
            dtMain.Columns.Add("WIPQTY", typeof(Int32));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            dtMain.Columns.Add("CREATEDATE", typeof(string));

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF27",	"CCN6FRFI11",	"N6FRFI1004",	320,		"ENP523275AVFD", "2016-06-27 15:29:41","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF27",	"CCN6FRFI11",	"N6FRFI1003",	420,		"ENP523275AVFD", "2016-06-27 15:25:46","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF27",	"CCN6FRFI11",	"N6FRFI1002",	420,		"ENP523275AVFD", "2016-06-27 15:25:41","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF27",	"CCN6FRFI11",	"N6FRFI1001",	420,		"ENP523275AVFD", "2016-06-27 15:25:38","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1009",	420,		"ENP523275AVFD", "2016-06-27 06:28:17","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1008",	420,		"ENP523275AVFD", "2016-06-27 06:28:14","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1007",	420,		"ENP523275AVFD", "2016-06-27 06:28:13","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1006",	420,		"ENP523275AVFD", "2016-06-27 05:19:41","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1005",	420,		"ENP523275AVFD", "2016-06-27 05:19:36","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);
            
            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1004",	420,		"ENP523275AVFD", "2016-06-27 05:19:35","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1003",	420,		"ENP523275AVFD", "2016-06-27 05:19:30","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1002",	420,		"ENP523275AVFD", "2016-06-27 05:19:16","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF26",	"CCN6FQFI12",	"N6FQFI1001",	420,		"ENP523275AVFD", "2016-06-27 05:19:14","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1011",	360,		"ENP523275AVFD", "2016-06-26 20:28:10","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1010",	420,		"ENP523275AVFD", "2016-06-26 18:40:00","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1009",	420,		"ENP523275AVFD", "2016-06-26 18:40:00","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1008",	420,		"ENP523275AVFD", "2016-06-26 18:39:59","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1007",	420,		"ENP523275AVFD", "2016-06-26 18:39:59","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1006",	420,		"ENP523275AVFD", "2016-06-26 18:39:58","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI12",	"N6FPFI1005",	420,		"ENP523275AVFD", "2016-06-26 18:39:57","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI11",	"N6FPFI1004",	165,		"ENP523275AVFD", "2016-06-25 20:45:35","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI11",	"N6FPFI1003",	315,		"ENP523275AVFD", "2016-06-25 20:45:28","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI11",	"N6FPFI1002",	315,		"ENP523275AVFD", "2016-06-25 20:45:25","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] {"523275L1 양극",	"CCN1PF25",	"CCN6FPFI11",	"N6FPFI1001",	315,		"ENP523275AVFD", "2016-06-25 20:45:21","ENP523275AVFD" };
            dtMain.Rows.Add(newRow);


            dgReadyBox.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_InputBasketData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            dtMain = new DataTable();
            dtMain.Columns.Add("SUBLOTID", typeof(string));
            dtMain.Columns.Add("FOLDLOT", typeof(string));
            dtMain.Columns.Add("WIPQTY", typeof(Int32));
            dtMain.Columns.Add("WIPSTAT", typeof(string));
            dtMain.Columns.Add("MODELID", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            dtMain.Columns.Add("INPUTDATE", typeof(string));

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2024", "CDPPG04ML1", 240, "RUN", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:35" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2023", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:34" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2025", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:32" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2026", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:31" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2027", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:30" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2029", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:29" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2028", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:28" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2030", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:26" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2055", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:25" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2056", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:24" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2057", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:23" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2058", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:22" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2059", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:19" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2060", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:18" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2043", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:16" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2042", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:15" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2041", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 17:00:14" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2044", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:45" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2045", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:44" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2046", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:42" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2047", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:41" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2048", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:40" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2049", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:39" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2050", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:38" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2051", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:36" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2052", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:29" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2053", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 11:55:28" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2010", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:52" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2011", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:51" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2012", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:50" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2013", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:49" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2014", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:48" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2040", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:47" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2039", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:46" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2038", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:44" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2037", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:43" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2036", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:42" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2035", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:41" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2034", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:39" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2033", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:38" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "P6G3FI2032", "CDPPG04ML1", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:37" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "CDPPG04ML1", "P6G3FI2031", 240, "TERMINATE", "CDP", "ENP426296AVFD", "BATTERY 426296L1 3675mAh  Folding Cell", "2016-07-04 10:07:36" };
            dtMain.Rows.Add(newRow);



            dgInBox.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_OutTrayData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            dtMain = new DataTable();
            dtMain.Columns.Add("TRAYID", typeof(string));
            dtMain.Columns.Add("QTY", typeof(Int32));
            dtMain.Columns.Add("SPECIALYN", typeof(string));
            dtMain.Columns.Add("REMARK", typeof(string));
            dtMain.Columns.Add("CONFIRMYN", typeof(string));
            dtMain.Columns.Add("VITALYN", typeof(string));
            dtMain.Columns.Add("POORTYN", typeof(string));
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("MODELID", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            dtMain.Columns.Add("OUTDATE", typeof(string));


            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000580", 11, "N", "", "N", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:27:59" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001528", 106, "N", "", "N", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:18:28" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000099", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:14:31" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001491", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:10:35" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000587", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:06:36" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000686", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 17:02:07" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000083", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:58:18" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000687", 107, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:54:30" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000683", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:50:35" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000693", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:46:45" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000704", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:42:59" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001493", 107, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:39:07" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000575", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:34:30" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000092", 107, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:30:35" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001609", 106, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:14:30" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000559", 106, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:10:10" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001515", 106, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:06:39" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000573", 106, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 16:01:54" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000623", 107, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 15:56:18" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD000649", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 15:49:48" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "LSQD001508", 108, "N", "", "Y", "N", "N", "356479L1 양극", "CDP", "ENP356479APPK", "BATTERY 356479L1 2920mAh Packaged Cell", "2016-07-04 15:45:03" };
            dtMain.Rows.Add(newRow);
            
            dgOutTray.ItemsSource = DataTableConverter.Convert(dtMain);

            ConfirmTrayCheck();
        }

        private void ConfirmTrayCheck()
        {
            
        }

        #endregion

        //private void dgOutTray_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        //{
        //    //DataGridRow row = e.Row;
        //    DataRowView rView = (DataRowView)e.Row.DataItem;
        //    if (rView != null && rView.Row.ItemArray[4].ToString().Contains("ERROR"))
        //    {
        //        ((DataRowView)e.Row.DataItem)..Background = new SolidColorBrush(Color.Red);
        //    }
        //    else
        //    {
        //        e.Row.Background = new SolidColorBrush(Color.Green);
        //    }
        //}
    }
}
