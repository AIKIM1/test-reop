using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092_ABNORMAL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_107_MERGE_CANCEL_INFO : C1Window, IWorkArea
    {
        #region Initialize
        String LOTID;
        public IFrameOperation FrameOperation { get; set; }

        public COM001_107_MERGE_CANCEL_INFO()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            LOTID = Convert.ToString(tmps[0]);

            GetFromLot(LOTID);

        }

        private void GetFromLot(string lotid)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotid;

                dt.Rows.Add(dr);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_LOTTRACE_MERGE_CANCEL_TRGT_LOT", "RQSTDT", "RSLTDT", dt);

                Util.GridSetData(dgMergeCancel, result, FrameOperation, true);

                //if ((decimal)result.Compute("Sum(MERGE_QTY)", "1=1") != (decimal)result.Select("LOTID = '" + lotid + "'")[0]["WIPQTY"])
                //{
                //    dgMergeCancel.Columns[5].IsReadOnly = false;
                //}
                //else
                //{
                //    dgMergeCancel.Columns[5].IsReadOnly = true;
                //}

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Event
        private void dgMergeCancel_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            int idx = e.Cell.Row.Index;

            if (Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgMergeCancel.Rows[idx].DataItem, "WIPQTY_IN"))) < Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgMergeCancel.Rows[idx].DataItem, "MERGE_CANCEL_QTY"))))
            {
                Util.MessageValidation("SFU4124"); //취소 수량이 투입량을 넘길 수 없습니다.

                decimal qty = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgMergeCancel.Rows[idx].DataItem, "INIT_MERGE_CANCEL_QTY"))); 
                DataTableConverter.SetValue(dgMergeCancel.Rows[idx].DataItem, "MERGE_CANCEL_QTY", qty);
                return;
            }
        }
        private void btnMergeCancel_Click(object sender, RoutedEventArgs e)
        {

            DataTable dt = DataTableConverter.Convert(dgMergeCancel.ItemsSource);

            if ((double)dt.Compute("Sum(MERGE_CANCEL_QTY)", "1=1") != (double)dt.Compute("Sum(WIPQTY)", "1=1"))
            {
                Util.MessageValidation("SFU4113"); //현재 재공량의 합과 취소수량이 합이 다릅니다.
                return;
            }


            //합권취소 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2010"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("LOTID", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("TO_LOT_QTY", typeof(decimal));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    row["LOTID"] = LOTID;
                    row["NOTE"] = "";
                    row["USERID"] = LoginInfo.USERID;
                    row["TO_LOT_QTY"] = Decimal.Parse(Util.NVC(DataTableConverter.Convert(dgMergeCancel.ItemsSource).Select("MERGE_TRGT_FLAG = 'Y'")[0]["MERGE_CANCEL_QTY"]));
                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable formLotID = indataSet.Tables.Add("FROM_LOT");
                    formLotID.Columns.Add("LOTID", typeof(string));
                    formLotID.Columns.Add("CANCEL_QTY", typeof(decimal));

                    row = null;

                    for (int i = 0; i < dgMergeCancel.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "MERGE_TRGT_FLAG")).Equals("N"))
                        {
                            row = formLotID.NewRow();
                            row["LOTID"] = DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "LOTID");
                            row["CANCEL_QTY"] = DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "MERGE_CANCEL_QTY");
                            indataSet.Tables["FROM_LOT"].Rows.Add(row);
                        }
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_REG_CANCEL_MERGE_LOT_QTY", "INDATA,FROM_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU2008");  //합권 취소 완료

                            this.DialogResult = MessageBoxResult.OK;
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }

                    }, indataSet);


                    //try
                    //{

                    //    DataSet ds = new DataSet();

                    //    DataTable indata = ds.Tables.Add("INDATA");
                    //    indata.Columns.Add("SRCTYPE", typeof(string));
                    //    indata.Columns.Add("LOTID", typeof(string));
                    //    indata.Columns.Add("NOTE", typeof(string));
                    //    indata.Columns.Add("USERID", typeof(string));

                    //    DataRow dr = indata.NewRow();
                    //    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    //    dr["LOTID"] = LOTID;
                    //    dr["NOTE"] = "";
                    //    dr["USERID"] = LoginInfo.USERID;
                    //    indata.Rows.Add(dr);

                    //    DataTable from_lot = ds.Tables.Add("FROM_LOT");
                    //    from_lot.Columns.Add("LOTID", typeof(string));
                    //    from_lot.Columns.Add("CANCEL_QTY", typeof(decimal));

                    //    for (int i = 0; i < dgMergeCancel.GetRowCount(); i++)
                    //    {
                    //        if (Util.NVC(DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "MERGE_TRGT_FLAG")).Equals("N"))
                    //        {
                    //            DataRow dr2 = from_lot.NewRow();
                    //            dr2["LOTID"] = DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "LOTID");
                    //            dr2["CANCEL_QTY"] = DataTableConverter.GetValue(dgMergeCancel.Rows[i].DataItem, "MERGE_QTY");
                    //            from_lot.Rows.Add(dr2);
                    //        }
                          

                    //    }

                    //    loadingIndicator.Visibility = Visibility.Visible;

                    //    new ClientProxy().ExecuteService_Multi("BR_REG_CANCEL_MERGE_LOT_QTY", "INDATA, FROM_LOT", null, (bizResult, bizException) =>
                    //    {
                    //        try
                    //        {
                    //            if (bizException != null)
                    //            {
                    //                Util.MessageException(bizException);
                    //                return;
                    //            }

                    //            Util.MessageInfo("SFU2008");  //합권 취소 완료

                    //            this.DialogResult = MessageBoxResult.OK;

                    //        }
                    //        catch (Exception ex)
                    //        {

                    //        }
                    //        finally
                    //        {
                    //            loadingIndicator.Visibility = Visibility.Collapsed;
                    //        }

                    //    }, ds);
                      

                    //}
                    //catch (Exception ex)
                    //{
                    //    Util.MessageException(ex);
                    //}
                }
            });
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }



        #endregion

       
    }
}
