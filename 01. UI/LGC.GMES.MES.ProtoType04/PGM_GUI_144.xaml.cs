/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_144 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Boolean bLoadComplete = false;

        public PGM_GUI_144()
        {
            InitializeComponent();
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);          
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            string sShopID = LoginInfo.CFG_AREA_ID;

            rdoALL.IsChecked = true;
            rdoRoll.IsChecked = true;

            if (sShopID == "01E")
            {
                rdoRoll.Content = ObjectDic.Instance.GetObjectName("점보롤1동");
                rdoplus.Content = ObjectDic.Instance.GetObjectName("점보롤3동");
                rdominus.Content = ObjectDic.Instance.GetObjectName("팬케이크양극");
                rdoSRS2.Content = ObjectDic.Instance.GetObjectName("팬케이크음극");
                rdoSRS.Visibility = Visibility.Hidden;
                rdoOUT.Visibility = Visibility.Hidden;
            }
            else
            {
                rdoRoll.Content = ObjectDic.Instance.GetObjectName("점보롤");
                rdoplus.Content = ObjectDic.Instance.GetObjectName("팬케이크양극");
                rdominus.Content = ObjectDic.Instance.GetObjectName("팬케이크음극");
                rdoSRS2.Content = ObjectDic.Instance.GetObjectName("SRS");
                rdoOUT.Content = ObjectDic.Instance.GetObjectName("팬케이크외부");
            }

            DataGridColAdd(dgList);
            DataGridRowAdd(dgList);

            dgList.LoadedCellPresenter += new EventHandler<DataGridCellEventArgs>(grid_LoadedCellPresenter);

        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        void grid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (e.Cell.Column.Name == "ExpectedDelivery")
            //{
            //    ProductDeliveryInfo p = (ProductDeliveryInfo)e.Cell.Row.DataItem;
            //    DateTime realDelivery = p.ReadyForDelivery.AddDays(p.DeliveryDays);
            //    int daysDifference = p.ExpectedDelivery.Subtract(realDelivery).Days;
            //    if (daysDifference < -2)
            //    {
            //        e.Cell.Presenter.Background = (Brush)Resources["ProblemBrush"];
            //    }
            //    else if (daysDifference < 0)
            //    {
            //        e.Cell.Presenter.Background = (Brush)Resources["DelayBrush"];
            //    }
            //    else if (daysDifference < 1)
            //    {
            //        e.Cell.Presenter.Background = (Brush)Resources["WarningBrush"];
            //    }
            //}
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rdo = sender as RadioButton;

            if (rdo.IsChecked.Value)
            {
                //if (rdoALL.IsChecked.Value && rdoJumbo.IsChecked.Value && rdoPancake.IsChecked.Value && rdoSRS.IsChecked.Value)
                //{

                //}

                //Search_Store
            }
        }

        private void Search_Store()
        {
            if (rdoALL.IsChecked.Value)
            {
                //Grid_Display
                //Grid_Display("QR_WH_EL_ALL_WH", spdStore);
            }
            else if (rdoJumbo.IsChecked.Value)
            {

            }
            else if (rdoPancake.IsChecked.Value)
            {

            }
            else if (rdoSRS.IsChecked.Value)
            {

            }
        }

        private void Grid_Dispaly(string Bizname, DataGrid Gridname)
        {

            //new ClientProxy().ExecuteService(Bizname, "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    if (ex != null)
            //    {
            //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    Gridname.ItemsSource = DataTableConverter.Convert(result);

            //});
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rdo = sender as RadioButton;

            if (rdo.IsChecked.Value)
            {
                if (rdoRoll.IsChecked.Value || rdoplus.IsChecked.Value || rdominus.IsChecked.Value || rdoSRS2.IsChecked.Value)
                {
                    Search_Status();
                }

                
            }
        }

        private void Search_Status()
        {
            if (bLoadComplete == true)
            {

            }

            if (rdoRoll.IsChecked.Value)
            {
                Grid_Draw("DA_PRD_SEL_STOCK_BY_JUMBOROLL");
            }
            else if (rdoplus.IsChecked.Value)
            {

            }
            else if (rdominus.IsChecked.Value)
            {

            }
            else if (rdoSRS2.IsChecked.Value)
            {

            }
        }

        private void Grid_Draw(string Bizname)
        {
            try
            {
                bLoadComplete = true;

                int irowidx = 0;
                int icolidx = 0;

                double dMaxCnt = 0;
                double dCurrentCnt = 0;
                double dCurrentCount_Pancake = 0;
                double dYield = 0;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService(Bizname, "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgList.ItemsSource = DataTableConverter.Convert(result);

                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            irowidx = Convert.ToInt32(result.Rows[i]["Y_POSITION"].ToString()) + 1;
                            icolidx = Convert.ToInt32(result.Rows[i]["X_POSITION"].ToString());

                            DataTableConverter.SetValue(dgList.Rows[irowidx].DataItem, "Col" + icolidx.ToString("00"), result.Rows[i]["RACK_ID"].ToString());

                            dMaxCnt = Convert.ToDouble(result.Rows[i]["MAX_QTY"].ToString());
                            dCurrentCnt = Convert.ToDouble(result.Rows[i]["RACK_CNT"].ToString());

                            //2016.07.13 김기덕K 요청으로 좌측에 보여주는 집계는 기존그대로 컷 기준으로 보이고, 우측에 보이는 부분은 팬케익 단위로 보이도록 수정함                        
                            if (Bizname == "DA_PRD_SEL_STOCK_BY_JUMBOROLL")
                            {
                                dCurrentCount_Pancake = dCurrentCnt;
                            }
                            else
                            {
                                dCurrentCount_Pancake = Convert.ToDouble(result.Rows[i]["STKCOUNT_PANCAKE"].ToString());
                            }

                            icolidx = icolidx + 1;

                            DataTableConverter.SetValue(dgList.Rows[irowidx].DataItem, "Col" + icolidx.ToString("00"), dCurrentCount_Pancake);

                            dYield = Math.Round((dCurrentCnt / dMaxCnt) * 100, 2);

                            if (dYield >= 100)
                            {

                            }
                            else if (dYield < 100 && dYield > 66)
                            {

                            }
                            else if (dYield < 66 && dYield > 33)
                            {

                            }
                            else if (dYield < 33 && dYield > 0)
                            {

                            }
                            else if (dYield <= 0)
                            {

                            }

                        } // for end

                        // 외부창고 조회 로직
                        if (Bizname == "")
                        {
                            DataTableConverter.SetValue(dgList.Rows[1].DataItem, "Col" + icolidx.ToString("00"), "복도");
                            DataTableConverter.SetValue(dgList.Rows[11].DataItem, "Col" + icolidx.ToString("00"), "노칭실");
                            DataTableConverter.SetValue(dgList.Rows[24].DataItem, "Col" + icolidx.ToString("00"), "슬리터실");
                            DataTableConverter.SetValue(dgList.Rows[34].DataItem, "Col" + icolidx.ToString("00"), "2층");

                            //spdList.ActiveSheet.Cells[1, 1].ForeColor = Color.Blue;
                            //spdList.ActiveSheet.Cells[11, 1].ForeColor = Color.Blue;
                            //spdList.ActiveSheet.Cells[24, 1].ForeColor = Color.Blue;
                            //spdList.ActiveSheet.Cells[34, 1].ForeColor = Color.Blue;

                        }

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void DataGridColAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.Columns.Clear();

            for (int i = 0; i < 30; i++)
            {
                LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dg, "Col"+i.ToString("00"), null, "Col"+i.ToString("00"), false, false, false, false, 20, HorizontalAlignment.Center, Visibility.Visible);
            }

            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            
            dg.BeginEdit();
            dg.ItemsSource = DataTableConverter.Convert(dt);
            dg.EndEdit();
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            DataTable dt = new DataTable();
            DataRow newRow = null;

            dt = new DataTable();
            //dt.Columns.Add("NUMBER", typeof(string));
            for (int row = 0; row < 50; row++)
            {
                dt.Columns.Add("Col" + row.ToString("00"), typeof(string));
            }

            List<object[]> list_Row = new List<object[]>();

            for (int i = 0; i < 50; i++)
            {
                list_Row.Add(new object[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                                            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                                            "", "", "", "", "", "", "", "", "", ""
                });
            }

            foreach (object[] item in list_Row)
            {
                newRow = dt.NewRow();
                newRow.ItemArray = item;
                dt.Rows.Add(newRow);
            }

            dgList.ItemsSource = DataTableConverter.Convert(dt);
        
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sLottype = string.Empty;
            string sElectrode = string.Empty;

            int rowidx = 0;
            int colidx = 0;

            DataSet ds = new DataSet();

            if (rdoRoll.IsChecked.Value)
            {
                sLottype = "R";
            }
            else if (rdoplus.IsChecked.Value)
            {
                sLottype = "P";
                sElectrode = "C";
            }
            else if (rdominus.IsChecked.Value)
            {
                sLottype = "P";
                sElectrode = "A";
            }
            else if (rdoSRS2.IsChecked.Value)
            {
                sLottype = "P";
                sElectrode = "S";
            }

            if (txtModel.Text != null)
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("ELECTRODE", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTTYPE"] = sLottype;
                dr["ELECTRODE"] = sElectrode;
                dr["MODELID"] = txtModel.Text;
                RQSTDT.Rows.Add(dr);

                //QR_WH_EL_SEL_INFO_MODEL
                new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_PLAN", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgMovePlan.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count !=0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            //searchResult.Rows[0]["MLOTID"].ToString();
                            rowidx = Convert.ToInt32(result.Rows[0]["YPOSITION"].ToString());
                            colidx = Convert.ToInt32(result.Rows[0]["XPOSITION"].ToString());
                        }
                    }

                });
            }
            else
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("ELECTRODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTTYPE"] = sLottype;
                dr["ELECTRODE"] = sElectrode;
                dr["LOTID"] = txtLotid.Text;
                RQSTDT.Rows.Add(dr);

                //QR_WH_EL_SEL_INFO_LOTID
                //new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_PLAN", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //    if (ex != null)
                //    {
                //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        return;
                //    }
                //    dgMovePlan.ItemsSource = DataTableConverter.Convert(result);

                //});
            }

        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //string strtmp = string.Format(@"CurrentCell.Row.Index : {0} CurrentCell.Column.Index : {1}", dgList.CurrentCell.Row.Index, dgList.CurrentCell.Column.Index);
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(strtmp);

            //string sRack_ID = dgList.SelectedItem.ToString();

            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("RACK_ID", typeof(string));
            //RQSTDT.Columns.Add("MODEL_ID", typeof(string));
            //RQSTDT.Columns.Add("LOT_ID", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["RACK_ID"] = sRack_ID;
            //dr["MODEL_ID"] = null;
            //dr["LOT_ID"] = null;
            //RQSTDT.Rows.Add(dr);

        }
    }
}
