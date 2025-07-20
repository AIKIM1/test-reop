/*************************************************************************************
 Created Date : 2016.08.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 노칭 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.03  INS 김동일K : Initial Created.
  
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
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// PGM_GUI_023.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_023 : UserControl
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        DataTable dtMain = new DataTable();
        CommonDataSet _Com = new CommonDataSet();
        DataRow newRow = null;
        #endregion

        #region Initialize
        public PGM_GUI_023()
        {
            InitializeComponent();
        }

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

            cboEqpt.ItemsSource = dt.Copy().AsDataView();

            dt.Clear();

            dt.Rows.Add("", "-SELECT-");
            dt.Rows.Add("MRT", "423996L1 R-TYPE");
            dt.Rows.Add("NCC", "355773B1 양극");
            dt.Rows.Add("AW1", "297576L1 R-type");
            dt.Rows.Add("AOS", "575577A1 (단면) L-type");
            dt.Rows.Add("ZDG", "MED R BASE X593");
            dt.Rows.Add("CRC", "426785L1 R-type");
            dt.Rows.Add("2SN", "N2.1 SRS");

            cboElecType.ItemsSource = dt.Copy().AsDataView();


            cboLine.SelectedIndex = 0;
            cboEqpt.SelectedIndex = 0;
            cboElecType.SelectedIndex = 0;


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

        private void InitializeWorkorderQuantityInfo()
        {
            txtBlockPlanQty.Text = "0";
            txtBlockOutQty.Text = "0";
            txtBlockRemainQty.Text = "0";
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
        }

        #region 조회조건
        private void cboLine_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {

        }

        private void cboEqpt_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {

        }

        private void cboElecType_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {

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

        #region 작업지시
        private void dgWorkOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("선택 작업지시 변경 구현 필요...", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region 작업대상
        private void chkProdList_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
                return;

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;            
            string sInputLot = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "INPUTLOT").ToString();
            string sChildSeq = DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "CHILDGRPSEQ").ToString();

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                //if (rowindex != i)
                //    (dglotlist.getcell(i, dglotlist.columns["chk"].index).presenter.content as checkbox).ischecked = false;

                if (sInputLot == DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "INPUTLOT").ToString() &&
                    sChildSeq == DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILDGRPSEQ").ToString())
                {
                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                }
                else
                {
                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                }
            }

            Get_DetailData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_FaultyData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_EqpDefectData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            //Get_QualityData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            Get_Remark(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }
        #endregion

        #region 실적확인
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("작업조 선택 팝업.", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("작업자 선택 팝업.", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region 불량정보
        private void btnSaveFaulty_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 특이사항
        private void cboRemarkLot_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {

        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 메인 버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Get_WorkOrderData();
            Get_MainData();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void btnRemarkHist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFCut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnWorkDiary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEqptIssue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnChangeMold_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEmptyMold_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnChangePancake_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #endregion

        #region Mehod

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


            dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_WorkOrderData()
        {
            dtMain = new DataTable();
            dtMain.Columns.Add("RANKING", typeof(Int32));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PLANQTY", typeof(Int32));
            dtMain.Columns.Add("OUTQTY", typeof(Int32));
            dtMain.Columns.Add("WDTYPE", typeof(string));
            dtMain.Columns.Add("WDSTATUS", typeof(string));
            dtMain.Columns.Add("MOVEORDER", typeof(string));
            dtMain.Columns.Add("WORKORDER", typeof(string));

            Random rnd = new Random();

            if (rnd.Next(0, 2) > 0)
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 1, "MBEV3601AM", 1000, 200, "양산", "작업중", "M0010", "2886316" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 2, "MBEV3601AP", 2000, 0, "양산", "대기", "MO0020", "2886317" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 3, "MBEV3801AB", 1000000, 0, "테스트", "대기", "M9000", "2886186" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 4, "MBSLURRYAA2", 10000, 0, "시생산", "대기", "M2011", "V4-1_1000A" };
                dtMain.Rows.Add(newRow);
            }
            else
            {
                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 1, "MBSLURRYAA2", 250000, 12242, "양산", "작업중", "V4-2_1250A", "0010" };
                dtMain.Rows.Add(newRow);

                newRow = dtMain.NewRow();
                newRow.ItemArray = new object[] { 3, "MBEV3801AB", 1000000, 0, "테스트", "대기", "M9000", "2886186" };
                dtMain.Rows.Add(newRow);
            }
            
            dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtMain);

            Set_WorkOrderQuantityInfo();
        }

        private void Set_WorkOrderQuantityInfo()
        {
            InitializeWorkorderQuantityInfo();

            if (dgWorkOrder.Rows.Count < 1)
                return;

            for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "WDSTATUS").ToString().Equals("작업중"))    // 하드코딩 변경 필요..
                {
                    if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").GetType() == typeof(Int32) &&
                        DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").GetType() == typeof(Int32))
                    {
                        txtBlockPlanQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString()));
                        txtBlockOutQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
                        txtBlockRemainQty.Text = string.Format("{0:n0}", Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PLANQTY").ToString()) 
                            - Int32.Parse(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "OUTQTY").ToString()));
                    }
                    break;
                }
            }
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
            //dtMain.Columns.Add("MODELID", typeof(string));
            //dtMain.Columns.Add("MODELNAME", typeof(string));
            dtMain.Columns.Add("PRODID", typeof(string));
            dtMain.Columns.Add("PRODNAME", typeof(string));

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if ((dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                    (bool)(dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                {
                    newRow = dtMain.NewRow();
                    newRow.ItemArray = new object[] { DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID").ToString(),
                                                      DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "EQPQTY").ToString(),
                                                      DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "EQPQTY").ToString(),
                                                      DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LENGTH").ToString(),
                                                      //DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "MODELNAME").ToString(),
                                                      DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PRODID").ToString(),
                                                      DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PRODNAME").ToString()
                                                    };
                    dtMain.Rows.Add(newRow);

                    /******************** 상세 정보 Set... ******************/
                    txtWorkorder.Text = DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WORKORDER").ToString();
                    txtWorkseq.Text = DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "OPERCODE").ToString(); 
                    txtLotStatus.Text = DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "STATUS").ToString();

                    txtStartTime.Text = DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "STARTTIME").ToString();

                    txtEndTime.Text = DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "EQPENDTIME").ToString();
                    txtWorkMinute.Text = Math.Truncate(Convert.ToDateTime(txtEndTime.Text).Subtract(Convert.ToDateTime(txtStartTime.Text)).TotalMinutes).ToString();
                }
            }

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "4A56D922CA", 269, 269, 0, "CB5", "456674L1음극", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching" };
            //dtMain.Rows.Add(newRow);

            //newRow = dtMain.NewRow();
            //newRow.ItemArray = new object[] { "4A56D922DA", 269, 269, 0, "CB5", "456674L1음극", "ENP456674APNA", "BATTERY 456674L1 3390mAh  ANODE Notching" };
            //dtMain.Rows.Add(newRow);

            dgDetail.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void Get_FaultyData(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            // Defect Column 생성..
            if (dgDetail.Rows.Count > 0)
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    //DataTableConverter.GetValue(dgFaulty.Columns[i].Header, "INPUTLOT").ToString();
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                        //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
                        dgFaulty.Columns.RemoveAt(i);
                }

                for (int i = 0; i < dgDetail.Rows.Count; i++)
                {
                    string sColName = "DEFECTQTY" + (i + 1).ToString();

                    DataGridNumericColumn col = new DataGridNumericColumn();
                    col.Header = DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"); // dgOutPrd.Columns["LOTID"].GetCellValue(dgOutPrd.Rows[i]);
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



            dgEqpFaulty.ItemsSource = DataTableConverter.Convert(dtMain);
        }

        //private void Get_QualityData(object SelectedItem)
        //{
        //    DataRowView rowview = SelectedItem as DataRowView;

        //    if (rowview == null)
        //    {
        //        return;
        //    }

        //    // Defect Column 생성..
        //    if (dgDetail.Rows.Count > 0)
        //    {
        //        // 기존 추가된 Col 삭제..                
        //        for (int i = dgQuality.Columns.Count; i-- > 0;)
        //        {
        //            //DataTableConverter.GetValue(dgFaulty.Columns[i].Header, "INPUTLOT").ToString();
        //            if (dgQuality.Columns[i].Name.ToString().StartsWith("QUALITYQTY"))
        //                //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
        //                dgQuality.Columns.RemoveAt(i);
        //        }

        //        for (int i = 0; i < dgDetail.Rows.Count; i++)
        //        {
        //            string sColName = "QUALITYQTY" + (i + 1).ToString();

        //            DataGridNumericColumn col = new DataGridNumericColumn();
        //            col.Header = DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"); // dgOutPrd.Columns["LOTID"].GetCellValue(dgOutPrd.Rows[i]);
        //            col.Binding = new Binding(sColName);
        //            col.Binding.Mode = BindingMode.TwoWay;
        //            col.SortMemberPath = sColName;
        //            col.FilterMemberPath = sColName;
        //            col.Name = sColName;
        //            col.HorizontalAlignment = HorizontalAlignment.Center;

        //            dgQuality.Columns.Add(col);
        //        }
        //    }

        //    Random rnd = new Random();

        //    dtMain = new DataTable();

        //    dtMain.Columns.Add("CLCTITEM", typeof(string));
        //    dtMain.Columns.Add("CLCTNAME", typeof(string));
        //    dtMain.Columns.Add("UML", typeof(string));
        //    dtMain.Columns.Add("LML", typeof(string));
        //    dtMain.Columns.Add("UNIT", typeof(string));
        //    dtMain.Columns.Add("QUALITYQTY1", typeof(Int32));
        //    dtMain.Columns.Add("QUALITYQTY2", typeof(Int32));

        //    if (rnd.Next(0, 2) > 0)
        //    {
        //        newRow = dtMain.NewRow();
        //        newRow.ItemArray = new object[] { "CLCTITM001", "인장강도", rnd.Next(0, 100), rnd.Next(0, 100), "Kg/f", rnd.Next(0, 100), rnd.Next(0, 100) };
        //        dtMain.Rows.Add(newRow);

        //        newRow = dtMain.NewRow();
        //        newRow.ItemArray = new object[] { "CLCTITM002", "Tape 하부 리드 (음극)", rnd.Next(0, 100), rnd.Next(0, 100), "O/X", rnd.Next(0, 100), rnd.Next(0, 100) };
        //        dtMain.Rows.Add(newRow);

        //        newRow = dtMain.NewRow();
        //        newRow.ItemArray = new object[] { "CLCTITM003", "Tab 단락(양극)", rnd.Next(0, 100), rnd.Next(0, 100), "유/무", rnd.Next(0, 100), rnd.Next(0, 100) };
        //        dtMain.Rows.Add(newRow);
        //    }
        //    else
        //    {
        //        newRow = dtMain.NewRow();
        //        newRow.ItemArray = new object[] { "CLCTITM003", "Tab 단락(양극)", rnd.Next(0, 100), rnd.Next(0, 100), "유/무", rnd.Next(0, 100), rnd.Next(0, 100) };
        //        dtMain.Rows.Add(newRow);
        //    }

        //    dgQuality.ItemsSource = DataTableConverter.Convert(dtMain);

        //}

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

            for (int i = 0; i < dgDetail.Rows.Count; i++)
            {
                dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
            }

            cboRemarkLot.ItemsSource = dt.Copy().AsDataView();
            cboRemarkLot.SelectedIndex = 0;

            //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);


            //Random string...
            Random rnd = new Random();

            string sRnd = new string(Enumerable.Range(0, rnd.Next(0, 500)).Select(_ => (char)rnd.Next('a', 'z')).ToArray());

            rtxRemark.AppendText(sRnd);

        }

        #endregion

        
    }
}
