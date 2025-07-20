/*************************************************************************************
 Created Date : 2016.09.19
      Creator : 이슬아D
   Decription : 재공/대차 위치 모니터링
                처리순서
                 1. Search Button → Auto Refresh 활성화 → processMain (& Timer Start) → tick(processMain)
                 2. processMain : updateRTLSData → DisplayMain → DisplayLocation
                 3. DisplayMain : dtMain Loop → Cart / LOT 별 Icon 생성 및 위치 이동
                 4. DisplayLocation : Location별 qty 표시
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.19  이슬아D : Initial Created.
  2016.09.23  김광호C : Modify / Canvas add
  
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    /// <summary>
    /// cnssalee01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_032 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        // 화면 표시용 DataTable
        DataTable dtMain = new DataTable();
        DataTable dtLocation = new DataTable();
        DataRow[] drLocation;
        private Boolean bRunning = false;
        private DateTime sRptDttm = Convert.ToDateTime("2000-01-01");   // 초기값은 NULL을 넘겨야 OUT_HISTORY값을 안가져옴(dtMain PK 오류방지)
        private double dCanvasWidth, dCanvasHeight;
        private String _CartMntType = "";

        public COM001_032()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void TimerSetting()
        {
            //_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            // OLD/NEW
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += new EventHandler(timer_Tick);
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitCartTypeCombo();
            AddDisplayType();
            //InitCompProcCombo();

            // Timer
            TimerSetting();

            // Location
            drLocation = getLocationInit();

            // GMES Open전까지 비 활성화
            cboDong.IsEnabled = false;
            cboElectrode.IsEnabled = false;
            cboProcess.IsEnabled = false;

            //Canvas 넓이 계산
            getCanvasWH();

            dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(-90);
            dtpDateTo.SelectedDateTime = DateTime.Today;

            // Default Cart
            chkCartType.IsChecked = true;            

            // Legend Display
            //setLegend();

        }
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;

            if (btnSearch.Tag.ToString() == "N")
            {
                chkCartType.IsChecked = true;
            }

            chkAutoRefresh.IsChecked = true;
        }

        // Batch
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // DB정보 읽어서 화면 처리
                processMain();
            }
            catch (Exception ex)
            {
                Util.Alert("Timer_Err" + ex.ToString());
            }
        }

        private void chkCartType_Checked(object sender, RoutedEventArgs e)
        {
            chkLotype.IsChecked = false;
            chkAutoRefresh.IsChecked = false;
            changeSearchType("C");
        }

        private void chkCartType_Unchecked(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
            changeSearchType("N");
        }

        private void chkLotype_Checked(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
            chkCartType.IsChecked = false;
            changeSearchType("L");
        }

        private void chkLotype_Unchecked(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
            changeSearchType("N");
        }

        private void chkAutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            dtMain.Clear();
            Canvas_Cart01.Children.Clear();
            Canvas_Zone01.Children.Clear();
            sRptDttm = Convert.ToDateTime("2000-01-01");
            setZoneInitPosition();
            //setLegend();

            processMain();
            _timer.Start();
        }

        private void chkAutoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            txtTest01.Text = "";
            txtTest02.Text = "";
            txtTest03.Text = "";
            txtTest04.Text = "";
            txtTest05.Text = "";
        }

        private void chkEmptyCart_Checked(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void chkEmptyCart_Unchecked(object sender, RoutedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void cboCartType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoutedEventArgs re = new RoutedEventArgs();
                btnSearch_Click(btnSearch, re);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void txtCartID_TextChanged(object sender, TextChangedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void txtLOTID_TextChanged(object sender, TextChangedEventArgs e)
        {
            chkAutoRefresh.IsChecked = false;
        }

        private void txtCartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoutedEventArgs re = new RoutedEventArgs();
                btnSearch_Click(btnSearch, re);
            }
        }

        private void cboDisplay_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            chkAutoRefresh.IsChecked = false;
        }



        // Cart Summary
        private void lblCartSummary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            COM001_032_SUMPOPUP wndSummaryCartLot = new COM001_032_SUMPOPUP();

            if (wndSummaryCartLot != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = "CT";

                C1WindowExtension.SetParameters(wndSummaryCartLot, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndSummaryCartLot.ShowModal()));
            }
        }

        private void lblLotSummary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            COM001_032_SUMPOPUP wndSummaryCartLot = new COM001_032_SUMPOPUP();

            if (wndSummaryCartLot != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = "LT";

                C1WindowExtension.SetParameters(wndSummaryCartLot, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndSummaryCartLot.ShowModal()));
            }
        }

        #endregion
        
        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void getCanvasWH()
        {
            // 전제 넓이 계산
            dCanvasWidth = Canvas_Cart01.ActualWidth;
            dCanvasHeight = Canvas_Cart01.ActualHeight;
        }

        private void processMain()
        {
            if (bRunning == true || chkAutoRefresh.IsChecked == false)
            {
                return;
            }

            bRunning = true;

            // 1. Update Data
            updateRTLSData();

            // 2. Tag
            if (dtMain.Rows.Count > 0)
            {
                // Cart Type
                //DisplayCartType();

                // Target Data Check
                DataRow[] drNewCartData;
                drNewCartData = dtMain.Select("DELETE_YN = 'Y' OR DELETE_YN = 'N'");

                if (drNewCartData.Count() > 0)
                {
                    // 화면 갱신
                    DisplayMain(drNewCartData);
                    //DisplayMainTest();

                    // 3. Location
                    currLocationCnt();
                }
            }
            bRunning = false;
        }

        void updateRTLSData()
        {
            DataTable dt = getDataFromRTLSDB();

            if (dt != null && dt.Rows.Count > 0)
            {
                if (dtMain.Rows.Count < 1)
                {
                    dtMain = dt.Copy();
                    dtMain.PrimaryKey = new DataColumn[] { dtMain.Columns["RFID_ID"] };
                }
                else
                {
                    dtMain.Merge(dt, false, MissingSchemaAction.Ignore);
                }

                // for test
                string srfid = "", sZoneID, sRptZoneID;
                DateTime sLastDttm;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sZoneID = dt.Rows[i]["LOCATION_CODE"].ToString();
                    sRptZoneID = dt.Rows[i]["RPT_ZONE_ID"].ToString();
                    srfid = dt.Rows[i]["RFID_ID"].ToString();
                    sLastDttm = Convert.ToDateTime(dt.Rows[i]["RPT_DTTM"]);

                    if (srfid == "CTXXXX000001")
                    {
                        string stemp;

                        stemp = srfid;
                    }
                }

                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    sZoneID = dtMain.Rows[i]["LOCATION_CODE"].ToString();
                    sRptZoneID = dtMain.Rows[i]["RPT_ZONE_ID"].ToString();
                    srfid = dtMain.Rows[i]["RFID_ID"].ToString();
                    sLastDttm = Convert.ToDateTime(dtMain.Rows[i]["RPT_DTTM"]);

                    if (srfid == "CTABE1600042")
                    {
                        string stemp2;

                        stemp2 = srfid;
                    }
                }
            }
        }


        private DataTable getDataFromRTLSDB()
        {
            try
            {
                string sCartType = "";
                string sCartNo = "";
                string sEmptyYN = "";
                string sLotID = "";
                DateTime dtStDt;
                DateTime dtEtDt;
                DateTime sLastDate;

                if (chkCartType.IsChecked == true)
                {
                    //_CartMntType = "CT";  // CT : Cart, LT : LOT

                    sCartType = Util.NVC(cboCartType.SelectedValue);
                    if (sCartType == "ALL")
                        sCartType = null;

                    sCartNo = txtCartID.Text;
                    if (sCartNo.Equals(""))
                        sCartNo = null;

                    if (chkEmptyCart.IsChecked == true)
                    {
                        sEmptyYN = "Y";
                    }
                    else
                    {
                        sEmptyYN = "N";
                    }
                }
                else
                {
                    //_CartMntType = "LT";  // CT : Cart, LT : LOT
                    // 나머지 조회조건은 GMES Open 이후 적용
                }

                sLotID = txtLOTID.Text;
                dtStDt = dtpDateFrom.SelectedDateTime;
                dtEtDt = dtpDateTo.SelectedDateTime;

                sLastDate = sRptDttm;

                DataTable inDataTable = new DataTable();
                DataTable outDataTable = new DataTable();


                // CT
                inDataTable.Columns.Add("RFID_TYPE", typeof(string));
                inDataTable.Columns.Add("CART_NO", typeof(string));
                inDataTable.Columns.Add("CART_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("EMPTY_CART_FLAG", typeof(string));

                // LT
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("AREA_ABBR_CODE", typeof(string));
                inDataTable.Columns.Add("PRODTYPE", typeof(string));
                inDataTable.Columns.Add("PROC_ABBR_CODE", typeof(string));
                inDataTable.Columns.Add("PROD_DTTM_ST", typeof(DateTime));
                inDataTable.Columns.Add("PROD_DTTM_ED", typeof(DateTime));

                inDataTable.Columns.Add("RPT_DTTM", typeof(DateTime));


                DataRow dr = inDataTable.NewRow();
                dr["RFID_TYPE"] = _CartMntType;
                dr["CART_NO"] = sCartNo;
                dr["CART_TYPE_CODE"] = sCartType;
                dr["EMPTY_CART_FLAG"] = sEmptyYN;
                dr["LOTID"] = sLotID;
                dr["AREA_ABBR_CODE"] = null;
                dr["PRODTYPE"] = null;
                dr["PROC_ABBR_CODE"] = null;
                dr["PROD_DTTM_ST"] = dtStDt;
                dr["PROD_DTTM_ED"] = dtEtDt;

                dr["RPT_DTTM"] = sLastDate;

                inDataTable.Rows.Add(dr);

                // OLD/NEW
                //outDataTable = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_LOCATION_TRACKING_MONITORING", "INDATA", "OUTDATA", inDataTable);
                outDataTable = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_RFID_PSTN_MONITORING", "INDATA", "OUTDATA", inDataTable);

                return outDataTable;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }


        private void DisplayMain(DataRow[] drNewData)
        {
            string sZoneID = "";
            string sRfidID = "";
            string sCartID = "";        // Usert Control Name
            string sRfidName = "";
            string sCartName = "";
            string sItemType = "";
            string sCartEmptyYn = "";
            string sDeleteYN = "";
            string sLocationYN = "";
            string sDisplayType = "";
            string sDisPlayCart = "";
            string sRptZoneID = "";     // RPT_ZONE_ID
            DateTime sLastDttm;
            double dPstnX = -100;
            double dPstnY = -100;
            int iRowCount;

            iRowCount = drNewData.Count();
            sDisplayType = Util.NVC(cboDisplayType.SelectedValue);

            for (int i = 0; i < iRowCount; i++)
            {
                sDeleteYN = drNewData[i]["DELETE_YN"].ToString();

                if (sDeleteYN != "Y" && sDeleteYN != "N")
                {
                    continue;
                }

                sZoneID = drNewData[i]["LOCATION_CODE"].ToString();
                sRptZoneID = drNewData[i]["RPT_ZONE_ID"].ToString();
                sRfidID = drNewData[i]["RFID_ID"].ToString();
                sRfidName = drNewData[i]["RFID_NAME"].ToString();
                sCartName = drNewData[i]["CART_NAME"].ToString();
                sItemType = drNewData[i]["CART_USG_TYPE_CODE"].ToString();
                sCartEmptyYn = drNewData[i]["EMPTY_FLAG"].ToString();
                sLastDttm = Convert.ToDateTime(drNewData[i]["RPT_DTTM"]);
                sLocationYN = drNewData[i]["LOCATION_YN"].ToString();

                if (drNewData[i]["PSTN_X"].ToString().Length > 0)
                {
                    dPstnX = Convert.ToDouble(drNewData[i]["PSTN_X"]);
                }
                else
                {
                    dPstnX = -1000;
                }

                if (drNewData[i]["PSTN_Y"].ToString().Length > 0)
                {
                    dPstnY = Convert.ToDouble(drNewData[i]["PSTN_Y"]);
                }
                else
                {
                    dPstnY = -1000;
                }

                if (sRptDttm == null)
                {
                    sRptDttm = sLastDttm;
                }
                else
                {
                    if (sLastDttm > sRptDttm)
                    {
                        sRptDttm = sLastDttm;
                    }
                }

                //
                txtUdttm.Text = sRptDttm.ToString("yyyy-MM-dd hh:mm:ss");

                //sCartID = "C" + sRfidID;
                sCartID = sRfidID;

                // Cart 처리
                if (_CartMntType == "CT")
                {
                    // 2016.10.25 kgh 주석처리 --> DDD 대비 좌표기준 처리로 변경
                    //// Location 내부는 따로 처리함
                    //if (sLocationYN == "N")
                    //{
                    // 삭제 Cart 처리
                    if (sDeleteYN == "Y")
                    {
                        // Cart Img Remove
                        removeCart(sCartID);
                    }
                    else
                    {
                        // 좌표값이 있는 경우만 처리
                        if (dPstnX > 0 && dPstnY > 0)
                        {
                            if (sDisplayType == "NA")
                            {
                                sDisPlayCart = "";
                            }
                            else if (sDisplayType == "CD")
                            {
                                sDisPlayCart = sRfidName;
                            }
                            else if (sDisplayType == "NM")
                            {
                                sDisPlayCart = sCartName;
                            }

                            // Cart Setting
                            setCartPosition(sCartID, sDisPlayCart, sItemType, sCartEmptyYn, dPstnX, dPstnY, _CartMntType, true);
                            //setCartPosition(sCartID, sDisPlayCart, sItemType, sCartEmptyYn, 28.4, 62.5, _CartMntType, true); 
                        }
                        // 좌표값이 없는 경우 삭제
                        else
                        {
                            removeCart(sCartID);
                        }
                    }
                }

                // Lot 처리 
                else
                {
                    // 삭제 Cart 처리
                    if (sDeleteYN == "Y")
                    {
                        // Cart Img Remove (LOT조회 시 Cart => Pouch ID로 추적)
                        removeCart(sCartID);
                    }
                    else
                    {
                        // 좌표값이 있는 경우만 처리
                        if (dPstnX > 0 && dPstnY > 0)
                        {
                            // Cart Setting
                            // Pouch_ID, LOTID, "", "", X, Y, Location, visible
                            setCartPosition(sCartID, sRfidName, sItemType, sCartEmptyYn, dPstnX, dPstnY, _CartMntType, true);
                        }
                        // 좌표값이 없는 경우 삭제
                        else
                        {
                            removeCart(sCartID);
                        }
                    }

                }

            }

            // 완료처리
            for (int i = dtMain.Rows.Count - 1; i > -1; i--)
            {
                sDeleteYN = dtMain.Rows[i]["DELETE_YN"].ToString();

                if (sDeleteYN != "Y" && sDeleteYN != "N")
                {
                    continue;
                }

                if (sDeleteYN == "Y")
                {
                    // Row Delete
                    DataRow dCartRow = dtMain.Rows[i];
                    dtMain.Rows.Remove(dCartRow);
                }
                else
                {
                    dtMain.Rows[i]["DELETE_YN"] = "";
                }
            }

        }

        // Cart Object 생성 및 이동
        private void setCartPosition(string pCartID, string pCartNo, string pCartType, string pEmptyYn, double pPosX, double pPosY, string pSearchType, Boolean pVisibleFlag)
        {
            Boolean bUpdateYn = false;

            // Cart 찾기
            foreach (FrameworkElement fe in Canvas_Cart01.Children)
            {
                if (fe.Name == "ZZZ" + pCartID)
                {
                    if (fe.Tag.ToString() == pEmptyYn)
                    {
                        // Cart 존재 시 좌표이동
                        bUpdateYn = true;
                        setCanvasCartPosition(fe, pPosX, pPosY, "C");
                    }
                    else
                    {
                        bUpdateYn = false;
                        Canvas_Cart01.Children.Remove(fe);
                    }

                    break;
                }
            }

            // Cart 미 존재 시 생성 후 좌표이동
            if (bUpdateYn == false)
            {
                try
                {
                    COM001_032_CRTCTL Carts = new COM001_032_CRTCTL();

                    Carts = new COM001_032_CRTCTL(pCartID, pCartNo, pCartType, pEmptyYn, pSearchType, dCanvasWidth, dCanvasHeight);
                    Carts.Name = "ZZZ" + pCartID;
                    Carts.Tag = pEmptyYn;
                    Canvas_Cart01.Children.Add(Carts);
                    //Canvas.SetZIndex(Carts, 10);
                    setCanvasCartPosition(Carts, pPosX, pPosY, "C");

                    if (pVisibleFlag == false)
                    {
                        Carts.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception ex)
                {
                    Util.Alert("Data_Err[010]" + ex.ToString());
                }
            }
        }

        // Location Count 
        private void setLocationCnt(string pZoneID, string pCartCnt, string pEmptyYn, string pCartType, string pCartNo, string pProdDttmSt, string pProdDttmEt, string pLotId)
        {
            double dPosX = 0, dPosY = 0;
            string sLocNM = "", sOldCnt = "";
            string[] sTemp;

            // Zone 찾기
            foreach (FrameworkElement fe in Canvas_Zone01.Children)
            {
                if (fe.Name == pZoneID)
                {
                    if (fe.Tag != null)
                    {
                        sTemp = fe.Tag.ToString().Split(':');
                        sLocNM = sTemp[0];
                        sOldCnt = sTemp[1];
                    }

                    if (sOldCnt != pCartCnt)
                    {
                        dPosX = Canvas.GetLeft(fe);
                        dPosY = Canvas.GetTop(fe);

                        COM001_032_LOCCTL uZone;
                        uZone = new COM001_032_LOCCTL(pZoneID, sLocNM, pCartCnt, "N", _CartMntType, pEmptyYn, pCartType, pCartNo, pProdDttmSt, pProdDttmEt, pLotId);
                        uZone.Name = pZoneID;
                        uZone.Tag = sLocNM + ":" + pCartCnt.ToString();
                        Canvas_Zone01.Children.Remove(fe);
                        Canvas_Zone01.Children.Add(uZone);
                        Canvas.SetLeft(uZone, dPosX);
                        Canvas.SetTop(uZone, dPosY);
                    }
                    break;
                }
            }
        }

        private void setCanvasCartPosition(FrameworkElement pFe, double pPosX, double pPosY, string sType)
        {
            double dRealWidth, dRealHeight;
            double dPosX, dPosY;
            double dMarginX = 0, dMarginY = 0;

            dRealWidth = 106;
            dRealHeight = 76;

            // Cart
            if (sType == "C")
            {
                dMarginX = 1.5;
                dMarginY = 3.0;
            }
            // Wharehouse
            else if (sType == "Z")
            {
                dMarginX = 0;
                dMarginY = 0;
            }

            // X는 왼쪽이 0
            dPosX = (dCanvasWidth / dRealWidth) * (pPosX - dMarginX);
            // Y는 아래쪽이 0
            dPosY = dCanvasHeight - (dCanvasHeight / dRealHeight) * (pPosY + dMarginY);

            Canvas.SetLeft(pFe, dPosX);
            Canvas.SetTop(pFe, dPosY);
        }

        // Cart Img 제거
        private void removeCart(string pCartID)
        {
            foreach (FrameworkElement fe in Canvas_Cart01.Children)
            {
                if (fe.Name == "ZZZ" + pCartID)
                {
                    Canvas_Cart01.Children.Remove(fe);
                    break;
                }
            }
        }

        private void setZoneInitPosition()
        {
            int iRowCount = 0;
            string sLocCD = "", sLocNM = "";
            string sEmptyYn = "", sCartType = "", sCartNo = "";
            string sLotId = "", dsStDt = "", dsEtDt = "";
            double dPosX = 0, dPosY = 0;

            if (chkEmptyCart.IsChecked == true)
            {
                sEmptyYn = "Y";
            }
            else
            {
                sEmptyYn = "N";
            }

            sCartType = Util.NVC(cboCartType.SelectedValue);
            if (sCartType == "ALL")
                sCartType = null;

            sCartNo = txtCartID.Text;
            if (sCartNo.Equals(""))
                sCartNo = null;

            sLotId = txtLOTID.Text;
            if (sLotId.Equals(""))
                sLotId = null;

            iRowCount = drLocation.Count();

            for (int i = 0; i < iRowCount; i++)
            {
                sLocCD = drLocation[i]["CMCODE"].ToString();
                sLocNM = drLocation[i]["CMCDNAME_L"].ToString();

                if (drLocation[i]["ATTRIBUTE1"].ToString().Length > 0)
                {
                    dPosX = Convert.ToDouble(drLocation[i]["ATTRIBUTE1"]);
                }
                else
                {
                    dPosX = -1000;
                }

                if (drLocation[i]["ATTRIBUTE2"].ToString().Length > 0)
                {
                    dPosY = Convert.ToDouble(drLocation[i]["ATTRIBUTE2"]);
                }
                else
                {
                    dPosY = -1000;
                }

                dsStDt = dtpDateFrom.SelectedDateTime.ToString();
                dsEtDt = dtpDateTo.SelectedDateTime.ToString();

                //sLocYN = drLocation[i]["CMCDNAME"].ToString();


                if (dPosX > 0 && dPosY > 0)
                {
                    COM001_032_LOCCTL uZone;
                    uZone = new COM001_032_LOCCTL(sLocCD, sLocNM, "0", "N", _CartMntType, sEmptyYn, sCartType, sCartNo, dsStDt, dsEtDt, sLotId);
                    uZone.Name = sLocCD;
                    uZone.Tag = sLocNM + ":0";
                    Canvas_Zone01.Children.Add(uZone);
                    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");
                }
            }
        }

        private DataRow[] getLocationInit()
        {
            try
            {
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_LOCATION_CODE";
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("COR_SEL_COMMONCODE_BAS", "RQSTDT", "RSLTDT", RQSTDT);

                drLocation = dtResult.Select("ATTRIBUTE3 = 'Y'");

                return drLocation;
            }
            catch (Exception ex)
            {
                Util.Alert("LocationInit_Err" + ex.ToString());
                return null;
            }
        }

        private void InitCartTypeCombo()
        {
            try
            {
                // Get Cart Type
                cboCartType.Text = string.Empty;
                //cboCartType.Items.Add(new { text = "전체", value = "ALL" });

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_CART_USG_TYPE_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("COR_SEL_COMMONCODE_BAS", "RQSTDT", "RSLTDT", RQSTDT);
                cboCartType.DisplayMemberPath = "CMCDNAME_L";
                cboCartType.SelectedValuePath = "CMCODE";
                cboCartType.ItemsSource = AddStatus(dtResult, "CMCODE", "CMCDNAME_L").Copy().AsDataView();
                //cboCartType.ItemsSource = DataTableConverter.Convert(dtResult);
                cboCartType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.Alert("CartTypeInit_Err" + ex.ToString());
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "전체";
            dr[sValue] = "ALL";
            dt.Rows.InsertAt(dr, 0);

            for (int i = 1; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["CMCDNAME_L"] = dt.Rows[i]["CMCODE"].ToString() + "(" + dt.Rows[i]["CMCDNAME_L"].ToString() + ")";
            }

            return dt;
        }

        private void AddDisplayType()
        {
            cboDisplayType.SelectedValuePath = "Key";
            cboDisplayType.DisplayMemberPath = "Value";
            cboDisplayType.Items.Add(new KeyValuePair<string, string>("NA", "N/A"));
            cboDisplayType.Items.Add(new KeyValuePair<string, string>("CD", "CODE"));
            cboDisplayType.Items.Add(new KeyValuePair<string, string>("NM", "NAME"));
            cboDisplayType.SelectedIndex = 1;
        }

        private void changeSearchType(string pType)
        {
            var bc = new BrushConverter();

            btnSearch.Tag = pType;

            if (pType == "C")
            {
                _CartMntType = "CT";
                gCartType.Background = (Brush)bc.ConvertFrom("#FFFFDADA");
                //gCartType.Background = (Brush)bc.ConvertFrom("#FFFFFFFF");
                gLotType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                txtLOTID.IsEnabled = false;
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
                cboCartType.IsEnabled = true;
                chkEmptyCart.IsEnabled = true;
                txtCartID.IsEnabled = true;
            }
            else if (pType == "L")
            {
                _CartMntType = "LT";
                gCartType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                gLotType.Background = (Brush)bc.ConvertFrom("#FFFFDADA");
                //gLotType.Background = (Brush)bc.ConvertFrom("#FFFFFFFF");
                txtLOTID.IsEnabled = true;
                dtpDateFrom.IsEnabled = true;
                dtpDateTo.IsEnabled = true;
                cboCartType.IsEnabled = false;
                chkEmptyCart.IsEnabled = false;
                txtCartID.IsEnabled = false;
            }
            else
            {
                _CartMntType = "";
                gLotType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                gCartType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                txtLOTID.IsEnabled = false;
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
                cboCartType.IsEnabled = false;
                chkEmptyCart.IsEnabled = false;
                txtCartID.IsEnabled = false;
            }

            // 모니터링 데이터 Reset → 조회조건 변경 시 적용
            dtMain.Clear();
            Canvas_Cart01.Children.Clear();
            Canvas_Zone01.Children.Clear();
            sRptDttm = Convert.ToDateTime("2000-01-01");
            setZoneInitPosition();
        }

        private void setLegend()
        {
            // Legend
            //Canvas_Zone01.Children.Add(ImgCartType);
            COM001_032_LEGENDCTL imgtemp = new COM001_032_LEGENDCTL();
            Canvas_Zone01.Children.Add(imgtemp);

            double dPosX, dPosY;
            // X는 왼쪽이 0
            dPosX = (dCanvasWidth / 106) * (90);
            // Y는 아래쪽이 0
            dPosY = dCanvasHeight - (dCanvasHeight / 76) * (75);

            Canvas.SetLeft(imgtemp, dPosX);
            Canvas.SetTop(imgtemp, dPosY);
        }

        private void currLocationCnt()
        {
            int iRowCount = 0;
            string sLocCD = "";
            string sQty = "";
            string sEmptyYn = "";
            string sCartType = "";
            string sCartNo = "";
            string filter = "";
            string sLotID = "", dsStDt = "", dsEtDt = "";
            DataRow[] drCurrentQty;

            iRowCount = drLocation.Count();

            // 조회조건 적용
            if (chkEmptyCart.IsChecked == true)
            {
                sEmptyYn = "Y";
            }
            else
            {
                sEmptyYn = "N";
            }

            sCartType = Util.NVC(cboCartType.SelectedValue);
            if (sCartType == "ALL")
                sCartType = null;

            sCartNo = txtCartID.Text;
            if (sCartNo.Equals(""))
                sCartNo = null;

            sLotID = txtLOTID.Text;
            if (sLotID.Equals(""))
                sLotID = null;

            dsStDt = dtpDateFrom.SelectedDateTime.ToString();
            dsEtDt = dtpDateTo.SelectedDateTime.ToString();

            for (int i = 0; i < iRowCount; i++)
            {
                sLocCD = drLocation[i]["CMCODE"].ToString();
                filter = string.Format("LOCATION_CODE='{0}'", sLocCD);

                drCurrentQty = dtMain.Select(filter);

                //dtMain.Select("DELETE_YN = 'Y' OR DELETE_YN = 'N'");

                if (drCurrentQty.Count() > 0)
                {
                    sQty = drCurrentQty.Count().ToString();
                }
                else
                {
                    sQty = "0";
                }

                setLocationCnt(sLocCD, sQty, sEmptyYn, sCartType, sCartNo, dsStDt, dsEtDt, sLotID);

            }
        }

        #endregion
        
    }
}
