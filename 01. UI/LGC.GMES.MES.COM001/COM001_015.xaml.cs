/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.20 손우석 CSR ID 5991 GMES 설비 Loss 현황의 일자별 loss 현황 운영 설비 상태 조회항목 구분 요청 [요청번호] C20191119-000176
  2020.09.25 김동일 C20200908-000102 : TAB WELD LOSS 상세 정보 더블 클릭 시 타 M/C T,U 정보 조회 팝업 추가.
  2021.03.05 김동일 C20210121-000068 : 작업조 조회 조건 추가
  2021.12.07 김동일 C20211202-000117 : Test(형개발) 내 부동내역 팝업 추가
  2022.11.03 정용석 C20221019-000636 : 일자별 Loss 현황 Grid에 설비, Trouble 약어 Column 추가 & Grid Double Click시에 일자정보 잘못입력되는 현상 수정
  2022.11.22 윤지해 C20221109-000499 : [생산PI팀] [업무변경/개선] GMES -설비 Loss 현황의 'Test(형개발) 내 부동내역' 버튼 삭제요청의 건
  2022.11.29
  2023.01.03 서인석 Merge            : 11월 29일 배포 건에 대해 Biz 및 UI Merge(정호준 책임 개발건, 배포는 아님)
  2023.01.04 정용석 C20221212-000497 : 설비 Loss 현황-Loss별 상세 내역에서 원인설비별/부동내용별 수치 상세화
  2023.01.16 정용석 C20221212-000497 : 원인설비/부동내역별 비율 옆에 원인설비/부동내역별 Loss 시간 합계 추가
  2023.02.07 김도형 C20221230-000001 : 설비LOSS > 설비 LOSS 현황 > 일자별 LOSS 현황 목록에 LOTID 추가
  2023.02.20 오화백 C20230103-000963 : [전극/조립MES팀]ESHM GMES 시스템 구축 (AZS Stacking 일 경우 Machine 설비 LOSS  수정가능하도록 처리)
  2023.04.18 강성묵 C20230103-000963 : ESHM Machine 설비 대응 로직 추가
  2023.06.21 안유수 E20230608-000919 : LOSS별 상세내역 GRID TROUBLE_CAUSE_EQPT_STAT 컬럼 추가
  2023-09-06 유재홍 C20230103-000963 : Mainchk 가 PROCID가 초기값으로 설정되어있을경우 나오지 않는 문제 수정
  2023-10-25 김민석 E20231025-000781 : 일자별 LOSS 현황 탭 콤보 박스 조회 시 공정, 설비가 조회되지 않는 현상 수정
  2024-08-21 김대현 E20240626-000975 : TEST/개발 W/O 자동실적 적용 전후 데이터 조회 기능 추가
  2024-11-04 복현수 MES 리빌딩 PJT   : 쿼리 데이터 타입 변경
  2025-02-24 이민형 MES 리빌딩 PJT   : Test/개발 Check 버튼 주석 처리
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
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        string _AREAID = "";
        string _EQSGID = "";
        string _PROCID = "";
        string _EQPTID = "";
        string _SEARCH_FROM_DATE = string.Empty;        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
        string _SEARCH_TO_DATE = string.Empty;          // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
        string _OPEN_TIME;
        //Machine 설비 Loss 수정 가능여부 Flag :    2023.02.20 오화백
        string MachineEqptChk = string.Empty;
        bool bPack;

        public COM001_015()
        {
            InitializeComponent();
            InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            // 동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            // 동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            C1ComboBox[] cboAreaDailyChild = { cboEquipmentSegmentDaily };
            _combo.SetCombo(cboAreaDaily, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaDailyChild);

            C1ComboBox[] cboAreaSPQChild = { cboEquipmentSegmentSPQ };
            _combo.SetCombo(cboAreaSPQ, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaSPQChild);


            if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {
                bPack = false;

                // 라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
                C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily, cboEquipmentDaily };
                _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);

                // 공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cbProcessChild = { cboEquipment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent);

                C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
                C1ComboBox[] cbProcessDailyChild = { cboEquipmentDaily };
                string strProcessDailyCase = string.Empty;

                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    strProcessDailyCase = "cboProcessPack";
                }
                else
                {
                    strProcessDailyCase = "PROCESS";
                }

                _combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbChild: cbProcessDailyChild, cbParent: cbProcessDailyParent);

                // 설비
                C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cbEquipmentParent);

                C1ComboBox[] cbEquipmentDailyParent = { cboEquipmentSegmentDaily, cboProcessDaily };
                _combo.SetCombo(cboEquipmentDaily, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT", cbParent: cbEquipmentDailyParent);

                //Machine 설비 콤보 조회 - by 오화백 2023-02-20
                SetMachineEqptCombo(cboEquipmentDaily_Machine);

                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
                //Machine 설비 콤보 조회 - by 오화백 2023-02-20
                cboProcessDaily.SelectedValueChanged += cboProcessDaily_SelectedValueChanged;

                cboEquipmentDaily.SelectedValueChanged += cboEquipmentDaily_SelectedValueChanged;
                SetTroubleUnitColumnDisplay();



            }
            else
            {
                bPack = true;

                // 라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
                C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily };
                _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);

                C1ComboBox[] cboEquipmentSegmentSPQParent = { cboAreaSPQ };
                _combo.SetCombo(cboEquipmentSegmentSPQ, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT", cbParent: cboEquipmentSegmentSPQParent);

                /*2023.10.25 
                 * 일자별 현황 TAB의 콤보박스가 설비 LOSS 현황 TAB의 콤보박스를 바라보고 있어 
                 * 조회가 정상적으로 되지 않는 현상으로 수정
                 * KIM MIN SEOK
                 */
                // 공정
                /*
                string strProcessCase = string.Empty;
                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    strProcessCase = "cboProcessPack";
                }
                else
                {
                    strProcessCase = "PROCESS";
                }
                */
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent);

                C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
                string[] sFilterDaily = new string[] { cboEquipmentSegmentDaily.SelectedValue.IsNullOrEmpty() ? null : cboEquipmentSegmentDaily.SelectedValue.ToString()};
                _combo.SetCombo(cboProcessDaily, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cbProcessDailyParent, sFilter: sFilterDaily);

                SetEquipment(cboEquipment, cboProcess);
                //2023.10.25 - KIM MIN SEOK
                SetEquipmentDaily(cboEquipmentDaily, cboProcessDaily);

                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                cboEquipmentSegmentDaily.SelectedItemChanged += cboEquipmentSegmentDaily_SelectedItemChanged;
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
                cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            }

            //2019.11.20
            SetEioStateCombo(cboEquipmentState);

            SetShiftCombo(cboShift);

            if (!this.GetPackApplyLIneByUI(LoginInfo.CFG_AREA_ID))
            {
                this.tabSPQ.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.tabSPQ.Visibility = Visibility.Visible;
            }

            // 2022.11.22 C20221109-000499 [생산PI팀] [업무변경/개선] GMES -설비 Loss 현황의 'Test(형개발) 내 부동내역' 버튼 삭제요청의 건
            if (LoginInfo.CFG_AREA_ID.StartsWith("A"))  // 전체 법인 자동차 조립일 경우에만 버튼 삭제
            {
                this.btnTestLossHist.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.btnTestLossHist.Visibility = Visibility.Visible;
            }
            MachineEqpt_Loss_Modify_Chk(cboProcessDaily.SelectedValue.ToString());
            //2023-09-06 유재홍 - 초기설정으로 PROCID가 설정되어있을경우 Mainchk 안나오는 문제 수정
        }

        private DateTime GetShopOpenTime()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dt);
            if (dtResult.Rows.Count != 0)
            {
                return Convert.ToDateTime(dtResult.Rows[0]["DATETO"]);
            }

            return System.DateTime.Now;

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime dtTmp = GetShopOpenTime();
            ldpDateFrom.SelectedDateTime = dtTmp;
            ldpDateLossFrom.SelectedDateTime = dtTmp;
            ldpDateWorkDate.SelectedDateTime = dtTmp;

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            // 조립동, 전극동의 경우 항목별 비율 Tab을 보이지 않게 함.
            this.tabLossRate.Visibility = (!LoginInfo.CFG_AREA_ID.StartsWith("P")) ? Visibility.Collapsed : Visibility.Visible;

            this.Loaded -= UserControl_Loaded;
        }

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearValue();

                //if (ldpDateFrom.SelectedDateTime.Date > GetShopOpenTime().Date)
                //{
                //    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                //    return;
                //}

                if (grdFromToDate.Visibility == Visibility.Visible && ldpDateTo.SelectedDateTime.Date > System.DateTime.Now.Date)
                {
                    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("STRTTIME", typeof(DateTime));
                RQSTDT.Columns.Add("ENDTIME", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);

                // 작업조로 조회.
                DateTime srartTime = DateTime.Now;
                DateTime endTime = DateTime.Now;

                bool bSearchShft = false;

                if (!Util.NVC(cboShift.SelectedValue).Equals("") && GetShiftTimes(out srartTime, out endTime))
                {
                    dr["STRTTIME"] = srartTime;
                    dr["ENDTIME"] = endTime;
                    dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateWorkDate);
                    dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateWorkDate);

                    bSearchShft = true;
                }

                RQSTDT.Rows.Add(dr);
                _AREAID = Util.GetCondition(cboArea);
                _EQSGID = Util.GetCondition(cboEquipmentSegment);
                _PROCID = Util.GetCondition(cboProcess);
                _EQPTID = Util.GetCondition(cboEquipment);
                this._SEARCH_FROM_DATE = Util.GetCondition(ldpDateFrom);    // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
                this._SEARCH_TO_DATE = Util.GetCondition(ldpDateTo);        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY", "RQSTDT", "RSLTDT", RQSTDT);
                string bzRuleID = string.Empty;              
                bzRuleID = "DA_EQP_SEL_EQPTLOSS_SUMMARY";              
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleID, "RQSTDT", "RSLTDT", RQSTDT);

                if (!dtResult.Columns.Contains("SHFT_SEARCH"))
                {
                    DataColumn newCol = new DataColumn("SHFT_SEARCH", typeof(string));
                    if (bSearchShft)
                        newCol.DefaultValue = "Y";
                    else
                        newCol.DefaultValue = "N";
                    dtResult.Columns.Add(newCol);
                }


                Util.GridSetData(dgLossList, dtResult, FrameOperation, true);

                dgLossDetail.ItemsSource = null;
                Util.gridClear(this.dgLossRate);
                SearchEqptRemark();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchDaily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_TO", typeof(string));
                //2019.11.20
                RQSTDT.Columns.Add("EIOSTAT", typeof(string));

                // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가
                RQSTDT.Columns.Add("EQPTLEVEL", typeof(string));
                RQSTDT.Columns.Add("MAIN_EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                string sAREAID = Util.NVC(cboAreaDaily.SelectedValue);
                string sEQSGID = Util.NVC(cboEquipmentSegmentDaily.SelectedValue);
                string sPROCID = Util.NVC(cboProcessDaily.SelectedValue);
                //string sEQPTID = Util.NVC(cboEquipmentDaily.SelectedValue);
                //2023.02.20 오화백  공통코드에 따라 설비정보 파라미터를 메인설비로 할지 Machine 설비로 할지 
                string sEQPTID = string.Empty;
                // 메인설비 체크박스가 Visible 상태이고 체크 되었을경우 : 메인설비 
                if (chkMain.Visibility == Visibility.Visible && chkMain.IsChecked == true)
                {
                    sEQPTID = Util.NVC(cboEquipmentDaily.SelectedValue);
                }
                // 메인설비 체크박스가 Visible 상태, 체크해제, Machine 설비 선택이 ALL 일경우 : 메인설비
                else if (chkMain.Visibility == Visibility.Visible && chkMain.IsChecked == false && cboEquipmentDaily_Machine.SelectedValue.GetString() == string.Empty)
                {
                    sEQPTID = Util.NVC(cboEquipmentDaily.SelectedValue);
                }
                // Machine 설비가 선택이 되어 있을경우
                else if (chkMain.Visibility == Visibility.Visible && chkMain.IsChecked == false && cboEquipmentDaily_Machine.SelectedValue.GetString() != string.Empty)
                {
                    sEQPTID = Util.NVC(cboEquipmentDaily_Machine.SelectedValue);
                }
                // Unit 설비수정 공정이 아닐 경우 : 동별 공통코드 관리
                else
                {
                    sEQPTID = Util.NVC(cboEquipmentDaily.SelectedValue);
                }

                // 2023.04.18 강성묵 ESHM Machine 설비 대응 로직 추가 (동별 공통코드 : EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS)
                if (MachineEqptChk == "Y")
                {
                    // 메인 설비만 조회
                    if (chkMain.IsChecked == true)
                    {
                        dr["EQPTLEVEL"] = "M";
                    }

                    // 선택된 메인 설비, Machine으로 조회 가능하도록 파라메터 변경
                    if (chkMain.IsChecked == false)
                    {
                        if (Util.NVC(cboEquipmentDaily.SelectedValue) == string.Empty)
                        {
                            dr["MAIN_EQPTID"] = null;
                        }
                        else
                        {
                            dr["MAIN_EQPTID"] = Util.NVC(cboEquipmentDaily.SelectedValue);
                        }

                        if (Util.NVC(cboEquipmentDaily_Machine.SelectedValue) == string.Empty)
                        {
                            sEQPTID = "";
                        }
                        else
                        {
                            sEQPTID = Util.NVC(cboEquipmentDaily_Machine.SelectedValue);
                        }
                    }
                }

                dr["LANGID"] = LoginInfo.LANGID;

                if (sAREAID == "SELECT")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    cboAreaDaily.Focus();
                    return;
                }
                else
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaDaily);
                }

                if (sEQSGID == "")
                {
                    dr["EQSGID"] = null;
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentDaily);
                }

                if (sPROCID == "")
                {
                    dr["PROCID"] = null;
                }
                else
                {
                    dr["PROCID"] = Util.GetCondition(cboProcessDaily);
                }

                if (sEQPTID == "")
                {
                    dr["EQPTID"] = null;
                }
                else
                {
                    //dr["EQPTID"] = Util.GetCondition(cboEquipmentDaily);
                    dr["EQPTID"] = sEQPTID;
                }

                dr["WRK_DATE_FROM"] = ldpDateLossFrom.SelectedDateTime.AddDays(-1).ToString("yyyy-MM-dd");
                dr["WRK_DATE_TO"] = ldpDateLossTo.SelectedDateTime.ToString("yyyy-MM-dd");
                //   dr["WRK_DATE"] = Util.GetCondition(ldpDateFrom_Daily);

                //2019.11.20
                dr["EIOSTAT"] = cboEquipmentState.SelectedValue;

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_DAILY", "RQSTDT", "RSLTDT", RQSTDT);
                string bzRuleId = string.Empty;
                if (chkTestDevDaily.IsChecked == true)
                {
                    bzRuleId = "DA_EQP_SEL_EQPTLOSSRAW_DAILY_TEST_DEV";
                }
                else
                {
                    bzRuleId = "DA_EQP_SEL_EQPTLOSSRAW_DAILY";
                }
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleId, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLossDailyList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchSPQ_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string areaID = Util.NVC(cboAreaSPQ.SelectedValue);
                string equipmentSegmentID = Util.NVC(cboEquipmentSegmentSPQ.SelectedValue);

                // Validation Check...
                if (string.IsNullOrEmpty(areaID) || areaID.Equals("SELECT"))
                {
                    Util.Alert("SFU1499");  // 동을 선택하세요.
                    cboAreaSPQ.Focus();
                    return;
                }

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["FROM_WRK_DATE"] = this.ldpDateFromLossSPQ.SelectedDateTime.Date.ToString("yyyyMMdd");
                drRQSTDT["TO_WRK_DATE"] = this.ldpDateToLossSPQ.SelectedDateTime.Date.AddDays(1).ToString("yyyyMMdd");
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL_WITH_SPQ", "RQSTDT", "RSLTDT", dtRQSTDT);

                Util.GridSetData(this.dgLossSPQ, dt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion Button

        #region Grid
        private void dgLossList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLossList.CurrentRow != null && dgLossList.CurrentColumn.Name.Equals("LOSSCNT"))
                {
					if(Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "LOSSCNT")) == "0") // 2024.12.10. 김영국 - 빈 건수 Check시 로직 처리.
						return;
					
                    SetTroubleUnitColumnDisplay();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("STRTTIME", typeof(DateTime));
                    RQSTDT.Columns.Add("ENDTIME", typeof(DateTime));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "LOSS_CODE"));
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "EQPTID"));
                    dr["FROM_WRK_DATE"] = this._SEARCH_FROM_DATE;
                    dr["TO_WRK_DATE"] = this._SEARCH_TO_DATE;
                    if (Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_SEARCH")).Equals("Y"))
                    {
                        dr["FROM_WRK_DATE"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_WRK_DATE");
                        dr["TO_WRK_DATE"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_WRK_DATE");
                        dr["STRTTIME"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_STRTTIME");
                        dr["ENDTIME"] = DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "SHFT_ENDTIME");
                    }
                    RQSTDT.Rows.Add(dr);

                    //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                    string bzRuleId = string.Empty;                  
                    bzRuleId = "DA_EQP_SEL_EQPTLOSS_SUMMARY_DETAIL";                    
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bzRuleId, "RQSTDT", "RSLTDT", RQSTDT);
                    object temp = dtResult.Rows[0]["LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE"].GetType();
                    Util.GridSetData(dgLossDetail, dtResult, FrameOperation, true);

                    if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                    {
                        Util.gridClear(this.dgLossRate);
                        Util.GridSetData(this.dgLossRate, this.GetLossRateByDimension(dtResult), FrameOperation, true);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable GetLossRateByDimension(DataTable dt)
        {
            DataTable dtResult = new DataTable();
            if (!CommonVerify.HasTableRow(dt))
            {
                return dtResult;
            }

            try
            {
                // 원인설비별 Loss Rate
                var querydOccurEquipmentID = dt.AsEnumerable().GroupBy(grp => new { EQPTID = grp.Field<string>("EQPTID"), OCCUR_EQPTID = grp.Field<string>("OCCUR_EQPTID") }).Select((y, index) => new
                {
                    INDEX = ++index,
                    EQPTID = y.Key.EQPTID,
                    OCCUR_EQPTID = string.IsNullOrEmpty(y.Key.OCCUR_EQPTID) ? "ZZZZZZZZ" : y.Key.OCCUR_EQPTID,
                    CAUSE_EQPTNAME = string.IsNullOrEmpty(y.Max(x => x.Field<string>("CAUSE_EQPTNAME"))) ? "Not Define" : y.Max(x => x.Field<string>("CAUSE_EQPTNAME")),
                    OCCUR_EQPTID_PROPORTION_BY_LOSS_RATE = y.Max(x => Convert.ToString(Math.Round(x.Field<double>("OCCUR_EQPTID_PROPORTION_BY_LOSS_RATE"), 1))) + " %", //decimal -> double 2024.11.04 MES 리빌딩 PJT
                    OCCUR_EQPTID_LOSS_SEC = y.Sum(x => x.Field<double>("LOSS_SEC")), //int -> double 2024.11.04 MES 리빌딩 PJT
                    LOSS_DETL_CODE = string.Empty,
                    LOSS_DETL_NAME = string.Empty,
                    LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE = string.Empty,
                    LOSS_DETL_CODE_LOSS_SEC = 0.0 //0 -> 0.0 2024.11.04 MES 리빌딩 PJT
                });

                // 부동내역별 Loss Rate
                var queryLossDetailCode = dt.AsEnumerable().GroupBy(grp => new { EQPTID = grp.Field<string>("EQPTID"), LOSS_DETL_CODE = grp.Field<string>("LOSS_DETL_CODE") }).Select((y, index) => new
                {
                    INDEX = ++index,
                    EQPTID = y.Key.EQPTID,
                    OCCUR_EQPTID = string.Empty,
                    CAUSE_EQPTNAME = string.Empty,
                    OCCUR_EQPTID_PROPORTION_BY_LOSS_RATE = string.Empty,
                    OCCUR_EQPTID_LOSS_SEC = 0.0, //0 -> 0.0 2024.11.04 MES 리빌딩 PJT
                    LOSS_DETL_CODE = string.IsNullOrEmpty(y.Key.LOSS_DETL_CODE) ? "ZZZZZZZZ" : y.Key.LOSS_DETL_CODE,
                    LOSS_DETL_NAME = string.IsNullOrEmpty(y.Max(x => x.Field<string>("LOSS_DETL_NAME"))) ? "Not Define" : y.Max(x => x.Field<string>("LOSS_DETL_NAME")),
                    LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE = y.Max(x => Convert.ToString(Math.Round(x.Field<double>("LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE"), 1))) + " %", //decimal -> double 2024.11.04 MES 리빌딩 PJT
                    LOSS_DETL_CODE_LOSS_SEC = y.Sum(x => x.Field<double>("LOSS_SEC")) //int -> double 2024.11.04 MES 리빌딩 PJT
                });

                // 위의 두마리 Full Outer Join
                var result = querydOccurEquipmentID.Union(queryLossDetailCode).GroupBy(grp => grp.INDEX).Select(y => new
                {
                    INDEX = y.Key,
                    EQPTID = y.Max(x => x.EQPTID),
                    OCCUR_EQPTID = y.Max(x => x.OCCUR_EQPTID),
                    CAUSE_EQPTNAME = y.Max(x => x.CAUSE_EQPTNAME),
                    OCCUR_EQPTID_PROPORTION_BY_LOSS_RATE = y.Max(x => x.OCCUR_EQPTID_PROPORTION_BY_LOSS_RATE),
                    OCCUR_EQPTID_LOSS_SEC = y.Max(x => x.OCCUR_EQPTID_LOSS_SEC),
                    LOSS_DETL_CODE = y.Max(x => x.LOSS_DETL_CODE),
                    LOSS_DETL_NAME = y.Max(x => x.LOSS_DETL_NAME),
                    LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE = y.Max(x => x.LOSS_DETL_CODE_PROPORTION_BY_LOSS_RATE),
                    LOSS_DETL_CODE_LOSS_SEC = y.Max(x => x.LOSS_DETL_CODE_LOSS_SEC)
                });

                dtResult = Util.queryToDataTable(result.ToList());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtResult;
        }

        private void dgLossList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOSSCNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgLossDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    if (sender == null) return;

            //    C1DataGrid dataGrid = (sender as C1DataGrid);
            //    Point pnt = e.GetPosition(null);
            //    C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

            //    if (cell != null)
            //    {
            //        if (dataGrid.Columns.Contains("EIOSTAT") &&
            //            dataGrid.Columns.Contains("EQGRID") &&
            //            dataGrid.Columns.Contains("EQSGID") &&
            //            //Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EIOSTAT")).Equals("W") &&
            //            Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQGRID")).Equals("PKG") &&
            //            Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQSGID")).Equals("A1A08") // 오창 8라인 선 적용 (추후 하드코딩 제거 예정)
            //           )
            //        {

            //            if (ChkMainUnit(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQPTID"))))
            //            {
            //                COM001_015_UNIT_LOSS_INFO wndUnitInfo = new COM001_015_UNIT_LOSS_INFO();
            //                wndUnitInfo.FrameOperation = FrameOperation;

            //                if (wndUnitInfo != null)
            //                {
            //                    object[] Parameters = new object[3];
            //                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "EQPTID"));
            //                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "START_TIME"));
            //                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "END_TIME"));

            //                    C1WindowExtension.SetParameters(wndUnitInfo, Parameters);
            //                    wndUnitInfo.Closed += new EventHandler(wndUnitInfo_Closed);

            //                    this.Dispatcher.BeginInvoke(new Action(() => wndUnitInfo.ShowModal()));
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgLossSPQ_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }
        #endregion Grid

        #region Combo

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                SetEquipment(cboEquipment, cboProcess);
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            }
        }

        private void cboEquipmentSegmentDaily_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboEquipmentSegmentDaily.Items.Count > 0 && cboEquipmentSegmentDaily.SelectedValue != null && !cboEquipmentSegmentDaily.SelectedValue.Equals("SELECT"))
            {
                cboProcessDaily.SelectedItemChanged -= cboProcessDaily_SelectedItemChanged;
                //2023.10.25 - KIM MIN SEOK
                SetEquipmentDaily(cboEquipmentDaily, cboProcessDaily);
                cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            }
        }
        #endregion [라인] - 조회 조건

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment(cboEquipment, cboProcess);
            }
            SetShiftCombo(cboShift);
        }

        private void cboProcessDaily_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment(cboEquipmentDaily, cboProcessDaily);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            SetTroubleUnitColumnDisplay();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            SetTroubleUnitColumnDisplay();
            SetShiftCombo(cboShift);
        }

        //2023.02.20 오화백  
        private void cboProcessDaily_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
                          
            //Machine 설비 Loss 수정 가능 여부 by 오화백  2023 02 01
             MachineEqpt_Loss_Modify_Chk(cboProcessDaily.SelectedValue.ToString());
        }

        //2023.02.20 오화백  
        private void cboEquipmentDaily_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            //Machine 설비 콤보 조회  by 오화백 2023 02 01
            SetMachineEqptCombo(cboEquipmentDaily_Machine);
        }



        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboShift != null && !Util.NVC(cboShift.SelectedValue).Equals(""))
            {
                grdFromToDate.Visibility = Visibility.Collapsed;
                grdWorkDate.Visibility = Visibility.Visible;
            }
            else
            {
                grdFromToDate.Visibility = Visibility.Visible;
                grdWorkDate.Visibility = Visibility.Collapsed;
            }
        }

        private void SetTroubleUnitColumnDisplay()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["EQPT_LOSS_UNIT_ALARM_DISP_FLAG"].GetString() == "Y")
                    {
                        dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;

                        // 2022.11.23 Trouble 원인설비명, Trouble 원인알람명 추가
                        dgLossDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                        dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;

                        dgLossDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                        dgLossDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                    }

                    if (ChkLnsLami())
                    {
                        dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;

                        dgLossDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Visible;
                        dgLossDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLossDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                    dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;

                    dgLossDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgLossDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                    dgLossDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                dgLossDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                dgLossDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;

                dgLossDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                dgLossDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                dgLossDetail.Columns["TROUBLE_CAUSE_EQPT_STAT"].Visibility = Visibility.Collapsed;

                Util.MessageException(ex);

            }
        }

        private bool ChkLnsLami()
        {
            if(dgLossList.CurrentRow == null)
            { return false; }

            const string bizRuleName = "DA_EQP_SEL_LNS_IN_LAMI";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "EQPTID"));
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            { return true; }
            else { return false; }

        }

        #endregion [공정] - 조회 조건

        //2019.11.20
        private void SetEioStateCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_BAS_SEL_EIOSTATE_CBO";
            //string[] arrColumn = { "LANGID" };
            //string[] arrCondition = { LoginInfo.LANGID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;
            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOSTATE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow dr_ = dtRslt.NewRow();
                    dr_["CBO_NAME"] = "ALL";
                    dr_["CBO_CODE"] = null;
                    dtRslt.Rows.Add(dr_);

                    dtRslt.AcceptChanges();
                }

                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);

                cbo.SelectedIndex = 5;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        /// <summary>
        /// Machine 설비 조회  - by 오화백 2023.02.20
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMachineEqptCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboAreaDaily.SelectedValue.GetString()) ? null : cboAreaDaily.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentDaily.SelectedValue.GetString()) ? null : cboEquipmentSegmentDaily.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcessDaily.SelectedValue.GetString()) ? null : cboProcessDaily.SelectedValue.GetString();
            dr["EQPTID"] = string.IsNullOrEmpty(cboEquipmentDaily.SelectedValue.GetString()) ? null : cboEquipmentDaily.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;

        }

        #endregion Combo

        #endregion Event

        #region Mehod

        private void ClearValue()
        {
            _AREAID = "";
            _EQSGID = "";
            _PROCID = "";
            _EQPTID = "";
            this._SEARCH_FROM_DATE = string.Empty;      // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
            this._SEARCH_TO_DATE = string.Empty;        // CSR 모름 : 43_설비 Loss 현황 Loss 상세내역 수정건
            MachineEqptChk = string.Empty;   //by 오화백 2023.02.20
        }

        private void SearchEqptRemark()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = this._SEARCH_FROM_DATE;
                dr["TO_DATE"] = this._SEARCH_TO_DATE;
                dr["EQPTID"] = _EQPTID;
                dr["AREAID"] = _AREAID;
                dr["EQSGID"] = _EQSGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PROCID"] = _PROCID;
                dr["SHFT_ID"] = Util.NVC(cboShift.SelectedValue).Equals("") ? null : Util.NVC(cboShift.SelectedValue);

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_NOTE_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    dgEQPTDetail.ItemsSource = DataTableConverter.Convert(dtResult);
                    //Util.GridSetData(dgEQPTDetail, dtResult, FrameOperation, true);
                }
                else
                {
                    dgEQPTDetail.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [### 설비 정보 가져오기]
        private void SetEquipment(C1ComboBox cbEqpt, C1ComboBox cbProc)
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegment;
                dr["PROCID"] = Util.GetCondition(cbProc);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cbEqpt.DisplayMemberPath = "CBO_NAME";
                cbEqpt.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cbEqpt.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbEqpt.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbEqpt.SelectedIndex < 0)
                        cbEqpt.SelectedIndex = 0;
                }
                else
                {
                    cbEqpt.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion  [### 설비 정보 가져오기]


        #region [### 일자별 현황 TAB 설비 정보 가져오기]
        /*2023.10.25 
         * 일자별 현황 TAB의 콤보박스가 설비 LOSS 현황 TAB의 콤보박스를 바라보고 있어 
         * 조회가 정상적으로 되지 않는 현상으로 해당 함수 추가
         * KIM MIN SEOK
         */
        private void SetEquipmentDaily(C1ComboBox cbEqpt, C1ComboBox cbProc)
        {
            try
            {
                string sEquipmentSegmentDaily = Util.GetCondition(cboEquipmentSegmentDaily);
                if (string.IsNullOrWhiteSpace(sEquipmentSegmentDaily))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegmentDaily;
                dr["PROCID"] = Util.GetCondition(cbProc);//cbProc.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cbEqpt.DisplayMemberPath = "CBO_NAME";
                cbEqpt.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-ALL-";
                //drIns["CBO_CODE"] = "";
                //dtResult.Rows.InsertAt(drIns, 0);

                cbEqpt.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbEqpt.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbEqpt.SelectedIndex < 0)
                        cbEqpt.SelectedIndex = 0;
                }
                else
                {
                    cbEqpt.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion  [### 설비 정보 가져오기]

        //private void wndUnitInfo_Closed(object sender, EventArgs e)
        //{
        //    COM001_015_UNIT_LOSS_INFO window = sender as COM001_015_UNIT_LOSS_INFO;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {

        //    }
        //}

        private bool ChkMainUnit(string sEqptID)
        {
            try
            {
                bool bMainUnit = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VWEQUIPMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("MAIN_EQPTID") && Util.NVC(dtResult.Rows[0]["MAIN_EQPTID"]).Equals(sEqptID))
                {
                    bMainUnit = true;
                }

                return bMainUnit;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetShiftCombo(C1ComboBox cbo)
        {
            try
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow["CBO_CODE"] = "";
                newRow["CBO_NAME"] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);
                cbo.ItemsSource = dtResult.Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                grdFromToDate.Visibility = Visibility.Visible;
                grdWorkDate.Visibility = Visibility.Collapsed;
            }
        }

        private bool GetShiftTimes(out DateTime startTime, out DateTime endTime)
        {
            startTime = DateTime.Now;
            endTime = DateTime.Now;

            try
            {
                bool bRet = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("SHFTID", typeof(string));
                RQSTDT.Columns.Add("CALDATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["SHFTID"] = Util.NVC(cboShift.SelectedValue);
                dr["CALDATE"] = Util.GetCondition(ldpDateWorkDate);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHFT_TIME_BY_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("STARTTIME") && dtResult.Columns.Contains("ENDTIME") && Util.NVC(dtResult.Rows[0]["VLD_YN"]).Equals("Y"))
                {
                    if (DateTime.TryParse(Util.NVC(dtResult.Rows[0]["STARTTIME"]), out startTime) && DateTime.TryParse(Util.NVC(dtResult.Rows[0]["ENDTIME"]), out endTime))
                        bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #region  Machine 설비 LOSS 가능 공정 확인 :  MachineEqpt_Loss_Modify_Chk()  by 오화백 2023.02.20
        /// <summary>
        /// Machine 설비 LOSS 가능 공정 확인
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        public void MachineEqpt_Loss_Modify_Chk(string Procid)
        {

            if (string.IsNullOrEmpty(Procid)) // 전체선택일 경우
            {
                MachineEqptChk = string.Empty;
                chkMain.IsChecked = true;
                ChkMain.Visibility = Visibility.Collapsed;
                Machine.Visibility = Visibility.Collapsed;
                return;
            }

               

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = Procid;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {

                    MachineEqptChk = "Y";
                    ChkMain.Visibility = Visibility.Visible;
                    

                }
                else
                {
                    MachineEqptChk = string.Empty;
                    chkMain.IsChecked = true;
                    ChkMain.Visibility = Visibility.Collapsed;
                    Machine.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion  Mehod

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    dgLossList.Columns["SHFT_WRK_DATE"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_STRTTIME"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_ENDTIME"].Visibility = Visibility.Visible;
                    dgLossList.Columns["SHFT_SEARCH"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnTestLossHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1499");
                    return;
                }
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1223");
                    return;
                }
                if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1459");
                    return;
                }
                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1153");
                    return;
                }
                if (cboShift.SelectedIndex < 0 || cboShift.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1844");
                    return;
                }


                COM001_015_TEST_LOSS_PRV_INFO wndTestLossInfo = new COM001_015_TEST_LOSS_PRV_INFO();
                wndTestLossInfo.FrameOperation = FrameOperation;

                if (wndTestLossInfo != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = Util.GetCondition(cboArea);
                    Parameters[1] = Util.GetCondition(cboEquipmentSegment);
                    Parameters[2] = Util.GetCondition(cboProcess);
                    Parameters[3] = Util.GetCondition(cboEquipment);
                    Parameters[4] = Util.GetCondition(ldpDateFrom);
                    Parameters[5] = Util.GetCondition(ldpDateTo);
                    Parameters[6] = Util.GetCondition(cboShift);
                    Parameters[7] = "";
                    Parameters[8] = "";

                    DateTime srartTime = DateTime.Now;
                    DateTime endTime = DateTime.Now;

                    if (!Util.NVC(cboShift.SelectedValue).Equals("") && GetShiftTimes(out srartTime, out endTime))
                    {
                        Parameters[7] = srartTime;
                        Parameters[8] = endTime;

                        Parameters[4] = Util.GetCondition(ldpDateWorkDate);
                        Parameters[5] = Util.GetCondition(ldpDateWorkDate);
                    }

                    C1WindowExtension.SetParameters(wndTestLossInfo, Parameters);
                    wndTestLossInfo.Closed += new EventHandler(wndTestLossInfo_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndTestLossInfo.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndTestLossInfo_Closed(object sender, EventArgs e)
        {
            COM001_015_TEST_LOSS_PRV_INFO window = sender as COM001_015_TEST_LOSS_PRV_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }


        private bool GetPackApplyLIneByUI(string areaID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
                drRQSTDT["CBO_CODE"] = "COM001_015";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        if (drRSLTDT["ATTRIBUTE1"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE2"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE3"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE4"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE5"].ToString().Contains(areaID))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }


        /// <summary>
        /// Main 체크박스 클릭시 - Machine 설비Loss 여부 체크 후  Machine 설비 콤보박스 Visible 여부  오화백 2023-01.19 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMain_Click(object sender, RoutedEventArgs e)
        {
            if (chkMain.IsChecked == true)
            {
                Machine.Visibility = Visibility.Collapsed;
            }
            else
            {
                Machine.Visibility = Visibility.Visible;
            }
        }

    }
}