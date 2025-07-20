/*************************************************************************************
 Created Date : 2024.04.30
      Creator : 이제섭
   Decription : 포장 출고 (Tray) (ESMI 1동 활성화 Tray 단위 포장 대응)
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.30  DEVELOPER : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_329 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();
        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        bool bRFUse = false;
        bool bShipWaitYN = false; // 미판정 여부
        int iPreValue = 0;

        private int isPalletQty = 0;
        private int isTrayQty = 0;
        private double isCellQty = 0;

        // 출고 LotID 로 조회하여, 처음으로 조회되는 조립Lot 저장하기 위한 변수
        private string isFirstLotID = string.Empty;

        // 출고 LotID 로 조회하여, 마지막으로 조회되는 조립Lot 저장하기 위한 변수
        private string isLastLotID = string.Empty;

        #region [조회시간]
        private string sWorkReSetTime = string.Empty;///
        private string sWorkEndTime = string.Empty;
        #endregion

        // Pallet Barcode 사용 여부를 저장하기 위한 변수
        bool isCellPalletBarcodeUseArea = false;

        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center

        };

        #region Declaration & Constructor 
        public BOX001_329()
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
          
            TabVisible();

            DailySearch.IsSelected = true;

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPackOut);
            listAuth.Add(btnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();

            Task<bool> task = WaitCallback();
            task.ContinueWith(_ =>
            {
                C1TabControl.SelectedIndex = 0;
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (cboEqsgShot_Daily.ItemsSource != null) succeeded = true;
                await Task.Delay(100);
            }
            return true;
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

            SetWorkResetTime();//////////////////////
            dtpDateFrom.SelectedDateTime = DateTime.Now;    
            dtpDateTo.SelectedDateTime = DateTime.Now;


            dtpFromTime.DateTime = GetJobDateFrom();///추가
            dtpToTime.DateTime = GetJobDateTo(); /////추가


            dtpDateFrom_Daily.SelectedDateTime = DateTime.Now;
            dtpDateTo_Daily.SelectedDateTime = DateTime.Now;

            String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };

            initCombo();

            _combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

            _combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            _combo.SetCombo(cboAreaAll2_daily, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            // SetComboBox2(cboLottype); //Lottype 양산 필터 추가
            

            txtPalletID.Focus();
            txtPalletID.SelectAll();

        }

        private void initCombo()     //// 
        {
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };
            String[] sFilter2 = { Process.CELL_BOXING };

            C1ComboBox[] cboToChild = { cboLocFrom };
            _combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.NONE, cbChild: cboToChild, sCase: "ALLAREA");

            C1ComboBox[] cboFromParent = { cboAreaAll };
           // C1ComboBox[] cboToChild2 = { cboLocTo };
            _combo.SetCombo(cboLocFrom, CommonCombo.ComboStatus.SELECT, cbParent: cboFromParent, sFilter: sFilter2);

            //C1ComboBox[] cboFromParent2 = { cboLocFrom };
            C1ComboBox[] cboFromChild = { cboComp };
            _comboF.SetCombo(cboLocTo, CommonCombo_Form.ComboStatus.SELECT, cbParent: cboFromParent, cbChild: cboFromChild);

            C1ComboBox[] cboCompParent = { cboLocTo, cboAreaAll };
            _comboF.SetCombo(cboComp, CommonCombo_Form.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            //LOTTYPE 세팅
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.SELECT, sCase: "LOTTYPE");
            //DEFAULT LOTTYPE 양산('P') 세팅
            cboLottype.SelectedValue = "P";
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom_Daily.SelectedDataTimeChanged += dtpDateFrom_Daily_SelectedDataTimeChanged;
            dtpDateTo_Daily.SelectedDataTimeChanged += dtpDateTo_Daily_SelectedDataTimeChanged;

            cboAreaAll2_daily.SelectedValueChanged += cboAreaAll2_daily_SelectedValueChanged;
            if (cboAreaAll2_daily.Items.Count > 0) cboAreaAll2_daily_SelectedValueChanged(null, null);
        }
        #endregion

        #region Mehod
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)   ///얘는 뭐냐...
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

        private void Initialize_Out()
        {
            Util.gridClear(dgPackOut);

            isPalletQty = 0;
            isCellQty = 0;

            txtPalletQty.Text = "0";
            txtTrayQty.Text = "0";
            txtCellQty.Text = "0";
            txtPalletID.Text = "";
            txtPalletID.Focus();

            cboLocTo.SelectedIndex = 0;
            cboLocFrom.SelectedIndex = 0;

            //LOTTYPE 설정 콤보박스 초기화
            cboLottype.IsEnabled = true;
        }



        private void Search_PalletInfo(int idx)
        {
            string sOutLotid = string.Empty;

            sOutLotid = DataTableConverter.GetValue(dgOutHist.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
            RQSTDT.Columns.Add("LANGID", typeof(String));
            RQSTDT.Columns.Add("PACK_EQSGID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["RCV_ISS_ID"] = sOutLotid;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PACK_EQSGID"] = DataTableConverter.GetValue(dgOutHist.Rows[idx].DataItem, "PACK_EQSGID").ToString();

            RQSTDT.Rows.Add(dr);

            DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIP_PALLET_DETAIL_BX", "RQSTDT", "RSLTDT", RQSTDT);

            DataView DetailResultView = DetailResult.DefaultView;

            DetailResultView.Sort = "OUTER_BOXID ASC";

            DataTable dtBinding = DetailResultView.ToTable();

            Util.gridClear(dgOutDetail);
            //Util.GridSetData(dgOutDetail, DetailResult, FrameOperation, true);
            Util.GridSetData(dgOutDetail, dtBinding, FrameOperation, true);

            string[] sColumnName = new string[] { "OUTER_BOXID" };
            _Util.SetDataGridMergeExtensionCol(dgOutDetail, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        }



        private void SetSelectedQty()
        {
            int lSumPallet = 0;
            int lSumCell = 0;

            for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
            {
                if (DataTableConverter.GetValue(dgOutHist.Rows[lsCount].DataItem, "CHK").ToString() == "1" || DataTableConverter.GetValue(dgOutHist.Rows[lsCount].DataItem, "CHK").ToString() == "True")
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);
                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["TOTALQTY"].Index).Value);
                }
            }

            txtPalletQty_Search.Value = lSumPallet;
            txtCellQty_Search.Value = lSumCell;
        }

        #region 
        /// <summary>
        /// 라인 정보 조회
        /// </summary>
        private void SetEqsgCombo()
        {
            //const string bizRuleName = "DA_BAS_SEL_COMBO_LINE";
            //string[] arrColumn = { "LANGID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, cboAreaAll2_daily.SelectedValue.ToString() };
            //string selectedValueText = cboEqsgShot_Daily.SelectedValuePath;
            //string displayMemberText = cboEqsgShot_Daily.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cboEqsgShot_Daily, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaAll2_daily.SelectedValue.ToString();
                dr["EXCEPT_GROUP"] = "VD";
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP_V2", "RQSTDT", "RSLTDT", RQSTDT);

                cboEqsgShot_Daily.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboEqsgShot_Daily.CheckAll();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// 모델랏 조회
        /// </summary>
        /// <param name="cboEqsg"></param>
        private void SetModelLotCombo()
        {
            //const string bizRuleName = "DA_BAS_SEL_MDLLOT_CBO";
            //string[] arrColumn = { "EQSGID" };
            //string[] arrCondition = { cboEqsgShot_Daily.SelectedValue.ToString() };
            //string selectedValueText = cboModelLot_daily.SelectedValuePath;
            //string displayMemberText = cboModelLot_daily.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cboModelLot_daily, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = cboEqsgShot_Daily.SelectedItemsToString ?? null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);


                cboModelLot_daily.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboModelLot_daily.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TabVisible()
        {

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "MULTI_PALLET_HOLD_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    DailySearch.Visibility = Visibility.Visible;
                }
                else
                {
                    DailySearch.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

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

        #region Button Event
        private void btnLotInformation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sRCVISSID = string.Empty;
                int iSelCnt = 0;
                for (int i = 0; i < dgOutHist.GetRowCount(); i++)
                {
                    if (Util.NVC(dgOutHist.GetCell(i, dgOutHist.Columns["CHK"].Index).Value) == "1" || Util.NVC(dgOutHist.GetCell(i, dgOutHist.Columns["CHK"].Index).Value) == "True")
                    {
                        iSelCnt = iSelCnt + 1;
                        if (iSelCnt == 1)
                        {
                            sRCVISSID = sRCVISSID + Util.NVC(dgOutHist.GetCell(i, dgOutHist.Columns["RCV_ISS_ID"].Index).Value);
                        }
                        else
                        {
                            sRCVISSID = sRCVISSID + "," + Util.NVC(dgOutHist.GetCell(i, dgOutHist.Columns["RCV_ISS_ID"].Index).Value);
                        }
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                BOX001_015_ASSY_LOT popUp = new BOX001_015_ASSY_LOT();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = sSHOPID;
                    Parameters[1] = "";
                    Parameters[2] = sAREAID;
                    Parameters[3] = sRCVISSID;

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.ShowModal();
                    popUp.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Initialize_Out();
                }
            });

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sOut_ID = string.Empty;

                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") <= 0)
                {
                    Util.AlertInfo("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") != 1)
                {
                    Util.AlertInfo("SFU2004"); //하나의 Pallet만 선택해주십시오.
                    return;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                {
                    // LOT ID, QTY 조회
                    if (DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_STAT_CODE").ToString() == "SHIPPING")
                    {
                        sOut_ID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_ID").ToString();

                        loadingIndicator.Visibility = Visibility.Visible;
                        string[] sParam = { sAREAID, sOut_ID };
                        // 포장 출고 취소
                        this.FrameOperation.OpenMenu("SFU010736051", true, sParam);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Util.MessageValidation("SFU3013");  //입고 전 출고요청만 취소 가능합니다.
                        return;
                    }
                }
                else
                {
                    Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER wndPopup = new CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; //  LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sOut_ID = string.Empty;
                string sLotTerm = string.Empty;

                isFirstLotID = "";
                isLastLotID = "";

                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") <= 0)
                {
                    Util.AlertInfo("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") != 1)
                {
                    Util.AlertInfo("SFU2004"); //하나의 Pallet만 선택해주십시오.
                    return;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                {
                    // LOT ID, QTY 조회
                    sOut_ID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_ID").ToString();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RCV_ISS_ID"] = sOut_ID;

                    RQSTDT.Rows.Add(dr);

                    DataTable PalletInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_BY_PACKID", "RQSTDT", "RSLTDT", RQSTDT);

                    if (PalletInfo.Rows.Count < 0)
                    {
                        Util.MessageValidation("SFU1192"); //Lot 정보를 조회할 수 없습니다.
                        return;
                    }

                    for (int i = 0; i < PalletInfo.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            isFirstLotID = PalletInfo.Rows[i]["LOTID"].ToString();
                        }
                        else
                        {
                            isLastLotID = PalletInfo.Rows[i]["LOTID"].ToString();
                        }
                    }

                    if (isLastLotID == "")
                    {
                        isLastLotID = isFirstLotID;
                    }


                    // 최대 편차 조회
                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["RCV_ISS_ID"] = sOut_ID;

                    RQSTDT2.Rows.Add(dr2);

                    DataTable dtLotterm = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_RCV_CP", "RQSTDT", "RSLTDT", RQSTDT2);

                    if (dtLotterm.Rows.Count < 0)
                    {
                        Util.MessageValidation("SFU3009");  //최대 편차 조회시 에러가 발생하였습니다.
                        return;
                    }

                    sLotTerm = dtLotterm.Rows[0]["LOTTERM"].ToString();


                    DataRow[] drChk = Util.gridGetChecked(ref dgOutHist, "CHK");

                    // 작업일자 
                    //DateTime ProdDate = Convert.ToDateTime(DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "INSDTTM").ToString());
                    DateTime ProdDate = Convert.ToDateTime(drChk[0]["INSDTTM"].ToString());
                    string sProdDate = ProdDate.Year.ToString() + "년" + ProdDate.Month.ToString() + "월" + ProdDate.Day.ToString() + "일" +
                                        ProdDate.Hour.ToString() + "시" + ProdDate.Minute.ToString() + "분" + ProdDate.Second.ToString() + "초";


                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT1.Columns.Add("LANGID", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["RCV_ISS_ID"] = sOut_ID;
                    dr1["LANGID"] = LoginInfo.LANGID;

                    RQSTDT1.Rows.Add(dr1);

                    DataTable dtLineResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_QTY_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT1);

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #region Report

                    DataTable dtPacking_Tag = new DataTable();

                    //Title
                    dtPacking_Tag.Columns.Add("Title", typeof(string));
                    dtPacking_Tag.Columns.Add("Sign", typeof(string));
                    dtPacking_Tag.Columns.Add("FCS", typeof(string));
                    dtPacking_Tag.Columns.Add("Pack", typeof(string));
                    dtPacking_Tag.Columns.Add("Logistics", typeof(string));
                    dtPacking_Tag.Columns.Add("Out_Pallet_Qty", typeof(string));
                    dtPacking_Tag.Columns.Add("Out_Pack_ID", typeof(string));
                    dtPacking_Tag.Columns.Add("Lot_Term", typeof(string));
                    dtPacking_Tag.Columns.Add("Out_Hist", typeof(string));
                    dtPacking_Tag.Columns.Add("ProdDate", typeof(string));
                    dtPacking_Tag.Columns.Add("ProdQty", typeof(string));
                    dtPacking_Tag.Columns.Add("Worker", typeof(string));
                    dtPacking_Tag.Columns.Add("ProdID", typeof(string));
                    dtPacking_Tag.Columns.Add("OUTWH", typeof(string));
                    dtPacking_Tag.Columns.Add("INWH", typeof(string));

                    // QTY_1 ~ QTY_8
                    for (int i = 1; i < 9; i++)
                    {
                        dtPacking_Tag.Columns.Add("QTY_" + i, typeof(string));
                    }

                    // Line1 ~ Line8
                    for (int i = 1; i < 9; i++)
                    {
                        dtPacking_Tag.Columns.Add("Line_" + i, typeof(string));
                    }

                    // Qty1 ~ Qty8
                    for (int i = 1; i < 9; i++)
                    {
                        dtPacking_Tag.Columns.Add("Qty" + i, typeof(string));
                    }

                    // Model
                    dtPacking_Tag.Columns.Add("Model", typeof(string));

                    // 출고 Pallet 수량
                    dtPacking_Tag.Columns.Add("Pallet_Qty", typeof(string));

                    // Pack_ID
                    dtPacking_Tag.Columns.Add("Pack_ID", typeof(string));

                    // Pack_ID Barcode
                    dtPacking_Tag.Columns.Add("HEAD_BARCODE", typeof(string));

                    // Lot 최대 편차
                    dtPacking_Tag.Columns.Add("Lot_Qty", typeof(string));

                    // 작업일자
                    dtPacking_Tag.Columns.Add("Prod_Date", typeof(string));

                    // 제품수량
                    dtPacking_Tag.Columns.Add("Prod_Qty", typeof(string));

                    // 제품 ID
                    dtPacking_Tag.Columns.Add("Prod_ID", typeof(string));

                    // 작업자
                    dtPacking_Tag.Columns.Add("User", typeof(string));

                    // 출고창고
                    dtPacking_Tag.Columns.Add("Out_WH", typeof(string));

                    // 입고창고
                    dtPacking_Tag.Columns.Add("In_WH", typeof(string));

                    // Pallet ID ( P_ID1 ~ P_ID120 ) => Pallet 수량 만큼 프린트 되도록 수정
                    for (int i = 1; i <= dgOutDetail.GetRowCount(); i++)
                    {
                        dtPacking_Tag.Columns.Add("P_ID" + i, typeof(string));
                    }

                    // 수량 ( P_Qty1 ~ P_Qty120 ) => Pallet 수량 만큼 프린트 되도록 수정
                    for (int i = 1; i <= dgOutDetail.GetRowCount(); i++)
                    {
                        dtPacking_Tag.Columns.Add("P_Qty" + i, typeof(string));
                    }

                    // Lot ID ( L_ID1 ~ L_ID24 ) => Lot 수량만큼 프린트 되도록 수정 
                    // 
                    for (int i = 1; i <= PalletInfo.Rows.Count; i++)
                    {
                        dtPacking_Tag.Columns.Add("L_ID" + i, typeof(string));
                    }

                    // 수량 ( L_Qty1 ~ L_Qty24 ) => Lot 수량만큼 프린트 되도록 수정 
                    for (int i = 1; i <= PalletInfo.Rows.Count; i++)
                    {
                        dtPacking_Tag.Columns.Add("L_Qty" + i, typeof(string));
                    }

                    string sType = string.Empty;

                    if (drChk[0]["BOXTYPE"].ToString() == "PLT")
                    {
                        sType = "P";
                    }
                    else if (drChk[0]["BOXTYPE"].ToString() == "MGZ")
                    {
                        sType = "M";
                    }
                    else if (drChk[0]["BOXTYPE"].ToString() == "BOX")
                    {
                        sType = "B";
                    }

                    DataRow drCrad = null;
                    DataRow drCrad2 = null;
                    drCrad = dtPacking_Tag.NewRow();
                    drCrad2 = dtPacking_Tag.NewRow();

                    int iPalletMax = 120;
                    int iLotMax = 24;
                    bool bAddFlag = (PalletInfo.Rows.Count > iLotMax || dgOutDetail.GetRowCount() > iPalletMax);

                    drCrad["Title"] = ObjectDic.Instance.GetObjectName("포장출고이력");
                    drCrad["Sign"] = ObjectDic.Instance.GetObjectName("서명");
                    drCrad["FCS"] = ObjectDic.Instance.GetObjectName("활성화");
                    drCrad["Pack"] = ObjectDic.Instance.GetObjectName("Pack실");
                    drCrad["Logistics"] = ObjectDic.Instance.GetObjectName("물류팀");
                    drCrad["Out_Pallet_Qty"] = ObjectDic.Instance.GetObjectName("출고Pallet수량");
                    drCrad["Out_Pack_ID"] = ObjectDic.Instance.GetObjectName("포장출고ID");
                    drCrad["Lot_Term"] = ObjectDic.Instance.GetObjectName("LOT최대편차");
                    drCrad["Out_Hist"] = ObjectDic.Instance.GetObjectName("발행이력");
                    drCrad["ProdDate"] = ObjectDic.Instance.GetObjectName("작업일자");
                    drCrad["ProdQty"] = ObjectDic.Instance.GetObjectName("제품수량");
                    drCrad["Worker"] = ObjectDic.Instance.GetObjectName("작업자");
                    drCrad["ProdID"] = ObjectDic.Instance.GetObjectName("제품ID");
                    drCrad["OUTWH"] = ObjectDic.Instance.GetObjectName("출고창고");
                    drCrad["INWH"] = ObjectDic.Instance.GetObjectName("입고창고");

                    if (bAddFlag)
                    {
                        drCrad2["Title"] = ObjectDic.Instance.GetObjectName("포장출고이력");
                        drCrad2["Sign"] = ObjectDic.Instance.GetObjectName("서명");
                        drCrad2["FCS"] = ObjectDic.Instance.GetObjectName("활성화");
                        drCrad2["Pack"] = ObjectDic.Instance.GetObjectName("Pack실");
                        drCrad2["Logistics"] = ObjectDic.Instance.GetObjectName("물류팀");
                        drCrad2["Out_Pallet_Qty"] = ObjectDic.Instance.GetObjectName("출고Pallet수량");
                        drCrad2["Out_Pack_ID"] = ObjectDic.Instance.GetObjectName("포장출고ID");
                        drCrad2["Lot_Term"] = ObjectDic.Instance.GetObjectName("LOT최대편차");
                        drCrad2["Out_Hist"] = ObjectDic.Instance.GetObjectName("발행이력");
                        drCrad2["ProdDate"] = ObjectDic.Instance.GetObjectName("작업일자");
                        drCrad2["ProdQty"] = ObjectDic.Instance.GetObjectName("제품수량");
                        drCrad2["Worker"] = ObjectDic.Instance.GetObjectName("작업자");
                        drCrad2["ProdID"] = ObjectDic.Instance.GetObjectName("제품ID");
                        drCrad2["OUTWH"] = ObjectDic.Instance.GetObjectName("출고창고");
                        drCrad2["INWH"] = ObjectDic.Instance.GetObjectName("입고창고");
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        drCrad["Line_" + (i + 1).ToString()] = "";
                        drCrad["Qty" + (i + 1).ToString()] = "";
                        drCrad["QTY_" + (i + 1).ToString()] = ObjectDic.Instance.GetObjectName("수량");

                        if (bAddFlag)
                        {
                            drCrad2["Line_" + (i + 1).ToString()] = "";
                            drCrad2["Qty" + (i + 1).ToString()] = "";
                            drCrad2["QTY_" + (i + 1).ToString()] = ObjectDic.Instance.GetObjectName("수량");
                        }
                    }

                    for (int i = 0; i < dtLineResult.Rows.Count; i++)
                    {
                        drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                        drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();

                        if (bAddFlag)
                        {
                            drCrad2["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                            drCrad2["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                        }
                    }

                    //drCrad["Line1"] = "";

                    //drCrad["Qty1"] = sType + "_" + drChk[0]["PALLETQTY"].ToString();

                    drCrad["Pallet_Qty"] = drChk[0]["PALLETQTY"].ToString();

                    drCrad["Model"] = drChk[0]["PROJECTNAME"].ToString() == string.Empty ? drChk[0]["MODLID"].ToString() : drChk[0]["PROJECTNAME"].ToString();

                    drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID

                    drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID

                    drCrad["Lot_Qty"] = sLotTerm;         //최대편차

                    drCrad["Prod_Date"] = sProdDate;         //작업일자

                    //Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);

                    drCrad["Prod_Qty"] = Math.Round(Convert.ToDouble(drChk[0]["TOTALQTY"].ToString()), 2, MidpointRounding.AwayFromZero);  //제품수량

                    //drCrad["Prod_ID"] = drChk[0]["PRODID"].ToString();         //제품ID
                    drCrad["Prod_ID"] = drChk[0]["TOP_PRODID"].ToString();         //완제품ID

                    drCrad["User"] = drChk[0]["INSUSERNAME"].ToString();         //작업자

                    drCrad["Out_WH"] = drChk[0]["FROM_SLOC_ID"].ToString();         //출고창고

                    drCrad["In_WH"] = drChk[0]["TO_SLOC_ID"].ToString();         //입고창고

                    #region 2022.12.2O 이제섭 ESWA 사외창고 운영 대응
                    if ("A5".Equals(LoginInfo.CFG_AREA_ID))
                    {
                        string sIntransitLocId = drChk[0]["INTRANSIT_SLOC_ID"].ToString();

                        if (!String.IsNullOrEmpty(sIntransitLocId))
                        {
                            drCrad["In_WH"] = String.Format("{0} >> {1}", sIntransitLocId, drCrad["In_WH"]);
                        }
                    }
                    #endregion

                    if (bAddFlag)
                    {
                        drCrad2["Pallet_Qty"] = drChk[0]["PALLETQTY"].ToString();
                        drCrad2["Model"] = drChk[0]["PROJECTNAME"].ToString() == string.Empty ? drChk[0]["MODLID"].ToString() : drChk[0]["PROJECTNAME"].ToString();
                        drCrad2["Pack_ID"] = sOut_ID;    //포장출고 ID                          
                        drCrad2["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID                           
                        drCrad2["Lot_Qty"] = sLotTerm;         //최대편차                              2
                        drCrad2["Prod_Date"] = sProdDate;         //작업일자                              2
                        //Math2.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);                              2
                        drCrad2["Prod_Qty"] = Math.Round(Convert.ToDouble(drChk[0]["TOTALQTY"].ToString()), 2, MidpointRounding.AwayFromZero);  //제품수량                              2
                        //drCrad2["Prod_ID"] = drChk[0]["PRODID"].ToString();         //제품ID                              2
                        drCrad2["Prod_ID"] = drChk[0]["TOP_PRODID"].ToString();         //완제품ID                              2
                        drCrad2["User"] = drChk[0]["INSUSERNAME"].ToString();         //작업자                              2
                        drCrad2["Out_WH"] = drChk[0]["FROM_SLOC_ID"].ToString();         //출고창고                              2
                        drCrad2["In_WH"] = drChk[0]["TO_SLOC_ID"].ToString();         //입고창고
                    }

                    // 현재..max 120
                    int j = 1;
                    for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                    {
                        //if(drCrad2)
                        if (j <= iPalletMax)
                        {
                            drCrad["P_ID" + j] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString();
                            //drCrad["P_Qty" + i] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString();
                            drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            int idx = j - iPalletMax; // C20210312-000216 두번째 페이지 출력 오류 수정
                            drCrad2["P_ID" + idx] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString();
                            //drCrad["P_Qty" + i] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString();
                            drCrad2["P_Qty" + idx] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                        }
                        j++;
                    }

                    // 현재..max 24
                    int k = 1;
                    for (int i = 0; i < PalletInfo.Rows.Count; i++)
                    {
                        if (k <= iLotMax)
                        {
                            drCrad["L_ID" + k] = PalletInfo.Rows[i]["LOTID"].ToString();
                            drCrad["L_Qty" + k] = PalletInfo.Rows[i]["LOTQTY"].ToString();
                        }
                        else
                        {
                            int idx = k - iLotMax;
                            drCrad2["L_ID" + idx] = PalletInfo.Rows[i]["LOTID"].ToString();
                            drCrad2["L_Qty" + idx] = PalletInfo.Rows[i]["LOTQTY"].ToString();
                        }
                        k++;
                    }

                    dtPacking_Tag.Rows.Add(drCrad);
                    if (bAddFlag) dtPacking_Tag.Rows.Add(drCrad2);

                    //object[] Parameters = new object[2];
                    //Parameters[0] = "Packing_Tag";
                    //Parameters[1] = dtPacking_Tag;

                    //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                    //C1WindowExtension.SetParameters(rs, Parameters);
                    //rs.Show();

                    LGC.GMES.MES.BOX001.Report_Packing_Tag rs = new LGC.GMES.MES.BOX001.Report_Packing_Tag();
                    rs.FrameOperation = this.FrameOperation;


                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[3];
                        Parameters[0] = "Packing_Tag";
                        Parameters[1] = dtPacking_Tag;
                        Parameters[2] = bAddFlag;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        // this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                    #endregion
                }

                else
                {
                    Util.MessageValidation("SFU1261"); //선택된LOT이 없습니다.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                Report_Packing_Tag wndPopup = sender as Report_Packing_Tag;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
                grdMain.Children.Remove(wndPopup);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable SelectRelsIDToPalletInformation(string relsID)
        {
            try
            {
                // QR_GETPALLET_BYRELSID
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = relsID;

                DataTable PalletInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_BY_PACKID", "RQSTDT", "RSLTDT", RQSTDT);

                if (PalletInfo.Rows.Count > 0)
                {
                    return PalletInfo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable SelectAssyLotInformation(string relsID)
        {
            try
            {
                // BR_MLB05_13_RELSID
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr[""] = relsID;

                DataTable PalletInfo = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                int iCnt = PalletInfo.Rows.Count;

                if (iCnt > 0)
                {
                    isFirstLotID = PalletInfo.Rows[0]["LOTID"].ToString();
                    if (iCnt == 1)
                    {
                        isLastLotID = isFirstLotID;
                    }
                    else
                    {
                        isLastLotID = PalletInfo.Rows[iCnt]["LOTID"].ToString();
                    }
                }

                return PalletInfo;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string sLotID = string.Empty;
            Boolean bResult = true;

            if (dgOutHist.Rows.Count > 0)
            {
                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") > 0)
                {
                    if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                    {
                        sLotID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "LOT_ID").ToString();

                        //SelectOutHistToExcelFile(sLotID)
                    }
                }
                else
                {
                    //출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오.
                    Util.MessageValidation("SFU3254");
                    return;
                }
            }
            else
            {
                //출고 이력을 조회 하신 후 버튼을 클릭해주십시오.
                Util.MessageValidation("SFU3255");
                return;
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            txtPalletQty_Search.Value = 0;
            txtCellQty_Search.Value = 0;

            txtPalletQty_Total.Value = 0;
            txtCellQty_Total.Value = 0;

            Util.gridClear(dgOutHist);
            Util.gridClear(dgOutDetail);

            Out_Hist();
        }

        private void Out_Hist()
        {
            try
            {

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00"); ///
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00"); ;    ///

                string sLot_Type = string.Empty;

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
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["RCV_ISS_STAT_CODE"] = "SHIPPING";

                // P로 시작하는 ID는 팔렛
                if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1).ToUpper() == "P" )
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
                // R로 시작하는 ID는 입출고 ID (RCV_ISS_ID)
                //else if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1).ToUpper() != "P")
                else if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1).ToUpper() == "R")
                {
                    dr["RCV_ISS_ID"] = txtBoxID.Text.Trim(); 
                }
                // 이외의 ID는 TRAY
                else if (txtBoxID.Text.Trim() != "" && !(txtBoxID.Text.Substring(0, 1).ToUpper() == "R" || txtBoxID.Text.Substring(0, 1).ToUpper() == "P"))
                {
                    dr["INNER_BOXID"] = txtBoxID.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date; 
                    dr["TO_DATE"] = sEnd_date;    
                    dr["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                    dr["SHIPTO_ID"] = Util.NVC(cboShipTo.SelectedValue) == "" ? null : Util.NVC(cboShipTo.SelectedValue); ;
                }
                dr["FROM_AREAID"] = string.IsNullOrWhiteSpace(sAREAID) ? null : sAREAID;
                if (!string.IsNullOrWhiteSpace(cboLine.SelectedValue.ToString())) dr["EQSGID"] = cboLine.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIP_PALLET_HIST_BX", "INDATA", "OUTDATA", RQSTDT); 
                Util.gridClear(dgOutHist);
                Util.GridSetData(dgOutHist, SearchResult, FrameOperation, true);


                dgOutHist.MergingCells -= dgOutHist_MergingCells;
                dgOutHist.MergingCells += dgOutHist_MergingCells;

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);

                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["TOTALQTY"].Index).Value);
                }

                txtPalletQty_Total.Value = lSumPallet;
                txtCellQty_Total.Value = lSumCell;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgOutHist_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                int TOTALQTYxS = 0;
                int TOTALQTYxE = 0;
                bool bTOTALQTYStrt = false;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgOutHist.TopRows.Count; i < dgOutHist.Rows.Count; i++)
                {

                    if (dgOutHist.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "RCV_ISS_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "RCV_ISS_ID")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                for (int m = idxS; m <= idxE; m++)
                                {
                                    if (!bTOTALQTYStrt)
                                    {
                                        bTOTALQTYStrt = true;
                                        sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                                        TOTALQTYxS = m;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                            TOTALQTYxE = m;
                                        else
                                        {
                                            if (TOTALQTYxS > TOTALQTYxE)
                                            {
                                                TOTALQTYxE = TOTALQTYxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                                            bTOTALQTYStrt = true;
                                            sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                                            TOTALQTYxS = m;
                                        }
                                    }
                                }

                                if (bTOTALQTYStrt)
                                {
                                    if (TOTALQTYxS > TOTALQTYxE)
                                    {
                                        TOTALQTYxE = TOTALQTYxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                                    bTOTALQTYStrt = false;
                                    sTmpTOTALQTY = string.Empty;
                                    TOTALQTYxE = 0;
                                    TOTALQTYxS = 0;

                                }
                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "RCV_ISS_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {

                        if (bStrt)
                        {
                            for (int m = idxS; m <= idxE; m++)
                            {
                                //수량
                                if (!bTOTALQTYStrt)
                                {
                                    bTOTALQTYStrt = true;
                                    sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                                    TOTALQTYxS = m;

                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                        TOTALQTYxE = m;
                                    else
                                    {
                                        if (TOTALQTYxS > TOTALQTYxE)
                                        {
                                            TOTALQTYxE = TOTALQTYxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                                        bTOTALQTYStrt = true;
                                        sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                                        TOTALQTYxS = m;
                                    }
                                }
                            }
                            //수량
                            if (bTOTALQTYStrt)
                            {
                                if (TOTALQTYxS > TOTALQTYxE)
                                {
                                    TOTALQTYxE = TOTALQTYxS;
                                }
                                e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                                bTOTALQTYStrt = false;
                                sTmpTOTALQTY = string.Empty;
                                TOTALQTYxE = 0;
                                TOTALQTYxS = 0;

                            }

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "RCV_ISS_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                    }
                }


                if (bStrt)
                {
                    for (int m = idxS; m <= idxE; m++)
                    {
                        //수량
                        if (!bTOTALQTYStrt)
                        {
                            bTOTALQTYStrt = true;
                            sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                            TOTALQTYxS = m;

                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                TOTALQTYxE = m;
                            else
                            {
                                if (TOTALQTYxS > TOTALQTYxE)
                                {
                                    TOTALQTYxE = TOTALQTYxS;
                                }
                                e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                                bTOTALQTYStrt = true;
                                sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[m].DataItem, "TOTALQTY"));
                                TOTALQTYxS = m;

                            }
                        }
                    }
                    //수량
                    if (bTOTALQTYStrt)
                    {
                        if (TOTALQTYxS > TOTALQTYxE)
                        {
                            TOTALQTYxE = TOTALQTYxS;
                        }
                        e.Merge(new DataGridCellsRange(dgOutHist.GetCell(TOTALQTYxS, dgOutHist.Columns["TOTALQTY"].Index), dgOutHist.GetCell(TOTALQTYxE, dgOutHist.Columns["TOTALQTY"].Index)));
                        bTOTALQTYStrt = false;
                        sTmpTOTALQTY = string.Empty;
                        TOTALQTYxE = 0;
                        TOTALQTYxS = 0;

                    }
                    bStrt = false;
                }

            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isFirstLotID = "";
                isLastLotID = "";

                string sLotTerm = string.Empty;
                string sArea = string.Empty;
                string sLocFrom = string.Empty;
                string sLocTo = string.Empty;
                string sComp = string.Empty;

                if (dgPackOut.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }

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

                // 출고창고 선택 확인
                if (cboLocFrom.SelectedIndex < 0 || cboLocFrom.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU2068");  //출고창고를 선택하세요.
                    return;
                }
                else
                {
                    sLocFrom = cboLocFrom.SelectedValue.ToString();
                }

                // 입고창고 선택 확인
                if (cboLocTo.SelectedIndex < 0 || cboLocTo.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU2069");  //입고창고를 선택하세요.                    
                    return;
                }
                else
                {
                    sLocTo = cboLocTo.SelectedValue.ToString();
                }

                // 출하처 선택 확인
                if (cboComp.SelectedIndex < 0 || cboComp.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU3173");   //출하처를 선택 하십시오
                    return;
                }
                else
                {
                    sComp = cboComp.SelectedValue.ToString();
                }

                //if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                //{
                //    // 자동창고 출고 여부를 선택하세요.
                //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgPackOut.ItemsSource);
                if (dtInfo.Select("CUST_FTP_YN = 'Y' AND SHIPTO_ID <> '" + sComp + "'").Count() > 0)
                {
                    //FTP 전송하는 고객사입니다. 출하처를 확인해주세요.
                    Util.MessageValidation("SFU3408");
                    return;
                }

                if (dgPackOut.GetRowCount() > 0)
                {
                    BOX001_011_CONFIRM wndConfirm = new BOX001_011_CONFIRM();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        this.btnPackOut.IsEnabled = false;

                        object[] Parameters = new object[6];
                        Parameters[0] = cboComp.Text;
                        Parameters[1] = cboLocTo.Text;

                        #region 2022.12.2O 이제섭 ESWA 사외창고 운영 대응
                        if ("A5".Equals(LoginInfo.CFG_AREA_ID))
                        {
                            string sIntransitLocId = String.IsNullOrEmpty(DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "INTRANSIT_SLOC_NAME").ToString()) ? "" : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "INTRANSIT_SLOC_NAME").ToString();

                            if (!String.IsNullOrEmpty(sIntransitLocId))
                            {
                                Parameters[1] = String.Format("{0} >> {1}", sIntransitLocId, Parameters[1]);
                            }
                        }
                        #endregion
                        Parameters[2] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                        Parameters[3] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();
                        Parameters[4] = txtPalletQty.Text;
                        Parameters[5] = txtCellQty.Text;
                        C1WindowExtension.SetParameters(wndConfirm, Parameters);
                        wndConfirm.Closing += WndConfirm_Closing;
                        wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                        //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        grdMain.Children.Add(wndConfirm);
                        wndConfirm.BringToFront();
                    }
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;                
            }
        }

        private void WndConfirm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_011_CONFIRM window = sender as BOX001_011_CONFIRM;

            try
            {

                if (window.DialogResult == MessageBoxResult.OK)
                {

                    isFirstLotID = "";
                    isLastLotID = "";

                    string sLotTerm = string.Empty;
                    string sArea = string.Empty;
                    string sLocFrom = string.Empty;
                    string sLocTo = string.Empty;
                    string sComp = string.Empty;
                    string sOut_ID = string.Empty;

                    if (dgPackOut.GetRowCount() == 0)
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }

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

                    // 출고창고 선택 확인
                    if (cboLocFrom.SelectedIndex < 0 || cboLocFrom.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU2068");  //출고창고를 선택하세요.
                        return;
                    }
                    else
                    {
                        sLocFrom = cboLocFrom.SelectedValue.ToString();
                    }

                    // 입고창고 선택 확인
                    if (cboLocTo.SelectedIndex < 0 || cboLocTo.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU2069");  //입고창고를 선택하세요.
                        return;
                    }
                    else
                    {
                        sLocTo = cboLocTo.SelectedValue.ToString();
                    }

                    // 출하처 선택 확인
                    if (cboComp.SelectedIndex < 0 || cboComp.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3173");   //출하처를 선택 하십시오
                        return;
                    }
                    else
                    {
                        sComp = cboComp.SelectedValue.ToString();
                    }

                    if (string.IsNullOrEmpty(txtWorker.Text))
                    {
                        //작업자를 선택해 주세요
                        Util.MessageValidation("SFU1843");
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                        return;
                    }

                    //if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                    //{
                    //    // 자동창고 출고 여부를 선택하세요.
                    //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                    //    return;
                    //}

                    DataTable RQSTDT4 = new DataTable();
                    RQSTDT4.TableName = "RQSTDT";
                    RQSTDT4.Columns.Add("FROM_AREAID", typeof(String));
                    RQSTDT4.Columns.Add("TO_SLOC_ID", typeof(String));

                    DataRow dr4 = RQSTDT4.NewRow();
                    dr4["FROM_AREAID"] = sArea;
                    dr4["TO_SLOC_ID"] = sLocTo;

                    RQSTDT4.Rows.Add(dr4);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_AREAID", "RQSTDT", "RSLTDT", RQSTDT4);

                    string sTo_Areaid = string.Empty;

                    if (SearchResult.Rows.Count > 0)
                    {
                        sTo_Areaid = SearchResult.Rows[0]["TO_AREAID"].ToString();
                    }
                    else if (SearchResult.Rows.Count == 0)
                    {
                        sTo_Areaid = null;
                    }

                    if (dgPackOut.GetRowCount() > 0)
                    {
                        // 포장 출고 처리
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("ISS_QTY", typeof(int));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("SHIP_WAIT_YN", typeof(string));
                        inData.Columns.Add("AUTO_WH_ISS_FLAG", typeof(string));
                        inData.Columns.Add("INTRANSIT_SLOC_ID", typeof(string)); // 사외 경유 창고


                        DataRow row = inData.NewRow();
                        row["FROM_AREAID"] = sArea;
                        row["FROM_SLOC_ID"] = sLocFrom; // Util.GetCondition(cboLocFrom, "출고창고를선택하세요.");//출고창고
                        row["TO_AREAID"] = sTo_Areaid;          //입고 Area
                        row["TO_SLOC_ID"] = sLocTo;     // Util.GetCondition(cboLocTo, "입고창고를선택하세요.");//입고저장위치
                        row["ISS_QTY"] = isCellQty;     //출고수량
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = sComp;       // Util.GetCondition(cboComp, "출하처를선택하세요.");//출하처
                        row["NOTE"] = "";
                        row["USERID"] = txtWorker.Tag as string;
                        row["SHIP_WAIT_YN"] = bShipWaitYN ? "Y" : "N"; // 2023.02.28 미판정 여부 추가
                        row["AUTO_WH_ISS_FLAG"] = "N"; //bRFUse ? cboAutoYN.SelectedValue.ToString() : "N";

                        if (row["FROM_SLOC_ID"].Equals("") || row["TO_SLOC_ID"].Equals("") || row["SHIPTO_ID"].Equals("")) return;

                        #region 2022.12.2O 이제섭 ESWA 사외창고 운영 대응
                        if ("A5".Equals(LoginInfo.CFG_AREA_ID))
                        {
                            string sIntransitLocId = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "INTRANSIT_SLOC_ID").ToString();

                            if (!String.IsNullOrEmpty(sIntransitLocId))
                            {
                                row["INTRANSIT_SLOC_ID"] = sIntransitLocId;          // 사외 경유 창고
                            }
                        }
                        #endregion

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inPallet = indataSet.Tables.Add("INPALLET");
                        inPallet.Columns.Add("BOXID", typeof(string));
                        inPallet.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            if (inPallet.Rows.Count > 0)
                            {
                                if (inPallet.Select("BOXID = '" + DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString() + "'").Count() == 0)
                                {
                                    DataRow row2 = inPallet.NewRow();
                                    row2["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                    row2["OWMS_BOX_TYPE_CODE"] = "AC";

                                    indataSet.Tables["INPALLET"].Rows.Add(row2);
                                }
                            }
                            else
                            {
                                DataRow row2 = inPallet.NewRow();
                                row2["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                row2["OWMS_BOX_TYPE_CODE"] = "AC";

                                indataSet.Tables["INPALLET"].Rows.Add(row2);
                            }


                        }

                        new ClientProxy().ExecuteService_Multi("BR_SET_SHIP_CELL_TRAY", "INDATA,INPALLET", "OUTDATA", (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                return;
                            }

                            if (result.Tables["OUTDATA"].Rows.Count > 0)
                            {
                                sOut_ID = result.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString();
                            }
                            else
                            {
                                Util.MessageValidation("SFU3010");  //출고 ID 가 생성되지 않았습니다.
                                return;
                            }

                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                            DataRow dr = RQSTDT.NewRow();
                            dr["RCV_ISS_ID"] = sOut_ID;

                            RQSTDT.Rows.Add(dr);

                            DataTable LotResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_BY_PACKID", "RQSTDT", "RSLTDT", RQSTDT);

                            if (LotResult.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                                return;
                            }

                            for (int i = 0; i < LotResult.Rows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    isFirstLotID = LotResult.Rows[i]["LOTID"].ToString();
                                }
                                else
                                {
                                    isLastLotID = LotResult.Rows[i]["LOTID"].ToString();
                                }
                            }

                            if (isLastLotID == "")
                            {
                                isLastLotID = isFirstLotID;
                            }


                            // 최대 편차 조회
                            DataTable RQSTDT2 = new DataTable();
                            RQSTDT2.TableName = "RQSTDT";
                            RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));

                            DataRow dr2 = RQSTDT2.NewRow();
                            dr2["RCV_ISS_ID"] = sOut_ID;

                            RQSTDT2.Rows.Add(dr2);


                            DataTable dtLotterm = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_RCV_CP", "RQSTDT", "RSLTDT", RQSTDT2);

                            if (dtLotterm.Rows.Count < 0)
                            {
                                Util.MessageValidation("SFU2883", sOut_ID); //{1%}의 재공정보가 없습니다.
                                return;
                            }

                            sLotTerm = dtLotterm.Rows[0]["LOTTERM"].ToString();

                            // 작업일자                     
                            string sProdDate = DateTime.Now.Year.ToString() + "월" + DateTime.Now.Month.ToString() + "월" + DateTime.Now.Day.ToString() + "일" +
                                               DateTime.Now.Hour.ToString() + "시" + DateTime.Now.Minute.ToString() + "분" + DateTime.Now.Second.ToString() + "초";

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                            RQSTDT1.Columns.Add("LANGID", typeof(String));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["RCV_ISS_ID"] = sOut_ID;
                            dr1["LANGID"] = LoginInfo.LANGID;

                            RQSTDT1.Rows.Add(dr1);

                            DataTable dtLineResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_QTY_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT1);

                            #region Report 출력

                            DataTable dtPacking_Tag = new DataTable();

                            //Title
                            dtPacking_Tag.Columns.Add("Title", typeof(string));
                            dtPacking_Tag.Columns.Add("Sign", typeof(string));
                            dtPacking_Tag.Columns.Add("FCS", typeof(string));
                            dtPacking_Tag.Columns.Add("Pack", typeof(string));
                            dtPacking_Tag.Columns.Add("Logistics", typeof(string));
                            dtPacking_Tag.Columns.Add("Out_Pallet_Qty", typeof(string));
                            dtPacking_Tag.Columns.Add("Out_Pack_ID", typeof(string));
                            dtPacking_Tag.Columns.Add("Lot_Term", typeof(string));
                            dtPacking_Tag.Columns.Add("Out_Hist", typeof(string));
                            dtPacking_Tag.Columns.Add("ProdDate", typeof(string));
                            dtPacking_Tag.Columns.Add("ProdQty", typeof(string));
                            dtPacking_Tag.Columns.Add("Worker", typeof(string));
                            dtPacking_Tag.Columns.Add("ProdID", typeof(string));
                            dtPacking_Tag.Columns.Add("OUTWH", typeof(string));
                            dtPacking_Tag.Columns.Add("INWH", typeof(string));

                            // QTY_1 ~ QTY_8
                            for (int i = 1; i < 9; i++)
                            {
                                dtPacking_Tag.Columns.Add("QTY_" + i, typeof(string));
                            }


                            // Line1 ~ Line8
                            for (int i = 1; i < 9; i++)
                            {
                                dtPacking_Tag.Columns.Add("Line_" + i, typeof(string));
                            }

                            // Qty1 ~ Qty8
                            for (int i = 1; i < 9; i++)
                            {
                                dtPacking_Tag.Columns.Add("Qty" + i, typeof(string));
                            }

                            // Model
                            dtPacking_Tag.Columns.Add("Model", typeof(string));

                            // 출고 Pallet 수량
                            dtPacking_Tag.Columns.Add("Pallet_Qty", typeof(string));

                            // Pack_ID
                            dtPacking_Tag.Columns.Add("Pack_ID", typeof(string));

                            // Pack_ID Barcode
                            dtPacking_Tag.Columns.Add("HEAD_BARCODE", typeof(string));

                            // Lot 최대 편차
                            dtPacking_Tag.Columns.Add("Lot_Qty", typeof(string));

                            // 작업일자
                            dtPacking_Tag.Columns.Add("Prod_Date", typeof(string));

                            // 제품수량
                            dtPacking_Tag.Columns.Add("Prod_Qty", typeof(string));

                            // 제품 ID
                            dtPacking_Tag.Columns.Add("Prod_ID", typeof(string));

                            // 작업자
                            dtPacking_Tag.Columns.Add("User", typeof(string));

                            // 출고창고
                            dtPacking_Tag.Columns.Add("Out_WH", typeof(string));

                            // 입고창고
                            dtPacking_Tag.Columns.Add("In_WH", typeof(string));

                            // Pallet ID ( P_ID1 ~ P_ID120 )
                            for (int i = 1; i <= dgPackOut.GetRowCount(); i++)
                            {
                                dtPacking_Tag.Columns.Add("P_ID" + i, typeof(string));
                            }

                            // 수량 ( P_Qty1 ~ P_Qty120 )
                            for (int i = 1; i <= dgPackOut.GetRowCount(); i++)
                            {
                                dtPacking_Tag.Columns.Add("P_Qty" + i, typeof(string));
                            }

                            // Lot ID ( L_ID1 ~ L_ID24 )
                            for (int i = 1; i <= LotResult.Rows.Count; i++)
                            {
                                dtPacking_Tag.Columns.Add("L_ID" + i, typeof(string));
                            }

                            // 수량 ( L_Qty1 ~ L_Qty24 )
                            for (int i = 1; i <= LotResult.Rows.Count; i++)
                            {
                                dtPacking_Tag.Columns.Add("L_Qty" + i, typeof(string));
                            }


                            string sType = string.Empty;

                            if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "PLT")
                            {
                                sType = "P";
                            }
                            else if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "MGZ")
                            {
                                sType = "M";
                            }
                            else if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "TRY")
                            {
                                sType = "B";
                            }

                            DataRow drCrad = null;
                            DataRow drCrad2 = null;
                            drCrad = dtPacking_Tag.NewRow();
                            drCrad2 = dtPacking_Tag.NewRow();

                            int iPalletMax = 120;
                            int iLotMax = 24;
                            bool bAddFlag = (LotResult.Rows.Count > iLotMax || dgPackOut.GetRowCount() > iPalletMax);


                            drCrad["Title"] = ObjectDic.Instance.GetObjectName("포장출고이력");
                            drCrad["Sign"] = ObjectDic.Instance.GetObjectName("서명");
                            drCrad["FCS"] = ObjectDic.Instance.GetObjectName("활성화");
                            drCrad["Pack"] = ObjectDic.Instance.GetObjectName("Pack실");
                            drCrad["Logistics"] = ObjectDic.Instance.GetObjectName("물류팀");
                            drCrad["Out_Pallet_Qty"] = ObjectDic.Instance.GetObjectName("출고Pallet수량");
                            drCrad["Out_Pack_ID"] = ObjectDic.Instance.GetObjectName("포장출고ID");
                            drCrad["Lot_Term"] = ObjectDic.Instance.GetObjectName("LOT최대편차");
                            drCrad["Out_Hist"] = ObjectDic.Instance.GetObjectName("발행이력");
                            drCrad["ProdDate"] = ObjectDic.Instance.GetObjectName("작업일자");
                            drCrad["ProdQty"] = ObjectDic.Instance.GetObjectName("제품수량");
                            drCrad["Worker"] = ObjectDic.Instance.GetObjectName("작업자");
                            drCrad["ProdID"] = ObjectDic.Instance.GetObjectName("제품ID");
                            drCrad["OUTWH"] = ObjectDic.Instance.GetObjectName("출고창고");
                            drCrad["INWH"] = ObjectDic.Instance.GetObjectName("입고창고");


                            if (bAddFlag)
                            {
                                drCrad2["Title"] = ObjectDic.Instance.GetObjectName("포장출고이력");
                                drCrad2["Sign"] = ObjectDic.Instance.GetObjectName("서명");
                                drCrad2["FCS"] = ObjectDic.Instance.GetObjectName("활성화");
                                drCrad2["Pack"] = ObjectDic.Instance.GetObjectName("Pack실");
                                drCrad2["Logistics"] = ObjectDic.Instance.GetObjectName("물류팀");
                                drCrad2["Out_Pallet_Qty"] = ObjectDic.Instance.GetObjectName("출고Pallet수량");
                                drCrad2["Out_Pack_ID"] = ObjectDic.Instance.GetObjectName("포장출고ID");
                                drCrad2["Lot_Term"] = ObjectDic.Instance.GetObjectName("LOT최대편차");
                                drCrad2["Out_Hist"] = ObjectDic.Instance.GetObjectName("발행이력");
                                drCrad2["ProdDate"] = ObjectDic.Instance.GetObjectName("작업일자");
                                drCrad2["ProdQty"] = ObjectDic.Instance.GetObjectName("제품수량");
                                drCrad2["Worker"] = ObjectDic.Instance.GetObjectName("작업자");
                                drCrad2["ProdID"] = ObjectDic.Instance.GetObjectName("제품ID");
                                drCrad2["OUTWH"] = ObjectDic.Instance.GetObjectName("출고창고");
                                drCrad2["INWH"] = ObjectDic.Instance.GetObjectName("입고창고");
                            }

                            for (int i = 0; i < 8; i++)
                            {
                                drCrad["Line_" + (i + 1).ToString()] = "";
                                drCrad["Qty" + (i + 1).ToString()] = "";
                                drCrad["QTY_" + (i + 1).ToString()] = ObjectDic.Instance.GetObjectName("수량");
                                if (bAddFlag)
                                {
                                    drCrad2["Line_" + (i + 1).ToString()] = "";
                                    drCrad2["Qty" + (i + 1).ToString()] = "";
                                    drCrad2["QTY_" + (i + 1).ToString()] = ObjectDic.Instance.GetObjectName("수량");
                                }
                            }

                            for (int i = 0; i < dtLineResult.Rows.Count; i++)
                            {
                                drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                if (bAddFlag)
                                {
                                    drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                    drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                }
                            }

                            // Pallet List 출력 시 완제품으로 출력
                            string topProdID = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TOP_PRODID").ToString();

                            drCrad["Pallet_Qty"] = txtPalletQty.Text.ToString();
                            drCrad["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? topProdID : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                            drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID
                            drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                            drCrad["Lot_Qty"] = sLotTerm;         //최대편차
                            drCrad["Prod_Date"] = sProdDate;         //작업일자
                            drCrad["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                            drCrad["Prod_ID"] = topProdID;         //제품ID
                            drCrad["User"] = txtWorker.Text;         //작업자
                            drCrad["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                            drCrad["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고

                            if (bAddFlag)
                            {
                                drCrad2["Pallet_Qty"] = txtPalletQty.Text.ToString();
                                drCrad2["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? topProdID : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                                drCrad2["Pack_ID"] = sOut_ID;    //포장출고 ID
                                drCrad2["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                                drCrad2["Lot_Qty"] = sLotTerm;         //최대편차
                                drCrad2["Prod_Date"] = sProdDate;         //작업일자
                                drCrad2["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                                drCrad2["Prod_ID"] = topProdID;         //제품ID
                                drCrad2["User"] = txtWorker.Text;         //작업자
                                drCrad2["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                                drCrad2["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고

                            }
                            int j = 1;
                            for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                            {
                                if (j <= iPalletMax)
                                {
                                    drCrad["P_ID" + j] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                    drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                                }
                                else
                                {
                                    int idx = j - iPalletMax;
                                    drCrad2["P_ID" + idx] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                    drCrad2["P_Qty" + idx] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                                }
                                j++;
                            }

                            int k = 1;
                            for (int i = 0; i < LotResult.Rows.Count; i++)
                            {
                                if (k <= iLotMax)
                                {
                                    drCrad["L_ID" + k] = LotResult.Rows[i]["LOTID"].ToString();
                                    drCrad["L_Qty" + k] = LotResult.Rows[i]["LOTQTY"].ToString();
                                }
                                else
                                {
                                    int idx = k - iLotMax;
                                    drCrad2["L_ID" + idx] = LotResult.Rows[i]["LOTID"].ToString();
                                    drCrad2["L_Qty" + idx] = LotResult.Rows[i]["LOTQTY"].ToString();
                                }
                                k++;
                            }


                            dtPacking_Tag.Rows.Add(drCrad);
                            if (bAddFlag) dtPacking_Tag.Rows.Add(drCrad2);

                            //태그 발행 창 화면에 띄움.
                            object[] Parameters = new object[4];
                            Parameters[0] = "Packing_Tag";
                            Parameters[1] = dtPacking_Tag;
                            Parameters[2] = bAddFlag;
                            Parameters[3] = "2";

                            Report_Packing_Tag rs = new Report_Packing_Tag();
                            C1WindowExtension.SetParameters(rs, Parameters);

                            rs.Closed += new EventHandler(wndPackingTag_Closed);
                            grdMain.Children.Add(rs);
                            rs.BringToFront();
                            #endregion

                        }, indataSet);
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
                grdMain.Children.Remove(window);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                if (window.DialogResult != MessageBoxResult.OK)
                    loadingIndicator.Visibility = Visibility.Collapsed;

                this.btnPackOut.IsEnabled = true;
            }
        }


        private void wndPackingTag_Closed(object sender, EventArgs e)
        {
            Report_Packing_Tag wndPopup = sender as Report_Packing_Tag;
            Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
            Initialize_Out();
            loadingIndicator.Visibility = Visibility.Collapsed;
            grdMain.Children.Remove(wndPopup);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    double dqty = Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[index].DataItem, "TOTAL_QTY").ToString());

                    isPalletQty = isPalletQty - 1;
                    txtPalletQty.Text = isPalletQty.ToString();

                    isCellQty = isCellQty - dqty;
                    txtCellQty.Text = isCellQty.ToString();

                    dgPackOut.IsReadOnly = false;
                    dgPackOut.RemoveRow(index);
                    dgPackOut.IsReadOnly = true;

                    //cell 정보 없을 경우 LOTTYPE 다시 활성화
                    if (dgPackOut.GetRowCount() == 0)
                    {
                        //LOTTYPE 설정 콤보박스 초기화
                        cboLottype.IsEnabled = true;
                    }

                }
            });
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((bool)(sender as CheckBox).IsChecked)
            {
                string sLot_ID = Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "LOTID").ToString());

                GetPalletHist(sLot_ID);

                (sender as CheckBox).IsChecked = true;
            }

        }

        private void GetPalletHist(string sLotid)
        {
            //조회 비즈 생성
            //기존 Biz name : QR_GETPALLETINFO_RELSID_v01
            //포장 출고 ID 로 조회함...

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("", typeof(String));
            RQSTDT.Columns.Add("", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr[""] = "";
            dr[""] = "";

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

            Util.gridClear(dgOutDetail);
            dgOutDetail.ItemsSource = DataTableConverter.Convert(SearchResult);
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sArea = string.Empty;

                    // 동 선택 확인
                    if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.MessageInfo("MMD0004");   //동을 선택해 주세요.
                        return;
                    }
                    else
                    {
                        sArea = cboAreaAll.SelectedValue.ToString();
                    }

                    string sPalletID = string.Empty;
                    sPalletID = txtPalletID.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        Util.MessageInfo("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtWorker.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1842");  //작업자를 선택 하세요.
                        return;
                    }

                    // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                    if (isCellPalletBarcodeUseArea)
                    {
                        string palletBarcodeID = getPalletID(sPalletID);
                        if (!palletBarcodeID.IsNullOrEmpty())
                        {
                            sPalletID = palletBarcodeID;
                        }
                    }
                    
                    Scan_Process(sPalletID, sArea);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {
            try
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

                if (string.IsNullOrWhiteSpace(txtWorker.Text.ToString()))
                {
                    Util.MessageInfo("SFU1842");  //작업자를 선택 하세요.
                    return;
                }

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("PALLETID", typeof(string));

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            DataRow dataRow = dataTable.NewRow();
                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                dataRow["PALLETID"] = cell.Text;
                            }

                            dataTable.Rows.Add(dataRow);
                        }

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            string sPalletID = dataTable.Rows[i]["PALLETID"].ToString();

                            if (Scan_Process(sPalletID, sArea) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool Scan_Process(string sPallet_ID, string sArea)
        {
            try
            {
                // 입고창고 선택 확인
                if (cboLocTo.SelectedIndex < 0 || cboLocTo.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU2069");  //입고창고를 선택하세요.                    
                    return false;
                }

                if (cboLottype.SelectedValue.ToString().Equals("") || cboLottype.SelectedValue.Equals("SELECT"))
                {
                    //LOT 유형을 선택하세요.
                    Util.MessageValidation("SFU4068");
                    return false;
                }

                // Tray 스캔 결과 조회
                DataTable dtScanValue = GetTrayPalletID(sPallet_ID);

                if (dtScanValue != null && dtScanValue.Rows.Count > 0 && !string.IsNullOrEmpty(dtScanValue.Rows[0]["BOXID"].ToString()))
                {
                    sPallet_ID = dtScanValue.Rows[0]["BOXID"].ToString();
                }
                else
                {
                    // SFU3191 잘못된 정보를 스캔하였습니다. Pallet / Tray 에 해당되지 않습니다. 
                    Util.MessageValidation("SFU3191");
                    return false;
                }

                // Pallet에 포장된 Inner Box List 조회
                DataTable dtInnerBoxList = GetInnerBox(sPallet_ID);

                DataTable PRQSTDT = new DataTable();
                PRQSTDT.Columns.Add("LANGID");
                PRQSTDT.Columns.Add("BOX_LIST");

                DataRow dp = PRQSTDT.NewRow();
                dp["LANGID"] = LoginInfo.LANGID;
                dp["BOX_LIST"] = sPallet_ID;
                PRQSTDT.Rows.Add(dp);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", PRQSTDT); // 시생산 cell 조회
                DataTable dnResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_NOT_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", PRQSTDT); // 시생산 아닌 cell 조회

                if (dsResult.Rows.Count > 0) // 시생산 cell이 존재하는 경우
                {
                    // 시생산 cell 있는 경우 popup
                    BOX001_PILOT_DETL popUp = new BOX001_PILOT_DETL();
                    popUp.FrameOperation = this.FrameOperation;

                    if (popUp != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = dsResult;
                        Parameters[1] = dnResult;

                        C1WindowExtension.SetParameters(popUp, Parameters);

                        popUp.Closed += new EventHandler(pilot_wndConfirm_Closed);

                        grdMain.Children.Add(popUp);
                        popUp.BringToFront();
                    }

                    return false;
                }


                // Pallet 상태 체크
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("LOTTYPE", typeof(String));
                RQSTDT.Columns.Add("SHIP_WAIT_YN", typeof(String)); // 2023.02.28 SHIP_WAIT_YN 추가

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPallet_ID;
                dr["AREAID"] = sArea;
                dr["USERID"] = txtWorker.Tag as string;
                dr["LOTTYPE"] = cboLottype.SelectedValue;
                dr["SHIP_WAIT_YN"] = bShipWaitYN ? "Y" : "N"; // 2023.02.28 SHIP_WAIT_YN 추가

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_CHK_SHIP_PALLET_BX", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1905");   //조회된 Data가 없습니다.
                    return false;
                }

                if (dgPackOut.GetRowCount() != 0)
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgPackOut.ItemsSource);

                    if (SearchResult.Rows[0]["TO_SLOC_ID"].ToString() != dtInfo.Rows[0]["TO_SLOC_ID"].ToString())
                    {
                        Util.MessageValidation("SFU3011");  //이전에 스캔한 Pallet 의 출고창고 정보와 다릅니다.
                        return false;
                    }

                    if (dtInfo.Select("BOXID = '" + sPallet_ID + "'").Count() > 0)
                    {
                        Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                        return false;
                    }

                    if (dtInfo.Select("PRODID <> '" + SearchResult.Rows[0]["PRODID"].ToString() + "'").Count() > 0)
                    {
                        Util.MessageValidation("SFU3012");  //다른 제품과 같이 출고할 수 없습니다.
                        return false;
                    }

                    DataTable dtBinding = new DataTable();

                    dtBinding = dtInfo.Copy();

                    for (int i = 0; i < dtInnerBoxList.Rows.Count; i++)
                    {
                        DataRow dr1 = dtBinding.NewRow();
                        dr1["BOXID"] = SearchResult.Rows[0]["BOXID"].ToString();
                        dr1["LOTYNAME"] = SearchResult.Rows[0]["LOTYNAME"].ToString();
                        dr1["CSTID"] = dtInnerBoxList.Rows[i]["TAG_ID"].ToString();
                        dr1["LOTID"] = dtInnerBoxList.Rows[i]["TRAYID"].ToString();
                        dr1["PROJECTNAME"] = SearchResult.Rows[0]["PROJECTNAME"].ToString();
                        dr1["PRODID"] = SearchResult.Rows[0]["PRODID"].ToString();
                        dr1["TOP_PRODID"] = SearchResult.Rows[0]["TOP_PRODID"].ToString();
                        dr1["TOTAL_QTY"] = (Decimal)SearchResult.Rows[0]["TOTAL_QTY"];
                        dr1["INTRANSIT_SLOC_ID"] = SearchResult.Rows[0]["INTRANSIT_SLOC_ID"].ToString();
                        dr1["INTRANSIT_SLOC_NAME"] = SearchResult.Rows[0]["INTRANSIT_SLOC_NAME"].ToString();
                        dr1["SHIPTO_ID"] = SearchResult.Rows[0]["SHIPTO_ID"].ToString();
                        dr1["SHIPTO_NAME"] = SearchResult.Rows[0]["SHIPTO_NAME"].ToString();
                        dr1["PACK_NOTE"] = SearchResult.Rows[0]["PACK_NOTE"].ToString();
                        dr1["EXP_DOM_TYPE_CODE"] = SearchResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString();
                        dr1["EXP_DOM_TYPE_NAME"] = SearchResult.Rows[0]["EXP_DOM_TYPE_NAME"].ToString();
                        dr1["BOXTYPE"] = SearchResult.Rows[0]["BOXTYPE"].ToString();
                        dr1["INNER_BOXTYPE"] = SearchResult.Rows[0]["INNER_BOXTYPE"].ToString();
                        dr1["TO_SLOC_ID"] = SearchResult.Rows[0]["TO_SLOC_ID"].ToString();
                        dr1["CUST_FTP_YN"] = SearchResult.Rows[0]["CUST_FTP_YN"].ToString();
                        dr1["BOXSTAT"] = SearchResult.Rows[0]["BOXSTAT"].ToString();

                        dtBinding.Rows.Add(dr1);
                    }
                    Util.GridSetData(dgPackOut, dtBinding, FrameOperation);
                }
                else
                {

                    DataTable dtBinding = new DataTable();
                    dtBinding.Columns.Add("BOXID");
                    dtBinding.Columns.Add("LOTYNAME");
                    dtBinding.Columns.Add("CSTID");
                    dtBinding.Columns.Add("LOTID");
                    dtBinding.Columns.Add("PROJECTNAME");
                    dtBinding.Columns.Add("PRODID");
                    dtBinding.Columns.Add("TOP_PRODID");
                    dtBinding.Columns.Add("TOTAL_QTY");
                    dtBinding.Columns.Add("INTRANSIT_SLOC_ID");
                    dtBinding.Columns.Add("INTRANSIT_SLOC_NAME");
                    dtBinding.Columns.Add("SHIPTO_ID");
                    dtBinding.Columns.Add("SHIPTO_NAME");
                    dtBinding.Columns.Add("PACK_NOTE");
                    dtBinding.Columns.Add("EXP_DOM_TYPE_CODE");
                    dtBinding.Columns.Add("EXP_DOM_TYPE_NAME");
                    dtBinding.Columns.Add("BOXTYPE");
                    dtBinding.Columns.Add("INNER_BOXTYPE");
                    dtBinding.Columns.Add("TO_SLOC_ID");
                    dtBinding.Columns.Add("CUST_FTP_YN");
                    dtBinding.Columns.Add("BOXSTAT");

                    for (int i = 0; i < dtInnerBoxList.Rows.Count; i++)
                    {
                        DataRow dr1 = dtBinding.NewRow();
                        dr1["BOXID"] = SearchResult.Rows[0]["BOXID"].ToString();
                        dr1["LOTYNAME"] = SearchResult.Rows[0]["LOTYNAME"].ToString();
                        dr1["CSTID"] = dtInnerBoxList.Rows[i]["TAG_ID"].ToString();
                        dr1["LOTID"] = dtInnerBoxList.Rows[i]["TRAYID"].ToString();
                        dr1["PROJECTNAME"] = SearchResult.Rows[0]["PROJECTNAME"].ToString();
                        dr1["PRODID"] = SearchResult.Rows[0]["PRODID"].ToString();
                        dr1["TOP_PRODID"] = SearchResult.Rows[0]["TOP_PRODID"].ToString();
                        dr1["TOTAL_QTY"] = (Decimal)SearchResult.Rows[0]["TOTAL_QTY"];
                        dr1["INTRANSIT_SLOC_ID"] = SearchResult.Rows[0]["INTRANSIT_SLOC_ID"].ToString();
                        dr1["INTRANSIT_SLOC_NAME"] = SearchResult.Rows[0]["INTRANSIT_SLOC_NAME"].ToString();
                        dr1["SHIPTO_ID"] = SearchResult.Rows[0]["SHIPTO_ID"].ToString();
                        dr1["SHIPTO_NAME"] = SearchResult.Rows[0]["SHIPTO_NAME"].ToString();
                        dr1["PACK_NOTE"] = SearchResult.Rows[0]["PACK_NOTE"].ToString();
                        dr1["EXP_DOM_TYPE_CODE"] = SearchResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString();
                        dr1["EXP_DOM_TYPE_NAME"] = SearchResult.Rows[0]["EXP_DOM_TYPE_NAME"].ToString();
                        dr1["BOXTYPE"] = SearchResult.Rows[0]["BOXTYPE"].ToString();
                        dr1["INNER_BOXTYPE"] = SearchResult.Rows[0]["INNER_BOXTYPE"].ToString();
                        dr1["TO_SLOC_ID"] = SearchResult.Rows[0]["TO_SLOC_ID"].ToString();
                        dr1["CUST_FTP_YN"] = SearchResult.Rows[0]["CUST_FTP_YN"].ToString();
                        dr1["BOXSTAT"] = SearchResult.Rows[0]["BOXSTAT"].ToString();

                        dtBinding.Rows.Add(dr1);
                    }

                    Util.GridSetData(dgPackOut, dtBinding, FrameOperation);
                }

                isPalletQty = isPalletQty + 1;
                txtPalletQty.Text = isPalletQty.ToString();

                isTrayQty = isTrayQty + dtInnerBoxList.Rows.Count;//dtScanValue.Rows.Count;
                txtTrayQty.Text = isTrayQty.ToString();

                isCellQty = isCellQty + Convert.ToDouble(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                txtCellQty.Text = isCellQty.ToString();

                txtPalletID.Text = "";
                txtPalletID.Focus();

                // cell 정보 조회 이후 lottype 변경 불가
                cboLottype.IsEnabled = false;

                string[] sColumnName = new string[] { "BOXID", "PROJECTNAME", "TOP_PRODID", "PRODID", "TOTAL_QTY", "SHIPTO_NAME", "PACK_NOTE", "EXP_DOM_TYPE_NAME"};

                _Util.SetDataGridMergeExtensionCol(dgPackOut, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                return true;

            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void pilot_wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_PILOT_DETL pilot_popup = sender as BOX001_PILOT_DETL;

            this.grdMain.Children.Remove(pilot_popup);
        }

        private string SelectPalletStatus(string sPalletID)
        {
            try
            {
                // 기존 Biz name : BR_GETPALLETINFO_PALLETID_LABELCHK_v04
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr[""] = sPalletID;

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count < 0)
                {
                    return "No_Data";
                }
                // 수작업 여부 확인 : MAGLOT 컬럼값이 Y라면 수작업으로 Lot 구성된 것임
                else if (SearchResult.Rows[0]["MAGLOT"].ToString() != "Y")
                {
                    // 수작업이 아닌 경우는 프로세스 상 라벨 발행(PL) 후 포장 출고를 하도록 되어 있으므로 상태값 확인함 20100618 홍광표S
                    // 개별 Lot 구성(박스 포장) / 매거진 Lot 은 라벨 발행을 하지 않고 포장 출고를 하기 때문에 상태(PA) 추가함 20101228 홍광표S
                    // 포장 출고 취소도 다시 포장 출고를 할 수 있기 때문에 상태(RC) 추가함 20101228 홍광표S

                    // PL : 라벨발행완료(PL)
                    // PA : PALLET 구성 확정(라벨 Tag까지) 
                    // RC : 출고대기
                    // PC : 물류입고 취소

                    #region 수작업이 아닌 경우
                    if (SearchResult.Rows[0]["STATUS"].ToString() != "PL" && SearchResult.Rows[0]["STATUS"].ToString() != "PA" &&
                        SearchResult.Rows[0]["STATUS"].ToString() != "RC" && SearchResult.Rows[0]["STATUS"].ToString() != "PC")
                    {
                        // 조회된 Pallet이 라벨 발행 후가 아닌 경우
                        // 출고 대기 상태가 아님.
                        return "Status";
                    }
                    else
                    {
                        string palletIDINDATA = string.Empty;

                        // 스프레드에 등록된 PalletID를 가져오기 위해
                        for (int iCount = 0; iCount <= dgPackOut.Rows.Count - 1; iCount++)
                        {
                            if (dgPackOut.Rows.Count > 0)
                            {
                                palletIDINDATA = palletIDINDATA + Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[iCount].DataItem, "PALLETID") + ",");
                            }
                        }
                        palletIDINDATA = "(" + palletIDINDATA + "'" + sPalletID + "')";

                        // Lot 편차 확인하는 함수 호출
                        // 출하처별로 Lot편차 일수를 조회하여 비교한다
                        Boolean result = SelectLotTerm(palletIDINDATA);

                        if (!result)
                        {
                            return "Term";
                        }

                        // 조회된 Pallet이 라벨 발행한 경우
                        if (dgPackOut.Rows.Count < 0)
                        {
                            // 스프레드에 새로 Row를 추가해야 해서 필요한 변수 ??
                            // 필요 여부 ...확인 필요
                        }
                        else
                        {
                            // 중복 체크
                            for (int i = 0; i < dgPackOut.Rows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "PALLETID")) == SearchResult.Rows[0]["PALLETID"].ToString())
                                {
                                    return "Duplication";
                                }
                            }
                        }


                        dgPackOut.IsReadOnly = false;
                        dgPackOut.BeginNewRow();
                        dgPackOut.EndNewRow(true);
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PALLETID", SearchResult.Rows[0]["PALLETID"].ToString());
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PROD_TYPE", SearchResult.Rows[0]["PROD_TYPE"].ToString());
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "QTY", SearchResult.Rows[0]["QTY"].ToString());
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "STATUS", SearchResult.Rows[0]["STATUS"].ToString());
                        DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "LOT_TYPE", SearchResult.Rows[0]["LOT_TYPE"].ToString());
                        dgPackOut.IsReadOnly = true;

                        isPalletQty = isPalletQty + 1;
                        txtPalletQty.Text = isPalletQty.ToString();

                        isCellQty = isCellQty + Convert.ToInt32(SearchResult.Rows[0]["QTY"].ToString());
                        txtCellQty.Text = isCellQty.ToString();

                        return "OK";
                    }
                    #endregion
                }
                else
                {
                    // 수작업인 경우는 패키지에서 발행한 태그를 그대로 사용할 예정이기 때문에 라벨 발행 여부를 확인하지 않는다
                    #region 수작업인 경우
                    if (SearchResult.Rows[0]["STATUS"].ToString() != "PL" && SearchResult.Rows[0]["STATUS"].ToString() != "PA" &&
                        SearchResult.Rows[0]["STATUS"].ToString() != "RC" && SearchResult.Rows[0]["STATUS"].ToString() != "PC")
                    {
                        // 조회된 Pallet이 라벨 발행 후가 아닌 경우
                        return "Status";
                    }

                    // 스프레드에 어떤 값도 입력이 되어 있지 않다면, new DataColumn을 사용해 DataTable 구조를 만듬.
                    // ?????????????????????????????????????
                    //if (sprPalletOut_Sheet1.Rows.Count <= 0)
                    //{

                    //}

                    // 중복 체크
                    for (int i = 0; i < dgPackOut.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "PALLETID")) == SearchResult.Rows[0]["PALLETID"].ToString())
                        {
                            return "Duplication";
                        }
                    }

                    dgPackOut.IsReadOnly = false;
                    dgPackOut.BeginNewRow();
                    dgPackOut.EndNewRow(true);
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "CHK", true);
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PALLETID", SearchResult.Rows[0]["PALLETID"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PROD_TYPE", SearchResult.Rows[0]["PROD_TYPE"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "QTY", SearchResult.Rows[0]["QTY"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "STATUS", SearchResult.Rows[0]["STATUS"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "LOT_TYPE", SearchResult.Rows[0]["LOT_TYPE"].ToString());
                    dgPackOut.IsReadOnly = true;

                    isPalletQty = isPalletQty + 1;
                    txtPalletQty.Text = isPalletQty.ToString();

                    isCellQty = isCellQty + Convert.ToInt32(SearchResult.Rows[0]["QTY"].ToString());
                    txtCellQty.Text = isCellQty.ToString();

                    return "OK";

                    #endregion

                }

                //return "OK";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        private Boolean SelectLotTerm(string sPallet_ID)
        {
            try
            {
                // 기존 Biz name : BR_CHK_LOT_TERM_v02
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr[""] = sPallet_ID;

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region Event
        private void dgOutHist_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                C1DataGrid dg = ((DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgOutHist.SelectedIndex = idx;

                Search_PalletInfo(idx);
            }
        }

        private void btnPackOut_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
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

                    if (string.IsNullOrWhiteSpace(txtWorker.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1842");  //작업자를 선택 하세요.
                        return;
                    }


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        string palletID = sPasteStrings[i];

                        //if (!string.IsNullOrEmpty(sPasteStrings[i]) && Scan_Process(sPasteStrings[i], sArea) == false)
                        if (!string.IsNullOrEmpty(palletID))
                        {
                            // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                            if (isCellPalletBarcodeUseArea)
                            {
                                string palletBarcodeID = getPalletID(palletID);
                                if (!palletBarcodeID.IsNullOrEmpty())
                                {
                                    palletID = palletBarcodeID;
                                }
                            }

                            if (Scan_Process(palletID, sArea) == false)
                            {
                                break;
                            }
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER window = sender as CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") <= 0)
            {
                Util.AlertInfo("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                return;
            }

            if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") != 1)
            {
                Util.AlertInfo("SFU2004"); //하나의 Pallet만 선택해주십시오.
                return;
            }

            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("OUTER_BOXID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                {
                    DataRow inData = inDataTable.NewRow();
                    inData["OUTER_BOXID"] = Util.NVC(dgOutDetail.GetCell(i, dgOutDetail.Columns["BOXID"].Index).Value);   // "P6A0900083";
                    inData["LANGID"] = LoginInfo.LANGID;
                    inDataTable.Rows.Add(inData);
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_CELL_FCS_MEARS_CP", "INDATA", "OUTDATA_HEADER,OUTDATA_VALUE", (dsResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    dgFCSData.Columns.Clear();

                    Util.SetGridColumnText(dgFCSData, "OUTER_BOXID", null, "PALLETID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "BOXID", null, "BOXID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "TAG_ID", null, "TAG_ID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "SUBLOTID", null, "CELLID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "BOX_PSTN_NO", null, "CELL위치", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);

                    for (int i = 0; i < dsResult.Tables["OUTDATA_HEADER"].Rows.Count; i++)
                    {
                        if (Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]).IndexOf("VALUE") > -1)
                        {
                            string sBinding = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]);
                            string sHeadName = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["MEASR_ITEM_NAME"]);

                            Util.SetGridColumnText(dgFCSData, sBinding, null, sHeadName, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }

                    dgFCSData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA_VALUE"]);

                    new ExcelExporter().Export(dgFCSData);

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboLocFrom_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dtInfo = DataTableConverter.Convert(cboLocFrom.ItemsSource);
            if (dtInfo != null && dtInfo.Select("CBO_CODE <> '" + CommonCombo.ComboStatus.SELECT + "'").Count() < 2)
            {
                cboLocFrom.SelectedValue = dtInfo.Rows[1][cboLocFrom.SelectedValuePath];
                cboLocFrom.SelectedIndex = 1;
            }

            //C1ComboBox[] cboFromParent = { cboAreaAll };
            //C1ComboBox[] cboToChild2 = { cboLocTo };
            //_combo.SetCombo(cboLocFrom, CommonCombo.ComboStatus.SELECT, cbParent: cboFromParent, cbChild: cboToChild2, sFilter: sFilter2);

            ////C1ComboBox[] cboFromParent2 = { cboLocFrom };
            //C1ComboBox[] cboFromChild = { cboComp };
            //_comboF.SetCombo(cboLocTo, CommonCombo_Form.ComboStatus.SELECT, cbParent: cboFromParent, cbChild: cboFromChild);

            //C1ComboBox[] cboCompParent = { cboLocTo, cboAreaAll };
            //_comboF.SetCombo(cboComp, CommonCombo_Form.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

        }

        private void cboAreaAll2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboAreaAll2.SelectedValue);

            if (sTemp == "")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }
            String[] sFilter = { sAREAID };    // Area

            _comboF.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");

        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboAreaAll2.SelectedValue);

            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine });

            if (cboLine.SelectedValue != null)
            {
                string[] sFilter3 = { sSHOPID, (string)cboLine.SelectedValue, sAREAID };
                _combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "SHIPTO_CP");
            }

        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgOutHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)

                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutHist.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutHist.Rows[i].DataItem, "CHK", true);
                }
            }

            SetSelectedQty();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutHist.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutHist.Rows[i].DataItem, "CHK", false);
                }
            }

            SetSelectedQty();
        }


        private void dgOutHist_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgOutHist.CurrentRow == null || dgOutHist.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgOutHist.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgOutHist.GetCell(dgOutHist.CurrentRow.Index, dgOutHist.Columns["CHK"].Index).Value);

                    //초기화
                    dgOutDetail.ItemsSource = null;

                    if (chkValue == "0" || chkValue == "False")
                    {
                        DataTableConverter.SetValue(dgOutHist.Rows[dgOutHist.CurrentRow.Index].DataItem, "CHK", true);
                        Search_PalletInfo(dgOutHist.CurrentRow.Index);

                    }
                    else
                        DataTableConverter.SetValue(dgOutHist.Rows[dgOutHist.CurrentRow.Index].DataItem, "CHK", false);


                    SetSelectedQty();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #region 2020 11. 13 오화백 추가 :  일자별 출하 조회 

        /// <summary>
        /// 동 정보 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboAreaAll2_daily_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (DailySearch.IsVisible == true)
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetEqsgCombo();
                }));
            }
        }



        /// <summary>
        /// 라인정보 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEqsgShot_Daily_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetModelLotCombo();
            }));
        }


        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Daily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                string sStart_date = dtpDateFrom_Daily.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo_Daily.SelectedDateTime.ToString("yyyyMMdd");

                //기존 컬럼 삭제
                if (dgList.Columns.Count > 1)
                {
                    for (int i = dgList.Columns.Count; i-- > 1;) //컬럼수
                        dgList.Columns.RemoveAt(i);
                }
                // Line 콤보박스 선택 체크
                if (cboEqsgShot_Daily.SelectedItemsToString == "SELECT" || cboEqsgShot_Daily.SelectedItemsToString == string.Empty)
                {
                    // 라인을 선택하세요.
                    Util.MessageInfo("SFU1223");
                    return;
                }
                // ModelLot 콤보박스 선택 체크
                if (cboModelLot_daily.SelectedItemsToString == "SELECT" || cboModelLot_daily.SelectedItemsToString == string.Empty)
                {
                    // 모델을 선택해주십시오.
                    Util.MessageInfo("SFU1257");
                    return;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("FROM_DATE", typeof(string));
                IndataTable.Columns.Add("TO_DATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("MDLLOT_ID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = cboAreaAll2_daily.SelectedValue.ToString();
                Indata["FROM_DATE"] = sStart_date;
                Indata["TO_DATE"] = sEnd_date;
                Indata["EQSGID"] = cboEqsgShot_Daily.SelectedItemsToString ?? null;
                Indata["MDLLOT_ID"] = cboModelLot_daily.SelectedItemsToString ?? null;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DAY_SHIP_CELL_QTY_TITLE", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {

                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                        return;

                    // 동적 컬럼 셋팅
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            List<string> sHeader = new List<string>() { result.Rows[i]["PRJT_NAME"].ToString(), result.Rows[i]["FORM_LINE_ID"].ToString() + "LINE" };

                            Util.SetGridColumnText(dgList, result.Rows[i]["BIND_DATA"].ToString(), sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);

                        }

                        DataTable BindTable = new DataTable();
                        BindTable.Columns.Add("CALDATE", typeof(string));

                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            BindTable.Columns.Add(result.Rows[i]["BIND_DATA"].ToString(), typeof(string));
                        }


                        DataTable GetData = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DAY_SHIP_CELL_QTY", "INDATA", "OUTDATA", IndataTable);

                        //날짜만 바인딩
                        for (int i = 0; i < GetData.Rows.Count; i++)
                        {
                            DataRow BindData = BindTable.NewRow();
                            BindData["CALDATE"] = GetData.Rows[i]["CALDATE"].ToString();
                            BindTable.Rows.Add(BindData);
                        }

                        //중복된 날짜 제외
                        DataView view = BindTable.DefaultView;
                        DataTable distinctTable = view.ToTable(true, new string[] { "CALDATE" });


                        //BindTable 내용 삭제 : 컬럼이 동적 생성이어어서 특정컬럼명을 명시하지 못해서 기존 데이터테이블의 내용을 삭제하고  중복제거한 데이터 다시 바인딩
                        BindTable.AcceptChanges();
                        foreach (DataRow row in BindTable.Rows)
                        {
                            row.Delete();

                        }
                        BindTable.AcceptChanges();

                        //날짜만 다시 바인딩
                        for (int i = 0; i < distinctTable.Rows.Count; i++)
                        {
                            DataRow BindData = BindTable.NewRow();
                            BindData["CALDATE"] = distinctTable.Rows[i]["CALDATE"].ToString();
                            BindTable.Rows.Add(BindData);
                        }


                        //동적으로 설정된 컬럼에 맞게 데이터 바인딩
                        for (int i = 0; i < GetData.Rows.Count; i++)
                        {
                            for (int j = 0; j < BindTable.Rows.Count; j++)
                            {
                                if (GetData.Rows[i]["CALDATE"].ToString() == BindTable.Rows[j]["CALDATE"].ToString())
                                {
                                    BindTable.Rows[j][GetData.Rows[i]["BIND_DATA"].ToString()] = GetData.Rows[i]["LINE_TOTAL_QTY"].ToString();
                                }
                            }

                        }

                        DataRow drTotal = BindTable.NewRow();
                        drTotal["CALDATE"] = ObjectDic.Instance.GetObjectName("합계");
                        BindTable.Rows.InsertAt(drTotal, BindTable.Rows.Count);

                        //합계 설정
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            for (int j = 0; j < BindTable.Columns.Count; j++)
                            {
                                if (BindTable.Columns[j].ColumnName == result.Rows[i]["BIND_DATA"].ToString())
                                {
                                    BindTable.Rows[BindTable.Rows.Count - 1][BindTable.Columns[j].ColumnName] = result.Rows[i]["LINE_TOTAL_QTY"].ToString();
                                }
                            }

                        }
                        Util.GridSetData(dgList, BindTable, FrameOperation, true);


                    }
                    else
                    {
                        Util.gridClear(dgList);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }


        /// <summary>
        ///  스프레드 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CALDATE")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
                if (!string.Equals(e.Cell.Column.Name, "CALDATE"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }

            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        /// <summary>
        /// 스프레드 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Column.Name.Equals("CALDATE")) return;
                if (string.IsNullOrEmpty(cell.Text)) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                int seleted_row = dgList.CurrentRow.Index;
                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sCaldate = string.Empty;

                if (DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "CALDATE").ToString() != ObjectDic.Instance.GetObjectName("합계"))
                {
                    sCaldate = Convert.ToDateTime(DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "CALDATE")).ToString("yyyyMMdd");
                }
                else
                {
                    sStart_date = dtpDateFrom_Daily.SelectedDateTime.ToString("yyyyMMdd");
                    sEnd_date = dtpDateTo_Daily.SelectedDateTime.ToString("yyyyMMdd");
                }


                //Column Name에서 모델랏 정보와  라인정보 추출
                string sTemp = Util.NVC(cell.Column.Name);
                string sEqsgid = string.Empty;
                string sModelLot = string.Empty;

                if (sTemp == "")
                {
                    sEqsgid = "";
                    sModelLot = "";
                }
                else
                {
                    string[] sArry = sTemp.Split('_');
                    sModelLot = sArry[0];
                    sEqsgid = sArry[2];

                }


                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "CALDATE").ToString() == ObjectDic.Instance.GetObjectName("합계"))
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }
                else
                {
                    dr["FROM_DATE"] = sCaldate;
                    dr["TO_DATE"] = sCaldate;
                }
                dr["RCV_ISS_STAT_CODE"] = "SHIPPING";
                dr["PACK_WRK_TYPE_CODE"] = null;
                dr["FROM_AREAID"] = string.IsNullOrWhiteSpace(cboAreaAll2_daily.SelectedValue.ToString()) ? null : cboAreaAll2_daily.SelectedValue.ToString();
                dr["BOXID"] = null;
                dr["RCV_ISS_ID"] = null;
                dr["EQSGID"] = sEqsgid;
                dr["MDLLOT_ID"] = sModelLot;
                dr["SHIPTO_ID"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PALLET_HIST", "INDATA", "OUTDATA", RQSTDT);

                OutPutHistory.IsSelected = true;
                Util.gridClear(dgOutHist);
                Util.gridClear(dgOutDetail);

                //dgOutHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgOutHist, SearchResult, FrameOperation, true);

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);

                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["TOTALQTY"].Index).Value);
                }

                txtPalletQty_Total.Value = lSumPallet;
                txtCellQty_Total.Value = lSumCell;



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// 날짜 로드시 7일 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpDateFrom_Daily_Loaded(object sender, RoutedEventArgs e)
        {
            //Tab안에 있어서 Tab이 Loaded될 때마다 오늘 날짜로 초기화 되어서 따로 이벤트 활용하여 초기값 설정.
            LGCDatePicker datePicker = sender as LGCDatePicker;

            if (datePicker == null)
            {
                return;
            }

            datePicker.SelectedDateTime = DateTime.Now; //해당 주의 월요일
            datePicker.Loaded -= dtpDateFrom_Daily_Loaded;
        }
        /// <summary>
        ///  날짜 앞뒤 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpDateFrom_Daily_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Daily.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Daily.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Daily_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Daily.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Daily.SelectedDateTime;
                return;
            }
        }

        private void BtnNGCellCheck_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void BtnNGCellCheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgPackOut.GetRowCount() > 0)
            {
                BOX001_011_OCV2_NG_CELL_LIST wndConfirm = new BOX001_011_OCV2_NG_CELL_LIST();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {

                    DataSet indataSet = new DataSet();
                    DataTable inPallet = indataSet.Tables.Add("RQSTDT");
                    inPallet.Columns.Add("OUTER_BOXID", typeof(string));

                    for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                    {
                        DataRow row2 = inPallet.NewRow();
                        row2["OUTER_BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                        indataSet.Tables["RQSTDT"].Rows.Add(row2);
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = inPallet;
                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            else
            {
                Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                return;
            }
        }


        #endregion

        #endregion

        private void cboEqsgShot_Daily_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetModelLotCombo();
        }

        private void SetComboBox(C1ComboBox cbo)
        {
            DataSet dsData = new DataSet();
            DataTable dtData = dsData.Tables.Add("ALL");
            dtData.Columns.Add("CBO_NAME", typeof(string));
            dtData.Columns.Add("CBO_CODE", typeof(string));

            DataRow drnewrow = dtData.NewRow();

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "Y";
            drnewrow["CBO_CODE"] = "Y";
            dtData.Rows.Add(drnewrow);

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "N";
            drnewrow["CBO_CODE"] = "N";
            dtData.Rows.Add(drnewrow);

            cbo.ItemsSource = DataTableConverter.Convert(dtData);

            cbo.SelectedIndex = 0;
        }

        private void SetComboBox2(C1ComboBox cbo)
        {
            DataSet dsData = new DataSet();
            DataTable dtData = dsData.Tables.Add("ALL");
            dtData.Columns.Add("CBO_NAME", typeof(string));
            dtData.Columns.Add("CBO_CODE", typeof(string));

            DataRow drnewrow = dtData.NewRow();

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("양산");
            drnewrow["CBO_CODE"] = "P";
            dtData.Rows.Add(drnewrow);

            cbo.ItemsSource = DataTableConverter.Convert(dtData);

            cbo.SelectedIndex = 0;
        }

        //private void chkNoInsp_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (cboLocTo == null)
        //        return;

        //    // Grid에 데이터가 존재한다면
        //    if (dgPackOut != null && dgPackOut.GetRowCount() > 0)
        //    {
        //        // 작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
        //        Util.MessageConfirm("FM_ME_0203", (result) =>
        //        {
        //            if (result == MessageBoxResult.OK)
        //            {
        //                Util.gridClear(dgPackOut);

        //                SetLocComboBox(cboLocTo);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        SetLocComboBox(cboLocTo);
        //    }

        //}

        //private void chkNoInsp_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (cboLocTo == null)
        //        return;

        //    // Grid에 데이터가 존재한다면
        //    if (dgPackOut != null && dgPackOut.GetRowCount() > 0)
        //    {
        //        // 작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
        //        Util.MessageConfirm("FM_ME_0203", (result) =>
        //        {
        //            if (result == MessageBoxResult.OK)
        //            {
        //                Util.gridClear(dgPackOut);

        //                initCombo();
        //            }
        //        });
        //    }
        //    else
        //    {
        //        initCombo();
        //    }
        //}

        //private void SetLocComboBox(C1ComboBox cbo)
        //{
        //    const string bizRuleName = "DA_BAS_SEL_TO_NO_INSP_SLOC_BY_AREA";
        //    string[] arrColumn = { "LANGID", "AREAID", "SHIP_TYPE_CODE" };
        //    string[] arrCondition = { LoginInfo.LANGID, cboAreaAll.SelectedValue.ToString(), "CELL" };
        //    string selectedValueText = cbo.SelectedValuePath;
        //    string displayMemberText = cbo.DisplayMemberPath;

        //    CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
        //}


        /// <summary>
        /// 고정식 RFID 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetRFIDMngtArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PALLET_RFID_MNGT_AREA";
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
        /// <summary>
        /// 2023.09.15 이제섭 ESNA 특화 기능 반영
        /// OCV2 예외 체크여부
        /// </summary>
        /// <returns></returns>
        private bool GetOCV2ExceptFlag()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_OCV2_MEASR_SHIP_EXCEPT_FLAG";

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// 입고창고 미판정/양품 여부 체크
        /// </summary>
        /// <returns></returns>
        private bool ChkToSloc()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("SLOCID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["SLOCID"] = cboLocTo.SelectedValue.ToString();
            dr["AREAID"] = cboAreaAll.SelectedValue.ToString();

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SLOC_CHECK_BX", "RQSTDT", "RSLTDT", inTable);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                // 미판정 창고 이면
                if (dtRslt.Rows[0]["CHECK_TYPE"].ToString() == "NO_INSP_SLOC")
                {
                    return true;
                }
                // 양품 창고 이면
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        private bool bComboEvent = true;

        private void cboLocTo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLocTo == null)
            {
                return;
            }
            
            if (!bComboEvent)
            {
                return;
            }
            // 입고창고 미지정 시, Return
            if (cboLocTo.SelectedValue.ToString() == "SELECT" || cboLocTo.SelectedIndex < 0)
            {
                return;
            }
            else
            {
                // Grid에 이미 조회된 데이터 존재 시 초기화 후 진행
                if (dgPackOut.GetRowCount() > 0)
                {
                    // FM_ME_0203 : 작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
                    Util.MessageConfirm("FM_ME_0203", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            // Grid 초기화
                            Util.gridClear(dgPackOut);
                            //변수 초기화
                            isPalletQty = 0;
                            isCellQty = 0;
                            // TextBox 초기화
                            txtPalletQty.Text = "0";
                            txtCellQty.Text = "0";
                            txtPalletID.Text = "";
                            txtPalletID.Focus();

                            // 미판정 여부 변수 설정
                            bShipWaitYN = ChkToSloc();
                        }
                        else
                        {
                            bComboEvent = false;
                            cboLocTo.SelectedValue = e.OldValue;
                            bComboEvent = true;
                            return;
                        }
                    });
                }
                else
                {
                    // 미판정 여부 변수 설정
                    bShipWaitYN = ChkToSloc();
                }
            }
        }
        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sBoxID = txtBoxID.Text.Trim();

                if (sBoxID != string.Empty)
                {
                    Out_Hist();
                }
            }
        }

        private void txtBoxID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    string sBoxIDMulti = string.Empty;

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        string sBoxID = sPasteStrings[i];

                        // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                        if (isCellPalletBarcodeUseArea)
                        {
                            string palletBarcodeID = getPalletID(sBoxID);
                            if (!palletBarcodeID.IsNullOrEmpty())
                            {
                                sBoxID = palletBarcodeID;
                            }
                        }

                        sBoxIDMulti += ',' + sBoxID;
                    }

                    try
                    {


                        // 조회 비즈 생성

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(String));
                        RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(String));
                        RQSTDT.Columns.Add("BOXID", typeof(String));
                        RQSTDT.Columns.Add("FROM_AREAID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["RCV_ISS_STAT_CODE"] = "SHIPPING";
                        dr["BOXID"] = sBoxIDMulti;
                        dr["FROM_AREAID"] = string.IsNullOrWhiteSpace(sAREAID) ? null : sAREAID;

                        RQSTDT.Rows.Add(dr);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIP_PALLET_HIST_BX", "INDATA", "OUTDATA", RQSTDT);
                        Util.gridClear(dgOutHist);
                        Util.GridSetData(dgOutHist, SearchResult, FrameOperation, true);


                        dgOutHist.MergingCells -= dgOutHist_MergingCells;
                        dgOutHist.MergingCells += dgOutHist_MergingCells;

                        int lSumPallet = 0;
                        int lSumCell = 0;

                        for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
                        {
                            lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);

                            lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["TOTALQTY"].Index).Value);
                        }

                        txtPalletQty_Total.Value = lSumPallet;
                        txtCellQty_Total.Value = lSumCell;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Scan Value (Tray/TrayLot) 조회 
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        private DataTable GetTrayPalletID(string sValue)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SCAN_VALUE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_VALUE"] = sValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_PACKING_TRAY_INFO_BX", "INDATA", "OUTCST", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    return dtRslt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        /// <summary>
        /// Pallet 에 포장된 Inner Box 조회
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        private DataTable GetInnerBox(string sValue)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    return dtRslt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
    }
}
