/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.09.02  안유수 C20220831-000586  포장 재구성 화면에서 포장정보삭제 기능 바활성화 요청 처리




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_013 : UserControl, IWorkArea
    {
        private int isPalletQty = 0;
        private double isCellQty = 0;

        Util _Util = new Util();

        #region Declaration & Constructor 
        public BOX001_013()
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
            listAuth.Add(btnRework);
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

            CommonCombo combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };

            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] {cboLine}, sCase: "ALLAREA");

            combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] {cboAreaAll2 }, cbChild: new C1ComboBox[] { cboModelLot }, sFilter: sFilter, sCase: "LINE_CP");

            combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine });

            txtPALLETID.Focus();
            txtPALLETID.SelectAll();

            //if (LoginInfo.CFG_SHOP_ID == "A050" || LoginInfo.CFG_SHOP_ID == "A040")
            //{
            //    tiDeleteBox.Visibility = Visibility.Visible;
            //}

            IsPersonByAuth(LoginInfo.USERID);
            // tiDeleteBox
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
            try
            {
                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); // Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); // Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                string sArea = string.Empty;
                string sLot_Type = string.Empty;
                string sLine_ID = string.Empty;

                Util.gridClear(dgReworkHist);

                //if (cboAreaAll2.SelectedIndex < 0 || cboAreaAll2.SelectedValue.ToString().Trim().Equals("SELECT"))
                //{
                //    sArea = null;
                //}
                //else
                //{
                //    sArea = cboAreaAll2.SelectedValue.ToString();
                //}

                // 동 선택 확인
                if (cboAreaAll2.SelectedIndex < 0 || cboAreaAll2.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("MMD0004");   //동을 선택해 주세요.
                    return;
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


                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("ACTID", typeof(String));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("EQPT_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "CANCEL_END_OUTER_PACKING";
                dr["CMCDTYPE"] = "BOX_STAT";
                dr["AREAID"] = sArea;

                if (txtBoxID.Text.Trim() != "")
                {
                    dr["BOXID"] = getPalletBCD(txtBoxID.Text.Trim());
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                    if (!string.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue))) dr["EQSGID"] = cboLine.SelectedValue;
                    if (!string.IsNullOrWhiteSpace(Util.NVC(cboModelLot.SelectedValue))) dr["MDLLOT_ID"] = cboModelLot.SelectedValue;
                    if (!string.IsNullOrWhiteSpace(cboEqpt.SelectedValue.ToString())) dr["EQPT_ID"] = cboEqpt.SelectedValue;
                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNPACK_PALLET_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgReworkHist);
                //dgReworkHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgReworkHist, SearchResult, FrameOperation);

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
                    Util.gridClear(dgRework);
                    Init_Data();
                }
            });
       
        }

        private void btnRework_Click(object sender, RoutedEventArgs e)
        {

                if (dgRework.GetRowCount() > 0)
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
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                try
                {
                    //재작업을 진행하시겠습니까?
                    Util.MessageConfirm("SFU2070", (msgresult) =>
                    {

                        if (msgresult == MessageBoxResult.OK)
                        {
                            // 포장 재작업
                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("BOX_QTY", typeof(string));
                            inData.Columns.Add("UNPACK_QTY", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("NOTE", typeof(string));
                            inData.Columns.Add("AREAID", typeof(string));

                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = "UI";
                            row["BOX_QTY"] = isPalletQty.ToString();
                            row["UNPACK_QTY"] = isCellQty.ToString();
                            row["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;//txtUserID.Tag;
                            row["NOTE"] = "";
                            row["AREAID"] = sArea;

                            indataSet.Tables["INDATA"].Rows.Add(row);

                            DataTable inPallet = indataSet.Tables.Add("INBOX");
                            inPallet.Columns.Add("BOXID", typeof(string));

                            for (int i = 0; i < dgRework.GetRowCount(); i++)
                            {
                                DataRow row2 = inPallet.NewRow();
                                row2["BOXID"] = DataTableConverter.GetValue(dgRework.Rows[i].DataItem, "BOXID").ToString();

                                indataSet.Tables["INBOX"].Rows.Add(row2);
                            }

                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_CELL_FOR_PALLET", "INDATA,INBOX", null, indataSet);

                            Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                            Util.gridClear(dgRework);
                            Init_Data();
                        }
                    });
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
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("재작업 대상이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
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
                    // 포장 재작업
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("BOX_QTY", typeof(string));
                    inData.Columns.Add("UNPACK_QTY", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["BOX_QTY"] = isPalletQty.ToString();
                    row["UNPACK_QTY"] = isCellQty.ToString();
                    //row["USERID"] = txtUserID.Text.ToString();
                    row["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;//txtUserID.Tag;
                    row["NOTE"] = "";
                    row["AREAID"] = sArea;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inPallet = indataSet.Tables.Add("INBOX");
                    inPallet.Columns.Add("BOXID", typeof(string));

                    for (int i = 0; i < dgRework.GetRowCount(); i++)
                    {
                        DataRow row2 = inPallet.NewRow();
                        row2["BOXID"] = DataTableConverter.GetValue(dgRework.Rows[i].DataItem, "BOXID").ToString();

                        indataSet.Tables["INBOX"].Rows.Add(row2);
                    }

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_CELL", "INDATA,INBOX", null, indataSet);

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    Util.gridClear(dgRework);
                    Init_Data();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            grdMain.Children.Remove(window);
        }

        private bool ScanPalletInfo(string sPalletID)
        {
            try
            {
                string sArea = string.Empty;
                // 동 선택 확인
                if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("MMD0004");   //동을 선택해 주세요.
                    return false;
                }
                else
                {
                    sArea = cboAreaAll.SelectedValue.ToString();
                }

                if (string.IsNullOrEmpty(sPalletID))
                {
                    Util.MessageValidation("SFU1411");   //PALLETID를 입력해주세요
                    return false;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sArea;
                dr["BOXID"] = sPalletID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU1905");   //조회된 Data가 없습니다.
                    return false;
                }

                if (dgRework.GetRowCount() != 0)
                {
                    for (int i = 0; i < dgRework.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgRework.Rows[i].DataItem, "BOXID").ToString() == sPalletID)
                        {
                            Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                            return false;
                        }
                    }

                    dgRework.IsReadOnly = false;
                    dgRework.BeginNewRow();
                    dgRework.EndNewRow(true);
                    DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "BOXID", SearchResult.Rows[0]["BOXID"].ToString());
                    DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "TOTAL_QTY", SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                    DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "BOXSTAT", SearchResult.Rows[0]["BOXSTAT"].ToString());
                    DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "PLLT_BCD_ID", SearchResult.Rows[0]["PLLT_BCD_ID"].ToString());
                    dgRework.IsReadOnly = true;
                }
                else
                {
                    dgRework.ItemsSource = DataTableConverter.Convert(SearchResult);
                }

                isPalletQty = isPalletQty + 1;
                txtPALLET_QTY.Text = isPalletQty.ToString();

                isCellQty = isCellQty + Convert.ToDouble(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                txtTotal_QTY.Text = isCellQty.ToString();

                txtPALLETID.Text = "";
                txtPALLETID.Focus();

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }


        private void txtPALLETID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletInfo(getPalletBCD(txtPALLETID.Text.Trim()));
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }

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

            //복구 하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1227"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1227", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    decimal dBoxQty = Convert.ToDecimal(DataTableConverter.GetValue(dgReworkHist.Rows[index].DataItem, "TOTAL_QTY").ToString());
                    string sBoxId = DataTableConverter.GetValue(dgReworkHist.Rows[index].DataItem, "BOXID").ToString();

                    try
                    {
                        // 포장 재작업
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("BOXID", typeof(string));
                        inData.Columns.Add("BOX_QTY", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));


                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["AREAID"] = sArea;
                        row["BOXID"] = sBoxId;
                        row["BOX_QTY"] = dBoxQty;
                        row["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;//txtUserID.Tag;
                        row["NOTE"] = "";

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_CELL_CANCEL", "INDATA", null, indataSet);

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                        dgReworkHist.IsReadOnly = false;
                        dgReworkHist.RemoveRow(index);
                        dgReworkHist.IsReadOnly = true;
                    }
                    catch(Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


                    double dqty = Convert.ToDouble(DataTableConverter.GetValue(dgRework.Rows[index].DataItem, "TOTAL_QTY").ToString());

                    isPalletQty = isPalletQty - 1;
                    txtPALLET_QTY.Text = isPalletQty.ToString();

                    isCellQty = isCellQty - dqty;
                    txtTotal_QTY.Text = isCellQty.ToString();

                    dgRework.IsReadOnly = false;
                    dgRework.RemoveRow(index);
                    dgRework.IsReadOnly = true;

                }
            });
        }

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
                // 기존 Biz name : QR_GETPALLETINFO_PALLETID

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
                //else if (SearchResult.Rows[0]["STATUS"].ToString() == "STATUS")
                //{
                //    sResult = "STATUS";
                //}
                else
                {
                    if (dgRework.Rows.Count == 0)
                    {
                        dgRework.ItemsSource = DataTableConverter.Convert(SearchResult);

                        sResult = "OK";
                    }
                    else
                    {
                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgRework.Rows[i].DataItem, "PALLET_ID").ToString() == txtPALLETID.Text.ToString())
                            {
                                sResult = "DUPLICATION";
                                return sResult;
                            }
                        }

                        dgRework.IsReadOnly = false;
                        dgRework.BeginNewRow();
                        dgRework.EndNewRow(true);
                        DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "PALLET_ID", SearchResult.Rows[0]["PALLET_ID"].ToString());
                        DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "LOT_ID", SearchResult.Rows[0]["LOT_ID"].ToString());
                        DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "PROD_ID", SearchResult.Rows[0]["PROD_ID"].ToString());
                        DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "QTY", SearchResult.Rows[0]["QTY"].ToString());
                        dgRework.IsReadOnly = true;

                        sResult = "OK";
                    }

                    //txtPALLET_QTY.Text = dgRework.Rows.Count.ToString();
                    txtPALLET_QTY.Text = Convert.ToString(dgRework.GetRowCount());
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
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletInfo(getPalletBCD(sPasteStrings[i].Trim())) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtDeletePALLETID_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtDeletePALLETID_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnDeleteRework_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }
            try
            {
                //재작업을 진행하시겠습니까?
                Util.MessageConfirm("SFU2070", (msgresult) =>
                {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        // 포장 재작업
                        // 조회 비즈 생성
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("PALLETID", typeof(String));
                        RQSTDT.Columns.Add("USERID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["PALLETID"] = txtDeletePALLETID.Text.ToString().Trim();
                        dr["USERID"] = txtWorker.Tag as string;

                        RQSTDT.Rows.Add(dr);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_REWORK_BOX_CELL", "RQSTDT", "", RQSTDT);

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.


                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

        }

        private void btnDeleteRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtDeletePALLETID.Clear();
        }

        private void IsPersonByAuth(string sUserID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = "GMES_SPEED_CALL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    tiDeleteBox.Visibility = Visibility.Visible;
            }
            catch (Exception ex) { }


        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLine.SelectedValue != null)
            {
                CommonCombo combo = new CommonCombo();
                C1ComboBox[] cboEquipmentParent = { cboLine };
                String[] sFilter2 = { Process.CELL_BOXING };
                combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT");
            }
        }

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(int idx, string sAreaID)
        {
            // 파레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (idx == 0)
                {
                    if (dgRework.Columns.Contains("PLLT_BCD_ID"))
                        dgRework.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgReworkHist.Columns.Contains("PLLT_BCD_ID"))
                        dgReworkHist.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (idx == 0)
                {
                    if (dgRework.Columns.Contains("PLLT_BCD_ID"))
                        dgRework.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (dgReworkHist.Columns.Contains("PLLT_BCD_ID"))
                        dgReworkHist.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                }
            }
        }

        // 팔레트 Box 정보 조회
        private string getPalletBCD(string palletid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "INDATA";
            RQSTDT.Columns.Add("CSTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["CSTID"] = palletid;

            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

            if (SearchResult != null && SearchResult.Rows.Count > 0)
            {
                return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
            }
            return palletid;
        }

        private void cboAreaAll_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAREAID = Util.NVC(cboAreaAll.SelectedValue);

            // Barcode 속성 표시 여부
            isVisibleBCD(0, sAREAID);
        }

        private void cboAreaAll2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAREAID = Util.NVC(cboAreaAll2.SelectedValue);

            // Barcode 속성 표시 여부
            isVisibleBCD(1, sAREAID);
        }
    }
}
