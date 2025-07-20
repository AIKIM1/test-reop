/*************************************************************************************
 Created Date : 2016.09.19
      Creator : 이슬아D
   Decription : 대차 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.19  이슬아D : Initial Created.
  
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
//using System.Windows.Forms;

namespace LGC.GMES.MES.ProtoType03
{

    /// <summary>
    /// cnssalee01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnskgh01 : UserControl, IWorkArea
    {
        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        // 화면 표시용 DataTable
        DataTable dtMain = new DataTable();
        DataTable dtLocation = new DataTable();
        private Boolean bRunning = false;
        private DateTime sRptDttm = Convert.ToDateTime("2000-01-01");
        private double dCanvasWidth, dCanvasHeight;
        private String _CartMntType = "";

        #region Initialize   
        public cnskgh01()
        {
            InitializeComponent();
        }

        #endregion
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }       

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCartTypeCombo();
            //InitCompProcCombo();
                        
            // Timer
            TimerSetting();

            // GMES Open전까지 비 활성화
            cboDong.IsEnabled = false;
            cboElectrode.IsEnabled = false;
            cboProcess.IsEnabled = false;

            //Canvas 넓이 계산
            getCanvasWH();

            // Default Cart
            chkCartType.IsChecked = true;

            // Zone 설정
            setZoneInitPosition();            

            resetControl();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void resetControl()
        {
            cboCartType.SelectedIndex = 0;
            //cboProcess.SelectedIndex = 0;
            
            chkEmptyCart.IsChecked = false;
            txtCartID.Text = "";
            txtLOTID.Text = "";
            dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Today;
        }


        private void getCanvasWH()
        {
            // 전제 넓이 계산
            dCanvasWidth = Canvas_Cart01.ActualWidth;
            dCanvasHeight = Canvas_Cart01.ActualHeight;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (btnSearch.Tag.ToString() == "N")
            {
                // Msgbox
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Select CART or LOT");
                return;
            }

            _timer.Stop();

            // 모니터링 데이터 Reset → 조회조건 변경 시 적용
            dtMain.Clear();
            Canvas_Cart01.Children.Clear();
            Canvas_Zone01.Children.Clear();
            sRptDttm = Convert.ToDateTime("2000-01-01");
            setZoneInitPosition();
            processMain();

            _timer.Start();
        }

        private void TimerSetting()
        {
            //_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += new EventHandler(timer_Tick);
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        //***************************************************************
        private void processMain()
        {
            if (bRunning == true)
            {
                return;
            }

            bRunning = true;
            //imgRunning.Visibility = Visibility.Visible;

            // 1. Update Data
            updateRTLSData();

            // 2. Tag
            if (dtMain.Rows.Count > 0)
            {
                // Target Data Check
                DataRow[] drNewCartData;
                drNewCartData = dtMain.Select("DELETE_YN = 'Y' OR DELETE_YN = 'N'");
                
                if (drNewCartData.Count() > 0)
                {
                    // 화면 갱신
                    DisplayMain(drNewCartData);
                }
            }

            if (_CartMntType == "CRT")
            {
                // 3. Location
                if (dtLocation.Rows.Count > 0)
                    DisplayLocation();
            }
            bRunning = false;
            //imgRunning.Visibility = Visibility.Hidden;
        }

        void updateRTLSData()
        {
            DataSet ds = getDataFromRTLSDB();

            if (ds != null && ds.Tables.Count > 0)
            {
                // Tag 적용
                if ((ds.Tables.IndexOf("TAG_DETAIL") > -1))
                {

                    if (dtMain.Columns.Count < 1)
                    {
                        dtMain = ds.Tables["TAG_DETAIL"].Copy();
                        dtMain.PrimaryKey = new DataColumn[] { dtMain.Columns["RFID_ID"] };
                    }
                    else
                    {
                        dtMain.Merge(ds.Tables["TAG_DETAIL"], false, MissingSchemaAction.Ignore);
                    }
                }

                // Location 적용
                dtLocation.Clear();
                if ((ds.Tables.IndexOf("SUM_BYLOC") > -1))
                {
                    dtLocation = ds.Tables["SUM_BYLOC"];
                }
            }
        }

        
        private DataSet getDataFromRTLSDB()
        {
            try
            {
                string sCartType = "";
                string sCartNo = "";
                string sEmptyYN = "";
                DateTime dtStDt;
                DateTime dtEtDt;
                DateTime sLastDate;

                if (chkCartType.IsChecked == true)
                {
                    //_CartMntType = "CRT";  // CRT : Cart, LOT : LOT

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
                    //_CartMntType = "LOT";  // CRT : Cart, LOT : LOT

                    

                    // 나머지 조회조건은 GMES Open 이후 적용

                }

                dtStDt = dtpDateFrom.SelectedDateTime;
                dtEtDt = dtpDateTo.SelectedDateTime;

                sLastDate = sRptDttm;

                DataSet dsInput = new DataSet();
                DataSet dsResult = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                // CRT
                INDATA.Columns.Add("RFID_TYPE", typeof(string));
                INDATA.Columns.Add("CART_NO", typeof(string));
                INDATA.Columns.Add("CART_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("EMPTY_CART_FLAG", typeof(string));

                // LOT
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("AREA_ABBR_CODE", typeof(string));
                INDATA.Columns.Add("PRODTYPE", typeof(string));
                INDATA.Columns.Add("PROC_ABBR_CODE", typeof(string));
                INDATA.Columns.Add("PROD_DTTM_ST", typeof(string));                
                INDATA.Columns.Add("PROD_DTTM_ED", typeof(string));

                INDATA.Columns.Add("RPT_DTTM", typeof(string));

                
                DataRow dr = INDATA.NewRow();
                dr["RFID_TYPE"] = _CartMntType;
                dr["CART_NO"] = sCartNo;
                dr["CART_TYPE_CODE"] = sCartType;
                dr["EMPTY_CART_FLAG"] = sEmptyYN;
                dr["LOTID"] = "";
                dr["AREA_ABBR_CODE"] = "";
                dr["PRODTYPE"] = "";
                dr["PROC_ABBR_CODE"] = "";
                dr["PROD_DTTM_ST"] = dtStDt;
                dr["PROD_DTTM_ED"] = dtEtDt;

                dr["RPT_DTTM"] = sLastDate;

                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_GET_MONITORING_MAIN", "INDATA", "TAG_DETAIL,SUM_BYLOC", dsInput, null);

                return dsResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void DisplayMain(DataRow[] drNewData)
        {
            string sZoneID = "";
            string sRfidID = "";
            string sCartID = "";        // Usert Control Name
            string sRfidName = "";
            string sItemType = "";
            string sCartEmptyYn = "";
            string sDeleteYN = "";
            string sLocationYN = "";
            DateTime sLastDttm;
            double dPstnX = -100;
            double dPstnY = -100;
            int iRowCount;

            iRowCount = dtMain.Rows.Count;

            for (int i = 0; i < iRowCount; i++)
            {
                sDeleteYN = dtMain.Rows[i]["DELETE_YN"].ToString();

                if (sDeleteYN != "Y" && sDeleteYN != "N")
                {
                    continue;
                }

                sZoneID = dtMain.Rows[i]["LOCATION_CODE"].ToString();
                sRfidID = dtMain.Rows[i]["RFID_ID"].ToString();
                sRfidName = dtMain.Rows[i]["RFID_NAME"].ToString();
                sItemType = dtMain.Rows[i]["CART_USG_TYPE_CODE"].ToString();
                sCartEmptyYn = dtMain.Rows[i]["EMPTY_FLAG"].ToString();
                sLastDttm = Convert.ToDateTime(dtMain.Rows[i]["RPT_DTTM"]);
                sLocationYN = dtMain.Rows[i]["LOCATION_YN"].ToString();

                if (dtMain.Rows[i]["PSTN_X"].ToString().Length > 0)
                {
                    dPstnX = Convert.ToDouble(dtMain.Rows[i]["PSTN_X"]);
                }
                else
                {
                    dPstnX = -1000;
                }

                if (dtMain.Rows[i]["PSTN_Y"].ToString().Length > 0)
                {
                    dPstnY = Convert.ToDouble(dtMain.Rows[i]["PSTN_Y"]);
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
                if (_CartMntType == "CRT")
                {
                    // Location 내부는 따로 처리함
                    if (sLocationYN == "N")
                    {
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
                                // Cart Setting
                                setCartPosition(sCartID, sRfidName, sItemType, sCartEmptyYn, dPstnX, dPstnY, _CartMntType, true);
                            }

                        }
                    }
                    // 창고 안으로 들어간 Cart 추적 후 삭제
                    else
                    {
                        removeCart(sCartID);
                    }
                }

                // Lot 처리 
                else
                {
                    // Location 내부표시는 처리제외
                    if (sLocationYN == "N")
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

                        }
                    }
                    // 창고 안으로 들어간 Cart 추적 후 삭제
                    else
                    {
                        removeCart(sCartID);
                    }
                }

            }

            // 완료처리
            for (int i = iRowCount - 1; i > -1; i--)
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

        private void DisplayLocation()
        {
            //LOCATION_CODE String
            // TAG_COUNT int16

            string sLocationCode;
            string sCnt;
            int iRowCount;

            iRowCount = dtLocation.Rows.Count;

            for (int i = 0; i < iRowCount; i++)
            {
                sLocationCode = dtLocation.Rows[i]["LOCATION_CODE"].ToString();
                
                if (dtLocation.Rows[i]["TAG_COUNT"].ToString().Length > 0)
                {
                    sCnt = dtLocation.Rows[i]["TAG_COUNT"].ToString();
                }
                else
                {
                    sCnt = "";
                }
                setLocationCnt(sLocationCode, sCnt);
            }
        }

        // Cart Object 생성 및 이동
        private void setCartPosition(string pCartID, string pCartNo, string pCartType, string pEmptyYn, double pPosX, double pPosY, string pSearchType, Boolean pVisibleFlag)
        {
            Boolean bUpdateYn = false;

            // Cart 찾기
            foreach (FrameworkElement fe in Canvas_Cart01.Children)
            {
                if (fe.Name == pCartID)
                {
                    // Cart 존재 시 좌표이동
                    bUpdateYn = true;
                    setCanvasCartPosition(fe, pPosX, pPosY, "C");
                    break;
                }
            }

            // Cart 미 존재 시 생성 후 좌표이동
            if (bUpdateYn == false)
            {
                cnskgh02 Carts = new cnskgh02();

                Carts = new cnskgh02(pCartID, pCartNo, pCartType, pEmptyYn, pSearchType);
                Carts.Name = pCartID;
                Canvas_Cart01.Children.Add(Carts);
                setCanvasCartPosition(Carts, pPosX, pPosY, "C");

                if (pVisibleFlag == false)
                {
                    Carts.Visibility = Visibility.Hidden;
                }
            }
        }

        // Location Count 
        private void setLocationCnt(string pZoneID, string pCartCnt)
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

                        cnskgh04 uZone = new cnskgh04();
                        uZone = new cnskgh04(pZoneID, sLocNM, pCartCnt, "N", _CartMntType);
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
                dMarginX = 3.5;
                dMarginY = 4.5;
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
                if (fe.Name == pCartID)
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
            double dPosX = 0, dPosY = 0;
            DataTable dtZoneInit = new DataTable();

            dtZoneInit = getLocationInit();

            iRowCount = dtZoneInit.Rows.Count;

            for (int i = 0; i < iRowCount; i++)
            {
                sLocCD = dtZoneInit.Rows[i]["CMCODE"].ToString();
                sLocNM = dtZoneInit.Rows[i]["CMCDNAME_L"].ToString();

                if (dtZoneInit.Rows[i]["ATTRIBUTE1"].ToString().Length > 0)
                {
                    dPosX = Convert.ToDouble(dtZoneInit.Rows[i]["ATTRIBUTE1"]);
                }
                else
                {
                    dPosX = -1000;
                }

                if (dtZoneInit.Rows[i]["ATTRIBUTE2"].ToString().Length > 0)
                {
                    dPosY = Convert.ToDouble(dtZoneInit.Rows[i]["ATTRIBUTE2"]);
                }
                else
                {
                    dPosY = -1000;
                }

                //sLocYN = dtZoneInit.Rows[i]["CMCDNAME"].ToString();


                if (dPosX > 0 && dPosY > 0)
                {
                    cnskgh04 uZone = new cnskgh04();
                    uZone = new cnskgh04(sLocCD, sLocNM, "0", "N", _CartMntType);
                    uZone.Name = sLocCD;
                    uZone.Tag = sLocNM + ":0";
                    Canvas_Zone01.Children.Add(uZone);

                    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");
                }
            }
        }

        private DataTable getLocationInit()
        {
            try
            {
                DataTable dsResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_LOCATION_CODE";
                RQSTDT.Rows.Add(dr);

                dsResult = new ClientProxy().ExecuteServiceSync("COR_SEL_COMMONCODE_BAS", "RQSTDT", "RSLTDT", RQSTDT);

                return dsResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
           
        }

        //private static Boolean checkZoneFlag(string pZoneID)
        //{
        //    Boolean bZoneFlag;

        //    // Location 변경할것
        //    // if (pZoneID == "E6N101" || pZoneID == "E6N102" || pZoneID == "E6U102" || pZoneID == "E6P102" || pZoneID == "E6L101" || pZoneID == "E6L102" || pZoneID == "E6U101" || pZoneID == "E6J101" || pZoneID == "E6J102")
        //    if (pZoneID == "E5NT101" || pZoneID == "E5NT102" || pZoneID == "E5PC101" || pZoneID == "E5SI102" || pZoneID == "E5PC102" || pZoneID == "E5SL101" || pZoneID == "E5SL102" || pZoneID == "E5SI101" || pZoneID == "E5JB101" || pZoneID == "E5JB102")
        //    {
        //        bZoneFlag = true;
        //    }
        //    else
        //    {
        //        bZoneFlag = false;
        //    }

        //    return bZoneFlag;
                
        //}
        

        // init CartType
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
                dr["CMCDTYPE"] = "RTLS_CARTTYPE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("COR_SEL_COMMONCODE_BAS", "RQSTDT", "RSLTDT", RQSTDT);
                cboCartType.DisplayMemberPath = "CMCDNAME"; 
                cboCartType.SelectedValuePath = "CMCODE";
                cboCartType.ItemsSource = AddStatus(dtResult, "CMCODE", "CMCDNAME").Copy().AsDataView();
                //cboCartType.ItemsSource = DataTableConverter.Convert(dtResult);
                cboCartType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "전체";
            dr[sValue] = "ALL";
            dt.Rows.InsertAt(dr, 0);

            return dt;            
        }

        private void changeSearchType(string pType)
        {
            var bc = new BrushConverter();

            btnSearch.Tag = pType;

            if (pType == "C")
            {
                _CartMntType = "CRT";
                gCartType.Background = (Brush)bc.ConvertFrom("#FFF9DDDD");
                gLotType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                txtLOTID.IsEnabled = false;
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
                cboCartType.IsEnabled = true;
                chkEmptyCart.IsEnabled = true;
                txtCartID.IsEnabled = true;
            }
            else if(pType == "L")
            {
                _CartMntType = "LOT";
                gCartType.Background = (Brush)bc.ConvertFrom("#fff2f2f2");
                gLotType.Background = (Brush)bc.ConvertFrom("#FFF9DDDD");
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
        }

 
        private void chkCartType_Checked(object sender, RoutedEventArgs e)
        {
            chkLotype.IsChecked = false;
            changeSearchType("C");
            _timer.Stop();
        }

        private void chkCartType_Unchecked(object sender, RoutedEventArgs e)
        {
            changeSearchType("N");
            _timer.Stop();
        }

        private void chkLotype_Checked(object sender, RoutedEventArgs e)
        {
            chkCartType.IsChecked = false;
            changeSearchType("L");
            _timer.Stop();
        }

        private void chkLotype_Unchecked(object sender, RoutedEventArgs e)
        {
            changeSearchType("N");
            _timer.Stop();
        }

        private void btnTest2_Click(object sender, RoutedEventArgs e)
        {
            


        }

        #region 향후 삭제

        //// Cart의 Tag로 이전 Zone 찾기 & Cart 수량 -1
        //private void setOldZoneMinusCnt(string pZoneID)
        //{

        //    foreach (FrameworkElement fe in Canvas_Zone01.Children)
        //    {
        //        // 기존 Cart가 Zone 내에 있다면 해당 Zone에서 Cart 수량 -1
        //        if (fe.Name == pZoneID)
        //        {
        //            int iCartCnt = 0;
        //            double dPosX, dPosY;

        //            iCartCnt = Convert.ToInt16(fe.Tag.ToString());
        //            if (iCartCnt > 0)
        //            {
        //                iCartCnt--;
        //            }
        //            else
        //            {
        //                iCartCnt = 0;
        //            }

        //            dPosX = Canvas.GetLeft(fe);
        //            dPosY = Canvas.GetTop(fe);

        //            Canvas_Zone01.Children.Remove(fe);

        //            cnskgh04 uZone = new cnskgh04();

        //            uZone = new cnskgh04(pZoneID, iCartCnt.ToString());
        //            uZone.Name = pZoneID;
        //            uZone.Tag = iCartCnt.ToString();
        //            Canvas_Zone01.Children.Add(uZone);

        //            Canvas.SetLeft(uZone, dPosX);
        //            Canvas.SetTop(uZone, dPosY);

        //            break;
        //        }
        //    }
        //}

        //private void cboCartType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    //changeSearchType("C");
        //}

        //private void chkEmptyCart_Checked(object sender, RoutedEventArgs e)
        //{
        //    //changeSearchType("C");
        //}

        //private void txtCartID_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //changeSearchType("C");
        //}

        //private void cboDong_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    //changeSearchType("L");
        //    //chkLotype.IsChecked = true;
        //}

        //private void cboElectrode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    //changeSearchType("L");
        //}

        //private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    //changeSearchType("L");
        //}

        //private void txtLOTID_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //changeSearchType("L");
        //}

        //private void dtpDateFrom_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    //changeSearchType("L");
        //}

        //private void dtpDateTo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    //changeSearchType("L");
        //}





        //// init Comp Process
        //private void InitCompProcCombo()
        //{
        //    try
        //    {
        //        // Get Cart Type
        //        cboCompProc.Text = string.Empty;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["CMCDTYPE"] = "RTLS_CARTTYPE";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("COR_SEL_COMMONCODE_BAS", "RQSTDT", "RSLTDT", RQSTDT);
        //        cboCompProc.DisplayMemberPath = "CMCODE";
        //        cboCompProc.SelectedValuePath = "CMCDNANE";
        //        cboCompProc.ItemsSource = DataTableConverter.Convert(dtResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        //private void cboLOTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    txtPouchID.Text = string.Empty;
        //    SetMappingList(string.Empty, e.NewValue.ToString());
        //}


        //private void txtProduct_KeyDown(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void CreateImg(int dNewX, int dNewY, int icnt)
        //{
        //    //double dNewX = 0, dNewY = 0;

        //    //dNewX = Convert.ToDouble(txtCartID.Text);
        //    //dNewY = Convert.ToDouble(txtLOTID.Text);

        //    Image pImg = new Image();

        //    if (icnt == 1)
        //    {
        //        pImg.Source = new BitmapImage(new Uri("D:\\#.Secure Work Folder\\문서\\Image\\cart_m_e.png"));
        //    }
        //    else
        //    {
        //        pImg.Source = new BitmapImage(new Uri("D:\\#.Secure Work Folder\\문서\\Image\\cart_m_f.png"));
        //    }
        //    pImg.Width = 30;
        //    pImg.Height = 30;

        //    this.Canvas_Cart01.Children.Add(pImg);

        //    Canvas.SetTop(pImg, dNewY);
        //    Canvas.SetLeft(pImg, dNewX);

        //    Label label = new Label(); //레이블 생성
        //    label.Content = "TestLabel"; //레이블 텍스트 입력하기 text는 string

        //    label.Width = 60;
        //    label.Height = 30;

        //    this.Canvas_Cart01.Children.Add(label);

        //    Canvas.SetTop(label, dNewY + 30);
        //    Canvas.SetLeft(label, dNewX);


        //}


        //private void InsertTest()
        //    {
        //        //dtMain.Columns.Add("CART_NO", typeof(int));
        //        //dtMain.Columns.Add("RFID_ID", typeof(string));
        //        //dtMain.Columns.Add("RFID_NAME", typeof(string));
        //        //dtMain.Columns.Add("RFID_TYPE", typeof(string));
        //        //dtMain.Columns.Add("PSTN_X", typeof(string));
        //        //dtMain.Columns.Add("PSTN_Y", typeof(string));
        //        //dtMain.Columns.Add("PSTN_Z", typeof(string));
        //        //dtMain.Columns.Add("RFID_CNT", typeof(string));
        //        //dtMain.Columns.Add("EMPTY_FLAG", typeof(string));
        //        //dtMain.Columns.Add("ZONE_ID", typeof(string));
        //        //dtMain.Columns.Add("EQIP_ID", typeof(string));
        //        //dtMain.Columns.Add("RPT_DTTM", typeof(string));
        //        //dtMain.Columns.Add("STATUS", typeof(string));

        //        //dtMain.Rows.Add("1", "CART1", "C1111C1111CC", "M", "110", "30", "0", "2", "N", "Z1", "EQPT1", "2016-09-21 16:21:29.177", "C");
        //        //dtMain.Rows.Add("2", "CART2", "C2222C2222CC", "S", "50", "160", "0", "5", "Y", "Z2", "EQPT2", "2016-09-21 16:21:29.177", "C");
        //        //dtMain.Rows.Add("3", "CART3", "C3333C3333CC", "M", "120", "110", "0", "5", "Y", "Z3", "EQPT3", "2016-09-21 16:21:29.177", "C");
        //        //dtMain.Rows.Add("4", "CART4", "C4444C4444CC", "W", "200", "300", "0", "5", "N", "Z4", "EQPT4", "2016-09-21 16:21:29.177", "C");
        //        //dtMain.Rows.Add("5", "CART5", "C5555C5555CC", "S", "0", "0", "0", "5", "N", "Z5", "EQPT5", "2016-09-21 16:21:29.177", "C");
        //        ////dtMain.Rows.Add("6", "CART", "", "", "", "", "0", "5", "", "", "", "", "" );
        //    }


        //private DataTable getDataFromRTLSDB()
        //{
        //    try
        //    {
        //        string sCartType = "";
        //        string sCartNo = "";
        //        string sEmptyYN = "";
        //        string sStDt = "";
        //        string sEtDt = "";
        //        DateTime sLastDate;

        //        if (chkCartType.IsChecked == true)
        //        {
        //            _CartMntType = "CRT";  // CRT : Cart, LOT : LOT

        //            sCartType = Util.NVC(cboCartType.SelectedValue);
        //            if (sCartType == "ALL")
        //                sCartType = null;

        //            sCartNo = txtCartID.Text;
        //            if (sCartNo.Equals(""))
        //                sCartNo = null;

        //            if (chkEmptyCart.IsChecked == true)
        //            {
        //                sEmptyYN = "Y";
        //            }
        //            else
        //            {
        //                sEmptyYN = "N";
        //            }
        //        }
        //        else
        //        {
        //            _CartMntType = "LOT";  // CRT : Cart, LOT : LOT

        //            sStDt = dtpDateFrom.SelectedDateTime.ToString("YYYYMMDD");
        //            sEtDt = dtpDateTo.SelectedDateTime.ToString("YYYYMMDD");

        //            // 나머지 조회조건은 GMES Open 이후 적용

        //        }

        //        sLastDate = sRptDttm;

        //        DataTable inDataTable = new DataTable();

        //        // CRT
        //        inDataTable.Columns.Add("RFID_TYPE", typeof(string));
        //        inDataTable.Columns.Add("CART_NO", typeof(string));
        //        inDataTable.Columns.Add("CART_TYPE_CODE", typeof(string));
        //        inDataTable.Columns.Add("EMPTY_CART_FLAG", typeof(string));

        //        // LOT
        //        inDataTable.Columns.Add("PROCID", typeof(string));
        //        inDataTable.Columns.Add("PRODID", typeof(string));
        //        inDataTable.Columns.Add("LOTID", typeof(string));
        //        inDataTable.Columns.Add("STDT", typeof(string));
        //        inDataTable.Columns.Add("EDDT", typeof(string));

        //        inDataTable.Columns.Add("RPT_DTTM", typeof(DateTime));

        //        DataRow newRow = inDataTable.NewRow();

        //        newRow["RFID_TYPE"] = _CartMntType;
        //        newRow["CART_NO"] = sCartNo;
        //        newRow["CART_TYPE_CODE"] = sCartType;
        //        newRow["EMPTY_CART_FLAG"] = sEmptyYN;

        //        newRow["PROCID"] = "";
        //        newRow["PRODID"] = "";
        //        newRow["LOTID"] = "";
        //        newRow["STDT"] = sStDt;
        //        newRow["EDDT"] = sEtDt;

        //        newRow["RPT_DTTM"] = sLastDate;

        //        inDataTable.Rows.Add(newRow);

        //        DataTable outDataTable = new DataTable();

        //        outDataTable = new ClientProxy().ExecuteServiceSync("BR_RTLS_GET_MONITORING_MAIN", "INDATA", "OUTDATA", inDataTable);

        //        return outDataTable;

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return null;
        //    }
        //}

        //private void setZoneInitPosition2()
        //{
        //    string sZoneID;
        //    double dPosX=0, dPosY=0;

        //    cnskgh04 uZone = new cnskgh04();

        //    sZoneID = "E5NT101";  // 
        //    //sZoneID = "E6N101";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);

        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5NT102";  // 
        //    //sZoneID = "E6N102";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5PC101"; //
        //    //sZoneID = "N/A";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5SI102";  // 
        //    //sZoneID = "E6U102";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5PC102";  // 
        //    //sZoneID = "E6P102";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5SL101";  // 
        //    //sZoneID = "E6L101";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5SL102";  // 
        //    //sZoneID = "E6L102";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5SI101";  // 
        //    //sZoneID = "E6U101";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5JB101";  // 
        //    //sZoneID = "E6J101";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //    sZoneID = "E5JB102";  // 
        //    //sZoneID = "E6J102";
        //    getZonePosition(sZoneID, ref dPosX, ref dPosY);
        //    uZone = new cnskgh04(sZoneID, "0");
        //    uZone.Name = sZoneID;
        //    Canvas_Zone01.Children.Add(uZone);
        //    setCanvasCartPosition(uZone, dPosX, dPosY, "Z");

        //}

        //// Zone ID별 Position 
        //private void getZonePosition(string pZoneID, ref double pPosX, ref double pPosY)
        //{
        //    switch (pZoneID)
        //    {
        //        case "E5NT101":  // E6N101
        //            pPosX = 6;
        //            pPosY = 74;
        //            break;
        //        case "E5NT102": // E6N102
        //            pPosX = 22.7;
        //            pPosY = 74;
        //            break;
        //        //case "E5EA104":
        //        //    pPosX = 25.2;
        //        //    pPosY = 57;
        //        //    break;
        //        case "E5PC101":  // N/A
        //            pPosX = 10.8;
        //            pPosY = 60.5;
        //            break;
        //        //case "E5EA103":
        //        //    pPosX = 17.7;
        //        //    pPosY = 43.4;
        //        //    break;
        //        case "E5SI102":  // E6U102
        //            pPosX = 22.2;
        //            pPosY = 60.5;
        //            break;
        //        case "E5PC102":  // E6P102
        //            pPosX = 30.5;
        //            pPosY = 60.5;
        //            break;
        //        case "E5SL101":  // E6L101
        //            pPosX = 6;
        //            pPosY = 51.5;
        //            break;
        //        case "E5SL102":  // E6L102
        //            pPosX = 27.0;
        //            pPosY = 51.5;
        //            break;
        //        case "E5SI101":  // E6U101
        //            pPosX = 42.0;
        //            pPosY = 51.5;
        //            break;
        //        case "E5JB101":  // E6J101
        //            pPosX = 62;
        //            pPosY = 51;
        //            break;
        //        case "E5JB102":  // E6J102
        //            pPosX = 86;
        //            pPosY = 51;
        //            break;
        //        //case "E5RP102":
        //        //    pPosX = 17.7;
        //        //    pPosY = 24.4;
        //        //    break;
        //        //case "E5RP101":
        //        //    pPosX = 17.7;
        //        //    pPosY = 8.1;
        //        //    break;
        //        //case "E5EA102":
        //        //    pPosX = 65.6;
        //        //    pPosY = 29.9;
        //        //    break;
        //        //case "E5EA101":
        //        //    pPosX = 65.6;
        //        //    pPosY = 2.7;
        //        //    break;
        //    }
        //}

        #endregion


    }


}
