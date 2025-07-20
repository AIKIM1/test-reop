/*************************************************************************************
 Created Date : 2022.10.20
      Creator : Choi WanYoung
   Decription : 재공정보현황(차기공정지연)
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.20  DEVELOPER : Initial Created.
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_150 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        //DispatcherTimer _timer = new DispatcherTimer();
        //private int sec = 0;

        public FCS002_150()
        {
            InitializeComponent();

            //_timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            //_timer.Tick += new EventHandler(timer_Tick);
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //_timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            //_timer.Tick += new EventHandler(timer_Tick);

            this.Loaded -= UserControl_Loaded;
            GetList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;


                if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dgWipbyOper.CurrentColumn.Name.ToString())).Equals("0")) return;
                string sOPER = string.Empty;//Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID"));
                string sOPER_NAME = string.Empty;//Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCNAME"));
                

                string sMAX_PROCID_PROC_GR_CODE = string.Empty;//Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID_PROC_GR_CODE"));
                //if (fpsWipbyOper.ActiveSheet.Rows[e.Row].Tag.ToString().Equals("sum"))
                //{
                //    sOPER = string.Empty;
                //    sOPER_NAME = "-ALL-";
                //}
                string sLINE_ID = string.Empty;//Util.GetCondition(cboLine);
                string sLINE_NAME = string.Empty;//cboLine.Text;

                string sROUTE_ID = string.Empty;//Util.GetCondition(cboRoute);
                string sROUTE_NAME = string.Empty;//cboRoute.Text;
                string sMODEL_ID = string.Empty;//Util.GetCondition(cboModel);
                string sMODEL_NAME = string.Empty;//cboModel.Text;
                string sStatus = string.Empty;// null;
                string sStatusName = string.Empty;//null;
                string sLotID = string.Empty;//Util.GetCondition(txtLotId);
                string sSpecial = string.Empty;//Util.GetCondition(cboSpecial);
                string sSpecialName = string.Empty;// cboSpecial.Text;
                string sRouteTypeDG = string.Empty;//Util.GetCondition(cboRouteDG);
                string sRouteTypeDGName = string.Empty;//cboRouteDG.Text;
                string sLotType = string.Empty;// Util.GetCondition(cboLotType);
                string sLotTypeName = string.Empty;//cboLotType.Text;

                if (dgWipbyOper.CurrentColumn.Name.Equals("PRE2HPCD_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("PRE2HPCD_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2HIGH_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2HIGH_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("HIGH2NORMAL_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("HIGH2NORMAL_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2DEGAS_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2DEGAS_CELL"))
                {
                    sRouteTypeDG = "D";
                    //sRouteTypeDGName = "BD";
                }
                else
                {
                    sRouteTypeDG = "E";
                    //sRouteTypeDGName = "AD";
                }
                
                sLINE_ID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LINE_ID"));
                sLINE_NAME = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LINE_NAME"));
                sStatus = "A";
                Load_FCS002_005_02(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName,sLotType, sLotTypeName);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Load_FCS002_005_02(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLotID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName)
        {
            //Tray List
            FCS002_005_02 TrayList = new FCS002_005_02();
            TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[19];
            Parameters[0] = sOPER; //sOPER
            Parameters[1] = sOPER_NAME; //sOPER_NAME
            Parameters[2] = sLINE_ID; //sLINE_ID
            Parameters[3] = sLINE_NAME; //sLINE_NAME
            Parameters[4] = sROUTE_ID; //sROUTE_ID
            Parameters[5] = sROUTE_NAME; //sROUTE_NAME
            Parameters[6] = sMODEL_ID; //sMODEL_ID
            Parameters[7] = sMODEL_NAME; //sMODEL_NAME
            Parameters[8] = sStatus; //sStatus
            Parameters[9] = sStatusName; //sStatusName
            Parameters[10] = sROUTE_TYPE_DG; //sROUTE_TYPE_DG
            Parameters[11] = sROUTE_TYPE_DG_NAME; //sROUTE_TYPE_DG_NAME
            Parameters[12] = sLotID; //sLotID
            Parameters[13] = sSPECIAL_YN; //sSPECIAL_YN
            Parameters[14] = ""; 
            Parameters[15] = ""; 
            Parameters[16] = "";
            Parameters[17] = sLotType;//sLotType
            Parameters[18] = sLotTypeName; //sLotTypeName

            this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
        }



        private void dgWipbyOper_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PRE2HPCD_TRAY") || e.Column.Name.Equals("PRE2HPCD_CELL") || e.Column.Name.Equals("NORMAL2HIGH_TRAY") || e.Column.Name.Equals("NORMAL2HIGH_CELL") ||
                        e.Column.Name.Equals("HIGH2NORMAL_TRAY") || e.Column.Name.Equals("HIGH2NORMAL_CELL") || e.Column.Name.Equals("NORMAL2DEGAS_TRAY") || e.Column.Name.Equals("NORMAL2DEGAS_CELL") ||
                        e.Column.Name.Equals("WAITOCV_TRAY") || e.Column.Name.Equals("WAITOCV_CELL") || e.Column.Name.Equals("SHIP2OCV_TRAY") || e.Column.Name.Equals("SHIP2OCV_CELL") ||
                        e.Column.Name.Equals("SHIP2SEL2_TRAY") || e.Column.Name.Equals("SHIP2SEL2_CELL") || e.Column.Name.Equals("SHIP2EOL_TRAY") || e.Column.Name.Equals("SHIP2EOL_CELL"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));

        }

        private void dgWipbyOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("PRE2HPCD_TRAY") || e.Cell.Column.Name.Equals("PRE2HPCD_CELL") || e.Cell.Column.Name.Equals("NORMAL2HIGH_TRAY") || e.Cell.Column.Name.Equals("NORMAL2HIGH_CELL") ||
                        e.Cell.Column.Name.Equals("HIGH2NORMAL_TRAY") || e.Cell.Column.Name.Equals("HIGH2NORMAL_CELL") || e.Cell.Column.Name.Equals("NORMAL2DEGAS_TRAY") || e.Cell.Column.Name.Equals("NORMAL2DEGAS_CELL") ||
                        e.Cell.Column.Name.Equals("WAITOCV_TRAY") || e.Cell.Column.Name.Equals("WAITOCV_CELL")|| e.Cell.Column.Name.Equals("SHIP2OCV_TRAY") || e.Cell.Column.Name.Equals("SHIP2OCV_CELL") ||
                        e.Cell.Column.Name.Equals("SHIP2SEL2_TRAY") || e.Cell.Column.Name.Equals("SHIP2SEL2_CELL") || e.Cell.Column.Name.Equals("SHIP2EOL_TRAY") || e.Cell.Column.Name.Equals("SHIP2EOL_CELL"))
                    {
                        if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LINE_NAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                }
            }));
            
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //sec++;
            //if (sec >= 30)
            //{
            //    tbTime.Visibility = Visibility.Visible;
            //    _timer.Stop();
            //}
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (_timer != null)
            //{
            //    _timer.Stop();
            //    _timer = null;
            //}
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                
                //if (_timer == null)
                //{
                //    _timer = new DispatcherTimer();
                //    _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                //    _timer.Tick += new EventHandler(timer_Tick);
                //}

                //tbTime.Visibility = Visibility.Collapsed;
                //sec = 0;
                //_timer.Start();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WIP_RETRIEVE_INFO_WORK_DELAYED", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Merge(gridSumRowAddALL(dtRslt));
                }
                Util.GridSetData(dgWipbyOper, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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

        private DataTable gridSumRowAddALL(DataTable dtRslt)
        {
            DataTable NewTable = new DataTable();

            NewTable = dtRslt.Copy();
            NewTable.Clear();

            int intHpcdTray = 0;
            int intHpcdCell = 0;
            int intHighTray = 0;
            int intHighCell = 0;
            int intNormalTray = 0;
            int intNormalCell = 0;
            int intDegasTray = 0;
            int intDegasCell = 0;
            int intOcvTray = 0;
            int intOcvCell = 0;
            int intShip2OcvTray = 0;
            int intShip2OcvCell = 0;
            int intShip2Sel2Tray = 0;
            int intShip2Sel2Cell = 0;
            int intShip2EolTray = 0;
            int intShip2EolCell = 0;

            for (int iRow = 0; iRow < dtRslt.Rows.Count; iRow++)
            {
                intHpcdTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["PRE2HPCD_TRAY"])));
                intHpcdCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["PRE2HPCD_CELL"])));
                intHighTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["NORMAL2HIGH_TRAY"])));
                intHighCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["NORMAL2HIGH_CELL"])));
                intNormalTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["HIGH2NORMAL_TRAY"])));
                intNormalCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["HIGH2NORMAL_CELL"])));
                intDegasTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["NORMAL2DEGAS_TRAY"])));
                intDegasCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["NORMAL2DEGAS_CELL"])));
                intOcvTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WAITOCV_TRAY"])));
                intOcvCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WAITOCV_CELL"])));
                intShip2OcvTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2OCV_TRAY"])));
                intShip2OcvCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2OCV_CELL"])));
                intShip2Sel2Tray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2SEL2_TRAY"])));
                intShip2Sel2Cell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2SEL2_CELL"])));
                intShip2EolTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2EOL_TRAY"])));
                intShip2EolCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["SHIP2EOL_CELL"])));
            }

            DataRow newRow = NewTable.NewRow();
            newRow["LINE_NAME"] = ObjectDic.Instance.GetObjectName("ALL_SUM");
            newRow["PRE2HPCD_TRAY"] = intHpcdTray;
            newRow["PRE2HPCD_CELL"] = intHpcdCell;
            newRow["NORMAL2HIGH_TRAY"] = intHighTray;
            newRow["NORMAL2HIGH_CELL"] = intHighCell;
            newRow["HIGH2NORMAL_TRAY"] = intNormalTray;
            newRow["HIGH2NORMAL_CELL"] = intNormalCell;
            newRow["NORMAL2DEGAS_TRAY"] = intDegasTray;
            newRow["NORMAL2DEGAS_CELL"] = intDegasCell;
            newRow["WAITOCV_TRAY"] = intOcvTray;
            newRow["WAITOCV_CELL"] = intOcvCell;
            newRow["SHIP2OCV_TRAY"] = intShip2OcvTray;
            newRow["SHIP2OCV_CELL"] = intShip2OcvCell;
            newRow["SHIP2SEL2_TRAY"] = intShip2Sel2Tray;
            newRow["SHIP2SEL2_CELL"] = intShip2Sel2Cell;
            newRow["SHIP2EOL_TRAY"] = intShip2EolTray;
            newRow["SHIP2EOL_CELL"] = intShip2EolCell;
            NewTable.Rows.Add(newRow);

            dtRslt = NewTable.Copy();

            return dtRslt;
        }


        #endregion
       

    }
}
