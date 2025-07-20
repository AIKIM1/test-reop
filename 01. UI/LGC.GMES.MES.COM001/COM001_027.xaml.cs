/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.  
  2021.08.19  김지은    : 반품 시 LOT유형의 시험생산구분코드 Validation
  2024.02.05  정재홍    : [E20240104-001353] - 기존 BOXID는 SKID 변경, 신규 BOXID 추가 (기존 BOXID는 데이터 조회시 SKID ID 조회 함)
 
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

using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_027 : UserControl, IWorkArea
    {

        #region Declaration & Constructor       
        private const string _MovingSTAT = "MOVING";
        Util _Util = new Util();
        public bool bCancel = false;
        private string _APPRV_PASS_NO = string.Empty;
        private bool _QMS_AREA_YN = false;
        private bool _Multi_Input_YN = false;

        #region [QMS 결과값]
        private bool _bQMSCheck = false;
        private string _Message = string.Empty;
        #endregion

        private C1ComboBox cboOperation = new C1ComboBox(); // LOT
        private C1ComboBox cboOperation2 = new C1ComboBox(); // SKID

        public DataTable dtBasicInfo;   //01
        public DataTable dtPackingCard; //02

        public COM001_027()
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
            InitCombo();
            InitControl();
            SetEvent();
            InitQMSYN();
        }

        private void InitQMSYN()
        {
            DataTable dtQmsChkTarget = Qms_Chk_Target(); //common에 등록된 품질검사 대상 동 담아둠

            if (dtQmsChkTarget != null && dtQmsChkTarget.Rows.Count > 0)
            {
                for (int i = 0; i < dtQmsChkTarget.Rows.Count; i++)
                {
                    if (dtQmsChkTarget.Rows[i]["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID))
                    {
                        _QMS_AREA_YN = true;
                    }
                }
            }
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOut);
            listAuth.Add(btnOut3);
            listAuth.Add(btnReturn);
            listAuth.Add(btnDeleteForPrint);
            listAuth.Add(btnPrint);
            listAuth.Add(btnReturn2);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);            
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            #region 인계화면
            //Move 동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //공정
            cboOperation.DisplayMemberPath = "PROCID";
            cboOperation.SelectedValuePath = "PROCID";

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboMoveToArea, cboOperation };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "PROCESSEQUIPMENTSEGMENT");

            //이동유형
            string[] sFilter = { "MOVE_MTHD_CODE" };
            combo.SetCombo(cboTransType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
            #endregion

            chkPrint.IsChecked = false;

            #region 인계화면 - Cutid
            //Move 동
            C1ComboBox[] cboAreaChild2 = { cboEquipmentSegment3 };
            combo.SetCombo(cboMoveToArea3, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild2, sCase: "MOVETOAREA");

            //공정
            cboOperation2.DisplayMemberPath = "PROCID";
            cboOperation2.SelectedValuePath = "PROCID";

            //라인
            C1ComboBox[] cboEquipmentSegmentParent2 = { cboMoveToArea3, cboOperation2 };
            combo.SetCombo(cboEquipmentSegment3, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent2, sCase: "PROCESSEQUIPMENTSEGMENT");

            //이동유형
            combo.SetCombo(cboTransType2, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
            #endregion

            #region 이력조회화면
            //동
            C1ComboBox[] cboAreaChildFrom = { cboHistToEquipmentSegment };
            //combo.SetCombo(cboFromArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildFrom, cbParent: null, sCase: "AREA");
            combo.SetCombo(cboHistToArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChildFrom, sCase: "MOVETOAREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentFrom = { cboHistToArea };
            combo.SetCombo(cboHistToEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom, sCase: "EQUIPMENTSEGMENT");

            //이동유형
            combo.SetCombo(cboTransType3, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            string[] sFilter1 = { "MOVE_ORD_STAT_CODE" };
            combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            #endregion

            #region 통문증
            //인수동
            combo.SetCombo(cboHistToAreaForPrint, CommonCombo.ComboStatus.ALL, sCase: "MOVETOAREA");
            #endregion

            #region 이력조회 - 신규
            C1ComboBox[] cboAreaChildFrom2 = { cboHistToEquipmentSegment2 };
            //combo.SetCombo(cboFromArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildFrom, cbParent: null, sCase: "AREA");
            combo.SetCombo(cboHistToArea2, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChildFrom2, sCase: "MOVETOAREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentFrom2 = { cboHistToArea2 };
            combo.SetCombo(cboHistToEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom2, sCase: "EQUIPMENTSEGMENT");

            //이동유형
            combo.SetCombo(cboTransType4, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            
            combo.SetCombo(cboStat2, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            #endregion
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFromForPrint.SelectedDateTime = DateTime.Now;
            dtpDateToForPrint.SelectedDateTime = DateTime.Now;

            dtpDateFrom2.SelectedDateTime = DateTime.Now;
            dtpDateTo2.SelectedDateTime = DateTime.Now;

            txtFromArea.Text = LoginInfo.CFG_AREA_NAME;
            txtFromArea3.Text = LoginInfo.CFG_AREA_NAME;

            txtLotID_Out.Focus();

            //2020.09.17 전극 소형동 PLANT 이동 검사결과 표기
            if (_Util.IsCommonCodeUse("GQMS_COLUMN_VISIBILITY", LoginInfo.CFG_AREA_ID) == true)
            {
                dgMoveList.Columns["QMS_MOVE_SHOP_VALUE"].Visibility = Visibility.Visible;
                dgCutidList.Columns["QMS_MOVE_SHOP_VALUE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgMoveList.Columns["QMS_MOVE_SHOP_VALUE"].Visibility = Visibility.Collapsed;
                dgCutidList.Columns["QMS_MOVE_SHOP_VALUE"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            dtpDateFromForPrint.SelectedDataTimeChanged += dtpDateFromForPrint_SelectedDataTimeChanged;
            dtpDateToForPrint.SelectedDataTimeChanged += dtpDateToForPrint_SelectedDataTimeChanged;
            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;
        }
        #endregion

        #region 동간이동 - LOT 단위
        private void txtLotID_Out_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    string sLotid = string.Empty;
                    sLotid = txtLotID_Out.Text.Trim();

                    if (sLotid == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(String));

                    if ((bool)chkSkidID.IsChecked)
                    {
                        RQSTDT.Columns.Add("CSTID", typeof(String));
                    }
                    // CSR : E20240104-001353
                    else if ((bool)chkBoxID.IsChecked)
                    {
                        RQSTDT.Columns.Add("BOXID", typeof(String));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("LOTID", typeof(String));
                    }
                    RQSTDT.Columns.Add("AREAID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;

                    if ((bool)chkSkidID.IsChecked)
                    {
                        dr["CSTID"] = sLotid;
                    }
                    // CSR : E20240104-001353
                    else if ((bool)chkBoxID.IsChecked)
                    {
                        dr["BOXID"] = sLotid;
                    }
                    else
                    {
                        if ((bool)chkRFID.IsChecked)
                        {
                            //RFID로 LOTID 가져옴
                            DataTable dt = new DataTable();
                            dt.TableName = "RQSTDT";
                            dt.Columns.Add("CSTID", typeof(string));

                            DataRow dr1 = dt.NewRow();
                            dr1["CSTID"] = sLotid;

                            dt.Rows.Add(dr1);

                            DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                            if (dtLot.Rows.Count == 0)
                            {
                                //재공 정보가 없습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }
                            else
                            {
                                sLotid = dtLot.Rows[0]["LOTID"].ToString();
                                dr["LOTID"] = sLotid;
                            }
                        }
                        else
                        {
                            dr["LOTID"] = sLotid;
                        }
                    }

                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                    RQSTDT.Rows.Add(dr);
                    //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count == 0)
                    {
                        //재공 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Init();
                            }
                        });
                        return;
                    }
                    else
                    {
                        #region [CWA 전극 1동 -> CWA 전극 2동 QMS 체크 - 임시 - 2021.01.16]
                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            bool bFlag = QMS_CHEK_IMSI(SearchResult.Rows[i]["LOTID"].ToString());
                            if (bFlag == false)
                                return;
                        }
                        #endregion

                        #region [QMS 결과값 (2018.06.12)]
                        string dtQmsResult = string.Empty; ; // 품질검사 결과 담아둠.                        
                        string sLotid_Target = string.Empty; // 검사대상 LOTID 임시저장

                        // 판정FLAG Column 생성
                        DataColumn newCol = new DataColumn("QMS_JUDG_FLAG", typeof(string));
                        newCol.DefaultValue = "";
                        SearchResult.Columns.Add(newCol);

                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            sLotid_Target = SearchResult.Rows[i]["LOTID"].ToString();
                            dtQmsResult = QmsResult(sLotid_Target);

                            DataRow[] dRow = SearchResult.Select("LOTID = '" + sLotid_Target + "'");

                            if (dRow.Length > 0)
                            {
                                dRow[0]["QMS_JUDG_VALUE"] = getQmsResultName(dtQmsResult);
                                dRow[0]["QMS_JUDG_FLAG"] = dtQmsResult;
                            }

                        }
                        #endregion

                        #region # 특별관리 Lot - 인계가능 라인 체크 
                        if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                        {
                            string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                            string sLine = Util.GetCondition(cboEquipmentSegment);

                            if (!SpclLine.Contains(sLine) || string.IsNullOrEmpty(sLine))
                            {
                               // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                               LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]), SpclLine }), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }
                        }
                        #endregion

                        for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                        {
                            if ((bool)chkSkidID.IsChecked)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CSTID")) == sLotid)
                                {
                                    //동일한 LOT이 스캔되었습니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1504"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            Init();
                                        }
                                    });
                                    return;
                                }
                            }
                            else if ((bool)chkBoxID.IsChecked)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "BOXID")) == sLotid)
                                {
                                    // %1은(는) 이미 스캔한 값 입니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            Init();
                                        }
                                    });
                                    return;
                                }
                            }
                            else
                            {
                                if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                                {
                                    //동일한 LOT이 스캔되었습니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1504"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            Init();
                                        }
                                    });
                                    return;
                                }
                            }

                            //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                            //기존 Validation 삭제	
                            //시생산 LOT인 경우 다른 LOTTYPE과 섞이면 안됨
                            //if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() != SearchResult.Rows[0]["LOTTYPE"].ToString())
                            //{
                            //    if ((DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() == "X") || (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X"))
                            //    {
                            //        //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5149"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //        {
                            //            if (result == MessageBoxResult.OK)
                            //            {
                            //                Init();
                            //            }
                            //        });
                            //        return;
                            //    }
                            //}

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
                                    return;
                                }
                            }
                            #endregion
                        }

                        // 재공정보 ROLL 정보 체크 [2017-09-26] -> 2018-05-23(점보 ROLL 중에서 이동 가능한 동의 경우 추가 : ROLL_PRODID_MOVE_AREA
                        if (IsRollAvailableArea(Util.NVC(LoginInfo.CFG_AREA_ID)) == false) //ROLL_PRODID_MOVE_AREA에 등록되어 있으는 동은 아래 점보Roll 체크 로직 건너 뜀..
                        {
                            if (IsRollCodeChange(Util.NVC(SearchResult.Rows[0]["PRODID"])) == true)
                            {
                                Util.MessageValidation("SFU4127");  //ROLL 코드변환한 LOT입니다. 코드변환 취소 후 동간이동하시기 바랍니다.
                                return;
                            }
                        }

                        if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                        {
                            if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("END"))
                            {
                                //재공 상태가 이동가능한 상태가 아닙니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1869"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                        }
                        /*20171117 Hold 로직 제외
                                                // HOLD 체크 로직 추가
                                                if (GetHoldCheckArea() == true && SearchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                                                {
                                                    //HOLD 된 LOT ID 입니다.
                                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1340"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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
                        dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr1["COM_TYPE_CODE"] = "AUTO_STOCK_OUT";

                        RQSTDT1.Rows.Add(dr1);
                        DataTable SearchResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AUTO_STOCK_OUT", "RQSTDT", "RSLTDT", RQSTDT1);

                        //if (SearchResult1.Rows.Count == 0)
                        //{
                            if (SearchResult.Rows[0]["RACK_ID"].ToString() != "" && SearchResult.Rows[0]["RACK_ID"].ToString() != null)
                            {
                                if (SearchResult.Rows[0]["PCSGID_1"].ToString() != "F")
                                {
                                    if (SearchResult1.Rows.Count == 0)
                                    {
                                        //창고에서 출고되지 않았습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2963"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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
                        //}

                        // 전극만 체크..
                        if (SearchResult.Rows[0]["PCSGID"].ToString() == "E" || SearchResult.Rows[0]["PCSGID"].ToString() == "S")
                        {
                            if (SearchResult.Rows[0]["PROD_LEVEL_CODE"].ToString() == "PC")
                            {
                                if (Convert.ToDecimal(SearchResult.Rows[0]["WIPQTY"].ToString()) <= 30)
                                {
                                    //스캔한 LOT 은 30M 이하입니다. 계속 하시겠습니까?
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3479"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                    {
                                        if (result == MessageBoxResult.Cancel)
                                        {
                                            Init();
                                        }
                                        else
                                        {
                                            if (dgMoveList.GetRowCount() == 0)
                                            {
                                                Util.GridSetData(dgMoveList, SearchResult, FrameOperation);
                                                Init();
                                            }
                                            else
                                            {
                                                if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                                                {
                                                    //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                                    if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                                    {
                                                        DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                                                        dtSource.Merge(SearchResult);

                                                        Util.gridClear(dgMoveList);
                                                        Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                                                        Init();
                                                    }
                                                    else
                                                    {
                                                        Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                                        Init();
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    //제품ID가 같지 않습니다.
                                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result2) =>
                                                    {
                                                        if (result2 == MessageBoxResult.OK)
                                                        {
                                                            Init();
                                                        }
                                                    });
                                                    return;
                                                }
                                            }
                                        }
                                    });
                                    return;
                                }
                            }
                        }

                        if (dgMoveList.GetRowCount() > 1)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                            {
                                //이전에 스캔한 LOT의 공정과 다릅니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1789"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }


                        }


                        if (dgMoveList.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgMoveList, SearchResult, FrameOperation);

                            // 공정SEGMENT 추가 [2017-10-11]
                            if (SearchResult != null && SearchResult.Rows.Count > 0)
                            {
                                DataView view = new DataView(SearchResult);
                                DataTable distinctValues = view.ToTable(true, "PROCID");

                                //cboOperation.Items.Add(Util.NVC(SearchResult.Rows[0]["PROCID"]));
                                cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                                cboOperation.SelectedIndex = 0;

                                string sOriginalMoveArea = Util.NVC(cboMoveToArea.SelectedValue);
                                string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                                cboMoveToArea.SelectedIndex = 0;
                                cboMoveToArea.SelectedValue = sOriginalMoveArea;
                                cboEquipmentSegment.SelectedValue = sOriginalLine;
                            }

                            Init();
                        }
                        else
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID")).Equals(SearchResult.Rows[0]["PRODID"].ToString()))
                            {
                                //제품ID가 같지 않습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                            if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "CURR_WIP_QLTY_TYPE_CODE")).Equals(Util.NVC(SearchResult.Rows[0]["CURR_WIP_QLTY_TYPE_CODE"])))
                            {
                                //재공 품질 유형코드가 같지 않습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4241"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                            //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                            if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                            {
                                Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                Init();
                                return;
                            }

                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "MKT_TYPE_CODE").ToString() != SearchResult.Rows[0]["MKT_TYPE_CODE"].ToString())
                            {
                                //내수용과 수출용은 같이 포장할 수 없습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4454"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return;
                            }

                            DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                            dtSource.Merge(SearchResult);

                            Util.gridClear(dgMoveList);
                            Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                            Init();


                            //if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                            //{
                            //    //DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                            //    //dtSource.Merge(SearchResult);

                            //    //Util.gridClear(dgMoveList);
                            //    //Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                            //    //Init();
                            //}
                            //else
                            //{
                            //    //제품ID가 같지 않습니다.
                            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //    {
                            //        if (result == MessageBoxResult.OK)
                            //        {
                            //            Init();
                            //        }
                            //    });
                            //    return;
                            //}

                            //if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "CURR_WIP_QLTY_TYPE_CODE")).Equals(Util.NVC(SearchResult.Rows[0]["CURR_WIP_QLTY_TYPE_CODE"])))
                            //{
                            //    DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                            //    dtSource.Merge(SearchResult);

                            //    Util.gridClear(dgMoveList);
                            //    Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                            //    Init();
                            //}
                            //else
                            //{
                            //    //재공 품질 유형코드가 같지 않습니다.
                            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4241"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //    {
                            //        if (result == MessageBoxResult.OK)
                            //        {
                            //            Init();
                            //        }
                            //    });
                            //    return;
                            //}
                        }

                        #region [QMS 검사 결과체크 SCAN 시 제외 [2018.06.08]]
                        //QMS 검사 결과체크 로직 추가(2018-05-28)
                        //QMS_CHECK(SearchResult);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private bool QMS_CHEK_IMSI(string sLotID)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = sLotID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(row);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_MOVE_AREA", "INDATA", null, ds);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void txtLotID_OUT_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl) && _Multi_Input_YN == true)
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
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
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Processing(sPasteStrings[i]) == false)
                            break;

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

        private bool Multi_Processing(string sLotid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));

                if ((bool)chkSkidID.IsChecked)
                {
                    RQSTDT.Columns.Add("CSTID", typeof(String));
                }
                // CSR : E20240104-001353
                else if ((bool)chkBoxID.IsChecked)
                {
                    RQSTDT.Columns.Add("BOXID", typeof(String));
                }
                else
                {
                    RQSTDT.Columns.Add("LOTID", typeof(String));
                }
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if ((bool)chkSkidID.IsChecked)
                {
                    dr["CSTID"] = sLotid;
                }
                // CSR : E20240104-001353
                else if ((bool)chkBoxID.IsChecked)
                {
                    dr["BOXID"] = sLotid;
                }
                else
                {
                    dr["LOTID"] = sLotid;
                }
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);
                //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4950",sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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

                    #region [QMS 결과값 (2018.06.12)]]
                    string dtQmsResult = string.Empty; ; // 품질검사 결과 담아둠.                        
                    string sLotid_Target = string.Empty; // 검사대상 LOTID 임시저장

                    // 판정FLAG Column 생성
                    DataColumn newCol = new DataColumn("QMS_JUDG_FLAG", typeof(string));
                    newCol.DefaultValue = "";
                    SearchResult.Columns.Add(newCol);

                    for (int i = 0; i < SearchResult.Rows.Count; i++)
                    {
                        sLotid_Target = SearchResult.Rows[i]["LOTID"].ToString();
                        dtQmsResult = QmsResult(sLotid_Target);

                        DataRow[] dRow = SearchResult.Select("LOTID = '" + sLotid_Target + "'");

                        if (dRow.Length > 0)
                        {
                            dRow[0]["QMS_JUDG_VALUE"] = getQmsResultName(dtQmsResult);
                            dRow[0]["QMS_JUDG_FLAG"] = dtQmsResult;
                        }
                    }
                    #endregion


                    #region # 특별관리 Lot - 인계가능 라인 체크
                    if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                    {
                        string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                        string sLine = Util.GetCondition(cboEquipmentSegment);

                        if (!SpclLine.Contains(sLine))
                        {
                            Init();
                            Util.MessageInfo("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]), SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                            return false;
                        }
                    }
                    #endregion

                    for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                    {
                        if ((bool)chkSkidID.IsChecked)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CSTID").ToString() == sLotid)
                            {
                                //동일한 LOT이 스캔되었습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4951", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                 {
                                     if (result == MessageBoxResult.OK)
                                     {
                                         Init();
                                     }
                                 });
                                return false;
                            }
                        }
                        else if ((bool)chkBoxID.IsChecked)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "BOXID").ToString() == sLotid)
                            {
                                //동일한 LOT이 스캔되었습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4951", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        Init();
                                    }
                                });
                                return false;
                            }
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                //동일한 LOT이 스캔되었습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4951", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                 {
                                     if (result == MessageBoxResult.OK)
                                     {
                                         Init();
                                     }
                                 });
                                return false;
                            }
                        }

                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                        //기존 Validation 삭제	
                        //시생산 LOT인 경우 다른 LOTTYPE과 섞이면 안됨
                        //if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() != SearchResult.Rows[0]["LOTTYPE"].ToString())
                        //{
                        //    if ((DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTTYPE").ToString() == "X") || (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X"))
                        //    {
                        //        //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5149"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                Init();
                        //            }
                        //        });
                        //        return false;
                        //    }
                        //}

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

                    // 재공정보 ROLL 정보 체크 [2017-09-26] -> 2018-05-23(점보 ROLL 중에서 이동 가능한 동의 경우 추가 : ROLL_PRODID_MOVE_AREA
                    if (IsRollAvailableArea(Util.NVC(LoginInfo.CFG_AREA_ID)) == false) //ROLL_PRODID_MOVE_AREA에 등록되어 있으는 동은 아래 점보Roll 체크 로직 건너 뜀..
                    {
                        if (IsRollCodeChange(Util.NVC(SearchResult.Rows[0]["PRODID"])) == true)
                        {
                            Util.MessageValidation("SFU4952", sLotid);  //ROLL 코드변환한 LOT입니다. 코드변환 취소 후 동간이동하시기 바랍니다.
                            return false;
                        }
                    }

                    if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("END"))
                        {
                            //재공 상태가 이동가능한 상태가 아닙니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4953", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                             {
                                 if (result == MessageBoxResult.OK)
                                 {
                                     Init();
                                 }
                             });
                            return false;
                        }

                    }
                    /*20171117 Hold 로직 제외
                                            // HOLD 체크 로직 추가
                                            if (GetHoldCheckArea() == true && SearchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                                            {
                                                //HOLD 된 LOT ID 입니다.
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1340"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                                {
                                                    if (result == MessageBoxResult.OK)
                                                    {
                                                        Init();
                                                    }
                                                });
                                                return;
                                            }
                    */
                    if (SearchResult.Rows[0]["RACK_ID"].ToString() != "" && SearchResult.Rows[0]["RACK_ID"].ToString() != null)
                    {
                        if (SearchResult.Rows[0]["PCSGID_1"].ToString() != "F")
                        {
                            //창고에서 출고되지 않았습니다.
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

                    // 전극만 체크..
                    if (SearchResult.Rows[0]["PCSGID"].ToString() == "E" || SearchResult.Rows[0]["PCSGID"].ToString() == "S")
                    {
                        if (SearchResult.Rows[0]["PROD_LEVEL_CODE"].ToString() == "PC")
                        {
                            if (Convert.ToDecimal(SearchResult.Rows[0]["WIPQTY"].ToString()) <= 30)
                            {
                                //스캔한 LOT 은 30M 이하입니다. 계속 하시겠습니까?
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4955", sLotid), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                 {
                                     if (result == MessageBoxResult.Cancel)
                                     {
                                         Init();
                                     }
                                     else
                                     {
                                         if (dgMoveList.GetRowCount() == 0)
                                         {
                                             Util.GridSetData(dgMoveList, SearchResult, FrameOperation);
                                             Init();
                                         }
                                         else
                                         {
                                             if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                                             {
                                                 //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                                 if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                                 {
                                                     DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                                                     dtSource.Merge(SearchResult);

                                                     Util.gridClear(dgMoveList);
                                                     Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                                                     Init();
                                                 }
                                                 else
                                                 {
                                                     Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                                     Init();
                                                     return;
                                                 }
                                             }
                                             else
                                             {
                                                 //제품ID가 같지 않습니다.
                                                 LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4956", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result2) =>
                                                  {
                                                      if (result2 == MessageBoxResult.OK)
                                                      {
                                                          Init();
                                                      }
                                                  });
                                                 return;
                                             }
                                         }
                                     }
                                 });
                                return false;
                            }
                        }
                    }

                    if (dgMoveList.GetRowCount() > 1)
                    {
                        if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                        {
                            //이전에 스캔한 LOT의 공정과 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4957", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                             {
                                 if (result == MessageBoxResult.OK)
                                 {
                                     Init();
                                 }
                             });
                            return false;
                        }


                    }


                    if (dgMoveList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgMoveList, SearchResult, FrameOperation);

                        // 공정SEGMENT 추가 [2017-10-11]
                        if (SearchResult != null && SearchResult.Rows.Count > 0)
                        {
                            DataView view = new DataView(SearchResult);
                            DataTable distinctValues = view.ToTable(true, "PROCID");

                            //cboOperation.Items.Add(Util.NVC(SearchResult.Rows[0]["PROCID"]));
                            cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                            cboOperation.SelectedIndex = 0;

                            string sOriginalMoveArea = Util.NVC(cboMoveToArea.SelectedValue);
                            string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                            cboMoveToArea.SelectedIndex = 0;
                            cboMoveToArea.SelectedValue = sOriginalMoveArea;
                            cboEquipmentSegment.SelectedValue = sOriginalLine;
                        }

                        Init();
                    }
                    else
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID")).Equals(SearchResult.Rows[0]["PRODID"].ToString()))
                        {
                            //제품ID가 같지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4956", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                             {
                                 if (result == MessageBoxResult.OK)
                                 {
                                     Init();
                                 }
                             });
                            return false;
                        }

                        if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "CURR_WIP_QLTY_TYPE_CODE")).Equals(Util.NVC(SearchResult.Rows[0]["CURR_WIP_QLTY_TYPE_CODE"])))
                        {
                            //재공 품질 유형코드가 같지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4958", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                             {
                                 if (result == MessageBoxResult.OK)
                                 {
                                     Init();
                                 }
                             });
                            return false;
                        }

                        if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "MKT_TYPE_CODE").ToString() != SearchResult.Rows[0]["MKT_TYPE_CODE"].ToString())
                        {
                            //내수용과 수출용은 같이 포장할 수 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4959", sLotid), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                             {
                                 if (result == MessageBoxResult.OK)
                                 {
                                     Init();
                                 }
                             });
                            return false;
                        }

                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (!Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                            Init();
                            return false;
                        }

                        DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                        dtSource.Merge(SearchResult);

                        Util.gridClear(dgMoveList);
                        Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                        Init();

                        return true;
                        //if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                        //{
                        //    //DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                        //    //dtSource.Merge(SearchResult);

                        //    //Util.gridClear(dgMoveList);
                        //    //Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                        //    //Init();
                        //}
                        //else
                        //{
                        //    //제품ID가 같지 않습니다.
                        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //    {
                        //        if (result == MessageBoxResult.OK)
                        //        {
                        //            Init();
                        //        }
                        //    });
                        //    return;
                        //}

                        //if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "CURR_WIP_QLTY_TYPE_CODE")).Equals(Util.NVC(SearchResult.Rows[0]["CURR_WIP_QLTY_TYPE_CODE"])))
                        //{
                        //    DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                        //    dtSource.Merge(SearchResult);

                        //    Util.gridClear(dgMoveList);
                        //    Util.GridSetData(dgMoveList, dtSource, FrameOperation);
                        //    Init();
                        //}
                        //else
                        //{
                        //    //재공 품질 유형코드가 같지 않습니다.
                        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4241"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //    {
                        //        if (result == MessageBoxResult.OK)
                        //        {
                        //            Init();
                        //        }
                        //    });
                        //    return;
                        //}
                    }

                    #region [QMS 검사 결과체크 SCAN 시 제외 [2018.06.08]]
                    //QMS 검사 결과체크 로직 추가(2018-05-28)
                    //QMS_CHECK(SearchResult);
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private bool QMS_CHECK(DataTable SearchResult)
        {
            try
            {
                // 2018-05-28 로직 추가 : 오창소형만 해당
                // 1. COMMON CODE에 'QMS_CHECK_AREA_MOVE'에 등록된 AREAID(오창 소형만 등록) 확인
                // 2. 1.에 등록된 동만 품질 검사Check : BR_PRD_CHK_QMS_FOR_PACKING_DETAIL_RESULT 호출 - RETURN 결과(R/P 미검사, R/P 불합격, C/T 미검사, C/T 불합격)
                // 2-1. BR_PRD_CHK_QMS_FOR_PACKING_DETAIL_RESULT -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                // 3. 공정이 E7000 인지에 따라 따라 이송 여부 결정 : E7000(Pancake) - 이송 불가능 / E700이외(ROLL) - 이송가능
                // 4. E7000이 아닌 경우(ROLL) 담당자에게 메일 발송(검사결과가 없거나, 불합격인 LOT LIST). 

                // 1.품질검사 대상동(오창 소형) SEARCH
                DataTable dtQmsChkTarget = Qms_Chk_Target(); //common에 등록된 품질검사 대상 동 담아둠

                bool qmsChk = false; //품질검사 대상 유무 변수
                bool moveYN = false; //이송가능한지 불가능한지 여부(Y:이송가능 / N:이송불가능)
                bool msgViewYn = false; //팝업 띄울지 여부
                bool msgMoveYN = true; //인계처리 버튼 클릭시 사용

                string dtQmsResult = string.Empty; ; // 품질검사 결과 담아둠.                        
                string sLotid_Target = string.Empty; // 검사대상 LOTID 임시저장
                string sProcid_Target = string.Empty;// 검사대상 LOT의 공정 임시저장
                string msg = string.Empty;           // 화면에 뿌릴 메시지
                string msg_RP_NoInsp = "R/P" + ObjectDic.Instance.GetObjectName("미검사") + " - "; // R/P 미검사
                string msg_RP_NG     = "R/P" + ObjectDic.Instance.GetObjectName("불합격") + " - "; // R/P 불합격
                string msg_CT_NoInsp = "C/T" + ObjectDic.Instance.GetObjectName("미검사") + " - "; // C/T 미검사
                string msg_CT_NG     = "C/T" + ObjectDic.Instance.GetObjectName("불합격") + " - "; // C/T 불합격
                string msg_QMS_NO    = ObjectDic.Instance.GetObjectName("검사결과") + ObjectDic.Instance.GetObjectName("없음") + " - ";// 겸사결과 없음

                //CWA 전극 2동 추가
                string msg_ST_NoInsp = "S/T" + ObjectDic.Instance.GetObjectName("미검사") + " - "; // S/T 미검사
                string msg_ST_NG     = "S/T" + ObjectDic.Instance.GetObjectName("불합격") + " - "; // S/T 불합격


                int _msg_RP_NoInsp = 0;
                int _msg_RP_NG = 0;
                int _msg_CT_NoInsp = 0;
                int _msg_CT_NG = 0;
                int _msg_QMS_NO = 0;

                //CWA 전극 2동 추가
                int _msg_ST_NoInsp = 0;
                int _msg_ST_NG = 0;

                _Message = string.Empty;

                if (dtQmsChkTarget != null && dtQmsChkTarget.Rows.Count > 0)
                {
                    for (int i = 0; i < dtQmsChkTarget.Rows.Count; i++)
                    {
                        if (dtQmsChkTarget.Rows[i]["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID))
                        {
                            qmsChk = true;
                        }
                    }
                }

                if (qmsChk)//품질검사 대상이므로 품질검사 로직 실행.
                {
                    _QMS_AREA_YN = true;
                    
                    if (sTotalInspectionAreaCheck())
                    {
                        moveYN = true;
                        msgViewYn = false;

                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            sLotid_Target = SearchResult.Rows[i]["LOTID"].ToString();
                            sProcid_Target = SearchResult.Rows[i]["PROCID"].ToString();


                            //2. 품질검사 결과 가져옴.
                            dtQmsResult = QmsResult(sLotid_Target);

                            #region 전수검사대상 - 공정 구분 없음
                            if (dtQmsResult.Length == 0) //품질검사 결과가 없을때
                            {
                                _msg_QMS_NO++;
                                msg_QMS_NO = msg_QMS_NO + sLotid_Target + ", ";
                            }

                            if (!dtQmsResult.Equals("P")) //품질검사 결과가 합격이 아닌 경우(미검사,불합격)
                            {
                                switch (dtQmsResult)
                                {
                                    case "RNO":// R/P 미검사
                                        if (_msg_RP_NoInsp == 0) { msg_RP_NoInsp = msg_RP_NoInsp + sLotid_Target; }
                                        else { msg_RP_NoInsp = msg_RP_NoInsp + ", " + sLotid_Target; }
                                        _msg_RP_NoInsp++;
                                        break;
                                    case "RNG":// R/P 불합격
                                        if (_msg_RP_NG == 0) { msg_RP_NG = msg_RP_NG + sLotid_Target; }
                                        else { msg_RP_NG = msg_RP_NG + ", " + sLotid_Target; }
                                        _msg_RP_NG++;
                                        break;
                                    case "CNO":// C/T 미검사
                                        if (_msg_CT_NoInsp == 0) { msg_CT_NoInsp = msg_CT_NoInsp + sLotid_Target; }
                                        else { msg_CT_NoInsp = msg_CT_NoInsp + ", " + sLotid_Target; }
                                        _msg_CT_NoInsp++;
                                        break;
                                    case "CNG":// C/T 불합격
                                        if (_msg_CT_NG == 0) { msg_CT_NG = msg_CT_NG + sLotid_Target; }
                                        else { msg_CT_NG = msg_CT_NG + ", " + sLotid_Target; }
                                        _msg_CT_NG++;
                                        break;
                                    case "SNO":// S/T 미검사
                                        if (_msg_ST_NoInsp == 0) { msg_ST_NoInsp = msg_ST_NoInsp + sLotid_Target; }
                                        else { msg_ST_NoInsp = msg_ST_NoInsp + ", " + sLotid_Target; }
                                        _msg_ST_NoInsp++;
                                        break;
                                    case "SNG":// S/T 불합격
                                        if (_msg_ST_NG == 0) { msg_ST_NG = msg_ST_NG + sLotid_Target; }
                                        else { msg_ST_NG = msg_ST_NG + ", " + sLotid_Target; }
                                        _msg_ST_NG++;
                                        break;
                                    default:
                                        break;
                                }
                                moveYN = false;
                                msgViewYn = true;
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            sLotid_Target = SearchResult.Rows[i]["LOTID"].ToString();
                            sProcid_Target = SearchResult.Rows[i]["PROCID"].ToString();

                            //2. 품질검사 결과 가져옴.
                            dtQmsResult = QmsResult(sLotid_Target);

                            #region 오창 소형의 경우 PANCAKE은 이송 불가. ROLL만 가능 - 현재기준
                            //3.공정구분
                            if (sProcid_Target.Equals("E7000")) //이송불가능(Pancake)
                            {
                                moveYN = false;

                                if (dtQmsResult.Length == 0) //품질검사 결과가 없을때
                                {
                                    _msg_QMS_NO++;
                                    msg_QMS_NO = msg_QMS_NO + sLotid_Target + ", ";
                                }

                                if (!dtQmsResult.Equals("P")) //품질검사 결과가 합격이 아닌 경우(미검사,불합격)
                                {
                                    switch (dtQmsResult)
                                    {
                                        case "RNO":// R/P 미검사
                                            if (_msg_RP_NoInsp == 0) { msg_RP_NoInsp = msg_RP_NoInsp + sLotid_Target; }
                                            else { msg_RP_NoInsp = msg_RP_NoInsp + ", " + sLotid_Target; }
                                            _msg_RP_NoInsp++;
                                            break;
                                        case "RNG":// R/P 불합격
                                            if (_msg_RP_NG == 0) { msg_RP_NG = msg_RP_NG + sLotid_Target; }
                                            else { msg_RP_NG = msg_RP_NG + ", " + sLotid_Target; }
                                            _msg_RP_NG++;
                                            break;
                                        case "CNO":// C/T 미검사
                                            if (_msg_CT_NoInsp == 0) { msg_CT_NoInsp = msg_CT_NoInsp + sLotid_Target; }
                                            else { msg_CT_NoInsp = msg_CT_NoInsp + ", " + sLotid_Target; }
                                            _msg_CT_NoInsp++;
                                            break;
                                        case "CNG":// C/T 불합격
                                            if (_msg_CT_NG == 0) { msg_CT_NG = msg_CT_NG + sLotid_Target; }
                                            else { msg_CT_NG = msg_CT_NG + ", " + sLotid_Target; }
                                            _msg_CT_NG++;
                                            break;
                                        default:
                                            break;
                                    }

                                    msgViewYn = true;
                                }
                                else
                                {
                                    msgViewYn = false;
                                }
                            }
                            else //이송가능(Roll)
                            {
                                moveYN = true;

                                if (dtQmsResult.Length == 0) //품질검사 결과가 없을때
                                {
                                    _msg_QMS_NO++;
                                    msg_QMS_NO = msg_QMS_NO + sLotid_Target + ", ";
                                }

                                if (!dtQmsResult.Equals("P")) //품질검사 결과가 합격이 아닌 경우(미검사,불합격)
                                {
                                    switch (dtQmsResult)
                                    {
                                        case "RNO":// R/P 미검사
                                            if (_msg_RP_NoInsp == 0) { msg_RP_NoInsp = msg_RP_NoInsp + sLotid_Target; }
                                            else { msg_RP_NoInsp = msg_RP_NoInsp + ", " + sLotid_Target; }
                                            _msg_RP_NoInsp++;
                                            break;
                                        case "RNG":// R/P 불합격
                                            if (_msg_RP_NG == 0) { msg_RP_NG = msg_RP_NG + sLotid_Target; }
                                            else { msg_RP_NG = msg_RP_NG + ", " + sLotid_Target; }
                                            _msg_RP_NG++;
                                            break;
                                        case "CNO":// C/T 미검사
                                            if (_msg_CT_NoInsp == 0) { msg_CT_NoInsp = msg_CT_NoInsp + sLotid_Target; }
                                            else { msg_CT_NoInsp = msg_CT_NoInsp + ", " + sLotid_Target; }
                                            _msg_CT_NoInsp++;
                                            break;
                                        case "CNG":// C/T 불합격
                                            if (_msg_CT_NG == 0) { msg_CT_NG = msg_CT_NG + sLotid_Target; }
                                            else { msg_CT_NG = msg_CT_NG + ", " + sLotid_Target; }
                                            _msg_CT_NG++;
                                            break;
                                        default:
                                            break;
                                    }
                                    msgViewYn = true;
                                }
                                else
                                {
                                    msgViewYn = false;
                                }
                            }
                            #endregion

                        }//for문 끝
                    }
                    
                    if (_msg_RP_NoInsp > 0)
                    {
                        msg = msg + msg_RP_NoInsp + "\r\n";
                    }

                    if (_msg_RP_NG > 0)
                    {
                        msg = msg + msg_RP_NG + "\r\n";
                    }

                    if(_msg_CT_NoInsp > 0)
                    {
                        msg = msg + msg_CT_NoInsp + "\r\n";
                    }

                    if(_msg_CT_NG > 0)
                    {
                        msg = msg + msg_CT_NG + "\r\n";
                    }

                    if (_msg_ST_NoInsp > 0)
                    {
                        msg = msg + msg_ST_NoInsp + "\r\n";
                    }

                    if (_msg_ST_NG > 0)
                    {
                        msg = msg + msg_ST_NG + "\r\n";
                    }

                    if (_msg_QMS_NO > 0)
                    {
                        msg = msg + _msg_QMS_NO + "\r\n";
                    }

                    msg = msg + "\r\n";                  

                    if (msgViewYn)
                    {
                        if (moveYN)
                        {
                            //Util.AlertInfo("SFU4943", msg); //%1 확인 바랍니다.
                            _bQMSCheck = true;  //QMS Message Check    
                            _Message = msg; // Message Popup
                        }
                        else
                        {
                            Util.AlertInfo("SFU4942", msg); //%1 이송이 불가합니다.                           
                        }
                    }

                    if(msgViewYn == true && moveYN ==false) //품질검사 불합격 내용이 있고 이송이 불가능한 경우
                    {
                        msgMoveYN = false;
                    }

                    if (msgViewYn == true && moveYN == true) //품질검사 불합격 내용이 있고 이송이 가능한 경우
                    {
                        msgMoveYN = false;
                    }
                }

                return msgMoveYN;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private DataTable Qms_Chk_Target()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "QMS_CHECK_AREA_MOVE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private string QmsResult(string sLotid)
        {
            try
            {
                string QmsResult = string.Empty;
                string sTotalInspection = string.Empty;

                // 폴란드 실전에만 존재하는 BIZ 주석처리 (2020.07.07)
                //sTotalInspection = sTotalInspectionAreaQMSResult(sLotid);
                //if (!string.IsNullOrEmpty(sTotalInspection))
                //{
                //    //현재 CWA 전극2동만 해당함 - 임시로 해달라고 했음. 그지같이....
                //    return sTotalInspection;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("BR_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["BLOCK_TYPE_CODE"] = "MOVE_AREA";        //NEW HOLD Type 변수
                dr["BR_TYPE"] = "P_RESULT";                 //OLD BR Search 변수

                RQSTDT.Rows.Add(dr);

                //BR_PRD_CHK_QMS_FOR_PACKING_DETAIL_RESULT -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                //신규 HOLD 적용을 위해 변경 작업
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    QmsResult = dtResult.Rows[0]["QMS_RESULT"].ToString();
                }
                else
                {
                    QmsResult = ""; 
                }

                return QmsResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string sTotalInspectionAreaQMSResult(string sLotid)
        {
            try
            {
                string QmsResult = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["CMCDTYPE"] = "QMS_INSP_CHECK_AREA_RP";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_QMS_FOR_ST_TOTAL_INSPECTION", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    QmsResult = dtResult.Rows[0]["QMS_RESULT"].ToString();
                }
                else
                {
                    QmsResult = String.Empty;
                }

                return QmsResult;
            }
            catch (Exception ex)
            {
                return string.Empty; 
            }
        }

        private bool sTotalInspectionAreaCheck()
        {
            try
            {
                string QmsResult = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "QMS_INSP_CHECK_AREA_RP";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FULL_INSP_AREA_TYPE_CHECK", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private string getQmsResultName(string sCode)
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
                dr["CMCDTYPE"] = "QMS_RESULT";
                dr["CMCODE"] = sCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["CMCDNAME"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private void sendMail()
        {
            try
            {

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void Init()
        {
            txtLotID_Out.Text = "";
            txtLotID_Out.Focus();
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 동간이동 인계

                string sMove_Ord_ID = string.Empty;

                if (dgMoveList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (cboMoveToArea.SelectedIndex < 0 || cboMoveToArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }
                
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals(""))
                {
                    Util.MessageInfo("라인을 선택하세요."); //라인을 선택하세요.
                    return;
                }

                #region # 특별관리 Lot - 인계가능 라인 체크
                string sLine = Util.GetCondition(cboEquipmentSegment);

                if (CommonVerify.HasDataGridRow(dgMoveList))
                {
                    DataTable dt = ((DataView)dgMoveList.ItemsSource).Table;
                    foreach (DataRow dRow in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(dRow["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(dRow["RSV_EQSGID_LIST"])))
                        {
                            if (!Util.NVC(dRow["RSV_EQSGID_LIST"]).Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(dRow["LOTID"]), Util.NVC(dRow["RSV_EQSGID_LIST"]) });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return;
                            }
                        }
                    }
                }
                #endregion
                                
                // 1. 품질검사 체크
                bool bQMS_Check;
                bQMS_Check = QMS_CHECK(DataTableConverter.Convert(dgMoveList.ItemsSource));

                //품질체크
                if (!bQMS_Check)
                {
                    if (sTotalInspectionAreaCheck())
                        return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2931"), !string.IsNullOrEmpty(_Message) ? _Message : null , "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    string sToshop = string.Empty;
                    string sToArea = string.Empty;
                    string sToEqsg = string.Empty;
                    string sMovetype = string.Empty;

                    if (result == MessageBoxResult.OK)
                    {
                        if (_QMS_AREA_YN) //품질검사 체크 동인 경우만
                        {
                            //품질체크
                            if (!bQMS_Check)
                            {
                                if (!sTotalInspectionAreaCheck())
                                {
                                    if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").Equals("E7000")) //Pancake의 경우 이동불가
                                    {
                                        return;
                                    }
                                }
                            }
                        }

                        sToArea = Util.GetCondition(cboMoveToArea);
                        sToEqsg = Util.GetCondition(cboEquipmentSegment);
                        sMovetype = Util.GetCondition(cboTransType);

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("AREAID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["AREAID"] = sToArea;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_SHIOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                        sToshop = Result.Rows[0]["SHOPID"].ToString();

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
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_EQSGID", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                        inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("MOVE_MTHD_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["PRODID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString();
                        row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["FROM_EQSGID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "EQSGID").ToString();
                        row["FROM_PROCID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString();
                        row["TO_SHOPID"] = sToshop;
                        row["TO_AREAID"] = sToArea;
                        row["TO_EQSGID"] = sToEqsg;
                        row["MOVE_ORD_QTY"] = dTotal;
                        row["MOVE_ORD_QTY2"] = dTotal2;
                        row["USERID"] = LoginInfo.USERID;
                        //row["NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                        row["NOTE"] = txtRemark.Text.ToString();
                        row["MOVE_MTHD_CODE"] = sMovetype;

                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("QMS_JUDG_VALUE", typeof(string));

                        for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["LOTID"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString();
                                row2["QMS_JUDG_VALUE"] = DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "QMS_JUDG_FLAG") == null ? null : DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "QMS_JUDG_FLAG").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_PACKLOT_AREA", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_SEND_PACKLOT_AREA", bizException.Message, bizException.ToString());
                                    Util.MessageException(bizException);
                                    return;
                                }

                                sMove_Ord_ID = bizResult.Tables["OUTDATA"].Rows[0]["MOVE_ORD_ID"].ToString();

                                #region [메일발송]
                                if (!sTotalInspectionAreaCheck())
                                {
                                    if (_QMS_AREA_YN) //품질검사 체크 동인 경우만
                                    {
                                        //품질체크
                                        if (!bQMS_Check)
                                        {
                                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").Equals("E7000")) //Pancake의 경우 이동불가
                                            {
                                                return;
                                            }
                                            else //ROLL은 품질 검사가 없거나 불합격이라도 이동가능 대신 담당자에게 해당 LIST 메일로 전송
                                            {
                                                MailSend mail = new CMM001.Class.MailSend();
                                                string sMsg = ObjectDic.Instance.GetObjectName("동간이동 QMS 검사결과 부적합 LIST");
                                                string sTitle = "동간이동(인계)-인계처리내역";
                                                string sNote = "동간이동 QMS 검사결과 부적합 LIST";

                                                string sTo = "";

                                                DataTable dtRqst = new DataTable();
                                                dtRqst.Columns.Add("AREAID", typeof(string));
                                                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));

                                                DataRow dr = dtRqst.NewRow();
                                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                                dr["COM_TYPE_CODE"] = "AREA_MOVE_QMS_MAIL";

                                                dtRqst.Rows.Add(dr);

                                                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", dtRqst);

                                                #region [인수동 메일링]
                                                searchResult.Merge(getTargetAreaMailList());
                                                #endregion

                                                if (searchResult != null && searchResult.Rows.Count > 0)
                                                {
                                                    foreach (DataRow dRow in searchResult.Rows)
                                                    {
                                                        sTo += Util.NVC(dRow["COM_CODE"]) + ";";
                                                    }

                                                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, "", sMsg, this.makeBodyApp(sTitle, sNote, DataTableConverter.Convert(dgMoveList.ItemsSource)));
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

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
                                    drCrad["TO_AREA"] = cboMoveToArea.Text;
                                    drCrad["NOTE"] = ObjectDic.Instance.GetObjectName("비고");
                                    drCrad["NOTE01"] = txtRemark.Text.ToString();
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

                                    LGC.GMES.MES.COM001.Report_Print rs = new LGC.GMES.MES.COM001.Report_Print();
                                    rs.FrameOperation = this.FrameOperation;

                                    if (rs != null)
                                    {
                                        // 태그 발행 창 화면에 띄움.
                                        object[] Parameters = new object[3];
                                        Parameters[0] = "Report_MoveInfo";
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

                                    Clear_Form();
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private DataTable getTargetAreaMailList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = Util.GetCondition(cboMoveToArea);
                dr["COM_TYPE_CODE"] = "AREA_MOVE_QMS_MAIL";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", dtRqst);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string makeBodyApp(string sTitle, string sContent, DataTable dtLot = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                sb.Append("<head>");
                sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                sb.Append("<title>Untitled Document</title>");
                sb.Append("<style>");
                sb.Append("	* {margin:0;padding:0;}");
                sb.Append("	body {font-family:Malgun Gothic, Arial, Helvetica, sans-serif;font-size:14px;line-height:1.8;color:#333333;}");
                sb.Append("	table {border-collapse:collapse;width:100%;}");
                sb.Append("	table th {background:#f5f5f5;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table td {background:#fff;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table tbody th {border-left:1px solid #e1e1e1;text-align:right;padding:6px 8px;		}");
                sb.Append("	table tbody td {text-align:left;padding:6px 8px;}");
                sb.Append("	table thead th {text-align:center;padding:3px;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;	border-bottom:1px solid #d1d1d1;}");
                sb.Append("	.hori-table table tbody td {text-align:center;padding:3px;}");
                sb.Append("	.vertical-table, .hori-table {margin-bottom:20px;}");
                sb.Append("</style>");
                sb.Append("</head>");
                sb.Append("<body>");
                sb.Append("	<div class=\"wrap\">");
                sb.Append("    	<div class=\"vertical-table\">");
                sb.Append("            <table style=\"border-top:2px solid #c8294b; max-width:720px;\">");
                sb.Append("                <tbody>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("내용") + "</th>");
                sb.Append("                        <td>" + sTitle + "</td>");
                sb.Append("                    </tr>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("사유") + "</th>");
                sb.Append("                        <td>" + sContent.Replace(Environment.NewLine, "<br>") + "&nbsp;</td>");
                sb.Append("                    </tr>                 ");
                sb.Append("                </tbody>");
                sb.Append("            </table>");
                sb.Append("        </div>");
                if (dtLot != null && dtLot.Rows.Count > 0)
                {
                    sb.Append("    <div class=\"hori-table\">");
                    sb.Append("        	<table style=\"border-top:2px solid #c8294b; max-width:720px;\" >");
                    sb.Append("            	<colgroup>");
                    sb.Append("                	<col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                </colgroup>");
                    sb.Append("                <thead>");
                    sb.Append("                	<tr>");
                    //sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("라인") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("LOTID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("수량") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품ID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품명") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("품질검사결과") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("공정명") + "</th>");
                    sb.Append("                    </tr>");
                    sb.Append("                </thead>");
                    sb.Append("                <tbody>");
                    foreach (DataRow dr in dtLot.Rows)
                    {
                        sb.Append("                	<tr>");
                        //sb.Append("                     <td>" + Util.NVC(dr["EQSGNAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["LOTID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC_NUMBER(dr["WIPQTY"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODNAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["QMS_JUDG_VALUE"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PROCID"]) + "&nbsp;</td>");
                        sb.Append("                 </tr>");
                    }
                    sb.Append("                </tbody>");
                    sb.Append("            </table>");
                    sb.Append("        </div>");
                }
                sb.Append("    </div>");
                sb.Append("</body>");
                sb.Append("</html>");


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sb.ToString();
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Print wndPopup = sender as LGC.GMES.MES.COM001.Report_Print;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                    Clear_Form();
                }
                else
                {
                    Util.AlertInfo("SFU2930");//인계처리 되었습니다. 이력카드는 발행되지 않았습니다.

                    Clear_Form();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

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
                    Init();
                }
            });
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Clear_Form();
        }

        private void Clear_Form()
        {
            Util.gridClear(dgMoveList);

            // 공정SEGMENT 초기화 추가 [2017-10-11]
            if (cboOperation != null && cboOperation.Items.Count > 0)
                cboOperation.SelectedIndex = -1;


            //rtxRemark.Document.Blocks.Clear();
            txtRemark.Text = null;
            txtLotID_Out.Text = null;
            txtLotID_Out.Focus();
            chkPrint.IsChecked = false;
        }

        #endregion

        #region 이력조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            try
            {
                Util.gridClear(dgMove_Detail);
                Util.gridClear(dgMove_Master);

                string sStart_date = Util.GetCondition(dtpDateFrom);
                string sEnd_date = Util.GetCondition(dtpDateTo);
                string sArea = Util.GetCondition(cboHistToArea, bAllNull: true);
                string sEqsg = Util.GetCondition(cboHistToEquipmentSegment, bAllNull: true);
                string sType = Util.GetCondition(cboTransType3, bAllNull: true);
                //string sPRJTNAME = txtPRJT_NAME.Text.Trim();

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
                RQSTDT.Columns.Add("MOVE_MTHD_CODE", typeof(String));
                //RQSTDT.Columns.Add("PRJT_NAME", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROM_DATE"] = sPRJTNAME != "" ? null : sStart_date;
                //dr["TO_DATE"] = sPRJTNAME != "" ? null : sEnd_date;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_AREAID"] = sArea;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat, bAllNull: true);
                dr["MOVE_MTHD_CODE"] = Util.GetCondition(cboTransType3, bAllNull: true);
                //dr["PRJT_NAME"] = sPRJTNAME == "" ? null : sPRJTNAME;
                dr["PRODID"] = txtProd_ID.Text == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove_Master, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
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
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;// - 2;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i + 2)
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgMove_Master.SelectedIndex = idx;

                SearchMoveLOTList(idx);
            }

        }

        private void SearchMoveLOTList(int idx)
        {
            try
            { 
                string sMoveOrderID = string.Empty;

                sMoveOrderID = DataTableConverter.GetValue(dgMove_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = sMoveOrderID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove_Detail, DetailResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
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
                    Parameters[0] = "Report_MoveInfo";
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

        #region EventHandler
        //이벤트 추가 2016.12.15 이슬아 
        #region [인계이력조회] - 기간 선택시 이벤트
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
        #endregion
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgMove_Master.GetRowCount() == 0 || dgMove_Detail.GetRowCount() == 0)
                {
                    Util.AlertInfo("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                //if (dgMove_Detail.GetRowCount() == 0)
                //{
                //    Util.AlertInfo("선택된 LOT이 없습니다.");
                //    return;
                //}

                //인계취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2932"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
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
                            Util.AlertInfo("SFU1939");  //취소 할 수 있는 상태가 아닙니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["MOVE_ORD_ID"] = sMoveOrderID;
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["USERID"] = LoginInfo.USERID;
                        //row["NOTE"] = new TextRange(rtxRemark2.Document.ContentStart, rtxRemark2.Document.ContentEnd).Text;
                        row["NOTE"] = rtxRemark2.Text.ToString();

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

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_PACKLOT_AREA", "INDATA,INLOT", null, indataSet);

                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                        //Util.gridClear(dgMove_Detail);
                        //Util.gridClear(dgMove_Master);

                        //Search_data();


                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_PACKLOT_AREA", "INDATA,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_CANCEL_PACKLOT_AREA", searchException.Message, searchException.ToString());
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                                Util.gridClear(dgMove_Detail);
                                Util.gridClear(dgMove_Master);

                                Search_data();
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStart_date = Util.GetCondition(dtpDateFrom);
                string sEnd_date = Util.GetCondition(dtpDateTo); string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sArea = Util.GetCondition(cboHistToArea, bAllNull: true);
                string sEqsg = Util.GetCondition(cboHistToEquipmentSegment, bAllNull: true);


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

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_AREAID"] = sArea;
                //dr["FROM_EQSGID"] = Util.NVC(cboFromEquipmentSegment.SelectedValue) == "" ? null : cboFromEquipmentSegment;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat, bAllNull: true);
                RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_EXCEL", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST", "RQSTDT", "RSLTDT", RQSTDT);


                Dictionary<string, string> dicHeader = new Dictionary<string, string>();

                dicHeader.Add("MOVE_ORD_ID", "이동ID");
                dicHeader.Add("MOVE_ORD_STAT_NAME", "상태");
                dicHeader.Add("PRODID", "제품코드");
                dicHeader.Add("PRODNAME", "제품명");
                dicHeader.Add("MODLID", "모델");
                dicHeader.Add("LOTQTY", "LOTQTY");

                dicHeader.Add("FROM_AREAID", "인계동");
                dicHeader.Add("FROM_EQSGID", "인계라인");
                dicHeader.Add("FROM_PROCID", "인계공정");
                dicHeader.Add("MOVE_ORD_QTY", "인계수량(Roll)");
                dicHeader.Add("MOVE_ORD_QTY2", "인계수량(Lane)");
                dicHeader.Add("MOVE_USERID", "인계자");
                dicHeader.Add("MOVE_STRT_DTTM", "인계시간");

                dicHeader.Add("TO_AREAID", "인수동");
                dicHeader.Add("TO_EQSGID", "인수라인");
                dicHeader.Add("TO_PROCID", "인수공정");
                dicHeader.Add("MOVE_CMPL_QTY", "인수수량(Roll)");
                dicHeader.Add("MOVE_CMPL_QTY2", "인수수량(Lane)");
                dicHeader.Add("RCPT_USERID", "인수자");
                dicHeader.Add("MOVE_END_DTTM", "인수시간");

                dicHeader.Add("NOTE", "NOTE");

                new ExcelExporter().DtToExcel(SearchResult, "동간이동", dicHeader);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        #endregion

        #region 동간이동 - CUT 단위
        private void txtCutID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sCutid = string.Empty;
                    //sCutid = txtCutID.Text.Trim().Substring(0, 9);
                    sCutid = txtCutID.Text.Trim();

                    if (sCutid == "")
                    {                        
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("CSTID", typeof(String));
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                    RQSTDT.Columns.Add("WIPHOLD", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = sCutid;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["WIPSTAT"] = "WAIT,END";
                    dr["WIPHOLD"] = "N";

                    RQSTDT.Rows.Add(dr);
                    //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE_CUT", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count < 1)
                    {
                        Util.Alert("SFU1869");  //재공 상태가 이동가능한 상태가 아닙니다.
                        return;
                    }


                    //for (int i = 0; i < SearchResult.Rows.Count; i++)
                    //{
                    //    if (!SearchResult.Rows[i]["WIPSTAT"].ToString().Equals("WAIT"))
                    //    {
                    //        Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                    //        return;
                    //    }

                    //    if (SearchResult.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                    //    {
                    //        Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                    //        return;
                    //    }

                    //    if (SearchResult.Rows[i]["RACK_ID"].ToString() != "" && SearchResult.Rows[0]["RACK_ID"].ToString() != null)
                    //    {
                    //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("창고에서 출고되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //        return;
                    //    }
                    //}

                    //for (int icnt = 0; icnt < dgCutidList.GetRowCount(); icnt++)
                    //{
                    //    if (DataTableConverter.GetValue(dgCutidList.Rows[icnt].DataItem, "CUT_ID").ToString() == sCutid)
                    //    {
                    //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동일한 CUT_ID 가 스캔되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //        return;
                    //    }
                    //}

                    #region [QMS 결과값 (2018.06.12)]]
                    string dtQmsResult = string.Empty; ; // 품질검사 결과 담아둠.                        
                    string sLotid_Target = string.Empty; // 검사대상 LOTID 임시저장

                    // 판정FLAG Column 생성
                    DataColumn newCol = new DataColumn("QMS_JUDG_FLAG", typeof(string));
                    newCol.DefaultValue = "";
                    SearchResult.Columns.Add(newCol);

                    for (int i = 0; i < SearchResult.Rows.Count; i++)
                    {
                        sLotid_Target = SearchResult.Rows[i]["LOTID"].ToString();
                        dtQmsResult = QmsResult(sLotid_Target);

                        DataRow[] dRow = SearchResult.Select("LOTID = '" + sLotid_Target + "'");

                        if (dRow.Length > 0)
                        {
                            dRow[0]["QMS_JUDG_VALUE"] = getQmsResultName(dtQmsResult);
                            dRow[0]["QMS_JUDG_FLAG"] = dtQmsResult;
                        }
                    }
                    #endregion

                    #region # 특별관리 Lot - 인계가능 라인 체크
                    if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                    {
                        string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                        string sLine = Util.GetCondition(cboEquipmentSegment3);

                        if (!SpclLine.Contains(sLine) || string.IsNullOrEmpty(sLine))
                        {
                            Init();
                            Util.MessageInfo("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]), SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                            return;
                        }
                    }
                    #endregion

                    if (dgCutidList.GetRowCount() > 1)
                    {
                        if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                        {
                            Util.Alert("SFU1789");  //이전에 스캔한 LOT 의 공정과 다릅니다.
                            return;
                        }

                        if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "CSTID").ToString() == sCutid)
                        {
                            Util.Alert("SFU2862");   //동일한 SKID ID 가 스캔되었습니다.
                            return;
                        }

                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE) Validation
                        //기존 Validation 삭제	
                        //if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "LOTTYPE").ToString() != SearchResult.Rows[0]["LOTTYPE"].ToString())
                        //{
                        //    if ((DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "LOTTYPE").ToString() == "X") || (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X"))
                        //    {
                        //        Util.Alert("SFU5149");  //시생산 LOT은 시생산 LOTTYPE 유형으로만 이동처리가 가능합니다.
                        //        return;
                        //    }
                        //}
                        #region # 특별관리 Lot - 혼입 체크 
                        if (Util.NVC(DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "SPCL_FLAG")) != Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]))
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "SPCL_FLAG")) == "Y") || (Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]) == "Y"))
                            {
                                Util.Alert("SFU8214");  //
                                return;
                            }
                        }
                        #endregion
                    }

                    if (dgCutidList.GetRowCount() == 0)
                    {
                        //dgCutidList.ItemsSource = DataTableConverter.Convert(SearchResult);
                        Util.GridSetData(dgCutidList, SearchResult, FrameOperation, true);

                        // 공정SEGMENT 추가 [2017-10-11]
                        if (SearchResult != null && SearchResult.Rows.Count > 0)
                        {
                            DataView view = new DataView(SearchResult);
                            DataTable distinctValues = view.ToTable(true, "PROCID");

                            //cboOperation.Items.Add(Util.NVC(SearchResult.Rows[0]["PROCID"]));
                            cboOperation2.ItemsSource = DataTableConverter.Convert(distinctValues);
                            cboOperation2.SelectedIndex = 0;

                            string sOriginalMoveArea = Util.NVC(cboMoveToArea3.SelectedValue);
                            string sOriginalLine = Util.NVC(cboEquipmentSegment3.SelectedValue);

                            cboMoveToArea3.SelectedIndex = 0;
                            cboMoveToArea3.SelectedValue = sOriginalMoveArea;
                            cboEquipmentSegment3.SelectedValue = sOriginalLine;
                        }
                    }
                    else
                    {
                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (!Util.NVC(DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                            Init();
                            return;
                        }

                        if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "MKT_TYPE_CODE").ToString() != SearchResult.Rows[0]["MKT_TYPE_CODE"].ToString())
                        {
                            Util.Alert("SFU4454");  //내수용과 수출용은 같이 포장할 수 없습니다.
                            return;
                        }


                        if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgCutidList.ItemsSource);
                            dtSource.Merge(SearchResult);

                            Util.gridClear(dgCutidList);
                            //dgCutidList.ItemsSource = DataTableConverter.Convert(dtSource);
                            Util.GridSetData(dgCutidList, dtSource, FrameOperation, true);
                        }
                        else
                        {
                            Util.Alert("SFU1893");  //제품ID가 같지 않습니다
                            return;
                        }
                    }

                    string[] sColumnName = new string[] { "CSTID" };
                    _Util.SetDataGridMergeExtensionCol(dgCutidList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                    txtCutID.Text = "";
                    txtCutID.Focus();

                    #region [QMS 검사 결과체크 SCAN 시 제외 [2018.06.08]]
                    //QMS 검사 결과체크 로직 추가(2018-05-28)
                    //QMS_CHECK(SearchResult);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnOut3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 동간이동 인계 (SKID ID)

                if (dgCutidList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (cboMoveToArea3.SelectedIndex < 0 || cboMoveToArea3.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                // 1. 품질검사 체크
                bool bQMS_Check;
                bQMS_Check = QMS_CHECK(DataTableConverter.Convert(dgCutidList.ItemsSource));

                #region # 특별관리 Lot - 인계가능 라인 체크
                string sLine = Util.GetCondition(cboEquipmentSegment3);

                if (CommonVerify.HasDataGridRow(dgCutidList))
                {
                    DataTable dt = ((DataView)dgCutidList.ItemsSource).Table;
                    foreach (DataRow dRow in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(dRow["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(dRow["RSV_EQSGID_LIST"])))
                        {
                            if (!Util.NVC(dRow["RSV_EQSGID_LIST"]).Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(dRow["LOTID"]), Util.NVC(dRow["RSV_EQSGID_LIST"]) });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return;
                            }
                        }
                    }
                }
                #endregion

                //인계처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2931"), !string.IsNullOrEmpty(_Message) ? _Message : null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (_QMS_AREA_YN) //품질검사 체크 동인 경우만
                        {
                            //품질체크
                            if (!bQMS_Check)
                            {
                                if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PROCID").Equals("E7000")) //Pancake의 경우 이동불가
                                {
                                    return;
                                }
                            }
                        }

                        string sToshop = string.Empty;
                        string sToArea = string.Empty;
                        string sToEqsg = string.Empty;
                        string sMovetype = string.Empty;

                        sToArea = Util.GetCondition(cboMoveToArea3);
                        sToEqsg = Util.GetCondition(cboEquipmentSegment3);
                        sMovetype = Util.GetCondition(cboTransType2);

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("AREAID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["AREAID"] = sToArea;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_SHIOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                        sToshop = Result.Rows[0]["SHOPID"].ToString();

                        decimal dSum = 0;
                        decimal dSum2 = 0;
                        decimal dTotal = 0;
                        decimal dTotal2 = 0;

                        for (int i = 0; i < dgCutidList.GetRowCount(); i++)
                        {
                            dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCutidList.Rows[i].DataItem, "WIPQTY")));
                            dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCutidList.Rows[i].DataItem, "WIPQTY2")));

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
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_EQSGID", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                        inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("MOVE_MTHD_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["PRODID"] = DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PRODID").ToString();
                        row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["FROM_EQSGID"] = DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "EQSGID").ToString();
                        row["FROM_PROCID"] = DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PROCID").ToString();
                        row["TO_SHOPID"] = sToshop;
                        row["TO_AREAID"] = sToArea;
                        row["TO_EQSGID"] = sToEqsg;
                        row["MOVE_ORD_QTY"] = dTotal;
                        row["MOVE_ORD_QTY2"] = dTotal2;
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = txtRemark.Text.ToString();
                        row["MOVE_MTHD_CODE"] = sMovetype;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("QMS_JUDG_VALUE", typeof(string));

                        for (int i = 0; i < dgCutidList.GetRowCount(); i++)
                        {
                            DataRow row2 = inLot.NewRow();
                            row2["LOTID"] = DataTableConverter.GetValue(dgCutidList.Rows[i].DataItem, "LOTID").ToString();
                            row2["QMS_JUDG_VALUE"] = DataTableConverter.GetValue(dgCutidList.Rows[i].DataItem, "QMS_JUDG_FLAG") == null ? null : DataTableConverter.GetValue(dgCutidList.Rows[i].DataItem, "QMS_JUDG_FLAG").ToString();
                            
                            indataSet.Tables["INLOT"].Rows.Add(row2);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_PACKLOT_AREA", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_SEND_PACKLOT_AREA", bizException.Message, bizException.ToString());
                                    Util.MessageException(bizException);
                                    return;
                                }

                                #region [메일발송]
                                if (_QMS_AREA_YN) //품질검사 체크 동인 경우만
                                {
                                    //품질체크
                                    if (!bQMS_Check)
                                    {
                                        if (DataTableConverter.GetValue(dgCutidList.Rows[0].DataItem, "PROCID").Equals("E7000")) //Pancake의 경우 이동불가
                                        {
                                            return;
                                        }
                                        else //ROLL은 품질 검사가 없거나 불합격이라도 이동가능 대신 담당자에게 해당 LIST 메일로 전송
                                        {
                                            MailSend mail = new CMM001.Class.MailSend();
                                            string sMsg = ObjectDic.Instance.GetObjectName("동간이동 QMS 검사결과 부적합 LIST");
                                            string sTitle = "동간이동(인계)-인계처리내역";
                                            string sNote = "동간이동 QMS 검사결과 부적합 LIST";

                                            string sTo = "";

                                            DataTable dtRqst = new DataTable();
                                            dtRqst.Columns.Add("AREAID", typeof(string));
                                            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));

                                            DataRow dr = dtRqst.NewRow();
                                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                            dr["COM_TYPE_CODE"] = "AREA_MOVE_QMS_MAIL";

                                            dtRqst.Rows.Add(dr);

                                            DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", dtRqst);

                                            #region [인수동 메일링]
                                            searchResult.Merge(getTargetAreaMailList());
                                            #endregion

                                            if (searchResult != null && searchResult.Rows.Count > 0)
                                            {
                                                foreach (DataRow dRow in searchResult.Rows)
                                                {
                                                    sTo += Util.NVC(dRow["COM_CODE"]) + ";";
                                                }

                                                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, "", sMsg, this.makeBodyApp(sTitle, sNote, DataTableConverter.Convert(dgCutidList.ItemsSource)));
                                            }
                                        }
                                    }
                                }
                                #endregion

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                Clear_Form_Cut();
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnDelete3_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgCutidList.IsReadOnly = false;
                    dgCutidList.RemoveRow(index);
                    dgCutidList.IsReadOnly = true;
                }
            });
        }

        private void btnRefresh3_Click(object sender, RoutedEventArgs e)
        {
            Clear_Form_Cut();
        }

        private void Clear_Form_Cut()
        {
            Util.gridClear(dgCutidList);

            // 공정SEGMENT 초기화 추가 [2017-10-11]
            if (cboOperation2 != null && cboOperation2.Items.Count > 0)
                cboOperation2.SelectedIndex = -1;


            //rtxRemark.Document.Blocks.Clear();
            txtRemark3.Text = null;
            txtCutID.Text = null;
            txtCutID.Focus();
        }

        #endregion

        #region 통문증 출력 [탭 추가] -2016.12.15 이슬아

        #region EventHandler

        #region [통문증 발행] - 기간 선택시 이벤트
        private void dtpDateFromForPrint_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateToForPrint.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateToForPrint_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFromForPrint.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region [통문증 발행] - 버튼 클릭 이벤트
        /// <summary>
        /// 조회 버튼 클릭시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchForPrint_Click(object sender, RoutedEventArgs e)
        {
            SearchDataForPrint();
        }
        #endregion

        private void btnDeleteForPrint_Click(object sender, RoutedEventArgs e)
        {
            List<int> list = _Util.GetDataGridCheckRowIndex(dgMove, "CHK");
            if (list.Count <= 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            dtRQSTDT.Columns.Add("MOVE_ORD_ID", typeof(string));


            foreach (int row in list)
            {
                DataRow ordRow = dtRQSTDT.NewRow();
                ordRow["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgMove.Rows[row].DataItem, "MOVE_ORD_ID");
                dtRQSTDT.Rows.Add(ordRow);
            }

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_DEL_APPRV_PASS_NO", "RQSTDT", null, dtRQSTDT);

            if (bCancel == false)
                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
            SearchDataForPrint();

            //new ClientProxy().ExecuteService("DA_PRD_DEL_APPRV_PASS_NO", "RQSTDT", null, dtRQSTDT, (searchResult, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            return;
            //        }
            //        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
            //        SearchDataForPrint();
            //    }
            //    catch (Exception ex)
            //    {
            //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);         
            //    }
            //});
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            List<int> list = _Util.GetDataGridCheckRowIndex(dgMove, "CHK");
            if (list.Count <= 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            Dictionary<string, string> valueList = new Dictionary<string, string>();
            string ORD_LIST = string.Empty;

            foreach (int row in list)
            {
                string strValue = Util.NVC(DataTableConverter.GetValue(dgMove.Rows[row].DataItem, "APPRV_PASS_NO"));
                if (!valueList.ContainsKey(strValue)) valueList.Add(strValue, row.ToString());

                ORD_LIST = ORD_LIST + Util.NVC(DataTableConverter.GetValue(dgMove.Rows[row].DataItem, "MOVE_ORD_ID"));
                if (list.Last() != row) ORD_LIST = ORD_LIST + ",";
            }
            if (valueList.Count() > 1)
            {
                if (valueList.ContainsKey(string.Empty))
                {
                    Util.Alert("SFU2928");     //이미 발행된 항목이 포함되어있습니다.
                    return;
                }
                else
                {
                    Util.Alert("SFU2902");    //상이한 통문이력을 선택하였습니다.\r\n삭제후 진행해주세요.
                    return;
                }
            }

            Print(valueList.ContainsKey(string.Empty) ? true : false, Util.NVC(valueList.Keys.First()), ORD_LIST);
        }

        private void wndPrint_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Multi wndPopup = sender as LGC.GMES.MES.COM001.Report_Multi;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    SearchDataForPrint();
                }
                else
                {
                    bCancel = true;
                    APPRV_PASS_NO_Init();
                    bCancel = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void APPRV_PASS_NO_Init()
        {
            try
            {
                if (_APPRV_PASS_NO == null || _APPRV_PASS_NO == "")
                {
                    List<int> list = _Util.GetDataGridCheckRowIndex(dgMove, "CHK");
                    if (list.Count <= 0)
                    {
                        Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                        return;
                    }

                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    dtRQSTDT.Columns.Add("MOVE_ORD_ID", typeof(string));

                    foreach (int row in list)
                    {
                        DataRow ordRow = dtRQSTDT.NewRow();
                        ordRow["MOVE_ORD_ID"] = DataTableConverter.GetValue(dgMove.Rows[row].DataItem, "MOVE_ORD_ID");
                        dtRQSTDT.Rows.Add(ordRow);
                    }

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_DEL_APPRV_PASS_NO", "RQSTDT", null, dtRQSTDT);

                    if (bCancel == false)
                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.                
                }
                SearchDataForPrint();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Method

        /// <summary>
        /// 통문증 발행용 자재내역 조회
        /// </summary>
        private void SearchDataForPrint()
        {
            try
            {
                string sStart_date = Util.GetCondition(dtpDateFromForPrint);
                string sEnd_date = Util.GetCondition(dtpDateToForPrint);
                string sArea = Util.GetCondition(cboHistToAreaForPrint, bAllNull: true);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("ADD_TYPE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_AREAID"] = sArea;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["ADD_TYPE"] = "CANCEL_MOVE";
                dr["PRODID"] = txtProd_ID_Gate.Text == "" ? null : txtProd_ID_Gate.Text;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print(bool bNew, string sAPPRV_PASS_NO, string sORD_LIST)
        {
            try
            {
                Report_Multi rs = new Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                _APPRV_PASS_NO = sAPPRV_PASS_NO;

                if (rs != null)
                { 
                    DataSet dtData = new DataSet();
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = bNew;
                    Parameters[1] = sAPPRV_PASS_NO;
                    Parameters[2] = "Report_Tag"; 
                    Parameters[3] = "Report_MTRL";
                    Parameters[4] = sORD_LIST;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(wndPrint_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #endregion

        //private void txtPRJT_NAME_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        if (txtPRJT_NAME.Text != "")
        //        {
        //            Search_data();
        //        }
        //        else
        //        {
        //            Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
        //            return;
        //        }
        //    }
        //}

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

        private void txtProd_ID_Gate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID_Gate.Text != "")
                {
                    SearchDataForPrint();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        #region 상세 이력 조회 - 신규
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

        private void dgMove_InfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;// - 2;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i + 2)
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgMove_Master.SelectedIndex = idx;
            }
        }

        private void btnReprint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMoveOrderID = string.Empty;
                string sToArea = string.Empty;
                string sNote = string.Empty;
                string sVld = string.Empty;
                string sVld_date = string.Empty;

                if (dgMove_Info.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgMove_Info, "CHK");

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


                //발행하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2873"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
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


                            for (int i = 2; i < dgMove_Info.GetRowCount()+2; i++)
                            {
                                if (sMoveOrderID == DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "MOVE_ORD_ID").ToString())
                                {
                                    DataRow drInfo = dtBasicInfo.NewRow();
                                    drInfo["LOTID"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "LOTID");
                                    drInfo["LOT"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "LOTID");
                                    drInfo["PROJECT"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "PRJT_NAME") + "-" + DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "ELECNAME");
                                    drInfo["VER"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "PROD_VER_CODE");
                                    drInfo["UNIT"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "UNIT_CODE");
                                    drInfo["QTY"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "WIPQTY");

                                    if (Util.NVC(DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "VLD_DATE")) == "")
                                    {
                                        sVld = null;
                                    }
                                    else
                                    {
                                        sVld_date = Util.NVC(DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "VLD_DATE"));
                                        sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                                    }

                                    drInfo["DATE"] = sVld;
                                    drInfo["HOLD"] = DataTableConverter.GetValue(dgMove_Info.Rows[i].DataItem, "WIPHOLD");

                                    dtBasicInfo.Rows.Add(drInfo);
                                }
                            }

                            LGC.GMES.MES.COM001.Report_Print rs = new LGC.GMES.MES.COM001.Report_Print();
                            rs.FrameOperation = this.FrameOperation;

                            if (rs != null)
                            {
                                // 태그 발행 창 화면에 띄움.
                                object[] Parameters = new object[3];
                                Parameters[0] = "Report_MoveInfo";
                                Parameters[1] = dtPackingCard;
                                Parameters[2] = dtBasicInfo;

                                C1WindowExtension.SetParameters(rs, Parameters);

                                rs.Closed += new EventHandler(RePrint_Result);
                                this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.AlertInfo(ex.ToString());
                            return;
                        }
                    }
                });               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReturn2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Search_MoveInfo();
        }

        private void Search_MoveInfo()
        {
            try
            {
                Util.gridClear(dgMove_Info);

                string sStart_date = Util.GetCondition(dtpDateFrom2);
                string sEnd_date = Util.GetCondition(dtpDateTo2);
                string sArea = Util.GetCondition(cboHistToArea2, bAllNull: true);
                string sEqsg = Util.GetCondition(cboHistToEquipmentSegment2, bAllNull: true);
                string sType = Util.GetCondition(cboTransType4, bAllNull: true);
                //string sPRJTNAME = txtPRJT_NAME.Text.Trim();

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
                RQSTDT.Columns.Add("MOVE_MTHD_CODE", typeof(String));
                //RQSTDT.Columns.Add("PRJT_NAME", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROM_DATE"] = sPRJTNAME != "" ? null : sStart_date;
                //dr["TO_DATE"] = sPRJTNAME != "" ? null : sEnd_date;
                dr["FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                dr["TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_AREAID"] = sArea;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat2, bAllNull: true);
                dr["MOVE_MTHD_CODE"] = Util.GetCondition(cboTransType4, bAllNull: true);
                //dr["PRJT_NAME"] = sPRJTNAME == "" ? null : sPRJTNAME;
                dr["PRODID"] = txtProd_ID2.Text == "" ? null : txtProd_ID2.Text;
                dr["LOTID"] = txtLotID2.Text == "" ? null : txtLotID2.Text;

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

                            string[] sColumnName = new string[] { "MOVE_ORD_ID" };
                            _Util.SetDataGridMergeExtensionCol(dgMove_Info, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

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

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    DataGridAggregateCount dgcount = new DataGridAggregateCount();
                    dagsum.ResultTemplate = dgMove_Info.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    daq.Add(dgcount);
                    DataGridAggregate.SetAggregateFunctions(dgMove_Info.Columns["LOTID"], daq);
                    DataGridAggregate.SetAggregateFunctions(dgMove_Info.Columns["WIPQTY"], dac);
                    DataGridAggregate.SetAggregateFunctions(dgMove_Info.Columns["WIPQTY2"], dac);

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

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
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

        private void dgMove_Info_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgMove_Info.GetRowCount() <= 0)
                {
                    return;
                }
                int x = 2;
                int x1 = 2;
                for (int i = x1; i < dgMove_Info.GetRowCount()+2; i++)
                {
                    if (Util.NVC(dgMove_Info.GetCell(x, dgMove_Info.Columns["MOVE_ORD_ID"].Index).Value) == Util.NVC(dgMove_Info.GetCell(i, dgMove_Info.Columns["MOVE_ORD_ID"].Index).Value))
                    {
                        x1 = i;
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dgMove_Info.GetCell((int)x, (int)0), dgMove_Info.GetCell((int)x1, (int)0)));

                        x = x1 + 1;
                        i = x1;
                    }
                }
                e.Merge(new DataGridCellsRange(dgMove_Info.GetCell((int)x, (int)0), dgMove_Info.GetCell((int)x1, (int)0)));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

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
        
         private bool IsRollAvailableArea(string sArea) //점보롤 코드중에서 동간이동 가능한 동 찾기
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sArea;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PROD_EXCEPTION_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }

        private bool IsRollCodeChange(string sProdID) //점보롤 코드로는 동간이동이 되지 않도록
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_PROD_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void SetProcessEquipmentSegment(C1ComboBox combo, string procID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboMoveToArea.SelectedValue);
                dr["PROCID"] = procID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    //combo.Items.Clear();
                    combo.ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch (Exception ex) { }
        }
        private void chkMultiInput_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if ((bool)chk.IsChecked)
            {
                _Multi_Input_YN = true;
                chkRFID.IsChecked = false;
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
        private void chkSkidID_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if((bool)chk.IsChecked)
            {
                chkRFID.IsChecked = false;
                chkBoxID.IsChecked = false;
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("SKID ID");
            }           
        }

        private void chkSkidID_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (!(bool)chk.IsChecked)
            {
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("Lot ID (팔레트 ID)");
            }   
        }

        private void chkRFID_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if ((bool)chk.IsChecked)
            {
                chkSkidID.IsChecked = false;
                chkBoxID.IsChecked = false;
                chkMultiInput.IsChecked = false;
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("Carrier ID");
            }
        }

        private void chkRFID_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (!(bool)chk.IsChecked)
            {
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("Lot ID (팔레트 ID)");
            }
        }

        private void dgMoveList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if(!_QMS_AREA_YN)
            {
                return;
            }
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                int row_idx = e.Cell.Row.Index;              
                var qms_vlaue = DataTableConverter.GetValue(dataGrid.Rows[row_idx].DataItem, "QMS_JUDG_FLAG");

                if(qms_vlaue != null)
                {               
                    if(qms_vlaue.ToString() != "P")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }  
                }           
            }));
        }

        private void chkBoxID_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if ((bool)chk.IsChecked)
            {
                chkSkidID.IsChecked = false;
                chkRFID.IsChecked = false;
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("BOX ID");
            }
        }

        private void chkBoxID_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (!(bool)chk.IsChecked)
            {
                tbLotID_Out.Text = ObjectDic.Instance.GetObjectName("Lot ID (팔레트 ID)");
            }
        }
    }
}
