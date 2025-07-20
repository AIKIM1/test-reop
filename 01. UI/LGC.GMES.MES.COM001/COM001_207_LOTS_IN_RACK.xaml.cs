/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_207_LOTS_IN_RACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;

        DataTable dtMain;

        public COM001_207_LOTS_IN_RACK()
        {
            InitializeComponent();
        }

        public COM001_207_LOTS_IN_RACK(string sRack, string sareaid, string swhid)
        {
            InitializeComponent();

            //string sBizName = "BR_PRD_GET_LAMI_STANBY_LOTLIST_IN_RACK";           
            try
            {
                StocListSearch("", dgList, sRack, sareaid, swhid); //양극

                //DataTable IndataTable = new DataTable();
                //IndataTable.Columns.Add("RACK_ID", typeof(string));
                //IndataTable.Columns.Add("WH_ID", typeof(string));
                //IndataTable.Columns.Add("AREAID", typeof(string));

                //DataRow Indata = IndataTable.NewRow();
                //Indata["RACK_ID"] = sRack.ToString();
                //Indata["WH_ID"] = swhid.ToString();
                //Indata["AREAID"] = sareaid.ToString();
                //IndataTable.Rows.Add(Indata);

                //dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", IndataTable);

                //if (dtMain.Rows.Count > 0)
                //{
                //    dgList.BeginEdit();
                //    dgList.ItemsSource = DataTableConverter.Convert(dtMain);
                //    dgList.EndEdit();
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void StocListSearch(string elec_type, C1DataGrid dg, string sRack, string sArea, string sWhid)
        {
            try
            {
                C1DataGrid dgGrid = dg;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("WIPHOLD", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("WH_ID", typeof(string));
                IndataTable.Columns.Add("PJT", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("ELEC_TYPE", typeof(string));
                IndataTable.Columns.Add("RACK_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = sArea;
                Indata["WIPHOLD"] = null;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["WH_ID"] = sWhid;
                Indata["PJT"] = null;
                Indata["LOTID"] = null;
                Indata["ELEC_TYPE"] = elec_type == "" ? null : elec_type;
                Indata["RACK_ID"] = sRack;
                IndataTable.Rows.Add(Indata);

                dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LAMI_STANDBY_PANCAKE_LIST", "RQSTDT", "RSLTDT", IndataTable);

                //txtPrj.Text = "";
                //txtPrj.Tag = "";
                //txtLotID.Text = "";

                if (dtMain.Rows.Count > 0)
                {
                    dgList.BeginEdit();
                    dgList.ItemsSource = DataTableConverter.Convert(dtMain);
                    dgList.EndEdit();
                }
                else
                {
                    dgGrid.ItemsSource = null;
                }

                return;
            }
            catch (Exception ex)
            {
                throw ex;
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
        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //사용자 권한별로 버튼 숨기기
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnSave);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //    //여기까지 사용자 권한별로 버튼 숨기기
        //}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Mehod     

        #endregion

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

                    if (e.Cell.Column.Name == "LOTID")
                    {
                        if (dtMain.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        }
                        else if (dtMain.Rows[e.Cell.Row.Index]["FIFO_GUBUN"].ToString() == "N")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }

                        if (dtMain.Rows[e.Cell.Row.Index]["WIPHOLD"].ToString() == "Y")
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}
