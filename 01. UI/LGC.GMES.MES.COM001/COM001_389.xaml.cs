/*************************************************************************************
      Creator :
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2023.09.09  황광선 : Initial Created.
 2023.11.20  김태우 : 마지막입력랏 >> 첫번째 입력랏 대표랏으로 변경
*************************************************************************************/

using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// Interaction logic for COM001_999.xaml
    /// </summary>
    public partial class COM001_389 : UserControl, IWorkArea
    {
        public COM001_389()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Init()
        {
            txtLotId1.Text = string.Empty;
            txtLotId1.Focus();
        }

        private void Init2()
        {
            txtLotId2.Text = string.Empty;
            txtLotId2.Focus();
        }

        private void Init3()
        {
            txtLotId3.Text = string.Empty;
            txtLotId3.Focus();
        }


        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgv1.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgv1.Columns["WIPQTY"], dac);
        }

        private void txtLotId1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!txtLotId1.Text.Equals(""))
                {
                    ScanProcess();
                }

            }
        }

        private void ScanProcess()
        {
            try
            {
                bool checkLot = false;

                DataSet inDataSet = new DataSet();

                //indata
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                //inlot
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newRow2 = inLot.NewRow();
                newRow2["LOTID"] = txtLotId1.Text;
                inLot.Rows.Add(newRow2);

                DataSet SearchResultDataset = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_SLIT_REP_LOT_INFO", "INDATA,INLOT", "OUTDATA", inDataSet);
                DataTable SearchResult = SearchResultDataset.Tables[0];

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "INDATA";
                //RQSTDT.Columns.Add("LANGID", typeof(String));
                //RQSTDT.Columns.Add("LOTID", typeof(String));
                //RQSTDT.Columns.Add("USERID", typeof(String));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["LOTID"] = txtLotId1.Text;
                //dr["USERID"] = LoginInfo.USERID;

                //RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_SM_SAMPLE", "INDATA", "OUTDATA", RQSTDT);//BR_PRD_GET_SLIT_REP_LOT_INFO

                SearchResult.Columns.Add("NO", typeof(String));
                int i = 1;
                foreach (DataRow drow in SearchResult.Rows)
                {
                    drow["NO"] = i++;

                    //if (drow["NO"].ToString() == "4")
                    //{
                    //    dgv1.CurrentRow.Presenter.Background = new SolidColorBrush(Color.FromRgb(133, 255, 224));
                    //}
                }

                SearchResult.AcceptChanges();

                if (SearchResult.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                        }
                    });
                    return;
                }
                else
                {
                    if (dgv1.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgv1, SearchResult, FrameOperation);
                        Init();
                    }
                    else
                    {
                        if (dgv1.GetRowCount() < 4)
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgv1.ItemsSource);

                            string sLot = txtLotId1.Text;

                            for (i = 0; i < dtSource.Rows.Count; i++)
                            {
                                if (dtSource.Rows[i]["LOTID"].ToString() == sLot)
                                {
                                    //condition
                                    checkLot = true;
                                }
                            }

                            if (checkLot == false)
                            {
                                dtSource.Merge(SearchResult);

                                int j = 1;
                                foreach (DataRow drow in dtSource.Rows)
                                {
                                    drow["NO"] = j++;
                                }

                                dtSource.AcceptChanges();

                                Util.gridClear(dgv1);
                                Util.GridSetData(dgv1, dtSource, FrameOperation);
                                Init();
                            }
                            else
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("LOT ID already exists!", "", "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                Init();
                                checkLot = false;
                            }
                        }
                        else
                        {
                            //warning
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Rows can not more than 4!", "", "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Init();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SaveProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                //indata
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                //inlot
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataTable dtSource = DataTableConverter.Convert(dgv1.ItemsSource) as DataTable;

                int a = 0;

                foreach (DataRow dr in dtSource.Rows)
                {
                    a++;
                    DataRow newRow2 = inLot.NewRow();
                    newRow2["LOTID"] = dtSource.Rows[a - 1]["LOTID"];
                    inLot.Rows.Add(newRow2);
                }

                ShowLoadingIndicator();

                DataTable dtOut = new DataTable();
                dtOut.TableName = "OUTDATA";

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SLIT_REP_LOT_CONF", "INDATA,INLOT", "OUTDATA", (result, ex) =>//BRS_MES_TEST1
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");

                    dgv1.ItemsSource = null;
                    Init();

                }, inDataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ScanProcess2(string lotId) //tab2
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;//txtLotId2.Text;

                RQSTDT.Rows.Add(dr);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_REP_LOT_CONF_LIST", "RQSTDT", "RSLTDT", RQSTDT);//DA_TEST_LOT_LIST
                //SearchResult.Columns.Remove("REP_LOT_FLAG");
                //SearchResult.AcceptChanges();

                if (SearchResult.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init2();

                            Util.gridClear(dgv2);
                            Util.GridSetData(dgv2, SearchResult, FrameOperation);
                        }
                    });
                    return;
                }
                else
                {
                    int a = 0;

                    foreach (DataRow dr1 in SearchResult.Rows)
                    {
                        // 2023.08.28 changed
                        if (Convert.ToString(dr1["LOTID"]) == lotId)
                        {
                            //dr1["CHK"] = "Y";
                            dr1["REP_LOT_FLAG"] = "True";
                        }
                        else
                            //dr1["CHK"] = "N";
                            dr1["REP_LOT_FLAG"] = "False";
                    }
                    SearchResult.AcceptChanges();

                    Util.gridClear(dgv2);
                    Util.GridSetData(dgv2, SearchResult, FrameOperation);
                    //Init2();

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ScanProcess3() //tab2
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId3.Text;

                RQSTDT.Rows.Add(dr);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_REP_LOT_CONF_HIST_LIST", "RQSTDT", "RSLTDT", RQSTDT);//DA_TEST_LOT_LIST_HIST


                if (SearchResult.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                        }
                    });
                    return;
                }
                else
                {
                    Util.gridClear(dgv3);
                    Util.gridClear(dgv4);
                    Util.GridSetData(dgv3, SearchResult, FrameOperation);
                    Init3();

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ShowDetail(string LotId) //tab2
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotId;

                RQSTDT.Rows.Add(dr);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_REP_LOT_CONF_HIST", "RQSTDT", "RSLTDT", RQSTDT);//DA_TEST_LOT_LIST_HIST_DETL


                if (SearchResult.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                        }
                    });
                    return;
                }
                else
                {
                    //Util.gridClear(dgv3);
                    Util.gridClear(dgv4);
                    Util.GridSetData(dgv4, SearchResult, FrameOperation);
                    //  Init3();

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            dgv1.ItemsSource = null;
            Init();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!txtLotId1.Text.Equals(""))
            {
                ScanProcess();
                // dgv1.SelectedIndex = 3;
            }
        }


        private void dgv1_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (dataGrid.GetRowCount() > 0)
                    {
                        if (string.Equals(e.Cell.Row.Index, 0))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(133, 255, 224));
                        }
                    }


                }
            }));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgv1.GetRowCount() > 0)
            {
                SaveProcess();
            }
        }

        private void txtLot1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!txtLotId2.Text.Equals(""))
                {
                    ScanProcess2(txtLotId2.Text);
                    // Init2();
                }

            }
        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtSource = DataTableConverter.Convert(dgv2.ItemsSource) as DataTable;
                //DataRow[] drCheck = dtSource.Select("REP_LOT_FLAG = 'Y'");
                //DataTable dtresult = drCheck.CopyToDataTable();

                if (dtSource.Rows.Count > 0) //(dtresult.Rows.Count == 1)
                {
                    //proses
                    try
                    {

                        if (dtSource.Select().Where(p => Convert.ToString(p["REP_LOT_FLAG"]) == "True").Count() > 0)
                        {

                            DataSet inDataSet = new DataSet();

                            //indata
                            DataTable inTable = inDataSet.Tables.Add("INDATA");
                            inTable.Columns.Add("LANGID", typeof(string));
                            inTable.Columns.Add("USERID", typeof(string));
                            inTable.Columns.Add("INPUTLOTID", typeof(string));

                            DataRow newRow = inTable.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["INPUTLOTID"] = this.txtLotId2.Text; //dtresult.Rows[0]["LOTID"];

                            inTable.Rows.Add(newRow);

                            DataTable inLot = inDataSet.Tables.Add("INLOT");
                            inLot.Columns.Add("LOTID", typeof(string));

                            foreach (DataRow dr in dtSource.Rows)
                            {
                                if (Convert.ToString(dr["REP_LOT_FLAG"]) == "True")
                                {
                                    DataRow drlot = inLot.NewRow();
                                    drlot["LOTID"] = Convert.ToString(dr["LOTID"]);

                                    inLot.Rows.Add(drlot);
                                }
                            }


                            DataTable outdata;
                            DataSet outresult;
                            ShowLoadingIndicator();

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SLIT_REP_LOT_CONF_REMOVE", "INDATA,INLOT", "OUTDATA", (result, ex) =>//BR_MES_TEST2
                            {
                                HiddenLoadingIndicator();

                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");

                                //ScanProcess2(txtLotId2.Text);

                                // 2023.08.28 
                                // 재 조회 
                                outdata = result.Tables["OUTDATA"];
                                foreach (DataRow dr1 in outdata.Rows)
                                {
                                    // 2023.08.28 changed
                                    if (Convert.ToString(dr1["LOTID"]) == this.txtLotId2.Text)
                                    {
                                        //dr1["CHK"] = "Y";
                                        dr1["REP_LOT_FLAG"] = "True";
                                    }
                                    else
                                        //dr1["CHK"] = "N";
                                        dr1["REP_LOT_FLAG"] = "False";
                                }
                                outdata.AcceptChanges();

                                Util.gridClear(dgv2);
                                Util.GridSetData(dgv2, outdata, FrameOperation);

                            }, inDataSet);
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                //else
                //{
                //    //warning
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Please select only one LOT!", "", "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSeach2_Click(object sender, RoutedEventArgs e)
        {
            if (!txtLotId2.Text.Equals(""))
            {
                ScanProcess2(txtLotId2.Text);
                // Init2();
                // dgv1.SelectedIndex = 3;
            }
        }

        private void txtLot2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!txtLotId3.Text.Equals(""))
                {
                    ScanProcess3();
                }

            }
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            if (!txtLotId3.Text.Equals(""))
            {
                ScanProcess3();
                // dgv1.SelectedIndex = 3;
            }
        }

        private void dgv3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgv3.ItemsSource == null || dgv3.Rows.Count <= 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                if (dg.CurrentColumn.Name.Equals("LOTID") && dgv3.Rows.Count > 0 && dg.CurrentRow != null)
                {
                    ShowDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //loadingIndicator.Visibility=Visibility.Collapsed;
            }
        }

        private void dgv2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (dataGrid.GetRowCount() > 0)
                    {
                        if (string.Equals(e.Cell.Row.Index, 0))// || string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NO")), "4"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(133, 255, 224));
                        }
                    }


                }
            }));
        }

        private void dgv3_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (dataGrid.GetRowCount() > 0)
                    {
                        if (string.Equals(e.Cell.Row.Index, 0))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(133, 255, 224));
                        }
                    }


                }
            }));
        }

        private void tabSlitting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (tabSlitting.SelectedIndex == 0)
            //{
            //    Util.gridClear(dgv1);
            //    Init();
            //}
            //else if (tabSlitting.SelectedIndex == 1)
            //{
            //    Util.gridClear(dgv2);
            //    Util.gridClear(dgv3);
            //    Util.gridClear(dgv4);
            //    Init2();
            //    Init3();
            //}
        }
    }
}
