/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.03.13  0.1  stmlsh  C20200109-000262 [ GMES ] Plant 이동 ( 전극 인수 ) 시 , V/D 별 인수라인 지정 요청 건
  2021.08.19  김지은    : 반품 시 LOT유형의 시험생산구분코드 Validation
  2023.05.17  김광규    : Multi-Input 기능 추가 (NJ 9동 요구사항)
  2024.02.20  조범모    : [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
  2024.05.17  배현우    : 대표LOT 사용동일 경우 CARRIERID -> 대표LOT 변경 (반품요청)
  2025.03.25  김선영   : 1) 인수상세이력 탭 그리드(dgMove_Info) : 설비/VD완공시간 컬럼 안보이게 처리
                               2) 인수상세이력 탭 그리드(dgMove_Info) : LOT 유형 (CODE), LOT 유형 컬럼 추가 
                          

 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_039 : UserControl, IWorkArea
    {

        Util _Util = new Util();
        private bool _Multi_Input_YN = false;

        //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 
        bool isTWS_LOADING_TRACKING = false;

        private bool RepLotUseArea = false;
        private bool errChk = false;
        private bool errFlag = false;
        #region Declaration & Constructor 
        public COM001_039()
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
            listAuth.Add(btnConfrim);
            listAuth.Add(btnReturn);
            listAuth.Add(btnReturnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();

            if (LoginInfo.CFG_AREA_ID.Equals("M1") || LoginInfo.CFG_AREA_ID.Equals("M2"))
            {
                Tab_VDQA_Search.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFromHist.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateToHist.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFromHist5.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateToHist5.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateTo6.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom6.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "MOVE_ORD_STAT_CODE" };
            combo.SetCombo(cboStatHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            cboStatHist.SelectedValue = "CLOSE_MOVE";

            combo.SetCombo(cboReturnStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            cboReturnStat.SelectedValue = "CLOSE_MOVE";

            string[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboFromEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT", sFilter: sFilter1);

            combo.SetCombo(cboStatHist5, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            cboStatHist5.SelectedValue = "CLOSE_MOVE";

            combo.SetCombo(cboReturnStat6, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            cboReturnStat6.SelectedValue = "CLOSE_MOVE";

            string[] sFilter2 = { "MOVE_RTN_TYPE_CODE" };
            combo.SetCombo(cboMoveRtnType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);


            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "CANCEL_RETURN_BTN_SHOW";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FOR_CANCEL_RETURN_BTN_SHOW", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count == 0)
            {
                btnReturnCancel.Visibility = Visibility.Collapsed;
            }


            string[] sFilter3 = { "ASSY_RETURN_POINT" };
            combo.SetCombo(cboReturnTarget, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter3);


            //인수콤보지정
            combo.SetCombo(cboToEquipmentsegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT", sFilter: sFilter1);

            SetVisibleReturnLine("ASSY_RETURN_USELINE", LoginInfo.CFG_AREA_ID);

            SearchAreaMoveConfirm();

            if (LoginInfo.CFG_SHOP_ID.Equals("A010")) //소형조립일때만 보여줌
            {
                VD_EQPTFLOOR.Visibility = Visibility.Visible;
                VD_EQPTNAME.Visibility = Visibility.Visible;
                VD_EQPT_FLOOR_RETURN.Visibility = Visibility.Visible;
                Tab_Return7.Visibility = Visibility.Visible;
                btnReturnRePrint.Visibility = Visibility.Visible;
            }

            //Plant간 이동 인수확정된 SKID
            //  GetReceiveSkid();

            #region   [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
            CheckAreaUseTWS(LoginInfo.CFG_AREA_ID);

            if (isTWS_LOADING_TRACKING == true)
            {
                dgMove_Detail.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                dgMove_Detail.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                dgMove_Detail.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Visible;

                dgMoveHist_Detail.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                dgMoveHist_Detail.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                dgMoveHist_Detail.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Visible;

                dgMove_Info.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                dgMove_Info.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                dgMove_Info.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Visible;
            }

            //2025.03.25  김선영  : 인수상세이력 탭 그리드 : 설비/VD완공시간 컬럼 안보이게 처리, START
            dgMove_Info.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
            dgMove_Info.Columns["VD_END_DTTM"].Visibility = Visibility.Collapsed;
            //2025.03.25  김선영  : 인수상세이력 탭 그리드 : 설비/VD완공시간 컬럼 안보이게 처리, END

            #endregion

            GetRepLotUseArea();
           
            if (RepLotUseArea)
                tbCARRIERID_01.Text = ObjectDic.Instance.GetObjectName("대표 LOTID");


        }
        #endregion

        #region Event
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;

            dtpDateFromHist.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateToHist.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFromHist5.SelectedDataTimeChanged += dtpDateFrom5_SelectedDataTimeChanged;
            dtpDateToHist5.SelectedDataTimeChanged += dtpDateTo5_SelectedDataTimeChanged;

            dtpDateFrom6.SelectedDataTimeChanged += dtpDateFrom6_SelectedDataTimeChanged;
            dtpDateTo6.SelectedDataTimeChanged += dtpDateTo6_SelectedDataTimeChanged;
        }
        #endregion

        #region Mehod

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateToHist.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateToHist.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFromHist.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFromHist.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom5_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateToHist5.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateToHist5.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo5_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFromHist5.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFromHist5.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom6_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo6.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo6.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo6_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom6.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom6.SelectedDateTime;
                return;
            }
        }
        #endregion


        #region [인수]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            setEquimentSegment_init();
            Serarch("Receive");
        }

        private void txtMoveOrderID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID")).Equals(txtMoveOrderID.Text))
                    {
                      //  Util.MessageValidation("SFU4963"); // 이미 선택된 MOVE ORDER ID 입니다.
                        txtMoveOrderID.Text = "";
                        txtMoveOrderID.Focus();
                        return;

                    }
                }


                string compare_areaid = "";
                string compare_prodid = "";
                string compare_RoutID = "";


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                dr["MOVE_ORD_STAT_CODE"] = "MOVING";
                dr["MOVE_ORD_ID"] = txtMoveOrderID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID2", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU4963"); //이동 중인 MOVE ORDER ID가 없습니다.
                    txtMoveOrderID.Text = "";
                    txtMoveOrderID.Focus();
                    return;
                }

              

                SearchResult.Rows[0]["CHK"] = 1;

                if (dgMove_Master.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgMove_Master.ItemsSource);
                    dt.Merge(SearchResult);

                    Util.GridSetData(dgMove_Master, dt, FrameOperation);
                }
                else
                {
                    Util.GridSetData(dgMove_Master, SearchResult, FrameOperation);
                }

                


                for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID")).Equals(txtMoveOrderID.Text))
                    {
                         dgMove_Master_ByMoveOrd(dgMove_Master, i);

                    }
                }

                txtMoveOrderID.Text = "";
                txtMoveOrderID.Focus();

            }


        }
        private void txtSKIDID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "PANCAKE_GR_ID")).Equals(txtSKIDID1.Text))
                    {
                        //  Util.MessageValidation("SFU4963"); // 이미 선택된 MOVE ORDER ID 입니다.
                        txtSKIDID1.Text = "";
                        txtSKIDID1.Focus();
                        return;

                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                dr["MOVE_ORD_STAT_CODE"] = "MOVING";
                dr["CSTID"] = Util.NVC(txtSKIDID1.Text.Trim());

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID2", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2844"); // SKID 정보가 없습니다.
                    txtSKIDID1.Text = "";
                    txtSKIDID1.Focus();
                    return;
                }

                for (int ii = 0; ii < SearchResult.Rows.Count; ii++)
                    SearchResult.Rows[ii]["CHK"] = 1;

                if (dgMove_Master.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgMove_Master.ItemsSource);
                    dt.Merge(SearchResult);

                    Util.GridSetData(dgMove_Master, dt, FrameOperation);
                }
                else
                {
                    Util.GridSetData(dgMove_Master, SearchResult, FrameOperation);
                }

                for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CSTID")).Equals(txtSKIDID1.Text))
                    {
                        dgMove_Master_ByMoveOrd(dgMove_Master, i);

                    }
                }

                txtSKIDID1.Text = "";
                txtSKIDID1.Focus();

            }
        }
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //btnConfrim.IsEnabled = false;
                string sMoveOrderID = string.Empty;
                string sEqsgid = string.Empty;


                if (dgMove_Master.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgMove_Master, "CHK") < 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (cboFromEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }
                else
                {
                    sEqsgid = cboFromEquipmentSegment.SelectedValue.ToString();
                }


                //for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                //{
                //    //if (DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK").ToString() == "True")
                //    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK")).Equals("1"))

                //    {
                //        sMoveOrderID = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID").ToString();
                //    }
                //}

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PCSGID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID").ToString());
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["EQSGID"] = sEqsgid;
                        row["PCSGID"] = "A";
                        row["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }


                //DataTable inLot = indataSet.Tables.Add("INLOT");
                //inLot.Columns.Add("LOTID", typeof(string));

                //for (int i = 0; i < dgMove_Detail.GetRowCount(); i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK")).Equals("True"))
                //    {
                //        if (DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                //        {
                //            DataRow row2 = inLot.NewRow();
                //            row2["LOTID"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID").ToString();

                //            indataSet.Tables["INLOT"].Rows.Add(row2);
                //        }
                //    }
                //}

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PACKLOT_SHOP", "INDATA", null, indataSet);

                Util.gridClear(dgMove_Master);
                Util.gridClear(dgMove_Detail);

                Serarch("Receive");

                Util.AlertInfo("SFU1793");  //인수되었습니다.

                //btnConfrim.IsEnabled = true;
            }
            catch (Exception ex)
            {
                btnConfrim.IsEnabled = true;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region [반품]
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReturnList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                //if (cboMoveRtnType.SelectedIndex == 0)
                //{
                //    Util.MessageValidation("SFU3640");  //반품유형을 선택해 주세요.
                //    return;
                //}

                // 품질이상일때만 반품사유 필수입력 체크 [2017-09-25]
                if (string.Equals(cboMoveRtnType.SelectedValue, "DR"))
                {
                    if (string.IsNullOrEmpty(Util.NVC(txtRemark.Text.Trim())))
                    {
                        Util.MessageValidation("SFU1554");  //반품사유를 입력하세요.
                        return;
                    }
                }

                if (dgReturnTarget.Visibility == Visibility.Visible)
                {
                    if (cboReturnTarget.SelectedIndex < 1)
                    {
                        Util.MessageValidation("SFU3518"); //반품동을 선택해 주세요.
                        return;
                    }
                }

                string sAlert = "";

                if (LoginInfo.CFG_SHOP_ID == "A010" || LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    sAlert = "SFU8351";
                }
                else
                {
                    sAlert = "SFU2868";
                }

                //반품처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sAlert), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //double dSum = 0;
                        //double dSum2 = 0;
                        //double dTotal = 0;
                        //double dTotal2 = 0;

                        //for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                        //{
                        //    if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CHK")).Equals("True"))
                        //    {
                        //        dSum = Convert.ToDouble(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY").ToString());
                        //        dSum2 = Convert.ToDouble(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY2").ToString());
                        //    }

                        //    dTotal = dTotal + dSum;
                        //    dTotal2 = dTotal2 + dSum2;
                        //}

                        decimal dSum = 0;
                        decimal dSum2 = 0;
                        decimal dTotal = 0;
                        decimal dTotal2 = 0;

                        for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {

                                dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                                dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                                //dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY2")));

                                dTotal = dTotal + dSum;
                                dTotal2 = dTotal2 + dSum2;
                            }
                        }


                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                        inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("MOVE_RTN_TYPE_CODE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));

                        if (dgReturnTarget.Visibility == Visibility.Visible)
                            inData.Columns.Add("AREAID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["MOVE_ORD_QTY"] = dTotal;
                        row["MOVE_ORD_QTY2"] = dTotal2;
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = txtRemark.Text.ToString();
                        row["MOVE_RTN_TYPE_CODE"] = (string)cboMoveRtnType.SelectedValue;
                        row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;

                        if (dgReturnTarget.Visibility == Visibility.Visible)
                            row["AREAID"] = Util.NVC(cboReturnTarget.SelectedValue);

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                        inLot.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));

                        for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["LOTID"] = DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "LOTID");
                                row2["MOVE_ORD_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                                row2["MOVE_ORD_QTY2"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                                //row2["MOVE_ORD_QTY2"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY2")));

                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_RETURN_PACKLOT_SHOP", "INDATA,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                                {

                                    DataTable dtTitle = null;
                                    DataTable dtLotInfo = null;


                                    string[] str = cboReturnTarget.Text.Split(':');
                                    PrintCard(dgReturnList, str[1], ref dtTitle, ref dtLotInfo);

                                    LGC.GMES.MES.COM001.Report_Plant_Move_Return rs = new LGC.GMES.MES.COM001.Report_Plant_Move_Return();
                                    rs.FrameOperation = this.FrameOperation;

                                    if (rs != null)
                                    {
                                        // 태그 발행 창 화면에 띄움.
                                        object[] Parameters = new object[3];
                                        Parameters[0] = "Report_Plant_Move_Return";
                                        Parameters[1] = dtTitle;
                                        Parameters[2] = dtLotInfo;

                                        C1WindowExtension.SetParameters(rs, Parameters);

                                        rs.Closed += new EventHandler(Print_Result);
                                        // 팝업 화면 숨겨지는 문제 수정.
                                        grdMain.Children.Add(rs);
                                        rs.BringToFront();

                                        txtLotid.Focus();
                                        Util.gridClear(dgReturnList);
                                        txtLotid.Text = "";
                                        txtReturnSkidID.Text = "";
                                        cboReturnTarget.SelectedIndex = 0;

                                    }

                                }
                                else
                                {
                                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                                    txtLotid.Focus();
                                    Util.gridClear(dgReturnList);
                                    txtLotid.Text = "";
                                    txtReturnSkidID.Text = "";
                                    cboReturnTarget.SelectedIndex = 0;
                                }

                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }

                        }, indataSet
                        );

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void txtSkidID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSkidID.Text = "";
        }
        private void cboMoveRtnType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtRemark.Text = "";
        }
        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Plant_Move_Return wndPopup = sender as LGC.GMES.MES.COM001.Report_Plant_Move_Return;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    // Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) => //정상 처리 되었습니다.
                    {
                        if (vResult == MessageBoxResult.OK)
                        {
                            txtSkidID.Focus();
                            Util.gridClear(dgReturnList);
                            txtLotid.Text = "";
                            cboReturnTarget.SelectedIndex = 0;
                        }
                    });

                    //Util.gridClear(dgReturnList);
                    //txtLotid.Text = "";
                    //txtRemark.Text = "";
                    //cboMoveRtnType.SelectedIndex = 0;


                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        // 반품처리 포장이력카드

        private void PrintCard(C1.WPF.DataGrid.C1DataGrid datagrid, string return_area, ref DataTable dtTitle, ref DataTable dtLotInfo)
        {
            decimal dSum = 0;
            decimal dSum2 = 0;
            decimal dTotal = 0;
            decimal dTotal2 = 0;
            decimal dQty = 0;
            int idxcount = 1;

 

            for (int i = 0; i < dgReturnList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CHK")).Equals("True"))
                {

                    dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                    dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY")));
                    //dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "WIPQTY2")));

                    dTotal = dTotal + dSum;
                    dTotal2 = dTotal2 + dSum2;
                    dQty = idxcount;
                    idxcount++;
                }
 
            }
            dtTitle = new DataTable();
            dtTitle.Columns.Add("TITLE", typeof(string));
            dtTitle.Columns.Add("SKIDID", typeof(string));
            dtTitle.Columns.Add("BARCODE_SKIDID", typeof(string));
            dtTitle.Columns.Add("TOTAL_QTY_TITLE", typeof(string));
            dtTitle.Columns.Add("TOTAL_QTY", typeof(string));
            dtTitle.Columns.Add("NOTE_TITLE", typeof(string));
            dtTitle.Columns.Add("NOTE", typeof(string));
            dtTitle.Columns.Add("PANCAKE_QTY_TITLE", typeof(string));
            dtTitle.Columns.Add("PANCAKE_QTY", typeof(string));
            dtTitle.Columns.Add("PRINT_DATE_TITLE", typeof(string));
            dtTitle.Columns.Add("PRINT_DATE", typeof(string));
            dtTitle.Columns.Add("LOTID_TITLE", typeof(string));
            dtTitle.Columns.Add("PRJT_NAME_TITLE", typeof(string));
            dtTitle.Columns.Add("PRODID_TITLE", typeof(string));
            dtTitle.Columns.Add("WIPQTY_TITLE", typeof(string));
            dtTitle.Columns.Add("WIPQTYPC_TITLE", typeof(string));
            dtTitle.Columns.Add("RETURN_AREA_TITLE", typeof(string));
            dtTitle.Columns.Add("RETURN_AREA", typeof(string));

            DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);

            DataRow dr = dtTitle.NewRow();
            dr.ItemArray = new object[] {
                ObjectDic.Instance.GetObjectName("SKIDID"),
                DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "CSTID"),
                DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "CSTID"),
                ObjectDic.Instance.GetObjectName("총수량"),
                String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(dTotal), 2))),
                //String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(dt.Compute("SUM(WIPQTY)", "1=1")), 2))),
                ObjectDic.Instance.GetObjectName("비고"),
                txtRemark.Text,
                //DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "NOTE"),
                ObjectDic.Instance.GetObjectName("Pancake수"),
                dt.Compute(dQty.ToString(), "1=1"),
                //dt.Compute("COUNT(LOTID)", "1=1"),
                ObjectDic.Instance.GetObjectName("발행일시"),
                System.DateTime.Now,
                ObjectDic.Instance.GetObjectName("LOTID"),
                ObjectDic.Instance.GetObjectName("프로젝트명"),
                ObjectDic.Instance.GetObjectName("제품코드"),
                ObjectDic.Instance.GetObjectName("수량(J/R)"),
                ObjectDic.Instance.GetObjectName("수량(P/C)"),
                ObjectDic.Instance.GetObjectName("반품동"),
                return_area
        };

            dtTitle.Rows.Add(dr);

            dtLotInfo = new DataTable();
            dtLotInfo.Columns.Add("NUM", typeof(string));
            dtLotInfo.Columns.Add("LOTID", typeof(string));
            dtLotInfo.Columns.Add("PRJT_NAME", typeof(string));
            dtLotInfo.Columns.Add("PRODID", typeof(string));
            dtLotInfo.Columns.Add("WIPQTY", typeof(string));
            dtLotInfo.Columns.Add("WIPQTYPC", typeof(string));

            int count = 1;
            for (int i = 0; i < datagrid.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr2 = dtLotInfo.NewRow();
                    dr2["NUM"] = count;
                    dr2["LOTID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "LOTID"));
                    dr2["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PROJECTNAME"));
                    dr2["PRODID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PRODID"));
                    dr2["WIPQTY"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "WIPQTY"))), 2)));
                    dr2["WIPQTYPC"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "WIPQTY"))), 2)));
                    //dr2["WIPQTYPC"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "WIPQTY2"))), 2)));

                    dtLotInfo.Rows.Add(dr2);
                    count++;
                }
            }

        }


        private void PrintReturnCard(C1.WPF.DataGrid.C1DataGrid datagrid, string return_area, string Note, ref DataTable dtTitle, ref DataTable dtLotInfo)
        {
                dtTitle = new DataTable();
                dtTitle.Columns.Add("TITLE", typeof(string));
                dtTitle.Columns.Add("SKIDID", typeof(string));
                dtTitle.Columns.Add("BARCODE_SKIDID", typeof(string));
                dtTitle.Columns.Add("TOTAL_QTY_TITLE", typeof(string));
                dtTitle.Columns.Add("TOTAL_QTY", typeof(string));
                dtTitle.Columns.Add("NOTE_TITLE", typeof(string));
                dtTitle.Columns.Add("NOTE", typeof(string));
                dtTitle.Columns.Add("PANCAKE_QTY_TITLE", typeof(string));
                dtTitle.Columns.Add("PANCAKE_QTY", typeof(string));
                dtTitle.Columns.Add("PRINT_DATE_TITLE", typeof(string));
                dtTitle.Columns.Add("PRINT_DATE", typeof(string));
                dtTitle.Columns.Add("LOTID_TITLE", typeof(string));
                dtTitle.Columns.Add("PRJT_NAME_TITLE", typeof(string));
                dtTitle.Columns.Add("PRODID_TITLE", typeof(string));
                dtTitle.Columns.Add("WIPQTY_TITLE", typeof(string));
                dtTitle.Columns.Add("WIPQTYPC_TITLE", typeof(string));
                dtTitle.Columns.Add("RETURN_AREA_TITLE", typeof(string));
                dtTitle.Columns.Add("RETURN_AREA", typeof(string));

                DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);

                DataRow dr = dtTitle.NewRow();
                dr.ItemArray = new object[] {
                ObjectDic.Instance.GetObjectName("SKIDID"),
                DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "CSTID"),
                DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "CSTID"),
                ObjectDic.Instance.GetObjectName("총수량"),
                String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(dt.Compute("SUM(WIPQTY)", "1=1")), 2))),
                ObjectDic.Instance.GetObjectName("비고"),
                Note.Trim(),
                //txtRemark.Text,
               // DataTableConverter.GetValue(datagrid.Rows[0].DataItem, "NOTE"),
                ObjectDic.Instance.GetObjectName("Pancake수"),
                dt.Compute("COUNT(LOTID)", "1=1"),
                ObjectDic.Instance.GetObjectName("발행일시"),
                System.DateTime.Now,
                ObjectDic.Instance.GetObjectName("LOTID"),
                ObjectDic.Instance.GetObjectName("프로젝트명"),
                ObjectDic.Instance.GetObjectName("제품코드"),
                ObjectDic.Instance.GetObjectName("수량(J/R)"),
                ObjectDic.Instance.GetObjectName("수량(P/C)"),
                ObjectDic.Instance.GetObjectName("반품동"),
                return_area
        };

                dtTitle.Rows.Add(dr);

                dtLotInfo = new DataTable();
                dtLotInfo.Columns.Add("NUM", typeof(string));
                dtLotInfo.Columns.Add("LOTID", typeof(string));
                dtLotInfo.Columns.Add("PRJT_NAME", typeof(string));
                dtLotInfo.Columns.Add("PRODID", typeof(string));
                dtLotInfo.Columns.Add("WIPQTY", typeof(string));
                dtLotInfo.Columns.Add("WIPQTYPC", typeof(string));

                for (int i = 0; i < datagrid.GetRowCount(); i++)
                {
                        DataRow dr2 = dtLotInfo.NewRow();
                        dr2["NUM"] = i + 1;
                        dr2["LOTID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "LOTID"));
                        dr2["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PROJECTNAME"));
                        dr2["PRODID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PRODID"));
                        dr2["WIPQTY"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "WIPQTY"))), 2)));
                        dr2["WIPQTYPC"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "WIPQTY2"))), 2)));

                        dtLotInfo.Rows.Add(dr2);                 
                }

            }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgReturnList);
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Scan_Lot_Search(txtLotid.Text.ToString().Trim());
            }
        }

        private bool Scan_Lot_Search(string sLotid)
        {
            try
            {
                //string sLotid = txtLotid.Text.ToString().Trim();

                if (sLotid == null || sLotid == "")
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return false;
                }

                for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                    {
                        Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                        return false;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count >= 1)
                {
                    if (!Result.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        Util.Alert("SFU1869");  //재공 상태가 이동가능한 상태가 아닙니다.
                        return false;
                    }

                    if (Result.Rows[0]["PCSGID"].ToString() != "A" )
                    {
                        Util.AlertInfo("SFU4937"); //조립에서만 반품이 가능합니다. 확인 바랍니다.

                        return false;
                    }

                    //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                    //기존 Validation 삭제
                    //for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                    //{
                    //    //시생산 LOT인 경우 다른 LOTTYPE과 섞이면 안됨
                    //    if (DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "LOTTYPE").ToString() != Result.Rows[0]["LOTTYPE"].ToString())
                    //    {
                    //        if ((DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "LOTTYPE").ToString() == "X") || (Result.Rows[0]["LOTTYPE"].ToString() == "X"))
                    //        {
                    //            Util.Alert("SFU5149");  //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                    //            return;
                    //        }
                    //    }
                    //}

                    //반품 승인 정보 있는지 Check.
                    if (!ChkRtnApprReq(sLotid))
                        return false;

                    /* 20171117 HOLD CHK 제외
                    if (Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                    {
                        Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                        return;
                    }
                    */

                    //if (Result.Rows[0]["RCVFLAG"].ToString().Equals("N"))
                    //{
                    //    Util.Alert("SFU2048");  //확정할 수 없는 상태입니다.
                    //    return;
                    //}

                    if (dgReturnList.GetRowCount() == 0)
                    {
                        dgReturnList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PRODID").ToString() == Result.Rows[0]["PRODID"].ToString())
                        {
                            if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "SLOC_ID").ToString() == Result.Rows[0]["SLOC_ID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgReturnList.ItemsSource);
                                    dtSource.Merge(Result);

                                    Util.gridClear(dgReturnList);
                                    dgReturnList.ItemsSource = DataTableConverter.Convert(dtSource);

                                    txtLotid.Text = "";
                                }
                                else
                                {
                                    Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    return false;
                                }
                            }
                            else
                            {
                                Util.MessageValidation("SFU4572"); //저장소가 다릅니다.
                                return false;
                            }
                        }
                        else
                        {
                            Util.MessageValidation("SFU1893");      //제품ID가 같지 않습니다.
                            return false;
                        }
                    }
                }
                else
                {
                    Util.Alert("SFU1870");      //재공 정보가 없습니다.
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void txtSkidID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Scan_Skid_Search();
            }
        }

        private void Scan_Skid_Search()
        {
            try
            {
                string sSkidID = txtSkidID.Text.ToString().Trim();

                if (string.IsNullOrEmpty(sSkidID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "CSTID").ToString() == sSkidID)
                    {
                        Util.MessageValidation("SFU2862");  //동일한 SKID ID 가 스캔되었습니다.
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                //RQSTDT.Columns.Add("WIPHOLD", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTID"] = sSkidID;
                dr["WIPSTAT"] = Wip_State.WAIT;
                //dr["WIPHOLD"] = "N";

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE_CUT", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count >= 1)
                {
                    /*
                    if (!Result.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        Util.Alert("SFU1869");  //재공 상태가 이동가능한 상태가 아닙니다.
                        return;
                    }

                    if (Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                    {
                        Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                        return;
                    }
                    */

                    if (Result.Rows[0]["PCSGID"].ToString() != "A")
                    {
                        Util.AlertInfo("SFU4937"); //조립에서만 반품이 가능합니다. 확인 바랍니다.
                        return;
                    }

                    DataTable prodDt = Result.DefaultView.ToTable(true, "PRODID");
                    if (prodDt != null && prodDt.Rows.Count > 1)
                    {
                        Util.MessageValidation("SFU4040", sSkidID); //해당SKID[%1]에 서로 다른 제품ID가 존재 합니다.
                        return;
                    }

                    //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                    //기존 Validation 삭제
                    //for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                    //{
                    //    if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "LOTTYPE").ToString() != Result.Rows[0]["LOTTYPE"].ToString())
                    //    {
                    //        if ((DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "LOTTYPE").ToString() == "X") || (Result.Rows[0]["LOTTYPE"].ToString() == "X"))
                    //        {
                    //            Util.Alert("SFU5149");  //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                    //            return;
                    //        }
                    //    }
                    //}

                    if (dgReturnList.GetRowCount() == 0)
                    {
                        dgReturnList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PRODID").ToString() == prodDt.Rows[0]["PRODID"].ToString())
                        {
                            if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "SLOC_ID").ToString() == Result.Rows[0]["SLOC_ID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgReturnList.ItemsSource);
                                    dtSource.Merge(Result);

                                    Util.gridClear(dgReturnList);
                                    dgReturnList.ItemsSource = DataTableConverter.Convert(dtSource);

                                    txtLotid.Text = "";
                                }
                                else
                                {
                                    Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    return;
                                }
                            }
                            else
                            {
                                Util.Alert("SFU4572"); //저장소가 다릅니다.
                                return;
                            }
                        }
                        else
                        {
                            Util.MessageValidation("SFU1893");      //제품ID가 같지 않습니다.
                            return;
                        }
                    }

                    // 반품동 자동선택 (에러가 나도 수동으로 처리할 수 있게 이쪽에 추가)
                    if (dgReturnTarget.Visibility == Visibility.Visible)
                    {
                        string sReturnLine = GetReturnLine(sSkidID);
                        if (!string.IsNullOrEmpty(sReturnLine))
                            cboReturnTarget.SelectedValue = sReturnLine;
                    }
                }
                else
                {
                    Util.MessageValidation("SFU1870");      //재공 정보가 없습니다.
                    return;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetVisibleReturnLine(string sCodeType, string sConnArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(row["CBO_CODE"], sConnArea))
                    {
                        dgReturnTarget.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }
            catch (Exception ex) { }
        }

        private string GetReturnLine(string sCstID)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["CSTID"] = sCstID;
                inDataTable.Rows.Add(inDataRow);

                DataSet outData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SLITTER_AREA", "INDATA", "OUTDATA", inDataSet);

                if (outData != null && outData.Tables["OUTDATA"] != null && outData.Tables["OUTDATA"].Rows.Count > 0)
                    return Util.NVC(outData.Tables["OUTDATA"].Rows[0]["AREAID"]);

                return "";
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion

        #region [반품이력]
        private void btnReturnRePrint_Click(object sender, RoutedEventArgs e)
        {

            DataTable dtTitle = null;
            DataTable dtLotInfo = null;


            PrintReturnCard(dgReturn_Detail, Util.NVC(DataTableConverter.GetValue(dgReturn_Master.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReturn_Master, "CHK")].DataItem, "TO_AREAID")), Util.NVC(DataTableConverter.GetValue(dgReturn_Master.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReturn_Master, "CHK")].DataItem, "NOTE")), ref dtTitle, ref dtLotInfo);

            LGC.GMES.MES.COM001.Report_Plant_Move_Return rs = new LGC.GMES.MES.COM001.Report_Plant_Move_Return();
            rs.FrameOperation = this.FrameOperation;

            if (rs != null)
            {
                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[3];
                Parameters[0] = "Report_Plant_Move_Return";
                Parameters[1] = dtTitle;
                Parameters[2] = dtLotInfo;

                C1WindowExtension.SetParameters(rs, Parameters);

                rs.Closed += new EventHandler(Print_Result);
                // 팝업 화면 숨겨지는 문제 수정.
                grdMain.Children.Add(rs);
                rs.BringToFront();
            }
        }


        private void btnReturnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMoveOrderID = string.Empty;

                // 2025.03.17 UI Validation 추가
                if (dgReturn_Master.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1498"); //데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgReturn_Master, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                else
                {
                    sMoveOrderID = drChk[0]["MOVE_ORD_ID"].ToString();
                }

                if (!drChk[0]["MOVE_ORD_STAT_CODE"].ToString().Equals("MOVING"))
                {
                    Util.AlertInfo("SFU1939");  //취소할 수 있는 상태가 아닙니다.
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["MOVE_ORD_ID"] = sMoveOrderID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgReturn_Detail.GetRowCount(); i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK")).Equals("True"))
                    //{
                    if (DataTableConverter.GetValue(dgReturn_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                    {
                        DataRow row2 = inLot.NewRow();
                        row2["LOTID"] = DataTableConverter.GetValue(dgReturn_Detail.Rows[i].DataItem, "LOTID").ToString();

                        indataSet.Tables["INLOT"].Rows.Add(row2);
                    }
                    //}
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_SEND_RETURN_PACKLOT_SHOP", "INDATA,INLOT", null, indataSet);

                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                Util.gridClear(dgReturn_Master);
                Util.gridClear(dgReturn_Detail);

                Serarch("Cancel");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReturnHist_Click(object sender, RoutedEventArgs e)
        {
            Serarch("Cancel");
        }

        private void dgReturn_MasterChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            //if ((bool)rb.IsChecked)
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
                //    dgReturn_Master.BeginEdit();
                //    dgReturn_Master.ItemsSource = DataTableConverter.Convert(dt);
                //    dgReturn_Master.EndEdit();
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
                dgReturn_Master.SelectedIndex = idx;

                SearchMoveLOTList_Radio(DataTableConverter.GetValue(dgReturn_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgReturn_Detail);
            }
        }
        #endregion

        #region [인수이력]

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            Serarch("History");
        }

        private void dgMoveHist_MasterChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            //if ((bool)rb.IsChecked)
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
                //    dgMoveHist_Master.BeginEdit();
                //    dgMoveHist_Master.ItemsSource = DataTableConverter.Convert(dt);
                //    dgMoveHist_Master.EndEdit();
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
                dgMoveHist_Master.SelectedIndex = idx;

                SearchMoveLOTList_Radio(DataTableConverter.GetValue(dgMoveHist_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgMoveHist_Detail);

            }
        }

        #endregion
        private void Serarch(string sType)
        {
            try
            {
                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;
                string sMoveOrdStatCode = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("END_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("END_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("START_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("START_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));

                if (sType == "Receive")
                {
                    sMoveType = "MOVE_SHOP";

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["TO_EQSGID"] = null; // Util.GetCondition(cboFromEquipmentSegment, bAllNull: true); -- 조회시에 동에 상관없이 모든 인계내용 보이도록 변경
                    dr["MOVE_TYPE_CODE"] = sMoveType;
                    dr["MOVE_ORD_STAT_CODE"] = "MOVING";
                    dr["CSTID"] = txtSKIDID1.Text == "" ? null : txtSKIDID1.Text;
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID2", "RQSTDT", "RSLTDT", RQSTDT);

                    Util.gridClear(dgMove_Detail);
                    Util.GridSetData(dgMove_Master, SearchResult, FrameOperation);
                }
                else if (sType == "Cancel")
                {
                    sStart_date = Util.GetCondition(dtpDateFrom2);
                    sEnd_date = Util.GetCondition(dtpDateTo2);

                    sMoveType = "RETURN_SHOP";

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["MOVE_TYPE_CODE"] = sMoveType;
                    dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboReturnStat, bAllNull: true);
                    dr["PRODID"] = txtProd_ID2.Text == "" ? null : txtProd_ID2.Text;
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_V01", "RQSTDT", "RSLTDT", RQSTDT);

                    Util.gridClear(dgReturn_Detail);
                    Util.GridSetData(dgReturn_Master, SearchResult, FrameOperation);
                }
                else if (sType == "History")
                {
                    sStart_date = Util.GetCondition(dtpDateFromHist);
                    sEnd_date = Util.GetCondition(dtpDateToHist);
                    sMoveType = "MOVE_SHOP";
                    sMoveOrdStatCode = Util.GetCondition(cboStatHist, bAllNull: true);

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["MOVE_TYPE_CODE"] = sMoveType;
                    dr["MOVE_ORD_STAT_CODE"] = sMoveOrdStatCode;
                    dr["PRODID"] = txtProd_ID.Text == "" ? null : txtProd_ID.Text;
                    if ("END_MOVE".Equals(sMoveOrdStatCode) || "CLOSE_MOVE".Equals(sMoveOrdStatCode))
                    {
                        dr["FROM_DATE"] = sStart_date;
                        dr["TO_DATE"] = sEnd_date;
                    }
                    else
                    {
                        dr["START_FROM_DATE"] = sStart_date;
                        dr["START_TO_DATE"] = sEnd_date;
                    }
                    //dr["END_FROM_DATE"] = sStart_date;
                    //dr["END_TO_DATE"] = sEnd_date;

                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST_V01", "RQSTDT", "RSLTDT", RQSTDT);

                    Util.gridClear(dgMoveHist_Detail);
                    Util.GridSetData(dgMoveHist_Master, SearchResult, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = sMoveOrderID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    return;
                }

                DataTable LotList = DataTableConverter.Convert(DataGrid.ItemsSource);

                if (DataGrid.ItemsSource == null)
                {
                    Util.gridClear(DataGrid);
                    Util.GridSetData(DataGrid, SearchResult, FrameOperation);
                }
                else
                {
                    DataTable dtSource = DataTableConverter.Convert(DataGrid.ItemsSource);
                    dtSource.Merge(SearchResult);

                    Util.gridClear(DataGrid);
                    Util.GridSetData(DataGrid, dtSource, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SearchMoveLOTList_Radio(String sMoveOrderID, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = sMoveOrderID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    return;
                }

                DataTable LotList = DataTableConverter.Convert(DataGrid.ItemsSource);

                Util.gridClear(DataGrid);
                Util.GridSetData(DataGrid, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgMove_Master_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgMove_Master.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int seleted_index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                        string seleted_RoutID = string.Empty;
                        string seleted_AreaID = DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "TO_AREAID_CODE").ToString();
                        string seleted_ProdID = DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MODLID").ToString();

                        string compare_RoutID = string.Empty;
                        string compare_areaid = string.Empty;
                        string compare_prodid = string.Empty;

                        seleted_RoutID = getRout(seleted_AreaID, seleted_ProdID);

                        if (cb.IsChecked == true)
                        {
                            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK").ToString() == "1")
                                {
                                    compare_areaid = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "TO_AREAID_CODE").ToString();
                                    compare_prodid = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MODLID").ToString();

                                    compare_RoutID = getRout(compare_areaid, compare_prodid);

                                    if (!(seleted_RoutID == compare_RoutID && seleted_AreaID == compare_areaid))
                                    {
                                        Util.AlertInfo("SFU4493"); //인수라인이 상이하여 동시에 입고 처리 불가합니다.
                                        DataTableConverter.SetValue(dgMove_Master.Rows[seleted_index].DataItem, "CHK", 0);
                                        return;
                                    }
                                }
                            }

                            setEquipmentSegment_seletedRow(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "TO_AREAID_CODE").ToString(), getProcid(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID").ToString()));

                            SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);

                        }
                        else if (cb.IsChecked == false)
                        {
                            int chk_cnt = 0;
                            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK").ToString() == "1")
                                {
                                    chk_cnt++;
                                }
                            }

                            if (chk_cnt == 0)
                            {
                                setEquimentSegment_init();
                                //pre.Content = chkAll;
                                //dgMove_Master.TopRows  .SetValue(dgMove_Master.TopRows, false);
                                //Column.HeaderPresenter.Content = pre;
                                //dgMove_Master_LoadedColumnHeaderPresenter(dgMove_Master, C1.WPF.DataGrid.DataGridColumnEventArgs e);
                                //dgMove_Master_LoadedColumnHeaderPresenter(dgMove_Master, dgMove_Master.Dat e);

                            }

                            string sMove_Ord_id = Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID"));
                            DataTable dt = DataTableConverter.Convert(dgMove_Detail.ItemsSource);
                            if (dt.Rows.Count != 0)
                            {
                                dgMove_Detail.ItemsSource = dt.Select("MOVE_ORD_ID <> '" + sMove_Ord_id + "'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select("MOVE_ORD_ID <> '" + sMove_Ord_id + "'").CopyToDataTable());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.ToString());
                }

            }));

        }

        private void dgMove_Master_ByMoveOrd(C1.WPF.DataGrid.C1DataGrid sender, int row)
        {
            dgMove_Master.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    

                        int seleted_index = row; 

                        string seleted_RoutID = string.Empty;
                        string seleted_AreaID = DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "TO_AREAID_CODE").ToString();
                        string seleted_ProdID = DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MODLID").ToString();

                        string compare_RoutID = string.Empty;
                        string compare_areaid = string.Empty;
                        string compare_prodid = string.Empty;

                        seleted_RoutID = getRout(seleted_AreaID, seleted_ProdID);

                        if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[row].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[row].DataItem, "CHK")).Equals("True")) 
                        {
                            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK").ToString() == "1")
                                {
                                    compare_areaid = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "TO_AREAID_CODE").ToString();
                                    compare_prodid = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MODLID").ToString();

                                    compare_RoutID = getRout(compare_areaid, compare_prodid);

                                    if (!(seleted_RoutID == compare_RoutID && seleted_AreaID == compare_areaid))
                                    {
                                        Util.AlertInfo("SFU4493"); //인수라인이 상이하여 동시에 입고 처리 불가합니다.
                                        DataTableConverter.SetValue(dgMove_Master.Rows[seleted_index].DataItem, "CHK", 0);
                                        return;
                                    }
                                }
                            }

                            setEquipmentSegment_seletedRow(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "TO_AREAID_CODE").ToString(), getProcid(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID").ToString()));

                            SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);

                        }
                        else if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[row].DataItem, "CHK")).Equals("False") || Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[row].DataItem, "CHK")).Equals("0") )
                        {
                            int chk_cnt = 0;
                            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "CHK").ToString() == "1")
                                {
                                    chk_cnt++;
                                }
                            }

                            if (chk_cnt == 0)
                            {
                                setEquimentSegment_init();

                            }

                            string sMove_Ord_id = Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[seleted_index].DataItem, "MOVE_ORD_ID"));
                            DataTable dt = DataTableConverter.Convert(dgMove_Detail.ItemsSource);
                            if (dt.Rows.Count != 0)
                            {
                                dgMove_Detail.ItemsSource = dt.Select("MOVE_ORD_ID <> '" + sMove_Ord_id + "'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select("MOVE_ORD_ID <> '" + sMove_Ord_id + "'").CopyToDataTable());
                            }
                        }
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.ToString());
                }

            }));

        }

        private string getRout(string sareaid, string prodid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = prodid;
                dr["AREAID"] = sareaid;
                

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    return SearchResult.Rows[0]["ROUTID"].ToString(); ;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setEquipmentSegment_seletedRow(string areaid, string procid)
        {
            
            CommonCombo combo = new CommonCombo();
            C1ComboBox cboOperation = new C1ComboBox(); // LOT
            C1ComboBox cboArea = new C1ComboBox(); // LOT

            cboArea.SelectedValue = areaid;
            cboOperation.SelectedValue = procid;

            //공정
            cboOperation.DisplayMemberPath = "PROCID";
            cboOperation.SelectedValuePath = "PROCID";

            if (procid.Length == 0)
            {
                cboFromEquipmentSegment.ItemsSource = null;               
            }
            else
            {
                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboOperation };
                combo.SetCombo(cboFromEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "PROCESSEQUIPMENTSEGMENT");
            }
           
        }

        private void setEquimentSegment_init()
        {
            CommonCombo combo = new CommonCombo();

            string[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            //string[] sFilter1 = {null };
            combo.SetCombo(cboFromEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT", sFilter: sFilter1);
        }

        private string getProcid(string MOVE_ORD_ID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = MOVE_ORD_ID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PROCID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["TO_PROCID"].ToString().Length > 0)
                {
                    return SearchResult.Rows[0]["TO_PROCID"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


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
        

        private void dgMove_Master_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            string rout = string.Empty;

            ClearDataGrid(dgMove_Detail);

            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
            {
                //string rout = string.Empty;
                //string shop = string.Empty;
                //string prod = string.Empty;
                string sRoutID = string.Empty;
                string sareaID = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "TO_AREAID_CODE").ToString();
                string sProdID = DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MODLID").ToString();
                //string shopID_NEXT = DataTableConverter.GetValue(dgMove_Master.Rows[i+1].DataItem, "TO_SHOPID_CODE").ToString();
                //string sProdID_NEXT = DataTableConverter.GetValue(dgMove_Master.Rows[i+1].DataItem, "PRODID").ToString();
                sRoutID = getRout(sareaID, sProdID);

                if (i == 0)
                {
                    //shop = shopID;
                    //prod = sProdID;
                    rout = sRoutID;
                }

                if (rout != sRoutID)
                {
                    Util.MessageValidation(" SFU4444"); //인수라인이 상이하여 동시에 입고 처리 불가합니다.
                    for (int j = 0; j < dgMove_Master.GetRowCount(); j++)
                    {
                        DataTableConverter.SetValue(dgMove_Master.Rows[j].DataItem, "CHK", false);
                    }

                    chkAll.IsChecked = false;
                    //DataTableConverter.SetValue(dgMove_Master.Rows[index].DataItem, "CHK", 0);
                    return;
                }

                DataTableConverter.SetValue(dgMove_Master.Rows[i].DataItem, "CHK", true);

                setEquipmentSegment_seletedRow(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "TO_AREAID_CODE").ToString(), getProcid(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID").ToString()));

                SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);

            }

            //bool routDiff_chk = true;

            //for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
            //{
            //    if (!Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "PRODID")).Equals(dgMove_Master.Rows[0]["PRODID"].ToString()))
            //    {
            //        routDiff_chk = false;
            //    }
            //}

            //if(!routDiff_chk)
            //{
            //    Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");

            //    ClearDataGrid(dgMove_Detail);


            //    return;
            //}

            

            //if ((bool)chkAll.IsChecked)
            //{
            //    for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
            //    {
            //        DataTableConverter.SetValue(dgMove_Master.Rows[i].DataItem, "CHK", true);
            //        SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);
            //    }
            //}
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgMove_Detail);
            for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgMove_Master.Rows[i].DataItem, "CHK", false);
            }

            setEquimentSegment_init();
        }

        private void dgMove_Master_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgMove_Detail_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    Serarch("History");
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
                {
                    Serarch("Cancel");
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearchHist5_Click(object sender, RoutedEventArgs e)
        {
            Search_MoveInfo();
        }
        private void dgMove_Info_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            initSumRow(sender, e);
        }

        private void initSumRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void txtProd_ID5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID5.Text != "")
                {
                    Search_MoveInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void Search_MoveInfo()
        {
            try
            {
                Util.gridClear(dgMove_Info);

                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;
                string sMoveOrdStatCode = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("END_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("END_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("START_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("START_TO_DATE", typeof(String));

                sStart_date = Util.GetCondition(dtpDateFromHist5);
                sEnd_date = Util.GetCondition(dtpDateToHist5);
                sMoveType = "MOVE_SHOP";
                sMoveOrdStatCode = Util.GetCondition(cboStatHist5, bAllNull: true);

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if("END_MOVE".Equals(sMoveOrdStatCode) || "CLOSE_MOVE".Equals(sMoveOrdStatCode))
                {
                    //dr["FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                    //dr["TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;

                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] =  sEnd_date;
                }
                else
                {
                    //dr["START_FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                    //dr["START_TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;

                    dr["START_FROM_DATE"] =  sStart_date;
                    dr["START_TO_DATE"] =  sEnd_date;
                }
                //dr["END_FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                //dr["END_TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = sMoveType;
                dr["MOVE_ORD_STAT_CODE"] = sMoveOrdStatCode;
                dr["PRODID"] = txtProd_ID5.Text == "" ? null : txtProd_ID5.Text;
                dr["LOTID"] = txtLotID2.Text == "" ? null : txtLotID2.Text;
                dr["CSTID"] = txtSkidID2.Text == "" ? null : txtSkidID2.Text;
                RQSTDT.Rows.Add(dr);

                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_MOVEINFO_HIST_V01", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgMove_Info, bizResult, FrameOperation);

                            string[] sColumnName = new string[] { "MOVE_ORD_ID", "CSTID" };
                            //_Util.SetDataGridMergeExtensionCol(dgReturn_Info, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            _Util.SetDataGridMergeExtensionCol(dgMove_Info, sColumnName, DataGridMergeMode.VERTICAL);

                            dgMove_Info.BottomRows[0].Visibility = Visibility.Visible;

                            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                            DataGridAggregateSum dagsum = new DataGridAggregateSum();
                            dagsum.ResultTemplate = dgMove_Info.Resources["ResultTemplate"] as DataTemplate;
                            dac.Add(dagsum);

                            DataGridAggregate.SetAggregateFunctions(dgMove_Info.Columns["WIPQTY"], dac);
                            DataGridAggregate.SetAggregateFunctions(dgMove_Info.Columns["WIPQTY2"], dac);


                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    });

                }
                catch (Exception ex)
                {
                    //조회 오류
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void txtProd_ID6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID6.Text != "")
                {
                    Search_ReturnInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnReturnHist6_Click(object sender, RoutedEventArgs e)
        {
            Search_ReturnInfo();
        }
        private void GetReceiveSkid()
        {
            try
            {
                //DateTime tmp;

                if (dtpReceiveToHist.SelectedDateTime == default(DateTime))
                {
                    return;
                }
                if (dtpReceiveFromHist.SelectedDateTime == default(DateTime))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));
                dt.Columns.Add("END_DTTM", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["STRT_DTTM"] = dtpReceiveFromHist.SelectedDateTime.ToShortDateString();
                dr["END_DTTM"] = dtpReceiveToHist.SelectedDateTime.Date.ToShortDateString();
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_ORD_RECEIVE_SKID", "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgRecieveSkidInfo, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void Search_ReturnInfo()
        {
            try
            {
                Util.gridClear(dgReturn_Info);

                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("END_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("END_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("SKIDID", typeof(String));

                sStart_date = Util.GetCondition(dtpDateFrom6);
                sEnd_date = Util.GetCondition(dtpDateTo6);

                sMoveType = "RETURN_SHOP";

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = txtLotID6.Text != "" ? null : sStart_date;
                dr["TO_DATE"] = txtLotID6.Text != "" ? null : sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = sMoveType;
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboReturnStat6, bAllNull: true);
                dr["PRODID"] = txtProd_ID6.Text == "" ? null : txtProd_ID6.Text;
                dr["LOTID"] = txtLotID6.Text == "" ? null : txtLotID6.Text;
                dr["SKIDID"] = txtReturnSkidID.Text == "" ? null : "%" + txtReturnSkidID.Text.Trim() + "%";
                RQSTDT.Rows.Add(dr);

                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_MOVEINFO_HIST", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgReturn_Info, bizResult, FrameOperation);

                            string[] sColumnName = new string[] { "MOVE_ORD_ID", "CSTID" };
                            //_Util.SetDataGridMergeExtensionCol(dgReturn_Info, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            _Util.SetDataGridMergeExtensionCol(dgReturn_Info, sColumnName, DataGridMergeMode.VERTICAL);

                            dgReturn_Info.BottomRows[0].Visibility = Visibility.Visible;

                            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                            DataGridAggregateSum dagsum = new DataGridAggregateSum();
                            dagsum.ResultTemplate = dgReturn_Info.Resources["ResultTemplate"] as DataTemplate;
                            dac.Add(dagsum);

                            DataGridAggregate.SetAggregateFunctions(dgReturn_Info.Columns["WIPQTY"], dac);
                            DataGridAggregate.SetAggregateFunctions(dgReturn_Info.Columns["WIPQTY2"], dac);

                            txtReturnSkidID.Text = "";

                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    });

                }
                catch (Exception ex)
                {
                    //조회 오류
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void dgReturn_Info_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            initSumRow(sender, e);
        }

        private void txtLotID6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID6.Text != "")
                {
                    Search_ReturnInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }
        private void txtReturnSkidID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtReturnSkidID.Text != "")
                {
                    Search_ReturnInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtLotID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID2.Text != "")
                {
                    Search_MoveInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void SearchAreaMoveConfirm()
        {
            try
            {
                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["CMCDTYPE"] = "AREA_AUTO_RECEIVE";
                dr3["CMCDIUSE"] = "Y";

                RQSTDT3.Rows.Add(dr3);
                DataTable dtArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);
                Tab_Receive.Visibility = Visibility.Visible;
                for (int i = 0; i < dtArea.Rows.Count; i++)
                {
                    if (dtArea.Rows[i]["CMCODE"].ToString().Equals(LoginInfo.CFG_AREA_ID))
                    {
                        Tab_Receive.Visibility = Visibility.Collapsed;  // Plant간이동(인수) Tab_Receive
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtLotID7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    SearchReturnConfrim(txtLotID7.Text.Trim());
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtSkid_ID7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtSkid_ID7.Text != "")
                {
                    //스키드 보기
                    SearchReturnConfrim(txtSkid_ID7.Text.Trim());

                    //인수확정
                    ReceiveSkidConfirmProcess();

                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }
        private void dtpReceiveFromHist_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            GetReceiveSkid();
        }
        private void dtpReceiveToHist_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            GetReceiveSkid();
        }
        //private void Tab_Return7_Loaded(object sender, RoutedEventArgs e)
        //{
        //    GetReceiveSkid();
        //}

        private void ReceiveSkidConfirmProcess()
        {
            int chk = 0;
            
            //C20200109-000262
            //cboToEquipmentsegment.SelectedValue
            if (cboToEquipmentsegment.SelectedIndex == 0 || cboToEquipmentsegment.SelectedValue.Equals("SELECT") || cboToEquipmentsegment.SelectedValue.Equals(string.Empty))
            {
                Util.MessageValidation("SFU1223"); //라인을 선택하세요
                return;
            }

            if (dgReturnConfrim_Info.Rows.Count == 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            System.Text.StringBuilder strConfirmMessage = new System.Text.StringBuilder();

            // 동일한 VD 호기 에서 인계된 SKID 중 현재 검색한 SKID 보다 이전에 인계하였지만 입고 안된 SKID 확인
            string sIncompMovePreCstInfo = GetIncompMovePreCst(txtLotID7.Text.Trim(), txtSkid_ID7.Text.Trim());
            if (!string.IsNullOrEmpty(sIncompMovePreCstInfo))
            {
                strConfirmMessage.Append(sIncompMovePreCstInfo);
                strConfirmMessage.Append("\n");
            }

            // 두께 불량 LOT 이 존재하는지 확인
            string sVdQaDfctHistInfo = GetVdQaDfctHist(txtLotID7.Text.Trim(), txtSkid_ID7.Text.Trim());
            if (!string.IsNullOrEmpty(sVdQaDfctHistInfo))
            {
                strConfirmMessage.Append(sVdQaDfctHistInfo);
                strConfirmMessage.Append("\n");
            }

            string sVdQaEtcHistInfo = GetVdQaEtcHist(txtLotID7.Text.Trim(), txtSkid_ID7.Text.Trim());
            if (!string.IsNullOrEmpty(sVdQaEtcHistInfo))
            {
                strConfirmMessage.Append(sVdQaEtcHistInfo);
                strConfirmMessage.Append("\n");
            }

            // 인계하는 팬케익 개수와 인수하는 팬케익 개수가 동일한지 확인
            string sPancakeCntInCstInfo = GetPancakeCntInCst(txtLotID7.Text.Trim(), txtSkid_ID7.Text.Trim());
            if (!string.IsNullOrEmpty(sPancakeCntInCstInfo))
            {
                strConfirmMessage.Append(sPancakeCntInCstInfo);
                strConfirmMessage.Append("\n");
            }

            //인수확정 하시겠습니까?
            strConfirmMessage.Append(MessageDic.Instance.GetMessage("SFU4430"));

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(strConfirmMessage.ToString(), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string sMoveOrderID = string.Empty;
                        string sMoveOrderID2 = string.Empty;
                        DataSet indataSet = new DataSet();

                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("EQSGID", typeof(string));
                        inData.Columns.Add("PCSGID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("MOVE_ORD_ID", typeof(string));

                        for (int j = 0; j < dgReturnConfrim_Info.Rows.Count; j++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "CHK")).Equals("1"))
                            {

                                DataRow row = inData.NewRow();
                                if (!sMoveOrderID.ToString().Equals(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString()) && !sMoveOrderID2.Equals(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString()))
                                {
                                    row["SRCTYPE"] = "UI";
                                    row["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
                                    row["AREAID"] = LoginInfo.CFG_AREA_ID;
                                    row["EQSGID"] = Convert.ToString(cboToEquipmentsegment.SelectedValue); //LoginInfo.CFG_EQSG_ID;
                                    row["PCSGID"] = "A";
                                    row["USERID"] = LoginInfo.USERID;

                                    indataSet.Tables["INDATA"].Rows.Add(row);
                                    if (chk == 0)
                                    {
                                        sMoveOrderID = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
                                        chk++;
                                    }
                                    sMoveOrderID2 = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
                                }
                            }
                        }

                        chk = 0;
                        for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
                        {

                            if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                DataRow row3 = inLot.NewRow();

                                row3["LOTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString();
                                row3["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "MOVE_ORD_ID").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row3);
                                chk++;
                            }
                        }

                        if (chk == 0)
                        {
                            Util.Alert("SFU1632");  //선택된 LOT이없습니다.
                            return;
                        }

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_PACKLOT_SHOP_MASTER", "INDATA,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.gridClear(dgReturnConfrim_Info);

                                System.Text.StringBuilder strResultMessage = new System.Text.StringBuilder();
                                if (!string.IsNullOrEmpty(sIncompMovePreCstInfo))
                                {
                                    strResultMessage.Append(sIncompMovePreCstInfo);
                                    strResultMessage.Append("\n");
                                }

                                if (!string.IsNullOrEmpty(sVdQaDfctHistInfo))
                                {
                                    strResultMessage.Append(sVdQaDfctHistInfo);
                                    strResultMessage.Append("\n");
                                }

                                //Util.AlertInfo("SFU1793");  //인수되었습니다.
                                strResultMessage.Append(MessageDic.Instance.GetMessage("SFU1793"));
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(strResultMessage.ToString(), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>    //인수되었습니다.
                                {
                                    if (vResult == MessageBoxResult.OK)
                                    {
                                        txtSkid_ID7.Focus();
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                GetReceiveSkid();
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                }
            });
        }

        private void btnRefresh7_Click(object sender, RoutedEventArgs e)
        {
            //초기화
            Util.gridClear(dgReturnConfrim_Info);
            txtLotID7.Text = null;
            txtSkid_ID7.Text = null;
            txtSkid_ID7.Focus();
        }

        private void txtLotID7_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSkid_ID7.Text = null;
        }

        private void txtSkid_ID7_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotID7.Text = null;
        }

        private void SearchReturnConfrim(string Lotid)
        {
            try
            {
                // SKID만 인수 처리 완료된 SKID인지 VALIDATION
                if (!string.IsNullOrEmpty(txtSkid_ID7.Text.ToString().Trim()))
                {
                    string sRecieveInfo = GetPlantRecieveConfirm(Lotid);
                    if (!string.IsNullOrEmpty(sRecieveInfo))
                    {
                        ControlsLibrary.MessageBox.Show(sRecieveInfo, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                }

                string sMoveOrderID = string.Empty;
                DataSet indataSet = new DataSet();

                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;

                sStart_date = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                sEnd_date = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                sMoveType = "MOVE_SHOP";

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LANGID", typeof(string));
                //  RQSTDT2.Columns.Add("TO_EQSGID", typeof(String));
                //RQSTDT2.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT2.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT2.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT2.Columns.Add("LOTID", typeof(String));
                RQSTDT2.Columns.Add("CSTID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();

                dr2["LANGID"] = LoginInfo.LANGID;

                //dr2["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                // dr2["TO_EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr2["MOVE_TYPE_CODE"] = sMoveType;
                dr2["MOVE_ORD_STAT_CODE"] = "MOVING"; // CLOSE_MOVE , MOVING
                if (txtSkid_ID7.Text.ToString() == "")
                    dr2["LOTID"] = Lotid == "" ? null : Lotid;
                if (txtLotID7.Text.ToString() == "")
                    dr2["CSTID"] = Lotid == "" ? null : Lotid;
                RQSTDT2.Rows.Add(dr2);

                DataTable Result2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID", "RQSTDT", "RSLTDT", RQSTDT2);

                if (Result2.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2048");   //확정할 수 없는 상태입니다.
                    return;
                }
                else
                {
                    sMoveOrderID = Result2.Rows[0]["MOVE_ORD_ID"].ToString();
                    if (!Result2.Rows[0]["MOVE_ORD_STAT_CODE"].ToString().Equals("MOVING"))
                    {
                        Util.AlertInfo("SFU2048"); //확정할 수 없는 상태입니다.
                        return;
                    }
                }

                if (dgReturnConfrim_Info.GetRowCount() == 0)
                {
                    Util.GridSetData(dgReturnConfrim_Info, Result2, FrameOperation);
                    dgReturnConfrim_Info_Checked(null, null);
                }
                else
                {
                    for (int i = 0; i < dgReturnConfrim_Info.GetRowCount(); i++)
                    {
                        for (int j = 0; j < Result2.Rows.Count; j++)
                        {
                            if (DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString().Equals(Result2.Rows[j]["LOTID"].ToString()))
                            {
                                Util.Alert("SFU1914");   //중복 스캔되었습니다.
                                return;
                            }
                            if (!DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "PRODID").ToString().Equals(Result2.Rows[j]["PRODID"].ToString()))
                            {
                                Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                                return;
                            }
                        }
                    }

                    dgReturnConfrim_Info.IsReadOnly = false;
                    for (int i = 0; i < Result2.Rows.Count; i++)
                    {
                        dgReturnConfrim_Info.BeginNewRow();
                        dgReturnConfrim_Info.EndNewRow(true);
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_ID", Result2.Rows[i]["MOVE_ORD_ID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_STAT_NAME", Result2.Rows[i]["MOVE_ORD_STAT_NAME"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "PRJT_NAME", Result2.Rows[i]["PRJT_NAME"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "PRODID", Result2.Rows[i]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "EQPTNAME", Convert.ToString(Result2.Rows[i]["EQPTNAME"]));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "VD_END_DTTM", Result2.Rows[i]["VD_END_DTTM"].ToString().Equals("") ? null : Result2.Rows[i]["VD_END_DTTM"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_QTY", Result2.Rows[i]["MOVE_ORD_QTY"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_CMPL_QTY", Util.NVC_DecimalStr(Result2.Rows[i]["MOVE_CMPL_QTY"].ToString().Equals("") ? "0" : Result2.Rows[i]["MOVE_CMPL_QTY"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_CNCL_QTY", Util.NVC_DecimalStr(Result2.Rows[i]["MOVE_CNCL_QTY"].ToString().Equals("") ? "0" : Result2.Rows[i]["MOVE_CNCL_QTY"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "FROM_AREAID", Result2.Rows[i]["FROM_AREAID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "FROM_EQSGID", Result2.Rows[i]["FROM_EQSGID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_USERID", Result2.Rows[i]["MOVE_USERID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "TO_AREAID", Result2.Rows[i]["TO_AREAID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "TO_EQSGID", Result2.Rows[i]["TO_EQSGID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "RCPT_USERID", Result2.Rows[i]["RCPT_USERID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "NOTE", Result2.Rows[i]["NOTE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_STAT_CODE", Result2.Rows[i]["MOVE_ORD_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_STAT_CODE", Result2.Rows[i]["ERP_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_ERR_CODE", Result2.Rows[i]["ERP_ERR_CODE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_ERR_CAUSE_CNTT", Result2.Rows[i]["ERP_ERR_CAUSE_CNTT"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "LOTID", Result2.Rows[i]["LOTID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "CSTID", Result2.Rows[i]["CSTID"].ToString());
                    }
                    dgReturnConfrim_Info.IsReadOnly = true;
                    dgReturnConfrim_Info.Refresh(true);
                    dgReturnConfrim_Info_Checked(null, null);



                }

                dgReturnConfrim_Info.BottomRows[0].Visibility = Visibility.Visible;

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgReturnConfrim_Info.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);

                DataGridAggregate.SetAggregateFunctions(dgReturnConfrim_Info.Columns["MOVE_ORD_QTY"], dac);
                DataGridAggregate.SetAggregateFunctions(dgReturnConfrim_Info.Columns["MOVE_CMPL_QTY"], dac);
                DataGridAggregate.SetAggregateFunctions(dgReturnConfrim_Info.Columns["MOVE_CNCL_QTY"], dac);

                txtSkid_ID7.Text = "";
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
                return;
            }
        }

        void dgReturnConfrim_Info_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgReturnConfrim_Info.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgReturnConfrim_Info.ItemsSource = DataTableConverter.Convert(dt);
        }



        private void btnReturnConfrim7_Click(object sender, RoutedEventArgs e)
        {
            ReceiveSkidConfirmProcess();
            //인수확정
            //int chk = 0;
            //if (dgReturnConfrim_Info.Rows.Count == 0)
            //{
            //    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
            //    return;
            //}
            //try
            //{
            //    string sMoveOrderID = string.Empty;
            //    string sMoveOrderID2 = string.Empty;
            //    DataSet indataSet = new DataSet();

            //    DataTable inData = indataSet.Tables.Add("INDATA");
            //    inData.Columns.Add("SRCTYPE", typeof(string));
            //    inData.Columns.Add("MOVE_ORD_ID", typeof(string));
            //    inData.Columns.Add("AREAID", typeof(string));
            //    inData.Columns.Add("EQSGID", typeof(string));
            //    inData.Columns.Add("PCSGID", typeof(string));
            //    inData.Columns.Add("USERID", typeof(string));

            //    DataTable inLot = indataSet.Tables.Add("INLOT");
            //    inLot.Columns.Add("LOTID", typeof(string));
            //    inLot.Columns.Add("MOVE_ORD_ID", typeof(string));

            //    for (int j = 0; j < dgReturnConfrim_Info.Rows.Count; j++)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "CHK")).Equals("1"))
            //        {

            //            DataRow row = inData.NewRow();
            //            if (!sMoveOrderID.ToString().Equals(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString()) && !sMoveOrderID2.Equals(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString()))
            //            {
            //                row["SRCTYPE"] = "UI";
            //                row["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
            //                row["AREAID"] = LoginInfo.CFG_AREA_ID;
            //                row["EQSGID"] = Convert.ToString(cboToEquipmentsegment.SelectedValue); //LoginInfo.CFG_EQSG_ID;
            //                row["PCSGID"] = "A";
            //                row["USERID"] = LoginInfo.USERID;

            //                indataSet.Tables["INDATA"].Rows.Add(row);
            //                if (chk == 0)
            //                {
            //                    sMoveOrderID = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
            //                    chk++;
            //                }
            //                sMoveOrderID2 = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[j].DataItem, "MOVE_ORD_ID").ToString();
            //            }
            //        }
            //    }

            //    chk = 0;
            //    for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
            //    {

            //        if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
            //        {
            //            DataRow row3 = inLot.NewRow();

            //            row3["LOTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString();
            //            row3["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "MOVE_ORD_ID").ToString();

            //            indataSet.Tables["INLOT"].Rows.Add(row3);
            //            chk++;
            //        }
            //    }

            //    if (chk == 0)
            //    {
            //        Util.Alert("SFU1632");  //선택된 LOT이없습니다.
            //        return;
            //    }

            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_PACKLOT_SHOP_MASTER", "INDATA,INLOT", null, (searchResult, searchException) =>
            //    {
            //        try
            //        {
            //            if (searchException != null)
            //            {
            //                Util.MessageException(searchException);
            //                return;
            //            }

            //            Util.gridClear(dgReturnConfrim_Info);
            //            //Util.AlertInfo("SFU1793");  //인수되었습니다.
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1793"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>    //인수되었습니다.
            //            {
            //                if (vResult == MessageBoxResult.OK)
            //                {
            //                    txtSkid_ID7.Focus();
            //                }
            //            });
            //        }
            //        catch (Exception ex)
            //        {
            //            Util.MessageException(ex);
            //        }
            //    }, indataSet);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //    return;
            //}
        }

        private void dgReturnConfrim_Info_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            initSumRow(sender, e);
        }


        #region[전극VD검사결과]
        private void txtVDSKIDID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SearchVDQA();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtVDSKIDID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtVDSKIDID.Text = "";
        }

        private void SearchVDQA()
        {

            DataSet ds = new DataSet();

            DataTable dt = ds.Tables.Add("INDATA");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["CSTID"] = txtVDSKIDID.Text.Equals("") ? null : txtVDSKIDID.Text;
            dt.Rows.Add(dr);

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_MOVE_SKID_VD_QA_RESULT", "INDATA", "OUTDATA", (result, exception) =>
            {
                try
                {
                    Util.GridSetData(dgVDQAInfo, result.Tables["OUTDATA"], FrameOperation, true);

                    _Util.SetDataGridMergeExtensionCol(dgVDQAInfo, new string[] { "VD_EQPTNAME" }, DataGridMergeMode.VERTICAL);
                    _Util.SetDataGridMergeExtensionCol(dgVDQAInfo, new string[] { "EQPT_FLOOR_NO" }, DataGridMergeMode.VERTICAL);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                    txtVDSKIDID.Text = "";
                }

            }, ds);

        }

        private void btnVDQASearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchVDQA();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgVDQAInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgVDQAInfo.CurrentCell.Column.Name.Equals("FINAL_JUDG_VALUE_NAME"))
            {
                COM001_039_VDQA wndPopup = new COM001_039_VDQA();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgVDQAInfo.CurrentRow.DataItem, "CSTID"));


                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        #endregion

        private void dgVDQAInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));

            if (e.Cell.Column.Name.Equals("FINAL_JUDG_VALUE_NAME"))
            {
                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("blue"));
            }
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1TabItem newitem = e.AddedItems[0] as C1TabItem;
                    if (newitem != null)
                    {
                        if (string.Equals(newitem.Name, "Tab_Return7"))
                        {
                            UpdateLayout();
                            txtSkid_ID7.Focus();
                        }
                    }
                }));
            }
        }

        private string GetPlantRecieveConfirm(string sCstID)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CSTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CSTID"] = sCstID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_RECIEVE_CONFIRM_SKID", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    strBuilder.Append(MessageDic.Instance.GetMessage("SFU4108", new object[] { sCstID })); // 이미 인수완료 처리 된 Skid[%1] 입니다.
                    strBuilder.Append("\n\n");
                    strBuilder.Append(ObjectDic.Instance.GetObjectName("이동종료시간") + " : " + Util.NVC(result.Rows[0]["MOVE_END_DTTM"]));
                    strBuilder.Append("\n");
                    strBuilder.Append(ObjectDic.Instance.GetObjectName("인수자") + " : " + Util.NVC(result.Rows[0]["RCPT_USER"]));
                    strBuilder.Append("\n");
                    strBuilder.Append(ObjectDic.Instance.GetObjectName("인수자") + "DESC : " + Util.NVC(result.Rows[0]["USERDESC"]));
                }
            }
            catch (Exception ex) { }

            return strBuilder.ToString();
        }

        private void txtSkidID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtSkidID2.Text != "")
                {
                    Search_MoveInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private string GetIncompMovePreCst(string sLotId, string sCstID)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            int chk = 0;

            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));

                DataRow rowINDATA = inData.NewRow();
                rowINDATA["LANGID"] = LoginInfo.LANGID;
                rowINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;

                indataSet.Tables["INDATA"].Rows.Add(rowINDATA);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("MOVE_TYPE_CODE", typeof(string));
                inLot.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (indataSet.Tables["INLOT"].Select("CSTID = '" + DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CSTID").ToString().Trim() + "'").Length == 0)
                        {
                            DataRow rowINLOT = inLot.NewRow();
                            rowINLOT["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                            rowINLOT["CSTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CSTID").ToString().Trim();
                            indataSet.Tables["INLOT"].Rows.Add(rowINLOT);
                        }

                        chk++;
                    }
                }

                if (chk > 0)
                {

                    DataSet outData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_INCOMP_MOVE_PRE_CST_LIST", "INDATA,INLOT", "OUTDATA", indataSet);

                    if (outData != null && outData.Tables["OUTDATA"] != null && outData.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        strBuilder.Append(MessageDic.Instance.GetMessage("SFU4429")); // 인수 완료되지 않은 SKID 가 존재합니다.
                        for (int inx = 0; inx < outData.Tables["OUTDATA"].Rows.Count; inx++)
                        {
                            strBuilder.Append("\n");
                            strBuilder.Append(ObjectDic.Instance.GetObjectName("SKID ID") + " : " + Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["CSTID"]) + ", ");
                            strBuilder.Append(ObjectDic.Instance.GetObjectName("인계일시") + " : " + Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["MOVE_STRT_DTTM"]));
                        }

                        strBuilder.Append("\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //return strBuilder.ToString();
            }

            return strBuilder.ToString();
        }

        private string GetPancakeCntInCst(string sLotId, string sCstID)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            int chk = 0;

            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));

                DataRow rowINDATA = inData.NewRow();
                rowINDATA["LANGID"] = LoginInfo.LANGID;

                indataSet.Tables["INDATA"].Rows.Add(rowINDATA);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (indataSet.Tables["INLOT"].Select("CSTID = '" + DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CSTID").ToString().Trim() + "'").Length == 0)
                        {
                            DataRow rowINLOT = inLot.NewRow();
                            rowINLOT["CSTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CSTID").ToString().Trim();
                            indataSet.Tables["INLOT"].Rows.Add(rowINLOT);
                        }

                        chk++;
                    }
                }

                if (chk > 0)
                {

                    DataSet outData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PANCAKE_CNT", "INDATA,INLOT", "OUTDATA", indataSet);

                    if (outData != null && outData.Tables["OUTDATA"] != null && outData.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        for (int inx = 0; inx < outData.Tables["OUTDATA"].Rows.Count; inx++)
                        {
                            decimal pancakeQty = Convert.ToDecimal(Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["PANCAKE_QTY"]));
                            decimal pancakeCnt = Convert.ToDecimal(Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["PANCAKE_CNT"]));
                            decimal selectCnt = DataTableConverter.Convert(dgReturnConfrim_Info.ItemsSource).Select("CSTID = '" + Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["CSTID"]) + "' AND CHK = 1").Length;

                            if (pancakeQty > 0 && ( pancakeQty != pancakeCnt || pancakeQty != selectCnt))
                            {
                                strBuilder.Append(MessageDic.Instance.GetMessage("SFU4923")); // 인수 SKID 총 개수와, 인계 SKID 총 개수가 다릅니다. 일정 시간 이후, 다시 인수 시도해보세요.
                                strBuilder.Append("\n");
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("SKID ID") + " : " + Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["CSTID"]) + ", ");
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("인계") + " : " + pancakeQty + ", ");
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("인수") + " : " + pancakeCnt + ", ");
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("선택") + " : " + selectCnt);
                                strBuilder.Append("\n");
                            }
                        }        
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //return strBuilder.ToString();
            }

            return strBuilder.ToString();
        }
        private string GetVdQaDfctHist(string sLotId, string sCstID)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            int chk = 0;

            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));

                DataRow rowINDATA = inData.NewRow();
                rowINDATA["LANGID"] = LoginInfo.LANGID;

                indataSet.Tables["INDATA"].Rows.Add(rowINDATA);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("DFCT_CODE", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        // VD 품질 검사가 LOT 7자리 기준으로 하므로 7 자리 까지 같은 LOT 인 경우 파라미터로 넣지 않음
                        if (indataSet.Tables["INLOT"].Select("LOTID LIKE '" + DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString().Trim().Substring(0, 7) + "%'").Length == 0)
                        {
                            DataRow rowINLOT = inLot.NewRow();
                            rowINLOT["DFCT_CODE"] = "DF";
                            rowINLOT["LOTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString().Trim();
                            indataSet.Tables["INLOT"].Rows.Add(rowINLOT);
                        }

                        chk++;
                    }
                }

                if (chk > 0)
                {
                    DataSet outData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_VD_QA_DFCT_HIST_LIST", "INDATA,INLOT", "OUTDATA", indataSet);

                    if (outData != null && outData.Tables["OUTDATA"] != null && outData.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        strBuilder.Append(MessageDic.Instance.GetMessage("SFU4432")); // 두께불량 차수입니다. 이력카드에 불량내역 별도 표기바랍니다.
                        for (int inx = 0; inx < outData.Tables["OUTDATA"].Rows.Count; inx++)
                        {
                            strBuilder.Append("\n");
                            strBuilder.Append(Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["LOTID"]));
                        }

                        strBuilder.Append("\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //return strBuilder.ToString();
            }

            return strBuilder.ToString();
        }

        private string GetVdQaEtcHist(string sLotId, string sCstID)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            int chk = 0;

            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));

                DataRow rowINDATA = inData.NewRow();
                rowINDATA["LANGID"] = LoginInfo.LANGID;

                indataSet.Tables["INDATA"].Rows.Add(rowINDATA);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("DFCT_CODE", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgReturnConfrim_Info.Rows.Count; i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        // VD 품질 검사가 LOT 7자리 기준으로 하므로 7 자리 까지 같은 LOT 인 경우 파라미터로 넣지 않음
                        if (indataSet.Tables["INLOT"].Select("LOTID LIKE '" + DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString().Trim().Substring(0, 7) + "%'").Length == 0)
                        {
                            DataRow rowINLOT = inLot.NewRow();
                            rowINLOT["DFCT_CODE"] = "ETC";
                            rowINLOT["LOTID"] = DataTableConverter.GetValue(dgReturnConfrim_Info.Rows[i].DataItem, "LOTID").ToString().Trim();
                            indataSet.Tables["INLOT"].Rows.Add(rowINLOT);
                        }

                        chk++;
                    }
                }

                if (chk > 0)
                {
                    DataSet outData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_VD_QA_DFCT_HIST_LIST", "INDATA,INLOT", "OUTDATA", indataSet);

                    if (outData != null && outData.Tables["OUTDATA"] != null && outData.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        strBuilder.Append(MessageDic.Instance.GetMessage("SFU4415")); // 기타불량 차수입니다. 이력카드에 불량내역 별도 표기바랍니다.
                                                                                      //최상민 : SFU4415 메시지가 실전 MMD에 사용안함, 운영에 미등록 상태, UI 소스에는 메시지 코드 적용된게 있음, 운영에 없으므로 미사용으로 인지하고 내용 변경함

                        for (int inx = 0; inx < outData.Tables["OUTDATA"].Rows.Count; inx++)
                        {
                            strBuilder.Append("\n");
                            strBuilder.Append(Util.NVC(outData.Tables["OUTDATA"].Rows[inx]["LOTID"]));
                        }
                        strBuilder.Append("\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //return strBuilder.ToString();
            }

            return strBuilder.ToString();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgMove_Master);
            Util.gridClear(dgMove_Detail);
        }

        private void txtCARRIERID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
               SearhCarrierID(txtCARRIERID.Text);
            }
        }

        private void SearhCarrierID(string sCarrierID)
        {
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID_MOVE", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCARRIERID.Focus();
                            txtCARRIERID.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    SearchSKIDID(dtLot.Rows[0]["LOTID"].ToString());

                    txtCARRIERID.Focus();
                    txtCARRIERID.SelectAll();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchSKIDID(string sLOTID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                dr["MOVE_ORD_STAT_CODE"] = "MOVING";
                dr["LOTID"] = sLOTID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID2", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU4564"); // 캐리어 정보가 없습니다.
                    txtCARRIERID.Focus();
                    txtCARRIERID.SelectAll();
                    return;
                }

                for (int ii = 0; ii < SearchResult.Rows.Count; ii++)
                    SearchResult.Rows[ii]["CHK"] = 1;

                if (dgMove_Master.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgMove_Master.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[i].DataItem, "MOVE_ORD_ID")).Equals(SearchResult.Rows[0]["MOVE_ORD_ID"].ToString()))
                        {
                            return;
                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgMove_Master.ItemsSource);
                    dt.Merge(SearchResult);

                    Util.GridSetData(dgMove_Master, dt, FrameOperation);
                }
                else
                {
                    Util.GridSetData(dgMove_Master, SearchResult, FrameOperation);
                }

                for (int j = 0; j < dgMove_Master.GetRowCount(); j++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Master.Rows[j].DataItem, "MOVE_ORD_ID")).Equals(SearchResult.Rows[0]["MOVE_ORD_ID"].ToString()))
                    {
                        dgMove_Master_ByMoveOrd(dgMove_Master, j);
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

        }

        private void txtCARRIERID_01_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!RepLotUseArea)
                    SearchReturnCarrierID(txtCARRIERID_01.Text);
                else
                {
                    errChk = false;
                    errFlag = false;
                    SearchReturnRepLotID(txtCARRIERID_01.Text);
                }
            }
        }

        private void SearchReturnCarrierID(string sCarrierID)
        {
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCARRIERID_01.Focus();
                            txtCARRIERID_01.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    ScanLotSearch(dtLot.Rows[0]["LOTID"].ToString());
                    
                    txtCARRIERID_01.Focus();
                    txtCARRIERID_01.SelectAll();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchReturnRepLotID(string sCarrierID)
        {
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
              
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_REP_LOTID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCARRIERID_01.Focus();
                            txtCARRIERID_01.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    for (int i = 0; i < dtLot.Rows.Count; i++)
                    {
                        if (errFlag)
                        {
                            errChk = true;
                            break;
                        }
                        ScanLotSearch(dtLot.Rows[i]["LOTID"].ToString());
                    }

                    txtCARRIERID_01.Focus();
                    txtCARRIERID_01.SelectAll();

                    if(RepLotUseArea && errChk)
                        Util.gridClear(dgReturnList);

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void ScanLotSearch(string sLotID)
        {
            try
            {
                if (sLotID == null || sLotID == "")
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                for (int i = 0; i < dgReturnList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgReturnList.Rows[i].DataItem, "LOTID").ToString() == sLotID)
                    {
                        Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                        return;
                    }
                }

                errFlag = true; //대표 LOT 적용시 LOT 오류 체크를 위해 설정

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count >= 1)
                {
                    if (!Result.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        Util.Alert("SFU1869");  //재공 상태가 이동가능한 상태가 아닙니다.
                        return;
                    }

                    if (Result.Rows[0]["PCSGID"].ToString() != "A")
                    {
                        Util.AlertInfo("SFU4937"); //조립에서만 반품이 가능합니다. 확인 바랍니다.

                        return;
                    }

                    //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                    //기존 Validation 삭제
                    //if (dgReturnList.GetRowCount() > 1)
                    //{
                    //    if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "LOTTYPE").ToString() != Result.Rows[0]["LOTTYPE"].ToString())
                    //    {
                    //        if ((DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "LOTTYPE").ToString() == "X") || (Result.Rows[0]["LOTTYPE"].ToString() == "X"))
                    //        {
                    //            Util.Alert("SFU5149");  //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                    //            return;
                    //        }
                    //    }
                    //}

                    //반품 승인 정보 있는지 Check.
                    if (!ChkRtnApprReq(sLotID))
                        return;

                    if (dgReturnList.GetRowCount() == 0)
                    {
                        dgReturnList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PRODID").ToString() == Result.Rows[0]["PRODID"].ToString())
                        {
                            if (DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "SLOC_ID").ToString() == Result.Rows[0]["SLOC_ID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgReturnList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgReturnList.ItemsSource);
                                    dtSource.Merge(Result);

                                    Util.gridClear(dgReturnList);
                                    dgReturnList.ItemsSource = DataTableConverter.Convert(dtSource);
                                    errFlag = false;
                                }
                                else
                                {
                                    Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    return;
                                }
                            }
                            else
                            {
                                Util.Alert("SFU4572"); //저장소가 다릅니다.
                                return;
                            }
                        }
                        else
                        {
                            Util.Alert("SFU1893");      //제품ID가 같지 않습니다.
                            return;
                        }
                        
                    }
                }
                else
                {
                    Util.Alert("SFU1870");      //재공 정보가 없습니다.
                    return;
                }

                errFlag = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private bool ChkRtnApprReq(string sLotID)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow row2 = inLot.NewRow();
                row2["LOTID"] = sLotID;

                indataSet.Tables["INLOT"].Rows.Add(row2);
                
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_CHK_RTN_APPR_REQ", "INDATA,INLOT", null, indataSet);

                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void chkMultiInput_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if ((bool)chk.IsChecked)
            {
                _Multi_Input_YN = true;
            }
        }

        private void chkMultiInput_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!(bool)chk.IsChecked)
            {
                _Multi_Input_YN = false;
            }
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V && _Multi_Input_YN == true)
            {
                try
                {
                    ShowLoadingIndicator();

                    string sPasteString = Clipboard.GetText();
                    string[] stringSeparators = new string[] { "\r\n" };
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i].Trim()) && Scan_Lot_Search(sPasteStrings[i].Trim()) == false)
                        {
                            break;
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        #region [E20240206-000574] 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
        private void CheckAreaUseTWS(string sArea)
        {
            try
            {
                isTWS_LOADING_TRACKING = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sArea;
                dr["COM_TYPE_CODE"] = "TWS_LOADING_TRACKING_YN";
                dr["COM_CODE"] = "TWS_LOADING_TRACKING_YN";
                dr["USE_FLAG"] = 'Y';

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["ATTR1"].ToString().Equals("Y") || dtResult.Rows[0]["ATTR1"].ToString().Equals("P"))
                        isTWS_LOADING_TRACKING = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void GetRepLotUseArea()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    RepLotUseArea = true;
                }
                else
                {
                    RepLotUseArea = false;
                }
            }
        }
    }
}
