/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.05.09  정문교      폴란드3동 대응 Carrier ID(CSTID) 조회조건 및 조회 칼럼 추가
                          BOXID를 Carrier ID로 변경

  2020.05.10  오화백      LOTID, Carrier ID 입력 후 조회시 데이터 조회가 안되는 경우 해당 입력 데이터 유지
  2020.06.08  이현호      C20200128-000453 Carrier ID입력 시 재공이 없을 경우 최근 장착되었던 LOT ID를 조회하도록 변경.
  2020.06.18  정문교      C20200610-000491 - 등급 정보 칼럼 추가 
  2020.08.09  오화백      폴란드 3동 조립일 경우 작업유형 나오도록 추가
  2020.11.06  오화백      폴란드 3동 코터호기 나오도록 추가
  2021.10.14  조영대      무지부/권취방향 컬럼 추가
  2022.03.23  장기훈      C20211202-000498 - 유효기간 컬럼추가  ===> JUDG_NAME, REV_NO 컬럼으로 인하여 배포 오류 소스 롤백 후 재작업
  2022.03.23  서용호      슬라이딩 측정값 컬럼 추가
  2022.05.16  방효식      오창 조립 JUDG_NAME, REV_NO 컬럼 추가
  2022.05.19  오화백      슬라이딩 측정값을 TAB, BOTTOM 으로 표기, 폴란드 공장에서만 보이도록 공통코드로 관리(SLID_MATC_GR_AREA)
  2023.01.13  유재홍      슬리팅 레인 번호(CHILD_GR_SEQNO) 컬럼 추가
  2023.02.17  윤기업      ROLLMAP 버튼 추가
  2023.02.28  신광희      ROLLMAP 설비 조회 BizRule(DA_PRD_SEL_LOT_STATUS2) 수정에 따른 바인딩 변수 수정
  2023.04.24  양영재       E20230228-000400 QA_SAMPLE_STATUS 칼럼 추가
  2023.05.11  신광희      조회결과 컬럼 추가(두께THICKNESS_VALUE, 폭WIDTH_VALUE)
  2023.05.27  강성묵      ESHM 불필요 데이터 제거 (Lot 정보 : 무지부/권취방향 중복)
  2023.06.29  신광희      롤맵 버튼 LOT 단위 ID 별 분기 처리
  2023.07.05  이주홍      차단 유형이 포장 출고, PLANT 이동, 공정 이동인 QMS 검사 결과 컬럼 추가
  2023.11.01  정재홍    : [E20231018-000975] - GMES LOT정보 조회시 재와인딩 공정 추가
  2024.08.13  김도형      [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
  2025.02.18  안민호     변경이력 탭 컬럼명 생산량 -> 재공량/생산량 으로 변경  
  2025.03.10  이민형      HD_OSS_0058 [MES2.0팀] Grid 컬럼 순서 변경. 
  2025.03.10  이민형      HD_OSS_0060 [MES2.0팀] Lotid, Carrierid 소문자 입력 시 대문자로 변환.
  2025.03.11  이민형      HD_OSS_0060 [MES2.0팀] Grid 현재 Lot상태 다국어 적용을 위한 header 변경 -> LOTSTAT
  2025.03.20  이동주      E20240904-000991  [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)
  2025.04.01  조성근     [E20250324-000244] 롤프레스 롤맵 수불 개선건( 롤맵실적조회버튼 추가 )
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_016 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREATYPE = "";

        // Roll Map
        private bool _isRollMapLot;

        private string _lotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _processCode = string.Empty;
        private string _laneQty = string.Empty;
        private string _wipStat = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _prod_Ver_Code = string.Empty;
        private string _lastProcId = string.Empty;


        public COM001_016()
        {

            InitializeComponent();

        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            txtLotId.Focus();

            object[] parameters = this.FrameOperation.Parameters;

            //2019.12.20 해당 SEQ의 투입위치를 보여주는 칼럼 추가 하였음. 허나 임시로 A8, S4만 적용되도록 하였음.
            if (LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("S4"))
            {
                dgHistory.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Visible;
            }

            if (parameters.Length > 0)
            {
                txtLotId.Text = Util.NVC(parameters[0]);

                if (!string.IsNullOrEmpty(txtLotId.Text))
                    btnSearch_Click(btnSearch, null);
            }
            if (string.Equals(GetAreaType(), "A"))
            {
                setGridForAssay();
            }
        }

        private void setGridForAssay()
        {
            try
            {
                int i = 1;
                dgHistory.Columns["LOTID"].DisplayIndex = i++;
                dgHistory.Columns["CARRIERID"].DisplayIndex = i++;
                dgHistory.Columns["OLDPROCNAME"].DisplayIndex = i++;
                dgHistory.Columns["EQPTID"].DisplayIndex = i++;
                dgHistory.Columns["EQPTNAME"].DisplayIndex = i++;
                dgHistory.Columns["ACTNAME"].DisplayIndex = i++;
                dgHistory.Columns["PRODUCT_QTY"].DisplayIndex = i++;
                dgHistory.Columns["WIPQTY_ED"].DisplayIndex = i++;
                dgHistory.Columns["CNFM_DFCT_QTY"].DisplayIndex = i++;
                dgHistory.Columns["CNFM_LOSS_QTY"].DisplayIndex = i++;
                dgHistory.Columns["CNFM_PRDT_REQ_QTY"].DisplayIndex = i++;
                dgHistory.Columns["ACTDTTM"].DisplayIndex = i++;
                dgHistory.Columns["PROCNAME"].DisplayIndex = i++;
                dgHistory.Columns["WIPSNAME"].DisplayIndex = i++;
                dgHistory.Columns["PROD_LOTID"].DisplayIndex = i++;
                dgHistory.Columns["TO_LOTID"].DisplayIndex = i++;
                dgHistory.Columns["MERGE_FROM_LOTID_LIST"].DisplayIndex = i++;
                dgHistory.Columns["WIPSEQ"].DisplayIndex = i++;
                dgHistory.Columns["EQPT_MOUNT_PSTN_NAME"].DisplayIndex = i++;
                dgHistory.Columns["UPDUSER"].DisplayIndex = i++;
                dgHistory.Columns["WIPNOTE"].DisplayIndex = i++;
                dgHistory.Columns["QA_INSP_JUDG_VALUE"].DisplayIndex = i++;
                dgHistory.Columns["QA_INSP_JUDG_VALUE_NAME"].DisplayIndex = i++;

            }
            catch { }
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sID = Util.GetCondition(txtLotId);
            string sID2 = Util.GetCondition(txtBoxid);
            if (sID == "" && sID2 == "")
            {
                Util.MessageInfo("SFU1009");
                return;
            }
            if (sID != "")
            {
                GetLotAll("LOT");
            }
            else
            {
                GetLotAll("BOX");
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text.Trim() != "")
                        GetLotAll("LOT");
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rbCheck_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            // 2024.12.16. 김영국 - 화면 조회 2번을 하는것을 방지하기 위해 해당 로직 주석처리함. >> 2025.01.14 이지은 기존 기능 유지 필요하여 주석 제거
            GetLotInfo((rb.DataContext as DataRowView).Row["LOTID"].ToString(), (rb.DataContext as DataRowView).Row["PROCID"].ToString());
            GetHistory((rb.DataContext as DataRowView).Row["LOTID"].ToString());
            ChkRollMapLotAttribute((rb.DataContext as DataRowView).Row["LOTID"].ToString());
            //2025.01.14

        }
        #endregion

        #region Mehod
        public void GetLotAll(string sType)
        {
            try
            {
                string sLOTID = string.Empty;
                if (!string.IsNullOrWhiteSpace(txtLotId.Text)) txtLotId.Text = txtLotId.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(txtBoxid.Text)) txtBoxid.Text = txtBoxid.Text.ToUpper();

                if (sType == "BOX")
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("CSTID", typeof(String));

                    DataRow dr2 = RQSTDT.NewRow();
                    dr2["CSTID"] = Util.GetCondition(txtBoxid);

                    RQSTDT.Rows.Add(dr2);
                    //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WIPATTR_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_CURR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count != 0)
                    {
                        sLOTID = SearchResult.Rows[0]["CURR_LOTID"].ToString();
                    }
                    else
                    {
                        //2020.06.08  이현호 C20200128-000453 Carrier ID입력 시 재공이 없을 경우 최근 장착되었던 LOT ID를 조회하도록 변경. 
                        if (sType == "BOX")
                        {
                            DataTable SearchResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WIPHISTORYATTR_LOT", "RQSTDT", "RSLTDT", RQSTDT);
                            if (SearchResult2.Rows.Count != 0)
                            {
                                sLOTID = SearchResult2.Rows[0]["CURR_LOTID"].ToString();
                            }
                            else
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtBoxid.Focus();
                                        Init();
                                        return;
                                    }
                                });
                            }
                        }
                        else
                        {
                            //재공 정보가 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtLotId.Focus();
                                    Init();
                                    return;
                                }
                            });
                        }
                    }
                }
                else
                {
                    sLOTID = Util.GetCondition(txtLotId);
                }

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                dr["GUBUN"] = (bool)rdoForward.IsChecked ? "F" : "R";

                dtRqst.Rows.Add(dr);

                //WIPACTHISTORY, 

                // CSR : [E20231018-000975] - 전극 재와인딩 공정 추가 Biz 분리 처리 [동별 공통코드 : REWINDING_LOT_INFO_TREE]
                string sBizName = string.Empty;
                bool iElecVL = IsAreaCommonCodeUse("REWINDING_LOT_INFO_TREE", null);

                if (iElecVL)
                    sBizName = "BR_PRD_SEL_LOT_INFO_END_ELEC_LV";
                else
                    sBizName = "BR_PRD_SEL_LOT_INFO_END";

                /////////////////////////////////////////////////////////////////////
                //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_LOT_INFO_END", "INDATA", "LOTSTATUS,TREEDATA", inData);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "LOTSTATUS,TREEDATA", inData);

                Util.gridClear(dgLotInfo);

                //가로 세로로
                //   DataTable dtLotStatus = GetDataLot(dsRslt.Tables["LOTSTATUS"]);
                //  dgLotInfo.ItemsSource = DataTableConverter.Convert(dtLotStatus);

                Util.gridClear(dgHistory);
                // dgHistory.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["WIPACTHISTORY"]);

                if (iElecVL)
                    dsRslt.Relations.Add("Relations", dsRslt.Tables["TREEDATA"].Columns["LEVEL1"], dsRslt.Tables["TREEDATA"].Columns["LEVEL2"], false);
                else
                    dsRslt.Relations.Add("Relations", dsRslt.Tables["TREEDATA"].Columns["LOTID"], dsRslt.Tables["TREEDATA"].Columns["FROM_LOTID"], false);

                ///////////////////////////////////////////////////////////////

                DataView dvRootNodes;
                dvRootNodes = dsRslt.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_LOTID IS NULL";

                trvData.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

                //GetLotInfo(txtLotId.Text); // 검색한 LOT정보 보여주기
                //GetHistory(txtLotId.Text);
                _AREATYPE = string.Empty;

                if (dsRslt != null && dsRslt.Tables["LOTSTATUS"].Rows.Count > 0)
                {
                    if (dsRslt.Tables["TREEDATA"].Rows.Count > 0)
                        _AREATYPE = dsRslt.Tables["TREEDATA"].Rows[0]["AREATYPE"].ToString();

                    GetLotInfo(sLOTID, dsRslt.Tables["LOTSTATUS"].Rows[0]["PROCID"].ToString());
                    GetHistory(sLOTID);
                    ChkRollMapLotAttribute(sLOTID);
                }

                txtBoxid.Text = "";
                txtLotId.Text = "";

                if (sType == "BOX")
                {
                    txtBoxid.Focus();
                }
                else
                {
                    txtLotId.Focus();
                    SelectFirstRadionButtonInTreeView();
                }


                //GetSanKey(dsRslt.Tables["SANKEYDATA"]);
            }
            catch (Exception ex)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Init();

                        if (sType == "BOX")
                        {
                            txtBoxid.Focus();
                        }
                        else
                        {
                            txtLotId.Focus();
                        }
                    }
                });
                return;
                //Util.MessageException(ex);
                //Init();
            }
        }

        private void Init()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgHistory);
            trvData.ItemsSource = null;

            btnRollMap.Visibility = Visibility.Collapsed;
            btnRollMap.IsEnabled = false;
            btnRollMapUpdate.Visibility = Visibility.Collapsed;
            btnRollMapUpdate.IsEnabled = false;

            _lotId = string.Empty;
            _wipSeq = string.Empty;
            _processCode = string.Empty;
            _laneQty = string.Empty;
            _wipStat = string.Empty;
            _equipmentCode = string.Empty;
            _equipmentName = string.Empty;
            _equipmentSegmentCode = string.Empty;
            _prod_Ver_Code = string.Empty;

            _lastProcId = string.Empty;

        }

        public void GetLotInfo(string sLot, string sProcID)
        {
            try
            {
                string Stap = string.Empty;
                string sBizName = _AREATYPE.Equals("A") ? "DA_PRD_SEL_LOT_STATUS2_ASSY" : "DA_PRD_SEL_LOT_STATUS2";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProcID;

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STATUS2", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgLotInfo);

                // TAB 에표시
                //C1TabItem TabItem = TabLotControl.Items[0] as C1TabItem;
                //if (dtResult.Rows.Count != 0 && !dtResult.Rows[0]["WH_NAME"].ToString().Equals("") || !dtResult.Rows[0]["RACK_ID"].ToString().Equals(""))
                //{
                //    if (!dtResult.Rows[0]["WH_NAME"].ToString().Equals(""))
                //        Stap = dtResult.Rows[0]["WH_NAME"].ToString();
                //    if (!dtResult.Rows[0]["WH_NAME"].ToString().Equals("")&& !dtResult.Rows[0]["RACK_ID"].ToString().Equals(""))
                //        Stap += " , ";
                //    if (!dtResult.Rows[0]["RACK_ID"].ToString().Equals(""))
                //        Stap += dtResult.Rows[0]["RACK_ID"].ToString();
                //    TabItem.Header = Stap;
                //}
                //else
                //{
                //    TabItem.Header = ObjectDic.Instance.GetObjectName("정보");
                //}
                //가로 세로로
                DataTable dtLotStatus = GetDataLot(dtResult, sProcID);    // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정) GetDataLot(dtResult)->GetDataLot(dtResult,sProcID)
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtLotStatus);

                // dtLotStatus Validation 추가
                if (!_AREATYPE.Equals("A") && dtLotStatus != null && dtLotStatus.Rows.Count > 0)
                {
                    _lotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    _wipSeq = Util.NVC(dtResult.Rows[0]["WIPSEQ"].GetString());
                    _processCode = Util.NVC(dtResult.Rows[0]["PROCID"]);
                    _laneQty = Util.NVC(dtResult.Rows[0]["LANE_QTY"]);
                    _wipStat = Util.NVC(dtResult.Rows[0]["WIPSTAT"]);
                    _equipmentCode = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTID"]);
                    _equipmentName = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTNAME"]);
                    _equipmentSegmentCode = Util.NVC(dtResult.Rows[0]["EQSGID"]);
                    _prod_Ver_Code = Util.NVC(dtResult.Rows[0]["PROD_VER_CODE"]);

                    _lastProcId = Util.NVC(dtResult.Rows[0]["LAST_PROC_ID"]);

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHistory(string sLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPACTHISTORY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgHistory);
                dgHistory.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [동별 전극 등급 관리 여부 정보]  
        private bool EltrGrdCodeColumnVisible()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
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
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion


        private DataTable GetDataLot(DataTable dataTable, string sProcID)
        {
            DataTable dtReturn = new DataTable();
            dtReturn.Columns.Add("ITEM", typeof(string));
            dtReturn.Columns.Add("DATA", typeof(string));

            if (dataTable.Rows.Count > 0)
            {
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("LOTID"), dataTable.Rows[0]["LOTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("MODELID"), dataTable.Rows[0]["MODLID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("제품ID"), dataTable.Rows[0]["PRODID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("제품명"), dataTable.Rows[0]["PRODNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("프로젝트명"), dataTable.Rows[0]["PRJT_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("대Lot"), dataTable.Rows[0]["LOTID_RT"]);
                if (_AREATYPE.Equals("A"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("부모LOTID"), dataTable.Rows[0]["PR_LOTID"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("LOT유형"), dataTable.Rows[0]["LOTTYPE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("시장유형"), dataTable.Rows[0]["MKT_TYPE_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("수량(Roll)"), Util.NVC_NUMBER(dataTable.Rows[0]["WIPQTY"]));
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("수량(Lane)"), Util.NVC_NUMBER(dataTable.Rows[0]["WIPQTY2"]));
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("HOLD"), dataTable.Rows[0]["WIPHOLD"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("QA sample"), dataTable.Rows[0]["QA_SAMPLE_STATUS"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("QMS 판정결과"), dataTable.Rows[0]["JUDG_NAME"]);
                // 2023.07.05 이주홍 차단 유형이 포장 출고, PLANT 이동, 공정 이동인 QMS 검사 결과 컬럼 추가 (조립일 때만)
                if (_AREATYPE.Equals("A"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("BLOCK RESULT (SHIP_PRODUCT)"), dataTable.Rows[0]["SHIP_PRODUCT_BLOCK_RSLT"]);
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("BLOCK RESULT (MOVE_SHOP)"), dataTable.Rows[0]["MOVE_SHOP_BLOCK_RSLT"]);
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("BLOCK RESULT (MOVE_PROC)"), dataTable.Rows[0]["MOVE_PROC_BLOCK_RSLT"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("전극버전"), dataTable.Rows[0]["PROD_VER_CODE"]);

                if (_AREATYPE.Equals("A"))
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("조립버전"), dataTable.Rows[0]["ASSY_PROD_VER_CODE"]);

                // 2024.11.18   이동주  E20240904-000991  [MES팀] 모델 버전별 반제품 / 설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건
                if (dataTable.Columns.Contains("CP_VER_CODE"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("CP_VERSION"), dataTable.Rows[0]["CP_VER_CODE"]);
                }

                //dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("슬라이딩 측정값"), dataTable.Rows[0]["SLID_MEASR_VALUE_GRD"]);       //Sliding 등급시스템 추가 : 서용호
                // 2022 05 19 오화백 공통코드에서 슬라이딩 관리동인지 확인 후 TAB, BOTTOM 값 추가
                if (ChkSlid_Matc_Area())
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("슬라이딩측정값_"), dataTable.Rows[0]["SLID_MEASR_VALUE_GRD"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("슬리팅 레인 번호"), dataTable.Rows[0]["CHILD_GR_SEQNO"]);


                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("SHOP"), dataTable.Rows[0]["SHOPNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("유효기간"), dataTable.Rows[0]["VLD_DATE"].ToString() == "" ? "" : DateTime.ParseExact(dataTable.Rows[0]["VLD_DATE"].ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd")); // 장기훈 : C20211202-000498 => 유효기간 컬럼추가
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("동"), dataTable.Rows[0]["AREANAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("라인"), dataTable.Rows[0]["EQSGNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("설비"), dataTable.Rows[0]["EQPTNAME"]);
                if (LoginInfo.CFG_AREA_ID == "A7")
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("작업유형"), dataTable.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                }
                if (dataTable.Rows[0]["PROCID"].ToString().Equals(Process.NOTCHING))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("NG Tag 개수"), dataTable.Rows[0]["NG_TAG_QTY"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("최종공정"), dataTable.Rows[0]["LAST_PROC"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("최종상태"), dataTable.Rows[0]["LAST_STATE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("창고명"), dataTable.Rows[0]["WH_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("RACK ID"), dataTable.Rows[0]["RACK_ID"]);
                //dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("SKID ID"), dataTable.Rows[0]["CSTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("SKID ID"), dataTable.Rows[0]["SKID"]);
                //코팅라인 추가
                if (LoginInfo.CFG_AREA_ID == "A7")
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Coating_LIne"), dataTable.Rows[0]["COATING_LINE"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(Plant)"), dataTable.Rows[0]["LAST_SHOP"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(동)"), dataTable.Rows[0]["LAST_AREA"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(Line)"), dataTable.Rows[0]["LAST_EQSG"]);
                //dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("BOXID"), dataTable.Rows[0]["CSTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Carrier ID"), dataTable.Rows[0]["CSTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("MCS_SKIDID"), dataTable.Rows[0]["MCS_SKIDID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("노칭위치"), dataTable.Rows[0]["NOTCH_OUT_LOT_PSTN"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Lami셀 LEVEL3 코드"), dataTable.Rows[0]["BICELL_LEVEL3_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("셀상세분류코드"), dataTable.Rows[0]["CELL_DETL_CLSS_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("포트명"), dataTable.Rows[0]["PORT_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("포트 설비명"), dataTable.Rows[0]["PORT_EQPTNAME"]);

                if (dataTable.Columns.Contains("PRV_CSTID"))
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("PRV_CARRIERID"), dataTable.Rows[0]["PRV_CSTID"]);

                if (EltrGrdCodeColumnVisible())
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("등급"), dataTable.Rows[0]["ELTR_GRD_CODE"]);

                //GQMS INTERLOCK Revision No 추가
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("REV_NO"), dataTable.Rows[0]["REV_NO"]);

                // 무지부/권취방향
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("NON_COATED_WINDING_DIRCTN"), dataTable.Rows[0]["SLIT_SIDE_NAME"]);

                // 2023.05.27 강성묵 ESHM 불필요 데이터 제거 (Lot 정보 : 무지부/권취방향 중복)
                //dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("NON_COATED_WINDING_DIRCTN"), dataTable.Rows[0]["CHILD_GR_SEQNO"]);

                if (IsElectrodeGradeInfo())
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("두께"), dataTable.Rows[0]["THICKNESS_VALUE"]);
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("폭"), dataTable.Rows[0]["WIDTH_VALUE"]);
                }

                // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
                string sLotInfoWeightView = GetLotInfoWeightViewYn(sProcID);
                if (sLotInfoWeightView.Equals("Y"))
                {
                    string sLotGrossWeight = GetsLotGrossWeight(dataTable.Rows[0]["LOTID"].ToString(), sProcID);
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("무게(Gross Weight)"), sLotGrossWeight);
                }
            }
            return dtReturn;
        }

        // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
        private string GetLotInfoWeightViewYn(string sProcID)
        {

            string sLotInfoWeightView = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_PROC_LOT_INFO_WEIGHT_VIEW";  // 전극 LOT정보 무게 보기
            sCmCode = sProcID;                            // 공정               

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
                    sLotInfoWeightView = "Y";
                }
                else
                {
                    sLotInfoWeightView = "N";
                }

                return sLotInfoWeightView;
            }
            catch (Exception ex)
            {
                sLotInfoWeightView = "N";
                //Util.MessageException(ex);
                return sLotInfoWeightView;
            }
        }

        // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
        private string GetsLotGrossWeight(string sLotid, string sProcID)
        {

            string sLotGrossWeight = string.Empty;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = sProcID;
                Indata["LOTID"] = sLotid;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_GROSS_WEIGHT_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sLotGrossWeight = Util.NVC(dtResult.Rows[0]["GROSS_WEIGHT_UNIT_INCLUDE"].ToString());
                }
                else
                {
                    sLotGrossWeight = "";
                }
            }
            catch (Exception ex)
            {
                sLotGrossWeight = "";
                return sLotGrossWeight;
            }

            return sLotGrossWeight;
        }


        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }
        #endregion

        private void trvData_ItemExpanded(object sender, SourcedEventArgs e)
        {
            rb_Checked();
        }

        private void rb_Checked()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNode(item);
            }
        }

        public void TreeItemExpandNode(C1TreeViewItem item)
        {

            item.IsExpanded = true;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem childItem in items)
            {
                if ((childItem.DataContext as DataRowView).Row.ItemArray[0].Equals(txtLotId.Text))
                {
                    childItem.IsSelected = true;
                }
                TreeItemExpandNode(childItem);
            }

        }

        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxid.Text.Trim() != "")
                        GetLotAll("BOX");
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotId.SelectAll();
        }

        private void txtBoxid_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBoxid.SelectAll();
        }
        /// <summary>
        ///  2022 05 19 오화백 공통코드에서 슬라이딩 관리동인지 확인
        /// </summary>
        private bool ChkSlid_Matc_Area()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "SLID_MATC_GR_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {
            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName;

            string processCode;
            string equipmentCode;
            string equipmentName;
            string lotCode;
            string wipSeq;
            string lotUnitCode = string.Empty;

            if (string.IsNullOrEmpty(_lotId))
            {
                btnRollMap.IsEnabled = false;
                return;
            }

            // LOT 단위 ID -> EA : 원통형 롤맵 UI 팝업 호출 , M 기존 롤맵 UI 팝업 호출
            lotUnitCode = GetLotUnitCode();


            if (string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                //LAST_PROC_ID 가 _processCode
                if (string.Equals(Util.NVC(_wipStat), Wip_State.PROC) && _lastProcId.Equals(_processCode))
                {
                    mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_COATER" : "COM001_RM_CHART_CT";
                    lotCode = string.Empty;
                    wipSeq = "1";
                    processCode = Process.COATING;
                    equipmentName = string.Empty;

                    DataTable dtRollMapInputLot = GetRollMapInputLotCode(Util.NVC(_lotId));

                    if (CommonVerify.HasTableRow(dtRollMapInputLot))
                    {
                        lotCode = dtRollMapInputLot.Rows[0]["INPUT_LOTID"].GetString();

                        DataTable dtEquipment = GetEquipmentCode(lotCode, wipSeq);

                        if (CommonVerify.HasTableRow(dtEquipment))
                        {
                            equipmentCode = dtEquipment.Rows[0]["EQPTID"].GetString();
                        }
                        else
                        {
                            equipmentCode = string.Empty;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //mainFormName = "COM001_ROLLMAP_ROLLPRESS_NEW";
                    mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_ROLLPRESS" : "COM001_RM_CHART_RP";
                    lotCode = Util.NVC(_lotId);
                    wipSeq = Util.NVC(_wipSeq);
                    processCode = _processCode;
                    equipmentCode = _equipmentCode;
                    equipmentName = _equipmentName + " [" + _equipmentCode + "]";

                }
            }
            else if (string.Equals(_processCode, Process.COATING))
            {
                //mainFormName = "COM001_ROLLMAP_COATER";
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_COATER" : "COM001_RM_CHART_CT";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }
            else if (string.Equals(_processCode, Process.SLIT_REWINDING) || string.Equals(_processCode, Process.REWINDING))
            {
                mainFormName = "COM001_RM_CHART_RW";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }
            else if (string.Equals(_processCode, Process.SLITTING))
            {
                //mainFormName = "COM001_ROLLMAP_SLITTING";
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_SLITTING" : "COM001_RM_CHART_SL";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }
            else if (string.Equals(_processCode, Process.HALF_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_MOBILE_HALFSLITTING";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }
            else if (string.Equals(_processCode, Process.TWO_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_TWOSLITTING";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }
            else
            {
                mainFormName = "COM001_ROLLMAP_COMMON";
                lotCode = Util.NVC(_lotId);
                wipSeq = Util.NVC(_wipSeq);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName + " [" + _equipmentCode + "]";
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] Parameters = new object[10];
            Parameters[0] = processCode;
            Parameters[1] = _equipmentSegmentCode;
            Parameters[2] = equipmentCode;
            Parameters[3] = lotCode;
            Parameters[4] = wipSeq;
            Parameters[5] = _laneQty;
            Parameters[6] = equipmentName;
            Parameters[7] = _prod_Ver_Code;

            C1Window popupRollMap = obj as C1Window;
            popupRollMap.Closed += new EventHandler(PopupRollMap_Closed);
            C1WindowExtension.SetParameters(popupRollMap, Parameters);
            if (popupRollMap != null)
            {
                popupRollMap.ShowModal();
                popupRollMap.CenterOnScreen();
            }
        }
        private void PopupRollMap_Closed(object sender, EventArgs e)
        {
            //
        }

        private void ChkRollMapLotAttribute(string sLotID)
        {
            try
            {
                btnRollMap.Visibility = Visibility.Collapsed;
                btnRollMap.IsEnabled = false;

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = sLotID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                {
                    if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                    {
                        btnRollMap.Visibility = Visibility.Visible;
                        btnRollMap.IsEnabled = true;
                    }
                }

                btnRollMapUpdate.Visibility = Visibility.Collapsed;

                if (btnRollMap.Visibility == Visibility.Visible && IsRollMapResultApply() == true)
                {
                    btnRollMapUpdate.Visibility = Visibility.Visible;
                    btnRollMapUpdate.IsEnabled = true;
                }
                return;

            }
            catch (Exception)
            {

            }
            return;
        }

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _processCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null & dtResult.Rows.Count > 0)
                    if (string.Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private DataTable GetRollMapInputLotCode(string lotId)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = _wipSeq;
            dr["EQPT_MEASR_PSTN_ID"] = "UW";
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_COLLECT_INFO", "RQSTDT", "RSLTDT", inTable);
        }
        private DataTable GetEquipmentCode(string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            //dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);
        }

        private bool IsElectrodeGradeInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "GRD_JUDG_DISP_AREA";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }

        private string GetLotUnitCode()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_VW_LOT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _lotId;
                inTable.Rows.Add(dr);
                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    return result.Rows[0]["LOTUID"].GetString();
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName == string.Empty ? null : sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SelectFirstRadionButtonInTreeView()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                foreach (RadioButton rdoButton in Util.FindVisualChildren<RadioButton>(trvData))
                {
                    if (rdoButton != null)
                    {
                        rdoButton.IsChecked = true;
                        break;
                    }
                }
            }));
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }

        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {// Roll Map 호출 

                if (string.IsNullOrEmpty(_lotId))
                {
                    btnRollMapUpdate.IsEnabled = false;
                    return;
                }

                object[] Parameters = new object[12];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentSegmentCode;
                Parameters[2] = _equipmentCode;
                Parameters[3] = _lotId;
                Parameters[4] = Util.NVC(_wipSeq); ;
                Parameters[5] = _laneQty;
                Parameters[6] = _equipmentName + " [" + _equipmentCode + "]";
                Parameters[7] = _prod_Ver_Code;
                Parameters[8] = "Y"; //Test Cut Visible false
                Parameters[9] = "Y"; //Search Mode True
                //Parameters[8] = "N"; //Test Cut Visible false
                //Parameters[9] = "N"; //Search Mode True
                Parameters[10] = "COM001_016"; //호출화면
                //Parameters[11] = _lotId.Substring(0, _lotId.Length - 1);

                // LOT 단위 ID -> EA : 원통형 롤맵 UI 팝업 호출 , M 기존 롤맵 UI 팝업 호출
                //lotUnitCode = GetLotUnitCode();

                if (string.Equals(_processCode, Process.ROLL_PRESSING))
                {
                    //LAST_PROC_ID 가 _processCode
                    if (string.Equals(Util.NVC(_wipStat), Wip_State.PROC) && _lastProcId.Equals(_processCode))
                    {
                        string lotCode = string.Empty;
                        string wipSeq = "1";
                        string processCode = Process.COATING;
                        string equipmentName = string.Empty;
                        string equipmentCode = string.Empty;

                        DataTable dtRollMapInputLot = GetRollMapInputLotCode(Util.NVC(_lotId));

                        if (CommonVerify.HasTableRow(dtRollMapInputLot))
                        {
                            lotCode = dtRollMapInputLot.Rows[0]["INPUT_LOTID"].GetString();

                            DataTable dtEquipment = GetEquipmentCode(lotCode, wipSeq);

                            if (CommonVerify.HasTableRow(dtEquipment))
                            {
                                equipmentCode = dtEquipment.Rows[0]["EQPTID"].GetString();
                            }
                            else
                            {
                                equipmentCode = string.Empty;
                            }

                            CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                            if (popupRollMapUpdate != null)
                            {


                                C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                                if (popupRollMapUpdate != null)
                                {
                                    popupRollMapUpdate.ShowModal();
                                    popupRollMapUpdate.CenterOnScreen();
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        CMM_RM_RP_RESULT popupRollMapUpdate = new CMM_RM_RP_RESULT { FrameOperation = FrameOperation };

                        if (popupRollMapUpdate != null)
                        {
                            C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                            if (popupRollMapUpdate != null)
                            {
                                popupRollMapUpdate.ShowModal();
                                popupRollMapUpdate.CenterOnScreen();
                            }
                        }
                    }
                }
                else if (string.Equals(_processCode, Process.COATING))
                {
                    CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                    if (popupRollMapUpdate != null)
                    {


                        C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                        if (popupRollMapUpdate != null)
                        {
                            popupRollMapUpdate.ShowModal();
                            popupRollMapUpdate.CenterOnScreen();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
