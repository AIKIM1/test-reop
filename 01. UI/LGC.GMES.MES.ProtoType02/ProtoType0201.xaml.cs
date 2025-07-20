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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0201 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtMain = new DataTable();
        DataTable dtDetail = new DataTable();
        DataRow newRow = null;
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        CommonDataSet _Com = new CommonDataSet();


        C1XLBook xlbook = new C1XLBook();
        XLSheet xlsheet;
        XLStyle sty_Board, sty_NoBoard;

        public ProtoType0201()
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
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        #endregion

        #region Event

        //Button =======================================================================================================
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!_Com.ChkDateValidation(dtpDateFrom.Text, dtpDateTo.Text))
            {
                return;
            }
            Get_MainData();

            dgMain.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            dgMain.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void btnMenuMove_Click(object sender, RoutedEventArgs e)
        {
            if (dgMain.SelectedItem != null)
            {
                this.FrameOperation.OpenMenu("MENUID070103", true, Util.NVC(DataTableConverter.GetValue(dgMain.SelectedItem, "MODEL")));
            }
        }

        private void btnControlSerach_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button tmp in Util.FindVisualChildren<Button>(LayoutRoot))
            {
                if (tmp.Name.ToString() == "btnSearch")
                { 
                    FrameOperation.PrintFrameMessage(tmp.Name.ToString());
                }
            }
        }

        private void btnExcelReport_Click(object sender, RoutedEventArgs e)
        {
            int fontsize = 20;
            //=================================================================================
            sty_Board = new XLStyle(xlbook);
            sty_Board.SetBorderStyle(XLLineStyleEnum.Thin);// = XLLineStyleEnum.Medium;
            sty_Board.BorderBottom = XLLineStyleEnum.Thin;
            sty_Board.BorderLeft = XLLineStyleEnum.Thin;
            sty_Board.BorderRight = XLLineStyleEnum.Thin;
            sty_Board.BorderTop = XLLineStyleEnum.Thin;
            sty_Board.BorderColorBottom = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorLeft = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorRight = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorTop = Color.FromArgb(255, 255, 0, 0);
            sty_Board.AlignVert = XLAlignVertEnum.Top;
            sty_Board.Font = new XLFont("Arial", fontsize, false, false);
            sty_Board.WordWrap = true;
            //=================================================================================
            sty_NoBoard = new XLStyle(xlbook);
            sty_NoBoard.AlignVert = XLAlignVertEnum.Top;
            sty_NoBoard.WordWrap = true;
            //=================================================================================

            xlbook.Sheets.RemoveAt(0);
            xlbook.Sheets.Add("Sheet01");

            xlsheet = xlbook.Sheets["Sheet01"];

            xlsheet.PrintSettings.MarginLeft = 0.45;
            xlsheet.PrintSettings.MarginRight = 0.45;
            xlsheet.PrintSettings.MarginBottom = 0.5;

            MergeXLCell(xlsheet, 1, 2, 2, 5, 8, "AAAAAAAAAAAA", "C");

            //==================================================================================
            WriteableBitmap img = new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/LGC.GMES.MES.ProtoType02;component/Images/GMES_icon.ico")));
            xlsheet[0, 0].Value = img;
            //==================================================================================
            XLPictureShape pic = new XLPictureShape(img, 3000, 3500, 2500, 900);
            pic.Rotation = 30.0f;
            pic.LineColor = Colors.DarkRed;
            pic.LineWidth = 100;
            xlsheet.Shapes.Add(pic);
            //==================================================================================

            xlbook.Save(@"c:\mybook.xls");

            System.Diagnostics.Process.Start(@"c:\mybook.xls");
        }

        //Main DataGrid =======================================================================================================
        private void dgMain_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODEL_NAME"), "").Trim() == "합계")
                {
                    if (Util.NVC(e.Cell.Column.Name) != "PROCESS")
                    {
                        if (e.Cell.Presenter != null)
                        { 
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                    }
                }
            }));
        }

        private void dgMain_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentRow == null)
            {
                return;
            }
                
            string strgetvalue = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MODEL"));

            DataRowView rowview = dg.SelectedItem as DataRowView;
            if (rowview != null)
            {
                FrameOperation.PrintFrameMessage("GetValue : " + strgetvalue + "    " + "DataRowView : " + Util.NVC(rowview["MODEL"]));
            }
            else
            {
                FrameOperation.PrintFrameMessage("GetValue : " + strgetvalue + "    " + "DataRowView : ");
            }
            Get_DetailData(dg.SelectedItem);
        }

        private void dgMain_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            _Util.gridLoadedRowHeaderPresenter(sender, e);
        }

        private void dgMain_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (!handle)
            {
                return;
            }
            e.Handled = true;

            double delta = e.Delta;
            double scalex = stfMain.ScaleX;
            double scaley = stfMain.ScaleY;

            if (scalex > 2 && delta > 0)
            {
                return;
            }

            if (scalex < 0.5 && delta < 0)
            {
                return;
            }

            stfMain.ScaleX += delta / 5000;
            stfMain.ScaleY += delta / 5000;

            FrameOperation.PrintFrameMessage(scalex.ToString());
        }

        //Detail DataGrid =======================================================================================================

        private void dgDetail_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            _Util.gridLoadedRowHeaderPresenter(sender,e);
        }

        #endregion

        #region Mehod

        //GetData =======================================================================================================

        private void Get_MainData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("PROCESS", typeof(string));
            dtMain.Columns.Add("MODEL_NAME", typeof(string));
            dtMain.Columns.Add("MODEL", typeof(string));
            dtMain.Columns.Add("VERSION", typeof(string));
            dtMain.Columns.Add("UNIT", typeof(string));
            dtMain.Columns.Add("STAY_LOT", typeof(Int32));
            dtMain.Columns.Add("STAY_WORK", typeof(Int32));
            dtMain.Columns.Add("HOLDING_LOT", typeof(Int32));
            dtMain.Columns.Add("HOLDING__WORK", typeof(Int32));
            dtMain.Columns.Add("RUN_LOT", typeof(Int32));
            dtMain.Columns.Add("RUN_WORK", typeof(Int32));
            dtMain.Columns.Add("COMPLETE_LOT", typeof(Int32));
            dtMain.Columns.Add("COMPLETE_WORK", typeof(Int32));
            dtMain.Columns.Add("TOTAL_LOT", typeof(Int32));
            dtMain.Columns.Add("TOTAL_WORK", typeof(Int32));


            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","V6_음극","XA2","","M",0,0,0,0,0,0,1,48,1,48};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","P1.5B음극","XA6","","M",0,0,0,0,0,0,1,2276,1,2276};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","P1.4음극","XAB","","M",0,0,0,0,0,0,1,645,1,645};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","V4-2음극","XAD","","M",0,0,0,0,0,0,1,44,1,44};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","JH2음극","XBD","","M",0,0,0,0,0,0,2,2846,2,2846};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","P2.7음극","XBG","","M",0,0,0,0,0,0,6,4808,6,4808};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","F2음극","XBI","","M",0,0,0,0,0,0,1,889,1,889};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","B10A음극","XBK","","M",0,0,0,0,0,0,4,8446,4,8446};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","N2.1음극","XBN","","M",0,0,0,0,0,0,1,1221,1,1221};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","A5-A음극(P36)","XBP","","M",0,0,0,0,0,0,1,224,1,224};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","P1.5B양극","XC6","T01","M",0,0,0,0,0,0,1,507,1,507};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","V4-2양극","XCD","","M",0,0,0,0,0,0,1,105,1,105};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","L4A양극","XD6","V01","M",0,0,0,0,0,0,1,908,1,908};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","P2.7양극","XDG","","M",0,0,0,0,0,0,1,209,1,209};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","E51(양극)","XDH","","M",0,0,0,0,0,0,1,93,1,93};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","LV4.5양극","XDJ","","M",0,0,0,0,0,0,1,331,1,331};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","B10A양극","XDK","","M",0,0,0,0,0,0,2,598,2,598};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","A5-A양극(P36)","XDP","","M",0,0,0,0,0,0,1,1800,1,1800};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","S15_음극","XGD","","M",0,0,0,0,0,0,1,893,1,893};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","E54_음극","XGG","T01","M",0,0,0,0,0,0,1,0,1,0};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","E63B-A_양극","XJ3","T00","M",0,0,0,0,0,0,1,1500,1,1500};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","E63B-A_양극","XJ3","","M",0,0,0,0,0,0,1,1500,1,1500};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","JH3_ESS_양극","XJU","","M",0,0,0,0,0,0,1,915,1,915};
            dtMain.Rows.Add(newRow);

            newRow=dtMain.NewRow();
            newRow.ItemArray=new object[]{"Coating","JP3_ESS_양극","XJV","","M",0,0,0,0,0,0,1,478,1,478};
            dtMain.Rows.Add(newRow);


            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "V6_음극","XA2","V00","M",46,111630,0,0,1,2600,0,0,47,114230};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "K4_음극", "XA3","V00","M",45,90000,0,0,1,1000,0,0,46,91000};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "P2.7S음극", "XA4","V01","M",38,68400,0,0,1,1800,1,1800,40,72000};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "F2음극", "XBI","V00","M",22,76570,0,0,0,0,0,0,22,76570};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "B10A음극", "XBK","V01","M",52,99052,0,0,0,0,0,0,52,99052};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E42음극", "XBM","T06","M",0,0,4,4880,0,0,0,0,4,4880};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "V6_양극", "XC2","T06","M",0,0,1,2600,0,0,0,0,1,2600};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "V6_양극", "XC2","T07","M",3,5600,0,0,0,0,0,0,3,5600};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "P2.7S양극", "XC4","V01","M",30,42000,3,3463,1,1400,0,0,34,46863};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "A5양극", "XCW","T23","M",5,7010,0,0,0,0,0,0,5,7010};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "A7양극", "XDE","T09","M",0,0,3,5400,0,0,0,0,3,5400};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "P2.7양극", "XDG","V04","M",49,75650,5,5950,0,0,0,0,54,81600};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{ "Roll - Press(압연전)", "F2양극", "XDI","V00","M",55,18658,0,0,1,3600,0,0,56,190180};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "B10A양극", "XDK","V01","M",44,77240,0,0,0,0,0,0,44,77240};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "F10양극", "XDL","T06","M",0,0,1,300,0,0,0,0,1,300};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E42양극", "XDM","T08","M",0,0,7,10390,0,0,0,0,7,10390};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E42양극", "XDM","T10","M",0,0,1,1500,0,0,0,0,1,1500};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "N2.1양극", "XDN","T11","M",0,0,2,1500,0,0,0,0,2,1500};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "N2.1양극", "XDN","T12","M",0,0,2,2380,0,0,0,0,2,2380};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "K4-1_음극", "XGC","T04","M",0,0,1,200,0,0,0,0,1,200};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E54_음극", "XGG","T05","M",1,2100,0,0,0,0,0,0,1,2100};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E61V_음극", "XGI","T05","M",8,7200,0,0,0,0,0,0,8,7200};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E48_음극", "XGQ","T04","M",0,0,1,220,0,0,0,0,1,220};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "P30_양극", "XJB","T04","M",6,10340,0,0,0,0,0,0,6,10340};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "K4-1_양극", "XJC","T03","M",0,0,1,200,0,0,0,0,1,200};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E63_양극", "XJH","T04","M",45,64070,2,2500,1,1500,0,0,48,68070};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "E61V_양극", "XJI","T05","M",4,7200,0,0,0,0,0,0,4,7200};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "JH3_ESS_양극", "XJU","T11","M",0,0,2,3660,0,0,0,0,2,3660};
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray=new object[]{"Roll - Press(압연전)", "JH3_ESS_양극", "XJU","V00","M",0,0,12,21050,0,0,0,0,12,21050};
            dtMain.Rows.Add(newRow);


            dgMain.ItemsSource = DataTableConverter.Convert(dtMain);

            string[] sColumnName = new string[] { "PROCESS", "MODEL_NAME", "MODEL", "VERSION" };
            _Util.SetDataGridMergeExtensionCol(dgMain, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            //==========================================================================================================================================

            dgMain.GroupBy(dgMain.Columns["PROCESS"]);
            dgMain.GroupRowPosition = DataGridGroupRowPosition.AboveData;

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["MODEL_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["MODEL"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["VERSION"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["UNIT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["STAY_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["STAY_WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["STAY_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["STAY_WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["HOLDING_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["HOLDING__WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["RUN_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["RUN_WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["COMPLETE_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["COMPLETE_WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["TOTAL_LOT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            DataGridAggregate.SetAggregateFunctions(dgMain.Columns["TOTAL_WORK"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

            //==========================================================================================================================================

        }

        private void Get_DetailData(object SelectedItem)
        {
            SetGridCboItem(dgDetail.Columns["LOT_TYPE"]);

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            if (Util.NVC(rowview["MODEL"]) == "XAD")
            {
                dtDetail = new DataTable();
                dtDetail.Columns.Add("MODEL", typeof(string));
                dtDetail.Columns.Add("VERSION", typeof(string));
                dtDetail.Columns.Add("WORK", typeof(string));
                dtDetail.Columns.Add("PATTERN", typeof(string));
                dtDetail.Columns.Add("WORK_STATE", typeof(string));
                dtDetail.Columns.Add("LOT_ID", typeof(string));
                dtDetail.Columns.Add("BIG_LOT", typeof(string));
                dtDetail.Columns.Add("LOCATION", typeof(string));
                dtDetail.Columns.Add("PROD_ID", typeof(string));
                dtDetail.Columns.Add("MODEL_NAME", typeof(string));
                dtDetail.Columns.Add("UNIT", typeof(string));
                dtDetail.Columns.Add("LOT_TYPE", typeof(string));
                dtDetail.Columns.Add("LOT_TYPE_NAME", typeof(string));
                dtDetail.Columns.Add("SHOP", typeof(string));
                dtDetail.Columns.Add("LINE", typeof(string));

                newRow = dtDetail.NewRow();
                newRow["MODEL"] = "XBY";
                newRow["VERSION"] = "T08";
                newRow["WORK"] = 700;
                newRow["PATTERN"] = 7478.632478632480;
                newRow["WORK_STATE"] = "WAIT";
                newRow["LOT_ID"] = "XBY5KW2111";
                newRow["BIG_LOT"] = "XBY5KW2";
                newRow["LOCATION"] = "GUARD";
                newRow["PROD_ID"] = "EVESESA0600I0";
                newRow["MODEL_NAME"] = "JH3_SLITTING ANODE";
                newRow["UNIT"] = "M";
                newRow["LOT_TYPE"] = "C";
                newRow["LOT_TYPE_NAME"] = "Pancake Lot";
                newRow["SHOP"] = "오창 1공장 전극";
                newRow["LINE"] = "1";
                dtDetail.Rows.Add(newRow);

                newRow = dtDetail.NewRow();
                newRow["MODEL"] = "XBY";
                newRow["VERSION"] = "T08";
                newRow["WORK"] = 700;
                newRow["PATTERN"] = 7478.6324;
                newRow["WORK_STATE"] = "WAIT";
                newRow["LOT_ID"] = "XBY5KW2112";
                newRow["BIG_LOT"] = "XBY5KW2";
                newRow["LOCATION"] = "GUARD";
                newRow["PROD_ID"] = "EVESESA0600I0";
                newRow["MODEL_NAME"] = "JH3_SLITTING ANODE";
                newRow["UNIT"] = "M";
                newRow["LOT_TYPE"] = "R";
                newRow["LOT_TYPE_NAME"] = "Pancake Lot";
                newRow["SHOP"] = "오창 1공장 전극";
                newRow["LINE"] = "1";
                dtDetail.Rows.Add(newRow);

                newRow = dtDetail.NewRow();
                newRow["MODEL"] = "XBY";
                newRow["VERSION"] = "T08";
                newRow["WORK"] = 700;
                newRow["PATTERN"] = 7478.632478632480;
                newRow["WORK_STATE"] = "WAIT";
                newRow["LOT_ID"] = "XBY5KW2113";
                newRow["BIG_LOT"] = "XBY5KW2";
                newRow["LOCATION"] = "GUARD";
                newRow["PROD_ID"] = "EVESESA0600I0";
                newRow["MODEL_NAME"] = "JH3_SLITTING ANODE";
                newRow["UNIT"] = "M";
                newRow["LOT_TYPE"] = "P";
                newRow["LOT_TYPE_NAME"] = "Pancake Lot";
                newRow["SHOP"] = "오창 1공장 전극";
                newRow["LINE"] = "1";
                dtDetail.Rows.Add(newRow);
                dgDetail.ItemsSource = DataTableConverter.Convert(dtDetail);
            }
            else
            {
                dgDetail.ItemsSource = null;
            }
        }

        //SetData =======================================================================================================

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            //DataTable dt = new DataTable();
            //dt.Columns.Add("CODE");
            //dt.Columns.Add("NAME");

            //dt.Rows.Add("C", "Coating Lot");
            //dt.Rows.Add("R", "Roll - Press Lot");
            //dt.Rows.Add("P", "Pancake Lot");

            //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }

        private void C1ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            dt.Rows.Add("C", "Coating Lot");
            dt.Rows.Add("R", "Roll - Press Lot");
            dt.Rows.Add("P", "Pancake Lot");

            C1.WPF.C1ComboBox cbo = sender as C1.WPF.C1ComboBox;
            cbo.ItemsSource = dt.AsDataView();// DataTableConverter.Convert(dt);
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void MergeXLCell(XLSheet xlsheet, int style, int x1, int x2, int y1, int y2, string value, string align)
        {
            // style : 0 - no ,  1 - board , 2 - board + color
            XLCellRange range = new XLCellRange();
            range = new XLCellRange(x1, x2, y1, y2);

            range.Style = sty_Board;
            xlsheet.MergedCells.Add(range);

            for (int i = x1; i < x2 + 1; i++)
            {
                xlsheet.Rows[i].Height = xlsheet.DefaultRowHeight; // DefaultRowHeight 
                for (int j = y1; j < y2 + 1; j++)
                {
                    if (style == 2)
                    {
                        xlsheet[i, j].Style = sty_Board.Clone();
                        xlsheet[i, j].Style.BackColor = Color.FromArgb(100, 242, 242, 242);
                        //xlsheet[i, j].Style.Font = new XLFont("Arial", 9, false, false);
                    }
                    else if (style == 1)
                    {
                        xlsheet[i, j].Style = sty_Board.Clone();
                    }
                    else
                    {
                        xlsheet[i, j].Style = sty_NoBoard.Clone();
                    }
                }
            }

            xlsheet[x1, y1].Value = value;

            if (align.Equals("R"))
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Right;
            }
            else if (align.Equals("L"))
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Left;
            }
            else
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Center;
            }
        }
       
        #endregion
    }
}
