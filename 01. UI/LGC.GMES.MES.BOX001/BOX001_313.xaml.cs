/*************************************************************************************
 Created Date : 2021.09.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.29  DEVELOPER : Initial Created.
  2022.06.29  안유수S   C20220628-000695 포장출고-출고이력조회화면의 'cell수' 출력 오류 수정 요청건
  2023.06.22  박나연    조회 조건에 lottype 추가하여 해당 LOTTYPE의 pallet만 조회 및 출고할 수 있도록 추가 및 컬럼에 lottype 추가
  2023.06.28  홍석원    Pallet Barcode를 사용하는 Area인 경우 Pallet Barcode ID를 이용하여 조회 할 수 있도록 수정 
  2023.07.20  홍석원    반제품 전환 대응 수정
  2023.08.17  홍석원    팔레트/출고ID 입력 후 조회시 입력된 값을 대문자로 변환하여 비교할 수 있도록 수정.
  2024.07.23  최석준    사외반품Cell 포함여부 추가 (2025년 적용예정, 수정시 연락부탁드립니다)
****************************************************************************/

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
    public partial class BOX001_313 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();
        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sAREAID_cfg = string.Empty;
        string sSHOPID_cfg = string.Empty;

        private int isPalletQty = 0;
        private double isCellQty = 0;

        // 출고 LotID 로 조회하여, 처음으로 조회되는 조립Lot 저장하기 위한 변수
        private string isFirstLotID = string.Empty;

        // 출고 LotID 로 조회하여, 마지막으로 조회되는 조립Lot 저장하기 위한 변수
        private string isLastLotID = string.Empty;

        // RFID 사용 여부
        bool bRFID_Use = false;

        // 활성화 GMES 여부
        bool bForm = false;

        // Pallet Barcode 사용 여부를 저장하기 위한 변수
        bool isCellPalletBarcodeUseArea = false;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        #region Declaration & Constructor 
        public BOX001_313()
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
            //TabVisible();
            DaileySearch.IsSelected = true;

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPackOut);
            listAuth.Add(btnCancel);
            listAuth.Add(btnShippingConfirm);
            listAuth.Add(btnShippingWaitCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            ChkPackOutConfigProcess();

            SetEvent();

            Task<bool> task = WaitCallback();
            task.ContinueWith(_ =>
            {
                C1TabControl.SelectedIndex = 0;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            //C1TabControl.SelectedIndex = 0;            

            // Pallet Barcode를 사용하는 Area인지 확인
            isCellPalletBarcodeUseArea = checkCellPalletBarcodeUseArea();

            // Pallet Barcode를 사용하는 Area인 경우 PalletBarcodeID 컬럼을 보여주기
            if (isCellPalletBarcodeUseArea)
            {
                dgPackOut.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgOutDetail.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
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
            //dtpDateFrom.Text = System.DateTime.Now.ToString("yyyy-MM-dd"); 
            //dtpDateTo.Text = System.DateTime.Now.ToString("yyyy-MM-dd"); 

            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            //C20210719-000519 OCV2 측정값 NG List button 추가

            if (LoginInfo.CFG_AREA_ID == "A3")
            {
                btnNGCellCheck.Visibility = Visibility.Visible;
            }
            else
            {
                btnNGCellCheck.Visibility = Visibility.Collapsed;
            }

            // 공통코드 조회하여 콤보박스 Visibility Set
            if (UseCommoncodePlant())
            {
                cboAutoYN.Visibility = Visibility.Visible;
                tbAutoYN.Visibility = Visibility.Visible;

                dgPackOut.Columns["TAG_ID"].Visibility = Visibility.Visible;

                bRFID_Use = true;
            }
            else
            {
                bRFID_Use = false;
            }

            // 활성화 GMES일 경우, 활성화 GMES 여부 true 설정
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                bForm = true;
            }

            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgPackOut.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;
            }

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "PACK_WRK_TYPE_CODE" };
            String[] sFilter2 = { Process.CELL_BOXING };

            C1ComboBox[] cboToChild = { cboLocFrom };
            _combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.NONE, cbChild: cboToChild, sCase: "ALLAREA");

            C1ComboBox[] cboFromParent = { cboAreaAll };
            //C1ComboBox[] cboToChild2 = { cboLocTo };
            _combo.SetCombo(cboLocFrom, CommonCombo.ComboStatus.SELECT, cbParent: cboFromParent, sFilter: sFilter2);

            //C1ComboBox[] cboFromParent2 = { cboLocFrom };
            C1ComboBox[] cboFromChild = { cboComp };
            _combo.SetCombo(cboLocTo, CommonCombo.ComboStatus.SELECT, cbParent: cboFromParent, cbChild: cboFromChild);

            C1ComboBox[] cboCompParent = { cboLocTo, cboAreaAll };
            _combo.SetCombo(cboComp, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            //LOTTYPE 세팅
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.SELECT, sCase: "LOTTYPE");
            //DEFAULT LOTTYPE 양산('P') 세팅
            cboLottype.SelectedValue = "P";

            _combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

            _combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            //2020 11 13 오화백 : 일자별 출하 조회  동정보 조회
            _combo.SetCombo(cboAreaAll2_daily, CommonCombo.ComboStatus.SELECT, sCase: "AREA");


            _combo.SetCombo(cboArea_cfg, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboType_cfg, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
            dtpDateFrom_cfg.SelectedDateTime = DateTime.Now;
            dtpDateTo_cfg.SelectedDateTime = DateTime.Now;

            SetComboBox(cboAutoYN);

            txtPalletID.Focus();
            txtPalletID.SelectAll();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom_cfg.SelectedDataTimeChanged += dtpDateFrom_cfg_SelectedDataTimeChanged;
            dtpDateTo_cfg.SelectedDataTimeChanged += dtpDateTo_cfg_SelectedDataTimeChanged;

            dtpDateFrom_Daily.SelectedDataTimeChanged += dtpDateFrom_Daily_SelectedDataTimeChanged;
            dtpDateTo_Daily.SelectedDataTimeChanged += dtpDateTo_Daily_SelectedDataTimeChanged;

            cboAreaAll2_daily.SelectedValueChanged += cboAreaAll2_daily_SelectedValueChanged;
            if (cboAreaAll2_daily.Items.Count > 0) cboAreaAll2_daily_SelectedValueChanged(null, null);
        }
        #endregion

        #region Mehod
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

        private void dtpDateFrom_cfg_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_cfg.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_cfg.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_cfg_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_cfg.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_cfg.SelectedDateTime;
                return;
            }
        }

        private void Initialize_Out()
        {
            Util.gridClear(dgPackOut);

            isPalletQty = 0;
            isCellQty = 0;

            txtPalletQty.Text = "0";
            txtCellQty.Text = "0";
            txtPalletID.Text = "";
            txtPalletID.Focus();

            cboLocTo.SelectedIndex = 0;
            cboLocFrom.SelectedIndex = 0;
            cboAutoYN.SelectedIndex = 0;

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

            DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

            Util.gridClear(dgOutDetail);
            //    dgOutDetail.ItemsSource = DataTableConverter.Convert(DetailResult);
            Util.GridSetData(dgOutDetail, DetailResult, FrameOperation, true);
        }



        private void SetSelectedQty()
        {
            int lSumPallet = 0;
            int lSumCell = 0;

            for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
            {
                //if (_Util.GetDataGridCheckValue(dgOutHist, "CHK", lsCount))\
                if (DataTableConverter.GetValue(dgOutHist.Rows[lsCount].DataItem, "CHK").ToString() == "1" || DataTableConverter.GetValue(dgOutHist.Rows[lsCount].DataItem, "CHK").ToString() == "True")
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);
                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["LINE_TOTAL_QTY"].Index).Value);
                }
            }

            txtPalletQty_Search.Value = lSumPallet;
            txtCellQty_Search.Value = lSumCell;
        }

        /// <summary>
        /// 고정식 RFID 사용  Plant 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodePlant()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLT_RFID_SHOP";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

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
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
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

        private void SetComboBox(C1ComboBox cbo)
        {
            DataSet dsData = new DataSet();
            DataTable dtData = dsData.Tables.Add("ALL");
            dtData.Columns.Add("CBO_NAME", typeof(string));
            dtData.Columns.Add("CBO_CODE", typeof(string));

            DataRow drnewrow = dtData.NewRow();

            //drnewrow = dtData.NewRow();
            //drnewrow["CBO_NAME"] = "-SELECT-";
            //drnewrow["CBO_CODE"] = "";
            //dtData.Rows.Add(drnewrow);

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

        #region 2020 11 13  오화백  : 일자별 출하 조회 (폴란드 전용) 
        /// <summary>
        /// 라인 정보 조회
        /// </summary>
        private void SetEqsgCombo()
        {
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

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

        /// 공통코드에 등록된 동에 따라 일자별출하조회 탭을 보여줌
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
                    DaileySearch.Visibility = Visibility.Visible;
                }
                else
                {
                    DaileySearch.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

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
                    Util.gridClear(CslyPackOutRspnList);

                    if (OutArea.RowDefinitions[3].Height.Value == 0) return;
                    OutArea.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                    btnSearch_PackOutCsly.Visibility = Visibility.Collapsed;
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
                        this.FrameOperation.OpenMenu("SFU010060060", true, sParam);
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

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

                    drCrad["Prod_ID"] = drChk[0]["PRODID"].ToString();         //제품ID

                    drCrad["User"] = drChk[0]["INSUSERNAME"].ToString();         //작업자

                    drCrad["Out_WH"] = drChk[0]["FROM_SLOC_ID"].ToString();         //출고창고

                    drCrad["In_WH"] = drChk[0]["TO_SLOC_ID"].ToString();         //입고창고

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
                        drCrad2["Prod_ID"] = drChk[0]["PRODID"].ToString();         //제품ID                              2
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Packing_Tag wndPopup = sender as LGC.GMES.MES.BOX001.Report_Packing_Tag;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
                grdMain.Children.Remove(wndPopup);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3254"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.MessageValidation("SFU3254");
                    return;
                }
            }
            else
            {
                //출고 이력을 조회 하신 후 버튼을 클릭해주십시오.
                Util.MessageValidation("SFU3255");
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3255"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

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
                //RQSTDT.Columns.Add("BOXTYPE", typeof(String));
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

                if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1).ToUpper() == "P")
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
                else if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1).ToUpper() != "P")
                {
                    dr["RCV_ISS_ID"] = txtBoxID.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                    //dr["BOXTYPE"] = Util.GetCondition(cboType, "선택하세요.");                    
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                    dr["SHIPTO_ID"] = Util.NVC(cboShipTo.SelectedValue) == "" ? null : Util.NVC(cboShipTo.SelectedValue); ;
                }
                dr["FROM_AREAID"] = string.IsNullOrWhiteSpace(sAREAID) ? null : sAREAID;
                if (!string.IsNullOrWhiteSpace(cboLine.SelectedValue.ToString())) dr["EQSGID"] = cboLine.SelectedValue;
                //if (dr["BOXTYPE"].Equals("") ) return;

                RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                //오화백 : DA를 BR로 변경
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PALLET_HIST", "INDATA", "OUTDATA", RQSTDT);
                Util.gridClear(dgOutHist);
                //dgOutHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgOutHist, SearchResult, FrameOperation, true);


                dgOutHist.MergingCells -= dgOutHist_MergingCells;
                dgOutHist.MergingCells += dgOutHist_MergingCells;

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgOutHist.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["PALLETQTY"].Index).Value);

                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["LINE_TOTAL_QTY"].Index).Value);
                }

                txtPalletQty_Total.Value = lSumPallet;
                txtCellQty_Total.Value = lSumCell;

                ChkPackOutConfigProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                // 자동창고 출고 여부 선택 확인
                if (bRFID_Use)
                {
                    if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                    {
                        // 자동창고 출고 여부를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                        return;
                    }
                }

                DataTable dtInfo = DataTableConverter.Convert(dgPackOut.ItemsSource);
                if (dtInfo.Select("CUST_FTP_YN = 'Y' AND SHIPTO_ID <> '" + sComp + "'").Count() > 0)
                {
                    //FTP 전송하는 고객사입니다. 출하처를 확인해주세요.
                    Util.MessageValidation("SFU3408");
                    return;
                }

                // 2021-10-20 염규범 S
                // 셀 공급 프로젝트 건의로 인한 추가
                if (CslyPackOutRspnList.GetRowCount() > 0 && !(_Util.GetDataGridFirstRowIndexWithTopRow(CslyPackOutRspnList, "CHK", true) == -1))
                {
                    if (!string.IsNullOrWhiteSpace(cboComp.SelectedValue.ToString()))
                    {
                        DataTable dt = DataTableConverter.Convert(CslyPackOutRspnList.ItemsSource).AsEnumerable()
                                                                                          .Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();

                        if (cboComp.SelectedValue.ToString().Equals("SELECT"))
                        {
                            Util.MessageInfo("SFU4096");
                            return;
                        }

                        string strAreaId = CslySelAeaId(cboComp.SelectedValue.ToString());

                        if (dt.AsEnumerable().Where(x => x.Field<string>("AREAID") != strAreaId).ToList().Count > 0)
                        {
                            Util.MessageInfo("SFU8417");
                            return;
                        }
                    }
                    //return;
                }



                if (dgPackOut.GetRowCount() > 0)
                {
                    BOX001_011_CONFIRM wndConfirm = new BOX001_011_CONFIRM();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cboComp.Text;
                        Parameters[1] = cboLocTo.Text;
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                    // 자동창고 출고 여부 선택 확인
                    if (bRFID_Use)
                    {
                        if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                        {
                            // 자동창고 출고 여부를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                            return;
                        }
                    }

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

                        string sBizRule = string.Empty;

                        if (bForm)
                        {
                            sBizRule = "BR_SET_SHIP_CELL";
                        }
                        else
                        {
                            sBizRule = "BR_PRD_REG_SHIP_CELL";
                        }

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
                        inData.Columns.Add("RFID_USE_FLAG", typeof(string)); // 고정식 RFID 사용여부
                        inData.Columns.Add("AUTO_WH_SHIP_FLAG", typeof(string)); // 자동창고 출하여부
                        inData.Columns.Add("SHIP_WAIT_YN", typeof(string));
                        inData.Columns.Add("AUTO_WH_ISS_FLAG", typeof(string));

                        DataRow row = inData.NewRow();
                        row["FROM_AREAID"] = sArea;
                        row["FROM_SLOC_ID"] = sLocFrom; // Util.GetCondition(cboLocFrom, "출고창고를선택하세요.");//출고창고
                        row["TO_AREAID"] = sTo_Areaid;          //입고 Area
                        row["TO_SLOC_ID"] = sLocTo;     // Util.GetCondition(cboLocTo, "입고창고를선택하세요.");//입고저장위치
                        row["ISS_QTY"] = isCellQty;     //출고수량
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = sComp;       // Util.GetCondition(cboComp, "출하처를선택하세요.");//출하처
                        row["NOTE"] = "";
                        //row["USERID"] = txtUserID.Text;
                        row["USERID"] = txtWorker.Tag as string;
                        row["RFID_USE_FLAG"] = bRFID_Use ? "Y" : "N";
                        row["AUTO_WH_SHIP_FLAG"] = bRFID_Use ? cboAutoYN.SelectedValue.ToString() : "N";
                        row["SHIP_WAIT_YN"] = "N";
                        row["AUTO_WH_ISS_FLAG"] = "N";

                        if (row["FROM_SLOC_ID"].Equals("") || row["TO_SLOC_ID"].Equals("") || row["SHIPTO_ID"].Equals("")) return;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inPallet = indataSet.Tables.Add("INPALLET");
                        inPallet.Columns.Add("BOXID", typeof(string));
                        inPallet.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            DataRow row2 = inPallet.NewRow();
                            row2["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                            row2["OWMS_BOX_TYPE_CODE"] = "AC";

                            indataSet.Tables["INPALLET"].Rows.Add(row2);
                        }


                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_CELL", "INDATA,INPALLET", "OUTDATA", indataSet);

                        new ClientProxy().ExecuteService_Multi(sBizRule, "INDATA,INPALLET,INRSPN", "OUTDATA", (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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

                            // 셀공급 프로젝트건으로 인한 추가의 건,
                            // 염규범 선임
                            if (CslyPackOutRspnList.GetRowCount() > 0)
                            {
                                if (DataTableConverter.Convert(CslyPackOutRspnList.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHK") == "True").Count() > 0)
                                {

                                    DataTable dt = DataTableConverter.Convert(CslyPackOutRspnList.ItemsSource).AsEnumerable()
                                                                                                     .Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();

                                    if (dt.Rows.Count > 0)
                                    {
                                        /*
                                        DataSet inCslyDataSet = new DataSet();

                                        //RCV_ISS_ID 입력
                                        DataTable inDataCsly = inCslyDataSet.Tables.Add("INDATA");
                                        inDataCsly.Columns.Add("RCV_ISS_ID", typeof(string));
                                        inDataCsly.Columns.Add("INSUSER", typeof(string));
                                        inDataCsly.Columns.Add("UPDUSER", typeof(string));
                                        inDataCsly.Columns.Add("FROM_AREAID", typeof(string));

                                        DataRow rowCsly = inDataCsly.NewRow();
                                        rowCsly["RCV_ISS_ID"] = sOut_ID;
                                        rowCsly["INSUSER"] = txtWorker.Tag as string;
                                        rowCsly["UPDUSER"] = txtWorker.Tag as string;
                                        rowCsly["FROM_AREAID"] = sArea;
                                        inCslyDataSet.Tables["INDATA"].Rows.Add(rowCsly);

                                        DataTable inDataCslyRspnID = inCslyDataSet.Tables.Add("INRSPN");
                                        inDataCslyRspnID.Columns.Add("RSPN_ID", typeof(string));

                                        //rcv_iss_id
                                        //sOut_ID
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            DataRow rowCsly2 = inDataCslyRspnID.NewRow();
                                            rowCsly2["RSPN_ID"]     = dt.Rows[i]["CELL_SPLY_RSPN_ID"].ToString();

                                            inCslyDataSet.Tables["INRSPN"].Rows.Add(rowCsly2);
                                        }

                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CSLY_RCV_ISS", "INDATA,INRSPN", null, inCslyDataSet);
                                        */

                                        DataSet inCslyDataSet = new DataSet();

                                        DataTable inDataCsly = inCslyDataSet.Tables.Add("IN_DATA");
                                        inDataCsly.Columns.Add("AREAID", typeof(string));
                                        inDataCsly.Columns.Add("USERID", typeof(string));
                                        inDataCsly.Columns.Add("NOTE", typeof(string));

                                        DataRow rowInDataCsly = inDataCsly.NewRow();
                                        rowInDataCsly["AREAID"] = sArea;
                                        rowInDataCsly["USERID"] = txtWorker.Tag as string;
                                        rowInDataCsly["NOTE"] = sOut_ID;

                                        inCslyDataSet.Tables["IN_DATA"].Rows.Add(rowInDataCsly);



                                        DataTable inRspnCsly = inCslyDataSet.Tables.Add("IN_RSPN");
                                        inRspnCsly.Columns.Add("RSPN_ID", typeof(string));
                                        inRspnCsly.Columns.Add("PRODID", typeof(string));


                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            DataRow rowInRspnCsly = inRspnCsly.NewRow();
                                            rowInRspnCsly["RSPN_ID"] = dt.Rows[i]["CELL_SPLY_RSPN_ID"].ToString();
                                            rowInRspnCsly["PRODID"] = dt.Rows[i]["PACK_PRODID"].ToString();

                                            inCslyDataSet.Tables["IN_RSPN"].Rows.Add(rowInRspnCsly);
                                        }



                                        DataTable inPalletCsly = inCslyDataSet.Tables.Add("IN_PALLET");
                                        inPalletCsly.Columns.Add("BOXID", typeof(string));
                                        inPalletCsly.Columns.Add("PRODID", typeof(string));
                                        inPalletCsly.Columns.Add("TOTAL_QTY", typeof(string));


                                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                                        {
                                            DataRow rowInPallet = inPalletCsly.NewRow();
                                            rowInPallet["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                            rowInPallet["PRODID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOP_PRODID").ToString();
                                            rowInPallet["TOTAL_QTY"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString();

                                            inCslyDataSet.Tables["IN_PALLET"].Rows.Add(rowInPallet);
                                        }

                                        DataTable inRcvIssIdCsly = inCslyDataSet.Tables.Add("IN_RCV_ISS_ID");
                                        inRcvIssIdCsly.Columns.Add("RCV_ISS_ID", typeof(string));

                                        DataRow rowInRcvIssCsly = inRcvIssIdCsly.NewRow();
                                        rowInRcvIssCsly["RCV_ISS_ID"] = sOut_ID;

                                        inCslyDataSet.Tables["IN_RCV_ISS_ID"].Rows.Add(rowInRcvIssCsly);

                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CSLY_RCV_ISS_PLLT", "IN_DATA,IN_RSPN,IN_PALLET,IN_RCV_ISS_ID", null, inCslyDataSet);

                                    }
                                }
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

                            // 자동 출하창고 출고 여부 TITLE
                            dtPacking_Tag.Columns.Add("AUTO_WH_TITLE", typeof(string));

                            // 자동 출하창고 출고 여부 FLAG
                            dtPacking_Tag.Columns.Add("AUTO_WH_FLAG", typeof(string));

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
                            drCrad["AUTO_WH_TITLE"] = ObjectDic.Instance.GetObjectName("자동창고 출고 여부");


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
                                drCrad2["AUTO_WH_TITLE"] = ObjectDic.Instance.GetObjectName("자동창고 출고 여부");
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
                                //drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                                //drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();

                                //if (bAddFlag)
                                //{
                                //    drCrad2["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                                //    drCrad2["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                //}


                                drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                if (bAddFlag)
                                {
                                    drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                    drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                }
                            }

                            drCrad["Pallet_Qty"] = txtPalletQty.Text.ToString();
                            drCrad["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString() : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                            drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID
                            drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                            drCrad["Lot_Qty"] = sLotTerm;         //최대편차
                            drCrad["Prod_Date"] = sProdDate;         //작업일자
                            drCrad["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                            drCrad["Prod_ID"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TOP_PRODID").ToString();         //제품ID
                            drCrad["User"] = txtWorker.Text;         //작업자
                            drCrad["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                            drCrad["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고
                            drCrad["AUTO_WH_FLAG"] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? cboAutoYN.SelectedValue : null; // 자동창고 출고 여부

                            if (bAddFlag)
                            {
                                drCrad2["Pallet_Qty"] = txtPalletQty.Text.ToString();
                                drCrad2["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString() : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                                drCrad2["Pack_ID"] = sOut_ID;    //포장출고 ID
                                drCrad2["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                                drCrad2["Lot_Qty"] = sLotTerm;         //최대편차
                                drCrad2["Prod_Date"] = sProdDate;         //작업일자
                                drCrad2["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                                drCrad2["Prod_ID"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TOP_PRODID").ToString();         //제품ID
                                drCrad2["User"] = txtWorker.Text;         //작업자
                                drCrad2["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                                drCrad2["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고
                                drCrad2["AUTO_WH_FLAG"] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? cboAutoYN.SelectedValue : null; // 자동창고 출고 여부

                            }
                            int j = 1;
                            for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                            {
                                //if(drCrad2)
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
                                // drCrad["Prod_Qty"] = Math.Round(Convert.ToDouble(txtCellQty.Text.ToString()), 2, MidpointRounding.AwayFromZero); 
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
                            Parameters[0] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? "Packing_Tag_Auto_WH_Ship" : "Packing_Tag";
                            Parameters[1] = dtPacking_Tag;
                            Parameters[2] = bAddFlag;
                            Parameters[3] = "2";

                            LGC.GMES.MES.BOX001.Report_Packing_Tag rs = new LGC.GMES.MES.BOX001.Report_Packing_Tag();
                            C1WindowExtension.SetParameters(rs, Parameters);

                            //rs.Closed += new EventHandler(wndQAMailSend_Closed);
                            rs.Closed += new EventHandler(wndPackingTag_Closed);
                            //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
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
                loadingIndicator.Visibility = Visibility.Collapsed;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                if (window.DialogResult != MessageBoxResult.OK)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        private void wndPackingTag_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.BOX001.Report_Packing_Tag wndPopup = sender as LGC.GMES.MES.BOX001.Report_Packing_Tag;
            //if (wndPopup.DialogResult == MessageBoxResult.OK)
            //{
            Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
            Initialize_Out();

            string strProdId = string.Empty;

            if (dgPackOut.GetRowCount() > 0)
            {
                DataView dtView = DataTableConverter.Convert(dgPackOut.ItemsSource).DefaultView;
                DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                string[] arrStr = arrObj.Cast<string>().ToArray();
                strProdId = string.Join(",", arrStr);
            }

            SearchPackOutCslyRspnHist(strProdId);

            loadingIndicator.Visibility = Visibility.Collapsed;
            grdMain.Children.Remove(wndPopup);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


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

            //if (dgPackOut.GetRowCount() == 0)
            //{
            //    return;
            //}

            //for (int i = 0; i < dgPackOut.Rows.Count; i++)
            //{
            //    if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "CHK").ToString() == "True")
            //    {
            //        dgPackOut.IsReadOnly = false;
            //        dgPackOut.RemoveRow(i);
            //        dgPackOut.IsReadOnly = true;
            //    }
            //}
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
                        Util.Alert("MMD0004");   //동을 선택해 주세요.
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
                        Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }

                    Scan_Process(sPalletID, sArea);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                        //dataTable.AcceptChanges();

                        //dgListUpload.ItemsSource = DataTableConverter.Convert(dataTable);

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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }
        }

        private bool Scan_Process(string sPallet_ID, string sArea)
        {
            try
            {

                string sBizRule = string.Empty;

                if (bForm)
                {
                    sBizRule = "BR_CHK_SHIP_PALLET_BX";

                    if (cboLottype.SelectedValue.ToString().Equals("") || cboLottype.SelectedValue.Equals("SELECT"))
                    {
                        //LOT 유형을 선택하세요.
                        Util.MessageValidation("SFU4068");
                        return false;
                    }
                    // Pallet Barcode 사용 Area인 경우 입력한 값이 Pallet Barcode인지 확인하여 Pallet Barcode이면 PalletID로 변환하여 조회
                    if (isCellPalletBarcodeUseArea)
                    {
                        string palletBarcodeID = getPalletID(sPallet_ID);
                        if (!palletBarcodeID.IsNullOrEmpty())
                        {
                            sPallet_ID = palletBarcodeID;
                        }
                    }
                }
                else
                {
                    sBizRule = "BR_PRD_GET_PALLET_INFO_FOR_SHIP";
                }

                //// Pallet 상태 체크
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("LOTTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPallet_ID;
                dr["AREAID"] = sArea;
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTTYPE"] = cboLottype.SelectedValue;

                RQSTDT.Rows.Add(dr);

                // DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_SHIP", "INDATA", "OUTDATA", RQSTDT);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizRule, "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                //if (dsRslt.Tables["OUTDATA"].Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1905");   //조회된 Data가 없습니다.
                    return false;
                }

                if (dgPackOut.GetRowCount() != 0)
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgPackOut.ItemsSource);

                    if (SearchResult.Rows[0]["TO_SLOC_ID"].ToString() != dtInfo.Rows[0]["TO_SLOC_ID"].ToString())
                    //if (dsRslt.Tables["OUTDATA"].Rows[0]["TO_SLOC_ID"].ToString() != DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TO_SLOC_ID").ToString())
                    {
                        Util.MessageValidation("SFU3011");  //이전에 스캔한 Pallet 의 출고창고 정보와 다릅니다.
                        return false;
                    }

                    //if (SearchResult.Rows[0]["TO_SLOC_ID"].ToString() != DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TO_SLOC_ID").ToString())
                    ////if (dsRslt.Tables["OUTDATA"].Rows[0]["TO_SLOC_ID"].ToString() != DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TO_SLOC_ID").ToString())
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이전에 스캔한 Pallet 의 출하처 정보와 다릅니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}

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

                    dtInfo.Merge(SearchResult);
                    Util.GridSetData(dgPackOut, dtInfo, FrameOperation);

                }
                else
                {
                    Util.GridSetData(dgPackOut, SearchResult, FrameOperation);
                    //dgPackOut.ItemsSource = DataTableConverter.Convert(SearchResult);
                }

                isPalletQty = isPalletQty + 1;
                txtPalletQty.Text = isPalletQty.ToString();

                //isCellQty = isCellQty + Convert.ToInt32(dsRslt.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"].ToString());
                isCellQty = isCellQty + Convert.ToDouble(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                txtCellQty.Text = isCellQty.ToString();

                txtPalletID.Text = "";
                txtPalletID.Focus();

                // cell 정보 조회 이후 lottype 변경 불가
                cboLottype.IsEnabled = false;

                if (!(CslyPackOutRspnList.GetRowCount() > 0))
                {
                    SearchPackOutCslyRspnHist(SearchResult.Rows[0]["TOP_PRODID"].ToString());
                }

                return true;

            }

            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                // Util.AlertByBiz("BR_PRD_GET_PALLET_INFO_FOR_SHIP", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return false;
            }
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
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgOutHist.BeginEdit();
                //    dgOutHist.ItemsSource = DataTableConverter.Convert(dt);
                //    dgOutHist.EndEdit();
                //}


                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgOutHist.SelectedIndex = idx;

                Search_PalletInfo(idx);
            }
        }

        private void btnPackOut_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Visible;
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

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Scan_Process(sPasteStrings[i], sArea) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
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

                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "OUTER_BOXID", null, "PALLETID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "BOXID", null, "BOXID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "TAG_ID", null, "TAG_ID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "SUBLOTID", null, "CELLID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "BOX_PSTN_NO", null, "CELL위치", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);

                    for (int i = 0; i < dsResult.Tables["OUTDATA_HEADER"].Rows.Count; i++)
                    {
                        if (Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]).IndexOf("VALUE") > -1)
                        {
                            string sBinding = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]);
                            string sHeadName = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["MEASR_ITEM_NAME"]);

                            LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, sBinding, null, sHeadName, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }

                    dgFCSData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA_VALUE"]);

                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgFCSData);

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

            // 활성화 여부 확인하여 콤보박스 조회를 다르게 처리함.
            if (bForm)
            {
                _comboF.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");
            }
            else
            {
                _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");
            }

            //  string[] sFilter3 = { sSHOPID, sEQSGID, sAREAID };
            //  combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine, cboAreaAll2 }, sFilter: sFilter3, sCase: "SHIPTO_CP");          


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
            if (DaileySearch.IsVisible == true)
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
            this.Dispatcher.BeginInvoke(new System.Action(() =>
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

                //if ((dtpDateTo_Daily.SelectedDateTime - dtpDateFrom_Daily.SelectedDateTime).TotalDays > 7)
                //{
                //    Util.MessageValidation("SFU2042", "7");        // 기간은 7일 이내로 조회
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //    return;
                //}

                loadingIndicator.Visibility = Visibility.Visible;

                string sStart_date = dtpDateFrom_Daily.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = dtpDateTo_Daily.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                //기존 컬럼 삭제
                if (dgList.Columns.Count > 1)
                {
                    for (int i = dgList.Columns.Count; i-- > 1;) //컬럼수
                        dgList.Columns.RemoveAt(i);
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
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
                    sStart_date = dtpDateFrom_Daily.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    sEnd_date = dtpDateTo_Daily.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
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

                    lSumCell = lSumCell + Util.NVC_Int(dgOutHist.GetCell(lsCount, dgOutHist.Columns["LINE_TOTAL_QTY"].Index).Value);
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

            //Lot별 사용이력 탭이 열릴 때만 시간 초기값으로 세팅
            if (DaileySearch.IsFocused)
            {
                //int deltaMDay = DayOfWeek.Monday - System.DateTime.Now.DayOfWeek;
                //DateTime thisMon = System.DateTime.Now.AddDays(deltaMDay);
                //dtpDateFrom_Daily.SelectedDateTime = thisMon;


                datePicker.SelectedDateTime = System.DateTime.Now; //해당 주의 월요일
                datePicker.Loaded -= dtpDateFrom_Daily_Loaded;
            }
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








        #endregion

        #endregion


        private void ChkPackOutConfigProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_OUT_CNF_PRS_SYS";
                dr["CMCODE"] = LoginInfo.SYSID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    this.tiPackOut.Header = ObjectDic.Instance.GetObjectName("SHIP_CONFIG");

                    tiPackConfirm.Visibility = Visibility.Collapsed;
                    //임시 염규범 선임
                    tiPackOut.Visibility = Visibility.Visible;
                    //tiPackOut.Visibility = Visibility.Collapsed;
                    btnPackOut.Visibility = Visibility.Visible;
                    //btnPackOut.Visibility = Visibility.Collapsed;
                    tiPackConfirm.Visibility = Visibility.Collapsed;
                    //tiPackConfirm.Visibility = Visibility.Visible;
                    btnPackOutConfig.Visibility = Visibility.Collapsed;
                    //btnPackOutConfig.Visibility = Visibility.Visible;
                }
                else
                {
                    this.tiPackOut.Header = ObjectDic.Instance.GetObjectName("포장출고");

                    btnPackOut.Visibility = Visibility.Visible;
                    btnPackOutConfig.Visibility = Visibility.Collapsed;
                    tiPackConfirm.Visibility = Visibility.Collapsed;
                    tiPackOut.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPackOutConfig_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnPackOutConfig_Click(object sender, RoutedEventArgs e)
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

                // 자동창고 출고 여부 선택 확인
                if (bRFID_Use)
                {
                    if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                    {
                        // 자동창고 출고 여부를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                        return;
                    }
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
                    BOX001_011_CONFIRM wndShipConfig = new BOX001_011_CONFIRM();
                    wndShipConfig.FrameOperation = FrameOperation;

                    if (wndShipConfig != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = cboComp.Text;
                        Parameters[1] = cboLocTo.Text;
                        Parameters[2] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                        Parameters[3] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();
                        Parameters[4] = txtPalletQty.Text;
                        Parameters[5] = txtCellQty.Text;
                        Parameters[6] = true; // 포장출고 구성 버튼 여부.
                        C1WindowExtension.SetParameters(wndShipConfig, Parameters);
                        wndShipConfig.Closing += wndShipConfig_Closing;
                        wndShipConfig.Closed += new EventHandler(wndShipConfig_Closed);
                        //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        grdMain.Children.Add(wndShipConfig);
                        wndShipConfig.BringToFront();
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void wndShipConfig_Closed(object sender, EventArgs e)
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

                    // 자동창고 출고 여부 선택 확인
                    if (bRFID_Use)
                    {
                        if (cboAutoYN.SelectedIndex < 0 || cboAutoYN.SelectedValue.ToString().Trim().Equals(""))
                        {
                            // 자동창고 출고 여부를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자동창고 출고 여부"));
                            return;
                        }
                    }

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
                        inData.Columns.Add("RFID_USE_FLAG", typeof(string)); // 고정식 RFID 사용여부
                        inData.Columns.Add("AUTO_WH_SHIP_FLAG", typeof(string)); // 자동창고 출하여부

                        DataRow row = inData.NewRow();
                        row["FROM_AREAID"] = sArea;
                        row["FROM_SLOC_ID"] = sLocFrom; // Util.GetCondition(cboLocFrom, "출고창고를선택하세요.");//출고창고
                        row["TO_AREAID"] = sTo_Areaid;          //입고 Area
                        row["TO_SLOC_ID"] = sLocTo;     // Util.GetCondition(cboLocTo, "입고창고를선택하세요.");//입고저장위치
                        row["ISS_QTY"] = isCellQty;     //출고수량
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = sComp;       // Util.GetCondition(cboComp, "출하처를선택하세요.");//출하처
                        row["NOTE"] = "";
                        //row["USERID"] = txtUserID.Text;
                        row["USERID"] = txtWorker.Tag as string;
                        row["RFID_USE_FLAG"] = bRFID_Use ? "Y" : "N";
                        row["AUTO_WH_SHIP_FLAG"] = bRFID_Use ? cboAutoYN.SelectedValue.ToString() : "N";

                        if (row["FROM_SLOC_ID"].Equals("") || row["TO_SLOC_ID"].Equals("") || row["SHIPTO_ID"].Equals("")) return;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inPallet = indataSet.Tables.Add("INPALLET");
                        inPallet.Columns.Add("BOXID", typeof(string));
                        inPallet.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            DataRow row2 = inPallet.NewRow();
                            row2["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                            row2["OWMS_BOX_TYPE_CODE"] = "AC";

                            indataSet.Tables["INPALLET"].Rows.Add(row2);
                        }

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_CELL", "INDATA,INPALLET", "OUTDATA", indataSet);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_WAIT_CELL", "INDATA,INPALLET", "OUTDATA", (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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

                            // 자동 출하창고 출고 여부 TITLE
                            dtPacking_Tag.Columns.Add("AUTO_WH_TITLE", typeof(string));

                            // 자동 출하창고 출고 여부 FLAG
                            dtPacking_Tag.Columns.Add("AUTO_WH_FLAG", typeof(string));

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
                            drCrad["AUTO_WH_TITLE"] = ObjectDic.Instance.GetObjectName("자동창고 출고 여부");


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
                                drCrad2["AUTO_WH_TITLE"] = ObjectDic.Instance.GetObjectName("자동창고 출고 여부");
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
                                //drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                                //drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();

                                //if (bAddFlag)
                                //{
                                //    drCrad2["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString(); // dtLineResult.Rows[i]["EQSGNAME"].ToString();
                                //    drCrad2["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                //}


                                drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                if (bAddFlag)
                                {
                                    drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGTYPE"].ToString() + "# " + dtLineResult.Rows[i]["FORM_LINE_ID"].ToString();
                                    drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                                }
                            }

                            drCrad["Pallet_Qty"] = txtPalletQty.Text.ToString();
                            drCrad["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString() : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                            drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID
                            drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                            drCrad["Lot_Qty"] = sLotTerm;         //최대편차
                            drCrad["Prod_Date"] = sProdDate;         //작업일자
                            drCrad["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                            drCrad["Prod_ID"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();         //제품ID
                            drCrad["User"] = txtWorker.Text;         //작업자
                            drCrad["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                            drCrad["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고
                            drCrad["AUTO_WH_FLAG"] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? cboAutoYN.SelectedValue : null; // 자동창고 출고 여부

                            if (bAddFlag)
                            {
                                drCrad2["Pallet_Qty"] = txtPalletQty.Text.ToString();
                                drCrad2["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString() : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                                drCrad2["Pack_ID"] = sOut_ID;    //포장출고 ID
                                drCrad2["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID
                                drCrad2["Lot_Qty"] = sLotTerm;         //최대편차
                                drCrad2["Prod_Date"] = sProdDate;         //작업일자
                                drCrad2["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량
                                drCrad2["Prod_ID"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();         //제품ID
                                drCrad2["User"] = txtWorker.Text;         //작업자
                                drCrad2["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고
                                drCrad2["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고
                                drCrad2["AUTO_WH_FLAG"] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? cboAutoYN.SelectedValue : null; // 자동창고 출고 여부

                            }
                            int j = 1;
                            for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                            {
                                //if(drCrad2)
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
                                // drCrad["Prod_Qty"] = Math.Round(Convert.ToDouble(txtCellQty.Text.ToString()), 2, MidpointRounding.AwayFromZero); 
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
                            Parameters[0] = bRFID_Use && cboAutoYN.Visibility == Visibility.Visible ? "Packing_Tag_Auto_WH_Ship" : "Packing_Tag";
                            Parameters[1] = dtPacking_Tag;
                            Parameters[2] = bAddFlag;
                            Parameters[3] = "2";

                            LGC.GMES.MES.BOX001.Report_Packing_Tag rs = new LGC.GMES.MES.BOX001.Report_Packing_Tag();
                            rs.FrameOperation = this.FrameOperation;
                            C1WindowExtension.SetParameters(rs, Parameters);

                            //rs.Closed += new EventHandler(wndQAMailSend_Closed);
                            rs.Closed += new EventHandler(wndPackingTag_Closed);
                            //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                if (window.DialogResult != MessageBoxResult.OK)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void wndShipConfig_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void cboArea_cfg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea_cfg.SelectedValue);

            if (sTemp == "" || sTemp.Equals("SELECT"))
            {
                sAREAID_cfg = "";
                sSHOPID_cfg = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID_cfg = sArry[0];
                sSHOPID_cfg = sArry[1];
            }
            String[] sFilter = { sAREAID_cfg };    // Area

            _combo.SetCombo(cboLine_cfg, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

        }

        private void cboLine_cfg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _combo.SetCombo(cboModelLot_cfg, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine_cfg }, sCase: "cboModelLot");

            if (cboLine_cfg.SelectedValue != null)
            {
                string[] sFilter3 = { sSHOPID_cfg, (string)cboLine_cfg.SelectedValue, sAREAID_cfg };
                _combo.SetCombo(cboShipTo_cfg, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "SHIPTO_CP");
            }
        }

        private void btnShippingWaitCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgShippingWait.GetRowCount() == 0)
                {
                    //Util.Alert("Data가 존재하지 않습니다.");
                    Util.MessageValidation("SFU1331");
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgShippingWait, "CHK") != 1)
                {
                    Util.MessageValidation("SFU3772"); //하나의 출고ID만 선택해 주세요.
                    return;
                }

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgShippingWait, "CHK");
                if (idx < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                // 출고 대기 취소 하시겠습니까?
                Util.MessageConfirm("SFU3771", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelShippingWait();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnShippingConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgShippingWait.GetRowCount() == 0)
                {
                    //Util.Alert("Data가 존재하지 않습니다.");
                    Util.MessageValidation("SFU1331");
                    return;
                }

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgShippingWait, "CHK");
                if (idx < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }


                if (_Util.GetDataGridCheckCnt(dgShippingWait, "CHK") != 1)
                {
                    Util.MessageValidation("SFU3772"); //하나의 출고ID만 선택해 주세요.
                    return;
                }

                /*
                 * 2021-10-01 염규범 선임
                 * 셀공급 시스템 요청건 추가의 건
                 *
                 */
                if (CslyRspnList.GetRowCount() > 0)
                {
                    if (!(_Util.GetDataGridFirstRowIndexWithTopRow(CslyRspnList, "CHK", true) == -1))
                    {
                        if (!CslyRegValidation()) return;
                    }

                    if (!string.IsNullOrWhiteSpace(cboShipTo_cfg.SelectedValue.ToString()))
                    {
                        DataTable dt = DataTableConverter.Convert(CslyRspnList.ItemsSource).AsEnumerable()
                                                                                          .Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();

                        //DataView dtView = dt.DefaultView;
                        //DataTable dtAreaId = dtView.ToTable(true, new string[] { "AREAID" });


                        if (!(dt.AsEnumerable().Where(x => x.Field<string>("AREAID").Equals(CslySelAeaId(cboShipTo_cfg.SelectedValue.ToString()))).Count() == dt.Rows.Count))
                        {
                            Util.MessageInfo("SFU8417");
                            return;
                        }
                    }
                }

                // 출고 확정 하시겠습니까?
                Util.MessageConfirm("SFU3373", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ShippingConfirm();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_cfg_Click(object sender, RoutedEventArgs e)
        {
            txtPalletQty_Search_cfg.Value = 0;
            txtCellQty_Search_cfg.Value = 0;

            txtPalletQty_Total_cfg.Value = 0;
            txtCellQty_Total_cfg.Value = 0;

            Util.gridClear(dgShippingWait);
            Util.gridClear(dgShippingWaitDetail);

            GetShippingWaitList();
        }

        private void dgShippingWait_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgShippingWait.CurrentRow == null || dgShippingWait.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgShippingWait.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgShippingWait.GetCell(dgShippingWait.CurrentRow.Index, dgShippingWait.Columns["CHK"].Index).Value);

                    //초기화
                    dgShippingWaitDetail.ItemsSource = null;

                    if (chkValue == "0" || chkValue == "False")
                    {
                        DataTableConverter.SetValue(dgShippingWait.Rows[dgShippingWait.CurrentRow.Index].DataItem, "CHK", true);
                        //GetShippingWaitDetlList(dgShippingWait.CurrentRow.Index);

                    }
                    else
                        DataTableConverter.SetValue(dgShippingWait.Rows[dgShippingWait.CurrentRow.Index].DataItem, "CHK", false);


                    SetSelectedQty_cfg();


                }

                GetShippingWaitDetlList(dgShippingWait.CurrentRow.Index);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgShippingWait_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

        }

        private void GetShippingWaitList()
        {
            try
            {

                string sStart_date = dtpDateFrom_cfg.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo_cfg.SelectedDateTime.ToString("yyyyMMdd");

                string sLot_Type = string.Empty;

                if (cboType_cfg.SelectedIndex < 0 || cboType_cfg.SelectedValue.ToString().Trim().Equals(""))
                {
                    sLot_Type = null;
                }
                else
                {
                    sLot_Type = cboType_cfg.SelectedValue.ToString();
                }

                // 조회 비즈 생성

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (txtBoxID_cfg.Text.Trim() != "" && txtBoxID_cfg.Text.Substring(0, 1).ToUpper() == "P")
                {
                    dr["BOXID"] = txtBoxID_cfg.Text.Trim();
                }
                else if (txtBoxID_cfg.Text.Trim() != "" && txtBoxID_cfg.Text.Substring(0, 1).ToUpper() != "P")
                {
                    dr["RCV_ISS_ID"] = txtBoxID_cfg.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["MDLLOT_ID"] = Util.NVC(cboModelLot_cfg.SelectedValue) == "" ? null : Util.NVC(cboModelLot_cfg.SelectedValue);
                    dr["PACK_WRK_TYPE_CODE"] = sLot_Type;
                    dr["SHIPTO_ID"] = Util.NVC(cboShipTo_cfg.SelectedValue) == "" ? null : Util.NVC(cboShipTo_cfg.SelectedValue); ;
                }

                if (sAREAID_cfg.Equals(""))
                {
                    Util.MessageValidation("SFU3206"); // 동을 선택해주세요
                    return;
                }
                dr["FROM_AREAID"] = string.IsNullOrWhiteSpace(sAREAID_cfg) ? null : sAREAID_cfg;
                if (!string.IsNullOrWhiteSpace(cboLine_cfg.SelectedValue.ToString())) dr["EQSGID"] = cboLine_cfg.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIP_WAIT_LIST", "INDATA", "OUTDATA", RQSTDT);
                Util.gridClear(dgShippingWait);
                Util.GridSetData(dgShippingWait, SearchResult, FrameOperation, true);


                dgShippingWait.MergingCells -= dgShippingWait_MergingCells;
                dgShippingWait.MergingCells += dgShippingWait_MergingCells;

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgShippingWait.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgShippingWait.GetCell(lsCount, dgShippingWait.Columns["PALLETQTY"].Index).Value);

                    lSumCell = lSumCell + Util.NVC_Int(dgShippingWait.GetCell(lsCount, dgShippingWait.Columns["TOTALQTY"].Index).Value);
                }

                txtPalletQty_Total_cfg.Value = lSumPallet;
                txtCellQty_Total_cfg.Value = lSumCell;

                ChkPackOutConfigProcess();

                if ((bool)btnExpand.IsChecked)
                {
                    if (dgShippingWait.GetRowCount() > 0)
                    {
                        string strProdId = string.Empty;

                        DataView dtView = DataTableConverter.Convert(dgShippingWait.ItemsSource).DefaultView;
                        DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                        object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                        string[] arrStr = arrObj.Cast<string>().ToArray();
                        strProdId = string.Join(",", arrStr);

                        SearchCslyRspnHist(strProdId);

                    }
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetShippingWaitDetlList(int idx)
        {
            string sOutLotid = string.Empty;

            sOutLotid = DataTableConverter.GetValue(dgShippingWait.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
            RQSTDT.Columns.Add("LANGID", typeof(String));
            RQSTDT.Columns.Add("PACK_EQSGID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["RCV_ISS_ID"] = sOutLotid;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PACK_EQSGID"] = DataTableConverter.GetValue(dgShippingWait.Rows[idx].DataItem, "PACK_EQSGID").ToString();

            RQSTDT.Rows.Add(dr);

            DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

            Util.gridClear(dgShippingWaitDetail);
            Util.GridSetData(dgShippingWaitDetail, DetailResult, FrameOperation, true);


        }

        private void SetSelectedQty_cfg()
        {
            int lSumPallet = 0;
            int lSumCell = 0;

            for (int lsCount = 0; lsCount < dgShippingWait.GetRowCount(); lsCount++)
            {
                if (DataTableConverter.GetValue(dgShippingWait.Rows[lsCount].DataItem, "CHK").ToString() == "1" || DataTableConverter.GetValue(dgShippingWait.Rows[lsCount].DataItem, "CHK").ToString() == "True")
                {
                    lSumPallet = lSumPallet + Util.NVC_Int(dgShippingWait.GetCell(lsCount, dgShippingWait.Columns["PALLETQTY"].Index).Value);
                    lSumCell = lSumCell + Util.NVC_Int(dgShippingWait.GetCell(lsCount, dgShippingWait.Columns["TOTALQTY"].Index).Value);
                }
            }

            txtPalletQty_Search_cfg.Value = lSumPallet;
            txtCellQty_Search_cfg.Value = lSumCell;
        }

        private void ShippingConfirm()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("RCV_ISS_ID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));



                for (int i = 0; i < dgShippingWait.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgShippingWait, "CHK", i)) continue;

                    DataRow[] drList = inData.Select("RCV_ISS_ID = '" + Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID")) + "'");
                    if (drList?.Length > 0) continue;

                    DataRow newRow = inData.NewRow();

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID"));
                    newRow["USERID"] = LoginInfo.USERID;

                    indataSet.Tables["INDATA"].Rows.Add(newRow);

                }


                /*
                 * 2021-10-01 염규범 선임
                 * 셀공급 프로젝트 추가 개발건으로 인한 추가건
                 * 기존 DataSet에, IN_RSPN 추가의 건
                 */
                if (CslyRspnList.GetRowCount() > 0)
                {
                    if (!(_Util.GetDataGridFirstRowIndexWithTopRow(CslyRspnList, "CHK", true) == -1))
                    {
                        DataTable in_Rspn = indataSet.Tables.Add("IN_RSPN");
                        in_Rspn.Columns.Add("RSPN_ID", typeof(string));


                        DataTable dtCslyRspnList = DataTableConverter.Convert(CslyRspnList.ItemsSource).AsEnumerable()
                                                                                         .Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();
                        for (int i = 0; i < dtCslyRspnList.Rows.Count; i++)
                        {
                            DataRow newRow = in_Rspn.NewRow();
                            newRow["RSPN_ID"] = dtCslyRspnList.Rows[i]["CELL_SPLY_RSPN_ID"].ToString();
                            indataSet.Tables["IN_RSPN"].Rows.Add(newRow);

                        }
                    }
                }


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_WAIT_CONFIRM_CELL_CSLY", "INDATA,IN_RSPN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_cfg_Click(null, null)));

                        if (ShipList.RowDefinitions[3].Height.Value == 0) return;

                        ShipList.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                        btnSearch_Csly.Visibility = Visibility.Collapsed;
                        Util.gridClear(CslyRspnList);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void CancelShippingWait()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("RCV_ISS_ID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgShippingWait.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgShippingWait, "CHK", i)) continue;

                    DataRow[] drList = inData.Select("RCV_ISS_ID = '" + Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID")) + "'");
                    if (drList?.Length > 0) continue;

                    DataRow newRow = inData.NewRow();

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID"));
                    newRow["USERID"] = LoginInfo.USERID;

                    indataSet.Tables["INDATA"].Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_WAIT_CELL", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_cfg_Click(null, null)));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void dgShippingWait_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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

                for (int i = dgShippingWait.TopRows.Count; i < dgShippingWait.Rows.Count; i++)
                {

                    if (dgShippingWait.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                for (int m = idxS; m <= idxE; m++)
                                {
                                    if (!bTOTALQTYStrt)
                                    {
                                        bTOTALQTYStrt = true;
                                        sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
                                        TOTALQTYxS = m;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                            TOTALQTYxE = m;
                                        else
                                        {
                                            if (TOTALQTYxS > TOTALQTYxE)
                                            {
                                                TOTALQTYxE = TOTALQTYxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
                                            bTOTALQTYStrt = true;
                                            sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
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
                                    e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
                                    bTOTALQTYStrt = false;
                                    sTmpTOTALQTY = string.Empty;
                                    TOTALQTYxE = 0;
                                    TOTALQTYxS = 0;

                                }
                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID"));
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
                                    sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
                                    TOTALQTYxS = m;

                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                        TOTALQTYxE = m;
                                    else
                                    {
                                        if (TOTALQTYxS > TOTALQTYxE)
                                        {
                                            TOTALQTYxE = TOTALQTYxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
                                        bTOTALQTYStrt = true;
                                        sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
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
                                e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
                                bTOTALQTYStrt = false;
                                sTmpTOTALQTY = string.Empty;
                                TOTALQTYxE = 0;
                                TOTALQTYxS = 0;

                            }

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[i].DataItem, "RCV_ISS_ID"));
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
                            sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
                            TOTALQTYxS = m;

                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY")).Equals(sTmpTOTALQTY))
                                TOTALQTYxE = m;
                            else
                            {
                                if (TOTALQTYxS > TOTALQTYxE)
                                {
                                    TOTALQTYxE = TOTALQTYxS;
                                }
                                e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
                                bTOTALQTYStrt = true;
                                sTmpTOTALQTY = Util.NVC(DataTableConverter.GetValue(dgShippingWait.Rows[m].DataItem, "TOTALQTY"));
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
                        e.Merge(new DataGridCellsRange(dgShippingWait.GetCell(TOTALQTYxS, dgShippingWait.Columns["TOTALQTY"].Index), dgShippingWait.GetCell(TOTALQTYxE, dgShippingWait.Columns["TOTALQTY"].Index)));
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



        /*
         *  셀 공급 프로젝트 2차 오픈건으로 인한 수정의 건
         *  2021.09.30 염규범 S
         *
         */

        private void BtnExpand_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShipList.RowDefinitions[3].Height.Value != 0) return;

                ShipList.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                btnSearch_Csly.Visibility = Visibility.Visible;

                string strProdId = string.Empty;

                if (dgShippingWait.GetRowCount() > 0)
                {
                    DataView dtView = DataTableConverter.Convert(dgShippingWait.ItemsSource).DefaultView;
                    DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                    object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                    string[] arrStr = arrObj.Cast<string>().ToArray();
                    strProdId = string.Join(",", arrStr);
                }


                SearchCslyRspnHist(strProdId);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnExpand_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShipList.RowDefinitions[3].Height.Value == 0) return;

                ShipList.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                btnSearch_Csly.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnSearch_Csly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProdId = string.Empty;

                if (dgShippingWait.GetRowCount() > 0)
                {
                    DataView dtView = DataTableConverter.Convert(dgShippingWait.ItemsSource).DefaultView;
                    DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                    object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                    string[] arrStr = arrObj.Cast<string>().ToArray();
                    strProdId = string.Join(",", arrStr);
                }

                SearchCslyRspnHist(strProdId);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SearchCslyRspnHist(string strProdId)
        {
            try
            {
                string sTemp = Util.NVC(cboArea_cfg.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RSPN_AREAID", typeof(string));

                if (strProdId != null && !string.IsNullOrWhiteSpace(strProdId)) RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                // 기존 화면 내용 유지
                dr["RSPN_AREAID"] = sAREAID_cfg;
                if (strProdId != null && !string.IsNullOrWhiteSpace(strProdId)) dr["PRODID"] = strProdId;


                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CSLY_RSPN", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dt = DataTableConverter.Convert(CslyRspnList.ItemsSource);

                Util.gridClear(CslyRspnList);
                Util.GridSetData(CslyRspnList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CslyRspnList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CslyRspnList.CurrentRow == null || CslyRspnList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = CslyRspnList.CurrentColumn.Name;
                string strChkValue = "";

                if (!sColName.Contains("CHK"))
                {
                    return;
                }


                if (e.ChangedButton.ToString().Equals("Left"))
                {

                    int indexRow = CslyRspnList.CurrentRow.Index;
                    int indexColumn = CslyRspnList.CurrentColumn.Index;

                    strChkValue = Util.NVC(CslyRspnList.GetCell(indexRow, indexColumn).Value);

                    if (string.IsNullOrEmpty(strChkValue) || strChkValue.Equals(""))
                        return;

                    if (strChkValue == "0" || strChkValue == "False")
                    {
                        if (dgShippingWait.GetRowCount() <= 0)
                        {
                            Util.MessageInfo("SFU8409");
                            return;
                        }

                        if (_Util.GetDataGridCheckCnt(dgShippingWait, "CHK") != 1)
                        {
                            Util.MessageValidation("SFU8412"); //하나의 출고ID만 선택해 주세요.
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgShippingWait.ItemsSource).AsEnumerable()
                                                                                          .Where(x => x.Field<int>("CHK") == 1).CopyToDataTable();

                        DataView dtView = dt.DefaultView;
                        DataTable dtProdId = dtView.ToTable(true, new string[] { "PRODID" });


                        if (!string.IsNullOrWhiteSpace(cboShipTo_cfg.SelectedValue.ToString()))
                        {
                            string strAreaId = CslyRspnList.GetValue(indexRow, "AREAID").ToString();

                            if (!CslySelAeaId(cboShipTo_cfg.SelectedValue.ToString()).Equals(strAreaId))
                            {
                                Util.MessageInfo("SFU8417");
                                return;
                            }
                        }


                        if (!(dtProdId.AsEnumerable().Where(x => x.Field<string>("PRODID").Equals(DataTableConverter.GetValue(CslyRspnList.Rows[CslyRspnList.CurrentRow.Index].DataItem, "PRODID").ToString())).Count() > 0))
                        {
                            Util.MessageInfo("SFU8408");
                            return;
                        }

                        DataTableConverter.SetValue(CslyRspnList.Rows[CslyRspnList.CurrentRow.Index].DataItem, sColName, true);
                    }
                    else if (strChkValue == "1" || strChkValue == "True")
                    {
                        DataTableConverter.SetValue(CslyRspnList.Rows[CslyRspnList.CurrentRow.Index].DataItem, sColName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean CslyRegValidation()
        {
            Boolean bChk = false;

            try
            {
                DataTable dt = DataTableConverter.Convert(CslyRspnList.ItemsSource).AsEnumerable()
                                                                                 .Where(x => x.Field<string>("CHK") == "True").CopyToDataTable();

                /*
                 * 제품 비교 부분
                 */
                DataView dtView = dt.DefaultView;
                DataTable dtProdIdTemp = dtView.ToTable(true, new string[] { "PRODID" });
                object[] arrProdIdObj = dtProdIdTemp.Select().Select(x => x["PRODID"]).ToArray();
                string[] arrProdIdStr = arrProdIdObj.Cast<string>().ToArray();

                foreach (string strProdId in arrProdIdStr)
                {
                    if (DataTableConverter.Convert(dgShippingWait.ItemsSource).Select("CHK = 1 AND PRODID <>'" + strProdId + "'").Count() > 0)
                    {
                        Util.MessageInfo("SFU8410");
                        return bChk;
                    }
                }

                /*
                 *  2021.10.04 염규범 선임
                 *  출고시에 동을 선택하는게 아니라, 출하처, SHIPTO 로 

                DataTable dtAreaIdTemp = dtView.ToTable(true, new string[] { "AREAID" });
                object[] arrAreaIdObj = dtAreaIdTemp.Select().Select(x => x["AREAID"]).ToArray();
                string[] arrAreaStr = arrAreaIdObj.Cast<string>().ToArray();

                foreach (string strAreaId in arrAreaStr)
                {
                    if (!cboShipTo_cfg.SelectedValue.ToString().Equals(strAreaId))
                    {
                        Util.MessageInfo("SFU8411");
                        return bChk;
                    }
                }

                */

                bChk = true;

                return bChk;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bChk;
            }
        }

        private string CslySelAeaId(string strShipToId)
        {
            string strAreaId = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "CSLY_UI_SHIPTO_AREAID_VALIDATION";
                dr["CMCODE"] = strShipToId;
                dr["ATTRIBUTE1"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0) strAreaId = dtRslt.Rows[0]["ATTRIBUTE2"].ToString();


                return strAreaId;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return strAreaId;
            }
        }

        private void CslyRspnList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Presenter == null || e.Cell.Value == DBNull.Value) return;

                if (e.Cell.Column.Name.Equals("AVA_RSPN_QTY"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void BtnPackOutExpand_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                if (OutArea.RowDefinitions[3].Height.Value != 0) return;
                OutArea.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                btnSearch_PackOutCsly.Visibility = Visibility.Visible;

                string strProdId = string.Empty;

                if (dgPackOut.GetRowCount() > 0)
                {
                    DataView dtView = DataTableConverter.Convert(dgPackOut.ItemsSource).DefaultView;
                    DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                    object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                    string[] arrStr = arrObj.Cast<string>().ToArray();
                    strProdId = string.Join(",", arrStr);
                }

                SearchPackOutCslyRspnHist(strProdId);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void BtnPackOutExpand_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OutArea.RowDefinitions[3].Height.Value == 0) return;

                OutArea.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                btnSearch_PackOutCsly.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BtnSearch_PackOut_Csly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProdId = string.Empty;

                if (dgPackOut.GetRowCount() > 0)
                {
                    DataView dtView = DataTableConverter.Convert(dgPackOut.ItemsSource).DefaultView;
                    DataTable dtTemp = dtView.ToTable(true, new string[] { "PRODID" });
                    object[] arrObj = dtTemp.Select().Select(x => x["PRODID"]).ToArray();
                    string[] arrStr = arrObj.Cast<string>().ToArray();
                    strProdId = string.Join(",", arrStr);
                }

                SearchPackOutCslyRspnHist(strProdId);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchPackOutCslyRspnHist(string strProdId)
        {
            try
            {
                string sTemp = Util.NVC(cboArea_cfg.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RSPN_AREAID", typeof(string));
                RQSTDT.Columns.Add("SHOP_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                if (strProdId != null && !string.IsNullOrWhiteSpace(strProdId)) RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                // 기존 화면 내용 유지
                dr["RSPN_AREAID"] = string.IsNullOrEmpty(cboAreaAll.SelectedValue.ToString()) ? null : cboAreaAll.SelectedValue.ToString();
                if (strProdId != null && !string.IsNullOrWhiteSpace(strProdId)) dr["PRODID"] = strProdId;
                dr["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;


                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CSLY_RSPN", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(CslyPackOutRspnList);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(CslyPackOutRspnList, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CslyPackOutRspnList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CslyPackOutRspnList.CurrentRow == null || CslyPackOutRspnList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = CslyPackOutRspnList.CurrentColumn.Name;
                string strChkValue = "";

                if (!sColName.Contains("CHK"))
                {
                    return;
                }


                if (e.ChangedButton.ToString().Equals("Left"))
                {

                    int indexRow = CslyPackOutRspnList.CurrentRow.Index;
                    int indexColumn = CslyPackOutRspnList.CurrentColumn.Index;

                    strChkValue = Util.NVC(CslyPackOutRspnList.GetCell(indexRow, indexColumn).Value);

                    if (string.IsNullOrEmpty(strChkValue) || strChkValue.Equals(""))
                        return;

                    if (strChkValue == "0" || strChkValue == "False")
                    {

                        if (dgPackOut.GetRowCount() <= 0)
                        {
                            Util.MessageInfo("SFU8409");
                            return;
                        }

                        if (cboComp.SelectedValue.ToString().Equals("SELECT"))
                        {
                            Util.MessageInfo("SFU4096");
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgPackOut.ItemsSource).AsEnumerable().CopyToDataTable();

                        DataView dtView = dt.DefaultView;
                        DataTable dtProdId = dtView.ToTable(true, new string[] { "TOP_PRODID" });


                        if (!string.IsNullOrWhiteSpace(cboComp.SelectedValue.ToString()))
                        {
                            string strAreaId = CslyPackOutRspnList.GetValue(indexRow, "AREAID").ToString();

                            if (!CslySelAeaId(cboComp.SelectedValue.ToString()).Equals(strAreaId))
                            {
                                Util.MessageInfo("SFU8417");
                                return;
                            }
                        }


                        if (!(dtProdId.AsEnumerable().Where(x => x.Field<string>("TOP_PRODID").Equals(DataTableConverter.GetValue(CslyPackOutRspnList.Rows[CslyPackOutRspnList.CurrentRow.Index].DataItem, "PACK_PRODID").ToString())).Count() > 0))
                        {
                            Util.MessageInfo("SFU8408");
                            return;
                        }

                        DataTableConverter.SetValue(CslyPackOutRspnList.Rows[CslyPackOutRspnList.CurrentRow.Index].DataItem, sColName, true);
                    }
                    else if (strChkValue == "1" || strChkValue == "True")
                    {
                        DataTableConverter.SetValue(CslyPackOutRspnList.Rows[CslyPackOutRspnList.CurrentRow.Index].DataItem, sColName, false);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CslyPackOutRspnList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
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
                        if (string.Equals(e.Cell.Column.Name, "AVA_RSPN_QTY"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
    }
}





