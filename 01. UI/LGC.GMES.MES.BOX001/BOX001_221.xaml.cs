/*************************************************************************************
 Created Date : 2018.05.01
      Creator : JMC
   Decription : Notched Pancake 포장 화면
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.01  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media.Animation;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_221 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();

        private System.Windows.Threading.DispatcherTimer timer;
        //private LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING_OQC_RP sampling;
        //private DataTable OriginSamplingData;
        //private bool IsSamplingCheck = false;

        public string sTempLot_1 = string.Empty;
        public string sTempLot_2 = string.Empty;
        public string sNote2 = string.Empty;
        private string _APPRV_PASS_NO = string.Empty;

        public string sProcid = string.Empty;      //대표 공정 : PANKINGCARD 에서 사용
        public string sRepre_Lotid = string.Empty; //대표 LOT (첫번째 LOT) : PANKINGCARD의 포장카드 클릭시 사용

        public Boolean bReprint = true;

        private BOX001_221_PACKINGCARD window01;// = new BOX001_221_PACKINGCARD();      

        public bool bNew_Load = false;
        public DataTable dtPackingCard;

        public bool bCancel = false;

        public bool bUsedNtcPkgQty = false;


        public BOX001_221()
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
            listAuth.Add(btnPackOut);
            listAuth.Add(btnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //SetEvent();

            //txtLotID.Focus();

            #region Quality Check [자동차 1,2동만 적용] 
            //if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
            //{
            //    // SampleData 
            //    SetActSamplingData();

            //    timer.Tick -= timer_Start;
            //    timer.Tick += timer_Start;
            //    timer.Start();
            //}
            #endregion
        }

        //private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
        //    {
        //        timer.Stop();
        //        timer.Tick -= timer_Start;
        //    }
        //}


        #endregion

        #region Initialize
        private void Initialize()
        {

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom_Hist.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo_Hist.SelectedDateTime = (DateTime)System.DateTime.Now;

            #region Combo Setting
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID }; //출고처
            String[] sFilter1 = { "WH_DIVISION" }; //제품구분
            //String[] sFilter2 = { "WH_SHIPMENT" };
            //String[] sFilter3 = { "WH_TYPE" };
            //String[] sFilter4 = { "WH_STATUS" };
            String[] sFilter5 = { "SHIP_BOX_RCV_ISS_STAT_CODE" }; //출고상태
        


            _combo.SetCombo(cboTransLoc,      CommonCombo.ComboStatus.ALL,  sFilter: sFilter);                     //출고처(출고이력조회)
            _combo.SetCombo(cboTransLoc2,     CommonCombo.ComboStatus.NONE, sFilter: sFilter,  sCase: "TRANSLOC"); //출고처(포장출고)
            //_combo.SetCombo(cboTransLoc6,     CommonCombo.ComboStatus.NONE, sFilter: sFilter,  sCase: "TRANSLOC"); //출고처(통문증발행)
            _combo.SetCombo(cboTransLoc7,     CommonCombo.ComboStatus.ALL,  sFilter: sFilter,  sCase: "TRANSLOC"); //출고처(상세이력)
            _combo.SetCombo(cboTransLoc_Hist, CommonCombo.ComboStatus.ALL,  sFilter: sFilter,  sCase: "TRANSLOC"); //출고처(포장이력조회)

            _combo.SetCombo(cboType,          CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE"); //제품구분(포장출고)
            _combo.SetCombo(cboStatus2,       CommonCombo.ComboStatus.ALL,  sFilter: sFilter5, sCase: "COMMCODE"); //출고상태(출고이력조회)
            //_combo.SetCombo(cboStatus6,       CommonCombo.ComboStatus.ALL,  sFilter: sFilter5, sCase: "COMMCODE"); //출고상태(통문증발행)
            _combo.SetCombo(cboStatus7,       CommonCombo.ComboStatus.ALL,  sFilter: sFilter5, sCase: "COMMCODE"); //출고상태(상세이력)

            

            if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
            {
                TabOUT_Hist.Visibility   = Visibility.Collapsed;   //출고이력조회 TAB
                //Tab_GatePrint.Visibility = Visibility.Collapsed; //통문증발행 TAB
                Tab_Detail.Visibility    = Visibility.Collapsed;    //상세이력 TAB
            }
            else
            {
                Tab_BoxHist.Visibility   = Visibility.Collapsed;   //포장이력조회 TAB
            }

            ChkNtcPkgArea();

            // C20210806-000184 Notched Pancake 포장 시, 포장 수량 설정을 위한 txt, cbo 추가
            if (bUsedNtcPkgQty)
            {
                _combo.SetCombo(cboSelectQty, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
                txtSelectQty.Visibility = Visibility.Visible;
                cboSelectQty.Visibility = Visibility.Visible;
            }
            else
            {
                txtSelectQty.Visibility = Visibility.Collapsed;
                cboSelectQty.Visibility = Visibility.Collapsed;
            }
            #endregion

            // PANCAKE 고정요청 [2017-09-04]
            cboType.SelectedValue = "PANCAKE"; //포장출고TAB의 제품구분

            SetDtpEvent(); //LGCDatePicker Date 포멧 설정 이벤트

            txtLotID.Focus();
        }

        private void SetDtpEvent()
        {
            //this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom_Box.SelectedDataTimeChanged += dtpDateFrom_Box_SelectedDataTimeChanged;
            dtpDateTo_Box.SelectedDataTimeChanged += dtpDateTo_Box_SelectedDataTimeChanged;

            dtpDateFrom_Hist.SelectedDataTimeChanged += dtpDateFrom_Hist_SelectedDataTimeChanged;
            dtpDateTo_Hist.SelectedDataTimeChanged += dtpDateTo_Hist_SelectedDataTimeChanged;
        }

        private void ChkNtcPkgArea()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "NTC_PKG_QTY";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_NTC_QTY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bUsedNtcPkgQty = true;
                else
                    bUsedNtcPkgQty = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event

        #region LGCDatePicker Date 포멧 설정 event
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

        private void dtpDateFrom_Box_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Box.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Box.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Box_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Box.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Box.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom_Hist_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Hist.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Hist.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Hist_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Hist.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Hist.SelectedDateTime;
                return;
            }
        }
        #endregion


        private void btnRefresh_Click(object sender, RoutedEventArgs e) //포장출고 TAB의 초기화 버튼
        {
            Util.gridClear(dgOut);
            dgSub.Children.Clear();

            dgOut.IsEnabled = true;
            txtLotID.IsReadOnly = false;
            btnPackOut.IsEnabled = true;
            txtLotID.Text = "";
            sNote2 = string.Empty;
            txtLotID.Focus();

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e) //포장출고 TAB의 포장구성 버튼
        {
            try
            {
                if (dgOut.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                // C20210806-000184 Notched Pancake 포장 시, 선택한 포장 수량(cboSelectQty)와 포장 출고 대상(dgOut.GetRowCount) Validation 추가

                
                if (bUsedNtcPkgQty)
                {
                    string sQTY = cboSelectQty.SelectedValue.ToString();
                    bool bEuqal = ((System.Data.DataRowView)cboSelectQty.SelectedItem).Row.ItemArray[1].ToString().Contains("=") ? true : false;

                    if (bEuqal)
                    {
                        if (sQTY != dgOut.GetRowCount().ToString())
                        {
                            Util.MessageValidation("SFU5171");// 포장 출고 대상 수량과 선택한 수량이 맞지 않습니다.
                            return;
                        }
                    }
                    else
                    {
                        if(dgOut.GetRowCount() > (Convert.ToDecimal(sQTY)-1))
                        {
                            Util.MessageValidation("SFU5171");// 포장 출고 대상 수량과 선택한 수량이 맞지 않습니다.
                            return;
                        }
                    }
                    
                }
                string sLot_Type = cboType.SelectedValue.ToString();

                txtLotID.IsReadOnly = true;
                btnPackOut.IsEnabled = false;

                //dgOut.IsReadOnly = true;
                dgOut.IsEnabled = false;

                if (sLot_Type == "PANCAKE")
                {
                    string sTempProdName_1 = string.Empty;
                    string sTempProdName_2 = string.Empty;
                    string sPackingLotType1 = string.Empty;
                    string sPackingLotType2 = string.Empty;

                    bReprint = false;

                    int imsiCheck = 0;                // 설명:처리해야할 LOT 개수 판단
                    int iCheckCount = 0;

                    sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (i == 0)
                        {
                            sTempLot_1 = "";
                            sTempLot_1 += Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        }
                        else
                            sTempLot_1 += "," + Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                    }


                    loadingIndicator.Visibility = Visibility.Visible;

                    Get_Sub();

                    loadingIndicator.Visibility = Visibility.Collapsed;
                    txtLotID.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                
                dgOut.IsEnabled = true;
                txtLotID.IsReadOnly = false;
                btnPackOut.IsEnabled = true;
                txtLotID.Text = "";
                txtLotID.Focus();
                return;
            }
        }

        private void Get_Sub() //포장출고 TAB의 포장구성시 BOX001_221_PACKINGCARD 불러와서 ADD시킴
        {
            if (dgSub.Children.Count == 0)
            {
                bNew_Load = true;
                window01 = new BOX001_221_PACKINGCARD();      
                window01.BOX001_221 = this;
                window01.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub.Children.Add(window01);
            }
        }      

        private void btnSearch2_Click(object sender, RoutedEventArgs e) //출고이력조회 TAB의 조회 버튼
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Boxmapping_Master(); //출고이력조회 Search
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void txtBoxid_KeyDown(object sender, KeyEventArgs e) //출고이력조회 TAB의 BOXID TEXTBOX 
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Boxmapping_Master(); //출고이력조회 Search
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void Boxmapping_Master()
        {
            try
            {
                string sShipToID = string.Empty;
                string sStatus = string.Empty;
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                Util.gridClear(dgOutHIst);
                Util.gridClear(dgOutDetail);

                if (cboStatus2.SelectedIndex < 0 || cboStatus2.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
                }
                else
                {
                    sStatus = cboStatus2.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["OUTER_BOXID"] = txtBoxid.Text.Trim() == "" ? null : txtBoxid.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID.Text.Trim() == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutHIst);
                Util.GridSetData(dgOutHIst, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        } //출고이력조회 TAB의 조회

        private void Boxmapping_Detail(int idx) //출고이력조회 TAB의 Detail 조회
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;
                string sOUTER_BOXID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgOutHIst.Rows[idx].DataItem, "RCV_ISS_ID").ToString();
                sOUTER_BOXID = DataTableConverter.GetValue(dgOutHIst.Rows[idx].DataItem, "OUTER_BOXID").ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["OUTER_BOXID"] = sOUTER_BOXID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutDetail);
                Util.GridSetData(dgOutDetail, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e) //출고이력조회 TAB의 재발행 BUTTON
        {
            try
            {
                if (dgOutHIst.GetRowCount() == 0 || dgOutDetail.GetRowCount() == 0)
                {
                    Util.Alert("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();

                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["RCV_ISS_ID"] = sRcv_Iss_Id;

                RQSTDT3.Rows.Add(dr3);

                DataTable ReprintResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_INNER_BOX", "RQSTDT", "RSLTDT", RQSTDT3);

                if (ReprintResult2.Rows.Count == 3)
                    SteelCaseReprint();
                else
                    NormalReprint(); //출고이력조회 TAB의 재발행
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void NormalReprint() //출고이력조회 TAB의 재발행
        {
            try
            {
                string sType = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK"); //출고정보GRID에서 체크한 Row 가져옴

                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();
                string sOuter_Boxid = drChk[0]["OUTER_BOXID"].ToString();

                //
                //1.출고이력조회
                //
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRcv_Iss_Id;
                dr["CMCDTYPE"] = "PRDT_ABBR_CODE";

                RQSTDT.Rows.Add(dr);

                DataTable ReprintResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT", "RQSTDT", "RSLTDT", RQSTDT);

                if (ReprintResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }

                //
                //2.OUTERBOXID, BOXID, LOTID(생략됨), CSTID, 수량정보
                //
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LANGID", typeof(String));
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["RCV_ISS_ID"] = sRcv_Iss_Id;

                RQSTDT1.Rows.Add(dr1);

                DataTable CutidResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_CUTID", "RQSTDT", "RSLTDT", RQSTDT1);

                if (CutidResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }
                else if (CutidResult.Rows.Count == 1)
                {
                    //3.1 BOX에 포함된 SUB_BOX정보(개수)
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("OUTER_BOXID", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["OUTER_BOXID"] = sOuter_Boxid;

                    RQSTDT3.Rows.Add(dr3);

                    DataTable dtCnt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTER_BOXID_CNT", "RQSTDT", "RSLTDT", RQSTDT3);

                    if (dtCnt.Rows.Count > 1)
                    {
                        sType = "TWO";
                    }
                    else
                    {
                        sType = "ONE";
                    }
                }
                else
                {
                    sType = "ONE";
                }

                string sBoxtype = string.Empty;
                string sBoxid = drChk[0]["OUTER_BOXID"].ToString();


                //BOXID의 재공정보 확인
                DataTable RQSTDT4 = new DataTable();
                RQSTDT4.TableName = "RQSTDT";
                RQSTDT4.Columns.Add("LANGID", typeof(String));
                RQSTDT4.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr4 = RQSTDT4.NewRow();
                dr4["LANGID"] = LoginInfo.LANGID;
                dr4["OUTER_BOXID"] = sBoxid;

                RQSTDT4.Rows.Add(dr4);

                DataTable Reprint_Main = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_PRINT", "RQSTDT", "RSLTDT", RQSTDT4);

                if (Reprint_Main.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                //BOX의 재공정보 확인 및 SUB_BOX 정보 및 포함된 LANE 수량 정보
                DataTable RQSTDT5 = new DataTable();
                RQSTDT5.TableName = "RQSTDT";
                RQSTDT5.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr5 = RQSTDT5.NewRow();
                dr5["OUTER_BOXID"] = sBoxid;

                RQSTDT5.Rows.Add(dr5);

                DataTable Reprint_Sub = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_PRINT_SUB_CELL", "RQSTDT", "RSLTDT", RQSTDT5);

                if (Reprint_Sub.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (Reprint_Main.Rows[0]["BOXTYPE"].ToString() == "CRT")
                {
                    sBoxtype = ObjectDic.Instance.GetObjectName("가대") + "#" + Reprint_Sub.Rows.Count;
                }
                else
                {
                    sBoxtype = "BOX";
                }

                string sPackageWay = ObjectDic.Instance.GetObjectName("조립포장카드") + " " + sBoxtype;
                string sVld = string.Empty;
                string sProdDate = string.Empty;
                string sVld2 = string.Empty;
                string sProdDate2 = string.Empty;
                string sPackingNo = string.Empty;
                string sPackingNo2 = string.Empty;
                string sModlid = string.Empty;
                string sFrom = string.Empty;
                string sTo = string.Empty;
                double iSum = 0;
                double iSum2 = 0;
                double iSum3 = 0;
                double iSum4 = 0;
                string sLotid = string.Empty;
                string sLotid2 = string.Empty;
                string sVer = string.Empty;
                string sVer2 = string.Empty;
                string sLane = string.Empty;
                string sLane2 = string.Empty;
                string sUnitCode = string.Empty;
                string sNote = string.Empty;
                string sOper_Desc = string.Empty;
                string sAbbrCode = string.Empty;
                string sLotid3 = string.Empty;
                string sLotid4 = string.Empty;

                // 환산자 처리
                decimal iConvSum = 0;
                decimal iConvSum2 = 0;
                decimal iConvSum3 = 0;
                decimal iConvSum4 = 0;

                string sTotal_M = string.Empty;
                string sTotal_C = string.Empty;
                string sTotal_M2 = string.Empty;
                string sTotal_C2 = string.Empty;
                string sTotal_C3 = string.Empty;
                string sM1 = string.Empty;
                string sC1 = string.Empty;
                string sM2 = string.Empty;
                string sC2 = string.Empty;

                string sD1 = string.Empty;
                string sD2 = string.Empty;

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                
                DateTime ProdDate;
                //if (ReprintResult.Rows[0]["BOXTYPE"].ToString() == "CRT")
                //{
                //    sBoxtype = ReprintResult.Rows.Count + ObjectDic.Instance.GetObjectName("가대");
                //}
                //else
                //{
                //    sBoxtype = "BOX";
                //}

                //string sPackageWay = ObjectDic.Instance.GetObjectName("조립포장카드") + " " + sBoxtype;

                for (int i = 0; i < CutidResult.Rows.Count; i++)
                {
                    if (i == 0)
                        sLotid = CutidResult.Rows[i]["CSTID"].ToString();
                    if (!sLotid.Equals(CutidResult.Rows[i]["CSTID"].ToString()))
                    {
                        sLotid2 = CutidResult.Rows[i]["CSTID"].ToString();
                    }
                }

                if (sLotid.Equals("")) //cstid가 없을 경우 : 조립에서는 CSTID가 없음
                {
                    //OUTERBOXID, BOXID, LOTID(생략됨), CSTID, 수량정보
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("LANGID", typeof(String));
                    RQSTDT3.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["LANGID"] = LoginInfo.LANGID;
                    dr3["RCV_ISS_ID"] = sRcv_Iss_Id;
                    dr3["CMCDTYPE"] = "PRDT_ABBR_CODE";

                    RQSTDT3.Rows.Add(dr3);

                    DataTable ReprintResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_ROLL", "RQSTDT", "RSLTDT", RQSTDT3);

                    if (ReprintResult2.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                        return;
                    }

                    for (int i = 0; i < ReprintResult2.Rows.Count; i++)
                    {
                        if (!ReprintResult2.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                        {
                            ProdDate = Convert.ToDateTime(ReprintResult2.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                        }
                        if (i == 0)
                            sLotid3 = ReprintResult2.Rows[i]["LOTID"].ToString();
                        if (!sLotid3.Equals(ReprintResult2.Rows[i]["LOTID"].ToString()))
                        {
                            sLotid4 = ReprintResult2.Rows[i]["LOTID"].ToString();
                            if (!ReprintResult2.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                            {
                                ProdDate = Convert.ToDateTime(ReprintResult2.Rows[i]["WIPSDTTM"].ToString());
                                sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                            }
                        }
                    }
                }

                if (sType == "ONE")
                {
                    for (int i = 0; i < ReprintResult.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }
                            if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                            {
                                ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                                sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                            }
/*
                            sPackingNo = ReprintResult.Rows[i]["OUTER_BOXID"].ToString();
                            sModlid = ReprintResult.Rows[i]["MODLID"].ToString();
                            sFrom = ReprintResult.Rows[i]["FROM_AREA"].ToString();
                            sTo = ReprintResult.Rows[i]["SHIPTO_NAME"].ToString();
                            iSum = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
                            sVer = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = ReprintResult.Rows[i]["LANE"].ToString();

                            //sLotid3 = ReprintResult.Rows[i]["LOTID"].ToString();
                            //sLotid4 = ReprintResult.Rows[i]["LOTID"].ToString();
                            sUnitCode = ReprintResult.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = ReprintResult.Rows[i]["PRDT_ABBR_CODE"].ToString();
*/


                            //BOXID 정보 변수에 담아두기
                            sPackingNo = Reprint_Main.Rows[i]["OUTER_BOXID"].ToString();
                            sModlid = Reprint_Main.Rows[i]["MODLID"].ToString();
                            sFrom = Reprint_Main.Rows[i]["EQSGNAME"].ToString();
                            sTo = Reprint_Main.Rows[i]["SHIPTO_NAME"].ToString();
                            sUnitCode = Reprint_Main.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = Reprint_Main.Rows[i]["PRDT_ABBR_CODE"].ToString();
                            sNote = Reprint_Main.Rows[i]["PACK_NOTE"].ToString();
                            sOper_Desc = Reprint_Main.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();

                            //SUB_BOX 정보 변수에 담아두기
                            iSum = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY2"].ToString());
                            sVer = Reprint_Sub.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = Reprint_Sub.Rows[i]["LANE"].ToString();
                            sLotid = Reprint_Sub.Rows[i]["BOXID"].ToString();



                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));                              

                                iConvSum = Math.Floor(Util.NVC_Decimal(iSum) * sConvLength) / 100;
                                iConvSum3 = Math.Floor(Util.NVC_Decimal(iSum3) * sConvLength) / 100;
                            }

                            //if (ReprintResult.Rows[i]["ISS_NOTE"].ToString() == "")
                            //{
                            //    sNote = null;
                            //}
                            //else
                            //{
                            //    sNote = ReprintResult.Rows[i]["ISS_NOTE"].ToString();
                            //}

                            if (ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                            {
                                sOper_Desc = null;
                            }
                            else
                            {
                                sOper_Desc = ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            }
                        }
                        else
                        {
                            if (CutidResult.Rows.Count == 2)
                            {
                                if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                                {
                                    sVld2 = null;
                                }
                                else
                                {
                                    string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                                    sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                                }

                                if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                                {
                                    ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                                    sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                                }

                                iSum2 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
                                iSum4 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
                                sVer2 = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                                sLane2 = ReprintResult.Rows[i]["LANE"].ToString();
                                //sLotid2 = CutidResult.Rows[1]["CSTID"].ToString();
                                if (string.Equals(sUnitCode, "EA"))
                                {
                                    decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                                    iConvSum2 = Math.Floor(Util.NVC_Decimal(iSum2) * sConvLength) / 100;
                                    iConvSum4 = Math.Floor(Util.NVC_Decimal(iSum4) * sConvLength) / 100;
                                }
                            }
                        }
                    }



                    if (CutidResult.Rows.Count == 1)
                    {
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            double total_M = Math.Floor(iSum);
                            sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", iConvSum) + "M";

                            double total_C = Math.Floor(iSum3);
                            sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", iConvSum3) + "M";
                        }
                        else
                        {
                            double total_M = Math.Floor(iSum);
                            sTotal_M = String.Format("{0:#,##0}", total_M);

                            double total_C = Math.Floor(iSum3);
                            sTotal_C = String.Format("{0:#,##0}", total_C);
                        }

                        sM1 = String.Format("{0:#,##0}", Math.Floor(iSum));
                        sM2 = "";
                        sC1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                        sC2 = "";

                        if (string.Equals(sUnitCode, "EA"))
                            sD1 = String.Format("{0:#,##0}", iConvSum3);
                        else
                            sD1 = String.Format("{0:#,##0}", Math.Floor(iSum3));

                        sD2 = "";
                    }
                    else
                    {
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            double total_M = Math.Floor(iSum) + Math.Floor(iSum2);
                            sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", (iConvSum + iConvSum2)) + "M";

                            double total_C = Math.Floor(iSum3) + Math.Floor(iSum4);
                            sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", (iConvSum3 + iConvSum4)) + "M";
                        }
                        else
                        {
                            double total_M = Math.Floor(iSum) + Math.Floor(iSum2);
                            sTotal_M = String.Format("{0:#,##0}", total_M);

                            double total_C = Math.Floor(iSum3) + Math.Floor(iSum4);
                            sTotal_C = String.Format("{0:#,##0}", total_C);
                        }

                        sM1 = String.Format("{0:#,##0}", Math.Floor(iSum));
                        sM2 = String.Format("{0:#,##0}", Math.Floor(iSum2));
                        sC1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                        sC2 = String.Format("{0:#,##0}", Math.Floor(iSum4));

                        if (string.Equals(sUnitCode, "EA"))
                        {
                            sD1 = String.Format("{0:#,##0}", iConvSum3);
                            sD2 = String.Format("{0:#,##0}", iConvSum4);
                        }
                        else
                        {
                            sD1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                            sD2 = String.Format("{0:#,##0}", Math.Floor(iSum4));
                        }
                    }

                    DataTable dtReprint = new DataTable();
                    DataRow drCrad = null;
                    /*
                                        if (sLotid.Equals(""))
                                        {

                                            dtReprint.Columns.Add("Title", typeof(string));
                                            dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                                            dtReprint.Columns.Add("PACK_NO", typeof(string));
                                            dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                                            dtReprint.Columns.Add("Transfer", typeof(string));

                                            dtReprint.Columns.Add("Total_Cell", typeof(string));
                                            dtReprint.Columns.Add("No1", typeof(string));
                                            dtReprint.Columns.Add("No2", typeof(string));
                                            dtReprint.Columns.Add("Lot1", typeof(string));
                                            dtReprint.Columns.Add("Lot2", typeof(string));

                                            dtReprint.Columns.Add("VLD_DATE1", typeof(string));
                                            dtReprint.Columns.Add("VLD_DATE2", typeof(string));
                                            dtReprint.Columns.Add("REG_DATE1", typeof(string));
                                            dtReprint.Columns.Add("REG_DATE2", typeof(string));
                                            dtReprint.Columns.Add("V1", typeof(string));

                                            dtReprint.Columns.Add("V2", typeof(string));
                                            dtReprint.Columns.Add("C1", typeof(string));
                                            dtReprint.Columns.Add("C2", typeof(string));
                                            dtReprint.Columns.Add("D1", typeof(string));
                                            dtReprint.Columns.Add("D2", typeof(string));

                                            dtReprint.Columns.Add("REMARK", typeof(string));
                                            dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                                            dtReprint.Columns.Add("V_DATE", typeof(string));
                                            dtReprint.Columns.Add("P_DATE", typeof(string));
                                            dtReprint.Columns.Add("OFFER_DESC", typeof(string));

                                            dtReprint.Columns.Add("PRODID", typeof(string));
                                            dtReprint.Columns.Add("Text5", typeof(string));
                                            dtReprint.Columns.Add("Text13", typeof(string));
                                            dtReprint.Columns.Add("Text7", typeof(string)); // LOT ID

                                            drCrad = dtReprint.NewRow();

                                            drCrad.ItemArray = new object[] { sPackageWay,
                                                                      sModlid,
                                                                      sPackingNo,
                                                                      "*" + sPackingNo + "*",
                                                                      sFrom + " -> " + sTo,
                                                                      sTotal_C,
                                                                      "1",
                                                                      "",
                                                                      sLotid3,
                                                                      sLotid4,
                                                                      sVld,
                                                                      sVld2,
                                                                      sProdDate,
                                                                      sProdDate2,
                                                                      sVer,
                                                                      sVer2,
                                                                      sC1,
                                                                      sC2,
                                                                      sD1,
                                                                      sD2,
                                                                      sNote,
                                                                      sUnitCode,
                                                                      sV_DATE,
                                                                      sP_DATE,
                                                                      sOper_Desc,
                                                                      Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                                      , ObjectDic.Instance.GetObjectName("수량").ToString() + " :"
                                                                      , ObjectDic.Instance.GetObjectName("수량").ToString()
                                                                      , ObjectDic.Instance.GetObjectName("LOT ID").ToString()
                                                                   };
                                        }
                                        else
                                        {

                                            dtReprint.Columns.Add("Title", typeof(string));
                                            dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                                            dtReprint.Columns.Add("PACK_NO", typeof(string));
                                            dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                                            dtReprint.Columns.Add("Transfer", typeof(string));
                                            dtReprint.Columns.Add("Total_M", typeof(string));
                                            dtReprint.Columns.Add("Total_Cell", typeof(string));
                                            dtReprint.Columns.Add("No1", typeof(string));
                                            dtReprint.Columns.Add("No2", typeof(string));
                                            dtReprint.Columns.Add("Lot1", typeof(string));
                                            dtReprint.Columns.Add("Lot2", typeof(string));
                                            dtReprint.Columns.Add("VLD_DATE1", typeof(string));
                                            dtReprint.Columns.Add("VLD_DATE2", typeof(string));
                                            dtReprint.Columns.Add("REG_DATE1", typeof(string));
                                            dtReprint.Columns.Add("REG_DATE2", typeof(string));
                                            dtReprint.Columns.Add("V1", typeof(string));
                                            dtReprint.Columns.Add("V2", typeof(string));
                                            dtReprint.Columns.Add("L1", typeof(string));
                                            dtReprint.Columns.Add("L2", typeof(string));
                                            dtReprint.Columns.Add("M1", typeof(string));
                                            dtReprint.Columns.Add("M2", typeof(string));
                                            dtReprint.Columns.Add("C1", typeof(string));
                                            dtReprint.Columns.Add("C2", typeof(string));
                                            dtReprint.Columns.Add("D1", typeof(string));
                                            dtReprint.Columns.Add("D2", typeof(string));
                                            dtReprint.Columns.Add("REMARK", typeof(string));
                                            dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                                            dtReprint.Columns.Add("V_DATE", typeof(string));
                                            dtReprint.Columns.Add("P_DATE", typeof(string));
                                            dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                                            dtReprint.Columns.Add("PRODID", typeof(string));

                                            drCrad = dtReprint.NewRow();

                                            drCrad.ItemArray = new object[] { sPackageWay,
                                                                          sModlid,
                                                                          sPackingNo,
                                                                          "*" + sPackingNo + "*",
                                                                          sFrom + " -> " + sTo,
                                                                          sTotal_M,
                                                                          sTotal_C,
                                                                          "1",
                                                                          "2",
                                                                          sLotid,
                                                                          sLotid2,
                                                                          sVld,
                                                                          sVld2,
                                                                          sProdDate,
                                                                          sProdDate2,
                                                                          sVer,
                                                                          sVer2,
                                                                          sLane,
                                                                          sLane2,
                                                                          sM1,
                                                                          sM2,
                                                                          sC1,
                                                                          sC2,
                                                                          sD1,
                                                                          sD2,
                                                                          sNote,
                                                                          sUnitCode,
                                                                          sV_DATE,
                                                                          sP_DATE,
                                                                          sOper_Desc,
                                                                          Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                                        };
                                        }
                    */

                    dtReprint.Columns.Add("Title", typeof(string));
                    dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                    dtReprint.Columns.Add("PACK_NO", typeof(string));
                    dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                    dtReprint.Columns.Add("Transfer", typeof(string));

                    dtReprint.Columns.Add("Total_M", typeof(string));
                    dtReprint.Columns.Add("Total_Cell", typeof(string));
                    dtReprint.Columns.Add("No1", typeof(string));
                    dtReprint.Columns.Add("No2", typeof(string));
                    dtReprint.Columns.Add("Lot1", typeof(string));

                    dtReprint.Columns.Add("Lot2", typeof(string));
                    dtReprint.Columns.Add("VLD_DATE1", typeof(string));
                    dtReprint.Columns.Add("VLD_DATE2", typeof(string));
                    dtReprint.Columns.Add("REG_DATE1", typeof(string));
                    dtReprint.Columns.Add("REG_DATE2", typeof(string));

                    dtReprint.Columns.Add("V1", typeof(string));
                    dtReprint.Columns.Add("V2", typeof(string));
                    dtReprint.Columns.Add("L1", typeof(string));
                    dtReprint.Columns.Add("L2", typeof(string));
                    dtReprint.Columns.Add("M1", typeof(string));

                    dtReprint.Columns.Add("M2", typeof(string));
                    dtReprint.Columns.Add("C1", typeof(string));
                    dtReprint.Columns.Add("C2", typeof(string));
                    dtReprint.Columns.Add("D1", typeof(string));
                    dtReprint.Columns.Add("D2", typeof(string));

                    dtReprint.Columns.Add("REMARK", typeof(string));
                    dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                    dtReprint.Columns.Add("V_DATE", typeof(string));
                    dtReprint.Columns.Add("P_DATE", typeof(string));
                    dtReprint.Columns.Add("OFFER_DESC", typeof(string));

                    dtReprint.Columns.Add("PRODID", typeof(string));

                    drCrad = dtReprint.NewRow();

                    drCrad.ItemArray = new object[] { sPackageWay,
                                                      sModlid,
                                                      sPackingNo,
                                                      "*" + sPackingNo + "*",
                                                      sFrom + " -> " + sTo,
                                                      sTotal_M,
                                                      sTotal_C,
                                                      "1",
                                                      "2",
                                                      sLotid,
                                                      sLotid2,
                                                      sVld,
                                                      sVld2,
                                                      sProdDate,
                                                      sProdDate2,
                                                      sVer,
                                                      sVer2,
                                                      sLane,
                                                      sLane2,
                                                      sM1,
                                                      sM2,
                                                      sC1,
                                                      sC2,
                                                      sD1,
                                                      sD2,
                                                      sNote,
                                                      sUnitCode,
                                                      sV_DATE,
                                                      sP_DATE,
                                                      sOper_Desc,
                                                      Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                    };

                    dtReprint.Rows.Add(drCrad);

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    //LGC.GMES.MES.BOX001.Report_Packing rs = new LGC.GMES.MES.BOX001.Report_Packing();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        if (sLotid.Equals(""))
                        {
                            Parameters[0] = "PackingCard_Assy";
                        }
                        else
                        {
                            Parameters[0] = "PackingCard_Assy";
                        }
                        Parameters[1] = dtReprint;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                }
                else if (sType == "TWO")
                {
                    DataTable RQSTDT6 = new DataTable();
                    RQSTDT6.TableName = "RQSTDT";
                    RQSTDT6.Columns.Add("LANGID", typeof(String));
                    RQSTDT6.Columns.Add("RCV_ISS_ID", typeof(String));

                    DataRow dr6 = RQSTDT6.NewRow();
                    dr6["LANGID"] = LoginInfo.LANGID;
                    dr6["RCV_ISS_ID"] = sRcv_Iss_Id;

                    RQSTDT6.Rows.Add(dr6);

                    DataTable Result_Add = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_ADD", "RQSTDT", "RSLTDT", RQSTDT6);

                    if (Result_Add.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                        return;
                    }

                    for (int i = 0; i < Result_Add.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (Result_Add.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = Result_Add.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate2 = Convert.ToDateTime(Result_Add.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate2.Year.ToString() + "-" + ProdDate2.Month.ToString("00") + "-" + ProdDate2.Day.ToString("00");

                            sPackingNo = Result_Add.Rows[i]["BOXID"].ToString();
                            sModlid = Result_Add.Rows[i]["MODLID"].ToString();
                            sFrom = Result_Add.Rows[i]["FROM_AREA"].ToString();
                            sTo = Result_Add.Rows[i]["SHIPTO_NAME"].ToString();
                            iSum = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY2"].ToString());
                            sVer = Result_Add.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = Result_Add.Rows[i]["LANE"].ToString();

                            sLotid = Result_Add.Rows[i]["CSTID"].ToString();
                            sUnitCode = Result_Add.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = ReprintResult.Rows[i]["PRDT_ABBR_CODE"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                                iConvSum = Math.Floor(Util.NVC_Decimal(iSum) * sConvLength) / 100;
                                iConvSum3 = Math.Floor(Util.NVC_Decimal(iSum3) * sConvLength) / 100;
                            }

                            if (Result_Add.Rows[i]["ISS_NOTE"].ToString() == "")
                            {
                                sNote = null;
                            }
                            else
                            {
                                sNote = Result_Add.Rows[i]["ISS_NOTE"].ToString();
                            }

                            if (Result_Add.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                            {
                                sOper_Desc = null;
                            }
                            else
                            {
                                sOper_Desc = Result_Add.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            }
                        }
                        else
                        {
                            sPackingNo2 = Result_Add.Rows[i]["BOXID"].ToString();
                            sLane2 = Result_Add.Rows[i]["LANE"].ToString();

                            if (Result_Add.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld2 = null;
                            }
                            else
                            {
                                string sVld_date = Result_Add.Rows[i]["VLD_DATE"].ToString();
                                sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            iSum2 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY"].ToString());
                            iSum4 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY2"].ToString());
                            sVer2 = Result_Add.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane2 = Result_Add.Rows[i]["LANE"].ToString();

                            sLotid2 = Result_Add.Rows[i]["CSTID"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                                iConvSum2 = Math.Floor(Util.NVC_Decimal(iSum2) * sConvLength) / 100;
                                iConvSum4 = Math.Floor(Util.NVC_Decimal(iSum4) * sConvLength) / 100;                             
                            }

                        }

                        if (string.Equals(sUnitCode, "EA"))
                        {
                            double total_M = iSum;
                            sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", iConvSum) + "M";

                            double total_C = iSum3;
                            sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", iConvSum3) + "M";

                            double total_M2 = iSum2;
                            sTotal_M2 = String.Format("{0:#,##0}", total_M2) + "/" + String.Format("{0:#,##0}", (iConvSum2)) + "M";

                            double total_C2 = iSum4;
                            sTotal_C2 = String.Format("{0:#,##0}", total_C2) + "/" + String.Format("{0:#,##0}", (iConvSum4)) + "M";

                            sTotal_C3 = String.Format("{0:#,##0}", total_C + total_C2) + "/" + String.Format("{0:#,##0}", iConvSum3 + iConvSum4) + "M";
                        }
                        else
                        {
                            double total_M = iSum;
                            sTotal_M = String.Format("{0:#,##0}", total_M);

                            double total_C = iSum3;
                            sTotal_C = String.Format("{0:#,##0}", total_C);

                            double total_M2 = iSum2;
                            sTotal_M2 = String.Format("{0:#,##0}", total_M2);

                            double total_C2 = iSum4;
                            sTotal_C2 = String.Format("{0:#,##0}", total_C + total_C2);

                            sTotal_C3 = String.Format("{0:#,##0}", total_C + total_C2);
                        }

                        sM1 = String.Format("{0:#,##0}", iSum);
                        sM2 = String.Format("{0:#,##0}", iSum2);
                        sC1 = String.Format("{0:#,##0}", iSum3);
                        sC2 = String.Format("{0:#,##0}", iSum4);

                        if (string.Equals(sUnitCode, "EA"))
                        {
                            sD1 = String.Format("{0:#,##0}", iConvSum3);
                            sD2 = String.Format("{0:#,##0}", iConvSum4);
                        }
                        else
                        {
                            sD1 = String.Format("{0:#,##0}", iSum3);
                            sD2 = String.Format("{0:#,##0}", iSum4);
                        }
                    }
                    dtPackingCard = new DataTable();
                    DataRow drCrad = null;

                    if (sLotid.Equals(""))
                    {
                        dtPackingCard.Columns.Add("Title", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                        dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                        dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                        dtPackingCard.Columns.Add("Transfer", typeof(string));
                        dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                        dtPackingCard.Columns.Add("No1", typeof(string));
                        dtPackingCard.Columns.Add("No2", typeof(string));
                        dtPackingCard.Columns.Add("Lot1", typeof(string));
                        dtPackingCard.Columns.Add("Lot2", typeof(string));
                        dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                        dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                        dtPackingCard.Columns.Add("V1", typeof(string));
                        dtPackingCard.Columns.Add("V2", typeof(string));
                        dtPackingCard.Columns.Add("C1", typeof(string));
                        dtPackingCard.Columns.Add("C2", typeof(string));
                        dtPackingCard.Columns.Add("D1", typeof(string));
                        dtPackingCard.Columns.Add("D2", typeof(string));
                        dtPackingCard.Columns.Add("REMARK", typeof(string));
                        dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                        dtPackingCard.Columns.Add("V_DATE", typeof(string));
                        dtPackingCard.Columns.Add("P_DATE", typeof(string));
                        dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                        dtPackingCard.Columns.Add("PRODID", typeof(string));
                        dtPackingCard.Columns.Add("Text5", typeof(string));
                        dtPackingCard.Columns.Add("Text13", typeof(string));
                        dtPackingCard.Columns.Add("Text7", typeof(string)); // LOT ID


                        drCrad = dtPackingCard.NewRow();

                        drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("2가대"),
                                                  sModlid,
                                                  sOuter_Boxid,
                                                  "*" + sOuter_Boxid + "*",
                                                  sFrom + " -> " + sTo,
                                                  sTotal_C3,
                                                  "1",
                                                  "2",
                                                  sLotid3,
                                                  sLotid4,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  sVer,
                                                  sVer2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString() + " :"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString()
                                                  , ObjectDic.Instance.GetObjectName("LOT ID").ToString()
                                               };
                    }
                    else
                    {


                        dtPackingCard.Columns.Add("Title", typeof(string));
                        dtPackingCard.Columns.Add("Title1", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME1", typeof(string));
                        dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                        dtPackingCard.Columns.Add("PACK_NO1", typeof(string));
                        dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                        dtPackingCard.Columns.Add("HEAD_BARCODE1", typeof(string));
                        dtPackingCard.Columns.Add("Transfer", typeof(string));
                        dtPackingCard.Columns.Add("Transfer1", typeof(string));
                        dtPackingCard.Columns.Add("Total_M", typeof(string));
                        dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                        dtPackingCard.Columns.Add("Total_M1", typeof(string));
                        dtPackingCard.Columns.Add("Total_Cell1", typeof(string));
                        dtPackingCard.Columns.Add("No1", typeof(string));
                        dtPackingCard.Columns.Add("No2", typeof(string));
                        dtPackingCard.Columns.Add("Lot1", typeof(string));
                        dtPackingCard.Columns.Add("Lot2", typeof(string));
                        dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                        dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                        dtPackingCard.Columns.Add("V1", typeof(string));
                        dtPackingCard.Columns.Add("V2", typeof(string));
                        dtPackingCard.Columns.Add("L1", typeof(string));
                        dtPackingCard.Columns.Add("L2", typeof(string));
                        dtPackingCard.Columns.Add("M1", typeof(string));
                        dtPackingCard.Columns.Add("M2", typeof(string));
                        dtPackingCard.Columns.Add("C1", typeof(string));
                        dtPackingCard.Columns.Add("C2", typeof(string));
                        dtPackingCard.Columns.Add("D1", typeof(string));
                        dtPackingCard.Columns.Add("D2", typeof(string));
                        dtPackingCard.Columns.Add("REMARK", typeof(string));
                        dtPackingCard.Columns.Add("REMARK1", typeof(string));
                        dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                        dtPackingCard.Columns.Add("UNIT_CODE1", typeof(string));
                        dtPackingCard.Columns.Add("V_DATE", typeof(string));
                        dtPackingCard.Columns.Add("P_DATE", typeof(string));
                        dtPackingCard.Columns.Add("V_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("P_DATE1", typeof(string));
                        dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                        dtPackingCard.Columns.Add("OFFER_DESC1", typeof(string));
                        dtPackingCard.Columns.Add("PRODID", typeof(string));
                        dtPackingCard.Columns.Add("PRODID1", typeof(string));

                        drCrad = null;

                        drCrad = dtPackingCard.NewRow();

                        drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("1가대"),
                                                   ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("2가대"),
                                                  sModlid,
                                                  sModlid,
                                                  sLotid + "0",
                                                  sLotid + "1",
                                                  "*" + sLotid + "0"+ "*",
                                                  "*" + sLotid + "1"+ "*",
                                                  sFrom + " -> " + sTo,
                                                  sFrom + " -> " + sTo,
                                                  sTotal_M,
                                                  sTotal_C,
                                                  sTotal_M2,
                                                  sTotal_C2,
                                                  "1",
                                                  "1",
                                                  sLotid,
                                                  sLotid,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate,
                                                  sVer,
                                                  sVer,
                                                  sLane,
                                                  sLane2,
                                                  sM1,
                                                  sM2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sNote,
                                                  sUnitCode,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                               };
                    }
                    dtPackingCard.Rows.Add(drCrad);

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        if (sLotid.Equals(""))
                        {
                            Parameters[0] = "PackingCard_Roll";
                        }
                        else
                        {
                            Parameters[0] = "PackingCard_2CRT";
                        }
                        Parameters[1] = dtPackingCard;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SteelCaseReprint()
        {
            try
            {
                string sType = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();
                string sOuter_Boxid = drChk[0]["OUTER_BOXID"].ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRcv_Iss_Id;
                dr["CMCDTYPE"] = "PRDT_ABBR_CODE";

                RQSTDT.Rows.Add(dr);

                DataTable ReprintResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT", "RQSTDT", "RSLTDT", RQSTDT);

                if (ReprintResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }


                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LANGID", typeof(String));
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["RCV_ISS_ID"] = sRcv_Iss_Id;

                RQSTDT1.Rows.Add(dr1);

                DataTable CutidResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_CUTID", "RQSTDT", "RSLTDT", RQSTDT1);

                if (CutidResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }

                string sVld = string.Empty;
                string sProdDate = string.Empty;
                string sVld2 = string.Empty;
                string sVld3 = string.Empty;
                string sProdDate2 = string.Empty;
                string sProdDate3 = string.Empty;
                string sPackingNo = string.Empty;
                string sPackingNo2 = string.Empty;
                string sModlid = string.Empty;
                string sFrom = string.Empty;
                string sTo = string.Empty;
                double iSum = 0;
                double iSum2 = 0;
                double iSum3 = 0;
                double iSum4 = 0;
                double iSum5 = 0;
                double iSum6 = 0;
                string sLotid = string.Empty;
                string sLotid2 = string.Empty;
                string sLotid9 = string.Empty;  // Add

                string sVer = string.Empty;
                string sVer2 = string.Empty;
                string sVer3 = string.Empty;
                string sLane = string.Empty;
                string sLane2 = string.Empty;
                string sLane3 = string.Empty;
                string sUnitCode = string.Empty;
                string sNote = string.Empty;
                string sOper_Desc = string.Empty;
                string sAbbrCode = string.Empty;
                string sLotid3 = string.Empty;  // Lot1
                string sLotid4 = string.Empty;  // Lot2
                string sLotid5 = string.Empty;  // Lot3

                // 환산자 처리
                decimal iConvSum = 0;
                decimal iConvSum2 = 0;
                decimal iConvSum3 = 0;
                decimal iConvSum4 = 0;
                decimal iConvSum5 = 0;
                decimal iConvSum6 = 0;

                string sTotal_M = string.Empty;
                string sTotal_C = string.Empty;
                string sTotal_M2 = string.Empty;
                string sTotal_C2 = string.Empty;
                string sTotal_C3 = string.Empty;
                string sM1 = string.Empty;
                string sC1 = string.Empty;
                string sM2 = string.Empty;
                string sC2 = string.Empty;
                string sM3 = string.Empty;
                string sC3 = string.Empty;

                string sD1 = string.Empty;
                string sD2 = string.Empty;
                string sD3 = string.Empty;

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");
                string sBoxtype = string.Empty;
                DateTime ProdDate;

                string sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드");

                for (int i = 0; i < CutidResult.Rows.Count; i++)
                {
                    if (i == 0)
                        sLotid = CutidResult.Rows[i]["CSTID"].ToString();
                    else if (i == 1)
                        sLotid2 = CutidResult.Rows[i]["CSTID"].ToString();
                    else
                    {
                        sLotid9 = CutidResult.Rows[i]["CSTID"].ToString();
                    }
                }

                for (int i = 0; i < ReprintResult.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                        {
                            sVld = null;
                        }
                        else
                        {
                            string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                            sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                        }
                        if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                        {
                            ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                        }

                        sPackingNo = ReprintResult.Rows[i]["OUTER_BOXID"].ToString();
                        sModlid = ReprintResult.Rows[i]["MODLID"].ToString();
                        sFrom = ReprintResult.Rows[i]["FROM_AREA"].ToString();
                        sTo = ReprintResult.Rows[i]["SHIPTO_NAME"].ToString();
                        iSum = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
                        iSum3 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
                        sVer = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                        sLane = ReprintResult.Rows[i]["LANE"].ToString();

                        //sLotid3 = ReprintResult.Rows[i]["LOTID"].ToString();
                        //sLotid4 = ReprintResult.Rows[i]["LOTID"].ToString();
                        sUnitCode = ReprintResult.Rows[i]["UNIT_CODE"].ToString();
                        sAbbrCode = ReprintResult.Rows[i]["PRDT_ABBR_CODE"].ToString();

                        if (string.Equals(sUnitCode, "EA"))
                        {
                            decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                            iConvSum = Math.Floor(Util.NVC_Decimal(iSum) * sConvLength) / 100;
                            iConvSum3 = Math.Floor(Util.NVC_Decimal(iSum3) * sConvLength) / 100;
                        }

                        if (ReprintResult.Rows[i]["ISS_NOTE"].ToString() == "")
                        {
                            sNote = null;
                        }
                        else
                        {
                            sNote = ReprintResult.Rows[i]["ISS_NOTE"].ToString();
                        }

                        if (ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                        {
                            sOper_Desc = null;
                        }
                        else
                        {
                            sOper_Desc = ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                        }
                    }
                    else if (i == 1)
                    {
                        if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                        {
                            sVld2 = null;
                        }
                        else
                        {
                            string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                            sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                        }

                        if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                        {
                            ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                        }

                        iSum2 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
                        iSum4 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
                        sVer2 = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                        sLane2 = ReprintResult.Rows[i]["LANE"].ToString();
                        //sLotid2 = CutidResult.Rows[1]["CSTID"].ToString();
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                            iConvSum2 = Math.Floor(Util.NVC_Decimal(iSum2) * sConvLength) / 100;
                            iConvSum4 = Math.Floor(Util.NVC_Decimal(iSum4) * sConvLength) / 100;
                        }
                    }
                    else
                    {
                        if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                        {
                            sVld3 = null;
                        }
                        else
                        {
                            string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                            sVld3 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                        }

                        if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                        {
                            ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate3 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                        }

                        iSum5 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
                        iSum6 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
                        sVer3 = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                        sLane3 = ReprintResult.Rows[i]["LANE"].ToString();
                        //sLotid2 = CutidResult.Rows[1]["CSTID"].ToString();
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"])));

                            iConvSum5 = Math.Floor(Util.NVC_Decimal(iSum2) * sConvLength) / 100;
                            iConvSum6 = Math.Floor(Util.NVC_Decimal(iSum4) * sConvLength) / 100;
                        }
                    }
                }


                if (string.Equals(sUnitCode, "EA"))
                {
                    double total_M = Math.Floor(iSum) + Math.Floor(iSum2);
                    sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", (iConvSum + iConvSum2 + iConvSum5)) + "M";

                    double total_C = Math.Floor(iSum3) + Math.Floor(iSum4);
                    sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", (iConvSum3 + iConvSum4 + iConvSum6)) + "M";
                }
                else
                {
                    double total_M = Math.Floor(iSum) + Math.Floor(iSum2) + Math.Floor(iSum5); ;
                    sTotal_M = String.Format("{0:#,##0}", total_M);

                    double total_C = Math.Floor(iSum3) + Math.Floor(iSum4) + Math.Floor(iSum6);
                    sTotal_C = String.Format("{0:#,##0}", total_C);
                }

                sM1 = String.Format("{0:#,##0}", Math.Floor(iSum));
                sM2 = String.Format("{0:#,##0}", Math.Floor(iSum2));
                sM3 = String.Format("{0:#,##0}", Math.Floor(iSum5));
                sC1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                sC2 = String.Format("{0:#,##0}", Math.Floor(iSum4));
                sC3 = String.Format("{0:#,##0}", Math.Floor(iSum6));

                if (string.Equals(sUnitCode, "EA"))
                {
                    sD1 = String.Format("{0:#,##0}", iConvSum3);
                    sD2 = String.Format("{0:#,##0}", iConvSum4);
                    sD3 = String.Format("{0:#,##0}", iConvSum5);
                }
                else
                {
                    sD1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                    sD2 = String.Format("{0:#,##0}", Math.Floor(iSum4));
                    sD3 = String.Format("{0:#,##0}", Math.Floor(iSum6));
                }

                DataTable dtReprint = new DataTable();
                DataRow drCrad = null;
                dtReprint.Columns.Add("Title", typeof(string));
                dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                dtReprint.Columns.Add("PACK_NO", typeof(string));
                dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                dtReprint.Columns.Add("Transfer", typeof(string));
                dtReprint.Columns.Add("Total_M", typeof(string));
                dtReprint.Columns.Add("Total_Cell", typeof(string));
                dtReprint.Columns.Add("No1", typeof(string));
                dtReprint.Columns.Add("No2", typeof(string));
                dtReprint.Columns.Add("No3", typeof(string));
                dtReprint.Columns.Add("Lot1", typeof(string));
                dtReprint.Columns.Add("Lot2", typeof(string));
                dtReprint.Columns.Add("Lot3", typeof(string));
                dtReprint.Columns.Add("VLD_DATE1", typeof(string));
                dtReprint.Columns.Add("VLD_DATE2", typeof(string));
                dtReprint.Columns.Add("VLD_DATE3", typeof(string));
                dtReprint.Columns.Add("REG_DATE1", typeof(string));
                dtReprint.Columns.Add("REG_DATE2", typeof(string));
                dtReprint.Columns.Add("REG_DATE3", typeof(string));
                dtReprint.Columns.Add("V1", typeof(string));
                dtReprint.Columns.Add("V2", typeof(string));
                dtReprint.Columns.Add("V3", typeof(string));
                dtReprint.Columns.Add("L1", typeof(string));
                dtReprint.Columns.Add("L2", typeof(string));
                dtReprint.Columns.Add("L3", typeof(string));
                dtReprint.Columns.Add("M1", typeof(string));
                dtReprint.Columns.Add("M2", typeof(string));
                dtReprint.Columns.Add("M3", typeof(string));
                dtReprint.Columns.Add("C1", typeof(string));
                dtReprint.Columns.Add("C2", typeof(string));
                dtReprint.Columns.Add("C3", typeof(string));
                dtReprint.Columns.Add("D1", typeof(string));
                dtReprint.Columns.Add("D2", typeof(string));
                dtReprint.Columns.Add("D3", typeof(string));
                dtReprint.Columns.Add("REMARK", typeof(string));
                dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                dtReprint.Columns.Add("V_DATE", typeof(string));
                dtReprint.Columns.Add("P_DATE", typeof(string));
                dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                dtReprint.Columns.Add("PRODID", typeof(string));

                drCrad = dtReprint.NewRow();

                drCrad.ItemArray = new object[] { sPackageWay,
                                                      sModlid,
                                                      sPackingNo,
                                                      "*" + sPackingNo + "*",
                                                      sFrom + " -> " + sTo,
                                                      sTotal_M,
                                                      sTotal_C,
                                                      "1",
                                                      "2",
                                                      "3",
                                                      sLotid,
                                                      sLotid2,
                                                      sLotid9,
                                                      sVld,
                                                      sVld2,
                                                      sVld3,
                                                      sProdDate,
                                                      sProdDate2,
                                                      sProdDate3,
                                                      sVer,
                                                      sVer2,
                                                      sVer3,
                                                      sLane,
                                                      sLane2,
                                                      sLane3,
                                                      sM1,
                                                      sM2,
                                                      sM3,
                                                      sC1,
                                                      sC2,
                                                      sC3,
                                                      sD1,
                                                      sD2,
                                                      sD3,
                                                      sNote,
                                                      sUnitCode,
                                                      sV_DATE,
                                                      sP_DATE,
                                                      sOper_Desc,
                                                      Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                    };

                dtReprint.Rows.Add(drCrad);

                LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    //태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "PackingCard_New_Three";
                    Parameters[1] = dtReprint;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result);
                    // 팝업 화면 숨겨지는 문제 수정.
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRODID"] = prodID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_MBOM_FOR_PRODID", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["MTRL_QTY"])))
                        return Util.NVC_Decimal(result.Rows[0]["MTRL_QTY"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private decimal GetPatternLengthVer(string prodID, string sVer)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = prodID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROD_VER_CODE"] = sVer;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_VER", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Common wndPopup = sender as LGC.GMES.MES.BOX001.Report_Common;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal iTotal_Qty = 0;
                decimal iTotal_Qty2 = 0;

                if (dgOutDetail.GetRowCount() <= 0)
                {
                    Util.AlertInfo("SFU1636");  //선택된 대상이 없습니다.
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.AlertInfo("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                if (!drChk[0]["BOX_RCV_ISS_STAT_CODE"].ToString().Equals("SHIPPING"))
                {
                    Util.AlertInfo("SFU1939");   //취소할수있는상태가아닙니다.
                    return;
                }

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRcv_Iss_Id;

                RQSTDT.Rows.Add(dr);

                //DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_INNER_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                if (BoxResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("OUTBOXID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("UNPACK_QTY", typeof(decimal));
                inData.Columns.Add("UNPACK_QTY2", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["OUTBOXID"] = drChk[0]["OUTER_BOXID"].ToString();
                row["PRODID"] = drChk[0]["PRODID"].ToString();
                row["UNPACK_QTY"] = drChk[0]["TOTAL_QTY"];
                row["UNPACK_QTY2"] = drChk[0]["TOTAL_QTY2"];
                row["USERID"] = LoginInfo.USERID;
                row["NOTE"] = "";

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inBox = indataSet.Tables.Add("INNERBOX");
                inBox.Columns.Add("BOXID", typeof(string));
                inBox.Columns.Add("PRODID", typeof(string));
                inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                inBox.Columns.Add("UNPACK_QTY", typeof(string));
                inBox.Columns.Add("UNPACK_QTY2", typeof(string));

                for (int i = 0; i < BoxResult.Rows.Count; i++)
                {
                    DataRow row2 = inBox.NewRow();

                    row2["BOXID"] = BoxResult.Rows[i]["BOXID"].ToString();
                    row2["PRODID"] = BoxResult.Rows[i]["PRODID"].ToString();
                    row2["PACK_LOT_TYPE_CODE"] = "LOT";
                    //row2["UNPACK_QTY"] = BoxResult.Rows[i]["WIPQTY"].ToString();
                    //row2["UNPACK_QTY2"] = BoxResult.Rows[i]["WIPQTY2"].ToString();
                    row2["UNPACK_QTY"] = BoxResult.Rows[i]["TOTAL_QTY"].ToString();
                    row2["UNPACK_QTY2"] = BoxResult.Rows[i]["TOTAL_QTY2"].ToString();

                    iTotal_Qty += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY"].ToString());
                    iTotal_Qty2 += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY2"].ToString());

                    indataSet.Tables["INNERBOX"].Rows.Add(row2);

                }

                DataTable inLot = indataSet.Tables.Add("INLOT");

                inLot.Columns.Add("BOXID", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                {
                    DataRow row3 = inLot.NewRow();

                    row3["BOXID"] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "BOXID").ToString();
                    row3["LOTID"] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "LOTID").ToString();

                    indataSet.Tables["INLOT"].Rows.Add(row3);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_PACKING", "INDATA,INNERBOX,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        Util.gridClear(dgOutDetail);
                        Boxmapping_Master();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotid = string.Empty;
                    string sLot_Type = string.Empty;

                    if (txtLotID.Text.ToString() == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    if (cboType.SelectedIndex < 0 || cboType.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.Alert("SFU1895");   //제품을 선택하세요
                        return;
                    }
                    else
                    {
                        sLot_Type = cboType.SelectedValue.ToString();
                    }


                    //if (dgOut.GetRowCount() >= 2)
                    //{
                    //    Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                    //    return;
                    //}

                    if (sLot_Type == "PANCAKE")
                    {
                        sLotid = txtLotID.Text.ToString().Trim();

                        // 출고 이력 조회
                        DataTable RQSTDT0 = new DataTable();
                        RQSTDT0.TableName = "RQSTDT";
                        RQSTDT0.Columns.Add("CSTID", typeof(String));
                        RQSTDT0.Columns.Add("AREAID", typeof(String));

                        DataRow dr0 = RQSTDT0.NewRow();
                        dr0["CSTID"] = sLotid;
                        dr0["AREAID"] = LoginInfo.CFG_AREA_ID;

                        RQSTDT0.Rows.Add(dr0);

                        DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);
                        if (OutResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                            return;
                        }
                        else
                        {
                            int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                            if (iCnt <= 0)
                            {
                                Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                                return;
                            }
                        }

                        // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        //RQSTDT1.Columns.Add("PROCID", typeof(String));
                        //RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = sLotid;
                        //dr1["PROCID"] = "E7000";
                        //dr1["WIPSTAT"] = "WAIT";

                        RQSTDT1.Rows.Add(dr1);

                        DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_CELL", "RQSTDT", "RSLTDT", RQSTDT1);

                        if (Prod_Result.Rows.Count == 0)
                        {
                            Util.Alert("SFU1870");   //재공 정보가 없습니다.
                            return;
                        }

                        for (int i = 0; i < Prod_Result.Rows.Count; i++)
                        {
                            if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                            {
                                Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                                return;
                            }
                        }

                        if (dgOut.GetRowCount() > 0)
                        {
                            if (sProcid != Prod_Result.Rows[0]["PROCID"].ToString())
                            {
                                Util.AlertInfo("SFU4167"); //동일한 공정이 아닙니다.
                                return;
                            }
                        }
                        else
                        {
                            sProcid = Prod_Result.Rows[0]["PROCID"].ToString();
                            sRepre_Lotid = sLotid;
                        }                        

                        DataTable dt = new DataTable();
                        dt.Columns.Add("AREAID", typeof(string));
                        dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                        dt.Columns.Add("COM_CODE", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                        dr["COM_CODE"] = "QMS_NOCHECK_PACKING";

                        dt.Rows.Add(dr);

                        //QMS 체크 제외 AREA
                        DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                        if (AreaResult.Rows.Count == 0)
                        {
                            DataTable dtChk = new DataTable();
                            dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                            DataRow drChk = dtChk.NewRow();
                            drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

                            dtChk.Rows.Add(drChk);

                            DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);
                            if (Chk_Result.Rows.Count != 0)
                            {
                                if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                                {
                                    // 품질결과 검사 체크
                                    DataSet indataSet = new DataSet();

                                    DataTable inData = indataSet.Tables.Add("INDATA");
                                    inData.Columns.Add("LOTID", typeof(string));
                                    inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                                    inData.Columns.Add("BR_TYPE", typeof(string));

                                    DataRow row = inData.NewRow();
                                    row["LOTID"] = sLotid;
                                    row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                                    row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                                    indataSet.Tables["INDATA"].Rows.Add(row);

                                    //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                                    //신규 HOLD 적용을 위해 변경 작업
                                    new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                                    {
                                        if (Exception != null)
                                        {
                                            Util.MessageException(Exception);
                                            return;
                                        }
                                        else
                                        {
                                            Search_Pancake(sLotid);

                                            txtLotID.SelectAll();
                                            txtLotID.Focus();
                                        }

                                    }, indataSet);
                                }
                                else
                                {
                                    // 품질결과 Skip
                                    Search_Pancake(sLotid);

                                    txtLotID.SelectAll();
                                    txtLotID.Focus();
                                }
                            }
                        }
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_Pancake(sLotid);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else if (sLot_Type == "JUMBO_ROLL")
                    {
                        sLotid = txtLotID.Text.ToString().Trim().ToUpper();

                        // 출고 이력 조회
                        DataTable RQSTDT0 = new DataTable();
                        RQSTDT0.TableName = "RQSTDT";
                        RQSTDT0.Columns.Add("LOT_ID", typeof(String));

                        DataRow dr0 = RQSTDT0.NewRow();
                        dr0["LOT_ID"] = sLotid;

                        RQSTDT0.Rows.Add(dr0);

                        DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT_JB", "RQSTDT", "RSLTDT", RQSTDT0);

                        if (OutResult.Rows.Count != 0)
                        {
                            if (OutResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString() == "SHIPPED" || OutResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString() == "SHIPPING")
                            {
                                Util.MessageValidation("SFU3018"); //출고 이력이 존재합니다.
                                return;
                            }
                        }

                        DataTable dt = new DataTable();
                        dt.Columns.Add("AREAID", typeof(string));
                        dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                        dt.Columns.Add("COM_CODE", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                        dr["COM_CODE"] = "QMS_NOCHECK_PACKING";

                        dt.Rows.Add(dr);

                        DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                        if (AreaResult.Rows.Count == 0)
                        {
                            // 품질결과 검사 체크
                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotid;
                            row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                    return;
                                }

                                Search_Roll(sLotid);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_Roll(sLotid);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void Search_QMS_Validation(string sLotid)
        {
            //WIP HOLD 체크
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));

            DataRow dr2 = dt2.NewRow();
            dr2["LOTID"] = sLotid;

            dt2.Rows.Add(dr2);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_FOR_HOLD_CHECK", "RQSTDT", "RSLTDT", dt2);
            if (Result.Rows.Count != 0)
            {
                if (Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                                             //return;
                }
                //QMS HOLD 체크 -DA_PRD_SEL_QMS_INFO  //  출하 단계에서 검사결과없으면 출하불능
                DataTable dt3 = new DataTable();
                dt3.Columns.Add("LOTID", typeof(string));

                DataRow dr3 = dt3.NewRow();
                dr3["LOTID"] = sLotid;

                dt3.Rows.Add(dr3);

                DataTable Result2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_INFO", "RQSTDT", "RSLTDT", dt3);

                // 포장카드에는“ OQC 검사 결과 없음”포장작업은 정상처리
                if (Result2.Rows.Count == 0)
                {
                    Util.Alert("SFU3492");   // 품질검사 결과가 없어서 출하가 불가합니다. 공정사에게 보고하세요.
                    sNote2 = ObjectDic.Instance.GetObjectName("OQC 검사 결과 없음");

                    // 품질검서 없을시 등록된 인원들에 대해 BIZ 내에서 메일을 보냄
                    DataTable dt4 = new DataTable();
                    dt4.TableName = "INDATA";
                    dt4.Columns.Add("LANGID", typeof(string));
                    dt4.Columns.Add("SKIDID", typeof(string));

                    DataRow dr4 = dt4.NewRow();
                    dr4["LANGID"] = LoginInfo.LANGID;
                    dr4["SKIDID"] = sLotid;

                    dt4.Rows.Add(dr4);
                    new ClientProxy().ExecuteService("BR_PRD_CHK_QMS_FOR_MAILING", "INDATA", null, dt4, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                    });
                }
            }
        }

        private void Search_Pancake(string sLotid)
        {
            try
            {
                // 재공정보 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(String));
                RQSTDT2.Columns.Add("LANGID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = sLotid;
                dr2["LANGID"] = LoginInfo.LANGID;

                RQSTDT2.Rows.Add(dr2);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_PANCAKE", "RQSTDT", "RSLTDT", RQSTDT2);

                if (Lot_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Lot_Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "MKT_TYPE_CODE").ToString())
                    {
                        Util.Alert("SFU4454");   //내수용과 수출용은 같이 포장할 수 없습니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    //if (NJ_AreaChk())
                    //{
                    //    if (DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PROD_VER_CODE").ToString() != Lot_Result.Rows[0]["PROD_VER_CODE"].ToString())
                    //    {
                    //        Util.Alert("SFU1501");   //동일 버전이 아닙니다.
                    //        return;
                    //    }
                    //}
                    
                    dgOut.IsReadOnly = false;
                    dgOut.BeginNewRow();
                    dgOut.EndNewRow(true);
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", Lot_Result.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", Lot_Result.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", Lot_Result.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LANE_QTY", Lot_Result.Rows[0]["LANE_QTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", Lot_Result.Rows[0]["M_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", Lot_Result.Rows[0]["CELL_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MKT_TYPE_CODE", Lot_Result.Rows[0]["MKT_TYPE_CODE"].ToString());

                    //DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROJECTNAME", Lot_Result.Rows[0]["PROJECTNAME"].ToString());
                    //DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRDT_CLSS_CODE", Lot_Result.Rows[0]["PRDT_CLSS_CODE"].ToString());
                    //DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRDT_CLSS_NAME", Lot_Result.Rows[0]["PRDT_CLSS_NAME"].ToString());
                    //DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROCID", Lot_Result.Rows[0]["PROCID"].ToString());                    
                    //DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROD_VER_CODE", Lot_Result.Rows[0]["PROD_VER_CODE"].ToString());
                    dgOut.IsReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private bool NJ_AreaChk()
        {
            bool area_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACKING_VERSION_CHECK_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && LoginInfo.CFG_AREA_ID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    area_chk = true;
                }
                else
                {
                    area_chk = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return area_chk;
        }

        private void Search_Roll(string sLotid)
        {
            try
            {
                // 재공정보 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotid;
                //dr1["PROCID"] = "E7000";  //<= 확인 필요
                dr1["WIPSTAT"] = "WAIT";

                RQSTDT1.Rows.Add(dr1);

                DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_JB", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Prod_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (Prod_Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                    return;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                    Util.GridSetData(dgOut, Prod_Result, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Prod_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    //dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;

                    dgOut.IsReadOnly = false;
                    dgOut.BeginNewRow();
                    dgOut.EndNewRow(true);
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", Prod_Result.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", Prod_Result.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", Prod_Result.Rows[0]["M_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", Prod_Result.Rows[0]["CELL_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", Prod_Result.Rows[0]["MODLID"].ToString());
                    dgOut.IsReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgOut.IsReadOnly = false;
                    dgOut.RemoveRow(index);
                    dgOut.IsReadOnly = true;
                }
            });
        }

        private void dgOutHIstChoice_Checked(object sender, RoutedEventArgs e)
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
                dgOutHIst.SelectedIndex = idx;

                Boxmapping_Detail(idx);

            }
        }

        #region PDA - 현재 미사용
        private void btnOut4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sBoxid = string.Empty;
                string sPackageWay = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgPDAOutHIst, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("10008");   //선택된 데이터가 없습니다.
                    return;
                }
                else
                {
                    sBoxid = drChk[0]["BOXID"].ToString();
                }


                // 발행
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["OUTER_BOXID"] = sBoxid;

                RQSTDT.Rows.Add(dr);

                DataTable CutID_Cnt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_CNT", "RQSTDT", "RSLTDT", RQSTDT);

                int iCnt = Convert.ToInt32(CutID_Cnt.Rows[0]["CNT"].ToString());


                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("AREAID", typeof(String));
                RQSTDT2.Columns.Add("SRCTYPE", typeof(String));
                RQSTDT2.Columns.Add("LANGID", typeof(String));
                RQSTDT2.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr2["SRCTYPE"] = "PDA";
                dr2["LANGID"] = LoginInfo.LANGID;
                dr2["OUTER_BOXID"] = sBoxid;

                RQSTDT2.Rows.Add(dr2);

                DataTable dtPrintInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_PDA_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드");

                if (iCnt == 1 && dtPrintInfo.Rows.Count == 1)
                {
                    if (dtPrintInfo.Rows[0]["BOXTYPE"].ToString() == "CRT")
                    {
                        sPackageWay = sPackageWay + ObjectDic.Instance.GetObjectName("가대#1");
                    }
                    else
                    {
                        sPackageWay = sPackageWay + "BOX";
                    }

                    string sVld_date = string.Empty;
                    string sVld = string.Empty;

                    if (dtPrintInfo.Rows[0]["VLD_DATE"].ToString() == null)
                    {
                        sVld = null;
                    }
                    else
                    {
                        sVld_date = dtPrintInfo.Rows[0]["VLD_DATE"].ToString();
                        sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                    }

                    if (dtPrintInfo.Rows[0]["WIPSDTTM"].ToString() == null)
                    {
                        Util.Alert("SFU1870");   //재공 정보가 없습니다.
                        return;
                    }

                    string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                    string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                    DateTime ProdDate = Convert.ToDateTime(dtPrintInfo.Rows[0]["WIPSDTTM"].ToString());
                    string sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                    dtPackingCard = new DataTable();

                    dtPackingCard.Columns.Add("Title", typeof(string));
                    dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                    dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                    dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                    dtPackingCard.Columns.Add("Transfer", typeof(string));
                    dtPackingCard.Columns.Add("Total_M", typeof(string));
                    dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                    dtPackingCard.Columns.Add("No1", typeof(string));
                    dtPackingCard.Columns.Add("No2", typeof(string));
                    dtPackingCard.Columns.Add("Lot1", typeof(string));
                    dtPackingCard.Columns.Add("Lot2", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("V1", typeof(string));
                    dtPackingCard.Columns.Add("V2", typeof(string));
                    dtPackingCard.Columns.Add("L1", typeof(string));
                    dtPackingCard.Columns.Add("L2", typeof(string));
                    dtPackingCard.Columns.Add("M1", typeof(string));
                    dtPackingCard.Columns.Add("M2", typeof(string));
                    dtPackingCard.Columns.Add("C1", typeof(string));
                    dtPackingCard.Columns.Add("C2", typeof(string));
                    dtPackingCard.Columns.Add("REMARK", typeof(string));
                    dtPackingCard.Columns.Add("V_DATE", typeof(string));
                    dtPackingCard.Columns.Add("P_DATE", typeof(string));

                    DataRow drCrad = null;

                    drCrad = dtPackingCard.NewRow();

                    drCrad.ItemArray = new object[] { sPackageWay,
                                                      dtPrintInfo.Rows[0]["MODLID"].ToString(),
                                                      dtPrintInfo.Rows[0]["OUTER_BOXID"].ToString(),
                                                      dtPrintInfo.Rows[0]["OUTER_BOXID"].ToString(),
                                                      dtPrintInfo.Rows[0]["TRANSFER"].ToString(),
                                                      dtPrintInfo.Rows[0]["WIPQTY"].ToString(),
                                                      dtPrintInfo.Rows[0]["WIPQTY2"].ToString(),
                                                      "1",
                                                      "",
                                                      dtPrintInfo.Rows[0]["CUT_ID"].ToString(),
                                                      "",
                                                      sVld,
                                                      "",
                                                      sProdDate,
                                                      "",
                                                      dtPrintInfo.Rows[0]["PROD_VER_CODE"].ToString(),
                                                      "",
                                                      dtPrintInfo.Rows[0]["CNT"].ToString(),
                                                      "",
                                                      dtPrintInfo.Rows[0]["WIPQTY"].ToString(),
                                                      "",
                                                      dtPrintInfo.Rows[0]["WIPQTY2"].ToString(),
                                                      "",
                                                      "",
                                                      sV_DATE,
                                                      sP_DATE
                                           };

                    dtPackingCard.Rows.Add(drCrad);

                }
                else
                {
                    if (dtPrintInfo.Rows[0]["BOXTYPE"].ToString() == "CRT")
                    {
                        sPackageWay = sPackageWay + "가대#2";
                    }
                    else
                    {
                        sPackageWay = sPackageWay + "BOX";
                    }

                    if (dtPrintInfo.Rows[0]["VLD_DATE"].ToString() == null)
                    {
                        //유효기간 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3256"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtPrintInfo.Rows[1]["VLD_DATE"].ToString() == null)
                    {
                        //유효기간 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3256"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtPrintInfo.Rows[0]["WIPSDTTM"].ToString() == null)
                    {
                        //생산일 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3257"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtPrintInfo.Rows[1]["WIPSDTTM"].ToString() == null)
                    {
                        //생산일 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3257"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string sVld_date = dtPrintInfo.Rows[0]["VLD_DATE"].ToString();
                    string sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);

                    DateTime ProdDate = Convert.ToDateTime(dtPrintInfo.Rows[0]["WIPSDTTM"].ToString());
                    string sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                    string sVld_date2 = dtPrintInfo.Rows[1]["VLD_DATE"].ToString();
                    string sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);

                    DateTime ProdDate2 = Convert.ToDateTime(dtPrintInfo.Rows[1]["WIPSDTTM"].ToString());
                    string sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                    double dSum = 0;
                    double dSum2 = 0;
                    double dSum3 = 0;
                    double dSum4 = 0;

                    dSum = Convert.ToDouble(dtPrintInfo.Rows[0]["WIPQTY"].ToString());
                    dSum2 = Convert.ToDouble(dtPrintInfo.Rows[1]["WIPQTY"].ToString());
                    dSum3 = Convert.ToDouble(dtPrintInfo.Rows[0]["WIPQTY2"].ToString());
                    dSum4 = Convert.ToDouble(dtPrintInfo.Rows[1]["WIPQTY2"].ToString());

                    dtPackingCard = new DataTable();

                    dtPackingCard.Columns.Add("Title", typeof(string));
                    dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                    dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                    dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                    dtPackingCard.Columns.Add("Transfer", typeof(string));
                    dtPackingCard.Columns.Add("Total_M", typeof(string));
                    dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                    dtPackingCard.Columns.Add("No1", typeof(string));
                    dtPackingCard.Columns.Add("No2", typeof(string));
                    dtPackingCard.Columns.Add("Lot1", typeof(string));
                    dtPackingCard.Columns.Add("Lot2", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("V1", typeof(string));
                    dtPackingCard.Columns.Add("V2", typeof(string));
                    dtPackingCard.Columns.Add("L1", typeof(string));
                    dtPackingCard.Columns.Add("L2", typeof(string));
                    dtPackingCard.Columns.Add("M1", typeof(string));
                    dtPackingCard.Columns.Add("M2", typeof(string));
                    dtPackingCard.Columns.Add("C1", typeof(string));
                    dtPackingCard.Columns.Add("C2", typeof(string));
                    dtPackingCard.Columns.Add("REMARK", typeof(string));

                    DataRow drCrad = null;

                    drCrad = dtPackingCard.NewRow();

                    drCrad.ItemArray = new object[] { sPackageWay,
                                                      dtPrintInfo.Rows[0]["MODLID"].ToString(),
                                                      dtPrintInfo.Rows[0]["OUTER_BOXID"].ToString(),
                                                      dtPrintInfo.Rows[0]["OUTER_BOXID"].ToString(),
                                                      dtPrintInfo.Rows[0]["TRANSFER"].ToString(),
                                                      dSum + dSum2,
                                                      dSum3 + dSum4,
                                                      "1",
                                                      "2",
                                                      dtPrintInfo.Rows[0]["CUT_ID"].ToString(),
                                                      dtPrintInfo.Rows[1]["CUT_ID"].ToString(),
                                                      sVld,
                                                      sVld2,
                                                      sProdDate,
                                                      sProdDate2,
                                                      dtPrintInfo.Rows[0]["PROD_VER_CODE"].ToString(),
                                                      dtPrintInfo.Rows[1]["PROD_VER_CODE"].ToString(),
                                                      dtPrintInfo.Rows[0]["CNT"].ToString(),
                                                      dtPrintInfo.Rows[1]["CNT"].ToString(),
                                                      dtPrintInfo.Rows[0]["WIPQTY"].ToString(),
                                                      dtPrintInfo.Rows[1]["WIPQTY"].ToString(),
                                                      dtPrintInfo.Rows[0]["WIPQTY2"].ToString(),
                                                      dtPrintInfo.Rows[1]["WIPQTY2"].ToString(),
                                                      ""
                                           };

                    dtPackingCard.Rows.Add(drCrad);

                }

                LGC.GMES.MES.BOX001.Report_Multi rs = new LGC.GMES.MES.BOX001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "PackingCard_New";
                    Parameters[1] = dtPackingCard;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Save_Result);
                    // 팝업 화면 숨겨지는 문제 수정.
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        private void Save_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Multi wndPopup = sender as LGC.GMES.MES.BOX001.Report_Multi;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    DataRow[] drChk = Util.gridGetChecked(ref dgPDAOutHIst, "CHK");

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["OUTER_BOXID"] = drChk[0]["BOXID"].ToString();

                    RQSTDT.Rows.Add(dr);

                    DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_PDA_BOXHIST", "RQSTDT", "RSLTDT", RQSTDT);

                    double dQty = 0;
                    double dQty2 = 0;
                    double dTotal = 0;
                    double dTotal2 = 0;

                    for (int i = 0; i < dgPDAOutDetail.GetRowCount(); i++)
                    {
                        dQty = Convert.ToDouble(DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "WIPQTY").ToString());
                        dQty2 = Convert.ToDouble(DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "WIPQTY2").ToString());

                        dTotal = dTotal + dQty;
                        dTotal2 = dTotal2 + dQty2;
                    }

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("FROM_AREAID", typeof(string));
                    inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                    inData.Columns.Add("TO_AREAID", typeof(string));
                    inData.Columns.Add("TO_SLOC_ID", typeof(string));
                    inData.Columns.Add("ISS_QTY", typeof(string));
                    inData.Columns.Add("ISS_QTY2", typeof(string));
                    inData.Columns.Add("ISS_NOTE", typeof(string));
                    inData.Columns.Add("SHIPTO_ID", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataRow row = inData.NewRow();

                    row["SRCTYPE"] = "UI";
                    row["FROM_AREAID"] = drChk[0]["FROM_AREAID"].ToString();
                    row["FROM_SLOC_ID"] = drChk[0]["FROM_SLOC_ID"].ToString();
                    row["TO_AREAID"] = "";
                    row["TO_SLOC_ID"] = drChk[0]["TO_SLOC_ID"].ToString();
                    row["ISS_QTY"] = dTotal;
                    row["ISS_QTY2"] = dTotal2;
                    row["ISS_NOTE"] = "LOT";
                    row["SHIPTO_ID"] = drChk[0]["SHIPTO_ID"].ToString();
                    row["NOTE"] = "";
                    row["USERID"] = LoginInfo.USERID;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inBox = indataSet.Tables.Add("INPALLET");

                    inBox.Columns.Add("BOXID", typeof(string));
                    inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                    for (int icnt = 0; icnt < BoxResult.Rows.Count; icnt++)
                    {
                        DataRow row2 = inBox.NewRow();

                        row2["BOXID"] = BoxResult.Rows[icnt]["BOXID"].ToString();
                        row2["OWMS_BOX_TYPE_CODE"] = "";

                        indataSet.Tables["INPALLET"].Rows.Add(row2);
                    }

                    DataTable inLot = indataSet.Tables.Add("INBOX");

                    inLot.Columns.Add("BOXID", typeof(string));
                    inLot.Columns.Add("LOTID", typeof(string));
                    inLot.Columns.Add("LOTQTY", typeof(string));
                    inLot.Columns.Add("LOTQTY2", typeof(string));
                    inLot.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                    for (int i = 0; i < dgPDAOutDetail.GetRowCount(); i++)
                    {
                        DataRow row3 = inLot.NewRow();

                        row3["BOXID"] = DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "BOXID").ToString();
                        row3["LOTID"] = DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "LOTID").ToString();
                        row3["LOTQTY"] = DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "WIPQTY").ToString();
                        row3["LOTQTY2"] = DataTableConverter.GetValue(dgPDAOutDetail.Rows[i].DataItem, "WIPQTY2").ToString();
                        row3["OWMS_BOX_TYPE_CODE"] = "";

                        indataSet.Tables["INBOX"].Rows.Add(row3);
                    }

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", "INDATA,INPALLET,INBOX", null, indataSet);

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void PDAHist_Detail(int idx)
        {
            string sBoxid = string.Empty;

            sBoxid = DataTableConverter.GetValue(dgPDAOutHIst.Rows[idx].DataItem, "BOXID").ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("OUTER_BOXID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["OUTER_BOXID"] = sBoxid;

            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_PDA_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

            Util.gridClear(dgPDAOutDetail);

            Util.GridSetData(dgPDAOutDetail, SearchResult, FrameOperation);

        }

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            PackPdaHist_Search();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void PackPdaHist_Search()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom4.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo4.SelectedDateTime);

                string sCutID = txtLotid4.Text.ToString().Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("CUT_ID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                if (sCutID == "" || sCutID == null)
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["CUT_ID"] = sCutID;
                }

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_PDA_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgPDAOutHIst);
                Util.gridClear(dgPDAOutDetail);
                Util.GridSetData(dgPDAOutHIst, SearchResult, FrameOperation);

                txtLotid4.Text = "";

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLotid4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sCutID = txtLotid4.Text.ToString().Trim();

                if (sCutID == "" || sCutID == null)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }
                loadingIndicator.Visibility = Visibility.Visible;
                PackPdaHist_Search();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgPDAOutHIstChoice_Checked(object sender, RoutedEventArgs e)
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
                dgPDAOutHIst.SelectedIndex = idx;

                PDAHist_Detail(idx);

            }
        }

        private void cboType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLot_Type = cboType.SelectedValue.ToString();

            if (sLot_Type == "JUMBO_ROLL")
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("출고");

                dgOut.Columns["M_WIPQTY"].Header = "S/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "N/ROLL";
            }
            else
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("포장구성");

                dgOut.Columns["M_WIPQTY"].Header = "C/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "S/ROLL";
            }
        }

        #endregion
       

        #region 포장 이력 조회 - 남경
        private void txtBoxID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxID_Hist.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist();
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtLotid_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotid_Hist.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist();
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtSkid_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSkid_Hist.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist();
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Box_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Serach_Box_Hist();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void Serach_Box_Hist()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom_Box.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo_Box.SelectedDateTime);

                if (txtLotid_Hist.Text != "" || txtBoxID_Hist.Text != "" || txtSkid_Hist.Text != "")
                {
                    sStart_date = null;
                    sEnd_date = null;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXSTAT", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PJTNAME", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXSTAT"] = "PACKED";
                dr["LOTID"] = txtLotid_Hist.Text.Trim() == "" ? null : txtLotid_Hist.Text;
                dr["BOXID"] = txtBoxID_Hist.Text.Trim() == "" ? null : txtBoxID_Hist.Text;
                dr["CSTID"] = txtSkid_Hist.Text.Trim() == "" ? null : txtSkid_Hist.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc_Hist, bAllNull: true);
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["PRODID"] = txtProd_ID_Hist.Text.Trim() == "" ? null : txtProd_ID_Hist.Text;
                dr["PJTNAME"] = Util.NVC(dr["PJTNAME"]).ToString();

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgBox_Hist);
                Util.GridSetData(dgBox_Hist, SearchResult, FrameOperation);

                string[] sColumnName = new string[] { "OUTER_BOXID", "CSTID" };
                _Util.SetDataGridMergeExtensionCol(dgBox_Hist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            DataGridAggregateCount dgcount = new DataGridAggregateCount();
            dagsum.ResultTemplate = dgBox_Hist.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            daq.Add(dgcount);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist.Columns["LOTID"], daq);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist.Columns["TOTAL_QTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist.Columns["WIPQTY2"], dac);
        }
        #endregion

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Boxmapping_Master();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

      

        private void txtProd_ID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID_Hist.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Serach_Box_Hist();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtBoxid7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Search_Out_Hist();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtProd_ID7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Search_Out_Hist();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearch7_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Search_Out_Hist();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void Search_Out_Hist()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom_Hist.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo_Hist.SelectedDateTime);
                string sStatus = cboStatus7.SelectedValue.ToString();

                Util.gridClear(dgOutHIst);
                Util.gridClear(dgOutDetail);

                if (cboStatus7.SelectedIndex < 0 || cboStatus7.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["OUTER_BOXID"] = txtBoxid7.Text.Trim() == "" ? null : txtBoxid7.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc7, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid7.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid7.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID7.Text.Trim() == "" ? null : txtProd_ID7.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOut_Hist);
                Util.GridSetData(dgOut_Hist, SearchResult, FrameOperation);

                string[] sColumnName = new string[] { "OUTER_BOXID" };
                _Util.SetDataGridMergeExtensionCol(dgOut_Hist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #region 자동차동 SAMPLING 전용 FUNCTION
        //private void btnQuality_Click(object sender, RoutedEventArgs e)
        //{
        //    sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
        //    sampling.FrameOperation = FrameOperation;

        //    if (sampling != null)
        //    {
        //        C1WindowExtension.SetParameters(sampling, null);

        //        sampling.Closed -= new EventHandler(OnCloseSampling);
        //        sampling.Closed += new EventHandler(OnCloseSampling);
        //        this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
        //    }
        //}

            /*
        private void timer_Start(object sender, EventArgs e)
        {
            
            if (sampling == null && IsSamplingCheck == false)
            {
                //LinearGradientBrush btnGradient = btnQuality.Background as LinearGradientBrush;

                //System.Windows.Media.Animation.ColorAnimation animation = new System.Windows.Media.Animation.ColorAnimation();
                //animation.From = System.Windows.Media.Colors.Blue;
                //animation.To = System.Windows.Media.Colors.Orange;
                //animation.Duration = TimeSpan.FromSeconds(1);
                //animation.AutoReverse = true;
                //animation.RepeatBehavior = RepeatBehavior.Forever;

                //Storyboard.SetTarget(animation, btnGradient);
                //Storyboard.SetTargetProperty(animation, new PropertyPath(SolidColorBrush.ColorProperty));

                //Storyboard sb = new Storyboard();
                //sb.Children.Add(animation);
                //sb.Begin();

                SetActSamplingData();
            }
        }

        /*
        private void OnCloseSampling(object sender, EventArgs e)
        {
            CMM001.CMM_ELEC_SAMPLING_OQC_RP window = sender as CMM001.CMM_ELEC_SAMPLING_OQC_RP;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (IsSamplingCheck)
                {
                    IsSamplingCheck = false;
                    Storyboard board = (Storyboard)this.Resources["storyBoard"];
                    if (board != null)
                        board.Stop();
                }
            }
            sampling.Close();
            sampling = null;
            GC.Collect();
        }
        */

/*
        private void SetActSamplingData()
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.ROLL_PRESSING;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_SAMPLE_CNA_QA", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                            throw searchException;

                        if (OriginSamplingData == null)
                            OriginSamplingData = result;
                        else
                            IsDiffSamplingData(OriginSamplingData, result);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { }
        }

 */

/*
        private void IsDiffSamplingData(DataTable oldData, DataTable newData)
        {
            bool IsChangeSampling = false;
            foreach ( DataRow oldRow in oldData.Rows)
            {
                foreach(DataRow newRow in newData.Rows)
                {
                    if ( string.Equals(oldRow["LOTID"], newRow["LOTID"] ))
                    {
                        // 변경된 데이터 검증
                        if (!string.Equals(oldRow["JUDG_FLAG"], newRow["JUDG_FLAG"]))
                            IsChangeSampling = true;

                        // 미검사 OR 불합격 판정 -> 합격 변경 시
                        if((string.IsNullOrEmpty(Util.NVC(oldRow["JUDG_FLAG"])) || string.Equals(oldRow["JUDG_FLAG"], "F")) && string.Equals(newRow["JUDG_FLAG"], "Y") && IsSamplingCheck == false)
                        {
                            IsSamplingCheck = true;
                            Storyboard board = (Storyboard)this.Resources["storyBoard"];
                            if (board != null)
                                board.Begin();

                            // 팝업 자동 생성
                            if (sampling == null && chkQuality.IsChecked == true)
                            {
                                sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
                                sampling.FrameOperation = FrameOperation;

                                if (sampling != null)
                                {
                                    C1WindowExtension.SetParameters(sampling, null);

                                    sampling.Closed -= new EventHandler(OnCloseSampling);
                                    sampling.Closed += new EventHandler(OnCloseSampling);
                                    this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // 갱신이 존재하면 신규 데이터로 변경 요청
            if (IsChangeSampling == false || (oldData.Rows.Count != newData.Rows.Count))
                IsChangeSampling = true;

            if (IsChangeSampling == true)
            {
                OriginSamplingData.Clear();
                OriginSamplingData = newData;
            }
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
*/   
     
      
        #endregion

        

      
       

      

       


    }
}

