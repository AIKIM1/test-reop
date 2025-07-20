/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.08.19  김지은    : 반품 시 LOT유형의 시험생산구분코드 Validation
  2023.11.06  김도형    : [E20231101-000795] MES UI improvement from pain point
  2024.03.04  이동주    : [E20240126-001976] Lot ID Multi 등록 기능
  2024.03.18  조범모    : [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
  2024.05.02  김도형    : [E20240320-001047] [소형_전극]전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
  2024.05.19  배현우    : 대표LOTID 사용하여 인계 기능 추가 공통코드(REP_LOT_USE_AREA) 사용 AREA
  2024.06.19  유명환    : [E20240603-000303] 인계상세이력, 반품상세이력 탭 LOTTYPE 조회조건및 조회내용 추가
  2025.03.26  오화백    :  HD  증설   LOTTYPE 콤보박스 조회시 공통코드  DEMAND_TYPE ==> LOTTYPE으로 변경함
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
using C1.WPF.DataGrid.Summaries;
using System.Linq;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_040 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        
        Util _Util = new Util();

        public DataTable dtBasicInfo;   //01
        public DataTable dtPackingCard; //02

        //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 
        bool isTWS_LOADING_TRACKING = false;

        // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청 
        private string _OPMODE = string.Empty;             // 동별공통코드 PLANT_TO_PLANT_TRANS_OPMODE_FLAG 의 ATTR1 값

        private bool RepLotUseArea = false; // 대표LOT 사용 AREA 조회
        private bool errChk = false;  // 대표LOT 조회시 하나만 NG 발생해도 LIST 초기화 하도록 수정 2024-05-19
        private bool errFlag = false; // 대표LOT 조회시 하나만 NG 발생해도 LIST 초기화 하도록 수정 2024-05-19
        public COM001_040()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOut);
            listAuth.Add(btnCancel);
            listAuth.Add(btnReturnConfrim);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;

            dtpDateFrom4.SelectedDataTimeChanged += dtpDateFrom4_SelectedDataTimeChanged;
            dtpDateTo4.SelectedDataTimeChanged += dtpDateTo4_SelectedDataTimeChanged;

            dtpDateFrom6.SelectedDataTimeChanged += dtpDateFrom6_SelectedDataTimeChanged;
            dtpDateTo6.SelectedDataTimeChanged += dtpDateTo6_SelectedDataTimeChanged;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                SearchAreaReturnConfirm();
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

                dtpDateFrom6.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo6.SelectedDateTime = (DateTime)System.DateTime.Now;

                dtpDateFrom4.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo4.SelectedDateTime = (DateTime)System.DateTime.Now;

                CommonCombo combo = new CommonCombo();

                string[] sFilter2 = { "ALL" };
                string[] sFilter3 = { "" };                

                //SHOP
                C1ComboBox[] cboFromAreaChild = { cboAreaFrom };
                combo.SetCombo(cboShopFrom, CommonCombo.ComboStatus.NONE, cbChild: cboFromAreaChild, sCase: "FROMSHOP");

                //동
                C1ComboBox[] cboFromAreaParent = { cboShopFrom };
                combo.SetCombo(cboAreaFrom, CommonCombo.ComboStatus.NONE, cbParent: cboFromAreaParent, sCase: "AREA_NO_AUTH", sFilter: sFilter2);
                


                //SHOP
                C1ComboBox[] cboShopChild = { cboArea_Plant, cboEquipmentSegment };
                combo.SetCombo(cboShop, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sCase: "SHOPRELEATION");

                //동
                C1ComboBox[] cboAreaParent = { cboShop };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                //combo.SetCombo(cboArea_Plant, CommonCombo.ComboStatus.NONE, cbParent: cboAreaParent, cbChild: cboAreaChild, sCase: "AREA_NO_AUTH", sFilter: sFilter3);

                string[] sParameter = { cboShopFrom.SelectedValue.ToString(), cboAreaFrom.SelectedValue.ToString(), cboShop.SelectedValue.ToString() };
                combo.SetCombo(cboArea_Plant, CommonCombo.ComboStatus.NONE, cbParent: cboAreaParent, cbChild: cboAreaChild, sCase: "PLANT_AREA", sFilter: sParameter);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea_Plant };
                combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

                //combo.SetCombo(cboFromShop2, CommonCombo.ComboStatus.NONE, sCase: "FROMSHOP");
                // 인계이력조회탭 [E20231101-000795] MES UI improvement from pain point 
                //인계SHOP    
                C1ComboBox[] cboShopChildFrom2 = { cboFromArea_Plant2, cboFromEquipmentSegment2 };
                combo.SetCombo(cboFromShop2, CommonCombo.ComboStatus.NONE, cbChild: cboShopChildFrom2, sCase: "FROMSHOP");
                //인계 동
                C1ComboBox[] cboAreaParentFrom2 = { cboFromShop2 };
                C1ComboBox[] cboAreaChildFrom2 = { cboFromEquipmentSegment2 };
                combo.SetCombo(cboFromArea_Plant2, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParentFrom2, cbChild: cboAreaChildFrom2, sCase: "AREA_NO_AUTH", sFilter: sFilter3);
                //인계 라인
                C1ComboBox[] cboEquipmentSegmentParentFrom2= { cboFromArea_Plant2 };
                combo.SetCombo(cboFromEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom2, sCase: "EQUIPMENTSEGMENT_PLANT");


                //SHOP
                C1ComboBox[] cboShopChildFrom = { cboFromArea_Plant, cboFromEquipmentSegment };
                combo.SetCombo(cboFromShop, CommonCombo.ComboStatus.ALL, cbChild: cboShopChildFrom, sCase: "SHOPRELEATION");

                //동
                C1ComboBox[] cboAreaParentFrom = { cboFromShop };
                C1ComboBox[] cboAreaChildFrom = { cboFromEquipmentSegment };
                combo.SetCombo(cboFromArea_Plant, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParentFrom, cbChild: cboAreaChildFrom, sCase: "AREA_NO_AUTH", sFilter: sFilter3);

                //라인
                C1ComboBox[] cboEquipmentSegmentParentFrom = { cboFromArea_Plant };
                combo.SetCombo(cboFromEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom, sCase: "EQUIPMENTSEGMENT_PLANT");

                string[] sFilter5 = { "MOVE_ORD_STAT_CODE" };
                combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter5);

                combo.SetCombo(cboReturnStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter5);

                cboReturnStat.SelectedValue = "MOVING";


                C1ComboBox[] cboShopChildFrom4 = { cboFromArea_Plant4, cboFromEquipmentSegment4 };
                combo.SetCombo(cboFromShop4, CommonCombo.ComboStatus.ALL, cbChild: cboShopChildFrom4, sCase: "SHOPRELEATION");

                C1ComboBox[] cboAreaParentFrom4 = { cboFromShop4 };
                C1ComboBox[] cboAreaChildFrom4 = { cboFromEquipmentSegment4 };
                combo.SetCombo(cboFromArea_Plant4, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParentFrom4, cbChild: cboAreaChildFrom4, sCase: "AREA_NO_AUTH", sFilter: sFilter3);

                C1ComboBox[] cboEquipmentSegmentParentFrom4 = { cboFromArea_Plant4 };
                combo.SetCombo(cboFromEquipmentSegment4, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom4, sCase: "EQUIPMENTSEGMENT_PLANT");

                combo.SetCombo(cboStat4, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter5);


                //combo.SetCombo(cboFromShop5, CommonCombo.ComboStatus.NONE, sCase: "FROMSHOP");
                // 인계상세이력 탭 [E20231101-000795] MES UI improvement from pain point 
                C1ComboBox[] cboShopChildFrom5 = { cboFromArea_Plant5, cboFromEquipmentSegment5 };
                combo.SetCombo(cboFromShop5, CommonCombo.ComboStatus.NONE, cbChild: cboShopChildFrom5, sCase: "FROMSHOP");
                //인계 동
                C1ComboBox[] cboAreaParentFrom5 = { cboFromShop5 };
                C1ComboBox[] cboAreaChildFrom5 = { cboFromEquipmentSegment5 };
                combo.SetCombo(cboFromArea_Plant5, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParentFrom5, cbChild: cboAreaChildFrom5, sCase: "AREA_NO_AUTH", sFilter: sFilter3);
                //인계 라인
                C1ComboBox[] cboEquipmentSegmentParentFrom5 = { cboFromArea_Plant5 };
                combo.SetCombo(cboFromEquipmentSegment5, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom5, sCase: "EQUIPMENTSEGMENT_PLANT");


                //if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "A")
                //{
                //    btnReturnConfrim.Visibility = Visibility.Collapsed;
                //    Tab_Return.Visibility = Visibility.Collapsed;
                //}

                if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                {
                    btnReturnConfrim.Visibility = Visibility.Visible;
                }
                else
                {
                    btnReturnConfrim.Visibility = Visibility.Collapsed;
                }

                txtLotID_Out.Focus();

                chkPrint.IsChecked = false;

                #region //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동
                CheckAreaUseTWS(LoginInfo.CFG_AREA_ID);

                if (isTWS_LOADING_TRACKING == true)
                {
                    dgMoveList.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                    dgMoveList.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                    dgMoveList.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Visible;

                    dgMove_Detail.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                    dgMove_Detail.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                    dgMove_Detail.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgMoveList.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Collapsed;
                    dgMoveList.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Collapsed;
                    dgMoveList.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Collapsed;

                    dgMove_Detail.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Collapsed;
                    dgMove_Detail.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Collapsed;
                    dgMove_Detail.Columns["LOAD_WEIGHT2"].Visibility = Visibility.Collapsed;
                }
                #endregion

                GetRepLotUseArea();

                if(RepLotUseArea)
                    tbCarrierId.Text = ObjectDic.Instance.GetObjectName("대표 LOTID");

                SetLotType();
            }

                
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
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

        private void dtpDateFrom4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo4.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo4.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom4.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom4.SelectedDateTime;
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

        private void SearchAreaReturnConfirm()
        {
            try
            {
                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["CMCDTYPE"] = "AREA_RETURN_CONFIRM";
                dr3["CMCDIUSE"] = "Y";

                RQSTDT3.Rows.Add(dr3);
                DataTable dtArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);
                Tab_Return7.Visibility = Visibility.Collapsed;
                for (int i = 0; i < dtArea.Rows.Count; i++)
                {
                    if (dtArea.Rows[i]["CMCODE"].ToString().Equals(LoginInfo.CFG_AREA_ID))
                    {
                        Tab_Return.Visibility = Visibility.Collapsed;
                        Tab_Return7.Visibility = Visibility.Visible;
                        break;
                    }
                    else
                    {
                        Tab_Return.Visibility = Visibility.Visible;
                        Tab_Return7.Visibility = Visibility.Collapsed;
                        //break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                //Util.gridClear(DataGrid);
                //DataGrid.ItemsSource = DataTableConverter.Convert(DetailResult);

                Util.GridSetData(DataGrid, DetailResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SetLotType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "LOTTYPE";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_CHK", "INDATA", "OUTDATA", inTable);

                cboLotType.DisplayMemberPath = "CMCDNAME";
                cboLotType.SelectedValuePath = "CMCODE";

                cboLotType2.DisplayMemberPath = "CMCDNAME";
                cboLotType2.SelectedValuePath = "CMCODE";

                DataRow dr = dtResult.NewRow();
                dr["CMCDNAME"] = "-ALL-";
                dr["CMCODE"] = "";
                dtResult.Rows.InsertAt(dr, 0);

                cboLotType.ItemsSource = dtResult.Copy().AsDataView();
                cboLotType2.ItemsSource = dtResult.Copy().AsDataView();

                cboLotType.SelectedIndex = 0;
                cboLotType2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion


        #region [인계]

        //인계처리
        private void btnOut_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                // Shop간이동 인계
                string sMove_Ord_ID = string.Empty;

                if (dgMoveList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
                // 2024.10.18. 김영국 - HD1에서 사용하지 않는 내용으로 주석처리함.
                //string sOpmodePopupYn = GetPlantToPlantTransOpmodeFlag(); // 동별공통코드 PLANT_TO_PLANT_TRANS_OPMODE_FLAG  
                string sOpmodePopupYn = "N"; // 2024.10.18.김영국 - HD1에서는 무조건 로직 처리.

                #region # 특별관리 Lot - 인계가능 라인 체크
                string sLine = Util.GetCondition(cboEquipmentSegment);

                if (CommonVerify.HasDataGridRow(dgMoveList))
                {
                    DataTable dt = DataTableConverter.Convert(dgMoveList.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.Equals(Util.NVC(dt.Rows[i]["SPCL_FLAG"].ToString()), "Y") && !string.IsNullOrEmpty(Util.NVC(dt.Rows[i]["RSV_EQSGID_LIST"].ToString())))
                        {
                            if (!Util.NVC(dt.Rows[i]["RSV_EQSGID_LIST"].ToString()).Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(dt.Rows[i]["LOTID"].ToString()), Util.NVC(dt.Rows[i]["RSV_EQSGID_LIST"].ToString()) });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return;
                            }
                        }
                    }
                }
                #endregion 
                //인계처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2931"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        #region 2024.10.18. 김영국 - HD1에서는 사용하지 않는 로직 으로 주석처리함.
                        ////2020.11.05 포장 LOT 갯수 부족 메시지 
                        //DataTable RQSTDT2 = new DataTable();
                        //RQSTDT2.TableName = "RQSTDT";
                        //RQSTDT2.Columns.Add("SHOPID", typeof(String));
                        //RQSTDT2.Columns.Add("AREAID", typeof(String));
                        //RQSTDT2.Columns.Add("PRODID", typeof(String));
                        //RQSTDT2.Columns.Add("BIZTYPE", typeof(String));
                        //RQSTDT2.Columns.Add("BOXTYPE", typeof(String));
                        //RQSTDT2.Columns.Add("LOTCNT", typeof(String));

                        //DataRow dr2 = RQSTDT2.NewRow();
                        //dr2["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        //dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                        //dr2["PRODID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString();
                        //dr2["BIZTYPE"] = "M";
                        //dr2["BOXTYPE"] = "ALL"; //BIZTYPE 이 'M'이면 'ALL' 고정 SETTING
                        //dr2["LOTCNT"] = DataTableConverter.Convert(dgMoveList.ItemsSource).Rows.Count;

                        //RQSTDT2.Rows.Add(dr2);

                        //DataTable BoxLotResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_BOX_LOT_COND", "RQSTDT", "RSLTDT", RQSTDT2);

                        //if (BoxLotResult.Rows.Count > 0)
                        //{
                        //    if (BoxLotResult.Rows[0]["YN_FLAG"].ToString() == "N")
                        //    {
                        //        Util.MessageInfo("SFU8265", (result2) =>
                        //        {
                        //            if (result2 == MessageBoxResult.OK)
                        //            {
                        //                if (sOpmodePopupYn.Equals("Y"))
                        //                {
                        //                    PlantMove_Run_OpModeChk(); // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
                        //                }
                        //                else
                        //                {
                        //                    PlantMove_Run();
                        //                }
                        //            }
                        //        });
                        //    }
                        //    else
                        //    {
                        //        if (sOpmodePopupYn.Equals("Y"))
                        //        {
                        //            PlantMove_Run_OpModeChk(); // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
                        //        }
                        //        else
                        //        {
                        //            PlantMove_Run();
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    if (sOpmodePopupYn.Equals("Y"))
                        //    {
                        //        PlantMove_Run_OpModeChk(); // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
                        //    }
                        //    else
                        //    {
                        //        PlantMove_Run();
                        //    }
                        //} 
                        #endregion
                        
                        PlantMove_Run(); // 2024.10.18.김영국 - HD1에서는 무조건 로직 처리.
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        // 포장카드발행(2018.06.18)
        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Print wndPopup = sender as LGC.GMES.MES.COM001.Report_Print;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                    Util.gridClear(dgMoveList);
                }
                else
                {
                    Util.AlertInfo("SFU2930");//인계처리 되었습니다. 이력카드는 발행되지 않았습니다.

                    Util.gridClear(dgMoveList);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMoveOrderID = string.Empty;
                string sToArea = string.Empty;
                string sNote = string.Empty;
                string sVld = string.Empty;
                string sVld_date = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgMove_Master, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                else
                {
                    sMoveOrderID = drChk[0]["MOVE_ORD_ID"].ToString();
                    sToArea = drChk[0]["TO_AREAID"].ToString();
                    sNote = drChk[0]["NOTE"].ToString();
                }

                if (dgMove_Detail.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                DateTime readDttm = DateTime.Now;
                string sPrintDate = readDttm.Year.ToString() + "-" + readDttm.Month.ToString("00") + "-" + readDttm.Day.ToString("00");

                // Title DataTable
                dtPackingCard = new DataTable();
                dtPackingCard.TableName = "dtPackingCard";
                dtPackingCard.Columns.Add("TITLE", typeof(string));
                dtPackingCard.Columns.Add("MOVE_ID", typeof(string));
                dtPackingCard.Columns.Add("FROM", typeof(string));
                dtPackingCard.Columns.Add("FROM_AREA", typeof(string));
                dtPackingCard.Columns.Add("TO", typeof(string));
                dtPackingCard.Columns.Add("TO_AREA", typeof(string));
                dtPackingCard.Columns.Add("NOTE", typeof(string));
                dtPackingCard.Columns.Add("NOTE01", typeof(string));
                dtPackingCard.Columns.Add("T_01", typeof(string));
                dtPackingCard.Columns.Add("T_02", typeof(string));
                dtPackingCard.Columns.Add("T_03", typeof(string));
                dtPackingCard.Columns.Add("T_04", typeof(string));
                dtPackingCard.Columns.Add("T_05", typeof(string));
                dtPackingCard.Columns.Add("DATE", typeof(string));
                dtPackingCard.Columns.Add("DATE01", typeof(string));


                DataRow drCrad = dtPackingCard.NewRow();
                drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("이동ID");
                drCrad["MOVE_ID"] = sMoveOrderID;
                drCrad["FROM"] = ObjectDic.Instance.GetObjectName("출고처");
                drCrad["FROM_AREA"] = LoginInfo.CFG_AREA_NAME;
                drCrad["TO"] = ObjectDic.Instance.GetObjectName("입고처");
                drCrad["TO_AREA"] = sToArea;
                drCrad["NOTE"] = ObjectDic.Instance.GetObjectName("비고");
                drCrad["NOTE01"] = sNote;
                drCrad["T_01"] = ObjectDic.Instance.GetObjectName("프로젝트명");
                drCrad["T_02"] = ObjectDic.Instance.GetObjectName("버전");
                drCrad["T_03"] = ObjectDic.Instance.GetObjectName("단위");
                drCrad["T_04"] = ObjectDic.Instance.GetObjectName("제공");
                drCrad["T_05"] = ObjectDic.Instance.GetObjectName("유효기간");
                drCrad["DATE"] = ObjectDic.Instance.GetObjectName("발행일시");
                drCrad["DATE01"] = sPrintDate;

                dtPackingCard.Rows.Add(drCrad);



                // Lot Info DataTable
                dtBasicInfo = new DataTable();
                dtBasicInfo.TableName = "dtBasicInfo";
                dtBasicInfo.Columns.Add("LOTID", typeof(string));
                dtBasicInfo.Columns.Add("LOT", typeof(string));
                dtBasicInfo.Columns.Add("PROJECT", typeof(string));
                dtBasicInfo.Columns.Add("VER", typeof(string));
                dtBasicInfo.Columns.Add("UNIT", typeof(string));
                dtBasicInfo.Columns.Add("QTY", typeof(string));
                dtBasicInfo.Columns.Add("DATE", typeof(string));
                dtBasicInfo.Columns.Add("HOLD", typeof(string));


                for (int i = 0; i < dgMove_Detail.GetRowCount(); i++)
                {
                    DataRow drInfo = dtBasicInfo.NewRow();
                    drInfo["LOTID"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID");
                    drInfo["LOT"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID");
                    drInfo["PROJECT"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "PROJECTNAME") + "-" + DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "ELECNAME");
                    drInfo["VER"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "PROD_VER_CODE");
                    drInfo["UNIT"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "UNIT_CODE");
                    drInfo["QTY"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "WIPQTY");

                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "VLD_DATE")) == "")
                    {
                        sVld = null;
                    }
                    else
                    {
                        sVld_date = Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "VLD_DATE"));
                        sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                    }

                    drInfo["DATE"] = sVld;
                    drInfo["HOLD"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "WIPHOLD");

                    dtBasicInfo.Rows.Add(drInfo);
                }

                LGC.GMES.MES.COM001.Report_Print rs = new LGC.GMES.MES.COM001.Report_Print();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "Report_PlantMoveInfo";
                    Parameters[1] = dtPackingCard;
                    Parameters[2] = dtBasicInfo;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(RePrint_Result);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void RePrint_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Print wndPopup = sender as LGC.GMES.MES.COM001.Report_Print;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //전체삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgMoveList.IsReadOnly = false;
                    dgMoveList.RemoveRow(index);
                    dgMoveList.IsReadOnly = true;
                }
            });
        }
        
        //lot id
        private void txtLotID_Out_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sArea = string.Empty;
                string sLotid = string.Empty;
                sLotid = txtLotID_Out.Text.Trim();

                //if (cboAreaFrom.SelectedIndex < 0 || cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
                if (cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
                {
                    Util.Alert("SFU2834");  //From(동) 정보를 선택하세요.
                    return;
                }
                else
                {
                    sArea = cboAreaFrom.SelectedValue.ToString();
                }

                if (txtLotID_Out.Text.ToString() == null && txtLotID_Out.Text.ToString() == "")
                {
                    Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                    return;
                }

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("COM_TYPE_CODE", typeof(String));
                RQSTDT2.Columns.Add("COM_CODE", typeof(String));
                RQSTDT2.Columns.Add("USE_FLAG", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                dr2["COM_CODE"] = "SHOPID_CHK_NJ";
                dr2["USE_FLAG"] = "Y";

                RQSTDT2.Rows.Add(dr2);
                DataTable ResultArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID_CHK", "RQSTDT", "RSLTDT", RQSTDT2);

                if (ResultArea.Rows.Count > 0)
                {
                    for(int i = 0; i < ResultArea.Rows.Count; i++)
                    {
                        if (LoginInfo.CFG_AREA_ID == ResultArea.Rows[i]["AREAID"].ToString())
                        //if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                        {
                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("LOTID", typeof(String));
                            RQSTDT1.Columns.Add("WIPSTAT", typeof(String));
                            RQSTDT1.Columns.Add("PROCID", typeof(String));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["LOTID"] = sLotid;
                            dr1["WIPSTAT"] = Wip_State.WAIT;
                            dr1["PROCID"] = Process.ELEC_STORAGE;

                            RQSTDT1.Rows.Add(dr1);
                            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                            if (Result.Rows.Count == 0)
                            {
                                //PLANT 정보가 없습니다.[TB_SFC_FP_DETL_PLAN.SHOPID 정보 누락 / PI팀에 데이터 확인 요청]"
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3137"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                            if (cboShopFrom.SelectedValue.ToString() != Result.Rows[0]["SHOPID"].ToString())
                            {
                                //선택한 PLANT 와 SCAN 한 LOTID 의 PLANT 정보가 일치하지 않습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3138"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }
                        }
                    }
                }

                // 2020-09-16
                // 품질검사 INTERLOCK PLNAT 체크
                DataTable RQSTDT4 = new DataTable();
                RQSTDT4.TableName = "RQSTDT";
                RQSTDT4.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT4.Columns.Add("CMCODE", typeof(String));
                RQSTDT4.Columns.Add("LANGID", typeof(String));

                DataRow dr4 = RQSTDT4.NewRow();
                dr4["CMCDTYPE"] = "BLOCK_TYPE_CODE_PLANT";
                dr4["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                dr4["LANGID"] = LoginInfo.LANGID;

                RQSTDT4.Rows.Add(dr4);
                DataTable dtBlock = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT4);

                bool chk = false;
                bool blockChk = false;

                if (dtBlock.Rows.Count > 0)
                {
                    if (LoginInfo.CFG_SHOP_ID == dtBlock.Rows[0]["CMCODE"].ToString())
                    {
                        chk = true;
                        blockChk = true;
                    }
                }

                if (!blockChk)
                {

                    // 2017-05-11 
                    // NJ 요청사항 적용
                    // 품질결과 검사 체크
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                    RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["CMCDTYPE"] = "JUDGE_HIST";
                    dr3["CMCDIUSE"] = "Y";

                    RQSTDT3.Rows.Add(dr3);
                    DataTable dtJudge = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);

                    //bool chk = false;

                    for (int i = 0; i < dtJudge.Rows.Count; i++)
                    {
                        if (sArea == dtJudge.Rows[i]["CMCODE"].ToString())
                        {
                            chk = true;
                        }
                    }
                }
                
                if (chk)
                {
                    DataSet indataSet = new DataSet();

                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("LOTID", typeof(string));
                    inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                    inData.Columns.Add("BR_TYPE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["LOTID"] = sLotid;
                    row["BLOCK_TYPE_CODE"] = "MOVE_SHOP";       //NEW HOLD Type 변수
                    row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                    //신규 HOLD 적용을 위해 변경 작업
                    new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA" , (result, Exception) =>
                    {
                        if (Exception != null)
                        {
                            Util.MessageException(Exception);
                        }
                        else
                        {
                            ScanProcess(sLotid, sArea);
                        }
                    }, indataSet);
                }
                else
                {
                    ScanProcess(sLotid, sArea);
                }
            }
        }

        private void txtSKIDID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanValidation(txtSKIDID.Text.Trim(), "SKID");
            }
        }

        private void txtCARRIERID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!RepLotUseArea)
                    ScanCarrierID(txtCARRIERID.Text.Trim());
                else
                {
                    errChk = false;
                    errFlag = false;
                    ScanRepLotID(txtCARRIERID.Text.Trim());
                }
            }
        }

        private void ScanRepLotID(string sLotid)
        {
            
            try
            {
                if (string.IsNullOrEmpty(sLotid))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }
                
                //RFID로 LOTID 가져옴
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = sLotid;

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_REP_LOTID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                            txtCARRIERID.Focus();
                            txtCARRIERID.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    for (int i = 0; i < dtLot.Rows.Count; i++)
                    {
                      
                        ScanLotID(dtLot.Rows[i]["LOTID"].ToString());
                    }

                    
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

        private void ScanLotID(string sLotid)
        {
            try
            {
                string sArea = string.Empty;
                
                if (cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
                {
                    Util.Alert("SFU2834");  //From(동) 정보를 선택하세요.
                    return;
                }
                else
                {
                    sArea = cboAreaFrom.SelectedValue.ToString();
                }

                if (txtLotID_Out.Text.ToString() == null && txtLotID_Out.Text.ToString() == "")
                {
                    Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                    return;
                }

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("COM_TYPE_CODE", typeof(String));
                RQSTDT2.Columns.Add("COM_CODE", typeof(String));
                RQSTDT2.Columns.Add("USE_FLAG", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                dr2["COM_CODE"] = "SHOPID_CHK_NJ";
                dr2["USE_FLAG"] = "Y";

                RQSTDT2.Rows.Add(dr2);
                DataTable ResultArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID_CHK", "RQSTDT", "RSLTDT", RQSTDT2);

                if (ResultArea.Rows.Count > 0)
                {
                    for (int i = 0; i < ResultArea.Rows.Count; i++)
                    {
                        if (LoginInfo.CFG_AREA_ID == ResultArea.Rows[i]["AREAID"].ToString())
                        {
                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("LOTID", typeof(String));
                            RQSTDT1.Columns.Add("WIPSTAT", typeof(String));
                            RQSTDT1.Columns.Add("PROCID", typeof(String));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["LOTID"] = sLotid;
                            dr1["WIPSTAT"] = Wip_State.WAIT;
                            dr1["PROCID"] = Process.ELEC_STORAGE;

                            RQSTDT1.Rows.Add(dr1);
                            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                            if (Result.Rows.Count == 0)
                            {
                                //PLANT 정보가 없습니다.[TB_SFC_FP_DETL_PLAN.SHOPID 정보 누락 / PI팀에 데이터 확인 요청]"
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3137"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                            if (cboShopFrom.SelectedValue.ToString() != Result.Rows[0]["SHOPID"].ToString())
                            {
                                //선택한 PLANT 와 SCAN 한 LOTID 의 PLANT 정보가 일치하지 않습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3138"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }
                        }
                    }
                }

                // 2020-09-16
                // 품질검사 INTERLOCK PLNAT 체크
                DataTable RQSTDT4 = new DataTable();
                RQSTDT4.TableName = "RQSTDT";
                RQSTDT4.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT4.Columns.Add("CMCODE", typeof(String));
                RQSTDT4.Columns.Add("LANGID", typeof(String));

                DataRow dr4 = RQSTDT4.NewRow();
                dr4["CMCDTYPE"] = "BLOCK_TYPE_CODE_PLANT";
                dr4["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                dr4["LANGID"] = LoginInfo.LANGID;

                RQSTDT4.Rows.Add(dr4);
                DataTable dtBlock = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT4);

                bool chk = false;
                bool blockChk = false;
                if (dtBlock.Rows.Count > 0)
                {
                    if (LoginInfo.CFG_SHOP_ID == dtBlock.Rows[0]["CMCODE"].ToString())
                    {
                        chk = true;
                        blockChk = true;
                    }
                }

                if (!blockChk)
                {
                    // 2017-05-11 
                    // NJ 요청사항 적용
                    // 품질결과 검사 체크
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                    RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["CMCDTYPE"] = "JUDGE_HIST";
                    dr3["CMCDIUSE"] = "Y";

                    RQSTDT3.Rows.Add(dr3);
                    DataTable dtJudge = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);

                    //bool chk = false;

                    for (int i = 0; i < dtJudge.Rows.Count; i++)
                    {
                        if (sArea == dtJudge.Rows[i]["CMCODE"].ToString())
                        {
                            chk = true;
                        }
                    }
                }

                if (chk)
                {
                    DataSet indataSet = new DataSet();

                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("LOTID", typeof(string));
                    inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                    inData.Columns.Add("BR_TYPE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["LOTID"] = sLotid;
                    row["BLOCK_TYPE_CODE"] = "MOVE_SHOP";       //NEW HOLD Type 변수
                    row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                    //신규 HOLD 적용을 위해 변경 작업
                    new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                    {
                        if (Exception != null)
                        {            
                            Util.MessageException(Exception);
                            errChk = true;
                            
                        }
                        else
                        {
                            errFlag = false; 
                            ScanProcess(sLotid, sArea);
                            if (errFlag)
                                errChk = true;                          
                        }

                        if (RepLotUseArea && errChk)
                            Util.gridClear(dgMoveList);
                    }, indataSet);
                }
                else
                {
                    ScanProcess(sLotid, sArea);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void ScanCarrierID(string sLotid)
        {
            try
            {
                if (string.IsNullOrEmpty(sLotid))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                //RFID로 LOTID 가져옴
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sLotid;

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                            txtCARRIERID.Focus();
                            txtCARRIERID.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    sLotid = dtLot.Rows[0]["LOTID"].ToString();
                    ScanLotID(sLotid);

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

        private void ScanValidation(string lotid, string type = null)
        {
            string sArea = string.Empty;
            string sLotid = string.Empty;
            sLotid = lotid;

            //if (cboAreaFrom.SelectedIndex < 0 || cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
            if (cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
            {
                Util.Alert("SFU2834");  //From(동) 정보를 선택하세요.
                return;
            }
            else
            {
                sArea = cboAreaFrom.SelectedValue.ToString();
            }

            if (lotid == null || string.IsNullOrWhiteSpace(lotid))
            {
                Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                return;
            }

            DataTable RQSTDT2 = new DataTable();
            RQSTDT2.TableName = "RQSTDT";
            RQSTDT2.Columns.Add("COM_TYPE_CODE", typeof(String));
            RQSTDT2.Columns.Add("COM_CODE", typeof(String));
            RQSTDT2.Columns.Add("USE_FLAG", typeof(String));

            DataRow dr2 = RQSTDT2.NewRow();
            dr2["COM_TYPE_CODE"] = "COM_TYPE_CODE";
            dr2["COM_CODE"] = "SHOPID_CHK_NJ";
            dr2["USE_FLAG"] = "Y";

            RQSTDT2.Rows.Add(dr2);
            DataTable ResultArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID_CHK", "RQSTDT", "RSLTDT", RQSTDT2);

            if (ResultArea.Rows.Count > 0)
            {
                for(int i = 0; i < ResultArea.Rows.Count; i++)
                {
                    if (LoginInfo.CFG_AREA_ID == ResultArea.Rows[i]["AREAID"].ToString())
                    //if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                    {
                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        RQSTDT1.Columns.Add("CSTID", typeof(String));
                        RQSTDT1.Columns.Add("WIPSTAT", typeof(String));
                        RQSTDT1.Columns.Add("PROCID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = string.IsNullOrEmpty(type) == false && type.Equals("SKID") ? null : lotid;
                        dr1["CSTID"] = string.IsNullOrEmpty(type) == false && type.Equals("SKID") ? lotid : null;
                        dr1["WIPSTAT"] = Wip_State.WAIT;
                        dr1["PROCID"] = Process.ELEC_STORAGE;

                        RQSTDT1.Rows.Add(dr1);
                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                        if (Result.Rows.Count == 0)
                        {
                            //PLANT 정보가 없습니다.[TB_SFC_FP_DETL_PLAN.SHOPID 정보 누락 / PI팀에 데이터 확인 요청]"
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3137"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Init();
                                }
                            });
                            return;
                        }

                        if (cboShopFrom.SelectedValue.ToString() != Result.Rows[0]["SHOPID"].ToString())
                        {
                            //선택한 PLANT 와 SCAN 한 LOTID 의 PLANT 정보가 일치하지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3138"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Init();
                                }
                            });
                            return;
                        }
                    }
                }                
            }

            // 2020-09-16
            // 품질검사 INTERLOCK PLNAT 체크
            DataTable RQSTDT4 = new DataTable();
            RQSTDT4.TableName = "RQSTDT";
            RQSTDT4.Columns.Add("CMCDTYPE", typeof(String));
            RQSTDT4.Columns.Add("CMCODE", typeof(String));
            RQSTDT4.Columns.Add("LANGID", typeof(String));

            DataRow dr4 = RQSTDT4.NewRow();
            dr4["CMCDTYPE"] = "BLOCK_TYPE_CODE_PLANT";
            dr4["CMCODE"] = LoginInfo.CFG_SHOP_ID;
            dr4["LANGID"] = LoginInfo.LANGID;

            RQSTDT4.Rows.Add(dr4);
            DataTable dtBlock = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT4);

            bool chk = false;
            bool blockChk = false;

            if (dtBlock.Rows.Count > 0)
            {
                if (LoginInfo.CFG_SHOP_ID == dtBlock.Rows[0]["CMCODE"].ToString())
                {
                    chk = true;
                    blockChk = true;
                }
            }

            if (!blockChk)
            {
                // 2017-05-11 
                // NJ 요청사항 적용
                // 품질결과 검사 체크
                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["CMCDTYPE"] = "JUDGE_HIST";
                dr3["CMCDIUSE"] = "Y";

                RQSTDT3.Rows.Add(dr3);
                DataTable dtJudge = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);

                //bool chk = false;

                for (int i = 0; i < dtJudge.Rows.Count; i++)
                {
                    if (sArea == dtJudge.Rows[i]["CMCODE"].ToString())
                    {
                        chk = true;
                    }
                }
            }

            if (chk)
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                inData.Columns.Add("BR_TYPE", typeof(string));

                DataRow row = inData.NewRow();
                row["LOTID"] = sLotid;
                row["BLOCK_TYPE_CODE"] = "MOVE_SHOP";       //NEW HOLD Type 변수
                row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                indataSet.Tables["INDATA"].Rows.Add(row);

                //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                //신규 HOLD 적용을 위해 변경 작업
                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);
                    }
                    else
                    {
                        ScanProcess(sLotid, sArea, "SKID");
                    }
                }, indataSet);
            }
            else
            {
                ScanProcess(sLotid, sArea, "SKID");
            }
        }

        private bool ScanProcess(string sLotid, string sArea, string sType = null)
        {
            try
            {

                errFlag = true;
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = string.IsNullOrEmpty(sType) == false && sType.Equals("SKID") ? null : sLotid;
                dr["CSTID"] = string.IsNullOrEmpty(sType) == false && sType.Equals("SKID") ? sLotid : null;
                dr["AREAID"] = sArea;

                RQSTDT.Rows.Add(dr);
                //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    //Lot[%1]의 재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4950", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                        }
                    });
                    return false;
                }
                else
                {
                    if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        //재공 상태가 이동가능한 상태가 아닙니다.
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1869"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //LOT[%1]의 재공 상태가 이동가능한 상태가 아닙니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4953", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Init();
                            }
                        });
                        return false; 
                    }
                    /* 20171117 HOLD 로직 제외
                                        // HOLD 체크 동 LOGIC 추가
                                        if (GetHoldCheckArea() == true && SearchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                                        {
                                            //HOLD 된 LOT ID 입니다.
                                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1340"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                            //LOT[%1]은 HOLD 된 LOT ID 입니다.
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU13401"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                            {
                                                if (result == MessageBoxResult.OK)
                                                {
                                                    Init();
                                                }
                                            });
                                            return;
                                        }


                    */

                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("AREAID", typeof(String));
                    RQSTDT1.Columns.Add("COM_TYPE_CODE", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["AREAID"] = sArea;
                    dr1["COM_TYPE_CODE"] = "AUTO_STOCK_OUT";

                    RQSTDT1.Rows.Add(dr1);
                    DataTable SearchResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AUTO_STOCK_OUT", "RQSTDT", "RSLTDT", RQSTDT1);

                    if (SearchResult1.Rows.Count == 0)
                    {
                        if (SearchResult.Rows[0]["PROCID"].ToString() != Process.ELEC_STORAGE)
                        {
                            //전극창고에 존재하지 않습니다.
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2947"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //LOT[%1]은 전극창고에 존재하지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU29471", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Init();
                                }
                            });
                            return false;
                        }


                        if (SearchResult.Rows[0]["WH_ID"].ToString() != "")
                        {
                            //창고에서 출고되지 않았습니다.
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2963"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //LOT[%1]은 창고에서 출고되지 않았습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4954", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {

                                    Init();
                                }
                            });
                            return false;

                        }
                    }

                    //if (SearchResult.Rows[0]["RACK_ID"].ToString() != "" && SearchResult.Rows[0]["RACK_ID"].ToString() != null)
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("창고에서 출고되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}

                    #region # 특별관리 Lot - 인계가능 라인 체크
                    if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]),"Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                    {
                        string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                        string sLine = Util.GetCondition(cboEquipmentSegment);

                        if (!SpclLine.Contains(sLine))
                        {
                            Init();
                            Util.MessageInfo("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]),  SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                            return false;
                        }
                    }
                    #endregion

                    for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, string.IsNullOrEmpty(sType) == false && sType.Equals("SKID") ? "CSTID" : "LOTID").ToString() == sLotid)
                        {
                            //동일한 LOT이 스캔되었습니다.
                            //Util.MessageInfo("SFU1504", (result) =>
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1504"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //LOT[%1]과 동일한 LOT이 스캔되었습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU15041", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {                                    
                                    Init();
                                }
                            });
                            return false;
                        }

                        //시생산 LOT인 경우 다른 LOTTYPE과 섞이면 안됨
                        if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() != SearchResult.Rows[0]["LOTTYPE"].ToString())
                        {
                            if ((DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() == "X") || (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X"))
                            {
                                //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5149"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return false;
                            }
                        }

                        #region # 특별관리 Lot - 혼입 체크 
                        if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) != Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]))
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) == "Y") || (Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]) == "Y"))
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8214"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return false;
                            }
                        }
                        #endregion
                    }

                    if (dgMoveList.GetRowCount() == 0)
                    {
                        //dgMoveList.ItemsSource = DataTableConverter.Convert(SearchResult);D:\SecureDevFolder\TFS\GMES\01. UI\LGC.GMES.MES.COM001\COM001_003.xaml
                        Util.GridSetData(dgMoveList, SearchResult, FrameOperation);
                        DataGrid_Summary();
                        Init();
                    }
                    else
                    {
                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                            Init();
                            return false;
                        }

                        if (!DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "MKT_TYPE_CODE").ToString().Equals(SearchResult.Rows[0]["MKT_TYPE_CODE"].ToString()))
                        {
                            //내수용과 수출용은 같이 포장할 수 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4454"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Init();
                                }
                            });
                            return false;
                        }

                        if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                            dtSource.Merge(SearchResult);

                            Util.gridClear(dgMoveList);
                            //dgMoveList.ItemsSource = DataTableConverter.Convert(dtSource);
                            Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                            DataGrid_Summary();
                            Init();
                        }
                        else
                        {
                            //제품ID가 같지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Init();
                                }
                            });
                            return false;
                        }

                    }
                    errFlag = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgMoveList.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgMoveList.Columns["WIPQTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgMoveList.Columns["WIPQTY2"], dac);
        }

        private void Init()
        {
            //txtLotID_Out.SelectAll();
            txtLotID_Out.Text = string.Empty;
            txtLotID_Out.Focus();

            txtSKIDID.Text = string.Empty;
            txtCARRIERID.Text = string.Empty;
           
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgMoveList);
            txtLotID_Out.Text = string.Empty;
            txtSKIDID.Text = string.Empty;
            txtCARRIERID.Text = string.Empty;

            txtLotID_Out.Focus();
            chkPrint.IsChecked = false;
        }

        #endregion

        #region [인계이력]
        //인계이력조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        //인계취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgMove_Master.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                
                string sMoveOrderID = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgMove_Master, "CHK");

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
                #region # (C20200312-000003)자동물류의 경우 취소 불가
                if (IsReturnImpossibleUser(Util.NVC(drChk[0]["MOVE_USERID"])))
                {
                    Util.AlertInfo("SFU8173");  //자동물류는 취소할 수 없습니다.
                    return;
                }
                #endregion
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

                for (int i = 0; i < dgMove_Detail.GetRowCount(); i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK")).Equals("True"))
                    //{
                    if (DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                    {
                        DataRow row2 = inLot.NewRow();
                        row2["LOTID"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID").ToString();

                        indataSet.Tables["INLOT"].Rows.Add(row2);
                    }
                    //}
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_PACKLOT_SHOP", "INDATA,INLOT", null, indataSet);

                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                Util.gridClear(dgMove_Detail);
                Util.gridClear(dgMove_Master);

                Search_data();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //이력조회
        private void Search_data()
        {
            try
            {
                Util.gridClear(dgMove_Detail);
                Util.gridClear(dgMove_Master);

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sShop = Util.GetCondition(cboFromShop, bAllNull: true);
                string sFromShop = Util.GetCondition(cboFromShop2, bAllNull: true);
                string sArea = Util.GetCondition(cboFromArea_Plant, bAllNull: true);
                string sEqsg = Util.GetCondition(cboFromEquipmentSegment, bAllNull: true);

                string sFromArea = Util.GetCondition(cboFromArea_Plant2, bAllNull: true);        // [E20231101-000795] MES UI improvement from pain point
                string sFromEqsg = Util.GetCondition(cboFromEquipmentSegment2, bAllNull: true);  // [E20231101-000795] MES UI improvement from pain point


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_SHOPID", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));   // [E20231101-000795] MES UI improvement from pain point
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));   // [E20231101-000795] MES UI improvement from pain point
                RQSTDT.Columns.Add("TO_SHOPID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                //dr["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["FROM_SHOPID"] = sFromShop;
                dr["FROM_AREAID"] = sFromArea;  // [E20231101-000795] MES UI improvement from pain point
                dr["FROM_EQSGID"] = sFromEqsg;  // [E20231101-000795] MES UI improvement from pain point

                dr["TO_SHOPID"] = sShop;
                dr["TO_AREAID"] = sArea;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat,bAllNull:true);
                dr["PRODID"] = txtProd_ID.Text == "" ? null : txtProd_ID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgMove_Master);
                Util.GridSetData(dgMove_Master, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgMove_MasterChoice_Checked(object sender, RoutedEventArgs e)
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
                //    dgMove_Master.BeginEdit();
                //    dgMove_Master.ItemsSource = DataTableConverter.Convert(dt);
                //    dgMove_Master.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i+2)
                        DataTableConverter.SetValue(dg.Rows[i+2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i+2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgMove_Master.SelectedIndex = idx;

                SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);

            }
            //else
            //{
            //    Util.gridClear(dgMove_Detail);
            //}
        }

        #endregion

        #region [반품조회]

        //반품조회
        private void btnReturnHistory_Click(object sender, RoutedEventArgs e)
        {
            SearchReturn();
        }

        //반품확정
        private void btnReturnConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReturn_Master.GetRowCount() ==0 || dgReturn_Detail.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //반품확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2869"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string sMoveOrderID = string.Empty;

                        DataRow[] drChk = Util.gridGetChecked(ref dgReturn_Master, "CHK");

                        if (!drChk[0]["MOVE_ORD_STAT_CODE"].ToString().Equals("MOVING"))
                        {
                            Util.AlertInfo("SFU2048"); //확정할 수 없는 상태입니다.
                            return;
                        }

                        sMoveOrderID = drChk[0]["MOVE_ORD_ID"].ToString();

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("EQSGID", typeof(string));
                        inData.Columns.Add("PCSGID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["MOVE_ORD_ID"] = sMoveOrderID;
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                        row["PCSGID"] = "E";
                        row["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgReturn_Detail.GetRowCount(); i++)
                        {
                            //if (Util.NVC(DataTableConverter.GetValue(dgReturn_Detail.Rows[i].DataItem, "CHK")).Equals("True"))
                            //{
                            if (DataTableConverter.GetValue(dgReturn_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["LOTID"] = DataTableConverter.GetValue(dgReturn_Detail.Rows[i].DataItem, "LOTID").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                            //}
                        }

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT", "INDATA,INLOT", null, indataSet);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT", "INDATA,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.gridClear(dgMove_Master);
                                Util.gridClear(dgReturn_Detail);
                                SearchReturn();

                                Util.AlertInfo("SFU1557");  //반품처리 되었습니다.
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


        private void SearchReturn()
        {
            try
            {
                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;
                string sMoveStateCode = string.Empty;


                sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);

                sMoveType = "RETURN_SHOP";
                sMoveStateCode = "MOVING";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));                
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = sMoveType;
                dr["MOVE_ORD_STAT_CODE"] = sMoveStateCode;
                dr["PRODID"] = txtProd_ID2.Text == "" ? null : txtProd_ID2.Text;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_CSTID_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                //Util.gridClear(dgReturn_Master);
                //dgReturn_Master.ItemsSource = DataTableConverter.Convert(SearchResult);

                Util.GridSetData(dgReturn_Master, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
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

                SearchMoveLOTList(DataTableConverter.GetValue(dgReturn_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgReturn_Detail);
            }
        }

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    Search_data();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }


        #endregion

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
                {
                    SearchReturn();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            Search_MoveInfo();
        }

        private void txtProd_ID4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID4.Text != "")
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

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom4.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo4.SelectedDateTime);
                string sShop = Util.GetCondition(cboFromShop4, bAllNull: true);
                string sFromShop = Util.GetCondition(cboFromShop5, bAllNull: true);
                string sArea = Util.GetCondition(cboFromArea_Plant4, bAllNull: true);
                string sEqsg = Util.GetCondition(cboFromEquipmentSegment4, bAllNull: true);

                string sFromArea = Util.GetCondition(cboFromArea_Plant5, bAllNull: true);           // [E20231101-000795] MES UI improvement from pain point
                string sFromEqsg = Util.GetCondition(cboFromEquipmentSegment5, bAllNull: true);     // [E20231101-000795] MES UI improvement from pain point

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_SHOPID", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));   // [E20231101-000795] MES UI improvement from pain point
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));   // [E20231101-000795] MES UI improvement from pain point
                RQSTDT.Columns.Add("TO_SHOPID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                dr["TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;
                dr["FROM_SHOPID"] = sFromShop;
                dr["FROM_AREAID"] = sFromArea;  // [E20231101-000795] MES UI improvement from pain point
                dr["FROM_EQSGID"] = sFromEqsg;  // [E20231101-000795] MES UI improvement from pain point
                dr["TO_SHOPID"] = sShop;
                dr["TO_AREAID"] = sArea;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_SHOP";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat4, bAllNull: true);
                dr["PRODID"] = txtProd_ID4.Text == "" ? null : txtProd_ID4.Text;
                dr["LOTID"] = txtLotID2.Text == "" ? null : txtLotID2.Text;

                string sLotType = Util.GetCondition(cboLotType);
                dr["LOTTYPE"] = string.IsNullOrWhiteSpace(sLotType) ? null : sLotType;

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

                            Util.GridSetData(dgMove_Info, bizResult, FrameOperation);

                            string[] sColumnName = new string[] { "MOVE_ORD_ID", "CSTID" };
                            //_Util.SetDataGridMergeExtensionCol(dgMove_Info, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            _Util.SetDataGridMergeExtensionCol(dgMove_Info, sColumnName, DataGridMergeMode.VERTICAL);

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

        private void btnReturnHistory6_Click(object sender, RoutedEventArgs e)
        {
            Search_ReturnInfo();
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

        private void Search_ReturnInfo()
        {
            try
            {
                Util.gridClear(dgReturn_Info);

                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;
                string sMoveStateCode = string.Empty;

                sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom6.SelectedDateTime);
                sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo6.SelectedDateTime);

                sMoveType = "RETURN_SHOP";
                sMoveStateCode = "CLOSE_MOVE";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = txtLotID6.Text != "" ? null : sStart_date;
                dr["TO_DATE"] = txtLotID6.Text != "" ? null : sEnd_date;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = sMoveType;
                dr["MOVE_ORD_STAT_CODE"] = sMoveStateCode;
                dr["PRODID"] = txtProd_ID6.Text == "" ? null : txtProd_ID6.Text;
                dr["LOTID"] = txtLotID6.Text == "" ? null : txtLotID6.Text;

                string sLotType = Util.GetCondition(cboLotType2);
                dr["LOTTYPE"] = string.IsNullOrWhiteSpace(sLotType) ? null : sLotType;

                RQSTDT.Rows.Add(dr);

                try
                {
                    ShowLoadingIndicator2();

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

                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            HiddenLoadingIndicator2();
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

        public void HiddenLoadingIndicator2()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator2()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
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

        private void txtSkid_ID7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtSkid_ID7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    SearchReturnConfrim(txtSkid_ID7.Text.Trim());
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
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

        private void Search_dgReturnConfrim_Info(string Lotid)
        {
            try
            {
                string sMoveOrderID = string.Empty;
                DataSet indataSet = new DataSet();

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                if (txtSkid_ID7.Text.ToString() == "")
                {
                    DataRow row2 = inLot.NewRow();
                    row2["LOTID"] = Lotid;
                    indataSet.Tables["INLOT"].Rows.Add(row2);
                }
                else
                {
                    // SKID Grid Data 조회
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("SKID_ID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SKID_ID"] = txtSkid_ID7.Text.ToString().Trim();

                    RQSTDT.Rows.Add(dr);

                    DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                    if (Result.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                        return;
                    }
                    else
                    {
                        Lotid = Result.Rows[0]["LOTID"].ToString();
                        for (int i = 0; i < Result.Rows.Count; i++)
                        {
                            DataRow row2 = inLot.NewRow();
                            row2["LOTID"] = Result.Rows[i]["LOTID"].ToString();
                            indataSet.Tables["INLOT"].Rows.Add(row2);
                        }
                    }
                }

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT2";
                RQSTDT2.Columns.Add("LANGID", typeof(String));
                RQSTDT2.Columns.Add("LOTID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LANGID"] = LoginInfo.LANGID;
                dr2["LOTID"] = Lotid;

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

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PCSGID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["MOVE_ORD_ID"] = sMoveOrderID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                row["PCSGID"] = "E";
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(row);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT", "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgReturnConfrim_Info);
                        //SearchReturnConfrim(Lotid);

                        Util.AlertInfo("SFU1557");  //반품처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchReturnConfrim(string Lotid)
        {
            try
            {
                string sMoveOrderID = string.Empty;
                DataSet indataSet = new DataSet();

                string sStart_date = string.Empty;
                string sEnd_date = string.Empty;
                string sMoveType = string.Empty;

                sStart_date = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                sEnd_date = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                sMoveType = "RETURN_SHOP";

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LANGID", typeof(string));
                //RQSTDT2.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT2.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT2.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT2.Columns.Add("LOTID", typeof(String));
                RQSTDT2.Columns.Add("CSTID", typeof(String));
                RQSTDT2.Columns.Add("MOVE_STAT_CODE", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();

                dr2["LANGID"] = LoginInfo.LANGID;
                //dr2["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr2["MOVE_TYPE_CODE"] = sMoveType;
                dr2["MOVE_ORD_STAT_CODE"] = "MOVING";

                /*
                if (txtSkid_ID7.Text.ToString() == "")
                    dr2["LOTID"] = Lotid == "" ? null : Lotid;
                if (txtLotID7.Text.ToString() == "")
                    dr2["CSTID"] = Lotid == "" ? null : Lotid;
                */

                if (!string.IsNullOrEmpty(txtSkid_ID7.Text.Trim()))
                    dr2["CSTID"] = Lotid == "" ? null : Lotid;
                else if (!string.IsNullOrEmpty(txtLotID7.Text.Trim()))
                    dr2["LOTID"] = Lotid == "" ? null : Lotid;
                else if (!string.IsNullOrEmpty(txtCARRIERID_01.Text.Trim()))
                    dr2["LOTID"] = Lotid == "" ? null : Lotid;
                

                dr2["MOVE_STAT_CODE"] = "MOVING";
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
                    for (int i = 0; i < Result2.Rows.Count ; i++)
                    {
                        dgReturnConfrim_Info.BeginNewRow();
                        dgReturnConfrim_Info.EndNewRow(true);
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_ID", Result2.Rows[i]["MOVE_ORD_ID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_STAT_NAME", Result2.Rows[i]["MOVE_ORD_STAT_NAME"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "PRJT_NAME", Result2.Rows[i]["PRJT_NAME"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "PRODID", Result2.Rows[i]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_QTY", Result2.Rows[i]["MOVE_ORD_QTY"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_CMPL_QTY", Util.NVC_DecimalStr(Result2.Rows[i]["MOVE_CMPL_QTY"].ToString().Equals("") ? "0": Result2.Rows[i]["MOVE_CMPL_QTY"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_CNCL_QTY", Util.NVC_DecimalStr(Result2.Rows[i]["MOVE_CNCL_QTY"].ToString().Equals("") ? "0" : Result2.Rows[i]["MOVE_CNCL_QTY"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "FROM_AREAID", Result2.Rows[i]["FROM_AREAID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "FROM_EQSGID", Result2.Rows[i]["FROM_EQSGID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_USERID", Result2.Rows[i]["MOVE_USERID"].ToString());
                        //DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_STRT_DTTM", DateTime.Parse(Result2.Rows[i]["MOVE_STRT_DTTM"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "TO_AREAID", Result2.Rows[i]["TO_AREAID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "TO_EQSGID", Result2.Rows[i]["TO_EQSGID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "RCPT_USERID", Result2.Rows[i]["RCPT_USERID"].ToString());
                        //DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_END_DTTM", DateTime.Parse(Result2.Rows[i]["MOVE_END_DTTM"].ToString()));
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "NOTE", Result2.Rows[i]["NOTE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "MOVE_ORD_STAT_CODE", Result2.Rows[i]["MOVE_ORD_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_STAT_CODE", Result2.Rows[i]["ERP_STAT_CODE"].ToString());
                        //DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_INS", Result2.Rows[i]["ERP_INS"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_ERR_CODE", Result2.Rows[i]["ERP_ERR_CODE"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "ERP_ERR_CAUSE_CNTT", Result2.Rows[i]["ERP_ERR_CAUSE_CNTT"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "LOTID", Result2.Rows[i]["LOTID"].ToString());
                        DataTableConverter.SetValue(dgReturnConfrim_Info.CurrentRow.DataItem, "CSTID", Result2.Rows[i]["CSTID"].ToString());
                    }
                    dgReturnConfrim_Info.IsReadOnly = true;
                    dgReturnConfrim_Info.Refresh(true);
                    dgReturnConfrim_Info_Checked(null, null);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReturnConfrim7_Click(object sender, RoutedEventArgs e)
        {
            //반품확정
            int chk = 0;
            if (dgReturnConfrim_Info.Rows.Count == 0)
            {
                Util.Alert("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                return;
            }
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
                            row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                            row["PCSGID"] = "E";
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT_MASTER", "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgReturnConfrim_Info);
                        Util.AlertInfo("SFU1557");  //반품처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
}

        private void btnRefresh7_Click(object sender, RoutedEventArgs e)
        {
            //초기화
            Util.gridClear(dgReturnConfrim_Info);
            txtLotID7.Text = null;
            txtSkid_ID7.Text = null;
            txtLotID7.Focus();
        }

        private void txtLotID7_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSkid_ID7.Text = null;
        }

        private void txtSkid_ID7_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotID7.Text = null;
        }

        private void txtCARRIERID_01_GotFocus(object sender, RoutedEventArgs e)
        {
            txtCARRIERID.SelectAll();
        }

        private void txtCARRIERID_01_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchReturnCarrierID(txtCARRIERID_01.Text);

                txtCARRIERID_01.Focus();
                txtCARRIERID_01.SelectAll();
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

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID_MOVE", "RQSTDT", "RSLTDT", dt);

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
                    loadingIndicator.Visibility = Visibility.Visible;
                    SearchReturnConfrim(dtLot.Rows[0]["LOTID"].ToString()); 
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        private bool GetHoldCheckArea()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WIP_HOLD_SEND_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(row["CBO_CODE"], LoginInfo.CFG_AREA_ID))
                        return false;
            }
            catch (Exception ex) { }

            return true;
        }

        // // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
        private void PlantMove_Run_OpModeChk()
        {
            try
            {
                // Shop간이동 인계
                string sMove_Ord_ID = string.Empty;
                string sToShop = string.Empty;
                string sToArea = string.Empty;
                string sToEqsg = string.Empty;

                sToShop = Util.GetCondition(cboShop, "SFU1424");    //SHOP을 선택하세요.
                if (sToShop.Equals("")) return;
                sToArea = Util.GetCondition(cboArea_Plant, "SFU1499");  //동을 선택하세요.
                if (sToArea.Equals("")) return;
                sToEqsg = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인을 선택하세요.
                if (sToEqsg.Equals("")) return;

                string sLotid = string.Empty;
                int iOpmodeCnt = 0;

                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {                          
                        sLotid = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString();
                        
                        if (IsLotattrProdLotOperModeCheck(sLotid))
                        {
                            iOpmodeCnt = iOpmodeCnt  + 1;
                        }                         
                    }
                }

                if (iOpmodeCnt > 0)  // OPMODE = 4 가 한건 이상 있을 경우 
                { 
                    COM001_040_PLANT_HANDOVER_YN wndPopup = new COM001_040_PLANT_HANDOVER_YN();
                    wndPopup.FrameOperation = FrameOperation;
                
                    object[] parameters = new object[2];
                    parameters[0] = sLotid;
                
                    C1WindowExtension.SetParameters(wndPopup, parameters);
                
                    wndPopup.Closed += new EventHandler(wndPlantHandoverChk_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
                else
                {
                    PlantMove_Run(); // OPMODE = 4 가 없는 경우 
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
        private void wndPlantHandoverChk_Closed(object sender, EventArgs e)
        {
            string sPLANT_HANDOVER_YN = string.Empty;

            COM001_040_PLANT_HANDOVER_YN window = sender as COM001_040_PLANT_HANDOVER_YN;

            sPLANT_HANDOVER_YN = window.PLANT_HANDOVER_YN;

            if (sPLANT_HANDOVER_YN.Equals("Y"))  // 팝업에서 인계 버튼 클릭 인 경우 
            {
                PlantMove_Run();
                return;
            }
            else
            {
                Util.MessageInfo("SFU1937");  // 취소되었습니다.
                return; 
            }
        }

        //인계처리 실행
        private void PlantMove_Run()
        {
            try
            {
                // Shop간이동 인계
                string sMove_Ord_ID = string.Empty;
                string sToShop = string.Empty;
                string sToArea = string.Empty;
                string sToEqsg = string.Empty;

                sToShop = Util.GetCondition(cboShop, "SFU1424");    //SHOP을 선택하세요.
                if (sToShop.Equals("")) return;
                sToArea = Util.GetCondition(cboArea_Plant, "SFU1499");  //동을 선택하세요.
                if (sToArea.Equals("")) return;
                sToEqsg = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인을 선택하세요.
                if (sToEqsg.Equals("")) return;


                //double dSum = 0;
                //double dSum2 = 0;
                //double dTotal = 0;
                //double dTotal2 = 0;

                //for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                //{
                //    dSum = Convert.ToDouble(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY").ToString());
                //    dSum2 = Convert.ToDouble(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY2").ToString());

                //    dTotal = dTotal + dSum;
                //    dTotal2 = dTotal2 + dSum2;
                //}

                decimal dSum = 0;
                decimal dSum2 = 0;
                decimal dTotal = 0;
                decimal dTotal2 = 0;

                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {
                    dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY")));
                    dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY2")));

                    dTotal = dTotal + dSum;
                    dTotal2 = dTotal2 + dSum2;
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("FROM_SHOPID", typeof(string));
                inData.Columns.Add("FROM_AREAID", typeof(string));
                inData.Columns.Add("FROM_EQSGID", typeof(string));
                inData.Columns.Add("FROM_PROCID", typeof(string));
                inData.Columns.Add("FROM_PCSGID", typeof(string));
                inData.Columns.Add("TO_SHOPID", typeof(string));
                inData.Columns.Add("TO_AREAID", typeof(string));
                inData.Columns.Add("TO_EQSGID", typeof(string));
                inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("TO_SLOC_ID", typeof(string));
                inData.Columns.Add("INTRANSIT_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["PRODID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString();
                row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                row["FROM_EQSGID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "EQSGID").ToString();
                row["FROM_PROCID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString();
                row["FROM_PCSGID"] = "E";
                row["TO_SHOPID"] = sToShop;
                row["TO_AREAID"] = sToArea;
                row["TO_EQSGID"] = sToEqsg;
                row["MOVE_ORD_QTY"] = dTotal;
                row["MOVE_ORD_QTY2"] = dTotal2;
                row["USERID"] = LoginInfo.USERID;
                row["TO_SLOC_ID"] = null;
                row["INTRANSIT_FLAG"] = "Y";

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow row2 = inLot.NewRow();
                        row2["LOTID"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString();

                        indataSet.Tables["INLOT"].Rows.Add(row2);
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_PACKLOT_SHOP_MASTER", "INDATA,INLOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        #region [포장카드발행(2018.06.18)]

                        sMove_Ord_ID = Util.NVC(searchResult.Tables["OUTDATA"].Rows[0]["MOVE_ORD_ID"].ToString());
                        string sVld = string.Empty;
                        string sVld_date = string.Empty;

                        if (chkPrint.IsChecked == true)
                        {
                            DateTime readDttm = DateTime.Now;
                            string sPrintDate = readDttm.Year.ToString() + "-" + readDttm.Month.ToString("00") + "-" + readDttm.Day.ToString("00");

                            // Title DataTable
                            dtPackingCard = new DataTable();
                            dtPackingCard.TableName = "dtPackingCard";
                            dtPackingCard.Columns.Add("TITLE", typeof(string));
                            dtPackingCard.Columns.Add("MOVE_ID", typeof(string));
                            dtPackingCard.Columns.Add("FROM", typeof(string));
                            dtPackingCard.Columns.Add("FROM_AREA", typeof(string));
                            dtPackingCard.Columns.Add("TO", typeof(string));
                            dtPackingCard.Columns.Add("TO_AREA", typeof(string));
                            dtPackingCard.Columns.Add("NOTE", typeof(string));
                            dtPackingCard.Columns.Add("NOTE01", typeof(string));
                            dtPackingCard.Columns.Add("T_01", typeof(string));
                            dtPackingCard.Columns.Add("T_02", typeof(string));
                            dtPackingCard.Columns.Add("T_03", typeof(string));
                            dtPackingCard.Columns.Add("T_04", typeof(string));
                            dtPackingCard.Columns.Add("T_05", typeof(string));
                            dtPackingCard.Columns.Add("DATE", typeof(string));
                            dtPackingCard.Columns.Add("DATE01", typeof(string));


                            DataRow drCrad = dtPackingCard.NewRow();
                            drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("이동ID");
                            drCrad["MOVE_ID"] = sMove_Ord_ID;
                            drCrad["FROM"] = ObjectDic.Instance.GetObjectName("출고처");
                            drCrad["FROM_AREA"] = LoginInfo.CFG_AREA_NAME;
                            drCrad["TO"] = ObjectDic.Instance.GetObjectName("입고처");
                            drCrad["TO_AREA"] = cboArea_Plant.Text;
                            drCrad["NOTE"] = ObjectDic.Instance.GetObjectName("비고");
                            drCrad["NOTE01"] = "";
                            drCrad["T_01"] = ObjectDic.Instance.GetObjectName("프로젝트명");
                            drCrad["T_02"] = ObjectDic.Instance.GetObjectName("버전");
                            drCrad["T_03"] = ObjectDic.Instance.GetObjectName("단위");
                            drCrad["T_04"] = ObjectDic.Instance.GetObjectName("재공");
                            drCrad["T_05"] = ObjectDic.Instance.GetObjectName("유효기간");
                            drCrad["DATE"] = ObjectDic.Instance.GetObjectName("발행일시");
                            drCrad["DATE01"] = sPrintDate;

                            dtPackingCard.Rows.Add(drCrad);

                            // Lot Info DataTable
                            dtBasicInfo = new DataTable();
                            dtBasicInfo.TableName = "dtBasicInfo";
                            dtBasicInfo.Columns.Add("LOTID", typeof(string));
                            dtBasicInfo.Columns.Add("LOT", typeof(string));
                            dtBasicInfo.Columns.Add("PROJECT", typeof(string));
                            dtBasicInfo.Columns.Add("VER", typeof(string));
                            dtBasicInfo.Columns.Add("UNIT", typeof(string));
                            dtBasicInfo.Columns.Add("QTY", typeof(string));
                            dtBasicInfo.Columns.Add("DATE", typeof(string));
                            dtBasicInfo.Columns.Add("HOLD", typeof(string));

                            #region << 멀티라인 출력 >>
                            /*
                            for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                            {
                                DataRow drInfo = dtBasicInfo.NewRow();
                                drInfo["LOTID"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID");
                                drInfo["LOT"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID");
                                drInfo["PROJECT"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "PROJECTNAME") + "-" + DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "ELECNAME");
                                drInfo["VER"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "PROD_VER_CODE");
                                drInfo["UNIT"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "UNIT_CODE");
                                drInfo["QTY"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY");

                                //DateTime sVld_date = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "VLD_DATE")));
                                //string sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);

                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "VLD_DATE")) == "")
                                {
                                    sVld = null;
                                }
                                else
                                {
                                    sVld_date = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "VLD_DATE"));
                                    sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                                }

                                drInfo["DATE"] = sVld;
                                drInfo["HOLD"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPHOLD");

                                dtBasicInfo.Rows.Add(drInfo);
                            }
                            */
                            #endregion << 멀티라인 출력 >>

                            #region << 싱글라인 출력 >>
                            DataTable InData = new DataTable("INDATA");
                            InData.Columns.Add("ORDERID", typeof(string));
                            InData.Rows.Add(sMove_Ord_ID);
                            DataTable dtLotID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_ORD_LOT", "INDATA", "OUTDATA", InData);

                            DataRow[] SingRow = DataTableConverter.Convert(dgMoveList.ItemsSource).Select(string.Format("LOTID = '{0}'", dtLotID.Rows[0][0].ToString()));
                            DataRow drInfo = dtBasicInfo.NewRow();
                            drInfo["LOTID"] = SingRow[0]["LOTID"].ToString();
                            drInfo["LOT"] = SingRow[0]["LOTID"].ToString();
                            drInfo["PROJECT"] = SingRow[0]["PROJECTNAME"].ToString() + "-" + SingRow[0]["ELECNAME"].ToString();
                            drInfo["VER"] = SingRow[0]["PROD_VER_CODE"].ToString();
                            drInfo["UNIT"] = SingRow[0]["UNIT_CODE"].ToString();
                            drInfo["QTY"] = SingRow[0]["WIPQTY"].ToString();

                            if (Util.NVC(SingRow[0]["VLD_DATE"]) == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                sVld_date = Util.NVC(SingRow[0]["VLD_DATE"]);
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            drInfo["DATE"] = sVld;
                            drInfo["HOLD"] = SingRow[0]["WIPHOLD"].ToString();

                            dtBasicInfo.Rows.Add(drInfo);

                            #endregion << 싱글라인 출력 >>

                            LGC.GMES.MES.COM001.Report_Print rs = new LGC.GMES.MES.COM001.Report_Print();
                            rs.FrameOperation = this.FrameOperation;

                            if (rs != null)
                            {
                                // 태그 발행 창 화면에 띄움.
                                object[] Parameters = new object[3];
                                Parameters[0] = "Report_PlantMoveInfo";
                                Parameters[1] = dtPackingCard;
                                Parameters[2] = dtBasicInfo;

                                C1WindowExtension.SetParameters(rs, Parameters);

                                rs.Closed += new EventHandler(Print_Result);
                                this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                            }
                        }
                        else
                        {
                            Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                            Util.gridClear(dgMoveList);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #region  # (C20200312-000003)자동물류의 경우 취소 불가 (MOVE_USERID = 'MCS')
        private bool IsReturnImpossibleUser(string ValueToFind)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLANT_RETURN_IMPOSSIBLE_USER";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(Util.NVC(row["CBO_CODE"]), ValueToFind))
                        return true;
                }
            }
            return false;
        }
        #endregion

        private void txtLotID_Out_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    ShowLoadingIndicator();

                    foreach (string sLotid in sPasteStrings)
                    {
                        DoEvents();

                        string sArea = string.Empty;

                        //if (cboAreaFrom.SelectedIndex < 0 || cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
                        if (cboAreaFrom.SelectedValue.ToString().Trim().Equals(""))
                        {
                            Util.Alert("SFU2834");  //From(동) 정보를 선택하세요.
                            return;
                        }
                        else
                        {
                            sArea = cboAreaFrom.SelectedValue.ToString();
                        }

                        if (txtLotID_Out.Text.ToString() == null && txtLotID_Out.Text.ToString() == "")
                        {
                            Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                            return;
                        }

                        DataTable RQSTDT2 = new DataTable();
                        RQSTDT2.TableName = "RQSTDT";
                        RQSTDT2.Columns.Add("COM_TYPE_CODE", typeof(String));
                        RQSTDT2.Columns.Add("COM_CODE", typeof(String));
                        RQSTDT2.Columns.Add("USE_FLAG", typeof(String));

                        DataRow dr2 = RQSTDT2.NewRow();
                        dr2["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                        dr2["COM_CODE"] = "SHOPID_CHK_NJ";
                        dr2["USE_FLAG"] = "Y";

                        RQSTDT2.Rows.Add(dr2);
                        DataTable ResultArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID_CHK", "RQSTDT", "RSLTDT", RQSTDT2);

                        if (ResultArea.Rows.Count > 0)
                        {
                            for (int i = 0; i < ResultArea.Rows.Count; i++)
                            {
                                if (LoginInfo.CFG_AREA_ID == ResultArea.Rows[i]["AREAID"].ToString())
                                //if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                                {
                                    DataTable RQSTDT1 = new DataTable();
                                    RQSTDT1.TableName = "RQSTDT";
                                    RQSTDT1.Columns.Add("LOTID", typeof(String));
                                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));
                                    RQSTDT1.Columns.Add("PROCID", typeof(String));

                                    DataRow dr1 = RQSTDT1.NewRow();
                                    dr1["LOTID"] = sLotid;
                                    dr1["WIPSTAT"] = Wip_State.WAIT;
                                    dr1["PROCID"] = Process.ELEC_STORAGE;

                                    RQSTDT1.Rows.Add(dr1);
                                    DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MOVE_PLAN_SHOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                                    if (Result.Rows.Count == 0)
                                    {
                                        //PLANT 정보가 없습니다.[TB_SFC_FP_DETL_PLAN.SHOPID 정보 누락 / PI팀에 데이터 확인 요청]"
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3137"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                Init();
                                            }
                                        });
                                        return;
                                    }

                                    if (cboShopFrom.SelectedValue.ToString() != Result.Rows[0]["SHOPID"].ToString())
                                    {
                                        //선택한 PLANT 와 SCAN 한 LOTID 의 PLANT 정보가 일치하지 않습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3138"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                Init();
                                            }
                                        });
                                        return;
                                    }
                                }
                            }
                        }

                        // 2020-09-16
                        // 품질검사 INTERLOCK PLNAT 체크
                        DataTable RQSTDT4 = new DataTable();
                        RQSTDT4.TableName = "RQSTDT";
                        RQSTDT4.Columns.Add("CMCDTYPE", typeof(String));
                        RQSTDT4.Columns.Add("CMCODE", typeof(String));
                        RQSTDT4.Columns.Add("LANGID", typeof(String));

                        DataRow dr4 = RQSTDT4.NewRow();
                        dr4["CMCDTYPE"] = "BLOCK_TYPE_CODE_PLANT";
                        dr4["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                        dr4["LANGID"] = LoginInfo.LANGID;

                        RQSTDT4.Rows.Add(dr4);
                        DataTable dtBlock = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT4);

                        bool chk = false;
                        bool blockChk = false;

                        if (dtBlock.Rows.Count > 0)
                        {
                            if (LoginInfo.CFG_SHOP_ID == dtBlock.Rows[0]["CMCODE"].ToString())
                            {
                                chk = true;
                                blockChk = true;
                            }
                        }

                        if (!blockChk)
                        {

                            // 2017-05-11 
                            // NJ 요청사항 적용
                            // 품질결과 검사 체크
                            DataTable RQSTDT3 = new DataTable();
                            RQSTDT3.TableName = "RQSTDT";
                            RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                            RQSTDT3.Columns.Add("CMCDIUSE", typeof(String));

                            DataRow dr3 = RQSTDT3.NewRow();
                            dr3["CMCDTYPE"] = "JUDGE_HIST";
                            dr3["CMCDIUSE"] = "Y";

                            RQSTDT3.Rows.Add(dr3);
                            DataTable dtJudge = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT3);

                            //bool chk = false;

                            for (int i = 0; i < dtJudge.Rows.Count; i++)
                            {
                                if (sArea == dtJudge.Rows[i]["CMCODE"].ToString())
                                {
                                    chk = true;
                                }
                            }
                        }

                        if (chk)
                        {
                            DataTable inData = new DataTable();
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotid;
                            row["BLOCK_TYPE_CODE"] = "MOVE_SHOP";       //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수
                            inData.Rows.Add(row);

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", inData);
                            if (dtRslt.Rows.Count != 0)
                            {
                                if (!ScanProcess(sLotid, sArea))
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (!ScanProcess(sLotid, sArea))
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }
        #region 대표LOT AREA 조회
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
        #endregion
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #region [E20231103-001744] [ESMI] 공정진척(R/N/S) 화면의 시작취소/장비완료취소 버튼 접근 제한 요청
        private bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

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

        #region [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청 
        private string GetPlantToPlantTransOpmodeFlag()
        {

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "PLANT_TO_PLANT_TRANS_OPMODE_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);
                
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _OPMODE = string.Empty;
                    _OPMODE = Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());
                    sOpmodeCheck = "Y";
                }
                else
                {
                    sOpmodeCheck = "N";
                }

                return sOpmodeCheck;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sOpmodeCheck = "N";
                return sOpmodeCheck;
            }
        }

        private bool IsLotattrProdLotOperModeCheck(string sLotid)
        {

            bool bRet = false;
            string sOpmode = "";

            try
            { 
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string)); 
                 
                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTATTR_PROD_LOT_OPER_MODE", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sOpmode = Util.NVC(dtResult.Rows[0]["PROD_LOT_OPER_MODE"].ToString());

                    if(_OPMODE.Equals(sOpmode) )
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                } 
                else
                {
                    bRet = false;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
                return bRet;
            }
        }
        #endregion  [E20240320-001047] 전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
    }

    
}

