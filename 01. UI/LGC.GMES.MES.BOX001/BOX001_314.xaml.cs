/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF.DataGrid;
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
    public partial class BOX001_314 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        private int isPalletQty = 0;
        private double isCellQty = 0;

        //C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        //{
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        //};

        //CheckBox chkAll = new CheckBox()
        //{
        //    Content = "ALL",
        //    IsChecked = true,
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    VerticalAlignment = System.Windows.VerticalAlignment.Center,
        //    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        //};

        #region Declaration & Constructor 
        public BOX001_314()
        {
            InitializeComponent();
            Initialize();

            Loaded += BOX001_314_Loaded;
        }

        private void BOX001_314_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_314_Loaded;

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                string sAreaid = ary.GetValue(0).ToString();
                cboAreaAll.SelectedValue = sAreaid;
                if (cboAreaAll.SelectedIndex < 0)
                {
                    cboAreaAll.SelectedIndex = 0;
                }
                
                // ScanID에 의한 PALLET 작업이력 조회 함수 호출
                ScanPalletInfo(string.Empty, ary.GetValue(1).ToString());
            }

        }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            // ComboBox 추가 필요
            CommonCombo combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };

            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            txtPALLETID.Focus();
            txtPALLETID.SelectAll();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        #endregion

        #region Event

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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");  //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
            string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

            string sArea = string.Empty;
            string sLot_Type = string.Empty;
            string sLine_ID = string.Empty;

            if (cboAreaAll2.SelectedIndex < 0 || cboAreaAll2.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                sArea = null;
            }
            else
            {
                sArea = cboAreaAll2.SelectedValue.ToString();
            }

            if (cboType.SelectedIndex < 0 || cboType.SelectedValue.ToString().Trim().Equals(""))
            {
                sLot_Type = null;
            }
            else
            {
                sLot_Type = cboType.SelectedValue.ToString();
            }

            try
            {
                // 조회 비즈 생성
                // 기존 Biz name : QR_GETRELEASE_CANCEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                //RQSTDT.Columns.Add("BOXTYPE", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_STAT_CODE"] = "CANCEL_SHIP";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_AREAID"] = sArea;

                if (txtBoxID.Text.Trim() != "")
                {
                    dr["BOXID"] = txtBoxID.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    //dr["BOXTYPE"] = Util.GetCondition(cboType, "선택하세요.");                
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                }                

                //if (dr["BOXTYPE"].Equals("") ) return;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CANCEL_PALLET_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgCancelHist);
                //dgCancelHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgCancelHist, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgCancel);
                    Init_Data();
                }

            });
           
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgCancel.GetRowCount() > 0)
            {
                string sArea = string.Empty;

                // 동 선택 확인
                if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("MMD0004");   //동을 선택해 주세요.
                    return;
                }
                else
                {
                    sArea = cboAreaAll.SelectedValue.ToString();
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                try
                {
                    try
                    {
                        //string sArea = cboAreaAll.SelectedValue.ToString();

                        // 출고 취소 처리
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("RCV_ISS_ID", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("CNCL_QTY", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["RCV_ISS_ID"] = DataTableConverter.GetValue(dgCancel.Rows[0].DataItem, "RCV_ISS_ID").ToString();
                        row["AREAID"] = sArea;
                        row["CNCL_QTY"] = isCellQty.ToString();
                        row["NOTE"] = "";
                        row["USERID"] = txtWorker.Tag as string;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inPallet = indataSet.Tables.Add("INPALLET");
                        inPallet.Columns.Add("BOXID", typeof(string));

                        for (int i = 0; i < dgCancel.GetRowCount(); i++)
                        {
                            DataRow row2 = inPallet.NewRow();
                            row2["BOXID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString();

                            indataSet.Tables["INPALLET"].Rows.Add(row2);
                        }

                        DataTable inLot = indataSet.Tables.Add("INBOX");
                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgCancel.GetRowCount(); i++)
                        {
                            DataRow row3 = inLot.NewRow();
                            row3["BOXID"] = "";
                            row3["LOTID"] = "";

                            indataSet.Tables["INBOX"].Rows.Add(row3);
                        }

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_SHIP_CELL", "INDATA,INPALLET,INBOX", null, indataSet);
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_CELL_CSLY", "INDATA,INPALLET,INBOX", null, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                                return;
                            }
                            else
                            {
                                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.


                                DataSet inCslyDataSet = new DataSet();

                                DataTable inRspnData = inCslyDataSet.Tables.Add("INDATA");
                                inRspnData.Columns.Add("SRCTYPE", typeof(string));
                                inRspnData.Columns.Add("RCV_ISS_ID", typeof(string));
                                inRspnData.Columns.Add("AREAID", typeof(string));
                                inRspnData.Columns.Add("CNCL_QTY", typeof(string));
                                inRspnData.Columns.Add("NOTE", typeof(string));
                                inRspnData.Columns.Add("USERID", typeof(string));

                                DataRow rowRspn = inRspnData.NewRow();
                                rowRspn["SRCTYPE"] = "UI";
                                rowRspn["RCV_ISS_ID"] = DataTableConverter.GetValue(dgCancel.Rows[0].DataItem, "RCV_ISS_ID").ToString();
                                rowRspn["AREAID"] = sArea;
                                rowRspn["CNCL_QTY"] = isCellQty.ToString();
                                rowRspn["NOTE"] = "";
                                rowRspn["USERID"] = txtWorker.Tag as string;

                                inCslyDataSet.Tables["INDATA"].Rows.Add(row);

                                DataTable inRspnPallet = inCslyDataSet.Tables.Add("INPALLET");
                                inRspnPallet.Columns.Add("BOXID", typeof(string));

                                for (int i = 0; i < dgCancel.GetRowCount(); i++)
                                {
                                    DataRow rowRspn2 = inRspnPallet.NewRow();
                                    rowRspn2["BOXID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString();

                                    inCslyDataSet.Tables["INPALLET"].Rows.Add(rowRspn2);
                                }

                                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_RCV_ISS_PLLT", "INDATA,INRSPN", null, inCslyDataSet);

                                Util.gridClear(dgCancel);
                                Init_Data();
                            }
                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 대상이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageInfo("10008");   //선택된 데이터가 없습니다.
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_CONFIRM window = sender as BOX001_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

                try
                {
                    string sArea = cboAreaAll.SelectedValue.ToString();

                    // 출고 취소 처리
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("RCV_ISS_ID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("CNCL_QTY", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["RCV_ISS_ID"] = DataTableConverter.GetValue(dgCancel.Rows[0].DataItem, "RCV_ISS_ID").ToString();
                    row["AREAID"] = sArea;
                    row["CNCL_QTY"] = isCellQty.ToString();
                    row["NOTE"] = "";
                    //row["USERID"] = txtUserID.Text.ToString();
                    row["USERID"] = txtWorker.Tag as string;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inPallet = indataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("BOXID", typeof(string));

                    for (int i = 0; i < dgCancel.GetRowCount(); i++)
                    {
                        DataRow row2 = inPallet.NewRow();
                        row2["BOXID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString();

                        indataSet.Tables["INPALLET"].Rows.Add(row2);
                    }

                    DataTable inLot = indataSet.Tables.Add("INBOX");
                    inLot.Columns.Add("BOXID", typeof(string));
                    inLot.Columns.Add("LOTID", typeof(string));

                    for (int i = 0; i < dgCancel.GetRowCount(); i++)
                    {
                        DataRow row3 = inLot.NewRow();
                        row3["BOXID"] = "";
                        row3["LOTID"] = "";

                        indataSet.Tables["INBOX"].Rows.Add(row3);
                    }

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_SHIP_CELL", "INDATA,INPALLET,INBOX", null, indataSet);

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    Util.gridClear(dgCancel);
                    Init_Data();                

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            grdMain.Children.Remove(window);
        }


        private void txtPALLETID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletInfo(txtPALLETID.Text.Trim(), txtRCVID.Text.Trim());
            }
        }

        private bool ScanPalletInfo(string sPalletID, string sRcvIssID)
        {
            try
            {
                if (string.IsNullOrEmpty(sPalletID) && string.IsNullOrEmpty(sRcvIssID))
                {
                    Util.MessageValidation("SFU1411");   //PALLETID를 입력해주세요
                    return false;
                }

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPalletID;
                dr["RCV_ISS_ID"] = sRcvIssID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);                
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PALLET_INFO_FOR_CANCEL_SHIP_CSLY", "INDATA", "OUTDATA,OUTDATA_RCV", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count == 0)
                {
                    Util.MessageInfo("SFU1905");   //조회된 Data가 없습니다.
                    return false;
                }

                if (dgCancel.GetRowCount() != 0)
                {
                    for (int i = 0; i < dgCancel.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString() == sPalletID)
                        {
                            Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                            return false;
                        }

                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "RCV_ISS_ID").ToString() == sRcvIssID)
                        {
                            Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                            return false;
                        }

                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "RCV_ISS_ID").ToString() != dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString())
                        {
                            Util.MessageValidation("SFU3014"); //동일한 출고 ID 만 출고 취소 가능합니다.
                            return false;
                        }
                    }

                    dgCancel.IsReadOnly = false;
                    dgCancel.BeginNewRow();
                    dgCancel.EndNewRow(true);
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "BOXID", dsRslt.Tables["OUTDATA"].Rows[0]["BOXID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PROJECTNAME", dsRslt.Tables["OUTDATA"].Rows[0]["PROJECTNAME"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "TOTAL_QTY", dsRslt.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"].ToString());
                    dgCancel.IsReadOnly = true;
                }
                else
                {
                    dgCancel.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);
                }


                isPalletQty = 0;
                isCellQty = 0;
                for (int i = 0; i < dgCancel.GetRowCount(); i++)
                {

                    isPalletQty = isPalletQty + 1;
                    //isCellQty = isCellQty + Convert.ToInt32(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                    isCellQty = isCellQty + Util.NVC_Int(DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "TOTAL_QTY").ToString());


                }
                txtPALLET_QTY.Text = isPalletQty.ToString();
                txtTotal_QTY.Text = isCellQty.ToString();
                txtPALLETID.Text = "";
                txtRCVID.Text = "";
                txtPALLETID.Focus();

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
           Util.MessageConfirm("SFU1230", (result) =>
           {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


                    double dqty = Convert.ToDouble(DataTableConverter.GetValue(dgCancel.Rows[index].DataItem, "TOTAL_QTY").ToString());

                    isPalletQty = isPalletQty - 1;
                    txtPALLET_QTY.Text = isPalletQty.ToString();

                    isCellQty = isCellQty - dqty;
                    txtTotal_QTY.Text = isCellQty.ToString();

                    dgCancel.IsReadOnly = false;
                    dgCancel.RemoveRow(index);
                    dgCancel.IsReadOnly = true;

                }
            });
        }


        //private void dgCancel_CommittedEdit(object sender, DataGridCellEventArgs e)
        //{

        //    C1DataGrid dg = sender as C1DataGrid;

        //    if (e.Cell != null &&
        //        e.Cell.Presenter != null &&
        //        e.Cell.Presenter.Content != null)
        //    {
        //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
        //        if (chk != null)
        //        {
        //            switch (Convert.ToString(e.Cell.Column.Name))
        //            {
        //                case "CHECKBOXCOLUMN01":
        //                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);

        //                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
        //                    if (!chk.IsChecked.Value)
        //                    {
        //                        chkAll.IsChecked = false;
        //                    }
        //                    else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHECKBOXCOLUMN01"]) == true).Count() == dt.Rows.Count)
        //                    {
        //                        chkAll.IsChecked = true;
        //                    }

        //                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //                    break;
        //            }
        //        }
        //    }


        //}

        //private void dgCancel_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (string.IsNullOrEmpty(e.Column.Name) == false)
        //        {
        //            if (e.Column.Name.Equals("CHECKBOXCOLUMN01"))
        //            {
        //                pre.Content = chkAll;
        //                e.Column.HeaderPresenter.Content = pre;
        //                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
        //                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //            }
        //        }
        //    }));
        //}

        //void checkAll_Checked(object sender, RoutedEventArgs e)
        //{
        //    for (int idx = 0; idx < dgCancel.Rows.Count; idx++)
        //    {
        //        C1.WPF.DataGrid.DataGridRow row = dgCancel.Rows[idx];
        //        DataTableConverter.SetValue(row.DataItem, "CHECKBOXCOLUMN01", true);
        //    }
        //}

        //void checkAll_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    for (int idx = 0; idx < dgCancel.Rows.Count; idx++)
        //    {
        //        C1.WPF.DataGrid.DataGridRow row = dgCancel.Rows[idx];
        //        DataTableConverter.SetValue(row.DataItem, "CHECKBOXCOLUMN01", false);
        //    }
        //}

        //private void dgCancel_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{
        //    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

        //    if (e.Cell != null &&
        //        e.Cell.Presenter != null &&
        //        e.Cell.Presenter.Content != null)
        //    {
        //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
        //        if (chk != null)
        //        {
        //            switch (Convert.ToString(e.Cell.Column.Name))
        //            {
        //                case "CHECKBOXCOLUMN01":
        //                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);

        //                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
        //                    if (!chk.IsChecked.Value)
        //                    {
        //                        chkAll.IsChecked = false;
        //                    }
        //                    else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHECKBOXCOLUMN01"]) == true).Count() == dt.Rows.Count)
        //                    {
        //                        chkAll.IsChecked = true;
        //                    }

        //                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //                    break;
        //            }
        //        }
        //    }
        //}
        #endregion

        #region Mehod

        private void Init_Data()
        {
            isPalletQty = 0;
            isCellQty = 0;

            txtPALLETID.Text = null;
            txtPALLET_QTY.Text = null;
            txtTotal_QTY.Text = null;

            txtPALLETID.Focus();
            txtPALLETID.SelectAll();
        }
        
        private string SelectPalletStatus(string sPalletID)
        {
            string sResult = "EMPTY";

            try
            { 
                // 조회 비즈 생성
                // 기존 Biz name : QR_GETPALLETINFO_PALLETID_WMSCHK

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr[""] = "";
                dr[""] = "";

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count <= 0)
                {
                    sResult = "EMPTY";
                }
                // 상태 비교 체크
                // 물류입고취소 , 포장출고 상태가 아니면 
                else if (SearchResult.Rows[0]["STATUS"].ToString() == "STATUS")
                {
                    sResult = "STATUS";
                }
                else
                {
                    if (dgCancel.Rows.Count == 0)
                    {
                        dgCancel.ItemsSource = DataTableConverter.Convert(SearchResult);

                        sResult = "OK";
                    }
                    else
                    {
                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "PALLET_ID").ToString() == txtPALLETID.Text.ToString())
                            {
                                sResult = "DUPLICATION";
                                return sResult;
                            }
                        }

                        dgCancel.IsReadOnly = false;
                        dgCancel.BeginNewRow();
                        dgCancel.EndNewRow(true);
                        DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PALLET_ID", SearchResult.Rows[0]["PALLET_ID"].ToString());
                        DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "LOT_ID", SearchResult.Rows[0]["LOT_ID"].ToString());
                        DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PROD_ID", SearchResult.Rows[0]["PROD_ID"].ToString());
                        DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "QTY", SearchResult.Rows[0]["QTY"].ToString());
                        dgCancel.IsReadOnly = true;

                        sResult = "OK";
                    }

                    txtPALLET_QTY.Text = dgCancel.Rows.Count.ToString();
                    txtTotal_QTY.Text = SearchResult.Rows[0]["QTY"].ToString();
                }

                return sResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return sResult;
            }
        }







        #endregion

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void txtPALLETID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {                    
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletInfo(sPasteStrings[i], string.Empty) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtRCVID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletInfo(string.Empty, sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void dgCancelHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (Util.NVC(cell.Column.Name).Equals("RCV_ISS_ID") || Util.NVC(cell.Column.Name).Equals("BOXID"))
                    {
                        BOX001_012_BOX_HIST wndHist = new BOX001_012_BOX_HIST();
                        wndHist.FrameOperation = FrameOperation;

                        if (wndHist != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "BOXID"));

                            C1WindowExtension.SetParameters(wndHist, Parameters);
                            wndHist.Closed += new EventHandler(wndHist_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            BOX001_012_BOX_HIST window = sender as BOX001_012_BOX_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }        
    }
}
