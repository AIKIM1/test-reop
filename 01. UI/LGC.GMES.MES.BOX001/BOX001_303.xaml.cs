/*************************************************************************************
 Created Date : 2020.12.28
      Creator : 이제섭
   Decription : 포장 재작업
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.28  DEVELOPER : Initial Created.
  2023.06.02  홍석원    : Pallet Barcode를 사용하는 Area인 경우 Pallet Barcode ID를 이용하여 조회 할 수 있도록 수정
  2023.11.07  임근영    : INDATA 추가. 
  2024.03.13  최동훈    : 포장재작업시 Remark입력 기능 개발(동별공통코드)
  2024.07.23  최석준    : 사외반품Cell 포함여부 추가 (2025년 적용예정, 수정 시 연락부탁드립니다)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
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
    public partial class BOX001_303 : UserControl, IWorkArea
    {
        private int isPalletQty = 0;
        private double isCellQty = 0;

        // Pallet Barcode 사용 여부를 저장하기 위한 변수
        bool isCellPalletBarcodeUseArea = false;

        // 2024.03.13 포장재작업시 Remark입력 기능 개발
        private bool bBOX001_303_REMARK_USEFLAG = false;
        private Util _Util = new Util();

        #region Declaration & Constructor 
        public BOX001_303()
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

            // Pallet Barcode를 사용하는 Area인지 확인
            isCellPalletBarcodeUseArea = checkCellPalletBarcodeUseArea();

            // Pallet Barcode를 사용하는 Area인 경우 PalletBarcodeID 컬럼을 보여주기
            if (isCellPalletBarcodeUseArea)
            {
                dgRework.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgReworkHist.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }

            // 2024.03.13 포장재작업시 Remark입력 기능 개발
            bBOX001_303_REMARK_USEFLAG = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "BOX001_303_REMARK_USEFLAG");
            if(bBOX001_303_REMARK_USEFLAG)
            {
                dgReworkHist.Columns["REMARK"].Visibility = Visibility.Visible;
            }

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

            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgRework.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;
                dgReworkHist.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;
            }

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo combo = new CommonCombo();
            CommonCombo_Form comboF = new CommonCombo_Form();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };

            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine }, sCase: "ALLAREA");

            comboF.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboAreaAll2 }, cbChild: new C1ComboBox[] { cboModelLot }, sFilter: sFilter, sCase: "LINE");

            combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine });

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

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "CANCEL_END_OUTER_PACKING";
                dr["CMCDTYPE"] = "BOX_STAT";
                dr["AREAID"] = sArea;

                if (txtBoxID.Text.Trim() != "")
                {
                    string palletID = txtBoxID.Text.Trim();

                    // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                    if (isCellPalletBarcodeUseArea)
                    {
                        string palletBarcodeID = getPalletID(palletID);
                        if (!palletBarcodeID.IsNullOrEmpty())
                        {
                            palletID = palletBarcodeID;
                        }
                    }

                    dr["BOXID"] = palletID;
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                    if (!string.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue))) dr["EQSGID"] = cboLine.SelectedValue;
                    if (!string.IsNullOrWhiteSpace(Util.NVC(cboModelLot.SelectedValue))) dr["MDLLOT_ID"] = cboModelLot.SelectedValue;
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

                    // 2024.03.13 포장재작업시 Remark입력 기능 개발
                    if (bBOX001_303_REMARK_USEFLAG)
                    {
                        BOX001_303_REMARK popupRemark = new BOX001_303_REMARK();
                        popupRemark.FrameOperation = this.FrameOperation;

                        object[] parameters = new object[1];
                        parameters[0] = sArea;

                        C1WindowExtension.SetParameters(popupRemark, parameters);
                        popupRemark.Closed -= new EventHandler(popupRemark_Closed);
                        popupRemark.Closed += new EventHandler(popupRemark_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => popupRemark.ShowModal()));
         
                        //grdMain.Children.Add(popupRemark);
                        //popupRemark.BringToFront();
                    }
                    else
                    {
                        //재작업을 진행하시겠습니까?
                        Util.MessageConfirm("SFU2070", (msgresult) =>
                        {
                            if (msgresult == MessageBoxResult.OK)
                            {
                                SaveRework(sArea, "");
                                    
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
            else
            {
                Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                return;
            }            
        }

        private void popupRemark_Closed(object sender, EventArgs e)
        {
            // 2024.03.13 포장재작업시 Remark입력 기능 개발
            BOX001_303_REMARK popupRemark = sender as BOX001_303_REMARK;

            if (popupRemark != null && popupRemark.DialogResult == MessageBoxResult.OK)
            {
                popupRemark.Closed -= new EventHandler(popupRemark_Closed);
                System.Windows.Forms.Application.DoEvents();

                // 포장 재작업 처리
                SaveRework(popupRemark.Area, popupRemark.Remark);

            }

            this.grdMain.Children.Remove(popupRemark);
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

                // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                if (isCellPalletBarcodeUseArea)
                {
                    string palletBarcodeID = getPalletID(sPalletID);
                    if (!palletBarcodeID.IsNullOrEmpty())
                    {
                        sPalletID = palletBarcodeID;
                    }
                }

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
                  //  DataTableConverter.SetValue(dgRework.CurrentRow.DataItem, "OCOP_RTN_CELL_ICL_FLAG", SearchResult.Rows[0]["OCOP_RTN_CELL_ICL_FLAG"].ToString());
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
                return false;
            }
        }


        private void txtPALLETID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletInfo(txtPALLETID.Text.Trim());
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
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
                        inData.Columns.Add("SHOPID", typeof(string)); //20231115 추가
                        inData.Columns.Add("LANGID", typeof(string)); //

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["AREAID"] = sArea;
                        row["BOXID"] = sBoxId;
                        row["BOX_QTY"] = dBoxQty;
                        row["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;//txtUserID.Tag;
                        row["NOTE"] = "";
                        row["SHOPID"] = LoginInfo.CFG_SHOP_ID;  //20231115 추가  
                        row["LANGID"] = LoginInfo.LANGID; //

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_RESTORE_PALLET_BX", "INDATA", null, indataSet);

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

        // Pallet Barcode를 사용하는 Area인지 확인
        private bool checkCellPalletBarcodeUseArea()
        {
            bool usePalletBarcode = false;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_PLT_BCD_USE_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (!dtRslt.IsNullOrEmpty() && dtRslt.Rows.Count > 0)
            {
                usePalletBarcode = true;
            }

            return usePalletBarcode;
        }

        // 활성화 사외 반품 처리 여부 사용 Area 조회
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Pallet Barcode ID를 PalletID로 전환
        private string getPalletID(string palletBarcodeID)
        {
            string palletID = "";

            DataTable inTable = new DataTable();
            inTable.Columns.Add("CSTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CSTID"] = palletBarcodeID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", inTable);

            if (!dtRslt.IsNullOrEmpty() && dtRslt.Rows.Count > 0)
            {
                palletID = dtRslt.Rows[0]["CURR_LOTID"].ToString();
            }

            return palletID;
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
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletInfo(sPasteStrings[i].Trim()) == false)
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

        private void SaveRework(string sArea, string sRemark)
        {
            // 2024.03.13 포장재작업시 Remark입력 기능 개발
            try
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
                inData.Columns.Add("SHOP_ID", typeof(string)); //20231107 추가 
                inData.Columns.Add("LANGID", typeof(string)); // 

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["BOX_QTY"] = isPalletQty.ToString();
                row["UNPACK_QTY"] = isCellQty.ToString();
                row["USERID"] = txtWorker.Tag as string;
                row["NOTE"] = sRemark;
                row["AREAID"] = sArea;
                row["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;  //20231107 추가  
                row["LANGID"] = LoginInfo.LANGID; //

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inPallet = indataSet.Tables.Add("INBOX");
                inPallet.Columns.Add("BOXID", typeof(string));

                for (int i = 0; i < dgRework.GetRowCount(); i++)
                {
                    DataRow row2 = inPallet.NewRow();
                    row2["BOXID"] = DataTableConverter.GetValue(dgRework.Rows[i].DataItem, "BOXID").ToString();

                    indataSet.Tables["INBOX"].Rows.Add(row2);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_UNPACK_PALLET_BX", "INDATA,INBOX", null, indataSet);

                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                Util.gridClear(dgRework);
                Init_Data();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
    }
}
