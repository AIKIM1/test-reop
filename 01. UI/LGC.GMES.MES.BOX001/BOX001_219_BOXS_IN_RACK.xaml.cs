/*************************************************************************************
 Created Date : 2018.01.22
      Creator : 장만철
   Decription : 전지 5MEGA-GMES 구축 - 전극 출하대기 창고 현황판
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_219_BOXS_IN_RACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;

        DataTable dtMain;
        DataTable dtDetail;

        public BOX001_219_BOXS_IN_RACK()
        {
            InitializeComponent();
        }

        public BOX001_219_BOXS_IN_RACK(string sRack, string swhid)
        {
            InitializeComponent();

            try
            {
                StocListSearch(sRack, swhid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);              
            }
        }        

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        #endregion

        #region Event       

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "BOXID")
                    {
                        if (dtMain.Rows[e.Cell.Row.Index]["VLD_CHECK"].ToString() == "Y" && dtMain.Rows[e.Cell.Row.Index]["BOX_QMS_RESULT"].ToString() == "Y" && dtMain.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["BOXID"].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["BOXID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgList == null || dgList.GetRowCount() == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgList.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string strLotID = DataTableConverter.GetValue(dgList.Rows[currentRow].DataItem, "BOXID").ToString();

                getLotList(strLotID);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "LOTID")
                    {
                        if (dtDetail.Rows[e.Cell.Row.Index]["VLD_CHECK"].ToString() == "Y" && dtDetail.Rows[e.Cell.Row.Index]["LOT_QMS_RESULT"].ToString() == "Y" && dtDetail.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgLotList.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgLotList.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.Cell.Column.Name == "VLD_CHECK_NAME")
                    {
                        if (dtDetail.Rows[e.Cell.Row.Index]["VLD_CHECK"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgLotList.Columns["VLD_CHECK_NAME"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.Cell.Column.Name == "LOT_QMS_RESULT_NAME")
                    {
                        if (dtDetail.Rows[e.Cell.Row.Index]["LOT_QMS_RESULT"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgLotList.Columns["LOT_QMS_RESULT_NAME"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.Cell.Column.Name == "WIPHOLD")
                    {
                        if (dtDetail.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, dgLotList.Columns["WIPHOLD"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod     

        private void StocListSearch(string sRack, string sWhid)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("WH_ID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("CSTID", typeof(string));
                IndataTable.Columns.Add("RACK_ID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["WH_ID"] = sWhid;
                Indata["PRODID"] = null;
                Indata["CSTID"] = null;
                Indata["RACK_ID"] = sRack;
                Indata["LANGID"] = LoginInfo.LANGID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_MONIT_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult.Rows.Count > 0)
                {
                    //dgList.ItemsSource = DataTableConverter.Convert(dtResult);

                    Util.gridClear(dgList);
                    Util.GridSetData(dgList, dtResult, FrameOperation);

                    dtMain = dtResult;
                }
                else
                {
                    dgList.ItemsSource = null;
                }

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgList.Columns["TOTAL_QTY"], dac);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getLotList(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_MONIT_INFO_LOT", "RQSTDT", "RSLTDT", IndataTable);

                Util.gridClear(dgLotList);

                if (dtResult.Rows.Count > 0)
                {
                    dgLotList.ItemsSource = DataTableConverter.Convert(dtResult);
                    dtDetail = dtResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


    }
}
