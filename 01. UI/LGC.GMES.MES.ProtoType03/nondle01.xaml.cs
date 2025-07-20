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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class nondle01 : UserControl
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtMain = new DataTable();
        CommonDataSet _Com = new CommonDataSet();
        DataRow newRow = null;

        public nondle01()
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

        private void dgWorkorder_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            _Util.gridLoadedRowHeaderPresenter(sender, e);
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotList.CurrentRow.DataItem == null)
                return;

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;            
            string sInputLot = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "INPUTLOT").ToString();
            string sChildSeq = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "CHILDGRPSEQ").ToString();

            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                //if (rowindex != i)
                //    (dglotlist.getcell(i, dglotlist.columns["chk"].index).presenter.content as checkbox).ischecked = false;

                if (sInputLot == DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "INPUTLOT").ToString() && 
                    sChildSeq == DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHILDGRPSEQ").ToString())
                {
                    (dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                }
                else
                {
                    (dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                }
            }
            
            Get_DetailData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_DefectData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_EqpDefectData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_QualityData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_Remark(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }
        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    chkEqpEnd.IsChecked = false;
                }
            }

        }

        private void chkRun_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    chkEqpEnd.IsChecked = false;
                }
            }
        }

        private void chkEqpEnd_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    chkWait.IsChecked = false;
                    chkRun.IsChecked = false;
                }
            }
        }
        #endregion

        #region Mehod
        private void InitializeControls()
        {
            DataTable dt = new DataTable();            
            dt.Columns.Add("CODE", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            
            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("CAL140", "남경 폴리머 14호");
            dt.Rows.Add("CAL150", "남경 폴리머 15호");
            dt.Rows.Add("CAL160", "남경 폴리머 16호");
            dt.Rows.Add("CAL170", "남경 폴리머 17호");
            dt.Rows.Add("CAL180", "남경 폴리머 3D-1호");

            cboLine.ItemsSource = dt.Copy().AsDataView();

            dt.Clear();

            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("N1ANTC428", "17라인 NOTCHING 1호기");
            dt.Rows.Add("N1ANTC429", "17라인 NOTCHING 2호기");
            dt.Rows.Add("N1ANTC430", "17라인 NOTCHING 3호기");
            dt.Rows.Add("N1ANTC431", "17라인 NOTCHING 4호기");
            dt.Rows.Add("N1ANTC432", "17라인 NOTCHING 5호기");

            cboEquipment.ItemsSource = dt.Copy().AsDataView();

            dt.Clear();

            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("MRT", "423996L1 R-TYPE");
            dt.Rows.Add("NCC", "355773B1 양극");
            dt.Rows.Add("AW1", "297576L1 R-type");
            dt.Rows.Add("AOS", "575577A1 (단면) L-type");
            dt.Rows.Add("ZDG", "MED R BASE X593");
            dt.Rows.Add("CRC", "426785L1 R-type");
            dt.Rows.Add("2SN", "N2.1 SRS");
            
            cboModel.ItemsSource = dt.Copy().AsDataView();


            cboLine.SelectedIndex = 0;
            cboEquipment.SelectedIndex = 0;
            cboModel.SelectedIndex = 0;


            //Normal Combo Box ( 비동기식 )
            //_Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboModel, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.SELECT, loadingIndicator);



            ////Multi Combo Box ( 비동기식 )
            //_Com.SetCOR_SEL_MLOT_TP_CODE_CBO_G_TEST(cboModel, null, null, null, null, null, null, loadingIndicator);

            ////Normal Combo Box ( 동기식 )
            //_Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboTest03, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.ALL, loadingIndicator, (X, Y) =>
            //{
            //    //Combo 설정 후 작업 내용
            //});

            ////Multi Combo Box ( 비동기식 )
            //_Com.SetCOR_SEL_MLOT_TP_CODE_CBO_G_TEST(cboTest04, null, null, null, null, null, null, loadingIndicator, (X, Y) =>
            //{
            //    //Multi Combo 설정 후 작업 내용
            //});
        }

        private void Get_MainData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("LOTID", typeof(string));
            dtMain.Columns.Add("LARGELOT", typeof(string));
            dtMain.Columns.Add("INPUTLOT", typeof(string));
            dtMain.Columns.Add("CHILDGRPSEQ", typeof(string));
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("STATUS", typeof(string));
            dtMain.Columns.Add("WIPQTY", typeof(Int32));
            dtMain.Columns.Add("EQPQTY", typeof(Int32));
            dtMain.Columns.Add("LENGTH", typeof(Int32));
            dtMain.Columns.Add("FCUTYN", typeof(string));
            dtMain.Columns.Add("STARTTIME", typeof(string));
            dtMain.Columns.Add("EQPENDTIME", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));
            dtMain.Columns.Add("OPERCODE", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));
            dtMain.Columns.Add("VERSION", typeof(string));
            dtMain.Columns.Add("CONV", typeof(string));


            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "4A56D922CA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "4A56D922DA", "4A56D92", "4A56D92221", "2", "CB5 456674L1음극", "장비완료", 0, 269, 0, "Y", "2016-06-22 20:18", "2016-06-22 23:50", "1332346", "10", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching", "", "" };
            dtMain.Rows.Add(newRow);


            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "6BW6F01ICD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "ENP466773APNA", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "6BW6F01IDD", "6BW6F01", "6BW6F01I22", "3", "CAW 466773L1음극", "장비완료", 0, 11569, 0, "Y", "2016-06-30 17:58:09", "2016-06-30 18:04:19", "1333384", "0010", "ENP466773APNA", "BATTERY 466773L1 4030mAh ANODE Notching", "", "" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "5JS6E106CG", "5JS6E10", "5JS6E10623", "1", "CFS 3449109L1양극", "장비완료", 0, 688, 0, "Y", "2016-06-28 9:58", "2016-06-28 10:53", "1331224", "0010", "ENP3449A9APNC", "BATTERY 3449109L1 2930mAh CATHODE Notch", "", "" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "5JS6E106DG", "5JS6E10", "5JS6E10623", "1", "CFS 3449109L1양극", "장비완료", 0, 688, 0, "Y", "2016-06-28 9:58", "2016-06-28 10:53", "1331224", "0010", "ENP3449A9APNC", "BATTERY 3449109L1 2930mAh  CATHODE Notch", "", "" };
            dtMain.Rows.Add(newRow);

            newRow = dtMain.NewRow();
            newRow.ItemArray = new object[] { "J4F6F024CA", "J4F6F02", "J4F6F02421", "5", "AHF 363391L1양극", "장비완료", 0, 23424, 0, "Y", "2016-07-01 14:50", "2016-07-01 14:51", "1333284", "0010", "ENP363391APNC", "BATTERY 363391L1 1640mAh  CATHODE Notchi", "", "" };
            dtMain.Rows.Add(newRow);

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "J4F6F024CA", "J4F6F02", "J4F6F02421", "5", "AHF 363391L1양극", "장비완료", 0, 23424, 0, "Y", "2016-07-01 14:50", "2016-07-01 14:51", "1333284", "0010", "ENP363391APNC", "BATTERY 363391L1 1640mAh  CATHODE Notchi", "", "" };
            //dtMain.Rows.Add(newRow);


            dgLotList.ItemsSource = DataTableConverter.Convert(dtMain);            
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

        private void Get_DetailData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            dtMain = new DataTable();

            dtMain.Columns.Add("LOTID", typeof(string));
            dtMain.Columns.Add("INPUTQTY", typeof(Int32));
            dtMain.Columns.Add("GOODQTY", typeof(Int32));
            dtMain.Columns.Add("LOSSQTY", typeof(Int32));
            dtMain.Columns.Add("MODELID", typeof(string));
            dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));

            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if((dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue && 
                    (bool)(dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                {
                    newRow = dtMain.NewRow();
                    newRow.ItemArray = new object[] { DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "EQPQTY").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "EQPQTY").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LENGTH").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MODELNAME").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODID").ToString(),
                                                      DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODNAME").ToString()
                                                    };
                    dtMain.Rows.Add(newRow);

                    /******************** 상세 정보 Set... ******************/
                    txt11.Text = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WORKORDER").ToString();
                    txt12.Text = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "STATUS").ToString();

                    txt31.Text = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "STARTTIME").ToString();

                    txt41.Text = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "EQPENDTIME").ToString();
                    txt42.Text = Math.Truncate(Convert.ToDateTime(txt41.Text).Subtract(Convert.ToDateTime(txt31.Text)).TotalMinutes).ToString();
                }
            }

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "4A56D922CA", 269, 269, 0, "CB5", "456674L1음극", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching" };
            //dtMain.Rows.Add(newRow);

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "4A56D922DA", 269, 269, 0, "CB5", "456674L1음극", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching" };
            //dtMain.Rows.Add(newRow);

            dgOutPrd.ItemsSource = DataTableConverter.Convert(dtMain);            
        }

        private void Get_DefectData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            // Defect Column 생성..
            if (dgOutPrd.Rows.Count > 0)
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    //DataTableConverter.GetValue(dgFaulty.Columns[i].Header, "INPUTLOT").ToString();
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                        //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
                        dgFaulty.Columns.RemoveAt(i);
                }

                for (int i = 0; i < dgOutPrd.Rows.Count; i++)
                {
                    string sColName = "DEFECTQTY" + (i + 1).ToString();

                    DataGridNumericColumn col = new DataGridNumericColumn();
                    col.Header = DataTableConverter.GetValue(dgOutPrd.Rows[i].DataItem, "LOTID"); // dgOutPrd.Columns["LOTID"].GetCellValue(dgOutPrd.Rows[i]);
                    col.Binding = new Binding(sColName);
                    col.Binding.Mode = BindingMode.TwoWay;
                    col.SortMemberPath = sColName;
                    col.FilterMemberPath = sColName;
                    col.Name = sColName;
                    col.HorizontalAlignment = HorizontalAlignment.Center;
                    
                    dgFaulty.Columns.Add(col);                    
                }
            }

            Random rnd = new Random();

            dtMain = new DataTable();

            dtMain.Columns.Add("DEFECTCODE", typeof(string));
            dtMain.Columns.Add("DEFECTNAME", typeof(string));
            dtMain.Columns.Add("DEFECTQTY1", typeof(Int32));
            dtMain.Columns.Add("DEFECTQTY2", typeof(Int32));

            if (rnd.Next(0, 2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0001", "비젼", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0002", "Pancake 외관", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0003", "기타", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0004", "길이부족", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0005", "조건조정", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0006", "연결", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LOSS0001", "{LOSS}자주검사", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "LOSS0002", "{LOSS}형교환", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0001", "비젼", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "DEF0002", "Pancake 외관", rnd.Next(0, 10000), rnd.Next(0, 10000) };
                dtMain.Rows.Add(newRow);
            }
            
            dgFaulty.ItemsSource = DataTableConverter.Convert(dtMain);

            
        }

        private void Get_EqpDefectData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            dtMain = new DataTable();
            dtMain.Columns.Add("EQPDEFECTCODE", typeof(string));
            dtMain.Columns.Add("EQPDEFECTNAME", typeof(string));
            dtMain.Columns.Add("EQPDEFECTQTY", typeof(Int32));
            dtMain.Columns.Add("EQPDEFECTTIME", typeof(string));

            Random rnd = new Random();

            if (rnd.Next(0, 2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF001", "[OP] PITCH1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF002", "[OP] LENGTH1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF003", "[OP] HEIGHT1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF004", "[OP] PITCH2", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF005", "[MC] LENGTH1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF006", "[MC] HEIGHT1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF007", "[MC] PITCH2", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF005", "[MC] LENGTH1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF006", "[MC] HEIGHT1", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "EQPDEF007", "[MC] PITCH2", rnd.Next(0, 300), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                dtMain.Rows.Add(newRow);
            }

            

            dgEqpdefect.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_QualityData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            // Defect Column 생성..
            if (dgOutPrd.Rows.Count > 0)
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgQuality.Columns.Count; i-- > 0;)
                {
                    //DataTableConverter.GetValue(dgFaulty.Columns[i].Header, "INPUTLOT").ToString();
                    if (dgQuality.Columns[i].Name.ToString().StartsWith("QUALITYQTY"))
                        //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
                        dgQuality.Columns.RemoveAt(i);
                }

                for (int i = 0; i < dgOutPrd.Rows.Count; i++)
                {
                    string sColName = "QUALITYQTY" + (i + 1).ToString();

                    DataGridNumericColumn col = new DataGridNumericColumn();
                    col.Header = DataTableConverter.GetValue(dgOutPrd.Rows[i].DataItem, "LOTID"); // dgOutPrd.Columns["LOTID"].GetCellValue(dgOutPrd.Rows[i]);
                    col.Binding = new Binding(sColName);
                    col.Binding.Mode = BindingMode.TwoWay;
                    col.SortMemberPath = sColName;
                    col.FilterMemberPath = sColName;
                    col.Name = sColName;
                    col.HorizontalAlignment = HorizontalAlignment.Center;

                    dgQuality.Columns.Add(col);
                }
            }

            Random rnd = new Random();

            dtMain = new DataTable();

            dtMain.Columns.Add("CLCTITEM", typeof(string));
            dtMain.Columns.Add("CLCTNAME", typeof(string));
            dtMain.Columns.Add("UML", typeof(string));
            dtMain.Columns.Add("LML", typeof(string));
            dtMain.Columns.Add("UNIT", typeof(string));
            dtMain.Columns.Add("QUALITYQTY1", typeof(Int32));
            dtMain.Columns.Add("QUALITYQTY2", typeof(Int32));

            if(rnd.Next(0,2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "CLCTITM001", "인장강도", rnd.Next(0, 100), rnd.Next(0, 100), "Kg/f", rnd.Next(0, 100), rnd.Next(0, 100) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "CLCTITM002", "Tape 하부 리드 (음극)", rnd.Next(0, 100), rnd.Next(0, 100), "O/X", rnd.Next(0, 100), rnd.Next(0, 100) };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "CLCTITM003", "Tab 단락(양극)", rnd.Next(0, 100), rnd.Next(0, 100), "유/무", rnd.Next(0, 100), rnd.Next(0, 100) };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { "CLCTITM003", "Tab 단락(양극)", rnd.Next(0, 100), rnd.Next(0, 100), "유/무", rnd.Next(0, 100), rnd.Next(0, 100) };
                dtMain.Rows.Add(newRow);
            }

            dgQuality.ItemsSource = DataTableConverter.Convert(dtMain);

        }

        private void Get_Remark(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            cboLine.ItemsSource = dt.Copy().AsDataView();

            for (int i = 0; i < dgOutPrd.Rows.Count; i++)
            {
                dt.Rows.Add(DataTableConverter.GetValue(dgOutPrd.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgOutPrd.Rows[i].DataItem, "LOTID"));                
            }

            cboRemarkLot.ItemsSource = dt.Copy().AsDataView();
            cboRemarkLot.SelectedIndex = 0;

            //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);


            //Random string...
            Random rnd = new Random();
            
            string sRnd = new string(Enumerable.Range(0, rnd.Next(0,500)).Select(_ => (char)rnd.Next('a', 'z')).ToArray());

            rtxtRemark.AppendText(sRnd);

        }
        #endregion

        private void dgLotList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string str = "1111";
        }
    }
}
