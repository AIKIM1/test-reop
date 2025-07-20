/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_033 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public BOX001_033()
        {
            InitializeComponent();            
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            SetEvent();

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            txtLotid.Focus();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region Mehod

        #endregion

        #region Event

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotid = string.Empty;
                    sLotid = txtLotid.Text.Trim();

                    if (sLotid == "")
                    {
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    if (dgReturn.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgReturn.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                Util.MessageValidation("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                return;
                            }
                        }
                    }

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("LANGID", typeof(string));
                    inData.Columns.Add("AREA", typeof(string));

                    DataRow row = inData.NewRow();
                    row["LANGID"] = LoginInfo.LANGID;
                    row["AREA"] = LoginInfo.CFG_AREA_ID;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inLot = indataSet.Tables.Add("INLOT");
                    inLot.Columns.Add("LOTID", typeof(string));

                    DataRow row2 = inLot.NewRow();
                    row2["LOTID"] = sLotid;

                    indataSet.Tables["INLOT"].Rows.Add(row2);

                    new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_CONFIRM_RETURN_PRODUCT_FOR_NJ", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                              //  Util.AlertByBiz("BR_PRD_SEL_CONFIRM_RETURN_PRODUCT_FOR_NJ", bizException.Message, bizException.ToString());
                                return;
                            }

                            if (dgReturn.GetRowCount() == 0)
                            {
                                dgReturn.ItemsSource = DataTableConverter.Convert(bizResult.Tables["OUTDATA"]);
                            }
                            else
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgReturn.ItemsSource);
                                dtSource.Merge(bizResult.Tables["OUTDATA"]);

                                Util.gridClear(dgReturn);
                                //dgReturn.ItemsSource = DataTableConverter.Convert(dtSource);
                                Util.GridSetData(dgReturn, dtSource, FrameOperation, true);
                            }

                            txtLotid.SelectAll();
                            txtLotid.Focus();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                           // LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }

                    }, indataSet);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReturn.GetRowCount() ==0)
                {
                    Util.MessageValidation("SFU2060");        //스캔한 데이터가 없습니다.
                    return;
                }

                //반품 입고 하시겠습니까?
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2808"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
               Util.MessageConfirm("SFU2808", (result) =>
               {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SHOPID", typeof(string));
                        inData.Columns.Add("AREA", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        row["AREA"] = LoginInfo.CFG_AREA_ID;
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = txtRemark.Text.ToString();

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("MOVE_ORD_ID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("WIPQTY2", typeof(string));

                        for (int i = 0; i < dgReturn.GetRowCount(); i++)
                        {
                            DataRow row2 = inLot.NewRow();

                            row2["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "MOVE_ORD_ID"));
                            row2["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID"));
                            row2["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "WIPQTY2"));

                            indataSet.Tables["INLOT"].Rows.Add(row2);                         
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT_FOR_NJ", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                   // Util.AlertByBiz("BR_PRD_REG_CONFIRM_RETURN_PRODUCT_FOR_NJ", bizException.Message, bizException.ToString());
                                    return;
                                }

                                Clear_Form();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                              //  LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Initialize_dgReceive()
        {
            Clear_Form();
        }

        private void Clear_Form()
        {
            Util.gridClear(dgReturn);
            txtLotid.Text = null;
            txtRemark.Text = null;
            txtLotid.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgMaster);
                Util.gridClear(dgDetail);

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_ORD_RETURN_HIST_FOR_NJ", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgMaster, SearchResult, FrameOperation, true);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_ORD_RETURN_HIST_FOR_NJ", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.AlertByBiz("DA_PRD_SEL_MOVE_ORD_RETURN_HIST_FOR_NJ", bizException.Message, bizException.ToString());
                //            return;
                //        }

                //        Util.GridSetData(dgMaster, bizResult, FrameOperation, true);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //});
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
          //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
          Util.MessageConfirm("SFU1230",(result) =>
          {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgReturn.IsReadOnly = false;
                    dgReturn.RemoveRow(index);
                    dgReturn.IsReadOnly = true;
                }
            });
        }

        private void btnDelete_All_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgReturn);
            txtLotid.Text = null;
            txtRemark.Text = null;
            txtLotid.Focus();
        }

        private void dgMasterChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgMaster.SelectedIndex = idx;

                    SearchMoveLOTList(DataTableConverter.GetValue(dgMaster.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgDetail);

                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SearchMoveLOTList(String sMoveOrderID, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            { 
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = sMoveOrderID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_ORD_RETURN_LOT_FOR_NJ", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(DataGrid, DetailResult, FrameOperation, true);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_ORD_RETURN_LOT_FOR_NJ", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.AlertByBiz("DA_PRD_SEL_MOVE_ORD_RETURN_LOT_FOR_NJ", bizException.Message, bizException.ToString());
                //            return;
                //        }

                //        Util.GridSetData(DataGrid, bizResult, FrameOperation, true);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //});
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        private void dgReturn_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경

                if (e.Cell.Column.Name.Equals("WIPQTY2"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
                }
             

            }));
        }
    }
}
